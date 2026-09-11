using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Services;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using zoeoeAndDestrospean.Misc;
using zoeoeAndDestrospean.Utils.ServantRolesMod;
using System;
using System.Collections.Generic;

namespace Sims3.Gameplay.Interfaces.zoeoeAndDestrospean.ServantRolesMod
{
    [Persistable]
    public interface IServiceProfile 
    {
        /// <summary>
        /// The actions for the service topic.
        /// </summary>
        List<ServiceUtils.ActiveTopicAction> Actions
        {
            get;
            set;
        }

        /// <summary>
        /// The message that shows when the service is cancelled.
        /// </summary>
        string CancelledMessage
        {
            get;
            set;
        }

        /// <summary>
        /// The message that shows when the service cancelled is there is already a service NPC of that service on the lot.
        /// </summary>
        string CancelledWhileActiveMessage
        {
            get;
            set;
        }

        /// <summary>
        /// The instance name of the car the service NPC arrives in. If <c>null</c> or empty, the service NPC will arrive and leave by foot.
        /// </summary>
        string CarInstanceName
        {
            get;
            set;
        }

        /// <summary>
        /// The product version of the car the service NPC arrives in. This is for when car is a Store, expansion pack, or stuff pack item.
        /// </summary>
        ProductVersion CarProductVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Length of time (in minutes) between checks that everything is done.
        /// </summary>
        float CheckTime
        {
            get;
            set;
        }

        /// <summary>
        /// Length of time (in hours) that the service NPC waits before routing to lot.
        /// </summary>
        float DelayBeforeArriving
        {
            get;
            set;
        }

        /// <summary>
        /// Length of time (in hours) that the service NPC waits before leaving the lot, after their work is done.
        /// </summary>
        float DelayBeforeLeaving
        {
            get;
            set;
        }

        /// <summary>
        /// Length of time (in minutes) that the service NPC takes to drive to lot.
        /// </summary>
        float DriveTime
        {
            get;
            set;
        }

        /// <summary>
        /// Extra time (in hours) to wait before leaving if the service NPC is socialized with.
        /// </summary>
        float ExtraWaitTimeAfterSocializing
        {
            get;
            set;
        }

        /// <summary>
        /// The list of hidden traits for the service NPC.
        /// </summary>
        List<TraitNames> HiddenTraits
        {
            get;
            set;
        }

        /// <summary>
        /// The items the service NPC spawns with.
        /// </summary>
        List<IGameObject> Inventory
        {
            get;
            set;
        }

        /// <summary>
        /// If set to <c>true</c>, the service NPC will stay with the household that requested them. Interactions for setting/unsetting their bed will be available to service NPCs with this property set to <c>true</c>.
        /// </summary>
        bool IsLiveInService
        {
            get;
            set;
        }

        /// <summary>
        /// If set to <c>true</c>, the service NPC will avoid interacting with objects in the same room as a sleeping Sim.
        /// </summary>
        bool IsQuietAroundSleepingSims
        {
            get;
            set;
        }

        /// <summary>
        /// If set to <c>true</c>, the service NPC will quit when they are in close proximity to Bonehilda, and will scream and run away dramatically.
        /// </summary>
        bool IsScaredOfBonehilda
        {
            get;
            set;
        }

        /// <summary>
        /// The service motives. Do not include the <see cref="ServiceMotive"/> property, as that is automatically adding upon creating an instance of <see cref="zoeoeAndDestrospean.Utils.ServantRolesMod.ServiceUtils.ServiceProfile"/>.
        /// </summary>
        List<CommodityKind> Motives
        {
            get;
            set;
        }

        /// <summary>
        /// The internal name of the service. Must be unique.
        /// </summary>
        string Name
        {
            get;
            set;
        }

        /// <summary>
        /// The outputs added to interaction tunings for the service motive.
        /// </summary>
        List<ServiceUtils.CommodityChange> Outputs
        {
            get;
            set;
        }

        /// <summary>
        /// The number of potential traits to randomly pick from the list of potential traits (see <see cref="PotentialTraits"/>) for the service NPC.
        /// </summary>
        int PotentialTraitCount
        {
            get;
            set;
        }

        /// <summary>
        /// The list of potential traits for the service NPC; will be randomly picked from, the number of which is specified by <see cref="PotentialTraitCount"/>.
        /// </summary>
        List<TraitNames> PotentialTraits
        {
            get;
            set;
        }

        /// <summary>
        /// If the service NPC's relationship with any YAE falls below this level, they will quit.
        /// </summary>
        float RelationshipLevelForQuit
        {
            get;
            set;
        }
            
        /// <summary>
        /// If set to <c>true</c>, the service NPC will call emergency services when there is a fire on the lot they're assigned to.
        /// </summary>
        bool ReportsFires
        {
            get;
            set;
        }
            
        /// <summary>
        /// The message that shows when a service NPC is requested.
        /// </summary>
        string RequestedMessage
        {
            get;
            set;
        }

        /// <summary>
        /// Note: this can (and should) be omitted for the UI for creating/editing service profiles.
        /// </summary>
        CommodityKind ServiceMotive
        {
            get;
            set;
        }

        /// <summary>
        /// Includes cost, whether the service is recurring, whether the service is an emergency service, etc.
        /// </summary>
        Service.ServiceTuning ServiceTuning
        {
            get;
            set;
        }

        /// <summary>
        /// Note: this can (and should) be omitted for the UI for creating/editing service profiles.
        /// </summary>
        ServiceType ServiceType
        {
            get;
            set;
        }

        /// <summary>
        /// The skills (and their levels) for the service NPC to start out with.
        /// </summary>
        List<SkillLevelPair> Skills
        {
            get;
            set;
        }

        /// <summary>
        /// How old leftovers can be out in minutes before the service NPC will put it away.
        /// </summary>
        float TimeWaitBeforePutawayLeftovers
        {
            get;
            set;
        }

        /// <summary>
        /// The display name of the service.
        /// </summary>
        string Title
        {
            get;
            set;
        }

        /// <summary>
        /// The list of explicit traits for the service NPC.
        /// </summary>
        List<TraitNames> Traits
        {
            get;
            set;
        }

        /// <summary>
        /// Multiplier for interactions in a room where a Sim is sleeping.
        /// </summary>
        float UseObjectInSameRoomAsSleeperMultiplier
        {
            get;
            set;
        }

        CASAgeGenderFlags ValidAges
        {
            get;
            set;
        }

        CASAgeGenderFlags ValidGenders
        {
            get;
            set;
        }

        bool WaitsBeforePuttingAwayLeftovers
        {
            get;
            set;
        }

        void AddHiddenTraits(params TraitNames[] traits);

        void AddMotives(params CommodityKind[] motives);

        void AddPotentialTraits(params TraitNames[] traits);

        void AddSkills(params SkillLevelPair[] skills);

        void AddTraits(params TraitNames[] traits);

        void RemoveHiddenTraits(Predicate<TraitNames> predicate);

        void RemoveHiddenTraits(params TraitNames[] traits);

        void RemoveMotives(params CommodityKind[] motives);

        void RemoveMotives(Predicate<CommodityKind> predicate);

        void RemovePotentialTraits(Predicate<TraitNames> predicate);

        void RemovePotentialTraits(params TraitNames[] traits);

        void RemoveSkills(Predicate<SkillLevelPair> predicate);

        void RemoveSkills(params SkillLevelPair[] skills);

        void RemoveTraits(Predicate<TraitNames> predicate);

        void RemoveTraits(params TraitNames[] traits);
    }
}
