# Lea Script Worker

The normal worker build is unchanged. `W32` is an optional, three-character
MSBuild alias for the experimental 32-bit-preferred worker:

```powershell
dotnet build Lea.Script.Worker/Lea.Script.Worker.csproj -c Release -p:W32=1
```

Omit `W32`, or use `W32=0`, to leave the existing build defaults alone.
`W32=1` selects AnyCPU with `Prefer32Bit=true`; invalid values, non-AnyCPU
targets and conflicting preference overrides are rejected before build setup.
The option affects only the worker, not the host/browser architecture, bridge
protocol, deadlines, permissions or runtime dependencies.

In three paired samples the candidate has a smaller measured Job peak process
memory footprint, but its broader scripting suite still times out during
fresh-realm startup. It is not the default, a verified release or a startup fix.
The worker must be copied with its seven matching runtime/configuration files.
Compression reduces archive size, not necessarily execution time.

See [worker bitness evidence](../LeaForge/WORKER_BITNESS.md) for exact samples,
failed gates and limitations. Diagnostics must support the target architecture.
