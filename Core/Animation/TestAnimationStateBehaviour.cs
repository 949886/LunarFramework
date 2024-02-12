// Created by LunarEclipse on 2024-2-1 7:6.

using UnityEngine;

namespace Luna.Core.Animation
{
    public class TestAnimationStateBehaviour: AnimationStateBehaviour
    {
        public override void OnAnimationStart(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Debug.LogWarning($"On Animation {name} Start");
        }

        public override void OnAnimationExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Debug.LogWarning($"On Animation {name} Exit");
        }

        public override void OnAnimationEnd(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Debug.LogWarning($"On Animation {name} End {stateInfo.normalizedTime}");
        }

        public override void OnAnimationFinish(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Debug.LogWarning($"On Animation {name} Finish {stateInfo.normalizedTime}");
        }

        public override void OnAnimationUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // Debug.LogWarning($"On Animation {name} Update");
        }

        public override void OnAnimationInterrupt(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Debug.LogWarning($"On Animation {name} Interrupt");
        }

        public override void OnAnimationTransitionInStart(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Debug.LogWarning($"On Animation {name} Transition In Start");
        }

        public override void OnAnimationTransitionInEnd(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Debug.LogWarning($"On Animation {name} Transition In End");
        }

        public override void OnAnimationTransitionOutStart(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Debug.LogWarning($"On Animation {name} Transition Out Start");
        }

        public override void OnAnimationTransitionOutEnd(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Debug.LogWarning($"On Animation {name} Transition Out End {stateInfo.normalizedTime}");
        }
    }
}