// Created by LunarEclipse on 2024-10-11 02:10.

#if UNITY_ANDROID

using System.Collections.Generic;
using UnityEngine;

namespace Luna
{
    public static class Android
    {
        public static readonly AndroidJavaObject unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        public static readonly AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        
        private static readonly Dictionary<string, AndroidJavaClass> _javaClasses = new();
        

        public static void Call(string className, string methodName, params object[] args)
        {
            if (_javaClasses.TryGetValue(className, out var javaClass) == false)
                _javaClasses[className] = javaClass = new AndroidJavaClass(className);
            javaClass.CallStatic(methodName, args);
        }

        public static T Call<T>(string className, string methodName, params object[] args)
        {
            if (_javaClasses.TryGetValue(className, out var javaClass) == false)
                _javaClasses[className] = javaClass = new AndroidJavaClass(className);
            return javaClass.CallStatic<T>(methodName, args);
        }
        
        public static void CallOnUiThread(string className, string methodName)
        {
            if (_javaClasses.TryGetValue(className, out var javaClass) == false)
                _javaClasses[className] = javaClass = new AndroidJavaClass(className);
            currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                javaClass.CallStatic(methodName);
            }));
        }
        
        public static T CallOnUiThread<T>(string className, string methodName)
        {
            if (_javaClasses.TryGetValue(className, out var javaClass) == false)
                _javaClasses[className] = javaClass = new AndroidJavaClass(className);
            T result = default;
            currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                result = javaClass.CallStatic<T>(methodName);
            }));
            return result;
        }
        
        public static T GetStatic<T>(string className, string fieldName)
        {
            if (_javaClasses.TryGetValue(className, out var javaClass) == false)
                _javaClasses[className] = javaClass = new AndroidJavaClass(className);
            return javaClass.GetStatic<T>(fieldName);
        }

        public static void RunOnUiThread(AndroidJavaRunnable action)
        {
            currentActivity.Call("runOnUiThread", action);
        }
        
        public static void ShowToast(string message)
        {
            if (_javaClasses.TryGetValue("android.widget.Toast", out var toastClass) == false)
                _javaClasses["android.widget.Toast"] = toastClass = new AndroidJavaClass("android.widget.Toast");
            currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                var toast = toastClass.CallStatic<AndroidJavaObject>("makeText", currentActivity, message, 1);
                toast.Call("show");
            }));
        }
    }
}

#endif