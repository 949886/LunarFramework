// Created by LunarEclipse on 2024-6-21 3:15.

using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace Luna.UI
{
    public abstract partial class Widget : MonoBehaviour
    {
        protected bool isDirty = false;
        
        protected Widget() {}
        
        public static T Create<T>(Transform parent = null) where T : Widget
        {
            // Find the widget in the database.
            var widget = Widget.Dictionary[typeof(T)];
            if (widget != null)
                return Instantiate(widget, parent).GetComponent<T>();

            // Create a new widget.
            var newWidget = new GameObject(typeof(T).Name).AddComponent<T>();
            newWidget.transform.SetParent(parent, false);
            return newWidget;
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