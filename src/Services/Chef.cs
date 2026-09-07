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
using Sims3.Gameplay.Skills;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Situations;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using zoeoeAndDestrospean.Utils;

namespace Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Services
{
    public class Chef : Service<Chef>, IAmLiveInService, IAmSociableService
    {
        const string kChefBook = "HowToServeAndNotBeServed";

        static readonly string sLocalizationKey = DerivedType.GetLocalizationKey();

        [Tunable]
        static ServiceTuning kServiceTuning = new ServiceTuning(1, 1000, false, true, true);

        [Tunable]
        [TunableComment("Length of time (in minutes) between checks that everything is done")]
        static float kCheckTime = 5f;

        [Tunable]
        [TunableComment("Length of time (in hours) that the chef waits before routing to lot")]
        static float kDelayBeforeArriving = 0.5f;

        [Tunable]
        [TunableComment("Length of time (in hours) that the chef waits before leaving the lot, after their work is done")]
        static float kDelayBeforeLeaving = 0.3f;

        [Tunable]
        [TunableComment("Length of time (in minutes) that the chef takes to drive to lot")]
        static float kDriveTime = 5f;

        [Tunable]
        [TunableComment("Extra time (in hours) to wait before leaving if the service NPC is socialized with")]
        static float kExtraWaitTimeAfterSocializing = 0.5f;

        [Tunable]
        [TunableComment("If the chef's relationship with any YAE falls below this level, they will quit")]
        static float kRelationshipLevelForQuit = -50f;

        [Tunable]
        [TunableComment("How old leftovers can be out in minutes before the chef will put it away")]
        static float kTimeWaitBeforePutawayLeftovers = 60f;

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

        public new static ServiceType ServiceTypeStatic
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
                return new List<CommodityKind>()
                {
                    ServiceMotive
                };
            }
        }

        public override bool WaitsBeforePuttingAwayLeftovers
        {
            get
            {
                return true;
            }
        }

        static Chef()
        {
            Init();
        }

        public Chef()
        {
            Instance = this;
        }

        public static string LocalizeString(string name, params object[] parameters)
        {
            return Localization.LocalizeString(sLocalizationKey + ":" + name, parameters);
        }

        public override ServiceSituation InternalCreateSituation(Lot assignedLot, Sim createdSim, int cost, ObjectGuid requestingSim)
        {
            ServiceSituation retVal = null;
            DebugUtils.TryDisplayScriptError(() =>
                {
                    createdSim.SimDescription.ShowSocialsOnSim = true;
                    createdSim.CanBeFired = true;
                    retVal = new ChefSituation(this, assignedLot, createdSim, cost);
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
                    simDescription.TraitManager.AddElement(TraitNames.Artistic);
                    simDescription.TraitManager.AddElement(TraitNames.NaturalCook);
                    List<TraitNames> potentialTraits = new List<TraitNames>
                        {
                            TraitNames.Neurotic,
                            TraitNames.HotHeaded,
                            TraitNames.Perfectionist,
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
                    Skill cookingSkill = sim.SkillManager.AddElement(SkillNames.Cooking);
                    int maxSkillLevel = cookingSkill.MaxSkillLevel;
                    for (int i = 0; i < maxSkillLevel; i++)
                    {
                        cookingSkill.ForceGainPointsForLevelUp();
                    }
                    Book book = BookGeneralData.GetBookGeneralByTitle(kChefBook);
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
