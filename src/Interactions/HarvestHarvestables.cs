using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.ObjectComponents;
using Sims3.Gameplay.Objects.Gardening;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Skills;
using Sims3.Gameplay.TuningValues;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.Destrospean.ExpandedHouseholdStaff.Situations;
using Sims3.SimIFace;
using Sims3.UI;
using System.Collections.Generic;

namespace Sims3.Gameplay.Destrospean.ExpandedHouseholdStaff.Interactions
{
    public class HarvestHarvestables : Plant.ChainableGardeningInteraction<HarvestPlant>, BurglarSituation.IStealInteraction
    {
        public class Definition : InteractionDefinition<Sim, HarvestPlant, HarvestHarvestables>
        {
            public override string GetInteractionName(Sim actor, HarvestPlant target, InteractionObjectPair iop)
            {
                return Localization.LocalizeString("Gameplay/Objects/Gardening/HarvestPlant/Harvest:InteractionName", actor, target.GetLocalizedName());
            }

            public override bool Test(Sim actor, HarvestPlant target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                return CustomServiceSituation.FindServiceSituationInvolving(actor) is CustomServiceSituation && target as ForbiddenFruitTree == null && HarvestPlant.HarvestTest(target, actor);
            }
        }

        public BurglarSituation mBurglarSituation;

        public Soil mDummyIk;

        public static InteractionDefinition Singleton = new Definition();

        public override void Cleanup()
        {
            if (mDummyIk != null)
            {
                mDummyIk.Destroy();
                mDummyIk = null;
            }
            base.Cleanup();
        }

        public override void ConfigureInteraction()
        {
            float harvestDuration = Target.GetHarvestDuration(Actor);
            TimedStage timedStage = new TimedStage(GetInteractionName(), harvestDuration, false, true, true);
            base.Stages = new List<Stage>
                {
                    timedStage
                };
        }

        public bool DoHarvest()
        {
            Target.RemoveHarvestStateTimeoutAlarm();
            StandardEntry();
            BeginCommodityUpdates();
            Soil dummyIk;
            StateMachineClient stateMachine = Target.GetStateMachine(Actor, out dummyIk);
            mDummyIk = dummyIk;
            bool hasHarvested = true;
            if (Actor.IsInActiveHousehold)
            {
                hasHarvested = false;
                foreach (SimDescription simDescription in Actor.Household.SimDescriptions)
                {
                    Gardening gardeningSkill = simDescription.SkillManager.GetSkill<Gardening>(SkillNames.Gardening);
                    if (gardeningSkill != null && gardeningSkill.HasHarvested())
                    {
                        hasHarvested = true;
                        break;
                    }
                }
            }
            if (stateMachine != null)
            {
                stateMachine.RequestState("x", "Loop Harvest");
            }
            Plant.StartStagesForTendableInteraction(this);
            while (!Actor.WaitForExitReason(Sim.kWaitForExitReasonDefaultTime, ExitReason.Default))
            {
                if (base.ActiveStage != null && base.ActiveStage.IsComplete(this))
                {
                    Actor.AddExitReason(ExitReason.StageComplete);
                }
            }
            Plant.PauseTendGardenInteractionStage(Actor.CurrentInteraction);
            if (Actor.HasExitReason(ExitReason.StageComplete))
            {
                DoHarvest(Actor, Target, hasHarvested, mBurglarSituation);
            }
            if (stateMachine != null)
            {
                stateMachine.RequestState("x", "Exit Standing");
            }
            EndCommodityUpdates(true);
            StandardExit();
            Plant.UpdateTendGardenTimeSpent(this, SetHarvestTimeSpent);
            return Actor.HasExitReason(ExitReason.StageComplete);
        }

        public static bool DoHarvest(Sim actor, HarvestPlant target, bool hasHarvested, BurglarSituation burglarSituation)
        {
            Slot[] containmentSlots = target.GetContainmentSlots();
            List<GameObject> seeds = new List<GameObject>();
            for (int i = 0; i < containmentSlots.Length; i++)
            {
                GameObject seed = target.GetContainedObject(containmentSlots[i]) as GameObject;
                if (seed != null && HarvestHarvestable(target, seed, actor, burglarSituation, CustomServiceSituation.FindServiceSituationInvolving(actor) as CustomServiceSituation))
                {
                    seeds.Add(seed);
                }
            }
            if (actor.TraitManager.HasElement(TraitNames.GathererTrait) && RandomUtil.RandomChance01(TraitTuning.GathererTraitExtraHarvestablesChance))
            {
                int gathererTraitNumberOfExtraHarvestables = TraitTuning.GathererTraitNumberOfExtraHarvestables;
                for (int i = 0; i < gathererTraitNumberOfExtraHarvestables; i++)
                {
                    GameObject seed = target.Seed.Copy(false) as GameObject;
                    if (seed != null)
                    {
                        seeds.Add(seed);
                    }
                }
            }
            ScienceSkill scienceSkill = (ScienceSkill)actor.SkillManager.GetElement(SkillNames.Science);
            if (scienceSkill != null && RandomUtil.RandomChance01(ScienceSkill.kSuccessRateBonusesGardening[scienceSkill.SkillLevel]))
            {
                int numExtraHarvestables = ScienceSkill.kNumExtraHarvestables[scienceSkill.SkillLevel];
                for (int i = 0; i < numExtraHarvestables; i++)
                {
                    GameObject seed = target.Seed.Copy(false) as GameObject;
                    if (seed != null)
                    {
                        seeds.Add(seed);
                    }
                }
            }
            if (seeds.Count > 0)
            {
                foreach (GameObject current in seeds)
                {
                    target.ManipulateHarvestable(actor, current);
                }
                Gardening gardeningSkill = actor.SkillManager.GetSkill<Gardening>(SkillNames.Gardening);
                Collecting collectingSkill = actor.SkillManager.GetSkill<Collecting>(SkillNames.Collecting);
                int skillDifficulty = target.PlantDef.GetSkillDifficulty();
                if (gardeningSkill != null)
                {
                    if (gardeningSkill.SkillLevel <= skillDifficulty)
                    {
                        if (actor.SimDescription.IsFairy && gardeningSkill.IsFairySkill())
                        {
                            gardeningSkill.AddPoints(target.PlantDef.SkillPointsHarvest * (float)Skill.SkillLevelBumpMultiplierForFairies);
                        }
                        else
                        {
                            gardeningSkill.AddPoints(target.PlantDef.SkillPointsHarvest);
                        }
                    }
                    target.UpdateGardeningSkillJournal(gardeningSkill, target.PlantDef, seeds);
                }
                if (!hasHarvested)
                {
                    actor.ShowTNSIfSelectable(Localization.LocalizeString(actor.IsFemale, "Gameplay/Objects/Gardening/HarvestPlant/Harvest:FirstHarvest", new object[] {
                        actor,
                        target.PlantDef.Name
                    }), StyledNotification.NotificationStyle.kGameMessagePositive, target.ObjectId, actor.ObjectId);
                }
                if (collectingSkill == null)
                {
                    collectingSkill = (Collecting)actor.SkillManager.AddElement(SkillNames.Collecting);
                }
                collectingSkill.CollectedFromHarvest(seeds);
                target.PostHarvest();
                return true;
            }
            return false;
        }

        public static bool HarvestHarvestable(HarvestPlant plant, GameObject harvestable, Sim actor, BurglarSituation burglarSituation, CustomServiceSituation customServiceSituation)
        {
            if (harvestable == null)
            {
                return false;
            }
            harvestable.SetOwnerLot(actor.LotHome);
            harvestable.UnParent();
            harvestable.RemoveFromWorld();
            bool hasHarvested = false;
            if (burglarSituation == null && customServiceSituation == null)
            {
                if (actor.Inventory.TryToAdd(harvestable))
                {
                    hasHarvested = true;
                }
            }
            else if (customServiceSituation != null)
            {
                if (customServiceSituation.TryToAddToInventory(harvestable))
                {
                    hasHarvested = true;
                }
            }
            else
            {
                burglarSituation.StolenObjects.Add(harvestable);
                burglarSituation.CurrentValue += harvestable.Value;
                hasHarvested = true;
            }
            if (hasHarvested)
            {
                harvestable.AddFlags(GameObject.FlagField.WasHarvested);
                harvestable.RemoveComponent<HarvestableComponent>();
                plant.PostHarvestHarvestable(actor, harvestable);
            }
            else
            {
                harvestable.Destroy();
            }
            return hasHarvested;
        }

        public override bool Run()
        {
            bool previousInteractionSuccessful = false;
            if (Target.RouteSimToMeAndCheckInUse(Actor) && HarvestPlant.HarvestTest(Target, Actor))
            {
                ConfigureInteraction();
                Plant.TryConfigureTendGardenInteraction(Actor.CurrentInteraction);
                previousInteractionSuccessful = DoHarvest();
            }
            if (IsChainingPermitted(previousInteractionSuccessful))
            {
                IgnorePlants.Add(Target);
                if (Target.LotCurrent != null && Target.LotCurrent.IsWorldLot)
                {
                    PushNextInteractionInChain(Singleton, HarvestPlant.HarvestTestWorldLot, Target.LotCurrent);
                }
                else
                {
                    PushNextInteractionInChain(Singleton, HarvestPlant.HarvestTest, Target.LotCurrent);
                }
            }
            return previousInteractionSuccessful;
        }

        public void SetBurglarSituation(BurglarSituation parent)
        {
            mBurglarSituation = parent;
        }

        public static void SetHarvestTimeSpent(Plant.ITendGarden tendGardenInteraction, float timeSpent)
        {
            tendGardenInteraction.HarvestTimeSpent = timeSpent;
        }
    }
}
