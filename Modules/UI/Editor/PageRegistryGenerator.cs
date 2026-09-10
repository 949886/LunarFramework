using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>Editor-side generated PageRegistry builder.</summary>
/// <remarks>
/// Scans Assets for prefab roots containing NavigationPage, normalizes their
/// NavigationPath, rejects duplicate identities, and writes only static
/// Path + prefab definitions. Runtime presentation metadata remains on prefabs.
/// </remarks>
[InitializeOnLoad]
public static class PageRegistryGenerator
{
    public const string AssetPath =
        "Assets/Resources/Cherry/PageRegistry.asset";

    private static bool _scheduled;
    private static bool _rebuilding;

    static PageRegistryGenerator()
    {
        ScheduleRebuild();
    }

    /// <summary>Immediately rebuilds the generated PageRegistry asset.</summary>
    [MenuItem("Tools/Cherry Navigation/Rebuild Page Registry")]
    public static void RebuildNow()
    {
        if (_rebuilding)
            return;

        _rebuilding = true;
        try
        {
            RebuildInternal();
        }
        finally
        {
            _rebuilding = false;
            _scheduled = false;
        }
    }

    internal static void ScheduleRebuild()
    {
        if (_scheduled)
            return;

        _scheduled = true;
        EditorApplication.delayCall += DelayedRebuild;
    }

    private static void DelayedRebuild()
    {
        if (EditorApplication.isCompiling ||
            EditorApplication.isUpdating ||
            EditorApplication.isPlayingOrWillChangePlaymode)
        {
            _scheduled = false;
            ScheduleRebuild();
            return;
        }

        RebuildNow();
    }

    private static void RebuildInternal()
    {
        string[] guids = AssetDatabase.FindAssets(
            "t:Prefab",
            new[] { "Assets" });

        SortedDictionary<string, NavigationPage> pages =
            new SortedDictionary<string, NavigationPage>(
                StringComparer.Ordinal);

        Dictionary<string, string> owners =
            new Dictionary<string, string>(StringComparer.Ordinal);

        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
                continue;

            NavigationPage page = prefab.GetComponent<NavigationPage>();
            if (page == null)
                continue;

            string normalized = PageRegistry.NormalizePath(page.NavigationPath);
            if (normalized.Length == 0)
                continue;

            string previous;
            if (owners.TryGetValue(normalized, out previous))
            {
                Debug.LogError(
                    "Cherry Navigation: duplicate NavigationPath '" +
                    normalized + "' in " + previous + " and " + assetPath + ".");
                continue;
            }

            owners[normalized] = assetPath;
            pages[normalized] = page;
        }

        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/Cherry");

        PageRegistry registry =
            AssetDatabase.LoadAssetAtPath<PageRegistry>(AssetPath);

        if (registry == null)
        {
            registry = ScriptableObject.CreateInstance<PageRegistry>();
            AssetDatabase.CreateAsset(registry, AssetPath);
        }

        UnityEngine.Object[] existing =
            AssetDatabase.LoadAllAssetsAtPath(AssetPath);

        for (int i = 0; i < existing.Length; i++)
        {
            if (existing[i] != registry)
                UnityEngine.Object.DestroyImmediate(existing[i], true);
        }

        List<PageDefinition> definitions = new List<PageDefinition>();

        foreach (KeyValuePair<string, NavigationPage> pair in pages)
        {
            PageDefinition definition =
                ScriptableObject.CreateInstance<PageDefinition>();

            definition.name =
                "PageDefinition_" +
                pair.Key.Replace("ui://", "").Replace("/", "_");

            definition.SetGenerated(pair.Key, pair.Value);
            AssetDatabase.AddObjectToAsset(definition, registry);
            definitions.Add(definition);
        }

        registry.ReplacePages(definitions);
        EditorUtility.SetDirty(registry);

        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(
            AssetPath,
            ImportAssetOptions.ForceUpdate);

        PageRegistry.SetRuntimeInstance(registry);

        Debug.Log(
            "Cherry Navigation: rebuilt " +
            AssetPath + " (" + definitions.Count + " pages).");
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string parent = Path.GetDirectoryName(path).Replace("\\", "/");
        string name = Path.GetFileName(path);

        if (!AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);

        AssetDatabase.CreateFolder(parent, name);
    }
}

/// <summary>Schedules PageRegistry rebuilds after project asset imports.</summary>
public sealed class CherryNavigationAssetPostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        if (TouchesPrefab(importedAssets) ||
            TouchesPrefab(deletedAssets) ||
            TouchesPrefab(movedAssets) ||
            TouchesPrefab(movedFromAssetPaths))
        {
            PageRegistryGenerator.ScheduleRebuild();
        }
    }

    private static bool TouchesPrefab(string[] paths)
    {
        if (paths == null)
            return false;

        for (int i = 0; i < paths.Length; i++)
        {
            if (paths[i].EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
