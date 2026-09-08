#if !UNITY_2023_1_OR_NEWER
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.LowLevel;
using RuntimePlayerLoop = UnityEngine.LowLevel.PlayerLoop;
using UpdateLoop = UnityEngine.PlayerLoop.Update;
using FixedLoop = UnityEngine.PlayerLoop.FixedUpdate;
using LateLoop = UnityEngine.PlayerLoop.PostLateUpdate;

namespace UnityEngine
{
    internal static class AwaitableScheduler
    {
        internal enum Timing { NextFrame, FixedUpdate, EndOfFrame, Delay }
        private struct Request
        {
            internal Awaitable Operation;
            internal int Frame;
            internal long Tick;
            internal double Deadline;
        }
        private struct UpdateMarker { }
        private struct FixedMarker { }
        private struct EndMarker { }
        private static readonly List<Request> nextFrame = new List<Request>();
        private static readonly List<Request> fixedUpdate = new List<Request>();
        private static readonly List<Request> endOfFrame = new List<Request>();
        private static readonly List<Request> delays = new List<Request>();
        private static readonly List<Awaitable> operations = new List<Awaitable>();
        private static readonly ConcurrentQueue<Action> mainThreadQueue = new ConcurrentQueue<Action>();
        private static int mainThreadId;
        private static long fixedTick, endTick;
        private static bool installed, stopped;
        internal static bool IsMainThread => Thread.CurrentThread.ManagedThreadId == Volatile.Read(ref mainThreadId);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            mainThreadId = Thread.CurrentThread.ManagedThreadId;
            Shutdown();
            stopped = false;
            installed = false;
            fixedTick = endTick = 0;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            mainThreadId = Thread.CurrentThread.ManagedThreadId;
            stopped = false;
            Install();
            Application.quitting -= Shutdown;
            Application.quitting += Shutdown;
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void InitializeEditor()
        {
            mainThreadId = Thread.CurrentThread.ManagedThreadId;
            UnityEditor.EditorApplication.update -= EditorUpdate;
            UnityEditor.EditorApplication.update += EditorUpdate;
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeChanged;
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload -= Shutdown;
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += Shutdown;
        }
        private static void EditorUpdate()
        {
            if (!Application.isPlaying) DrainMainThreadQueue();
        }
        private static void OnPlayModeChanged(UnityEditor.PlayModeStateChange change)
        {
            if (change == UnityEditor.PlayModeStateChange.ExitingPlayMode) Shutdown();
        }
#endif
        internal static void CheckMainThread()
        {
            if (!IsMainThread) throw new InvalidOperationException("This Awaitable operation must be created on the Unity main thread.");
        }
        internal static void CheckRunning()
        {
            if (stopped) throw new OperationCanceledException("Unity is exiting Play mode.");
        }
        internal static void Post(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            mainThreadQueue.Enqueue(action);
        }
        internal static void CompleteOnThread(bool mainThread, Action action)
        {
            if (mainThread == IsMainThread) action();
            else if (mainThread) Post(action);
            else ThreadPool.QueueUserWorkItem(_ => action());
        }
        internal static Awaitable Schedule(Timing timing, float seconds, CancellationToken token)
        {
            CheckMainThread();
            token.ThrowIfCancellationRequested();
            CheckRunning();
            if (float.IsNaN(seconds)) throw new ArgumentOutOfRangeException(nameof(seconds));
            Install();
            var operation = new Awaitable();
            operation.State.RegisterCancellation(token);
            var request = new Request
            {
                Operation = operation, Frame = Time.frameCount + 1,
                Deadline = Time.timeAsDouble + Math.Max(0, seconds)
            };
            switch (timing)
            {
                case Timing.NextFrame: nextFrame.Add(request); break;
                case Timing.Delay: delays.Add(request); break;
                case Timing.FixedUpdate: request.Tick = fixedTick + 1; fixedUpdate.Add(request); break;
                case Timing.EndOfFrame: request.Tick = endTick + 1; endOfFrame.Add(request); break;
            }
            return operation;
        }
        internal static void TrackOperation(Awaitable operation) => operations.Add(operation);

        private static void Install()
        {
            if (installed) return;
            var loop = RuntimePlayerLoop.GetCurrentPlayerLoop();
            RemoveMarkers(ref loop);
            bool update = Insert(ref loop, typeof(UpdateLoop), typeof(UpdateMarker), Update, true);
            bool fixedPhase = Insert(ref loop, typeof(FixedLoop), typeof(FixedMarker), FixedUpdate, false);
            bool end = Insert(ref loop, typeof(LateLoop), typeof(EndMarker), EndOfFrame, false);
            if (!update || !fixedPhase || !end)
                throw new InvalidOperationException("The current PlayerLoop does not contain the required Awaitable phases.");
            RuntimePlayerLoop.SetPlayerLoop(loop);
            installed = true;
        }
        private static bool Insert(ref PlayerLoopSystem loop, Type parent, Type marker, PlayerLoopSystem.UpdateFunction update, bool first)
        {
            if (loop.type == parent)
            {
                var children = new List<PlayerLoopSystem>(loop.subSystemList ?? Array.Empty<PlayerLoopSystem>());
                children.Insert(first ? 0 : children.Count, new PlayerLoopSystem { type = marker, updateDelegate = update });
                loop.subSystemList = children.ToArray();
                return true;
            }
            if (loop.subSystemList == null) return false;
            for (int i = 0; i < loop.subSystemList.Length; i++)
                if (Insert(ref loop.subSystemList[i], parent, marker, update, first)) return true;
            return false;
        }
        private static void RemoveMarkers(ref PlayerLoopSystem loop)
        {
            if (loop.subSystemList == null) return;
            var children = new List<PlayerLoopSystem>();
            foreach (var child in loop.subSystemList)
            {
                if (child.type == typeof(UpdateMarker) || child.type == typeof(FixedMarker) || child.type == typeof(EndMarker)) continue;
                var clean = child;
                RemoveMarkers(ref clean);
                children.Add(clean);
            }
            loop.subSystemList = children.ToArray();
        }
        private static void Update()
        {
            DrainMainThreadQueue();
            Pump(nextFrame, Timing.NextFrame);
            Pump(delays, Timing.Delay);
            // Canceled fixed waits must also be released while timeScale is zero.
            fixedUpdate.RemoveAll(request => request.Operation.IsCompleted);
            endOfFrame.RemoveAll(request => request.Operation.IsCompleted);
            operations.RemoveAll(operation => operation.IsCompleted);
        }
        private static void FixedUpdate() { fixedTick++; Pump(fixedUpdate, Timing.FixedUpdate); }
        private static void EndOfFrame() { endTick++; Pump(endOfFrame, Timing.EndOfFrame); }
        private static void Pump(List<Request> pending, Timing timing)
        {
            List<Awaitable> ready = null;
            int kept = 0;
            for (int i = 0; i < pending.Count; i++)
            {
                var request = pending[i];
                if (request.Operation.IsCompleted) continue;
                bool due = timing == Timing.FixedUpdate ? request.Tick <= fixedTick :
                    timing == Timing.EndOfFrame ? request.Tick <= endTick :
                    request.Frame <= Time.frameCount && (timing != Timing.Delay || request.Deadline <= Time.timeAsDouble);
                if (due)
                {
                    if (ready == null) ready = new List<Awaitable>();
                    ready.Add(request.Operation);
                }
                else pending[kept++] = request;
            }
            pending.RemoveRange(kept, pending.Count - kept);
            if (ready == null) return;
            // Remove requests before invoking callbacks: callbacks can queue new waits or trigger shutdown.
            foreach (var operation in ready)
            {
                try
                {
                    if (stopped) operation.Cancel();
                    else operation.State.TryComplete(true);
                }
                catch (Exception exception) { Debug.LogException(exception); }
            }
        }
        private static void DrainMainThreadQueue()
        {
            int count = mainThreadQueue.Count;
            for (int i = 0; i < count && mainThreadQueue.TryDequeue(out var action); i++)
            {
                try { action(); }
                catch (Exception exception) { Debug.LogException(exception); }
            }
        }
        private static void Shutdown()
        {
            stopped = true;
            var pending = new List<Awaitable>(operations);
            operations.Clear();
            foreach (var list in new[] { nextFrame, delays, fixedUpdate, endOfFrame })
            {
                foreach (var request in list) pending.Add(request.Operation);
                list.Clear();
            }
            foreach (var operation in pending)
            {
                try { operation.Cancel(); }
                catch (Exception exception) { Debug.LogException(exception); }
            }
            DrainMainThreadQueue();
        }
    }
}
#endif
