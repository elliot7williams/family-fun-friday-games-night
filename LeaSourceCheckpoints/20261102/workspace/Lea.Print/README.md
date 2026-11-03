# Lea.Print

Optional .NET Framework 4.8 print library for the mature independent Lea engine.
Do not resolve references against the earlier root engine prototypes or combine
DLLs from different candidates. Requires Windows and .NET Framework 4.8 targeting
tools. WPF and WinForms are Windows UI frameworks, not another browser engine.

## Build In A Solution

Add `Lea.Print.csproj` as an existing project. Select explicit matching engine and
view paths through MSBuild properties (or your solution's local property sheet):

```powershell
msbuild Lea.Print.csproj /t:Rebuild /p:Configuration=Release /p:EnableWpfPrint=true /p:LeaCorePath="C:\matched-runtime\Lea.Engine.dll" /p:LeaViewPath="C:\matched-runtime\Lea.Win.dll"
```

`LeaCorePath` is required. `EnableWpfPrint=true` includes vector pagination, XPS,
preview and the WinForms preview bridge. `LeaViewPath` includes live-view capture;
omit it for detached snapshots only. Without WPF the project builds the snapshot,
geometry and earlier GDI text primitives only. The command above is the tested
full configuration, not proof of every optional configuration or an IDE session.
Do not ship reference assemblies as application runtime dependencies.

## API And Ownership

`Po` holds immutable options; `Pg.Cp` derives oriented geometry. `Pc.Cs` captures
styled content from an existing view on its UI thread. `Pw.Dc` creates a disposable
print plan; `Pp` adapts it for pagination; `Ph` hosts preview and `Px.Wr` exports
XPS to a caller-owned empty stream. Dispose the preview before its owning plan.
These aliases stay short; public member names retain their existing contracts.

`new Pr(pages, first, last)` selects a one-based inclusive page range within the
captured document. It retains original page numbers/headers, geometry, dispatcher
and plan lifetime; it does not own or reflow the plan. `Px.Rg(range, stream)`
exports that range with the same empty caller-owned stream contract as `Px.Wr`.
Ranges outside the captured document are rejected, not silently clamped.

`Pd.Cp(options, capabilities)` converts Windows printer capabilities into Lea
geometry. Get capabilities for the selected validated ticket; their dimensions
are already oriented, so the bridge does not rotate them again. It intersects
user margins with the device imageable area and rejects missing/inconsistent
geometry rather than assuming borderless Letter paper. It does not query a
queue, open a dialog or send a job. Windows uses 1/96-inch API units; the bridge
converts them to Pg's hundredths of an inch. See Microsoft's
[capabilities units](https://learn.microsoft.com/en-us/dotnet/api/system.printing.printcapabilities.orientedpagemediawidth)
and [oriented canvas schema](https://learn.microsoft.com/en-us/windows/win32/printdocs/pageimageablesize).

`Pj` is a disposable, UI-thread-owned native WinForms printer-selection dialog.
Construct it with a page bound (1..256) and optional caller settings to clone.
`Sh(ownerForm)` requires a live visible enabled owner, returns false on cancel,
and never submits a job. After acceptance, `St` returns a defensive settings
clone; cancellation clears any previous accepted selection. No selection/current
page mode is exposed while ordinary document selection is unfinished. Printer
properties, copies/collation and some-pages controls use the standard Windows
dialog (`UseEXDialog=false`), not the currently unqualified extended path.
Keep it alive until the modal dialog closes; busy disposal/reentry is rejected.

After native acceptance, `Pj.Cp(options)` creates an owning disposable `Pk`.
It converts the accepted DEVMODE using a bounded locked buffer and safe handle,
merges/validates the ticket against the selected queue, preserves copies/collation
or fails, then queries capabilities for that ticket. `Geometry` is authoritative
for the next capture/plan. The accepted driver orientation and grayscale are
honored; Lea margins/scale/header/footer still apply. Ordinary selection,
print-to-file and additional driver scaling are rejected, not silently changed.
User/default queue tickets are not committed. See Microsoft's
[converter validation requirement](https://learn.microsoft.com/en-us/dotnet/api/system.printing.interop.printticketconverter).

`Pk.Rg(pages)` validates exact geometry identity and resolved images before
creating the selected range. `Tk` returns a defensive ticket clone. `Wr(pages)`
is the explicit synchronous submission call with a generic job description and
one-shot attempt guard; it is implemented but its successful-output path is
NOT verified yet. Do not retry the same preparation after uncertain submission.
Dispose the preparation on its UI thread; it owns queue/server handles, not the
caller's plan. `Pj` and `Pk` can have separate lifetimes after preparation.

Images, full CSS fidelity, ordinary document selection, printer options and actual
Firespider physical-print commands remain unfinished. Remote scripting remains
disabled. A print-library build does not qualify a complete browser or SDK.

## Verified Candidate

Current prepared-ticket library SHA256
`3C014359204CEEDD11E8164A28467D46CB3BA83E5DF6A81802C0F3A7D3AF1795`:
own293 prior+37 range+16 geometry+27 native dialog+22 preparation checks pass.
Receipt `continuity-evidence/print-ticket-project-v1/result.json`, SHA256
`39527111558E9FD59C210E8779B6F1B96182ED9E145E44083C750BE70352C6AD`.
Builds2855/1652ms under120s; prior15665ms/30s and new4895ms/30s. Original
exits0/Job0/output, no timeout/kill, max240ms/5s retirement, held inputs unchanged;
owner39653 retired0 guarded. Installed XPS driver conversion/validation/capabilities,
ticket clones, geometry-bound paginator/local XPS export and pre-dispatch guards
verified. No valid queue submission, physical page, network driver/multi-copy
qualification or new Firespider adoption. Other final release gates stay open.

Earlier native-selection-only candidate:

2026-10-28 current selection library SHA256
`3C000641438BE8EAEE512E4084D45EFBA224F1442E418A473901B9502DED89F4`:
own293 prior+37 range+16 geometry+27 dialog checks pass CE2CBA66. Native dialog
was exercised with installed Microsoft XPS Document Writer, accepted/canceled
settings only; no dispatch code is present. Native ownership/disable/restore,
thread/busy/disposal guards, defensive clones and canceled stale-selection
revocation checked. Direct builds1805/2885ms under120s, prior checks13378ms/30s,
new checks3757ms/30s; original exits/Job0/output/no timeout/kill, max198ms/5s
retirement, inputs unchanged. Owner95544 retired0 guarded. Full receipt:
`continuity-evidence/print-dialog-project-v6/result.json`, SHA256
`CE2CBA669E425BBBB1A155A645D6A0DE1B6FAEE69DB015C96FE337215462080F`.

V1-V3 extended-dialog probes timed out at the unchanged30s deadline. V4 standard
dialog passed behavior but required job termination after parent exit and is NOT
accepted as a pass. The MSBuild test EXE preferred32-bit, unlike the current
AnyCPU browser/older probes. Explicit Prefer32Bit=false matches that browser;
V5 cancellation and V6 acceptance tests passed without termination. Which child
caused V4 activity was not established. No32-bit/extended-dialog, arbitrary
driver, physical output or browser-adoption qualification is implied. Earlier
failures and exact source remain retained. Next validated-ticket conversion,
queue dispatch and actual Firespider routing; printer selection alone is not print.

Earlier geometry-only candidate:

Current printer-geometry library SHA256
`1D9BB7C5CD2E33CCCDB989671ACD6F70F6580A553AA138A9E847DFF7A675771B`:
own293 prior+37 range+16 device checks pass on coreA85/view568. Direct project
build4056ms/120s, prior checks16991ms/30s, test build2191ms/120s, range/device
checks1734ms/30s. Original exits0/Job0/output/no timeout/kill, max147ms/5s
retirement, held inputs unchanged; owner26582 retired0 guarded. Device checks
use actual PrintCapabilities objects constructed from synthetic XML (Letter,
landscape, metric/asymmetric/borderless, missing/outside areas and user options).
No installed queue or physical printer validation. Receipt
`continuity-evidence/print-device-project-v1/result.json`, SHA256
`EB16313E392AE0C3A8ED8AD7935DE0C62BEB1EDEFD0EDF0C6D09AF983486970A`.

Earlier range-only candidate (superseded for new device work):

Current range library SHA256
`45E29BEFDB649EE338CE2492B80A05B0EBB09414A8ED9C475B1DD0681DFD2B2C`:
own293 prior checks plus37 new range checks pass on coreA85/view568. Direct
project build3935ms/120s, prior checks16371ms/30s, range test build2228ms/120s,
range checks3628ms/30s. Original exits0/Job0/output/no timeout/kill; held inputs
unchanged, owner16659 retired0 guarded. XPS readback proves selected body and
original page2/3 footers, not a physical printer. Receipt
`continuity-evidence/print-range-project-v2/result.json`, SHA256
`F14D10005B696D0A8F45645701BE651AB0C3A9ADB7C7642166E7605FAB4634B5`.
V1 stopped before compilation on a harness source-pin lookup error; retained.
Only Px refactoring plus Pr/project inclusion differ from prior library sources;
inverse checks verify unchanged surrounding Px/project source. Browser printer
dialog adoption, physical printing and full-browser validation remain pending.

Earlier direct-project candidate (superseded for new range work):

2026-10-27 direct installed MSBuild build: 4120ms/120s, library SHA256
`D313C3D95A9B2B2178D33C847DB42C883F1891316576C27A6C1F31B64AFE3B47`.
Matched core A85B02C6/view568A2639, all13 library C# inputs pinned to the earlier
live-capture source proof. Existing293 print/capture checks passed20420ms/30s;
rendered PNG inspected. Original process exits0, Job active0, complete output,
no timeout/kill, unchanged held inputs. Owner24296 retired0 with ownership guard.
Receipt: `continuity-evidence/print-project-v1/result.json`, SHA256
`B07107DD76037EC3C21246858899F01B49A2611F2B26307B62DE093003E2E841`.

Installed Framework MSBuild4.8 treats ToolsVersion15 as4.0 and emits a toolset
notice, but builds successfully. The DLL differs from the response-file build;
this candidate has its own focused checks, not inherited whole-browser results.
Published Firespider packages and installed/default browsers are unchanged.
