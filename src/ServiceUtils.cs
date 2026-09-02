using Sims3.Gameplay.Abstracts.zoeoeAndDestrospean.ServantRolesMod;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using System;
using System.Collections.Generic;

namespace Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod
{
    public static class ServiceUtils
    {
        internal static Dictionary<Type, Service> Instances = new Dictionary<Type, Service>();

        internal static List<Type> PreloadedServices = new List<Type>();

        internal static Dictionary<Type, CommodityKind> ServiceMotives = new Dictionary<Type, CommodityKind>();

        public static string GetServiceTitle(this Service service)
        {
            return Localization.LocalizeString(service.GetType().GetLocalizationKey() + ":Title");
        }

        public static bool IsFromServantRolesMod<T>() where T : Service
        {
            return IsFromServantRolesMod(typeof(T));
        }

        public static bool IsFromServantRolesMod(this Service service)
        {
            return IsFromServantRolesMod(service.GetType());
        }

        public static bool IsFromServantRolesMod(Type type)
        {
            return typeof(Service<>).IsAssignableFrom(type);
        }

        public static void RequestService(this Sim simRequestingService, Service service)
        {
            service.MakeServiceRequest(simRequestingService.LotCurrent, true, simRequestingService.ObjectId);
            StyledNotification.Format format = new StyledNotification.Format(Localization.LocalizeString(service.GetType().GetLocalizationKey() + ":ServiceRequested"), StyledNotification.NotificationStyle.kSimTalking);
            if (Responder.Instance.ServicesModel.DoesSimHaveFuturePhone(simRequestingService.ObjectId))
            {
                StyledNotification.Show(format, "w_future_phone", null, ProductVersion.EP11, ProductVersion.EP11);
            }
            else if (GameUtils.IsInstalled(ProductVersion.EP9))
            {
                StyledNotification.Show(format, "w_smart_phone", null, ProductVersion.EP9, ProductVersion.EP9);
            }
            else
            {
                StyledNotification.Show(format, "glb_tns_phone_r2");
            }
        }
    }
}
