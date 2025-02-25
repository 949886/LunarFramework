// Created by LunarEclipse on 2024-10-01 14:10.

using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Luna.Extensions;
using UnityEngine;

#if USE_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Luna
{
    public class Asset<T> : IAsyncDisposable where T : UnityEngine.Object
    {
        public readonly string address;
        
        private AsyncOperationHandle<T> handle;
        
        public Asset(string address)
        {
            this.address = address;
            // handle = Addressables.LoadAssetAsync<T>(address);
            // handle = Assets.LoadHandle<T>(address);
        }
        
        public async ValueTask DisposeAsync()
        {
            await handle.Task;
            // Addressables.Release(handler);
            Assets.Unload(address);
            
            Debug.Log($"Disposing {handle.Result.name}");
        }
        
        // Load asset asynchronously
        // Example: var prefab = await new Asset<GameObject>("path/to/prefab").Load();
        public async Task<T> Load()
        {
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            handle = Assets.LoadHandle<T>(address);
            return await handle.Task;
        }
        
        // Load asset asynchronously with progress callback.
        //  - progress: A callback that receives the loading progress (0-1).
        //
        // Example: var prefab = await new Asset<GameObject>("path/to/prefab").Load(progress => Debug.Log(progress));
        public async Task<T> Load(Action<float> progress)
        {
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            handle = Assets.LoadHandle<T>(address);
            
            UniTask.Void(async () =>
            {
                while (!handle.IsDone)
                {
                    progress.Invoke(handle.PercentComplete);
                    Debug.Log($"[Assets] Loading {address}: {handle.PercentComplete * 100}%");
                    await UniTask.Yield();
                }
                Debug.Log($"[Assets] Loading {address}: 100%");
            });
            
            return await handle.Task;
        }
        
        // Unload asset asynchronously
        public async void Unload()
        {
            await handle.Task;
            Assets.Unload(address);
        }

        // Load asset synchronously
        // Note: This will **block the main thread** until the asset is loaded.
        // Example: var prefab = !new Asset<GameObject>("path/to/prefab");
        public static T operator !(Asset<T> asset)
        {
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
            asset.handle = Assets.LoadHandle<T>(asset.address);
            if (!asset.handle.IsDone)
                asset.handle.WaitForCompletion();
            stopWatch.Stop();
            Debug.Log($"[Asset] Loaded asset: {asset.handle.Result.name} in {stopWatch.ElapsedMilliseconds}ms.");
            return asset.handle.Result;
        }
        
        public static implicit operator T(Asset<T> asset)
        {
            return !asset;
        }
    }
}
#endif