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
        public enum CommodityKindType
        {
            Motive,
            Skill,
            Posture,
            PostureCheck,
            Trait
        }

        public enum DummyEnum
        {
            DummyValue
        }

        public delegate void Action();

        public const string kAuthorName = "zoeoeAndDestrospean";

        [Tunable]
        public static bool kShowDebugMessages = true;

        public static void AddEnumValue<T>(string key, object value) where T : struct
        {
            Type enumType = typeof(T);
            EnumParser caseInsensitiveEnumParser, caseSensitiveEnumParser;
            if (!ParserFunctions.sCaseInsensitiveEnumParsers.TryGetValue(enumType, out caseInsensitiveEnumParser))
            {
                caseInsensitiveEnumParser = new EnumParser(enumType, true);
                ParserFunctions.sCaseInsensitiveEnumParsers.Add(enumType, caseInsensitiveEnumParser);
            }
            if (!ParserFunctions.sCaseSensitiveEnumParsers.TryGetValue(enumType, out caseSensitiveEnumParser))
            {
                caseSensitiveEnumParser = new EnumParser(enumType, false);
                ParserFunctions.sCaseSensitiveEnumParsers.Add(enumType, caseSensitiveEnumParser);
            }
            if (!caseInsensitiveEnumParser.mLookup.ContainsKey(key.ToLowerInvariant()) && !caseSensitiveEnumParser.mLookup.ContainsKey(key))
            {
                caseInsensitiveEnumParser.mLookup.Add(key.ToLowerInvariant(), value);
                caseSensitiveEnumParser.mLookup.Add(key, value);
            }
        }

        public static CommodityKind GetCommodityKind(string name, CommodityKindType type)
        {
            uint retVal = ResourceUtils.HashString32(name);
            switch (type)
            {
                case CommodityKindType.Motive:
                    retVal = retVal & 0xF0FFFFFF | 0x01000000;
                    break;
                case CommodityKindType.Skill:
                    retVal = retVal & 0x0FFFFFFF | 0x20000000;
                    break;
                case CommodityKindType.Posture:
                    retVal = retVal & 0x00FFFFFF | 0x04000000;
                    break;
                case CommodityKindType.PostureCheck:
                    retVal = retVal & 0x00FFFFFF | 0x05000000;
                    break;
                case CommodityKindType.Trait:
                    retVal = retVal & 0x0FFFFFFF | 0x10000000;
                    break;
            }
            return (CommodityKind)retVal;
        }

        public static string GetLocalizationKey(this Type type)
        {
            return type.Namespace.Substring(type.Namespace.IndexOf(kAuthorName)).Replace('.', '/') + "/" + type.Name;
        }

        public static void LoadMotive(string instanceName)
        {
            MotiveTuning.LoadTuning(Simulator.LoadXML(instanceName));
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

        public static void TryDisplayScriptError(Action action)
        {
            Exception exception;
            if (TryGetException(action, out exception))
            {
                ((IScriptErrorWindow)AppDomain.CurrentDomain.GetData("ScriptErrorWindow")).DisplayScriptError(null, exception);
            }
        }

        public static bool TryGetException(Action action, out Exception exception)
        {
            try
            {
                exception = null;
                action();
                return false;
            }
            catch (Exception ex)
            {
                exception = ex;
                return true;
            }
        }
    }
}
