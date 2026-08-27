using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Objects.Electronics;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Services;
using Sims3.SimIFace;

namespace Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Interactions
{
    public class RequestHousekeeper : ImmediateInteraction<Sim, Phone>
    {
        [DoesntRequireTuning]
        public class Definition : ImmediateInteractionDefinition<Sim, Phone, RequestHousekeeper>
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
                if (Housekeeper.Instance != null && (Housekeeper.Instance.IsServiceRequested(target.LotCurrent) || Housekeeper.Instance.IsAnySimActiveOnLot(target.LotCurrent)))
                {
                    greyedOutTooltipCallback = () => LocalizeString("/Phone/Tooltips:AlreadyRequested");
                    return false;
                }
                return true;
                */
                return Housekeeper.Instance != null && !Housekeeper.Instance.IsServiceRequested(target.LotCurrent) && !Housekeeper.Instance.IsAnySimActiveOnLot(target.LotCurrent);
            }
        }

        public static readonly InteractionDefinition Singleton = new Definition();

        public static string LocalizeString(string entryKey, params object[] parameters)
        {
            return Localization.LocalizeString(typeof(RequestHousekeeper).GetLocalizationKey() + entryKey, parameters);
        }

        public override bool Run()
        {
            if (Housekeeper.Instance == null)
            {
                Housekeeper.Create();
            }
            Housekeeper.Instance.MakeServiceRequest(Target.LotCurrent, true, Actor.ObjectId, false, 1);
            return true;
        }
    }
}
