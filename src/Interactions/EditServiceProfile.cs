using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces.Destrospean.ExpandedHouseholdStaff;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using Destrospean.Utils;
using Destrospean.Utils.ExpandedHouseholdStaff;
using ObjectPickerDialog = Destrospean.UI.Dialogs.ObjectPickerDialog;

namespace Sims3.Gameplay.Destrospean.ExpandedHouseholdStaff.Interactions
{
    public class EditServiceProfile : ImmediateInteraction<Sim, Sim>
    {
        [DoesntRequireTuning]
        public class Definition : ImmediateInteractionDefinition<Sim, Sim, EditServiceProfile>
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
                return actor == target && !isAutonomous && ServiceUtils.ServiceProfiles.FindAll(x => !x.IsImmutable).Count > 0;
            }
        }

        static readonly string sLocalizationKey = typeof(EditServiceProfile).GetLocalizationKey();

        public static InteractionDefinition Singleton = new Definition();

        public override bool Run()
        {
            IServiceProfile[] profiles;
            if (ServiceUtils.TryUIGetSelectedServiceProfiles(out profiles, ServiceUtils.ServiceProfiles.ToArray(), null, 1))
            {
                string entryKey = typeof(ObjectPickerDialog).GetLocalizationKey().Replace(typeof(ObjectPickerDialog).Name, "EditServiceProfileDialog");
                profiles[0].Title = StringInputDialog.Show(Localization.LocalizeString(entryKey + ":Title"), Localization.LocalizeString(entryKey + ":Prompt"), profiles[0].Title, -1, ThumbnailKey.kInvalidThumbnailKey, new Vector2(-1, -1), StringInputDialog.Validation.None, false, ModalDialog.PauseMode.PauseSimulator, false, true) ?? profiles[0].Title;
                profiles[0].EditServiceProfile();
            }
            return true;
        }
    }
}
