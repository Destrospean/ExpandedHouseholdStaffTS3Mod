using Sims3.Gameplay.Abstracts.zoeoeAndDestrospean.ServantRolesMod;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Services;
using System;
using System.Collections.Generic;

namespace Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod
{
    public static class ServiceUtils
    {
        internal static Dictionary<Type, Service> Instances = new Dictionary<Type, Service>();

        internal static List<Type> PreloadedServices = new List<Type>();

        internal static Dictionary<Type, CommodityKind> ServiceMotives = new Dictionary<Type, CommodityKind>();

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
    }
}
