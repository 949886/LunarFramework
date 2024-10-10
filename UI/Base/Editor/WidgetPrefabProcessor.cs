#if UNITY_EDITOR

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Luna.UI;
using UnityEngine;
using UnityEditor;
using Debug = UnityEngine.Debug;

#if USE_ADDRESSABLES
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif

public class WidgetPrefabProcessor : AssetPostprocessor
{
    // Find all prefabs with StatefulWidget component and save them to a scriptable object.
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
    {
        var stopwatch = Stopwatch.StartNew();
        
        RemoveAllMissingPrefabs();
        foreach (var asset in importedAssets)
        {
            if (asset.EndsWith(".prefab"))
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(asset);
                ProcessPrefab(prefab);
            }
        }
        
        stopwatch.Stop();
        Debug.Log($"[WidgetPrefabProcessor] Processed {importedAssets.Length} prefabs in {stopwatch.ElapsedMilliseconds}ms");
    }
    
    // Find all prefabs with StatefulWidget component and save them to a scriptable object
    // when the editor is initialized or detects modifications to scripts.
    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        var stopwatch = Stopwatch.StartNew();
        
        RemoveAllMissingPrefabs();
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            ProcessPrefab(prefab);
        }
        
        stopwatch.Stop();
        Debug.Log($"[WidgetPrefabProcessor] Initialize {guids.Length} prefabs in {stopwatch.ElapsedMilliseconds}ms");
    }
    
    private static void ProcessPrefab(GameObject prefab)
    {
#if USE_ADDRESSABLES && !DISABLE_ADDRESSABLE_NAVIGATION
        var component = prefab.GetComponent<Widget>();
        if (component == null) return;
        
        // Add the prefab to the addressable group.
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        AddressableAssetGroup group = settings.FindGroup("Widgets");
        if (group == null)
        {
            Debug.Log("Creating new group: Widgets");
            group = settings.CreateGroup("Widgets", false, false, false, settings.DefaultGroup.Schemas);
        }

        var path = AssetDatabase.GetAssetPath(prefab);
        var ns = component.GetType().Namespace ?? "global";
        AddressableAssetEntry entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(path), group);
        entry.SetAddress($"{ns}.{component.GetType().Name}");
        
        // Remove existing labels.
        entry.labels.Clear();
        
        // Create new labels if they don't exist.
        if (!settings.GetLabels().Contains("UI"))
            settings.AddLabel("UI");
        if (!settings.GetLabels().Contains(ns))
            settings.AddLabel(ns);
        
        entry.SetLabel("UI", true);
        entry.SetLabel(ns, true);
#else // USE SCRIPTABLE OBJECT
        if (prefab.GetComponent<Widget>() != null)
        {
            // Create the scriptable object if it doesn't exist.
            Widgets widgets = Resources.Load<Widgets>(Widget.WIDGETS_DB_FILE_NAME);
            if (widgets == null)
            {
                // Create directory if it doesn't exist.
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                    AssetDatabase.CreateFolder("Assets", "Resources");
                
                widgets = ScriptableObject.CreateInstance<Widgets>();
                AssetDatabase.CreateAsset(widgets, $"Assets/Resources/{Widget.WIDGETS_DB_FILE_NAME}.asset");
            }

            widgets.Add(prefab);
            EditorUtility.SetDirty(widgets);
        }
#endif
        
    }

    private static void RemoveAllMissingPrefabs()
    {
#if USE_ADDRESSABLES && !DISABLE_ADDRESSABLE_NAVIGATION
        // AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        // AddressableAssetGroup group = settings.FindGroup("Widgets");
        // if (group != null)
        // {
        //     List<AddressableAssetEntry> entries = new List<AddressableAssetEntry>(group.entries);
        //     foreach (var entry in entries)
        //     {
        //         // Debug.Log($"[WidgetPrefabProcessor] Found entry: {entry.address}");
        //         if (entry == null)
        //         {
        //             group.RemoveAssetEntry(entry);
        //             Debug.Log($"Removed missing prefab from Widgets group");
        //         }
        //     }
        // }
#else // USE SCRIPTABLE OBJECT
        Widgets widgets = Resources.Load<Widgets>(Widget.WIDGETS_DB_FILE_NAME);
        if (widgets != null)
        {
            bool removed = false;
            for (int i = widgets.Prefabs.Count - 1; i >= 0; i--)
            {
                if (widgets.Prefabs[i] == null)
                {
                    Debug.Log("Removed missing prefab from Widgets.g");
                    widgets.Prefabs.RemoveAt(i);
                    removed = true;
                }
            }

            if (removed)
            {
                EditorUtility.SetDirty(widgets);
            }
        }
#endif
    }
    
#if USE_JSON_FILE_WIDGET_DB
    // Find all prefabs with StatefulWidget component and save them to a JSON file.
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
    {
        foreach (var asset in importedAssets)
        {
            Debug.Log("Imported asset: " + asset);
            if (asset.EndsWith(".prefab"))
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(asset);
                if (prefab.GetComponent<StatefulWidget>() != null)
                {
                    // Create the JSON file if it doesn't exist.
                    string jsonFilePath = Application.dataPath + "/Resources/StatefulWidgets.json";
                    if (!File.Exists(jsonFilePath))
                    {
                        Directory.CreateDirectory(Application.dataPath + "/Resources");
                        File.Create(jsonFilePath).Dispose();
                    }
    
                    // Read the existing JSON data from the file.
                    var json = Resources.Load<TextAsset>("StatefulWidgets");
                    string jsonData = json != null ? json.text : "";
                    Debug.Log("Loaded JSON data: " + jsonData);
    
                    // Deserialize the JSON data into a list of prefab paths.
                    Dictionary<string, string> prefabPaths = new();
                    if (!string.IsNullOrEmpty(jsonData))
                    {
                        prefabPaths = JsonUtility.FromJson<Dictionary<string, string>>(jsonData);
                    }
    
                    // Add the prefab path to the list.
                    string prefabPath = AssetDatabase.GetAssetPath(prefab);
                    Debug.Log("Added prefab: " + prefab.name + " at path: " + prefabPath);
                    prefabPaths[prefab.name] = prefabPath;
                    foreach (var path in prefabPaths)
                    {
                        Debug.Log("Prefab path: " + path.Value);
                    }
    
                    // Serialize the updated list of prefab paths back to JSON.
                    string updatedJsonData = JsonUtility.FromJson(prefabPaths);
                    Debug.Log("Updated JSON data: " + updatedJsonData);
    
                    // Write the updated JSON data back to the file.
                    File.WriteAllText(jsonFilePath, updatedJsonData);
                    
                    // Refresh the asset database.
                    EditorUtility.SetDirty(json);
                }
            }
        }
    }
#endif

}

#endif