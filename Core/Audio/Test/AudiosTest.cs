using System;
using System.Collections;
using Luna;
using Luna.Luna.UI;
using Modules.UI.Misc;
using UnityEngine;

public class AudiosTest : MonoBehaviour
{
    public Audios audios;
    
    void Start()
    {

    }

    private async void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (audios == null)
            {
                Debug.LogWarning("Audios is null. Loading from Resources...");
                audios = Resources.Load<Audios>("Audios.g");
            }
            SFXManager.Play(audios.Clips.GetRandomly());
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (audios == null)
            {
                Debug.LogWarning("Audios is null. Loading from Resources...");
                audios = Resources.Load<Audios>("Audios.g");
            }
            BgmManager.Play(audios.Clips.GetRandomly());
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (audios == null)
            {
                Debug.LogWarning("Audios is null. Loading from Resources...");
                audios = Resources.Load<Audios>("Audios.g");
            }
            var audioClip = audios.Clips.GetRandomly();
            SFXManager.PlayRepeatedly(audioClip);
            
            // Wait for 3 seconds
            StartCoroutine(StopAudioClip(audioClip, 1));
        }
#if USE_ADDRESSABLES
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            var audioClip = await new Asset<AudioClip>("Audio/Key_effect").Load();
            SFXManager.Play(audioClip);
            
            // Wait for 3 seconds
            StartCoroutine(StopAudioClip(audioClip, 1));
        }
#endif

    }

    private IEnumerator StopAudioClip(AudioClip audioClip, float duration)
    {
        yield return new WaitForSeconds(duration);
        // SFXManager.Stop(audioClip);
        GC.Collect();
    }
}
