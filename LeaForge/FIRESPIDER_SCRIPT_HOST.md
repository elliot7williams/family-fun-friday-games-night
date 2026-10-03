# Firespider Script Host Candidate

Milestone 145, October 3, 2026. The scoped Lea 1.0 baseline is complete;
broader lightweight-browser readiness remains about 65%, a planning estimate,
not a standards conformance score. This candidate is separate from the installed
Firespider browser and is not a finished general-purpose modern-web engine.

## Changes

The reproducible candidate builder copies declared live VB project inputs and
resources into a new folder, overlays the clean Lea adapters, migrates only
that copied project/configuration to .NET Framework 4.8, and compiles it.
The copied project is usable in Visual Studio with 4.8 targeting support.
Original input and final overlay SHA-256 values are recorded separately.
Unused historic Flash/Director references are removed only after a guarded
compiled-source check; framework-provided assemblies are not duplicated.

Optional isolated scripting attaches to ordinary/resumed tabs and both split
panes. Pure JavaScript state prevents automatic hibernation even without DOM
writes. Closing, navigation and split-window disposal retire the appropriate
session. Bounded errors appear in the host's status bar. Split comparison
controls refresh their idle state after script completion even when scroll
synchronization is disabled; the regression was reproduced in the earlier
candidate before testing the fix.

Script-free pages remain dormant and start no interpreter process. Classic
inline blocks, supported authored handlers and explicit `JsA` code opt in;
template contents remain inert. Later supplied code establishes the requested
ready/load lifecycle. Invalid event types and foreign nodes are rejected even
while dormant. The core now loads `about:blank` internally on both navigation
paths rather than sending it to the HTTP transport.

See [candidate build, package and test instructions](../Lea.Firespider/CANDIDATE.md)
and [scripting boundaries and aliases](../Lea.Script/README.md).

## Verification

- Release solution build succeeds with 18 existing warnings in older
  experiments and no errors. Scripting browser/test builds have no warnings
  or errors.
- All 305 source and extracted-package scripting checks pass, including dormant/supplied lifecycle,
  handler-only pages, inert templates and dormant event validation.
- The full core suite and focused internal blank-document check pass.
  Blank URLs retain query/fragment, bypass the HTTP cache, reload, and obey
  navigation policy. Unsupported `about` URLs remain rejected.
- All 67 copied original live inputs and 25 final enabled-candidate overlays
  match their source manifest. The installed EXE/engine/view hashes remain
  unchanged.
- All 43 final enabled-host Boot cases pass, including scripted tab/split
  isolation, error recovery, disposal and the split-comparison idle regression.
  Both main and split screenshots are nonblank and were visually inspected;
  the split comparison button is enabled with scroll sync off.
- All six actual final EXE startup scenarios pass: clean, restored, locked,
  oversized, lazy-background and lazy-all. The windows remain responsive,
  no error dialog is detected, no MSHTML loads and lazy-all resumes. Observed
  readiness is 4.2..8.2 seconds, diagnostic timing, not a benchmark.
- The copied-project framework guard rejects optional scripting on 4.6.1
  with the expected migration error; it does not rebuild or alter the runtime.
- The `-NoScript` candidate compiles without warnings/errors and contains no
  interpreter/adapter/worker files or optional assembly references. Its clean,
  split, hibernation and lazy-all Boot cases pass; its full 42-case matrix is
  not repeated in this run.
- All five distribution archives pass complete inventory, SHA-256 and length
  audits: 370 enabled runtime files, 104 enabled host-source files, 341 small
  runtime files, 76 small host-source files and 40 standalone script files.
- The extracted enabled host-source ZIP rebuilds independently without
  warnings/errors. Its saved project property retains optional scripting
  without a command-line override. This verifies buildability, not
  bit-identical compiler output.
- The extracted enabled runtime passes the scripted-host Boot case and the
  actual EXE clean-startup scenario. The
  extracted standalone browser passes inline startup/native counter activation
  with its unchanged 2,000 ms default, verified isolation and no legacy browser
  modules. Its rendered screenshot was visually inspected and is nonblank.

Complete Lea and original live Firespider source backups are produced with
`Backup-Lea.ps1` and `Backup-Firespider.ps1`; the latter now retains the required
`My Project\app.manifest`. The buildable migrated host source is the separate
104-file archive listed below. The source-only archives and Git history retain
the Lea implementation as well as these host integration changes.

## Artifacts

The verified enabled candidate EXE is:

```text
C:\Users\Elliot\Documents\Family Fun Friday Games Night copy\dist\Firespider\Lea-145-2026-10-03-v2\runtime\Firespider Internet 19.exe
SHA256 0A4E8F97A32925D751A991912C7B64BCC005B6C7B9B643D638FE12D5DB80FE0E
```

The small candidate is in `dist\Firespider\Lea-145-small-2026-10-03\runtime`:

```text
SHA256 438B67CA807FE1455F6049AB7A150BF58241AFD56B80754E7DB0A8AD5DE273E9
```

Distribution archives on F: are:

```text
F:\Firespider-Lea-145-script-runtime_2026-10-03_16-43-34.zip
Bytes 3678689; files 370
SHA256 05A32C80028861695E49D20CA67DF36F0E20F06A3267DF4D1BF373C567172783

F:\Firespider-Lea-145-script-host-source_2026-10-03_16-43-34.zip
Bytes 3335072; files 104
SHA256 06F65743035390C6D0E8DFF4C6AA8CE1E2805E3E2B391B1B9A3357FCE906A5A6

F:\Firespider-Lea-145-small-runtime_2026-10-03_16-43-48.zip
Bytes 1101676; files 341
SHA256 CDB349F767D589B94F5F61D2356BA49F6AC37ECEA53AF3A434E457B21BFA575B

F:\Firespider-Lea-145-small-host-source_2026-10-03_16-43-48.zip
Bytes 772156; files 76
SHA256 ECEA4B569DA087012CC9FD3036BFE1F12FF65494B43271F4C438C4386EC5C6FC

F:\Lea-js-145-native_2026-10-03_16-43-58.zip
Bytes 2948444; files 40
SHA256 1FF6F2243F6BF70425D68D3AE8D72443A51ECD689D844DE0A421A16FBA642793
```

The standalone package's 23 binary/config files total 7,924,891 bytes.
The required small engine/view/image subset totals 739,840 bytes, excluding
optional scripting, host EXE, fixtures and icons. Shared host/standalone
dependency hashes match. Earlier failed/first candidates are preserved for
diagnosis and are not the final v2 artifact above.

The installed EXE/engine/view remain at these original SHA-256 values:

```text
EXE    1C7FA3D7683AB0CE86B0F9BF93B1B4D0495E63A2BC8C508351AC2B77135516AE
Engine BBD8871F18D5A29A73C6BBF3F5B04CE3EA6B24D06399856DEB896A1E963F321B
View   44B3E32F6375B6D02F6DDDD5CE96BC89A7B32255AADD076FC52B089CCA93AE8E
```

## Limits

The enabled host uses a 5,000 ms execution ceiling; the optional adapter and
standalone browser keep their 2,000 ms default. These are limits, not observed
latency or performance guarantees. The host harness runs under a real WinForms
message loop and awaits script calls on their owning UI context. Earlier
off-message-loop harness failures were corrected, not used as engine timing
claims.

HTTP/HTTPS/UNC scripts remain refused without starting a worker. External and
module scripts, broader DOM/CSSOM APIs, general event-loop semantics, arbitrary
remote-site safety and modern-site compatibility remain unfinished. Explicit
legacy fallback and advanced legacy-only commands remain in Firespider.
This candidate does not make the entire application Lea-only.

Worker launch requires local NTFS/ACL-capable storage. F: stores ZIPs only;
extract to a supported local volume before running. Compression reduces
distribution size, not proof of faster execution or lower working memory.
The host-source ZIP contains compiled Lea dependencies for offline host
builds; retain the complete Lea source backup separately.
