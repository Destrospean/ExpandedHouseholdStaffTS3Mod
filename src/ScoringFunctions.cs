using Sims3.Gameplay.Abstracts.zoeoeAndDestrospean.ServantRolesMod;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Interfaces.zoeoeAndDestrospean.ServantRolesMod;
using Sims3.Gameplay.ObjectComponents;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Utilities;
using System.Reflection;

namespace Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod
{
    public class ScoringFunctions
    {
        [ScoringFunction]
        public static float ServantRolesMod_PutAwayLeftOversScoringFunction(Sim Actor, InteractionObjectPair interactionObjectPair)
        {
            PropertyInfo timeWaitBeforePutawayLeftoversProperty = Actor.Service.GetType().GetProperty("TimeWaitBeforePutawayLeftovers");
            if (Actor.Service.ServiceType == ServiceType.Butler && !typeof(Service<>).IsAssignableFrom(Actor.Service.GetType()) || Actor.Service is IWaitToPutAwayLeftOvers && timeWaitBeforePutawayLeftoversProperty != null && timeWaitBeforePutawayLeftoversProperty.PropertyType == typeof(float))
            {
                IPreparedFood preparedFood = interactionObjectPair.Target as IPreparedFood;
                return preparedFood != null && SimClock.ElapsedTime(TimeUnit.Minutes) - preparedFood.TimeOfCreation <= (float)timeWaitBeforePutawayLeftoversProperty.GetValue(null, null) ? 0 : 1;
            }
            return CleanableComponent.CleaningScoringFunction(Actor, interactionObjectPair);
        }
    }
}
