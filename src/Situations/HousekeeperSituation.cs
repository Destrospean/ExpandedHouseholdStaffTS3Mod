using Sims3.Gameplay.Abstracts.zoeoeAndDestrospean.ServantRolesMod;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Objects;
using Sims3.Gameplay.Objects.FoodObjects;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Tutorial;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Services;
using Sims3.UI;
using System;
using System.Collections.Generic;

namespace Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Situations
{
    public class HousekeeperSituation : ServiceSituation<HousekeeperSituation>
    {
        public new class WaitToRoute : ChildSituation<HousekeeperSituation>
        {
            public AlarmHandle mAlarmHandle;

            public WaitToRoute()
            {
            }

            public WaitToRoute(HousekeeperSituation parent) : base(parent)
            {
            }

            public override void Init(HousekeeperSituation parent)
            {
                CommonUtils.TryDisplayScriptError(() => mAlarmHandle = base.AlarmManager.AddAlarm(Housekeeper.DelayBeforeArriving, TimeUnit.Hours, TimeToRoute, "Housekeeper waiting to route", AlarmType.DeleteOnReset, parent.Worker));
            }

            public void TimeToRoute()
            {
                CommonUtils.TryDisplayScriptError(() =>
                    {
                        Parent.OnServiceStarting();
                        RouteToLot<HousekeeperSituation, StartCleaning> routeToLot = new WalkToLot<HousekeeperSituation, StartCleaning>(Parent);
                        routeToLot.SetRouteTime(Housekeeper.DriveTime);
                        Parent.SetState(routeToLot);
                    });
            }

            public override void CleanUp()
            {
                CommonUtils.TryDisplayScriptError(() =>
                    {
                        base.AlarmManager.RemoveAlarm(mAlarmHandle);
                        base.CleanUp();
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
                CommonUtils.TryDisplayScriptError(() =>
                    {
                        Parent.OnArriveOnLot();
                        Parent.SetMotivesAndCommodities();
                        Parent.SetState(new Cleaning(Parent));
                    });
            }
        }

        public class Cleaning : ChildSituation<HousekeeperSituation>
        {
            public AlarmHandle mAlarmHandle = AlarmHandle.kInvalidHandle;

            public Cleaning()
            {
            }

            public Cleaning(HousekeeperSituation parent) : base(parent)
            {
            }

            public override void Init(HousekeeperSituation parent)
            {
                CommonUtils.TryDisplayScriptError(() => mAlarmHandle = parent.Worker.AddAlarmRepeating(Housekeeper.CheckTime, TimeUnit.Minutes, CheckIfEverythingDone, Housekeeper.CheckTime, TimeUnit.Minutes, "Time for Housekeeper to check if everything is cleaned", AlarmType.AlwaysPersisted));
            }

            public override void CleanUp()
            {
                CommonUtils.TryDisplayScriptError(() =>
                    {
                        Parent.Worker.Autonomy.Motives.RemoveMotive(CommodityKind.BeMaid);
                        Parent.Worker.Autonomy.Motives.RemoveMotive(Housekeeper.BeServiceCommodityKind);
                        Parent.Worker.WorkMotive = CommodityKind.None;
                        base.AlarmManager.RemoveAlarm(mAlarmHandle);
                        base.CleanUp();
                    });
            }

            public void CheckIfEverythingDone()
            {
                CommonUtils.TryDisplayScriptError(() =>
                    {
                        if (!CheckIfHasDuties() && !Parent.IsLiveInService && (!Parent.Worker.BuffManager.HasElement(BuffNames.Scared) || Parent.Worker.BuffManager.GetElement(BuffNames.Scared).BuffOrigin != Origin.FromSeeingBonehilda))
                        {
                            Parent.SetState(new HangAroundBeforeLeaving(Parent));
                        }
                    });
            }

            public bool CheckIfHasDuties()
            {
                bool retVal = false;
                CommonUtils.TryDisplayScriptError(() =>
                    {
                        foreach (Sim sim in Lot.GetObjects<Sim>())
                        {
                            if (sim.SimDescription.IsBonehilda && sim.RoomId == Parent.Worker.RoomId)
                            {
                                Parent.Worker.BuffManager.AddElement(BuffNames.Scared, Origin.FromSeeingBonehilda);
                                Parent.SetState(new QuitCauseOfBonehilda(Parent));
                                return;
                            }
                        }
                        if (Parent.Worker.CurrentInteraction != null && Parent.Worker.CurrentInteraction.GetPriority().Level <= InteractionPriorityLevel.Autonomous)
                        {
                            InteractionInstance interactionInstance = Parent.Worker.Autonomy.FindBestAction();
                            if (interactionInstance != null && Parent.IsInteractionBetterThanCurrent(interactionInstance))
                            {
                                Parent.Worker.AddExitReason(ExitReason.CanceledByScript);
                                return;
                            }
                        }
                        if (Lot.FindObjectToClean() != null || Lot.HasCleanAllAblePuddlesOrBurntTiles() || Lot.HasUnmadeBed())
                        {
                            retVal = true;
                            return;
                        }
                        InteractionQueue interactionQueue = Parent.Worker.InteractionQueue;
                        if (interactionQueue != null)
                        {
                            InteractionInstance headInteraction = interactionQueue.GetHeadInteraction();
                            if (headInteraction != null && headInteraction.SatisfiesCommodity(Housekeeper.BeServiceCommodityKind))
                            {
                                retVal = true;
                                return;
                            }
                        }
                    });
                return retVal;
            }
        }

        public class HangAroundBeforeLeaving : ChildSituation<HousekeeperSituation>
        {
            public AlarmHandle mAlarmHandle;

            public HangAroundBeforeLeaving()
            {
            }

            public HangAroundBeforeLeaving(HousekeeperSituation parent) : base(parent)
            {
            }

            public override void Init(HousekeeperSituation parent)
            {
                CommonUtils.TryDisplayScriptError(() => mAlarmHandle = base.AlarmManager.AddAlarm(Housekeeper.DelayBeforeLeaving, TimeUnit.Hours, TimeToRoute, "Housekeeper waiting to leave", AlarmType.DeleteOnReset, parent.Worker));
            }

            public void TimeToRoute()
            {
                CommonUtils.TryDisplayScriptError(() =>
                    {
                        Parent.NPCLeavingMessage(LeavingReason.NPCJobDone);
                        Parent.SetState(new LeaveLot<HousekeeperSituation>(Parent));
                    });
            }

            public override void CleanUp()
            {
                CommonUtils.TryDisplayScriptError(() =>
                    {
                        base.AlarmManager.RemoveAlarm(mAlarmHandle);
                        base.CleanUp();
                    });
            }

            public override void OnSocializedWith(Sim sim)
            {
                CommonUtils.TryDisplayScriptError(() =>
                    {
                        float timeLeft = base.AlarmManager.GetTimeLeft(mAlarmHandle, TimeUnit.Hours);
                        if (timeLeft < Housekeeper.ExtraWaitTimeAfterSocializing)
                        {
                            base.AlarmManager.UpdateAlarmTime(mAlarmHandle, Housekeeper.ExtraWaitTimeAfterSocializing - timeLeft, TimeUnit.Hours);
                        }
                    });
            }
        }

        public new class NPCIsFired : ChildSituation<HousekeeperSituation>
        {
            public Sim mFirer;

            public NPCIsFired()
            {
            }

            public NPCIsFired(Sim firer, HousekeeperSituation parent) : base(parent)
            {
                mFirer = firer;
            }

            public override void Init(HousekeeperSituation parent)
            {
                CommonUtils.TryDisplayScriptError(() =>
                    {
                        Relationship relationship = Relationship.Get(parent.Worker, mFirer, true);
                        if (relationship.LTR.Liking <= 20)
                        {
                            ForceSituationSpecificInteraction(mFirer, parent.Worker, new SituationSocial.Definition("Insult", new string[0], null, false), null, OnFinished, OnFinished);
                        }
                        else if (relationship.LTR.Liking >= 50 || LTRData.Get(relationship.LTR.CurrentLTR).IsRomantic)
                        {
                            ForceSituationSpecificInteraction(mFirer, parent.Worker, new SituationSocial.Definition("Cry on Shoulder", new string[0], null, false), null, OnFinished, OnFinished);
                        }
                        else
                        {
                            ForceSituationSpecificInteraction(mFirer, parent.Worker, new SituationSocial.Definition("Chat", new string[0], null, false), null, OnFinished, OnFinished);
                        }
                    });
            }

            public void OnFinished(Sim actor, float x)
            {
                CommonUtils.TryDisplayScriptError(() =>
                    {
                        Parent.NPCLeavingMessage(LeavingReason.NPCFired);
                        Parent.SetState(new LeaveLot<HousekeeperSituation>(Parent));
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
                CommonUtils.TryDisplayScriptError(() =>
                    {
                        parent.Worker.InteractionQueue.CancelAllInteractions();
                        RequestWalkStyle(parent.Worker, Sim.WalkStyle.Run);
                        ForceSituationSpecificInteraction(parent.Lot, parent.Worker, new Maid.QuitBecauseOfBonehilda.Definition(), null, null, null);
                        parent.Worker.Service.ClearServiceForLot(parent.Lot);
                        parent.Worker.Service.EndService(parent.Worker.SimDescription);
                        ForceSituationSpecificInteraction(parent.Lot, parent.Worker, new DriveAwayInServiceCar.Definition(parent.Car), null, null, null);
                    });
            }
        }

        public int mDateLastPaid;

        public AlarmHandle mPayHousekeeperAlarm = AlarmHandle.kInvalidHandle;

        public override bool IsLiveInService
        {
            get
            {
                return true;
            }
        }

        public HousekeeperSituation()
        {
        }

        public HousekeeperSituation(Service<Housekeeper> service, Lot lot, Sim worker, int cost) : base(service, lot, worker, cost)
        {
            mDateLastPaid = SimClock.ElapsedCalendarDays();
        }

        public override string NotEnoughFundsMessage()
        {
            return typeof(Housekeeper).GetLocalizationKey() + ":NotEnoughFunds";
        }

        public void UnsetHousekeeperBed()
        {
            IBed bed = Worker.Bed as IBed;
            if (bed != null && bed.LotCurrent == Lot)
            {
                bed.RelinquishOwnership(Worker, null);
            }
        }

        public void PayHousekeeper()
        {
            if (ChargeForServiceWhileActive())
            {
                mDateLastPaid = SimClock.ElapsedCalendarDays();
            }
        }

        public int NumDaysSinceLastPayment()
        {
            int numDaysSinceLastPayment = SimClock.ElapsedCalendarDays() - mDateLastPaid;
            if (numDaysSinceLastPayment <= 0)
            {
                return 1;
            }
            return numDaysSinceLastPayment;
        }

        public override void OnArriveOnLot()
        {
            Tutorialette.TriggerLesson(Lessons.Butler, null);
            mDateLastPaid = SimClock.ElapsedCalendarDays();
            mPayHousekeeperAlarm = base.AlarmManager.AddAlarmRepeating(1, TimeUnit.Weeks, PayHousekeeper, 1, TimeUnit.Weeks, "Housekeeper weekly payment Alarm", AlarmType.AlwaysPersisted, Worker);
        }

        public override int CostTotal()
        {
            return Cost * NumDaysSinceLastPayment() / 7;
        }

        public bool ChargeForServiceWhileActive()
        {
            int totalCost = CostTotal();
            if (totalCost > 0)
            {
                if (Lot.Household.FamilyFunds < totalCost)
                {
                    SetToFire(Worker, Worker);
                    return false;
                }
                Lot.Household.ModifyFamilyFunds(-totalCost);
                StyledNotification.Show(new StyledNotification.Format(Localization.LocalizeString(typeof(Housekeeper).GetLocalizationKey().Replace(typeof(Housekeeper).Name, "WeeklyPayment:" + typeof(Housekeeper).Name), totalCost), Worker.ObjectId, StyledNotification.NotificationStyle.kSimTalking));
            }
            return true;
        }

        public override void SetToLeave()
        {
            UnsetHousekeeperBed();
            base.SetToLeave();
        }

        public override void SetToJobDone()
        {
            UnsetHousekeeperBed();
            base.SetToJobDone();
        }

        public override void SetToFire(Sim serviceSim, Sim firer)
        {
            UnsetHousekeeperBed();
            if (serviceSim == firer)
            {
                EventTracker.SendEvent(EventTypeId.kServiceNPCFired, firer, serviceSim);
                NPCLeavingMessage(LeavingReason.NPCSelfTerminated);
                SetState(new LeaveLot<HousekeeperSituation>(this));
                Service.FireSim(Worker);
            }
            else
            {
                base.SetToFire(serviceSim, firer);
            }
        }

        public override void EndService()
        {
            Worker.RemoveAlarm(mPayHousekeeperAlarm);
            base.EndService();
        }

        public override bool ServiceTerminated()
        {
            Household household = Lot.Household;
            if (household == null)
            {
                return true;
            }
            foreach (Sim sim in household.Sims)
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

        public override bool IsInteractionBetterThanCurrent(InteractionInstance ii)
        {
            return ii.ScoreIsConsiderablyHigher(Worker.CurrentInteraction.GetPriority().Value) && Worker.CurrentInteraction.Autonomous;
        }

        public override void FreezeMotives()
        {
            Worker.Autonomy.Motives.MaxEverything();
            Worker.Autonomy.Motives.FreezeDecayEverythingExcept(CommodityKind.Energy, CommodityKind.Hygiene);
        }

        public override void SetMotivesAndCommodities()
        {
            CommonUtils.UpdateMotiveTunings(Worker, Housekeeper.BeServiceCommodityKind);
            Worker.Motives.MaxEverything();
            Worker.WorkMotive = Housekeeper.BeServiceCommodityKind;
            Worker.Motives.CreateMotive(CommodityKind.BeMaid);
            Worker.Motives.CreateMotive(Housekeeper.BeServiceCommodityKind);
        }
    }
}
