// Created by LunarEclipse on 2024-01-08 4:02.

using UnityEngine;

namespace Luna.Core.Locomotion.Character
{
    public class ThirdPersonCharacterBehaviour : StateMachineBehaviour
    {
        State _state = State.Idle;
        
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // Debug.Log("On State Enter");
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // Debug.Log("On State Exit");
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // Debug.Log("On State Update");
        }

        public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // Debug.Log("On State Move");
        }

        public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // Debug.Log("On State IK");
        }
        
        public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
        {
            // Debug.Log("On State Machine Enter");
        }
        
        public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
        {
            // Debug.Log("On State Machine Exit");
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