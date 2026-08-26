using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Objects;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Situations;
using Sims3.UI;
using System.Collections.Generic;

namespace Sims3.Gameplay.Abstracts.zoeoeAndDestrospean.ServantRolesMod
{
    public abstract class LiveInServiceSituation : ServiceSituation
    {
        public class WaitToRoute : ChildSituation<LiveInServiceSituation>
        {
            public AlarmHandle mAlarmHandle;

            public WaitToRoute()
            {
            }

            public WaitToRoute(LiveInServiceSituation parent) : base(parent)
            {
            }

            public override void Init(LiveInServiceSituation parent)
            {
                mAlarmHandle = base.AlarmManager.AddAlarm(parent.DelayBeforeArriving(), TimeUnit.Hours, TimeToRoute, "Service waiting to route", AlarmType.DeleteOnReset, parent.Worker);
            }

            public void TimeToRoute()
            {
                RouteToLot<LiveInServiceSituation, DummySituation> routeToLot = new RouteToLot<LiveInServiceSituation, DummySituation>(Parent);
                routeToLot.SetRouteTime(Babysitter.DriveTime);
                Parent.SetState(routeToLot);
            }

            public override void CleanUp()
            {
                base.AlarmManager.RemoveAlarm(mAlarmHandle);
                base.CleanUp();
            }
        }

        public class DummySituation : ChildSituation<LiveInServiceSituation>
        {
            public bool mInformedFireDepartment;

            public DummySituation()
            {
            }

            public DummySituation(LiveInServiceSituation parent) : base(parent)
            {
            }

            public override void Init(LiveInServiceSituation parent)
            {
                Parent.OnArriveOnLot();
                Parent.SetMotivesAndCommodities();
                if (Parent.ReportsFires)
                {
                    parent.mCheckForTasks = Parent.Worker.AddAlarmRepeating(Babysitter.BabysitterCheckForChildTime, TimeUnit.Minutes, CheckForFire, Babysitter.BabysitterCheckForChildTime, TimeUnit.Minutes, "Service: Check for Tasks", AlarmType.DeleteOnReset);
                }
            }

            public override void CleanUp()
            {
                base.CleanUp();
            }

            public void CheckForFire()
            {
                bool isFireOnLot = Parent.Lot.IsFireOnLot();
                if (isFireOnLot && !mInformedFireDepartment)
                {
                    mInformedFireDepartment = true;
                    Firefighter instance = Firefighter.Instance;
                    if (instance != null)
                    {
                        instance.MakeServiceRequest(Lot, true, Parent.Worker.ObjectId);
                        string titleText = Localization.LocalizeString("Gameplay/Services/Babysitter:CalledFireDept");
                        StyledNotification.Format format = new StyledNotification.Format(titleText, Parent.Worker.ObjectId, StyledNotification.NotificationStyle.kSimTalking);
                        StyledNotification.Show(format);
                    }
                }
                else if (!isFireOnLot)
                {
                    mInformedFireDepartment = false;
                }
            }
        }

        public class NPCIsFired : ChildSituation<LiveInServiceSituation>
        {
            public Sim mFirer;

            public NPCIsFired()
            {
            }

            public NPCIsFired(Sim firer, LiveInServiceSituation parent) : base(parent)
            {
                mFirer = firer;
            }

            public override void Init(LiveInServiceSituation parent)
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
                Parent.SetState(new LeaveLot<LiveInServiceSituation>(Parent));
                Parent.Service.FireSim(actor);
            }
        }

        public AlarmHandle mCheckForTasks = AlarmHandle.kInvalidHandle;

        public ulong LastInteractionId;

        public virtual bool IsLiveInService
        {
            get
            {
                return true;
            }
        }

        public virtual bool ReportsFires
        {
            get
            {
                return false;
            }
        }

        public LiveInServiceSituation()
        {
        }

        public LiveInServiceSituation(Service service, Lot lot, Sim worker, int cost) : base(service, lot, worker, cost)
        {
            worker.AssignRole(this);
            FreezeMotives();
            worker.Autonomy.AllowedToRunMetaAutonomy = false;
            SetState(new WaitToRoute(this));
            ScheduleSwitchWorkerToServiceOutfit();
        }

        public LiveInServiceSituation(object dummyObjectForBaseOfBase, Service service, Lot lot, Sim worker, int cost) : base(service, lot, worker, cost)
        {
        }

        public virtual float DelayBeforeArriving()
        {
            return Babysitter.DelayBeforeArriving;
        }

        public virtual void OnArriveOnLot()
        {
        }

        public override string NotEnoughFundsMessage()
        {
            return "Gameplay/Services/Babysitter:NotEnoughFunds";
        }

        public virtual bool ServiceTerminated()
        {
            if (IsLiveInService)
            {
                return false;
            }
            SetToLeave();
            return true;
        }

        public override void SetToLeave()
        {
            NPCLeavingMessage(LeavingReason.NPCDismissed);
            SetState(new LeaveLot<LiveInServiceSituation>(this));
        }

        public virtual void SetToJobDone()
        {
            NPCLeavingMessage(LeavingReason.NPCJobDone);
            SetState(new LeaveLot<LiveInServiceSituation>(this));
        }

        public override void SetToFire(Sim serviceSim, Sim firer)
        {
            base.SetToFire(serviceSim, firer);
            NPCLeavingMessage(LeavingReason.NPCFired);
            SetState(new NPCIsFired(firer, this));
        }

        public virtual void FreezeMotives()
        {
            Worker.Autonomy.Motives.MaxEverything();
            Worker.Autonomy.Motives.FreezeDecayEverythingExcept(CommodityKind.Fun, CommodityKind.Social);
        }

        public virtual void SetMotivesAndCommodities()
        {
            Worker.Motives.MaxEverything();
            Worker.Motives.SetValue(CommodityKind.Fun, 95);
            Worker.Motives.SetValue(CommodityKind.Social, 0);
        }

        public override void EndService()
        {
            RestoreMotives();
            Worker.RemoveAlarm(mCheckForTasks);
            Worker.Service = null;
            mDestroyWorkerOnExit = false;
            Exit();
        }

        public abstract bool IsInteractionBetterThanCurrent(InteractionInstance ii);

        public float GetNewInteractionPriorityValue()
        {
            InteractionPriority interactionPriority = new InteractionPriority(InteractionPriorityLevel.Zero, 0);
            InteractionInstance currentInteraction = Worker.CurrentInteraction;
            if (currentInteraction != null)
            {
                interactionPriority = currentInteraction.GetPriority();
            }
            if (interactionPriority.Level == InteractionPriorityLevel.Zero)
            {
                return 1;
            }
            return interactionPriority.Value + 1;
        }
    }
}

