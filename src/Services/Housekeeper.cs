using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Objects;
using Sims3.Gameplay.Scenarios;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Skills;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;

namespace Sims3.Gameplay.zoeoe.ServantRolesMod.Services
{
    public class Housekeeper : Service, IAmCleaningService
    {
        static readonly string sLocalizationKey = typeof(Housekeeper).GetLocalizationKey();

        static string kButlerBook = "HowToServeAndNotBeServed";

        [Tunable]
        static ServiceTuning kServiceTuning = new ServiceTuning(7, 2, true, true, false);

        [Tunable]
        [TunableComment("If any of the CTYAE Sims get below this hunger value, Butler starts cooking")]
        static float kMinHungerBeforeStartCooking = -20f;

        [Tunable]
        [TunableComment("How often the butler will cook in minutes. This prevents the butler from Autonomously cooking continuously. Only used when successfully cooked")]
        static float kTimeBetweenSuccessfulCookingSessions = 40f;

        [TunableComment("When you interrupt a butler while he's cooking, he'll try to continue cooking. But, if he hasn't cooked anything after this time, he'll restart the meal from scratch.")]
        [Tunable]
        static float kTimeBeforeRestartCooking = 20f;

        [TunableComment("How old leftovers can be out in minutes before the butler will put it away")]
        [Tunable]
        static float kTimeWaitBeforePutawayLeftovers = 60f;

        [Tunable]
        [TunableComment("Multiplier for interactions in a room where a sim is sleeping")]
        static float kUseObjectInSameRoomAsSleeperMultiplier = 0.1f;

        [Tunable]
        [TunableComment("If Butler's relationship with any YAE falls below this level, he will quit")]
        static float kRelationshipLevelForQuit = -50f;

        [TunableComment("Butler can start cooking this many hours before the target sim wakes up. This allows the butler to have food ready when the sleeping sim wakes up")]
        [Tunable]
        static float kTimeCanStartCookBeforeSimWakes = 1.8f;

        [TunableComment("Chance you get the good advice moodlet when asking for advice from the butler")]
        [Tunable]
        static float kChanceGetGoodAdviceMoodlet = 25f;

        [TunableComment("Length of time (in hours) that the butler waits before routing to lot")]
        [Tunable]
        static float kDelayBeforeArriving = 0.5f;

        static Housekeeper sHousekeeper = null;

        public override ServiceTuning Tuning
        {
            get
            {
                return kServiceTuning;
            }
        }

        public static float MinHungerBeforeStartCooking
        {
            get
            {
                return kMinHungerBeforeStartCooking;
            }
        }

        public static float TimeBetweenSuccessfulCookingSessions
        {
            get
            {
                return kTimeBetweenSuccessfulCookingSessions;
            }
        }

        public static float TimeBeforeRestartCooking
        {
            get
            {
                return kTimeBeforeRestartCooking;
            }
        }

        public static float TimeWaitBeforePutawayLeftovers
        {
            get
            {
                return kTimeWaitBeforePutawayLeftovers;
            }
        }

        public static float UseObjectInSameRoomAsSleeperMultiplier
        {
            get
            {
                return kUseObjectInSameRoomAsSleeperMultiplier;
            }
        }

        public static float RelationshipLevelForQuit
        {
            get
            {
                return kRelationshipLevelForQuit;
            }
        }

        public static float TimeCanStartCookBeforeSimWakes
        {
            get
            {
                return kTimeCanStartCookBeforeSimWakes;
            }
        }

        public static float ChanceGetGoodAdviceMoodlet
        {
            get
            {
                return kChanceGetGoodAdviceMoodlet;
            }
        }

        public static float DelayBeforeArriving
        {
            get
            {
                return kDelayBeforeArriving;
            }
        }

        public static Housekeeper Instance
        {
            get
            {
                return sHousekeeper;
            }
        }

        public override ServiceType ServiceType
        {
            get
            {
                return ServiceType.Butler;
            }
        }

        public override List<CommodityKind> ServiceMotives
        {
            get
            {
                return new List<CommodityKind>(new CommodityKind[5]
                    {
                        CommodityKind.LookAfterBabyOrToddler,
                        CommodityKind.LookAfterChild,
                        CommodityKind.BabysitterClean,
                        CommodityKind.BeMaid,
                        CommodityKind.BeButler
                    });
            }
        }

        public override bool IsPaidWeekly
        {
            get
            {
                return true;
            }
        }

        static string LocalizeString(string name, params object[] parameters)
        {
            return Localization.LocalizeString(sLocalizationKey + ":" + name, parameters);
        }

        public Housekeeper()
        {
            if (!GameUtils.IsInstalled(ProductVersion.EP3))
            {
                sHousekeeper = null;
            }
            else
            {
                sHousekeeper = this;
            }
        }

        public static void Create()
        {
            if (ServiceNPCSpecifications.ValidForCurrentWorld(ServiceType.Butler))
            {
                if (sHousekeeper == null)
                {
                    new Housekeeper();
                }
                else
                {
                    sHousekeeper.PostLoadFixup();
                }
            }
            else
            {
                DestroyHousekeeper();
            }
        }

        public static void Destroy()
        {
            DestroyHousekeeper();
        }

        static void DestroyHousekeeper()
        {
            Service.Destroy(sHousekeeper);
            sHousekeeper = null;
        }

        public static void RemoveHousekeepersFromLot(Lot lot)
        {
            Housekeeper instance = Instance;
            if (instance == null || (!instance.IsServiceRequested(lot) && !instance.IsAnySimAssignedToLot(lot)))
            {
                return;
            }
            List<Sim> simsAssignedToLot = instance.GetSimsAssignedToLot(lot);
            foreach (Sim item in simsAssignedToLot)
            {
                ButlerSituation butlerSituation = ServiceSituation.FindServiceSituationInvolving(item) as ButlerSituation;
                if (butlerSituation != null)
                {
                    butlerSituation.SetToLeave();
                }
            }
        }

        public override bool NeedsAssignment(Lot lot)
        {
            if (IsServiceRequested(lot))
            {
                return !IsAnySimAssignedToLot(lot);
            }
            return false;
        }

        public override ServiceSituation InternalCreateSituation(Lot assignedLot, Sim createdSim, int cost, ObjectGuid requestingSim)
        {
            if (assignedLot.MoveInScenario is Rodents)
            {
                ServiceSituation result = assignedLot.MoveInScenario.SetupSituation(this, createdSim);
                assignedLot.MoveInScenario = null;
                return result;
            }
            createdSim.SimDescription.ShowSocialsOnSim = true;
            createdSim.CanBeFired = true;
            return new ButlerSituation(this, assignedLot, createdSim, cost);
        }

        public override void UpdateCreatedSim(Sim sim)
        {
            Skill skill = sim.SkillManager.AddElement(SkillNames.Handiness);
            Skill skill2 = sim.SkillManager.AddElement(SkillNames.Cooking);
            int maxSkillLevel = skill.MaxSkillLevel;
            for (int i = 0; i < maxSkillLevel; i++)
            {
                skill2.ForceGainPointsForLevelUp();
                skill.ForceGainPointsForLevelUp();
            }
            Book bookGeneralByTitle = BookGeneralData.GetBookGeneralByTitle(kButlerBook);
            Inventory inventory = sim.Inventory;
            if (inventory != null)
            {
                inventory.DestroyItems();
                if (!inventory.TryToAdd(bookGeneralByTitle))
                {
                    bookGeneralByTitle.Destroy();
                }
            }
        }

        public override void SetServiceNPCProperties(SimDescription simDescription)
        {
            simDescription.CanBeKilledOnJob = true;
            simDescription.Marryable = true;
            simDescription.Contactable = true;
            simDescription.ShowSocialsOnSim = true;
        }

        public override void SetTraits(SimDescription simDescription)
        {
            TraitManager traitManager = simDescription.TraitManager;
            traitManager.AddElement(TraitNames.Neat);
            traitManager.AddElement(TraitNames.Brave);
            traitManager.AddElement(TraitNames.FamilyOriented);
            List<Trait> validTraits = AgingManager.GetValidTraits(simDescription, false, false, true);
            for (int i = 0; i < 2; i++)
            {
                if (validTraits.Count > 0)
                {
                    Trait randomObjectFromList = RandomUtil.GetRandomObjectFromList(validTraits);
                    traitManager.AddElement(randomObjectFromList.Guid);
                    validTraits.Remove(randomObjectFromList);
                }
            }
        }

        public override string GetServiceTopic(Sim serviceSim)
        {
            return "Butler Service";
        }

        public override bool CanRequestServiceFromPhone(Lot lot)
        {
            if (base.CanRequestServiceFromPhone(lot))
            {
                if (GameUtils.GetCurrentWorldType() != WorldType.Vacation)
                {
                    return !lot.IsBaseCampLotType;
                }
                return false;
            }
            return false;
        }

        public static void AskToCook(Sim sim)
        {
            ButlerSituation butlerSituation = ServiceSituation.FindServiceSituationInvolving(sim) as ButlerSituation;
            if (butlerSituation != null)
            {
                butlerSituation.PrepareFood(false);
            }
        }
    }
}
