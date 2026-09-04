using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using System;
using System.Collections.Generic;
using System.Reflection;

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

        public delegate T Func<T>();

        const string kAuthorName = "zoeoeAndDestrospean";

        [Tunable]
        public static bool kShowDebugMessages = true;

        /// <summary>
        /// Adds the commodity kind as a commodity change output to an interaction tuning.
        /// </summary>
        /// <param name="commodityKind">Commodity kind.</param>
        /// <param name="advertised">Advertised value.</param>
        /// <param name="locked">If set to <c>true</c>, locked.</param>
        /// <param name="actual">Actual value.</param>
        /// <param name="updateType">Update type.</param>
        /// <param name="timeDependsOn">If set to <c>true</c> time depends on commodity filling.</param>
        /// <param name="updateEvenOnFailure">If set to <c>true</c> update even on failure.</param>
        /// <param name="updateAboveAndBelowZero">Update above and below zero.</param>
        /// <typeparam name="InteractionDefinition">Interaction definition type.</typeparam>
        /// <typeparam name="Target">Target type.</typeparam>
        public static void AddAsOutput<InteractionDefinition, Target>(this CommodityKind commodityKind, float advertised, bool locked, float actual, OutputUpdateType updateType, bool timeDependsOn = false, bool updateEvenOnFailure = false, UpdateAboveAndBelowZeroType updateAboveAndBelowZero = UpdateAboveAndBelowZeroType.Either) where InteractionDefinition : Gameplay.Interactions.InteractionDefinition where Target : IGameObject
        {
            commodityKind.AddAsOutput(typeof(InteractionDefinition), typeof(Target), advertised, locked, actual, updateType, timeDependsOn, updateEvenOnFailure, updateAboveAndBelowZero);
        }

        /// <summary>
        /// Adds the commodity kind as a commodity change output to an interaction tuning.
        /// </summary>
        /// <param name="commodityKind">Commodity kind.</param>
        /// <param name="interactionDefinitionType">Interaction definition type.</param>
        /// <param name="targetType">Target type.</param>
        /// <param name="advertised">Advertised value.</param>
        /// <param name="locked">If set to <c>true</c>, locked.</param>
        /// <param name="actual">Actual value.</param>
        /// <param name="updateType">Update type.</param>
        /// <param name="timeDependsOn">If set to <c>true</c> time depends on commodity filling.</param>
        /// <param name="updateEvenOnFailure">If set to <c>true</c> update even on failure.</param>
        /// <param name="updateAboveAndBelowZero">Update above and below zero.</param>
        public static void AddAsOutput(this CommodityKind commodityKind, Type interactionDefinitionType, Type targetType, float advertised, bool locked, float actual, OutputUpdateType updateType, bool timeDependsOn = false, bool updateEvenOnFailure = false, UpdateAboveAndBelowZeroType updateAboveAndBelowZero = UpdateAboveAndBelowZeroType.Either)
        {
            List<CommodityChange> outputs = AutonomyTuning.GetTuning(interactionDefinitionType.FullName, targetType).mTradeoff.mOutputs;
            outputs.RemoveAll(x => x.Commodity == commodityKind);
            outputs.Add(new CommodityChange(commodityKind, advertised, locked, actual, updateType, timeDependsOn, updateEvenOnFailure, updateAboveAndBelowZero));
            RefreshInteractionObjectPairs(interactionDefinitionType, targetType);
        }

        /// <summary>
        /// Adds the commodity kind as a commodity change output to an interaction tuning.
        /// </summary>
        /// <param name="commodityKind">Commodity kind.</param>
        /// <param name="interactionDefinitionTypeFullName">Interaction definition type full name.</param>
        /// <param name="targetTypeFullName">Target type full name.</param>
        /// <param name="advertised">Advertised value.</param>
        /// <param name="locked">If set to <c>true</c>, locked.</param>
        /// <param name="actual">Actual value.</param>
        /// <param name="updateType">Update type.</param>
        /// <param name="timeDependsOn">If set to <c>true</c> time depends on commodity filling.</param>
        /// <param name="updateEvenOnFailure">If set to <c>true</c> update even on failure.</param>
        /// <param name="updateAboveAndBelowZero">Update above and below zero.</param>
        public static void AddAsOutput(this CommodityKind commodityKind, string interactionDefinitionTypeFullName, string targetTypeFullName, float advertised, bool locked, float actual, OutputUpdateType updateType, bool timeDependsOn = false, bool updateEvenOnFailure = false, UpdateAboveAndBelowZeroType updateAboveAndBelowZero = UpdateAboveAndBelowZeroType.Either)
        {
            Type interactionDefinitionType, targetType;
            if (TryGetType(interactionDefinitionTypeFullName, out interactionDefinitionType) && TryGetType(targetTypeFullName, out targetType))
            {
                commodityKind.AddAsOutput(interactionDefinitionType, targetType, advertised, locked, actual, updateType, timeDependsOn, updateEvenOnFailure, updateAboveAndBelowZero);
            }
        }

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

        /// <summary>
        /// Gets a valid commodity kind value from a string.
        /// </summary>
        /// <returns>The commodity kind.</returns>
        /// <param name="name">The name of the commodity kind.</param>
        /// <param name="type">The type of commodity.</param>
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

        /// <summary>
        /// Gets the localization key from the class name and namespace of the object type.
        /// </summary>
        /// <returns>The localization key.</returns>
        /// <param name="type">Type.</param>
        public static string GetLocalizationKey(this Type type)
        {
            return type.Namespace.Substring(type.Namespace.IndexOf(kAuthorName)).Replace('.', '/') + "/" + type.Name;
        }

        public static void LoadMotive(string instanceName)
        {
            MotiveTuning.LoadTuning(Simulator.LoadXML(instanceName));
        }

        /// <summary>
        /// Refreshes the interaction object pairs of a specified interaction definition for all existing objects of a specified type in the world.
        /// </summary>
        /// <typeparam name="InteractionDefinition">Interaction definition type.</typeparam>
        /// <typeparam name="Target">Target type.</typeparam>
        public static void RefreshInteractionObjectPairs<InteractionDefinition, Target>() where InteractionDefinition : Gameplay.Interactions.InteractionDefinition where Target : IGameObject
        {
            RefreshInteractionObjectPairs(typeof(InteractionDefinition), typeof(Target));
        }

        /// <summary>
        /// Refreshes the interaction object pairs of a specified interaction definition for all existing objects of a specified type in the world.
        /// </summary>
        /// <param name="interactionDefinitionType">Interaction definition type.</param>
        /// <param name="targetType">Target type.</param>
        public static void RefreshInteractionObjectPairs(Type interactionDefinitionType, Type targetType)
        {
            foreach (GameObject gameObject in Sims3.Gameplay.Queries.GetObjects<GameObject>())
            {
                if (targetType.IsAssignableFrom(gameObject.GetType()))
                {
                    foreach (InteractionObjectPair interactionObjectPair in new List<InteractionObjectPair>(gameObject.Interactions))
                    {
                        if (interactionDefinitionType.IsAssignableFrom(interactionObjectPair.mInteraction.GetType()) && targetType.IsAssignableFrom(interactionObjectPair.mTargetType))
                        {
                            gameObject.RemoveInteraction(interactionObjectPair);
                            gameObject.AddInteraction(new InteractionObjectPair(interactionObjectPair.InteractionDefinition, gameObject, AutonomyTuning.GetTuning(interactionDefinitionType.FullName, targetType)));
                            if (gameObject.ItemComp != null && gameObject.ItemComp.InteractionsInventory.Contains(interactionObjectPair))
                            {
                                gameObject.ItemComp.InteractionsInventory.Remove(interactionObjectPair);
                                gameObject.AddInventoryInteraction(interactionObjectPair.InteractionDefinition);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Refreshes the interaction object pairs of a specified interaction definition for all existing objects of a specified type in the world.
        /// </summary>
        /// <param name="interactionDefinitionTypeFullName">Interaction definition type full name.</param>
        /// <param name="targetTypeFullName">Target type full name.</param>
        public static void RefreshInteractionObjectPairs(string interactionDefinitionTypeFullName, string targetTypeFullName)
        {
            Type interactionDefinitionType, targetType;
            if (TryGetType(interactionDefinitionTypeFullName, out interactionDefinitionType) && TryGetType(targetTypeFullName, out targetType))
            {
                RefreshInteractionObjectPairs(interactionDefinitionType, targetType);
            }
        }

        /// <summary>
        /// Shows a debug message dialog (only when kShowDebugMessages is set to <c>true</c>)
        /// </summary>
        public static void ShowDebugMessageDialog(string message)
        {
            if (kShowDebugMessages)
            {
                SimpleMessageDialog.Show("Servants Mod", message);
            }
        }

        /// <summary>
        /// Shows a debug message notification (only when kShowDebugMessages is set to <c>true</c>)
        /// </summary>
        public static void ShowDebugMessageNotification(string message)
        {
            StyledNotification.Format format = new StyledNotification.Format(message, StyledNotification.NotificationStyle.kSimTalking);
            if (GameUtils.IsInstalled(ProductVersion.EP9))
            {
                StyledNotification.Show(format, "w_smart_phone", null, ProductVersion.EP9, ProductVersion.EP9);
            }
            else
            {
                StyledNotification.Show(format, "glb_tns_phone_r2");
            }
        }

        /// <summary>
        /// Displays a script error in a the script error window if one is found.
        /// </summary>
        /// <returns><c>true</c>, if a script error was found, <c>false</c> otherwise.</returns>
        /// <param name="action">Function to execute and check for an error.</param>
        public static bool TryDisplayScriptError(Action action)
        {
            Exception exception;
            if (TryGetException(action, out exception))
            {
                ((IScriptErrorWindow)AppDomain.CurrentDomain.GetData("ScriptErrorWindow")).DisplayScriptError(null, exception);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Displays a script error in a the script error window if one is found.
        /// </summary>
        /// <returns><c>true</c>, if a script error was found, <c>false</c> otherwise.</returns>
        /// <param name="callback">Function to execute and check for an error.</param>
        /// <param name="value">Return value of the callback function.</param>
        /// <typeparam name="T">Return value type.</typeparam>
        public static bool TryDisplayScriptError<T>(Func<T> callback, out T value)
        {
            T retVal = default(T);
            bool errorDisplayed = TryDisplayScriptError(() => retVal = callback());
            value = retVal;
            return errorDisplayed;
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

        public static bool TryGetType(string fullName, out Type type)
        {
            type = null;
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type tempType in assembly.GetTypes())
                {
                    if (tempType.FullName == fullName)
                    {
                        type = tempType;
                        return true;
                    }
                }
            }
            return false;
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
