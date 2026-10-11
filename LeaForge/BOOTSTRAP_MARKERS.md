# Private Bootstrap Checkpoints

Milestone 146 stays open; broader readiness remains about 65%, an engineering
estimate rather than a conformance result. This checkpoint improves startup
evidence and verifies a complete copied host, not an installed startup fix or
a stable scripting release.

## Initialization Boundary

Both trusted and isolated runtimes now use the same private `Bm` sequencer.
Its `Of` accepts exactly one string for each of six ordered initialization
stages, rejecting unknown names, extra arguments, wrong order and duplicates.
`End` rejects incomplete setup. The state belongs to one realm; the prepared
syntax cache never owns it. Empty calls retain the existing microtask drain.
Once initialized, nonempty calls remain rejected.

No new callback is exposed. The trusted fixed asset uses its existing `__jobs`
callback for markers, captures it privately, and deletes the global property
before returning its dispatcher. Page code cannot access that captured host
function. The worker logs only the six known labels, with optional existing
wall/CPU timing, never page source or marker arguments supplied by a page.

| Stage | Completed work |
| --- | --- |
| `declarations` | Global script declaration setup; immediately before the private block |
| `scope` | Private block declaration setup and initial callback captures |
| `events` | Event helpers and shared event descriptors |
| `interfaces` | Captured intrinsics, DOMException and DOMTokenList interfaces |
| `elements` | Shared element/tree descriptors and wrapper factory declarations |
| `document` | Document/window bindings and event-target initialization |

Jint's pinned [script evaluation implementation](https://raw.githubusercontent.com/sebastienros/jint/884bd0c4ebf7806f70041eae0370ee11a9c138bc/Jint/Engine.cs)
places global declaration setup after BeforeEvaluate. Its
[block implementation](https://raw.githubusercontent.com/sebastienros/jint/884bd0c4ebf7806f70041eae0370ee11a9c138bc/Jint/Runtime/Interpreter/Statements/JintBlockStatement.cs)
creates block declarations before the first block statement. The two first
markers therefore distinguish those boundaries from the existing `parsed`
callback. They do not identify a slow initializer or prove a speedup alone.

Normal production evaluation uses these same markers; timing does not choose
another bootstrap. `Be=0` remains the default and the delivered experiment
uses `Be=2`. The native parent startup deadline remains 5,000 ms, the worker
interpreter entry limit 5,000 ms, and trusted entry limit 1,000 ms. Statement,
allocation, recursion, regex, stack, array, CLR, CPU-rate, memory, process-count,
AppContainer, inherited-pipe, canary, network and hello guards are unchanged.

The user-facing diagnostic stays capped at 512 characters. Timing can push
early markers out of that tail; tests check its final `document` marker while
checking the compiled entry and all six unique ordered stages in full stderr.
Warm page evaluation must not repeat them. Existing diagnostic-only class
profiling uses a no-op callback instead of an undefined `__jobs` value.

## Checks And Observations

`Lea.Script.Tests.exe <repository> --bootstrap-markers-contract`, or `-bm`,
passes 42 checks against the actual trusted and worker assemblies. Both CLI
spellings pass separately. A retained preceding Script DLL fails the same
contract with missing `Lea.Script.Bm`; extra CLI arguments are rejected before
tests. This is a compiled contract, not a native worker launch.

The prepared candidate also passes six compiled-entry, 14 options, four asset,
14 cache-isolation/reset/concurrency/GC, 16 native phase, 36 trusted/native
scope, and seven real-worker deadline/retirement/recovery checks. The default
`Be=0` control compiles cleanly and passes four entry and 16 native phase checks.
The indirect mode is not newly compiled or run in this checkpoint.

One timed prepared worker records:

| Boundary | Worker wall/CPU ms |
| --- | ---: |
| Preparation complete | 1,122 / 62 |
| BeforeEvaluate | 1,134 / 62 |
| Global declarations complete | 1,284 / 62 |
| Private scope ready | 1,342 / 62 |
| Event setup complete | 1,716 / 78 |
| Interfaces complete | 1,794 / 93 |
| Element setup complete | 1,794 / 93 |
| Document setup complete | 1,828 / 93 |
| DOM bootstrap returned | 1,845 / 93 |
| Hello completion | 1,864 / 93 |

These are one successful worker's checkpoint observations, not a cold-OS
benchmark or a reliability/performance claim.

## Broad Failure And Copied Host

The one broad scripting run fails after **1,392 passing assertions**, actual
exit one, during isolated timer recovery. The worker had completed bootstrap
and hello: startup wall 2,213 ms / CPU 187 ms. The later page-execution wait
instead times out at `phase=read`, wall 5,003 ms / CPU 0 ms. All six bootstrap
markers and `ready` remain in its stderr. There is no retry or substituted
passing broad run. The planned full total would be 2,270 checks, but it is not
achieved. This failure remains a regression risk against the preceding broad
pass and is not reclassified as a startup success for release purposes.

The separate copied .NET 4.8 Firespider matrix passes **all 43 cases**, including
scripting, after its 14 clipboard and 12 observation contracts. The pinned
harness verifies each child exit zero and one genuine completion line. First
script demand takes 4,058 ms; split-load wait takes 4,655 ms. These aggregate
host waits are not individual worker handshake times or latency guarantees.
The standalone browser also exits zero with its expected isolated mutation.
Four fresh browser/host captures are inspected, including both split views.

Those passes do not clear the broad read timeout, previous intermittent startup
failures, modern-site compatibility gaps, or the original installed-startup
requirement. The original .NET 4.6.1 host is not rebuilt/migrated/installed, and
its six actual-EXE startup scenarios are not newly run. The complete renderer
smoke is not rerun; required renderer bytes match the preceding tested build.

The next investigation should distinguish frame receipt, deferred native
`Frames()` translation, interpreter entry and bridge reply progress during
fresh-realm timer recovery. CPU zero does not prove which side was waiting;
an earlier sampled comparer stack does not explain this completed-bootstrap
failure. Startup/page deadlines and containment must not be loosened to mask it.

## Preservation And Delivery

An initial compile misses the Jint extension-method namespace; its failed log,
source and initial tool are retained. A later test-tool-only build needs its
frozen project-reference DLLs staged at the overridden output path. Both setup
failures are corrected before any test launch. Bounded-tail test assertions
are finalized before native tests, without changing runtime bytes or retrying
a failed runtime gate. The final tool and all tested runtime bytes are frozen.

Readable bootstrap: 34,315 UTF-8 bytes. Generated compact asset: 24,901 bytes,
with a matching parsed structure and SHA256
`A881FC1D9297BFBC7D47FC8F5CFDDA66536712FB465ED588855947EEEE906A67`.
Only Script and native-worker binaries change in the 19-file runtime:

| File | Bytes | SHA256 |
| --- | ---: | --- |
| `Lea.Script.dll` | 97,792 | `D2B4ABE8AEAEFD16D5EA15E882F0BE6F09C67EA1F1EAE1636AEC0A8670C5E969` |
| `ScriptWorker/Lea.Script.Worker.exe` | 49,152 | `2E4666D2F3DC5D47499089DF478C0B49DBFF73CB0716D413AB513641A880903B` |

The required Engine/Win/Image bundle remains 749,056 bytes. The 24-file
runtime-only ZIP compresses 8,271,100 raw bytes to **2,900,319 bytes**; every
entry matches the tested bytes. Compression reduces storage/transfer size,
not proven execution cost. Tests and symbols are excluded. Keep the eight
worker runtime files together and extract to local NTFS for native execution.

Experimental browser folder:
`C:/Users/Elliot/AppData/Local/Lea/EngineBuilds/Lea-146-bootstrap-markers-2026-10-11`.
Builds, failures, logs, snapshots, archives and Git checkpoint are retained under
`H:/Lea Evidence/Lea-146-bootstrap-markers-2026-10-11`.
Normal production outputs, 67 original host inputs, three installed files and
the 379-file host basis are checked unchanged. Source is separately backed up
with every archive entry verified. Default adoption and the active engine goal
remain unfinished.
