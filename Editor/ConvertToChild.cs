using System;
using System.Collections.Generic;
using System.Linq;
using Luna.Extensions;
using Luna.Utils;
using UnityEditor;
using UnityEngine;

namespace Luna
{
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class ConvertToChildEditor : UnityEditor.Editor
    {
        // Add the context menu to MonoBehaviour components
        [MenuItem("CONTEXT/MonoBehaviour/Convert to Child", false, 602)]
        private static void OpenConvertToChildWindow(MenuCommand command)
        {
            MonoBehaviour targetComponent = (MonoBehaviour)command.context;
            ConvertToChildWindow.ShowWindow(targetComponent);
        }
    }

    public class ConvertToChildWindow : EditorWindow
    {
        private MonoBehaviour targetComponent;
        private List<Type> derivedTypes = new();
        private List<Type> filteredTypes = new();
        private string searchQuery = "";
        private Vector2 scrollPosition;

        public static void ShowWindow(MonoBehaviour target)
        {
            ConvertToChildWindow window = GetWindow<ConvertToChildWindow>("Convert to Child");
            window.targetComponent = target;
            window.FindDerivedClasses();
            window.Show();
        }

        private void FindDerivedClasses()
        {
            // Get the base type of the current component
            Type baseType = targetComponent.GetType();

            // Find all derived types of the current component
            derivedTypes = baseType.GetDerivedTypes().ToList();

            // Initialize filtered types as the full list initially
            filteredTypes = new List<Type>(derivedTypes);
        }

        private void OnGUI()
        {
            if (targetComponent == null)
            {
                EditorGUILayout.LabelField("No component selected.");
                return;
            }

            EditorGUILayout.LabelField("Select a derived class to convert to:", EditorStyles.boldLabel);

            // Draw search field with an icon
            DrawSearchField();

            // Filter the list based on the search query
            if (!string.IsNullOrEmpty(searchQuery))
            {
                filteredTypes = derivedTypes
                    .Where(type => type.Name.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
            }
            else
            {
                filteredTypes = new List<Type>(derivedTypes); // Reset to the full list if search query is empty
            }

            if (filteredTypes == null || filteredTypes.Count == 0)
            {
                EditorGUILayout.LabelField("No derived classes found.");
                return;
            }

            // Scrollable area to list filtered derived classes
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (Type type in filteredTypes)
            {
                if (GUILayout.Button(type.Name, GUILayout.Height(30)))
                {
                    ConvertTo(type);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawSearchField()
        {
            // Create a horizontal layout for the search bar with an icon
            GUILayout.BeginHorizontal();

            // Display the search icon (using a built-in search icon from Unity)
            GUIContent searchIcon = EditorGUIUtility.IconContent("d_Search Icon"); // Unity's built-in search icon
            GUILayout.Label(searchIcon, GUILayout.Width(20), GUILayout.Height(20));

            // Create the search text field next to the icon
            searchQuery = EditorGUILayout.TextField(searchQuery);

            GUILayout.EndHorizontal();
        }

        private void ConvertTo(Type newType)
        {
            if (targetComponent != null)
            {
                bool isPrefabInstance = PrefabUtility.IsPartOfAnyPrefab(targetComponent.gameObject);
                var prefabComponent = PrefabUtility.GetCorrespondingObjectFromSource(targetComponent);
                
                if (isPrefabInstance)
                    MonoBehaviourUtils.ChangeScript(prefabComponent, newType);
                
                MonoBehaviourUtils.ChangeScript(targetComponent, newType);
                
                Close(); // Close the window once conversion is done
            }
        }
    }
}