using Sims3.SimIFace;
using Sims3.UI;

namespace Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod
{
    public static class CommonUtils
    {
        public const string kAuthorName = "zoeoeAndDestrospean";

        [Tunable]
        public static bool kShowDebugMessages = true;

        public static string GetLocalizationKey(this System.Type type)
        {
            return type.Namespace.Substring(type.Namespace.IndexOf(kAuthorName)).Replace('.', '/') + "/" + type.Name;
        }

        public static void ShowDebugMessageDialog(string message)
        {
            if (kShowDebugMessages)
            {
                SimpleMessageDialog.Show("Servants Mod", message);
            }
        }
    }
}
