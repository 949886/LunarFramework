// Created by LunarEclipse on 2024-7-31 18:3.

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

#if USE_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

namespace Luna
{
    public class Assets
    {
        
#if USE_ADDRESSABLES
        
        private static readonly Dictionary<string, AsyncOperationHandle> _labelHandlers = new ();
        
        public static Task<IList<T>> Load<T>(params string[] labels)
        {
            var key = string.Join(",", labels);
            if (_labelHandlers.TryGetValue(key, out var cachedHandler))
                return cachedHandler.Convert<IList<T>>().Task;
            
            var handler = Addressables.LoadAssetsAsync<T>(new List<string>(labels), null, Addressables.MergeMode.Intersection);
            handler.Completed += op =>
            {
                foreach (var obj in op.Result)
                    Debug.Log($"[Widget] Loaded asset: {obj}");
            };
            _labelHandlers.Add(key, handler);
            return handler.Task;
        }
        
        public static void Unload(params string[] labels)
        {
            var key = string.Join(",", labels);
            if (_labelHandlers.ContainsKey(key))
            {
                Addressables.Release(_labelHandlers[key]);
                _labelHandlers.Remove(key);
            }
            else Debug.LogWarning($"[Widget] Label not found: {key}");
        }
        
#endif
    }
}