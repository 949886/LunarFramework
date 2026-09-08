#if !UNITY_2022_2_OR_NEWER
using System.Threading;
using UnityEngine;

namespace Luna.Extensions.Unity
{
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    internal sealed class DestroyCancellationToken : MonoBehaviour
    {
        private readonly CancellationTokenSource source = new CancellationTokenSource();
        internal CancellationToken Token => source.Token;
        private void Awake() { hideFlags = HideFlags.HideInInspector; }
        private void OnDestroy()
        {
            try { source.Cancel(); }
            finally { source.Dispose(); }
        }
    }
}
#endif
