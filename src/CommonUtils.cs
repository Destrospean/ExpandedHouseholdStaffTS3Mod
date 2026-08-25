using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using System;
using System.Collections.Generic;

namespace Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod
{
    public static class CommonUtils
    {
        public const string kAuthorName = "zoeoeAndDestrospean";

        [Tunable]
        public static bool kShowDebugMessages = true;

        public static void AddEnumValue<T>(string key, object value) where T : struct
        {
            Type typeFromHandle = typeof(T);
            EnumParser value0;
            if (!ParserFunctions.sCaseInsensitiveEnumParsers.TryGetValue(typeFromHandle, out value0))
            {
                value0 = new EnumParser(typeFromHandle, true);
                ParserFunctions.sCaseInsensitiveEnumParsers.Add(typeFromHandle, value0);
            }
            EnumParser value1;
            if (!ParserFunctions.sCaseSensitiveEnumParsers.TryGetValue(typeFromHandle, out value1))
            {
                value1 = new EnumParser(typeFromHandle, false);
                ParserFunctions.sCaseSensitiveEnumParsers.Add(typeFromHandle, value1);
            }
            if (!value0.mLookup.ContainsKey(key.ToLowerInvariant()) && !value1.mLookup.ContainsKey(key))
            {
                value0.mLookup.Add(key.ToLowerInvariant(), value);
                value1.mLookup.Add(key, value);
            }
        }

        public static string GetLocalizationKey(this System.Type type)
        {
            return type.Namespace.Substring(type.Namespace.IndexOf(kAuthorName)).Replace('.', '/') + "/" + type.Name;
        }

        public static void LoadMotive(string xmlName)
        {
            MotiveTuning.LoadTuning(Simulator.LoadXML(xmlName));
        }

        public static void ShowDebugMessageDialog(string message)
        {
            if (kShowDebugMessages)
            {
                SimpleMessageDialog.Show("Servants Mod", message);
            }
        }

        public static void UpdateMotiveTunings(Sim sim, CommodityKind commodityKind)
        {
            IEnumerable<MotiveTuning> allTunings = MotiveTuning.GetAllTunings(commodityKind);
            if (allTunings == null)
            {
                return;
            }
            MotiveTuning motiveTuning = null;
            float score = float.MinValue;
            foreach (MotiveTuning tuning in allTunings)
            {
                float tempScore = sim.ScoreMotiveTuning(tuning);
                if (tempScore > score)
                {
                    score = tempScore;
                    motiveTuning = tuning;
                }
            }
            if (motiveTuning != null)
            {
                sim.mMotiveTuning[(int)commodityKind] = motiveTuning;
            }
        }
    }
}
