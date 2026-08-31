using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Controllers;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Objects.Beds;
using Sims3.Gameplay.Objects.Electronics;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Services;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using System;
using System.Collections.Generic;
using Sims3.UI;

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

                public override string GetInteractionName(Sim actor, Phone target, InteractionObjectPair interaction)
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
                Instance.MakeServiceRequest(Actor.LotCurrent, true, Actor.ObjectId);
                /*
                StyledNotification.Format format = new StyledNotification.Format(Localization.LocalizeString(DerivedType.GetLocalizationKey() + ":RequestService"), StyledNotification.NotificationStyle.kSimTalking);
                if (Responder.Instance.ServicesModel.DoesSimHaveFuturePhone(Actor.ObjectId))
                {
                    StyledNotification.Show(format, "w_future_phone", null, ProductVersion.EP11, ProductVersion.EP11);
                }
                else if (GameUtils.IsInstalled(ProductVersion.EP9))
                {
                    StyledNotification.Show(format, "w_smart_phone", null, ProductVersion.EP9, ProductVersion.EP9);
                }
                else
                {
                    StyledNotification.Show(format, "glb_tns_phone_r2");
                }
                */
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

        public static T sInstance;

        public static Dictionary<Type, CommodityKind> sServiceMotives = new Dictionary<Type, CommodityKind>();

        public static Type DerivedType
        {
            get
            {
                return typeof(T);
            }
        }

        public static CommodityKind ServiceMotive
        {
            get
            {
                CommodityKind serviceMotive;
                if (!sServiceMotives.TryGetValue(DerivedType, out serviceMotive))
                {
                    serviceMotive = CommonUtils.GetCommodityKind("Be" + DerivedType.Name, CommonUtils.CommodityKindType.Motive);
                }
                return serviceMotive;
            }
        }

        public virtual bool IsHomelessService
        {
            get
            {
                return true;
            }
        }

        public static Service<T> Instance
        {
            get
            {
                return sInstance;
            }
        }

        public override SimDescription FindSimForAssignment(Lot lot)
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
                    //CommonUtils.ShowDebugMessageDialog("A SimDescription: " + simDescription?.ToString() ?? "NULL");
                }
            }
            if (pool.Count == 0)
            {
                SimDescription simDescription = CreateOrUpdateServiceNpc(null, lot);
                if (simDescription != null)
                {
                    AddSimToPool(simDescription);
                }
                //CommonUtils.ShowDebugMessageDialog("Created SimDescription: " + simDescription?.ToString() ?? "NULL");
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
                        //CommonUtils.ShowDebugMessageDialog("Existing SimDescription: " + simDescription?.ToString() ?? "NULL");
                        return simDescription;
                    }
                }
                else
                {
                    mPreferredServiceNpc[lot.Household.HouseholdId] = randomSimDescription;
                }
            }
            //CommonUtils.ShowDebugMessageDialog("Random SimDescription: " + randomSimDescription?.ToString() ?? "NULL");
            return randomSimDescription;
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
            WorldName currentWorld = GameUtils.GetCurrentWorld();
            bool randomlyCreated;
            SimDescription simDescription = CreateSimDescriptionInternal(service, null, ageIfRandom, genderIfRandom, currentWorld, out randomlyCreated);
            string firstName, lastName;
            service.GetNameForNewNpc(simDescription.IsMale, currentWorld, out firstName, out lastName);
            simDescription.FirstName = firstName;
            simDescription.LastName = lastName;
            return simDescription;
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
    }
}
