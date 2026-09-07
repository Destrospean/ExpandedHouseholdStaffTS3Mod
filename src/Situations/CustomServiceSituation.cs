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
                DebugUtils.TryDisplayScriptError(() => mAlarmHandle = AlarmManager.AddAlarm(CustomService.DelayBeforeLeaving, TimeUnit.Hours, TimeToRoute, parent.Worker.Service.GetType().Name + " waiting to leave", AlarmType.DeleteOnReset, parent.Worker));
            }

            public override void OnSocializedWith(Sim sim)
            {
                DebugUtils.TryDisplayScriptError(() =>
                    {
                        float timeLeft = AlarmManager.GetTimeLeft(mAlarmHandle, TimeUnit.Hours);
                        if (timeLeft < CustomService.ExtraWaitTimeAfterSocializing)
                        {
                            AlarmManager.UpdateAlarmTime(mAlarmHandle, CustomService.ExtraWaitTimeAfterSocializing - timeLeft, TimeUnit.Hours);
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
                        Parent.Service.FireSim(actor);
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
                            if (Parent.Worker.CurrentInteraction != null && Parent.Worker.CurrentInteraction.GetPriority().Level <= InteractionPriorityLevel.Autonomous)
                            {
                                InteractionInstance interactionInstance = AutonomyUtils.FindBestAction(Parent.Worker.Autonomy);
                                if (interactionInstance != null && Parent.IsInteractionBetterThanCurrent(interactionInstance))
                                {
                                    Parent.Worker.AddExitReason(ExitReason.CanceledByScript);
                                    return false;
                                }
                            }
                            InteractionQueue interactionQueue = Parent.Worker.InteractionQueue;
                            if (interactionQueue != null)
                            {
                                InteractionInstance headInteraction = interactionQueue.GetHeadInteraction();
                                if (headInteraction != null && headInteraction.SatisfiesCommodity(((CustomService)Parent.Worker.Service).ServiceMotive))
                                {
                                    return true;
                                }
                            }
                            return false;
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
                DebugUtils.TryDisplayScriptError(() => mAlarmHandle = parent.Worker.AddAlarmRepeating(CustomService.CheckTime, TimeUnit.Minutes, CheckForDuties, CustomService.CheckTime, TimeUnit.Minutes, "Time for " + parent.Worker.Service.GetType().Name + " to check if everything is done", AlarmType.AlwaysPersisted));
            }

            public override void CleanUp()
            {
                DebugUtils.TryDisplayScriptError(() =>
                    {
                        Parent.Worker.WorkMotive = CommodityKind.None;
                        Parent.Worker.Autonomy.Motives.RemoveMotive(((CustomService)Parent.Worker.Service).ServiceMotive);
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
                DebugUtils.TryDisplayScriptError(() => mAlarmHandle = AlarmManager.AddAlarm(CustomService.DelayBeforeArriving, TimeUnit.Hours, TimeToRoute, parent.Worker.Service.GetType().Name + " waiting to route", AlarmType.DeleteOnReset, parent.Worker));
            }

            public void TimeToRoute()
            {
                DebugUtils.TryDisplayScriptError(() =>
                    {
                        Parent.OnServiceStarting();
                        RouteToLot<CustomServiceSituation, StartPerformingDuties> routeToLot = new WalkToLot<CustomServiceSituation, StartPerformingDuties>(Parent);
                        routeToLot.SetRouteTime(CustomService.DriveTime);
                        Parent.SetState(routeToLot);
                    });
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
                        if (relationship.LTR.Liking < CustomService.RelationshipLevelForQuit)
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

        public override void FreezeMotives()
        {
            Worker.Autonomy.Motives.MaxEverything();
            Worker.Autonomy.Motives.FreezeDecayEverythingExcept(CommodityKind.Energy, CommodityKind.Hygiene);
        }

        public override string GetUniformName(SimDescription simDescription)
        {
            return base.GetUniformName(simDescription);
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
            CommodityKind serviceMotive = ((CustomService)Worker.Service).ServiceMotive;
            CommonUtils.UpdateMotiveTunings(Worker, serviceMotive);
            Worker.Motives.MaxEverything();
            Worker.WorkMotive = serviceMotive;
            Worker.Motives.CreateMotive(serviceMotive);
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
    }
}
