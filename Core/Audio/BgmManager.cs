// Created by LunarEclipse on 2024-7-30 17:59.

using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Luna.UI.Audio
{
    public static class BgmManager
    {
        private static AudioSource audioSource;

        [RuntimeInitializeOnLoadMethod]
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
        
        public static void Play(AssetReferenceT<AudioClip> clipRef)
        {
            var previousClip = audioSource.clip;
            
            clipRef.LoadAssetAsync().Completed += handle =>
            {
                Play(handle.Result);
                Addressables.Release(previousClip);
            };
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
    }
}