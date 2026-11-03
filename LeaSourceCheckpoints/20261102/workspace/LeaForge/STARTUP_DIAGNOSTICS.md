# Startup Diagnostics And Scheduling

Milestone 146 follow-up, still experimental. Broader readiness remains about
65%; the intermittent startup deadline is not declared fixed.

## Changes

- `Jp.Sd` retains at most 512 characters of native-startup diagnostics, including
  phase (`launch`, `probe`, `hello`, `ready`), launch-stage ms, parent positive
  control ms, startup wall ms, and worker CPU ms. Warm calls preserve it. It is
  empty if runtime-file validation fails before native launch starts. CPU reads
  are diagnostic only; a failed read currently reports zero, not an assertion
  that the child used no CPU.
  Asynchronous launch-stage timing includes background dispatch and owner
  resumption; it is not a pure native CreateProcess measurement.
- `LEA_SCRIPT_STARTUP_TRACE=1` enables bounded stderr checkpoints in the trusted
  worker. The wire protocol is unchanged (version 2), and no trace API enters
  the page realm. Worker wall time starts inside Main, so it excludes the CLR
  work that occurs before Main enters.
- The worker begins its loopback-denial connection before VM/DOM initialization
  and checks the connection result afterward. Only trusted, embedded bootstrap
  overlaps the probe. The remaining wait is bounded by the original 500 ms
  observation window, and an already-completed successful connection is checked
  even after that window. Invalid ports and unrelated socket failures do not
  count as denial.
- The complete hello, including canary denial, remains mandatory. Parent-side
  token/ASLR/Job read-back, no-loopback-exemption checks, a working positive
  control, and no pending listener connection remain mandatory before page
  frames are sent. There is no early ready message, retry, CPU burst, or new
  fallback.
- `ExA`/`EvA` run native launch on a dedicated background thread. File leases,
  profile ownership, and the running flag remain owned until it returns. Stop
  during launch does not abandon a process-creation task; the launch thread
  publishes the endpoint under Stop's lock and closes its Job for a pending
  Stop without needing an owner continuation. The owner then checks
  cancellation/permission/page changes and performs session cleanup before
  any page code. Dispose still requires awaiting
  the active task. Native OS launch itself is not preemptible and can extend
  the total call duration; the five-second clock still starts before launch.
- Shared `Rp` grants and exact-SID cleanup now coordinate file ACL
  read/modify/write with a path-keyed `Global\\Lea.FileAcl.<SHA256>` mutex. Waits
  are bounded to 1,000 ms, abandoned ownership is acquired and re-read, and
  object-name conflicts become typed I/O failures. The default object security
  is retained; Lea grants no extra mutex rights to worker SIDs or broad
  application groups.
  This coordinates Lea launchers using the same normalized path across
  processes/sessions; it does not serialize unrelated filesystem modifiers or
  aliases/hard links to that path. Hostile hosts/runtime modification remain
  outside the boundary.

The startup budget remains 5,000 ms, default execution 2,000 ms (25..5,000),
native memory 128 MiB, CPU hard-cap rate 2,500/10,000, and one child per Job.
Core rendering and the image-worker EXE and Job implementation are unchanged.
The shared launcher's file-permission updates change, so core/image regressions
are required too. Engine + Win + image binary bytes are 419,328 + 312,320 + 9,216
= 740,864 bytes, 1,024 more than the prior required bundle; no interpreter
dependency is added to it.

## Evidence

Diagnostic-only candidate v5, before overlap/background launch:

| Observation | Launch | Startup Wall | CPU | Result |
| --- | ---: | ---: | ---: | --- |
| Candidate first demand | 406 ms | 4,047 ms | 281 ms | Pass |
| New NTFS runtime path first demand | 413 ms | 4,807 ms | 593 ms | Pass |

The fresh-path trace reports VM construction at 1,210 ms and completed DOM
bootstrap at 3,661 ms (worker-relative), after approximately 589 ms in the
sequential network probe. These observations identify the bootstrap region as
the largest measured region; they do not identify a specific OS/AV/JIT cause.

Before background launch, a UI test observed a 496 ms maximum tick gap. The
normal full follow-up run observes 19..43 ms gaps across its five startup cases,
with ticks occurring while the worker PID is unpublished. Early permission
revocation and Stop both return `off`/`cancel` at phase `launch`, with no page
writes. These are observations on this machine, not a controlled benchmark or
an end-to-end UI latency guarantee. Validation, callbacks, and teardown remain
owner-thread work.

Verification chronology (logs in `%TEMP%`):

1. `Lea-146-startup-overlap-build.log`: solution builds, zero warnings/errors.
   Initial overlap-only `--startup` passes 31 checks. Its UI run still observes
   the synchronous launch gap, prompting the background-launch change.
2. `Lea-146-startup-overlap-async-build.log`: solution builds, zero warnings/errors.
   Additional early-launch cancellation/revocation cases bring startup coverage
   to 37 checks. The focused test project also builds with zero warnings/errors.
3. `Lea-146-startup-overlap-full-tests.log`: first full follow-up run fails at
   `isolated computed script: timeout; Worker startup deadline exceeded.` after
   the startup cases. The assertion did not yet include `Sd`; its precise phase
   breakdown is therefore unavailable. The failure is retained, not erased by
   later passes. The first-worker assertion now includes `Sd` on failure.
4. `Lea-146-startup-overlap-worker-trace.log`: focused worker suite passes all
   84 checks with diagnostic output.
5. `Lea-146-startup-overlap-full-trace.log`: all 423 checks pass with tracing.
6. `Lea-146-startup-overlap-full-normal.log`: all 423 checks pass without tracing
   globally; the startup suite intentionally enables it for one copied-runtime
   case. That suite tests three distinct first-launch runtime paths, successful
   connections after 0/550 ms overlap, invalid probe ports, bounded/persistent
   reports, exact ACL/process retirement, UI execution, revocation, cancellation,
   and revocation/Stop while native launch is still unpublished.

Three new-path worker observations in the normal run: 2,693 / 2,427 / 2,303 ms
startup wall time. This is not a cold disk-cache comparison. Fresh Firespider
package paths and actual-EXE startup checks are separate release gates; earlier
v4 package failure and the follow-up full-suite failure remain relevant.

Further safety verification:

7. `Lea-146-startup-no-pump-red.log`: a regression deliberately stops UI pumping
   during native launch. It reproduces the handoff flaw (`pid=0` before the owner
   continuation): Stop could not retire the new child until UI work resumed.
8. `Lea-146-startup-handoff-build.log` and `Lea-146-startup-handoff-green.log`:
   atomic endpoint/Job publication fixes the regression. All 40 startup checks
   pass, including killing the child without a UI continuation, completing with
   no page writes, and fresh-worker recovery.
9. `Lea-146-startup-final-build.log`: solution builds with zero warnings/errors.
   Twenty-four permission checks exercise eight simultaneous grants, mixed
   removal/addition, preservation of other SIDs, and restoration of all unrelated
   rules across eight rounds. These are thread-concurrency tests, not a complete
   cross-session/hostile-object security audit.
10. `Lea-146-startup-final-script-tests.log`: all 450 checks pass, including 64
    startup/permission checks. Final copied-runtime observations are 3,230 /
    4,354 / 3,073 ms; normal UI startup gaps are 28..41 ms. Timing variation and
    the earlier deadline failure prevent a stability/benchmark claim.
11. `Lea-146-startup-final-core-tests.log`: full `--core` suite passes, including
    image bounds, AppContainer probes/token checks and worker Job cleanup.
12. `Lea-146-startup-final-candidate-build.log` and `Lea-146-startup-final-boot.log`:
    v7 builds and all 43 host cases pass, including concurrent script split
    panes and image isolation. First-demand total is 4,910 ms and split-load
    wait 5,423 ms, both including work beyond a single startup handshake.
13. `Lea-146-startup-final-startup.log`: all six actual-EXE scenarios pass:
    clean/restored/locked/oversized/lazy-background/lazy-all, responsive and no
    legacy module. The lazy-all selected page starts deferred and resumes.
    Observed readiness is 4,474..6,578 ms, not a controlled benchmark.
14. `Lea-146-startup-final-small-build.log` and `Lea-146-startup-final-small-boot.log`:
    the script-free variant builds and passes clean/split/decode/hibernate/
    lazy-all. Its full matrix is not repeated.

The preserved v6 runtime/source archives were audited completely and its first
fresh-package host scripting case passes, but v6 predates the atomic Stop
handoff and permission coordination. The current candidate is v7; later package
and host checks must use that version. Earlier artifacts are retained for
diagnosis, not recommended installed replacements.

Package and independent-source verification:

15. The five final v7/small/native archives pass complete normalized-path,
    inventory, byte-length and SHA256 audits (370 / 104 / 341 / 76 / 40 files).
    Small-host PE references and inventory contain no Lea.Script/Jint/Acornima
    dependency. This is a dependency check, not a website compatibility claim.
16. `Lea-146-startup-final-package-a-script.log`: the freshly extracted v7
    Firespider runtime passes its scripting case. First demand totals 3,784 ms
    and split load wait 4,313 ms.
17. `Lea-146-startup-final-package-script-tests.log`: the first extracted
    standalone full suite passes all 64 startup/permission cases and the first
    computed worker execution, but fails `recovery after cancellation`. That
    assertion only reported a boolean; the typed result and startup phase were
    not captured. Its cause is unknown, not a proven startup deadline. The
    failure is retained and prevents treating this milestone as finished.
18. `Lea-146-startup-recovery-diagnostic-build.log`: the test project builds with
    zero warnings/errors after adding result status/message and `Sd` to the
    recovery assertion. `Lea-146-recovery-diagnostic-fresh-worker.log`: a fresh
    copied runtime passes all 84 worker checks with tracing, including recovery
    after cancellation (`ok`, startup wall 2,232 ms). No production budget or
    runtime change follows the failure.
19. `Lea-146-startup-diagnostic-package.log`: a new standalone ZIP retains that
    assertion. All 40 files pass inventory/length/SHA256 and safe-path audits.
    `Lea-146-startup-diagnostic-package-full.log`: its fresh NTFS extraction
    passes all 450 checks without globally enabling trace. The startup suite
    deliberately traces one copied worker, as before. First copied-worker
    startup walls are 2,951 / 2,470 / 2,392 ms; cancellation recovery reports
    `ok`, phase ready, 2,106 ms. These passes do not diagnose the earlier failure
    or establish reproducible cold-start latency.
20. `Lea-146-startup-final-host-source-rebuild.log`: the audited 104-file v7
    host-source ZIP rebuilds independently with the saved net48/optional-script
    project settings and installed Framework MSBuild, without a restore.
    Rebuilt EXE SHA256 is
    `67083604A82C58F83BCC92CA1EEC8AA87070E337F218B46D5E10B285F93E62AA`.
    It is not claimed bit-identical to the candidate EXE.
21. `Lea-146-startup-final-host-source-script.log` and its `-diagnostic.log`
    follow-up fail navigation because this build output has no harness HTML
    fixtures. The diagnostic run confirms the old worker exited, no new result
    was produced, and the fixture file is absent. This is a test-setup failure,
    distinct from the unresolved standalone recovery failure. The Boot harness
    now preflights this file and reports bounded title/PID/result/startup state
    on navigation, supplied-code and split-load failures, without weakening its
    assertions or extending deadlines.
22. `Lea-146-startup-final-host-source-missing-fixture.log`: the new preflight
    reports the missing fixture before first script demand. Fixtures copied
    from the v7 runtime extraction are rechecked against its manifest.
    `Lea-146-startup-final-host-source-script-fixtures.log`: the independent
    rebuild then passes the scripting host case, including inline navigation,
    native events, recovery, split isolation/disposal, tab retirement and
    remote-origin refusal. First demand is 3,877 ms; split wait 4,192 ms. Its
    complete 43-case host/startup matrix is not repeated for this rebuild.

## Preserved Packages

All paths below are ZIPs on `F:\`. Extract runtime packages to a new local
NTFS/ACL-capable directory before launching them. The 22-03-21 standalone ZIP
adds test diagnostics, not a production-runtime fix for the intermittent failure.

| ZIP | Bytes | Files |
| --- | ---: | ---: |
| `Firespider-Lea-146-script-runtime_2026-10-03_21-46-30.zip` | 3,622,855 | 370 |
| `Firespider-Lea-146-script-host-source_2026-10-03_21-46-30.zip` | 3,284,229 | 104 |
| `Firespider-Lea-146-small-runtime_2026-10-03_21-47-25.zip` | 1,098,481 | 341 |
| `Firespider-Lea-146-small-host-source_2026-10-03_21-47-25.zip` | 768,409 | 76 |
| `Lea-js-146-native_2026-10-03_21-47-54.zip` | 2,902,703 | 40 |
| `Lea-js-146-native_2026-10-03_22-03-21.zip` | 2,960,411 | 40 |

SHA256, in table order:

```text
2F217179717F83401AD6028308963EADCE092300D7A35AD9C8D7C1EC44BB4BCE
28922FB25C7D5155924F52428052AA25B9D78DD2FEC16F391F5B095906994C99
A049C197243C6EB69A698BD042AC63CFEFF9F47D5615F23B9D74C215CAE35B87
A6EE658897C46D89B8F72B5375DE26D465F321B3DEA3D2FD3D436217105A3CD1
34E7024E47E83A45C52D029BE3C1FC674780B36016A0C7C8CAB05702FFC15FC6
25177E8E5197ACCE8649F9E77B08AA27BE9AC740894A848DE0352B879F02CDCB
```

The separate candidate EXE is
`dist\Firespider\Lea-146-startup-final-v7\runtime\Firespider Internet 19.exe`,
SHA256 `0431133F6E6890095EDF2114195CB60D38D4CBBE2615CDAF4A84B4848A38F7FD`.
The installed `L:\Firespider Internet` binaries/source are not replaced.
Host-source ZIPs contain compiled Lea dependencies; complete Lea C# sources
must also be retained with `Backup-Lea.ps1`, alongside the live-host source
snapshot from `Backup-Firespider.ps1`.

## Commands

```powershell
dotnet build Lea.sln -c Release --no-restore
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe . --startup
$env:LEA_SCRIPT_STARTUP_TRACE = '1'
powershell.exe -NoProfile -ExecutionPolicy Bypass -STA -File .\Lea.Build\Test-FirespiderBoot.ps1 -r <separate-runtime> -c script
Remove-Item Env:\LEA_SCRIPT_STARTUP_TRACE
.\Lea.Build\Backup-Lea.ps1 -o 'F:\Lea Source Backups'
.\Lea.Build\Backup-Firespider.ps1 -s 'L:\Firespider Internet' -o 'F:\Firespider Source Backups'
```

See [microtask chronology](MICROTASKS.md), [worker boundaries](../Lea.Script/README.md),
and [Microsoft's CPU Job rate semantics](https://learn.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-jobobject_cpu_rate_control_information)
and [kernel object namespaces](https://learn.microsoft.com/en-us/windows/win32/termserv/kernel-object-namespaces).
