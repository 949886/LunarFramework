// Created by LunarEclipse on 2025-08-06 03:08.

#if UNITY_EDITOR

using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Luna
{
    public class PreBuildHook
    {
        [InitializeOnLoadMethod]
        public static void OnProjectLoad()
        {
            if (Application.isBatchMode)
            {
                bool isDevBuild = System.Environment.GetCommandLineArgs().ToList().Contains("-development");
                if (isDevBuild)
                    EditorUserBuildSettings.development = true;
            }
        }
    }
}

#endif