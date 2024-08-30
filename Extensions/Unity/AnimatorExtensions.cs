// Created by LunarEclipse on 2024-01-05 16:38.

#if UNITY_2021_3_OR_NEWER

using System;
using System.Reflection;
using Luna.Core;
using UnityEngine;

namespace Luna.Extensions.Unity
{
    public static class AnimatorExtensions
    {
        // From https://forum.unity.com/threads/current-animator-state-name.331803/
        
        private static Func<Animator, int, string> _getCurrentStateName;
        /// <summary>[FOR DEBUGGING ONLY] Calls an internal method on <see cref="Animator"/> that
        /// returns the name of the current state for a layer. The internal method could be removed
        /// or refactored at any time, and may not have good performance.</summary>
        /// <param name="animator">The animator to get the current state from.</param>
        /// <param name="layer">The layer to get the current state from.</param>
        /// <returns>The name of the currently running state.</returns>
        public static string GetCurrentStateName(this Animator animator, int layer)
        {
            if (_getCurrentStateName == null)
                _getCurrentStateName = Reflection.AccessPrivateFunction<Animator, int, string>("GetCurrentStateName");
            return _getCurrentStateName(animator, layer);
        }
     
        private static Func<Animator, int, string> _getNextStateName;
        /// <summary>[FOR DEBUGGING ONLY] Calls an internal method on <see cref="Animator"/> that
        /// returns the name of the next state for a layer. The internal method could be removed or
        /// refactored at any time, and may not have good performance.</summary>
        /// <param name="animator">The animator to get the next state from.</param>
        /// <param name="layer">The layer to get the next state from.</param>
        /// <returns>The name of the next running state.</returns>
        public static string GetNextStateName(this Animator animator, int layer)
        {
            if (_getNextStateName == null)
                _getNextStateName = Reflection.AccessPrivateFunction<Animator, int, string>("GetNextStateName");
            return _getNextStateName(animator, layer);
        }
     
     
        private static Func<Animator, int, string> _resolveHash;
        /// <summary>[FOR DEBUGGING ONLY] Calls an internal method on <see cref="Animator"/> that
        /// returns the string used to create a hash from
        /// <see cref="Animator.StringToHash(string)"/>. The internal method could be removed or
        /// refactored at any time, and may not have good performance.</summary>
        /// <param name="animator">The animator to get the string from.</param>
        /// <param name="hash">The hash to get the original string for.</param>
        /// <returns>The name of the string for <paramref name="hash"/>.</returns>
        public static string ResolveHash(this Animator animator, int hash)
        {
            if (_resolveHash == null)
                _resolveHash = Reflection.AccessPrivateFunction<Animator, int, string>("ResolveHash");
            return _resolveHash(animator, hash);
        }
    }
}

#endif