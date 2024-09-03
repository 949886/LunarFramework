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
        
        /// Get the position of the RectTransform in screen space.
        /// The origin is at the bottom-left corner of the screen.
        public static Vector2 PositionOnScreen(this RectTransform rectTransform)
        {
            // Convert the world position to screen point
            var worldPosition = rectTransform.position;
            var screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPosition);
            
            return screenPoint;
        }
        
        /// Get the position of the RectTransform relative to the pivot of the Canvas.
        public static Vector2 PositionOnCanvas(this RectTransform rectTransform)
        {
            var canvas = rectTransform.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No canvas found in parent.");
                return Vector2.zero;
            }
            
            var screenPoint = PositionOnScreen(rectTransform);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, screenPoint, canvas.worldCamera, out var position);
            return position;
        }
    }
}