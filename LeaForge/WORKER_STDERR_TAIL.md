# Bounded Worker Stderr Tail

Milestone 146 stays open. Broader browser readiness remains about 65%, a
planning estimate rather than a standards score. This checkpoint corrects
diagnostic retention; it does not resolve intermittent native worker startup.

## Change

The shared `Lea.Win/Rp.cs` launcher is compiled into both Win and Script.
Previously it kept the first 1,024 characters and decoded each byte read
independently. Later startup markers could disappear once the buffer filled,
and a UTF-8 character split across reads could become replacement characters.

`Drain` now uses an incremental UTF-8 `StreamReader` and a rolling text tail.
The retained `E` text stays bounded to 1,024 UTF-16 code units under the existing
lock. Leading U+FEFF is preserved as text, not automatically removed as a
preamble. The reader leaves the pipe open for its original owner's disposal.
This is a bounded tail, not a complete log or Unicode-scalar-aligned boundary;
an old surrogate pair can still be cut at the retention boundary. Malformed
UTF-8 uses the decoder's replacement fallback.

No public alias changes. The existing 512-character startup-report/message
limits, exact-process CPU reporting, retirement and pipe ownership stay intact.
No startup/page deadline, AppContainer permission, Job limit, mitigation,
interpreter limit, retry policy or page-code admission changes.

## Verification

`Lea.Script.Tests --worker-stderr` runs six samples against each linked launcher:
empty, short ASCII, leading U+FEFF, one-byte fragmented multibyte UTF-8, a
multibyte sample across the retention boundary, and more than 5,000 characters
with late startup markers. These use the real drain thread with a test-only
fragmented memory stream; they do not launch or grant permissions to a worker.

The identical final test EXE fails on the preceding production runtime at the
leading-U+FEFF case, then passes all 12 checks on the new runtime. The earlier
ten-case fixture and its old-runtime failure are also retained. They preceded
the added preamble regression and are not retries of the final gate.

| Gate | Result |
| --- | --- |
| Worker stderr | 12 checks pass |
| Native CPU lifetime | 17 checks pass |
| Startup report | 7 checks pass |
| Bootstrap assets | 4 checks pass |
| Ordinary unobserved scripting suite | 1,784 checks pass, one candidate run |
| Required native renderer smoke | Pass, one run |
| Actual standalone browser EXE | Lifecycle, click, isolation and retirement pass |
| Separately copied Firespider script case | Fail, one observed run, no retry |

The Tests, Win and Script SDK builds complete with zero warnings/errors.
The standalone browser capture is fresh and visually inspected: counter 1,
native name field, page content and Lea title render. The failed host flow
never reaches the capture stage; no fresh host screenshots are claimed.
The full 43-case host matrix, six actual-EXE startup scenarios, release
packaging and installation are not performed for this checkpoint.

## Retained Failures

Before the production edit, the broad baseline run fails after 406 assertions:
startup `hello`, wall 5,049ms, CPU 671ms, last recorded marker `parsed`.
The final raw-counter test source is used, but this startup failure occurs
before the CPU fixture. It does not diagnose the preceding checkpoint's
unlogged second-worker CPU assertion failure.

The candidate's observed Firespider script flow also fails during the initial
supplied-script worker: launch 560ms, parent probe 61ms, wall 5,045ms, CPU 734ms,
last recorded marker `parsed` (worker wall 2,876ms / CPU 468ms). The retired
sampler retains 36 samples for one PID, with no dropped requests/samples,
observer errors, counter errors or thread errors. The final sample reports
718ms CPU. Neither an absent later marker nor sampled thread states establish
the native startup cause. The passing ordinary suite does not replace either
failed gate; no intermittent-startup fix or speedup is established.

## Size And Preservation

Only `Lea.Win.dll` and `Lea.Script.dll` change among the 19 production records.
Their sizes remain 316,416 and 93,184 bytes. The required Engine/Win/Image bundle
remains 746,496 bytes; it is not the complete optional scripting distribution.
Worker, interpreter, bootstrap and browser/main EXE bytes do not change.

Tests and isolated host copies run on NTFS. H: holds evidence and backups,
not native AppContainer execution. C: remains short of free space; no causal
claim about disk space and the startup timeouts is made. The original 67
Firespider inputs and three installed binaries are unchanged.

Evidence, initial/final frozen runtimes, plans, build logs, actual test exits,
negative controls, observer data, the standalone capture and a complete source
ZIP are kept at `H:/Lea Evidence/Lea-146-cpu-raw-broad-2026-10-09`.
The source ZIP is also kept in `H:/Lea Source Backups`; every source entry is
verified against its current size and SHA-256. Previous sealed checkpoints
and their failures remain unmodified.
