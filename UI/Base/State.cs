// Created by LunarEclipse on 2024-6-21 3:16.

using System;

namespace Luna.UI
{
    public abstract class State
    {
        public Widget Widget { get; internal set; }

        public virtual void InitState() { }

        public virtual Widget Build() => Widget;
        
        public virtual void OnDispose() { }

        public virtual void Dispose() { }

        public virtual void SetState(Action setStateAction)
        {
            setStateAction?.Invoke();
            Widget = Build();
        }
    }
}