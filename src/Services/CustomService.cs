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
    public class CustomLiveInService : CustomService, IAmLiveInService
    {
        public CustomLiveInService(ServiceProfile profile) : base(profile)
        {
        }
    }

    public class CustomService : Service<CustomService>, IAmSociableService
    {
        public class ServiceProfile
        {
            public string CancelledServiceTitle;

            public List<CommodityKind> Motives;

            public string Name;

            public List<CommodityChange> Outputs;

            public int PotentialTraitCount;

            public List<TraitNames> PotentialTraits;

            public CommodityKind ServiceMotive;

            public List<SkillNames> Skills;

            public string Title;

            public List<TraitNames> Traits;

            public bool IsLiveInService = false;

            public ServiceTuning ServiceTuning = new ServiceTuning();

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

            /// <summary>
            /// If the custom service NPC's relationship with any YAE falls below this level, they will quit.
            /// </summary>
            public float RelationshipLevelForQuit = -50f;

            /// <summary>
            /// How old leftovers can be out in minutes before the custom service NPC will put it away.
            /// </summary>
            public float TimeWaitBeforePutawayLeftovers = 60f;

            public ServiceProfile(string name, string title, string cancelledServiceTitle = null, CommodityKind? serviceMotive = null, List<CommodityKind> motives = null, List<CommodityChange> outputs = null, List<TraitNames> traits = null, List<TraitNames> potentialTraits = null, int potentialTraitCount = 0, List<SkillNames> skills = null)
            {
                Name = name;
                Title = title;
                CancelledServiceTitle = cancelledServiceTitle ?? title;
                ServiceMotive = serviceMotive ?? CommonUtils.GetCommodityKind("Be" + name, CommodityKindType.Motive);
                Motives = motives ?? new List<CommodityKind>();
                Outputs = outputs ?? new List<CommodityChange>();
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

        public new class SetUnsetServiceBed : Service<CustomService>.SetUnsetServiceBed
        {
            public new class Definition : Service<CustomService>.SetUnsetServiceBed.Definition
            {
                string mServiceTitle;

                public Definition(string serviceTitle)
                {
                    mServiceTitle = serviceTitle;
                }

                public override string GetInteractionName(Sim actor, Bed target, InteractionObjectPair iop)
                {
                    if (Instance == null)
                    {
                        return mServiceTitle;
                    }
                    List<Sim> simsAssignedToLot = Instance.GetSimsAssignedToLot(actor.LotHome);
                    return Localization.LocalizeString(actor.IsFemale, DerivedType.GetLocalizationKey() + "/" + typeof(SetUnsetServiceBed).Name + (target.FindOwnedBed(simsAssignedToLot[0]) == target ? ":Unset" : ":Set") + "InteractionName", simsAssignedToLot[0].SimDescription, mServiceTitle);
                }
            }
        }

        const string kCustomServiceBook = "HowToServeAndNotBeServed";

        public override ServiceTuning Tuning
        {
            get
            {
                return Profile.ServiceTuning;
            }
        }

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

        public float RelationshipLevelForQuit
        {
            get
            {
                return Profile.RelationshipLevelForQuit;
            }
        }

        public float TimeWaitBeforePutawayLeftovers
        {
            get
            {
                return Profile.TimeWaitBeforePutawayLeftovers;
            }
        }

        public ServiceProfile Profile;

        public SetUnsetServiceBed.Definition SetUnsetServiceBedInstance;

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
            SetUnsetServiceBedInstance = new SetUnsetServiceBed.Definition(profile.Title);
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
                        if (typeof(IAmLiveInService).IsAssignableFrom(DerivedType))
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
                        else if (profile.IsLiveInService)
                        {
                            new CustomLiveInService(profile);
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

        /// <summary>
        /// Call this method for every class derived from this one within its static constructor.
        /// </summary>
        public static void Init(ServiceProfile profile)
        {
            CustomService service;
            DebugUtils.TryDisplayScriptError(() =>
                {
                    Create(profile);
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
                        service.SetOutputs();
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
                    if (ServiceUtils.CustomServices.ContainsKey(profile.Name))
                    {
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

        public override void SetOutputs()
        {
            foreach (CommodityChange output in Outputs)
            {
                ServiceMotive.AddAsOutput(output.InteractionDefinitionType, output.TargetType, output.ConstantChange, output.Locked, output.ActualValue, output.UpdateType, output.TimeDependsOnCommodityFilling, output.UpdateEvenOnFailure, output.UpdateAboveAndBelowZero);
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
