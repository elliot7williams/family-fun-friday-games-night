# Native Image Cancellation and Diagnostics

Later prefix-validation work is recorded in `IMAGE_HELLO.md`: a malformed first
word is rejected before waiting on guards, and the bounded diagnostic now has
a separate `guards` phase. The results below describe the earlier checkpoint;
they do not clear later intermittent startup failures.

Milestone 146 remains in progress. This follow-up fixes a reproduced native
image-decoder cancellation race and adds a compact diagnostic, not a different
renderer, a larger timeout, or a demonstrated browser-wide speed improvement.

## Cancellation Contract

Canceling inside the decoder's `started` callback previously produced a faulted
task with `NullReferenceException`. The cancellation callback retires the
shared worker and clears its reader/writer/process/job references; the decode
then continued through those cleared references. The retained red test records
that failure after 47 ms. It does not establish the cause of earlier image
admission or optional-script startup deadline failures.

The decoder now keeps local references for one serialized operation, schedules
with the caller's cancellation token, and checks that token after native-worker
registration and host/admission/start callbacks. An I/O error caused by worker
retirement is translated to the requested cancellation token. The task is
canceled rather than faulted; the owned helper is retired and later decoding
can create a fresh worker. A canceled waiter releases the semaphore only when
it actually acquired it, never another operation's permit.

Native process creation/preparation is still synchronous inside the worker
delegate. Cancellation during that section is registered on its return; this
change does not make every native launch step immediately interruptible.

## Compact Diagnostic

Read-only `V.Idg` is initially empty and atomically publishes the last completed
decoder-delegate report for that view, with a 512-character maximum:

```text
phase=done; result=ok; worker=45376; wall=60; ready=1; kind=; steps=queue=0;launch=0;host=0;admission=17;start=29;request=29;response=29;bitmap=30;done=60;
```

The sample is from one successful recovery, not a performance target. Fields:

- `phase`: last entered queue/launch/hello/host/admission/start/request/response/bitmap/done phase.
- `result`: `ok`, `cancel`, or `error`.
- `worker`: observed helper PID, or zero before one is available.
- `wall`: elapsed milliseconds from entry to the decode delegate.
- `ready`: whether this operation's worker passed its hardening handshake,
  not whether that process remains alive when the report is read.
- `kind`: caught exception type, not its message; a canceled pipe read may
  report `EndOfStreamException` while the task result is typed cancellation.
- `steps`: phase-entry times on that same clock; a reused worker omits `hello`.

The clock includes semaphore waiting and launch/preparation once the delegate
starts. It excludes network fetch, image preflight and scheduling before that
entry. Existing `Tm` remains the shared parent deadline for all of those stages.
The report contains no URL, path, image content, or worker stderr. It is not a
per-image history, a whole-navigation latency measurement, or a live-progress
API. A task canceled before its delegate starts publishes no new report, so
the previous value is retained. Diagnostic callback/formatting failures cannot
change decode results or prevent semaphore cleanup.

## Regression Evidence

The focused cancellation suite passes all six setup phases: started, host,
admission, a genuinely blocked partial handshake, pre-canceled scheduling, and
a genuinely blocked semaphore wait. It checks matching cancellation tokens,
completion within the existing two-second post-cancel assertion, exactly-once
owned-worker cleanup, no permit theft, bounded reports, diagnostic-consumer
failure containment, and normal native-view recovery. A further native
pre-canceled request preserves the prior report and live reusable worker.

`Lea.Image.Handshake.exe` is a tiny test-only helper built from
`Lea.Tests/ImageHandshake.cs`; it flushes only the four-byte hello marker and
waits. The test observes the Framework pipe byte counter without blocking,
then cancels while the decoder is reading the missing guard word. It holds a
separate process handle to verify exit without relying on PID reuse. Private
Framework byte/waiter counters are test setup instrumentation, not a runtime
engine dependency. This helper is not included in browser runtimes.

Earlier setup failures are retained. The first helper using the large test EXE
did not reach the observer's three-second setup boundary; its report remained
in `launch` after 3,424 ms, with no established cause. A later pipe-based
observer waited until the tiny helper's 60-second exit. Synchronous pipe-handle
inspection can block; replacing that observer with a nonblocking byte-counter
read reaches the intended `hello` phase. These failed setups do not justify
relaxing the production deadline or claiming the original intermittent startup
failure is solved.

## Verification

The picture-limit suite, focused final cancellation suite and complete final
core suite pass. All 713 scripting checks pass from source and from a fresh
standalone ZIP extraction. Native timer captures at 940/420 pixels are inspected
and retain nonblank painting, the Lea image, and nonoverlapping native fields.
The solution builds with zero errors and 18 existing prototype warnings; the
core-test and standalone-browser builds have zero errors/warnings.

Separate v13 compiles from 67 host inputs and passes all 43 host cases and six
actual-EXE startup scenarios. Readiness samples are 4,397..6,874 ms; lazy-all
starts deferred and resumes into Lea, with no legacy renderer. Host scripting
first-demand/split waits are 4,997/4,506 ms: aggregate host waits, not individual
worker startup measurements. Real recovery and hibernation timers are checked.

The extracted host runtime passes its scripting case. The extracted host-source
ZIP independently rebuilds with Framework 4.8 MSBuild; its 19 dependency records
match the runtime manifest, and 348 verified fixture/license files are supplied
for the harness. The rebuilt host passes scripting too. The full 43-case and
six-EXE matrices are not repeated on those extractions/rebuild.

The required bundle is 743,936 bytes: engine 419,840, WinForms 314,880, image
worker 9,216. The view DLL grows 1,024 bytes; the test-only handshake helper is
4,608 bytes and is not a browser dependency. The rebuilt optional script worker
is 35,328 bytes. These are binary-size measurements, not compressed size or
runtime speed claims. Engine SHA256:
`7C81BCF979F915975330FCDF1C4B2F5DA6C2CA781EDED40C190DC7EC65F575B1`.
View SHA256:
`AA0738B4B8A5A332123238519B7F71B67C9FB5763B00C0FC6BF297C3CD9B19AC`.
Image-worker SHA256:
`40F724209909C3A4D588DE7EB1902E3A1142C4C745DA461E55A83EA8AAF2245B`.
Optional script-worker SHA256:
`C0CD68E18BB0E3E028CA1B8CDD24395FF623005FA35B11D470252E9BC6958BAD`.

No isolation, protocol, deadline, CPU/memory limit, required dependency, or
installed Firespider file is changed. Earlier intermittent startup failures
remain unexplained; these passing runs do not establish stable cold-start
readiness. Milestone 146 stays in progress and broader readiness stays about 65%.

## Experimental Artifacts

Candidate EXE: `dist/Firespider/Lea-146-image-v13/runtime/Firespider Internet 19.exe`.
SHA256: `50072D9615407EEC422ACCD750E8DD8DADD094A7A6DA054AAEFD056DEA5360F3`.

Three archives pass exact inventory, length/SHA256, current-snapshot and fresh
extraction checks. They remain experimental, not a stable everyday-browser
release. Failed setup logs and passing verification records are retained in
`dist/Bench/Lea-146-image-v13-evidence`.

| Kind | Files | ZIP bytes | Archive name |
| --- | ---: | ---: | --- |
| Standalone | 41 | 2,983,028 | `Lea-js-146-native_2026-10-04_16-34-12.zip` |
| Host runtime | 372 | 3,693,035 | `Firespider-Lea-146-script-runtime_2026-10-04_16-34-26.zip` |
| Host source | 104 | 3,343,545 | `Firespider-Lea-146-script-host-source_2026-10-04_16-34-26.zip` |

Standalone SHA256:
`C76E160A090B49BF12D048702A8FA028F9DC2A0EFD6B39CF994F70C41153D901`.
Runtime SHA256:
`62290A59C4A94FD9C7B4D85D72A99875C6204635E2567195E4449C3638F4763E`.
Host-source SHA256:
`E0E2D07B16E7E947B0DE6F1C5B15217091535D25D2EE69547EC5803806633773`.

Standalone files are under `dist/LeaScript/Lea-146-image-v13`; host archives are
under `dist/Firespider/Lea-146-image-v13-packages`. Extract workers onto a local
ACL-capable filesystem, not FAT32. Host-source supplies buildable VB plus
compiled Lea dependencies; complete engine source is retained separately by
`Backup-Lea.ps1` and Git. Broader DOM/site compatibility, remote-origin policy,
full event-loop semantics and Lea-only host integration remain work.
