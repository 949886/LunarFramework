// Created by LunarEclipse on 2024-01-05 23:04.

using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Luna.Core.Animation;
using Luna.Extensions.Unity;
using UnityEngine;
using UnityEngine.Serialization;

namespace Luna.Core.Locomotion.Character
{
    public class ThirdPersonCharacterAttackBehaviour : AnimationStateBehaviour
    {
        [Header("Attack")]
        [FormerlySerializedAs("AttackIndex")] public int attackIndex = 0; // 0 = Idle, 1-10 = Attack
        [FormerlySerializedAs("Weapon")] public GameObject weapon;
        public float windUpTime;
        public int attactLayer = 0;

        public override void OnAnimationStart(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (layerIndex != attactLayer) return;
            
            Debug.Log("On Attack Enter: \n" + animator.GetCurrentStateName(0));
            
            if (stateInfo.IsTag("Attack"))
            {
                ++attackIndex;
                Debug.Log("Attack Index: " + attackIndex);
            }
            else
            {
                attackIndex = 0;
            }
            
            animator.SetInteger("Attack Index", attackIndex);
            
            // await UniTask.DelayFrame(2);
            
            animator.SetBool("Attack", false);
            animator.SetBool("Extra Attack", false);
        }

        public override void OnAnimationExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (layerIndex != attactLayer) return;
            
            Debug.Log("On Attack Exit: \n" + animator.GetCurrentStateName(0));
            
            animator.SetInteger("Attack Index", 0);
            if (!stateInfo.IsTag("Attack"))
            {
                
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
            // float currentPercentage = progress * 100f;
            // Debug.Log("Current Play Percentage: " + currentPercentage + "%");
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
        
        public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
        {
            Debug.Log("On Attack State Machine Enter");
            weapon?.SetActive(true);
        }
        
        public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
        {
            Debug.Log("On Attack State Machine Exit");
            attackIndex = 0;
            animator.SetInteger("Attack Index", attackIndex);
            weapon?.SetActive(false);
        }
    }
}