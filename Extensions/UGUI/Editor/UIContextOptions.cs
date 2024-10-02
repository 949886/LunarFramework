// Created by LunarEclipse on 2024-10-02 15:10.

using System;
using System.Linq;
using Luna.Utils;
using UnityEditor;
using UnityEngine;
using MonoScript = UnityEditor.MonoScript;

namespace Luna.Extensions
{
    public class UIContextOptions
    {
        // [MenuItem("CONTEXT/MonoBehaviour/Convert", false, 602)]
        // public static void ConvertToChild(MenuCommand command)
        // {
        //     
        // }

        // [MenuItem("CONTEXT/MonoBehaviour/Convert to Child")]
        // private static void ConvertToChild(MenuCommand command)
        // {
        //     // Get the target MonoBehaviour component
        //     MonoBehaviour target = (MonoBehaviour)command.context;
        //
        //     // Get the type of the component
        //     Type baseType = target.GetType();
        //
        //     // Get all derived types of this component's type
        //     var derivedTypes = AppDomain.CurrentDomain.GetAssemblies()
        //         .SelectMany(assembly => assembly.GetTypes())
        //         .Where(type => type.IsSubclassOf(baseType) && !type.IsAbstract)
        //         .ToArray();
        //
        //     // Display a selection list of derived types
        //     string[] derivedTypeNames = derivedTypes.Select(type => type.Name).ToArray();
        //     int selected = EditorUtility.DisplayDialogComplex(
        //         "Convert to Child",
        //         "Select a child class to convert to:",
        //         derivedTypeNames.Length > 0 ? derivedTypeNames[0] : "None",
        //         "Cancel",
        //         null
        //     );
        //
        //     // If a selection is made, replace the component
        //     if (selected >= 0 && selected < derivedTypes.Length)
        //     {
        //         Undo.DestroyObjectImmediate(target); // Remove current component
        //         target.gameObject.AddComponent(derivedTypes[selected]); // Add new derived class component
        //     }
        // }

        // [MenuItem("CONTEXT/MonoBehaviour/Loading...", true)]
        // public static bool ValidateConvertToChild(MenuCommand command)
        // {
        //     Debug.Log($"ValidateConvertToChild: {command.context}");
        //     
        //     var monoBehaviour = command.context as MonoBehaviour;
        //     AddConvertToChildOption(monoBehaviour);
        //     
        //     return false;
        // }

        private static int AddConvertToChildOption(MonoBehaviour monoBehaviour)
        {
            if (monoBehaviour == null) return 0;

            var type = monoBehaviour.GetType();
            var derivedTypes = type.GetDerivedTypes().ToArray();

            var existingItems = Internal.MenuUtils.GetMenuItems($"CONTEXT/{type.Name}/Convert to child", true);

            Debug.Log($"Found {derivedTypes.Length} derived types for {type.Name}");
            foreach (var derivedType in derivedTypes)
            {
                var name = $"CONTEXT/{type.Name}/Convert to child/{derivedType.Name}";

                // if (existingItems.Any(item => item.Path == name)) continue;

                Internal.MenuUtils.AddMenuItem(name, 602, () =>
                {
                    Debug.Log($"Convert {type.Name} to {derivedType.Name}");
                    MonoBehaviourUtils.ChangeScript(monoBehaviour, derivedType);
                });
            }

            return derivedTypes.Length;
        }
    }
}