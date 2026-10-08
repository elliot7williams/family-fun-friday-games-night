# Firespider Event Options Candidate

Separate v22 brings the tested optional scripting runtime into the copied
.NET Framework 4.8 Firespider project. It is a test candidate, not an installed
replacement or a fix for the intermittent cold-worker startup failure.

## Host Regression

The script case now checks ordered event type/dictionary conversion, null and
duplicate callbacks, getter-error identity, and atomic registration failure.
Real native counter activation verifies primitive capture options, while a
second canceled-link activation verifies primitive removal and retention of the
original duplicate listener's `once` flag. Existing lifecycle, timers, DOM,
split isolation/disposal, tab cleanup, and remote-origin refusal remain covered.

Only the host test and documentation changed at this checkpoint. Production
engine binaries, embedding aliases, dependency versions, deadlines, mitigation
settings, and remote-script gates are unchanged from the event-options build.

## Verification

| Build | Host cases | Actual EXE startup scenarios |
| --- | --- | --- |
| Separate candidate | All 43 pass | All six pass |
| Fresh runtime ZIP extraction | All 43 pass | All six pass |
| Independent archived-source rebuild | Complete script case passes | All six pass |

Each startup run covers clean, restored, locked, oversized, lazy-background,
and lazy-all sessions, with responsiveness, checked exception/recovery dialogs,
and MSHTML loading checks. Lazy-all also verifies explicit resume. The host
matrix deliberately includes explicit legacy-fallback cases; it does not claim
that those cases never load MSHTML. Native Lea-only script cases reject it.

All 14 deterministic clipboard fixture contract checks pass before each host
run. Candidate and extraction captures are visually inspected: nonblank page,
native counter update, and independent split-pane state. Current engine source
and standalone-package 1,157-check results remain at the preceding checkpoint;
no engine SDK rebuild or new full scripting-suite run occurred here.

Candidate input audits cover 67 original files, 25 overlays, four current
integration overlays, and 19 exact runtime dependencies, including an exact
eight-file worker inventory. Packaging verifies all 375 runtime payload files
and 103 host-source payload files against the copied source, ZIP streams, and
fresh extractions. The independent framework build changes none of its archived
source inputs and retains the same 19 dependencies. Its script test receives
hash-verified fixtures/assets from the runtime archive after compilation; these
are test inputs, not dependencies used to compile the archived project.

The two host EXEs are 1,134,080 bytes. The independent framework compilation
has a different EXE hash; reproducible host compilation is not claimed.

## Artifacts

- Candidate: `L:/Lea Builds/Lea-146-event-options-v22/runtime`
- Test extraction: `L:/Lea Builds/Lea-146-event-options-host-extracted-v22`
- Archived source/rebuild: `L:/Lea Builds/Lea-146-event-options-host-source-v22`
- Packages: `L:/Lea Builds/Lea-146-event-options-host-packages`
- Evidence: `L:/Lea Evidence/Lea-146-event-options-host-2026-10-07`

Runtime ZIP `Firespider-Lea-146-script-runtime_2026-10-07_23-01-56.zip`:
3,645,237 bytes, 376 files including its manifest, SHA-256
`DAB5B9F9551139B78A741905E66DF7506FF83BFE44556803A46705C9F1FF2969`.

Host-source ZIP `Firespider-Lea-146-script-host-source_2026-10-07_23-01-56.zip`:
3,296,315 bytes, 104 files including its manifest, SHA-256
`FEB2C6B5E9B83E094D2D6DADECCF615D267CF13FD429B63898506B32C63E0C96`.
This is the migrated host project with frozen Lea dependencies and licenses;
the full Lea engine source is preserved separately by `Backup-Lea.ps1` and Git.

Candidate/extracted EXE SHA-256:
`BFE54F3A35164E1C3F147E5C32F94A5DEC4897050A7C4B05FF1695DE5EAFB99E`.
Independent rebuilt EXE SHA-256:
`C4B5F21AF3A891217B42B6A37FCFA659D8B739E339736C408B83C07F176C9C8E`.

## Remaining Work

First-demand host script observations are 3,981 ms (candidate), 3,907 ms
(extraction), and 4,396 ms (rebuild). Split-load observations are 4,423, 4,753,
and 4,762 ms respectively. These are functional runs, not controlled benchmarks
or cold-cache experiments. They do not explain the retained earlier startup
deadline failures and are not evidence of a speedup from compression.

Milestone 146 stays open and broader readiness stays about 65%. The installed
Firespider root EXE and engine DLLs remain unchanged. Before adoption, resolve
the intermittent scripting startup failure, complete remaining legacy-only
commands, and expand origin/resource policy and real-site compatibility.
