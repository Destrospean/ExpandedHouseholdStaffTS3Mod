using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.ObjectComponents;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Utilities;
using System;
using System.Reflection;

namespace Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod
{
    public class ScoringFunctions
    {
        [ScoringFunction]
        public static float ServantRolesMod_PutAwayLeftOversScoringFunction(Sim Actor, InteractionObjectPair interactionObjectPair)
        {
            Type serviceType = Actor.Service.GetType();
            bool serviceIsFromThisMod = Actor.Service.IsFromServantRolesMod();
            PropertyInfo timeWaitBeforePutawayLeftoversProperty = serviceType.GetProperty("TimeWaitBeforePutawayLeftovers");
            if (!serviceIsFromThisMod && Actor.Service.ServiceType == ServiceType.Butler || serviceIsFromThisMod && (bool)serviceType.GetProperty("WaitsBeforePuttingAwayLeftovers").GetValue(Actor.Service, null) && timeWaitBeforePutawayLeftoversProperty != null && timeWaitBeforePutawayLeftoversProperty.PropertyType == typeof(float))
            {
                IPreparedFood preparedFood = interactionObjectPair.Target as IPreparedFood;
                return preparedFood != null && SimClock.ElapsedTime(TimeUnit.Minutes) - preparedFood.TimeOfCreation <= (float)timeWaitBeforePutawayLeftoversProperty.GetValue(null, null) ? 0 : 1;
            }
            return CleanableComponent.CleaningScoringFunction(Actor, interactionObjectPair);
        }
    }
}
