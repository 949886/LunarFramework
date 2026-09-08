#if !UNITY_2023_1_OR_NEWER
using System;
using System.Threading;

namespace UnityEngine
{
    public sealed class AwaitableCompletionSource
    {
        private Awaitable awaitable = new Awaitable();
        public Awaitable Awaitable => Volatile.Read(ref awaitable);
        public void Reset() => Interlocked.Exchange(ref awaitable, new Awaitable());
        public bool TrySetResult() => Awaitable.State.TryComplete(true);
        public bool TrySetCanceled() => Awaitable.State.TryComplete(false, new OperationCanceledException());
        public bool TrySetException(Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));
            return Awaitable.State.TryComplete(false, exception);
        }
        public void SetResult() { if (!TrySetResult()) throw AlreadyCompleted(); }
        public void SetCanceled() { if (!TrySetCanceled()) throw AlreadyCompleted(); }
        public void SetException(Exception exception) { if (!TrySetException(exception)) throw AlreadyCompleted(); }
        internal static InvalidOperationException AlreadyCompleted() =>
            new InvalidOperationException("The Awaitable has already completed.");
    }

    public sealed class AwaitableCompletionSource<T>
    {
        private Awaitable<T> awaitable = new Awaitable<T>();
        public Awaitable<T> Awaitable => Volatile.Read(ref awaitable);
        public void Reset() => Interlocked.Exchange(ref awaitable, new Awaitable<T>());
        public bool TrySetResult(in T result) => Awaitable.State.TryComplete(result);
        public bool TrySetCanceled() => Awaitable.State.TryComplete(default, new OperationCanceledException());
        public bool TrySetException(Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));
            return Awaitable.State.TryComplete(default, exception);
        }
        public void SetResult(in T result) { if (!TrySetResult(result)) throw AwaitableCompletionSource.AlreadyCompleted(); }
        public void SetCanceled() { if (!TrySetCanceled()) throw AwaitableCompletionSource.AlreadyCompleted(); }
        public void SetException(Exception exception) { if (!TrySetException(exception)) throw AwaitableCompletionSource.AlreadyCompleted(); }
    }
}
#endif
