// Created by LunarEclipse on 2024-7-11 3:30.

using UnityEngine.UI;

namespace Luna.Extensions.Unity
{
    using UnityEngine;

    public static class CanvasExtensions
    {
        public static Vector2 GetScaleFactor(this Canvas canvas, bool ignoreScreenScale = false)
        {
            var screenScaleFactor = canvas.GetScreenScaleFactor();
            Vector2 proportion = Vector2.one / canvas.transform.localScale;
            return ignoreScreenScale ? proportion : new Vector2(proportion.x / screenScaleFactor.x, proportion.y / screenScaleFactor.y);
        }
        
        public static Vector2 GetScreenScaleFactor(this Canvas canvas)
        {
            var canvasScaler = canvas.GetComponent<CanvasScaler>();
            if (canvasScaler == null)
            {
                Debug.LogError("CanvasScaler component not found on the canvas.");
                return Vector2.one;
            }

            var referenceResolution = canvasScaler.referenceResolution;
            var currentResolution = Screen.currentResolution;

            return new Vector2(currentResolution.width / referenceResolution.x, currentResolution.height / referenceResolution.y);
        }
        
        public static Vector2 GetScreenScaleFactor(this CanvasScaler canvasScaler)
        {
            var referenceResolution = canvasScaler.referenceResolution;
            var currentResolution = new Vector2(Screen.width, Screen.height);

            return new Vector2(currentResolution.x / referenceResolution.x, currentResolution.y / referenceResolution.y);
        }
    }
}