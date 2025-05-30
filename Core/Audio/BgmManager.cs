// Created by LunarEclipse on 2024-7-30 17:59.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Luna.Extensions;
using UnityEngine;
using UnityEngine.Audio;

#if USE_ADDRESSABLES
using UnityEngine.AddressableAssets;
#endif

namespace Luna
{
    public static class BgmManager
    {
        private static AudioSource audioSource;
        
        
        public static float Volume
        {
            get => audioSource.volume;
            set => audioSource.volume = value;
        }
        
        public static AudioMixerGroup Mixer
        {
            get => audioSource.outputAudioMixerGroup;
            set => audioSource.outputAudioMixerGroup = value;
        }
        
        // public static int HistoryLimit { get; set; } = 10;
        
        public static AudioClip CurrentBgm => audioSource.clip;
        

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            GameObject soundManagerObject = new GameObject("BGM Manager");
            audioSource = soundManagerObject.AddComponent<AudioSource>();
            audioSource.loop = true;
            Object.DontDestroyOnLoad(soundManagerObject);
        }
        
        public static void Play(AudioClip clip)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
        
        public static void Play(AudioClip clip, float fadeDuration)
        {
            void FadeIn(float duration)
            {
                audioSource.volume = 0;
                audioSource.clip = clip;
                audioSource.Play();
                DOTween.To(() => audioSource.volume, x => audioSource.volume = x, 1, duration);
            }

            if (audioSource.isPlaying)
            {
                // Fade out
                DOTween.To(() => audioSource.volume, x => audioSource.volume = x, 0, fadeDuration * 0.5f).OnComplete(() =>
                {
                    audioSource.Stop();
                    FadeIn(fadeDuration * 0.5f);
                });
            }
            else FadeIn(fadeDuration);
        }
        
#if USE_ADDRESSABLES
        public static void Play(Asset<AudioClip> asset)
        {
            asset.Load().Then(Play);
        }
        
        public static void Play(AssetReferenceT<AudioClip> clipRef)
        {
            var previousClip = audioSource.clip;
            
            clipRef.LoadAssetAsync().Completed += handle =>
            {
                Play(handle.Result);
                Addressables.Release(previousClip);
            };
        }
#endif
        
        public static async void PlaySequentially(List<AudioClip> clips, float fadeDuration = 1.0f)
        {
            foreach (var clip in clips)
            {
                Play(clip, fadeDuration);
                await UniTask.Delay((int)((clip.length + fadeDuration) * 1000));
            }
        }
        
        public static async void PlayRandomly(List<AudioClip> clips, float fadeDuration = 1.0f)
        {
            while (true)
            {
                var clip = clips[Random.Range(0, clips.Count)];
                Play(clip, fadeDuration);
                await UniTask.Delay((int)((clip.length + fadeDuration) * 1000));
            }
        }
        
        public static void Stop()
        {
            audioSource.Stop();
        }
        
        public static void Stop(float fadeDuration)
        {
            DOTween.To(() => audioSource.volume, x => audioSource.volume = x, 0, fadeDuration).OnComplete(() =>
            {
                audioSource.Stop();
            });
        }
        
        public static void Pause()
        {
            audioSource.Pause();
        }
        
        public static void Pause(float fadeDuration)
        {
            DOTween.To(() => audioSource.volume, x => audioSource.volume = x, 0, fadeDuration).OnComplete(() =>
            {
                audioSource.Pause();
            });
        }
        
        public static void Resume()
        {
            audioSource.UnPause();
        }
        
        public static void Resume(float fadeDuration)
        {
            audioSource.volume = 0;
            audioSource.UnPause();
            DOTween.To(() => audioSource.volume, x => audioSource.volume = x, 1, fadeDuration);
        }
        
        public static void SetVolume(float volume, float fadeDuration = 0.5f)
        {
            DOTween.To(() => audioSource.volume, x => audioSource.volume = x, volume, fadeDuration);
        }
        
        public static void SetPitch(float pitch, float fadeDuration = 0.5f)
        {
            DOTween.To(() => audioSource.pitch, x => audioSource.pitch = x, pitch, fadeDuration);
        }
    }
}