using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Abstracts.zoeoeAndDestrospean.ServantRolesMod;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.InteractionsShared;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Interfaces.zoeoeAndDestrospean.ServantRolesMod;
using Sims3.Gameplay.Objects;
using Sims3.Gameplay.Objects.Environment;
using Sims3.Gameplay.Objects.Fireplaces;
using Sims3.Gameplay.Scenarios;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Skills;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Situations;
using Sims3.SimIFace;
using Sims3.Store.Objects;
using System;
using System.Collections.Generic;

namespace Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Services
{
    public class Housekeeper : Service<Housekeeper>, IAmCleaningService, IAmLiveInService
    {
        const string kHousekeeperBook = "HowToServeAndNotBeServed";

        static readonly string sLocalizationKey = typeof(Housekeeper).GetLocalizationKey();

        [Tunable]
        static ServiceTuning kServiceTuning = new ServiceTuning(1, 800, false, true, true);

        [Tunable]
        [TunableComment("Length of time (in minutes) between checks that everything is cleaned")]
        static float kCheckTime = 5f;

        [TunableComment("Length of time (in hours) that the housekeeper waits before routing to lot")]
        [Tunable]
        static float kDelayBeforeArriving = 0.5f;

        [TunableComment("Length of time (in hours) that the housekeeper waits before leaving the lot, after her work is done")]
        [Tunable]
        static float kDelayBeforeLeaving = 0.3f;

        [TunableComment("Length of time (in minutes) that the housekeeper takes to drive to lot")]
        [Tunable]
        static float kDriveTime = 5f;

        [Tunable]
        [TunableComment("Extra time (in hours) to wait before leaving if the service NPC is socialized with")]
        static float kExtraWaitTimeAfterSocializing = 0.5f;

        [Tunable]
        [TunableComment("If the housekeeper's relationship with any YAE falls below this level, she will quit")]
        static float kRelationshipLevelForQuit = -50f;

        [TunableComment("How old leftovers can be out in minutes before the housekeeper will put it away")]
        [Tunable]
        static float kTimeWaitBeforePutawayLeftovers = 60f;

        [Tunable]
        [TunableComment("Multiplier for interactions in a room where a sim is sleeping")]
        static float kUseObjectInSameRoomAsSleeperMultiplier = 0.1f;

        public override ServiceTuning Tuning
        {
            get
            {
                return kServiceTuning;
            }
        }

        public static float CheckTime
        {
            get
            {
                return kCheckTime;
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

        public static float DriveTime
        {
            get
            {
                return kDriveTime;
            }
        }

        public static float ExtraWaitTimeAfterSocializing
        {
            get
            {
                return kExtraWaitTimeAfterSocializing;
            }
        }

        public static float RelationshipLevelForQuit
        {
            get
            {
                return kRelationshipLevelForQuit;
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
                    Housekeeper.ServiceMotive
                };
            }
        }

        public override bool IsPaidWeekly
        {
            get
            {
                return true;
            }
        }

        public override bool IsQuietAroundSleepingSims
        {
            get
            {
                return true;
            }
        }

        public override bool WaitsBeforePuttingAwayLeftovers
        {
            get
            {
                return true;
            }
        }

        static Housekeeper()
        {
            Init();
            World.sOnWorldLoadFinishedEventHandler += (sender, e) => CommonUtils.TryDisplayScriptError(() =>
                {
                    ServiceMotive.AddAsOutput<Bookshelf_ReadSomething.Definition, Bookshelf>(2, true, 2, OutputUpdateType.ContinuousFlow);
                    ServiceMotive.AddAsOutput<Tablet.ChooseBookOnTablet.Definition, Tablet>(1, true, 1, OutputUpdateType.ContinuousFlow);
                    ServiceMotive.AddAsOutput<FirePit.LightFirePit.Definition, FirePit>(200, true, 200, OutputUpdateType.ContinuousFlow);
                    ServiceMotive.AddAsOutput<Fireplace.LightFire.Definition, Fireplace>(200, true, 200, OutputUpdateType.ContinuousFlow);
                    ServiceMotive.AddAsOutput<ReadBook.Definition, Book>(1, true, 1, OutputUpdateType.ContinuousFlow);
                    ServiceMotive.AddAsOutput<ReadBookChooser.Definition, Book>(1, true, 1, OutputUpdateType.ContinuousFlow);
                    ServiceMotive.AddAsOutput<Tablet.ReadBookOnTablet.Definition, Book>(1, true, 1, OutputUpdateType.ContinuousFlow);
                    ServiceMotive.AddAsOutput<Sim.ReadSomethingInInventory.Definition, Sim>(2, true, 2, OutputUpdateType.ContinuousFlow);
                    ServiceMotive.AddAsOutput<SitAndWait.Definition, GameObject>(1, false, 1, OutputUpdateType.ImmediateDelta);
                });
        }

        public Housekeeper()
        {
            Instance = this;
        }

        public static void Create()
        {
            CommonUtils.TryDisplayScriptError(() =>
                {
                    if (ServiceNPCSpecifications.ValidForCurrentWorld(ServiceType.Maid))
                    {
                        if (Instance == null)
                        {
                            new Housekeeper();
                        }
                        else
                        {
                            Instance.PostLoadFixup();
                        }
                    }
                    else
                    {
                        DestroyHousekeeper();
                    }
                });
        }

        public static void Destroy()
        {
            DestroyHousekeeper();
        }

        public static void DestroyHousekeeper()
        {
            Destroy(Instance);
            Instance = null;
        }

        public override string GetServiceTopic(Sim serviceSim)
        {
            return "Housekeeper Service";
        }

        public static string LocalizeString(string name, params object[] parameters)
        {
            return Localization.LocalizeString(sLocalizationKey + ":" + name, parameters);
        }

        public override bool NeedsAssignment(Lot lot)
        {
            bool retVal;
            return !CommonUtils.TryDisplayScriptError(() =>
                {
                    if (IsServiceRequested(lot))
                    {
                        return !IsAnySimAssignedToLot(lot);
                    }
                    return false;
                }, out retVal) && retVal;
        }

        public override ServiceSituation InternalCreateSituation(Lot assignedLot, Sim createdSim, int cost, ObjectGuid requestingSim)
        {
            ServiceSituation retVal = null;
            CommonUtils.TryDisplayScriptError(() =>
                {
                    if (assignedLot.MoveInScenario is Rodents)
                    {
                        retVal = assignedLot.MoveInScenario.SetupSituation(this, createdSim);
                        assignedLot.MoveInScenario = null;
                        return;
                    }
                    createdSim.SimDescription.ShowSocialsOnSim = true;
                    createdSim.CanBeFired = true;
                    retVal = new HousekeeperSituation(this, assignedLot, createdSim, cost);
                });
            return retVal;
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
            CommonUtils.TryDisplayScriptError(() =>
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
                });
        }

        public override void UpdateCreatedSim(Sim sim)
        {
            CommonUtils.TryDisplayScriptError(() =>
                {
                    /*
                    Skill handinessSkill = sim.SkillManager.AddElement(SkillNames.Handiness);
                    int maxSkillLevel = handinessSkill.MaxSkillLevel;
                    for (int i = 0; i < maxSkillLevel; i++)
                    {
                        handinessSkill.ForceGainPointsForLevelUp();
                    }
                    */
                    Book book = BookGeneralData.GetBookGeneralByTitle(kHousekeeperBook);
                    Inventory inventory = sim.Inventory;
                    if (inventory != null)
                    {
                        inventory.DestroyItems();
                        if (!inventory.TryToAdd(book))
                        {
                            book.Destroy();
                        }
                    }
                });
        }
    }
}
