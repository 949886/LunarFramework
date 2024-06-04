#if USE_VISUAL_EFFECT_GRAPH

using UnityEngine;

namespace Pyro
{
    public class VFXFloatProperty : MonoBehaviour
    {
        public float value {
            get { return _value; }
            set { _value = value; }
        }

        [SerializeField] float _value;
    }
}

#endif