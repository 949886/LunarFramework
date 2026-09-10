using System;
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class NavigationValidationBootstrap
{
    private const string Running = "Cherry.NavigationValidation.Running";
    private const string Deadline = "Cherry.NavigationValidation.Deadline";

    static NavigationValidationBootstrap()
    {
        EditorApplication.update += Update;
    }

    public static void Run()
    {
        try
        {
            var legacy = AssetDatabase.LoadAssetAtPath<FadeNavigationTransition>("Assets/LegacyFade.asset");
            Require(legacy != null && Mathf.Approximately(legacy.Duration, 0.37f), "Legacy Fade duration was lost.");
            Require(legacy.Easing == null || Mathf.Approximately(legacy.Easing.Evaluate(0.5f), 0.5f), "Legacy Fade is no longer linear.");
            ValidateAsset<SlideNavigationTransition>(0.28f);
            ValidateAsset<SlideFadeNavigationTransition>(0.22f);
            ValidateAsset<ScaleNavigationTransition>(0.24f);
            SessionState.SetBool(Running, true);
            SessionState.SetString(Deadline, DateTime.UtcNow.AddMinutes(3).ToString("O"));
            EditorApplication.isPlaying = true;
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            EditorApplication.Exit(1);
        }
    }

    private static void ValidateAsset<T>(float defaultDuration) where T : TweenNavigationTransition
    {
        T asset = ScriptableObject.CreateInstance<T>();
        Require(Mathf.Approximately(asset.Duration, defaultDuration), "Wrong default duration: " + typeof(T).Name);
        Require(Mathf.Approximately(asset.Easing.Evaluate(0.5f), 0.875f), "Default easing is not Cubic Out.");
        asset.Duration = 0.43f;
        asset.Easing = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        if (asset is SlideNavigationTransition slide)
        {
            slide.FromEdge = SlideNavigationTransition.Edge.Left;
            slide.DistanceRatio = 0.6f;
        }
        if (asset is ScaleNavigationTransition scale)
        {
            scale.HiddenScale = 1.2f;
            scale.PivotRatio = new Vector2(0.2f, 0.8f);
            scale.Fade = false;
        }
        string path = "Assets/" + typeof(T).Name + ".asset";
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        Resources.UnloadAsset(asset);
        T loaded = AssetDatabase.LoadAssetAtPath<T>(path);
        Require(loaded != null && Mathf.Approximately(loaded.Duration, 0.43f), "Inherited duration did not serialize.");
        Require(Mathf.Approximately(loaded.Easing.Evaluate(0.5f), 0.5f), "Easing did not serialize.");
        if (loaded is SlideNavigationTransition loadedSlide)
            Require(loadedSlide.FromEdge == SlideNavigationTransition.Edge.Left && Mathf.Approximately(loadedSlide.DistanceRatio, 0.6f), "Slide settings did not serialize.");
        if (loaded is ScaleNavigationTransition loadedScale)
            Require(Mathf.Approximately(loadedScale.HiddenScale, 1.2f) && loadedScale.PivotRatio == new Vector2(0.2f, 0.8f) && !loadedScale.Fade, "Scale settings did not serialize.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void Update()
    {
        if (!SessionState.GetBool(Running, false)) return;
        if (DateTime.UtcNow > DateTime.Parse(SessionState.GetString(Deadline, DateTime.MaxValue.ToString("O"))))
        {
            Debug.LogError("NAVIGATION_VALIDATION_TIMEOUT");
            EditorApplication.Exit(2);
            return;
        }
        if (!File.Exists("results.json")) return;
        string report = File.ReadAllText("results.json");
        SessionState.SetBool(Running, false);
        if (!report.Contains("\"failure\": \"\"") && !report.Contains("\"failure\": null"))
        {
            Debug.LogError(report);
            EditorApplication.Exit(1);
            return;
        }
        Debug.Log("NAVIGATION_VALIDATION_PASSED");
        EditorApplication.Exit(0);
    }
}
