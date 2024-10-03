// Created by LunarEclipse on 2024-7-14 7:54.

using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Luna
{
    public static class SFXManager
    {
        private static GameObject soundManagerObject;
        private static AudioSource sfxAudioSource;

        private static Dictionary<AudioClip, AudioSource> audioSources = new Dictionary<AudioClip, AudioSource>();
        
        [RuntimeInitializeOnLoadMethod]
        static void Initialize()
        {
            soundManagerObject = new GameObject("SFX Manager");
            sfxAudioSource = soundManagerObject.AddComponent<AudioSource>();
            Object.DontDestroyOnLoad(soundManagerObject);
        }
        
        public static void Play(AudioClip clip)
        {
            sfxAudioSource.PlayOneShot(clip);
        }
        
        /// Create a new audio source and play the clip repeatedly until `Stop` is called.
        public static void PlayRepeatedly(AudioClip clip)
        {
            if (!audioSources.ContainsKey(clip))
            {
                var audioSource = soundManagerObject.AddComponent<AudioSource>();
                audioSource.clip = clip;
                audioSource.loop = true;
                audioSource.Play();
                audioSources[clip] = audioSource;
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
            foreach (var audioSource in audioSources.Values)
            {
                audioSource.Stop();
                Object.Destroy(audioSource);
            }
            audioSources.Clear();
        }
    }   
}