using System.Threading.Tasks;
using UnityEngine;

/// <summary>Scales around a configurable pivot, optionally fading at the same time.</summary>
[CreateAssetMenu(fileName = "ScaleNavigationTransition", menuName = "Cherry Navigation/Scale Transition")]
public sealed class ScaleNavigationTransition : TweenNavigationTransition
{
    [SerializeField, Min(0.01f)] private float _hiddenScale = 0.9f;
    [SerializeField, Tooltip("Normalized RectTransform pivot: (0, 0) is bottom left.")]
    private Vector2 _pivotRatio = new Vector2(0.5f, 0.5f);
    [SerializeField] private bool _fade = true;

    /// <summary>Relative scale at the hidden endpoint. Below 1 grows in; above 1 shrinks in.</summary>
    public float HiddenScale
    {
        get { return _hiddenScale; }
        set { _hiddenScale = Mathf.Max(0.01f, value); }
    }

    public Vector2 PivotRatio
    {
        get { return _pivotRatio; }
        set { _pivotRatio = value; }
    }

    public bool Fade
    {
        get { return _fade; }
        set { _fade = value; }
    }

    public ScaleNavigationTransition()
    {
        Duration = 0.24f;
        Easing = CubicEaseOut();
    }

    public override Task PushAsync(NavigationPage incoming, NavigationPage outgoing)
    {
        return AnimateAsync(incoming, true, Vector2.zero, _hiddenScale, _fade, _pivotRatio);
    }

    public override Task PopAsync(NavigationPage outgoing, NavigationPage incoming)
    {
        return AnimateAsync(outgoing, false, Vector2.zero, _hiddenScale, _fade, _pivotRatio);
    }
}
