using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Abstracts.zoeoeAndDestrospean.ServantRolesMod;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Interfaces.zoeoeAndDestrospean.ServantRolesMod;
using Sims3.Gameplay.Objects.Beds;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Skills;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Situations;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using System;
using System.Collections.Generic;
using System.Reflection;
using zoeoeAndDestrospean.Misc;
using zoeoeAndDestrospean.Utils;
using zoeoeAndDestrospean.Utils.ServantRolesMod;

namespace Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Services
{
    public class CustomService : Service<CustomService>, IAmSociableService
    {
        public new class SetUnsetServiceBed : Service<CustomService>.SetUnsetServiceBed
        {
            public new class Definition : Service<CustomService>.SetUnsetServiceBed.Definition
            {
                ServiceUtils.ServiceProfile mServiceProfile;

                public Definition(ServiceUtils.ServiceProfile serviceProfile)
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
                    if (actor.LotHome != null)
                    {
                        if (target.LotCurrent != actor.LotHome)
                        {
                            return false;
                        }
                        CustomService service;
                        if (ServiceUtils.CustomServices.TryGetValue(mServiceProfile.Name, out service) && service != null)
                        {
                            List<Sim> simsAssignedToLot = service.GetSimsAssignedToLot(actor.LotHome);
                            if (simsAssignedToLot.Count > 0)
                            {
                                Bed bed = target.FindOwnedBed(simsAssignedToLot[0]);
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

        public override bool IsPaidWeekly
        {
            get
            {
                return Profile.IsLiveInService;
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
        public override List<ServiceUtils.CommodityChange> Outputs
        {
            get
            {
                return Profile.Outputs;
            }
        }

        public ServiceUtils.ServiceProfile Profile;

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

        public override ServiceType ServiceType
        {
            get
            {
                return Profile?.ServiceType ?? ServiceType.Maid;
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

        public CustomService(ServiceUtils.ServiceProfile profile)
        {
            Profile = profile;
            ServiceUtils.CustomServices[profile.Name] = this;
            SetUnsetServiceBedInstance = new SetUnsetServiceBed.Definition(profile);
        }

        public new void AddInteractions(Bed bed)
        {
            DebugUtils.TryDisplayScriptError(() => bed.AddInteraction(SetUnsetServiceBedInstance, true));
        }

        public override void AddOutputs()
        {
            foreach (ServiceUtils.CommodityChange output in Outputs)
            {
                ServiceMotive.AddAsOutput(output.InteractionDefinitionType, output.TargetType, output.ConstantChange, output.Locked, output.ActualValue, output.UpdateType, output.TimeDependsOnCommodityFilling, output.UpdateEvenOnFailure, output.UpdateAboveAndBelowZero);
            }
        }

        public static void Create(ServiceUtils.ServiceProfile profile)
        {
            DebugUtils.TryDisplayScriptError(() =>
                {
                    if (ServiceNPCSpecifications.ValidForCurrentWorld(profile.ServiceType))
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

        public static void Deinit(ServiceUtils.ServiceProfile profile, bool removeAllInteractions = true)
        {
            DebugUtils.TryDisplayScriptError(() =>
                {
                    CustomService service;
                    if (ServiceUtils.CustomServices.TryGetValue(profile.Name, out service) && service != null)
                    {
                        service.RemoveOutputs();
                        string activeTopic = profile.Title + " Service";
                        foreach (ServiceUtils.ActiveTopicAction action in profile.Actions)
                        {
                            CommonUtils.RemoveActions(activeTopic, action.Grouping, action.IsActive, action.Name);
                        }
                        if (ActiveTopicData.Exists(activeTopic))
                        {
                            ActiveTopicData.sData.Remove(activeTopic);
                        }
                        ServiceUtils.CustomServices.Remove(profile.Name);
                        if (removeAllInteractions)
                        {
                            foreach (Bed bed in Sims3.Gameplay.Queries.GetObjects<Bed>())
                            {
                                service.RemoveInteractions(bed);
                            }
                        }
                    }
                });
        }

        public override CASAgeGenderFlags GetGenderForNewNpc(Lot lot)
        {
            return Profile.ValidGenders;
        }

        public override string GetServiceTopic(Sim serviceSim)
        {
            return Profile.Title + " Service";
        }

        public static void Init(ServiceUtils.ServiceProfile profile)
        {
            DebugUtils.TryDisplayScriptError(() =>
                {
                    CommonUtils.AddEnumValue<CommodityKind>("Be" + profile.Name, profile.ServiceMotive);
                    LoadServiceMotive(profile.ServiceMotive);
                    string activeTopic = profile.Title + " Service";
                    if (!ActiveTopicData.Exists(activeTopic))
                    {
                        ActiveTopicData.Add(new ActiveTopicData(activeTopic, false, 1000, "", true, true, false, true, null, 0f, "", false));
                    }
                    foreach (ServiceUtils.ActiveTopicAction action in profile.Actions)
                    {
                        CommonUtils.AddActions(activeTopic, action.Grouping, action.IsActive, action.Name);
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
                        World.OnObjectPlacedInLotEventHandler += (sender, e) => DebugUtils.TryDisplayScriptError(() =>
                            {
                                World.OnObjectPlacedInLotEventArgs onObjectPlacedInLotEventArgs = e as World.OnObjectPlacedInLotEventArgs;
                                if (onObjectPlacedInLotEventArgs != null)
                                {
                                    GameObject gameObject = GameObject.GetObject(onObjectPlacedInLotEventArgs.mObjectId);
                                    if (service.Profile.IsLiveInService)
                                    {
                                        Bed bed = gameObject as Bed;
                                        if (bed != null)
                                        {
                                            service.AddInteractions(bed);
                                        }
                                    }
                                }
                            });
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

        public void RemoveInteractions(Bed bed)
        {
            DebugUtils.TryDisplayScriptError(() => bed.RemoveInteractionByType(SetUnsetServiceBedInstance));
        }

        public override void RemoveOutputs()
        {
            foreach (ServiceUtils.CommodityChange output in Outputs)
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

        public override bool ShouldSimBeRemovedFromService(SimDescription sim)
        {
            if (AgingManager.NumDaysBeforeAging(sim) <= kNumDaysBeforeBirthdayToRemoveSim)
            {
                return true;
            }
            if ((sim.AgeAfterInstantiation & Profile.ValidAges) == 0)
            {
                return true;
            }
            if (sim.Household == null || !sim.Household.IsServiceNpcHousehold)
            {
                return true;
            }
            if (sim.DeathStyle != 0)
            {
                return true;
            }
            return false;
        }

        public override void UpdateCreatedSim(Sim sim)
        {
            DebugUtils.TryDisplayScriptError(() =>
                {
                    foreach (SkillLevelPair skillLevelPair in Profile.Skills)
                    {
                        Skill skill = sim.SkillManager.AddElement(skillLevelPair.SkillName);
                        for (int i = 0; i < (skillLevelPair.SkillLevel < 0 ? skill.MaxSkillLevel : skillLevelPair.SkillLevel); i++)
                        {
                            skill.ForceGainPointsForLevelUp();
                        }
                    }
                    if (sim.Inventory != null)
                    {
                        sim.Inventory.DestroyItems();
                        foreach (IGameObject item in Profile.Inventory)
                        {
                            if (!sim.Inventory.TryToAdd(item))
                            {
                                item.Destroy();
                            }
                        }
                    }
                });
        }
    }
}
