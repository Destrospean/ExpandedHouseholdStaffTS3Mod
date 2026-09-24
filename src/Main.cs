using Sims3.Gameplay;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Interfaces.Destrospean.ExpandedHouseholdStaff;
using Sims3.Gameplay.ObjectComponents;
using Sims3.Gameplay.Objects;
using Sims3.Gameplay.Objects.Electronics;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Situations;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.Destrospean.ExpandedHouseholdStaff;
using Sims3.Gameplay.Destrospean.ExpandedHouseholdStaff.Interactions;
using Sims3.Gameplay.Destrospean.ExpandedHouseholdStaff.Services;
using Sims3.Gameplay.Destrospean.ExpandedHouseholdStaff.Situations;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.UI.Controller;
using System;
using System.Collections.Generic;
using System.Reflection;
using Destrospean.ExpandedHouseholdStaff.Interactions;
using Destrospean.Utils;
using Destrospean.Utils.ExpandedHouseholdStaff;

namespace Destrospean.ExpandedHouseholdStaff
{
    public class Main
    {
        [Tunable]
        protected static bool kInstantiator;

        static Main()
        {
            CommonUtils.ReplaceMethod<Inventory, Main>("AddInternal");
            CommonUtils.ReplaceMethod<SocialComponent, Main>("IsInServicePreventingSocialization");
            DebugUtils.ShowDebugMessages = Tuning.kShowDebugMessages;
            InteractionObjectTypeUtils.InitTypes();
            if (Tuning.kIntegrateNRaasMasterController && Array.Exists(AppDomain.CurrentDomain.GetAssemblies(), x => x.GetName().Name == "NRaasMasterController"))
            {
                NRaasMasterControllerIntegration.Init();
            }
            List<EventListener> listeners = new List<EventListener>();
            LoadSaveManager.ObjectGroupsPreLoad += () => Phone.CallForServices.Singleton = CallForServices.Singleton;
            World.OnObjectPlacedInLotEventHandler += (sender, e) => DebugUtils.TryDisplayScriptError(() =>
                {
                    World.OnObjectPlacedInLotEventArgs onObjectPlacedInLotEventArgs = e as World.OnObjectPlacedInLotEventArgs;
                    if (onObjectPlacedInLotEventArgs != null)
                    {
                        GameObject gameObject = GameObject.GetObject(onObjectPlacedInLotEventArgs.ObjectId);
                        gameObject.AddInteraction(ListInteractions.Singleton, true);
                        (gameObject as Mailbox).AddServiceProfileInteractions();
                    }
                });
            World.sOnWorldLoadFinishedEventHandler += (sender, e) => DebugUtils.TryDisplayScriptError(() =>
                {
                    foreach (GameObject gameObject in Sims3.Gameplay.Queries.GetObjects<GameObject>())
                    {
                        gameObject.AddInteraction(ListInteractions.Singleton, true);
                        (gameObject as Mailbox).AddServiceProfileInteractions();
                    }
                    foreach (IServiceProfile profile in new List<IServiceProfile>(ServiceUtils.ServiceProfiles))
                    {
                        if (profile.IsImmutable && (!Array.Exists(ServiceUtils.ReservedProfileNames, x => x == profile.Name)))
                        {
                            profile.RemoveServiceFromSaveGame();
                            continue;
                        }
                        CustomService.Init(profile);
                        profile.FixUp();
                    }
                    /*
                    string entryKey = typeof(CustomService).GetLocalizationKey().Replace("CustomService", "");
                    if (ServiceUtils.CanAddServiceToSaveGame("Chef"))
                    {
                        ServiceUtils.AddServiceToSaveGame(new ServiceUtils.ServiceProfile("Chef", Localization.LocalizeString(entryKey + "Chef:Title"))
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
                                IsImmutable = true,
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
                    if (ServiceUtils.CanAddServiceToSaveGame("HouseMaid"))
                    {
                        ServiceUtils.AddServiceToSaveGame(new ServiceUtils.ServiceProfile("HouseMaid", Localization.LocalizeString(entryKey + "HouseMaid:Title"), new List<CommodityKind>
                            {
                                CommodityKind.BeMaid
                            })
                            {
                                Actions = new List<ServiceUtils.ActiveTopicAction>
                                    {
                                        new ServiceUtils.ActiveTopicAction("Dismiss"),
                                        new ServiceUtils.ActiveTopicAction("Fire")
                                    },
                                CancelledMessage = Localization.LocalizeString(entryKey + "HouseMaid:ServiceCancelled"),
                                CancelledWhileActiveMessage = Localization.LocalizeString(entryKey + "HouseMaid:ServiceCancelledWhileActive"),
                                CarInstanceName = "CarServiceMaid",
                                GetUniformFromName = true,
                                HiddenTraits = new List<TraitNames>
                                    {
                                        TraitNames.MakesNoMesses,
                                        TraitNames.SpeedyCleaner
                                    },
                                IsImmutable = true,
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
                                        TraitNames.Kleptomaniac
                                    },
                                RequestedMessage = Localization.LocalizeString(entryKey + "HouseMaid:ServiceRequested"),
                                ServiceTuning = new Service.ServiceTuning(1, 800, false, true, true),
                                Traits = new List<TraitNames>
                                    {
                                        TraitNames.Neat
                                    },
                                WaitsBeforePuttingAwayLeftovers = true
                            });
                    }
                    */
                });
            World.sOnWorldQuitEventHandler += (sender, e) =>
                {
                    foreach (EventListener listener in listeners)
                    {
                        EventTracker.RemoveListener(listener);
                    }
                    listeners.Clear();
                    foreach (CustomService service in new List<CustomService>(ServiceUtils.CustomInstances.Values))
                    {
                        CustomService.Deinit(service.Profile, true);
                    }
                };
        }

        public InventoryItem AddInternal(IGameObject gameObject, uint stackNumber, InventoryStack stack, bool testPurge)
        {
            Inventory self = (Inventory)(object)this;
            if (testPurge && gameObject as INonPurgeableFromNPCInventory == null)
            {
                Sim sim = self.Owner as Sim;
                if (sim != null && sim.IsNPC && !sim.Service.IsFromExpandedHouseholdStaff())
                {
                    gameObject.Destroy();
                    if (stack.List == null)
                    {
                        self.mItems.Remove(stackNumber);
                    }
                    return null;
                }
            }
            GameObject obj = gameObject as GameObject;
            if (stack.List != null)
            {
                foreach (InventoryItem item in stack.List)
                {
                    if (item.Object == obj)
                    {
                        return item;
                    }
                }
            }
            InventoryItem inventoryItem = new InventoryItem(obj, stackNumber);
            InventoryEvent inventoryEvent = stack.AddItem(inventoryItem);
            Inventory.RemoveItemFromWorld(obj);
            World.ObjectSetOpacity(obj.ObjectId, 1f, 0f);
            obj.AddFlags(GameObject.FlagField.InInventory);
            if (obj.ItemComp != null)
            {
                obj.ItemComp.InventoryParent = self;
                self.UpdateBuffCounters(obj.ItemComp.InventoryBuffs, obj.ItemComp.Reaction, 1, obj);
            }
            obj.SetCommodityInteractionMap(null);
            if (obj.ItemComp != null)
            {
                obj.ItemComp.TriggerOnAddToInventoryEvent(self);
            }
            self.CallEventCallbacks(stackNumber, inventoryEvent, obj);
            if (obj.ItemComp == null || obj.ItemComp != null && obj.ItemComp.ShouldAddChildren(self.UnparentingStyle))
            {
                foreach (Slot slotName in obj.GetContainmentSlots())
                {
                    IGameObject containedObject = obj.GetContainedObject(slotName);
                    if (containedObject != null && containedObject as IUnparentableWhenInInventory == null && containedObject.HandToolAllowUserPickupBase())
                    {
                        containedObject.UnParent();
                        self.AddInternal(containedObject, self.CanStack(obj, containedObject) ? stackNumber : 0u, true);
                    }
                }
            }
            self.SetLotOwner(obj);
            return inventoryItem;
        }

        [ScoringFunction]
        public static float ExpandedHouseholdStaff_PutAwayLeftOversScoringFunction(Sim actor, InteractionObjectPair iop)
        {
            Type serviceDataType = actor.Service.GetType();
            bool serviceIsFromThisMod = actor.Service.IsFromExpandedHouseholdStaff();
            PropertyInfo timeWaitBeforePutawayLeftoversProperty = serviceDataType.GetProperty("TimeWaitBeforePutawayLeftovers");
            if (!serviceIsFromThisMod && actor.Service.ServiceType == ServiceType.Butler || serviceIsFromThisMod && (bool)serviceDataType.GetProperty("WaitsBeforePuttingAwayLeftovers").GetValue(actor.Service, null) && timeWaitBeforePutawayLeftoversProperty != null && timeWaitBeforePutawayLeftoversProperty.PropertyType == typeof(float))
            {
                IPreparedFood preparedFood = iop.Target as IPreparedFood;
                return preparedFood != null && SimClock.ElapsedTime(TimeUnit.Minutes) - preparedFood.TimeOfCreation <= (float)timeWaitBeforePutawayLeftoversProperty.GetValue(null, null) ? 0 : 1;
            }
            return CleanableComponent.CleaningScoringFunction(actor, iop);
        }

        public static bool IsInServicePreventingSocialization(Sim target)
        {
            return target.IsPerformingAService && !VisitSituation.IsSocializing(target) && target.Service as Butler == null && target.Service as IAmSociableService == null;
        }
    }
}
