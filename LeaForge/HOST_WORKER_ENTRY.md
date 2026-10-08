# Firespider Worker Entry Candidate

This records the initial v23 attempt at checkpoint `c62e63e`. Subsequent
candidate, extracted-package, and archived-source gates are recorded separately
in [HOST_WORKER_VERIFICATION.md](HOST_WORKER_VERIFICATION.md); the failure below
is retained unchanged.

Separate v23 builds the latest guarded-entry worker into the copied .NET
Framework 4.8 Firespider project. The build and input audits pass, but its host
matrix is incomplete after a disk-full failure. It is not an installed
replacement, a verified release, or a fix for the intermittent startup timeout.

## Build And Input Audit

- The candidate compiles from 67 original host inputs and 25 overlays, giving
  90 final source files. Original inputs and final copies pass length/hash checks.
- All four integration overlays match the current repository. All 19 runtime
  dependencies match the tested standalone build in both source and runtime
  folders, including an exact eight-file worker inventory.
- The engine SDK binaries are not rebuilt. Only the worker differs from the
  preceding v22 runtime dependencies; aliases, versions, isolation, and limits
  are unchanged.
- The host EXE is 1,134,080 bytes, SHA-256
  `CF8E122D18AE28653A2B78DB915B58E46672EC47EEA30130DFC63F9408555F59`.
- Worker SHA-256 remains
  `74B277AACB131AB476F8B5B301AD520778C7F1CE892A51AED8C1BC3EF0CF5EE0`;
  parent SHA-256 remains
  `A19FEB7E4CF4B5E42F4D85BCF2E207A8090208ED029E81A48F6FB0B4BD2F3ABD`.

## Retained Failure

The 14 deterministic clipboard contract checks pass. Seven host cases complete:
`clean`, `lea`, `legacy`, `recovery`, `private`, `second`, and `limit`.
Recovery includes its real 30-second snapshot timer.

The next case, `invalid`, prints its assertion PASS marker but then exits with
status 1 while restoring test settings. The complete matrix also exits 1:

```text
ConfigurationErrorsException: Failed to save settings
IOException: There is not enough space on the disk.
LocalFileSettingsProvider.SetPropertyValues
ApplicationSettingsBase.Save
```

The affected file is the test harness's `user.config` under `%LOCALAPPDATA%`
on C:. Volume inspection immediately after failure reports zero free bytes;
a later snapshot reports about 49 MiB. Space is fluctuating and remains low.
The `invalid` case is not counted as a pass despite its earlier marker.
The settings cleanup is not suppressed, retried, redirected, or weakened.

The script host case is last and is not reached. No six-scenario actual-EXE
startup run, runtime/source ZIP publication, extracted-package test, or archived
source rebuild is verified for v23. The earlier standalone 1,171-check results
remain scoped to that build; they do not replace these missing host gates.

This observed disk-full test failure does not explain the earlier five-second
native worker startup timeouts. No causal fix, cold-start stability, or general
speedup is claimed.

## Preservation And Next Gates

Candidate: `L:/Lea Builds/Lea-146-worker-entry-v23`.
Evidence: `H:/Lea Evidence/Lea-146-worker-entry-host-2026-10-08`.
Evidence includes the failed log, build log, input audits, frozen 91-file host
source snapshot with its manifest, 375 runtime payload files, and the harness.
H: is distribution/evidence storage, not a native worker launch location.

Installed Firespider EXE and core DLL hashes remain unchanged. No owned browser,
script-worker, or image-worker test process remains after the failed run.

After stable disk space is available, repeat the complete host matrix and all
six actual-EXE startup scenarios without erasing this failure. Passing gates
must precede runtime/source packaging, stream/extraction audits, extracted host
tests, and an independent archived-source build. Reliable native worker startup,
remaining legacy-only commands, origin/resource policy, and real-site
compatibility remain required before adoption.

Milestone 146 stays open and broader readiness stays about 65%, a practical
project estimate rather than a standards conformance score.
