// Created by LunarEclipse on 2024-7-29 14:38.

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Games.Yamanote
{
    [RequireComponent(typeof(Image))]
    public class ImageShaderController : MonoBehaviour
    {
        [SerializeField, OnChanged(nameof(OnOffsetChange))]
        private Vector2 _offset = Vector2.zero;

        [SerializeField, OnChanged(nameof(OnTilingChange))]
        private Vector2 _tiling = Vector2.one;
        
        [FormerlySerializedAs("_speed")] public Vector2 speed;
        
        private Image _image;
        private Material _material;
        
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        
        public Vector2 Offset
        {
            get => _offset;
            set
            {
                _offset = value;
                OnOffsetChange();
            }
        }
        
        public Vector2 Tiling
        {
            get => _tiling;
            set
            {
                _tiling = value;
                OnTilingChange();
            }
        }

        private void Awake()
        {
            _image = GetComponent<Image>();
            _material = new Material(_image.material);
            _image.material = _material;
        }
        
        private void Update()
        {
            if (speed != Vector2.zero)
            {
                _offset += speed * Time.deltaTime;
                OnOffsetChange();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Set the material to the image's material
            var path = AssetDatabase.GUIDToAssetPath("bcee5b78355a23247821f55cc79d8c7a");
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            var image = GetComponent<Image>();
            if (image.material == null)
                image.material = material;
        }
#endif

        public void OnOffsetChange()
        {
            _material.SetTextureOffset(MainTex, _offset);
        }
        
        public void OnTilingChange()
        {
            _material.SetTextureScale(MainTex, _tiling);
        }
    }
}