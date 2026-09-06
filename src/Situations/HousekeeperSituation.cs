using Sims3.Gameplay.Abstracts.zoeoeAndDestrospean.ServantRolesMod;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Services;
using Sims3.UI;
using System;
using System.Collections.Generic;
using zoeoeAndDestrospean.Utils;
using zoeoeAndDestrospean.Utils.ServantRolesMod;

namespace Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Situations
{
    public class HousekeeperSituation : ServiceSituation<HousekeeperSituation>
    {
        public class Cleaning : ChildSituation<HousekeeperSituation>
        {
            AlarmHandle mAlarmHandle = AlarmHandle.kInvalidHandle;

            public bool HasDuties
            {
                get
                {
                    bool retVal;
                    return !DebugUtils.TryDisplayScriptError(() =>
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
                            if (Parent.Worker.CurrentInteraction != null && Parent.Worker.CurrentInteraction.GetPriority().Level <= InteractionPriorityLevel.Autonomous)
                            {
                                InteractionInstance interactionInstance = AutonomyUtils.FindBestAction(Parent.Worker.Autonomy);
                                if (interactionInstance != null && Parent.IsInteractionBetterThanCurrent(interactionInstance))
                                {
                                    Parent.Worker.AddExitReason(ExitReason.CanceledByScript);
                                    return false;
                                }
                            }
                            if (Lot.FindObjectToClean() != null || Lot.HasCleanAllAblePuddlesOrBurntTiles() || Lot.HasUnmadeBed())
                            {
                                return true;
                            }
                            InteractionQueue interactionQueue = Parent.Worker.InteractionQueue;
                            if (interactionQueue != null)
                            {
                                InteractionInstance headInteraction = interactionQueue.GetHeadInteraction();
                                if (headInteraction != null && headInteraction.SatisfiesCommodity(Housekeeper.ServiceMotive))
                                {
                                    return true;
                                }
                            }
                            return false;
                        }, out retVal) && retVal;
                }
            }

            public Cleaning()
            {
            }

            public Cleaning(HousekeeperSituation parent) : base(parent)
            {
            }

            public override void Init(HousekeeperSituation parent)
            {
                DebugUtils.TryDisplayScriptError(() =>
                    {
                        parent.Worker.GreetSimOnLot(parent.Lot);
                        mAlarmHandle = parent.Worker.AddAlarmRepeating(Housekeeper.CheckTime, TimeUnit.Minutes, CheckForDuties, Housekeeper.CheckTime, TimeUnit.Minutes, "Time for Housekeeper to check if everything is cleaned", AlarmType.AlwaysPersisted);
                    });
            }

            public override void CleanUp()
            {
                DebugUtils.TryDisplayScriptError(() =>
                    {
                        Parent.Worker.WorkMotive = CommodityKind.None;
                        Parent.Worker.Autonomy.Motives.RemoveMotive(CommodityKind.BeMaid);
                        Parent.Worker.Autonomy.Motives.RemoveMotive(Housekeeper.ServiceMotive);
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

        public class HangAroundBeforeLeaving : ChildSituation<HousekeeperSituation>
        {
            AlarmHandle mAlarmHandle;

            public HangAroundBeforeLeaving()
            {
            }

            public HangAroundBeforeLeaving(HousekeeperSituation parent) : base(parent)
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

            public override void Init(HousekeeperSituation parent)
            {
                DebugUtils.TryDisplayScriptError(() => mAlarmHandle = AlarmManager.AddAlarm(Housekeeper.DelayBeforeLeaving, TimeUnit.Hours, TimeToRoute, "Housekeeper waiting to leave", AlarmType.DeleteOnReset, parent.Worker));
            }

            public override void OnSocializedWith(Sim sim)
            {
                DebugUtils.TryDisplayScriptError(() =>
                    {
                        float timeLeft = AlarmManager.GetTimeLeft(mAlarmHandle, TimeUnit.Hours);
                        if (timeLeft < Housekeeper.ExtraWaitTimeAfterSocializing)
                        {
                            AlarmManager.UpdateAlarmTime(mAlarmHandle, Housekeeper.ExtraWaitTimeAfterSocializing - timeLeft, TimeUnit.Hours);
                        }
                    });
            }

            public void TimeToRoute()
            {
                DebugUtils.TryDisplayScriptError(() =>
                    {
                        Parent.NPCLeavingMessage(LeavingReason.NPCJobDone);
                        Parent.SetState(new LeaveLot<HousekeeperSituation>(Parent));
                    });
            }
        }

        public new class NPCIsFired : ServiceSituation<HousekeeperSituation>.NPCIsFired
        {
            public NPCIsFired()
            {
            }

            public NPCIsFired(Sim firer, HousekeeperSituation parent) : base(firer, parent)
            {
            }

            public override void OnFinished(Sim actor, float x)
            {
                DebugUtils.TryDisplayScriptError(() =>
                    {
                        Parent.NPCLeavingMessage(LeavingReason.NPCFired);
                        Parent.SetState(new LeaveLot<ServiceSituation<HousekeeperSituation>>(Parent));
                        Parent.Service.FireSim(actor);
                    });
            }
        }

        public class QuitCauseOfBonehilda : ChildSituation<HousekeeperSituation>
        {
            public QuitCauseOfBonehilda()
            {
            }

            public QuitCauseOfBonehilda(HousekeeperSituation parent) : base(parent)
            {
            }

            public override void Init(HousekeeperSituation parent)
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

        public class StartCleaning : ChildSituation<HousekeeperSituation>
        {
            public StartCleaning()
            {
            }

            public StartCleaning(HousekeeperSituation parent) : base(parent)
            {
            }

            public override void Init(HousekeeperSituation parent)
            {
                DebugUtils.TryDisplayScriptError(() =>
                    {
                        parent.OnArriveOnLot();
                        parent.SetMotivesAndCommodities();
                        parent.SetState(new Cleaning(parent));
                    });
            }
        }

        public new class WaitToRoute : ChildSituation<HousekeeperSituation>
        {
            AlarmHandle mAlarmHandle;

            public WaitToRoute()
            {
            }

            public WaitToRoute(HousekeeperSituation parent) : base(parent)
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

            public override void Init(HousekeeperSituation parent)
            {
                DebugUtils.TryDisplayScriptError(() => mAlarmHandle = AlarmManager.AddAlarm(Housekeeper.DelayBeforeArriving, TimeUnit.Hours, TimeToRoute, "Housekeeper waiting to route", AlarmType.DeleteOnReset, parent.Worker));
            }

            public void TimeToRoute()
            {
                DebugUtils.TryDisplayScriptError(() =>
                    {
                        Parent.OnServiceStarting();
                        RouteToLot<HousekeeperSituation, StartCleaning> routeToLot = new WalkToLot<HousekeeperSituation, StartCleaning>(Parent);
                        routeToLot.SetRouteTime(Housekeeper.DriveTime);
                        Parent.SetState(routeToLot);
                    });
            }
        }

        int mDateLastPaid;

        AlarmHandle mPayHousekeeperAlarm = AlarmHandle.kInvalidHandle;

        public int DayCountSinceLastPayment
        {
            get
            {
                int dayCountSinceLastPayment = SimClock.ElapsedCalendarDays() - mDateLastPaid;
                return dayCountSinceLastPayment > 0 ? dayCountSinceLastPayment : 1;
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
                        if (relationship.LTR.Liking < Housekeeper.RelationshipLevelForQuit)
                        {
                            SetToFire(Worker, Worker);
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public HousekeeperSituation()
        {
        }

        public HousekeeperSituation(Service<Housekeeper> service, Lot lot, Sim worker, int cost) : base(service, lot, worker, cost)
        {
            mDateLastPaid = SimClock.ElapsedCalendarDays();
        }

        public override int CostTotal()
        {
            return Cost * DayCountSinceLastPayment / 7;
        }

        public override void EndService()
        {
            Worker.RemoveAlarm(mPayHousekeeperAlarm);
            base.EndService();
        }

        public override void FreezeMotives()
        {
            Worker.Autonomy.Motives.MaxEverything();
            Worker.Autonomy.Motives.FreezeDecayEverythingExcept(CommodityKind.Energy, CommodityKind.Hygiene);
        }

        public override bool IsInteractionBetterThanCurrent(InteractionInstance ii)
        {
            return ii.ScoreIsConsiderablyHigher(Worker.CurrentInteraction.GetPriority().Value) && Worker.CurrentInteraction.Autonomous;
        }

        public override void OnArriveOnLot()
        {
            //GreyedOutTooltipCallback greyedOutTooltipCallback = () => "";
            //DebugUtils.ShowDebugMessageNotification(DerivedType + ".OnArriveOnLot -- SimDescription: " + Worker.SimDescription + "\nmSocializationDisableCount: " + Worker.mSocializationDisableCount + "\nShowSocialsOnSim: " + Worker.SimDescription.ShowSocialsOnSim + "\nCanSocializeWith: " + SocialInteractionA.Definition.CanSocializeWith("Chat", Sim.ActiveActor, Worker, false, ref greyedOutTooltipCallback));
            //Tutorialette.TriggerLesson(Lessons.Maid, null);
            mDateLastPaid = SimClock.ElapsedCalendarDays();
            mPayHousekeeperAlarm = AlarmManager.AddAlarmRepeating(1, TimeUnit.Weeks, PayHousekeeper, 1, TimeUnit.Weeks, "Housekeeper weekly payment Alarm", AlarmType.AlwaysPersisted, Worker);
        }

        public void PayHousekeeper()
        {
            if (ChargeForServiceWhileActive())
            {
                mDateLastPaid = SimClock.ElapsedCalendarDays();
            }
        }

        public override void SetMotivesAndCommodities()
        {
            CommonUtils.UpdateMotiveTunings(Worker, Housekeeper.ServiceMotive);
            Worker.Motives.MaxEverything();
            Worker.WorkMotive = Housekeeper.ServiceMotive;
            Worker.Motives.CreateMotive(CommodityKind.BeMaid);
            Worker.Motives.CreateMotive(Housekeeper.ServiceMotive);
        }

        public override void SetToFire(Sim serviceSim, Sim firer)
        {
            if (serviceSim == firer)
            {
                EventTracker.SendEvent(EventTypeId.kServiceNPCFired, firer, serviceSim);
                NPCLeavingMessage(LeavingReason.NPCSelfTerminated);
                UnsetServiceBed();
                SetState(new LeaveLot<HousekeeperSituation>(this));
                Service.FireSim(Worker);
            }
            else
            {
                base.SetToFire(serviceSim, firer);
            }
        }

        public override void SetToJobDone()
        {
            base.SetToJobDone();
        }

        public override void SetToLeave()
        {
            base.SetToLeave();
        }
    }
}
