# Awaitable validation

Run from PowerShell with an installed Unity Editor. Use a new temporary project directory, separate from your game project:

```powershell
./run-validation.ps1 -UnityEditor 'D:/Software/Unity/2022.3.60f1/Editor/Unity.exe' -ProjectPath './AwaitableValidation2022'
```

The script copies the compatibility sources, destruction-token helpers, and contract tests into a minimal standalone project. It refuses to overwrite an existing project unless it contains the runner's `.awaitable-validation` marker. The generated project uses only Unity's built-in JSON module; it does not need a test-framework package.

The suite checks frame and fixed-update timing, scaled time, cancellation (including cancellation from a worker thread while paused), completion sources and reset, generic and non-generic async state machines, Task interop, exception identity, main/background switching, concurrent completion, resource-load awaiting, destruction tokens, and coroutine integration. Older Unity versions additionally check duplicate-consumer rejection and cancellation when Play Mode exits.

Each run enters Play Mode twice with Domain Reload disabled, to exercise static scheduler reset and PlayerLoop reinstallation. Successful runs exit with code 0 and log `AWAITABLE_VALIDATION_PASSED_BOTH_PLAY_SESSIONS`. Inspect `results-round-1.json`, `results-round-2.json`, and `Editor.log` in the generated project. A nonzero exit code is a failure. The editor bootstrap enforces a three-minute timeout per round; the launcher defaults to a ten-minute overall limit, adjustable with `-TimeoutSeconds`.

Unity 2021.3.45f1 and 2022.3.60f1 exercise the compatibility implementation. Passing a 2023.1-or-newer Editor exercises native Awaitable instead, with the compatibility assembly excluded. Native `EndOfFrameAsync` checks are explicitly skipped in Editor batch mode because [Unity does not support that combination](https://docs.unity3d.com/ScriptReference/Awaitable.EndOfFrameAsync.html); the JSON reports record this omission. The compatibility implementation's PostLateUpdate timing is still checked in batch mode. Editor startup or import failures must be resolved before treating such a run as a contract result.

Validated on 2026-09-09: Unity 2021.3.45f1 and 2022.3.60f1 each passed all 13 test groups in both Play sessions, including shutdown cancellation. Native Unity 6000.0.47f1 passed its 12 test groups in both sessions, with the documented batch-mode EndOfFrame checks skipped.
