using UnityEngine;

namespace Luna.Extensions.Unity
{
    public static class RectTransformExtensions
    {
        public static float Top(this RectTransform rectTransform)
        {
            return rectTransform.offsetMax.y;
        }
        
        public static void SetTop(this RectTransform rectTransform, float value)
        {
            rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, value);
        }
        
        public static float Bottom(this RectTransform rectTransform)
        {
            return rectTransform.offsetMin.y;
        }
        
        public static void SetBottom(this RectTransform rectTransform, float value)
        {
            rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, value);
        }
        
        public static float Left(this RectTransform rectTransform)
        {
            return rectTransform.offsetMin.x;
        }
        
        public static void SetLeft(this RectTransform rectTransform, float value)
        {
            rectTransform.offsetMin = new Vector2(value, rectTransform.offsetMin.y);
        }
        
        public static float Right(this RectTransform rectTransform)
        {
            return rectTransform.offsetMax.x;
        }
        
        public static void SetRight(this RectTransform rectTransform, float value)
        {
            rectTransform.offsetMax = new Vector2(value, rectTransform.offsetMax.y);
        }
        
        public static Vector2 Center(this RectTransform rectTransform)
        {
            return new Vector2(rectTransform.offsetMin.x + rectTransform.rect.width / 2, rectTransform.offsetMin.y + rectTransform.rect.height / 2);
        }
        
        public static void SetCenter(this RectTransform rectTransform, Vector2 value)
        {
            var halfWidth = rectTransform.rect.width / 2;
            var halfHeight = rectTransform.rect.height / 2;
            rectTransform.offsetMin = new Vector2(value.x - halfWidth, value.y - halfHeight);
            rectTransform.offsetMax = new Vector2(value.x + halfWidth, value.y + halfHeight);
        }
    }
}