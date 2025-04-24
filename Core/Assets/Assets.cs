// Created by LunarEclipse on 2024-7-31 18:3.

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

#if USE_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
#endif

namespace Luna
{
    public static class Assets
    {
        
#if USE_ADDRESSABLES
        
        private static readonly Dictionary<string, AsyncOperationHandle> _cachedHandlers = new();

        public static Task<Object> Load(string label)
        {
            return Load<Object>(label);
        }
        
        public static async Task<T> Load<T>(string label)
        {
            return await LoadHandle<T>(label).Task;
        }

        public static Task<IList<Object>> Load(params string[] labels)
        {
            return Load<Object>(labels);
        }
        
        public static async Task<IList<T>> Load<T>(params string[] labels)
        {
            var key = string.Join(",", labels);
            if (_cachedHandlers.TryGetValue(key, out var cachedHandler))
                return await cachedHandler.Convert<IList<T>>().Task;
            
            var handle = Addressables.LoadAssetsAsync<T>(new List<string>(labels), null, Addressables.MergeMode.Intersection);
            handle.Completed += op => {
                foreach (var obj in op.Result)
                    Debug.Log($"[Assets] Loaded asset: {obj}");
            };
            _cachedHandlers.Add(key, handle);
            return await handle.Task;
        }
        
        public static AsyncOperationHandle<T> LoadHandle<T>(string label)
        {
            var key = label;
            if (_cachedHandlers.TryGetValue(key, out var cachedHandler))
            {
                Debug.Log($"[Assets] Using cached asset: {cachedHandler.Result}");
                return cachedHandler.Convert<T>();
            }
            
            var handle = Addressables.LoadAssetAsync<T>(label);
            handle.Completed += op => Debug.Log($"[Assets] Loaded asset: {op.Result}");
            _cachedHandlers.Add(key, handle);
            return handle;
        }

        public static async Task<SceneInstance> LoadScene(string label, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            return await LoadHandle<SceneInstance>(label).Task;
        }

        public static AsyncOperationHandle<SceneInstance> LoadSceneHandle(string label, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            var key = label;
            if (_cachedHandlers.TryGetValue(key, out var cachedScene))
            {
                Debug.Log($"[Assets] Using cached scene: {cachedScene.Result}");
                return cachedScene.Convert<SceneInstance>();
            }
            
            var handle = Addressables.LoadSceneAsync(label, loadMode);
            handle.Completed += op => Debug.Log($"[Assets] Loaded scene: {op.Result}");
            _cachedHandlers.Add(key, handle);
            
            return handle;
        }
        
        public static void Unload(params string[] labels)
        {
            var key = string.Join(",", labels);
            if (_cachedHandlers.ContainsKey(key))
            {
                Addressables.Release(_cachedHandlers[key]);
                _cachedHandlers.Remove(key);
            }
            else Debug.LogWarning($"[Assets] Label not found: {key}");
        }

        public static void UnloadScene(string label)
        {
            if (_cachedHandlers.TryGetValue(label, out var cachedScene))
            {
                Addressables.UnloadSceneAsync(cachedScene);
                _cachedHandlers.Remove(label);
            }
            else Debug.LogWarning($"[Assets] Scene not found: {label}");
        }
        
        
#endif
        
    }
}