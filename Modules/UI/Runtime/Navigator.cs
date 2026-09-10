using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Cherry route-stack Navigator for Unity uGUI.
/// </summary>
/// <remarks>
/// API names and transaction semantics mirror the Godot C# implementation while
/// adapting scene mounting to Unity prefabs, RectTransforms, CanvasGroups, and
/// Tasks.
///
/// Navigator tracks two stacks. ScheduledRoutes is the accepted logical future
/// state and changes synchronously when operations are accepted. Routes is the
/// committed/mounted state and changes only at transaction commit points after
/// configured callbacks and transitions. All structural mutations are serialized
/// through one FIFO operation queue.
/// </remarks>
[DisallowMultipleComponent]
public sealed class Navigator : MonoBehaviour
{
    private sealed class QueuedOperation
    {
        public string Name;
        public Func<Task> Action;
    }

    private readonly List<NavigationRoute> _routes = new List<NavigationRoute>();
    private readonly List<NavigationRoute> _mountedRoutes = new List<NavigationRoute>();
    private readonly Queue<QueuedOperation> _operationQueue =
        new Queue<QueuedOperation>();

    [SerializeField] private NavigationTransition _defaultTransition;
    [SerializeField] private Color _modalScrimColor =
        new Color(0f, 0f, 0f, 0.48f);

    private Navigator _explicitParentNavigator;
    private NavigationRoute _currentRoute;
    private bool _isProcessingOperations;
    private bool _operationRunning;

    /// <summary>Raised when a push-style transaction begins before page preparation.</summary>
    public event Action<NavigationRoute> RoutePushing;
    /// <summary>Raised after an incoming route commits and receives Entered.</summary>
    public event Action<NavigationRoute> RoutePushed;
    /// <summary>Raised when a route begins a pop-style exit; second argument is result.</summary>
    public event Action<NavigationRoute, object> RoutePopping;
    /// <summary>Raised after a popped route exits; second argument is completed result.</summary>
    public event Action<NavigationRoute, object> RoutePopped;
    /// <summary>Raised after Replace commits; old route first, new route second.</summary>
    public event Action<NavigationRoute, NavigationRoute> RouteReplaced;
    /// <summary>Raised after Remove permanently disposes its target route.</summary>
    public event Action<NavigationRoute> RouteRemoved;
    /// <summary>Raised when route preparation or mounting fails.</summary>
    public event Action<NavigationRoute, Exception> RouteFailed;
    /// <summary>Raised when a transition throws while Navigator is processing a route.</summary>
    public event Action<NavigationRoute, Exception> TransitionFailed;
    /// <summary>Raised when one serialized navigation operation begins.</summary>
    public event Action<string> OperationStarted;
    /// <summary>Raised when one serialized navigation operation finishes.</summary>
    public event Action<string> OperationFinished;
    /// <summary>Raised when running-plus-waiting operation count changes.</summary>
    public event Action<int> OperationQueueChanged;

    /// <summary>Fallback transition when a route's effective Transition is null.</summary>
    public NavigationTransition DefaultTransition
    {
        get { return _defaultTransition; }
        set { _defaultTransition = value; }
    }

    /// <summary>
    /// Explicit back-delegation parent override or dynamic nearest ancestor
    /// Navigator fallback.
    /// </summary>
    /// <remarks>
    /// Setting null clears the override. The relation is used by MaybePop only;
    /// structural Pop/Replace/Remove/Clear remain local. Self-links and cycles
    /// are rejected.
    /// </remarks>
    public Navigator ParentNavigator
    {
        get
        {
            if (_explicitParentNavigator != null)
                return _explicitParentNavigator;
            return FindAncestorNavigator();
        }
        set
        {
            if (value == null)
            {
                _explicitParentNavigator = null;
                return;
            }

            if (ReferenceEquals(value, this) || WouldCreateParentCycle(value))
                throw new InvalidOperationException(
                    "Navigator parent relation cannot contain a cycle.");

            _explicitParentNavigator = value;
        }
    }

    /// <summary>Committed navigation-current route, or null.</summary>
    /// <remarks>An incoming route is not CurrentRoute until transition commit.</remarks>
    public NavigationRoute CurrentRoute { get { return _currentRoute; } }

    /// <summary>First committed mounted route, or null.</summary>
    public NavigationRoute FirstRoute
    {
        get { return _mountedRoutes.Count == 0 ? null : _mountedRoutes[0]; }
    }

    /// <summary>Number of committed mounted routes.</summary>
    public int RouteCount { get { return _mountedRoutes.Count; } }

    /// <summary>Route that will be current after all accepted operations finish.</summary>
    public NavigationRoute ScheduledCurrentRoute
    {
        get { return _routes.Count == 0 ? null : _routes[_routes.Count - 1]; }
    }

    /// <summary>Number of routes in the accepted logical future stack.</summary>
    public int ScheduledRouteCount { get { return _routes.Count; } }

    /// <summary>Whether Navigator is currently awaiting a route transition.</summary>
    public bool IsTransitioning { get; private set; }

    /// <summary>Whether an operation is running or waiting in the FIFO queue.</summary>
    public bool IsOperating
    {
        get { return _isProcessingOperations || _operationQueue.Count > 0; }
    }

    /// <summary>Running operation (0 or 1) plus operations still waiting.</summary>
    public int PendingOperationCount
    {
        get { return _operationQueue.Count + (_operationRunning ? 1 : 0); }
    }

    /// <summary>Whether the scheduled local stack can Pop while preserving root.</summary>
    /// <remarks>Does not include whether ParentNavigator can pop.</remarks>
    public bool CanPop { get { return _routes.Count > 1; } }

    /// <summary>Committed mounted stack. Changes only at transaction commit points.</summary>
    public IReadOnlyList<NavigationRoute> Routes { get { return _mountedRoutes; } }
    /// <summary>Accepted logical stack after all already queued operations.</summary>
    public IReadOnlyList<NavigationRoute> ScheduledRoutes { get { return _routes; } }

    /// <summary>Schedules a typed parameter-based Push.</summary>
    /// <returns>The accepted route immediately; await Mounted or Popped for milestones.</returns>
    public NavigationRoute<TPage> Push<TPage>(object parameters = null)
        where TPage : NavigationPage
    {
        PageDefinition definition = PageRegistry.Instance.Resolve<TPage>();
        NavigationRoute<TPage> route =
            new NavigationRoute<TPage>(definition, parameters);

        AcceptPush(
            route,
            null,
            NavigationEnterReason.Push,
            "push");
        return route;
    }

    /// <summary>Schedules a typed configured Push with no Parameters payload.</summary>
    /// <remarks>
    /// Configure is awaited after Navigator/Route injection and route snapshot,
    /// while the route host is inactive and before first Start(). Do not await
    /// another operation on this same Navigator from configure; that operation
    /// queues behind the current transaction and would deadlock.
    /// </remarks>
    public NavigationRoute<TPage> Push<TPage>(Func<TPage, Task> configure)
        where TPage : NavigationPage
    {
        if (configure == null)
            throw new ArgumentNullException("configure");

        PageDefinition definition = PageRegistry.Instance.Resolve<TPage>();
        NavigationRoute<TPage> route =
            new NavigationRoute<TPage>(definition, null);

        AcceptPush(
            route,
            async delegate(NavigationPage page)
            {
                await configure((TPage)page);
            },
            NavigationEnterReason.Push,
            "push_configured");

        return route;
    }

    /// <summary>Pushes TPage and asynchronously returns its eventual route result.</summary>
    public async Task<object> PushAsync<TPage>(object parameters = null)
        where TPage : NavigationPage
    {
        return await Push<TPage>(parameters).Popped;
    }

    /// <summary>Configured typed Push convenience that awaits Route.Popped.</summary>
    public async Task<object> PushAsync<TPage>(Func<TPage, Task> configure)
        where TPage : NavigationPage
    {
        return await Push<TPage>(configure).Popped;
    }

    /// <summary>Parameter-based typed Push convenience that casts the Pop result.</summary>
    public async Task<TResult> PushAsync<TPage, TResult>(object parameters = null)
        where TPage : NavigationPage
    {
        return CastResult<TResult>(await PushAsync<TPage>(parameters));
    }

    /// <summary>Configured typed Push convenience that casts the Pop result.</summary>
    public async Task<TResult> PushAsync<TPage, TResult>(
        Func<TPage, Task> configure)
        where TPage : NavigationPage
    {
        return CastResult<TResult>(await PushAsync<TPage>(configure));
    }

    /// <summary>Schedules dynamic URI/path Push with optional parameters.</summary>
    /// <remarks>
    /// /foo shorthand is canonicalized to ui://foo. Query and fragment remain on
    /// NavigationRoute.Uri but never participate in PageRegistry identity lookup.
    /// </remarks>
    public NavigationRoute Push(string path, object parameters = null)
    {
        string identity;
        Uri uri;
        ParseNavigationAddressOrThrow(path, out identity, out uri);

        PageDefinition definition = PageRegistry.Instance.Resolve(identity);
        NavigationRoute route =
            new NavigationRoute(definition, parameters, uri);

        AcceptPush(
            route,
            null,
            NavigationEnterReason.Push,
            "push");
        return route;
    }

    /// <summary>Schedules dynamic configured URI/path Push with null Parameters.</summary>
    public NavigationRoute Push(
        string path,
        Func<NavigationPage, Task> configure)
    {
        if (configure == null)
            throw new ArgumentNullException("configure");

        string identity;
        Uri uri;
        ParseNavigationAddressOrThrow(path, out identity, out uri);

        PageDefinition definition = PageRegistry.Instance.Resolve(identity);
        NavigationRoute route =
            new NavigationRoute(definition, null, uri);

        AcceptPush(
            route,
            configure,
            NavigationEnterReason.Push,
            "push_configured");
        return route;
    }

    /// <summary>Dynamic parameter Push convenience that awaits Popped.</summary>
    public async Task<object> PushAsync(string path, object parameters = null)
    {
        return await Push(path, parameters).Popped;
    }

    /// <summary>Dynamic configured Push convenience that awaits Popped.</summary>
    public async Task<object> PushAsync(
        string path,
        Func<NavigationPage, Task> configure)
    {
        return await Push(path, configure).Popped;
    }

    /// <summary>Pushes a caller-supplied PageDefinition with optional parameters.</summary>
    public NavigationRoute PushDefinition(
        PageDefinition definition,
        object parameters = null)
    {
        ValidateDefinition(definition);

        NavigationRoute route =
            new NavigationRoute(definition, parameters);

        AcceptPush(
            route,
            null,
            NavigationEnterReason.Push,
            "push_definition");
        return route;
    }

    /// <summary>Configured Push for a caller-supplied PageDefinition.</summary>
    public NavigationRoute PushDefinition(
        PageDefinition definition,
        Func<NavigationPage, Task> configure)
    {
        ValidateDefinition(definition);
        if (configure == null)
            throw new ArgumentNullException("configure");

        NavigationRoute route =
            new NavigationRoute(definition, null);

        AcceptPush(
            route,
            configure,
            NavigationEnterReason.Push,
            "push_definition_configured");
        return route;
    }

    /// <summary>Pushes TPage with a route-local Modal presentation override.</summary>
    /// <remarks>Modal is non-opaque and receives a blocking Navigator-owned scrim.</remarks>
    public NavigationRoute<TPage> ShowModal<TPage>(object parameters = null)
        where TPage : NavigationPage
    {
        return PushWithPresentation<TPage>(
            NavigationPresentation.Modal,
            parameters,
            null,
            "show_modal");
    }

    /// <summary>Configured typed Modal; configure runs before first activation.</summary>
    public NavigationRoute<TPage> ShowModal<TPage>(
        Func<TPage, Task> configure)
        where TPage : NavigationPage
    {
        if (configure == null)
            throw new ArgumentNullException("configure");

        return PushWithPresentation<TPage>(
            NavigationPresentation.Modal,
            null,
            async delegate(NavigationPage page)
            {
                await configure((TPage)page);
            },
            "show_modal_configured");
    }

    /// <summary>Dynamic URI Modal with optional parameters.</summary>
    public NavigationRoute ShowModal(string uri, object parameters = null)
    {
        return PushWithPresentation(
            uri,
            NavigationPresentation.Modal,
            parameters,
            null,
            "show_modal");
    }

    /// <summary>Dynamic configured Modal with null Parameters.</summary>
    public NavigationRoute ShowModal(
        string uri,
        Func<NavigationPage, Task> configure)
    {
        return PushWithPresentation(
            uri,
            NavigationPresentation.Modal,
            null,
            configure,
            "show_modal_configured");
    }

    /// <summary>Pushes TPage as a non-opaque Overlay without a modal scrim.</summary>
    /// <remarks>
    /// Overlay means visual composition, not click-through: covered routes remain
    /// input-blocked by Cherry's navigation policy.
    /// </remarks>
    public NavigationRoute<TPage> ShowOverlay<TPage>(object parameters = null)
        where TPage : NavigationPage
    {
        return PushWithPresentation<TPage>(
            NavigationPresentation.Overlay,
            parameters,
            null,
            "show_overlay");
    }

    /// <summary>Configured typed Overlay; configure runs before first activation.</summary>
    public NavigationRoute<TPage> ShowOverlay<TPage>(
        Func<TPage, Task> configure)
        where TPage : NavigationPage
    {
        if (configure == null)
            throw new ArgumentNullException("configure");

        return PushWithPresentation<TPage>(
            NavigationPresentation.Overlay,
            null,
            async delegate(NavigationPage page)
            {
                await configure((TPage)page);
            },
            "show_overlay_configured");
    }

    /// <summary>Dynamic URI Overlay with optional parameters.</summary>
    public NavigationRoute ShowOverlay(string uri, object parameters = null)
    {
        return PushWithPresentation(
            uri,
            NavigationPresentation.Overlay,
            parameters,
            null,
            "show_overlay");
    }

    /// <summary>Dynamic configured Overlay with null Parameters.</summary>
    public NavigationRoute ShowOverlay(
        string uri,
        Func<NavigationPage, Task> configure)
    {
        return PushWithPresentation(
            uri,
            NavigationPresentation.Overlay,
            null,
            configure,
            "show_overlay_configured");
    }

    /// <summary>Shows Cherry's built-in DefaultDialog as a specialized Modal route.</summary>
    /// <remarks>
    /// Its PageDefinition is created in memory and never enters the generated user
    /// registry. Dialog requires an existing route underneath and closes through
    /// ordinary Pop/result semantics.
    /// </remarks>
    public NavigationRoute<DefaultDialog> ShowDialog(
        Func<DefaultDialog, Task> configure)
    {
        if (configure == null)
            throw new ArgumentNullException("configure");
        EnsureDialogBaseRoute();

        PageDefinition definition = PageDefinition.CreateRuntime(
            "ui://__cherry/default-dialog",
            delegate(Transform parent)
            {
                GameObject go = new GameObject(
                    "DefaultDialog",
                    typeof(RectTransform),
                    typeof(DefaultDialog));
                go.transform.SetParent(parent, false);
                return go.GetComponent<DefaultDialog>();
            });

        NavigationRoute<DefaultDialog> route =
            new NavigationRoute<DefaultDialog>(
                definition,
                null,
                null,
                NavigationPresentation.Modal);

        AcceptPush(
            route,
            async delegate(NavigationPage page)
            {
                await configure((DefaultDialog)page);
            },
            NavigationEnterReason.Push,
            "show_dialog");

        return route;
    }

    /// <summary>Shows a registered custom NavigationDialog prefab as a Modal route.</summary>
    /// <remarks>Dialog is not a separate NavigationPresentation enum value.</remarks>
    public NavigationRoute<TDialog> ShowDialog<TDialog>(
        Func<TDialog, Task> configure)
        where TDialog : NavigationDialog
    {
        if (configure == null)
            throw new ArgumentNullException("configure");
        EnsureDialogBaseRoute();

        PageDefinition definition = PageRegistry.Instance.Resolve<TDialog>();
        NavigationRoute<TDialog> route =
            new NavigationRoute<TDialog>(
                definition,
                null,
                null,
                NavigationPresentation.Modal);

        AcceptPush(
            route,
            async delegate(NavigationPage page)
            {
                await configure((TDialog)page);
            },
            NavigationEnterReason.Push,
            "show_dialog");

        return route;
    }

    /// <summary>Schedules a local structural Pop of the scheduled current route.</summary>
    /// <returns>False when the local scheduled stack cannot pop.</returns>
    /// <remarks>Pop does not consult PopScope and never propagates to ParentNavigator.</remarks>
    public bool Pop(object result = null)
    {
        if (_routes.Count <= 1)
            return false;

        NavigationRoute route = _routes[_routes.Count - 1];
        _routes.RemoveAt(_routes.Count - 1);
        route.State = NavigationRouteState.Popping;

        EnqueueOperation(
            "pop",
            delegate
            {
                return ExecutePop(
                    route,
                    result,
                    NavigationExitReason.Pop,
                    true);
            });

        return true;
    }

    /// <summary>
    /// Back-policy entry point that evaluates local PopScopes and may propagate
    /// to ParentNavigator when the local stack cannot pop.
    /// </summary>
    /// <remarks>
    /// An operating Navigator returns false rather than skipping an in-flight
    /// transaction. Local PopScope denial blocks propagation. PopScope discovery
    /// stops at descendant Navigator boundaries.
    /// </remarks>
    public bool MaybePop(object result = null)
    {
        if (IsOperating)
            return false;

        List<PopScope> scopes = CollectLocalPopScopes();
        for (int i = 0; i < scopes.Count; i++)
        {
            if (!scopes[i].CanPop)
            {
                NotifyScopes(scopes, false, result);
                return false;
            }
        }

        if (CanPop)
        {
            bool accepted = Pop(result);
            NotifyScopes(scopes, accepted, result);
            return accepted;
        }

        Navigator parent = ParentNavigator;
        if (parent == null)
        {
            NotifyScopes(scopes, false, result);
            return false;
        }

        bool delegated = parent.MaybePop(result);
        NotifyScopes(scopes, delegated, result);
        return delegated;
    }

    /// <summary>Replaces scheduled current with typed TPage.</summary>
    /// <remarks>The old route remains committed current until incoming transition commit.</remarks>
    public NavigationRoute<TPage> Replace<TPage>(
        object parameters = null,
        object oldResult = null)
        where TPage : NavigationPage
    {
        PageDefinition definition = PageRegistry.Instance.Resolve<TPage>();
        NavigationRoute<TPage> route =
            new NavigationRoute<TPage>(definition, parameters);

        AcceptReplace(route, null, oldResult, "replace");
        return route;
    }

    /// <summary>Configured typed Replace with null incoming Parameters.</summary>
    /// <remarks>On an empty stack Replace falls back to configured Push semantics.</remarks>
    public NavigationRoute<TPage> Replace<TPage>(
        Func<TPage, Task> configure,
        object oldResult = null)
        where TPage : NavigationPage
    {
        if (configure == null)
            throw new ArgumentNullException("configure");

        PageDefinition definition = PageRegistry.Instance.Resolve<TPage>();
        NavigationRoute<TPage> route =
            new NavigationRoute<TPage>(definition, null);

        AcceptReplace(
            route,
            async delegate(NavigationPage page)
            {
                await configure((TPage)page);
            },
            oldResult,
            "replace_configured");

        return route;
    }

    /// <summary>Dynamic URI/path Replace with parameters and old-route result.</summary>
    public NavigationRoute Replace(
        string path,
        object parameters = null,
        object oldResult = null)
    {
        string identity;
        Uri uri;
        ParseNavigationAddressOrThrow(path, out identity, out uri);

        PageDefinition definition = PageRegistry.Instance.Resolve(identity);
        NavigationRoute route =
            new NavigationRoute(definition, parameters, uri);

        AcceptReplace(route, null, oldResult, "replace");
        return route;
    }

    /// <summary>Dynamic configured Replace with null incoming Parameters.</summary>
    public NavigationRoute Replace(
        string path,
        Func<NavigationPage, Task> configure,
        object oldResult = null)
    {
        if (configure == null)
            throw new ArgumentNullException("configure");

        string identity;
        Uri uri;
        ParseNavigationAddressOrThrow(path, out identity, out uri);

        PageDefinition definition = PageRegistry.Instance.Resolve(identity);
        NavigationRoute route =
            new NavigationRoute(definition, null, uri);

        AcceptReplace(route, configure, oldResult, "replace_configured");
        return route;
    }

    /// <summary>Pops all routes above the nearest scheduled route matching predicate.</summary>
    /// <remarks>Compound pop exits top-to-bottom and reveals the final target once.</remarks>
    public int PopUntil(Predicate<NavigationRoute> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException("predicate");

        int targetIndex = -1;
        for (int i = _routes.Count - 1; i >= 0; i--)
        {
            if (predicate(_routes[i]))
            {
                targetIndex = i;
                break;
            }
        }

        if (targetIndex < 0)
            return 0;

        return AcceptCompoundPop(targetIndex);
    }

    /// <summary>Pops back to a route identity resolved from URI-shaped input.</summary>
    /// <remarks>Query and fragment are ignored for target identity.</remarks>
    public int PopTo(string path)
    {
        string identity;
        Uri uri;
        if (!TryParseNavigationAddress(path, out identity, out uri))
            return 0;

        return PopUntil(delegate(NavigationRoute route)
        {
            return route.Path == identity;
        });
    }

    /// <summary>Pops back to the nearest scheduled route whose definition is TPage.</summary>
    public int PopTo<TPage>() where TPage : NavigationPage
    {
        return PopUntil(delegate(NavigationRoute route)
        {
            return route.Definition != null &&
                   route.Definition.MatchesPageType<TPage>();
        });
    }

    /// <summary>Schedules removal of one specific local route.</summary>
    /// <remarks>Removing covered does not reveal; removing current reveals underlying.</remarks>
    public bool Remove(NavigationRoute route, object result = null)
    {
        if (route == null)
            return false;

        int index = _routes.IndexOf(route);
        if (index < 0)
            return false;

        _routes.RemoveAt(index);
        route.State = NavigationRouteState.Popping;

        EnqueueOperation(
            "remove",
            delegate
            {
                return ExecuteRemove(route, result);
            });

        return true;
    }

    /// <summary>Schedules disposal of the entire local route stack.</summary>
    /// <remarks>Clear exits top-to-bottom and emits no Revealed callback.</remarks>
    public int Clear(object result = null)
    {
        if (_routes.Count == 0)
            return 0;

        List<NavigationRoute> removed =
            new List<NavigationRoute>(_routes);
        _routes.Clear();

        for (int i = 0; i < removed.Count; i++)
            removed[i].State = NavigationRouteState.Popping;

        EnqueueOperation(
            "clear",
            delegate
            {
                return ExecuteClear(removed, result);
            });

        return removed.Count;
    }

    private NavigationRoute<TPage> PushWithPresentation<TPage>(
        NavigationPresentation presentation,
        object parameters,
        Func<NavigationPage, Task> configure,
        string operationName)
        where TPage : NavigationPage
    {
        PageDefinition definition = PageRegistry.Instance.Resolve<TPage>();
        NavigationRoute<TPage> route =
            new NavigationRoute<TPage>(
                definition,
                parameters,
                null,
                presentation);

        AcceptPush(
            route,
            configure,
            NavigationEnterReason.Push,
            operationName);

        return route;
    }

    private NavigationRoute PushWithPresentation(
        string path,
        NavigationPresentation presentation,
        object parameters,
        Func<NavigationPage, Task> configure,
        string operationName)
    {
        if (configure == null &&
            (operationName.EndsWith("_configured", StringComparison.Ordinal)))
        {
            throw new ArgumentNullException("configure");
        }

        string identity;
        Uri uri;
        ParseNavigationAddressOrThrow(path, out identity, out uri);

        PageDefinition definition = PageRegistry.Instance.Resolve(identity);
        NavigationRoute route =
            new NavigationRoute(
                definition,
                parameters,
                uri,
                presentation);

        AcceptPush(
            route,
            configure,
            NavigationEnterReason.Push,
            operationName);

        return route;
    }

    // Accept scheduled state immediately, then serialize preparation/transition/
    // commit through the operation queue. This preserves logical vs committed state.
    private void AcceptPush(
        NavigationRoute route,
        Func<NavigationPage, Task> configure,
        NavigationEnterReason enterReason,
        string operationName)
    {
        ValidateDefinition(route.Definition);

        _routes.Add(route);
        EnqueueOperation(
            operationName,
            delegate
            {
                return ExecutePush(route, configure, enterReason);
            });
    }

    // Push transaction: prepare inactive host/page -> configure -> activate ->
    // transition -> commit mounted/current -> cover previous -> enter incoming.
    private async Task ExecutePush(
        NavigationRoute route,
        Func<NavigationPage, Task> configure,
        NavigationEnterReason enterReason)
    {
        NavigationRoute previous = _currentRoute;

        try
        {
            route.State = NavigationRouteState.Pushing;
            route.Navigator = this;
            Raise(RoutePushing, route);

            await PrepareRoute(route, configure);

            SetAllCommittedInput(false);
            SetRouteInput(route, false);

            await RunPushTransition(route, previous);

            _mountedRoutes.Add(route);
            _currentRoute = route;

            if (previous != null)
            {
                previous.State = NavigationRouteState.Covered;
                SetRouteInput(previous, false);
                previous.Page.ApplyCoveredBehavior(previous.CoveredBehavior);
                previous.Page.NotifyNavigationCovered(route);
            }

            route.State = NavigationRouteState.Active;
            SetRouteInput(route, true);
            RecomputeComposition();

            route.Page.NotifyNavigationEntered(previous, enterReason);
            route.MarkMounted();
            Raise(RoutePushed, route);
        }
        catch (Exception exception)
        {
            _routes.Remove(route);
            DestroyRouteHost(route);
            FailRoute(route, exception);
        }
    }

    // Replace changes scheduled current immediately while old committed current
    // remains visible/current until the incoming transition finishes.
    private void AcceptReplace(
        NavigationRoute route,
        Func<NavigationPage, Task> configure,
        object oldResult,
        string operationName)
    {
        ValidateDefinition(route.Definition);

        if (_routes.Count == 0)
        {
            AcceptPush(
                route,
                configure,
                NavigationEnterReason.Push,
                operationName);
            return;
        }

        NavigationRoute oldLogical = _routes[_routes.Count - 1];
        _routes[_routes.Count - 1] = route;

        EnqueueOperation(
            operationName,
            delegate
            {
                return ExecuteReplace(
                    oldLogical,
                    route,
                    configure,
                    oldResult);
            });
    }

    private async Task ExecuteReplace(
        NavigationRoute oldRoute,
        NavigationRoute newRoute,
        Func<NavigationPage, Task> configure,
        object oldResult)
    {
        try
        {
            newRoute.State = NavigationRouteState.Pushing;
            newRoute.Navigator = this;
            Raise(RoutePushing, newRoute);

            await PrepareRoute(newRoute, configure);

            SetAllCommittedInput(false);
            SetRouteInput(newRoute, false);

            await RunPushTransition(newRoute, oldRoute);

            int oldIndex = _mountedRoutes.IndexOf(oldRoute);
            if (oldIndex >= 0)
                _mountedRoutes[oldIndex] = newRoute;
            else
                _mountedRoutes.Add(newRoute);

            _currentRoute = newRoute;

            oldRoute.Page.RestoreCoveredBehavior();
            DeactivateAndDetach(oldRoute);
            oldRoute.State = NavigationRouteState.Disposed;
            oldRoute.Page.NotifyNavigationExited(
                oldResult,
                NavigationExitReason.Replace);
            oldRoute.Complete(oldResult);

            newRoute.State = NavigationRouteState.Active;
            SetRouteInput(newRoute, true);
            RecomputeComposition();

            newRoute.Page.NotifyNavigationEntered(
                oldRoute,
                NavigationEnterReason.Replace);
            newRoute.MarkMounted();

            Raise(RouteReplaced, oldRoute, newRoute);
            Raise(RoutePushed, newRoute);

            DestroyRouteHost(oldRoute);
        }
        catch (Exception exception)
        {
            int logicalIndex = _routes.IndexOf(newRoute);
            if (logicalIndex >= 0)
                _routes[logicalIndex] = oldRoute;

            DestroyRouteHost(newRoute);
            FailRoute(newRoute, exception);
        }
    }

    private async Task ExecutePop(
        NavigationRoute route,
        object result,
        NavigationExitReason reason,
        bool revealIncoming)
    {
        Raise(RoutePopping, route, result);

        NavigationRoute incoming = null;
        int routeIndex = _mountedRoutes.IndexOf(route);
        if (routeIndex > 0)
            incoming = _mountedRoutes[routeIndex - 1];

        SetAllCommittedInput(false);

        if (incoming != null)
            SetRouteVisible(incoming, true);

        await RunPopTransition(route, incoming);

        _mountedRoutes.Remove(route);
        route.Page.RestoreCoveredBehavior();
        DeactivateAndDetach(route);
        route.State = NavigationRouteState.Disposed;
        route.Page.NotifyNavigationExited(result, reason);

        _currentRoute = _mountedRoutes.Count == 0
            ? null
            : _mountedRoutes[_mountedRoutes.Count - 1];

        if (revealIncoming && incoming != null)
        {
            incoming.Page.RestoreCoveredBehavior();
            incoming.State = NavigationRouteState.Active;
            SetRouteInput(incoming, true);
            RecomputeComposition();
            incoming.Page.NotifyNavigationRevealed(route);
        }
        else
        {
            RecomputeComposition();
            if (_currentRoute != null)
                SetRouteInput(_currentRoute, true);
        }

        route.Complete(result);
        Raise(RoutePopped, route, result);
        DestroyRouteHost(route);
    }

    private int AcceptCompoundPop(int targetIndex)
    {
        int count = _routes.Count - targetIndex - 1;
        if (count <= 0)
            return 0;

        List<NavigationRoute> removed = _routes.GetRange(
            targetIndex + 1,
            count);

        _routes.RemoveRange(targetIndex + 1, count);

        for (int i = 0; i < removed.Count; i++)
            removed[i].State = NavigationRouteState.Popping;

        EnqueueOperation(
            "pop_until",
            delegate
            {
                return ExecuteCompoundPop(removed);
            });

        return count;
    }

    private async Task ExecuteCompoundPop(List<NavigationRoute> removed)
    {
        if (removed.Count == 0)
            return;

        NavigationRoute topOutgoing = removed[removed.Count - 1];
        NavigationRoute finalIncoming = null;
        int bottomIndex = _mountedRoutes.IndexOf(removed[0]);

        if (bottomIndex > 0)
            finalIncoming = _mountedRoutes[bottomIndex - 1];

        SetAllCommittedInput(false);
        if (finalIncoming != null)
            SetRouteVisible(finalIncoming, true);

        await RunPopTransition(topOutgoing, finalIncoming);

        for (int i = removed.Count - 1; i >= 0; i--)
        {
            NavigationRoute route = removed[i];
            _mountedRoutes.Remove(route);
            route.Page.RestoreCoveredBehavior();
            DeactivateAndDetach(route);
            route.State = NavigationRouteState.Disposed;
            route.Page.NotifyNavigationExited(
                null,
                NavigationExitReason.Pop);
            route.Complete(null);
            Raise(RoutePopped, route, (object)null);
            DestroyRouteHost(route);
        }

        _currentRoute = _mountedRoutes.Count == 0
            ? null
            : _mountedRoutes[_mountedRoutes.Count - 1];

        if (finalIncoming != null)
        {
            finalIncoming.Page.RestoreCoveredBehavior();
            finalIncoming.State = NavigationRouteState.Active;
            SetRouteInput(finalIncoming, true);
            RecomputeComposition();
            finalIncoming.Page.NotifyNavigationRevealed(removed[0]);
        }
    }

    private async Task ExecuteRemove(NavigationRoute route, object result)
    {
        int index = _mountedRoutes.IndexOf(route);
        bool wasCurrent = ReferenceEquals(_currentRoute, route);

        if (index < 0)
        {
            route.Complete(result);
            return;
        }

        if (wasCurrent)
        {
            await ExecutePop(
                route,
                result,
                NavigationExitReason.Remove,
                true);
            Raise(RouteRemoved, route);
            return;
        }

        _mountedRoutes.RemoveAt(index);
        route.Page.RestoreCoveredBehavior();
        DeactivateAndDetach(route);
        route.State = NavigationRouteState.Disposed;
        route.Page.NotifyNavigationExited(
            result,
            NavigationExitReason.Remove);
        route.Complete(result);
        Raise(RouteRemoved, route);
        RecomputeComposition();
        DestroyRouteHost(route);
    }

    private async Task ExecuteClear(
        List<NavigationRoute> removed,
        object result)
    {
        if (removed.Count == 0)
            return;

        NavigationRoute top = _currentRoute;
        SetAllCommittedInput(false);

        if (top != null)
            await RunPopTransition(top, null);

        for (int i = removed.Count - 1; i >= 0; i--)
        {
            NavigationRoute route = removed[i];
            _mountedRoutes.Remove(route);

            if (route.Page != null)
            {
                route.Page.RestoreCoveredBehavior();
                DeactivateAndDetach(route);
                route.State = NavigationRouteState.Disposed;
                route.Page.NotifyNavigationExited(
                    result,
                    NavigationExitReason.Clear);
            }

            route.Complete(result);
            Raise(RoutePopped, route, result);
            DestroyRouteHost(route);
        }

        _currentRoute = null;
    }

    // Route preparation intentionally occurs before first page activation:
    // instantiate -> inject Navigator/Route -> snapshot -> await configure -> activate.
    private async Task PrepareRoute(
        NavigationRoute route,
        Func<NavigationPage, Task> configure)
    {
        GameObject host = new GameObject(
            "RouteHost:" + route.Path,
            typeof(RectTransform),
            typeof(CanvasGroup));

        host.transform.SetParent(transform, false);
        host.SetActive(false);

        RectTransform hostRect = host.GetComponent<RectTransform>();
        Stretch(hostRect);

        CanvasGroup group = host.GetComponent<CanvasGroup>();
        group.alpha = 1f;
        group.interactable = false;
        group.blocksRaycasts = false;

        route.Host = host;
        route.HostGroup = group;

        NavigationPage page = route.Definition.InstantiatePage(host.transform);
        if (page == null)
            throw new InvalidOperationException(
                "PageDefinition did not create a NavigationPage: " + route.Path);

        route.Page = page;
        page.Inject(this, route);
        route.SnapshotPageSettings(page);

        RectTransform pageRect = page.transform as RectTransform;
        if (pageRect != null)
            Stretch(pageRect);

        if (route.Presentation == NavigationPresentation.Modal)
            CreateModalScrim(route);

        if (configure != null)
            await configure(page);

        host.SetActive(true);

        // One player-loop turn gives newly activated page components their
        // normal Unity Start phase before route transition/commit.
        await Task.Yield();
    }

    private void CreateModalScrim(NavigationRoute route)
    {
        GameObject scrimObject = new GameObject(
            "ModalScrim",
            typeof(RectTransform),
            typeof(Image));

        RectTransform rect = scrimObject.GetComponent<RectTransform>();
        rect.SetParent(route.Host.transform, false);
        Stretch(rect);
        rect.SetAsFirstSibling();

        Image image = scrimObject.GetComponent<Image>();
        image.color = _modalScrimColor;
        image.raycastTarget = true;
        route.PresentationScrim = image;
    }

    private async Task RunPushTransition(
        NavigationRoute route,
        NavigationRoute previous)
    {
        NavigationTransition transition =
            route.Transition != null ? route.Transition : _defaultTransition;

        if (transition == null)
            return;

        IsTransitioning = true;
        try
        {
            await transition.PushAsync(
                route.Page,
                previous != null ? previous.Page : null);
        }
        catch (Exception exception)
        {
            Raise(TransitionFailed, route, exception);
            Debug.LogException(exception, this);
        }
        finally
        {
            IsTransitioning = false;
        }
    }

    private async Task RunPopTransition(
        NavigationRoute route,
        NavigationRoute incoming)
    {
        NavigationTransition transition =
            route.Transition != null ? route.Transition : _defaultTransition;

        if (transition == null)
            return;

        IsTransitioning = true;
        try
        {
            await transition.PopAsync(
                route.Page,
                incoming != null ? incoming.Page : null);
        }
        catch (Exception exception)
        {
            Raise(TransitionFailed, route, exception);
            Debug.LogException(exception, this);
        }
        finally
        {
            IsTransitioning = false;
        }
    }

    private void RecomputeComposition()
    {
        bool visible = true;

        for (int i = _mountedRoutes.Count - 1; i >= 0; i--)
        {
            NavigationRoute route = _mountedRoutes[i];
            SetRouteVisible(route, visible);

            if (visible && route.Opaque)
                visible = false;
        }
    }

    private static void SetRouteVisible(NavigationRoute route, bool visible)
    {
        if (route != null && route.HostGroup != null)
            route.HostGroup.alpha = visible ? 1f : 0f;
    }

    private static void SetRouteInput(NavigationRoute route, bool enabled)
    {
        if (route == null || route.HostGroup == null)
            return;

        route.HostGroup.interactable = enabled;
        route.HostGroup.blocksRaycasts = enabled;
    }

    private void SetAllCommittedInput(bool enabled)
    {
        for (int i = 0; i < _mountedRoutes.Count; i++)
            SetRouteInput(_mountedRoutes[i], enabled);
    }

    private static void DeactivateAndDetach(NavigationRoute route)
    {
        if (route == null || route.Host == null)
            return;

        route.Host.SetActive(false);
        route.Host.transform.SetParent(null, false);
    }

    private static void DestroyRouteHost(NavigationRoute route)
    {
        if (route == null || route.Host == null)
            return;

        UnityEngine.Object.Destroy(route.Host);
        route.Host = null;
        route.HostGroup = null;
        route.PresentationScrim = null;
    }

    // All stack mutations share this FIFO queue. _operationRunning is separate
    // from queue draining so PendingOperationCount never double-counts the front item.
    private void EnqueueOperation(string name, Func<Task> action)
    {
        _operationQueue.Enqueue(new QueuedOperation
        {
            Name = name,
            Action = action,
        });

        Raise(OperationQueueChanged, PendingOperationCount);

        if (!_isProcessingOperations)
            ProcessOperationQueue();
    }

    private async void ProcessOperationQueue()
    {
        if (_isProcessingOperations)
            return;

        _isProcessingOperations = true;

        try
        {
            while (_operationQueue.Count > 0)
            {
                QueuedOperation operation = _operationQueue.Dequeue();
                _operationRunning = true;

                Raise(OperationStarted, operation.Name);
                Raise(OperationQueueChanged, PendingOperationCount);

                try
                {
                    await operation.Action();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                }
                finally
                {
                    _operationRunning = false;
                    Raise(OperationFinished, operation.Name);
                    Raise(OperationQueueChanged, PendingOperationCount);
                }
            }
        }
        finally
        {
            _isProcessingOperations = false;
        }
    }

    private List<PopScope> CollectLocalPopScopes()
    {
        List<PopScope> result = new List<PopScope>();
        if (_currentRoute == null || _currentRoute.Page == null)
            return result;

        CollectLocalPopScopesRecursive(
            _currentRoute.Page.transform,
            result,
            true);

        return result;
    }

    private static void CollectLocalPopScopesRecursive(
        Transform node,
        List<PopScope> output,
        bool isRoot)
    {
        if (!isRoot)
        {
            Navigator nested = node.GetComponent<Navigator>();
            if (nested != null)
                return;
        }

        PopScope[] scopes = node.GetComponents<PopScope>();
        for (int i = 0; i < scopes.Length; i++)
            output.Add(scopes[i]);

        for (int i = 0; i < node.childCount; i++)
        {
            CollectLocalPopScopesRecursive(
                node.GetChild(i),
                output,
                false);
        }
    }

    private static void NotifyScopes(
        List<PopScope> scopes,
        bool didPop,
        object result)
    {
        for (int i = 0; i < scopes.Count; i++)
            scopes[i].Notify(didPop, result);
    }

    private Navigator FindAncestorNavigator()
    {
        Transform cursor = transform.parent;
        while (cursor != null)
        {
            Navigator navigator = cursor.GetComponent<Navigator>();
            if (navigator != null)
                return navigator;
            cursor = cursor.parent;
        }

        return null;
    }

    private bool WouldCreateParentCycle(Navigator candidate)
    {
        Navigator cursor = candidate;
        HashSet<Navigator> seen = new HashSet<Navigator>();

        while (cursor != null && seen.Add(cursor))
        {
            if (ReferenceEquals(cursor, this))
                return true;
            cursor = cursor.ParentNavigator;
        }

        return false;
    }

    private void EnsureDialogBaseRoute()
    {
        if (_routes.Count == 0)
            throw new InvalidOperationException(
                "ShowDialog requires an existing route beneath the dialog.");
    }

    private static void ValidateDefinition(PageDefinition definition)
    {
        if (definition == null)
            throw new ArgumentNullException("definition");

        if (string.IsNullOrEmpty(definition.Path))
            throw new InvalidOperationException(
                "PageDefinition has an invalid navigation path.");

        if (definition.Scene == null &&
            !definition.Path.StartsWith(
                "ui://__cherry/",
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "PageDefinition has no NavigationPage prefab: " +
                definition.Path);
        }
    }

    private void FailRoute(NavigationRoute route, Exception exception)
    {
        route.Fail(exception);
        Raise(RouteFailed, route, exception);
        Debug.LogException(exception, this);
    }

    private static TResult CastResult<TResult>(object value)
    {
        if (value == null)
            return default(TResult);

        if (value is TResult)
            return (TResult)value;

        throw new InvalidCastException(
            "Navigation result is " + value.GetType().FullName +
            ", not " + typeof(TResult).FullName + ".");
    }

    // Parse the full concrete System.Uri first, then remove Query/Fragment only
    // for registry identity. The full URI remains stored on NavigationRoute.
    private static void ParseNavigationAddressOrThrow(
        string value,
        out string identity,
        out Uri uri)
    {
        if (!TryParseNavigationAddress(value, out identity, out uri))
            throw new UriFormatException(
                "Invalid Cherry navigation URI: " + value);
    }

    private static bool TryParseNavigationAddress(
        string value,
        out string identity,
        out Uri uri)
    {
        identity = "";
        uri = null;

        string raw = (value ?? "").Trim();
        if (raw.Length == 0)
            return false;

        string canonical;
        if (raw.StartsWith("/", StringComparison.Ordinal))
        {
            canonical = raw == "/"
                ? "ui:///"
                : "ui://" + raw.Substring(1);
        }
        else
        {
            canonical = raw;
        }

        if (canonical == "ui://")
            canonical = "ui:///";

        Uri parsed;
        if (!Uri.TryCreate(canonical, UriKind.Absolute, out parsed))
            return false;

        if (!string.Equals(parsed.Scheme, "ui", StringComparison.OrdinalIgnoreCase))
            return false;

        int queryIndex = canonical.IndexOf('?');
        int fragmentIndex = canonical.IndexOf('#');

        int end = canonical.Length;
        if (queryIndex >= 0)
            end = Math.Min(end, queryIndex);
        if (fragmentIndex >= 0)
            end = Math.Min(end, fragmentIndex);

        string identitySource = canonical.Substring(0, end);
        identity = PageRegistry.NormalizePath(identitySource);

        if (identity.Length == 0)
            return false;

        uri = parsed;
        return true;
    }

    private static void Stretch(RectTransform rect)
    {
        if (rect == null)
            return;

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void Raise(Action<NavigationRoute> action, NavigationRoute route)
    {
        if (action != null)
            action(route);
    }

    private static void Raise(
        Action<NavigationRoute, object> action,
        NavigationRoute route,
        object result)
    {
        if (action != null)
            action(route, result);
    }

    private static void Raise(
        Action<NavigationRoute, NavigationRoute> action,
        NavigationRoute a,
        NavigationRoute b)
    {
        if (action != null)
            action(a, b);
    }

    private static void Raise(
        Action<NavigationRoute, Exception> action,
        NavigationRoute route,
        Exception exception)
    {
        if (action != null)
            action(route, exception);
    }

    private static void Raise(Action<string> action, string value)
    {
        if (action != null)
            action(value);
    }

    private static void Raise(Action<int> action, int value)
    {
        if (action != null)
            action(value);
    }
}
