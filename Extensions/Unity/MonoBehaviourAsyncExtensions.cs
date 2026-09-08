using System;
using System.Threading;
using UnityEngine;

namespace Luna.Extensions.Unity
{
    public static class MonoBehaviourAsyncExtensions
    {
        /// <summary>Returns a token canceled when this behaviour's GameObject is destroyed.</summary>
        public static CancellationToken GetCancellationTokenOnDestroy(this MonoBehaviour behaviour)
        {
            if (behaviour == null) throw new ArgumentNullException(nameof(behaviour));
#if UNITY_2022_2_OR_NEWER
            return behaviour.destroyCancellationToken;
#else
            var lifetime = behaviour.GetComponent<DestroyCancellationToken>();
            if (lifetime == null) lifetime = behaviour.gameObject.AddComponent<DestroyCancellationToken>();
            return lifetime.Token;
#endif
        }
    }
}
