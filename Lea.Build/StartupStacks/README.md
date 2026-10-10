# Controlled startup call stacks

This optional Windows x64 developer tool inspects only a known Lea script
worker or its controlled stack fixture. It is not shipped with the browser,
referenced by engine projects, or added to the required renderer bundle.
Its .NET 8 project pins Microsoft.Diagnostics.Runtime 4.0.732401 and its
transitive dependencies in `packages.lock.json`. Build outputs and package
caches for the retained experiment live on H:, outside the engine runtime.

The tool checks PID, parent PID, executable path, creation FILETIME and image
SHA-256 through a retained native process handle before capture. Keeping that
handle open prevents PID reuse during the operation. Only the installed x64
desktop CLR DAC is loaded; symbol lookup is disabled. It retains at most 16
managed threads and 32 frames per thread, with 512-character method names.
It collects no heap contents, object values or method arguments, and writes no
memory dump. Reports use create-new semantics and preserve capture errors.

`--sample` uses `DataTarget.CreateSnapshotAndAttach`. The original prototype
passes eight controlled contracts, including a known fixture frame, five
identity rejections, overwrite protection and invalid usage. The native
snapshot request fails with Windows quota error 1816. The corresponding
observed selector flow nevertheless passes its 26 checks. The quota failure
does not prove which quota rejected the snapshot; Job-limit involvement is
an inference, not an established cause.

The final tool also offers explicit `--suspend` inspection through
`DataTarget.AttachToProcess(pid, suspend: true)`. Its eight controlled
contracts pass. This mode holds the original process suspended during stack
inspection and releases it on disposal; it does not create a snapshot clone.
It is deliberately separate from snapshot mode, not an automatic fallback.
Neither mode is a benchmark, production warmup or unobserved release gate.

## Native observation

The one suspended-mode native attempt captures 14 frames from PID 37284,
matched by creation FILETIME to all 33 worker-observer samples. Its main
thread includes `EqualityComparer<Jint.Key>.CreateComparer`, its static
initializer, `Jint.Engine.GlobalDeclarationInstantiation`, `ScriptEvaluation`,
`Evaluate`, and Lea's worker `Run`/`Main`. This is the trusted bootstrap's
declaration path before any page frame, not a sampled page-script loop or
pipe read. The capture is released and all owned test processes retire.

That instrumented native flow fails its first call at startup: parent wall
5,022 ms / CPU 484 ms, last checkpoint parsed at worker wall 1,710 ms / CPU
328 ms. There are zero page mutations and no completed DOM/hello checkpoint.
The observer retires with no errors or dropped requests/samples. The debugger
pause visibly perturbs the run, so this is not evidence that comparer
initialization caused any prior unobserved deadline failure or dominates CPU.
It is a concrete lead for measuring cold declaration/CLR initialization next.

All 19 production records and the ordinary test executable stay byte-identical
to `31e474a`. The earlier 468-assertion full-suite failure and copied Firespider
startup failure still apply. No production fix, speedup or new browser release
is claimed. Milestone 146 remains open; readiness remains about 65%.

Evidence: `H:\Lea Evidence\Lea-146-startup-stacks-2026-10-10` retains the original
snapshot failure and prototype sources/binaries. Its `suspended` subdirectory
retains the distinct successful capture and failed native flow. No failing
native attempt was retried or overwritten.

## Running

Build the standalone project with the .NET 8 SDK and direct its outputs and
intermediates to a separate diagnostic directory. Restore with locked mode;
do not copy its diagnostic dependencies into the browser's runtime folders.
After building, use a new existing proof directory for each phase/mode:

```powershell
& 'L:\Lea Development\Lea.Build\Test-StartupStacks.ps1' `
    -Phase Contract -Mode Suspended -Proof 'H:\new-contract' `
    -Tool 'H:\diagnostic-tool\Lea.StartupStacks.exe'
& 'L:\Lea Development\Lea.Build\Test-StartupStacks.ps1' `
    -Phase Native -Mode Suspended -Proof 'H:\new-native' `
    -Tool 'H:\diagnostic-tool\Lea.StartupStacks.exe'
```

Native mode runs the existing observed selector diagnostic once and attempts
one capture of its first worker at 2,500 ms process age. Tool-launch delay
means actual attachment is later; reports retain request/completion UTC.
An already-retired first worker is not replaced for sampling. Native startup
5,000 ms, default page 2,000 ms, AppContainer, denial probes, memory 128 MiB,
CPU rate 2500, active-process one and mitigation guards remain unchanged.
The wrapper records the native exit separately from capture exit; a successful
capture does not imply a successful browser flow.

Primary references: [ClrMD getting started](https://github.com/microsoft/clrmd/blob/main/doc/GettingStarted.md),
[pinned ClrMD source](https://github.com/microsoft/clrmd/tree/f160f6897f33198170895a60444de38bcb76bcf3),
and [Microsoft error 1816](https://learn.microsoft.com/en-us/windows/win32/debug/system-error-codes--1700-3999-).
