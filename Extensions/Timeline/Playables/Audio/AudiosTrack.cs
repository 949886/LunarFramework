using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.Timeline;

namespace Luna.Extensions.Timeline
{
    /// <summary>
    /// A Timeline track that can play AudioClips.
    /// </summary>
    [Serializable]
    // [TrackColor(1f, 0.6f, 0f)]
    [TrackColor(1f, 1f, 0f)]
    [TrackClipType(typeof(AudiosTrackClip), false)]
    [TrackBindingType(typeof(AudioSource))]
    [ExcludeFromPreset]
    [TimelineHelpURL(typeof(AudioTrack))]
    public class AudiosTrack : TrackAsset
    {
        [SerializeField]
        AudiosMixerProperties trackProperties = new AudiosMixerProperties();

#if UNITY_EDITOR
        Playable m_LiveMixerPlayable = Playable.Null;
#endif

        /// <summary>
        /// Create an TimelineClip for playing an AudioClip on this track.
        /// </summary>
        /// <param name="clip">The audio clip to play</param>
        /// <returns>A TimelineClip with an AudioPlayableAsset asset.</returns>
        public TimelineClip CreateClip(AudioClip clip)
        {
            Debug.Log("CreateClip");
            
            if (clip == null)
                return null;

            var newClip = CreateDefaultClip();

            var audioAsset = newClip.asset as AudiosTrackClip;
            if (audioAsset != null)
                audioAsset.clips[0] = clip;

            newClip.duration = clip.length;
            newClip.displayName = clip.name;

            return newClip;
        }

        internal override Playable CompileClips(PlayableGraph graph, GameObject go, IList<TimelineClip> timelineClips, IntervalTree<RuntimeElement> tree)
        {
            Debug.Log("CompileClips");
            
            var clipBlender = AudioMixerPlayable.Create(graph, timelineClips.Count);

#if UNITY_EDITOR
            clipBlender.GetHandle().SetScriptInstance(trackProperties.Clone());
            m_LiveMixerPlayable = clipBlender;
#else
            if (hasCurves)
                clipBlender.GetHandle().SetScriptInstance(m_TrackProperties.Clone());
#endif

            for (int i = 0; i < timelineClips.Count; i++)
            {
                var c = timelineClips[i];
                var asset = c.asset as PlayableAsset;
                if (asset == null)
                    continue;

                var buffer = 0.1f;
                var audioAsset = c.asset as AudiosTrackClip;
                if (audioAsset != null)
                    buffer = audioAsset.bufferingTime;

                var source = asset.CreatePlayable(graph, go);
                if (!source.IsValid())
                    continue;

                if (source.IsPlayableOfType<AudioClipPlayable>())
                {
                    // Enforce initial values on all clips
                    var audioClipPlayable = (AudioClipPlayable)source;
                    var audioClipProperties = audioClipPlayable.GetHandle().GetObject<AudiosTrackClip>();
                    
                    // Get random clip
                    var audioClips = audioClipProperties.clips;
                    var clip = audioClips[UnityEngine.Random.Range(0, audioClips.Count)].audioClip;
                    
                    Debug.Log("clip: " + clip.name);
                    audioClipPlayable.SetClip(clip);
                    // audioClipPlayable.SetVolume(1); 
                    // audioClipPlayable.SetVolume(Mathf.Clamp01(m_TrackProperties.volume * audioClipProperties.volume));
                    audioClipPlayable.SetStereoPan(Mathf.Clamp(trackProperties.stereoPan, -1.0f, 1.0f));
                    audioClipPlayable.SetSpatialBlend(Mathf.Clamp01(trackProperties.spatialBlend));
                }

                tree.Add(new ScheduleRuntimeClip(c, source, clipBlender, buffer));
                graph.Connect(source, 0, clipBlender, i);
                source.SetSpeed(c.timeScale);
                source.SetDuration(c.extrapolatedDuration);
                clipBlender.SetInputWeight(source, 1.0f);
            }

            ConfigureTrackAnimation(tree, go, clipBlender);

            return clipBlender;
        }

        /// <inheritdoc/>
        public override IEnumerable<PlayableBinding> outputs
        {
            get { yield return AudioPlayableBinding.Create(name, this); }
        }

#if UNITY_EDITOR
        internal void LiveLink()
        {
            if (!m_LiveMixerPlayable.IsValid())
                return;

            var audioMixerProperties = m_LiveMixerPlayable.GetHandle().GetObject<AudiosMixerProperties>();

            if (audioMixerProperties == null)
                return;

            audioMixerProperties.volume = trackProperties.volume;
            audioMixerProperties.stereoPan = trackProperties.stereoPan;
            audioMixerProperties.spatialBlend = trackProperties.spatialBlend;
        }
#endif

        void OnValidate()
        {
            trackProperties.volume = Mathf.Clamp01(trackProperties.volume);
            trackProperties.stereoPan = Mathf.Clamp(trackProperties.stereoPan, -1.0f, 1.0f);
            trackProperties.spatialBlend = Mathf.Clamp01(trackProperties.spatialBlend);
        }
    }
}
