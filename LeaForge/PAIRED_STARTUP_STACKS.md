# Paired Worker Startup Observation

Milestone 146 remains open and broader readiness stays about 65%. This
checkpoint changes the optional observation script and documentation only.
All 19 production runtime records remain identical to the animation-frame
checkpoint, including default `Be=0`; required bytes remain 749,568. Original
Firespider source, installed files and historical normal outputs are untouched.
There is no startup fix, speedup, new browser build or stable release claim.

## Targeted Aliases

`Lea.Build/Stacks/Observe.ps1` now accepts `-m worker` to run the existing
87-assertion `--worker` regression rather than the broad suite. Optional `-p`
samples only the newest matching worker when CIM enumerates at least two owned
worker children. A tied newest creation timestamp is rejected for sampling.
Default full mode remains available, with the current 2,651-check footer;
`-n` explicitly selects the expected count for another compiled test revision.
Success requires both actual test exit zero and exactly one matching footer.
The driver now explicitly returns zero on success and one on gate failure.

Worker mode records `worker.log/worker.err`, not files mislabeled as a full run.
Both mode and expected footer/count are retained in the plan/result. A new
`cohortAtEnumeration` records child PIDs, creation times and images. This is
the enumeration used for selection, not a claim that every peer remains alive
throughout inspection. The selected worker still requires exact native parent,
birth, image and live-process identity checks with a pinned native handle.

Existing bounds remain: eight runtime file read leases and hash checks,
non-reparse ordinary runtime files, at most 32 snapshots, one attempt per
enumerated PID/birth, five-second inspector watchdog, bounded test watchdog,
and final cleanup. The engine's five-second startup and default two-second page
execution deadlines, memory/CPU Job limits, AppContainer and network/file
denial probes are not relaxed. The external startup-trace flag is cleared and
restored; tests can deliberately enable their own tracing.

The optional ClrMD inspector is unchanged. It uses supported suspended live
attachment with `finally` disposal, no dumps, locals, heap data or symbol server,
and bounded thread/frame descriptions. A temporarily paused worker is not an
unobserved timing sample. The observer is outside every engine/browser package.

## Controls And Results

Evidence: `H:\Lea Evidence\Lea-146-paired-startup-stacks-2026-10-11`.

- Eight pure eligibility controls cover empty/single/multiple children, paired
  versus ordinary mode, ordering and ambiguous timestamps.
- Wrapper-only executables prove worker arguments, exact 87-count recognition,
  rejection of an 86-count expectation despite test exit zero, and the current
  2,651 full footer, including both default counts without overrides. These
  controls launch no native worker and are not engine
  conformance tests. `-m full -p` rejects before creating output or a child.
- The initial evidence helper used array rather than hashtable splatting; its
  parameter-binding failure occurs before entering the observer. The original
  helper, output and failure are preserved, with separately named corrections.
- The first corrected wrapper reaches a passing test/report but fails its
  caller exit check because the observer lacks explicit `exit 0`. That result
  is preserved. The final observer explicitly returns success/failure and all
  final wrapper controls pass. The eligibility function bytes are unchanged.
- One real observed worker run passes 87 assertions, actual exit zero, in
  65,048 ms overall. This is a test-suite elapsed value, not browser startup.
- One stack capture selects PID 52804 from a two-worker enumeration (older PID
  48488, test parent 50680). Exact target identity and runtime hashes verify.
  Enumeration age is 811.4587 ms; inspector launch plus driver work takes
  1,855 ms. Attachment creation is 156 ms, inspector report elapsed 675 ms;
  these are distinct intervals, not interchangeable pause measurements.
- The attachment is created and disposed; 13 named main-thread managed frames
  include `Acornima.Parser.ParseExprSubscripts`, parser expression/top-level
  methods, `Jint.Engine.Evaluate`, and the guarded worker `Run/Main` path.
  Its runtime frame is `PrestubMethodFrame`. This captures bootstrap parsing
  in a passing run, not the earlier VM-construction stall.

The next operation after `startup configured` is the pinned
[Jint constructor](https://github.com/sebastienros/jint/blob/884bd0c4ebf7806f70041eae0370ee11a9c138bc/Jint/Engine.cs), which initializes its realm
and parser before evaluation. This new capture is later, in evaluation. It
does not locate the exact blocking operation in the older failed attempt.

The preceding quiet worker failure after 45 assertions remains unresolved:
second-worker startup timeout, parent phase `hello`, wall 5,012 ms, CPU 31 ms,
last recorded stderr marker `startup configured`. The prior broad run's
565-assertion microtask-replacement failure also remains unclassified; no
broad rerun is performed here. A system reading just before this new run shows
2,087 MB available physical memory, 83% committed memory and zero pages
input/output per second. It was not sampled at either old failure and cannot
establish their cause. All earlier native failures remain in the audit.

Owned candidate, control and inspector processes are verified absent after
completion. The source backup, per-file hashes and checkpoint Git bundle
preserve these results. Do not append new runs to a sealed evidence directory.

## Reproduction

Use absolute paths and a new output directory:

```powershell
& 'L:\Lea Development\Lea.Build\Stacks\Observe.ps1' -t '<Lea.Script.Tests.exe>' -f 'L:\Lea Development' -i '<Lea.Stacks.exe>' -o '<new-directory>' -m worker -p -s 500 -n 87
```

The script names and verifies those exact executables. Never infer a startup
fix from a passing observed run; the suspended attachment changes timing.
