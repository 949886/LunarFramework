// Created by LunarEclipse on 2024-8-7 17:34.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Luna.UI.Navigation
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class ModalMask: Graphic, IPointerClickHandler
    {
        protected override void Awake()
        {
            base.Awake();
            
            // Set stretch
            var rect = GetComponent<RectTransform>();
            rect.sizeDelta = Vector2.zero;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Close modal
            Navigator.Pop();
        }
    }
}