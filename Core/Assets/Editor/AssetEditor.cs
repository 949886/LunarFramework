// Created by LunarEclipse on 2024-8-3 10:2.

#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;

namespace Core.Assets.Editor
{
    [CustomEditor(typeof(DefaultAsset))]
    public partial class AssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // .svg files are imported as a DefaultAsset.
            // Need to determine that this default asset is an .svg file
            var path = AssetDatabase.GetAssetPath(target);
            
            Debug.Log(path);

            if (path.EndsWith(".data1"))
                SVGInspectorGUI();
            else if (path.EndsWith(".gson"))
                JsonInspectorGUI(path);
            else base.OnInspectorGUI();
        }

        private void SVGInspectorGUI()
        {  
            // TODO: Add inspector code here
            Debug.Log("Data Inspector");
            
            // Draw text field
            EditorGUILayout.TextField("Data", "Data");
        }
        
        private string jsonContent = "";
        private Vector2 scrollPosition;
            
        public void JsonInspectorGUI(string path)
        {
            EditorGUILayout.LabelField("JSON Editor", EditorStyles.boldLabel);

            if (string.IsNullOrEmpty(jsonContent))
            {
                jsonContent = File.ReadAllText(path);
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(400));
            jsonContent = EditorGUILayout.TextArea(jsonContent, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Save JSON"))
            {
                SaveJson(path);
            }
        }

        private void SaveJson(string path)
        {
            File.WriteAllText(path, jsonContent);
            AssetDatabase.Refresh();
        }
        
    }
}

#endif