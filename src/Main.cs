using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Interfaces.zoeoeAndDestrospean.ServantRolesMod;
using Sims3.Gameplay.ObjectComponents;
using Sims3.Gameplay.Objects;
using Sims3.Gameplay.Objects.Electronics;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Situations;
using Sims3.Gameplay.Skills;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Interactions;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Services;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.UI.Controller;
using System;
using System.Collections.Generic;
using System.Reflection;
using zoeoeAndDestrospean.Misc;
using zoeoeAndDestrospean.Utils;
using zoeoeAndDestrospean.Utils.ServantRolesMod;

namespace zoeoeAndDestrospean.ServantRolesMod
{
    public class Main
    {
        [Tunable]
        static bool kInitializeIncludedServices = true;

        [Tunable]
        static bool kIntegrateNRaasMasterController = true;

        [Tunable]
        static bool kShowDebugMessages = true;

        static Main()
        {
            CommonUtils.ReplaceMethod<SocialComponent, Main>("IsInServicePreventingSocialization");
            DebugUtils.ShowDebugMessages = kShowDebugMessages;
            InteractionObjectTypeUtils.InitTypes();
            if (kIntegrateNRaasMasterController && Array.Exists(AppDomain.CurrentDomain.GetAssemblies(), x => x.GetName().Name == "NRaasMasterController"))
            {
                NRaasMasterControllerIntegration.Init();
            }
            LoadSaveManager.ObjectGroupsPreLoad += () => Phone.CallForServices.Singleton = CallForServices.Singleton;
            World.sOnWorldLoadFinishedEventHandler += (sender, e) => DebugUtils.TryDisplayScriptError(() =>
                {
                    foreach (IServiceProfile profile in ServiceUtils.ServiceProfiles)
                    {
                        CustomService.Init(profile);
                    }
                    if (kInitializeIncludedServices)
                    {
                        string entryKey = typeof(CustomService).GetLocalizationKey().Replace("CustomService", "");
                        CustomService.Init(new ServiceUtils.ServiceProfile("Housekeeper", Localization.LocalizeString(entryKey + "Housekeeper:Title"), new List<CommodityKind>
                            {
                                CommodityKind.BeMaid
                            })
                            {
                                Actions = new List<ServiceUtils.ActiveTopicAction>
                                    {
                                        new ServiceUtils.ActiveTopicAction("Dismiss"),
                                        new ServiceUtils.ActiveTopicAction("Fire")
                                    },
                                CarInstanceName = "CarServiceMaid",
                                CancelledMessage = Localization.LocalizeString(entryKey + "Housekeeper:ServiceCancelled"),
                                CancelledWhileActiveMessage = Localization.LocalizeString(entryKey + "Housekeeper:ServiceCancelledWhileActive"),
                                GetUniformFromName = true,
                                HiddenTraits = new List<TraitNames>
                                    {
                                        TraitNames.MakesNoMesses,
                                        TraitNames.SpeedyCleaner
                                    },
                                Inventory = new List<IGameObject>
                                    {
                                        BookGeneralData.GetBookGeneralByTitle("HowToServeAndNotBeServed")
                                    },
                                IsLiveInService = true,
                                IsQuietAroundSleepingSims = true,
                                IsScaredOfBonehilda = true,
                                Outputs = new List<ServiceUtils.CommodityChange>
                                    {
                                        new ServiceUtils.CommodityChange("Sims3.Gameplay.Actors.Sim+ReadSomethingInInventory+Definition", "Sims3.Gameplay.Actors.Sim", 2f, true, 2f, OutputUpdateType.ContinuousFlow),
                                        new ServiceUtils.CommodityChange("Sims3.Gameplay.InteractionsShared.SitAndWait+Definition", "Sims3.Gameplay.Abstracts.GameObject", 1f, false, 1f, OutputUpdateType.ImmediateDelta),
                                        new ServiceUtils.CommodityChange("Sims3.Gameplay.Objects.Bookshelf_ReadSomething+Definition", "Sims3.Gameplay.Objects.Bookshelf", 2f, true, 2f, OutputUpdateType.ContinuousFlow),
                                        new ServiceUtils.CommodityChange("Sims3.Gameplay.Objects.Environment.FirePit+LightFirePit+Definition", "Sims3.Gameplay.Objects.Environment.FirePit", 200f, true, 200f, OutputUpdateType.ContinuousFlow),
                                        new ServiceUtils.CommodityChange("Sims3.Gameplay.Objects.Fireplaces.Fireplace+LightFire+Definition", "Sims3.Gameplay.Objects.Fireplaces.Fireplace", 200f, true, 200f, OutputUpdateType.ContinuousFlow),
                                        new ServiceUtils.CommodityChange("Sims3.Gameplay.Objects.ReadBook+Definition", "Sims3.Gameplay.Objects.Book", 1f, true, 1f, OutputUpdateType.ContinuousFlow),
                                        new ServiceUtils.CommodityChange("Sims3.Gameplay.Objects.ReadBookChooser+Definition", "Sims3.Gameplay.Objects.Book", 1f, true, 1f, OutputUpdateType.ContinuousFlow),
                                        new ServiceUtils.CommodityChange("Sims3.Store.Objects.Tablet+ChooseBookOnTablet+Definition", "Sims3.Store.Objects.Tablet", 1f, true, 1f, OutputUpdateType.ContinuousFlow),
                                        new ServiceUtils.CommodityChange("Sims3.Store.Objects.Tablet+ReadBookOnTablet+Definition", "Sims3.Gameplay.Objects.Book", 1f, true, 1f, OutputUpdateType.ContinuousFlow)
                                    },
                                PotentialTraitCount = 2,
                                PotentialTraits = new List<TraitNames>
                                    {
                                        TraitNames.Charismatic,
                                        TraitNames.Flirty,
                                        TraitNames.Kleptomaniac,
                                    },
                                RequestedMessage = Localization.LocalizeString(entryKey + "Housekeeper:ServiceRequested"),
                                ServiceTuning = new Service.ServiceTuning(1, 800, false, true, true),
                                Traits = new List<TraitNames>
                                    {
                                        TraitNames.Neat
                                    },
                                WaitsBeforePuttingAwayLeftovers = true
                            });
                        CustomService.Init(new ServiceUtils.ServiceProfile("Chef", Localization.LocalizeString(entryKey + "Chef:Title"))
                            {
                                Actions = new List<ServiceUtils.ActiveTopicAction>
                                    {
                                        new ServiceUtils.ActiveTopicAction("Dismiss"),
                                        new ServiceUtils.ActiveTopicAction("Fire")
                                    },
                                CancelledMessage = Localization.LocalizeString(entryKey + "Chef:ServiceCancelled"),
                                CancelledWhileActiveMessage = Localization.LocalizeString(entryKey + "Chef:ServiceCancelledWhileActive"),
                                GetUniformFromName = true,
                                GetUniformNameCallback = (simDescription) => "career_execchef_" + (simDescription.IsFemale ? "female" : "male") + (simDescription.Elder ? "elder" : ""),
                                IsLiveInService = true,
                                PotentialTraitCount = 2,
                                PotentialTraits = new List<TraitNames>
                                    {
                                        TraitNames.HotHeaded,
                                        TraitNames.Neurotic,
                                        TraitNames.Perfectionist
                                    },
                                RequestedMessage = Localization.LocalizeString(entryKey + "Chef:ServiceRequested"),
                                ServiceTuning = new Service.ServiceTuning(1, 1000, false, true, true),
                                Skills = new List<SkillLevelPair>
                                    {
                                        new SkillLevelPair(SkillNames.Cooking)
                                    },
                                Traits = new List<TraitNames>
                                    {
                                        TraitNames.Artistic,
                                        TraitNames.NaturalCook
                                    },
                                WaitsBeforePuttingAwayLeftovers = true
                            });
                    }
                });
            World.sOnWorldQuitEventHandler += (sender, e) =>
                {
                    foreach (CustomService service in new List<CustomService>(ServiceUtils.CustomInstances.Values))
                    {
                        CustomService.Deinit(service.Profile, true);
                    }
                };
        }

        public static bool IsInServicePreventingSocialization(Sim target)
        {
            return target.IsPerformingAService && !VisitSituation.IsSocializing(target) && target.Service as Butler == null && target.Service as IAmSociableService == null;
        }

        [ScoringFunction]
        public static float ServantRolesMod_PutAwayLeftOversScoringFunction(Sim actor, InteractionObjectPair iop)
        {
            Type serviceDataType = actor.Service.GetType();
            bool serviceIsFromThisMod = actor.Service.IsFromServantRolesMod();
            PropertyInfo timeWaitBeforePutawayLeftoversProperty = serviceDataType.GetProperty("TimeWaitBeforePutawayLeftovers");
            if (!serviceIsFromThisMod && actor.Service.ServiceType == ServiceType.Butler || serviceIsFromThisMod && (bool)serviceDataType.GetProperty("WaitsBeforePuttingAwayLeftovers").GetValue(actor.Service, null) && timeWaitBeforePutawayLeftoversProperty != null && timeWaitBeforePutawayLeftoversProperty.PropertyType == typeof(float))
            {
                IPreparedFood preparedFood = iop.Target as IPreparedFood;
                return preparedFood != null && SimClock.ElapsedTime(TimeUnit.Minutes) - preparedFood.TimeOfCreation <= (float)timeWaitBeforePutawayLeftoversProperty.GetValue(null, null) ? 0 : 1;
            }
            return CleanableComponent.CleaningScoringFunction(actor, iop);
        }
    }
}
