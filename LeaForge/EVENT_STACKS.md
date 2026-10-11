# Event recovery and managed stacks

Milestone 146 remains open; broader readiness stays about **65%**. The prior
quiet full-suite failure after 714 assertions, at the five-second isolated
bootstrap deadline after startup evaluate, remains unexplained. This change
adds a focused recovery fixture and a separate read-only stack inspector,
not a production startup fix. All 19 production runtime records remain
byte-identical; the required Engine/Win/Image bundle remains 749,056 bytes.

## Controlled tests

`Lea.Script.Tests.exe "L:\Lea Development" -er new-report.json` is the short
alias for `--event-recovery`. Six fixed cycles install 128 listeners, exceed
the existing DOM0/listener ceiling, verify realm retirement, recover a fresh
realm without its predecessor's variables, then deliberately retire it again.
The fixture checks the real isolation, network/file denial and mitigation
flags, reported mutation counts and CPU/memory limits. It uses the same
explicit five-second page budget as the failing Events suite; production's
default two-second page budget and five-second startup limit are unchanged.

One quiet invocation passes **25 checks**, with no timing trace or observer.
A separately labeled observation also passes all 25 and sees 12 worker
identities. Two workers remain visible past the three-second observation
threshold, but both inspector reads fail: the first verifies identity then
cannot enumerate process modules; the second cannot finish identity queries.
Neither provides managed frames or a captured stall. These are sampling
failures, not passing snapshots or proof that the prior failure is resolved.
No second invocation substitutes for either recovery result.

`-st` creates an actual zero-capability restricted script worker and leaves it
waiting for its init frame for 12 seconds. It sends no startup protocol or
page code and then disposes the Job and restores its eight file grants. This
test-only hold does not extend a normal Jp startup deadline. Its two launch/
cleanup checks pass. The corrected inspector control captures the main
thread's native console read, Wire.ReadBytes, Wire.Read and worker Run frames,
in preemptive GC mode, without suspension or a dump. This proves the inspector
can read this controlled blocked worker, not that it captured the real bug.

The inspector requires matching PID, parent, creation time, full image path
and a live retained native identity. Its malformed-argument and mismatched
creation-time controls reject before CLR attachment. Frame output and error
messages are bounded. The inspector project is .NET 8 and separate from all
production dependencies, using pinned [Microsoft ClrMD](https://www.nuget.org/packages/Microsoft.Diagnostics.Runtime/3.1.512801)
and the installed Windows .NET Framework DAC, without a symbol-server lookup.
Passive live reads are explicitly not claimed to be consistent snapshots.

## Preserved evidence

Proof: `H:\Lea Evidence\Lea-146-event-stacks-2026-10-11`.

Both compilation jobs finish without warnings/errors. The original inspector
control driver reads the newly created target report while still empty and
passes an incomplete request; the inspector correctly exits 2 before attach.
The target itself exits zero and passes both launch/cleanup assertions. That
failed wrapper/result and initial driver source are retained. The corrected
driver waits for valid complete request fields and uses separately named
reports; no production or compiled test/inspector bytes change.

The corrected control's raw observer flag was also accidentally false despite
its recorded external snapshot. The final audit explicitly corrects that
metadata interpretation without overwriting the original result. It is an
observed control, never a quiet timing baseline.

The current full scripting suite is not rerun here; the preceding failed
714-assertion gate remains the latest broad result. Firespider's actual-EXE
startup scenarios and copied-host matrix are not rerun. Original host inputs,
installed binaries and normal build outputs are untouched. Source is preserved
in a separately entry-verified full backup. No speedup, stable release,
installed replacement or resolved-intermittence claim is made.
