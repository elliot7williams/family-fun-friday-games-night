# Production-Path Startup Tracing

Milestone 146 remains experimental, with broader readiness about 65%, not a
standards score. This follow-up improves diagnosis and removes a parser fork;
it does not establish the cause or fix of intermittent startup failures.

## Implementation

Traced and untraced workers both call `vm.Evaluate(source)` for the embedded
DOM bootstrap. The diagnostic-only prepared-script branch is removed, keeping
the normal parser, sequential initialization and existing 100-ms regex setting.
The evaluate-to-DOM region includes parsing and execution; it no longer reports
those as separate phases. Timing tracing still adds I/O and measurement overhead
and can perturb scheduling. A passing traced run is not proof of normal latency.

The worker emits compact fixed startup phase markers to its existing stderr:
entry, guards, init, canary, network, VM, bridge, source, evaluate, DOM and probes.
`LEA_SCRIPT_STARTUP_TRACE=1` adds wall/CPU measurements, rather than selecting
another parser. The existing 1,024-character stderr retention and 512-character
`Jp.Sd` report bounds remain. A missing early marker can mean the child has not
entered Main, or that the asynchronous drain has not captured it; markers are
observations, not a process-stack trace. Warm calls retain the startup report.

Microtask result/status assertions include the actual status, bounded message,
startup report and at most 256 authored code characters on failure. Worker
cooperative/recursion/async warmups also retain `Sd`. Their assertions, ordering,
budgets and check counts do not change. Six new startup checks cover production
path tracing and marker retention, bringing focused startup to 70 and full source
to 630 checks.

Startup remains 5,000 ms; parent execution defaults to 2,000 ms, clamped to
25..5,000. The zero-capability AppContainer, positive control and denial probes,
exact-file ACLs/leases, verified mitigations, 128-MiB/25%-CPU/one-process Job,
hard termination, permission generation and owner-thread DOM rules remain.
Wire version 3 and public aliases are unchanged. No profiler, required-core or
interpreter dependency is added.

## Verification Chronology

Logs are retained in `%TEMP%`; later passes do not erase earlier failures.

1. `Lea-startup-stage-full-trace.log`: the earlier split-parser diagnostic build
   passes all 624 checks. This does not verify the later production-path change.
2. `Lea-startup-observational-red-build.log` and
   `Lea-startup-observational-red.log`: build succeeds without warnings/errors;
   the new assertion reproduces the exact parser-path substitution failure.
3. `Lea-startup-observational-build.log`: the unified parser builds without
   warnings/errors. `Lea-startup-observational-full-trace.log` passes its new
   startup checks but fails the recursive queueMicrotask limit assertion. That
   assertion did not record the actual result, so its cause is not inferred.
4. `Lea-startup-result-diagnostic-build.log`: improved result assertions build
   without warnings/errors. `Lea-startup-result-diagnostic-full-trace.log`
   passes all 627 checks. The normal counterpart,
   `Lea-startup-result-diagnostic-full-normal.log`, fails fresh-worker recursion
   warmup with `timeout; Worker startup deadline exceeded`. The warmup did not
   yet record `Sd`; no child phase is available for that failure.
5. `Lea-startup-compact-red-build.log` and `Lea-startup-compact-red.log`: build
   succeeds without warnings/errors; the exact missing normal-path marker
   regression is reproduced. Both red wrappers require the specific expected
   failure, rather than accepting an arbitrary failed process.
6. `Lea-startup-compact-build.log`: compact markers and warmup diagnostics build
   without warnings/errors. `Lea-startup-compact-full-normal.log` fails early
   at the trusted in-process computed script with status `limit`, before any
   isolated worker launches. Its assertion lacks the exact constraint/phase;
   this is not evidence about isolated startup or a proven common cause.
7. `Lea-startup-compact-green.log`: all 70 focused startup checks pass, including
   fresh runtime paths, exact ACL/process retirement, persistent bounded
   reports, timing modes, UI cancellation/revocation and no-owner-pump Stop.
8. `Lea-startup-compact-final-full-normal.log`: all 630 current-source checks
   pass, including isolated recovery/limits, events, microtasks, timers, native
   fields/queues, 940/420-pixel rendering and remote-origin refusal. Timing is
   not globally enabled; the startup suite intentionally enables one copied
   runtime as part of its normal coverage. This is one passing full run, not a
   reproducible cold-start guarantee or a diagnosis of earlier failures.
9. `Lea-startup-compact-solution-build.log`: the solution and standalone browser
   compile successfully, with 18 pre-existing warnings in unchanged prototypes.
   No core/image code changes in this round; their full suites are not repeated.

## Separate Firespider Build

`Lea-startup-trace-v10-candidate-build.log` compiles the separate net48 optional
script host with 67 declared source inputs. Its diagnostic runtime is
`dist/Firespider/Lea-146-trace-v10/runtime`. The EXE SHA256 is
`8D962A2C7738F14F41755982598C8FF6DA2C977F074CC7066938D75B6FBAEE68`.
The current worker hash matches the test, standalone browser and host copies:
`5D8AF6AC0C74D3CD842670616DAFFC4E86383EE4297593394AD63BDA435F39DF`.
Its 34,816 bytes are 512 fewer than v9's 35,328, not a measured speed improvement.
The required Engine/Win/image bundle remains 742,400 bytes.

`Lea-startup-trace-v10-host-script.log` passes the scripting case, including
bounded timers, inline/lifecycle/native events, recovery, split isolation,
disposal and remote refusal. First demand is 5,944 ms and split wait 5,750 ms;
these aggregate multi-call waits are not individual startup-budget measurements.

`Lea-startup-trace-v10-exe-startup.log` passes all six actual-EXE scenarios:
clean/restored/locked/oversized/lazy-background/lazy-all, responsive with no
legacy engine loaded. Observed readiness is 4,456..7,210 ms, not a latency SLA.
The lazy-all selected tab starts deferred and resumes. The full 43-case host
matrix, fresh extracted runtime and independent source rebuild are not verified
for v10. No new release ZIP is published. The installed root EXE/Engine/Win
hashes match their preserved pre-work values; the live host is not replaced.

## Next Gate

Retain the failed logs and gather an actual failed normal-path worker phase,
then address its measured cause without relaxing isolation or deadlines.
Repeated cold-path and package/source verification remain necessary alongside
broader DOM, remote-origin/network policy and Lea-only host integration.
See [bootstrap work](BOOTSTRAP.md), [timers](TIMERS.md) and
[progress](PROGRESS.md).
