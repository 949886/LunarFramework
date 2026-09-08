#if USE_TEXTMESHPRO

using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Luna.Extensions.UGUI
{
    public class TextField: TMP_InputField, ISelectHandler
    {
        public bool editOnFocus = false;
        
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
        
        public override async void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);

            await Awaitable.EndOfFrameAsync();
            
            if (!editOnFocus)
                DeactivateInputField();
        }
    }
}

#endif