﻿// Created by LunarEclipse on 2024-6-19 0:40.

using System.Collections.Generic;
using UnityEngine;

#if USE_ADDRESSABLES
using System;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

namespace Luna.UI
{
    public partial class Widget
    {
#if USE_ADDRESSABLES
        private static readonly Dictionary<Type, AsyncOperationHandle<GameObject>> _typeHandlers = new ();
        private static readonly Dictionary<string, AsyncOperationHandle<IList<GameObject>>> _labelHandlers = new ();
        
        public static GameObject Load<T>() where T : Widget
        {
            if (_typeHandlers.TryGetValue(typeof(T), out var cachedHandler))
                return cachedHandler.WaitForCompletion();
            
            var key = GetAddressableKey<T>();
            var handler = Addressables.LoadAssetAsync<GameObject>(key);
            var prefab = handler.WaitForCompletion();
            _typeHandlers.Add(typeof(T), handler);
            return prefab;
        }
        
        public static Task<GameObject> LoadAsync<T>() where T : Widget
        {
            if (_typeHandlers.TryGetValue(typeof(T), out var cachedHandler))
                return cachedHandler.Task;
            
            var key = GetAddressableKey<T>();
            var handler = Addressables.LoadAssetAsync<GameObject>(key);
            _typeHandlers.Add(typeof(T), handler);
            return handler.Task;
        }
        
        public static Task<IList<GameObject>> Load(string label)
        {
            if (_labelHandlers.TryGetValue(label, out var cachedHandler))
                return cachedHandler.Task;
            
            var handler = Addressables.LoadAssetsAsync<GameObject>(label, null);
            handler.Completed += op =>
            {
                foreach (var obj in op.Result)
                    Debug.Log($"[Widget] Loaded asset: {obj.name}");
            };
            _labelHandlers.Add(label, handler);
            return handler.Task;
        }

        public static void Unload<T>() where T : Widget
        {
            var key = typeof(T);
            if (_typeHandlers.ContainsKey(key))
            {
                Addressables.Release(_typeHandlers[key]);
                _typeHandlers.Remove(key);
            }
            else Debug.LogWarning($"[Widget] Key not found: {key}");
        }
        
        public static void Unload(string label)
        {
            if (_labelHandlers.ContainsKey(label))
            {
                Addressables.Release(_labelHandlers[label]);
                _labelHandlers.Remove(label);
            }
            else Debug.LogWarning($"[Widget] Label not found: {label}");
        }
        
        public static string GetAddressableKey<T>() where T : Widget
        {
            return (typeof(T).Namespace ?? "global") + "." + typeof(T).Name;
        }
#else
        public static List<GameObject> All { get; private set; } = new ();
        public static Dictionary<Type, GameObject> Dictionary { get; private set; } = new ();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            // Widget database file name
            const string WIDGETS_DB_FILE_NAME = "Widgets.g";
            
            // Find all Widget in the project at startup.
            // Load widgets scriptable object
            var widgets = Resources.Load<Widgets>(WIDGETS_DB_FILE_NAME);
            if (widgets != null)
            {
                All = widgets.Prefabs;
                foreach (var widget in All)
                {
                    Debug.Log($"[Widget Database] Found widget: {widget.name}");
                    Dictionary.TryAdd(widget.GetComponent<Widget>().GetType(), widget);
                }
            }
        }
#endif
        
        // public static readonly Dictionary<string, Widget> All = new ();
        //
        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        // private static void Initialize()
        // {
        //     var widgets = Resources.FindObjectsOfTypeAll<Widget>();
        //     foreach (var widget in widgets)
        //     {
        //         All.Add(widget.name, widget);
        //         Debug.Log($"[Widget] Found Widget: {widget.name}");
        //     }
        // }
    }
}