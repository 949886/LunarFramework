using System;
using UnityEngine;

/// <summary>Generated static lookup record for one navigable prefab.</summary>
/// <remarks>
/// Unity maps Cherry's PackedScene concept to a NavigationPage prefab. The
/// definition intentionally contains only Path and Scene/Prefab. Presentation,
/// URI query/fragment, parameters, transition state, and results remain runtime
/// NavigationPage/NavigationRoute concerns.
/// </remarks>
public sealed class PageDefinition : ScriptableObject
{
    [SerializeField] private string _path = "";
    [SerializeField] private NavigationPage _scene;

    [NonSerialized] private Func<Transform, NavigationPage> _runtimeFactory;

    /// <summary>Canonical static application identity, for example ui://settings.</summary>
    public string Path { get { return _path; } }

    /// <summary>NavigationPage prefab instantiated whenever this definition is pushed.</summary>
    /// <remarks>
    /// Named Scene for cross-engine Cherry API parity even though Unity stores a prefab.
    /// </remarks>
    public NavigationPage Scene { get { return _scene; } }

    /// <summary>Unity-friendly alias for Scene.</summary>
    public NavigationPage Prefab { get { return _scene; } }

    internal void SetGenerated(string path, NavigationPage scene)
    {
        _path = PageRegistry.NormalizePath(path);
        _scene = scene;
        _runtimeFactory = null;
    }

    internal static PageDefinition CreateRuntime(
        string path,
        Func<Transform, NavigationPage> factory)
    {
        PageDefinition definition = CreateInstance<PageDefinition>();
        definition.hideFlags = HideFlags.HideAndDontSave;
        definition._path = PageRegistry.NormalizePath(path);
        definition._runtimeFactory = factory;
        return definition;
    }

    internal NavigationPage InstantiatePage(Transform parent)
    {
        if (_runtimeFactory != null)
            return _runtimeFactory(parent);

        if (_scene == null)
            throw new InvalidOperationException(
                "PageDefinition '" + _path + "' has no NavigationPage prefab.");

        return UnityEngine.Object.Instantiate(_scene, parent, false);
    }

    internal bool MatchesPageType<TPage>() where TPage : NavigationPage
    {
        return _scene != null && _scene is TPage;
    }
}
