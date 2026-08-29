using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Objects.Electronics;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Services;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using System.Collections.Generic;
using Sims3.Gameplay.Services;

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
                return Housekeeper.Instance == null || !Housekeeper.Instance.IsServiceRequested(target.LotCurrent) && !Housekeeper.Instance.IsAnySimActiveOnLot(target.LotCurrent);
            }
        }

        public static readonly InteractionDefinition Singleton = new Definition();

        public static string LocalizeString(string entryKey, params object[] parameters)
        {
            return Localization.LocalizeString(typeof(RequestHousekeeper).GetLocalizationKey() + entryKey, parameters);
        }

        public override bool Run()
        {
            CommonUtils.TryDisplayScriptError(() =>
                {
                    if (Housekeeper.Instance == null)
                    {
                        Housekeeper.Create();
                    }
                    List<SimDescription> pool = new List<SimDescription>(Housekeeper.Instance.Pool);
                    if (pool.Count == 0 || pool.TrueForAll(Housekeeper.Instance.mSituationsAssignedToSims.ContainsKey))
                    {
                        CommonUtils.ShowDebugMessageDialog("Pool check START");
                        SimDescription createdSimDescription = Housekeeper.Instance.CreateOrUpdateServiceNpc(Housekeeper.CreateSimDescription(Housekeeper.Instance, CASAgeGenderFlags.YoungAdult | CASAgeGenderFlags.Adult, CASAgeGenderFlags.Female), Target.LotCurrent);
                        CommonUtils.ShowDebugMessageDialog("Created SimDescription: " + createdSimDescription?.ToString() ?? "NULL");
                        Housekeeper.Instance.AddSimToPool(createdSimDescription);
                        Housekeeper.Instance.mPreferredServiceNpc[Target.LotCurrent.Household.HouseholdId] = createdSimDescription;
                        CommonUtils.ShowDebugMessageDialog("Pool check END");
                    }
                    Housekeeper.Instance.MakeServiceRequest(Target.LotCurrent, true, Actor.ObjectId);
                });
            return true;
        }
    }
}
