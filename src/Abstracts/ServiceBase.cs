using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Services;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using System;
using System.Collections.Generic;

namespace Sims3.Gameplay.Abstracts.zoeoeAndDestrospean.ServantRolesMod
{
    public abstract class ServiceBase : Service
    {
        public virtual bool IsHomelessService
        {
            get
            {
                return false;
            }
        }

        public new SimDescription CreateOrUpdateServiceNpc(SimDescription preCreatedSim, Lot lot)
        {
            SimDescription simDescription = preCreatedSim;
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
            return simDescription;
        }

        public static SimDescription CreateSimDescriptionInternal(ServiceBase service, string outfitName, CASAgeGenderFlags ageIfRandom, CASAgeGenderFlags genderIfRandom, WorldName homeWorld, out bool randomlyCreated)
        {
            randomlyCreated = false;
            if ((service.IsHomelessService || LotManager.SelectRandomLotForNPCMoveIn(x => x.Household == null) == null) && Household.NpcHousehold == null)
            {
                return null;
            }
            SimDescription simDescription = null;
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
                randomlyCreated = true;
            }
            Lot lot = service.IsHomelessService ? null : LotManager.SelectRandomLotForNPCMoveIn(x => x.Household == null);
            if (lot == null)
            {
                InitialServiceNpcSetup(service, simDescription);
                return simDescription;
            }
            Household household = new SimUtils.HouseholdCreationSpec().Instantiate();
            household.Add(simDescription);
            lot.MoveIn(household);
            simDescription.CreatedByService = service;
            return simDescription;
        }

        public static CommodityKind GetServiceCommodityKind(Type serviceType)
        {
            return (CommodityKind)ResourceUtils.HashString32("Be" + serviceType.Name);
        }

        public static CommodityKind GetServiceCommodityKind<T>() where T : ServiceBase
        {
            return GetServiceCommodityKind(typeof(T));
        }
    }
}
