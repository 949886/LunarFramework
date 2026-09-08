// Created by LunarEclipse on 2025-01-08

using System;
#if !UNITY_2023_1_OR_NEWER
using Cysharp.Threading.Tasks;
#endif
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Extensions.UGUI
{

    public class ConsoleInputField : TMP_InputField
    {
        // public TMP_InputField inputField;
        
        public event Action<TMP_InputField, string> onInputValueChanged;
        public event Action<TMP_InputField, string> onInputEnd;
        
        void Start()
        {
            base.Start();
            
            this.onValueChanged.AddListener(OnInputValueChanged);
            this.onEndEdit.AddListener(OnInputEnd);
        }

        private void OnInputEnd(string value)
        {
            this.Select();
            onInputEnd?.Invoke(this, value);
        }

        private void OnInputValueChanged(string newValue)
        {
            onInputValueChanged?.Invoke(this, newValue);
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            
            this.DeactivateInputField();
        }
        
        public override async void OnSubmit(BaseEventData eventData)
        {
            base.OnSubmit(eventData);
            
#if UNITY_2023_1_OR_NEWER
            await Awaitable.NextFrameAsync();
#else
            await UniTask.NextFrame();
#endif
            this.Select();
        }
    }
}