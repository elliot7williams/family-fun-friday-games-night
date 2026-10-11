# Experimental Prepared Native Bootstrap

Milestone 146 remains open. Broader browser readiness stays about 65%.
This adds a constrained native-worker alternative and a smaller verified
distribution, not a default adoption, native startup fix or release.

## Build Alias

`Be=0` remains the ordinary string-evaluation default. `Be=1` retains intrinsic
indirect eval. New `Be=2` prepares the fixed trusted bootstrap with Jint static
analysis, then evaluates the prepared syntax tree. It does not prepare or
cache page code, serialize an AST, share a realm between processes, or defer
bootstrap work until after hello. Each worker prepares its own asset once.

The prepared path uses explicit `StaticAnalysis=true`, `FoldConstants=false`,
interpreted regexes and a 100-ms regex timeout. Referenced-global collection
stays off. The factory is private and compiled only for `Be=2`. The trusted
in-process cache and all page/event/timer dispatch paths remain unchanged.

The pinned
[preparation implementation](https://raw.githubusercontent.com/sebastienros/jint/884bd0c4ebf7806f70041eae0370ee11a9c138bc/Jint/Engine.Ast.cs)
precomputes syntax metadata. This informs the alternative but does not explain
Lea's intermittent startup failures or prove a speedup.

```powershell
dotnet build 'L:\Lea Development\Lea.Script.Worker\Lea.Script.Worker.csproj' -c Release --no-restore `
  -p:Be=2 '-p:OutputPath=H:\Lea Builds\Prepared' -p:AppendTargetFrameworkToOutputPath=false `
  '-p:IntermediateOutputPath=H:\Lea Builds\Prepared-obj'
```

Keep all eight worker runtime files together on local NTFS for native testing.
The H: volume here holds builds and evidence, not runnable AppContainer worker
deployments. `W32` remains a separate option; combinations are not newly
verified by this checkpoint.

Build guards reject unknown modes, missing/replaced prepared symbols,
substring impostors, cross-mode symbols and mixed indirect/prepared symbols.
`Be=3` is now the invalid-mode control; historical `Be=2` rejection evidence
belongs to the older two-mode checkpoint. Six validation-only positive cases
and seven actual-build rejection cases pass without producing a guard worker.
All three real worker modes also compile without warnings or errors.

Assembly metadata identifies the entry. Compiled-call contracts check the
actual `PrepareScript` and by-reference prepared `Evaluate` calls, absence of
intrinsic eval in that path, and the unchanged explicit options constructor.
The same prepared-only contract rejects the compiled indirect control after
its four legitimate entry checks.

## Deadlines And Checkpoints

The native parent startup deadline stays 5,000 ms and covers the whole launch,
preparation, evaluation and handshake. The interpreter still has its separate
5,000-ms entry budget. Like parsing in ordinary `Be=0`, prepared analysis runs
outside that entry budget; passing it alone is not evidence of lower total
startup cost. The Job's 128-MiB process bound still applies during preparation.
Only the fixed 24,766-byte embedded asset is admitted to this preparation path.

The 20,000-statement, 8-MiB interpreter-allocation, recursion-64, 100-ms regex,
65,536-array and stack guards, disabled CLR access, 25% CPU-rate hard cap,
one-process limit, kill-on-close, low zero-capability AppContainer, canary and
network checks, three inherited pipes, mitigations and verified hello ordering
are unchanged. Remote/external page scripts remain disabled.

`prepare` and `prepared` bracket actual preparation, followed by `evaluate`.
The existing `parsed` marker comes from Jint's real BeforeEvaluate callback;
for this mode it observes the already-prepared AST, not a second parse. The
callback is removed before page execution. Timing does not select another
bootstrap strategy. Tests verify ordering and absence of preparation markers
in the other compiled modes.

One timed native checkpoint run records:

| Checkpoint | Worker wall/CPU ms |
| --- | ---: |
| Prepare entry | 563 / 125 |
| Preparation complete | 1,256 / 156 |
| Evaluation entry | 1,257 / 156 |
| BeforeEvaluate callback | 1,268 / 156 |
| DOM complete | 1,958 / 187 |
| Hello entry | 1,959 / 187 |
| Hello completion | 1,977 / 203 |

These worker-relative diagnostic times are not a cold-OS benchmark.

## Native Evidence

All gates run once, sequentially, without an external debugger or startup
observer. Three predeclared alternating pairs compare newly compiled indirect
and prepared workers from the same source with matching dependencies. Every
launch must complete its first page mutation, preserve policy, and retire the
child and exact ACLs; hello alone does not pass.

| Pair | Indirect parent startup wall/CPU ms | Prepared parent startup wall/CPU ms |
| --- | ---: | ---: |
| 0 | 3,078 / 234 | 2,569 / 187 |
| 1 | 2,380 / 171 | 2,413 / 46 |
| 2 | 2,432 / 171 | 2,494 / 78 |

All 27 comparison checks pass. Mixed wall times and three small samples do not
establish a speedup. Job peak process memory is about 30.5 to 30.8 MiB across
these launches; it is not a demonstrated memory reduction.

- Six prepared-entry and eight direct/indirect compiled-entry checks pass.
- Thirteen build guards, 14 interpreter-options and four asset checks pass.
- All 14 unchanged trusted-cache isolation/reset/concurrency/GC checks pass.
- Fourteen native checkpoint and 36 combined trusted/native scope checks pass.
- Seven real-worker deadline/retirement/recovery checks pass.
- The complete broad scripting executable passes **2,226 checks**, actual
  exit zero and empty stderr, in its one declared run.
- The standalone browser and complete renderer smoke pass, with actual exit
  zero and two freshly inspected captures.

The broad wrapper initially reports failure because its prefix matcher counts
20 lines, including individual assertions named `Lea.Script:`. Inspection
finds exactly one numeric final completion line and all 2,226 assertion lines.
An exact completion matcher validates the same preserved log and exit code.
The initial wrapper result is retained; neither tool nor runtime is rebuilt
and the suite is not rerun to repair that classification.

## Copied Host Failure

The full copied Firespider matrix is attempted under .NET 4.8 using the pinned
boot harness. Clipboard/observation contracts pass 14/12 checks. All 42
non-scripting cases pass, but the final scripting case fails on first demand:

```text
host supplied script: timeout
Worker startup deadline exceeded. phase=hello
launch=521; probe=65; wall=5022; cpu=765 ms
last checkpoint: startup parsed
code=var retainedState=41;
```

Preparation has completed and the BeforeEvaluate callback has fired, but DOM
completion/hello has not. This narrows the failed phase; it does not identify
the cause. The failed host child exits one, owned processes retire, and no
script-host capture is produced. There is no separate scripting-case retry
or replacement passing run. The passing broad suite and browser do not clear
this failure or the earlier intermittent startup failures.

The original .NET 4.6.1 host is not rebuilt, migrated or installed. No fix to
the installed Firespider EXE is claimed. Default builds remain `Be=0` pending
reliability and adoption work.

## Persistent Test Build And Compression

Twelve common dependency DLLs disappear from each of two temporary C: build
folders after the initial stage. Their remaining files match the retained H:
copies; the cause is unknown and both temporary folders are left untouched.
The first wrapper fails its baseline file check before any test process starts.
The exact compiled candidate is staged again in a new persistent NTFS folder,
with no binary rebuild or native retry. H: baselines, source backup, original
inputs and the persistent host basis remain intact.

The delivered experimental browser is now in
`C:/Users/Elliot/AppData/Local/Lea/EngineBuilds/Lea-146-native-prepared-2026-10-10`.
Its existing browser EXE is paired with the tested prepared worker and exact
dependencies. The runtime-only ZIP includes 24 files, excluding tests/symbols:
8,269,052 raw bytes compress to **2,899,390 bytes**. Each ZIP entry is verified
against the tested bytes. Extract to local NTFS; H: is not the native execution
volume. Compression reduces transfer/storage size, not proven execution cost.

Only the candidate worker EXE differs in the 19-file runtime: 48,128 bytes,
SHA256 `A1D790253B6A7F42C89C81E717E587A2F426A9A650FA568D49E8CABC27E907BD`.
The required Engine/Win/Image bundle stays 749,056 bytes, and the compact
bootstrap remains SHA256
`E0CE6586B3A718D065650236CB282E2EB6E866CBB0CC8463C55C3A2AC348716C`.

Normal outputs, 67 original Firespider inputs, three installed files and the
379-file host basis are verified unchanged. All worker modes, failures,
staging/completion repairs, reports, two captures, verified runtime/source
archives and Git checkpoint are retained under
`H:/Lea Evidence/Lea-146-native-prepared-2026-10-10`.

The next reliability work must investigate execution after the prepared AST
callback and pass the complete copied host before considering default adoption.
Full modern-site compatibility and the original installed-startup verification
remain unfinished; the active engine goal is not complete.
