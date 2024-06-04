// Created by LunarEclipse on 2024-01-08 4:02.

using Luna.Core.Animation;
using UnityEngine;

namespace Luna.Core.Locomotion.Character
{
    public class ThirdPersonCharacterBehaviour : AnimationStateBehaviour
    {
        State _state = State.Idle;

        public override void OnAnimationStart(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // Debug.LogError($"On Animation {name} {this.GetHashCode()} Start");
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
            // Debug.Log($"Progress: {progress}");
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
        
        private enum State: int
        {
            Idle,
            Moving,
            Attacking,
            Jumping,
            Running,
            Sprinting,
            Crouching,
            Interacting,
            PickingUp,
            Skill1,
            Skill2,
            Skill3,
        }
        
        private enum Substate: int
        {
            Start,      // or Windup
            Active,
            Recovery,   // or Backswing
        }
    }
}