# Captured Attribute Name Validation

Lea's bounded `setAttribute` now validates names using captured string
operations. No public engine alias or dependency is added. This is a specific
DOM robustness fix, not a full-web compatibility, startup or speed claim.
Milestone 146 stays open; broader readiness remains about 65%.

## Reproduced Failure

The preceding production bootstrap calls a regular expression's public `test`
method to reject forbidden name characters. A page replacing
`RegExp.prototype.test` can therefore break an otherwise valid attribute write.
The same new regression test against the exact preceding runtime fails with
`poisoned attribute-name intrinsic`. That negative control and runtime are
preserved separately from the successful fix.

## Implementation

The validator captures `String.prototype.indexOf` beside the existing captured
`slice` operation. It checks the nine forbidden ASCII separators through those
private captures and the existing captured `Reflect.apply`. Page replacement
of regex `test`/`exec` or string `indexOf`/`slice` cannot intercept this check.
The bootstrap's only regex literal is removed; JavaScript supplied by a page
still uses the unchanged interpreter and regex policy.

Validation remains after receiver/argument checks and name-then-value DOM
string conversion. Empty names or forbidden separators raise the existing
`InvalidCharacterError` DOMException; syntax rejection precedes the 64-unit
name bound, even when an oversized name has a forbidden final character.
Accepted names/values, mutation accounting, ownership, permission admission,
native aliases and parent-side validation are unchanged. Removal still permits
a missing name without set-name syntax validation.

There is a fixed nine-iteration scan rather than a JavaScript loop over an
untrusted name's full length. Each captured native search may inspect the
input string; this is not a claim of constant-time validation. Existing call,
statement, allocation, startup, memory and native isolation limits stay intact.
This covers the named validation intrinsics, not every mutable DOM intrinsic.

## Verification

Evidence is retained under
`H:\Lea Evidence\Lea-146-attribute-name-captures-2026-10-09`.

- Four targeted Release builds pass with zero warnings/errors, using the
  existing pinned packages and unchanged Engine/Win/image/Script.Win binaries.
- All 330 focused attribute-edit checks pass in the model, trusted realm,
  isolated native worker and WinForms view. Eight added checks exercise the
  poisoned prototypes, ten invalid-name cases, conversion/atomicity and
  100,000-unit names with syntax-before-bound precedence.
- All 28 private-scope checks pass, including the two new capture names.
  Four compiled-resource checks confirm the generated bootstrap is embedded
  identically in the trusted and isolated hosts.
- Generation, fixed-point and AST comparison checks pass. Readable bytes grow
  from 32,304 to 32,591; compact bytes grow from 23,386 to 23,596. The latter is
  27.6% smaller than its readable equivalent, not smaller than the old version.
  Script DLL and worker PE each grow 512 bytes; the 745,472-byte required
  Engine/Win/image bundle is unchanged. No performance improvement is claimed.
- The full ordinary scripting suite passes all 1,721 checks in one attempt,
  with timing tracing off outside the suite's own diagnostic cases and an
  actual zero exit code. It includes genuine isolated selector-limit recovery.
  This pass does not erase the preceding 5,017-ms failure or prove that its
  intermittent startup cause has been fixed.
- The rebuilt standalone browser EXE passes one explicit-handle lifecycle
  check: inline startup, native counter activation, isolated worker, no
  loopback exemption, 128 MiB admission and successful process exit. Its fresh
  980-by-680 capture is inspected; the Lea icon, counter and fields render.
  The browser's 78 observed modules include no MSHTML/WebView2/CEF renderer.
- All 19 production files match in the test SDK, browser SDK and frozen
  evidence. Only the Script DLL and worker differ from the preceding runtime.
  The unchanged native core's preceding passing `--core` receipt is retained
  and its five runtime files still match; that suite is not rerun here.
  No new package or independent Firespider release gate is claimed.

## Startup Diagnostic

The test-only `-ss` / `--selector-startup` command runs the existing isolated
selector suite with per-call reports and opt-in phase timing. It includes bridge
budget exhaustion and genuine fresh-process recovery, without trusted warmup,
retry, worker reuse or production-limit changes. Reports are create-new and
retained on failure. This command is not a full scripting gate.

Four predeclared samples alternate previous/current/current/previous workers
with the same final test executable and parent runtime. Each uses a unique C:
NTFS worker directory with eight hash-verified files and unchanged 5,000-ms
startup, 128 MiB memory and 25% CPU admission. All four pass 24 checks; all eight
launches retain guard mask 15, AppContainer, denied network/files and no loopback
exemption. The worker is retired on the bridge limit before replacement.

| Sample | Version | Initial ready wall ms | Recovery ready wall ms |
| --- | --- | ---: | ---: |
| 0 | Previous | 3750 | 2971 |
| 1 | Current | 3285 | 2231 |
| 2 | Current | 3064 | 2377 |
| 3 | Previous | 3147 | 2266 |

These are instrumented observations, not controlled cold-cache benchmarks.
Ranges overlap and both versions pass. They do not establish a startup fix,
speedup or reliable memory reduction. The prior full scripting timeout at
5,017ms and earlier host failures remain preserved in their original evidence.

```powershell
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe . -ae
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe . --scope
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe . -ss 'H:\new-selector-report.json'
```

The live Firespider checkout and installed browser are not rebuilt or replaced.
A standalone source test does not constitute a new verified Firespider release.
