# Isolated JavaScript Experiment

Milestone 143, October 3, 2026. Broader browser readiness remains about 65%.
This is an optional scripting transport, not a full browser DOM/event loop or
automatic remote-script integration. Lea still owns parsing, style, layout,
painting and the page model. See [Lea.Script](../Lea.Script/README.md) for the
compact `Jp/Ex/ExA/St` API, permission rules, isolation checks and limits.

## Verified

- Locked dependency restore and Release solution build succeed. The 18 build
  warnings are the existing nullable/obsolete warnings in older experiments;
  the script projects build without warnings.
- All 117 script checks pass from source output and from an extracted package:
  original trusted-interpreter behavior, protocol bounds/schema/ordering,
  invalid capabilities/handles, independent sessions, retained fields,
  detached nodes, literal writes, permission generations, hard deadline,
  worker memory pressure, cancellation, recovery and ACL cleanup.
- The seven focused WinForms checks keep a timer running during asynchronous
  RPC, reject reentry/disposal during execution, preserve owner-thread DOM
  access, and handle live permission revocation and cancellation.
- Full core/native-edit regressions and focused image-worker regressions pass
  after the shared native launcher changes.
- A separate Firespider EXE copy, using the existing host executable with the
  latest compiled Lea.Engine/Lea.Win/Lea.Image files, passes all six startup
  cases: clean, restored, locked, oversized, lazy-background and lazy-all.
  Each remains responsive, shows no detected error dialog and loads no MSHTML;
  lazy-all successfully resumes. This proves the tested startup paths, not
  every host feature or performance parity. Startup readiness observations
  on this run were 4.9..6.6 seconds; these are diagnostic observations, not a
  controlled benchmark.
- The core assembly references only mscorlib, System and System.Core. Jint and
  Acornima remain optional module/worker dependencies.
- Running the worker from FAT32 fails closed before page-script execution.
  The ZIP may be stored on F:, but its runtime must be extracted to NTFS or
  another local ACL-capable volume. Runtime-file ACLs do not grant folder
  access; no unconfined fallback is used.

## Artifacts

The separate test package is:

```text
F:\Lea-js-143-isolated_2026-10-03_06-39-13.zip
SHA256 4DE8EB0E2D50B978233F51A204150255FACFA6F35C398CE14C3F58C8700FA892
```

All 33 extracted files matched the manifest and archive inventory. The audit
copy that passed the complete test suite is:

```text
C:\Users\Elliot\AppData\Local\Temp\Lea-js-143-package-audit_2026-10-03_06-39-13
```

This package is a test EXE, not an updated installed browser. Its 18 runtime
files total 7,122,034 bytes (about 6.79 MiB), including both the trusted parent
runtime and the separate worker's dependencies. Per-file GZip estimation
totals 2,699,756 bytes; the complete ZIP is 2,686,336 bytes. Compression reduces
distribution size, not interpreter execution time. A persistent worker avoids
repeating CLR/process startup after successful calls.

The installed `L:\Firespider Internet` EXE and engine/view DLL hashes remain:

```text
EXE  1C7FA3D7683AB0CE86B0F9BF93B1B4D0495E63A2BC8C508351AC2B77135516AE
Core BBD8871F18D5A29A73C6BBF3F5B04CE3EA6B24D06399856DEB896A1E963F321B
View 44B3E32F6375B6D02F6DDDD5CE96BC89A7B32255AADD076FC52B089CCA93AE8E
```

## Next Work

Connect the optional session to native view invalidation, navigation/disposal
lifetimes, and DOM event dispatch. Firespider's .NET Framework 4.6.1 host needs
a deliberate 4.6.2-or-later migration before referencing this module. External
scripts, browser event-loop semantics, DOM creation, fetch/origin policies,
storage, and wider standards/site compatibility remain incomplete. Keep remote
script execution disabled until those policies and integration tests exist.
