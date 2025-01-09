// Created by LunarEclipse on 2025-01-08

using System;
using Cysharp.Threading.Tasks;
using TMPro;
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
            
            await UniTask.NextFrame();
            this.Select();
        }
    }
}