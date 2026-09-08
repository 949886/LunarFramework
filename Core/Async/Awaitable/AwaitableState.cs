#if !UNITY_2023_1_OR_NEWER
using System;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace UnityEngine
{
    // A single consumer state. Completion and continuation registration may race across threads.
    internal sealed class AwaitableState<T>
    {
        private readonly object gate = new object();
        private bool completed, claimed, consumed, continuationRegistered;
        private T result;
        private ExceptionDispatchInfo error;
        private Action continuation;
        private Action cleanup;
        private CancellationTokenRegistration cancellation;

        internal bool IsCompleted { get { lock (gate) return completed; } }

        internal void Claim()
        {
            lock (gate)
            {
                if (claimed) throw new InvalidOperationException("An Awaitable can only be awaited once.");
                claimed = true;
            }
        }

        internal void OnCompleted(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            bool invoke;
            lock (gate)
            {
                if (continuationRegistered || consumed)
                    throw new InvalidOperationException("An Awaitable can only have one continuation.");
                continuationRegistered = true;
                invoke = completed;
                if (!invoke) continuation = action;
            }
            if (invoke) action();
        }

        internal T GetResult()
        {
            ExceptionDispatchInfo failure;
            T value;
            lock (gate)
            {
                if (!completed) throw new InvalidOperationException("The Awaitable has not completed.");
                if (consumed) throw new InvalidOperationException("The Awaitable result was already consumed.");
                consumed = true;
                value = result;
                failure = error;
                result = default;
                error = null;
            }
            failure?.Throw();
            return value;
        }

        internal bool TryComplete(T value, Exception exception = null)
        {
            Action callback, release;
            CancellationTokenRegistration registration;
            lock (gate)
            {
                if (completed) return false;
                result = value;
                error = exception == null ? null : ExceptionDispatchInfo.Capture(exception);
                completed = true;
                callback = continuation;
                release = cleanup;
                registration = cancellation;
                continuation = null;
                cleanup = null;
                cancellation = default;
            }
            // Never hold the state lock while disposing registrations or running user code.
            registration.Dispose();
            try { release?.Invoke(); }
            finally { callback?.Invoke(); }
            return true;
        }

        internal void RegisterCancellation(CancellationToken token)
        {
            if (!token.CanBeCanceled) return;
            var registration = token.Register(() => TryComplete(default, new OperationCanceledException(token)));
            bool dispose;
            lock (gate)
            {
                dispose = completed;
                if (!dispose) cancellation = registration;
            }
            // Register can invoke synchronously when cancellation won the race.
            if (dispose) registration.Dispose();
        }

        internal void SetCleanup(Action action)
        {
            bool invoke;
            lock (gate)
            {
                invoke = completed;
                if (!invoke) cleanup = action;
            }
            if (invoke) action();
        }
    }
}
#endif
