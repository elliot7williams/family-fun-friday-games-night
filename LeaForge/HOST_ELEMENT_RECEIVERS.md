# Firespider Element receiver integration

## Candidate

Separate experimental v30 carries the Element receiver runtime verified at
`6f17b4e59ef0e937a91b3c4e4cdc957825f48723` into the .NET Framework 4.8
Firespider host. All 19 production files match that source checkpoint. No SDK
rebuild, interpreter preparation/cache, public alias, protocol, compression,
startup deadline, sandbox or resource-limit change is made here.

The candidate builds from 67 original inputs and 25 overlays, with 90 unique
final source/build inputs. Its 376 raw runtime files match the fresh NTFS test
copy and remain unchanged after testing. The harness adds one file.

Candidate EXE: 1,134,080 bytes, SHA256
`CD9A5C17302BFDD9DE9C828E72EE7E87B15C0C8A26D526CE076949A97CB52826`.
Final harness B SHA256:
`274D860F08BA17B8983EDF49736139506DE51470940C6C7F6BC6DDC3EBC0B7BD`.

## Host checks

Both first-demand and inline-navigation realms check shared `getAttribute`/
`hasAttribute` methods, names, arity, non-constructibility and existing own
attributes. Shared class-list accessors must retain separate stable token lists.
Invalid receivers, including nullish values, document/window, primitives,
inherited lookalikes and proxies, must throw `TypeError` before name conversion.
Missing and Symbol names also reject. Each group has zero document mutations
and retains its worker PID; that does not mean zero DOM reads.

A native field check edits the input to `Elliot`, borrows another element's
class-list setter, and renames the input. Borrowed attribute reads must use the
input, not the source counter. The two writes preserve the native editor, field
identity/value, accessible focus/label and worker realm. The fixture ID and
empty class are then restored through two separately checked cleanup writes,
retaining the edit, so later authored-script recovery can find the input.

## Retained harness failure

The original full matrix attempt passes all 42 non-script cases and both new
receiver-realm groups plus the edited-field checks. It then fails the script
case during intentional error recovery: the test left the input renamed, so
rerun authored code cannot find `field` and reports `Cannot read property
'addEventListener' of null`. The native worker had started successfully; this
is not a startup timeout or a passed 43-case matrix.

Harness A, source, log, result and main capture are retained. Its EXE SHA256 is
`399D6D92C3E06793475F59B557B5476AA623B685F2EACCDFE7B069D94308B0EC`.
The defect was also identified by static review while that attempt was running.

Harness B adds fixture restoration, with no production changes, and passes the
complete script case. This includes first demand, inline lifecycle, native
events/defaults, receiver bindings and captured callbacks, timers/microtasks,
live attributes/selectors/tree navigation, intentional errors and recovery,
split isolation/disposal, tab cleanup and remote-script refusal. Both the
14-check clipboard contract and 12-check observation contract pass with each
harness. Neither run enables observation or external timing trace.

The 42 non-script passes belong to A; only the complete script case runs on B.
There is no claim of a single full 43-case pass on the final harness, and no
repeat of the failed A matrix. The final ordinary full-matrix gate remains open.
The separate 1,385-check source pass belongs to the preceding exact source test
EXE and the same 19 production files, not a new source-suite run here.

## Identical-harness control

The exact final B harness and candidate host are copied into another fresh
NTFS runtime, replacing only the 19 production files with the frozen preceding
event-receiver runtime. All other files match. Worker startup, exception
constants and event receiver checks pass; the control then exits 1 at the
first-demand `host element receiver rejection` assertion.

This is the expected Element regression, not an interpreter startup timeout.
It does not reach the later borrowed-field check. The old runtime, unchanged
harness hash, stdout/stderr and failure result are retained without a retry.

## Actual EXE startup

All six existing actual-EXE startup scenarios pass, each observed for at least
eight seconds. Each stays responsive without the checked invalid-cast/JIT/
crash-recovery dialog or MSHTML module. Lazy-all also resumes the selected tab.

| Scenario | First ready ms | Observed ms |
| --- | ---: | ---: |
| Clean | 5,503 | 8,266 |
| Restored | 6,879 | 8,133 |
| Locked | 5,844 | 8,198 |
| Oversized | 5,808 | 8,208 |
| Lazy background | 4,852 | 8,234 |
| Lazy all | 3,619 | 9,121 |

These are surrounding executable/UI-readiness measurements, not five-second
native worker handshake measurements. Likewise B's 4,894 ms first-demand and
6,075 ms split-load waits cover surrounding host work. Machine load and caches
are uncontrolled; no speed improvement is established. Start-page scenarios
can leave the optional interpreter dormant and do not prove JavaScript
cold-start reliability. Windows PowerShell runs the trusted STA helper source
without changing execution policy; its exit and six structured results pass.

Final main/split captures are inspected and nonblank. The main editor retains
`Elliot`; split counters remain independent at 1 and 0, with the logo rendered.

## Preservation and next gate

Evidence is under `H:\Lea Evidence\Lea-146-host-element-receivers-2026-10-08`,
including the frozen 377-file tested runtime, 90 final host source/build inputs,
67 original host inputs, 19 negative overlays, both harness revisions, logs,
captures, full Lea source archive, checkpoint bundle and sealed inventory.
Read-only exploration/capture errors and a corrected null aggregate-size report
are retained; none is relabeled as a runtime test failure or success.

The test candidate is at
`C:\Users\Elliot\AppData\Local\Lea\HostDiagnostics\Lea-146-element-receivers-host-v30`.
Its EXE requires the accompanying dependencies. It is not an installed
replacement or a verified release package. The original 67 host inputs and
three installed browser binaries remain unchanged.

No new runtime/source release ZIP, package extraction, independent archived
host-source build/runtime gate, startup-timeout fix or new full-source pass is
claimed. Earlier v28 packages do not contain this Element revision. The next
gates include the final ordinary host matrix, fresh packages/independent source,
startup reliability, safe remote-script policy and broader website compatibility.
The trusted 1,000 ms constraint and native 5,000 ms startup deadline are unchanged.
Milestone 146 remains open; broader readiness stays about 65%, a practical
estimate rather than a standards score.
