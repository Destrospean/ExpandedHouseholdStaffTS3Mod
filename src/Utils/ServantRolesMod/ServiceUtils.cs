using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Interfaces.zoeoeAndDestrospean.ServantRolesMod;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Skills;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Services;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.UI;
using Sims3.UI.Controller;
using System;
using System.Collections.Generic;
using zoeoeAndDestrospean.Enums;
using zoeoeAndDestrospean.Misc;

namespace zoeoeAndDestrospean.Utils.ServantRolesMod
{
    public static class ServiceUtils
    {
        public delegate string GetUniformNameDelegate(SimDescription simDescription);

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

            /// <summary>
            /// If set to <c>true</c>, the action is an FPA; otherwise, it's an SPA.
            /// </summary>
            public bool IsActive;

            public string Name;

            protected ActiveTopicAction()
            {
            }

            /// <summary>
            /// Initializes a new instance of the
            /// <see cref="zoeoeAndDestrospean.Utils.ServantRolesMod.ServiceUtils+ActiveTopicAction"/> class.
            /// </summary>
            /// <param name="name">Name.</param>
            /// <param name="grouping">Grouping.</param>
            /// <param name="isActive">If set to <c>true</c>, the action is an FPA; otherwise, it's an SPA.</param>
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

        public class ServiceProfile : IServiceProfile
        {
            List<ActiveTopicAction> mActions = new List<ActiveTopicAction>();

            uint mCarProductVersion = 0u;

            float mCheckTime = 5f;

            float mDelayBeforeArriving = 0.5f;

            float mDelayBeforeLeaving = 0.3f;

            float mDriveTime = 5f;

            float mExtraWaitTimeAfterSocializing = 0.5f;

            ulong mFlags = 0uL;

            List<IGameObject> mInventory = new List<IGameObject>();

            List<ulong> mHiddenTraits = new List<ulong>();

            List<int> mMotives = new List<int>();

            List<CommodityChange> mOutputs = new List<CommodityChange>();

            int mPotentialTraitCount = 0;

            List<ulong> mPotentialTraits = new List<ulong>();

            float mRelationshipLevelForQuit = -50f;

            int mServiceMotive = 0;

            [Persistable]
            public Service.ServiceTuning mServiceTuning = new Service.ServiceTuning();

            ulong mServiceType = 1uL;

            List<SkillLevelPair> mSkills = new List<SkillLevelPair>();

            Dictionary<string, string> mStrings = new Dictionary<string, string>();

            float mTimeToSpendWorking = 8f;

            float mTimeWaitBeforePutawayLeftovers = 60f;

            List<ulong> mTraits = new List<ulong>();

            float mUseObjectInSameRoomAsSleeperMultiplier = 0.1f;

            uint mValidAges = 48u;

            uint mValidGenders = 0u;

            /// <summary>
            /// The list of actions for the service topic.
            /// </summary>
            public List<ActiveTopicAction> Actions
            {
                get
                {
                    return mActions;
                }
                set
                {
                    mActions = value; 
                }
            }

            /// <summary>
            /// The message that shows when the service is cancelled.
            /// </summary>
            public string CancelledMessage
            {
                get
                {
                    string value;
                    return mStrings.TryGetValue("CancelledMessage", out value) ? value : null;
                }
                set
                {
                    mStrings["CancelledMessage"] = value;
                }
            }

            /// <summary>
            /// The message that shows when the service is cancelled and there is already a service NPC of that service on the lot.
            /// Note: this will only be used when <see cref="IsLiveInService"/> is set to <c>false</c>.
            /// <see cref="CancelledMessage"/> will be used instead if <see cref="IsLiveInService"/> is set to <c>true</c>.
            /// </summary>
            public string CancelledWhileActiveMessage
            {
                get
                {
                    string value;
                    return mStrings.TryGetValue("CancelledWhileActiveMessage", out value) ? value : null;
                }
                set
                {
                    mStrings["CancelledWhileActiveMessage"] = value;
                }
            }

            /// <summary>
            /// The instance name of the car the service NPC arrives in. If <c>null</c> or empty, the service NPC will arrive and leave by foot.
            /// </summary>
            public string CarInstanceName
            {
                get
                {
                    string value;
                    return mStrings.TryGetValue("CarInstanceName", out value) ? value : null;
                }
                set
                {
                    mStrings["CarInstanceName"] = value;
                }
            }

            /// <summary>
            /// The product version of the car the service NPC arrives in. This is for when the car is a Store, expansion pack, or stuff pack item.
            /// </summary>
            public ProductVersion CarProductVersion
            {
                get
                {
                    return (ProductVersion)mCarProductVersion;
                }
                set
                {
                    mCarProductVersion = (uint)value;
                }
            }

            /// <summary>
            /// Length of time (in minutes) between checks that everything is done.
            /// </summary>
            public float CheckTime
            {
                get
                {
                    return mCheckTime;
                }
                set
                {
                    mCheckTime = value;
                }
            }

            /// <summary>
            /// Length of time (in hours) that the service NPC waits before routing to lot.
            /// </summary>
            public float DelayBeforeArriving
            {
                get
                {
                    return mDelayBeforeArriving;
                }
                set
                {
                    mDelayBeforeArriving = value;
                }
            }

            /// <summary>
            /// Length of time (in hours) that the service NPC waits before leaving the lot, after their work is done.
            /// </summary>
            public float DelayBeforeLeaving
            {
                get
                {
                    return mDelayBeforeLeaving;
                }
                set
                {
                    mDelayBeforeLeaving = value;
                }
            }

            /// <summary>
            /// Length of time (in minutes) that the service NPC takes to drive to lot.
            /// </summary>
            public float DriveTime
            {
                get
                {
                    return mDriveTime;
                }
                set
                {
                    mDriveTime = value;
                }
            }

            /// <summary>
            /// Extra time (in hours) to wait before leaving if the service NPC is socialized with.
            /// </summary>
            public float ExtraWaitTimeAfterSocializing
            {
                get
                {
                    return mExtraWaitTimeAfterSocializing;
                }
                set
                {
                    mExtraWaitTimeAfterSocializing = value;
                }
            }

            public GetUniformNameDelegate GetUniformNameCallback = null;

            public bool GetUniformFromName = false;

            /// <summary>
            /// The list of hidden traits for the service NPC.
            /// Note: do not add or remove elements from this property directly, as that will not work. Use the <see cref="AddHiddenTraits"/> and <see cref="RemoveHiddenTraits"/> methods instead.
            /// </summary>
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
            /// <summary>
            /// The items the service NPC spawns with.
            /// </summary>
            public List<IGameObject> Inventory
            {
                get
                {
                    return mInventory;
                }
                set
                {
                    mInventory = value;
                }
            }

            /// <summary>
            /// If set to <c>true</c>, the service NPC will stay with the household that requested them.
            /// Interactions for setting/unsetting their bed will be available to the service NPC with this property set to <c>true</c>.
            /// </summary>
            public bool IsLiveInService
            {
                get
                {
                    return (mFlags & (ulong)ServiceProfileFlags.LiveInService) != 0uL;
                }
                set
                {
                    if (value)
                    {
                        mFlags |= (ulong)ServiceProfileFlags.LiveInService;
                    }
                    else
                    {
                        mFlags &= ulong.MaxValue ^ (ulong)ServiceProfileFlags.LiveInService;
                    }
                }
            }

            /// <summary>
            /// If set to <c>true</c>, the service NPC will avoid interacting with objects in the same room as a sleeping Sim.
            /// </summary>
            public bool IsQuietAroundSleepingSims
            {
                get
                {
                    return (mFlags & (ulong)ServiceProfileFlags.QuietAroundSleepingSims) != 0uL;
                }
                set
                {
                    if (value)
                    {
                        mFlags |= (ulong)ServiceProfileFlags.QuietAroundSleepingSims;
                    }
                    else
                    {
                        mFlags &= ulong.MaxValue ^ (ulong)ServiceProfileFlags.QuietAroundSleepingSims;
                    }
                }
            }

            /// <summary>
            /// If set to <c>true</c>, the service NPC will quit when they are in close proximity to Bonehilda, and will scream and run away dramatically.
            /// </summary>
            public bool IsScaredOfBonehilda
            {
                get
                {
                    return (mFlags & (ulong)ServiceProfileFlags.ScaredOfBonehilda) != 0uL;
                }
                set
                {
                    if (value)
                    {
                        mFlags |= (ulong)ServiceProfileFlags.ScaredOfBonehilda;
                    }
                    else
                    {
                        mFlags &= ulong.MaxValue ^ (ulong)ServiceProfileFlags.ScaredOfBonehilda;
                    }
                }
            }

            /// <summary>
            /// The service motives. If assigning manually (rather than as a parameter in the constructor) be sure include the <see cref="ServiceMotive"/> property.
            /// Note: do not add or remove elements from this property directly, as that will not work. Use the <see cref="AddMotives"/> and <see cref="RemoveMotives"/> methods instead.
            /// </summary>
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

            /// <summary>
            /// The internal name of the service. Must be unique.
            /// </summary>
            public string Name
            {
                get
                {
                    string value;
                    return mStrings.TryGetValue("Name", out value) ? value : null;
                }
                set
                {
                    mStrings["Name"] = value;
                }
            }

            /// <summary>
            /// The list of outputs added to interaction tunings for the service motive.
            /// </summary>
            public List<CommodityChange> Outputs
            {
                get
                {
                    return mOutputs;
                }
                set
                {
                    mOutputs = value;
                }
            }

            /// <summary>
            /// The number of potential traits to randomly pick from the list of potential traits (see <see cref="PotentialTraits"/>) for the service NPC.
            /// </summary>
            public int PotentialTraitCount
            {
                get
                {
                    return mPotentialTraitCount;
                }
                set
                {
                    mPotentialTraitCount = value;
                }
            }

            /// <summary>
            /// The list of potential traits for the service NPC; will be randomly picked from, the number of which is specified by <see cref="PotentialTraitCount"/>.
            /// Note: do not add or remove elements from this property directly, as that will not work. Use the <see cref="AddPotentialTraits"/> and <see cref="RemovePotentialTraits"/> methods instead.
            /// </summary>
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

            // <summary>
            /// If the service NPC's relationship with any YAE falls below this level, they will quit.
            /// </summary>
            public float RelationshipLevelForQuit
            {
                get
                {
                    return mRelationshipLevelForQuit;
                }
                set
                {
                    mRelationshipLevelForQuit = value;
                }
            }


            /// <summary>
            /// If set to <c>true</c>, the service NPC will call emergency services when there is a fire on the lot they're assigned to.
            /// </summary>
            public bool ReportsFires
            {
                get
                {
                    return (mFlags & (ulong)ServiceProfileFlags.ReportsFires) != 0uL;
                }
                set
                {
                    if (value)
                    {
                        mFlags |= (ulong)ServiceProfileFlags.ReportsFires;
                    }
                    else
                    {
                        mFlags &= ulong.MaxValue ^ (ulong)ServiceProfileFlags.ReportsFires;
                    }
                }
            }
                
            /// <summary>
            /// The message that shows when a service NPC is requested.
            /// </summary>
            public string RequestedMessage
            {
                get
                {
                    string requestedMessage;
                    return mStrings.TryGetValue("RequestedMessage", out requestedMessage) ? requestedMessage : null;
                }
                set
                {
                    mStrings["RequestedMessage"] = value;
                }
            }
                
            /// <summary>
            /// Note: this can (and should) be omitted for the UI for creating/editing service profiles.
            /// </summary>
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

            /// <summary>
            /// Includes cost, whether the service is recurring, whether the service is an emergency service, etc.
            /// </summary>
            public Service.ServiceTuning ServiceTuning
            {
                get
                {
                    return mServiceTuning;
                }
                set
                {
                    mServiceTuning = value;
                }
            }
                
            /// <summary>
            /// Note: this can (and should) be omitted for the UI for creating/editing service profiles.
            /// </summary>
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
                
            /// <summary>
            /// The list of skills (and their levels) for the service NPC to start out with.
            /// </summary>
            public List<SkillLevelPair> Skills
            {
                get
                {
                    return mSkills;
                }
                set
                {
                    mSkills = value;
                }
            }

            /// <summary>
            /// Length of time (in hours) that the service NPC spends performing their duties before they charge for their service and leave.
            /// Note: this will only be used when <see cref="IsLiveInService"/> is set to <c>false</c>.
            /// </summary>
            public float TimeToSpendWorking
            {
                get
                {
                    return mTimeToSpendWorking;
                }
                set
                {
                    mTimeToSpendWorking = value;
                }
            }
                
            /// <summary>
            /// How old leftovers can be out in minutes before the service NPC will put it away.
            /// Note: this will only be used when <see cref="WaitsBeforePuttingAwayLeftovers"/> is set to <c>true</c>.
            /// </summary>
            public float TimeWaitBeforePutawayLeftovers
            {
                get
                {
                    return mTimeWaitBeforePutawayLeftovers;
                }
                set
                {
                    mTimeWaitBeforePutawayLeftovers = value;
                }
            }

            /// <summary>
            /// The display name of the service.
            /// </summary>
            public string Title
            {
                get
                {
                    string value;
                    return mStrings.TryGetValue("Title", out value) ? value : null;
                }
                set
                {
                    mStrings["Title"] = value;
                }
            }
                
            /// <summary>
            /// The list of explicit traits for the service NPC.
            /// Note: do not add or remove elements from this property directly, as that will not work. Use the <see cref="AddTraits"/> and <see cref="RemoveTraits"/> methods instead.
            /// </summary>
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
            /// Multiplier for interactions in a room where a Sim is sleeping.
            /// Note: this will only be used when <see cref="IsQuietAroundSleepingSims"/> is set to <c>true</c>.
            /// </summary>
            public float UseObjectInSameRoomAsSleeperMultiplier
            {
                get
                {
                    return mUseObjectInSameRoomAsSleeperMultiplier;
                }
                set
                {
                    mUseObjectInSameRoomAsSleeperMultiplier = value;
                }
            }

            /// <summary>
            /// The allowed range of ages that the service NPC can be.
            /// </summary>
            public CASAgeGenderFlags ValidAges
            {
                get
                {
                    return (CASAgeGenderFlags)mValidAges;
                }
                set
                {
                    mValidAges = (uint)(value & CASAgeGenderFlags.AgeMask);
                }
            }

            /// <summary>
            /// The allowed range of genders that the service NPC can be.
            /// </summary>
            public CASAgeGenderFlags ValidGenders
            {
                get
                {
                    return (CASAgeGenderFlags)mValidGenders;
                }
                set
                {
                    CASAgeGenderFlags gender = value & CASAgeGenderFlags.GenderMask;
                    mValidGenders = gender == CASAgeGenderFlags.GenderMask ? 0u : (uint)gender;
                }
            }

            /// <summary>
            /// If set to <c>true</c>, the service NPC waits a bit after a meal is prepared before putting it away.
            /// </summary>
            public bool WaitsBeforePuttingAwayLeftovers
            {
                get
                {
                    return (mFlags & (ulong)ServiceProfileFlags.WaitsBeforePuttingAwayLeftovers) != 0uL;
                }
                set
                {
                    if (value)
                    {
                        mFlags |= (ulong)ServiceProfileFlags.WaitsBeforePuttingAwayLeftovers;
                    }
                    else
                    {
                        mFlags &= ulong.MaxValue ^ (ulong)ServiceProfileFlags.WaitsBeforePuttingAwayLeftovers;
                    }
                }
            }

            protected ServiceProfile()
            {
            }

            public ServiceProfile(string name, string title) : this(name, title, additionalMotives: null)
            {
            }

            public ServiceProfile(string name, string title, List<CommodityKind> additionalMotives = null) : this(name, title, serviceMotive: null, additionalMotives: additionalMotives)
            {
            }

            public ServiceProfile(string name, string title, string requestedMessage = null, string cancelledMessage = null, string cancelledWhileActiveMessage = null, List<CommodityKind> additionalMotives = null, List<CommodityChange> outputs = null, List<TraitNames> traits = null, List<TraitNames> hiddenTraits = null, List<TraitNames> potentialTraits = null, int potentialTraitCount = 0, List<SkillLevelPair> skills = null) : this(name, title, requestedMessage, cancelledMessage, cancelledWhileActiveMessage, null, additionalMotives, outputs, traits, hiddenTraits, potentialTraits, potentialTraitCount, skills)
            {
            }

            public ServiceProfile(string name, string title, string requestedMessage = null, string cancelledMessage = null, string cancelledWhileActiveMessage = null, CommodityKind? serviceMotive = null, List<CommodityKind> additionalMotives = null, List<CommodityChange> outputs = null, List<TraitNames> traits = null, List<TraitNames> hiddenTraits = null, List<TraitNames> potentialTraits = null, int potentialTraitCount = 0, List<SkillLevelPair> skills = null)
            {
                Name = name;
                Title = title;
                RequestedMessage = requestedMessage ?? title;
                CancelledMessage = cancelledMessage ?? title;
                CancelledWhileActiveMessage = cancelledWhileActiveMessage ?? title;
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
                Skills = skills ?? new List<SkillLevelPair>();
            }

            public void AddActions(params ActiveTopicAction[] actions)
            {
                mActions.AddRange(actions);
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

            public void AddOutputs(params CommodityChange[] outputs)
            {
                mOutputs.AddRange(outputs);
            }

            public void AddPotentialTraits(params TraitNames[] traits)
            {
                foreach (TraitNames trait in traits)
                {
                    mPotentialTraits.Add((ulong)trait);
                }
            }

            public void AddSkills(params SkillLevelPair[] skills)
            {
                mSkills.AddRange(skills);
            }

            public void AddTraits(params TraitNames[] traits)
            {
                foreach (TraitNames trait in traits)
                {
                    mTraits.Add((ulong)trait);
                }
            }

            public void RemoveActions(params ActiveTopicAction[] actions)
            {
                foreach (ActiveTopicAction action in actions)
                {
                    mActions.Remove(action);
                }
            }

            public void RemoveActions(Predicate<ActiveTopicAction> predicate = null)
            {
                mActions.RemoveAll(predicate ?? (x => true));
            }

            public void RemoveHiddenTraits(Predicate<TraitNames> predicate = null)
            {
                mHiddenTraits.RemoveAll(x => predicate == null || predicate((TraitNames)x));
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

            public void RemoveMotives(Predicate<CommodityKind> predicate = null)
            {
                mMotives.RemoveAll(x => predicate == null || predicate((CommodityKind)x));
            }

            public void RemoveOutputs(params CommodityChange[] outputs)
            {
                foreach (CommodityChange output in outputs)
                {
                    mOutputs.Remove(output);
                }
            }

            public void RemoveOutputs(Predicate<CommodityChange> predicate = null)
            {
                mOutputs.RemoveAll(predicate ?? (x => true));
            }

            public void RemovePotentialTraits(Predicate<TraitNames> predicate = null)
            {
                mPotentialTraits.RemoveAll(x => predicate == null || predicate((TraitNames)x));
            }

            public void RemovePotentialTraits(params TraitNames[] traits)
            {
                foreach (TraitNames trait in traits)
                {
                    mPotentialTraits.Remove((ulong)trait);
                }
            }

            public void RemoveSkills(Predicate<SkillLevelPair> predicate = null)
            {
                mSkills.RemoveAll(predicate ?? (x => true));
            }

            public void RemoveSkills(params SkillLevelPair[] skills)
            {
                foreach (SkillLevelPair skill in skills)
                {
                    Skills.Remove(skill);
                }
            }

            public void RemoveTraits(Predicate<TraitNames> predicate = null)
            {
                mTraits.RemoveAll(x => predicate == null || predicate((TraitNames)x));
            }

            public void RemoveTraits(params TraitNames[] traits)
            {
                foreach (TraitNames trait in traits)
                {
                    mTraits.Remove((ulong)trait);
                }
            }
        }

        public static readonly Dictionary<string, CustomService> CustomInstances = new Dictionary<string, CustomService>();

        public static readonly List<Type> LoadedTypes = new List<Type>();

        public static readonly Dictionary<string, Service> PredefinedInstances = new Dictionary<string, Service>();

        public static readonly Dictionary<Type, CommodityKind> ServiceMotives = new Dictionary<Type, CommodityKind>();

        [PersistableStatic(true)]
        public static List<IServiceProfile> ServiceProfiles = new List<IServiceProfile>();

        /// <summary>
        /// Adds a custom service with the specified profile to the savegame, requestable via the <see cref="Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Interactions.CallForServices"/> interaction.
        /// </summary>
        public static void AddServiceToSaveGame(this IServiceProfile profile)
        {
            if (CanAddServiceToSaveGame(profile))
            {
                ServiceProfiles.Add(profile);
                CustomService.Init(profile);
            }
        }

        /// <summary>
        /// Gets whether a custom service with the specified profile name can be added to the savegame.
        /// </summary>
        public static bool CanAddServiceToSaveGame(string name)
        {
            return !CustomInstances.ContainsKey(name) && !ServiceProfiles.Exists(x => x.Name == name);
        }

        /// <summary>
        /// Gets whether a custom service with the specified profile can be added to the savegame.
        /// </summary>
        public static bool CanAddServiceToSaveGame(this IServiceProfile profile)
        {
            return !CustomInstances.ContainsKey(profile.Name) && !ServiceProfiles.Contains(profile);
        }

        /// <summary>
        /// Gets whether a custom service with the specified profile name can be removed from the savegame.
        /// </summary>
        public static bool CanRemoveServiceFromSaveGame(string name)
        {
            CustomService service;
            return CustomInstances.TryGetValue(name, out service) && ServiceProfiles.Contains(service.Profile);
        }

        /// <summary>
        /// Gets whether a custom service with the specified profile can be removed from the savegame.
        /// </summary>
        public static bool CanRemoveServiceFromSaveGame(this IServiceProfile profile)
        {
            return CanRemoveServiceFromSaveGame(profile.Name);
        }

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
            return typeof(Sims3.Gameplay.Interfaces.zoeoeAndDestrospean.ServantRolesMod.IService).IsAssignableFrom(type);
        }

        /// <summary>
        /// Removes a custom service with the specified profile name from the savegame.
        /// </summary>
        public static void RemoveServiceFromSaveGame(string name)
        {
            if (CanRemoveServiceFromSaveGame(name))
            {
                CustomService.Deinit(CustomInstances[name].Profile);
                ServiceProfiles.Remove(CustomInstances[name].Profile);
            }
        }

        /// <summary>
        /// Removes a custom service with the specified profile from the savegame.
        /// </summary>
        public static void RemoveServiceFromSaveGame(this IServiceProfile profile)
        {
            RemoveServiceFromSaveGame(profile.Name);
        }

        /// <summary>
        /// Opens a series of dialogs to add an output to an interaction for a service motive of the specified profile.
        /// </summary>
        /// <returns><c>true</c>, if the an output was added, <c>false</c> otherwise.</returns>
        public static bool TryUIAddOutput(this IServiceProfile profile)
        {
            string entryKey = typeof(UI.Dialogs.ObjectPickerDialog).GetLocalizationKey();
            entryKey = entryKey.Remove(entryKey.LastIndexOf('/'));
            Type[] interactionDefinitionTypes = null;
            Type[] targetTypes = null;
            string advertised = null;
            string actual = null;
            bool locked = false;
            OutputUpdateType updateType = 0;
            byte step = 0;
            while (true)
            {
                if (step == 0)
                {
                    if (!CommonUtils.TryUIGetSelectedTypes(out interactionDefinitionTypes, InteractionObjectTypeUtils.InteractionDefinitionTypes, Localization.LocalizeString(entryKey + "/NamespaceListDialog/Titles:InteractionDefinition"), Localization.LocalizeString(entryKey + "/TypeListDialog/Titles:InteractionDefinition")))
                    {
                        return false;
                    }
                    step++;
                }
                if (step == 1)
                {
                    if (!CommonUtils.TryUIGetSelectedTypes(out targetTypes, InteractionObjectTypeUtils.GameObjectTypes, Localization.LocalizeString(entryKey + "/NamespaceListDialog/Titles:Target"), Localization.LocalizeString(entryKey + "/TypeListDialog/Titles:Target")))
                    {
                        step--;
                        continue;
                    }
                    step++;
                }
                if (step == 2)
                {
                    advertised = StringInputDialog.Show(Localization.LocalizeString(entryKey + "/AdvertisedValueDialog:Title"), Localization.LocalizeString(entryKey + "/AdvertisedValueDialog:Prompt"), "200", -1, ThumbnailKey.kInvalidThumbnailKey, new Vector2(-1f, -1f), StringInputDialog.Validation.FloatNumber, false, ModalDialog.PauseMode.PauseSimulator, false, true);
                    if (advertised == null)
                    {
                        step--;
                        continue;
                    }
                    step++;
                }
                if (step == 3)
                {
                    actual = StringInputDialog.Show(Localization.LocalizeString(entryKey + "/ActualValueDialog:Title"), Localization.LocalizeString(entryKey + "/ActualValueDialog:Prompt"), "200", -1, ThumbnailKey.kInvalidThumbnailKey, new Vector2(-1f, -1f), StringInputDialog.Validation.FloatNumber, false, ModalDialog.PauseMode.PauseSimulator, false, true);
                    if (actual == null)
                    {
                        step--;
                        continue;
                    }
                    step++;
                }
                if (step == 4)
                {
                    if (!CommonUtils.TryUIGetBooleanValue(Localization.LocalizeString(entryKey + "/LockedDialog:Title"), out locked))
                    {
                        step--;
                        continue;
                    }
                    step++;
                }
                if (step == 5)
                {
                    if (!CommonUtils.TryUIGetUpdateType(Localization.LocalizeString(entryKey + "/UpdateTypeDialog:Title"), out updateType))
                    {
                        step--;
                        continue;
                    }
                    step++;
                }
                profile.RemoveOutputs(x => x.InteractionDefinitionType == interactionDefinitionTypes[0].FullName && x.TargetType == targetTypes[0].FullName);
                profile.AddOutputs(new CommodityChange(interactionDefinitionTypes[0], targetTypes[0], ParserFunctions.ParseFloat(advertised, 200f), locked, ParserFunctions.ParseFloat(actual, 200f), updateType));
                return true;
            }
        }

        /// <summary>
        /// Opens a dialog to create a service profile.
        /// </summary>
        /// <returns><c>true</c>, if a service profile was created, <c>false</c> otherwise.</returns>
        public static bool TryUICreateServiceProfile(out IServiceProfile profile)
        {
            profile = null;
            string entryKey = typeof(UI.Dialogs.ObjectPickerDialog).GetLocalizationKey();
            entryKey = entryKey.Remove(entryKey.LastIndexOf('/')) + "/CreateServiceProfileDialog";
            List<string> results = TwoStringInputDialog.Show(Localization.LocalizeString(entryKey + ":Title"), Localization.LocalizeString(entryKey + "/Prompts:FirstPrompt"), Localization.LocalizeString(entryKey + "/Prompts:SecondPrompt"), "", "", Localization.LocalizeString("Ui/Caption/Global:Accept"), Localization.LocalizeString("Ui/Caption/Global:Cancel"));
            if (results == null)
            {
                return false;
            }
            if (string.IsNullOrEmpty(results[0]))
            {
                SimpleMessageDialog.Show(Localization.LocalizeString(entryKey + ":ServiceCreationFailed"), Localization.LocalizeString(entryKey + ":NameEmpty"));
                return false;
            }
            if (string.IsNullOrEmpty(results[1]))
            {
                SimpleMessageDialog.Show(Localization.LocalizeString(entryKey + ":ServiceCreationFailed"), Localization.LocalizeString(entryKey + ":TitleEmpty"));
                return false;
            }
            if (!CanAddServiceToSaveGame(results[0]))
            {
                SimpleMessageDialog.Show(Localization.LocalizeString(entryKey + ":ServiceCreationFailed"), Localization.LocalizeString(entryKey + ":NotUnique"));
                return false;
            }
            profile = new ServiceProfile(results[0], results[1])
                {
                    Actions = new List<ServiceUtils.ActiveTopicAction>
                        {
                            new ServiceUtils.ActiveTopicAction("Dismiss"),
                            new ServiceUtils.ActiveTopicAction("Fire")
                        }
                };
            return true;
        }

        /// <summary>
        /// Opens a series of dialogs to remove an output from an interaction for a service motive of the specified profile.
        /// </summary>
        /// <returns><c>true</c>, if the an output was removed, <c>false</c> otherwise.</returns>
        public static bool TryUIRemoveOutput(this IServiceProfile profile)
        {
            string entryKey = typeof(UI.Dialogs.ObjectPickerDialog).GetLocalizationKey();
            entryKey = entryKey.Remove(entryKey.LastIndexOf('/'));
            Type[] interactionDefinitionTypes, targetTypes;
            while (true)
            {
                if (!CommonUtils.TryUIGetSelectedTypes(out interactionDefinitionTypes, Array.FindAll(InteractionObjectTypeUtils.InteractionDefinitionTypes, x => profile.Outputs.Exists(y => y.InteractionDefinitionType == x.FullName)), Localization.LocalizeString(entryKey + "/NamespaceListDialog/Titles:InteractionDefinition"), Localization.LocalizeString(entryKey + "/TypeListDialog/Titles:InteractionDefinition")))
                {
                    return false;
                }
                if (!CommonUtils.TryUIGetSelectedTypes(out targetTypes, Array.FindAll(InteractionObjectTypeUtils.GameObjectTypes, x => profile.Outputs.Exists(y => y.TargetType == x.FullName && Array.Exists(interactionDefinitionTypes, z => z.FullName == y.InteractionDefinitionType))), Localization.LocalizeString(entryKey + "/NamespaceListDialog/Titles:Target"), Localization.LocalizeString(entryKey + "/TypeListDialog/Titles:Target")))
                {
                    continue;
                }
                profile.RemoveOutputs(x => x.InteractionDefinitionType == interactionDefinitionTypes[0].FullName && x.TargetType == targetTypes[0].FullName);
                return true;
            }
        }

        /// <summary>
        /// Opens a dialog to set feedback messages for Sims requesting and cancelling services.
        /// </summary>
        /// <returns><c>true</c>, if feedback messages were set, <c>false</c> otherwise.</returns>
        public static bool TryUISetMessages(this IServiceProfile profile)
        {
            string entryKey = typeof(UI.Dialogs.ObjectPickerDialog).GetLocalizationKey();
            entryKey = entryKey.Remove(entryKey.LastIndexOf('/')) + "/SetMessagesDialog";
            string[] results = ThreeStringInputDialog.Show(Localization.LocalizeString(entryKey + ":Title"), new string[]
                {
                    Localization.LocalizeString(entryKey + "/Prompts:SetRequestedMessage"),
                    Localization.LocalizeString(entryKey + "/Prompts:SetCancelledMessage"),
                    Localization.LocalizeString(entryKey + "/Prompts:SetCancelledWhileActiveMessage")
                },
                new string[]
                {
                    profile.RequestedMessage,
                    profile.CancelledMessage,
                    profile.CancelledWhileActiveMessage
                }, false);
            if (results == null)
            {
                return false;
            }
            profile.RequestedMessage = results[0];
            profile.CancelledMessage = results[1];
            profile.CancelledWhileActiveMessage = results[2];
            return true;
        }
    }
}
