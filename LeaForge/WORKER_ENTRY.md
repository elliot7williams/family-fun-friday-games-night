# Worker Entry Boundary

The worker now validates arguments and starts its guarded diagnostic path before
interpreter binding. A private `Run` method with `NoInlining` separates interpreter
code from the entry method. Its binding/JIT exceptions are inside the existing
entry exception handler, after mitigation setup and the entry clock.

This fixes a reproduced dependency-failure path, not the intermittent five-second
startup timeout. The old worker fails even an empty argument list with a Jint
`FileNotFoundException` before returning the intended argument status.

## Regression And Verification

- A separate AppDomain containing only the worker, its config, and the test EXE
  reproduces that failure with no Jint or Acornima DLL. The initial regression
  fails against the preserved old worker; its three-file fixture is retained.
- All 14 new checks pass. Empty, unknown, and extra arguments return status 2
  without output or interpreter loading. Metadata verifies the non-inlined
  runtime boundary and BCL-only entry locals.
- A separate process with valid pipe arguments and missing interpreter DLLs
  returns the handled status 4. Its stderr proves entry and guards precede the
  dependency failure, with no init marker or stdout. No page code is supplied.
- Full source and freshly extracted standalone-package runs each pass 1,171
  checks. The new group runs last, after the existing startup/execution gates.
- Worker, tests, and browser builds have zero warnings/errors. The extracted
  actual browser EXE passes inline startup, native counter activation, isolation,
  128 MiB limit, and module checks; its nonblank capture is visually inspected.
- Stage, ZIP-stream, and extraction audits verify all 45 payload files plus the
  manifest. Only the worker changes among 19 runtime dependencies; the other 18
  are byte-identical to the previous verified candidate.

The worker remains 44,032 bytes. There is an additional non-inlined call;
unchanged PE size is not a zero-overhead or speedup claim. Aliases, protocol,
dependency versions, bootstrap bytes, memory/CPU/statement limits, five-second
startup budget, two-second execution default, and remote-origin gate are unchanged.

The boundary uses the documented
[MethodImplOptions.NoInlining](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.methodimploptions?view=netframework-4.8)
flag. The regression verifies behavior on this installed framework; it does not
claim that every runtime loads all assemblies at an identical point.

## Startup Observations

A diagnostic-only readable-bootstrap profile completes at 1,657 ms wall/109 ms
reported CPU initially and 10 ms wall/0 ms reported CPU warm. This is an
instrumented trusted path with coarse CPU accounting, not an isolated-worker or
cold-cache benchmark. The wall/CPU gap does not identify its cause.

The existing native comparison then alternates three pairs of fresh runtime
paths, using identical seven companion files and the original limits:

| Pair | Old Worker Wall/CPU (ms) | New Worker Wall/CPU (ms) |
| --- | --- | --- |
| 0 | 3135 / 421, pass | 2393 / 171, pass |
| 1 | 5012 / 46, timeout | 2387 / 78, pass |
| 2 | 2365 / 171, pass | 2549 / 171, pass |

The comparison exits with failure because one old-worker launch times out. Its
last captured marker is `network`, before VM construction completes, rather
than `parsed`. All samples and that failed eight-file runtime are preserved;
there are no retries. Three new-worker passes do not prove a causal timeout fix
or a general speedup. Machine/cache conditions are not controlled.

## Provenance And Gates

Worker SHA-256:
`74B277AACB131AB476F8B5B301AD520778C7F1CE892A51AED8C1BC3EF0CF5EE0`.
Parent SHA-256 remains
`A19FEB7E4CF4B5E42F4D85BCF2E207A8090208ED029E81A48F6FB0B4BD2F3ABD`.
The worker was built from `4abef21` plus these source edits. No engine dependency
or parent SDK rebuild is transferred from a later Git metadata version.

Standalone package:
`L:/Lea Builds/Lea-146-worker-entry/Lea-js-146-native_2026-10-07_23-41-17.zip`,
2,990,438 bytes, SHA-256
`317A2766F20CA3AB24FA45238C1CD03AD98042E835E60603C74FC0C4A172DEA9`.
Evidence is under `L:/Lea Evidence/Lea-146-startup-boundaries-2026-10-07`.

No new Firespider candidate or host matrix is verified for this worker. The
previous v22 archives retain the old worker and their own verification scope.
Installed Firespider files stay unchanged. Milestone 146 stays open and broader
readiness stays about 65%; reliable cold startup and current host verification
remain required before adoption.
