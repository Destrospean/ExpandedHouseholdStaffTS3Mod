using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces.zoeoeAndDestrospean.ServantRolesMod;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Roles;
using Sims3.SimIFace;
using System.Collections.Generic;

namespace Sims3.Gameplay.Objects.zoeoeAndDestrospean.ServantRolesMod
{
    public class ServantManager : GameObject
    {
        public List<IServantRole> ServantRoles = new List<IServantRole>();

        public void StartWorkForAllServants()
        {
            foreach (IServantRole role in ServantRoles)
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
                IServantRole maidRole = new HouseMaid(Actor.SimDescription, Target.LotCurrent);
                Target.ServantRoles.Add(maidRole);
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
}
