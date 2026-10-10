# Experimental Worker Bitness

Milestone 146 remains open; broader readiness stays about 65%. This adds an
opt-in `W32` build alias and a separate experimental browser bundle. It does
not replace the normal runtime or installed Firespider, and it is not a
startup-timeout fix or verified release.

## Build Alias

`W32=0` or an omitted property leaves existing defaults alone. `W32=1`
explicitly selects AnyCPU with 32-bit preference for the script worker only:

```powershell
dotnet build Lea.Script.Worker/Lea.Script.Worker.csproj -c Release -p:W32=1
```

The early build guard rejects other values, non-AnyCPU targets and a conflicting
`Prefer32Bit=false` override. Explicit AnyCPU avoids the SDK's temporary default
x86/RID inference before reference resolution. The guard does not alter normal
builds. Seven final build contracts pass: omitted, zero, one, two invalid values,
a conflicting x64 target and a conflicting preference. Omitted/zero builds
match the matched AnyCPU control byte-for-byte; one matches the actual tested
candidate byte-for-byte. Two earlier guard-validation failures remain preserved.

Microsoft documents the .NET Framework bitness behavior in its
[compiler output options](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/output#platformtarget).
PE inspection shows normal `0x1` flags and candidate `0x20003` flags, with
`32BITPREF=1` and `32BITREQ=0` reported for the candidate. This is a worker
process preference, not a 32-bit renderer/host conversion.

## Matched Comparison

The baseline source is commit `73684ddb0dffa6ba6a9fcfb0ddfb09364bb260ac`.
The control and candidate use identical sources, packages and fixed revision
metadata. All seven other worker files match exactly. Both worker EXEs remain
46,592 bytes. All 19 normal production files remain unchanged; the required
renderer bundle stays 747,520 bytes. The candidate is staged separately on C:.

The predeclared comparison runs three opposite-order pairs once, in fresh
NTFS directories, with no external debugger or startup observer. All 27 checks
pass, including first-page execution, default limits, isolation, process exit
and exact ACL retirement. The reported wall/CPU values are handshake readiness,
not total page/browser latency. Peak is `PeakProcessMemoryUsed` from the native
Job information, not a physical-RAM or whole-browser measurement.

| Pair | Order | Normal ready wall/CPU ms | W32 ready wall/CPU ms | Normal / W32 Job peak bytes |
| --- | --- | ---: | ---: | ---: |
| 0 | normal, W32 | 3,141 / 218 | 2,134 / 46 | 32,239,616 / 18,190,336 |
| 1 | W32, normal | 2,482 / 140 | 1,796 / 187 | 32,235,520 / 18,190,336 |
| 2 | normal, W32 | 2,537 / 156 | 1,817 / 125 | 32,215,040 / 18,194,432 |

These samples show roughly 30.7 versus 17.4 MiB Job peak process memory. They
also show lower ready-wall times in this small comparison, but CPU is not
uniformly lower. Fresh directories are not cold OS/CLR caches; the sample size
is small and system load/cache state is uncontrolled. No universal speedup,
tail-latency fix or security equivalence across architectures is established.
The 32-bit address space and applicable ASLR behavior differ from 64-bit.

## Gates And Failures

- Four compiled-resource checks pass; the candidate embeds the exact generated
  bootstrap and remains inspectable by the unchanged parent test executable.
- The declared ordinary 2,084-check suite stops after 328 assertions at isolated
  selector bridge-limit recovery. Exit is 1; startup wall is 5,015 ms, CPU 15 ms,
  launch 71 ms, probe 5 ms. The last checkpoint is evaluate, before parsed.
- All eight native-view load-failure/recovery checks pass. Default page/CPU/memory
  limits remain 2,000 ms / 2,500 ms / 128 MiB, with verified AppContainer,
  network/file denial, mitigation mask 15 and worker retirement.
- The actual standalone browser smoke passes startup, native counter click and
  disposal. Its fresh capture shows `Lea 1`, counter 1, the Lea icon and fields.
- One copied Firespider script case passes the existing 14 clipboard contracts,
  12 observation contracts and script-host checks without an active observer.
  Inline lifecycle, field retention, callbacks, timers, DOM operations, explicit
  recovery, split isolation/disposal and remote-origin refusal are covered.
  Main, narrow-attribute and split captures are inspected. This is not the
  complete 43-case host matrix or a new six-EXE startup validation.

The app-wrapper's first setup attempt accidentally launches an additional
suite through dot-sourced script scope. The identity-checked owned process is
stopped with exit -1; its attempt, logs and setup failure remain preserved and
excluded from verification. The corrected wrapper has a nonexecuting helper
mode and runs each declared app gate once. That interrupted run does not
replace the 328-assertion failed gate or count as a pass.

## Preservation And Decision

Evidence, both matched workers, alias revisions, failed gates, actual results,
captures, inventories and source backup are retained under
`H:/Lea Evidence/Lea-146-worker-bitness-2026-10-10`. The runnable candidate is
`C:/Users/Elliot/AppData/Local/Temp/Lea-146-worker-bitness-2026-10-10/Lea.Script.Browser.exe`.
The archive is labeled experimental; its compressed size is separate from
execution speed. No new framework, package, CLR permission, deadline, retry
policy, browser control or production dependency is introduced.

The compressed bundle is
`H:/Lea Experimental Builds/Lea-js-146-worker32-2026-10-10.zip`,
2,933,052 bytes, SHA256
`AD561D1C5EEAE8E367822DB0E0541F1408ADE59B50F931AFD1EFC9CA46AF1FB5`.
All 38 files verify in the stage, ZIP entry streams and fresh NTFS extraction.
The extracted browser separately passes its smoke with exit zero. That does
not establish an extracted full-suite pass. The embedded experiment notes
precede this archive's size/hash addition, avoiding a self-referential hash.

The original 67 Firespider inputs, three installed binaries and 379-file host
basis remain unchanged. The complete source is backed up and the checkpoint
is pushed to GitHub. The memory result justifies keeping an opt-in experiment,
not adopting it as default. Fresh isolated startup still needs diagnosis,
including parser/loader work before the parsed checkpoint. Earlier failed
normal-runtime and Firespider gates remain retained.
