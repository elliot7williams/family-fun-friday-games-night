# Observed full-suite stacks

Milestone 146 remains open; broader readiness stays about **65%**. Production's
19 runtime records, default bootstrap entry, five-second startup deadline,
128 MiB Job memory limit and 2500-basis-point CPU cap are unchanged. The required
Engine/Win/Image bundle stays 749,056 bytes. Installed Firespider and normal
build outputs are not replaced or rebuilt.

## Corrected inspection contract

The preceding passive inspector used `AttachToProcess(pid, suspend: false)`
against a running worker. ClrMD explicitly does not support that operation;
its observations cannot establish a bootstrap cause. This limitation appears
in the installed 3.1.512801 API XML and in
[Microsoft's supported attachment guidance](https://github.com/microsoft/clrmd/blob/main/doc/GettingStarted.md).
Historical records and their original labels remain archived rather than being
overwritten or promoted to reliable snapshots.

A Windows PSS snapshot prototype compiles, but its guarded init-wait target
rejects capture with error 1816. No Job quota or other protection is widened,
and no PSS full-suite run substitutes for that failed control. The prototype's
source-input hashes and binaries are retained; a reconstructed source copy is
not byte-identical to the recorded input and is not rebuild-verified.

The final optional tool uses supported suspended attachment, validates retained
native PID/parent/birth/image identity first, and disposes the attachment in
`finally`. It disables the symbol file locator and uses the installed Framework
DAC. It emits no dumps, locals or heap values. Thread/frame/method/error output
is bounded; missing named main-thread frames return failure. Both final and
preceding suspended variants compile with zero warnings/errors. Each passes its
own init-wait control and rejects malformed arguments and wrong birth time
before inspection. The final control has no full-suite timing claim.

## Observation coverage

`Lea.Build/Stacks/Observe.ps1` offers short `-t/-f/-i/-o/-s/-w` aliases. It starts
one full scripting test, samples only its worker children, retains actual exit
codes, and never retries or reuses an existing output directory. The external
startup trace variable is cleared/restored, but the fixture deliberately
enables tracing in some cases. Inspector and test watchdogs are 5 and 600
seconds; up to 32 inspections are permitted.

The initial two-second threshold used time since first sighting, missing time
already spent launching. The corrected threshold uses enumerated UTC creation
time; exact native ticks are verified independently, without requiring CIM's
microsecond timestamp to equal native 100 ns precision.

Initial coverage accepted only the base worker path and missed copied runtime
directories used by fresh-path tests. Optional `-w` adds one explicit staging
root. Each candidate must still belong to the test parent, match its eight
selected runtime file hashes, use ordinary bounded files and non-reparse
directories, and pass exact native identity validation. Read leases protect
those verified files during inspection. Four negative launcher input guards
pass without launching a test or creating a new report directory.

## Actual Results

All four differently configured observations fail; none replaces another:

| Configuration | Assertions Before Failure | Evidence |
| --- | ---: | --- |
| Passive, two seconds after first sighting | 217 | Startup phase control times out; inspector reaches an exited worker |
| Passive, 500 ms from birth | 217 | Same phase control times out; raw receipt has an empty main stack and is unsupported |
| Supported suspended reader, base path only | 236 | Two parser stacks; fresh-path worker 0 fails outside the path filter |
| Supported suspended reader, verified staged copies | 244 | Four named main stacks, including fresh-path workers; worker 1 times out |

The final staged observation captures tokenizer/parser paths in three workers
and interpreter property setup in the fourth. All four validate runtime bytes
and identity; their attachments dispose successfully. Fresh-path worker 0
passes its execution/isolation/report/ACL checks. Worker 1 later times out at
the unchanged startup deadline. Its trace reaches DOM, probe cleanup and the
hello-write entry, but not ready, before retirement. Its earlier sampled stack
is in `Jint.Engine.SetValue`/property definition, not the deadline location.
Prestub frames are recorded, but do not prove a JIT defect or a particular
cold-start bottleneck. These pauses alter timings and cannot diagnose the
preceding unobserved failure by themselves.

The latest quiet broad result remains the earlier failure after 714 assertions.
No new quiet full run, current Firespider startup matrix, universal startup fix,
speedup, stable release or installed replacement is claimed. Next diagnosis
must capture the late failing path with less observer startup cost, rather than
assuming the early stacks explain the timeout.

Proof: `H:\Lea Evidence\Lea-146-full-stacks-2026-10-11`. Raw failures, each
observer source variant, prototype/final binaries, final source inputs and
separate final-control records are retained. Final ownership checks cover the
base candidate, scratch directory and all four inspector roots. The complete
current source is preserved in a separately entry-verified backup and Git.
