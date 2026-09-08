using System;
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class AwaitableValidationBootstrap
{
    private const string Running = "Luna.AwaitableValidation.Running";
    private const string Round = "Luna.AwaitableValidation.Round";
    private const string Deadline = "Luna.AwaitableValidation.Deadline";
    static AwaitableValidationBootstrap()
    {
        EditorApplication.update += Update;
        EditorApplication.playModeStateChanged += StateChanged;
    }
    public static void Run()
    {
        SessionState.SetBool(Running, true);
        SessionState.SetInt(Round, 1);
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        BeginRound();
    }
    private static string Result => Path.GetFullPath("results.json");
    private static void BeginRound()
    {
        if (File.Exists(Result)) File.Delete(Result);
        if (File.Exists("shutdown.txt")) File.Delete("shutdown.txt");
        SessionState.SetString(Deadline, DateTime.UtcNow.AddMinutes(3).ToString("O"));
        EditorApplication.isPlaying = true;
    }
    private static void Update()
    {
        if (!SessionState.GetBool(Running, false)) return;
        if (DateTime.UtcNow > DateTime.Parse(SessionState.GetString(Deadline, DateTime.MaxValue.ToString("O"))))
        {
            Debug.LogError("AWAITABLE_TEST_TIMEOUT");
            EditorApplication.Exit(2);
        }
        if (!EditorApplication.isPlaying || !File.Exists(Result)) return;
        string result = File.ReadAllText(Result);
        if (result.Contains("\"failure\": \"\"") == false && result.Contains("\"failure\": null") == false)
        {
            Debug.LogError(result);
            EditorApplication.Exit(1);
            return;
        }
        File.Copy(Result, "results-round-" + SessionState.GetInt(Round, 0) + ".json", true);
        EditorApplication.isPlaying = false;
    }
    private static void StateChanged(PlayModeStateChange state)
    {
        if (!SessionState.GetBool(Running, false) || state != PlayModeStateChange.EnteredEditMode) return;
#if !UNITY_2023_1_OR_NEWER
        if (!File.Exists("shutdown.txt"))
        {
            Debug.LogError("AWAITABLE_SHUTDOWN_DID_NOT_CANCEL");
            EditorApplication.Exit(1);
            return;
        }
#endif
        if (SessionState.GetInt(Round, 0) == 1)
        {
            SessionState.SetInt(Round, 2);
            EditorApplication.delayCall += BeginRound;
        }
        else
        {
            SessionState.SetBool(Running, false);
            Debug.Log("AWAITABLE_VALIDATION_PASSED_BOTH_PLAY_SESSIONS");
            EditorApplication.Exit(0);
        }
    }
}
