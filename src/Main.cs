using Sims3.Gameplay.Objects.Electronics;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod
{
    public class Main
    {
        [Tunable]
        public static bool kServantsModMain = false;
        static Main()
        {
            World.sOnStartupAppEventHandler += OnStartupApp;
        }

        private static void OnStartupApp(object sender, EventArgs e)
        {
            Phone.CallForServices.Singleton = CustomInteractions.CallForServicesCustom.Singleton;
        }
    }
}
