# Bounded Parent I/O Dispatch

This is a source and standalone-runtime checkpoint, not a new Firespider
release. Broader lightweight-browser readiness remains **about 65%**, an
estimate rather than a standards conformance score. The original scoped Lea
1.0 baseline remains complete; everyday-browser compatibility and release
reliability are separate work.

## Scope

`Jp` previously requested a `LongRunning` task for every blocking parent pipe
operation. The .NET Framework default scheduler's
[reference implementation](https://raw.githubusercontent.com/microsoft/referencesource/main/mscorlib/system/threading/Tasks/ThreadPoolTaskScheduler.cs)
creates a dedicated background thread for that option. The compact public
`Ex/ExA/St` API and all runtime limits stay unchanged.

Each active public script execution now owns one background dispatcher reused
for its parent I/O. A bounded queue permits one waiting action behind the
active action and rejects overflow without blocking the owner. Action errors
retain their original exception. Retirement completes the queue without an
owner-thread join; accepted work drains and the consumer disposes its queue.
The collection uses the framework's
[bounded producer/consumer API](https://learn.microsoft.com/en-us/dotnet/api/system.collections.concurrent.blockingcollection-1?view=netframework-4.8.1).

The dispatcher is retired at execution completion, reset, and disposal. A
successful execution can preserve its JavaScript worker and realm without
owning an idle dispatcher between executions. Late completion of retired work
cannot replace a successor dispatcher. Blocking pipe work does not occupy the
shared thread pool. Native launch retains its separate existing launch thread.

Absolute I/O deadlines, owner-state polling, cancellation, permission checks,
protocol version 3, DOM thread affinity, AppContainer isolation, origin policy,
and worker implementation are unchanged. This does not make native OS launch
interruptible or establish consistently reliable cold startup.

## Verification

- The initial regression fails against the old implementation: 32 sequential
  actions observe 32 distinct managed thread IDs instead of one.
- A per-realm prototype passes the focused tests but fails a new native check
  requiring no owned idle dispatcher after execution. It is superseded by the
  per-execution lifetime described above; its logs remain preserved.
- The final focused dispatcher and existing blocked-I/O suite passes **31
  checks**, including reuse, exception identity, bounded FIFO admission,
  nonblocking retirement, late completion, and existing revocation/deadline
  behavior.
- The final real-worker suite passes **87 checks**, including both new idle
  dispatcher assertions, persistent realm identity, hard deadlines, native UI
  behavior, isolation, and cleanup.
- The full current-source scripting suite passes **1,040 checks**. The tests
  and standalone browser build with zero warnings and zero errors; all 19
  shared runtime dependencies match. These are source/runtime results, not a
  full current Firespider host or packaged-release pass.

The earlier cold-start failures recorded in `HOST_TREE_NAVIGATION.md` and other
checkpoint documents remain valid historical results. A later full pass does
not erase them or identify their cause.

## Local Measurements

Frozen old/new runtime snapshots share the benchmark source. Each run records
seven 128-action dispatcher samples and seven native samples performing 32
read-only `document.title` accesses. Both implementations uniformly omit sample
zero from warm medians; all raw samples and all-sample medians are retained.

| Sequential pair | Old/new warm dispatch median | Old/new warm native-read median | Native-read reduction | Old/new startup wall time |
| --- | ---: | ---: | ---: | ---: |
| A: old then new | 99.8102 / 11.7413 ms | 73.56905 / 35.25055 ms | 52.09% | 3831 / 4549 ms |
| C: new then old | 93.4346 / 8.7918 ms | 62.70860 / 34.17650 ms | 45.50% | 3559 / 3535 ms |

The old dispatcher observes 76-128 distinct managed thread IDs per 128-action
sample; the new dispatcher observes one. IDs can be recycled, so these are not
exact counts of thread creations. The separate `after-b` run is excluded from
the comparison because it was launched before the prior terminal's closure
was polled, allowing possible final-cleanup overlap. Earlier prototype
benchmark logs are also excluded from final comparisons.

These two local pairs show lower warm overhead for this specific read workload,
not a 45-52% faster browser. They do not measure page rendering, network or load
performance, memory, allocations, compressed size, or general website speed.
Cold startup is not consistently lower. No such broader improvement is claimed.

## Size And Evidence

The parent `Lea.Script.dll` grows from **96,256 to 97,792 bytes** (+1,536).
All eight worker-runtime files are byte-identical between the frozen snapshots.
This trades a small parent code increase for lower observed dispatch overhead;
shorter aliases or compressed files alone do not establish execution speed.

| Frozen file | SHA-256 |
| --- | --- |
| Old parent | `647AC04464658F0AD196379C8667144ADB509B82D3F147FB56C4B5744E128626` |
| Final parent | `2BF973996E549B38810F9B43E6384DD25CBD78D794762E50051EE29B36DCCF5C` |
| Unchanged worker executable | `B8D9D1CE1CF79C16C8CDF1D0522EC98D03B192FD5B450DF1EACEAD8B94A81FA0` |

Builds carry the pre-commit `e437f4e` SDK informational version. The recorded
hashes identify the actual tested binaries; rebuilding after a commit may
change metadata and requires its own audit.

Evidence is preserved in `L:/Lea Evidence/Lea-146-io-dispatch-2026-10-07`, with
raw successes and failures, exact old/final runtime inventories, benchmark
comparison and exclusions, and source backup verification. The installed
Firespider files remain unchanged. Separate v19 does not contain this parent
dispatcher change. A new host build, complete corrected host suite, packaged
and extracted-runtime checks, and source-rebuild gates remain open before a
new release can be considered verified.
