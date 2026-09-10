using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Interfaces.zoeoeAndDestrospean.ServantRolesMod;
using Sims3.Gameplay.ObjectComponents;
using Sims3.Gameplay.Objects.Electronics;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Situations;
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
            InteractionObjectTypeUtils.InitTypes();
            LoadSaveManager.ObjectGroupsPreLoad += () => Phone.CallForServices.Singleton = CallForServices.Singleton;
            CommonUtils.ReplaceMethod<SocialComponent, Main>("IsInServicePreventingSocialization");
            DebugUtils.ShowDebugMessages = kShowDebugMessages;
            if (kIntegrateNRaasMasterController && Array.Exists(AppDomain.CurrentDomain.GetAssemblies(), x => x.GetName().Name == "NRaasMasterController"))
            {
                NRaasCompatibility.IntegrateNRaasMasterController();
            }
            World.sOnWorldLoadFinishedEventHandler += (sender, e) => DebugUtils.TryDisplayScriptError(() =>
                {
                    foreach (ServiceUtils.ServiceProfile profile in ServiceUtils.ServiceProfiles)
                    {
                        CustomService.Init(profile);
                    }
                    if (kInitializeIncludedServices)
                    {
                        string entryKey = typeof(CustomService).GetLocalizationKey().Replace("CustomService", "");
                        CustomService.Init(new ServiceUtils.ServiceProfile("Housekeeper", Localization.LocalizeString(entryKey + "Housekeeper:Title"), Localization.LocalizeString(entryKey + "Housekeeper:ServiceRequested"), Localization.LocalizeString(entryKey + "Housekeeper:ServiceCancelled"), Localization.LocalizeString(entryKey + "Housekeeper:ServiceCancelledWhileActive"), new List<CommodityKind>
                            {
                                CommodityKind.BeMaid
                            }, new List<ServiceUtils.CommodityChange>
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
                            })
                            {
                                Actions = new List<ServiceUtils.ActiveTopicAction>
                                    {
                                        new ServiceUtils.ActiveTopicAction("Dismiss"),
                                        new ServiceUtils.ActiveTopicAction("Fire")
                                    },
                                GetUniformFromName = true,
                                HiddenTraits = new List<TraitNames>
                                    {
                                        TraitNames.MakesNoMesses,
                                        TraitNames.SpeedyCleaner
                                    },
                                IsLiveInService = true,
                                IsQuietAroundSleepingSims = true,
                                IsScaredOfBonehilda = true,
                                PotentialTraitCount = 2,
                                PotentialTraits = new List<TraitNames>
                                    {
                                        TraitNames.Neurotic,
                                        TraitNames.Flirty,
                                        TraitNames.Kleptomaniac,
                                        TraitNames.Charismatic
                                    },
                                ServiceTuning = new Service.ServiceTuning(1, 800, false, true, true),
                                Traits = new List<TraitNames>
                                    {
                                        TraitNames.Neat
                                    },
                                WaitsBeforePuttingAwayLeftovers = true
                            });
                        CustomService.Init(new ServiceUtils.ServiceProfile("Chef", Localization.LocalizeString(entryKey + "Chef:Title"), Localization.LocalizeString(entryKey + "Chef:ServiceRequested"), Localization.LocalizeString(entryKey + "Chef:ServiceCancelled"), Localization.LocalizeString(entryKey + "Housekeeper:ServiceCancelledWhileActive"), null)
                            {
                                Actions = new List<ServiceUtils.ActiveTopicAction>
                                    {
                                        new ServiceUtils.ActiveTopicAction("Dismiss"),
                                        new ServiceUtils.ActiveTopicAction("Fire")
                                    },
                                GetUniformFromName = true,
                                GetUniformNameCallback = (simDescription) => "career_execchef_" + (simDescription.IsFemale ? "female" : "male") + (simDescription.Elder ? "elder" : ""),
                                IsLiveInService = true,
                                PotentialTraitCount = 2,
                                PotentialTraits = new List<TraitNames>
                                    {
                                        TraitNames.Neurotic,
                                        TraitNames.Perfectionist,
                                        TraitNames.HotHeaded,
                                    },
                                ServiceTuning = new Service.ServiceTuning(1, 1000, false, true, true),
                                Traits = new List<TraitNames>
                                    {
                                        TraitNames.Artistic,
                                        TraitNames.NaturalCook
                                    },
                                WaitsBeforePuttingAwayLeftovers = true
                            });
                    }
                });
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
