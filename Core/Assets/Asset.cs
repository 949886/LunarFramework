// Created by LunarEclipse on 2024-10-01 14:10.

using System;
using System.Threading.Tasks;
using UnityEngine;

#if USE_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Luna
{
    public class Asset<T> : IAsyncDisposable where T : UnityEngine.Object
    {
        public readonly string address;
        public readonly AsyncOperationHandle<T> handler;
        
        public Asset(string address)
        {
            this.address = address;
            // handler = Addressables.LoadAssetAsync<T>(address);
            handler = Assets.LoadHandle<T>(address);
        }
        
        public async ValueTask DisposeAsync()
        {
            await handler.Task;
            // Addressables.Release(handler);
            Assets.Unload(address);
            
            Debug.Log($"Disposing {handler.Result.name}");
        }
        
        public Task<T> Load()
        {
            return handler.Task;
        }

        public static implicit operator T(Asset<T> asset)
        {
            return !asset;
        }

        public static T operator !(Asset<T> asset)
        {
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
            asset.handler.WaitForCompletion();
            stopWatch.Stop();
            Debug.Log($"[Asset] Loaded asset: {asset.handler.Result.name} in {stopWatch.ElapsedMilliseconds}ms.");
            return asset.handler.Result;
        }
    }
}
#endif

