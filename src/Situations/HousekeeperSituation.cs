using System.Collections.Generic;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Services;
using Sims3.Gameplay.Actors;
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
using Sims3.UI;
using Service = Sims3.Gameplay.Abstracts.zoeoeAndDestrospean.ServantRolesMod.Service;

namespace Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Situations
{
    public class HousekeeperSituation : BabysitterSituationBase
    {
        public int mDateLastPaid;

        public AlarmHandle mPayHousekeeperAlarm = AlarmHandle.kInvalidHandle;

        public override bool RequireBeInSameRoom
        {
            get
            {
                return false;
            }
        }

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

        public HousekeeperSituation(Service service, Lot lot, Sim worker, int cost) : base(service, lot, worker, cost)
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
                SetState(new LeaveLot<BabysitterSituationBase>(this));
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
            if (ii.ScoreIsConsiderablyHigher(Worker.CurrentInteraction.GetPriority().Value) && Worker.CurrentInteraction.Autonomous)
            {
                return true;
            }
            return false;
        }

        public override void FreezeMotives()
        {
            Worker.Autonomy.Motives.MaxEverything();
            Worker.Autonomy.Motives.FreezeDecayEverythingExcept(CommodityKind.Energy, CommodityKind.Hygiene);
        }

        public override void SetMotivesAndCommodities()
        {
            Worker.Motives.MaxEverything();
            Worker.WorkMotive = CommodityKind.BeButler;
            Worker.Motives.CreateMotive(CommodityKind.BeButler);
            Worker.Motives.CreateMotive(CommodityKind.BeMaid);
            Worker.Motives.CreateMotive(CommodityKind.BabysitterClean);
        }
    }
}
