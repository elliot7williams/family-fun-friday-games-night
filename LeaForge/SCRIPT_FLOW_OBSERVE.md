# Script-flow startup observation

## Purpose

The preceding v26 host matrix fails when a worker starts after an intentional
script error. Its first-demand observer has already stopped, so it does not
sample that failed launch. This checkpoint extends test-only observation through
the existing script flow without changing production code or treating that
earlier timeout as passed.

`Test-FirespiderBoot.ps1` keeps `-Observe` / `-obs` with default scope `demand`.
The optional `-ObserveScope script` / `-os script` watches `CheckScript`, including
navigation, error recovery, split panes and disposal. The existing three native
boot arguments remain compatible; an optional fourth argument selects `demand`
or `script`. A scope without a report, unknown scope or unsupported case is an
error, not a silent fallback.

Example, with a new filename in an existing directory:

```powershell
& 'L:\Lea Development\Lea.Build\Test-FirespiderBoot.ps1' `
    -r 'C:\Users\Elliot\AppData\Local\Lea\HostDiagnostics\Lea-146-script-flow-observe-v27' `
    -c script -obs 'H:\Lea Evidence\new-script-flow.json' -os script
```

With `-Case all`, only the final script case gets the report/scope arguments.
The whole-script report's `kind` is `firespider-script-flow`; `flowPassed` covers
the script core and its existing 50-second completion check, not the other host
cases or later settings-restoration bookkeeping. The wrapper result remains the
authority for the complete case. First-demand reports keep their previous kind
and narrower meaning. No host-case observer or tracing is enabled without a
report. The separate contract subprocess exercises observer lifecycle without
starting a browser worker.

## Bounded collection and failure handling

Whole-script observation traverses this test process's native controls, selects
at most four undisposed Lea views, and rotates one view per 125 ms timer tick.
Four continuously active views therefore each get a turn about every 500 ms;
UI scheduling and sampler cost can delay a turn. This avoids starving later
views behind the shared sampler's single pending slot. It does not guarantee
sampling every short-lived process or every startup phase.

The shared sampler itself is unchanged: exact native process/path/parent
identity checks, a one-slot queue, 512 retained samples, thread snapshots paced
at least one second per identity, 32 identities and 16 threads as bounds.
Counter/thread errors, dropped requests/samples, truncation and retirement stay
visible in the report. Timing trace is restored when observation stops.

A failed flow still saves a failed report and propagates its original exception.
If report output also fails, that secondary error is printed without replacing
the original flow failure. An output error after a successful flow still fails
the test. Create-new output semantics remain intact.

The worker's 5,000 ms startup deadline, script case's 50-second completion check,
wrapper's 60-second process wait, native Job limits, AppContainer, restricted
handles and interpreter budgets are unchanged. No failed runtime case is retried.

## Verification

The final Framework-compatible harness and wrapper pass:

- All 12 deterministic observation-contract checks, including original failure
  identity, create-new output, successful-flow reporting failures, trace restore,
  fair four-view rotation, a shrinking view set and an empty view set.
- All 14 wrapper/direct argument and report-path guards. Existing report and
  compiled-harness sentinels stay unchanged; invalid outputs remain absent.
- Three controlled failures: missing fixture before first demand, missing worker
  dependency with whole-script scope, and the compatible three-argument
  first-demand path. Each exits 1, retains the original error and writes a failed
  report with zero samples and a retired sampler. These are synthetic failures,
  not reproductions of the intermittent startup timeout.
- The ordinary unobserved clean host case, plus the existing 14 clipboard
  contracts. The complete script case passes with final round-robin observation.
  Main and split captures from that run are inspected and nonblank.

An initial all-views-per-tick implementation also passes the script case, but
its one-slot queue drops 136 requests and captures only six workers. That result
motivates the scheduler change; it is retained rather than discarded.

| Observed implementation | Samples | Workers | Thread snapshots | Dropped requests | Observer elapsed |
| --- | ---: | ---: | ---: | ---: | ---: |
| Initial competing requests | 330 | 6 | 46 | 136 | 33,886 ms |
| Final rotating views | 227 | 8 | 58 | 0 | 32,116 ms |

Neither run drops retained samples, records counter/thread/observer errors or
truncates threads. All attempted thread snapshots obey the one-second pacing.
The final sampler retires and records later main/split recovery workers,
including the worker reported after the split's intentional script error.
Its counter mean/max are 0.07/10 ms, thread mean/max 45.26/121 ms, total measured
sampling wall time 2,681 ms and maximum queue lag 80 ms. These are observer costs,
not an application benchmark. Caches/load are uncontrolled, and the different
observed runs do not establish a speedup or a startup fix.

An external launch command is initially blocked by Windows PowerShell's script
policy before compilation or browser startup. Its subsequent result writer
cannot hash the unbuilt harness. Both errors are retained. The wrapper then
runs in the existing PowerShell session without changing machine policy or
retrying a browser failure. Initial harness sources are reconstructed after the
scheduler edit and verified against their exact tested source hashes; initial
and final compiled harnesses are frozen separately.

## Preservation and remaining work

Evidence is under `H:\Lea Evidence\Lea-146-script-flow-observe-2026-10-08`, with
the exact runtime, initial/final harnesses and source, results, complete Lea
source archive, checkpoint bundle and sealed inventory. The three fresh NTFS
diagnostic copies retain the preceding host EXE and all tested production bytes;
each controlled copy's single omitted file is explicit. Installed Firespider
and all 67 original host inputs remain unchanged.

No production engine or shared sampler changes, SDK rebuild, new 1,267-check
full-source run, full 43-case host matrix, six actual-EXE startup scenarios,
release package or installed replacement is claimed. The earlier full-source
pass belongs to its original test executable and the preceding full host matrix
remains failed. No actual timeout is sampled in this checkpoint. Startup
reliability is still unresolved; milestone 146 stays open and broader readiness
remains about 65%.
