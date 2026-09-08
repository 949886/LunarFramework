#if USE_TEXTMESHPRO

using System;
#if !UNITY_2023_1_OR_NEWER
using Cysharp.Threading.Tasks;
#endif
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

#if UNITY_2023_1_OR_NEWER
            await Awaitable.EndOfFrameAsync();
#else
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
#endif
            
            if (!editOnFocus)
                DeactivateInputField();
        }
    }
}

#endif