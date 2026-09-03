using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Moving;
using Sims3.Gameplay.Objects.Electronics;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Tutorial;
using Sims3.Gameplay.UI;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Services;
using Sims3.SimIFace;
using Sims3.UI;
using System;
using System.Collections.Generic;
using System.Text;
using static Sims3.Gameplay.Objects.Electronics.Phone;

namespace Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.CustomInteractions
{
    public class CallForServicesCustom : Phone.Call
    {
        private static string LocalizeString(string name, params object[] parameters)
        {
            return Localization.LocalizeString("Gameplay/Objects/Electronics/Phone/CallForServices:" + name, parameters);
        }
        public static readonly InteractionDefinition Singleton = new Definition();

        private sealed class Definition : CallDefinition<CallForServicesCustom>
        {
		    public override bool Test(Sim a, Phone target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                if (!target.IsUsableBy(a))
                {
                    return false;
                }
                if (!a.HouseholdOwnsResidentialLot(a.LotCurrent))
                {
                    greyedOutTooltipCallback = (() => LocalizeString("ServicesOnlyOnHomeLot"));
                    return false;
                }
                return base.Test(a, target, isAutonomous, ref greyedOutTooltipCallback);
            }

            public override string[] GetPath(bool isFemale)
            {
                return new string[1]
                {
                Phone.LocalizeString("Services") + Localization.Ellipsis
                };
            }
            public override string GetInteractionName(ref InteractionInstanceParameters parameters)
            {
                return LocalizeString("InteractionName");
            }
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
                if (ServicesModelCustom.Show(Actor.LotCurrent, Actor.ObjectId))
                {
                    return ConversationBehavior.TalkBriefly;
                }
            }
            return ConversationBehavior.JustHangUp;
        }
    }

    public class ServicesModelCustom : ServicesModel, IServicesModel
    {
        public static new bool Show(Lot lot, ObjectGuid simGuid)
        {
            UI.Responder.Instance.mServicesModel = new ServicesModelCustom();
            ServicesModelCustom servicesModel = UI.Responder.Instance.ServicesModel as ServicesModelCustom;
            servicesModel.Lot = lot;
            servicesModel.SimGuid = simGuid;
            return ServicesController.Show(simGuid);
        }

        List<ServiceInfo> IServicesModel.GetServices()
        {
            List<ServiceInfo> list = new List<ServiceInfo>();
            Service[] array = new Service[16]
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
            PerformanceArtist.Instance,
            Housekeeper.Instance,
            Chef.Instance
            };
            Service[] array2 = array;
            foreach (Service service in array2)
            {
                if (service != null && service.CanRequestServiceFromPhone(Lot))
                {
                    ServiceInfo serviceInfo = GetServiceInfo(service);
                    list.Add(serviceInfo);
                }
            }
            return list;
        }
    }
}
