// Created by LunarEclipse on 2025-08-06 03:08.

using UnityEngine;

namespace Luna
{
    public class InitializeHook
    {
        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeInitialize()
        {
#if DEBUG
            Debug.unityLogger.logEnabled = true;
#else
            Debug.unityLogger.logEnabled = false;
#endif
        }
    }
}