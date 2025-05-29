// // Created by LunarEclipse on 2025-05-29 17:05.

#if USE_UGUI

using Luna.UI.Navigation;
using Luna.Utils;
using UnityEditor;
using UnityEditor.UI.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Luna.UI.Editor
{
    public class UIMenuOptions
    {
        #region UGUI

        [MenuItem("GameObject/UI/Horizontal Layout Group", false, 2200)]
        public static void AddHorizontalLayoutGroup(MenuCommand menuCommand)
        {
            var root = EditorGuiUtils.GetOrCreateCanvasGameObject();
            var horizontalLayoutGroup = new GameObject("Row");
            horizontalLayoutGroup.AddComponent<RectTransform>();
            horizontalLayoutGroup.AddComponent<HorizontalLayoutGroup>();
            EditorGuiUtils.PlaceUIElement(horizontalLayoutGroup, menuCommand);
            Undo.RegisterCreatedObjectUndo(horizontalLayoutGroup, "Create " + horizontalLayoutGroup.name);
            Selection.activeObject = horizontalLayoutGroup;
        }

        [MenuItem("GameObject/UI/Vertical Layout Group", false, 2201)]
        public static void AddVerticalLayoutGroup(MenuCommand menuCommand)
        {
            var root = EditorGuiUtils.GetOrCreateCanvasGameObject();
            var verticalLayoutGroup = new GameObject("Column");
            verticalLayoutGroup.AddComponent<RectTransform>();
            verticalLayoutGroup.AddComponent<VerticalLayoutGroup>();
            EditorGuiUtils.PlaceUIElement(verticalLayoutGroup, menuCommand);
            Undo.RegisterCreatedObjectUndo(verticalLayoutGroup, "Create " + verticalLayoutGroup.name);
            Selection.activeObject = verticalLayoutGroup;
        }

        [MenuItem("GameObject/UI/Grid Layout Group", false, 2202)]
        public static void AddGridLayoutGroup(MenuCommand menuCommand)
        {
            var root = EditorGuiUtils.GetOrCreateCanvasGameObject();
            var verticalLayoutGroup = new GameObject("Grid");
            verticalLayoutGroup.AddComponent<RectTransform>();
            verticalLayoutGroup.AddComponent<GridLayoutGroup>();
            EditorGuiUtils.PlaceUIElement(verticalLayoutGroup, menuCommand);
            Undo.RegisterCreatedObjectUndo(verticalLayoutGroup, "Create " + verticalLayoutGroup.name);
            Selection.activeObject = verticalLayoutGroup;
        }

        #endregion


        [MenuItem("GameObject/UI Widget/Navigator", false, 100)]
        public static void AddNavigator(MenuCommand menuCommand)
        {
            var navigator = new GameObject("UI Navigator").AddComponent<Navigator>();
            navigator.canvas = Object.FindObjectOfType<Canvas>()?.rootCanvas;
            navigator.rootWidget = Object.FindObjectOfType<Widget>();
            Selection.activeObject = navigator.gameObject;
        }
        
        [MenuItem("GameObject/UI Widget/Loading Indicator", false, 200)]
        public static void AddLoadingIndicator(MenuCommand menuCommand)
        {
            var go = EditorUtils.InstantiatePrefab(new GUID("8d8ff971562e09242a7328fe57647c0b"), menuCommand);
            go.name = "Circular Loading Indicator";
            EditorGuiUtils.PlaceUIElement(go, menuCommand);
            Selection.activeObject = go;
        }
    }
}

#endif