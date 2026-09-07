using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI.Controller;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;
using zoeoeAndDestrospean.Enums;

namespace zoeoeAndDestrospean.Utils
{
    public static class CommonUtils
    {
        const string kAuthorName = "zoeoeAndDestrospean";

        /// <summary>
        /// Adds actions to an active topic (which this method creates if it doesn't exist).
        /// </summary>
        /// <param name="activeTopic">Active topic.</param>
        /// <param name="grouping">Grouping.</param>
        /// <param name="isActive">If set to <c>true</c> it's an FPA, otherwise it's an SPA.</param>
        /// <param name="newActions">New actions.</param>
        public static void AddActions(string activeTopic, LongTermRelationshipTypes grouping, bool isActive, params string[] newActions)
        {
            Dictionary<LongTermRelationshipTypes, Dictionary<bool, List<string>>> groups;
            if (!ActionAvailabilityData.sActiveTopicInteractions.TryGetValue(activeTopic, out groups))
            {
                groups = new Dictionary<LongTermRelationshipTypes, Dictionary<bool, List<string>>>();
                ActionAvailabilityData.sActiveTopicInteractions.Add(activeTopic, groups);
            }
            Dictionary<bool, List<string>> group;
            if (!groups.TryGetValue(grouping, out group))
            {
                group = new Dictionary<bool, List<string>>();
                groups.Add(grouping, group);
            }
            List<string> actions;
            if (!group.TryGetValue(isActive, out actions))
            {
                actions = new List<string>();
                group.Add(isActive, actions);
            }
            actions.AddRange(newActions);
        }

        /// <summary>
        /// Adds the commodity kind as a commodity change output to an interaction tuning.
        /// </summary>
        /// <param name="commodityKind">Commodity kind.</param>
        /// <param name="constantChange">Advertised value.</param>
        /// <param name="locked">If set to <c>true</c>, locked.</param>
        /// <param name="actualValue">Actual value.</param>
        /// <param name="updateType">Update type.</param>
        /// <param name="timeDependsOnCommodityFilling">If set to <c>true</c> time depends on commodity filling.</param>
        /// <param name="updateEvenOnFailure">If set to <c>true</c> update even on failure.</param>
        /// <param name="updateAboveAndBelowZero">Update above and below zero.</param>
        /// <typeparam name="InteractionDefinition">Interaction definition type.</typeparam>
        /// <typeparam name="Target">Target type.</typeparam>
        public static void AddAsOutput<InteractionDefinition, Target>(this CommodityKind commodityKind, float constantChange, bool locked, float actualValue, OutputUpdateType updateType, bool timeDependsOnCommodityFilling = false, bool updateEvenOnFailure = false, UpdateAboveAndBelowZeroType updateAboveAndBelowZero = UpdateAboveAndBelowZeroType.Either) where InteractionDefinition : Sims3.Gameplay.Interactions.InteractionDefinition where Target : IGameObject
        {
            commodityKind.AddAsOutput(typeof(InteractionDefinition), typeof(Target), constantChange, locked, actualValue, updateType, timeDependsOnCommodityFilling, updateEvenOnFailure, updateAboveAndBelowZero);
        }

        /// <summary>
        /// Adds the commodity kind as a commodity change output to an interaction tuning.
        /// </summary>
        /// <param name="commodityKind">Commodity kind.</param>
        /// <param name="interactionDefinitionType">Interaction definition type.</param>
        /// <param name="targetType">Target type.</param>
        /// <param name="constantChange">Advertised value.</param>
        /// <param name="locked">If set to <c>true</c>, locked.</param>
        /// <param name="actualValue">Actual value.</param>
        /// <param name="updateType">Update type.</param>
        /// <param name="timeDependsOnCommodityFilling">If set to <c>true</c> time depends on commodity filling.</param>
        /// <param name="updateEvenOnFailure">If set to <c>true</c> update even on failure.</param>
        /// <param name="updateAboveAndBelowZero">Update above and below zero.</param>
        public static void AddAsOutput(this CommodityKind commodityKind, Type interactionDefinitionType, Type targetType, float constantChange, bool locked, float actualValue, OutputUpdateType updateType, bool timeDependsOnCommodityFilling = false, bool updateEvenOnFailure = false, UpdateAboveAndBelowZeroType updateAboveAndBelowZero = UpdateAboveAndBelowZeroType.Either)
        {
            List<CommodityChange> outputs = AutonomyTuning.GetTuning(interactionDefinitionType.FullName, targetType).mTradeoff.mOutputs;
            outputs.RemoveAll(x => x.Commodity == commodityKind);
            outputs.Add(new CommodityChange(commodityKind, constantChange, locked, actualValue, updateType, timeDependsOnCommodityFilling, updateEvenOnFailure, updateAboveAndBelowZero));
            RefreshInteractionObjectPairs(interactionDefinitionType, targetType);
        }

        /// <summary>
        /// Adds the commodity kind as a commodity change output to an interaction tuning.
        /// </summary>
        /// <param name="commodityKind">Commodity kind.</param>
        /// <param name="interactionDefinitionTypeFullName">Interaction definition type full name.</param>
        /// <param name="targetTypeFullName">Target type full name.</param>
        /// <param name="constantChange">Advertised value.</param>
        /// <param name="locked">If set to <c>true</c>, locked.</param>
        /// <param name="actualValue">Actual value.</param>
        /// <param name="updateType">Update type.</param>
        /// <param name="timeDependsOnCommodityFilling">If set to <c>true</c> time depends on commodity filling.</param>
        /// <param name="updateEvenOnFailure">If set to <c>true</c> update even on failure.</param>
        /// <param name="updateAboveAndBelowZero">Update above and below zero.</param>
        public static void AddAsOutput(this CommodityKind commodityKind, string interactionDefinitionTypeFullName, string targetTypeFullName, float constantChange, bool locked, float actualValue, OutputUpdateType updateType, bool timeDependsOnCommodityFilling = false, bool updateEvenOnFailure = false, UpdateAboveAndBelowZeroType updateAboveAndBelowZero = UpdateAboveAndBelowZeroType.Either)
        {
            Type interactionDefinitionType, targetType;
            if (TryGetType(interactionDefinitionTypeFullName, out interactionDefinitionType) && TryGetType(targetTypeFullName, out targetType))
            {
                commodityKind.AddAsOutput(interactionDefinitionType, targetType, constantChange, locked, actualValue, updateType, timeDependsOnCommodityFilling, updateEvenOnFailure, updateAboveAndBelowZero);
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

        public static void LoadSocialData(string instanceName)
        {
            XmlDocument xmlDocument = Simulator.LoadXML(instanceName);
            bool isEp5Installed = GameUtils.IsInstalled(ProductVersion.EP5);
            if (instanceName != null)
            {
                foreach (XmlElement element in new XmlElementLookup(xmlDocument)["Action"])
                {
                    CommodityTypes commodityTypes;
                    ParserFunctions.TryParseEnum<CommodityTypes>(element.GetAttribute("com"), out commodityTypes, CommodityTypes.Undefined);
                    ActionData data = new ActionData(element.GetAttribute("key"), commodityTypes, ProductVersion.BaseGame, new XmlElementLookup(element), isEp5Installed);
                    ActionData.Add(data);
                }
            }
        }

        public static void LoadSocializingActionAvailability(string instanceName)
        {
            XmlDbData data = XmlDbData.ReadData(instanceName);
            if (data != null)
            {
                DebugUtils.TryDisplayScriptError(() =>
                    {
                        if (data.Tables.ContainsKey("SAA"))
                        {
                            SocialManager.ParseStcActionAvailability(data);
                        }
                        if (data.Tables.ContainsKey("TAA"))
                        {
                            SocialManager.ParseActiveTopic(data);
                        }
                    });
            }
        }

        /// <summary>
        /// Refreshes the interaction object pairs of a specified interaction definition for all existing objects of a specified type in the world.
        /// </summary>
        /// <typeparam name="InteractionDefinition">Interaction definition type.</typeparam>
        /// <typeparam name="Target">Target type.</typeparam>
        public static void RefreshInteractionObjectPairs<InteractionDefinition, Target>() where InteractionDefinition : Sims3.Gameplay.Interactions.InteractionDefinition where Target : IGameObject
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
        /// Replaces a method (and its overloads) with another method.
        /// (Note: Only the overloads defined in the new type will replace the corresponding overloads of the old type.)
        /// </summary>
        /// <typeparam name="OldType">The class that holds the method to be replaced.</typeparam>
        /// <typeparam name="NewType">The class that holds the new method. (Note: Only the overloads defined in the new type will replace the corresponding overloads of the old type.)</typeparam>
        public static void ReplaceMethod<OldType, NewType>(string methodName)
        {
            ReplaceMethod<OldType, NewType>(methodName, methodName);
        }

        /// <summary>
        /// This method was borrowed from Lazy Duchess' Mono Patcher.
        /// </summary>
        public static void ReplaceMethod(MethodInfo oldMethod, MethodInfo newMethod)
        {
            byte[] replacementByteArray = new byte[40];
            Marshal.Copy(newMethod.MethodHandle.Value, replacementByteArray, 0, 40);
            Marshal.Copy(replacementByteArray, 0, oldMethod.MethodHandle.Value, 24);
            Marshal.Copy(replacementByteArray, 28, new IntPtr(oldMethod.MethodHandle.Value.ToInt32() + 28), 12);
        }

        /// <summary>
        /// Replaces a method (and its overloads) with another method.
        /// (Note: Only the overloads defined in the new type will replace the corresponding overloads of the old type.)
        /// </summary>
        /// <param name="oldMethodName">The name of the method to be replaced.</param>
        /// <param name="newMethodName">The name of the new method.</param>
        /// <typeparam name="OldType">The class that holds the method to be replaced.</typeparam>
        /// <typeparam name="NewType">The class that holds the new method. (Note: Only the overloads defined in the new type will replace the corresponding overloads of the old type.)</typeparam>
        public static void ReplaceMethod<OldType, NewType>(string oldMethodName, string newMethodName)
        {
            foreach (MethodInfo method in typeof(NewType).GetMethods((BindingFlags)0x3C))
            {
                if (method.Name == newMethodName)
                {
                    MethodInfo oldMethod = typeof(OldType).GetMethod(oldMethodName, (BindingFlags)0x3C, null, Array.ConvertAll(method.GetParameters(), x => x.ParameterType), null);
                    if (oldMethod != null)
                    {
                        CommonUtils.ReplaceMethod(oldMethod, method);
                    }
                }
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
