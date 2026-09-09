using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Interfaces.zoeoeAndDestrospean.ServantRolesMod;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Skills;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Services;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.UI.Controller;
using System;
using System.Collections.Generic;
using zoeoeAndDestrospean.Enums;

namespace zoeoeAndDestrospean.Utils.ServantRolesMod
{
    public static class ServiceUtils
    {
        [Persistable]
        public class ActiveTopicAction
        {
            short mGrouping;

            public LongTermRelationshipTypes Grouping
            {
                get
                {
                    return (LongTermRelationshipTypes)mGrouping;
                }
                set
                {
                    mGrouping = (short)value;
                }
            }

            public bool IsActive;

            public string Name;

            protected ActiveTopicAction()
            {
            }

            public ActiveTopicAction(string name, LongTermRelationshipTypes grouping = LongTermRelationshipTypes.Default, bool isActive = false)
            {
                Name = name;
                Grouping = grouping;
                IsActive = isActive;
            }
        }

        [Persistable]
        public class CommodityChange
        {
            int mUpdateAboveAndBelowZero;

            int mUpdateType;

            public float ActualValue;

            public float ConstantChange;

            public string InteractionDefinitionType;

            public bool Locked;

            public string TargetType;

            public bool TimeDependsOnCommodityFilling;

            public UpdateAboveAndBelowZeroType UpdateAboveAndBelowZero
            {
                get
                {
                    return (UpdateAboveAndBelowZeroType)mUpdateAboveAndBelowZero;
                }
                set
                {
                    mUpdateAboveAndBelowZero = (int)value;
                }
            }

            public bool UpdateEvenOnFailure;

            public OutputUpdateType UpdateType
            {
                get
                {
                    return (OutputUpdateType)mUpdateType;
                }
                set
                {
                    mUpdateType = (int)value;
                }
            }

            protected CommodityChange()
            {
            }

            public CommodityChange(string interactionDefinitionType, string targetType, float constantChange, bool locked, float actualValue, OutputUpdateType updateType, bool timeDependsOnCommodityFilling = false, bool updateEvenOnFailure = false, UpdateAboveAndBelowZeroType updateAboveAndBelowZero = UpdateAboveAndBelowZeroType.Either)
            {
                InteractionDefinitionType = interactionDefinitionType;
                TargetType = targetType;
                ConstantChange = constantChange;
                Locked = locked;
                ActualValue = actualValue;
                UpdateType = updateType;
                TimeDependsOnCommodityFilling = timeDependsOnCommodityFilling;
                UpdateEvenOnFailure = updateEvenOnFailure;
                UpdateAboveAndBelowZero = updateAboveAndBelowZero;
            }

            public CommodityChange(Type interactionDefinitionType, Type targetType, float constantChange, bool locked, float actualValue, OutputUpdateType updateType, bool timeDependsOnCommodityFilling = false, bool updateEvenOnFailure = false, UpdateAboveAndBelowZeroType updateAboveAndBelowZero = UpdateAboveAndBelowZeroType.Either) : this(interactionDefinitionType.FullName, targetType.FullName, constantChange, locked, actualValue, updateType, timeDependsOnCommodityFilling, updateEvenOnFailure, updateAboveAndBelowZero)
            {
            }
        }

        [Persistable]
        public class ServiceProfile
        {
            List<ulong> mHiddenTraits = new List<ulong>();

            List<int> mMotives = new List<int>();

            List<ulong> mPotentialTraits = new List<ulong>();

            int mServiceMotive = 0;

            ulong mServiceType = 1uL;

            List<ulong> mSkills = new List<ulong>();

            List<ulong> mTraits = new List<ulong>();

            uint mValidAges = (uint)(CASAgeGenderFlags.YoungAdult | CASAgeGenderFlags.Adult);

            uint mValidGenders = (uint)CASAgeGenderFlags.GenderMask;

            public List<ActiveTopicAction> Actions = new List<ActiveTopicAction>();

            public string CancelledServiceTitle;

            /// <summary>
            /// Length of time (in minutes) between checks that everything is done.
            /// </summary>
            public float CheckTime = 5f;

            /// <summary>
            /// Length of time (in hours) that the custom service NPC waits before routing to lot.
            /// </summary>
            public float DelayBeforeArriving = 0.5f;

            /// <summary>
            /// Length of time (in hours) that the custom service NPC waits before leaving the lot, after their work is done.
            /// </summary>
            public float DelayBeforeLeaving = 0.3f;

            /// <summary>
            /// Length of time (in minutes) that the custom service NPC takes to drive to lot.
            /// </summary>
            public float DriveTime = 5f;

            /// <summary>
            /// Extra time (in hours) to wait before leaving if the service NPC is socialized with.
            /// </summary>
            public float ExtraWaitTimeAfterSocializing = 0.5f;

            public List<TraitNames> HiddenTraits
            {
                get
                {
                    return mHiddenTraits.ConvertAll(x => (TraitNames)x);
                }
                set
                {
                    mHiddenTraits = value.ConvertAll(x => (ulong)x);
                }
            }

            public bool IsLiveInService = false;

            public bool IsLoaded = false;

            public bool IsQuietAroundSleepingSims = false;

            public List<CommodityKind> Motives
            {
                get
                {
                    return mMotives.ConvertAll(x => (CommodityKind)x);
                }
                set
                {
                    mMotives = value.ConvertAll(x => (int)x);
                }
            }

            public string Name;

            public List<CommodityChange> Outputs = new List<CommodityChange>();

            public int PotentialTraitCount = 0;

            public List<TraitNames> PotentialTraits
            {
                get
                {
                    return mPotentialTraits.ConvertAll(x => (TraitNames)x);
                }
                set
                {
                    mPotentialTraits = value.ConvertAll(x => (ulong)x);
                }
            }

            /// <summary>
            /// If the custom service NPC's relationship with any YAE falls below this level, they will quit.
            /// </summary>
            public float RelationshipLevelForQuit = -50f;

            public CommodityKind ServiceMotive
            {
                get
                {
                    return (CommodityKind)mServiceMotive;
                }
                set
                {
                    mServiceMotive = (int)value;
                }
            }

            [Persistable]
            public Service.ServiceTuning ServiceTuning = new Service.ServiceTuning();

            public ServiceType ServiceType
            {
                get
                {
                    return (ServiceType)mServiceType;
                }
                set
                {
                    mServiceType = (ulong)value;
                }
            }

            public List<SkillNames> Skills
            {
                get
                {
                    return mSkills.ConvertAll(x => (SkillNames)x);
                }
                set
                {
                    mSkills = value.ConvertAll(x => (ulong)x);
                }
            }

            /// <summary>
            /// How old leftovers can be out in minutes before the custom service NPC will put it away.
            /// </summary>
            public float TimeWaitBeforePutawayLeftovers = 60f;

            public string Title;

            public List<TraitNames> Traits
            {
                get
                {
                    return mTraits.ConvertAll(x => (TraitNames)x);
                }
                set
                {
                    mTraits = value.ConvertAll(x => (ulong)x);
                }
            }

            /// <summary>
            /// Multiplier for interactions in a room where a sim is sleeping.
            /// </summary>
            public float UseObjectInSameRoomAsSleeperMultiplier = 0.1f;

            public bool UseServiceTypeOutfit = false;

            public CASAgeGenderFlags ValidAges
            {
                get
                {
                    return (CASAgeGenderFlags)mValidAges;
                }
                set
                {
                    mValidAges = (uint)value;
                }
            }

            public CASAgeGenderFlags ValidGenders
            {
                get
                {
                    return (CASAgeGenderFlags)mValidGenders;
                }
                set
                {
                    mValidGenders = (uint)value;
                }
            }

            public bool WaitsBeforePuttingAwayLeftovers = false;

            protected ServiceProfile()
            {
            }

            public ServiceProfile(string name, string title, string cancelledServiceTitle = null, List<CommodityKind> additionalMotives = null, List<CommodityChange> outputs = null, List<TraitNames> traits = null, List<TraitNames> hiddenTraits = null, List<TraitNames> potentialTraits = null, int potentialTraitCount = 0, List<SkillNames> skills = null) : this(name, title, cancelledServiceTitle, null, additionalMotives, outputs, traits, hiddenTraits, potentialTraits, potentialTraitCount, skills)
            {
            }

            public ServiceProfile(string name, string title, string cancelledServiceTitle = null, CommodityKind? serviceMotive = null, List<CommodityKind> additionalMotives = null, List<CommodityChange> outputs = null, List<TraitNames> traits = null, List<TraitNames> hiddenTraits = null, List<TraitNames> potentialTraits = null, int potentialTraitCount = 0, List<SkillNames> skills = null)
            {
                Name = name;
                Title = title;
                CancelledServiceTitle = cancelledServiceTitle ?? title;
                ServiceMotive = serviceMotive ?? CommonUtils.GetCommodityKind("Be" + name, CommodityKindType.Motive);
                Motives = additionalMotives ?? new List<CommodityKind>();
                Outputs = outputs ?? new List<CommodityChange>();
                if (!mMotives.Contains(mServiceMotive))
                {
                    mMotives.Add(mServiceMotive);
                }
                Traits = traits ?? new List<TraitNames>();
                HiddenTraits = hiddenTraits ?? new List<TraitNames>();
                PotentialTraits = potentialTraits ?? new List<TraitNames>();
                PotentialTraitCount = potentialTraitCount;
                Skills = skills ?? new List<SkillNames>();
            }

            public void AddHiddenTraits(params TraitNames[] traits)
            {
                foreach (TraitNames trait in traits)
                {
                    mHiddenTraits.Add((ulong)trait);
                }
            }

            public void AddMotives(params CommodityKind[] motives)
            {
                foreach (CommodityKind motive in motives)
                {
                    mMotives.Add((int)motive);
                }
            }

            public void AddPotentialTraits(params TraitNames[] traits)
            {
                foreach (TraitNames trait in traits)
                {
                    mPotentialTraits.Add((ulong)trait);
                }
            }

            public void AddSkills(params SkillNames[] skills)
            {
                foreach (SkillNames skill in skills)
                {
                    mSkills.Add((ulong)skill);
                }
            }

            public void AddTraits(params TraitNames[] traits)
            {
                foreach (TraitNames trait in traits)
                {
                    mTraits.Add((ulong)trait);
                }
            }

            public void RemoveHiddenTraits(Predicate<TraitNames> predicate)
            {
                mHiddenTraits.RemoveAll(x => predicate((TraitNames)x));
            }

            public void RemoveHiddenTraits(params TraitNames[] traits)
            {
                foreach (TraitNames trait in traits)
                {
                    mHiddenTraits.Remove((ulong)trait);
                }
            }

            public void RemoveMotives(params CommodityKind[] motives)
            {
                foreach (CommodityKind motive in motives)
                {
                    mMotives.Remove((int)motive);
                }
            }

            public void RemoveMotives(Predicate<CommodityKind> predicate)
            {
                mMotives.RemoveAll(x => predicate((CommodityKind)x));
            }

            public void RemovePotentialTraits(Predicate<TraitNames> predicate)
            {
                mPotentialTraits.RemoveAll(x => predicate((TraitNames)x));
            }

            public void RemovePotentialTraits(params TraitNames[] traits)
            {
                foreach (TraitNames trait in traits)
                {
                    mPotentialTraits.Remove((ulong)trait);
                }
            }

            public void RemoveSkills(Predicate<SkillNames> predicate)
            {
                mSkills.RemoveAll(x => predicate((SkillNames)x));
            }

            public void RemoveSkills(params SkillNames[] skills)
            {
                foreach (SkillNames skill in skills)
                {
                    mSkills.Remove((ulong)skill);
                }
            }

            public void RemoveTraits(Predicate<TraitNames> predicate)
            {
                mTraits.RemoveAll(x => predicate((TraitNames)x));
            }

            public void RemoveTraits(params TraitNames[] traits)
            {
                foreach (TraitNames trait in traits)
                {
                    mTraits.Remove((ulong)trait);
                }
            }
        }

        public static readonly Dictionary<string, CustomService> CustomServices = new Dictionary<string, CustomService>();

        public static readonly Dictionary<Type, Service> Instances = new Dictionary<Type, Service>();

        public static readonly List<Type> PreloadedTypes = new List<Type>();

        public static readonly Dictionary<Type, CommodityKind> ServiceMotives = new Dictionary<Type, CommodityKind>();

        public static bool IsFromServantRolesMod<Service>() where Service : Sims3.Gameplay.Services.Service
        {
            return IsFromServantRolesMod(typeof(Service));
        }

        public static bool IsFromServantRolesMod(this Service service)
        {
            return IsFromServantRolesMod(service.GetType());
        }

        public static bool IsFromServantRolesMod(Type type)
        {
            return typeof(IService).IsAssignableFrom(type);
        }
    }
}
