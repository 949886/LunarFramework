// Created by LunarEclipse on 2024-6-19 6:25.

using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Luna.UI.Navigation
{
    public class Route
    {
        public Widget To;
        public Widget From;
        
        public GameObject LastSelected { get; set; }
        
        /// A task which completes when the widget is popped.
        /// It takes a dynamic value that passes from the popped widget to the previous widget.
        public Task<dynamic> Popped => popCompleter.Task;
        
        internal readonly TaskCompletionSource<dynamic> popCompleter = new();

        protected Navigator _navigator;
        
        // Constructor
        public Route(Widget to)
        {
            this.To = to;
            this.From = Navigator.Instance.TopWidget;
            _navigator = Navigator.Instance;
        }
        
        public virtual Widget GetTarget()
        {
            return To;
        }
        
        /// Called when the widget will be pushed.
        /// Override this method to perform custom actions.
        public virtual void OnPush() {}
        public virtual void OnPop() {}
    }
    
    public class Route<T>: Route where T: Widget
    {
        public Action<T> callback;
        
        public new T To
        {
            get => base.To as T;
            set => base.To = value;
        }

        public Route(Action<T> callback = null): base(Widget.New<T>(false))
        {
            this.callback = callback;
            To.transform.SetParent(_navigator.canvas.transform, false);
        }
        
        public Route(T to, Action<T> callback = null): base(to)
        {
            this.callback = callback;
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