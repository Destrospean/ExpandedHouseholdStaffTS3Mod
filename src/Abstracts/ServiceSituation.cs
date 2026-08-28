using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Objects;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Situations;
using Sims3.UI;
using System;
using System.Collections.Generic;

namespace Sims3.Gameplay.Abstracts.zoeoeAndDestrospean.ServantRolesMod
{
    public abstract class ServiceSituation<T> : ServiceSituation where T : ServiceSituation<T>
    {
        public class DummySituation : ChildSituation<ServiceSituation<T>>
        {
            public DummySituation()
            {
            }

            public DummySituation(ServiceSituation<T> parent) : base(parent)
            {
            }

            public override void Init(ServiceSituation<T> parent)
            {
                parent.OnArriveOnLot();
                parent.SetMotivesAndCommodities();
            }
        }

        public class NPCIsFired : ChildSituation<ServiceSituation<T>>
        {
            public Sim mFirer;

            public NPCIsFired()
            {
            }

            public NPCIsFired(Sim firer, ServiceSituation<T> parent) : base(parent)
            {
                mFirer = firer;
            }

            public override void Init(ServiceSituation<T> parent)
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
            }

            public void OnFinished(Sim actor, float x)
            {
                Parent.SetState(new LeaveLot<ServiceSituation<T>>(Parent));
                Parent.Service.FireSim(actor);
            }
        }

        public class WaitToRoute : ChildSituation<ServiceSituation<T>>
        {
            public AlarmHandle mAlarmHandle;

            public WaitToRoute()
            {
            }

            public WaitToRoute(ServiceSituation<T> parent) : base(parent)
            {
            }

            public override void Init(ServiceSituation<T> parent)
            {
                mAlarmHandle = base.AlarmManager.AddAlarm(parent.DelayBeforeArriving, TimeUnit.Hours, TimeToRoute, "Service waiting to route", AlarmType.DeleteOnReset, parent.Worker);
            }

            public void TimeToRoute()
            {
                RouteToLot<ServiceSituation<T>, DummySituation> routeToLot = new RouteToLot<ServiceSituation<T>, DummySituation>(Parent);
                routeToLot.SetRouteTime(Babysitter.DriveTime);
                Parent.SetState(routeToLot);
            }

            public override void CleanUp()
            {
                base.AlarmManager.RemoveAlarm(mAlarmHandle);
                base.CleanUp();
            }
        }

        public AlarmHandle mCheckForFireAlarmHandle = AlarmHandle.kInvalidHandle;

        public bool mInformedFireDepartment;

        public ulong LastInteractionId;

        public virtual float DelayBeforeArriving
        {
            get
            {
                return Babysitter.DelayBeforeArriving;
            }
        }

        public static Type DerivedType
        {
            get
            {
                return typeof(T);
            }
        }

        public virtual bool IsLiveInService
        {
            get
            {
                return false;
            }
        }

        public virtual bool ReportsFires
        {
            get
            {
                return false;
            }
        }

        public virtual bool ServiceTerminated
        {
            get
            {
                if (IsLiveInService)
                {
                    return false;
                }
                SetToLeave();
                return true;
            }
        }

        public ServiceSituation()
        {
        }

        public ServiceSituation(Service service, Lot lot, Sim worker, int cost) : base(service, lot, worker, cost)
        {
            worker.AssignRole(this);
            worker.Autonomy.AllowedToRunMetaAutonomy = false;
            FreezeMotives();
            SetState((Situation)Activator.CreateInstance(DerivedType.GetNestedType("WaitToRoute"), this));
            ScheduleSwitchWorkerToServiceOutfit();
            if (ReportsFires)
            {
                AddCheckForFireAlarm();
            }
        }

        public ServiceSituation(CommonUtils.DummyEnum dummyArgForBaseOfBase, Service service, Lot lot, Sim worker, int cost) : base(service, lot, worker, cost)
        {
        }

        public void AddCheckForFireAlarm()
        {
            mCheckForFireAlarmHandle = Worker.AddAlarmRepeating(Babysitter.BabysitterCheckForChildTime, TimeUnit.Minutes, CheckForFire, Babysitter.BabysitterCheckForChildTime, TimeUnit.Minutes, "Service: Check for Fire", AlarmType.DeleteOnReset);
        }

        public void CheckForFire()
        {
            if (ServiceTerminated)
            {
                return;
            }
            bool isFireOnLot = Lot.IsFireOnLot();
            if (isFireOnLot && !mInformedFireDepartment)
            {
                mInformedFireDepartment = true;
                Firefighter firefighter = Firefighter.Instance;
                if (firefighter != null)
                {
                    firefighter.MakeServiceRequest(Lot, true, Worker.ObjectId);
                    StyledNotification.Show(new StyledNotification.Format(Localization.LocalizeString("Gameplay/Services/Babysitter:CalledFireDept"), Worker.ObjectId, StyledNotification.NotificationStyle.kSimTalking));
                }
                return;
            }
            if (!isFireOnLot)
            {
                mInformedFireDepartment = false;
            }
        }

        public override void EndService()
        {
            if (ReportsFires)
            {
                Worker.RemoveAlarm(mCheckForFireAlarmHandle);
            }
            RestoreMotives();
            Worker.Service = null;
            mDestroyWorkerOnExit = false;
            Exit();
        }

        public virtual void FreezeMotives()
        {
            Worker.Autonomy.Motives.MaxEverything();
            Worker.Autonomy.Motives.FreezeDecayEverythingExcept(CommodityKind.Fun, CommodityKind.Social);
        }

        public float GetNewInteractionPriorityValue()
        {
            InteractionPriority interactionPriority = new InteractionPriority(InteractionPriorityLevel.Zero, 0);
            if (Worker.CurrentInteraction != null)
            {
                interactionPriority = Worker.CurrentInteraction.GetPriority();
            }
            if (interactionPriority.Level == InteractionPriorityLevel.Zero)
            {
                return 1;
            }
            return interactionPriority.Value + 1;
        }

        public abstract bool IsInteractionBetterThanCurrent(InteractionInstance ii);

        public override string NotEnoughFundsMessage()
        {
            return Service.GetType().GetLocalizationKey() + ":NotEnoughFunds";
        }

        public virtual void OnArriveOnLot()
        {
        }

        public virtual void SetMotivesAndCommodities()
        {
            Worker.Motives.MaxEverything();
            Worker.Motives.SetValue(CommodityKind.Fun, 95);
            Worker.Motives.SetValue(CommodityKind.Social, 0);
        }

        public override void SetToFire(Sim serviceSim, Sim firer)
        {
            base.SetToFire(serviceSim, firer);
            NPCLeavingMessage(LeavingReason.NPCFired);
            SetState(new NPCIsFired(firer, this));
        }

        public virtual void SetToJobDone()
        {
            NPCLeavingMessage(LeavingReason.NPCJobDone);
            SetState(new LeaveLot<ServiceSituation<T>>(this));
        }

        public override void SetToLeave()
        {
            NPCLeavingMessage(LeavingReason.NPCDismissed);
            SetState(new LeaveLot<ServiceSituation<T>>(this));
        }
    }
}
