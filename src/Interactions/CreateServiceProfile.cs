using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces.Destrospean.ExpandedHouseholdStaff;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Destrospean.Utils;
using Destrospean.Utils.ExpandedHouseholdStaff;

namespace Sims3.Gameplay.Destrospean.ExpandedHouseholdStaff.Interactions
{
    public class CreateServiceProfile : ImmediateInteraction<Sim, Sim>
    {
        [DoesntRequireTuning]
        public class Definition : ImmediateInteractionDefinition<Sim, Sim, CreateServiceProfile>
        {
            public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
            {
                return Localization.LocalizeString(target.IsFemale, sLocalizationKey + ":Name", actor.FirstName, target.FirstName);
            }

            public override string[] GetPath(bool isFemale)
            {
                return new[]
                {
                    Localization.LocalizeString(isFemale, sLocalizationKey + ":Path")
                };
            }

            public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                return actor == target && !isAutonomous;
            }
        }

        static readonly string sLocalizationKey = typeof(CreateServiceProfile).GetLocalizationKey();

        public static InteractionDefinition Singleton = new Definition();

        public override bool Run()
        {
            IServiceProfile profile;
            if (ServiceUtils.TryUICreateServiceProfile(out profile))
            {
                ServiceUtils.AddServiceToSaveGame(profile);
            }
            return true;
        }
    }
}
