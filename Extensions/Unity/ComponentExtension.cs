// Created by LunarEclipse on 2024-01-11 23:09.

using UnityEngine;

namespace Luna.Extensions.Unity
{
    public static class ComponentExtension
    {
        public static void SetTransform(this Component gameObject, Transform newTransform)
        {
            var transform = gameObject.transform;
            transform.position = newTransform.position;
            transform.rotation = newTransform.rotation;
            transform.localScale = newTransform.localScale;
        }
        
        public static void Hide(this Component gameObject)
        {
            gameObject.transform.localScale = Vector3.zero;
        }
        
        public static void Show(this Component gameObject)
        {
            gameObject.transform.localScale = Vector3.one;
        }
    }
}