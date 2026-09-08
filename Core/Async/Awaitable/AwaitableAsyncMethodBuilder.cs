#if !UNITY_2023_1_OR_NEWER
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace UnityEngine
{
    internal interface IAwaitableStateMachineRunner { Action MoveNext { get; } }
    internal sealed class AwaitableStateMachineRunner<T> : IAwaitableStateMachineRunner where T : IAsyncStateMachine
    {
        internal T StateMachine;
        public Action MoveNext { get; }
        internal AwaitableStateMachineRunner() { MoveNext = Run; }
        private void Run() => StateMachine.MoveNext();
    }

    public struct AwaitableAsyncMethodBuilder
    {
        private Awaitable awaitable;
        private IAwaitableStateMachineRunner runner;
        private bool startedOnMainThread;
        public static AwaitableAsyncMethodBuilder Create() => new AwaitableAsyncMethodBuilder
        {
            awaitable = new Awaitable(), startedOnMainThread = AwaitableScheduler.IsMainThread
        };
        public Awaitable Task => awaitable;
        public void Start<T>(ref T stateMachine) where T : IAsyncStateMachine => stateMachine.MoveNext();
        public void SetStateMachine(IAsyncStateMachine stateMachine) { }
        public void SetResult()
        {
            var operation = awaitable;
            AwaitableScheduler.CompleteOnThread(startedOnMainThread, () => operation.State.TryComplete(true));
        }
        public void SetException(Exception exception)
        {
            var operation = awaitable;
            AwaitableScheduler.CompleteOnThread(startedOnMainThread, () => operation.State.TryComplete(false, exception));
        }
        private Action GetContinuation<T>(ref T stateMachine) where T : IAsyncStateMachine
        {
            if (runner == null)
            {
                var typedRunner = new AwaitableStateMachineRunner<T>();
                // Store the runner before copying the state machine, so its builder shares this runner.
                runner = typedRunner;
                typedRunner.StateMachine = stateMachine;
            }
            return runner.MoveNext;
        }
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine =>
            awaiter.OnCompleted(GetContinuation(ref stateMachine));
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine =>
            awaiter.UnsafeOnCompleted(GetContinuation(ref stateMachine));
    }

    public struct AwaitableAsyncMethodBuilder<T>
    {
        private Awaitable<T> awaitable;
        private IAwaitableStateMachineRunner runner;
        private bool startedOnMainThread;
        public static AwaitableAsyncMethodBuilder<T> Create() => new AwaitableAsyncMethodBuilder<T>
        {
            awaitable = new Awaitable<T>(), startedOnMainThread = AwaitableScheduler.IsMainThread
        };
        public Awaitable<T> Task => awaitable;
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine => stateMachine.MoveNext();
        public void SetStateMachine(IAsyncStateMachine stateMachine) { }
        public void SetResult(T result)
        {
            var operation = awaitable;
            AwaitableScheduler.CompleteOnThread(startedOnMainThread, () => operation.State.TryComplete(result));
        }
        public void SetException(Exception exception)
        {
            var operation = awaitable;
            AwaitableScheduler.CompleteOnThread(startedOnMainThread, () => operation.State.TryComplete(default, exception));
        }
        private Action GetContinuation<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            if (runner == null)
            {
                var typedRunner = new AwaitableStateMachineRunner<TStateMachine>();
                runner = typedRunner;
                typedRunner.StateMachine = stateMachine;
            }
            return runner.MoveNext;
        }
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine =>
            awaiter.OnCompleted(GetContinuation(ref stateMachine));
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine =>
            awaiter.UnsafeOnCompleted(GetContinuation(ref stateMachine));
    }

    public readonly struct MainThreadAwaitable : INotifyCompletion
    {
        public MainThreadAwaitable GetAwaiter() => this;
        public bool IsCompleted => AwaitableScheduler.IsMainThread;
        public void GetResult() { }
        public void OnCompleted(Action continuation) => AwaitableScheduler.Post(continuation);
    }

    public readonly struct BackgroundThreadAwaitable : INotifyCompletion
    {
        public BackgroundThreadAwaitable GetAwaiter() => this;
        public bool IsCompleted => !AwaitableScheduler.IsMainThread;
        public void GetResult() { }
        public void OnCompleted(Action continuation)
        {
            if (continuation == null) throw new ArgumentNullException(nameof(continuation));
            ThreadPool.QueueUserWorkItem(_ => continuation());
        }
    }
}
#endif
