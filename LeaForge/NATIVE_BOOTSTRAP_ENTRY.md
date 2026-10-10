# Experimental Native Bootstrap Entry

This is the retained native-entry checkpoint. Subsequent
[trusted bootstrap cache work](TRUSTED_BOOTSTRAP_CACHE.md) changes only `Js`;
the native worker described here remains byte-identical in that candidate.

Milestone 146 stays open. Broader browser readiness remains about 65%.
This checkpoint adds a real native-worker alternative, not a startup fix,
default adoption, speedup or release.

## Build Alias

`Be=0` is the default. It retains `Engine.Evaluate` and the real `parsed`
debugger callback. Explicit `Be=1` compiles the same trusted bootstrap through
`Engine.Invoke(vm.Intrinsics.Eval, Undefined, source)` and emits `eval-end`
only after that call completes. Indirect eval has no script parser callback;
the worker does not fabricate a `parsed` marker or parse twice for tracing.

For a separate developer output with the pinned dependencies already restored:

```powershell
dotnet build "L:\Lea Development\Lea.Script.Worker\Lea.Script.Worker.csproj" -c Release --no-restore `
  -p:Be=1 "-p:OutputPath=H:\Lea Builds\Be1" -p:AppendTargetFrameworkToOutputPath=false `
  "-p:IntermediateOutputPath=H:\Lea Builds\Be1-obj"
```

Keep the worker's existing eight runtime files together on a local NTFS volume
when testing native isolation. H: here is a build/evidence volume, not the
AppContainer worker deployment volume. Symbol files are preserved separately,
not added to the leased native worker runtime. `Be=2` and direct symbol
overrides with `Be=0` are rejected before producing a worker. `Be=1` also
rejects global compiler-symbol replacements that remove its required symbol;
both valid modes are checked through the validation target.
Compiler symbols are matched as tokens, not substrings: unrelated names are
accepted for direct builds and cannot masquerade as the indirect symbol.

The build choice is not a page command, environment toggle, wire field or new
host capability. Assembly metadata records the compiled entry, and the test
contract checks actual calls against it. The compiled public invocation uses
`Invoke(JsValue, object, object[])`, not Jint's internal property-invoke API.
This stays on Jint's normal constrained entry.

Trusted in-process `Js` stays unchanged, including its one-second bootstrap
budget. Both native builds use the same explicit five-second interpreter
options and parent five-second startup limit, 20,000 statements, 8 MiB
allocation, recursion 64, regex 100 ms, arrays 65,536, stack guarding and
disabled CLR access. Parsing inside indirect eval is included in that entry's
budget. The Job remains 128 MiB, one process, kill-on-close and a 25% hard
CPU-rate cap (`Cp=2500` basis points, not milliseconds).

The zero-capability low AppContainer, suspended Job assignment, file canary,
network denial/positive controls, no loopback exemption, three inherited
pipes, mitigations and page frames only after verified hello remain unchanged.
Remote/external scripts stay disabled. `W32` remains a separate experimental
choice; combinations are not newly verified here.

## Native Evidence

Direct and indirect control workers are compiled from the same source using
the same SDK and matching dependencies. One corrected compiled test tool is
used for the comparison. Three predeclared alternating pairs run without
warmup, debugger, startup observer or retries. Each result must also complete
its first page mutation and retire its child/exact ACLs; merely reaching hello
does not pass.

| Pair | Direct startup wall/CPU ms | Indirect startup wall/CPU ms |
| --- | ---: | ---: |
| 0 | 3,891 / 375 | 3,101 / 593 |
| 1 | 2,799 / 281 | 3,093 / 265 |
| 2 | 2,820 / 265 | 3,093 / 453 |

All six launches pass the 27 comparison checks. These small, mixed results do
not demonstrate a speedup, a cold OS/CLR-cache benchmark or elimination of
previous intermittent failures.

- Eight compiled-entry checks pass across both builds. An older worker
  without entry metadata is rejected by the same compiled assertion.
- Four embedded-resource and 14 options/compiled-constructor checks pass.
  The exact compact bootstrap remains 24,766 bytes, SHA256
  `E0CE6586B3A718D065650236CB282E2EB6E866CBB0CC8463C55C3A2AC348716C`.
- Twelve native checkpoint checks pass with timing both off and on, proving
  the correct unique entry checkpoint and absence of the other marker,
  unchanged limits, verified hello ordering and no repeat on page execution.
- A separate isolated-only invocation of the existing private scope suite
  passes 19 checks without warmup. It covers private captures, page-global
  collisions, wrapper intrinsics, token/DOM mutations, microtasks, global
  string timers, budget reset and fresh-realm recovery, plus native guards.
- Seven real-worker execution-report deadline/retirement/recovery checks pass.

## Failures And Repairs

The combined `--scope` test fails in its first, unchanged trusted session:
`limit; Script budget exceeded. phase=bootstrap; reason=TimeoutException`.
It does not reach its native assertions. Its failure remains retained.

The broad scripting suite runs once without an observer and fails after 24
passing assertions at `computed script`, with the same trusted bootstrap
timeout. It does not reach the full native/security/DOM matrix. There is no
retry or warmup-to-green run. Earlier native startup failures remain open too.

The first new test build has a namespace-resolution error; the corrected build
uses qualified `Jint.Engine`. A compiled-call contract initially expects the
wrong overload; inspection confirms the actual public call and the assertion
is corrected. Both initial failures and binaries remain preserved. All 19
runtime files are unchanged by that test-only correction; no native launch
has occurred before it.

The first isolated-only proof helper selects its H: application-base worker
path and is rejected during setup. A separate helper explicitly selects the
known NTFS candidate path and invokes the same existing suite. Both helpers
and results remain retained. No runtime guard is weakened and no failed
native execution is retried.

Only this run's redundant initial C: test folder is retired after all 28 files
(10,205,850 bytes) are hash-verified against their retained H: copy and no
live process is found. No source or installed files are deleted. This frees
approximately 10 MB, not the requested 250-MB general disk headroom; disk
space is not established as the startup-failure cause.

## Browser And Preservation

The standalone browser smoke passes with actual exit zero, isolated script
mutations and verified policy. One copied Firespider scripting case passes
under .NET 4.8 using the pinned harness; its helper checks child exit zero,
while the outer PowerShell result has no exit code. The original .NET 4.6.1
host is not migrated, rebuilt or installed. The full host matrix is not rerun.

Four fresh captures are inspected: browser 980x680, copied host main and
attributes 1382x736, and split 1100x700. The attribute capture has a narrow
viewport and unused black area; it is not proof of the full attribute layout.
The renderer smoke is not rerun because renderer/core bytes remain unchanged.

Only the experimental candidate worker EXE differs from the preceding
execution-report runtime: 47,104 bytes, SHA256
`56E99485180CBB6B8439FAC12629C328EF5B2DA7FD7026BD652C959538C7D758`.
The required engine bundle stays 749,056 bytes. Normal outputs, 67 original
Firespider source inputs, three installed files and the 379-file host basis
are verified unchanged. No original Firespider startup fix is claimed.

The separate experimental browser is in
`C:/Users/Elliot/AppData/Local/Temp/Lea-146-native-indirect-2026-10-10-r3`.
Sources, both workers, corrected/initial test tools, all failures and reports,
captures, byte inventories, verified source ZIP and Git checkpoint are kept
under `H:/Lea Evidence/Lea-146-native-indirect-2026-10-10`.

Default builds stay on `Be=0`. The next reliability work must address the
trusted bootstrap failure and pass the broad native/browser/host gates before
considering adoption. Passing focused native checks does not close milestone
146 or prove full modern-site compatibility.

The pinned upstream [engine invocation](https://raw.githubusercontent.com/sebastienros/jint/884bd0c4ebf7806f70041eae0370ee11a9c138bc/Jint/Engine.cs)
and [indirect eval](https://raw.githubusercontent.com/sebastienros/jint/884bd0c4ebf7806f70041eae0370ee11a9c138bc/Jint/Native/Function/EvalFunction.cs)
describe these entries and strict/global environment selection. They inform
the alternative; they do not establish a cause for Lea's retained failures.
