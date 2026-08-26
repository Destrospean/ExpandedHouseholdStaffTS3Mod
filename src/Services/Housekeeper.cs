using Sims3.Gameplay.Abstracts.zoeoeAndDestrospean.ServantRolesMod;
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
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Situations;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;

namespace Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Services
{
    public class Housekeeper : Service<Housekeeper>, IAmCleaningService
    {
        public static readonly string sLocalizationKey = typeof(Housekeeper).GetLocalizationKey();

        public static string kHousekeeperBook = "HowToServeAndNotBeServed";

        [Tunable]
        public static ServiceTuning kServiceTuning = new ServiceTuning();

        [TunableComment("How old leftovers can be out in minutes before the housekeeper will put it away")]
        [Tunable]
        public static float kTimeWaitBeforePutawayLeftovers = 60;

        [Tunable]
        [TunableComment("Multiplier for interactions in a room where a sim is sleeping")]
        public static float kUseObjectInSameRoomAsSleeperMultiplier = .1f;

        [Tunable]
        [TunableComment("If the housekeeper's relationship with any YAE falls below this level, she will quit")]
        public static float kRelationshipLevelForQuit = -50;

        [TunableComment("Length of time (in hours) that the housekeeper waits before routing to lot")]
        [Tunable]
        public static float kDelayBeforeArriving = .5f;

        [TunableComment("Length of time (in hours) that the housekeeper waits before leaving the lot, after her work is done")]
        [Tunable]
        public static float kDelayBeforeLeaving = .3f;

        [Tunable]
        [TunableComment("Length of time (in minutes) between checks that everything is cleaned")]
        public static float kCheckTime = 5;

        [Tunable]
        [TunableComment("Extra time (in hours) to wait before leaving if the service NPC is socialized with")]
        public static float kExtraWaitTimeAfterSocializing = .5f;

        [TunableComment("Length of time (in minutes) that the housekeeper takes to drive to lot")]
        [Tunable]
        public static float kDriveTime = 5;

        public static Housekeeper sHousekeeper = null;

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

        public static float DelayBeforeArriving
        {
            get
            {
                return kDelayBeforeArriving;
            }
        }

        public static float DelayBeforeLeaving
        {
            get
            {
                return kDelayBeforeLeaving;
            }
        }

        public static float CheckTime
        {
            get
            {
                return kCheckTime;
            }
        }

        public static float ExtraWaitTimeAfterSocializing
        {
            get
            {
                return kExtraWaitTimeAfterSocializing;
            }
        }

        public static float DriveTime
        {
            get
            {
                return kDriveTime;
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
                return ServiceType.Maid;
            }
        }

        public override List<CommodityKind> ServiceMotives
        {
            get
            {
                return new List<CommodityKind>()
                {
                    CommodityKind.BeMaid,
                    Housekeeper.BeServiceCommodityKind
                };
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

        public static string LocalizeString(string name, params object[] parameters)
        {
            return Localization.LocalizeString(sLocalizationKey + ":" + name, parameters);
        }

        public Housekeeper()
        {
            sHousekeeper = this;
        }

        public static void Create()
        {
            if (ServiceNPCSpecifications.ValidForCurrentWorld(ServiceType.Maid))
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
            Service<Housekeeper>.Destroy(sHousekeeper);
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
                HousekeeperSituation housekeeperSituation = ServiceSituation.FindServiceSituationInvolving(item) as HousekeeperSituation;
                if (housekeeperSituation != null)
                {
                    housekeeperSituation.SetToLeave();
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
            return new HousekeeperSituation(this, assignedLot, createdSim, cost);
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
            simDescription.TraitManager.AddHiddenElement(TraitNames.MakesNoMesses);
            simDescription.TraitManager.AddHiddenElement(TraitNames.SpeedyCleaner);
            List<TraitNames> potentialTraits = new List<TraitNames>
                {
                    TraitNames.Neurotic,
                    TraitNames.Flirty,
                    TraitNames.Kleptomaniac,
                    TraitNames.Charismatic
                };
            for (int i = 0; i < 2; i++)
            {
                TraitNames traitName = RandomUtil.GetRandomObjectFromList(potentialTraits);
                simDescription.TraitManager.AddElement(traitName);
                potentialTraits.Remove(traitName);
            }
            simDescription.TraitManager.AddRandomTrait(2);
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
    }
}
