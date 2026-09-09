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
using Sims3.Gameplay.Socializing;
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

            public ActiveTopicAction()
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
        public class ServiceProfile
        {
            List<ulong> mHiddenTraits = new List<ulong>();

            List<int> mMotives = new List<int>();

            List<ulong> mPotentialTraits = new List<ulong>();

            int mServiceMotive = 0;

            List<ulong> mSkills = new List<ulong>();

            List<ulong> mTraits = new List<ulong>();

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
            public ServiceTuning ServiceTuning = new ServiceTuning();

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

            public bool WaitsBeforePuttingAwayLeftovers = false;

            public ServiceProfile()
            {
            }

            public ServiceProfile(string name, string title, string cancelledServiceTitle = null, CommodityKind? serviceMotive = null, List<CommodityKind> motives = null, List<CommodityChange> outputs = null, List<TraitNames> traits = null, List<TraitNames> hiddenTraits = null, List<TraitNames> potentialTraits = null, int potentialTraitCount = 0, List<SkillNames> skills = null)
            {
                Name = name;
                Title = title;
                CancelledServiceTitle = cancelledServiceTitle ?? title;
                ServiceMotive = serviceMotive ?? CommonUtils.GetCommodityKind("Be" + name, CommodityKindType.Motive);
                Motives = motives ?? new List<CommodityKind>();
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

        public new class SetUnsetServiceBed : Service<CustomService>.SetUnsetServiceBed
        {
            public new class Definition : Service<CustomService>.SetUnsetServiceBed.Definition
            {
                ServiceProfile mServiceProfile;

                public Definition(ServiceProfile serviceProfile)
                {
                    mServiceProfile = serviceProfile;
                }

                public override string GetInteractionName(Sim actor, Bed target, InteractionObjectPair iop)
                {
                    CustomService service;
                    if (!ServiceUtils.CustomServices.TryGetValue(mServiceProfile.Name, out service) || service == null)
                    {
                        return mServiceProfile.Title;
                    }
                    List<Sim> simsAssignedToLot = service.GetSimsAssignedToLot(actor.LotHome);
                    return Localization.LocalizeString(actor.IsFemale, DerivedType.GetLocalizationKey() + "/" + typeof(SetUnsetServiceBed).Name + (target.FindOwnedBed(simsAssignedToLot[0]) == target ? ":Unset" : ":Set") + "InteractionName", simsAssignedToLot[0].SimDescription, mServiceProfile.Title);
                }

                public override bool Test(Sim actor, Bed target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    Lot lotHome = actor.LotHome;
                    if (lotHome != null)
                    {
                        if (target.LotCurrent != lotHome)
                        {
                            return false;
                        }
                        CustomService service;
                        if (ServiceUtils.CustomServices.TryGetValue(mServiceProfile.Name, out service) && service != null)
                        {
                            List<Sim> simsAssignedToLot = service.GetSimsAssignedToLot(lotHome);
                            if (simsAssignedToLot.Count > 0)
                            {
                                Sim owner = simsAssignedToLot[0];
                                Bed bed = target.FindOwnedBed(owner);
                                if (bed == null || bed == target)
                                {
                                    return target.CanBeUsedAsBed;
                                }
                            }
                        }
                    }
                    return false;
                }
            }
        }

        const string kCustomServiceBook = "HowToServeAndNotBeServed";

        public float CheckTime
        {
            get
            {
                return Profile.CheckTime;
            }
        }

        public float DelayBeforeArriving
        {
            get
            {
                return Profile.DelayBeforeArriving;
            }
        }

        public float DelayBeforeLeaving
        {
            get
            {
                return Profile.DelayBeforeLeaving;
            }
        }

        public float DriveTime
        {
            get
            {
                return Profile.DriveTime;
            }
        }

        public float ExtraWaitTimeAfterSocializing
        {
            get
            {
                return Profile.ExtraWaitTimeAfterSocializing;
            }
        }

        public override bool IsQuietAroundSleepingSims
        {
            get
            {
                return Profile.IsQuietAroundSleepingSims;
            }
        }

        /// <summary>
        /// Outputs for interactions and their target types that the service motive of the service is inserted into,
        /// </summary>
        public override List<CommodityChange> Outputs
        {
            get
            {
                return Profile.Outputs;
            }
        }

        public ServiceProfile Profile;

        public float RelationshipLevelForQuit
        {
            get
            {
                return Profile.RelationshipLevelForQuit;
            }
        }

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

        public SetUnsetServiceBed.Definition SetUnsetServiceBedInstance;

        public float TimeWaitBeforePutawayLeftovers
        {
            get
            {
                return Profile.TimeWaitBeforePutawayLeftovers;
            }
        }

        public override ServiceTuning Tuning
        {
            get
            {
                return Profile.ServiceTuning;
            }
        }

        public float UseObjectInSameRoomAsSleeperMultiplier
        {
            get
            {
                return Profile.UseObjectInSameRoomAsSleeperMultiplier;
            }
        }

        public override bool WaitsBeforePuttingAwayLeftovers
        {
            get
            {
                return Profile.WaitsBeforePuttingAwayLeftovers;
            }
        }

        public CustomService(ServiceProfile profile)
        {
            Profile = profile;
            ServiceUtils.CustomServices[profile.Name] = this;
            SetUnsetServiceBedInstance = new SetUnsetServiceBed.Definition(profile);
        }

        protected new void AddInteractions(Bed bed)
        {
            DebugUtils.TryDisplayScriptError(() => bed.AddInteraction(SetUnsetServiceBedInstance, true));
        }

        protected new void OnObjectPlacedInLot(object sender, EventArgs e)
        {
            DebugUtils.TryDisplayScriptError(() =>
                {
                    World.OnObjectPlacedInLotEventArgs onObjectPlacedInLotEventArgs = e as World.OnObjectPlacedInLotEventArgs;
                    if (onObjectPlacedInLotEventArgs != null)
                    {
                        GameObject gameObject = GameObject.GetObject(onObjectPlacedInLotEventArgs.mObjectId);
                        if (Profile.IsLiveInService)
                        {
                            Bed bed = gameObject as Bed;
                            if (bed != null)
                            {
                                AddInteractions(bed);
                            }
                        }
                    }
                });
        }

        public override void AddOutputs()
        {
            foreach (CommodityChange output in Outputs)
            {
                ServiceMotive.AddAsOutput(output.InteractionDefinitionType, output.TargetType, output.ConstantChange, output.Locked, output.ActualValue, output.UpdateType, output.TimeDependsOnCommodityFilling, output.UpdateEvenOnFailure, output.UpdateAboveAndBelowZero);
            }
        }

        public static void Create(ServiceProfile profile)
        {
            DebugUtils.TryDisplayScriptError(() =>
                {
                    if (ServiceNPCSpecifications.ValidForCurrentWorld(ServiceTypeStatic))
                    {
                        CustomService service;
                        if (ServiceUtils.CustomServices.TryGetValue(profile.Name, out service) && service != null)
                        {
                            service.PostLoadFixup();
                        }
                        else
                        {
                            new CustomService(profile);
                        }
                    }
                    else if (ServiceUtils.CustomServices.ContainsKey(profile.Name))
                    {
                        Destroy(ServiceUtils.CustomServices[profile.Name]);
                        ServiceUtils.CustomServices.Remove(profile.Name);
                    }
                });
        }

        public override string GetServiceTopic(Sim serviceSim)
        {
            return Profile.Title + " Service";
        }

        public static void Init(ServiceProfile profile)
        {
            DebugUtils.TryDisplayScriptError(() =>
                {
                    if (!profile.IsLoaded)
                    {
                        CommonUtils.AddEnumValue<CommodityKind>("Be" + profile.Name, profile.ServiceMotive);
                        LoadServiceMotive(profile.ServiceMotive);
                        string activeTopic = profile.Title + " Service";
                        if (!ActiveTopicData.Exists(activeTopic))
                        {
                            ActiveTopicData.Add(new ActiveTopicData(activeTopic, false, 1000, "", true, true, false, true, null, 0f, "", false));
                        }
                        foreach (ActiveTopicAction action in profile.Actions)
                        {
                            CommonUtils.AddActions(activeTopic, action.Grouping, action.IsActive, action.Name);
                        }
                        profile.IsLoaded = true;
                    }
                    Create(profile);
                    CustomService service;
                    if (ServiceUtils.CustomServices.TryGetValue(profile.Name, out service) && service != null)
                    {
                        if (profile.IsLiveInService)
                        {
                            foreach (Bed bed in Sims3.Gameplay.Queries.GetObjects<Bed>())
                            {
                                service.AddInteractions(bed);
                            }
                        }
                        World.OnObjectPlacedInLotEventHandler += service.OnObjectPlacedInLot;
                        service.AddOutputs();
                        IEnumerator<SimDescription> enumerator = service.Pool.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            if (enumerator.Current != null && enumerator.Current.CreatedSim != null)
                            {
                                CommonUtils.UpdateMotiveTunings(enumerator.Current.CreatedSim, service.ServiceMotive);
                            }
                        }
                    }
                });
            World.sOnWorldQuitEventHandler += (sender, e) =>
                {
                    CustomService service;
                    if (ServiceUtils.CustomServices.TryGetValue(profile.Name, out service) && service != null)
                    {
                        service.RemoveOutputs();
                        string activeTopic = service.GetServiceTopic(null);
                        foreach (ActiveTopicAction action in profile.Actions)
                        {
                            CommonUtils.RemoveActions(activeTopic, action.Grouping, action.IsActive, action.Name);
                        }
                        if (ActiveTopicData.Exists(activeTopic))
                        {
                            ActiveTopicData.sData.Remove(activeTopic);
                        }
                        ServiceUtils.CustomServices.Remove(profile.Name);
                    }
                };
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

        public override void RemoveOutputs()
        {
            foreach (CommodityChange output in Outputs)
            {
                ServiceMotive.RemoveAsOutput(output.InteractionDefinitionType, output.TargetType, output.ConstantChange, output.Locked, output.ActualValue, output.UpdateType, output.TimeDependsOnCommodityFilling, output.UpdateEvenOnFailure, output.UpdateAboveAndBelowZero);
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
            DebugUtils.TryDisplayScriptError(() =>
                {
                    foreach (TraitNames traitName in Profile.Traits)
                    {
                        simDescription.TraitManager.AddElement(traitName);
                    }
                    foreach (TraitNames traitName in Profile.HiddenTraits)
                    {
                        simDescription.TraitManager.AddHiddenElement(traitName);
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
