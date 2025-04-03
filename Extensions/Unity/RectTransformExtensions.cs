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
        
        
        /// <summary>
        /// Counts the bounding box corners of the given RectTransform that are visible from the given Camera in screen space.
        /// </summary>
        /// <returns>The amount of bounding box corners that are visible from the Camera.</returns>
        /// <param name="rectTransform">Rect transform.</param>
        /// <param name="camera">Camera.</param>
        private static int CountCornersVisibleFrom(this RectTransform rectTransform, Camera camera)
        {
            Rect screenBounds = new Rect(0f, 0f, Screen.width, Screen.height); // Screen space bounds (assumes camera renders across the entire screen)
            Vector3[] objectCorners = new Vector3[4];
            rectTransform.GetWorldCorners(objectCorners);

            int visibleCorners = 0;
            Vector3 tempScreenSpaceCorner; // Cached
            for (var i = 0; i < objectCorners.Length; i++) // For each corner in rectTransform
            {
                tempScreenSpaceCorner = camera.WorldToScreenPoint(objectCorners[i]); // Transform world space position of corner to screen space
                if (screenBounds.Contains(tempScreenSpaceCorner)) // If the corner is inside the screen
                {
                    visibleCorners++;
                }
            }
            return visibleCorners;
        }

        /// <summary>
        /// Determines if this RectTransform is fully visible from the specified camera.
        /// Works by checking if each bounding box corner of this RectTransform is inside the cameras screen space view frustrum.
        /// </summary>
        /// <returns><c>true</c> if is fully visible from the specified camera; otherwise, <c>false</c>.</returns>
        /// <param name="rectTransform">Rect transform.</param>
        /// <param name="camera">Camera.</param>
        public static bool IsFullyVisibleFrom(this RectTransform rectTransform, Camera camera)
        {
            return CountCornersVisibleFrom(rectTransform, camera) == 4; // True if all 4 corners are visible
        }

        /// <summary>
        /// Determines if this RectTransform is at least partially visible from the specified camera.
        /// Works by checking if any bounding box corner of this RectTransform is inside the cameras screen space view frustrum.
        /// </summary>
        /// <returns><c>true</c> if is at least partially visible from the specified camera; otherwise, <c>false</c>.</returns>
        /// <param name="rectTransform">Rect transform.</param>
        /// <param name="camera">Camera.</param>
        public static bool IsVisibleFrom(this RectTransform rectTransform, Camera camera)
        {
            return CountCornersVisibleFrom(rectTransform, camera) > 0; // True if any corners are visible
        }

        public static bool IsVisible(this RectTransform rectTransform)
        {
            return IsVisibleFrom(rectTransform, Camera.main);
        }
    }
}