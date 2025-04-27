using Luna;
using Luna.Core.Animation;
using Luna.Extensions;
using UnityEngine;

public class AudioStateBehaviour : AnimationStateBehaviour
{
    private static int _currentIndex;
    
    public PlayMode playMode = PlayMode.Random;
    public bool waitFinish = true;
    public AudioClip[] audioClips;

    private AudioSource _audioSource = null;
    
    public override void OnAnimationStart(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_audioSource == null)
        {
            _audioSource = animator.GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = animator.gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
            }
        }
        
        if (playMode == PlayMode.Sequential)
        {
            var clip = audioClips[_currentIndex.Mod(audioClips.Length)];
            if (waitFinish && !_audioSource.isPlaying)
                _audioSource.PlayOneShot(clip);
            _currentIndex++;
        }
        else if (playMode == PlayMode.Random)
        {
            var clip = audioClips[Random.Range(0, audioClips.Length)];
            if (waitFinish && !_audioSource.isPlaying)
                _audioSource.PlayOneShot(clip);
            
            Debug.Log($"[AudioStateBehaviour] Playing sound: {clip} {layerIndex}");
        }
    }

    public override void OnAnimationExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }

    public override void OnAnimationEnd(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }

    public override void OnAnimationFinish(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }

    public override void OnAnimationUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }

    public override void OnAnimationInterrupt(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }

    public override void OnAnimationTransitionInStart(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }

    public override void OnAnimationTransitionInEnd(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }

    public override void OnAnimationTransitionOutStart(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }

    public override void OnAnimationTransitionOutEnd(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }

    public enum PlayMode
    {
        Sequential,
        Random,
    }
}