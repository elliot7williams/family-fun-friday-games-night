# Firespider Tree Navigation Follow-Up

The separate v19 candidate contains the live element-tree implementation from
`adf4172`. The installed Firespider project and EXE are unchanged. This is an
experimental build, not a newly verified release.

## Host Coverage

`Lea.Firespider/Boot.cs` now checks all six element navigation properties plus
document child endpoints/count inside the actual net48 host. It verifies live
wrapper identity and that navigation reads do not rebuild or mutate the page.
The existing script case still covers native events, timers, error recovery,
separate split-pane workers, disposal, and remote-origin refusal.

The first added assertion incorrectly expected the body's last element to be
the visible `inline` div. The fixture has script elements after that div, so
the DOM endpoint must be a `SCRIPT`, irrespective of rendering. The corrected
assertion reports separate parent, sibling, endpoint, and document errors.
The focused tree suite also adds one title/style/script inclusion check for
each transport. Production engine/worker code and dependencies are unchanged
by this follow-up; no deadline or isolation protection is relaxed. A later SDK
test rebuild records the newly committed revision in assembly metadata, as
described below; its binaries are not silently substituted into v19.

## Candidate And Verification

Candidate: `L:/Lea Builds/Lea-146-tree-v19/runtime`.

EXE: `Firespider Internet 19.exe`, 1,134,080 bytes, SHA256:
`152288BBF48016903F8DF39F0C46F052EEE53C25BF2D9C39BA9697D52475F6CC`.

Root logs in `L:/Lea Development` use the `Lea-tree-host-` prefix:

- `candidate-build.log`: separate net48 host compilation succeeds.
- `candidate-audit.log`: all 67 original inputs, 25 overlays, four current
  integration sources, and 19 runtime dependencies match. The worker has
  exactly eight files. The pre-test runtime inventory is retained separately.
- `exe-startup.log`: all six actual-EXE scenarios pass: clean, restored,
  locked, oversized, lazy-background, and lazy-all. They remain functional
  checks, not startup-performance guarantees.
- `all.log`: all 42 non-optional-script host cases pass, including both real
  30-second timers, settings restoration, editing, clipboard recovery, split
  panes, policy, and image-worker cleanup. The final script case fails the
  incorrect new endpoint assertion after successful worker startup. This is
  not a 43-case aggregate pass.
- `script-corrected.log`: the corrected script case passes separately,
  including the tree reads, split isolation, cleanup, and origin gate. First
  demand takes 4,977 ms and split loading takes 5,961 ms as observed by the
  harness; these are not controlled benchmarks or deadline changes. The
  original 42 results and this separate pass do not constitute a new complete
  aggregate run of the corrected harness.
- `tests-build.log`: updated source tests compile with zero warnings/errors.
- `build-revisions.log`: direct PE metadata inspection records `060f229` in
  v19/standalone SDK assemblies built before the tree source commit and
  `adf4172` in the later test rebuild. Both builds include the same production
  source implementation, but their Script, Script.Win, and worker hashes
  differ. The v19 host results and later source-test results remain scoped to
  their own frozen binaries; no byte-identical runtime claim is made between
  these two builds. The earlier candidate audit matches all 19 dependencies
  to its standalone build.
- `focused.log`: the new non-rendered-element assertion passes in trusted
  and isolated modes, but later isolated recovery times out at 5,028 ms wall /
  875 ms CPU. Its final marker is `startup probes`; no 55-check focused pass
  is claimed.
- `full-source.log`: a distinct normal full run completes all 55 tree checks
  (24 per transport and seven native checks), as well as the 28 private-scope
  checks, but later fails isolated class-list startup at 5,025 ms wall /
  1,156 ms CPU, with `startup parsed` last. The planned full count is 1,023;
  this is not a full-suite pass. The earlier 1,021-check pass remains historical
  evidence for the preceding source checkpoint, not the expanded test suite.

Free C: space is 905,711,616 bytes at candidate audit and 888,098,816 bytes
before the full source run, with 869,208,064 bytes observed afterward. The
startup problem therefore remains observed after storage recovery; this does
not identify its cause. No user-file cleanup or disk-filling experiment is used.

## Remaining Work

The timeout after `startup probes` lies between completed denial probes and
the parent receiving the hello frame. The code path includes serialization,
pipe writing, and parent scheduling/reading; the marker alone does not prove
which operation consumed time. The later failure stops earlier in bootstrap,
so a single shared cause is still unproven.

Cold-start reliability, a fresh complete 43-case corrected-host run, release
packaging, extracted-runtime checks, and archived-host-source rebuild checks
remain open. No release ZIP or installed-browser replacement is made.
Broader lightweight-browser readiness remains about 65%, a practical estimate
and not a web-platform conformance score.

## Commands

Run the wrappers with Windows PowerShell `-STA` and set `TEMP`/`TMP` to the
NTFS tool-cache folder first:

```powershell
.\Lea.Build\Test-FirespiderStartup.ps1 -r 'L:\Lea Builds\Lea-146-tree-v19\runtime' -s all
.\Lea.Build\Test-FirespiderBoot.ps1 -r 'L:\Lea Builds\Lea-146-tree-v19\runtime' -c all
.\Lea.Build\Test-FirespiderBoot.ps1 -r 'L:\Lea Builds\Lea-146-tree-v19\runtime' -c script
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe . --tree
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe .
```
