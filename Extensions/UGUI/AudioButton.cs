// Created by LunarEclipse on 2024-10-02 10:10.

#if USE_UGUI

using Luna.UI.Audio;
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
        
        public bool focusOnEnable = false;
        
        protected override void OnEnable()
        {
            base.OnEnable();
        
            if (focusOnEnable) 
                EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            SFXManager.Play(clickAudio);
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            base.OnSubmit(eventData);
            SFXManager.Play(submitAudio);
        }
        
        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);
        
            Debug.Log("DoStateTransition: " + state);

            switch (state)
            {
                case SelectionState.Normal: break;
                case SelectionState.Highlighted:
                    SFXManager.Play(hoverAudio);
                    break;
                case SelectionState.Pressed:break;
                case SelectionState.Selected:
                    SFXManager.Play(selectAudio);
                    break;
                case SelectionState.Disabled:break;
            }
        }
    }

}

#endif