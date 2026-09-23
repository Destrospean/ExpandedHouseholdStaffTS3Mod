using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Objects;
using Sims3.Gameplay.Objects.FoodObjects;
using Sims3.Gameplay.Objects.Insect;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.Destrospean.ExpandedHouseholdStaff.Situations;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.SimIFace.Enums;

namespace Sims3.Gameplay.Destrospean.ExpandedHouseholdStaff.Interactions
{
    public class HarvestHoney : BeekeepingBox.Harvest
    {
        public new class Definition : InteractionDefinition<Sim, BeekeepingBox, HarvestHoney>
        {
            public override string GetInteractionName(Sim actor, BeekeepingBox target, InteractionObjectPair interaction)
            {
                return BeekeepingBox.LocalizeString("HarvestHoney");
            }

            public string NoHoneyStored()
            {
                return BeekeepingBox.LocalizeString("NoHoneyStored");
            }

            public override bool Test(Sim actor, BeekeepingBox target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                if (CustomServiceSituation.FindServiceSituationInvolving(actor) as CustomServiceSituation == null)
                {
                    return false;
                }
                if (target.Charred || target.mCurrentAttackTarget != null)
                {
                    return false;
                }
                if (target.mStoredHoneyInfo != null && target.mStoredHoneyInfo.Count > 0)
                {
                    return true;
                }
                greyedOutTooltipCallback = NoHoneyStored;
                return false;
            }
        }

        public static new InteractionDefinition Singleton = new Definition();

        public static bool ChangeSimToBeekeeperOutfit(Sim actor, out bool swimsuitUsed, BeekeepingBox target)
        {
            if (actor.BuffManager.HasTransformBuff() || actor.GetCurrentOutfitCategoryFromOutfitInGameObject() == OutfitCategories.Singed)
            {
                swimsuitUsed = false;
                return false;
            }
            bool result = false;
            string specialOutfitKey = GetBeekeeperOutfitName(actor);
            actor.RefreshCurrentOutfit(false);
            if (actor.TraitManager.HasAnyElement(TraitNames.Daredevil, TraitNames.Insane))
            {
                target.mLastOutfit = actor.CurrentOutfit;
                actor.SwitchToOutfitWithSpin(Sim.ClothesChangeReason.GoingToSwim, OutfitCategories.Swimwear, false, true, true);
                swimsuitUsed = true;
                result = true;
            }
            else
            {
                SimDescription simDescription = actor.SimDescription;
                bool hasBeekeeperOutfit = simDescription.HasSpecialOutfit(specialOutfitKey);
                SimOutfit uniform = null;
                if (hasBeekeeperOutfit || OutfitUtils.TryGenerateSimOutfit(specialOutfitKey, ProductVersion.EP7, out uniform))
                {
                    SimOutfit resultOutfit = null;
                    if (hasBeekeeperOutfit || OutfitUtils.TryApplyUniformToOutfit(simDescription.GetOutfit(OutfitCategories.Everyday, 0), uniform, simDescription, "BeekeepingBox.ApplyBeekeepingOutfit", out resultOutfit))
                    {
                        if (resultOutfit == null)
                        {
                            resultOutfit = simDescription.GetSpecialOutfit(specialOutfitKey);
                        }
                        else
                        {
                            simDescription.AddSpecialOutfit(resultOutfit, specialOutfitKey);
                        }
                        target.mLastOutfit = actor.CurrentOutfit;
                        actor.SwitchToOutfitWithSpin(resultOutfit.Key);
                        result = true;
                    }
                }
                swimsuitUsed = false;
            }
            return result && actor.CurrentOutfitCategory == OutfitCategories.Special && actor.CurrentOutfitIndex == actor.SimDescription.GetSpecialOutfitIndexFromKey(ResourceUtils.HashString32(specialOutfitKey));
        }

        public static bool DoBaseInteractionFunctionality(StateMachineClient stateMachineClient, Sim actor, InteractionInstance instance, out bool outfitChanged, out bool swimwearUsed, BeekeepingBox target)
        {
            if (!actor.RouteToSlotAndCheckInUse(target, Slot.RoutingSlot_0))
            {
                actor.AddExitReason(ExitReason.RouteFailed);
                outfitChanged = false;
                swimwearUsed = false;
                return false;
            }
            outfitChanged = ChangeSimToBeekeeperOutfit(actor, out swimwearUsed, target);
            instance.StandardEntry();
            instance.BeginCommodityUpdates();
            instance.EnterStateMachine("beekeepingbox", "Enter", "x", "box");
            bool result = true;
            bool attackOccurred = false;
            bool stingOccurred = false;
            if (instance.InteractionDefinition != BeekeepingBox.SmokeOut.Singleton && !actor.SimDescription.IsMummy && !actor.SimDescription.IsRobot)
            {
                target.GetAttackAndStingOccurrences(actor, out attackOccurred, out stingOccurred);
            }
            instance.mCurrentStateMachine.SetParameter("stingOccurs", stingOccurred ? YesOrNo.yes : YesOrNo.no);
            instance.AnimateSim("Action");
            if (stingOccurred)
            {
                BuffInstance element = actor.BuffManager.GetElement(BuffNames.BeeSting);
                if (element == null || element.EffectValue >= BuffManager.BuffDictionary[7533959535590961648uL].EffectValue)
                {
                    actor.BuffManager.AddElement(BuffNames.BeeSting, Origin.FromBeingStung);
                }
            }
            else if (attackOccurred)
            {
                actor.RequestWalkStyle(Sim.WalkStyle.AutoSelect);
                target.mVfxBeeAttack = VisualEffect.Create("ep7BeesAggressiveSim_main");
                target.mVfxBeeAttack.ParentTo(actor, Sim.FXJoints.Spine0);
                target.mVfxBeeAttack.Start();
                instance.AnimateSim("Attack");
                instance.mCurrentStateMachine.RemoveActor(actor);
                actor.RequestWalkStyle(Sim.WalkStyle.OnFire);
                actor.RouteAway(0.5f, 5, true, new InteractionPriority(InteractionPriorityLevel.Fire), false, true, true, RouteDistancePreference.PreferFurthestFromRouteOrigin);
                target.mCurrentAttackTarget = actor;
                target.mAttackEndingAlarm = actor.AddAlarmRepeating(3, TimeUnit.Seconds, target.CheckForBeeAttackEnding, "BeekeepingBox.CheckForBeeAttackEnding", AlarmType.AlwaysPersisted);
                actor.BuffManager.AddElement(BuffNames.BeeAttack, Origin.FromBeingAttackedByBees);
                result = false;
            }
            return result;
        }

        public static string GetBeekeeperOutfitName(Sim actor)
        {
            return string.Format("{0}{1}Beekeeping", OutfitUtils.GetAgePrefix(actor.SimDescription.Age, true), actor.IsMale ? "m" : "f");
        }

        public override bool Run()
        {
            bool baseInteractionFunctionalityDone = false;
            bool outfitChanged;
            bool swimwearUsed;
            baseInteractionFunctionalityDone = DoBaseInteractionFunctionality(mCurrentStateMachine, Actor, this, out outfitChanged, out swimwearUsed, Target);
            if (baseInteractionFunctionalityDone)
            {
                CustomServiceSituation situation = CustomServiceSituation.FindServiceSituationInvolving(Actor) as CustomServiceSituation;
                AnimateSim("Harvest");
                bool honeyStorageLimitMet = Target.mStoredHoneyInfo.Count == BeekeepingBox.kHoneyStorageLimit;
                while (Target.mStoredHoneyInfo.Count > 0)
                {
                    BeekeepingBox.HoneyCreationInfo honeyCreationInfo = Target.mStoredHoneyInfo[0];
                    Target.mStoredHoneyInfo.RemoveAt(0);
                    Ingredient honey = Ingredient.Create(IngredientData.NameToDataMap["Honey"]);
                    honey.SetQuality(honeyCreationInfo.Quality);
                    situation?.TryToAddToInventory(honey);
                    EventTracker.SendEvent(new GuidEvent<Quality>(EventTypeId.kHarvestedHoney, Actor, honeyCreationInfo.Quality));
                    if (honeyCreationInfo.HasBeesWax)
                    {
                        mCurrentStateMachine.SetParameter("harvestsBeeswax", YesOrNo.yes);
                        Ingredient beesWax = Ingredient.Create(IngredientData.NameToDataMap["BeesWax"]);
                        beesWax.SetQuality(honeyCreationInfo.Quality);
                        situation?.TryToAddToInventory(beesWax);
                    }
                }
                AnimateSim("Success");
                if (honeyStorageLimitMet)
                {
                    Target.mHoneyProductionAlarm = Target.AddAlarmRepeating(BeekeepingBox.kHoneyProductionTimeInSimHours, TimeUnit.Hours, Target.ProduceHoney, "Produce Honey", AlarmType.AlwaysPersisted);
                }
            }
            EndCommodityUpdates(baseInteractionFunctionalityDone);
            StandardExit();
            Target.RestorePreviousOutfit(Actor, swimwearUsed, outfitChanged, baseInteractionFunctionalityDone);
            return baseInteractionFunctionalityDone;
        }
    }
}
