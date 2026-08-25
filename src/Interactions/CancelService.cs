using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Objects.Electronics;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Services;
using Sims3.SimIFace;

namespace Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Interactions
{
    public class CancelHousekeeper : ImmediateInteraction<Sim, Phone>
    {
        [DoesntRequireTuning]
        public class Definition : ImmediateInteractionDefinition<Sim, Phone, CancelHousekeeper>
        {
            public override string GetInteractionName(Sim actor, Phone target, InteractionObjectPair iop)
            {
                return LocalizeString("/Phone:CancelService");
            }

            public override string[] GetPath(bool isFemale)
            {
                return new[]
                {
                    LocalizeString("/Phone:CallServices")
                };
            }

            public override bool Test(Sim actor, Phone target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                if (Housekeeper.Instance != null && !Housekeeper.Instance.IsServiceRequested(target.LotCurrent) && !Housekeeper.Instance.IsAnySimActiveOnLot(target.LotCurrent))
                {
                    greyedOutTooltipCallback = () => LocalizeString("/Phone/Tooltips:AlreadyCancelled");
                    return false;
                }
                return true;
            }
        }

        public static readonly InteractionDefinition Singleton = new Definition();

        public override bool Run()
        {
            Housekeeper.Instance.DismissWorker();
            return true;
        }

        public static string LocalizeString(string entryKey, params object[] parameters)
        {
            return Localization.LocalizeString(typeof(CancelHousekeeper).GetLocalizationKey() + entryKey, parameters);
        }
    }
}
