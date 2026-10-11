# Lea.Stacks

Optional Windows test diagnostic, outside Lea.sln and every engine/browser
runtime package. Requires the installed .NET 8 runtime. It pins Microsoft's
[ClrMD 3.1.512801](https://www.nuget.org/packages/Microsoft.Diagnostics.Runtime/3.1.512801)
in a separate project/lock file; Lea.Engine, Lea.Win and Lea.Script gain no
dependency on it.

The five arguments are worker PID, expected test parent PID, exact UTC creation
ticks, exact worker executable path, and a new absolute JSON report path.
Only a live image named Lea.Script.Worker.exe is accepted. The native identity
handle stays retained during inspection to prevent a recycled PID from being
silently accepted. Mismatched identities are rejected before CLR attachment.

Inspection uses passive attachment without suspension and an explicit local
Windows .NET Framework DAC. It does not request dumps, locals, heap values,
source files or a symbol server. Output is capped at 16 threads, 64 frames per
thread and 256 characters per method description. The external driver kills
an inspector that exceeds its five-second watchdog. A live unsuspended read
can be inconsistent or fail if the worker exits; this is not a coherent dump
or a release/startup/performance test.

Build separately with `dotnet build Lea.Build/Stacks/Lea.Stacks.csproj`.
`Lea.Script.Tests -er` is the quiet six-cycle event-recovery test, and `-st`
creates a guarded initialization-wait target for inspector verification only.
Both require a fixture root and a new report path; normal browser startup is
not changed. See [event recovery stack diagnostics](../../LeaForge/EVENT_STACKS.md).
