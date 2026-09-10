using UnityEngine;

/// <summary>A short slide combined with an alpha fade, useful for panels and modals.</summary>
[CreateAssetMenu(fileName = "SlideFadeNavigationTransition", menuName = "Cherry Navigation/Slide Fade Transition")]
public sealed class SlideFadeNavigationTransition : SlideNavigationTransition
{
    public SlideFadeNavigationTransition()
    {
        Duration = 0.22f;
        FromEdge = Edge.Bottom;
        DistanceRatio = 0.08f;
    }

    protected override bool UsesFade { get { return true; } }
}
