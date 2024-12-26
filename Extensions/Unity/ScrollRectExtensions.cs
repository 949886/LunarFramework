// Created by LunarEclipse on 2024-12-26

using UnityEngine;

namespace Luna.Extensions.Unity
{
    public static class ScrollRectExtensions
    {
        public static Vector2 GetScrollOffset(this UnityEngine.UI.ScrollRect scrollRect)
        {
            return scrollRect.content.anchoredPosition;
        }
        
        public static Rect GetScrollSize(this UnityEngine.UI.ScrollRect scrollRect)
        {
            return ((RectTransform)scrollRect.transform).rect;
        }
        
        public static Rect GetContentSize(this UnityEngine.UI.ScrollRect scrollRect)
        {
            return scrollRect.content.rect;
        }
    }
}