# Direct JavaScript bridge bindings

Trusted `Js.Start` and isolated worker `Program.Run` construct two Jint
`ClrFunction` values: the private host bridge and the microtask checkpoint.
Previously, overload resolution selected `Engine.SetValue<ClrFunction>` for
each binding. The pinned Jint binary routes that generic overload through
`JsValue.FromObject` and `FromObjectWithType`, which returns an existing
`JsValue` unchanged. This was not evidence of a functional or security defect.

Both startup methods now explicitly upcast these already-created functions to
`JsValue`, selecting `SetValue(string, JsValue)` directly. This avoids the
generic CLR-conversion entry point for these four bindings without changing
the function objects, closures, private bootstrap captures or public aliases.
No dependency, parser, bootstrap, protocol, constraint or native protection
changes are involved. External scripts remain disabled; startup remains
5,000 ms and default page execution remains 2,000 ms.

## Compiled-call regression

`Lea.Build/Test-ScriptBridgeBindings.ps1` uses the installed Microsoft IL
disassembler to inspect the actual two compiled startup methods. Each must
contain exactly two direct `JsValue` calls and no generic `SetValue` call.
The helper requires new report and disassembly paths, records exact runtime
and tool hashes, and writes its result before rejecting a failing contract.
It never executes a worker or treats timing as a correctness check.

```powershell
& 'L:\Lea Development\Lea.Build\Test-ScriptBridgeBindings.ps1' `
    -Runtime 'L:\Lea Development\Lea.Script.Tests\bin\Release\net462' `
    -Report 'H:\Lea Evidence\new-bridge-report.json'
```

The Windows SDK IL disassembler is a developer test prerequisite, not a new
runtime dependency. An alternate installed path can be supplied with
`-Ildasm`. The exact same helper rejects both preceding startup methods:
each has zero direct and two generic calls. Both candidate methods pass.
These controls demonstrate call selection, not a broken preceding browser.

## Runtime checks

Evidence is retained in
`H:\Lea Evidence\Lea-146-direct-bridge-values-2026-10-10`.
The unchanged test executable passes 36 bootstrap scope/privacy checks,
four embedded-resource checks and eight native initial-load failure/recovery
checks. Private helpers stay hidden; retained DOM closures, page-global
collisions, microtasks, string timers and fresh-realm recovery retain their
behavior. The fresh 940x700 native capture is visually inspected.

The single ordinary 2,084-check scripting attempt stops after 468 passing
assertions at active-page test setup. A fresh isolated worker times out at
5,015 ms parent wall, with VM creation as its last recorded checkpoint.
The retained process counter reports 0 ms; this is not an unavailable-counter
substitution or proof that the worker performed no work. The earlier recursion
warmup passes in this attempt, but the later failure leaves the broad gate red.
It is not retried or replaced by the 48 focused successes.

The required renderer smoke passes. The separate observed copied Firespider
script case fails its first supplied script, before any host capture, at
5,018 ms parent wall / 625 ms captured CPU. Its last complete checkpoint is
parsed at worker wall 2,371 ms / CPU 328 ms, without a completed DOM or hello
write checkpoint. Its observer retires with 35 samples from one PID and no
observer/counter/thread errors or dropped requests/samples. Renderer and host
process intervals overlap; this was not a controlled timing comparison, and
no performance conclusion follows from that host run. Both failed gates are
retained. The full host matrix, actual-browser startup scenarios and a new
Firespider release are not verified. Original 67 host inputs and three installed
binaries remain unchanged.

Only the Script DLL and worker change among 19 production records. Their
94,720-byte and 46,592-byte sizes remain unchanged, as does the 747,520-byte
required Engine/Win/Image bundle. No measured speedup or intermittent-startup
fix is claimed. A single successful worker or suite cannot explain preceding
failures. Milestone 146 remains open; broader readiness stays about 65%.
