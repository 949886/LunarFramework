// Created by LunarEclipse on 2024-12-25

using UnityEngine;
using UnityEngine.UI;

namespace Luna.Extensions
{
    public class StateButton : Button
    {
        public event OnStateChangedDelegate onStateChanged;
        public event OnStateDelegate onNormal;
        public event OnStateDelegate onHighlighted;
        public event OnStateDelegate onPressed;
        public event OnStateDelegate onSelect;
        public event OnStateDelegate onDisabled;

        public bool Initialized { get; private set; } = false;
        public State PreviousState { get; private set; } = State.Disabled;
        public State CurrentState { get; private set; } = State.Disabled;

        protected override void Start()
        {
            base.Start();
            Initialized = true;
        }
        
        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);
        
            Debug.Log("DoStateTransition: " + state);
            
            CurrentState = (State)state;
            if (PreviousState == CurrentState || !Initialized) return;
            
            onStateChanged?.Invoke(CurrentState, PreviousState);

            switch (state)
            {
                case SelectionState.Normal:
                    onNormal?.Invoke(PreviousState);
                    break;
                case SelectionState.Highlighted:
                    onHighlighted?.Invoke(PreviousState);
                    break;
                case SelectionState.Pressed:
                    onPressed?.Invoke(PreviousState);
                    break;
                case SelectionState.Selected:
                    onSelect?.Invoke(PreviousState);
                    break;
                case SelectionState.Disabled:
                    onDisabled?.Invoke(PreviousState);
                    break;
            }
            
            PreviousState = (State)state;
        }
        
        public enum State
        {
            Normal,
            Highlighted,
            Pressed,
            Selected,
            Disabled
        }
        
        public delegate void OnStateDelegate(State previousState);
        public delegate void OnStateChangedDelegate(State currentState, State previousState);
    }
}