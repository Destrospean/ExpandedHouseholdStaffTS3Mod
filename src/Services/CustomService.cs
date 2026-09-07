using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Abstracts.zoeoeAndDestrospean.ServantRolesMod;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Interfaces.zoeoeAndDestrospean.ServantRolesMod;
using Sims3.Gameplay.Objects;
using Sims3.Gameplay.Objects.Beds;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Skills;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Situations;
using Sims3.SimIFace;
using Sims3.UI.Controller;
using System;
using System.Collections.Generic;
using System.Reflection;
using zoeoeAndDestrospean.Enums;
using zoeoeAndDestrospean.Utils;
using zoeoeAndDestrospean.Utils.ServantRolesMod;

namespace Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Services
{
    public class CustomService : Service<CustomService>, IAmSociableService
    {
        public class ServiceProfile
        {
            public readonly List<CommodityKind> Motives = new List<CommodityKind>();

            public string Name;

            public int PotentialTraitCount;

            public readonly List<TraitNames> PotentialTraits = new List<TraitNames>();

            public CommodityKind ServiceMotive;

            public readonly List<SkillNames> Skills = new List<SkillNames>();

            public string Title;

            public readonly List<TraitNames> Traits = new List<TraitNames>();

            public ServiceProfile(string name, string title, CommodityKind? serviceMotive, List<CommodityKind> motives = null, List<TraitNames> traits = null, List<TraitNames> potentialTraits = null, int potentialTraitCount = 0, List<SkillNames> skills = null)
            {
                Name = name;
                Title = title;
                ServiceMotive = serviceMotive ?? CommonUtils.GetCommodityKind("Be" + name, CommodityKindType.Motive);
                Motives = motives ?? new List<CommodityKind>();
                if (!Motives.Contains(ServiceMotive))
                {
                    Motives.Add(ServiceMotive);
                }
                Traits = traits ?? new List<TraitNames>();
                PotentialTraits = potentialTraits ?? new List<TraitNames>();
                PotentialTraitCount = potentialTraitCount;
                Skills = skills ?? new List<SkillNames>();
            }
        }

        const string kCustomServiceBook = "HowToServeAndNotBeServed";

        [Tunable]
        static ServiceTuning kServiceTuning = new ServiceTuning(1, 1000, false, true, true);

        [Tunable]
        [TunableComment("Length of time (in minutes) between checks that everything is done")]
        static float kCheckTime = 5f;

        [Tunable]
        [TunableComment("Length of time (in hours) that the custom service NPC waits before routing to lot")]
        static float kDelayBeforeArriving = 0.5f;

        [Tunable]
        [TunableComment("Length of time (in hours) that the custom service NPC waits before leaving the lot, after their work is done")]
        static float kDelayBeforeLeaving = 0.3f;

        [Tunable]
        [TunableComment("Length of time (in minutes) that the custom service NPC takes to drive to lot")]
        static float kDriveTime = 5f;

        [Tunable]
        [TunableComment("Extra time (in hours) to wait before leaving if the service NPC is socialized with")]
        static float kExtraWaitTimeAfterSocializing = 0.5f;

        [Tunable]
        [TunableComment("If the custom service NPC's relationship with any YAE falls below this level, they will quit")]
        static float kRelationshipLevelForQuit = -50f;

        [Tunable]
        [TunableComment("How old leftovers can be out in minutes before the custom service NPC will put it away")]
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

        public ServiceProfile Profile;

        /// <summary>
        /// Gets the motive commodity kind for the service.
        /// </summary>
        public new CommodityKind ServiceMotive
        {
            get
            {
                return Profile.ServiceMotive;
            }
        }

        public override List<CommodityKind> ServiceMotives
        {
            get
            {
                return Profile.Motives;
            }
        }

        public new static ServiceType ServiceTypeStatic
        {
            get
            {
                return ServiceType.Maid;
            }
        }

        public override bool WaitsBeforePuttingAwayLeftovers
        {
            get
            {
                return true;
            }
        }

        public CustomService(ServiceProfile profile)
        {
            Profile = profile;
            ServiceUtils.CustomServices[profile.Name] = this;
        }

        public static void Create(ServiceProfile profile)
        {
            DebugUtils.TryDisplayScriptError(() =>
                {
                    if (ServiceNPCSpecifications.ValidForCurrentWorld(ServiceTypeStatic))
                    {
                        if (ServiceUtils.CustomServices.ContainsKey(profile.Name) && ServiceUtils.CustomServices[profile.Name] != null)
                        {
                            ServiceUtils.CustomServices[profile.Name].PostLoadFixup();
                        }
                        else
                        {
                            new CustomService(profile);
                        }
                    }
                    else
                    {
                        Destroy();
                    }
                });
        }

        /// <summary>
        /// Call this method for every class derived from this one within its static constructor.
        /// </summary>
        public static void Init(ServiceProfile profile)
        {
            World.OnObjectPlacedInLotEventHandler += OnObjectPlacedInLot;
            World.sOnWorldLoadFinishedEventHandler += (sender, e) => DebugUtils.TryDisplayScriptError(() =>
                {
                    Create(profile);
                    if (!ServiceUtils.CustomServices.ContainsKey(profile.Name) || ServiceUtils.CustomServices[profile.Name] == null)
                    {
                        return;
                    }
                    if (typeof(IAmLiveInService).IsAssignableFrom(DerivedType))
                    {
                        foreach (Bed bed in Sims3.Gameplay.Queries.GetObjects<Bed>())
                        {
                            AddInteractions(bed);
                        }
                    }
                });
            World.sOnWorldQuitEventHandler += OnWorldQuit;
        }

        public override ServiceSituation InternalCreateSituation(Lot assignedLot, Sim createdSim, int cost, ObjectGuid requestingSim)
        {
            ServiceSituation retVal = null;
            DebugUtils.TryDisplayScriptError(() =>
                {
                    createdSim.SimDescription.ShowSocialsOnSim = true;
                    createdSim.CanBeFired = true;
                    retVal = new CustomServiceSituation(this, assignedLot, createdSim, cost);
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
                    foreach (TraitNames traitName in Profile.Traits)
                    {
                        simDescription.TraitManager.AddElement(traitName);
                    }
                    List<TraitNames> potentialTraits = new List<TraitNames>(Profile.PotentialTraits);
                    for (int i = 0; i < Profile.PotentialTraitCount; i++)
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
                    foreach (SkillNames skillName in Profile.Skills)
                    {
                        Skill skill = sim.SkillManager.AddElement(skillName);
                        int maxSkillLevel = skill.MaxSkillLevel;
                        for (int i = 0; i < maxSkillLevel; i++)
                        {
                            skill.ForceGainPointsForLevelUp();
                        }
                    }
                    Book book = BookGeneralData.GetBookGeneralByTitle(kCustomServiceBook);
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
