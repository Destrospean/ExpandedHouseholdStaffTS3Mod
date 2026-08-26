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
    public class HousekeeperSituation : LiveInServiceSituation
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
                mAlarmHandle = base.AlarmManager.AddAlarm(Housekeeper.DelayBeforeArriving, TimeUnit.Hours, TimeToRoute, "Housekeeper waiting to route", AlarmType.DeleteOnReset, parent.Worker);
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
                base.AlarmManager.RemoveAlarm(mAlarmHandle);
                base.CleanUp();
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
                Parent.OnArriveOnLot();
                Parent.SetMotivesAndCommodities();
                Parent.SetState(new Cleaning(Parent));
            }
        }

        public class Cleaning : ChildSituation<HousekeeperSituation>
        {
            public AlarmHandle mHandle = AlarmHandle.kInvalidHandle;

            public int mChecksWithNothingLeft;

            public Cleaning()
            {
            }

            public Cleaning(HousekeeperSituation parent) : base(parent)
            {
            }

            public override void Init(HousekeeperSituation parent)
            {
                parent.Worker.Autonomy.Motives.CreateMotive(CommodityKind.BeMaid);
                parent.Worker.Autonomy.Motives.CreateMotive(Housekeeper.BeServiceCommodityKind);
                parent.Worker.WorkMotive = Housekeeper.BeServiceCommodityKind;
                mHandle = parent.Worker.AddAlarmRepeating(Housekeeper.CheckTime, TimeUnit.Minutes, CheckIfEverythingCleaned, Housekeeper.CheckTime, TimeUnit.Minutes, "Time for Housekeeper to check if everything is cleaned", AlarmType.AlwaysPersisted);
            }

            public override void CleanUp()
            {
                Parent.Worker.Autonomy.Motives.RemoveMotive(CommodityKind.BeMaid);
                Parent.Worker.Autonomy.Motives.RemoveMotive(Housekeeper.BeServiceCommodityKind);
                Parent.Worker.WorkMotive = CommodityKind.None;
                base.AlarmManager.RemoveAlarm(mHandle);
                base.CleanUp();
            }

            public void CheckIfEverythingCleaned()
            {
                if (!ShouldHousekeeperContinueCleaning() && !Parent.IsLiveInService && (!Parent.Worker.BuffManager.HasElement(BuffNames.Scared) || Parent.Worker.BuffManager.GetElement(BuffNames.Scared).BuffOrigin != Origin.FromSeeingBonehilda))
                {
                    Parent.SetState(new HangAroundBeforeLeaving(Parent));
                }
            }

            public bool ShouldHousekeeperContinueCleaning()
            {
                Lot lot = Parent.Lot;
                foreach (Sim sim in Lot.GetObjects<Sim>())
                {
                    if (sim.SimDescription.IsBonehilda && sim.RoomId == Parent.Worker.RoomId)
                    {
                        Parent.Worker.BuffManager.AddElement(BuffNames.Scared, Origin.FromSeeingBonehilda);
                        Parent.SetState(new QuitCauseOfBonehilda(Parent));
                        return false;
                    }
                }
                if (lot.FindObjectToClean() != null || lot.HasCleanAllAblePuddlesOrBurntTiles() || lot.HasUnmadeBed())
                {
                    return true;
                }
                InteractionQueue interactionQueue = Parent.Worker.InteractionQueue;
                if (interactionQueue != null)
                {
                    InteractionInstance headInteraction = interactionQueue.GetHeadInteraction();
                    if (headInteraction != null && headInteraction.SatisfiesCommodity(Housekeeper.BeServiceCommodityKind))
                    {
                        return true;
                    }
                }
                return false;
            }

            public bool OkToInterruptCurrentInteraction()
            {
                bool result = true;
                if (Parent.Worker.CurrentInteraction != null)
                {
                    Tradeoff tradeoff = Parent.Worker.CurrentInteraction.InteractionObjectPair.Tradeoff;
                    if (tradeoff != null)
                    {
                        result = !tradeoff.SatisfiesCommodity(CommodityKind.TraitNeat);
                    }
                }
                return result;
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
                Parent.NPCLeavingMessage(LeavingReason.NPCJobDone);
                Parent.SetState(new LeaveLot<HousekeeperSituation>(Parent));
            }

            public override void CleanUp()
            {
                base.AlarmManager.RemoveAlarm(mAlarmHandle);
                base.CleanUp();
            }

            public override void OnSocializedWith(Sim sim)
            {
                float timeLeft = base.AlarmManager.GetTimeLeft(mAlarmHandle, TimeUnit.Hours);
                if (timeLeft < Housekeeper.ExtraWaitTimeAfterSocializing)
                {
                    float timeDelta = Housekeeper.ExtraWaitTimeAfterSocializing - timeLeft;
                    base.AlarmManager.UpdateAlarmTime(mAlarmHandle, timeDelta, TimeUnit.Hours);
                }
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
                Relationship relationship = Relationship.Get(parent.Worker, mFirer, true);
                if (relationship.LTR.Liking <= 20)
                {
                    SituationSocial.Definition i = new SituationSocial.Definition("Insult", new string[0], null, false);
                    ForceSituationSpecificInteraction(mFirer, parent.Worker, i, null, OnFinished, OnFinished);
                }
                else if (relationship.LTR.Liking >= 50 || LTRData.Get(relationship.LTR.CurrentLTR).IsRomantic)
                {
                    SituationSocial.Definition i2 = new SituationSocial.Definition("Cry on Shoulder", new string[0], null, false);
                    ForceSituationSpecificInteraction(mFirer, parent.Worker, i2, null, OnFinished, OnFinished);
                }
                else
                {
                    SituationSocial.Definition i3 = new SituationSocial.Definition("Chat", new string[0], null, false);
                    ForceSituationSpecificInteraction(mFirer, parent.Worker, i3, null, OnFinished, OnFinished);
                }
            }

            public void OnFinished(Sim actor, float x)
            {
                Parent.NPCLeavingMessage(LeavingReason.NPCFired);
                Parent.SetState(new LeaveLot<HousekeeperSituation>(Parent));
                Parent.Service.FireSim(actor);
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
                if (parent.Worker.SimDescription.TeenOrAbove)
                {
                    parent.Worker.InteractionQueue.CancelAllInteractions();
                    RequestWalkStyle(parent.Worker, Sim.WalkStyle.Run);
                    ForceSituationSpecificInteraction(parent.Lot, parent.Worker, new Maid.QuitBecauseOfBonehilda.Definition(), null, null, null);
                    parent.Worker.Service.ClearServiceForLot(parent.Lot);
                    parent.Worker.Service.EndService(parent.Worker.SimDescription);
                    ForceSituationSpecificInteraction(parent.Lot, parent.Worker, new DriveAwayInServiceCar.Definition(parent.Car), null, null, null);
                }
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

        public override float DelayBeforeArriving()
        {
            return Housekeeper.DelayBeforeArriving;
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
                SetState(new LeaveLot<LiveInServiceSituation>(this));
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
