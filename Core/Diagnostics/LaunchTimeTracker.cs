// Created by LunarEclipse on 2024-9-3 14:41.

#if DEVELOPMENT_BUILD || UNITY_EDITOR

using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace Core.Diagnostics
{
    public class LaunchTimeTracker : MonoBehaviour
    {
        private static Stopwatch _stopwatch = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void CaptureLaunchTime()
        {
            _stopwatch.Start();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            _stopwatch.Stop();
            Debug.Log($"[LaunchTimeTracker] Application launched in {_stopwatch.ElapsedMilliseconds}ms.");
        }
    }
}

#endif