// Created by LunarEclipse on 2024-8-7 15:14.

using System;
using UnityEngine;

namespace Luna.UI.Navigation
{
    public class ModalRoute<T>: Route<T> where T: Widget
    {
        public bool maskDismissible = true;
        public Color maskColor = new Color(0, 0, 0, 0.5f);
        public Vector2 offset = Vector2.zero;
        
        // Constructor with all parameters
        public ModalRoute(Action<T> builder = null, bool maskDismissible = true, Color? maskColor = null, Vector2 offset = default) : base(builder)
        {
            this.maskDismissible = maskDismissible;
            this.maskColor = maskColor ?? this.maskColor;
            this.offset = offset;
        }
        
        public override void OnPush()
        {
            var go = To.gameObject;
            
            // Disable the previous widget
            From.GetComponent<CanvasGroup>().interactable = false;
            From.enabled = false;
            
            // Add modal mask
            var mask = new GameObject("ModalMask").AddComponent<ModalMask>();
            mask.color = maskColor;
            mask.raycastTarget = maskDismissible;
            mask.transform.SetParent(go.transform, false);
            mask.transform.SetSiblingIndex(0);
            
            // Set stretch
            var rect = To.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = Vector2.zero;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.anchoredPosition += offset;
            }
        }
        
        public override void OnPop()
        {
            From.enabled = true;
            From.GetComponent<CanvasGroup>().interactable = true;
        }
    }
}