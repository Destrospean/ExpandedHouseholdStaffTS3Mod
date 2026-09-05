using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Interfaces.zoeoeAndDestrospean.ServantRolesMod;
using Sims3.Gameplay.Services;
using System;
using System.Collections.Generic;

namespace zoeoeAndDestrospean.Utils.ServantRolesMod
{
    public static class ServiceUtils
    {
        public static readonly Dictionary<Type, Service> Instances = new Dictionary<Type, Service>();

        public static readonly List<Type> PreloadedTypes = new List<Type>();

        public static readonly Dictionary<Type, CommodityKind> ServiceMotives = new Dictionary<Type, CommodityKind>();

        public static bool IsFromServantRolesMod<Service>() where Service : Sims3.Gameplay.Services.Service
        {
            return IsFromServantRolesMod(typeof(Service));
        }

        public static bool IsFromServantRolesMod(this Service service)
        {
            return IsFromServantRolesMod(service.GetType());
        }

        public static bool IsFromServantRolesMod(Type type)
        {
            return typeof(IService).IsAssignableFrom(type);
        }
    }
}
