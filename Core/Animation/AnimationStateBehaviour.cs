// Created by LunarEclipse on 2024-2-1 3:58.

#pragma warning disable CS0067 // Event is never used

using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Luna.Core.Animation
{
    public abstract class AnimationStateBehaviour: StateMachineBehaviour
    {
        public string name;
        
        protected float progress;
        
        private bool _isTransitioning;
        private AnimationState _animationState;
        
        public abstract void OnAnimationStart(Animator animator, AnimatorStateInfo stateInfo, int layerIndex);
        public abstract void OnAnimationExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex);
        public abstract void OnAnimationEnd(Animator animator, AnimatorStateInfo stateInfo, int layerIndex);
        
        // Called when the animation is fully played.
        public abstract void OnAnimationFinish(Animator animator, AnimatorStateInfo stateInfo, int layerIndex);
        
        // Called every frame when the animation is playing.
        public abstract void OnAnimationUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex);
        public abstract void OnAnimationInterrupt(Animator animator, AnimatorStateInfo stateInfo, int layerIndex);
        public abstract void OnAnimationTransitionInStart(Animator animator, AnimatorStateInfo stateInfo, int layerIndex);
        public abstract void OnAnimationTransitionInEnd(Animator animator, AnimatorStateInfo stateInfo, int layerIndex);
        public abstract void OnAnimationTransitionOutStart(Animator animator, AnimatorStateInfo stateInfo, int layerIndex);
        public abstract void OnAnimationTransitionOutEnd(Animator animator, AnimatorStateInfo stateInfo, int layerIndex);
        
        
        public event AnimationStateDelegate OnAnimationStartEvent;
        public event AnimationStateDelegate OnAnimationExitEvent;
        public event AnimationStateDelegate OnAnimationEndEvent;
        public event AnimationStateDelegate OnAnimationFinishEvent;
        public event AnimationStateDelegate OnAnimationUpdateEvent;
        public event AnimationStateDelegate OnAnimationInterruptEvent;
        public event AnimationStateDelegate OnAnimationTransitionInStartEvent;
        public event AnimationStateDelegate OnAnimationTransitionInEndEvent;
        public event AnimationStateDelegate OnAnimationTransitionOutStartEvent;
        public event AnimationStateDelegate OnAnimationTransitionOutEndEvent;
        
        public delegate void AnimationStateDelegate(AnimationStateBehaviour sender, Animator animator, AnimatorStateInfo stateInfo, int layerIndex);
        
        
        public sealed override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // name = $"Test1 {this.GetHashCode()}";
        }

        public sealed override async void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!_isTransitioning)
                OnAnimationExit(animator, stateInfo, layerIndex);
            await UniTask.DelayFrame(1);
            OnAnimationEnd(animator, stateInfo, layerIndex);
            progress = 0f;
        }

        public sealed override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // Debug.Log($"Progress of {name}: {progress} {stateInfo.normalizedTime}");
            OnAnimationUpdate(animator, stateInfo, layerIndex);

            if (progress == 0f)
            {
                OnAnimationStart(animator, stateInfo, layerIndex);
            }
            
            // Check animation transition start/end.
            if (animator.IsInTransition(layerIndex) && !_isTransitioning)
            {
                if (progress == 0f)
                {
                    _isTransitioning = true;
                    _animationState = AnimationState.In;
                    OnAnimationTransitionInStart(animator, stateInfo, layerIndex);
                }
                else
                {
                    _isTransitioning = true;
                    _animationState = AnimationState.Out;
                    OnAnimationExit(animator, stateInfo, layerIndex);
                    OnAnimationTransitionOutStart(animator, stateInfo, layerIndex);
                }
            }
            else if (!animator.IsInTransition(layerIndex) && _isTransitioning)
            {
                switch (_animationState)
                {
                    case AnimationState.In:
                        OnAnimationTransitionInEnd(animator, stateInfo, layerIndex);
                        break;
                    case AnimationState.Out:
                        OnAnimationTransitionOutEnd(animator, stateInfo, layerIndex);
                        break;
                }
                _isTransitioning = false;
            }
            
            progress = stateInfo.normalizedTime;
            
            // When animation is fully played.
            if (progress >= 1f)
            {
                OnAnimationFinish(animator, stateInfo, layerIndex);
            }
            
        }
        
        
        public sealed override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {}
        public sealed override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {}
        // public sealed override void OnStateMachineEnter(Animator animator, int stateMachinePathHash) {}
        // public sealed override void OnStateMachineExit(Animator animator, int stateMachinePathHash) {}

        protected enum AnimationState
        {
            In,
            Out
        }
    }
}