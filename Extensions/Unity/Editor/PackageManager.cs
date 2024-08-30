// Created by LunarEclipse on 2024-08-30 17:34.

#if UNITY_EDITOR

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Luna.Editor
{
    public static class PackageManager
    {
        public static event Action OnLoadSuccess;
        public static event Action OnAddSuccess;
        
        private static AddRequest _addRequest;
        private static ListRequest _listRequest;

        [InitializeOnLoadMethod]
        static void Install()
        {
            //Add("com.cysharp.unitask", "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask");
        }
        
        public static void Add(string packageId, string url = null)
        {
            // Add a package to the project if it's not already installed
            _listRequest = Client.List();
            EditorApplication.update += ListProgress;
            
            if (_listRequest.Status == StatusCode.Success)
                AddIfNotInstalled(packageId);
            else OnLoadSuccess = () => AddIfNotInstalled(packageId);
        }
        
        private static void AddIfNotInstalled(string packageId, string url = null)
        {
            if (_listRequest.Result.Any(p => p.name == packageId))
                Debug.Log("Package already installed");
            else
            {
                _addRequest = Client.Add(url ?? packageId);
                EditorApplication.update += AddProgress;
            }
        }

        private static void AddProgress()
        {
            if (_addRequest.IsCompleted)
            {
                if (_addRequest.Status == StatusCode.Success)
                {
                    Debug.Log("Package added successfully");
                    OnAddSuccess?.Invoke();
                }
                else if (_addRequest.Status >= StatusCode.Failure)
                    Debug.Log(_addRequest.Error.message);

                EditorApplication.update -= AddProgress;
            }
        }

        static void ListProgress()
        {
            if (_listRequest.IsCompleted)
            {
                if (_listRequest.Status == StatusCode.Success)
                {
                    // foreach (var package in _listRequest.Result)
                    //     Debug.Log("Package name: " + package.name);
                    
                    OnLoadSuccess?.Invoke();
                }

                else if (_listRequest.Status >= StatusCode.Failure)
                    Debug.Log(_listRequest.Error.message);

                EditorApplication.update -= ListProgress;
            }
        }
        
        
    }
    
}

#endif