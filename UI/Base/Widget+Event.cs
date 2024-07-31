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

        private event KeyEventHandler _OnKey;
        public event KeyEventHandler OnKey
        {
            add
            {
                Debug.Log("OnKey added.");
                _OnKey += value;
                UnityEngine.InputSystem.InputSystem.onEvent += OnInputEvent;
            }
            remove
            {
                Debug.Log("OnKey removed.");
                _OnKey -= value;
                UnityEngine.InputSystem.InputSystem.onEvent -= OnInputEvent;
            }
        }
        
        private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
        {
            // Debug.Log(eventPtr);
            if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
                return;
 
            foreach (var control in eventPtr.EnumerateChangedControls())
            {
                // Debug.Log(control); 
                // Debug.Log(control.IsPressed());
                // Debug.Log(control.IsActuated());
                
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
        
        // protected virtual void OnEnable()
        // {
        //     UnityEngine.InputSystem.InputSystem.onEvent += OnInputEvent;
        // }
        //
        // protected virtual void OnDisable()
        // {
        //     UnityEngine.InputSystem.InputSystem.onEvent -= OnInputEvent;
        // }
        //
        // protected virtual KeyEventResult OnKey(KeyControl keyCode, KeyEvent keyEvent)
        // {
        //     return KeyEventResult.Unhandled;
        // }
        //
        // private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
        // {
        //     // Debug.Log(eventPtr);
        //     if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
        //         return;
        //
        //     foreach (var control in eventPtr.EnumerateChangedControls())
        //     {
        //         // Debug.Log(control); 
        //         // Debug.Log(control.IsPressed());
        //         // Debug.Log(control.IsActuated());
        //         
        //         if (control is KeyControl keyControl)
        //         {
        //             var result = OnKey(keyControl, control.IsPressed() ? KeyEvent.KeyUp : KeyEvent.KeyDown);
        //             if (result == KeyEventResult.Handled)
        //             {
        //                 eventPtr.handled = true;
        //             }
        //         }
        //     }
        // }
    }
}