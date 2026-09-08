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
using System;
using System.Collections.Generic;
using System.Reflection;
using zoeoeAndDestrospean.Utils;
using zoeoeAndDestrospean.Utils.ServantRolesMod;
using Sims3.Gameplay.Objects;
using Sims3.Store.Objects;
using Sims3.Gameplay.Objects.Environment;
using Sims3.Gameplay.Objects.Fireplaces;
using Sims3.Gameplay.InteractionsShared;
using Sims3.Gameplay.Abstracts;

namespace zoeoeAndDestrospean.ServantRolesMod
{
    public class Main
    {
        [Tunable]
        protected static bool kInitTestServices = true;

        [Tunable]
        protected static bool kInstantiator;

        static Main()
        {
            LoadSaveManager.ObjectGroupsPreLoad += () => Phone.CallForServices.Singleton = CallForServices.Singleton;
            CommonUtils.ReplaceMethod<SocialComponent, Main>("IsInServicePreventingSocialization");
            if (kInitTestServices)
            {
                World.sOnWorldLoadFinishedEventHandler += (sender, e) =>
                    {
                        CustomService.ServiceProfile profile = new CustomService.ServiceProfile("TestHousekeeper", "Test Housekeeper", "test housekeeping", null, new List<CommodityKind>
                            {
                                CommodityKind.BeMaid
                            }, new List<CustomService.CommodityChange>
                            {
                                new CustomService.CommodityChange(typeof(Bookshelf_ReadSomething.Definition), typeof(Bookshelf), 2f, true, 2f, OutputUpdateType.ContinuousFlow),
                                new CustomService.CommodityChange(typeof(Tablet.ChooseBookOnTablet.Definition), typeof(Tablet), 1f, true, 1f, OutputUpdateType.ContinuousFlow),
                                new CustomService.CommodityChange(typeof(FirePit.LightFirePit.Definition), typeof(FirePit), 200f, true, 200f, OutputUpdateType.ContinuousFlow),
                                new CustomService.CommodityChange(typeof(Fireplace.LightFire.Definition), typeof(Fireplace), 200f, true, 200f, OutputUpdateType.ContinuousFlow),
                                new CustomService.CommodityChange(typeof(ReadBook.Definition), typeof(Book), 1f, true, 1f, OutputUpdateType.ContinuousFlow),
                                new CustomService.CommodityChange(typeof(ReadBookChooser.Definition), typeof(Book), 1f, true, 1f, OutputUpdateType.ContinuousFlow),
                                new CustomService.CommodityChange(typeof(Tablet.ReadBookOnTablet.Definition), typeof(Book), 1f, true, 1f, OutputUpdateType.ContinuousFlow),
                                new CustomService.CommodityChange(typeof(Sim.ReadSomethingInInventory.Definition), typeof(Sim), 2f, true, 2f, OutputUpdateType.ContinuousFlow),
                                new CustomService.CommodityChange(typeof(SitAndWait.Definition), typeof(GameObject), 1f, false, 1f, OutputUpdateType.ImmediateDelta)
                            }, new List<TraitNames>
                            {
                                TraitNames.Neat
                            }, new List<TraitNames>
                            {
                                TraitNames.Neurotic,
                                TraitNames.Flirty,
                                TraitNames.Kleptomaniac,
                                TraitNames.Charismatic
                            }, 2)
                            {
                                IsLiveInService = true,
                                ServiceTuning = new Service.ServiceTuning(1, 800, false, true, true)
                            };
                        CustomService.Init(profile);
                    };
            }
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
