using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Route presentation used when a page becomes the committed current route.
/// </summary>
/// <remarks>
/// <see cref="NavigationPresentation.Page"/> is ordinary page navigation.
/// <see cref="NavigationPresentation.Modal"/> keeps lower routes visually
/// composed and adds a blocking Navigator-owned scrim.
/// <see cref="NavigationPresentation.Overlay"/> keeps lower routes visually
/// composed without a scrim.
///
/// Dialog is intentionally not a presentation enum value. A
/// <see cref="NavigationDialog"/> is a specialized Modal page contract.
/// </remarks>
public enum NavigationPresentation
{
    /// <summary>Ordinary full-page route presentation.</summary>
    Page,

    /// <summary>Non-opaque route with a blocking modal scrim.</summary>
    Modal,

    /// <summary>Non-opaque route without a modal scrim.</summary>
    Overlay,
}

/// <summary>
/// Processing policy applied to a page while another route covers it.
/// </summary>
/// <remarks>
/// Covered is a navigation-current concept, not a visibility concept. Covered
/// routes never own Cherry navigation-layer UI input.
///
/// Unity has no exact equivalent of Godot Node.ProcessMode. Cherry therefore
/// implements Suspend by temporarily disabling non-uGUI Behaviour components in
/// the covered page subtree while preserving the GameObject and visual uGUI
/// components. Process keeps authored Behaviour enabled states but still blocks
/// UI input through the route host CanvasGroup.
/// </remarks>
public enum CoveredBehavior
{
    /// <summary>Block input and suspend non-uGUI page Behaviours while covered.</summary>
    Suspend,

    /// <summary>Block input but keep page Behaviours processing while covered.</summary>
    Process,
}

/// <summary>Reason supplied when a route enters as committed current.</summary>
public enum NavigationEnterReason
{
    /// <summary>The route entered through Push or another push-style API.</summary>
    Push,

    /// <summary>The route became current by replacing the previous route.</summary>
    Replace,
}

/// <summary>Reason supplied when a route permanently exits Navigator.</summary>
public enum NavigationExitReason
{
    /// <summary>The route left through Pop, MaybePop, PopTo, or PopUntil.</summary>
    Pop,

    /// <summary>The route was replaced by another route.</summary>
    Replace,

    /// <summary>The route was explicitly removed.</summary>
    Remove,

    /// <summary>The route left because the Navigator stack was cleared.</summary>
    Clear,
}

/// <summary>
/// Base <see cref="MonoBehaviour"/> for every Cherry navigable Unity prefab.
/// </summary>
/// <remarks>
/// Navigator instantiates the prefab beneath an inactive route host, injects
/// <see cref="Navigator"/> and <see cref="Route"/>, snapshots authored route
/// presentation settings, optionally awaits a configured callback, and only
/// then activates the host. A configured Push/Replace callback therefore runs
/// before the page's first activation and before <c>Start()</c>.
///
/// Presentation, Opaque, CoveredBehavior, and Transition are authored defaults.
/// Each concrete route snapshots them before configured callbacks run. Mutating
/// the page afterward does not implicitly rewrite that route snapshot; explicit
/// per-navigation overrides belong on <see cref="Route"/>.
/// </remarks>
public abstract class NavigationPage : MonoBehaviour
{
    [SerializeField] private string _navigationPath = "";
    [SerializeField] private NavigationPresentation _presentation =
        NavigationPresentation.Page;
    [SerializeField] private bool _opaque = true;
    [SerializeField] private CoveredBehavior _coveredBehavior =
        CoveredBehavior.Suspend;
    [SerializeField] private NavigationTransition _transition;

    private readonly Dictionary<Behaviour, bool> _coveredBehaviourStates =
        new Dictionary<Behaviour, bool>();

    private GameObject _coveredSelectedObject;
    private bool _coveredPolicyActive;

    /// <summary>
    /// Static prefab navigation identity, for example <c>ui://settings</c>.
    /// </summary>
    /// <remarks>
    /// The editor registry generator reads this from prefab roots. Query and
    /// fragment belong to a concrete NavigationRoute.Uri and should not be
    /// authored into NavigationPath.
    /// </remarks>
    public string NavigationPath
    {
        get { return _navigationPath; }
        set { _navigationPath = value ?? ""; }
    }

    /// <summary>Prefab-authored default route presentation.</summary>
    public NavigationPresentation Presentation
    {
        get { return _presentation; }
        set { _presentation = value; }
    }

    /// <summary>
    /// Whether lower committed routes are hidden from visual composition.
    /// </summary>
    /// <remarks>Modal and Overlay normalize the effective route value to false.</remarks>
    public bool Opaque
    {
        get { return _opaque; }
        set { _opaque = value; }
    }

    /// <summary>Prefab-authored processing policy used while this page is covered.</summary>
    public CoveredBehavior CoveredBehavior
    {
        get { return _coveredBehavior; }
        set { _coveredBehavior = value; }
    }

    /// <summary>
    /// Optional transition asset snapshotted into each concrete route.
    /// </summary>
    /// <remarks>Null falls back to Navigator.DefaultTransition.</remarks>
    public NavigationTransition Transition
    {
        get { return _transition; }
        set { _transition = value; }
    }

    /// <summary>
    /// Navigator that owns this concrete page instance.
    /// </summary>
    /// <remarks>Injected before first activation, so it is available from Start().</remarks>
    public Navigator Navigator { get; internal set; }
    /// <summary>
    /// Concrete route associated with this page instance.
    /// </summary>
    /// <remarks>
    /// Use Route for the exact URI, parameters, effective presentation, route
    /// state, Mounted/Popped tasks, and per-navigation runtime overrides.
    /// </remarks>
    public NavigationRoute Route { get; internal set; }

    /// <summary>Arbitrary runtime payload supplied by parameter-based navigation.</summary>
    /// <remarks>Configured APIs intentionally create routes with null Parameters.</remarks>
    public object Parameters
    {
        get { return Route != null ? Route.Parameters : null; }
    }

    /// <summary>Static PageRegistry definition used to instantiate this route.</summary>
    public PageDefinition Definition
    {
        get { return Route != null ? Route.Definition : null; }
    }

    /// <summary>
    /// Called once when this route becomes the committed current route.
    /// </summary>
    /// <param name="previousRoute">Previous committed current route, or null initially.</param>
    /// <param name="reason">Push or Replace.</param>
    /// <remarks>
    /// Runs after first activation/Start opportunity, after the push transition,
    /// and after commit. At callback time Route.State is Active and
    /// Navigator.CurrentRoute equals Route. This notification is synchronous;
    /// Navigator does not await asynchronous work started by an override.
    /// </remarks>
    protected virtual void OnNavigationEntered(
        NavigationRoute previousRoute,
        NavigationEnterReason reason)
    {
    }

    /// <summary>Called when another route commits above this route.</summary>
    /// <param name="nextRoute">The newly active committed current route.</param>
    /// <remarks>
    /// Covered does not mean hidden: this also fires below a non-opaque Modal or
    /// Overlay. The page instance remains mounted and its CoveredBehavior has
    /// already been applied.
    /// </remarks>
    protected virtual void OnNavigationCovered(NavigationRoute nextRoute)
    {
    }

    /// <summary>Called when this covered route becomes committed current again.</summary>
    /// <param name="removedRoute">The route that directly covered this route.</param>
    /// <remarks>
    /// The outgoing pop transition and removal have completed and authored
    /// processing/input state has been restored. Compound PopTo/PopUntil reveals
    /// only the final target once.
    /// </remarks>
    protected virtual void OnNavigationRevealed(NavigationRoute removedRoute)
    {
    }

    /// <summary>Called exactly once when this route permanently leaves Navigator.</summary>
    /// <param name="result">Value used to complete NavigationRoute.Popped.</param>
    /// <param name="reason">Pop, Replace, Remove, or Clear.</param>
    /// <remarks>
    /// At callback time the route is Disposed, is no longer current, and is
    /// absent from the scheduled stack. This is a synchronous notification.
    /// </remarks>
    protected virtual void OnNavigationExited(
        object result,
        NavigationExitReason reason)
    {
    }

    internal void Inject(Navigator navigator, NavigationRoute route)
    {
        Navigator = navigator;
        Route = route;
    }

    internal void NotifyNavigationEntered(
        NavigationRoute previousRoute,
        NavigationEnterReason reason)
    {
        OnNavigationEntered(previousRoute, reason);
    }

    internal void NotifyNavigationCovered(NavigationRoute nextRoute)
    {
        OnNavigationCovered(nextRoute);
    }

    internal void NotifyNavigationRevealed(NavigationRoute removedRoute)
    {
        OnNavigationRevealed(removedRoute);
    }

    internal void NotifyNavigationExited(
        object result,
        NavigationExitReason reason)
    {
        OnNavigationExited(result, reason);
    }

    /// <summary>
    /// Unity approximation of Cherry's per-page processing policy.
    /// Process keeps scripts enabled; Suspend disables non-uGUI Behaviour
    /// components in the page subtree while retaining visual UI components.
    /// Input ownership is separately blocked by Navigator's route CanvasGroup.
    /// </summary>
    internal void ApplyCoveredBehavior(CoveredBehavior behavior)
    {
        RestoreCoveredBehavior();
        _coveredPolicyActive = true;

        if (EventSystem.current != null)
        {
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            if (selected != null && selected.transform.IsChildOf(transform))
            {
                _coveredSelectedObject = selected;
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        if (behavior != CoveredBehavior.Suspend)
            return;

        Behaviour[] behaviours = GetComponentsInChildren<Behaviour>(true);
        for (int i = 0; i < behaviours.Length; i++)
        {
            Behaviour behaviour = behaviours[i];
            if (behaviour == null || ShouldKeepEnabledForVisuals(behaviour))
                continue;

            _coveredBehaviourStates[behaviour] = behaviour.enabled;
            if (behaviour.enabled)
                behaviour.enabled = false;
        }
    }

    internal void RestoreCoveredBehavior()
    {
        if (!_coveredPolicyActive)
            return;

        foreach (KeyValuePair<Behaviour, bool> pair in _coveredBehaviourStates)
        {
            if (pair.Key != null)
                pair.Key.enabled = pair.Value;
        }

        _coveredBehaviourStates.Clear();
        _coveredPolicyActive = false;

        if (_coveredSelectedObject != null &&
            EventSystem.current != null &&
            _coveredSelectedObject.activeInHierarchy)
        {
            EventSystem.current.SetSelectedGameObject(_coveredSelectedObject);
        }

        _coveredSelectedObject = null;
    }

    private static bool ShouldKeepEnabledForVisuals(Behaviour behaviour)
    {
        if (behaviour is Canvas ||
            behaviour is CanvasGroup ||
            behaviour is GraphicRaycaster)
            return true;

        // uGUI Behaviour components are retained so the covered page can remain
        // visually composed underneath a non-opaque route.
        return behaviour.GetType().Assembly == typeof(Button).Assembly;
    }
}
