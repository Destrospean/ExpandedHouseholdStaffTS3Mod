using Sims3.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sims3.Gameplay.zoeoe.ServantRolesMod
{
    public static class Utils
    {
        public static bool kShowDebugMsgs = true;
        public static void ShowDebugMessageDialog(string message)
        {
            if (kShowDebugMsgs)
            {
                SimpleMessageDialog.Show("Servants Mod", message);
            }
        }
    }
}
