using NRaas;
using NRaas.CommonSpace.Options;
using NRaas.MasterControllerSpace;
using NRaas.MasterControllerSpace.Sims;
using Sims3.Gameplay;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.CAS;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;

namespace Destrospean.Utils
{
    public class NRaasMasterControllerIntegration
    {
        public static void Init()
        {
            OutfitExtensions.EditSpecialOutfit = (sim, specialOutfitKey) =>
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
                };

            OutfitExtensions.EditSpecialOutfitSimDescription = (simDescription, specialOutfitKey, category) =>
                {
                    if (!simDescription.HasSpecialOutfit(specialOutfitKey))
                    {
                        simDescription.AddSpecialOutfit(simDescription.GetOutfit(category, 0), specialOutfitKey);
                    }
                    simDescription.AddOutfit(simDescription.GetSpecialOutfit(specialOutfitKey), category, 0);
                    simDescription.RemoveSpecialOutfit(specialOutfitKey);
                    new Stylist().Perform(new GameHitParameters<SimDescriptionObject>(Sim.ActiveActor, new SimDescriptionObject(simDescription), GameObjectHit.NoHit));
                    while (GameStates.NextInWorldStateId != 0)
                    {
                        SpeedTrap.Sleep();
                    }
                    simDescription.AddSpecialOutfit(simDescription.GetOutfit(category, 0), specialOutfitKey);
                    simDescription.RemoveOutfit(category, 0, true);
                    return !CASChangeReporter.Instance.CasCancelled;
                };
        }
    }
}
