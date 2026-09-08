// Created by LunarEclipse on 2024-7-7 20:39.

using System;
using Luna.Extensions.Unity;
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


        /// The event will be triggered when a key is pressed or released immediately
        /// before key event being handled by other components.
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
        
        /// The event will be triggered when a key is pressed or released at the end of the frame.
        private event KeyEventHandler _OnLateKey;
        public event KeyEventHandler OnLateKey
        {
            add
            {
                Debug.Log("OnKey added.");
                _OnLateKey += value;
                Subscribed = _OnLateKey != null;
            }
            remove
            {
                Debug.Log("OnKey removed.");
                _OnLateKey -= value;
                Subscribed = false;
            }
        }
        
        /// The event will be triggered when an input is started or ended immediately
        /// before input event being handled by other components.
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
        
        /// The event will be triggered when an input is started or ended at the end of the frame.
        /// This event is useful when you want to handle input after all other components have handled it.
        private event InputEventHandler _OnLateInput;
        public event InputEventHandler OnLateInput
        {
            add
            {
                Debug.Log("OnInput added.");
                _OnLateInput += value;
                Subscribed = _OnLateInput != null;
            }
            remove
            {
                Debug.Log("OnInput removed.");
                _OnLateInput -= value;
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
                // Debug.Log(control); 
                // Debug.Log(control.IsPressed());
                // Debug.Log(control.IsActuated());

                var inputEvent = control.IsPressed() ? InputEvent.End : InputEvent.Start;
                var keyEvent = control.IsPressed() ? KeyEvent.Up : KeyEvent.Down;
                _OnInput?.Invoke(control, inputEvent);
                
                if (control is KeyControl keyControl)
                {
                    var result = _OnKey?.Invoke(keyControl, keyEvent);
                    if (result == KeyEventResult.Handled)
                        eventPtr.handled = true;
                }
                
                // Run at the end of the frame.
                if (_OnLateInput != null || _OnLateKey != null)
                    DispatchLateInput(control, inputEvent, keyEvent);
            }
        }

        private async void DispatchLateInput(InputControl control, InputEvent inputEvent, KeyEvent keyEvent)
        {
            if (this == null) return;
            var cancellationToken = this.GetCancellationTokenOnDestroy();
            try
            {
                // InputEventPtr memory is only valid during the synchronous input callback.
                await Awaitable.EndOfFrameAsync(cancellationToken);
                if (this == null) return;
                if (control is KeyControl keyControl)
                    _OnLateKey?.Invoke(keyControl, keyEvent);
                _OnLateInput?.Invoke(control, inputEvent);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Destroying the widget cancels pending callbacks.
            }
        }
    }
    
    public enum KeyEvent
    {
        /// The key was pressed.
        Down,
        /// The key was released.
        Up,
        /// The key was held down.
        // KeyHold,
    }
    
    public enum InputEvent
    {
        Start,
        End,
    }
    
    public enum KeyEventResult
    {
        /// The key event has been handled, and the event should not be propagated to
        /// other key event handlers.
        Handled,
        /// The key event has not been handled, and the event should continue to be
        /// propagated to other key event handlers, even non-Flutter ones.
        Unhandled,
    }
}
