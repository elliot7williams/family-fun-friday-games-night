# Initial script failure and automatic load dispatch

Both native view load paths previously ignored the result of `Xl()` before
dispatching the automatic `load` event. When optional JavaScript startup or
execution failed, `W.PgA()` cleared its loaded flag and retired the failed realm.
The subsequent `W.EvA(null, "load")` could create another worker and replay the
same page's inline blocks automatically. This could repeat completed mutations,
replace the failure with a later successful result, and prolong a busy page.

`V.ShowA` and `V.ShownScripts` now capture the initial-script result. Automatic
`load` dispatch requires that result to be successful, in addition to the
existing current-page, factory and generation checks. No status is reclassified
as success. Image loading, native rendering, `Done` notification and navigation
cleanup still finish after a failed initial script. `Xo` retains that failure
until an explicit operation replaces it.

Explicit `Xl()` or supplied `JsA()` recovery remains available; the optional
runtime can create a fresh isolated realm for it. A host can explicitly dispatch
`Xe(null, "load")` after a successful retry. A failed initial script no longer
receives automatic load callbacks or an automatic replacement realm. An
interactive `readyState` in a recovered realm is not promoted to complete until
the applicable load dispatch occurs. This keeps real completion distinct from
a failed script's partial effects.

## Verification

Evidence is retained in
`H:\Lea Evidence\Lea-146-load-failure-gate-2026-10-10`.
The identical final test executable on the preceding runtime reproduces a
`setup` result followed by an unintended automatic `load` call.

The candidate passes 200 controlled checks: ten statuses, synchronous `Go` and
asynchronous `GoA`, and immediate/deferred provider completion. They verify
single initial dispatch, retained failure or successful load results, isolated
image completion, idle document state, later explicit commands, and exact-once
provider stop/disposal. CLI names are `--load-failures` and `-lf`.

Eight real native checks use `javascript-load-failure.html`. Its first block
increments a retained counter, writes its title, then deliberately throws.
The failed block runs once; both completed writes remain, images render, the
view is idle, and no worker is created automatically afterward. Explicit retry
increments the counter once more in a fresh verified realm. Explicit load and
later code reuse that realm, which retires on disposal. A fresh 940x700 capture
of the initial failed page is visually inspected. CLI names are
`--load-failure-native` and `-ln`.

The single ordinary unobserved suite stops after 445 passing assertions during
fresh-worker recursion warmup, before reaching the new cases at its end.
The retained startup timeout is 5,012 ms parent wall / 31 ms captured CPU, with
VM creation as the last recorded checkpoint. Its planned total is 2,084 checks;
the 208 focused successes do not turn that failed broad gate into a pass.
The failed run is not retried.

The required renderer smoke passes. The separate observed Firespider script
case also passes, including split isolation/recovery and disposal. Its sampler
retires with 244 samples from eight PIDs, no observer/counter/thread errors and
no dropped requests/samples. Three fresh host captures are visually inspected,
in addition to the focused native capture. This passing host case does not
replace the failed broad suite or earlier host failures. The full host matrix,
actual-browser EXE startup scenarios and a new release are not verified here.
Original 67 host source inputs and three installed binaries stay unchanged.

Only `Lea.Win.dll` changes among 19 production records, with unchanged
316,416-byte size. Required Engine/Win/Image size remains 747,520 bytes.
No parser, bootstrap, worker, wire-format, dependency or public alias changes
are included. AppContainer/file/network/Job/mitigation protections and startup
5,000-ms/default execution 2,000-ms deadlines remain unchanged. External page
scripts remain disabled.

This fixes the implicit replay and result-loss defect. It does not establish
why the first isolated startup sometimes exceeds its deadline or prove a
speedup. Prior full and Firespider startup failures remain preserved. Milestone
146 stays open and broader readiness remains about 65%.
