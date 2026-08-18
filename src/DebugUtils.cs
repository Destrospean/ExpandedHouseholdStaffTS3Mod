using Sims3.SimIFace;
using Sims3.UI;

namespace Sims3.Gameplay.zoeoe.ServantRolesMod
{
    public static class DebugUtils
    {
        [Tunable]
        public static bool kShowDebugMessages = true;

        public static void ShowDebugMessageDialog(string message)
        {
            if (kShowDebugMessages)
            {
                SimpleMessageDialog.Show("Servants Mod", message);
            }
        }
    }
}
