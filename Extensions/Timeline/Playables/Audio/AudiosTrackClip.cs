using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;
#if UNITY_EDITOR
using System.ComponentModel;
#endif
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.Timeline;
using Random = System.Random;

namespace Luna.Extensions.Timeline
{
    /// <summary>
    /// PlayableAsset wrapper for an AudioClip in Timeline.
    /// </summary>
    [Serializable]
#if UNITY_EDITOR
    [DisplayName("Audio Clip")]
#endif
    public class AudiosTrackClip : PlayableAsset, ITimelineClipAsset, IPlayableBehaviour
    {
        [TableList] public List<WeightedAudioClip> clips = new();
        
        // The probability of whether the audio clip will play or not.
        [Range(0.0f, 1.0f)] public float probability = 0.5f;
        
        // Whether the audio clip loops.
        // Use this to loop the audio clip when the duration of the timeline clip exceeds that of the audio clip.
        [Header("Settings")]
        [SerializeField] bool loop;
        
        [Range(-1.0f, 1.0f)] public float stereoPan = 0.0f;
        [Range(0.0f, 1.0f)] public float spatialBlend = 0.0f;
        
        [SerializeField, HideInInspector] AudiosClipProperties clipProperties = new AudiosClipProperties();
        
        // the amount of time to give the clip to load prior to it's start time
        [SerializeField, HideInInspector] public float bufferingTime = 0.1f;

        private Random _random = new Random();

#if UNITY_EDITOR
        Playable _LiveClipPlayable = Playable.Null;
#endif
        
        /// <summary>
        /// Returns the capabilities of TimelineClips that contain an AudioPlayableAsset
        /// </summary>
        public ClipCaps clipCaps
        {
            get
            {
                return ClipCaps.ClipIn |
                       ClipCaps.SpeedMultiplier |
                       ClipCaps.Blending |
                       (loop ? ClipCaps.Looping : ClipCaps.None);
            }
        }

        /// <summary>
        /// Returns a description of the PlayableOutputs that may be created for this asset.
        /// </summary>
        public override IEnumerable<PlayableBinding> outputs
        {
            get { yield return AudioPlayableBinding.Create(name, this); }
        }

        /// <summary>
        /// Creates the root of a Playable subgraph to play the audio clip.
        /// </summary>
        /// <param name="graph">PlayableGraph that will own the playable</param>
        /// <param name="go">The GameObject that triggered the graph build</param>
        /// <returns>The root playable of the subgraph</returns>
        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            Debug.Log("CreatePlayable");
            
            if (clips.First() == null)
                return Playable.Null;

            var audioClipPlayable = AudioClipPlayable.Create(graph, clips.First(), loop);
            audioClipPlayable.GetHandle().SetScriptInstance(this);

#if UNITY_EDITOR
            _LiveClipPlayable = audioClipPlayable;
#endif
            return audioClipPlayable;
        }

#if UNITY_EDITOR
        internal void LiveLink()
        {
            if (!_LiveClipPlayable.IsValid())
                return;

            var audioMixerProperties = _LiveClipPlayable.GetHandle().GetObject<AudiosClipProperties>();

            if (audioMixerProperties == null)
                return;

            audioMixerProperties.volume = clipProperties.volume;
        }
#endif
        
        public void OnGraphStart(Playable playable)
        {

        }

        public void OnGraphStop(Playable playable)
        {

        }

        public void OnPlayableCreate(Playable playable)
        {

        }

        public void OnPlayableDestroy(Playable playable)
        {

        }

        public void OnBehaviourPlay(Playable playable, FrameData info)
        {
            var next = _random.NextDouble();
            Debug.Log($"OnBehaviourPlay {next}");
            var clip = clips[UnityEngine.Random.Range(0, clips.Count)];
            var audioClipPlayable = (AudioClipPlayable)playable;
            if (next < probability)
                audioClipPlayable.SetVolume(clip.volume);
            audioClipPlayable.SetClip(clip.audioClip);
            audioClipPlayable.SetStereoPan(Mathf.Clamp(stereoPan, -1.0f, 1.0f));
            audioClipPlayable.SetSpatialBlend(Mathf.Clamp01(spatialBlend));
        }

        public void OnBehaviourPause(Playable playable, FrameData info)
        {
            Debug.Log("OnBehaviourPause");
            var clip = clips[UnityEngine.Random.Range(0, clips.Count)];
            var audioClipPlayable = (AudioClipPlayable)playable;
            audioClipPlayable.SetVolume(0);
        }

        public void PrepareFrame(Playable playable, FrameData info)
        {

        }

        public void ProcessFrame(Playable playable, FrameData info, object playerData)
        {

        }
        
                
        [Serializable]
        public class WeightedAudioClip
        {
            [TableColumnWidth(200)]
            public AudioClip audioClip;
            public float weight = 1.0f;
            public float volume = 1.0f;
            
            public static implicit operator AudioClip(WeightedAudioClip weightedClip)
            {
                return weightedClip.audioClip;
            }
            
            public static implicit operator WeightedAudioClip(AudioClip clip)
            {
                return new WeightedAudioClip { audioClip = clip };
            }
        }

    }
}
