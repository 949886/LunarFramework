// Created by LunarEclipse on 2024-6-21 3:15.

using Cysharp.Threading.Tasks;
using UnityEngine;

#if USE_ADDRESSABLES
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
#endif

namespace Luna.UI
{
    public abstract partial class Widget : MonoBehaviour
    {
        protected bool isDirty = false;
        
        protected Widget() {}

        // protected async void SetDirty()
        // {
        //     if (!isDirty)
        //     {
        //         isDirty = true;
        //         if (isActiveAndEnabled)
        //             await UniTask.Yield();
        //         // else await UniTask.WaitUntil(() => isActiveAndEnabled);
        //         Build();
        //         isDirty = false;
        //     }
        // }
        //
        // protected virtual void Build() {}
        
#if USE_ADDRESSABLES
        public static T New<T>(Transform parent = null) where T : Widget
        {
            var key = (typeof(T).Namespace ?? "global") + "." + typeof(T).Name;
            var handler = Addressables.LoadAssetAsync<GameObject>(key);
            var prefab = handler.WaitForCompletion();
            if (prefab == null)
            {
                var newWidget = new GameObject(typeof(T).Name).AddComponent<T>();
                newWidget.transform.SetParent(parent, false);
                return newWidget;
            }
            else
            {
                var widget = Instantiate(prefab, parent).GetComponent<T>();
                widget.transform.SetParent(parent, false);
                return widget;
            }
        }
        
        public static Task<T> NewAsync<T>(Transform parent = null) where T : Widget
        {
            var tcs = new TaskCompletionSource<T>();
            
            var key = (typeof(T).Namespace ?? "global") + "." + typeof(T).Name;
            var handler = Addressables.LoadAssetAsync<GameObject>(key);
            handler.Completed += op =>
            {
                if (op.Result == null)
                {
                    var newWidget = new GameObject(typeof(T).Name).AddComponent<T>();
                    newWidget.transform.SetParent(parent, false);
                    tcs.SetResult(newWidget);
                }
                else
                {
                    var widget = Instantiate(op.Result, parent).GetComponent<T>();
                    widget.transform.SetParent(parent, false);
                    tcs.SetResult(widget);
                }
            };
            return tcs.Task;
        }
#else
        public static T New<T>(Transform parent = null) where T : Widget
        {
            // Find the widget in the database.
            var widget = Widget.Dictionary[typeof(T)];
            if (widget != null)
                return Instantiate(widget, parent).GetComponent<T>();

            // Create a new widget.
            var newWidget = new GameObject(typeof(T).Name).AddComponent<T>();
            newWidget.transform.SetParent(parent, false);
            return newWidget;
        }
#endif
    }
}