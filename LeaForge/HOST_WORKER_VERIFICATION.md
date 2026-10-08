# Firespider Worker Verification

Separate v23 now verifies the guarded-entry worker in Firespider, its freshly
extracted runtime ZIP, and an independent build of the archived host source.
It remains an experimental test build, not an installed replacement or a fix
for the intermittent five-second native worker startup timeout.

## Gates And Retained Failure

| Build | Host cases | Actual EXE startup scenarios |
| --- | --- | --- |
| Candidate, second complete attempt | All 43 pass | All six pass |
| Fresh runtime ZIP extraction | All 43 pass | All six pass |
| Independent archived-source build | Complete script case passes | All six pass |

Each host run first passes all 14 deterministic clipboard contract checks.
The script case covers dormant loading, state retention, inline lifecycle,
native events and option conversion, bounded timers, live DOM operations,
error recovery, split isolation/disposal, tab cleanup, and remote-origin refusal.

The six actual-EXE scenarios cover clean, restored, locked, oversized,
lazy-background, and lazy-all sessions. They check responsiveness, specific
exception/recovery dialogs, MSHTML absence, and explicit lazy-all Resume.
The full host matrix includes explicit legacy-fallback cases; those are not
claimed to avoid MSHTML. The native Lea-only script case checks its absence.
Candidate and extraction captures are visually inspected: nonblank pages,
counter updates, and independent split-pane values.

The initial v23 host attempt still fails settings restoration on full C:.
Seven cases complete; the next case's assertion marker does not make its
cleanup failure a pass. The second complete attempt follows positive free-space
observations and the actual-EXE startup checks. It is not a retry within an
individual case or a cold-cache experiment. Its passing result does not erase
the first attempt; see [the preserved failure](HOST_WORKER_ENTRY.md).
C: remains low on space, and no user files are removed to obtain these results.

## Source And Runtime Integrity

The candidate's 67 original inputs, 25 overlays, 90 final source files, four
integration overlays, 19 runtime dependencies, and exact eight-file worker
inventory are audited. The original host inputs are unchanged from v22.
Only the worker differs among those 19 runtime dependencies. No engine SDK
rebuild, alias change, dependency upgrade, budget increase, or isolation
relaxation occurs at this host-verification checkpoint.

All 375 runtime payload files and 103 host-source payload files match the copied
candidate, ZIP streams, and fresh extractions. Each archive adds its own manifest.
The independent framework compilation changes none of its 104 archived files
and retains all 19 runtime dependencies. Its 353 manifest-verified fixtures,
assets, licenses, and README are copied only after compilation for testing;
they are not inputs needed to compile the archived project.

The host EXEs are 1,134,080 bytes. The independent compilation has a different
EXE hash; reproducible host compilation is not claimed. The latest engine
source/package 1,171-check runs remain at the preceding worker-entry checkpoint;
no new full engine scripting-suite run is claimed here.

Worker SHA-256 remains
`74B277AACB131AB476F8B5B301AD520778C7F1CE892A51AED8C1BC3EF0CF5EE0`;
parent SHA-256 remains
`A19FEB7E4CF4B5E42F4D85BCF2E207A8090208ED029E81A48F6FB0B4BD2F3ABD`.

## Artifacts

- Candidate: `L:/Lea Builds/Lea-146-worker-entry-v23/runtime`
- Fresh extraction: `L:/Lea Builds/Lea-146-worker-entry-host-extracted-v23`
- Archived source: `H:/Lea Builds/Lea-146-worker-entry-host-source-v23`
- Independent native output: `L:/Lea Builds/Lea-146-worker-entry-host-rebuilt-v23`
- Packages: `H:/Lea Builds/Lea-146-worker-entry-host-packages`
- Evidence: `H:/Lea Evidence/Lea-146-worker-entry-startup-2026-10-08`

H: holds distribution/source/evidence files. Native worker execution stays on
ACL-capable NTFS L:, including the independent build's overridden output path.
The host-source archive contains the migrated VB project and frozen Lea
dependencies; complete Lea engine source is backed up separately and kept in Git.

Runtime ZIP `Firespider-Lea-146-script-runtime_2026-10-08_08-26-50.zip`:
3,645,442 bytes, 376 files, SHA-256
`9349EB19DE0D77EF90FC757433B344E60AEB05319E8DE2C94FDFE35E7E0109A4`.

Host-source ZIP `Firespider-Lea-146-script-host-source_2026-10-08_08-26-50.zip`:
3,296,367 bytes, 104 files, SHA-256
`80ABBD77127BF1D05E8BEF8FFEEB4A84858CA091C2990CD1EFE7348C71B2DB05`.

Candidate/extracted EXE SHA-256:
`CF8E122D18AE28653A2B78DB915B58E46672EC47EEA30130DFC63F9408555F59`.
Independent rebuilt EXE SHA-256:
`4EA0B0696EF4AD1A5B20BAC0FF8433AD68E5672BD25A9F47FFDC32A30ED8204D`.

## Remaining Work

First-demand host observations are 4,087 ms (candidate), 3,970 ms (extraction),
and 5,666 ms (rebuild). Split-load observations are 4,418, 4,396, and 4,451 ms.
These are end-to-end functional host observations, not worker-handshake timings,
controlled benchmarks, or proof of a speedup. The native five-second startup
budget and existing execution ceilings are unchanged.

Installed Firespider files stay unchanged. Milestone 146 remains open and
broader readiness stays about 65%, a practical estimate rather than a standards
score. Reliable native worker startup, remaining legacy-only commands,
origin/resource policy, and real-site compatibility still gate adoption.
