# Probe Cleanup And Command Progress

Milestone 146 remains open. Broader browser readiness stays about 65%, an
engineering estimate, not a standards conformance score or a release claim.

## Readiness Boundary

The isolated script worker now closes its trusted startup network probe before
writing hello. Previously its `using` block ended after hello/ready, leaving
probe disposal on the first page command's execution deadline. The verified
denial result is retained, but cleanup must now finish inside the existing
5,000-ms startup budget. A new `startup closed` marker follows disposal and
precedes hello. No page code is admitted before the verified handshake.

This closes a readiness gap. It does not establish that probe disposal caused
the preceding read timeout, and does not justify a startup-fix or speedup claim.
The earlier failures remain preserved in their sealed evidence folders.

## Private Progress Trace

The existing opt-in `LEA_SCRIPT_STARTUP_TRACE=1` also records native command
progress. Normal runs emit no `frame` lines. The unchanged wire format still
uses validated sequence IDs, typed bridge requests and the existing reply checks.

| Stage | Boundary |
| --- | --- |
| `invoke` | Ready has been written; deferred page-loop invocation follows |
| `enter` | The non-inlined native page-loop method has entered |
| `read` | The loop is about to read another frame |
| `exec`, `timer`, `event` | Frame kind, fields and sequence accepted; payload handling follows |
| `call` | A bounded DOM bridge request is about to be sent |
| `reply` | Its reply has been received and validated, before value conversion |
| `tasks` | Page script execution returned; its microtask checkpoint follows |
| `end` | Execution/checkpoint returned; completion write follows |
| `error` | A handled interpreter/protocol budget failure reached error reporting |

Lines contain only known stage names, validated command/request IDs and elapsed
worker-clock milliseconds. They do not include page source, URLs, titles, bridge
payloads or exception text. Bridge traces retain the first four requests and
request 256 only; later ordinary requests are not logged. The parent retains
its existing bounded stderr tail and 512-character public diagnostics.

`end` precedes the write, so it is not a proof that the parent received completion.
Likewise `call` is before serialization/write and `reply` before value conversion.
These boundaries distinguish places to investigate, not individual root causes.
Opt-in tracing can perturb timings. It never chooses another parsing strategy,
pre-JITs page code, bypasses validation or changes the execution budgets.

## Regression Coverage

`Lea.Script.Tests.exe <repository> --frame-progress`, or `-fp`, exercises fresh
native workers with tracing off and on. Its 23 assertions cover default budgets,
verified isolation, cleanup ordering, actual receipt/bridge/completion order,
trace privacy and request suppression, timer failure, exact child retirement,
a clean replacement realm and timer execution after recovery. Parent sequence
IDs continue across worker replacement. Extra CLI arguments are rejected.

The ordinary full suite includes these checks. Existing worker-entry tests keep
the non-inlined post-handshake page-loop boundary and dependency-error handling.
Existing startup-phase tests now require probe cleanup before hello.

One focused prepared worker records invocation at 1,882 ms, page-loop entry at
1,890 ms, command receipt at 1,894 ms and completion entry at 2,022 ms. Those
are one successful, instrumented worker's observations, not a cold-cache
benchmark, speedup or reliability measurement.

The candidate builds without warnings or errors and passes 15 worker-entry,
six prepared-entry, 23 command-progress, 16 startup-phase and seven native
hard-deadline/recovery assertions. The short command alias passes the same 23
checks in its separately declared run; extra arguments fail as expected.
The default `Be=0` control also compiles cleanly and passes four compiled-entry
and 23 native command-progress/recovery assertions. It is a separate control,
not adoption of the prepared bootstrap mode.

The single broad run passes **1,417 assertions** and exits one during a native
timer restart after Stop. The earlier isolated-timer recovery boundary passes
in this run, but this is not proof that the previous timeout is resolved. The
new failure is a **startup** timeout at `phase=hello`, wall 5,007 ms / CPU 31 ms,
after `configured` and before `vm`. It has not reached probe cleanup or hello.
Its log and stopped runtime remain retained without retry. The planned full
total is 2,293 assertions; that total is not achieved.

The failure identifies the engine-construction boundary, not the cause of its
wait. There is no debugger/observer attached to this run, no deadline extension,
and no substituted passing full run. Startup reliability remains unfinished.

The standalone browser smoke passes with actual exit zero, verified isolation,
its expected title and counter mutation. Its fresh 980x680 capture is inspected.
The copied .NET 4.8 Firespider matrix passes **42 cases**, plus the 14 clipboard
and 12 observation contracts, then fails its scripting case on a later supplied
script (`var retainedState=41;`). That worker reaches `closed` and hello-entry,
but the parent deadline expires at 5,025 ms / CPU 578 ms before a verified
handshake completes. It is retained without retry. The three final script-host
captures are not produced; older host captures are not substituted.

The pinned host harness checks each passing child's actual exit zero and exactly
one completion line; the failed script child exits one. The outer application
wrapper has no child exit code of its own. This is a regression risk against the
preceding 43-case host pass. No installed .NET 4.6.1 host is rebuilt, migrated or
replaced, and its six actual-EXE startup scenarios are not newly verified. The
renderer smoke is not rerun; its bytes match the preceding tested build.

There are no surviving owned test processes. The original 67 host inputs, three
installed files, normal production outputs and 379-file copied-host basis remain
checksum-verified unchanged. Source and failures are preserved separately from
the experimental runtime package.

## Unchanged Protections

`Be=0` remains the build default; the experimental browser uses `Be=2`.
Startup stays 5,000 ms, default page execution 2,000 ms, Job CPU rate 25%,
Job memory 128 MiB, and active process count one. Interpreter statement,
allocation, recursion, regex, stack, array and CLR restrictions are unchanged.
AppContainer, exact inherited pipes, mitigations, canary/network checks,
permission revocation and worker retirement remain required. No retry,
unrestricted fallback or remote-script enablement is added.

Only `ScriptWorker/Lea.Script.Worker.exe` changes among the 19 production
records. It is 50,176 bytes, SHA256
`68E260BD8FEA0809CD55F927108CD58D7BAAEFD0CD622BC706C25B2B32D8E86B`.
The required Engine/Win/Image bundle stays 749,056 bytes. JavaScript libraries
remain additional; short aliases and ZIP compression are not execution-speed
measurements. All original/installed Firespider files and normal production
outputs remain unchanged.

Evidence is retained under
`H:/Lea Evidence/Lea-146-frame-readiness-2026-10-11`.

The runtime-only ZIP contains the 24 tested production/browser files, excluding
tests and symbols. Its 8,272,124 raw bytes compress to 2,900,553 bytes; every
entry is verified against the frozen tested bytes. Extract to local NTFS and
keep the eight worker files together. This is an experimental storage package,
not a startup or release pass. The source is archived separately and verified
entry by entry before the Git checkpoint is sealed.

Experimental standalone browser:
`C:/Users/Elliot/AppData/Local/Lea/EngineBuilds/Lea-146-frame-readiness-2026-10-11/Lea.Script.Browser.exe`.
