// Created by LunarEclipse on 2024-10-02 11:10.

#if UNITY_EDITOR

using System;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Luna.Utils
{
    public static class MonoBehaviourUtils
    {
        /// Change the script of a MonoBehaviour to another script by selecting it from the project window
        /// From: https://discussions.unity.com/t/how-to-change-a-component-to-a-child-of-the-same-component/841080/9
        [MenuItem("CONTEXT/MonoBehaviour/Change Script", false, 601)]
        public static void ChangeScript(MenuCommand command)
        {
            if (command.context == null) return;

            var monoBehaviour = command.context as MonoBehaviour;
            var monoScript = UnityEditor.MonoScript.FromMonoBehaviour(monoBehaviour);

            var scriptPath = AssetDatabase.GetAssetPath(monoScript);
            var directoryPath = new FileInfo(scriptPath).Directory?.FullName;

            // Allow the user to select which script to replace with
            var newScriptPath = EditorUtility.OpenFilePanel("Select replacement script", directoryPath, "cs");
            
            // Don't log anything if they cancelled the window
            if (string.IsNullOrEmpty(newScriptPath)) return;
            
            // Load the selected asset
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            var relativePath = Path.GetRelativePath(projectRoot, newScriptPath);
            relativePath = relativePath.Replace(@"Library\PackageCache\", @"Packages\");
            var chosenTextAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(relativePath) as UnityEditor.MonoScript;
            
            if (chosenTextAsset == null)
            {
                Debug.LogWarning($"Selected script couldn't be loaded ({relativePath})");
                return;
            }
            
            ChangeScript(monoBehaviour, chosenTextAsset);
        }
        
        /// Change the script of a MonoBehaviour to another script.
        ///
        /// Parameters:
        ///  - monoBehaviour: The MonoBehaviour to change the script of.
        ///  - newScript: The new script to change to.
        public static void ChangeScript(MonoBehaviour monoBehaviour, UnityEditor.MonoScript newScript)
        {
            if (monoBehaviour == null || newScript == null) return;
            
            Undo.RegisterCompleteObjectUndo(monoBehaviour, "Changing component script");
            
            var so = new SerializedObject(monoBehaviour);
            var scriptProperty = so.FindProperty("m_Script");
            so.Update();
            scriptProperty.objectReferenceValue = newScript;
            so.ApplyModifiedProperties();
        }
        
        /// Change the script of a MonoBehaviour to another script by type.
        ///
        /// Parameters:
        ///  - monoBehaviour: The MonoBehaviour to change the script of.
        ///  - type: The type of the new script to change to.
        public static void ChangeScript(MonoBehaviour monoBehaviour, Type type)
        {
            if (monoBehaviour == null || type == null) return;
            
            var newScript = Internal.MonoScript.FromType(type);
            ChangeScript(monoBehaviour, newScript);
        }
        
        /// Change the script of a MonoBehaviour to another script by GUID.
        ///
        /// Parameters:
        ///  - monoBehaviour: The MonoBehaviour to change the script of.
        ///  - guid: The GUID of the new script to change to.
        public static void ChangeScript(MonoBehaviour monoBehaviour, string guid)
        {
            if (monoBehaviour == null || string.IsNullOrEmpty(guid)) return;
            
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var newScript = AssetDatabase.LoadAssetAtPath<UnityEditor.MonoScript>(assetPath);
            
            ChangeScript(monoBehaviour, newScript);
        }
        
        // [MenuItem("CONTEXT/MonoBehaviour/Change Script2", false, 601)]
        // public static void ChangeScript2(MenuCommand command)
        // {
        //     if (command.context == null) return;
        //
        //     var monoBehaviour = command.context as MonoBehaviour;
        //     var monoScript = MonoScript.FromMonoBehaviour(monoBehaviour);
        //
        //     var scriptPath = AssetDatabase.GetAssetPath(monoScript);
        //     var directoryPath = new FileInfo(scriptPath).Directory?.FullName;
        //     
        //     var rect = new Rect(0, 0, 300, 300);
        //     PickComponentWindow.Show(rect, script => {
        //         Undo.RegisterCompleteObjectUndo(command.context, "Changing component script");
        //     
        //         var so = new SerializedObject(monoBehaviour);
        //         var scriptProperty = so.FindProperty("m_Script");
        //         so.Update();
        //         scriptProperty.objectReferenceValue = script;
        //         so.ApplyModifiedProperties();
        //     });
        // }
    }
}

#endif