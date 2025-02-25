using Luna.Extensions;
using UnityEngine;

namespace Luna
{
    public static class AssetExtensions
    {
        public static void Play(this Asset<AudioClip> asset)
        {
            asset.Load().Then(SFXManager.Play);
        }
        
        public static void PlayAsBgm(this Asset<AudioClip> asset)
        {
            asset.Load().Then(BgmManager.Play);
        }
    }
}