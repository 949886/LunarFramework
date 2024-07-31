// Created by LunarEclipse on 2024-6-19 6:25.

using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Luna.UI.Navigation
{
    public class Route
    {
        // public Action<Widget> onPushed;
        // public Action<Widget> onPopped;

        // public dynamic pendingResult;
        public GameObject lastSelected;
        
        /// A task which completes when the widget is popped.
        /// It takes a dynamic value that passes from the popped widget to the previous widget.
        internal Task<dynamic> Popped => popCompleter.Task;
        
        internal readonly TaskCompletionSource<dynamic> popCompleter = new();
    }
}