using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Interfaces.zoeoeAndDestrospean.ServantRolesMod;
using Sims3.Gameplay.Socializing;
using Sims3.UI.Controller;
using System.Collections.Generic;

namespace Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Replacements
{
    public class SocialComponentPatch : SocialComponent
    {
        public new IEnumerable<InteractionObjectPair> GetAllInteractionsForSim(Sim actor, bool isAutonomous)
        {
            if (IsInServicePreventingSocialization(mSim, actor) && mSim.Service as IAmSociableService == null)
            {
                return GetAllServiceInteractions(actor);
            }
            List<InteractionObjectPair> interactions = new List<InteractionObjectPair>();
            if (actor.IsBeingRiddenBy(mSim))
            {
                return interactions;
            }
            if (isAutonomous)
            {
                interactions.AddRange(GetAllInteractionsForAutonomy(actor));
                return interactions;
            }
            if (mSim.Posture is ISeatedSocialPosture && actor.Posture is ISeatedSocialPosture)
            {
                Relationship relationship = Relationship.Get(mSim, actor, false);
                if (relationship == null || relationship.LTR.CurrentLTR == LongTermRelationshipTypes.Stranger)
                {
                    interactions.AddRange(SocialsForNewConversation(actor, mSim, false));
                    return interactions;
                }
                if (mSim.NeedsToBeGreeted(actor))
                {
                    interactions.AddRange(SocialsForGreeting(actor, mSim));
                    return interactions;
                }
                return new List<InteractionObjectPair>();
            }
            interactions.AddRange(GetAllInteractionsForPieMenu(actor));
            return interactions;
        }
    }
}
