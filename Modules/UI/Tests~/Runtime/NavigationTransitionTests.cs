using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public sealed class NavigationTransitionTests : MonoBehaviour
{
    [Serializable] private sealed class Report
    {
        public string unityVersion;
        public int checks;
        public string failure;
    }

    private readonly Report _report = new Report();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        new GameObject("Navigation tests").AddComponent<NavigationTransitionTests>();
    }

    private void Check(bool condition, string message)
    {
        _report.checks++;
        if (!condition) throw new InvalidOperationException(message);
    }

    private TransitionTestPage MakePage()
    {
        var parent = new GameObject("Page parent", typeof(RectTransform)).GetComponent<RectTransform>();
        parent.SetParent(transform, false);
        parent.sizeDelta = new Vector2(640, 360);
        parent.localScale = new Vector3(1.3f, 0.9f, 1.1f);
        parent.localRotation = Quaternion.Euler(0, 0, -8);
        var page = new GameObject("Page", typeof(RectTransform), typeof(CanvasGroup), typeof(TransitionTestPage)).GetComponent<TransitionTestPage>();
        var rect = (RectTransform)page.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.2f, 0.1f);
        rect.anchorMax = new Vector2(0.8f, 0.7f);
        rect.pivot = new Vector2(0.15f, 0.75f);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 400);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 240);
        rect.anchoredPosition3D = new Vector3(23, 31, 7);
        rect.localScale = new Vector3(0.8f, 1.2f, 1.4f);
        rect.localRotation = Quaternion.Euler(5, 11, 17);
        page.GetComponent<CanvasGroup>().alpha = 0.6f;
        return page;
    }

    private static Vector3[] Corners(RectTransform rect)
    {
        var corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        return corners;
    }

    private void CheckRestored(RectTransform rect, Vector3 position, Vector3 scale, Vector2 pivot, Vector3[] corners)
    {
        Check(Vector3.Distance(rect.anchoredPosition3D, position) < 0.001f, "Position was not restored.");
        Check(Vector3.Distance(rect.localScale, scale) < 0.0001f && rect.pivot == pivot, "Scale/pivot was not restored.");
        Vector3[] actual = Corners(rect);
        for (int i = 0; i < actual.Length; i++)
            Check(Vector3.Distance(actual[i], corners[i]) < 0.002f, "World corners shifted after animation.");
        Check(Mathf.Approximately(rect.GetComponent<CanvasGroup>().alpha, 0.6f), "Authored alpha was not restored.");
    }

    private async void Start()
    {
        _report.unityVersion = Application.unityVersion;
        try
        {
            // Allow initial editor activation to settle before measuring runtime animation.
            for (int i = 0; i < 3; i++) await Task.Yield();
            await TestEffects();
            await TestLifetimeAndSharing();
            await TestNavigator();
        }
        catch (Exception error) { _report.failure = error.ToString(); }
        finally
        {
            Time.timeScale = 1f;
            File.WriteAllText(Path.Combine(Application.dataPath, "../results.json"), JsonUtility.ToJson(_report, true));
        }
    }

    private async Task TestEffects()
    {
        var transitions = new List<TweenNavigationTransition>
        {
            ScriptableObject.CreateInstance<FadeNavigationTransition>(),
            ScriptableObject.CreateInstance<ScaleNavigationTransition>()
        };
        foreach (SlideNavigationTransition.Edge edge in Enum.GetValues(typeof(SlideNavigationTransition.Edge)))
        {
            var slide = ScriptableObject.CreateInstance<SlideNavigationTransition>();
            slide.FromEdge = edge;
            transitions.Add(slide);
            var slideFade = ScriptableObject.CreateInstance<SlideFadeNavigationTransition>();
            slideFade.FromEdge = edge;
            transitions.Add(slideFade);
        }
        foreach (TweenNavigationTransition transition in transitions)
        {
            TransitionTestPage page = MakePage();
            var rect = (RectTransform)page.transform;
            Vector3 position = rect.anchoredPosition3D;
            Vector3 scale = rect.localScale;
            Vector2 pivot = rect.pivot;
            Vector3[] corners = Corners(rect);
            transition.Duration = 0.1f;
            bool fades = !(transition is SlideNavigationTransition) || transition is SlideFadeNavigationTransition;
            Vector3 desiredPivot = rect.TransformPoint(Vector2.Scale(new Vector2(0.5f, 0.5f) - pivot, rect.rect.size));
            foreach (bool entering in new[] { true, false })
            {
                Task run = entering ? transition.PushAsync(page, null) : transition.PopAsync(page, null);
                Check(!run.IsCompleted, "Positive duration must await frames.");
                if (entering && transition is SlideNavigationTransition slide)
                {
                    Vector2[] directions = { Vector2.left, Vector2.right, Vector2.up, Vector2.down };
                    Vector2 offset = Vector2.Scale(directions[(int)slide.FromEdge], rect.rect.size) * slide.DistanceRatio;
                    Check(Vector3.Distance(rect.anchoredPosition3D, position + (Vector3)offset) < 0.001f, "Wrong slide edge or relative distance.");
                }
                if (transition is ScaleNavigationTransition)
                {
                    Check(Vector3.Distance(rect.position, desiredPivot) < 0.002f, "Scaled/rotated page pivot moved in world space.");
                    if (entering)
                        Check(Mathf.Approximately(rect.localScale.x, scale.x * 0.9f) && Mathf.Approximately(rect.localScale.z, scale.z), "Hidden scale must be relative and preserve depth.");
                }
                if (entering && fades)
                    Check(Mathf.Approximately(page.GetComponent<CanvasGroup>().alpha, 0f), "Incoming page must start transparent.");
                await Task.Yield();
                if (!run.IsCompleted && fades)
                {
                    float alpha = page.GetComponent<CanvasGroup>().alpha;
                    Check(alpha > 0f && alpha < 0.6f, "Fade must interpolate while the task is running.");
                }
                await run;
                CheckRestored(rect, position, scale, pivot, corners);
            }
            foreach (float duration in new[] { 0f, -1f })
            {
                transition.Duration = duration;
                Check(transition.PushAsync(page, null).IsCompleted && transition.PopAsync(page, null).IsCompleted, "Nonpositive duration must complete immediately.");
                CheckRestored(rect, position, scale, pivot, corners);
            }
            await transition.PushAsync(null, null);
            await transition.PopAsync(null, null);
            Destroy(page.transform.parent.gameObject);
            Destroy(transition);
        }
    }

    private async Task TestLifetimeAndSharing()
    {
        var shared = ScriptableObject.CreateInstance<ScaleNavigationTransition>();
        shared.Duration = 0.1f;
        var first = MakePage();
        var second = MakePage();
        var firstRect = (RectTransform)first.transform;
        var secondRect = (RectTransform)second.transform;
        secondRect.anchoredPosition += new Vector2(100, 50);
        Vector3 firstPosition = firstRect.anchoredPosition3D;
        Vector3 secondPosition = secondRect.anchoredPosition3D;
        second.GetComponent<CanvasGroup>().alpha = 0.4f;
        Time.timeScale = 0f;
        Task a = shared.PushAsync(first, null);
        Task b = shared.PopAsync(second, null);
        shared.Duration = 0f;
        shared.Easing.MoveKey(1, new Keyframe(1, 0));
        await Task.WhenAll(a, b);
        Check(Vector3.Distance(firstRect.anchoredPosition3D, firstPosition) < 0.001f &&
              Vector3.Distance(secondRect.anchoredPosition3D, secondPosition) < 0.001f, "Shared asset mixed page state.");
        Check(Mathf.Approximately(first.GetComponent<CanvasGroup>().alpha, 0.6f) &&
              Mathf.Approximately(second.GetComponent<CanvasGroup>().alpha, 0.4f), "Shared asset mixed alpha.");
        Time.timeScale = 1f;
        Destroy(first.transform.parent.gameObject);
        Destroy(second.transform.parent.gameObject);

        var scaleOnly = MakePage();
        shared.Duration = 0.05f;
        shared.HiddenScale = 1.1f;
        shared.PivotRatio = new Vector2(0.2f, 0.8f);
        shared.Fade = false;
        shared.Easing = null;
        Task scaleRun = shared.PushAsync(scaleOnly, null);
        Check(Mathf.Approximately(scaleOnly.transform.localScale.x, 0.8f * 1.1f) &&
              Mathf.Approximately(scaleOnly.GetComponent<CanvasGroup>().alpha, 0.6f), "Scale-only configuration must preserve alpha.");
        Check(((RectTransform)scaleOnly.transform).pivot == shared.PivotRatio, "Custom scale pivot was not applied.");
        await scaleRun;
        Destroy(scaleOnly.transform.parent.gameObject);
        shared.Fade = true;

        shared.Duration = 0.1f;
        var destroyed = MakePage();
        Task interrupted = shared.PushAsync(destroyed, null);
        DestroyImmediate(destroyed.gameObject);
        await interrupted;
        Check(interrupted.Status == TaskStatus.RanToCompletion, "Destroyed page stranded its transition.");
        var noGroup = MakePage();
        Task removedGroup = shared.PopAsync(noGroup, null);
        DestroyImmediate(noGroup.GetComponent<CanvasGroup>());
        await removedGroup;
        Check(removedGroup.Status == TaskStatus.RanToCompletion, "Destroyed CanvasGroup stranded its transition.");
        Destroy(noGroup.transform.parent.gameObject);

        var plain = new GameObject("Plain page", typeof(TransitionTestPage)).GetComponent<TransitionTestPage>();
        var fade = ScriptableObject.CreateInstance<FadeNavigationTransition>();
        fade.Duration = 0f;
        await fade.PushAsync(plain, null);
        Check(plain.GetComponent<CanvasGroup>() == null, "Zero duration should not add components.");
        fade.Duration = 0.05f;
        await fade.PushAsync(plain, null);
        Check(Mathf.Approximately(plain.GetComponent<CanvasGroup>().alpha, 1f), "Fade must support pages without a RectTransform.");
        Destroy(plain.gameObject);
        Destroy(fade);
        Destroy(shared);
    }

    private async Task TestNavigator()
    {
        var navigator = new GameObject("Navigator", typeof(RectTransform), typeof(Navigator)).GetComponent<Navigator>();
        ((RectTransform)navigator.transform).sizeDelta = new Vector2(640, 360);
        var template = new GameObject("Route template", typeof(RectTransform), typeof(CanvasGroup), typeof(TransitionTestPage)).GetComponent<TransitionTestPage>();
        template.GetComponent<CanvasGroup>().alpha = 0.65f;
        var definition = ScriptableObject.CreateInstance<PageDefinition>();
        definition.SetGenerated("ui://test", template);
        NavigationRoute first = navigator.PushDefinition(definition);
        await first.Mounted;
        var transition = ScriptableObject.CreateInstance<SlideFadeNavigationTransition>();
        transition.Duration = 0.1f;
        navigator.DefaultTransition = transition;
        NavigationRoute second = navigator.PushDefinition(definition);
        NavigationRoute third = navigator.PushDefinition(definition);
        Check(navigator.CurrentRoute == first && navigator.ScheduledRouteCount == 3, "Queued pushes committed early.");
        await third.Mounted;
        Check(navigator.CurrentRoute == third && navigator.RouteCount == 3, "Queued pushes did not commit.");
        Check(((RectTransform)second.Page.transform).anchoredPosition == Vector2.zero, "Queued page position was not restored.");
        navigator.Pop();
        await third.Popped;
        Check(navigator.CurrentRoute == second && Mathf.Approximately(second.Page.GetComponent<CanvasGroup>().alpha, 0.65f), "Pop did not restore underlying route.");
        navigator.DefaultTransition = null;
        navigator.Clear();
        while (navigator.PendingOperationCount > 0) await Task.Yield();
        Check(navigator.RouteCount == 0, "Queue did not drain after transitions.");
        Destroy(navigator.gameObject);
        Destroy(definition);
        Destroy(transition);
        Destroy(template.gameObject);
    }
}
