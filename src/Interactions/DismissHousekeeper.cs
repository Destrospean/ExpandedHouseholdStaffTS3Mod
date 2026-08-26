using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Objects.Electronics;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Services;
using Sims3.SimIFace;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Situations;

namespace Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Interactions
{
    public class DismissHousekeeper : ImmediateInteraction<Sim, Phone>
    {
        [DoesntRequireTuning]
        public class Definition : ImmediateInteractionDefinition<Sim, Phone, DismissHousekeeper>
        {
            public override string GetInteractionName(Sim actor, Phone target, InteractionObjectPair iop)
            {
                return LocalizeString(":Name");
            }

            public override string[] GetPath(bool isFemale)
            {
                return new[]
                {
                    LocalizeString(":Path")
                };
            }

            public override bool Test(Sim actor, Phone target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                /*
                if (Housekeeper.Instance != null && !Housekeeper.Instance.IsServiceRequested(target.LotCurrent) && !Housekeeper.Instance.IsAnySimActiveOnLot(target.LotCurrent))
                {
                    greyedOutTooltipCallback = () => LocalizeString("/Phone/Tooltips:AlreadyCancelled");
                    return false;
                }
                */
                return Housekeeper.Instance != null && (Housekeeper.Instance.IsServiceRequested(target.LotCurrent) || Housekeeper.Instance.IsAnySimActiveOnLot(target.LotCurrent));
                //return true;
            }
        }

        public static readonly InteractionDefinition Singleton = new Definition();

        public override bool Run()
        {
            Housekeeper.Instance.EndService(Housekeeper.Instance.FindSimForAssignment(Target.LotCurrent));
            return true;
        }

        public static string LocalizeString(string entryKey, params object[] parameters)
        {
            return Localization.LocalizeString(typeof(DismissHousekeeper).GetLocalizationKey() + entryKey, parameters);
        }
    }
}
