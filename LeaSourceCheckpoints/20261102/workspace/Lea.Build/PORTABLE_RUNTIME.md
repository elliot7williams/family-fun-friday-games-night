# Portable Development Runtime

`Build-TestedRuntimePackage.ps1` packages the exact 398-file candidate qualified
by `host-adapter-rebuilt-v1/result.json`. It checks every recorded file hash and
rejects source reparse points. The package contains runtime files and test
fixtures, not a complete matching source or compiler kit. It is not an installer,
release certification, license clearance, or an update to the installed browser.

The current host configuration targets .NET Framework 4.8. The scripting worker
configuration requires Microsoft.NETCore.App 8.0.32 with roll-forward disabled.
These runtimes are not bundled. Extraction on this development machine does not
prove clean-PC portability. Runtime prerequisites, installation checks and a clean
Windows validation remain delivery requirements; do not loosen the runtime pin
just to make a package start.

Run the read-only prerequisite inventory with PowerShell:

```powershell
pwsh -NoProfile -File Lea.Build/Invoke-LeaPreflight.ps1 -Root '<extracted runtime directory>'
```

It reads the worker PE machine, worker configuration, essential file presence,
Framework registration, and registered x64 .NET runtime/host-resolver files. The
x64 .NET registration is read from the 32-bit registry view; Framework from the
64-bit view. Exit codes: 0 means prerequisites found, 2 means missing/incompatible,
1 means the inspection failed. PowerShell is required for this development tool
and is not bundled. No registry writes, downloads, browser launch or installation
are performed. This inventory is not an integrity check, runtime-load proof or
security verification; use the manifest verifier and browser tests separately.

`Test-RuntimePrerequisites.Tests.ps1` passed 14 checks, including this machine's
exact extracted candidate, while retaining all candidate file hashes. Receipt:
`runtime-prerequisites-v1/result.json`, SHA256
`6FD55C9AE3B5F48E04CDF7A7D3ABE62E2A6CDB6E37A2064F647A27594D9F72F8`.

`Expand-VerifiedRuntime.ps1` checks the externally supplied archive and manifest
hashes before extraction. It rejects duplicate/unsafe paths, unexpected inventory,
oversized contents and changed extracted file hashes. The destination must be new.
Archive hashes establish identity against local evidence, not publisher trust.

`Test-RuntimePackageRejection.ps1` retains six negative-path checks. The generated
`Test-PackagedRuntimeHost.generated.ps1` uses a fresh verified extraction and
runs the existing 15 host/startup checks with unchanged deadlines. Read its exact
result receipt before claiming a pass. Neither these checks nor compression prove
all-site compatibility, cold-start reliability, full security, or faster execution.

On 2026-10-25 the fresh extraction passed all 15 checks on this development
machine: `host-package-extracted-v1/result.json`, SHA256
`0FBA544D311F22A84E046CFE99F97D381AB85D1FE936851CDDFB3833386823BD`.
All test parents exited zero with drained output and no timeout/kill; candidate
bytes and inputs stayed unchanged. The only accepted permission change was the
existing narrowly checked fixed decoder ACE. The original test owner retired
with zero retained leases and no unverified child.

The preserved engine/worker/interpreter/parser/host source archives and their
rebuild receipts remain separate in `continuity-evidence`. A complete portable
source/toolchain kit, full matched regression coverage, and license review remain
required delivery work. Preserve failed receipts and do not publish this package
as a finished release.
