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
using Sims3.Gameplay.CAS;
using Sims3.SimIFace.CAS;
using Sims3.SimIFace;

namespace Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Situations
{
    public class ChefSituation : ServiceSituation<ChefSituation>
    {
        public class HangAroundBeforeLeaving : ChildSituation<ChefSituation>
        {
            AlarmHandle mAlarmHandle;

            public HangAroundBeforeLeaving()
            {
            }

            public HangAroundBeforeLeaving(ChefSituation parent) : base(parent)
            {
            }

            public override void CleanUp()
            {
                CommonUtils.TryDisplayScriptError(() =>
                    {
                        AlarmManager.RemoveAlarm(mAlarmHandle);
                        base.CleanUp();
                    });
            }

            public override void Init(ChefSituation parent)
            {
                CommonUtils.TryDisplayScriptError(() => mAlarmHandle = AlarmManager.AddAlarm(Chef.DelayBeforeLeaving, TimeUnit.Hours, TimeToRoute, "Chef waiting to leave", AlarmType.DeleteOnReset, parent.Worker));
            }

            public override void OnSocializedWith(Sim sim)
            {
                CommonUtils.TryDisplayScriptError(() =>
                    {
                        float timeLeft = AlarmManager.GetTimeLeft(mAlarmHandle, TimeUnit.Hours);
                        if (timeLeft < Chef.ExtraWaitTimeAfterSocializing)
                        {
                            AlarmManager.UpdateAlarmTime(mAlarmHandle, Chef.ExtraWaitTimeAfterSocializing - timeLeft, TimeUnit.Hours);
                        }
                    });
            }

            public void TimeToRoute()
            {
                CommonUtils.TryDisplayScriptError(() =>
                    {
                        Parent.NPCLeavingMessage(LeavingReason.NPCJobDone);
                        Parent.SetState(new LeaveLot<ChefSituation>(Parent));
                    });
            }
        }

        public new class NPCIsFired : ServiceSituation<ChefSituation>.NPCIsFired
        {
            public NPCIsFired()
            {
            }

            public NPCIsFired(Sim firer, ChefSituation parent) : base(firer, parent)
            {
            }

            public override void OnFinished(Sim actor, float x)
            {
                CommonUtils.TryDisplayScriptError(() =>
                    {
                        Parent.NPCLeavingMessage(LeavingReason.NPCFired);
                        Parent.SetState(new LeaveLot<ServiceSituation<ChefSituation>>(Parent));
                        Parent.Service.FireSim(actor);
                    });
            }
        }

        public class StartWaitingToCook : ChildSituation<ChefSituation>
        {
            public StartWaitingToCook()
            {
            }

            public StartWaitingToCook(ChefSituation parent) : base(parent)
            {
            }

            public override void Init(ChefSituation parent)
            {
                CommonUtils.TryDisplayScriptError(() =>
                    {
                        parent.OnArriveOnLot();
                        parent.SetMotivesAndCommodities();
                        parent.SetState(new WaitToCook(parent));
                    });
            }
        }

        public class WaitToCook : ChildSituation<ChefSituation>
        {
            AlarmHandle mAlarmHandle = AlarmHandle.kInvalidHandle;

            public bool HasDuties
            {
                get
                {
                    bool retVal;
                    return !CommonUtils.TryDisplayScriptError(() =>
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
                                if (headInteraction != null && headInteraction.SatisfiesCommodity(Chef.ServiceMotive))
                                {
                                    return true;
                                }
                            }
                            return false;
                        }, out retVal) && retVal;
                }
            }

            public WaitToCook()
            {
            }

            public WaitToCook(ChefSituation parent) : base(parent)
            {
            }

            public override void Init(ChefSituation parent)
            {
                CommonUtils.TryDisplayScriptError(() => mAlarmHandle = parent.Worker.AddAlarmRepeating(Chef.CheckTime, TimeUnit.Minutes, CheckForDuties, Chef.CheckTime, TimeUnit.Minutes, "Time for Chef to check if everything is cleaned", AlarmType.AlwaysPersisted));
            }

            public override void CleanUp()
            {
                CommonUtils.TryDisplayScriptError(() =>
                    {
                        Parent.Worker.WorkMotive = CommodityKind.None;
                        Parent.Worker.Autonomy.Motives.RemoveMotive(Chef.ServiceMotive);
                        AlarmManager.RemoveAlarm(mAlarmHandle);
                        base.CleanUp();
                    });
            }

            public void CheckForDuties()
            {
                CommonUtils.TryDisplayScriptError(() =>
                    {
                        if (!Parent.ServiceTerminated && !HasDuties && !Parent.IsLiveInService && (!Parent.Worker.BuffManager.HasElement(BuffNames.Scared) || Parent.Worker.BuffManager.GetElement(BuffNames.Scared).BuffOrigin != Origin.FromSeeingBonehilda))
                        {
                            Parent.SetState(new HangAroundBeforeLeaving(Parent));
                        }
                    });
            }
        }

        public new class WaitToRoute : ChildSituation<ChefSituation>
        {
            AlarmHandle mAlarmHandle;

            public WaitToRoute()
            {
            }

            public WaitToRoute(ChefSituation parent) : base(parent)
            {
            }

            public override void CleanUp()
            {
                CommonUtils.TryDisplayScriptError(() =>
                    {
                        AlarmManager.RemoveAlarm(mAlarmHandle);
                        base.CleanUp();
                    });
            }

            public override void Init(ChefSituation parent)
            {
                CommonUtils.TryDisplayScriptError(() => mAlarmHandle = AlarmManager.AddAlarm(Chef.DelayBeforeArriving, TimeUnit.Hours, TimeToRoute, "Chef waiting to route", AlarmType.DeleteOnReset, parent.Worker));
            }

            public void TimeToRoute()
            {
                CommonUtils.TryDisplayScriptError(() =>
                    {
                        Parent.OnServiceStarting();
                        RouteToLot<ChefSituation, StartWaitingToCook> routeToLot = new WalkToLot<ChefSituation, StartWaitingToCook>(Parent);
                        routeToLot.SetRouteTime(Chef.DriveTime);
                        Parent.SetState(routeToLot);
                    });
            }
        }

        int mDateLastPaid;

        AlarmHandle mPayChefAlarm = AlarmHandle.kInvalidHandle;

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
                        if (relationship.LTR.Liking < Chef.RelationshipLevelForQuit)
                        {
                            SetToFire(Worker, Worker);
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public ChefSituation()
        {
        }

        public ChefSituation(Service<Chef> service, Lot lot, Sim worker, int cost) : base(service, lot, worker, cost)
        {
            mDateLastPaid = SimClock.ElapsedCalendarDays();
        }

        public override int CostTotal()
        {
            return Cost * DayCountSinceLastPayment / 7;
        }

        public override void EndService()
        {
            Worker.RemoveAlarm(mPayChefAlarm);
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
            //Tutorialette.TriggerLesson(Lessons.Butler, null);
            mDateLastPaid = SimClock.ElapsedCalendarDays();
            mPayChefAlarm = AlarmManager.AddAlarmRepeating(1, TimeUnit.Weeks, PayChef, 1, TimeUnit.Weeks, "Chef weekly payment Alarm", AlarmType.AlwaysPersisted, Worker);
        }

        public void PayChef()
        {
            if (ChargeForServiceWhileActive())
            {
                mDateLastPaid = SimClock.ElapsedCalendarDays();
            }
        }

        public override void SetMotivesAndCommodities()
        {
            CommonUtils.UpdateMotiveTunings(Worker, Chef.ServiceMotive);
            Worker.Motives.MaxEverything();
            Worker.WorkMotive = Chef.ServiceMotive;
            Worker.Motives.CreateMotive(Chef.ServiceMotive);
        }

        public override void SetToFire(Sim serviceSim, Sim firer)
        {
            if (serviceSim == firer)
            {
                EventTracker.SendEvent(EventTypeId.kServiceNPCFired, firer, serviceSim);
                NPCLeavingMessage(LeavingReason.NPCSelfTerminated);
                UnsetServiceBed();
                SetState(new LeaveLot<ChefSituation>(this));
                Service.FireSim(Worker);
            }
            else
            {
                base.SetToFire(serviceSim, firer);
            }
        }

        public override void SwitchWorkerToServiceOutfit()
        {
            SimDescription simDescription = Worker.SimDescription;
            SimOutfit uniform = new SimOutfit(ResourceKey.CreateOutfitKeyFromProductVersion("career_execchef_" + (simDescription.IsFemale ? "female" : "male") + (simDescription.Elder ? "elder" : ""), ProductVersion.BaseGame));
            SimOutfit resultOutfit;
            if (OutfitUtils.TryApplyUniformToOutfit(simDescription.GetOutfit(OutfitCategories.Everyday, 0), uniform, simDescription, DerivedType.Name + ".SwitchWorkerToServiceOutfit", out resultOutfit))
            {
                simDescription.AddOutfit(resultOutfit, OutfitCategories.Career, true);
                for (int i = 1; i < simDescription.GetOutfitCount(OutfitCategories.Career); i++)
                {
                    simDescription.RemoveOutfit(OutfitCategories.Career, i, true);
                }
            }
            base.SwitchWorkerToServiceOutfit();
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
