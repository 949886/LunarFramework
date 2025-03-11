// Created by LunarEclipse on 2024-12-26

using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Luna.UI
{
    [RequireComponent(typeof(ScrollRect))]
    public class BaseListView :  Widget
    {
        public ListSnapMode snapMode = ListSnapMode.End;
        
        protected ScrollRect _scrollRect;
        
        public int PreviousIndex { get; protected set; }
        public int SelectedIndex { get; protected set; }
        
        public Vector2 ScrollOffset
        {
            get => _scrollRect.content.anchoredPosition;
            set => _scrollRect.content.anchoredPosition = value;
        }

        public Rect ScrollSize => ((RectTransform)_scrollRect.transform).rect;
        public Rect ContentSize => _scrollRect.content.rect;
        
        
        /// Override this method to customize the snap behavior.
        public virtual void SnapTo(RectTransform target)
        {
            SnapTo(target, 0.5f);
        }
        
        public virtual void SnapTo(RectTransform target, float duration)
        {
            switch (snapMode)
            {
                case ListSnapMode.End:
                    SnapToEnd(target, duration);
                    break;
                case ListSnapMode.Edge:
                    SnapToEdge(target, duration);
                    break;
            }
        }
        
        // public void SnapTo(int index)
        // {
        //     if (index < cells.Count)
        //         SnapTo(cells[index].transform as RectTransform);
        // }

        protected void SnapToEnd(RectTransform target, float duration = 0.5f)
        {
            var y = -target.offsetMin.y - ((RectTransform)_scrollRect.transform).rect.height;
            y = Mathf.Clamp(y, 0, _scrollRect.content.rect.height);
            var pos = new Vector2(_scrollRect.content.anchoredPosition.x, y);
            DOTween.To(() => _scrollRect.content.anchoredPosition, v => _scrollRect.content.anchoredPosition = v, pos, duration);
        }
        
        protected void SnapToEdge(RectTransform target, float duration = 0.5f)
        {
            float y;
            
            if (SelectedIndex > PreviousIndex) // Scroll down
            {   
                y = -target.offsetMin.y - ScrollSize.height;
                if (y < ScrollOffset.y) return;
            }
            else // Scroll up
            {
                y = -target.offsetMax.y;
                if (y > ScrollOffset.y) return;
            }
            
            y = Mathf.Clamp(y, 0, ContentSize.height);
            var pos = new Vector2(ScrollOffset.x, y);
            DOTween.To(() => ScrollOffset, v => ScrollOffset = v, pos, duration);
        }
    }
}