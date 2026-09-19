using Sims3.Gameplay.Abstracts;
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

namespace Destrospean.ExpandedHouseholdStaff.Interactions
{
    public class EditServiceProfile : ImmediateInteraction<Sim, GameObject>
    {
        [DoesntRequireTuning]
        public class Definition : ImmediateInteractionDefinition<Sim, GameObject, EditServiceProfile>
        {
            public override string GetInteractionName(Sim actor, GameObject target, InteractionObjectPair iop)
            {
                return Localization.LocalizeString(actor.IsFemale, LocalizationKey + ":Name");
            }

            public override string[] GetPath(bool isFemale)
            {
                return new[]
                {
                    Localization.LocalizeString(isFemale, LocalizationKey + ":Path")
                };
            }

            public override bool Test(Sim actor, GameObject target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                return !isAutonomous && ServiceUtils.ServiceProfiles.Exists(x => !x.IsImmutable);
            }
        }

        public static readonly string LocalizationKey = typeof(EditServiceProfile).GetLocalizationKey();

        public static InteractionDefinition Singleton = new Definition();

        public override bool Run()
        {
            IServiceProfile[] profiles;
            if (ServiceUtils.TryUIGetSelectedServiceProfiles(out profiles, ServiceUtils.ServiceProfiles.FindAll(x => !x.IsImmutable).ToArray(), Localization.LocalizeString(LocalizationKey + ":Name"), 1))
            {
                string entryKey = typeof(ObjectPickerDialog).GetLocalizationKey().Replace("ObjectPickerDialog", "EditServiceProfileDialog");
                profiles[0].Title = StringInputDialog.Show(Localization.LocalizeString(entryKey + ":Title"), Localization.LocalizeString(entryKey + ":Prompt"), profiles[0].Title, -1, ThumbnailKey.kInvalidThumbnailKey, new Vector2(-1, -1), StringInputDialog.Validation.None, false, ModalDialog.PauseMode.PauseSimulator, false, true) ?? profiles[0].Title;
                profiles[0].EditServiceProfile();
            }
            return true;
        }
    }
}
