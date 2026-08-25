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
using Service = Sims3.Gameplay.Abstracts.zoeoe.ServantRolesMod.Service;

namespace Sims3.Gameplay.zoeoe.ServantRolesMod.Services
{
    public class Housekeeper : Service, IAmCleaningService
    {
        static readonly string sLocalizationKey = typeof(Housekeeper).GetLocalizationKey();

        static string kHousekeeperBook = "HowToServeAndNotBeServed";

        [Tunable]
        static ServiceTuning kServiceTuning = new ServiceTuning();

        [TunableComment("How old leftovers can be out in minutes before the housekeeper will put it away")]
        [Tunable]
        static float kTimeWaitBeforePutawayLeftovers = 60;

        [Tunable]
        [TunableComment("Multiplier for interactions in a room where a sim is sleeping")]
        static float kUseObjectInSameRoomAsSleeperMultiplier = .1f;

        [Tunable]
        [TunableComment("If Housekeeper's relationship with any YAE falls below this level, she will quit")]
        static float kRelationshipLevelForQuit = -50;

        [TunableComment("Chance you get the good advice moodlet when asking for advice from the housekeeper")]
        [Tunable]
        static float kChanceGetGoodAdviceMoodlet = 25;

        [TunableComment("Length of time (in hours) that the housekeeper waits before routing to lot")]
        [Tunable]
        static float kDelayBeforeArriving = .5f;

        static Housekeeper sHousekeeper = null;

        public override ServiceTuning Tuning
        {
            get
            {
                return kServiceTuning;
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
                return new List<CommodityKind>(new CommodityKind[]
                    {
                        CommodityKind.BabysitterClean,
                        CommodityKind.BeMaid,
                        CommodityKind.BeButler
                    });
            }
        }

        public override bool IsHomelessService
        {
            get
            {
                return true;
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
            sHousekeeper = this;
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
            Skill handinessSkill = sim.SkillManager.AddElement(SkillNames.Handiness);
            int maxSkillLevel = handinessSkill.MaxSkillLevel;
            for (int i = 0; i < maxSkillLevel; i++)
            {
                handinessSkill.ForceGainPointsForLevelUp();
            }
            Book bookGeneralByTitle = BookGeneralData.GetBookGeneralByTitle(kHousekeeperBook);
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
            simDescription.TraitManager.AddElement(TraitNames.Neat);
            simDescription.TraitManager.AddElement(TraitNames.Neurotic);
            simDescription.TraitManager.AddHiddenElement(TraitNames.MakesNoMesses);
            simDescription.TraitManager.AddHiddenElement(TraitNames.SpeedyCleaner);
            List<TraitNames> potentialTraits = new List<TraitNames>
                {
                    TraitNames.Flirty,
                    TraitNames.Kleptomaniac,
                    TraitNames.Charismatic
                };
            for (int i = 0; i < 2; i++)
            {
                TraitNames randomObjectFromList = RandomUtil.GetRandomObjectFromList(potentialTraits);
                simDescription.TraitManager.AddElement(randomObjectFromList);
                potentialTraits.Remove(randomObjectFromList);
            }
        }

        public override string GetServiceTopic(Sim serviceSim)
        {
            return "Housekeeper Service";
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
