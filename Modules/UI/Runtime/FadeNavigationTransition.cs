using System.Threading.Tasks;
using UnityEngine;

/// <summary>CanvasGroup alpha fade transition driven by unscaled Unity time.</summary>
/// <remarks>
/// Unscaled time keeps UI navigation responsive while gameplay timeScale is zero.
/// A CanvasGroup is added to the page when one does not already exist.
/// </remarks>
[CreateAssetMenu(
    fileName = "FadeNavigationTransition",
    menuName = "Cherry Navigation/Fade Transition")]
public sealed class FadeNavigationTransition : NavigationTransition
{
    [SerializeField, Min(0f)] private float _duration = 0.18f;

    /// <summary>Fade duration in seconds, clamped to zero or greater.</summary>
    public float Duration
    {
        get { return _duration; }
        set { _duration = Mathf.Max(0f, value); }
    }

    public override async Task PushAsync(
        NavigationPage incoming,
        NavigationPage outgoing)
    {
        if (incoming == null || _duration <= 0f)
            return;

        CanvasGroup group = GetOrAddCanvasGroup(incoming.gameObject);
        float targetAlpha = group.alpha;
        group.alpha = 0f;
        await Fade(group, 0f, targetAlpha);
    }

    public override async Task PopAsync(
        NavigationPage outgoing,
        NavigationPage incoming)
    {
        if (outgoing == null || _duration <= 0f)
            return;

        CanvasGroup group = GetOrAddCanvasGroup(outgoing.gameObject);
        await Fade(group, group.alpha, 0f);
    }

    private async Task Fade(CanvasGroup group, float from, float to)
    {
        float elapsed = 0f;
        group.alpha = from;

        while (elapsed < _duration && group != null)
        {
            await Task.Yield();
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / _duration));
        }

        if (group != null)
            group.alpha = to;
    }

    private static CanvasGroup GetOrAddCanvasGroup(GameObject gameObject)
    {
        CanvasGroup group = gameObject.GetComponent<CanvasGroup>();
        if (group == null)
            group = gameObject.AddComponent<CanvasGroup>();
        return group;
    }
}
