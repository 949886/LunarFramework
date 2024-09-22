// Created by LunarEclipse on 2024-09-22 18:09.

using System;
using UnityEngine;

namespace Luna.UI.Navigation
{

#if UNITY_ANDROID
    public partial class Navigator
    {
        /// Launch application by package name
        ///
        /// Parameters:
        /// - packageName: The package name of the application, e.g. "com.example.myapplication".
        /// 
        public static void LaunchApplication(string packageName)
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager"))
            {
                AndroidJavaObject intent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", packageName);
                if (intent != null)
                {
                    // intent.Call<AndroidJavaObject>("addFlags", 0x10000000); // FLAG_ACTIVITY_NEW_TASK
                    currentActivity.Call("startActivity", intent);
                }
            }
        }

        /// Launch application by package name with data
        ///
        /// Parameters:
        ///  - packageName: The package name of the application, e.g. "com.example.myapplication".
        ///  - data: The data to pass to the application.
        /// 
        public static void LaunchApplication<T>(string packageName, T data)
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager"))
            {
                AndroidJavaObject intent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", packageName);
                if (intent != null)
                {
                    // intent.Call<AndroidJavaObject>("addFlags", 0x10000000); // FLAG_ACTIVITY_NEW_TASK
                    intent.Call("putExtra", "data", data);
                    currentActivity.Call("startActivity", intent);
                }
            }
        }
        
        /// Launch application by URI
        ///
        /// Parameters:
        ///  - uri: The URI of the application, e.g. open Google Play Store with "market://details?id=" + appPackageName.
        /// 
        public static void LaunchApplication(Uri uri)
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", "android.intent.action.VIEW", uri);
                currentActivity.Call("startActivity", intent);
            }
        }
        
        /// Launch application by URI with data
        ///
        /// Parameters:
        ///  - uri: The URI of the application, e.g. open Google Play Store with "market://details?id=" + appPackageName.
        ///  - data: The data to pass to the application.
        /// 
        public static void LaunchApplication<T>(Uri uri, T data)
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", "android.intent.action.VIEW", uri);
                intent.Call("putExtra", "data", data);
                currentActivity.Call("startActivity", intent);
            }
        }
        
        /// Launch activity by class name
        ///
        /// Parameters:
        ///  - activityName: The class name of the activity.
        /// 
        public static void LaunchActivity(string activityName)
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", currentActivity, new AndroidJavaClass(activityName));
                currentActivity.Call("startActivity", intent);
            }
        }
        
        /// Quit application.
        public static void QuitApplication()
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                // currentActivity.Call("finish");
                currentActivity.Call<bool>("moveTaskToBack", true);
            }
        }
    }
#endif
}