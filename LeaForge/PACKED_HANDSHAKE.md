# Packed transport and prepared hello

Milestone 146 remains open. Broader lightweight-browser readiness stays about
65%; this change is not a startup fix, a measured speedup, or a stable release.

## Engine change

Private `Wire.Write` serializes through the existing `JavaScriptSerializer`,
counts strict UTF-8 bytes, and encodes the version-three header and JSON into
one bounded byte array. `Send` performs one managed `Stream.Write` and one
flush instead of separate header/body writes. This proves a stream-call and
buffer-count change, not one OS syscall, atomic delivery, or faster browsing.
Counting and encoding UTF-8 take two passes; no throughput benchmark is claimed.

Short internal `Hi` prepares a sealed `Wire.H` containing three bounded hello
packets for network outcomes 0/1/2. `Hr` sends only the actual probe outcome.
Preparation overlaps the existing trusted denial probe, before Jint setup.
Nothing is sent until that probe and its cleanup have completed. Failed guard,
file and network results remain accurately represented and are rejected by the
unchanged parent readiness checks. Unknown masks/outcomes and null prepared
state fail before output. Prepared packets never become page-visible objects.

Magic, version 3, field order/types, 128-KiB payload bound, recursion limit,
strict UTF-8, sequence counters and command schemas are unchanged. The
five-second startup deadline, default `Be=0`, 2,000-ms page budget, 25% CPU-rate
cap, 128-MiB Job limit, AppContainer restrictions and exact cleanup are unchanged.
No retry, early readiness, in-process fallback or guard relaxation was added.

## Verification

Script, worker, tests and the separate browser compile with zero warnings/errors.
Current compiled parent/worker checks add 98 assertions to the existing suite.

| Gate | Recorded result |
| --- | --- |
| `-wf` and `--wire-frames` | 186 assertions each; exact independent legacy bytes, fragmented reads, one write/flush, prepared success/failed outcomes and pre-output rejection |
| Old-parent negative control | Expected exit 1 at the single-write contract |
| Old-worker negative control | Expected exit 1 at the compact-factory call contract |
| Extra arguments | Expected exit 1 before tests |
| Native startup phases | 16 assertions |
| Native worker | 87 assertions, including isolation, limits, cancellation and cleanup |
| Six event-recovery cycles | 25 assertions |
| Fresh standalone browser | Exit 0; isolated startup, JavaScript click and counter update; capture inspected |
| Full scripting regression | Exit 1 after 2,073 assertions; expected completion is 2,516 |

The first `-wf` wrapper misrecognized the footer and counted that footer as
an assertion, even though the actual test child exited 0. Its original
`wire-result.json` remains failed with a raw count of 187. A separate
`wire-recognition.json` validates the exact source footer and 186 assertions
from the unchanged log; no test was rerun for this correction. The long alias
uses the corrected recognizer and passes independently.

The full run fails in `NativeCpu.Retire` for the `Lea.Script` linked launcher.
The independent native reading reports exit `0x00000003` and 78 ms CPU, not
the required exit 0 and at least 100 ms. The test-only helper's source returns
3 when its unchanged three-second wall deadline expires before that CPU target.
Why this occurred is not established. No threshold/deadline was weakened and
the full gate was not repeated. Earlier startup, EOF and observed-full failures
remain preserved and unresolved; reaching a later check does not fix them.

## Size and test browser

Only two of the 19 production runtime records change: `Lea.Script.dll` grows
from 93,696 to 94,208 bytes; the worker EXE grows from 46,080 to 47,104 bytes.
Combined growth is 1,536 bytes. The other 17 records, including required
Engine/Win/Image bytes totaling 749,056, are unchanged. Short aliases do not
by themselves establish smaller binaries or faster execution.

The fresh .NET Framework 4.6.2 test EXE is deployed separately at
`C:\Users\Elliot\AppData\Local\Lea\EngineBuilds\Lea-146-packed-handshake-browser-2026-10-11\Lea.Script.Browser.exe`.
It uses Lea's renderer and the optional isolated JavaScript runtime, not
Chromium or Safari. This is an experimental candidate, not an installed
Firespider replacement. Its one-shot smoke passes: title/counter become `Lea 1`
and `1`, with isolation, no loopback exemption, the 128-MiB limit and no
`mshtml.dll` in the host module list. The inspected 980-by-680 capture shows
Lea's icon, page, fields and updated counter without a startup dialog. This is
one desktop smoke, not a complete site/layout compatibility assessment.
Packaging includes license notices and this limitations report. ZIP compression
is for distribution, not a measured runtime speedup.

Evidence: `H:\Lea Evidence\Lea-146-packed-handshake-2026-10-11`.
The original 67 Firespider source inputs, three installed files and historical
normal outputs are checked unchanged. The current Firespider startup/matrix
was not rerun. A complete source ZIP is entry-hash verified before Git checkpoint
and evidence sealing; metadata is in `source-backup.json` and `checkpoint.json`.
