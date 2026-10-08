# Firespider Captured Callback Verification

## Current Result

Separate v24 carries the current listener-invocation bootstrap into Firespider.
The candidate passes all 43 host cases and all six actual-EXE startup scenarios.
A fresh runtime ZIP extraction passes 42 non-script cases, then fails its first
JavaScript demand with the existing worker startup deadline. **The extracted
package is not a verified release.** No retry, installed replacement, relaxed
deadline, isolation change, or startup fix is claimed.

| Gate | Result |
| --- | --- |
| Old-worker negative control | Expected callback failure after successful startup |
| Current-worker positive control, identical harness EXE | Complete script case passes |
| Candidate host matrix | All 43 cases pass |
| Candidate actual-EXE startup | All six scenarios pass |
| Fresh runtime extraction host matrix | 42 cases pass; script case fails startup |
| Fresh extraction actual-EXE startup | Not run after the failed matrix |
| Independent archived-source compilation | Passes; all 104 archived inputs unchanged |
| Independent rebuilt runtime tests | Not run; release gate remains held |

Each matrix first passes the 14 deterministic clipboard contract checks. The
candidate's six startup scenarios are clean, restored, locked, oversized,
lazy-background, and lazy-all, including explicit Resume. They verify
responsiveness, checked exception/recovery dialogs, and absence of MSHTML.
The host matrix includes explicit legacy-fallback cases; the native script case
and native-only checks separately assert MSHTML absence.

## Callback Regression

The host script case now installs function, object, and DOM0 callbacks on the
fixture counter. A real keyboard activation checks receivers, event arguments,
one deferred object-operation lookup, callback ordering, DOM0 return-false
cancellation, handler identity, and event-state restoration. Throwing own
`.call` getters and a null object-operation `.call` must not be used. Page code
also replaces public `Function.prototype.call`, `Object.prototype.hasOwnProperty`,
and `Reflect.apply`; captured engine helpers must remain effective.

A disposable control changes only `ScriptWorker/Lea.Script.Worker.exe` to the
preceding v23 worker. All other host/runtime bytes remain identical. It completes
worker startup, then fails the new dispatch assertion with
`host public Function.call reached`. The same exact boot-harness EXE is copied
to the current-worker candidate, where the full script case passes. The negative
result is not a worker-startup timeout and is not counted as a normal host pass.
Both control runtimes and their exact executable hashes are retained.

The complete candidate matrix subsequently recompiles the unchanged harness
source through the normal wrapper and passes all 43 cases. Candidate captures
are visually inspected: a nonblank counter update and independent split values
of 1 and 0. The failed extracted script case never reaches its paint captures;
no candidate image is presented as an extracted-script pass.

## Retained Startup Failure

The fresh extraction's first supplied script is `var retainedState=41;`.
It returns `timeout` with `Worker startup deadline exceeded`:

- phase: `hello`
- launch: 579 ms; probe: 64 ms
- startup wall: 5,013 ms; CPU: 484 ms
- last captured child marker: `startup evaluate`

The worker is retired and the host case exits 1. All 42 earlier cases complete;
their passes do not make the full matrix pass. The last captured marker is not
a stack trace and does not prove the exact operation or cause of the delay.
No per-thread observer is attached to this failed run.

The extraction is on NTFS C: to preserve space on removable NTFS L:, not as a
production relocation or a proposed timeout fix. The failure on C: is consistent
with the preceding volume investigation's lack of demonstrated relocation
benefit. Machine load and caches are uncontrolled; later free-space observations
are not simultaneous failure telemetry. End-to-end functional timings are not
worker-handshake timings or speed benchmarks.

## Integrity And Source

All 67 original host inputs still match v23's original inputs. The candidate's
25 overlays, 90 final source inputs, four repository integration overlays, 19
production dependencies, and exact eight-file worker inventory are audited.
Those 19 dependencies match the bytes used by the preceding 1,261-check full
source pass; no engine SDK rebuild occurs here.

Both archives pass exact manifest, candidate-file, ZIP-stream, and fresh
extraction checks: 375 runtime payload files and 103 host-source payload files,
plus one package manifest each. The source archive builds independently with
the installed .NET Framework compiler using the same explicit net48,
`GenerateManifests=false`, and binding-redirect settings as the candidate build.
Compilation changes none of the 104 archived files and retains all 19 runtime
dependencies. The independent EXE has a different hash; reproducible host
compilation is not claimed. No test fixtures are copied into that output and
no rebuilt-runtime tests are run after the extraction failure.

Two external verification mistakes are retained separately from browser results:
an inventory filter initially expected forward slashes in a backslash baseline;
capture freshness initially reparsed an already-UTC JSON DateTime and lost its
Kind. Normalized paths and the original typed UTC value verify the same existing
files without rerunning host cases. Neither mistake is a production fix.

## Artifacts

- Candidate build/source staging: `H:/Lea Builds/Lea-146-listener-host-v24`
- Native tested candidate: `L:/Lea Builds/Lea-146-listener-host-v24/runtime`
- Negative control: `L:/Lea Builds/Lea-146-listener-host-red-v24`
- Failed fresh extraction: `C:/Users/Elliot/AppData/Local/Lea/HostDiagnostics/Lea-146-listener-host-extracted-v24`
- Archived host source: `H:/Lea Builds/Lea-146-listener-host-source-v24`
- Untested independent output: `C:/Users/Elliot/AppData/Local/Lea/HostDiagnostics/Lea-146-listener-host-rebuilt-v24`
- Held packages: `H:/Lea Builds/Lea-146-listener-host-packages`
- Evidence: `H:/Lea Evidence/Lea-146-listener-host-2026-10-08`

The H: copies are storage/evidence; worker execution uses ACL-capable NTFS C:/L:.
The host-source archive contains the migrated VB project and frozen runtime
dependencies. Complete engine source is separately backed up and kept in Git.

Runtime ZIP `Firespider-Lea-146-script-runtime_2026-10-08_13-22-40.zip`:
3,708,218 bytes, 376 files, SHA-256
`01BCCAC8F47A6993500DB7AFE955F5E6141DBEA65A400AC6C8477F7D571852F3`.

Host-source ZIP `Firespider-Lea-146-script-host-source_2026-10-08_13-22-40.zip`:
3,351,007 bytes, 104 files, SHA-256
`DDB9371333EF195C7422A72CD0B91B156BD7D08960C1064E582DE5F1C535D95C`.

Candidate/extracted EXE, 1,134,080 bytes, SHA-256:
`831E6A3DDCA2135860168A47EA0F50D653F57B96A693F71827AAC54FEEE4EB27`.
Independent rebuilt EXE, 1,134,080 bytes, SHA-256:
`4D301743A76B62E00748BC0CE28AB0B15353FD5CE7BD2F9A0ED13B5B53665167`.

Installed Firespider and the original project remain unchanged. Milestone 146
stays open and broader readiness remains about 65%. Reliable worker startup,
origin/resource policy, legacy-only commands, and real-site compatibility still
gate adoption. The next startup work needs observations from an actual failure;
the preceding successful observer run did not capture this stall.
