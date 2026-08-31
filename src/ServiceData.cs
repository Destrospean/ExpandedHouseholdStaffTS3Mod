using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Services;
using System;
using System.Collections.Generic;

namespace Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod
{
    public static class ServiceData
    {
        public static Dictionary<Type, Service> Instances = new Dictionary<Type, Service>();

        public static Dictionary<Type, CommodityKind> ServiceMotives = new Dictionary<Type, CommodityKind>();
    }
}
