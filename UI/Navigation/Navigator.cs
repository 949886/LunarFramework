// Created by LunarEclipse on 2024-6-18 22:17.

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
        
        
        public Canvas canvas; // Reference to the Canvas
        public GameObject rootWidget; // The root widget of the game
        
        public bool focusAutomatically = true; // Focus on last selected object when a new widget is popped
        public bool escToPop = false; // Pop the top widget when the escape key is pressed
        
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
                if (!(canvas = FindObjectOfType<Canvas>().rootCanvas))
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

        public static void Push(GameObject widgetPrefab)
        {
            Instance._Push(widgetPrefab);
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
            return Instance._Push(callback);
        }
        
        public static void Pop()
        {
            Instance._Pop(0);
        }
        
        public static void Pop<T>(T result = default)
        {
            Instance._Pop(result);
        }
        
        public static void PopToRoot()
        {
            Instance._PopToRoot();
        }

        protected GameObject _Push(GameObject widgetPrefab)
        {
            if (_widgetStack.Count > 0)
                _widgetStack.Peek().SetActive(false);

            GameObject newWidget = Instantiate(widgetPrefab, canvas.transform);
            _widgetStack.Push(newWidget);
            return newWidget;
        }
        
        protected Task<dynamic> _Push<T>(Action<T> callback = null) where T : Widget
        {
            var route = new Route();
            route.lastSelected = EventSystem.current.currentSelectedGameObject;
            
            if (_widgetStack.Count > 0)
                _widgetStack.Peek().SetActive(false);
            
            if (Widget.Dictionary.TryGetValue(typeof(T), out GameObject widgetPrefab))
            {
                widgetPrefab.SetActive(false);
                
                // Instantiating the widget.
                // Widget will execute Awake method.
                var newWidget = Instantiate(widgetPrefab, canvas.transform);
                _widgetStack.Push(newWidget);
                _routeStack.Push(route);
                
                // Executing the callback.
                if (callback is not null)
                    callback.Invoke(newWidget.GetComponent<T>());
                
                // Setting widget active.
                // Widget will execute OnEnable and Start methods.
                newWidget.SetActive(true);
                widgetPrefab.SetActive(true);
            }
            else Debug.LogError($"[Navigator] Widget of type {typeof(T)} not found.");
            
            return route.Popped;
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
                if (focusAutomatically && route.lastSelected != null)
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