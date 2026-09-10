using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Generated path/type lookup table for NavigationPage prefabs.</summary>
/// <remarks>
/// PageRegistryGenerator scans prefab roots under Assets and writes
/// Assets/Resources/Cherry/PageRegistry.asset. Registry identity is static
/// ui://... Path only; query and fragment stay on concrete NavigationRoute.Uri.
/// </remarks>
public sealed class PageRegistry : ScriptableObject
{
    public const string RegistryPath = "Cherry/PageRegistry";

    private static PageRegistry _instance;

    [SerializeField] private List<PageDefinition> _pages = new List<PageDefinition>();

    private readonly Dictionary<string, PageDefinition> _byPath =
        new Dictionary<string, PageDefinition>(StringComparer.Ordinal);

    private bool _indexValid;

    /// <summary>Process-local registry singleton loaded from Unity Resources.</summary>
    /// <remarks>
    /// Missing generated assets produce a temporary empty registry plus an error,
    /// rather than silently creating page mappings.
    /// </remarks>
    public static PageRegistry Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<PageRegistry>(RegistryPath);
                if (_instance == null)
                {
                    _instance = CreateInstance<PageRegistry>();
                    _instance.hideFlags = HideFlags.HideAndDontSave;
                    Debug.LogError(
                        "Cherry Navigation: generated PageRegistry was not found at " +
                        "Assets/Resources/Cherry/PageRegistry.asset. " +
                        "Use Tools/Cherry Navigation/Rebuild Page Registry.");
                }
            }

            return _instance;
        }
    }

    /// <summary>Ordered generated static page definitions.</summary>
    public IReadOnlyList<PageDefinition> Pages { get { return _pages; } }

    /// <summary>Normalizes Cherry shorthand/static identity to ui://...</summary>
    /// <returns>Canonical identity, or empty string when invalid.</returns>
    /// <remarks>
    /// This is deliberately not a URI parser. Query/fragment parsing belongs to
    /// Navigator and concrete NavigationRoute.Uri.
    /// </remarks>
    public static string NormalizePath(string path)
    {
        string raw = (path ?? "").Trim();
        if (raw == "/" || raw == "ui://")
            return "ui://";

        string suffix;
        if (raw.StartsWith("ui://", StringComparison.Ordinal))
            suffix = raw.Substring(5);
        else if (raw.StartsWith("/", StringComparison.Ordinal))
            suffix = raw.Substring(1);
        else
            return "";

        while (suffix.StartsWith("/", StringComparison.Ordinal))
            suffix = suffix.Substring(1);
        while (suffix.EndsWith("/", StringComparison.Ordinal) && suffix.Length > 0)
            suffix = suffix.Substring(0, suffix.Length - 1);

        if (suffix.Length == 0)
            return "ui://";
        if (suffix.Contains("://"))
            return "";

        return "ui://" + suffix;
    }

    /// <summary>Resolves a normalized/shorthand static identity or throws.</summary>
    public PageDefinition Resolve(string path)
    {
        PageDefinition definition;
        if (!TryResolve(path, out definition))
            throw new InvalidOperationException("Navigation page not found: " + path);
        return definition;
    }

    /// <summary>Attempts to resolve a static identity without throwing.</summary>
    public bool TryResolve(string path, out PageDefinition definition)
    {
        if (!_indexValid)
            RebuildIndexes();

        string normalized = NormalizePath(path);
        if (normalized.Length == 0)
        {
            definition = null;
            return false;
        }

        return _byPath.TryGetValue(normalized, out definition);
    }

    /// <summary>Returns whether the registry contains the normalized identity.</summary>
    public bool Contains(string path)
    {
        PageDefinition unused;
        return TryResolve(path, out unused);
    }

    /// <summary>Resolves the first generated prefab whose root page is TPage.</summary>
    /// <remarks>Typed Navigator APIs use this lookup instead of a route string.</remarks>
    public PageDefinition Resolve<TPage>() where TPage : NavigationPage
    {
        PageDefinition definition;
        if (!TryResolve<TPage>(out definition))
            throw new InvalidOperationException(
                "Navigation page type is not registered: " + typeof(TPage).FullName);
        return definition;
    }

    /// <summary>Attempts typed prefab lookup without throwing.</summary>
    public bool TryResolve<TPage>(out PageDefinition definition)
        where TPage : NavigationPage
    {
        for (int i = 0; i < _pages.Count; i++)
        {
            PageDefinition candidate = _pages[i];
            if (candidate != null && candidate.MatchesPageType<TPage>())
            {
                definition = candidate;
                return true;
            }
        }

        definition = null;
        return false;
    }

    /// <summary>Replaces all definitions and immediately rebuilds indexes.</summary>
    /// <remarks>Primarily intended for editor generation and regression tests.</remarks>
    public void ReplacePages(IReadOnlyList<PageDefinition> newPages)
    {
        _pages.Clear();
        if (newPages != null)
        {
            for (int i = 0; i < newPages.Count; i++)
                _pages.Add(newPages[i]);
        }

        _indexValid = false;
        RebuildIndexes();
    }

    /// <summary>Compares ordered Path/Prefab content with another definition list.</summary>
    public bool ContentEquals(IReadOnlyList<PageDefinition> otherPages)
    {
        if (otherPages == null || otherPages.Count != _pages.Count)
            return false;

        for (int i = 0; i < _pages.Count; i++)
        {
            PageDefinition a = _pages[i];
            PageDefinition b = otherPages[i];

            if (a == null || b == null || a.Path != b.Path)
                return false;
            if (a.Scene != b.Scene)
                return false;
        }

        return true;
    }

    /// <summary>Rebuilds the process-local normalized path lookup table.</summary>
    public void RebuildIndexes()
    {
        _byPath.Clear();

        for (int i = 0; i < _pages.Count; i++)
        {
            PageDefinition page = _pages[i];
            if (page == null)
                continue;

            string normalized = NormalizePath(page.Path);
            if (normalized.Length != 0)
                _byPath[normalized] = page;
        }

        _indexValid = true;
    }

    internal static void SetRuntimeInstance(PageRegistry registry)
    {
        _instance = registry;
        if (_instance != null)
            _instance.RebuildIndexes();
    }
}
