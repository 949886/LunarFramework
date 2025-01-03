// Created by LunarEclipse on 2024-7-14 7:54.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
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

        [RuntimeInitializeOnLoadMethod]
        static void Initialize()
        {
            soundManagerObject = new GameObject("SFX Manager");
            sfxAudioSource = soundManagerObject.AddComponent<AudioSource>();
            Object.DontDestroyOnLoad(soundManagerObject);
        }
        
        public static void Play(AudioClip clip, float volumeScale = 1.0f)
        {
            sfxAudioSource.PlayOneShot(clip, volumeScale);
        }
        
        /// Create a new audio source and play the clip repeatedly until `Stop` is called.
        public static void PlayRepeatedly(AudioClip clip)
        {
            if (!audioSources.ContainsKey(clip))
            {
                var audioSource = soundManagerObject.AddComponent<AudioSource>();
                audioSource.clip = clip;
                audioSource.loop = true;
                audioSource.volume = Volume;
                audioSource.Play();
                audioSources[clip] = audioSource;
            }
        }
        
        public static async Task PlayOccasionally(AudioClip clip, (float min, float max) waitTime, bool loop = true)
        {
            if (audioSources.ContainsKey(clip))
                return;
            
            var delay = UnityEngine.Random.Range(waitTime.min, waitTime.max);
            var audioSource = soundManagerObject.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.loop = false;
            audioSource.volume = Volume;
            audioSources[clip] = audioSource;
            
            while (loop && audioSources.ContainsKey(clip)) 
            {
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
    }   
}