using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Objects.Fireplaces;
using Sims3.Gameplay.Roles;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sims3.Gameplay.zoeoe.ServantRolesMod
{
    public class ServantManager : GameObject
    {
        public List<ServantRole> servantRoles = new List<ServantRole>();

        public void StartWorkForAllServants()
        {
            foreach(ServantRole role in servantRoles)
            {
                role.GetReadyForWork();
            }
        }


        public override void OnStartup()
        {
            base.OnStartup();
            AddInteraction(AddSimToRole.Singleton);
            AddInteraction(StartWork.Singleton);
        }


        public class AddSimToRole : ImmediateInteraction<Sim, ServantManager>
        {
            public static InteractionDefinition Singleton = new Definition();
            public class Definition : ImmediateInteractionDefinition<Sim, ServantManager, AddSimToRole>
            {
                public override bool Test(Sim actor, ServantManager target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return true;
                }
            }
            public override bool Run()
            {
                //ServantRole servantRole = new ServantRole(Actor.SimDescription, Target.LotCurrent);
                ServantRole maidRole = new HouseMaid(Actor.SimDescription, Target.LotCurrent);
                Target.servantRoles.Add(maidRole);
                return true;
                
            }
        }
        public class StartWork : ImmediateInteraction<Sim, ServantManager>
        {
            public static InteractionDefinition Singleton = new Definition();
            public class Definition : ImmediateInteractionDefinition<Sim, ServantManager, StartWork>
            {
                public override bool Test(Sim actor, ServantManager target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return true;
                }
            }
            public override bool Run()
            {
                Target.StartWorkForAllServants();
                return true;

            }
        }

    }
    public class ServantRole
    {
        //assume full time for now

        protected SimDescription mSim;
        protected Lot mLot;
        private float mGetReadyForWorkLengthMins = 120f;
        private DateAndTime mStartWorkTime;

        public ServantRole(SimDescription sim, Lot lot)
        {
            mSim = sim;
            mLot = lot;
        }

        public virtual void GetReadyForWork()
        {
            Sim sim = mSim.CreatedSim;
            Utils.ShowDebugMessageDialog("GetReadyForWork() starting for: " + sim.FullName);
            
            sim.InteractionQueue.CancelAllInteractions();
            InteractionInstance changeClothesInst = new Sim.ClothesSpin.Definition(Sim.ClothesChangeReason.GoingToWork, SimIFace.CAS.OutfitCategories.Everyday).CreateInstance(sim, sim, new InteractionPriority(InteractionPriorityLevel.High), false, false);
            mSim.CreatedSim.InteractionQueue.AddInteraction(changeClothesInst, false);

            StartWork();
        }
        public virtual void StartWork()
        {

        }
    }

    public class HouseMaid : ServantRole
    {
        //light fires in house
        //clean toilets
        //make beds

        private void LightFireplaces()
        {
            Fireplace[] fireplaces = (Fireplace[])Queries.GetObjects(typeof(Fireplace), mLot);
            Sim actor = mSim.CreatedSim;
            foreach(Fireplace fireplace in fireplaces)
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
