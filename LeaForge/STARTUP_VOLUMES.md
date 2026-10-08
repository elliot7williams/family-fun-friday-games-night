# Startup Volume Diagnostic

This investigation tests runtime location without changing production engine
binaries. Windows reports the development/worker volume `L:` as removable NTFS
(`USB DISK`) and `C:` as fixed NTFS. That observation alone does not identify the
cause of the intermittent five-second worker-startup timeout.

## Method And Results

The opt-in `--startup-volumes` harness copies identical eight-file runtimes
(3,335,225 bytes), verifies every SHA-256, and alternates three pairs of launches.
Each launch uses production `Jp.Ex`, the production parser, timing-enabled
startup markers, a fresh AppContainer profile, and unchanged limits: 5,000-ms
startup, 2,000-ms default execution, 128 MiB, and CPU rate 2500. Successful launches
verify file/network denial, no loopback exemption, all mitigations, first typed
mutation, child exit, and exact ACL restoration. No page script runs before hello.

All six final launches and all 25 diagnostic checks pass, without retries within
that comparison. The two earlier audit-aborted attempts below are separate and
are not erased by this result.

| Pair | Order | L: Startup Wall/CPU (ms) | C: Startup Wall/CPU (ms) | L: Copy (ms) | C: Copy (ms) |
| --- | --- | --- | --- | --- | --- |
| 0 | L, C | 3276 / 281 | 2382 / 140 | 7416 | 42 |
| 1 | C, L | 2479 / 156 | 2539 / 281 | 6163 | 34 |
| 2 | L, C | 2498 / 187 | 4083 / 312 | 4787 | 38 |

Startup medians are 2,498 ms on L: and 2,539 ms on C:, with overlapping ranges.
These samples do not demonstrate a startup improvement from relocating the
worker, or establish that the USB volume causes the earlier timeouts. Copying
the runtime is substantially slower on L: in this run, but that is deployment
I/O, not browser startup, rendering, or navigation performance.

Copy timing excludes subsequent hash/ACL preparation. Startup wall/CPU comes
from the parent's bounded report; the JSON also records call-plus-disposal time.
Copies, validation, and ACL preparation occur before the measured launch.
Fresh paths are not cold OS/CLR caches; machine load, cache state, and differing
directory ACLs are not controlled. Timing instrumentation is enabled. This is
neither a cold-start stability test nor a whole-browser speed benchmark.

## ACL Baseline Correction

The first attempt starts one L: worker successfully, then fails its exact SDDL
audit: Windows adds `D:AI` while retaining identical access entries. A second
attempt tries an unchanged `FileSecurity` write, which is a no-op, and stops on
the same audit. Both incomplete JSON/logs and their runtimes are retained.
Neither reaches C: or completes the six-sample comparison.

The final harness explicitly persists the same DACL on its disposable copies
before taking the baseline. It verifies binary-identical access entries and
permits only the `DiscretionaryAclAutoInherited` bookkeeping flag to change at
this preparation step. After the worker exits, complete SDDL must match that
baseline exactly. No production permission code is changed. All eight L: files
need this normalization; the C: files already have the flag.

This bookkeeping behavior is consistent with Microsoft's description of
[automatic ACL inheritance](https://learn.microsoft.com/en-us/windows/win32/secauthz/automatic-propagation-of-inheritable-aces).

## Safety And Reproduction

The test requires existing roots on different ready local volumes, rejects
reparse-point paths, and refuses to overwrite a report. Free-space checks retain
64 MiB on the system volume or 32 MiB elsewhere, plus room for the copied files.
Successful runs delete only their eight known files and empty unique directory;
failed or audit-aborted runtimes remain available. Production files, installed
Firespider, and unrelated temporary data are not modified or cleaned.

```powershell
$env:TEMP = 'L:\Lea Tool Cache\temp'
$env:TMP = $env:TEMP
& '.\Lea.Script.Tests.exe' 'L:\Lea Development' --startup-volumes `
  'C:\Users\Elliot\AppData\Local\Lea\StartupDiagnostics' `
  'H:\new-startup-volume-report.json'
```

Four final-executable guard cases pass: missing arguments, same volume, missing
root, and existing report. The existing 90-check listener regression passes
through the new test executable too. Release builds have zero warnings/errors.
The framework JSON serializer reference is test-only; all 19 production runtime
dependency hashes remain identical to the listener-invocation checkpoint.

Evidence is under `H:/Lea Evidence/Lea-146-startup-volumes-2026-10-08`. The final
comparison is `persisted-baseline-volumes.json`; earlier `volumes.json` and
`normalized-volumes.json` describe the two incomplete attempts. A post-comparison
machine snapshot is not treated as simultaneous failure telemetry.

The normal full suite is not rerun for this diagnostic-only change; its last two
startup failures remain unpassed. No new release, host candidate, default runtime
relocation, deadline change, or performance fix is claimed. Milestone 146 stays
open and broader readiness stays about 65%. The next investigation needs evidence
captured during an actual stall, not an assumption that a volume move solves it.
