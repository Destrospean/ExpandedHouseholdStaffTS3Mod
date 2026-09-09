using Sims3.Gameplay.Abstracts.zoeoeAndDestrospean.ServantRolesMod;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Interfaces.zoeoeAndDestrospean.ServantRolesMod;
using Sims3.Gameplay.Objects;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Situations;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using System;
using System.Collections.Generic;
using zoeoeAndDestrospean.Utils;
using zoeoeAndDestrospean.Utils.ServantRolesMod;

namespace Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Services
{
    public class Housekeeper : Service<Housekeeper>, IAmCleaningService, IAmLiveInService, IAmSociableService
    {
        const string kHousekeeperBook = "HowToServeAndNotBeServed";

        [Tunable]
        static ServiceTuning kServiceTuning = new ServiceTuning(1, 800, false, true, true);

        [Tunable]
        [TunableComment("Allows the service role to be filled by male Sims")]
        static bool kAllowMale = true;

        [Tunable]
        [TunableComment("Allows the service role to be filled by female Sims")]
        static bool kAllowFemale = true;

        [Tunable]
        [TunableComment("Allows the service role to be filled by children")]
        static bool kAllowChild = false;

        [Tunable]
        [TunableComment("Allows the service role to be filled by teenagers")]
        static bool kAllowTeen = false;

        [Tunable]
        [TunableComment("Allows the service role to be filled by young adults")]
        static bool kAllowYoungAdult = true;

        [Tunable]
        [TunableComment("Allows the service role to be filled by adults")]
        static bool kAllowAdult = true;

        [Tunable]
        [TunableComment("Allows the service role to be filled by elders")]
        static bool kAllowElder = false;

        [Tunable]
        [TunableComment("Length of time (in minutes) between checks that everything is done")]
        static float kCheckTime = 5f;

        [Tunable]
        [TunableComment("Length of time (in hours) that the housekeeper waits before routing to lot")]
        static float kDelayBeforeArriving = 0.5f;

        [Tunable]
        [TunableComment("Length of time (in hours) that the housekeeper waits before leaving the lot, after their work is done")]
        static float kDelayBeforeLeaving = 0.3f;

        [Tunable]
        [TunableComment("Length of time (in minutes) that the housekeeper takes to drive to lot")]
        static float kDriveTime = 5f;

        [Tunable]
        [TunableComment("Extra time (in hours) to wait before leaving if the service NPC is socialized with")]
        static float kExtraWaitTimeAfterSocializing = 0.5f;

        [Tunable]
        [TunableComment("If the housekeeper's relationship with any YAE falls below this level, they will quit")]
        static float kRelationshipLevelForQuit = -50f;

        [Tunable]
        [TunableComment("How old leftovers can be out in minutes before the housekeeper will put it away")]
        static float kTimeWaitBeforePutawayLeftovers = 60f;

        [Tunable]
        [TunableComment("Multiplier for interactions in a room where a Sim is sleeping")]
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

        public new static ServiceType ServiceTypeStatic
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
                    ServiceMotive
                };
            }
        }

        public override bool IsQuietAroundSleepingSims
        {
            get
            {
                return true;
            }
        }

        public override CASAgeGenderFlags ValidAges
        {
            get
            {
                CASAgeGenderFlags retVal = CASAgeGenderFlags.None;
                if (kAllowChild)
                {
                    retVal |= CASAgeGenderFlags.Child;
                }
                if (kAllowTeen)
                {
                    retVal |= CASAgeGenderFlags.Teen;
                }
                if (kAllowYoungAdult)
                {
                    retVal |= CASAgeGenderFlags.YoungAdult;
                }
                if (kAllowAdult)
                {
                    retVal |= CASAgeGenderFlags.Adult;
                }
                if (kAllowElder)
                {
                    retVal |= CASAgeGenderFlags.Elder;
                }
                return retVal;
            }
        }

        public override CASAgeGenderFlags ValidGenders
        {
            get
            {
                CASAgeGenderFlags retVal = CASAgeGenderFlags.None;
                if (kAllowFemale)
                {
                    retVal |= CASAgeGenderFlags.Female;
                }
                if (kAllowMale)
                {
                    retVal |= CASAgeGenderFlags.Male;
                }
                return retVal;
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
        }

        public Housekeeper()
        {
            Instance = this;
            Outputs.AddRange(new[]
                {
                    new ServiceUtils.CommodityChange("Sims3.Gameplay.Actors.Sim+ReadSomethingInInventory+Definition", "Sims3.Gameplay.Actors.Sim", 2f, true, 2f, OutputUpdateType.ContinuousFlow),
                    new ServiceUtils.CommodityChange("Sims3.Gameplay.InteractionsShared.SitAndWait+Definition", "Sims3.Gameplay.Abstracts.GameObject", 1f, false, 1f, OutputUpdateType.ImmediateDelta),
                    new ServiceUtils.CommodityChange("Sims3.Gameplay.Objects.Bookshelf_ReadSomething+Definition", "Sims3.Gameplay.Objects.Bookshelf", 2f, true, 2f, OutputUpdateType.ContinuousFlow),
                    new ServiceUtils.CommodityChange("Sims3.Gameplay.Objects.Environment.FirePit+LightFirePit+Definition", "Sims3.Gameplay.Objects.Environment.FirePit", 200f, true, 200f, OutputUpdateType.ContinuousFlow),
                    new ServiceUtils.CommodityChange("Sims3.Gameplay.Objects.Fireplaces.Fireplace+LightFire+Definition", "Sims3.Gameplay.Objects.Fireplaces.Fireplace", 200f, true, 200f, OutputUpdateType.ContinuousFlow),
                    new ServiceUtils.CommodityChange("Sims3.Gameplay.Objects.ReadBook+Definition", "Sims3.Gameplay.Objects.Book", 1f, true, 1f, OutputUpdateType.ContinuousFlow),
                    new ServiceUtils.CommodityChange("Sims3.Gameplay.Objects.ReadBookChooser+Definition", "Sims3.Gameplay.Objects.Book", 1f, true, 1f, OutputUpdateType.ContinuousFlow),
                    new ServiceUtils.CommodityChange("Sims3.Store.Objects.Tablet+ChooseBookOnTablet+Definition", "Sims3.Store.Objects.Tablet", 1f, true, 1f, OutputUpdateType.ContinuousFlow),
                    new ServiceUtils.CommodityChange("Sims3.Store.Objects.Tablet+ReadBookOnTablet+Definition", "Sims3.Gameplay.Objects.Book", 1f, true, 1f, OutputUpdateType.ContinuousFlow)
                });
        }

        public override ServiceSituation InternalCreateSituation(Lot assignedLot, Sim createdSim, int cost, ObjectGuid requestingSim)
        {
            ServiceSituation retVal = null;
            DebugUtils.TryDisplayScriptError(() =>
                {
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
            DebugUtils.TryDisplayScriptError(() =>
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
                });
        }

        public override void UpdateCreatedSim(Sim sim)
        {
            DebugUtils.TryDisplayScriptError(() =>
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
