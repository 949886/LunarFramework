// Created by LunarEclipse on 2024-6-19 6:25.

using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Luna.UI.Navigation
{
    public class Route
    {
        public Action<Widget> onPushed;
        public Action<Widget> onPopped;

        // public dynamic pendingResult;
        public GameObject lastSelected;
        
        internal Task<dynamic> Popped => popCompleter.Task;
        internal readonly TaskCompletionSource<dynamic> popCompleter = new();
    }
}