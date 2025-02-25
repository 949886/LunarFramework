// Created by LunarEclipse on 2024-7-31 18:3.

using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

#if USE_ADDRESSABLES
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

namespace Luna
{
    public class Assets
    {
        
#if USE_ADDRESSABLES
        
        private static readonly Dictionary<string, AsyncOperationHandle> _labelHandlers = new ();

        public static Task<Object> Load(string label)
        {
            return Load<Object>(label);
        }
        
        public static async Task<T> Load<T>(string label)
        {
            await UniTask.DelayFrame(5);
            return await LoadHandle<T>(label).Task;
        }

        public static Task<IList<Object>> Load(params string[] labels)
        {
            return Load<Object>(labels);
        }
        
        public static async Task<IList<T>> Load<T>(params string[] labels)
        {
            await UniTask.DelayFrame(5);
            
            var key = string.Join(",", labels);
            if (_labelHandlers.TryGetValue(key, out var cachedHandler))
                return await cachedHandler.Convert<IList<T>>().Task;
            
            var handle = Addressables.LoadAssetsAsync<T>(new List<string>(labels), null, Addressables.MergeMode.Intersection);
            handle.Completed += op => {
                foreach (var obj in op.Result)
                    Debug.Log($"[Assets] Loaded asset: {obj}");
            };
            _labelHandlers.Add(key, handle);
            return await handle.Task;
        }
        
        public static AsyncOperationHandle<T> LoadHandle<T>(string label)
        {
            var key = label;
            if (_labelHandlers.TryGetValue(key, out var cachedHandler))
            {
                Debug.Log($"[Assets] Using cached asset: {cachedHandler.Result}");
                return cachedHandler.Convert<T>();
            }
            
            var handle = Addressables.LoadAssetAsync<T>(label);
            handle.Completed += op => Debug.Log($"[Assets] Loaded asset: {op.Result}");
            _labelHandlers.Add(key, handle);
            return handle;
        }
        
        public static void Unload(params string[] labels)
        {
            var key = string.Join(",", labels);
            if (_labelHandlers.ContainsKey(key))
            {
                Addressables.Release(_labelHandlers[key]);
                _labelHandlers.Remove(key);
            }
            else Debug.LogWarning($"[Assets] Label not found: {key}");
        }
        
#endif
        
    }
}