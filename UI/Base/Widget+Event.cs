// Created by LunarEclipse on 2024-7-7 20:39.

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

namespace Luna.UI
{
    public partial class Widget
    {
        public delegate KeyEventResult KeyEventHandler(KeyControl keyCode, KeyEvent keyEvent);
        
        public delegate void InputEventHandler(InputControl input, InputEvent inputEvent);


        private event KeyEventHandler _OnKey;
        public event KeyEventHandler OnKey
        {
            add
            {
                Debug.Log("OnKey added.");
                _OnKey += value;
                Subscribed = _OnKey != null;
            }
            remove
            {
                Debug.Log("OnKey removed.");
                _OnKey -= value;
                Subscribed = false;
            }
        }
        
        private event InputEventHandler _OnInput;
        public event InputEventHandler OnInput
        {
            add
            {
                Debug.Log("OnInput added.");
                _OnInput += value;
                Subscribed = _OnInput != null;
            }
            remove
            {
                Debug.Log("OnInput removed.");
                _OnInput -= value;
                Subscribed = false;
            }
        }

        private bool _subscribed = false;
        private bool Subscribed
        {
            get => _subscribed;
            set
            {
                if (_subscribed == value)
                    return;
                
                _subscribed = value;
                if (_subscribed)
                    UnityEngine.InputSystem.InputSystem.onEvent += OnInputEvent;
                else UnityEngine.InputSystem.InputSystem.onEvent -= OnInputEvent;
            }
        }

        
        private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
        {
            // Debug.Log(eventPtr);
            if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
                return;
 
            foreach (var control in eventPtr.EnumerateChangedControls())
            {
                Debug.Log(control); 
                // Debug.Log(control.IsPressed());
                // Debug.Log(control.IsActuated());

                _OnInput.Invoke(control, control.IsPressed() ? InputEvent.End : InputEvent.Start);
                
                if (control is KeyControl keyControl)
                {
                    var result = _OnKey?.Invoke(keyControl, control.IsPressed() ? KeyEvent.Up : KeyEvent.Down);
                    if (result == KeyEventResult.Handled)
                    {
                        eventPtr.handled = true;
                    }
                }
                
            }
        }
    }
}