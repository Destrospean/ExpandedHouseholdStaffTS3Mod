using Sims3.Gameplay.Actors;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces.zoeoe.ServantRolesMod;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.zoeoe.ServantRolesMod;

namespace Sims3.Gameplay.Abstracts.zoeoe.ServantRolesMod
{
    public abstract class ServantRoleBase : IServantRole
    {
        protected float mGetReadyForWorkLengthMins = 120; // Assume full time for now

        protected Lot mLot;

        protected SimDescription mSim;

        protected DateAndTime mStartWorkTime;

        public ServantRoleBase(SimDescription sim, Lot lot)
        {
            mSim = sim;
            mLot = lot;
        }

        public virtual void GetReadyForWork()
        {
            Sim sim = mSim.CreatedSim;
            DebugUtils.ShowDebugMessageDialog("GetReadyForWork() starting for: " + sim.FullName);

            sim.InteractionQueue.CancelAllInteractions();
            InteractionInstance changeClothes = new Sim.ClothesSpin.Definition(Sim.ClothesChangeReason.GoingToWork, SimIFace.CAS.OutfitCategories.Everyday).CreateInstance(sim, sim, new InteractionPriority(InteractionPriorityLevel.High), false, false);
            mSim.CreatedSim.InteractionQueue.AddInteraction(changeClothes, false);

            StartWork();
        }

        public virtual void StartWork()
        {
        }
    }
}
