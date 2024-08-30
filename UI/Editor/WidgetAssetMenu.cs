// Created by LunarEclipse on 2024-08-30 14:32.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Threading.Tasks;
using UnityEditor.Callbacks;
using UnityEditor.ProjectWindowCallback;
using Object = UnityEngine.Object;

public class WidgetAssetMenu : EditorWindow
{
        #region Widgets
        
        private static string PREFAB_CACHE_PATH = Application.temporaryCachePath + "/Widgets/";
    
        [MenuItem("Assets/Create/UI Widget Script", false, 81)]
        public static void AddWidgetScript()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            var widgetName = "Test";

            if (widgetName != null)
            {

                // Create a csharp script
                var scriptPath = path + "/" + widgetName + ".cs";

                // File.WriteAllText(scriptPath, scriptContent);
                ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, CreateInstance<OnCreateScript>(), "NewWidget", null, null);
            }
        }
        
        internal class OnCreateScript : EndNameEditAction
        {
            private Queue<Action> _actions = new();
            
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                if (!pathName.EndsWith(".cs"))
                    pathName += ".cs";
                pathName = AssetDatabase.GenerateUniqueAssetPath(pathName);
                
                var fileName = Path.GetFileNameWithoutExtension(pathName);
                
                var scriptContent = 
                    @"
using Luna.UI;
using Luna.UI.Navigation;
using UnityEngine;

public class " + fileName + @": Widget
{
    
}
";
                //ProjectWindowUtil.CreateScriptAssetFromTemplateFile(pathName, resourceFile);
                var monoScript = CreateScriptAssetWithContent(pathName, scriptContent) as MonoScript; // ProjectWindowUtil.CreateScriptAssetFromTemplateFile(pathName, resourceFile);
                ProjectWindowUtil.ShowCreatedAsset(monoScript);
                
                // Create a prefab
                var prefab = new GameObject(fileName);
                // Type scriptClass = monoScript.GetClass();
                // prefab.AddComponent(scriptClass);
                PrefabUtility.SaveAsPrefabAsset(prefab, pathName.Replace(".cs", ".prefab"));
                DestroyImmediate(prefab);
                
                // Save the script path as text to cache directory for further processing
                var scriptPath = Path.Combine(PREFAB_CACHE_PATH, fileName + ".txt");
                if (!Directory.Exists(PREFAB_CACHE_PATH))
                    Directory.CreateDirectory(PREFAB_CACHE_PATH);
                File.WriteAllTextAsync(scriptPath, pathName);
            }
            
            [DidReloadScripts]
            private static async void CreatePrefabAfterCompile()
            {
                // Read all the prefabs in the cache directory and add script component
                var files = Directory.GetFiles(PREFAB_CACHE_PATH, "*.txt");
                foreach (var file in files)
                {
                    var scriptPath = await File.ReadAllTextAsync(file);
                    var script = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
                    var scriptClass = script.GetClass();
                    var prefab = PrefabUtility.LoadPrefabContents(scriptPath.Replace(".cs", ".prefab"));
                    prefab.AddComponent(scriptClass);
                    EditorUtility.SetDirty(prefab);
                    File.Delete(file);
                }
            }

            public override void Cancelled(int instanceId, string pathName, string resourceFile)
            {
                Selection.activeObject = null;
            }
            
            private static Object CreateScriptAssetWithContent(string assetPath, string scriptContent)
            {
                string fullPath = Path.GetFullPath(assetPath);
                File.WriteAllText(fullPath, scriptContent);

                // Import the asset
                AssetDatabase.ImportAsset(assetPath);

                return AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
            }
        }
        


        #endregion
}
