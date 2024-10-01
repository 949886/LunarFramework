// Created by LunarEclipse on 2024-7-14 9:34.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Luna.UI.Audio
{
    namespace Luna.UI
    {
        public class Audios : ScriptableObject
        {
            public List<AudioClip> Clips = new();

            public void Add(AudioClip clip)
            {
                if (Clips.Contains(clip))
                {
                    // Debug.LogWarning("Prefab already exists in the list.");
                    return;
                }
                Clips.Add(clip);
                Debug.Log("Add clip: " + clip.name);
            }
        }
    }
}