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
    public class HousekeeperSituation : ServiceSituationBase
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
                Exception exception;
                CommonUtils.TryGetException(() => mAlarmHandle = base.AlarmManager.AddAlarm(Housekeeper.DelayBeforeArriving, TimeUnit.Hours, TimeToRoute, "Housekeeper waiting to route", AlarmType.DeleteOnReset, parent.Worker), out exception);
            }

            public void TimeToRoute()
            {
                Exception exception;
                CommonUtils.TryGetException(() =>
                    {
                        Parent.OnServiceStarting();
                        RouteToLot<HousekeeperSituation, StartCleaning> routeToLot = new WalkToLot<HousekeeperSituation, StartCleaning>(Parent);
                        routeToLot.SetRouteTime(Housekeeper.DriveTime);
                        Parent.SetState(routeToLot);
                    }, out exception);
            }

            public override void CleanUp()
            {
                Exception exception;
                CommonUtils.TryGetException(() =>
                    {
                        base.AlarmManager.RemoveAlarm(mAlarmHandle);
                        base.CleanUp();
                    }, out exception);
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
                Exception exception;
                CommonUtils.TryGetException(() =>
                    {
                        Parent.OnArriveOnLot();
                        Parent.SetMotivesAndCommodities();
                        Parent.SetState(new Cleaning(Parent));
                    }, out exception);
            }
        }

        public class Cleaning : ChildSituation<HousekeeperSituation>
        {
            public AlarmHandle mAlarmHandle = AlarmHandle.kInvalidHandle;

            public int mChecksWithNothingLeft;

            public Cleaning()
            {
            }

            public Cleaning(HousekeeperSituation parent) : base(parent)
            {
            }

            public override void Init(HousekeeperSituation parent)
            {
                Exception exception;
                CommonUtils.TryGetException(() =>
                    {
                        parent.Worker.Autonomy.Motives.CreateMotive(CommodityKind.BeMaid);
                        parent.Worker.Autonomy.Motives.CreateMotive(Housekeeper.BeServiceCommodityKind);
                        parent.Worker.WorkMotive = Housekeeper.BeServiceCommodityKind;
                        mAlarmHandle = parent.Worker.AddAlarmRepeating(Housekeeper.CheckTime, TimeUnit.Minutes, CheckIfEverythingCleaned, Housekeeper.CheckTime, TimeUnit.Minutes, "Time for Housekeeper to check if everything is cleaned", AlarmType.AlwaysPersisted);
                    }, out exception);
            }

            public override void CleanUp()
            {
                Exception exception;
                CommonUtils.TryGetException(() =>
                    {
                        Parent.Worker.Autonomy.Motives.RemoveMotive(CommodityKind.BeMaid);
                        Parent.Worker.Autonomy.Motives.RemoveMotive(Housekeeper.BeServiceCommodityKind);
                        Parent.Worker.WorkMotive = CommodityKind.None;
                        base.AlarmManager.RemoveAlarm(mAlarmHandle);
                        base.CleanUp();
                    }, out exception);
            }

            public void CheckIfEverythingCleaned()
            {
                Exception exception;
                CommonUtils.TryGetException(() =>
                    {
                        if (!CheckIfTasksToDo() && !Parent.IsLiveInService && (!Parent.Worker.BuffManager.HasElement(BuffNames.Scared) || Parent.Worker.BuffManager.GetElement(BuffNames.Scared).BuffOrigin != Origin.FromSeeingBonehilda))
                        {
                            Parent.SetState(new HangAroundBeforeLeaving(Parent));
                        }
                    }, out exception);
            }

            public bool CheckIfTasksToDo()
            {
                bool retVal = false;
                Exception exception;
                CommonUtils.TryGetException(() =>
                    {
                        foreach (Sim sim in Lot.GetObjects<Sim>())
                        {
                            if (sim.SimDescription.IsBonehilda && sim.RoomId == Parent.Worker.RoomId)
                            {
                                Parent.Worker.BuffManager.AddElement(BuffNames.Scared, Origin.FromSeeingBonehilda);
                                Parent.SetState(new QuitCauseOfBonehilda(Parent));
                                retVal = false;
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
                        if (Parent.Worker.CurrentInteraction != null)
                        {
                            if (Parent.Worker.CurrentInteraction.GetPriority().Level <= InteractionPriorityLevel.Autonomous)
                            {
                                InteractionInstance interactionInstance = Parent.Worker.Autonomy.FindBestAction();
                                if (interactionInstance != null && Parent.IsInteractionBetterThanCurrent(interactionInstance))
                                {
                                    Parent.Worker.AddExitReason(ExitReason.CanceledByScript);
                                    retVal = false;
                                    return;
                                }
                            }
                        }
                        if (!retVal && Parent.Worker.CurrentInteraction != null)
                        {
                            if (Parent.Worker.CurrentInteraction.Id == Parent.LastInteractionId)
                            {
                                Parent.Worker.AddExitReason(ExitReason.Finished);
                            }
                            Parent.LastInteractionId = Parent.Worker.CurrentInteraction.Id;
                        }
                        retVal = false;
                        return;
                    }, out exception);
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
                mAlarmHandle = base.AlarmManager.AddAlarm(Housekeeper.DelayBeforeLeaving, TimeUnit.Hours, TimeToRoute, "Housekeeper waiting to leave", AlarmType.DeleteOnReset, parent.Worker);
            }

            public void TimeToRoute()
            {
                Exception exception;
                CommonUtils.TryGetException(() =>
                    {
                        Parent.NPCLeavingMessage(LeavingReason.NPCJobDone);
                        Parent.SetState(new LeaveLot<HousekeeperSituation>(Parent));
                    }, out exception);
            }

            public override void CleanUp()
            {
                Exception exception;
                CommonUtils.TryGetException(() =>
                    {
                        base.AlarmManager.RemoveAlarm(mAlarmHandle);
                        base.CleanUp();
                    }, out exception);
            }

            public override void OnSocializedWith(Sim sim)
            {
                Exception exception;
                CommonUtils.TryGetException(() =>
                    {
                        float timeLeft = base.AlarmManager.GetTimeLeft(mAlarmHandle, TimeUnit.Hours);
                        if (timeLeft < Housekeeper.ExtraWaitTimeAfterSocializing)
                        {
                            base.AlarmManager.UpdateAlarmTime(mAlarmHandle, Housekeeper.ExtraWaitTimeAfterSocializing - timeLeft, TimeUnit.Hours);
                        }
                    }, out exception);
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
                Exception exception;
                CommonUtils.TryGetException(() =>
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
                    }, out exception);
            }

            public void OnFinished(Sim actor, float x)
            {
                Exception exception;
                CommonUtils.TryGetException(() =>
                    {
                        Parent.NPCLeavingMessage(LeavingReason.NPCFired);
                        Parent.SetState(new LeaveLot<HousekeeperSituation>(Parent));
                        Parent.Service.FireSim(actor);
                    }, out exception);
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
                Exception exception;
                CommonUtils.TryGetException(() =>
                    {
                        parent.Worker.InteractionQueue.CancelAllInteractions();
                        RequestWalkStyle(parent.Worker, Sim.WalkStyle.Run);
                        ForceSituationSpecificInteraction(parent.Lot, parent.Worker, new Maid.QuitBecauseOfBonehilda.Definition(), null, null, null);
                        parent.Worker.Service.ClearServiceForLot(parent.Lot);
                        parent.Worker.Service.EndService(parent.Worker.SimDescription);
                        ForceSituationSpecificInteraction(parent.Lot, parent.Worker, new DriveAwayInServiceCar.Definition(parent.Car), null, null, null);
                    }, out exception);
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

        public HousekeeperSituation(Service<Housekeeper> service, Lot lot, Sim worker, int cost) : base(null, service, lot, worker, cost)
        {
            worker.AssignRole(this);
            worker.Autonomy.Motives.MaxEverything();
            worker.Autonomy.Motives.FreezeDecayEverythingExcept();
            worker.Autonomy.AllowedToRunMetaAutonomy = false;
            SetState(new WaitToRoute(this));
            ScheduleSwitchWorkerToServiceOutfit();
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
                SetState(new LeaveLot<ServiceSituationBase>(this));
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
            Worker.Motives.MaxEverything();
            Worker.WorkMotive = Housekeeper.BeServiceCommodityKind;
            Worker.Motives.CreateMotive(CommodityKind.BeMaid);
            Worker.Motives.CreateMotive(Housekeeper.BeServiceCommodityKind);
        }
    }
}
