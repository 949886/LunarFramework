using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Luna.Extensions.Unity;
using UnityEngine;

// Copied into a minimal project by run-validation.ps1; no test-framework package is required.
public sealed class AwaitableContractTests : MonoBehaviour
{
    [Serializable] private sealed class Report
    {
        public string unityVersion;
        public List<string> passed = new List<string>();
        public List<string> skipped = new List<string>();
        public string failure;
    }
    private Report report;
    private int mainThread;
    private int lateFrame;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        var go = new GameObject("Awaitable contract tests");
        DontDestroyOnLoad(go);
        go.AddComponent<AwaitableContractTests>();
    }
    private void LateUpdate() { lateFrame = Time.frameCount; }
    private IEnumerator Start() { yield return null; Run(); }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
    private async void Run()
    {
        report = new Report { unityVersion = Application.unityVersion };
        mainThread = Thread.CurrentThread.ManagedThreadId;
        try
        {
            await Test("next frame and end of frame", FrameTiming);
            await Test("fixed update", FixedTiming);
            await Test("scaled delay and paused frames", ScaledDelay);
            await Test("pre-canceled and pending cancellation", Cancellation);
            await Test("completion sources and reset", CompletionSources);
            await Test("generic/non-generic async builders and Task interop", Builders);
            await Test("exception propagation", Exceptions);
            await Test("main/background thread switches", ThreadSwitches);
            await Test("continuation registration/completion races", CompletionRaces);
            await Test("AsyncOperation adapters", AsyncOperations);
            await Test("destroy cancellation", DestroyToken);
            await Test("coroutine adapter", CoroutineAdapter);
#if !UNITY_2023_1_OR_NEWER
            await Test("single consumer enforcement", SingleConsumer);
            AwaitShutdown();
#endif
        }
        catch (Exception exception) { report.failure = exception.ToString(); }
        finally
        {
            Time.timeScale = 1;
            var path = Path.GetFullPath(Path.Combine(Application.dataPath, "../results.json"));
            File.WriteAllText(path, JsonUtility.ToJson(report, true));
            Debug.Log("AWAITABLE_TEST_RESULT " + (report.failure == null ? "PASS" : report.failure));
        }
    }
    private async Task Test(string name, Func<Task> action)
    {
        Debug.Log("AWAITABLE_TEST_START " + name);
        await action();
        Check(Thread.CurrentThread.ManagedThreadId == mainThread, "Test did not return to the main thread: " + name);
        report.passed.Add(name);
        Debug.Log("AWAITABLE_TEST_PASS " + name);
    }
    private async Task FrameTiming()
    {
        for (int i = 0; i < 3; i++)
        {
            int frame = Time.frameCount;
            await Awaitable.NextFrameAsync();
            Check(Time.frameCount > frame, "NextFrameAsync resumed in its creation frame.");
        }
        if (CanTestEndOfFrame())
        {
            int current = Time.frameCount;
            await Awaitable.EndOfFrameAsync();
            Check(Time.frameCount == current, "EndOfFrameAsync skipped a frame when called before rendering.");
            Check(lateFrame == current, "EndOfFrameAsync ran before LateUpdate.");
        }
        await Awaitable.NextFrameAsync();
    }
    private async Task FixedTiming()
    {
        double time = Time.fixedTimeAsDouble;
        await Awaitable.FixedUpdateAsync();
        Check(Time.fixedTimeAsDouble > time, "FixedUpdateAsync did not wait for a physics tick.");
    }
    private async Task ScaledDelay()
    {
        Time.timeScale = 0;
        bool done = false;
        async Task Delay() { await Awaitable.WaitForSecondsAsync(0.04f); done = true; }
        var wait = Delay();
        for (int i = 0; i < 4; i++) await Awaitable.NextFrameAsync();
        Check(!done, "Scaled delay advanced while paused.");
        if (CanTestEndOfFrame()) await Awaitable.EndOfFrameAsync();
        Time.timeScale = 2;
        double start = Time.timeAsDouble;
        await wait;
        Check(Time.timeAsDouble - start >= 0.035, "Scaled delay finished too early.");
        Time.timeScale = 1;
        await Awaitable.WaitForSecondsAsync(0);
    }
    private bool CanTestEndOfFrame()
    {
#if UNITY_EDITOR && UNITY_2023_1_OR_NEWER
        // Native EndOfFrameAsync is unsupported in Editor batch mode.
        // https://docs.unity3d.com/ScriptReference/Awaitable.EndOfFrameAsync.html
        if (Application.isBatchMode)
        {
            const string reason = "native EndOfFrameAsync: unsupported in Editor batch mode";
            if (!report.skipped.Contains(reason)) report.skipped.Add(reason);
            return false;
        }
#endif
        return true;
    }
    private async Task Cancellation()
    {
        using (var source = new CancellationTokenSource())
        {
            source.Cancel();
            bool canceled = false;
            try { await Awaitable.NextFrameAsync(source.Token); }
            catch (OperationCanceledException) { canceled = true; }
            Check(canceled, "Pre-canceled token was ignored.");
        }
        using (var source = new CancellationTokenSource())
        {
            var operation = Awaitable.WaitForSecondsAsync(1000, source.Token);
            source.Cancel();
            bool canceled = false;
            try { await operation; }
            catch (OperationCanceledException) { canceled = true; }
            Check(canceled, "Pending delay did not cancel.");
        }
        var direct = Awaitable.NextFrameAsync();
        direct.Cancel();
        bool directlyCanceled = false;
        try { await direct; }
        catch (OperationCanceledException) { directlyCanceled = true; }
        Check(directlyCanceled, "Cancel() was ignored.");
        using (var source = new CancellationTokenSource())
        {
            Time.timeScale = 0;
            var operation = Awaitable.FixedUpdateAsync(source.Token);
            var cancel = Task.Run(() => source.Cancel());
            bool canceled = false;
            try { await operation; }
            catch (OperationCanceledException) { canceled = true; }
            await cancel;
            await Awaitable.MainThreadAsync();
            Time.timeScale = 1;
            Check(canceled, "Background cancellation did not release a paused fixed-update wait.");
        }
    }
    private async Task CompletionSources()
    {
        var source = new AwaitableCompletionSource<int>();
        var operation = source.Awaitable;
        bool resumed = false;
        async Task<int> Consume() { int value = await operation; resumed = true; return value; }
        var consumer = Consume();
        source.SetResult(42);
        Check(resumed, "Completion source did not invoke its continuation synchronously.");
        Check(await consumer == 42, "Generic result was lost.");
        Check(!source.TrySetResult(99), "Double completion succeeded.");
        source.Reset();
        var fresh = source.Awaitable;
        source.SetResult(7);
        Check(await fresh == 7, "Reset did not create a fresh operation.");
        var empty = new AwaitableCompletionSource();
        var emptyOperation = empty.Awaitable;
        empty.SetResult();
        await emptyOperation;
        Check(!empty.TrySetCanceled(), "Completed source accepted cancellation.");
        empty.Reset();
        var canceledOperation = empty.Awaitable;
        empty.SetCanceled();
        bool canceled = false;
        try { await canceledOperation; } catch (OperationCanceledException) { canceled = true; }
        Check(canceled, "Completion source cancellation failed.");
    }
    private readonly struct NotifyOnly
    {
        public Awaiter GetAwaiter() => new Awaiter();
        public readonly struct Awaiter : INotifyCompletion
        {
            public bool IsCompleted => false;
            public void GetResult() { }
            public void OnCompleted(Action continuation) => ThreadPool.QueueUserWorkItem(_ => continuation());
        }
    }
    private async Awaitable<int> NestedBuilder(int seed)
    {
        int value = seed;
        await Awaitable.NextFrameAsync(); value += 2;
        await Task.Delay(1); value *= 3;
        await new NotifyOnly(); value += 1;
        await Awaitable.MainThreadAsync();
        return value;
    }
    private async Task Builders()
    {
        Check(await NestedBuilder(4) == 19, "State machine locals were corrupted across suspension.");
        async Awaitable Nothing() { await Awaitable.NextFrameAsync(); }
        await Nothing();
        async Awaitable<int> Immediate() { return 13; }
        Check(await Immediate() == 13, "Synchronous builder completion failed.");
    }
    private async Task Exceptions()
    {
        var expected = new InvalidOperationException("Expected failure");
        async Awaitable<int> Fail() { await Awaitable.NextFrameAsync(); throw expected; }
        Exception actual = null;
        try { await Fail(); } catch (Exception exception) { actual = exception; }
        Check(ReferenceEquals(expected, actual), "Async exception identity was not preserved.");
        var source = new AwaitableCompletionSource();
        var operation = source.Awaitable;
        source.SetException(expected);
        actual = null;
        try { await operation; } catch (Exception exception) { actual = exception; }
        Check(ReferenceEquals(expected, actual), "Completion source exception was lost.");
    }
    private async Task ThreadSwitches()
    {
        await Awaitable.MainThreadAsync();
        Check(Thread.CurrentThread.ManagedThreadId == mainThread, "Main thread fast path failed.");
        await Awaitable.BackgroundThreadAsync();
        Check(Thread.CurrentThread.ManagedThreadId != mainThread, "Background switch failed.");
        int background = Thread.CurrentThread.ManagedThreadId;
        await Awaitable.BackgroundThreadAsync();
        Check(Thread.CurrentThread.ManagedThreadId == background, "Background fast path switched threads.");
        await Awaitable.MainThreadAsync();
        Check(Thread.CurrentThread.ManagedThreadId == mainThread, "Return to Unity main thread failed.");
        async Awaitable<int> FinishInBackground()
        {
            await Awaitable.BackgroundThreadAsync();
            return Thread.CurrentThread.ManagedThreadId;
        }
        Check(await FinishInBackground() != mainThread, "Nested work did not execute in background.");
        Check(Thread.CurrentThread.ManagedThreadId == mainThread, "Builder did not preserve main-thread completion affinity.");
    }
    private async Task CompletionRaces()
    {
        for (int i = 0; i < 200; i++)
        {
            var source = new AwaitableCompletionSource<int>();
            var operation = source.Awaitable;
            int winners = 0;
            var success = Task.Run(() => { if (source.TrySetResult(5)) Interlocked.Increment(ref winners); });
            var cancellation = Task.Run(() => { if (source.TrySetCanceled()) Interlocked.Increment(ref winners); });
            try { Check(await operation == 5, "Racing completion returned a corrupt result."); }
            catch (OperationCanceledException) { }
            await Task.WhenAll(success, cancellation);
            Check(winners == 1, "Completion race had more than one winner.");
        }
        await Awaitable.MainThreadAsync();
    }
    private async Task AsyncOperations()
    {
        var request = Resources.LoadAsync<TextAsset>("AwaitableFixture");
        await request;
        Check(request.isDone && request.asset != null, "AsyncOperation awaiter failed.");
        await Awaitable.FromAsyncOperation(request);
        Check(((TextAsset)request.asset).text.Contains("fixture"), "Loaded asset is invalid.");
        using (var source = new CancellationTokenSource())
        {
            source.Cancel();
            bool canceled = false;
            try { await Awaitable.FromAsyncOperation(request, source.Token); }
            catch (OperationCanceledException) { canceled = true; }
            Check(canceled, "AsyncOperation ignored pre-cancellation.");
        }
    }
    private async Task DestroyToken()
    {
        var go = new GameObject("Destroy token test");
        var component = go.AddComponent<AwaitableContractProbe>();
        var token = component.GetCancellationTokenOnDestroy();
        Destroy(go);
        await Awaitable.NextFrameAsync();
        await Awaitable.NextFrameAsync();
        Check(token.IsCancellationRequested, "Destroy did not cancel the lifetime token.");
    }
    private async Task CoroutineAdapter()
    {
        bool finished = false;
        IEnumerator Routine()
        {
            yield return Awaitable.NextFrameAsync();
            finished = true;
        }
        StartCoroutine(Routine());
        for (int i = 0; i < 4 && !finished; i++) await Awaitable.NextFrameAsync();
        Check(finished, "Coroutine did not resume after the Awaitable.");
    }
#if !UNITY_2023_1_OR_NEWER
    private async Task SingleConsumer()
    {
        var source = new AwaitableCompletionSource<int>();
        var operation = source.Awaitable;
        source.SetResult(4);
        Check(await operation == 4, "First consumption failed.");
        bool rejected = false;
        try { await operation; } catch (InvalidOperationException) { rejected = true; }
        Check(rejected, "Second consumption was not rejected.");
    }
    private async void AwaitShutdown()
    {
        try { await Awaitable.WaitForSecondsAsync(float.MaxValue); }
        catch (OperationCanceledException)
        {
            File.WriteAllText(Path.Combine(Application.dataPath, "../shutdown.txt"), "canceled");
        }
    }
#endif
}
public sealed class AwaitableContractProbe : MonoBehaviour { }
