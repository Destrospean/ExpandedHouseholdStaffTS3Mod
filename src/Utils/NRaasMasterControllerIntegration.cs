using NRaas;
using NRaas.CommonSpace.Options;
using NRaas.MasterControllerSpace.Sims;
using Sims3.Gameplay;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Destrospean.ExpandedHouseholdStaff;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;

namespace Destrospean.Utils
{
    public class NRaasMasterControllerIntegration
    {
        public static void Init()
        {
            OutfitExtensions.EditSpecialOutfitFunc editSpecialOutfit = OutfitExtensions.EditSpecialOutfit;
            OutfitExtensions.EditSpecialOutfit = (sim, specialOutfitKey) =>
                {
                    if (Settings.kIntegrateNRaasMasterController)
                    {
                        SimDescription simDescription = sim.SimDescription;
                        if (!simDescription.HasSpecialOutfit(specialOutfitKey))
                        {
                            simDescription.AddSpecialOutfit(simDescription.GetOutfit(OutfitCategories.Everyday, 0), specialOutfitKey);
                        }
                        OutfitCategories previousOutfitCategory = sim.CurrentOutfitCategory;
                        int previousOutfitIndex = sim.CurrentOutfitIndex;
                        simDescription.AddOutfit(simDescription.GetSpecialOutfit(specialOutfitKey), OutfitCategories.Everyday, 0);
                        simDescription.RemoveSpecialOutfit(specialOutfitKey);
                        sim.SwitchToOutfitWithoutSpin(OutfitCategories.Everyday, 0);
                        CASLogic casLogic = CASLogic.GetSingleton();
                        new Stylist().Perform(new GameHitParameters<GameObject>(sim, sim, GameObjectHit.NoHit));
                        casLogic.ShowUI += OutfitExtensions.OnShowUI;
                        while (GameStates.NextInWorldStateId != 0)
                        {
                            SpeedTrap.Sleep();
                        }
                        casLogic.ShowUI -= OutfitExtensions.OnShowUI;
                        simDescription.AddSpecialOutfit(simDescription.GetOutfit(OutfitCategories.Everyday, 0), specialOutfitKey);
                        simDescription.RemoveOutfit(OutfitCategories.Everyday, 0, true);
                        sim.SwitchToOutfitWithoutSpin(previousOutfitCategory, previousOutfitIndex);
                        return !CASChangeReporter.Instance.CasCancelled;
                    }
                    return editSpecialOutfit(sim, specialOutfitKey);
                };
        }
    }
}
