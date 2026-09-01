using Sims3.Gameplay.Abstracts.zoeoeAndDestrospean.ServantRolesMod;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Interfaces.zoeoeAndDestrospean.ServantRolesMod;
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
    public class Chef : Service<Chef>, IAmLiveInService
    {
        const string kChefBook = "HowToServeAndNotBeServed";

        static readonly string sLocalizationKey = typeof(Chef).GetLocalizationKey();

        [Tunable]
        static ServiceTuning kServiceTuning = new ServiceTuning();

        [Tunable]
        [TunableComment("Length of time (in minutes) between checks that everything is cleaned")]
        static float kCheckTime = 5f;

        [TunableComment("Length of time (in hours) that the chef waits before routing to lot")]
        [Tunable]
        static float kDelayBeforeArriving = 0.5f;

        [TunableComment("Length of time (in hours) that the chef waits before leaving the lot, after their work is done")]
        [Tunable]
        static float kDelayBeforeLeaving = 0.3f;

        [TunableComment("Length of time (in minutes) that the chef takes to drive to lot")]
        [Tunable]
        static float kDriveTime = 5f;

        [Tunable]
        [TunableComment("Extra time (in hours) to wait before leaving if the service NPC is socialized with")]
        static float kExtraWaitTimeAfterSocializing = 0.5f;

        [Tunable]
        [TunableComment("If the chef's relationship with any YAE falls below this level, they will quit")]
        static float kRelationshipLevelForQuit = -50f;

        [TunableComment("How old leftovers can be out in minutes before the chef will put it away")]
        [Tunable]
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
                return new List<CommodityKind>()
                {
                    Chef.ServiceMotive
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

        public static void Create()
        {
            CommonUtils.TryDisplayScriptError(() =>
                {
                    if (ServiceNPCSpecifications.ValidForCurrentWorld(ServiceType.Butler))
                    {
                        if (Instance == null)
                        {
                            new Chef();
                        }
                        else
                        {
                            Instance.PostLoadFixup();
                        }
                    }
                    else
                    {
                        DestroyChef();
                    }
                });
        }

        public static void Destroy()
        {
            DestroyChef();
        }

        public static void DestroyChef()
        {
            Destroy(Instance);
            Instance = null;
        }

        public override string GetServiceTopic(Sim serviceSim)
        {
            return "Chef Service";
        }

        public override string GetUniformName(SimDescription simDescription)
        {
            return "career_execchef_" + (simDescription.IsFemale ? "female" : "male") + (simDescription.Elder ? "elder" : "");
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
                    retVal = new ChefSituation(this, assignedLot, createdSim, cost);
                });
            return retVal;
        }

        public static void RemoveChefsFromLot(Lot lot)
        {
            CommonUtils.TryDisplayScriptError(() =>
                {
                    if (Instance == null || !Instance.IsServiceRequested(lot) && !Instance.IsAnySimAssignedToLot(lot))
                    {
                        return;
                    }
                    List<Sim> simsAssignedToLot = Instance.GetSimsAssignedToLot(lot);
                    foreach (Sim item in simsAssignedToLot)
                    {
                        ChefSituation ChefSituation = ServiceSituation.FindServiceSituationInvolving(item) as ChefSituation;
                        if (ChefSituation != null)
                        {
                            ChefSituation.SetToLeave();
                        }
                    }
                });
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
                    simDescription.TraitManager.AddElement(TraitNames.Artistic);
                    simDescription.TraitManager.AddElement(TraitNames.NaturalCook);
                    //simDescription.TraitManager.AddHiddenElement(TraitNames.BornToCook);
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
                    simDescription.TraitManager.AddRandomTrait(2);
                });
        }

        public override void UpdateCreatedSim(Sim sim)
        {
            CommonUtils.TryDisplayScriptError(() =>
                {
                    
                    Skill cookingSkill = sim.SkillManager.AddElement(SkillNames.Cooking);
                    int maxSkillLevel = cookingSkill.MaxSkillLevel;
                    for (int i = 0; i < maxSkillLevel; i++)
                    {
                        cookingSkill.ForceGainPointsForLevelUp();
                    }
                    Book bookGeneralByTitle = BookGeneralData.GetBookGeneralByTitle(kChefBook);
                    Inventory inventory = sim.Inventory;
                    if (inventory != null)
                    {
                        inventory.DestroyItems();
                        if (!inventory.TryToAdd(bookGeneralByTitle))
                        {
                            bookGeneralByTitle.Destroy();
                        }
                    }
                });
        }
    }
}
