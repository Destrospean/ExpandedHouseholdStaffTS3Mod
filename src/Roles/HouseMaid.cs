using Sims3.Gameplay.Abstracts.zoeoeAndDestrospean.ServantRolesMod;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Objects.Fireplaces;

namespace Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Roles
{
    public class HouseMaid : ServantRoleBase
    {
        /*
           To do:
           - Light fires in house
           - Clean toilets
           - Make beds
        */

        public void LightFireplaces()
        {
            Fireplace[] fireplaces = Queries.GetObjects<Fireplace>(mLot);
            Sim actor = mSim.CreatedSim;
            foreach (Fireplace fireplace in fireplaces)
            {
                InteractionInstance lightFire = Fireplace.LightFire.Singleton.CreateInstance(fireplace, actor, new InteractionPriority(InteractionPriorityLevel.High), false, true);
                actor.InteractionQueue.Add(lightFire);
            }
        }

        public override void StartWork()
        {
            base.StartWork();
            LightFireplaces();
        }

        public HouseMaid(SimDescription sim, Lot lot) : base(sim, lot)
        {
        }
    }
}
