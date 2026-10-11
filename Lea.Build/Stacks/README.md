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

Inspection uses ClrMD's supported suspended attachment and the installed
Windows .NET Framework DAC. A `finally` disposes the attachment to release
suspension, including failed unwinds. No dumps, locals or heap values are
requested; the symbol file locator is disabled. Output is capped at 16 threads, 64 frames per thread and 256
characters per method description. Missing named main-thread frames are a
failure, not a usable stack. This diagnostic pauses the worker and is never an
unobserved startup/performance gate. A five-second watchdog belongs to the
external driver; the owning test must also retain its Job cleanup/deadline.

The earlier passive implementation was unsupported by ClrMD, not merely a
potentially inconsistent coherent snapshot. Its archived records cannot
establish the bootstrap cause. A PSS clone experiment was rejected with error
1816 on the guarded target; no production quotas were relaxed. See
[Microsoft's supported attachment modes](https://github.com/microsoft/clrmd/blob/main/doc/GettingStarted.md).

Build separately with `dotnet build Lea.Build/Stacks/Lea.Stacks.csproj`.
`Lea.Script.Tests -er` is the quiet six-cycle event-recovery test, and `-st`
creates a guarded initialization-wait target for inspector verification only.
Both require a fixture root and a new report path; normal browser startup is
not changed.

`Observe.ps1 -t <test.exe> -f <fixture-root> -i <inspector.exe> -o <new-directory>`
runs the full scripting test once with a separately labeled observer. All paths
must be absolute; executables must have their expected Lea filenames. `-s`
selects a 500..3000 ms sampling threshold (default 2000), measured from CIM's
UTC process creation time, not first sighting. Exact native birth time, parent
and image are checked separately by the inspector. Only the test's own worker
children are considered, at most 32 times, with bounded inspector/test watchdogs
and retained native handles. The external trace variable is cleared/restored;
the fixture can deliberately enable its own tracing. Existing output is
rejected, and failure is recorded without retry or relabeling.

Optional `-w` admits copied workers beneath one explicit staging root. All eight
runtime files must match the base runtime's hashes; bounded ordinary files,
non-reparse directories and retained read leases are required before inspection.

See [observed full-suite diagnostics](../../LeaForge/OBSERVED_FULL_STACKS.md) and
the historical [event recovery diagnostics](../../LeaForge/EVENT_STACKS.md).
