using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.ChildAndTeenUpdates;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.ObjectComponents;
using Sims3.Gameplay.Seasons;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Situations;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.TuningValues;
using Sims3.Gameplay.UI;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.SimIFace.Enums;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod
{
    /// <summary>
    /// Alternative methods for autonomy-related things. Use these methods instead of the originals for services and service situations from the Servant Roles Mod.
    /// </summary>
    public class AutonomyUtils
    {
        static InteractionInstance FindBestAction(Autonomy.Autonomy autonomy, CommodityKind c, bool metaAutonomy)
        {
            autonomy.ClearScoring();
            autonomy.mHasBeenScored = Autonomy.Autonomy.InteractionCheckTable.Allocate();
            autonomy.AddDecisionSnapshot();
            ScoreInteractionsOnObjects(autonomy, c, metaAutonomy);
            InteractionInstance result = autonomy.AssignProbabilitiesAndChooseInteraction();
            autonomy.ClearScoring();
            LiveDragHelperModel.ClearCachedTopDraggedObject();
            return result;
        }

        static void ScoreInteractionsForLocalAutonomy(Autonomy.Autonomy autonomy, CommodityKind c)
        {
            Lot lot = autonomy.mActor.LotCurrent;
            if (autonomy.mOverriddenLocalAutonomyLot != null)
            {
                lot = autonomy.mOverriddenLocalAutonomyLot;
            }
            else if (lot.IsWorldLot)
            {
                lot = autonomy.FindClosestLotForLocalAutonomy();
            }
            if (lot != null)
            {
                autonomy.ScoreInteractions(lot.Map, c);
                autonomy.ScoreInteractions(autonomy.mActor.SimCommodityInteractionMap, c);
                if (!lot.IsWorldLot || autonomy.mOverriddenLocalAutonomyLot != null)
                {
                    foreach (Sim sim in lot.GetObjects<Sim>())
                    {
                        if (sim != autonomy.mActor)
                        {
                            autonomy.ScoreInteractions(sim.SimCommodityInteractionMap, c);
                        }
                    }
                }
            }
            if (!autonomy.mActor.IsNPC || !autonomy.mActor.LotCurrent.IsWorldLot)
            {
                autonomy.ScoreInteractions(autonomy.mActor.Inventory.CommodityInteractionMap, c);
            }
            if (c == CommodityKind.None)
            {
                ScoreInteractionsForSituations(autonomy);
            }
            if (c == CommodityKind.None || c == CommodityKind.AlienBrainPower || autonomy.mForceScoreSoloInteractions)
            {
                autonomy.ScoreSoloSimInteractions(c);
            }
        }

        static void ScoreInteractionsForSituations(Autonomy.Autonomy autonomy)
        {
            foreach (Situation situation in autonomy.mSituationComponent.Situations)
            {
                foreach (Sim sim in situation.SimsWithInteractions)
                {
                    if (sim.HasBeenDestroyed)
                    {
                        continue;
                    }
                    List<InteractionObjectPair> situationSpecificInteractionsForActor = sim.GetSituationSpecificInteractionsForActor(autonomy.mActor);
                    if (situationSpecificInteractionsForActor == null)
                    {
                        continue;
                    }
                    foreach (InteractionObjectPair item in situationSpecificInteractionsForActor)
                    {
                        CalculateScoreAndAddToCandidates(autonomy, item);
                    }
                }
            }
        }

        static Autonomy.Autonomy.YieldResult ScoreInteractionsOnObjects(Autonomy.Autonomy autonomy, CommodityKind c, bool metaAutonomy)
        {
            Autonomy.Autonomy.YieldResult result = Autonomy.Autonomy.YieldResult.Continue;
            autonomy.mUseCachedValues = true;
            try
            {
                autonomy.CacheValuesToOptimizeTestFunctions(autonomy.mActor);
                if (autonomy.ShouldRunLocalAutonomy)
                {
                    ScoreInteractionsForLocalAutonomy(autonomy, c);
                }
                if (metaAutonomy)
                {
                    return autonomy.ScoreInteractionsForMetaAutonomy(c);
                }
                return result;
            }
            finally
            {
                autonomy.mUseCachedValues = false;
            }
        }

        public static float CalculateScore(Autonomy.Autonomy autonomy, InteractionObjectPair iop)
        {
            return TestAndCalculateScore(iop, autonomy, autonomy.CurrentSearchType, autonomy.GetInteractionFlagsForCurrentSearch());
        }

        public static void CalculateScoreAndAddToCandidates(Autonomy.Autonomy autonomy, InteractionObjectPair iop)
        {
            if (autonomy.mHasBeenScored == null)
            {
                autonomy.mHasBeenScored = Autonomy.Autonomy.InteractionCheckTable.Allocate();
            }
            if (autonomy.mHasBeenScored.TryAdd(iop))
            {
                float score = CalculateScore(autonomy, iop);
                autonomy.mBestScores.Add(iop, score);
            }
        }

        public static float CalculateScoreForObjectInteraction(Autonomy.Autonomy autonomy, InteractionObjectPair iop)
        {
            ScoreDebugInfo tradeoffScore;
            return CalculateScoreForObjectInteraction(autonomy, iop, out tradeoffScore);
        }

        public static float CalculateScoreForObjectInteraction(Autonomy.Autonomy autonomy, InteractionObjectPair iop, out ScoreDebugInfo tradeoffScore)
        {
            tradeoffScore = null;
            Tradeoff tradeoff = iop.Tradeoff;
            if (tradeoff != null)
            {
                float initialMultiplier = 1f;
                if (iop.Tuning.ScoringFunction != null)
                {
                    initialMultiplier = iop.Tuning.ScoringFunction(autonomy.mActor, iop);
                }
                CommodityKind mostValuedCommodity = CommodityKind.None;
                float metaOutputScore = 0f;
                IMetaInteractionDefinition metaInteractionDefinition = iop.InteractionDefinition as IMetaInteractionDefinition;
                if (metaInteractionDefinition != null)
                {
                    metaOutputScore = metaInteractionDefinition.MetaAdScore(autonomy.mActor, iop.Target, autonomy.mInteractionScorer, ref mostValuedCommodity);
                }
                Sim actor = null;
                if (autonomy.ActorsGroupSituation != null)
                {
                    actor = autonomy.ActorsGroupSituation.Leader;
                }
                if (actor == null)
                {
                    actor = autonomy.mActor;
                }
                float score = tradeoff.GetScore(actor, autonomy.mInteractionScorer, iop.Target, iop, metaOutputScore, out tradeoffScore, initialMultiplier, iop.Tuning.ScoringFunctionOnlyAppliesToSpecificCommodity);
                float multiplier = 1f;
                IGameObject target = iop.Target;
                Sim targetSim = iop.Target as Sim;
                bool isOutside = target.IsOutside;
                if (isOutside)
                {
                    if (autonomy.mActor.TraitManager.HasElement(TraitNames.HatesOutdoors))
                    {
                        multiplier -= TraitTuning.HatesOutdoorsTraitAdvertisingMultiplier;
                    }
                    else if (autonomy.mActor.TraitManager.HasElement(TraitNames.LovesTheOutdoors))
                    {
                        multiplier += TraitTuning.LovesOutdoorsTraitAdvertisingMultiplier;
                    }
                }
                if (autonomy.mActor.IsADogSpecies && autonomy.mActor.TraitManager.HasElement(TraitNames.LoyalPet) && autonomy.Actor != targetSim)
                {
                    Sim bffOrSimWithHighestLikingForPet = Relationship.GetBFFOrSimWithHighestLikingForPet(autonomy.mActor);
                    if (bffOrSimWithHighestLikingForPet != null)
                    {
                        if (targetSim == bffOrSimWithHighestLikingForPet)
                        {
                            multiplier *= TraitTuning.LoyalDogTraitAdvertisingMultiplierForFavoriteSim;
                        }
                        else if (bffOrSimWithHighestLikingForPet.LotCurrent == autonomy.Actor.LotCurrent && bffOrSimWithHighestLikingForPet.RoomId == target.RoomId)
                        {
                            multiplier *= TraitTuning.LoyalDogTraitAdvertisingMultiplier;
                        }
                    }
                }
                if (autonomy.mActor.TraitManager.HasElement(TraitNames.Loner))
                {
                    if (autonomy.mActor.Bed != null && autonomy.mActor.Bed.LotCurrent == target.LotCurrent && autonomy.mActor.Bed.RoomId == target.RoomId)
                    {
                        multiplier += TraitTuning.LonerTraitAdvertisingMultiplier;
                    }
                    int simCount = 0;
                    if (target.LotCurrent != null)
                    {
                        foreach (Sim sim in iop.Target.LotCurrent.GetAllActors())
                        {
                            if (sim != autonomy.mActor && sim.RoomId == autonomy.mActor.RoomId)
                            {
                                simCount++;
                            }
                        }
                    }
                    multiplier -= Math.Min(simCount * TraitTuning.LonerTraitAdvertisingMultiplierForSims, TraitTuning.LonerTraitAdvertisingMultiplierForSimsMax);
                    multiplier = Math.Max(0f, multiplier);
                }
                if (autonomy.IsGuestAtAParty)
                {
                    Party partyIAmGuestAt = Party.GetPartyIAmGuestAt(autonomy.mActor);
                    if (partyIAmGuestAt != null)
                    {
                        Sim host = partyIAmGuestAt.Host;
                        if (autonomy.mActor.LotCurrent == host.LotCurrent && target.RoomId == host.RoomId)
                        {
                            multiplier += Party.ScoringMultiplierForNearHostAtParty;
                        }
                    }
                }
                if (autonomy.ActorsGroupSituation != null && (tradeoff.SatisfiesCommodity(CommodityKind.Fun) || tradeoff.SatisfiesCommodity(CommodityKind.Social)))
                {
                    multiplier += GroupingSituation.kScoringMulitplierForGroup;
                }
                if (autonomy.mActor.SimDescription.Teen && (iop.Target as GameObject).ActorsUsingMe.Count > 0)
                {
                    foreach (Sim sim in (target as GameObject).ActorsUsingMe)
                    {
                        Sim s = sim as Sim;
                        Relationship relationship = Relationship.Get(s, autonomy.mActor, false);
                        if (relationship != null && (relationship.LTR.LTRInteractionBits & LongTermRelationship.InteractionBits.BFF) != 0)
                        {
                            multiplier += SocialComponent.BFFAutonomyMultiplier;
                            break;
                        }
                    }
                }
                if (autonomy.mActor.Service != null)
                {
                    Type serviceType = autonomy.mActor.Service.GetType();
                    PropertyInfo useObjectInSameRoomAsSleeperMultiplierProperty = serviceType.GetProperty("UseObjectInSameRoomAsSleeperMultiplier");
                    if (autonomy.mActor.Service.ServiceType == ServiceType.Babysitter)
                    {
                        if ((target as GameObject).ActorsUsingMe.Count > 0)
                        {
                            foreach (Sim sim in (target as GameObject).ActorsUsingMe)
                            {
                                if (sim.Service == null && sim.SimDescription.ChildOrBelow && score != 0f)
                                {
                                    score *= Babysitter.UseObjectInSameRoomAsChildMultiplier;
                                }
                            }
                        }
                        Household household = autonomy.mActor.LotCurrent.Household;
                        if (household != null && !iop.Tradeoff.SatisfiesCommodity(CommodityKind.LookAfterBabyOrToddler))
                        {
                            foreach (Sim sim in household.Sims)
                            {
                                if (autonomy.mActor.RoomId != sim.RoomId && sim.SimDescription.ChildOrBelow && sim.RoomId == target.RoomId && score != 0f)
                                {
                                    score += Babysitter.UseObjectInSameRoomAsChildBaseScore;
                                }
                            }
                        }
                    }
                    else if (ServiceUtils.IsFromServantRolesMod(serviceType) && (bool)serviceType.GetProperty("IsQuietAroundSleepingSims").GetValue(autonomy.mActor.Service, null) && useObjectInSameRoomAsSleeperMultiplierProperty != null && useObjectInSameRoomAsSleeperMultiplierProperty.PropertyType == typeof(float) && target as IBed == null)
                    {
                        int roomId = iop.Target.RoomId;
                        if (autonomy.mActor.LotCurrent.Household != null)
                        {
                            foreach (Sim sim in autonomy.mActor.LotCurrent.Household.Sims)
                            {
                                if (sim.IsSleeping && sim.RoomId == roomId)
                                {
                                    score *= (float)useObjectInSameRoomAsSleeperMultiplierProperty.GetValue(null, null);
                                }
                            }
                        }
                    }
                }
                if (iop.Tuning.ScoringFunction != null && iop.Tuning.ScoringFunctionOnlyAppliesToSpecificCommodity == CommodityKind.None)
                {
                    multiplier *= initialMultiplier;
                }
                if (target != null)
                {
                    multiplier *= target.GetBlockingScore();
                }
                if (target.IsInPublicResidentialRoom && (target.LotCurrent != autonomy.mActor.LotCurrent || !autonomy.mActor.IsInPublicResidentialRoom))
                {
                    multiplier *= Autonomy.Autonomy.AutonomyPublicAreaPenaltyMultiplier;
                }
                for (Posture posture = autonomy.mActor.Posture; posture != null; posture = posture.PreviousPosture)
                {
                    multiplier *= posture.GetAutonomyScoreMultiplierForInteraction(autonomy.mActor, target, iop.Tuning.PosturePreconditions);
                }
                if (isOutside && autonomy.mActor.SimDescription.IsVampire && !SimClock.IsNightTime())
                {
                    multiplier *= Autonomy.Autonomy.kVampireDaylightOutdoorMultiplier;
                }
                if (targetSim != null && targetSim.Posture is IInBoxStallPosture && iop.InteractionDefinition as IBoxStallAllowedInteractionDefinition == null && (autonomy.mActor != targetSim || autonomy.mCurrentSearchType != AutonomySearchType.BuffAutoSolve))
                {
                    return 0f;
                }
                if (autonomy.mActor.mSnubManager != null && autonomy.mActor.mSnubManager.ListOfSnubTargets != null)
                {
                    foreach (ulong simDescriptionId in autonomy.mActor.mSnubManager.ListOfSnubTargets)
                    {
                        SimDescription simDescription = SimDescription.Find(simDescriptionId);
                        if (simDescription == null)
                        {
                            continue;
                        }
                        Sim createdSim = simDescription.CreatedSim;
                        if (createdSim != null)
                        {
                            if ((iop.Target.Position - createdSim.Position).LengthSqr() <= (float)SnubManager.kAutonomousInteractionLimiterDistanceSqr)
                            {
                                multiplier *= SnubManager.kAutonomousInteractionLimiterMultiplier;
                            }
                        }
                    }
                }
                if (iop.Tuning.Availability.HasFlags(Availability.FlagField.AllowEvenIfNotAllowedInRoomAutonomous) && !autonomy.mActor.IsAllowedInRoom(iop.Target.RoomId))
                {
                    multiplier *= Autonomy.Autonomy.kAllowEvenIfNotAllowedInRoomAutonomousMultiplier;
                }
                if (SeasonsManager.Enabled && (SeasonsManager.CurrentWeather == Weather.Rain || SeasonsManager.CurrentWeather == Weather.Hail) && autonomy.mCurrentSearchType == AutonomySearchType.Autonomy)
                {
                    if (isOutside && targetSim == null && !SeasonsManager.IsShelteredFromPrecipitation(target, iop.InteractionDefinition.CreateInstance(target, autonomy.Actor, new InteractionPriority(InteractionPriorityLevel.Autonomous), true, true)))
                    {
                        multiplier *= SeasonsManager.kRainHailObjectAutonomyScoringMultiplier;
                    }
                    if (target.InInventory)
                    {
                        ItemComponent itemComp = target.ItemComp;
                        Inventory inventory = autonomy.mActor.Inventory;
                        if (itemComp != null && inventory != null && itemComp.InventoryParent == inventory && !SeasonsManager.IsShelteredFromPrecipitation(autonomy.mActor))
                        {
                            multiplier = target is IDontUseAutonomouslyOutsideInRainObject ? 0f : multiplier * SeasonsManager.kRainHailObjectAutonomyScoringMultiplier;
                        }
                    }
                }
                score *= multiplier;
                float scoreBonus = 0f;
                InteractionInstance currentInteraction = autonomy.Actor.CurrentInteraction;
                if (currentInteraction as OccultImaginaryFriend.ImaginaryFriendCleanHouse == null)
                {
                    scoreBonus = autonomy.ComputeCheckScore(iop.Checks, tradeoffScore, tradeoff, autonomy.mActor, iop.Target);
                }
                float finalScore = score + scoreBonus;
                if (autonomy.mInteractionScorer.InteractionModifier != null)
                {
                    finalScore *= autonomy.mInteractionScorer.InteractionModifier(iop);
                }
                float inUseAutonomyMultiplier = target.GetInUseAutonomyMultiplier(autonomy.mActor);
                finalScore *= inUseAutonomyMultiplier;
                multiplier *= inUseAutonomyMultiplier;
                return finalScore;
            }
            autonomy.AddSnapshotScore(iop, "[No tradeoff] " + iop.InteractionDefinition.ToString(), iop.Target.ToString());
            return 0f;
        }

        /// <summary>
        /// Finds the best available interaction. Use this instead of the original for service situations from the Servant Roles Mod.
        /// </summary>
        public static InteractionInstance FindBestAction(Autonomy.Autonomy autonomy)
        {
            autonomy.SetSearchType(AutonomySearchType.Generic);
            InteractionInstance result = FindBestAction(autonomy, CommodityKind.None, false);
            autonomy.ClearSearchType(AutonomySearchType.Generic);
            return result;
        }

        public static float TestAndCalculateScore(InteractionObjectPair iop, Autonomy.Autonomy autonomy, AutonomySearchType autonomySearchType, InteractionFlags flags)
        {
            if (autonomy.OverrideInteractionParameters.HasValue)
            {
                flags |= autonomy.OverrideInteractionParameters.Value.Flags;
            }
            InteractionInstanceParameters parameters = new InteractionInstanceParameters(iop, autonomy.Actor, new InteractionPriority(InteractionPriorityLevel.Autonomous), autonomySearchType, flags);
            GreyedOutTooltipCallback greyedOutTooltipCallback = null;
            if (iop.InteractionDefinition.Test(ref parameters, ref greyedOutTooltipCallback) != 0)
            {
                return 0f;
            }
            IOverridesCalculateScore overridesCalculateScore = iop.mInteraction as IOverridesCalculateScore;
            if (overridesCalculateScore != null)
            {
                return overridesCalculateScore.CalculateScore(iop, autonomy);
            }
            return CalculateScoreForObjectInteraction(autonomy, iop);
        }
    }
}
