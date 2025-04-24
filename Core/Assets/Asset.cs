// Created by LunarEclipse on 2024-10-01 14:10.

using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

#if USE_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Luna
{
    public class Asset<T> : IAsyncDisposable
    {
        public event Action<T> onLoaded;                // Triggered when the asset is loaded.
        public event Action<float> onProgress;          // A callback that receives the loading progress (0-1).
        public event Action<DownloadStatus> onDownload; // A callback that receives the download progress (0-1) for remote assets. If the asset is local, this event will be triggered once with parameter progress 1.
        
        protected string address;
        protected AsyncOperationHandle<T> handle;
        
        public string Address => address;
        
        public Asset(string address)
        {
            this.address = address;
        }
        
        public async ValueTask DisposeAsync()
        {
            await handle.Task;
            Assets.Unload(address);
            
            Debug.Log($"Disposing {handle.Result}");
        }
        
        // Load asset asynchronously
        // Example: var prefab = await new Asset<GameObject>("path/to/prefab").Load();
        public async Task<T> Load()
        {
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);

            if (typeof(T) == typeof(SceneInstance))
                handle = ((AsyncOperationHandle)Addressables.LoadSceneAsync(address)).Convert<T>();
            else handle = Assets.LoadHandle<T>(address);

            TrackProgress();
            
            return await handle.Task;
        }

        public async void Load(LoadSceneMode loadSceneMode)
        {
            Debug.Assert(typeof(T) == typeof(SceneInstance), "Load with LoadSceneMode is only available for SceneInstance.");
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            handle = ((AsyncOperationHandle)Addressables.LoadSceneAsync(address, loadSceneMode)).Convert<T>();
            TrackProgress();
        }
        
        // Unload asset asynchronously
        public async void Unload()
        {
            await handle.Task;
            Assets.Unload(address);
        }
        
        private async void TrackProgress()
        {
            // AsyncOperationHandle<long> getDownloadSize = Addressables.GetDownloadSizeAsync(address);
            
            while (!handle.GetDownloadStatus().IsDone)
            {
                onDownload?.Invoke(handle.GetDownloadStatus());
                await UniTask.Yield();
            }
            onDownload?.Invoke(handle.GetDownloadStatus());
                
            while (!handle.IsDone)
            {
                onProgress?.Invoke(handle.PercentComplete);
                await UniTask.Yield();
            }

            onProgress?.Invoke(1);
        }

        // Load asset synchronously
        // Note: This will **block the main thread** until the asset is loaded.
        // Example: var prefab = !new Asset<GameObject>("path/to/prefab");
        public static T operator !(Asset<T> asset)
        {
            asset.Load();
            if (!asset.handle.IsDone)
                asset.handle.WaitForCompletion();
            return asset.handle.Result;
        }
        
        public static implicit operator T(Asset<T> asset)
        {
            return !asset;
        }
    }
}
#endif