# Element Selector Methods

Experimental follow-up to the private-block bootstrap and live class-list work.
Broader readiness remains about 65%, not a standards score. Earlier intermittent
startup failures remain unexplained; new DOM methods do not resolve that issue.

## Compact Host API

| Alias | Result |
| --- | --- |
| `node.Mt(selector)` | Whether this element matches |
| `node.Cl(selector)` | Nearest matching inclusive ancestor, or null |

Both aliases reuse `QueryRules` and the existing shared CSS matcher. Each call
compiles once; closest checks self before walking parents, independently of
selector-list order. It is not implemented as membership in the truncated
256-result query collection. There is no second parser or persistent selector
cache. Host reads use the supplied node, not an active-page lookup; live fields,
attributes and retained detached subtree relationships remain observable.

Null, malformed, unsupported and over-limit host selector strings throw
`FormatException`. Documents/text are not elements and return false/null after
selector validation. Reading a detached node does not make it a permitted
mutation target. Host calls are trusted node inspection, like `N.Av`, and do not
execute scripts or consult the page scripting switch.

## JavaScript API

Wrapped elements expose `matches` and `closest` with one required argument.
Private event-target state holds each element's existing handle; no extra
per-element lookup map is allocated. Captured native WeakMap registration and
lookup protect receiver state from page-replaced map methods. An own literal
handle property avoids inherited setters. Lookup validates the actual receiver,
including handle zero, before argument conversion. Methods
can be borrowed between real wrappers, not forged by inheriting from a wrapper.
Documents, window, primitives and arbitrary objects are rejected as receivers.

Conversion uses the captured DOM string converter: Symbols are TypeErrors,
explicit null/undefined become selector strings, and omitted arguments fail.
Private bridge operations `mt/cl` return boolean/handle results; distinct error
sentinels become branded `SyntaxError` DOMExceptions. Caught selector errors do
not reset the realm. Nearest matches use the existing wrapper cache.

The [DOM matches/closest algorithms](https://dom.spec.whatwg.org/#dom-element-closest)
define inclusive nearest-first ancestor selection and syntax exceptions. Lea
implements these operations only over its existing selector subset, not complete
browser selector conformance. In particular, legacy ID/class/attribute matching
is case-insensitive, unlike the new case-sensitive DOMTokenList. `:scope`, CSS
escapes, namespaces, pseudo-elements, Shadow DOM and complete DOM prototypes
remain unsupported. A valid browser selector outside Lea's subset is rejected
as unsupported; it is not necessarily invalid CSS. Document `Q/Qa` keep their
existing empty-result behavior for unsupported selectors.

## Bounds

- 1,024 raw UTF-16 selector units, sixteen groups and sixteen total chain parts.
- Existing bounded functional/nth/relational selector compilation and matching,
  including the per-`:has` 4,096-candidate visit ceiling.
- Closest inspects at most 128 ancestor candidates, matching the HTML depth cap.
- Script calls retain the existing 256 bridge-call and 512 handle limits.
- Interpreter statement/memory bounds, process deadlines, AppContainer and Job
  protections, permission generations and remote-script refusal are unchanged.

`Mt/Cl` are new read-only aliases, not additions to the literal command language.
The worker protocol and required dependency list do not change; deploy matching
Engine/Script/worker binaries together. These methods neither create nodes nor
introduce network access, attribute writes or navigation permissions.

## Evidence

- `Lea-select-red.log` fails specifically because the element selector methods
  are absent before the implementation, after successful bootstrap.
- `Lea-select-final-focused.log` passes all 55 focused trusted, isolated and native-view
  checks. Cases cover matching, nearest/list order, structural/functional/has
  reuse, siblings/form state, borrowed/forged receivers, conversion, captured
  intrinsics, syntax/size limits, read-only accounting, live attributes, fields,
  detached internal ancestry, mutation refusal, bridge exhaustion and recovery.
- `Lea-select-setter-red.log` and `Lea-select-key-red.log` reproduce page-replaced
  WeakMap registration and an inherited handle setter intercepting wrapper
  creation. Both cases are included in the final focused and complete suites.
- `Lea-select-core-focused.log` passes the new host alias smoke, including the
  last of 300 matching siblings beyond `Qa`'s existing 256-result cap.
- `Lea-select-core-full.log` passes the complete core suite.
- `Lea-select-final-full.log` passes all 895 source scripting checks before the
  later startup-result diagnostic change. The first
  complete run, `Lea-select-full-source.log`, fails the active-page worker setup
  without a typed result. The assertion now retains status and phase diagnostics;
  the subsequent diagnostic and final runs pass. That does not establish a cause
  for the earlier failure or prove stable cold-start readiness.
- Native desktop 940px and narrow 420px captures are inspected. Delegated click
  and input handlers execute, the edited field retains identity/value, and
  native editor bounds stay visible inside the viewport.
- `Lea-select-solution-build.log` compiles with zero errors and eighteen existing
  prototype warnings. The final script and browser builds have no warnings or
  errors.

The required Engine/Win/Image bundle remains 743,936 bytes, unchanged in size
from v14. The optional script worker grows 1,024 bytes to 49,152. These are binary
size measurements, not runtime speed measurements; no dependency is added.

## Startup Follow-Up

The first v15 candidate reproduces a split-pane startup deadline in both workers.
At the failure, the current `Sd` report is empty and a new worker PID exists:
another lifecycle attempt can replace the earlier report before the host samples
it. Startup failures now snapshot the report into the existing bounded `Jr.M`
message before cleanup; a later attempt cannot change that result object.
Execution-failure messages, statuses, deadlines and isolation remain unchanged.

`Lea-select-startup-snapshot-red.log` reproduces the missing result evidence with
real native startup cancellation while the owner does not pump. After the fix,
`Lea-select-startup-snapshot-green.log` passes all five checks, including worker
retirement, no page writes, a bounded launch-phase snapshot and retention after
successful recovery. This fixes lost diagnostic evidence, not the startup
deadline's underlying cause.

The complete rerun after that change, `Lea-select-snapshot-full-source.log`,
passes the updated startup tests and all 55 selector checks, then fails the later
isolated computed-script startup gate: `phase=hello`, 5,040 ms wall,
906 ms child CPU, with `startup evaluate` the last child marker. It reaches
trusted bootstrap evaluation but not the DOM-ready marker. That region includes
both parsing and execution; this observation does not distinguish them or prove
a common cause for every earlier failure. The updated full suite has 897 planned
checks, not a completed 897-check pass.

`Lea-select-snapshot-focused.log` separately passes the 24 trusted checks, then
fails first isolated startup at 5,010 ms wall / 515 ms CPU with the same last
`startup evaluate` marker. A further explicitly observed diagnostic run,
`Lea-select-thread-run.log`, passes all 55 checks. Its 43 samples across three
owned workers are retained in `Lea-select-thread-samples.log`. Observed threads
include ready, running and wait states; without stacks this does not identify a
blocking call or the cause. Sampling can perturb timing and is not a performance
benchmark or a successful normal full-suite gate. No runtime retry or deadline
change converts these failures into passes.

## Host Verification

`dist/Firespider/Lea-146-selectors-v15` compiles separately from the live project.
`Lea-select-host-all.log` passes 27 host cases, including the real recovery timer,
then fails the edit case with Windows clipboard error `0x800401D0`. This is not a
completed 43-case run. The separate `Lea-select-host-script.log` passes the new
host selector assertions but fails the later scripted split-pane load with two
worker startup timeouts; it is not a completed scripting case. Both failed builds
and logs are retained. The diagnostic change is compiled into a new separate
`dist/Firespider/Lea-146-selectors-v16` candidate.

`Lea-select-v16-host-script-trace.log` passes the complete v16 scripting case
with opt-in phase timing, including the new selector assertions, split isolation,
error recovery and remote refusal. The same source parser and budgets are used.
Observed split-load wait is 5,501 ms across the host workflow, not an individual
handshake measurement. This diagnostic pass does not explain the failed normal
source run or the earlier v15 failure. The full 43-case host matrix has not been
repeated on v16.

`Lea-select-v16-exe-startup.log` passes all six actual-EXE startup scenarios with
responsive windows, no detected error dialogs and no MSHTML. Lazy-all starts
deferred and resumes successfully. Readiness observations range 4,935..15,835 ms;
the slow clean/lazy-background launches are not a speed result or startup SLA.

`Lea-select-v16-candidate-audit-fixed.log` verifies all 67 original inputs against
the live source, 90 final overlaid snapshot files and all 19 deployed dependencies
against the current browser build. The initial verifier incorrectly compared
original `app.config` to the deliberately transformed candidate file; its setup
failure is retained separately. Overlay records take precedence in the corrected
audit, matching the builder/packer contract.

The v16 candidate EXE SHA256 is
`CD7DAACB275E4868B81BF15E2EF14C23348FBD087943F4F2C0DCB953AE45FDBD`.

No new release ZIP is published from these incomplete host gates. The earlier
v14 package remains intact; its verification does not apply to these changed
binaries. The installed Firespider host remains untouched.
Complete Lea source is backed up separately from test binaries. No fresh ZIP
extraction or archived host-source rebuild gate is implied for v15/v16. Retained
logs, samples and native captures are copied with a length/SHA256 manifest under
`dist/Bench/Lea-146-element-selectors-evidence`.

## Commands

```powershell
dotnet build Lea.sln -c Release --no-restore /m:1
.\Lea.Tests\bin\Release\Lea.Tests.exe . --select
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe . --select
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe . --startup-stop
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe .
```
