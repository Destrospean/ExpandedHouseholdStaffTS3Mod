using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using Sims3.UI;
﻿using System;
using System.Collections.Generic;
using System.Reflection;
using zoeoeAndDestrospean.UI.Columns;
using ObjectPickerDialog = zoeoeAndDestrospean.UI.Dialogs.ObjectPickerDialog;

namespace zoeoeAndDestrospean.Utils
{
    public static class InteractionObjectTypeUtils
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

        public static bool TryUIGetSelectedTypes(out Type[] selectedTypes, Type[] allTypes = null, string namespaceListTitle = null, string typeListTitle = null)
        {
            bool retVal;
            Type[] tempSelectedTypes = null;
            if (DebugUtils.TryDisplayScriptError(() =>
                {
                    string entryKey = typeof(ObjectPickerDialog).GetLocalizationKey();
                    entryKey = entryKey.Remove(entryKey.LastIndexOf("/"));
                    string namespaceListDialogLocalizationPath = entryKey + "/Dialogs/NamespaceListDialog";
                    string typeListDialogLocalizationPath = entryKey + "/Dialogs/TypeListDialog";
                    Array.Sort(allTypes, (a, b) => a.FullName.CompareTo(b.FullName));
                    List<string> namespaces = new List<string>();
                    foreach (Type type in allTypes)
                    {
                        if (!namespaces.Contains(type.Namespace))
                        {
                            namespaces.Add(type.Namespace);
                        }
                    }
                    bool cancelled, confirmed;
                    while (true)
                    {
                        List<string> selectedNamespaces = ObjectPickerDialog.Show(namespaceListTitle ?? Responder.Instance.LocalizationModel.LocalizeString(namespaceListDialogLocalizationPath + ":Title"), new List<ObjectPicker.TabInfo>
                            {
                                new ObjectPicker.TabInfo("shop_all_r2", Responder.Instance.LocalizationModel.LocalizeString("Ui/Caption/ObjectPicker:All"), namespaces.ConvertAll(x => new ObjectPicker.RowInfo(x, new List<ObjectPicker.ColumnInfo>())))
                            }, new List<ObjectPickerDialog.CommonHeaderInfo<string>>
                            {
                                new TextColumn(namespaceListDialogLocalizationPath)
                            }, 1, out confirmed, out cancelled);
                        if (cancelled)
                        {
                            tempSelectedTypes = null;
                            return false;
                        }
                        tempSelectedTypes = (ObjectPickerDialog.Show(typeListTitle ?? Responder.Instance.LocalizationModel.LocalizeString(typeListDialogLocalizationPath + ":Title"), new List<ObjectPicker.TabInfo>
                            {
                                new ObjectPicker.TabInfo("shop_all_r2", selectedNamespaces[0], new List<Type>(allTypes).FindAll(x => x.Namespace == selectedNamespaces[0]).ConvertAll(x => new ObjectPicker.RowInfo(x, new List<ObjectPicker.ColumnInfo>())))
                            }, new List<ObjectPickerDialog.CommonHeaderInfo<Type>>
                            {
                                new TypeColumn(typeListDialogLocalizationPath)
                            }, int.MaxValue, out confirmed, out cancelled) ?? new List<Type>()).ToArray();
                        if (confirmed)
                        {
                            return true;
                        }
                    }
                }, out retVal))
            {
                selectedTypes = null;
                return false;
            }
            selectedTypes = tempSelectedTypes;
            return retVal;
        }
    }
}
