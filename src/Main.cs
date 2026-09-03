using Sims3.Gameplay.Objects.Electronics;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Interactions;
using Sims3.SimIFace;

namespace zoeoeAndDestrospean.ServantRolesMod
{
    public class Main
    {
        [Tunable]
        internal static bool kInstantiator = false;

        static Main()
        {
            LoadSaveManager.ObjectGroupsPreLoad += () => Phone.CallForServices.Singleton = CallForServices.Singleton;
        }
    }
}
