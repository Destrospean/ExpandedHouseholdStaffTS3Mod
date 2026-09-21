using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Destrospean.Utils;
using Destrospean.Utils.ExpandedHouseholdStaff;

namespace Destrospean.ExpandedHouseholdStaff.Interactions
{
    public class CreateServiceProfilesXml : ImmediateInteraction<Sim, GameObject>
    {
        [DoesntRequireTuning]
        public class Definition : ImmediateInteractionDefinition<Sim, GameObject, CreateServiceProfilesXml>
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
                return !isAutonomous && ServiceUtils.ServiceProfiles.Count > 0;
            }
        }

        public static readonly string LocalizationKey = typeof(CreateServiceProfilesXml).GetLocalizationKey();

        public static InteractionDefinition Singleton = new Definition();

        public override bool Run()
        {
            DebugUtils.TryDisplayScriptError(() =>
                {
                    string xml;
                    if (FileUtils.TryCreateServiceProfilesXml(out xml))
                    {
                        uint fileHandle = 0u;
                        Simulator.CreateExportFile(ref fileHandle, "ServiceProfiles");
                        if (fileHandle != 0u)
                        {
                            CustomXmlWriter customXmlWriter = new CustomXmlWriter(fileHandle);
                            customXmlWriter.WriteToBuffer(xml);
                            customXmlWriter.WriteEndDocument();
                        }
                    }
                });
            return true;
        }
    }
}
