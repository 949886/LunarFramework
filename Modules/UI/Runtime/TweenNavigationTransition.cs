using System;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>Shared timing and visual state restoration for built-in transitions.</summary>
/// <remarks>
/// Uses unscaled time and keeps all animation state local to each invocation,
/// so the same asset may be shared by independent Navigators.
/// </remarks>
public abstract class TweenNavigationTransition : NavigationTransition
{
    // Keep the original Fade field name so existing assets retain their duration.
    [SerializeField, Min(0f)] private float _duration = 0.18f;
    [SerializeField] private AnimationCurve _easing = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    public float Duration
    {
        get { return _duration; }
        set { _duration = Mathf.Max(0f, value); }
    }

    /// <summary>Maps normalized elapsed time to animation progress. Null means linear.</summary>
    public AnimationCurve Easing
    {
        get { return _easing; }
        set { _easing = value; }
    }

    /// <summary>The cubic ease-out used by Cherry's slide and scale transitions.</summary>
    protected static AnimationCurve CubicEaseOut()
    {
        return new AnimationCurve(new Keyframe(0f, 0f, 3f, 3f), new Keyframe(1f, 1f, 0f, 0f));
    }

    protected static RectTransform RequireRectTransform(NavigationPage page)
    {
        RectTransform rect = page.transform as RectTransform;
        if (rect == null)
            throw new InvalidOperationException("Slide and scale transitions require a RectTransform on the NavigationPage root.");
        return rect;
    }

    protected async Task AnimateAsync(NavigationPage page, bool entering, Vector2 offset,
        float scaleFactor, bool fade, Vector2? pivotRatio = null)
    {
        float duration = _duration;
        if (page == null || duration <= 0f)
            return;

        bool scaling = !Mathf.Approximately(scaleFactor, 1f);
        bool moving = offset.sqrMagnitude > 0f || scaling;
        RectTransform rect = moving ? RequireRectTransform(page) : null;
        Vector3 originalPosition = rect != null ? rect.anchoredPosition3D : Vector3.zero;
        Vector3 originalLocalPosition = rect != null ? rect.localPosition : Vector3.zero;
        Vector3 originalScale = rect != null ? rect.localScale : Vector3.one;
        Vector2 originalPivot = rect != null ? rect.pivot : Vector2.zero;
        CanvasGroup group = null;
        float originalAlpha = 1f;
        if (fade)
        {
            group = page.GetComponent<CanvasGroup>();
            if (group == null)
                group = page.gameObject.AddComponent<CanvasGroup>();
            originalAlpha = group.alpha;
        }
        // Editing a shared asset mid-animation must not change an in-flight run.
        AnimationCurve curve = _easing != null ? new AnimationCurve(_easing.keys) : null;

        try
        {
            if (scaling)
            {
                Vector2 pivot = pivotRatio ?? new Vector2(0.5f, 0.5f);
                Vector2 delta = Vector2.Scale(pivot - originalPivot, rect.rect.size);
                rect.pivot = pivot;
                // Unity positions RectTransforms at their pivot. Compensate in parent
                // space so changing it preserves the page's original world corners.
                rect.localPosition = originalLocalPosition +
                    rect.localRotation * Vector3.Scale(new Vector3(delta.x, delta.y, 0f), originalScale);
            }

            Vector3 shownPosition = rect != null ? rect.anchoredPosition3D : Vector3.zero;
            Vector3 hiddenPosition = shownPosition + new Vector3(offset.x, offset.y, 0f);
            float factor = Mathf.Max(scaleFactor, 0.01f);
            Vector3 hiddenScale = new Vector3(originalScale.x * factor, originalScale.y * factor, originalScale.z);
            float elapsed = 0f;
            Apply(entering ? 0f : 1f);
            while (elapsed < duration)
            {
                await Task.Yield();
                // A scene switch or user callback may destroy a page while awaiting.
                if (page == null || (moving && rect == null) || (fade && group == null))
                    return;
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                float eased = curve != null ? curve.Evaluate(progress) : progress;
                Apply(entering ? eased : 1f - eased);
            }
            Apply(entering ? 1f : 0f);

            void Apply(float shownAmount)
            {
                if (moving)
                    rect.anchoredPosition3D = Vector3.LerpUnclamped(hiddenPosition, shownPosition, shownAmount);
                if (scaling)
                    rect.localScale = Vector3.LerpUnclamped(hiddenScale, originalScale, shownAmount);
                if (fade)
                    group.alpha = Mathf.Lerp(0f, originalAlpha, shownAmount);
            }
        }
        finally
        {
            // Pop detaches the page after completion, without another rendered frame.
            if (rect != null)
            {
                if (scaling)
                {
                    rect.localScale = originalScale;
                    rect.pivot = originalPivot;
                }
                rect.anchoredPosition3D = originalPosition;
            }
            if (group != null)
                group.alpha = originalAlpha;
        }
    }
}
