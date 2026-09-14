using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Destrospean.Utils
{
    public class InteractionObjectTypeUtils
    {
        static Type[] sGameObjectTypes;

        static Type[] sInteractionDefinitionTypes;

        public static Type[] GameObjectTypes
        {
            get
            {
                if (sGameObjectTypes == null)
                {
                    InitTypes();
                }
                return sGameObjectTypes;
            }
        }

        public static Type[] InteractionDefinitionTypes
        {
            get
            {
                if (sInteractionDefinitionTypes == null)
                {
                    InitTypes();
                }
                return sInteractionDefinitionTypes;
            }
        }

        public static void InitTypes()
        {
            List<Type> gameObjectTypes = new List<Type>();
            List<Type> interactionDefinitionTypes = new List<Type>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types = assembly.GetTypes();
                gameObjectTypes.AddRange(Array.FindAll(types, x => typeof(IGameObject).IsAssignableFrom(x) && x.IsClass));
                interactionDefinitionTypes.AddRange(Array.FindAll(types, x => typeof(InteractionDefinition).IsAssignableFrom(x) && x.IsClass));
            }
            sGameObjectTypes = gameObjectTypes.ToArray();
            sInteractionDefinitionTypes = interactionDefinitionTypes.ToArray();
        }
    }
}
