using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using System;
using System.Collections.Generic;

namespace Sims3.Gameplay.Abstracts.zoeoeAndDestrospean.ServantRolesMod
{
    public abstract class Service<T> : Service where T : Service<T>
    {
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
                return false;
            }
        }

        public override SimDescription FindSimForAssignment(Lot lot)
        {
            CommonUtils.ShowDebugMessageDialog("Lot: " + lot?.ToString() ?? "NULL");
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
                    CommonUtils.ShowDebugMessageDialog("A SimDescription: " + simDescription?.ToString() ?? "NULL");
                }
            }
            if (pool.Count == 0)
            {
                SimDescription simDescription = CreateOrUpdateServiceNpc(null, lot);
                if (simDescription != null)
                {
                    AddSimToPool(simDescription);
                }
                CommonUtils.ShowDebugMessageDialog("Created SimDescription: " + simDescription?.ToString() ?? "NULL");
                return simDescription;
            }
            SimDescription randomObjectFromList = RandomUtil.GetRandomObjectFromList<SimDescription>(pool);
            if (AlwaysTryToSendSameSim && lot.Household != null)
            {
                SimDescription simDescription = null;
                if (mPreferredServiceNpc.TryGetValue(lot.Household.HouseholdId, out simDescription))
                {
                    if (pool.Contains(simDescription))
                    {
                        CommonUtils.ShowDebugMessageDialog("Existing SimDescription: " + simDescription?.ToString() ?? "NULL");
                        return simDescription;
                    }
                }
                else
                {
                    mPreferredServiceNpc[lot.Household.HouseholdId] = randomObjectFromList;
                }
            }
            return randomObjectFromList;
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
