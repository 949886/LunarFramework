// Created by LunarEclipse on 2024-9-3 14:41.

#if DEVELOPMENT_BUILD || UNITY_EDITOR

using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace Luna.Diagnostics
{
    public class LaunchTimeTracker : MonoBehaviour
    {
        public static readonly Stopwatch stopwatch = new();
        private static long _elapsedMilliseconds;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void CaptureLaunchTime()
        {
            stopwatch.Start();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void AfterAssembliesLoaded()
        { 
            Debug.Log($"[LaunchTimeTracker] Assemblies loaded in {stopwatch.ElapsedMilliseconds}ms.");
            _elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void BeforeSplashScreen()
        {
            Debug.Log($"[LaunchTimeTracker] Prepare splash screen in {stopwatch.ElapsedMilliseconds - _elapsedMilliseconds}ms.");
            _elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void BeforeSceneLoad()
        {
            Debug.Log($"[LaunchTimeTracker] Splash screen loaded in {stopwatch.ElapsedMilliseconds - _elapsedMilliseconds}ms.");
            _elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AfterSceneLoad()
        {
            // Debug.Log($"[LaunchTimeTracker] Scene loaded in {_stopwatch.ElapsedMilliseconds - _elapsedMilliseconds}ms.");
            Debug.Log($"[LaunchTimeTracker] Application launched in {stopwatch.ElapsedMilliseconds}ms from startup.");
        }
        
        private static void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            Debug.Log($"[LaunchTimeTracker] Scene {arg0.name} loaded in {stopwatch.ElapsedMilliseconds - _elapsedMilliseconds}ms.");
            _elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            var go = new GameObject("LaunchTimeTracker", typeof(LaunchTimeTracker));
            DontDestroyOnLoad(go);
        }

        private void Start()
        {
            Debug.Log($"[LaunchTimeTracker] First frame rendered in {stopwatch.ElapsedMilliseconds}ms from startup.");
        }
    }
}

#endif