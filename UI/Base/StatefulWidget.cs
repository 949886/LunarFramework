// Created by LunarEclipse on 2024-6-19 0:37.

using UnityEngine;

namespace Luna.UI
{
    public abstract partial class StatefulWidget<T> : Widget where T : State, new()
    {
        public readonly State state = new T();

        protected StatefulWidget()
        {
            state.Widget = this;
        }
        
        protected void Awake()
        {
            state.InitState();
        }
        
        protected void OnDestroy()
        {
            state.OnDispose();
        }
    }
}