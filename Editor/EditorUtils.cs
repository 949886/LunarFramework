// Created by LunarEclipse on 2024-08-30 13:55.

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.UI.Extensions;
using UnityEngine;

namespace Luna.Utils
{
    public class EditorUtils
    {
        public static GameObject InstantiatePrefab(string path)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
            {
                Debug.LogError("Prefab not found at path: " + path);
                return null;
            }

            return Object.Instantiate(prefab);
        }
        
        
        public static GameObject InstantiatePrefab(GUID guid, MenuCommand menuCommand = null)
        {
            // Convert the GUID back to an asset path
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            
            if (string.IsNullOrEmpty(prefabPath))
            {
                Debug.LogError("Prefab not found with GUID: " + guid);
                return null;
            }

            // Load the prefab as an object
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            GameObject go = Object.Instantiate(prefab);	
            // GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            EditorGuiUtils.PlaceUIElement(go, menuCommand);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
            return go;
        }
        
    }
}

#endif