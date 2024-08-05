﻿// Created by LunarEclipse on 2024-6-18 22:17.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Luna.UI.Navigation
{
    public class Navigator : MonoBehaviour
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
        
        private readonly Stack<GameObject> _widgetStack = new();
        // private readonly Stack<GameObject> _widgetHistory = new();
        private readonly Stack<Route> _routeStack = new();
        
        private bool _isDontDestroyOnLoad = false;
        
        
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
                if (rootWidget.activeInHierarchy == false)
                {
                    Push(rootWidget);
                }
                else
                {
                    _widgetStack.Push(rootWidget);
                }
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
        

        public static Task<dynamic> Push(GameObject widgetPrefab, Action<GameObject> callback = null)
        {
            return Instance._Push(widgetPrefab, callback);
        }
        
        /// <summary>
        /// Push a widget to the top of the stack.
        /// The widget will be instantiated and added to the canvas.
        /// </summary>
        /// <param name="callback">
        /// A callback to be executed after the widget is instantiated.
        /// You can use this callback to pass data to the widget.
        /// Note: The callback will be executed before the widget is enabled.
        /// Therefore, you should initialize the widget in Awake method.
        /// </param>
        /// <typeparam name="T">
        /// The type of the widget to be pushed.
        /// Prefab with Widget component will be registered in the Widgets prefab database automatically.
        /// Note: A widget type should only have one prefab that corresponds to it.
        /// </typeparam>
        public static Task<dynamic> Push<T>(Action<T> callback = null) where T : Widget
        {
            return Instance._Push<T>(callback);
        }
        
        public static Task<dynamic> PushReplacement<T>(Action<T> callback = null) where T : Widget
        {
            return Instance._PushReplacement(callback);
        }
        
        /// <summary>
        /// Pop the top widget from the stack.
        /// </summary>
        public static void Pop()
        {
            Instance._Pop(0);
        }
        
        /// <summary>
        /// Pop the top widget from the stack with a result.
        /// </summary>
        /// <param name="result">
        /// The result to be passed to the previous widget.
        /// </param>
        /// <typeparam name="T">
        /// The type of the result to be passed to the previous widget.
        /// </typeparam>
        public static void Pop<T>(T result = default)
        {
            Instance._Pop(result);
        }
        
        public static void PopToRoot()
        {
            Instance._PopToRoot();
        }

        protected Task<dynamic> _Push(GameObject widgetPrefab, Action<GameObject> callback = null)
        {
            if (_widgetStack.Count > 0)
                _widgetStack.Peek().SetActive(false);
            
            // Instantiating the widget.
            // Widget will execute Awake method.
            var go = Instantiate(widgetPrefab, canvas.transform);
            _widgetStack.Push(go);
            
            var route = new Route();
            route.lastSelected = EventSystem.current.currentSelectedGameObject;
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
        
        
#if USE_ADDRESSABLES
        protected async Task<dynamic> _Push<T>(Action<T> callback = null) where T : Widget
        {
            if (_widgetStack.Count > 0)
                _widgetStack.Peek().SetActive(false);
            
            // Load the widget prefab from addressables.
            var widgetPrefab = Widget.Load<T>();
            if (widgetPrefab == null)
            {
                Debug.LogError($"[Navigator] Widget of type {typeof(T)} not found.");
                return route.Popped;
            }
            
            widgetPrefab.SetActive(false);
                
            // Instantiating the widget.
            // Widget will execute Awake method.
            var newWidget = Instantiate(widgetPrefab, canvas.transform);
            _widgetStack.Push(newWidget);

            var component = newGameObject.GetComponent<T>();
            component.enabled = false;

            var route = new Route();
            route.lastSelected = EventSystem.current.currentSelectedGameObject;
            _routeStack.Push(route);
                
            // Setting game object active.
            // Widget will execute Awake methods.
            newGameObject.SetActive(true);
            widgetPrefab.SetActive(true);

            // Executing the callback.
            if (callback is not null)
                callback.Invoke(component);

            // Enabling the widget component.
            // Widget will execute OnEnable and Start methods.
            widget.enabled = true;

            // Wait for the widget to be popped and release the addressable asset.
            var result = await route.Popped;
            Widget.Unload<T>();
            return result;
        }
#else // USE SCRIPTABLE OBJECT
        protected Task<dynamic> _Push<T>(Route<T> route = null) where T : Widget
        {
            if (_widgetStack.Count > 0)
                _widgetStack.Peek().SetActive(false);

            if (Widget.Dictionary.TryGetValue(typeof(T), out GameObject widgetPrefab))
            {
                widgetPrefab.SetActive(false);
                
                // Instantiating the widget.
                var newGameObject = Instantiate(widgetPrefab, canvas.transform);
                _widgetStack.Push(newGameObject);
                
                var widget = newGameObject.GetComponent<T>();
                widget.enabled = false;
                
                // Creating a new navigation route if not provided.
                if (route == null)
                    route = new Route<T>();
                route.lastSelected = EventSystem.current.currentSelectedGameObject;
                _routeStack.Push(route);
                
                // Setting game object active.
                // Widget will execute Awake methods.
                newGameObject.SetActive(true);
                widgetPrefab.SetActive(true);
                
                // Executing the callback.
                if (route is not null)
                    route.callback?.Invoke(widget);
                
                // Enabling the widget component.
                // Widget will execute OnEnable and Start methods.
                widget.enabled = true;
            }  
            else Debug.LogError($"[Navigator] Widget of type {typeof(T)} not found.");
            return route.Popped;
        }
#endif
        
        protected Task<dynamic> _PushReplacement<T>(Action<T> callback = null) where T : Widget
        {
            if (_widgetStack.Count > 0)
            {
                Destroy(_widgetStack.Pop());
                _routeStack.Pop();
            }
            
            return _Push<T>(callback);
        }

        protected void _Pop<T>(T result = default)
        {
            if (_widgetStack.Count > 1)
            {
                // Pop & destroy the top widget
                GameObject topWidget = _widgetStack.Pop();
                Destroy(topWidget);
                
                // Pass the result to the previous widget
                var route = _routeStack.Pop();
                if (route != null)
                    route.popCompleter.SetResult(result);
                
                // Show the previous widget
                if (_widgetStack.Count > 0)
                    _widgetStack.Peek().SetActive(true);
                
                // Focus on the last selected object
                if (focusAutomatically && route?.lastSelected != null)
                    EventSystem.current.SetSelectedGameObject(route.lastSelected);
            }
        }

        protected void _PopToRoot()
        {
            while (_widgetStack.Count > 1)
            {
                Destroy(_widgetStack.Pop());
            }

            if (_widgetStack.Count > 0)
            {
                _widgetStack.Peek().SetActive(true);
            }
        }
    }

}