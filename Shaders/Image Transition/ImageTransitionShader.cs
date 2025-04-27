using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Modules.Shader
{
    [RequireComponent(typeof(Image))]
    public class ImageTransitionShader: MonoBehaviour
    {
        private const string TextureName = "_Base_Map";
        private const string BufferName = "_Buffer";
        private const string DurationProperty = "_Duration";
        private const string LastTimeProperty = "_TransitionTime";
        
        public List<Sprite> sprites;
        public float duration = 1f;
        //public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

        private Image _image;
        private Material _material;
        
        private float _time;
        private bool _isPlaying;
        private int _index;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _material = _image.material;
            _material.SetTexture(TextureName, _image.sprite.texture);
            _material.SetTexture(BufferName, sprites[_index].texture);
            _material.SetFloat(DurationProperty, duration);
        }

        private void Update()
        {
            _time += Time.deltaTime;
            
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Play();
            }
        }
        
        public async Task Play()
        {
            if (_isPlaying) return;
            _isPlaying = true;
            
            // _image.sprite = sprite;
            _material.SetTexture(TextureName, sprites[_index].texture);
            _material.SetFloat(LastTimeProperty, Time.time);
            await UniTask.Delay(TimeSpan.FromSeconds(duration));
            _isPlaying = false;
            _material.SetTexture(BufferName, sprites[_index].texture);
            
            _index = (_index + 1) % sprites.Count;
        }

        public void Select(int index)
        {
            _index = index;
            Play();
        }
    }
}