// Created by LunarEclipse on 2024-6-21 3:15.

using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

#if USE_ADDRESSABLES
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
#endif

namespace Luna.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasRenderer))]
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public partial class Widget : MonoBehaviour
    {
        protected bool isDirty = false;
        public RectTransform RectTransform => transform as RectTransform;
        public Canvas Canvas => GetComponent<Canvas>();
        public CanvasRenderer CanvasRenderer => GetComponent<CanvasRenderer>();
        public CanvasGroup CanvasGroup => GetComponent<CanvasGroup>();
        
        private bool _isActive = true;
        public bool Active
        {
            get => _isActive;
            set
            {
                _isActive = value;
                CanvasGroup.alpha = value ? 1 : 0;
                CanvasGroup.interactable = value;
                enabled = value;
            }
        }

        public float Alpha
        {
            get => CanvasGroup.alpha;
            set => CanvasGroup.alpha = value;
        }
        
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
        
        
#if USE_ADDRESSABLES && !DISABLE_ADDRESSABLE_NAVIGATION
        public static T New<T>(bool active = true, Transform parent = null) where T : Widget
        {
            var prefab = Load<T>();
            if (prefab == null)
            {
                var newWidget = new GameObject(typeof(T).Name) { active = active } .AddComponent<T>();
                newWidget.transform.SetParent(parent, false);
                return newWidget;
            }
            else
            {
                if (!active) prefab.SetActive(false);
                var widget = Instantiate(prefab, parent).GetComponent<T>();
                if (!active) prefab.SetActive(true);
                widget.transform.SetParent(parent, false);
                return widget;
            }
        }
#else
        public static T New<T>(bool active = true, Transform parent = null) where T : Widget
        {
            // Find the widget in the database.
            if (Widget.Dictionary.TryGetValue(typeof(T), out GameObject widgetPrefab))
            {
                if (!active) widgetPrefab.SetActive(false);
                var widget = Instantiate(widgetPrefab, parent).GetComponent<T>();
                if (!active) widgetPrefab.SetActive(true);
                return widget;
            }
            
            // Create a new widget.
            var newWidget = new GameObject(typeof(T).Name) { active = active } .AddComponent<T>();
            newWidget.Active = active;
            newWidget.transform.SetParent(parent, false);
            return newWidget;
        }
#endif

#if USE_ADDRESSABLES
        public static Task<T> NewAsync<T>(bool active = true, Transform parent = null) where T : Widget
        {
            var tcs = new TaskCompletionSource<T>();
            
            var task = LoadAsync<T>();
            
            task.ContinueWith(op =>
            {
                if (op.Result == null)
                {
                    var newWidget = new GameObject(typeof(T).Name) { active = active } .AddComponent<T>();
                    newWidget.transform.SetParent(parent, false);
                    tcs.SetResult(newWidget);
                }
                else
                {
                    if (!active) op.Result.SetActive(false);
                    var widget = Instantiate(op.Result, parent).GetComponent<T>();
                    if (!active) op.Result.SetActive(true);
                    widget.transform.SetParent(parent, false);
                    tcs.SetResult(widget);
                }
            });
            return tcs.Task;
        }
#endif
        
        public static implicit operator GameObject(Widget widget)
        {
            return widget.gameObject;
        }
    }
}