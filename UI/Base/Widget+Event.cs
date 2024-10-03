// Created by LunarEclipse on 2024-7-7 20:39.

using Cysharp.Threading.Tasks;
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

        
        private async void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
        {
            // Debug.Log(eventPtr);
            if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
                return;
 
            foreach (var control in eventPtr.EnumerateChangedControls())
            {
                // Debug.Log(control); 
                // Debug.Log(control.IsPressed());
                // Debug.Log(control.IsActuated());

                _OnInput?.Invoke(control, control.IsPressed() ? InputEvent.End : InputEvent.Start);
                
                if (control is KeyControl keyControl)
                {
                    var result = _OnKey?.Invoke(keyControl, control.IsPressed() ? KeyEvent.Up : KeyEvent.Down);
                    if (result == KeyEventResult.Handled)
                        eventPtr.handled = true;
                }
                
                // Run at the end of the frame.
                if (_OnLateInput != null || _OnLateKey != null)
                {   
                    await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, cancellationToken: this.GetCancellationTokenOnDestroy());
                    if (control is KeyControl keyControl2)
                        _OnLateKey?.Invoke(keyControl2, control.IsPressed() ? KeyEvent.Up : KeyEvent.Down);
                    _OnLateInput?.Invoke(control, control.IsPressed() ? InputEvent.End : InputEvent.Start);
                }
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