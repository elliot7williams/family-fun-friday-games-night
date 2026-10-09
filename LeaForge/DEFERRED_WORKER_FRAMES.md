# Deferred Worker Page Loop

Milestone 146 remains open; broader browser readiness stays about 65%.
This is a verified startup-code boundary, not a demonstrated speedup or a
fix for the preceding intermittent Firespider startup timeouts.

## Change

The script worker's page-frame loop now lives in a non-inlined local function
called only after the guarded bootstrap writes its verified hello. This keeps
page-only frame validation, execution and error handling out of the initial
`Run` method. The same captured request/sequence state and streams remain in use.
Page commands, DOM bootstrap bytes and parsing strategy are unchanged.

Only `ScriptWorker/Lea.Script.Worker.exe` changes among the nineteen production
records. Its PE size remains 46,080 bytes. The required renderer bundle remains
745,984 bytes; the optional JavaScript interpreter dependencies are additional.
Public compact aliases remain unchanged.

The native five-second handshake, default two-second page deadline, 25% Job CPU
rate, 128-MiB Job memory cap and interpreter limits remain unchanged. AppContainer
isolation, exact pipe inheritance, mitigations and denied file/network access
still precede page execution. There is no queue, warm fallback or automatic retry.

## Verification

Both changed projects build without warnings or errors. A new worker-entry
assertion verifies the separate non-inlined page-loop boundary. All fifteen
worker-entry checks pass, including rejection before interpreter binding and
the intentional missing-dependency failure. The ordinary, unobserved full
scripting suite passes all 1,747 checks once with the same final test executable.
Its expected missing-Jint message is not an unexpected startup failure.

Six predeclared alternating native launches use identical parent/dependency
bytes and fresh copied paths. All 27 comparison checks pass, including first
page execution with the default deadline, isolation, process retirement and
restoration of exact ACLs. Fresh paths are not cold OS/CLR caches.

| Pair | Worker | Ready wall ms | CPU ms |
| --- | --- | ---: | ---: |
| 0 | Previous | 3015 | 218 |
| 0 | Candidate | 2728 | 281 |
| 1 | Candidate | 2617 | 171 |
| 1 | Previous | 2609 | 281 |
| 2 | Previous | 2553 | 296 |
| 2 | Candidate | 2600 | 265 |

The ranges overlap. These samples do not demonstrate a startup speedup, lower
CPU cost, lower memory cost or reduced timeout probability.

## Firespider And Browser

Four separately planned observed whole-script cases run in previous, candidate,
candidate, previous order. Each starts from a fresh native copy of the same
379-file v33 host, with the identical host EXE and checksum-verified harness.
Only the candidate worker changes. All four pass, including split loading,
independent pane state, expected script errors/recovery, timer/event behavior,
worker disposal, UI-error checks and remote-origin refusal.

| Sample | Worker | Counter samples | Workers | Thread snapshots |
| --- | --- | ---: | ---: | ---: |
| 0 | Previous | 283 | 8 | 72 |
| 1 | Candidate | 252 | 8 | 59 |
| 2 | Candidate | 244 | 8 | 59 |
| 3 | Previous | 238 | 8 | 55 |

All samplers retire with zero counter/thread/observer errors, dropped requests,
dropped samples or thread truncation. Sampling wall time includes observer work;
it is not measured observer CPU or UI-thread delay. Both versions pass, so this
comparison cannot explain or replace the preceding captured failed split gate.
No full host-matrix, packaging or independent source-rebuild release gate is
claimed here.

The actual standalone test-browser EXE, with the candidate worker, passes its
inline lifecycle, click, isolation and process-retirement smoke once. Its EXE
and other production dependencies stay unchanged. Three fresh candidate host
captures and the standalone capture are visually inspected. Host captures are
validated against the candidate case's start/end times, not reused from an
earlier run.

## Preservation And Next Work

All original 67 Firespider source inputs and three installed binaries remain
unchanged. Previous production bytes, the candidate source/runtime, planned
attempts, logs, raw observations and capture provenance are retained under
`H:/Lea Evidence/Lea-146-deferred-worker-frames-2026-10-09`. A complete source
archive is verified entry by entry before the Git checkpoint is sealed.

The earlier after-parsed/before-DOM startup failures remain preserved. Reducing
work in the initial method is useful isolation of startup code, but a reliable
startup fix still needs failure-specific evidence. Keep the protections and
five-second deadline intact while investigating bootstrap evaluation and the
first-demand/concurrent native lifecycle. Remote scripting and broader web
compatibility also remain unfinished.
