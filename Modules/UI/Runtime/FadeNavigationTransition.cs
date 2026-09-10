using System.Threading.Tasks;
using UnityEngine;

/// <summary>CanvasGroup alpha fade transition driven by unscaled Unity time.</summary>
/// <remarks>A CanvasGroup is added when needed; the page's authored alpha is restored.</remarks>
[CreateAssetMenu(
    fileName = "FadeNavigationTransition",
    menuName = "Cherry Navigation/Fade Transition")]
public sealed class FadeNavigationTransition : TweenNavigationTransition
{
    public override Task PushAsync(
        NavigationPage incoming,
        NavigationPage outgoing)
    {
        return AnimateAsync(incoming, true, Vector2.zero, 1f, true);
    }

    public override Task PopAsync(
        NavigationPage outgoing,
        NavigationPage incoming)
    {
        return AnimateAsync(outgoing, false, Vector2.zero, 1f, true);
    }
}
