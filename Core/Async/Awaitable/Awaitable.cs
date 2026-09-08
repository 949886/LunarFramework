#if !UNITY_2023_1_OR_NEWER
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;

namespace UnityEngine
{
    /// <summary>Compatibility implementation of Unity's single-consumer asynchronous operation.</summary>
    [AsyncMethodBuilder(typeof(AwaitableAsyncMethodBuilder))]
    public sealed class Awaitable : IEnumerator
    {
        internal readonly AwaitableState<bool> State = new AwaitableState<bool>();
        private bool enumerating;
        internal Awaitable() { }
        public bool IsCompleted => State.IsCompleted;
        public void Cancel() => State.TryComplete(false, new OperationCanceledException());
        public Awaiter GetAwaiter() { State.Claim(); return new Awaiter(State); }

        public readonly struct Awaiter : ICriticalNotifyCompletion
        {
            private readonly AwaitableState<bool> state;
            internal Awaiter(AwaitableState<bool> state) { this.state = state; }
            public bool IsCompleted => state.IsCompleted;
            public void GetResult() => state.GetResult();
            public void OnCompleted(Action continuation) => state.OnCompleted(continuation);
            public void UnsafeOnCompleted(Action continuation) => state.OnCompleted(continuation);
        }

        object IEnumerator.Current => null;
        bool IEnumerator.MoveNext()
        {
            if (!enumerating) { State.Claim(); enumerating = true; }
            if (!IsCompleted) return true;
            State.GetResult();
            return false;
        }
        void IEnumerator.Reset() => throw new NotSupportedException();

        public static Awaitable NextFrameAsync(CancellationToken cancellationToken = default) =>
            AwaitableScheduler.Schedule(AwaitableScheduler.Timing.NextFrame, 0, cancellationToken);
        public static Awaitable FixedUpdateAsync(CancellationToken cancellationToken = default) =>
            AwaitableScheduler.Schedule(AwaitableScheduler.Timing.FixedUpdate, 0, cancellationToken);
        public static Awaitable EndOfFrameAsync(CancellationToken cancellationToken = default) =>
            AwaitableScheduler.Schedule(AwaitableScheduler.Timing.EndOfFrame, 0, cancellationToken);
        public static Awaitable WaitForSecondsAsync(float seconds, CancellationToken cancellationToken = default) =>
            AwaitableScheduler.Schedule(AwaitableScheduler.Timing.Delay, seconds, cancellationToken);
        public static MainThreadAwaitable MainThreadAsync() => new MainThreadAwaitable();
        public static BackgroundThreadAwaitable BackgroundThreadAsync() => new BackgroundThreadAwaitable();

        public static Awaitable FromAsyncOperation(AsyncOperation operation, CancellationToken cancellationToken = default)
        {
            AwaitableScheduler.CheckMainThread();
            if (operation == null) throw new ArgumentNullException(nameof(operation));
            cancellationToken.ThrowIfCancellationRequested();
            AwaitableScheduler.CheckRunning();
            var result = new Awaitable();
            if (operation.isDone) result.State.TryComplete(true);
            else
            {
                Action<AsyncOperation> onCompleted = _ => result.State.TryComplete(true);
                operation.completed += onCompleted;
                result.State.SetCleanup(() =>
                {
                    if (AwaitableScheduler.IsMainThread) operation.completed -= onCompleted;
                    else AwaitableScheduler.Post(() => operation.completed -= onCompleted);
                });
                result.State.RegisterCancellation(cancellationToken);
                AwaitableScheduler.TrackOperation(result);
                if (operation.isDone) result.State.TryComplete(true);
            }
            return result;
        }
    }

    [AsyncMethodBuilder(typeof(AwaitableAsyncMethodBuilder<>))]
    public sealed class Awaitable<T>
    {
        internal readonly AwaitableState<T> State = new AwaitableState<T>();
        internal Awaitable() { }
        public bool IsCompleted => State.IsCompleted;
        public void Cancel() => State.TryComplete(default, new OperationCanceledException());
        public Awaiter GetAwaiter() { State.Claim(); return new Awaiter(State); }
        public readonly struct Awaiter : ICriticalNotifyCompletion
        {
            private readonly AwaitableState<T> state;
            internal Awaiter(AwaitableState<T> state) { this.state = state; }
            public bool IsCompleted => state.IsCompleted;
            public T GetResult() => state.GetResult();
            public void OnCompleted(Action continuation) => state.OnCompleted(continuation);
            public void UnsafeOnCompleted(Action continuation) => state.OnCompleted(continuation);
        }
    }

    public static class AsyncOperationAwaitableExtensions
    {
        public static Awaitable.Awaiter GetAwaiter(this AsyncOperation operation) =>
            Awaitable.FromAsyncOperation(operation).GetAwaiter();
    }
}
#endif
