using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces.zoeoeAndDestrospean.ServantRolesMod;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Moving;
using Sims3.Gameplay.Objects.Electronics;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Tutorial;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using System;
using System.Collections.Generic;

namespace Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Interactions
{
    public class CallForServices : Phone.Call
    {
        public class ServicesModel : UI.ServicesModel, IServicesModel
        {
            class ServiceInfoEx : ServiceInfo
            {
                public Service Service;
            }

            List<ServiceInfo> IServicesModel.GetServices()
            {
                List<ServiceInfo> serviceInfoList = new List<ServiceInfo>();
                List<Service> services = new List<Service>
                    {
                        Babysitter.Instance,
                        Maid.Instance,
                        PizzaDelivery.Instance,
                        Repairman.Instance,
                        SocialWorkerAdoption.Instance,
                        Police.Instance,
                        Firefighter.Instance,
                        sNewspaperDeliveryService,
                        Butler.Instance,
                        BartenderService.Instance,
                        Magician.Instance,
                        DJ.Instance,
                        Singer.Instance,
                        PerformanceArtist.Instance
                    };
                services.AddRange(ServiceUtils.Instances.Values);
                foreach (Service service in services)
                {
                    if (service != null && service.CanRequestServiceFromPhone(Lot))
                    {
                        serviceInfoList.Add(GetServiceInfo(this, service));
                    }
                }
                return serviceInfoList;
            }

            bool IServicesModel.MakeServiceRequest(ServiceInfo info)
            {
                if (info as ServiceInfoEx == null && info.mServiceType == 100)
                {
                    mLot.Household.AutoBabysitter = info.mActive;
                    return true;
                }
                Service service = GetServiceFromInfo(this, info);
                if (service != null && (service.IsServiceRequested(mLot) || service.IsAnySimAssignedToLot(mLot)))
                {
                    foreach (Sim sim in service.GetSimsAssignedToLot(mLot))
                    {
                        ServiceSituation.FindServiceSituationInvolving(sim)?.SetToLeave();
                    }
                }
                if (service != null && info.mActive != service.IsServiceRequested(mLot))
                {
                    service.MakeServiceRequest(mLot, info.mActive, SimGuid);
                    SimGuid = ObjectGuid.InvalidObjectGuid;
                    return true;
                }
                return false;
            }

            public static Service GetServiceFromInfo(UI.ServicesModel servicesModel, ServiceInfo info)
            {
                ServiceInfoEx serviceInfo = info as ServiceInfoEx;
                return serviceInfo == null ? servicesModel.GetServiceFromInfo(info) :  serviceInfo.Service;
            }

            public static ServiceInfo GetServiceInfo(UI.ServicesModel servicesModel, Service service)
            {
                if (!service.IsFromServantRolesMod())
                {
                    return servicesModel.GetServiceInfo(service);
                }
                string entryKey = service.GetType().GetLocalizationKey();
                ServiceInfoEx serviceInfo = new ServiceInfoEx();
                serviceInfo.mName = Localization.LocalizeString(entryKey + ":Title");
                serviceInfo.mAlreadyActiveToolTip = Localization.LocalizeString("Gameplay/UI/ServicesUIWindow:AlreadyActive");
                if (service.IsRecurrent())
                {
                    serviceInfo.mAlreadyActiveToolTip = Localization.LocalizeString("Gameplay/UI/ServicesUIWindow:RecurrentAlreadyActive");
                    serviceInfo.mCancelledTns = Localization.LocalizeString(entryKey + ":ServiceCancelled" + (service as IAmLiveInService == null && service.IsAnySimAssignedToLot(servicesModel.Lot) ? "WhileActive" : ""));
                }
                serviceInfo.mRequestedTns = Localization.LocalizeString(entryKey + ":ServiceRequested");
                serviceInfo.mServiceType = (int)service.ServiceType;
                serviceInfo.mActive = service.IsServiceRequested(servicesModel.Lot) || service.IsAnySimAssignedToLot(servicesModel.Lot);
                serviceInfo.mPrice = service.IsEmergencyService ? 0 : service.Cost();
                serviceInfo.mRecurring = service.IsRecurrent();
                serviceInfo.mIsPaidWeekly = service.IsPaidWeekly;
                serviceInfo.mActiveButMissingRequirementToolTip = string.Empty;
                serviceInfo.Service = service;
                return serviceInfo;
            }

            public static new bool Show(Lot lot, ObjectGuid simGuid)
            {
                UI.Responder.Instance.mServicesModel = new ServicesModel();
                ServicesModel servicesModel = (ServicesModel)UI.Responder.Instance.ServicesModel;
                servicesModel.Lot = lot;
                servicesModel.SimGuid = simGuid;
                return ServicesController.Show(simGuid);
            }
        }

        public static InteractionDefinition Singleton = new Definition();

        public class Definition : CallDefinition<CallForServices>
        {
            public override string GetInteractionName(ref InteractionInstanceParameters parameters)
            {
                return LocalizeString("InteractionName");
            }

            public override string[] GetPath(bool isFemale)
            {
                return new string[]
                {
                    Phone.LocalizeString("Services") + Localization.Ellipsis
                };
            }

            public override bool Test(Sim actor, Phone target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                if (!target.IsUsableBy(actor))
                {
                    return false;
                }
                if (!actor.HouseholdOwnsResidentialLot(actor.LotCurrent))
                {
                    greyedOutTooltipCallback = () => LocalizeString("ServicesOnlyOnHomeLot");
                    return false;
                }
                return base.Test(actor, target, isAutonomous, ref greyedOutTooltipCallback);
            }
        }

        public static string LocalizeString(string name, params object[] parameters)
        {
            return Localization.LocalizeString("Gameplay/Objects/Electronics/Phone/CallForServices:" + name, parameters);
        }

        public override ConversationBehavior OnCallConnected()
        {
            if (!UIUtils.IsOkayToStartModalDialog())
            {
                return ConversationBehavior.JustHangUp;
            }
            if (Actor.LotHome == null)
            {
                return ConversationBehavior.ShakeHead;
            }
            if (Actor.Household.IsActive && !MovingSituation.MovingInProgress)
            {
                Tutorialette.TriggerLesson(Lessons.HiringServices, Actor);
                if (ServicesModel.Show(Actor.LotCurrent, Actor.ObjectId))
                {
                    return ConversationBehavior.TalkBriefly;
                }
            }
            return ConversationBehavior.JustHangUp;
        }
    }
}
