// Created by LunarEclipse on 2024-6-19 6:25.

using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Luna.UI.Navigation
{
    public class Route
    {
        // public dynamic pendingResult;
        public GameObject lastSelected { get; internal set; }
        
        /// A task which completes when the widget is popped.
        /// It takes a dynamic value that passes from the popped widget to the previous widget.
        public Task<dynamic> Popped => popCompleter.Task;
        
        internal readonly TaskCompletionSource<dynamic> popCompleter = new();
    }
    
    public class Route<T>: Route where T: Widget
    {
        public Action<T> callback;
        
        public Route(Action<T> callback = null)
        {
            this.callback = callback;
        }
        
        public virtual void OnPush(T widget)
        {
            
        }
        
        public virtual void OnPop(T widget)
        {
            
        }
        
        // Implicit conversion from Action to Route
        public static implicit operator Route<T>(Action<T> callback)
        {
            return new Route<T>(callback);
        }
        
        // Implicit conversion from Route to Action
        public static implicit operator Action<T>(Route<T> route)
        {
            return route.callback;
        }
    }
}