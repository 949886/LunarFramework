#if USE_UGUI

using UnityEditor.UI.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    public class UIMenuOptions: Editor
    {
        #region Unity UI

        [MenuItem("GameObject/UI/Horizontal Layout Group", false, 2200)]
        public static void AddHorizontalLayoutGroup(MenuCommand menuCommand)
        {
            var root = UGUIEditorUtils.GetOrCreateCanvasGameObject();
            var horizontalLayoutGroup = new GameObject("Row");
            horizontalLayoutGroup.AddComponent<RectTransform>();
            horizontalLayoutGroup.AddComponent<HorizontalLayoutGroup>();
            UGUIEditorUtils.PlaceUIElement(horizontalLayoutGroup, menuCommand);
            Undo.RegisterCreatedObjectUndo(horizontalLayoutGroup, "Create " + horizontalLayoutGroup.name);
            Selection.activeObject = horizontalLayoutGroup;
        }

        [MenuItem("GameObject/UI/Vertical Layout Group", false, 2201)]
        public static void AddVerticalLayoutGroup(MenuCommand menuCommand)
        {
            var root = UGUIEditorUtils.GetOrCreateCanvasGameObject();
            var verticalLayoutGroup = new GameObject("Column");
            verticalLayoutGroup.AddComponent<RectTransform>();
            verticalLayoutGroup.AddComponent<VerticalLayoutGroup>();
            UGUIEditorUtils.PlaceUIElement(verticalLayoutGroup, menuCommand);
            Undo.RegisterCreatedObjectUndo(verticalLayoutGroup, "Create " + verticalLayoutGroup.name);
            Selection.activeObject = verticalLayoutGroup;
        }

        [MenuItem("GameObject/UI/Grid Layout Group", false, 2202)]
        public static void AddGridLayoutGroup(MenuCommand menuCommand)
        {
            var root = UGUIEditorUtils.GetOrCreateCanvasGameObject();
            var verticalLayoutGroup = new GameObject("Grid");
            verticalLayoutGroup.AddComponent<RectTransform>();
            verticalLayoutGroup.AddComponent<GridLayoutGroup>();
            UGUIEditorUtils.PlaceUIElement(verticalLayoutGroup, menuCommand);
            Undo.RegisterCreatedObjectUndo(verticalLayoutGroup, "Create " + verticalLayoutGroup.name);
            Selection.activeObject = verticalLayoutGroup;
        }

        #endregion
    }
}

#endif