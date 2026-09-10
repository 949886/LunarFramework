using System.Threading.Tasks;
using UnityEngine;

/// <summary>Slides in from a page edge and exits toward the same edge on Pop.</summary>
[CreateAssetMenu(fileName = "SlideNavigationTransition", menuName = "Cherry Navigation/Slide Transition")]
public class SlideNavigationTransition : TweenNavigationTransition
{
    public enum Edge { Left, Right, Top, Bottom }

    [SerializeField] private Edge _fromEdge = Edge.Right;
    [SerializeField, Min(0f)] private float _distanceRatio = 1f;

    public Edge FromEdge
    {
        get { return _fromEdge; }
        set { _fromEdge = value; }
    }

    /// <summary>Travel as a fraction of the page width/height (1 = one full page).</summary>
    public float DistanceRatio
    {
        get { return _distanceRatio; }
        set { _distanceRatio = Mathf.Max(0f, value); }
    }

    public SlideNavigationTransition()
    {
        Duration = 0.28f;
        Easing = CubicEaseOut();
    }

    protected virtual bool UsesFade { get { return false; } }

    public override Task PushAsync(NavigationPage incoming, NavigationPage outgoing)
    {
        return incoming == null || Duration <= 0f ? Task.CompletedTask :
            AnimateAsync(incoming, true, GetOffset(incoming), 1f, UsesFade);
    }

    public override Task PopAsync(NavigationPage outgoing, NavigationPage incoming)
    {
        return outgoing == null || Duration <= 0f ? Task.CompletedTask :
            AnimateAsync(outgoing, false, GetOffset(outgoing), 1f, UsesFade);
    }

    private Vector2 GetOffset(NavigationPage page)
    {
        RectTransform rect = RequireRectTransform(page);
        rect.ForceUpdateRectTransforms();
        Vector2 extent = rect.rect.size;
        RectTransform parent = rect.parent as RectTransform;
        if (parent != null)
        {
            if (extent.x <= 0f)
                extent.x = parent.rect.width;
            if (extent.y <= 0f)
                extent.y = parent.rect.height;
        }
        float distance = Mathf.Max(0f, _distanceRatio);
        switch (_fromEdge)
        {
            case Edge.Left: return Vector2.left * extent.x * distance;
            case Edge.Top: return Vector2.up * extent.y * distance;
            case Edge.Bottom: return Vector2.down * extent.y * distance;
            default: return Vector2.right * extent.x * distance;
        }
    }
}
