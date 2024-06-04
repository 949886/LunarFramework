// Created by LunarEclipse on 2024-01-11 22:35.

using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Luna.Core.Animation
{
    public class TimelineStateBehaviour : AnimationStateBehaviour
    {
        public TimelineAsset timeline;
        public bool playOnEnter = true;
        public double initialTime = 0f;
        
        private PlayableDirector _playableDirector;

        private float progress;
        
        public override void OnAnimationStart(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (timeline is null) return;
            
            // Initialize the playable director.
            if (_playableDirector is null)
            {
                _playableDirector = animator.GetComponent<PlayableDirector>();
                if (_playableDirector == null)
                    _playableDirector = animator.gameObject.AddComponent<PlayableDirector>();
                _playableDirector.playableAsset = timeline;
            }

            if (_playableDirector.state == PlayState.Playing)
            {
                // Debug.LogWarning("Stopping");
                _playableDirector.Stop();
            }
            
            Debug.Log("Timeline Initial Time: " + initialTime);
            _playableDirector.time = initialTime;
                
            if (playOnEnter)
                _playableDirector.Play();
        }

        public override void OnAnimationExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // Debug.Log("Stopping on Exit");
            
            // Stop playing timeline when interrupted by another state.
            if (_playableDirector.state == PlayState.Playing)
            {
                _playableDirector.Stop();
            }
        }

        public override void OnAnimationEnd(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            
        }

        public override void OnAnimationFinish(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            
        }

        public override void OnAnimationUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // Stop playing timeline when animation is finished.
            progress = stateInfo.normalizedTime;
            if (progress >= 1f)
            {
                // Debug.LogWarning("Stopping on Exit");
                if (_playableDirector.state == PlayState.Playing)
                {
                    _playableDirector.Stop();
                }
            }
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

        // public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        // {
        //     if (timeline is null) return;
        //     
        //     // Initialize the playable director.
        //     if (_playableDirector is null)
        //     {
        //         _playableDirector = animator.GetComponent<PlayableDirector>();
        //         if (_playableDirector == null)
        //             _playableDirector = animator.gameObject.AddComponent<PlayableDirector>();
        //         _playableDirector.playableAsset = timeline;
        //     }
        //
        //     if (_playableDirector.state == PlayState.Playing)
        //     {
        //         // Debug.LogWarning("Stopping");
        //         _playableDirector.Stop();
        //     }
        //     
        //     Debug.Log("Timeline Initial Time: " + initialTime);
        //     _playableDirector.time = initialTime;
        //         
        //     if (playOnEnter)
        //         _playableDirector.Play();
        // }
        //
        // public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        // {
        //     // Debug.Log("Stopping on Exit");
        //     
        //     // Stop playing timeline when interrupted by another state.
        //     if (_playableDirector.state == PlayState.Playing)
        //     {
        //         _playableDirector.Stop();
        //     }
        // }
        //
        // public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        // {
        //     // float progress = stateInfo.normalizedTime;
        //     // if (progress >= 1f)
        //     // {
        //     //     animator.SetBool("Attack", false);
        //     // }
        //     //
        //     // float currentPercentage = progress * 100f;
        //     // Debug.Log("Current Play Percentage: " + currentPercentage + "%");
        // }
        //
        // public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        // {
        //     // Stop playing timeline when animation is finished.
        //     progress = stateInfo.normalizedTime;
        //     if (progress >= 1f)
        //     {
        //         // Debug.LogWarning("Stopping on Exit");
        //         if (_playableDirector.state == PlayState.Playing)
        //         {
        //             _playableDirector.Stop();
        //         }
        //     }
        // }
        //
        // public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        // {
        // }
    }
}