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

        public new SimDescription CreateOrUpdateServiceNpc(SimDescription preCreatedSim, Lot lot)
        {
            CommonUtils.ShowDebugMessageDialog("CreateOrUpdateServiceNpc START");
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
            CommonUtils.ShowDebugMessageDialog("CreateOrUpdateServiceNpc END");
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
            CommonUtils.ShowDebugMessageDialog("CreateSimDescriptionInternal START");
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
            CommonUtils.ShowDebugMessageDialog("CreateSimDescriptionInternal END - randomlyCreated: " + randomlyCreated);
            return simDescription;
        }
    }
}
