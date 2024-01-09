// Created by LunarEclipse on 2024-01-05 23:04.

using Cysharp.Threading.Tasks;
using Luna.Extensions.Unity;
using UnityEngine;

namespace Luna.Core.Locomotion.Character
{
    public class ThirdPersonCharacterAttackBehaviour : StateMachineBehaviour
    {
        public int AttackIndex = 0; // 0 = Idle, 1-10 = Attack
        public GameObject Weapon;
        
        public override async void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Debug.Log("On Attack Enter: \n" + animator.GetCurrentStateName(0));
            
            if (stateInfo.IsTag("Attack"))
            {
                ++AttackIndex;
                Debug.Log("Attack Index: " + AttackIndex);
            }
            else
            {
                AttackIndex = 0;
            }
            
            animator.SetInteger("Attack Index", AttackIndex);
            
            // await UniTask.DelayFrame(2);
            
            animator.SetBool("Attack", false);
            animator.SetBool("Extra Attack", false);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Debug.Log("On Attack Exit: \n" + animator.GetCurrentStateName(0));
            
            animator.SetInteger("Attack Index", AttackIndex);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // Debug.Log("On Attack Update");

            if (!stateInfo.IsTag("Attack"))
            {
                return;
            }
            else
            {
                animator.SetBool("Attack", false);
            }
            
            // Calculate the normalized time (percentage) of the animation
            float progress = stateInfo.normalizedTime;
 
            // Display the current play percentage in the console
            float currentPercentage = progress * 100f;
            // Debug.Log("Current Play Percentage: " + currentPercentage + "%");
        }

        public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // Debug.Log("On Attack Move ");
        }
        public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Debug.Log("On Attack IK");
        }
        
        public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
        {
            Debug.Log("On Attack State Machine Enter");
            // Weapon.SetActive(true);
        }
        
        public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
        {
            Debug.Log("On Attack State Machine Exit");
            AttackIndex = 0;
            animator.SetInteger("Attack Index", AttackIndex);
            // Weapon.SetActive(false);
        }
    }
}