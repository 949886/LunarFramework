// Created by LunarEclipse on 2024-10-02 10:10.

#if USE_UGUI

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Luna.Extensions
{
    public class AudioButton : Button
    {
        public AudioClip clickAudio;
        public AudioClip submitAudio;
        public AudioClip hoverAudio;
        public AudioClip selectAudio;
        
        public bool mute = false;
        public bool focusOnEnable = false;
        
        private bool _initialized = false;
        
        private SelectionState _state = SelectionState.Disabled;
        
        
        protected override void Start()
        {
            base.Start();
            _initialized = true;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            if (focusOnEnable) 
                EventSystem.current.SetSelectedGameObject(gameObject);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            PlayAudio(clickAudio);
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            base.OnSubmit(eventData);
            PlayAudio(submitAudio);
        }
        
        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);
        
            Debug.Log("DoStateTransition: " + state);
            
            if (_state == state || !_initialized)
            {
                _state = state;
                return;
            }

            switch (state)
            {
                case SelectionState.Normal: break;
                case SelectionState.Highlighted:
                    PlayAudio(hoverAudio);
                    break;
                case SelectionState.Pressed:break;
                case SelectionState.Selected:
                    if (_state is not (SelectionState.Disabled or SelectionState.Pressed))
                        PlayAudio(selectAudio);
                    break;
                case SelectionState.Disabled:break;
            }
            
            _state = state;
        }
        
        private void PlayAudio(AudioClip clip)
        {
            if (!_initialized || !interactable || mute || clip == null) return;
            SFXManager.Play(clip);
        }
    }

}

#endif