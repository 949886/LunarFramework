using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Luna.Extensions.Timeline
{
    [Serializable]
    [NotKeyable]
    class AudiosClipProperties : PlayableBehaviour
    {
        [Range(0.0f, 1.0f)]
        public float volume = 1.0f;
    }
}
