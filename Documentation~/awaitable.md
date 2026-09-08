# Awaitable

Lunar Framework uses `UnityEngine.Awaitable` for asynchronous operations on every supported Unity version.

| Unity version | Implementation |
| --- | --- |
| 2021.3 and 2022.3 | Bundled `Luna.Awaitable` assembly |
| 2023.1 and newer | Unity's native implementation |

The compatibility assembly and its sources are excluded when `UNITY_2023_1_OR_NEWER` is defined. There are no external async dependencies. Versions older than the framework's minimum, Unity 2021.3, are unsupported.

Scripts in `Assembly-CSharp` receive the compatibility assembly automatically. Custom assembly definitions that directly use these APIs on older Unity versions must reference `Luna.Awaitable`. The reference can remain when upgrading Unity; the compatibility assembly is then excluded.

## Usage

```csharp
using System.Threading;
using UnityEngine;

public static class Example
{
    public static async Awaitable<TextAsset> LoadAsync(CancellationToken token)
    {
        await Awaitable.NextFrameAsync(token);
        var request = Resources.LoadAsync<TextAsset>("Settings");
        await Awaitable.FromAsyncOperation(request, token);
        return (TextAsset)request.asset;
    }
}
```

`await request` also works for `AsyncOperation` subclasses when no cancellation token is needed. Canceling its Awaitable stops the wait; it does not abort Unity's underlying resource load or scene operation.

Supported APIs include `Awaitable`, `Awaitable<T>`, their async method builders, `AwaitableCompletionSource` and `AwaitableCompletionSource<T>`, `Cancel`, frame waits, scaled delays, thread switching, `AsyncOperation` awaiting, and yielding a non-generic Awaitable from a coroutine.

| Wait | Compatibility behavior |
| --- | --- |
| `NextFrameAsync` | Resumes in the next frame's Update phase |
| `FixedUpdateAsync` | Resumes after the next FixedUpdate phase |
| `EndOfFrameAsync` | Resumes at the end of PostLateUpdate, after LateUpdate and rendering phases |
| `WaitForSecondsAsync` | Uses scaled game time; does not advance while `Time.timeScale` is zero |
| `MainThreadAsync` | Returns to Unity's main thread; completes immediately if already there |
| `BackgroundThreadAsync` | Uses the thread pool; completes immediately if already off the main thread |

Create frame, delay, and `AsyncOperation` waits on the main thread. Their timing follows the runtime PlayerLoop. Editor update pumps thread-switch continuations, but does not simulate runtime frames outside Play Mode. Thread-pool work requires a platform that supports managed worker threads.

```csharp
await Awaitable.BackgroundThreadAsync();
// Perform computation without accessing Unity objects.
await Awaitable.MainThreadAsync();
// Access Unity objects here.
```

## Lifetime and completion

Each Awaitable has one consumer: await it once, or yield it once from a coroutine. Create a fresh operation for each wait. Completion-source continuations run synchronously on the completing thread. An `async Awaitable` method completes on the main thread if it started there, or a background thread if it started there.

The compatibility implementation uses managed objects, locks, and a PlayerLoop scheduler. It does not pool Awaitable instances and does not promise allocation-free execution or identical internal scheduling to Unity's native engine implementation. Duplicate consumption raises `InvalidOperationException` in the compatibility implementation; never rely on consuming an operation twice on any version.

Pass cancellation tokens to interrupt pending waits, and handle `OperationCanceledException` at the owning async entry point. Pending compatibility-scheduler operations are canceled when Play Mode exits or the application quits. `AwaitableCompletionSource.Reset()` creates a new Awaitable: finish the old operation and its consumer before reusing the source, and do not reset it concurrently with producers.

The framework's destruction-token helper is available on all supported versions:

```csharp
using Luna.Extensions.Unity;

// Within a MonoBehaviour, before destruction:
var token = this.GetCancellationTokenOnDestroy();
await Awaitable.NextFrameAsync(token);
```

Unity 2022.2 and newer use `MonoBehaviour.destroyCancellationToken`. Earlier versions attach a hidden helper component and cancel when its GameObject is destroyed. Destroying only the calling component does not cancel that older fallback token. As with Unity's `OnDestroy`, an object must have been active for its destruction callback to run.

The compatibility contract is exercised by the [standalone validation runner](../Tests~/Awaitable/README.md).
