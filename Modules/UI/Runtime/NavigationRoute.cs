using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Lifecycle state of one concrete navigation route.</summary>
public enum NavigationRouteState
{
    /// <summary>Accepted route not yet executing its mount transaction.</summary>
    Created,

    /// <summary>Preparing, configuring, activating, or transitioning incoming.</summary>
    Pushing,

    /// <summary>Committed navigation-current route.</summary>
    Active,

    /// <summary>Committed route retained below another current route.</summary>
    Covered,

    /// <summary>Leaving route that may remain committed until transition finishes.</summary>
    Popping,

    /// <summary>Permanently removed route whose Popped task is complete.</summary>
    Disposed,

    /// <summary>Route preparation/mount failed; Mounted/Popped fault.</summary>
    Failed,
}

/// <summary>
/// One concrete Cherry navigation entry.
/// </summary>
/// <remarks>
/// PageDefinition.Path is static page identity. Uri is the exact concrete
/// navigation request and may include query and fragment. Multiple routes can
/// therefore share one PageDefinition while carrying different URI values,
/// parameters, presentation overrides, lifecycle state, and results.
/// </remarks>
public class NavigationRoute
{
    private readonly TaskCompletionSource<bool> _mounted =
        new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly TaskCompletionSource<object> _popped =
        new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

    internal NavigationRoute(
        PageDefinition definition,
        object parameters,
        Uri uri = null,
        NavigationPresentation? presentationOverride = null)
    {
        Definition = definition;
        Parameters = parameters;
        Uri = uri ?? BuildDefinitionUri(definition != null ? definition.Path : "");
        PresentationOverride = presentationOverride;
    }

    /// <summary>Static identity/prefab definition resolved for this route.</summary>
    public PageDefinition Definition { get; internal set; }
    /// <summary>Exact concrete System.Uri for this navigation instance.</summary>
    /// <remarks>
    /// Dynamic string routing preserves query/fragment here while PageRegistry
    /// matching uses only the base identity. Typed routing derives Uri from Path.
    /// </remarks>
    public Uri Uri { get; internal set; }
    /// <summary>Arbitrary payload for parameter-based navigation, otherwise null.</summary>
    public object Parameters { get; internal set; }
    /// <summary>Owning Navigator after route execution begins.</summary>
    public Navigator Navigator { get; internal set; }
    /// <summary>Instantiated page after route preparation begins.</summary>
    public NavigationPage Page { get; internal set; }

    /// <summary>Effective presentation for this concrete route.</summary>
    /// <remarks>
    /// Snapshotted before configured callbacks, then overridden by ShowModal,
    /// ShowOverlay, or ShowDialog when applicable.
    /// </remarks>
    public NavigationPresentation Presentation { get; set; }

    /// <summary>Whether lower committed routes are hidden from visual composition.</summary>
    /// <remarks>
    /// Unity implements this with Navigator-owned CanvasGroups rather than
    /// deactivating covered GameObjects, preserving Process semantics.
    /// </remarks>
    public bool Opaque { get; set; }

    /// <summary>How this page processes while another route covers it.</summary>
    /// <remarks>This policy belongs to the covered route, not the covering route.</remarks>
    public CoveredBehavior CoveredBehavior { get; set; }

    /// <summary>Effective transition snapshot for this concrete route.</summary>
    /// <remarks>Null falls back to Navigator.DefaultTransition.</remarks>
    public NavigationTransition Transition { get; set; }

    /// <summary>Current route lifecycle state.</summary>
    public NavigationRouteState State { get; internal set; }

    /// <summary>Canonical static page identity from Definition.</summary>
    public string Path
    {
        get { return Definition != null ? Definition.Path : ""; }
    }

    /// <summary>Whether this route is Navigator's committed current route.</summary>
    public bool IsCurrent
    {
        get { return Navigator != null && ReferenceEquals(Navigator.CurrentRoute, this); }
    }

    /// <summary>Whether this route is first in the committed mounted stack.</summary>
    public bool IsFirst
    {
        get { return Navigator != null && ReferenceEquals(Navigator.FirstRoute, this); }
    }

    /// <summary>Whether the route has not reached Disposed or Failed.</summary>
    /// <remarks>Covered and Popping routes are still alive; inspect State for precision.</remarks>
    public bool IsActive
    {
        get
        {
            return State != NavigationRouteState.Disposed &&
                   State != NavigationRouteState.Failed;
        }
    }

    /// <summary>Completes after the route commits and receives Entered.</summary>
    /// <remarks>Faults when route preparation/mounting fails.</remarks>
    public Task Mounted { get { return _mounted.Task; } }
    /// <summary>Completes with the route result when it permanently leaves Navigator.</summary>
    /// <remarks>Pop, Replace, Remove, and Clear all complete this same route task.</remarks>
    public Task<object> Popped { get { return _popped.Task; } }

    internal NavigationPresentation? PresentationOverride { get; private set; }
    internal GameObject Host { get; set; }
    internal CanvasGroup HostGroup { get; set; }
    internal Image PresentationScrim { get; set; }

    internal void SnapshotPageSettings(NavigationPage page)
    {
        Presentation = page.Presentation;
        Opaque = page.Opaque;
        CoveredBehavior = page.CoveredBehavior;
        Transition = page.Transition;

        if (PresentationOverride.HasValue)
            Presentation = PresentationOverride.Value;

        if (Presentation != NavigationPresentation.Page)
            Opaque = false;
    }

    internal void MarkMounted()
    {
        _mounted.TrySetResult(true);
    }

    internal void Complete(object result)
    {
        _popped.TrySetResult(result);
    }

    internal void Fail(Exception exception)
    {
        State = NavigationRouteState.Failed;
        _mounted.TrySetException(exception);
        _popped.TrySetException(exception);
    }

    private static Uri BuildDefinitionUri(string path)
    {
        string value = path == "ui://" ? "ui:///" : path;
        Uri uri;
        if (!Uri.TryCreate(value, UriKind.Absolute, out uri))
            return new Uri("ui:///", UriKind.Absolute);
        return uri;
    }
}

/// <summary>Typed route wrapper whose Page property is exposed as TPage.</summary>
public sealed class NavigationRoute<TPage> : NavigationRoute
    where TPage : NavigationPage
{
    internal NavigationRoute(
        PageDefinition definition,
        object parameters,
        Uri uri = null,
        NavigationPresentation? presentationOverride = null)
        : base(definition, parameters, uri, presentationOverride)
    {
    }

    public new TPage Page
    {
        get { return (TPage)base.Page; }
    }
}
