# Private Block Bootstrap

Experimental follow-up to [live class lists](CLASS_LISTS.md). Broader browser
readiness remains about 65%, not a conformance score. Earlier intermittent
startup failures are not explained by a successful test run.

## Implementation

The shared `Dom.js` bootstrap uses one strict lexical block instead of an
immediately invoked outer function. It captures `__lea` and `__jobs` in private
constants before deleting their global properties. The final function expression
is the block's completion value, consumed as the dispatcher by both existing
hosts. DOM, event and timer closures retain the private environment after setup.
There is no new alias, dependency, worker protocol, permission or timeout.

[ECMAScript block evaluation](https://tc39.es/ecma262/multipage/ecmascript-language-statements-and-declarations.html#sec-block-runtime-semantics-evaluation)
creates a declarative environment, restores the enclosing environment on exit,
and propagates the statement-list completion value. Strict mode prevents legacy
block-function global aliases. The pinned
[Jint block implementation](https://github.com/sebastienros/jint/blob/v4.16.4/Jint/Runtime/Interpreter/Statements/JintBlockStatement.cs)
restricts slot/environment reuse to small blocks without function declarations
or escaping closures; this bootstrap does not qualify. Runtime regressions,
rather than that implementation inspection alone, verify retained captures.

## Scope Checks

`BootstrapScopes.cs` adds 28 checks across trusted and isolated execution:

- Private host callbacks and representative helpers cannot be reached through
  global properties, direct eval, Function or a public function's constructor.
- Node identity, token-list identity and typed writes survive separate calls.
- Page lexical declarations and fake global host callbacks do not replace the
  private captures used by the bridge and microtask checkpoint.
- String timers observe page-global lexical declarations, not private helpers.
- A budget failure discards page collisions and initializes a connected fresh
  realm without exposing the replacement host callbacks.

These tests cover the named boundaries, not a proof against every interpreter
or operating-system vulnerability. They use the existing supported 5,000-ms
functional call budget. Production defaults and isolation remain unchanged.

## Alternating Launch Comparison

The baseline is the committed `f415f734` bootstrap built and copied before the
block change. Both variants use the same build revision and exact matching
dependency inventories/hashes. Baseline worker SHA256:
`BFF04BFC06125562B0ED463C4647993B87274ED4F09685EA82A36E5607731DAB`.
Compared block worker SHA256:
`A6DBFEDF5C909968073B24BDF3A8349E500C30687697A2812BAB086ECC53B479`.

Three pairs alternate baseline/current, current/baseline, baseline/current. Each
launch uses a fresh NTFS runtime path, normal production parsing, tracing off,
the existing 5,000-ms handshake and default 2,000-ms call budget. A failed sample
is retained and reported while later samples still run; the aggregate gate fails
if any launch fails. No automatic retries convert failures into passes.

| Pair | Baseline wall ms | Block wall ms | Baseline Job peak bytes | Block Job peak bytes |
| --- | ---: | ---: | ---: | ---: |
| 0 | 2854 | 2391 | 32206848 | 32059392 |
| 1 | 2459 | 2976 | 32354304 | 32251904 |
| 2 | 2571 | 2587 | 32264192 | 32124928 |

All six launches pass, with unchanged isolation/defaults and verified worker
exit/exact-SID ACL retirement: 27 checks. Wall-time ranges overlap; the block
median is 16 ms slower. The native peak median is 139,264 bytes lower, too small
and too few samples to establish a memory improvement. Fresh paths do not flush
OS/CLR caches. This is not a cold-start reliability or speed claim.

Separate in-process Start probes observe 1,920 ms before and 1,674 ms after.
Those single samples include construction, parsing and setup, not just the
execution-time window, and do not establish a speed improvement either.

## Regression Evidence

Root `Lea-block-*.log` files retain the observations; an audited evidence copy
is kept under `dist/Bench/Lea-146-block-bootstrap-evidence`.

- `Lea-block-scope.log`: all 28 private-scope checks pass.
- `Lea-block-comparison.log`: all 27 alternating-launch checks pass.
- `Lea-block-startup.log`: all 70 focused startup checks pass.
- `Lea-block-first-classes.log`: all 99 normal class-list checks pass, without
  the warm diagnostic bypass. Desktop 940px and narrow 420px captures are
  inspected; the edited field remains visible and fits its bounds.
- `Lea-block-full-source.log`: the normal full suite passes all 840 checks,
  including the new scope tests, class lists, formatter behavior, events,
  microtasks, timers, native queues and remote-script refusal.
- `Lea-block-solution-build.log`: zero errors and 18 existing prototype warnings.
  Focused test builds have no warnings or errors.
- `Lea-block-core-full.log`: the complete core suite passes.
- `Lea-block-v14-host-all.log`: all 43 host cases pass, including the real
  recovery/hibernation timers, image worker and optional scripting case.
- `Lea-block-v14-exe-startup.log`: all six actual-EXE scenarios pass with
  responsive windows and no MSHTML. Lazy-all starts deferred and resumes.
  Observed readiness is 4,410..6,827 ms, not a latency SLA.
- `Lea-block-v14-extracted-script.log`: all 840 checks pass from the fresh
  standalone ZIP extraction, using its own fixtures and runtime files.
- `Lea-block-v14-extracted-host-script.log`: the extracted host runtime passes
  its scripting case, including lifecycle, timers, recovery and remote refusal.
- `Lea-block-v14-source-rebuild.log` and `Lea-block-v14-source-rebuild-audit.log`:
  the archived host source independently rebuilds with Framework 4.8. All 19
  deployed dependencies come from the source ZIP's `Lea/` payload and match both
  manifests; 348 fixture/license records come from the runtime ZIP. Every
  archived source payload remains unchanged by rebuilding.
- `Lea-block-v14-rebuilt-host-script.log`: that rebuilt EXE passes its scripting
  case. The full host and six-EXE matrices are not repeated on either extraction
  or the independent rebuild.

The host scripting case observes first demand 5,106 ms and split-load wait
4,450 ms. These aggregate multi-call host waits are not individual worker
handshake times. The full 43-case/six-EXE matrices are not implied for extractions
or independent rebuilds by the original candidate results.

The required Engine/Win/Image bundle remains 743,936 bytes with exactly the v13
hashes; optional script work does not add a required renderer dependency.
The compared worker remains 48,128 bytes, the same length as its baseline.

## Experimental Artifacts

Separate candidate: `dist/Firespider/Lea-146-block-v14/runtime`.
`Firespider Internet 19.exe` SHA256:
`28E24F070BEC4891CF5FCCD97F449FBE57835317F9BD5D9ECFCEC33E466BDDED`.

| Archive | Files | Bytes | SHA256 |
| --- | ---: | ---: | --- |
| Standalone | 41 | 2941554 | `D96FEE455A1B9703EDC84E53D3015F8AB10B9F158CD376FB77170A8F2ABC70F7` |
| Host runtime | 372 | 3637356 | `93337591BF0C8F74061B32C9053C15ADD9C20B466E1F5A83538631E766A149B9` |
| Host source | 104 | 3295050 | `23153218CFD2D8D1E366713369EC74BFC24ED8830F24CB5873A3636302319FFA` |

Standalone archive: `dist/LeaScript/Lea-146-block-v14/Lea-js-146-native_2026-10-07_02-43-56.zip`.
Host archives are under `dist/Firespider/Lea-146-block-v14-packages`, named
`Firespider-Lea-146-script-runtime_2026-10-07_02-44-35.zip` and
`Firespider-Lea-146-script-host-source_2026-10-07_02-44-35.zip`.

Every archived payload length/SHA and exact file inventory matches its manifest
and staging input. Every payload is checked again after fresh NTFS extraction.
The initial audit's Windows ZIP separator mismatch and its later license-source
path assumption are verifier setup failures, not engine failures; both logs are
retained. The corrected source audit verifies license inputs against
`runtime/licenses`, as the packer specifies. Host source contains buildable VB
plus compiled Lea dependencies; full engine source is retained separately.

Compression measurements describe distribution size, not runtime speed.
Earlier v13 archives, the installed browser and all failure evidence are intact.
`Lea-block-installed-audit.log` verifies the three installed binary hashes and
all 140 files in the existing full Firespider source backup against the live
source. A new complete Lea source backup and its inventory/hash audit are
retained independently of runtime packages. None of these passes explains the
earlier intermittent timeouts, establishes a startup SLA, or completes document
origin/resource policy; milestone 146 remains open.

## Commands

```powershell
dotnet build Lea.Script.Tests/Lea.Script.Tests.csproj -c Release --no-restore /m:1
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe . --scope
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe . --bootstrap 'C:\baseline\ScriptWorker'
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe . --startup
.\Lea.Script.Tests\bin\Release\net462\Lea.Script.Tests.exe .
```
