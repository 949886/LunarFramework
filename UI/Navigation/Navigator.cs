// Created by LunarEclipse on 2024-6-18 22:17.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.UI.Navigation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Luna.UI.Navigation
{
    public partial class Navigator : MonoBehaviour
    {
        // Singleton instance for easy access
        public static Navigator Instance;
        
        private static readonly Stack<Navigator> _navigatorStack = new();
        
        
        // Reference to the root canvas. If not found, a new canvas will be created.
        public Canvas canvas; 
        // The root widget of the navigator
        public GameObject rootWidget; 
        
        // Focus on last selected object when a new widget is popped
        public bool focusAutomatically = true; 
        // Pop the top widget when the escape key is pressed
        public bool escToPop = false; 
        
        public static DeactivateMode deactivateMode = DeactivateMode.GameObject;
        
        
        private readonly Stack<Widget> _widgetStack = new();
        // private readonly Stack<GameObject> _widgetHistory = new();
        private readonly Stack<Route> _routeStack = new();
        
        private bool _isDontDestroyOnLoad = false;
        
        
        public Widget TopWidget => _widgetStack.Peek();
        
        public Route PreviousRoute => _routeStack.Count > 1 ? _routeStack.Peek() : null;
        
        public event Action<Route> onPushed;
        public event Action<Route> onPopped;
        
        
        
        // Load the Navigator instance on startup if it doesn't exist
        [RuntimeInitializeOnLoadMethod]
        private static void InitializeRootNavigator()
        {
            if (Instance == null)
            {
                GameObject navigator = new GameObject("UI Navigator");
                Instance = navigator.AddComponent<Navigator>();
                DontDestroyOnLoad(navigator);
                Instance._isDontDestroyOnLoad = true;
            }
        }

        public static Navigator Create(GameObject rootWidget)
        {
            GameObject navigator = new GameObject("UI Navigator");
            Navigator instance = navigator.AddComponent<Navigator>();
            instance.rootWidget = rootWidget;
            return instance;
        }

        private void Awake()
        {
            foreach (var navigator in _navigatorStack)
                navigator.gameObject.SetActive(false);
            
            Instance = this;
            _navigatorStack.Push(this);
            
            // if (Instance == null)
            //     Instance = this;
            // else Destroy(gameObject); 
        }
        
        private void Start()
        {
            if (canvas == null)
            {
                // Create a new canvas if none is found
                if (!(canvas = FindObjectOfType<Canvas>()?.rootCanvas))
                {
                    GameObject canvasObject = new GameObject("Canvas");
                    canvas = canvasObject.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvasObject.AddComponent<CanvasScaler>();
                    canvasObject.AddComponent<GraphicRaycaster>();
                }
            }
            
            // Preload all widgets in the game
            // if (widgets.Count == 0)
            // {
            //     // Load stateful widgets scriptable object
            //     Widgets widgets = Resources.Load<Widgets>("Widgets.g");
            //     if (widgets != null)
            //     {
            //         this.widgets = widgets.Prefabs;
            //         foreach (var widget in this.widgets)
            //         {
            //             Debug.Log($"[Navigator] Found widget: {widget.name}");
            //             _widgetDictionary.TryAdd(widget.GetComponent<Widget>().GetType(), widget);
            //         }
            //     }
            // }
            
            // Load the rootWidget
            if (rootWidget != null)
            {
                if (rootWidget.activeInHierarchy)
                {
                    var widget = rootWidget.GetComponent<Widget>() ?? rootWidget.AddComponent<Widget>();
                    _widgetStack.Push(widget);
                }
                else Push(rootWidget);
            } 
            
        }

        private void Update()
        {
            if (escToPop && Input.GetKeyDown(KeyCode.Escape))
            {
                Pop();
            }
        }

        private void OnDestroy()
        {
            _navigatorStack.Pop();
            if (_navigatorStack.Count > 0)
            {
                var navigator = _navigatorStack.Peek();
                navigator.gameObject.SetActive(true);
                Instance = navigator;
            }
                
        }
        
        public static Task<dynamic> Push(Widget widget, Action callback = null)
        {
            return Instance._Push(widget, callback);
        }
        
        public static Task<dynamic> Push(GameObject widgetPrefab, Action<GameObject> callback = null)
        {
            return Instance._Push(widgetPrefab, callback);
        }
        
        /// Push a widget to the top of the stack.
        /// The widget will be instantiated and added to the canvas. <br/>
        ///
        ///  - [T] The type of the widget to be pushed.
        ///     Prefab with Widget component will be registered in the Widgets prefab database automatically.
        ///     Note: A widget type should only have one prefab that corresponds to it. <br/>
        ///
        ///  - [callback] A callback to be executed after the widget is instantiated.
        ///     You can use this callback to pass data to the widget.
        ///     Note: The callback will be executed before the widget is enabled.
        ///     Therefore, you should initialize the widget in Awake method.
        public static Task<dynamic> Push<T>(Action<T> callback = null) where T : Widget
        {
            return Instance._Push<T>(callback);
        }
        
        public static Task<dynamic> Push<T>(bool keepSelectionOnPop, Action<T> callback = null) where T : Widget
        {
            return Instance._Push<T>(callback, keepSelectionOnPop);
        }
        
        /// Push a widget to replace the top widget in the stack. <br/>
        public static Task<dynamic> PushReplacement<T>(Action<T> callback = null) where T : Widget
        {
            return Instance._PushReplacement(callback);
        }
        

        /// Pop the top widget from the stack.
        public static void Pop()
        {
            Instance._Pop(0);
        }
        
        /// Pop the top widget from the stack with a result. <br/>
        ///
        ///  - [T] The type of the result to be passed to the previous widget.
        /// 
        ///  - [result] The result to be passed to the previous widget.
        public static void Pop<T>(T result = default)
        {
            Instance._Pop(result);
        }
        
        /// Pop all widgets until the root widget. <br/>
        /// Returns the root widget.
        public static void PopToRoot()
        {
            Instance._PopToRoot(0);
        }
        
        /// Pop all widgets until the root widget. <br/>
        /// Returns the root widget.
        public static void PopToRoot<T>(T result = default)
        {
            Instance._PopToRoot(result);
        }
        
        /// Pop widgets until the top widget is of type T. <br/>
        ///
        ///  - [T] The type of the target widget.
        /// 
        public static void PopUntil<T>() where T : Widget
        {
            Instance._PopUntil<T, int>(0);
        }
        
        
        /// Pop widgets until the top widget is of type T. <br/>
        /// Return the top widget of type T. <br/>
        public static T BackTo<T>() where T : Widget
        {
            Instance._PopUntil<T, int>(0);
            return Instance.TopWidget as T;
        }
        
        /// Pop widgets until the top widget is of type T. <br/>
        ///
        /// Type Argument:
        ///  - [T] The type of the target widget.
        ///  - [U] The result to be passed to the target widget.
        ///
        /// Parameters:
        ///  - [result] The result to be passed to the target widget.
        public static void PopUntil<T, U>(U result = default) where T : Widget
        {
            Instance._PopUntil<T, U>(result);
        }
        
        public static T BackTo<T, U>(U result = default) where T : Widget
        {
            Instance._PopUntil<T, U>(result);
            return Instance.TopWidget as T;
        }
        
        /// Show a modal widget on top of the current widget. <br/>
        ///
        ///  - [T] The type of the modal widget to be shown.
        ///
        ///  - [maskDismissible] Whether the modal mask is dismissible by clicking outside the widget.
        ///  - [maskColor] The color of the modal mask.
        ///  - [offset] The offset of the modal widget.
        public static Task<dynamic> ShowModal<T>(Action<T> builder = null, bool maskDismissible = true, Color? maskColor = null, Vector2 offset = default) where T : Widget
        {
            var modalRoute = new ModalRoute<T>(builder, maskDismissible, maskColor, offset);
            return Instance._Push<T>(modalRoute, hidePrevious: false);
        }
        
        public static Widget GetPrevious()
        {
            return Instance.PreviousRoute?.To;
        }
        
        public static T GetPrevious<T>() where T : Widget
        {
            return Instance.PreviousRoute?.To as T;
        }

        protected Task<dynamic> _Push(Widget widget, Action callback = null)
        {
            if (_widgetStack.Count > 0)
                _widgetStack.Peek().SetNaviActive(false);
            
            _widgetStack.Push(widget);
            
            var route = new Route(widget);
            route.To ??= widget;
            route.LastSelected = EventSystem.current.currentSelectedGameObject;
            route.OnPush();
            _routeStack.Push(route);
            
            widget.Active = true;
            widget.transform.SetParent(canvas.transform, false);
            
            // Executing the callback.
            callback?.Invoke();
            
            return route.Popped;
        }
        
        protected Task<dynamic> _Push(GameObject widgetPrefab, Action<GameObject> callback = null)
        {
            if (_widgetStack.Count > 0)
                _widgetStack.Peek().SetNaviActive(false);
            
            // Instantiating the widget.
            // Widget will execute Awake method.
            var go = Instantiate(widgetPrefab, canvas.transform);
            var widget = go.GetComponent<Widget>() ?? go.AddComponent<Widget>();
            // var widget = widgetPrefab.activeInHierarchy ?
            //     (widgetPrefab.GetComponent<Widget>() ?? widgetPrefab.AddComponent<Widget>()) : 
            //     Instantiate(widgetPrefab, canvas.transform).GetComponent<Widget>();
            _widgetStack.Push(widget);
            
            var route = new Route(widget);
            route.To ??= widget;
            route.LastSelected = EventSystem.current.currentSelectedGameObject;
            route.OnPush();
            _routeStack.Push(route);
            
            // Executing the callback.
            if (callback is not null)
                callback.Invoke(go);
                
            // Setting widget active.
            // Widget will execute OnEnable and Start methods.
            go.SetActive(true);
            widgetPrefab.SetActive(true);

            return route.Popped;
        }
        
        
#if USE_ADDRESSABLES && !DISABLE_ADDRESSABLE_NAVIGATION
        protected async Task<dynamic> _Push<T>(Route<T> route = default, bool keepSelectionOnPop = true, bool hidePrevious = true) where T : Widget
        {
            if (_widgetStack.Count > 0 && hidePrevious)
                _widgetStack.Peek().SetNaviActive(false);

            // if (route.To == null)
            // {
            //     // Load the widget prefab from addressables.
            //     var widgetPrefab = await Widget.LoadAsync<T>();
            //     if (widgetPrefab == null)
            //     {
            //         Debug.LogError($"[Navigator] Widget of type {typeof(T)} not found.");
            //         return route.Popped;
            //     }
            //
            //     widgetPrefab.SetActive(false);
            //
            //     // Instantiating the widget.
            //     // Widget will execute Awake method.
            //     var go = Instantiate(widgetPrefab, canvas.transform);
            //     var newWidget = go.GetComponent<T>();
            //     newWidget.enabled = false;
            //     route.To = newWidget;
            // }
            
            // Creating a new navigation route.
            route.To ??= await Widget.NewAsync<T>();
            route.LastSelected = keepSelectionOnPop ? EventSystem.current.currentSelectedGameObject : null;
            route.OnPush();
            _routeStack.Push(route);
            
            var widget = route.To;
            widget.enabled = false;
            widget.transform.SetParent(canvas.transform, false);
            _widgetStack.Push(widget);
                
            // Setting game object active.
            // Widget will execute Awake methods.
            widget.Active = true;

            // Executing the callback.
            route.callback?.Invoke(widget);

            // Enabling the widget component.
            // Widget will execute OnEnable and Start methods.
            widget.enabled = true;

            // Wait for the widget to be popped and release the addressable asset.
            var result = await route.Popped;
            Widget.Unload<T>();
            
            onPushed?.Invoke(route);
            
            return result;
        }
#else // USE SCRIPTABLE OBJECT
        protected Task<dynamic> _Push<T>(Route<T> route = default, bool keepSelectionOnPop = true, bool hidePrevious = true) where T : Widget
        {
            if (_widgetStack.Count > 0 && hidePrevious)
                _widgetStack.Peek().SetNaviActive(false);
            
            // Creating a new navigation route if not provided.
            // Instantiating the widget in typed route.
            route.To ??= Widget.New<T>();
            route.LastSelected = keepSelectionOnPop ? EventSystem.current.currentSelectedGameObject : null;
            route.OnPush();
            _routeStack.Push(route);

            var widget = route.To;
            widget.enabled = false;
            _widgetStack.Push(widget);
                
            // Setting game object active.
            // Widget will execute Awake methods.
            widget.gameObject.SetActive(true);
                
            // Executing the callback.
            route.callback?.Invoke(widget);
                
            // Enabling the widget component.
            // Widget will execute OnEnable and Start methods.
            widget.enabled = true;
            
            onPushed?.Invoke(route);

            return route.Popped;
        }
#endif
        
        protected Task<dynamic> _PushReplacement<T>(Action<T> callback = null) where T : Widget
        {
            if (_widgetStack.Count > 0)
            {
                var topWidget = _widgetStack.Pop();
                Destroy(topWidget.gameObject);
            }

            var task = _Push<T>(callback);
            var newRoute = _routeStack.Pop();
            _routeStack.Peek().To = newRoute.To;
            
            return task;
        }

        protected void _Pop<T>(T result = default)
        {
            if (_widgetStack.Count > 1)
            {
                // Pop & destroy the top widget
                var topWidget = _widgetStack.Pop();
                Destroy(topWidget.gameObject);
                
                var route = _routeStack.Pop();
                
                // Focus on the last selected object
                if (focusAutomatically && route?.LastSelected != null)
                    EventSystem.current.SetSelectedGameObject(route.LastSelected);
                
                // Pass the result to the previous widget
                if (route != null)
                {
                    route.OnPop();
                    route.popCompleter.SetResult(result);
                }
                
                // Show the previous widget
                if (_widgetStack.Count > 0)
                    _widgetStack.Peek().SetNaviActive(true);
                
                onPopped?.Invoke(route);
            }
        }

        protected void _PopToRoot<T>(T result = default)
        {
            while (_widgetStack.Count > 1)
            {
                var widget = _widgetStack.Pop();
                var route = _routeStack.Pop();
                Destroy(widget.gameObject);
                
                if (route != null)
                {
                    route.OnPop();
                    route.popCompleter.SetResult(result);   
                }
                
                if (_widgetStack.Count == 1)
                {
                    TopWidget.SetNaviActive(true);
                    
                    // Focus on the last selected object
                    if (focusAutomatically && route?.LastSelected != null)
                        EventSystem.current.SetSelectedGameObject(route.LastSelected);
                    
                    onPopped?.Invoke(route);
                    
                    break;
                }
            }
        }
        
        protected void _PopUntil<T, U>(U result = default) where T : Widget
        {
            while (_widgetStack.Count > 1)
            {
                var widget = _widgetStack.Pop();
                var route = _routeStack.Pop();
                Destroy(widget.gameObject);

                if (route != null)
                {
                    route.OnPop();
                    route.popCompleter.SetResult(result);   
                }
                
                if (TopWidget is T)
                {
                    TopWidget.SetNaviActive(true);
                    
                    // Focus on the last selected object
                    if (focusAutomatically && route?.LastSelected != null)
                        EventSystem.current.SetSelectedGameObject(route.LastSelected);
                    
                    onPopped?.Invoke(route);
                    
                    break;
                }
                // else route?.popCompleter.SetResult(default);
            }
        }
        
        public enum DeactivateMode
        {
            GameObject,     // Use `SetActive(false)` to deactivate the widget
            CanvasGroup,    // Use `alpha` & `interactable` of CanvasGroup to deactivate the widget
            Canvas 
        }
    }
}

namespace Luna.UI
{
    public partial class Widget
    {
        internal void SetNaviActive(bool active)
        {
            switch (Navigator.deactivateMode)
            {
                case Navigator.DeactivateMode.GameObject:
                    this.gameObject.SetActive(active);
                    break;
                case Navigator.DeactivateMode.CanvasGroup:
                    this.Active = active;
                    break;
                case Navigator.DeactivateMode.Canvas:
                    this.Canvas.enabled = active;
                    break;
            }
        }
    }
}