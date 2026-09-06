using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.ObjectComponents;
using Sims3.Gameplay.Objects.Electronics;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Interactions;
using Sims3.SimIFace;
using System;
using System.Reflection;
using zoeoeAndDestrospean.Utils;
using zoeoeAndDestrospean.Utils.ServantRolesMod;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Replacements;

namespace zoeoeAndDestrospean.ServantRolesMod
{
    public class Main
    {
        [Tunable]
        protected static bool kInstantiator = false;

        static Main()
        {
            LoadSaveManager.ObjectGroupsPreLoad += () => Phone.CallForServices.Singleton = CallForServices.Singleton;
            CommonUtils.ReplaceMethod(typeof(SocialComponent).GetMethod("GetAllInteractionsForSim"), typeof(SocialComponentPatch).GetMethod("GetAllInteractionsForSim"));
        }

        [ScoringFunction]
        public static float ServantRolesMod_PutAwayLeftOversScoringFunction(Sim actor, InteractionObjectPair iop)
        {
            Type serviceDataType = actor.Service.GetType();
            bool serviceIsFromThisMod = actor.Service.IsFromServantRolesMod();
            PropertyInfo timeWaitBeforePutawayLeftoversProperty = serviceDataType.GetProperty("TimeWaitBeforePutawayLeftovers");
            if (!serviceIsFromThisMod && actor.Service.ServiceType == ServiceType.Butler || serviceIsFromThisMod && (bool)serviceDataType.GetProperty("WaitsBeforePuttingAwayLeftovers").GetValue(actor.Service, null) && timeWaitBeforePutawayLeftoversProperty != null && timeWaitBeforePutawayLeftoversProperty.PropertyType == typeof(float))
            {
                IPreparedFood preparedFood = iop.Target as IPreparedFood;
                return preparedFood != null && SimClock.ElapsedTime(TimeUnit.Minutes) - preparedFood.TimeOfCreation <= (float)timeWaitBeforePutawayLeftoversProperty.GetValue(null, null) ? 0 : 1;
            }
            return CleanableComponent.CleaningScoringFunction(actor, iop);
        }
    }
}
