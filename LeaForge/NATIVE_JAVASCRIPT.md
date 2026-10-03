# Native JavaScript Test Browser

Milestone 144, October 3, 2026. The scoped Lea 1.0 baseline is complete;
broader lightweight-browser readiness remains about 65%, a planning estimate,
not a standards conformance score. This milestone does not finish a
general-purpose modern-web browser or replace the installed Firespider build.

Lea owns HTML parsing, styles, layout, painting, fields and the document tree.
The optional `Lea.Script.Win` adapter attaches an isolated JavaScript session
to the native view. `Lea.Script.Browser.exe` is a separate test browser, not a
Chromium/WebView wrapper. See [the scripting README](../Lea.Script/README.md)
for embedding, short aliases, security boundaries and supported behavior.

## Verified

- Locked restore and Release solution build succeed. The initial solution
  build has 18 existing warnings in older experiments and no errors; the final
  scripting browser/test builds have no warnings or errors.
- All 294 script checks pass from source output and the extracted package.
  They include 128 event checks across trusted and isolated transports and
  45 native-view checks covering initialization, image-aware load, native
  editing, canceled defaults, checkbox preactivation, retained fields,
  queue bounds/order, page retirement, recovery and provider disposal.
- Full core and native-edit regressions pass. The core assembly references
  only mscorlib, System and System.Core, not the optional interpreter.
- The extracted browser smoke passes with its unchanged 2,000 ms execution
  budget: startup, keyboard counter activation, isolated worker, memory limit,
  no loopback exemption and a rendered screenshot. Module inspection detects
  no MSHTML, Chromium, WebKit or WebView2. Desktop/narrow native-view image
  checks are nonblank.
- All six real staged Firespider EXE startup cases pass: clean, restored,
  locked, oversized, lazy-background and lazy-all. The staged copy uses the
  older host EXE with the newest engine/view/image files. It stays responsive,
  has no detected error dialog and loads no MSHTML; lazy-all resumes. Observed
  readiness is 4.2..6.3 seconds, diagnostic observations, not a benchmark.
- The expanded Boot harness passes 24 of 42 cases, then fails at `split`:
  the older host still creates legacy browser controls in split view. The
  remaining 18 cases are not verified in this run. Startup success must not
  be presented as complete host migration or proof that every error is fixed.

## Artifacts

The final separately packaged candidate is:

```text
F:\Lea-js-144-native_2026-10-03_14-28-32.zip
Bytes 2946831
SHA256 4E867AD54CDB750CFF41B8062CCA0BE68546391C9EAD695F058443B947D3C9FD
```

All 40 extracted files match the archive inventory and manifest hashes/lengths.
The extracted copy that passes all 294 checks and the browser smoke is:

```text
C:\Users\Elliot\AppData\Local\Temp\Lea-js-144-package-audit_2026-10-03_14-28-32
```

Run `Lea.Script.Browser.exe` from that complete directory, or build the browser
project in Release and use its `bin\Release\net462` output. Extract the ZIP
to a local NTFS/ACL-capable volume; F: is FAT32 and is distribution storage
only. Worker launch from FAT32 fails closed. Earlier candidates remain
separate and are not the verified final artifact above.

The 23 binary/config files total 7,920,283 bytes (about 7.55 MiB), including
both test/browser executables and parent/worker dependencies. The separate
icon asset is 370,070 bytes; fixtures, README, licenses and manifest are also
required package content. Per-file GZip estimation for the 23 binary/config
files totals 2,892,922 bytes. The complete distribution ZIP is about 2.81 MiB.
The required small engine/view/image subset totals 739,328 bytes, excluding
optional scripting, icons and documents. Compression reduces distribution
size; these numbers do not prove faster execution or lower working memory.

The installed `L:\Firespider Internet` build remains unchanged. Its EXE,
engine and view SHA256 values are:

```text
EXE  1C7FA3D7683AB0CE86B0F9BF93B1B4D0495E63A2BC8C508351AC2B77135516AE
Core BBD8871F18D5A29A73C6BBF3F5B04CE3EA6B24D06399856DEB896A1E963F321B
View 44B3E32F6375B6D02F6DDDD5CE96BC89A7B32255AADD076FC52B089CCA93AE8E
```

## Remaining Work

The optional adapter runs local file/about documents only and excludes UNC
files. Remote-page scripts, external scripts/modules, a complete DOM/CSSOM,
general browser event loop, storage, fetch/XHR and wider event families are
not implemented. Listener exceptions currently fail dispatch and reset the
realm, rather than standard browser exception reporting/continuation. This
is a bounded subset, not full standards or production untrusted-site safety.

Functional event/view fixtures use the supported 5,000 ms execution budget.
Cooperative-limit assertions retain strict result checks after function
warmup; the independent 25 ms native-regex deadline tests forcible termination.
Cold calls under competing compilation have hit deadlines. The default-budget
browser smoke passes, but neither test establishes a latency SLA or performance
parity. Controlled cold/warm benchmarks and startup optimization remain work.

Firespider's .NET Framework 4.6.1 host needs a deliberate 4.6.2-or-later
migration, a rebuilt separate candidate and the complete host test matrix
before this adapter is adopted. Keep the installed browser and older source
backups intact while that integration continues.
