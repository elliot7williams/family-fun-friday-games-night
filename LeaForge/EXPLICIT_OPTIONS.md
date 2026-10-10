# Explicit interpreter options

Both script startup paths now construct Jint with `Engine(Options)` rather
than its callback constructor. Internal two-letter `Vo.Of/At` helpers supply
the same configuration to the trusted session and linked worker source.
They create fresh options for each realm; interpreter state and constraint
objects are never shared between sessions. The trusted test factory retains
its existing callback seam without changing the production constructor.

The worker's optional bounded startup trace adds `options` and `configured`
between network-denial setup and VM construction. These checkpoints separate
configuration work from the constructor; they do not identify a root cause.
Handshake validation and deferred page-frame dispatch are unchanged.

## Limits

| Boundary | Unchanged setting |
| --- | --- |
| Trusted interpreter task | 1,000 ms |
| Worker interpreter task | 5,000 ms |
| Native startup | 5,000 ms |
| Adapter page execution default | 2,000 ms |
| Statements / allocation / recursion | 20,000 / 8 MiB / 64 |
| Regular expressions / array size | 100 ms / 65,536 |
| Job process memory / CPU | 128 MiB / 2,500 ms |

Stack guarding remains enabled. CLR interop, `GetType` and CLR writes remain
disabled. The zero-capability restricted AppContainer, native mitigation mask,
one-process Job, inherited-pipe allowlist, denial probes and disposal rules
are unchanged. Remote and external scripts remain disabled by the adapter.

Only the two declared interpreter budgets are accepted by the internal
builder. This is not a new public timeout control or a larger budget.

## Focused Verification

`Lea.Script.Tests.exe <source-root> --vm-options` passes 14 checks. An
independent copy of the preceding configuration is compared with the new
options before Jint resets constraint counters. Checks cover configured
scalar/constraint fields, CLR restrictions, fresh per-realm objects, invalid
budgets, null rejection, both compiled constructor calls and the linked
worker configuration.

A proof-only helper loads the preceding Script assembly and runs the same
compiled-method assertion from the new test executable. It rejects the old
callback constructor as intended. This control inspects compiled methods;
it does not execute a page or measure startup.

Four exact embedded-resource checks pass. Both runtimes still embed the
same 24,766-byte `Dom.compact.js`; no new compression is introduced.
All eight native view load-failure/recovery checks pass, including retention
of the initial failure, explicit recovery and worker retirement. The fresh
940x700 capture is visually inspected and nonblank.

## Retained Failures

The initial comparison stops before native launch because its inventory
mistakenly treats the rebuilt worker PDB as a runtime dependency. The
comparison now excludes only that known symbol file; all executable inputs
remain hash checked. The test-only repair changes no candidate runtime bytes.

The corrected comparison makes all six native launches once. All three
candidate launches pass; one of three baseline launches times out. The
comparison therefore fails overall. Candidate wall times are 3,820, 3,890
and 2,801 ms. Successful baseline times are 3,172 and 3,773 ms; the failed
baseline reaches 5,023 ms. These uncontrolled samples do not establish a
speedup, cold-cache result, startup fix or causal explanation.

The initial broad suite fails its exact worker ACL assertion because the
proof's deployment includes an unleased PDB. Normal deployment copies eight
worker files, matching the native lease. A fresh corrected deployment omits
only that PDB and preserves all 19 runtime hashes and the test executable.
The symbol file and failed gate are retained; the ACL assertion is unchanged.

The corrected broad suite fails after 236 passing assertions at `recovery
after unpublished-launch cancellation`. Preceding assertions verify that
Stop kills the unpublished child, no code is executed, cancellation is
reported and its failure report is bounded. The recovery assertion does
not print its new result, so this run alone does not identify the cause of
the recovery failure. No retry or startup-limit increase is made.

The complete renderer smoke and standalone browser smoke pass. The browser
exits with actual code 0 and reports native isolation, no loopback exemption
and the 128 MiB Job limit. One separate copied .NET Framework 4.8 Firespider
script case also passes, covering lifecycle, events, fields, error recovery,
split isolation/disposal and remote-origin refusal. Its harness checks the
child result and exit; the outer report leaves the numeric exit code null.
The full 43-case host matrix and installed EXE startup are not reverified
or claimed fixed. All five fresh captures are inspected. The attributes
capture has a narrowed content viewport and unused black area, as in the
preceding copied-host test; it is not evidence of polished full-window layout.

Evidence: `H:\Lea Evidence\Lea-146-explicit-options-2026-10-10`.
Normal production outputs and the original Firespider source/installed
binaries remain unchanged. Milestone 146 is open; broader readiness remains
about 65%, a practical estimate rather than a conformance score.

## Size And Next Work

The tested Script DLL is 95,232 bytes, 512 bytes larger than the preceding
candidate. The worker remains 47,104 bytes. Required Engine/Win/Image files
remain 749,056 bytes, excluding optional interpreter/framework dependencies.
Short aliases simplify embedding but do not make machine code execute faster.
Archive compression reduces distribution size, not necessarily resident
memory or runtime cost. No optimized release or default adoption is claimed.

The explicit constructor follows the pinned
[Jint 4.16.4 Engine source](https://github.com/sebastienros/jint/blob/884bd0c4ebf7806f70041eae0370ee11a9c138bc/Jint/Engine.cs).
The installed compiler inventory contains Visual Studio 2017, not the
[Visual Studio 2022 C++ toolchain required for Windows Native AOT](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/?tabs=windows).
No AOT build, framework migration or toolchain installation is performed.

Next work is repeatable native startup and cancellation/recovery diagnostics,
followed by passing full script and Firespider migration gates. Document
origins alone do not provide CORS, CSP, executable-resource fetching or safe
general website scripting; those remain separate required work.
