using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Convenience command for locating the checked-in Navigation Demo scene after
/// the package sample has been imported through Package Manager.
/// </summary>
public static class CherryDemoInstaller
{
    [MenuItem("Tools/Cherry Navigation/Open Navigation Demo Scene")]
    public static void OpenDemoScene()
    {
        string[] guids = AssetDatabase.FindAssets(
            "CherryNavigationDemo t:Scene",
            new[] { "Assets" });

        if (guids.Length == 0)
        {
            Debug.LogError(
                "Cherry Navigation: demo scene was not found under Assets. " +
                "Open Package Manager, select Cherry Navigation, and import " +
                "the 'Navigation Demo' sample first.");
            return;
        }

        string scenePath = AssetDatabase.GUIDToAssetPath(guids[0]);
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        Selection.activeObject =
            AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);

        Debug.Log("Cherry Navigation: opened demo scene " + scenePath);
    }
}
