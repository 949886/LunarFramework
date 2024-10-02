#if USE_TEXTMESHPRO

using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine.EventSystems;

namespace Luna.Extensions.UGUI
{
    public class TextField: TMP_InputField, ISelectHandler
    {
        public bool editOnFocus = false;
        
        public override async void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            
            await UniTask.NextFrame();
            
            if (!editOnFocus)
                DeactivateInputField();
        }
    }
}

#endif