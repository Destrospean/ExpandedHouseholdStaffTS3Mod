using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Abstracts.zoeoeAndDestrospean.ServantRolesMod;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Services;
using Sims3.SimIFace.CAS;
using Sims3.UI;
using System;
using System.Collections.Generic;
using zoeoeAndDestrospean.Utils;
using zoeoeAndDestrospean.Utils.ServantRolesMod;

namespace Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Situations
{
    public class CustomServiceSituation : ServiceSituation<CustomServiceSituation>
    {
        public class HangAroundBeforeLeaving : ChildSituation<CustomServiceSituation>
        {
            AlarmHandle mAlarmHandle;

            public HangAroundBeforeLeaving()
            {
            }

            public HangAroundBeforeLeaving(CustomServiceSituation parent) : base(parent)
            {
            }

            public override void CleanUp()
            {
                DebugUtils.TryDisplayScriptError(() =>
                    {
                        AlarmManager.RemoveAlarm(mAlarmHandle);
                        base.CleanUp();
                    });
            }

            public override void Init(CustomServiceSituation parent)
            {
                DebugUtils.TryDisplayScriptError(() => mAlarmHandle = AlarmManager.AddAlarm(((CustomService)parent.Worker.Service).DelayBeforeLeaving, TimeUnit.Hours, TimeToRoute, parent.Worker.Service.GetType().Name + " waiting to leave", AlarmType.DeleteOnReset, parent.Worker));
            }

            public override void OnSocializedWith(Sim sim)
            {
                DebugUtils.TryDisplayScriptError(() =>
                    {
                        CustomService service = (CustomService)Parent.Worker.Service;
                        float timeLeft = AlarmManager.GetTimeLeft(mAlarmHandle, TimeUnit.Hours);
                        if (timeLeft < service.ExtraWaitTimeAfterSocializing)
                        {
                            AlarmManager.UpdateAlarmTime(mAlarmHandle, service.ExtraWaitTimeAfterSocializing - timeLeft, TimeUnit.Hours);
                        }
                    });
            }

            public void TimeToRoute()
            {
                DebugUtils.TryDisplayScriptError(() =>
                    {
                        Parent.NPCLeavingMessage(LeavingReason.NPCJobDone);
                        Parent.SetState(new LeaveLot<CustomServiceSituation>(Parent));
                    });
            }
        }

        public new class NPCIsFired : ServiceSituation<CustomServiceSituation>.NPCIsFired
        {
            public NPCIsFired()
            {
            }

            public NPCIsFired(Sim firer, CustomServiceSituation parent) : base(firer, parent)
            {
            }

            public override void OnFinished(Sim actor, float x)
            {
                DebugUtils.TryDisplayScriptError(() =>
                    {
                        Parent.NPCLeavingMessage(LeavingReason.NPCFired);
                        Parent.SetState(new LeaveLot<ServiceSituation<CustomServiceSituation>>(Parent));
                        Parent.Worker.Service.FireSim(actor);
                    });
            }
        }

        public class PerformDuties : ChildSituation<CustomServiceSituation>
        {
            AlarmHandle mAlarmHandle = AlarmHandle.kInvalidHandle;

            public bool HasDuties
            {
                get
                {
                    bool retVal;
                    return !DebugUtils.TryDisplayScriptError(() =>
                        {
                            if ((Parent.Worker.Service as CustomService)?.Profile.IsScaredOfBonehilda ?? false)
                            {
                                foreach (Sim sim in Lot.GetObjects<Sim>())
                                {
                                    if (sim.SimDescription.IsBonehilda && sim.RoomId == Parent.Worker.RoomId)
                                    {
                                        Parent.Worker.BuffManager.AddElement(BuffNames.Scared, Origin.FromSeeingBonehilda);
                                        Parent.SetState(new QuitCauseOfBonehilda(Parent));
                                        return false;
                                    }
                                }
                            }
                            if (Parent.Worker.CurrentInteraction != null && Parent.Worker.CurrentInteraction.GetPriority().Level <= InteractionPriorityLevel.Autonomous)
                            {
                                InteractionInstance interactionInstance = AutonomyUtils.FindBestAction(Parent.Worker.Autonomy);
                                if (interactionInstance != null && Parent.IsInteractionBetterThanCurrent(interactionInstance))
                                {
                                    Parent.Worker.AddExitReason(ExitReason.CanceledByScript);
                                    return false;
                                }
                            }
                            return true;
                        }, out retVal) && retVal;
                }
            }

            public PerformDuties()
            {
            }

            public PerformDuties(CustomServiceSituation parent) : base(parent)
            {
            }

            public override void Init(CustomServiceSituation parent)
            {
                CustomService service = (CustomService)Parent.Worker.Service;
                DebugUtils.TryDisplayScriptError(() => mAlarmHandle = parent.Worker.AddAlarmRepeating(service.CheckTime, TimeUnit.Minutes, CheckForDuties, service.CheckTime, TimeUnit.Minutes, "Time for " + Parent.Worker.Service.GetType().Name + " to check if everything is done", AlarmType.AlwaysPersisted));
            }

            public override void CleanUp()
            {
                DebugUtils.TryDisplayScriptError(() =>
                    {
                        Parent.Worker.WorkMotive = CommodityKind.None;
                        foreach (CommodityKind motive in Parent.Worker.Service?.ServiceMotives ?? new List<CommodityKind>())
                        {
                            Parent.Worker.Autonomy.Motives.RemoveMotive(motive);
                        }
                        AlarmManager.RemoveAlarm(mAlarmHandle);
                        base.CleanUp();
                    });
            }

            public void CheckForDuties()
            {
                DebugUtils.TryDisplayScriptError(() =>
                    {
                        if (!Parent.ServiceTerminated && !HasDuties && !Parent.IsLiveInService && (!Parent.Worker.BuffManager.HasElement(BuffNames.Scared) || Parent.Worker.BuffManager.GetElement(BuffNames.Scared).BuffOrigin != Origin.FromSeeingBonehilda))
                        {
                            Parent.SetState(new HangAroundBeforeLeaving(Parent));
                        }
                    });
            }
        }

        public class QuitCauseOfBonehilda : ChildSituation<CustomServiceSituation>
        {
            public QuitCauseOfBonehilda()
            {
            }

            public QuitCauseOfBonehilda(CustomServiceSituation parent) : base(parent)
            {
            }

            public override void Init(CustomServiceSituation parent)
            {
                DebugUtils.TryDisplayScriptError(() =>
                    {
                        parent.Worker.InteractionQueue.CancelAllInteractions();
                        RequestWalkStyle(parent.Worker, Sim.WalkStyle.OnFire);
                        ForceSituationSpecificInteraction(parent.Lot, parent.Worker, new Maid.QuitBecauseOfBonehilda.Definition(), null, null, null);
                        parent.Worker.Service.ClearServiceForLot(parent.Lot);
                        parent.Worker.Service.EndService(parent.Worker.SimDescription);
                        ForceSituationSpecificInteraction(parent.Lot, parent.Worker, new DriveAwayInServiceCar.Definition(parent.Car), null, null, null);
                    });
            }
        }

        public class StartPerformingDuties : ChildSituation<CustomServiceSituation>
        {
            public StartPerformingDuties()
            {
            }

            public StartPerformingDuties(CustomServiceSituation parent) : base(parent)
            {
            }

            public override void Init(CustomServiceSituation parent)
            {
                DebugUtils.TryDisplayScriptError(() =>
                    {
                        parent.OnArriveOnLot();
                        parent.SetMotivesAndCommodities();
                        parent.SetState(new PerformDuties(parent));
                    });
            }
        }

        public new class WaitToRoute : ChildSituation<CustomServiceSituation>
        {
            AlarmHandle mAlarmHandle;

            public WaitToRoute()
            {
            }

            public WaitToRoute(CustomServiceSituation parent) : base(parent)
            {
            }

            public override void CleanUp()
            {
                DebugUtils.TryDisplayScriptError(() =>
                    {
                        AlarmManager.RemoveAlarm(mAlarmHandle);
                        base.CleanUp();
                    });
            }

            public override void Init(CustomServiceSituation parent)
            {
                DebugUtils.TryDisplayScriptError(() => mAlarmHandle = AlarmManager.AddAlarm(((CustomService)Parent.Worker.Service).DelayBeforeArriving, TimeUnit.Hours, TimeToRoute, Parent.Worker.Service.GetType().Name + " waiting to route", AlarmType.DeleteOnReset, parent.Worker));
            }

            public void TimeToRoute()
            {
                DebugUtils.TryDisplayScriptError(() =>
                    {
                        Parent.OnServiceStarting();
                        RouteToLot<CustomServiceSituation, StartPerformingDuties> routeToLot = new WalkToLot<CustomServiceSituation, StartPerformingDuties>(Parent);
                        routeToLot.SetRouteTime(((CustomService)Parent.Worker.Service).DriveTime);
                        Parent.SetState(routeToLot);
                    });
            }
        }

        public override bool IsLiveInService
        {
            get
            {
                return (Worker.Service as CustomService)?.Profile.IsLiveInService ?? false;
            }
        }

        public override bool ReportsFires
        {
            get
            {
                return (Worker.Service as CustomService)?.Profile.ReportsFires ?? false;
            }
        }

        public override bool ServiceTerminated
        {
            get
            {
                if (Lot.Household == null)
                {
                    return true;
                }
                foreach (Sim sim in Lot.Household.Sims)
                {
                    if (sim.SimDescription.YoungAdultOrAbove)
                    {
                        Relationship relationship = Relationship.Get(Worker, sim, true);
                        if (relationship.LTR.Liking < ((CustomService)Worker.Service).RelationshipLevelForQuit)
                        {
                            SetToFire(Worker, Worker);
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public CustomServiceSituation()
        {
        }

        public CustomServiceSituation(Service<CustomService> service, Lot lot, Sim worker, int cost) : base(service, lot, worker, cost)
        {
        }

        public override void ChargeForService(Callback callbackOnCompletion)
        {
            int totalCost = CostTotal();
            if (totalCost > 0 && Lot.EffectiveHousehold != null && !mbHasCharged && Lot.EffectiveHousehold.IsActive)
            {
                if (Lot.EffectiveHousehold.FamilyFunds < totalCost)
                {
                    string entryKey = NotEnoughFundsMessage();
                    if (entryKey != null)
                    {
                        string titleText = Localization.LocalizeString(entryKey, (Worker.Service as CustomService)?.Profile.Title);
                        StyledNotification.Show(new StyledNotification.Format(titleText, StyledNotification.NotificationStyle.kGameMessageNegative));
                    }
                    mNumStealAttempts = 0;
                    mObjectsFailedToSteal = new List<GameObject>();
                    mCallbackOnStealCompletion = callbackOnCompletion;
                    ForceSituationSpecificInteraction(Worker, Worker, new ServiceNPCSteal.Definition(this), null, mCallbackOnStealCompletion, StealFailed, new InteractionPriority(InteractionPriorityLevel.High));
                    return;
                }
                Lot.EffectiveHousehold.ModifyFamilyFunds(-totalCost);
            }
            mbHasCharged = true;
            if (callbackOnCompletion != null)
            {
                callbackOnCompletion(Worker, 0f);
            }
        }

        public override void FreezeMotives()
        {
            Worker.Autonomy.Motives.MaxEverything();
            Worker.Autonomy.Motives.FreezeDecayEverythingExcept(CommodityKind.Energy, CommodityKind.Hygiene);
        }

        /*
        public override void OnArriveOnLot()
        {
            Tutorialette.TriggerLesson(Lessons.Maid, null);
            base.OnArriveOnLot();
        }
        */

        public override void SetMotivesAndCommodities()
        {
            CustomService service = (CustomService)Worker.Service;
            CommonUtils.UpdateMotiveTunings(Worker, service.ServiceMotive);
            Worker.Motives.MaxEverything();
            Worker.WorkMotive = service.ServiceMotive;
            foreach (CommodityKind motive in service.ServiceMotives)
            {
                Worker.Motives.CreateMotive(motive);
            }
        }

        public override void SetToFire(Sim serviceSim, Sim firer)
        {
            if (serviceSim == firer)
            {
                EventTracker.SendEvent(EventTypeId.kServiceNPCFired, firer, serviceSim);
                NPCLeavingMessage(LeavingReason.NPCSelfTerminated);
                UnsetServiceBed();
                SetState(new LeaveLot<CustomServiceSituation>(this));
                Service.FireSim(Worker);
            }
            else
            {
                base.SetToFire(serviceSim, firer);
            }
        }

        public override void SwitchWorkerToServiceOutfit()
        {
            DebugUtils.TryDisplayScriptError(() =>
                {
                    CustomService service = (CustomService)Worker.Service;
                    OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                    if (Worker.SimDescription.TryGetOutfitAssignment(service.Profile, out outfitAssignment) && Worker.AddAssignedOutfit(outfitAssignment.SpecialOutfitKey))
                    {
                        Worker.SimDescription.AddOutfit(new SimOutfit(Worker.SimDescription.GetSpecialOutfit(outfitAssignment.SpecialOutfitKey).Key), OutfitCategories.Career, true);
                        for (int i = Worker.SimDescription.GetOutfitCount(OutfitCategories.Career) - 1; i > 0; i--)
                        {
                            Worker.SimDescription.RemoveOutfit(OutfitCategories.Career, i, true);
                        }
                    }
                    else if (service.Profile.UseServiceTypeOutfit)
                    {
                        base.SwitchWorkerToServiceOutfit();
                    }
                });
        }
    }
}
