// Created by LunarEclipse on 2024-7-14 7:54.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using Luna.Extensions;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;

namespace Luna
{
    public static class SFXManager
    {
        private static GameObject soundManagerObject;
        private static AudioSource sfxAudioSource;

        private static Dictionary<AudioClip, AudioSource> audioSources = new();
        
        public static float Volume
        {
            get => sfxAudioSource.volume;
            set
            {
                sfxAudioSource.volume = value;
                foreach (var audioSource in audioSources.Values)
                    audioSource.volume = value;
            }
        }
        
        public static AudioMixerGroup Mixer
        {
            get => sfxAudioSource.outputAudioMixerGroup;
            set
            {
                sfxAudioSource.outputAudioMixerGroup = value;
                foreach (var audioSource in audioSources.Values)
                    if (audioSource.outputAudioMixerGroup == null)
                        audioSource.outputAudioMixerGroup = value;
            }
        }
        

        [RuntimeInitializeOnLoadMethod]
        static void Initialize()
        {
            soundManagerObject = new GameObject("SFX Manager");
            sfxAudioSource = soundManagerObject.AddComponent<AudioSource>();
            Object.DontDestroyOnLoad(soundManagerObject);
        }
        
        // Play one-shot audio clip with default volume scale of 1.0f
        public static void Play(AudioClip clip)
        {
            Play(clip, 1.0f);
        }
        
        public static void Play(AudioClip clip, float volumeScale)
        {
            sfxAudioSource.PlayOneShot(clip, volumeScale);
        }

#if USE_ADDRESSABLES
        public static void Play(Asset<AudioClip> asset)
        {
            asset.Load().Then(audioClip => SFXManager.Play(audioClip));
        }
#endif
        
        /// Create a new audio source and play the clip repeatedly until `Stop` is called.
        public static void PlayRepeatedly(AudioClip clip)
        {
            if (!audioSources.ContainsKey(clip))
            {
                var audioSource = GetAudioSource(clip);
                audioSource.Play();
            }
        }
        
        public static async void PlayRepeatedly(AudioClip clip, (float min, float max) interval)
        {
            var audioSource = audioSources.GetValueOrDefault(clip);
            if (audioSource == null)
            {
                audioSource = GetAudioSource(clip);
            }

            while (audioSources.ContainsKey(clip) && audioSource != null)
            {
                var delay = UnityEngine.Random.Range(interval.min, interval.max);
                audioSource.PlayOneShot(clip);
                await Task.Delay(TimeSpan.FromSeconds(clip.length + delay));
            }
        }
        
        public static async Task PlayOccasionally(AudioClip clip, (float min, float max) interval, bool loop = true)
        {
            if (audioSources.ContainsKey(clip))
                return;

            var audioSource = GetAudioSource(clip);
            while (loop && audioSources.ContainsKey(clip) && audioSource != null) 
            {
                var delay = UnityEngine.Random.Range(interval.min, interval.max);
                audioSource.PlayDelayed(delay);
                await Task.Delay(TimeSpan.FromSeconds(clip.length + delay));
            }
        }

        public static void Stop()
        {
            sfxAudioSource.Stop();
        }

        public static void Stop(AudioClip clip)
        {
            if (audioSources.ContainsKey(clip))
            {
                audioSources[clip].Stop();
                Object.Destroy(audioSources[clip]);
                audioSources.Remove(clip);
            }
        }
        
        public static void Stop(AudioClip clip, float fadeDuration)
        {
            if (audioSources.ContainsKey(clip))
            {
                var audioSource = audioSources[clip];
                DOTween.To(() => audioSource.volume, x => audioSource.volume = x, 0, fadeDuration).OnComplete(() =>
                {
                    audioSource.Stop();
                    Object.Destroy(audioSource);
                    audioSources.Remove(clip);
                });
            }
        }
        
        public static void StopAll()
        {
            sfxAudioSource.Stop();
            
            foreach (var audioSource in audioSources.Values)
            {
                audioSource.Stop();
                Object.Destroy(audioSource);
            }
            audioSources.Clear();
        }
        
        public static void SetVolume(float volume, float duration = 0.5f)
        {
            DOTween.To(() => Volume, x => Volume = x, volume, duration);
        }

        private static AudioSource GetAudioSource(AudioClip clip)
        {
            if (audioSources.ContainsKey(clip))
                return audioSources[clip];

            var audioSource = soundManagerObject.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.loop = false;
            audioSource.volume = Volume;
            if (Mixer != null)
                audioSource.outputAudioMixerGroup = Mixer;
            audioSources[clip] = audioSource;
            return audioSource;
        }
    }   
}