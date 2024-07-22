using System.Collections.Generic;
using System.IO;
using Luna.UI;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR

public class StatefulWidgetPrefabProcessor : AssetPostprocessor
{
    // Find all prefabs with StatefulWidget component and save them to a scriptable object.
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
    {
        RemoveAllMissingPrefabs();
        foreach (var asset in importedAssets)
        {
            if (asset.EndsWith(".prefab"))
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(asset);
                ProcessPrefab(prefab);
            }
        }
    }
    
    // Find all prefabs with StatefulWidget component and save them to a scriptable object
    // when the editor is initialized or detects modifications to scripts.
    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        RemoveAllMissingPrefabs();
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab.GetComponent<Widget>() != null)
            {
                ProcessPrefab(prefab);
            }
        }
    }
    
    private static void ProcessPrefab(GameObject prefab)
    {
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
    }

    private static void RemoveAllMissingPrefabs()
    {
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
    }


    
    // Find all prefabs with StatefulWidget component and save them to a JSON file.
    // static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
    // {
    //     foreach (var asset in importedAssets)
    //     {
    //         Debug.Log("Imported asset: " + asset);
    //         if (asset.EndsWith(".prefab"))
    //         {
    //             var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(asset);
    //             if (prefab.GetComponent<StatefulWidget>() != null)
    //             {
    //                 // Create the JSON file if it doesn't exist.
    //                 string jsonFilePath = Application.dataPath + "/Resources/StatefulWidgets.json";
    //                 if (!File.Exists(jsonFilePath))
    //                 {
    //                     Directory.CreateDirectory(Application.dataPath + "/Resources");
    //                     File.Create(jsonFilePath).Dispose();
    //                 }
    //
    //                 // Read the existing JSON data from the file.
    //                 var json = Resources.Load<TextAsset>("StatefulWidgets");
    //                 string jsonData = json != null ? json.text : "";
    //                 Debug.Log("Loaded JSON data: " + jsonData);
    //
    //                 // Deserialize the JSON data into a list of prefab paths.
    //                 Dictionary<string, string> prefabPaths = new();
    //                 if (!string.IsNullOrEmpty(jsonData))
    //                 {
    //                     prefabPaths = JsonUtility.FromJson<Dictionary<string, string>>(jsonData);
    //                 }
    //
    //                 // Add the prefab path to the list.
    //                 string prefabPath = AssetDatabase.GetAssetPath(prefab);
    //                 Debug.Log("Added prefab: " + prefab.name + " at path: " + prefabPath);
    //                 prefabPaths[prefab.name] = prefabPath;
    //                 foreach (var path in prefabPaths)
    //                 {
    //                     Debug.Log("Prefab path: " + path.Value);
    //                 }
    //
    //                 // Serialize the updated list of prefab paths back to JSON.
    //                 string updatedJsonData = JsonUtility.FromJson(prefabPaths);
    //                 Debug.Log("Updated JSON data: " + updatedJsonData);
    //
    //                 // Write the updated JSON data back to the file.
    //                 File.WriteAllText(jsonFilePath, updatedJsonData);
    //                 
    //                 // Refresh the asset database.
    //                 EditorUtility.SetDirty(json);
    //             }
    //         }
    //     }
    // }
}

#endif