// Created by LunarEclipse on 2024-10-02 10:10.

#if USE_UGUI

#if !UNITY_2023_1_OR_NEWER
using Cysharp.Threading.Tasks;
#endif
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
        
        private bool _ready = false;
        
        private SelectionState _state = SelectionState.Disabled;
        
        
        protected override void Start()
        {
            base.Start();
            _ready = true;
        }

        protected override async void OnEnable()
        {
            base.OnEnable();
            
            if (focusOnEnable) 
                EventSystem.current.SetSelectedGameObject(gameObject);

#if UNITY_2023_1_OR_NEWER
            await Awaitable.NextFrameAsync();
#else
            await UniTask.NextFrame();
#endif
            _ready = true;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _ready = false;
            _state = SelectionState.Disabled;
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            PlayAudio(clickAudio);
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            PlayAudio(submitAudio);
            base.OnSubmit(eventData);
        }
        
        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);
            
            if (_state == state || !_ready)
                return;
            
            _state = state;
            
            // Debug.Log($"DoStateTransition: {state} : {gameObject.activeInHierarchy}");

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
            if (!_ready || !interactable || mute || clip == null) return;
            SFXManager.Play(clip);
        }
    }

}

#endif