using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Shader
{
    [ExecuteAlways]
    [RequireComponent(typeof(Image))]
    public class CircularLoadingIndicatorShader: MonoBehaviour
    {
        private Image _image;
        private Material _material;
        
        
        private static readonly int ColorID = UnityEngine.Shader.PropertyToID("_BaseColor");
        
        [SerializeField, OnChanged(nameof(OnColorChange))]
        private Color _color = Color.white;
        public Color Color
        {
            get => _color;
            set
            {
                _color = value;
                OnColorChange();
            }
        }
        
        private static readonly int RadiusID = UnityEngine.Shader.PropertyToID("_Radius");
        
        [SerializeField, OnChanged(nameof(OnRadiusChange))]
        private float _radius = 0.25f;
        public float Radius
        {
            get => _radius;
            set
            {
                _radius = value;
                OnRadiusChange();
            }
        }
        
        private static readonly int ThicknessID = UnityEngine.Shader.PropertyToID("_Thickness");
        
        [SerializeField, OnChanged(nameof(OnThicknessChange))]
        private float _thickness = 0.05f;
        public float Thickness
        {
            get => _thickness;
            set
            {
                _thickness = value;
                OnThicknessChange();
            }
        }
        
        private static readonly int SpeedID = UnityEngine.Shader.PropertyToID("_Speed");
        
        [SerializeField, OnChanged(nameof(OnSpeedChange))]
        private float _speed = 1f;
        public float Speed
        {
            get => _speed;
            set
            {
                _speed = value;
                OnSpeedChange();
            }
        }
        

        private void Awake()
        {
            _image = GetComponent<Image>();
        }
        

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Set the material to the image's material
            var path = AssetDatabase.GUIDToAssetPath("ee8c94336ae6b164e96665480ed5065f");
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            var image = GetComponent<Image>();
            if (image.material == null)
                image.material = new Material(material);
        }
#endif
        
        public void OnColorChange()
        {
            _material.SetColor(ColorID, _color);
        }
        
        public void OnRadiusChange()
        {
            _material.SetFloat(RadiusID, _radius);
        }
        
        public void OnSpeedChange()
        {
            _material.SetFloat(SpeedID, _speed);
        }
        
        public void OnThicknessChange()
        {
            _material.SetFloat(ThicknessID, _thickness);
        }
        
    }
}