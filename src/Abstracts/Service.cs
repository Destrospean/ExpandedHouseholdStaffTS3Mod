using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Controllers;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces.zoeoeAndDestrospean.ServantRolesMod;
using Sims3.Gameplay.Objects.Beds;
using Sims3.Gameplay.Objects.Electronics;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Services;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.UI;
using System;
using System.Collections.Generic;

namespace Sims3.Gameplay.Abstracts.zoeoeAndDestrospean.ServantRolesMod
{
    public abstract class Service<T> : Service where T : Service<T>
    {
        public class CallForService : Phone.Call
        {
            [DoesntRequireTuning]
            public class Definition : CallDefinition<CallForService>
            {
                public Definition()
                {
                }

                public override string GetInteractionName(Sim actor, Phone target, InteractionObjectPair iop)
                {
                    return Localization.LocalizeString(DerivedType.GetLocalizationKey() + ":RequestService");
                }

                public override string[] GetPath(bool isFemale)
                {
                    string localizationKey = DerivedType.GetLocalizationKey();
                    return new string[]
                    {
                        Localization.LocalizeString(localizationKey.Remove(localizationKey.IndexOf(DerivedType.Name) - (localizationKey.IndexOf(DerivedType.Name) < 1 ? 0 : 1)) + ":Path") + Localization.Ellipsis
                    };
                }

                public int GetTotalFunds(Sim actor)
                {
                    Lot lotHome = actor.LotHome;
                    if (lotHome == null)
                    {
                        return 0;
                    }
                    if (lotHome.EffectiveHousehold != null)
                    {
                        return lotHome.EffectiveHousehold.FamilyFunds;
                    }
                    return 0;
                }

                public override bool Test(Sim actor, Phone target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    if (!target.IsUsableBy(actor))
                    {
                        return false;
                    }
                    if (isAutonomous)
                    {
                        return false;
                    }
                    if (!actor.HouseholdOwnsResidentialLot(actor.LotCurrent))
                    {
                        greyedOutTooltipCallback = InteractionInstance.CreateTooltipCallback(Localization.LocalizeString("Gameplay/Objects/Electronics/Phone/CallForServices:ServicesOnlyOnHomeLot"));
                        return false;
                    }
                    if (Instance == null)
                    {
                        return false;
                    }
                    if (Instance.IsServiceRequested(actor.LotCurrent) || Instance.IsAnySimAssignedToLot(actor.LotCurrent))
                    {
                        greyedOutTooltipCallback = InteractionInstance.CreateTooltipCallback(Localization.LocalizeString("Gameplay/UI/ServicesUIWindow:AlreadyActive"));
                        return false;
                    }
                    if (Instance.Tuning.kCost > GetTotalFunds(actor))
                    {
                        greyedOutTooltipCallback = InteractionInstance.CreateTooltipCallback(Responder.Instance.LocalizationModel.LocalizeString("Gameplay/UI/ShoppingUIWindow:InsufficientFundsDialogTitle"));
                        return false;
                    }
                    return base.Test(actor, target, isAutonomous, ref greyedOutTooltipCallback);
                }
            }

            public static InteractionDefinition Singleton = new Definition();

            public override DialBehavior GetDialBehavior()
            {
                return base.InteractionDefinition is Definition ? DialBehavior.Pickup : DialBehavior.DoNotPick;
            }

            public override ConversationBehavior OnCallConnected()
            {
                if (base.InteractionDefinition as Definition == null)
                {
                    return ConversationBehavior.JustHangUp;
                }
                Actor.RequestService(Instance);
                return ConversationBehavior.TalkBriefly;
            }
        }

        public class SetUnsetServiceBed : ImmediateInteraction<Sim, Bed>
        {
            public class Definition : InteractionDefinition<Sim, Bed, SetUnsetServiceBed>
            {
                public override string GetInteractionName(Sim actor, Bed target, InteractionObjectPair iop)
                {
                    if (Instance == null)
                    {
                        return StringTable.GetLocalizedString(DerivedType.GetLocalizationKey() + ":Title");
                    }
                    List<Sim> simsAssignedToLot = Instance.GetSimsAssignedToLot(actor.LotHome);
                    return Localization.LocalizeString(actor.IsFemale, DerivedType.GetLocalizationKey() + "/" + typeof(SetUnsetServiceBed).Name + (target.FindOwnedBed(simsAssignedToLot[0]) == target ? ":Unset" : ":Set") + "InteractionName", simsAssignedToLot[0].SimDescription);
                }

                public override bool Test(Sim actor, Bed target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    Lot lotHome = actor.LotHome;
                    if (lotHome != null)
                    {
                        if (target.LotCurrent != lotHome)
                        {
                            return false;
                        }
                        if (Instance != null)
                        {
                            List<Sim> simsAssignedToLot = Instance.GetSimsAssignedToLot(lotHome);
                            if (simsAssignedToLot.Count > 0)
                            {
                                Sim owner = simsAssignedToLot[0];
                                Bed bed = target.FindOwnedBed(owner);
                                if (bed == null || bed == target)
                                {
                                    return target.CanBeUsedAsBed;
                                }
                            }
                        }
                    }
                    return false;
                }
            }

            public static InteractionDefinition Singleton = new Definition();

            public override bool Run()
            {
                if (Instance != null)
                {
                    Sim simActiveOnLot = Instance.GetSimActiveOnLot(Actor.LotHome);
                    if (simActiveOnLot != null)
                    {
                        Bed bed = Target.FindOwnedBed(simActiveOnLot);
                        if (bed == Target)
                        {
                            Target.RelinquishOwnership(simActiveOnLot);
                            return true;
                        }
                        if (Target.PartComponent != null && Target.PartComponent.PartDataList != null)
                        {
                            foreach (BedData value in Target.PartComponent.PartDataList.Values)
                            {
                                if (value != null)
                                {
                                    Target.ClaimOwnership(simActiveOnLot, value);
                                    break;
                                }
                            }
                        }
                    }
                }
                return true;
            }
        }

        public static Type DerivedType
        {
            get
            {
                return typeof(T);
            }
        }

        public static Service<T> Instance
        {
            get
            {
                Service service;
                return ServiceUtils.Instances.TryGetValue(DerivedType, out service) ? (Service<T>)service : null;
            }
            set
            {
                ServiceUtils.Instances[DerivedType] = value;
            }
        }

        public virtual bool IsHomelessService
        {
            get
            {
                return true;
            }
        }

        public virtual bool IsQuietAroundSleepingSims
        {
            get
            {
                return false;
            }
        }

        public virtual bool WaitsBeforePuttingAwayLeftovers
        {
            get
            {
                return false;
            }
        }

        public static CommodityKind ServiceMotive
        {
            get
            {
                CommodityKind serviceMotive;
                if (!ServiceUtils.ServiceMotives.TryGetValue(DerivedType, out serviceMotive))
                {
                    serviceMotive = CommonUtils.GetCommodityKind("Be" + DerivedType.Name, CommonUtils.CommodityKindType.Motive);
                }
                return serviceMotive;
            }
        }

        static void AddInteractions(Bed bed)
        {
            CommonUtils.TryDisplayScriptError(() => bed.AddInteraction(SetUnsetServiceBed.Singleton, true));
        }

        static void AddInteractions(Phone phone)
        {
            CommonUtils.TryDisplayScriptError(() => phone.AddInteraction(CallForService.Singleton, true));
        }

        static void AddInteractions(PhoneCell phoneCell)
        {
            CommonUtils.TryDisplayScriptError(() =>
                {
                    foreach (InteractionObjectPair interaction in phoneCell.Interactions)
                    {
                        if (interaction.InteractionDefinition.GetType() == CallForService.Singleton.GetType())
                        {
                            return;
                        }
                    }
                    phoneCell.AddInteraction(CallForService.Singleton);
                    phoneCell.AddInventoryInteraction(CallForService.Singleton);
                });
        }

        static void InitInjection()
        {
            CommonUtils.TryDisplayScriptError(() =>
                {
                    foreach (PhoneCell phoneCell in Sims3.Gameplay.Queries.GetObjects<PhoneCell>())
                    {
                        AddInteractions(phoneCell);
                    }
                    EventTracker.AddListener(EventTypeId.kInventoryObjectAdded, OnObjectChanged);
                    EventTracker.AddListener(EventTypeId.kObjectStateChanged, OnObjectChanged);
                });
        }

        static ListenerAction OnObjectChanged(Event e)
        {
            ListenerAction retVal;
            CommonUtils.TryDisplayScriptError(() =>
                {
                    PhoneCell phoneCell = e.TargetObject as PhoneCell;
                    if (phoneCell != null)
                    {
                        AddInteractions(phoneCell);
                    }
                    return ListenerAction.Keep;
                }, out retVal);
            return retVal;
        }

        static void OnObjectPlacedInLot(object sender, EventArgs e)
        {
            CommonUtils.TryDisplayScriptError(() =>
                {
                    World.OnObjectPlacedInLotEventArgs onObjectPlacedInLotEventArgs = e as World.OnObjectPlacedInLotEventArgs;
                    if (onObjectPlacedInLotEventArgs != null)
                    {
                        GameObject gameObject = GameObject.GetObject(onObjectPlacedInLotEventArgs.mObjectId);
                        if (typeof(IAmLiveInService).IsAssignableFrom(DerivedType))
                        {
                            Bed bed = gameObject as Bed;
                            if (bed != null)
                            {
                                AddInteractions(bed);
                                return;
                            }
                        }
                        Phone phone = gameObject as Phone;
                        if (phone != null)
                        {
                            AddInteractions(phone);
                        }
                    }
                });
        }

        static void OnPreLoad()
        {
            CommonUtils.TryDisplayScriptError(() =>
                {
                    if (!ServiceUtils.PreloadedServices.Contains(DerivedType))
                    {
                        XmlDbData xmlDbData = XmlDbData.ReadData("ServantRolesMod_" + DerivedType.Name + "_ActiveTopic");
                        if (xmlDbData != null)
                        {
                            SocialManager.ParseActiveTopic(xmlDbData);
                        }
                        ServiceUtils.PreloadedServices.Add(DerivedType);
                    }
                });
        }

        static ListenerAction OnSimSelected(Event e)
        {
            ListenerAction retVal;
            CommonUtils.TryDisplayScriptError(() =>
                {
                    if (Household.ActiveHousehold != null)
                    {
                        InitInjection();
                        return ListenerAction.Remove;
                    }
                    return ListenerAction.Keep;
                }, out retVal);
            return retVal;
        }

        static void OnStartupApp(object sender, EventArgs args)
        {
            CommonUtils.TryDisplayScriptError(() => CommonUtils.LoadMotive("Be" + DerivedType.Name + "Motive"));
        }

        static void OnWorldLoadFinished(object sender, EventArgs e)
        {
            CommonUtils.TryDisplayScriptError(() =>
                {
                    DerivedType.GetMethod("Create").Invoke(null, null);
                    if (Instance == null)
                    {
                        return;
                    }
                    IEnumerator<SimDescription> enumerator = Instance.Pool.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current != null && enumerator.Current.CreatedSim != null)
                        {
                            CommonUtils.UpdateMotiveTunings(enumerator.Current.CreatedSim, ServiceMotive);
                        }
                    }
                    if (Household.ActiveHousehold != null)
                    {
                        InitInjection();
                    }
                    else
                    {
                        EventTracker.AddListener(EventTypeId.kEventSimSelected, OnSimSelected);
                    }
                    foreach (Bed bed in Sims3.Gameplay.Queries.GetObjects<Bed>())
                    {
                        AddInteractions(bed);
                    }
                    foreach (Phone phone in Sims3.Gameplay.Queries.GetObjects<Phone>())
                    {
                        AddInteractions(phone);
                    }
                });
        }

        static void OnWorldQuit(object sender, EventArgs e)
        {
            if (Instance != null)
            {
                Instance = null;
            }
        }

        public override bool CanRequestServiceFromPhone(Lot lot)
        {
            return false;
        }

        public new SimDescription CreateOrUpdateServiceNpc(SimDescription preCreatedSim, Lot lot)
        {
            SimDescription simDescription = preCreatedSim;
            CommonUtils.TryDisplayScriptError(() =>
                {
                    bool shouldUsePlumbot = false;
                    if (simDescription == null)
                    {
                        if (GameUtils.IsInstalled(ProductVersion.EP11))
                        {
                            shouldUsePlumbot = ServiceNPCSpecifications.ShouldUseServobot(ServiceType.ToString());
                        }
                        if (shouldUsePlumbot)
                        {
                            simDescription = CreateRobotSim(this);
                        }
                        else
                        {
                            CASAgeGenderFlags age = ServiceNPCSpecifications.GetAge(ServiceType.ToString());
                            simDescription = CreateSimDescription(this, age, GetGenderForNewNpc(lot));
                        }
                        simDescription.FindSuitableVirtualHome();
                    }
                    else
                    {
                        simDescription.CreatedByService = this;
                    }
                    if (simDescription != null)
                    {
                        ClearAllTraits(simDescription);
                        SetServiceNPCProperties(simDescription);
                        if (!shouldUsePlumbot)
                        {
                            SetTraits(simDescription);
                            SetRandomTraits(simDescription);
                            OverlayUniform(simDescription, ServiceType.ToString());
                        }
                    }
                });
            return simDescription;
        }

        public static SimDescription CreateSimDescription(Service<T> service, CASAgeGenderFlags ageIfRandom, CASAgeGenderFlags genderIfRandom)
        {
            SimDescription retVal;
            return CommonUtils.TryDisplayScriptError(() =>
                {
                    WorldName currentWorld = GameUtils.GetCurrentWorld();
                    bool randomlyCreated;
                    SimDescription simDescription = CreateSimDescriptionInternal(service, null, ageIfRandom, genderIfRandom, currentWorld, out randomlyCreated);
                    string firstName, lastName;
                    service.GetNameForNewNpc(simDescription.IsMale, currentWorld, out firstName, out lastName);
                    simDescription.FirstName = firstName;
                    simDescription.LastName = lastName;
                    return simDescription;
                }, out retVal) ? null : retVal;
        }

        public static SimDescription CreateSimDescriptionInternal(Service<T> service, string outfitName, CASAgeGenderFlags ageIfRandom, CASAgeGenderFlags genderIfRandom, WorldName homeWorld, out bool randomlyCreated)
        {
            SimDescription simDescription = null;
            bool tempRandomlyCreated = false;
            CommonUtils.TryDisplayScriptError(() =>
                {
                    tempRandomlyCreated = false;
                    if ((service.IsHomelessService || LotManager.SelectRandomLotForNPCMoveIn(x => x.Household == null) == null) && Household.NpcHousehold == null)
                    {
                        simDescription = null;
                        return;
                    }
                    bool outfitIsInvalid = false;
                    if (outfitName != null)
                    {
                        SimOutfit simOutfit = new SimOutfit(ResourceKey.CreateOutfitKey(outfitName, 0));
                        if (simOutfit.IsValid)
                        {
                            SimBuilder simBuilder = new SimBuilder
                            {
                                UseCompression = true
                            };
                            OutfitUtils.SetOutfit(simBuilder, simOutfit, null);
                            SimOutfit outfit = new SimOutfit(simBuilder.CacheOutfit("Service_" + outfitName));
                            simDescription = new SimDescription(outfit);
                            simDescription.AddOutfit(outfit, OutfitCategories.Everyday, true);
                        }
                        else
                        {
                            outfitIsInvalid = true;
                        }
                    }
                    if (outfitName == null || outfitIsInvalid)
                    {
                        simDescription = genderIfRandom == CASAgeGenderFlags.None ? Genetics.MakeSim(ageIfRandom) : Genetics.MakeSim(ageIfRandom, genderIfRandom, homeWorld, uint.MaxValue);
                        tempRandomlyCreated = true;
                    }
                    Lot lot = service.IsHomelessService ? null : LotManager.SelectRandomLotForNPCMoveIn(x => x.Household == null);
                    if (lot == null)
                    {
                        InitialServiceNpcSetup(service, simDescription);
                        return;
                    }
                    Household household = new SimUtils.HouseholdCreationSpec().Instantiate();
                    household.Add(simDescription);
                    lot.MoveIn(household);
                    simDescription.CreatedByService = service;
                    return;
                });
            randomlyCreated = tempRandomlyCreated;
            return simDescription;
        }

        public override SimDescription FindSimForAssignment(Lot lot)
        {
            SimDescription retVal;
            return CommonUtils.TryDisplayScriptError(() =>
                {
                    bool shouldUseServobot = false;
                    if (GameUtils.IsInstalled(ProductVersion.EP11))
                    {
                        shouldUseServobot = ServiceNPCSpecifications.ShouldUseServobot(ServiceType.ToString());
                    }
                    List<SimDescription> pool = new List<SimDescription>();
                    foreach (SimDescription simDescription in mPool)
                    {
                        if (!IsSimAssignedTask(simDescription) && CanSimBeAssignedToLot(simDescription, lot) && (shouldUseServobot && simDescription.IsEP11Bot || !shouldUseServobot && !simDescription.IsEP11Bot) && simDescription.CreatedSim == null)
                        {
                            pool.Add(simDescription);
                        }
                    }
                    if (pool.Count == 0)
                    {
                        SimDescription simDescription = CreateOrUpdateServiceNpc(null, lot);
                        if (simDescription != null)
                        {
                            AddSimToPool(simDescription);
                        }
                        return simDescription;
                    }
                    SimDescription randomSimDescription = RandomUtil.GetRandomObjectFromList<SimDescription>(pool);
                    if (AlwaysTryToSendSameSim && lot.Household != null)
                    {
                        SimDescription simDescription = null;
                        if (mPreferredServiceNpc.TryGetValue(lot.Household.HouseholdId, out simDescription))
                        {
                            if (pool.Contains(simDescription))
                            {
                                return simDescription;
                            }
                        }
                        else
                        {
                            mPreferredServiceNpc[lot.Household.HouseholdId] = randomSimDescription;
                        }
                    }
                    return randomSimDescription;
                }, out retVal) ? null : retVal;
        }

        public static void Init()
        {
            CommonUtils.AddEnumValue<CommodityKind>("Be" + DerivedType.Name, ServiceMotive);
            LoadSaveManager.ObjectGroupsPreLoad += OnPreLoad;
            World.OnObjectPlacedInLotEventHandler += OnObjectPlacedInLot;
            World.sOnStartupAppEventHandler += OnStartupApp;
            World.sOnWorldLoadFinishedEventHandler += OnWorldLoadFinished;
            World.sOnWorldQuitEventHandler += OnWorldQuit;
        }
    }
}
