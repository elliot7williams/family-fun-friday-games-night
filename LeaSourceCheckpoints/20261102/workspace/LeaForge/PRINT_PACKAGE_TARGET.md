# Supported Package Target Investigation

Updated2026-10-28. Experimental test code, NOT an adopted engine backend or a
successful-print claim. Current browser ZIP B5C452C4 and print DLL3C014359 remain
unchanged. The public managed writer's valid-output path is still unverified.

## Purpose And Source

Use the supported Windows document-package target to retain the original COM
target and its terminal notifications before testing actual virtual output.
Direct StartXpsPrintJob was rejected as an adoption path because Microsoft marks
it unsupported. These interfaces are documented for Windows8+ desktop apps:
[factory](https://learn.microsoft.com/en-us/windows/win32/api/documenttarget/nf-documenttarget-iprintdocumentpackagetargetfactory-createdocumentpackagetargetforprintjob),
[status event](https://learn.microsoft.com/en-us/windows/win32/api/documenttarget/nn-documenttarget-iprintdocumentpackagestatusevent),
[status payload](https://learn.microsoft.com/en-us/windows/win32/api/documenttarget/ns-documenttarget-printdocumentpackagestatus).

- Lea.Tests/PrintPackageAbi.cpp compiles against installed SDK10.0.16299.0 and
  prints native GUIDs, status/message sizes, offsets and completion constants.
- Lea.Tests/PrintPackageLease.cs has typed factory/target/event bindings, an
  inert bounded4MiB output IStream, original-target cancellation and gated release.
- Lea.Build/Test-PrintPackageCancel.ps1 retains a normal-user NoExit64-bit STA
  owner before creation; the original target/event, not a job ID/PID lookup,
  proves completion. Unknown creation/retirement keeps its original lease alive.
- Lea.Build/Read-OwnedJobMembers.cs reads compiler job membership through its
  retained SafeHandle. Process-name snapshots are diagnostic, never kill authority.

## Verified Evidence

V4 result continuity-evidence/print-package-cancel-v4/result.json SHA256
71E2BA9C404CD22E0BA6CC0470C7A5A6D9A5A6C3DC2BF9743D7FEA6D7E8A0473.
Native SDK and managed bindings match all11 ABI fields: factory CLSID348EF17D,
factory interfaceD2959BF7, target1B8EFEC4, eventED90C8AD, status24bytes,
completion/HRESULT offsets16/20, MSG48bytes, completion constants1/2/3.
Three deliberately wrong ABI bindings are rejected.

Actual local Microsoft XPS Document Writer v4 / PORTPROMPT: verified before
creation; no network/physical/default fallback. Empty target created47ms,
cancel+notification phase total63ms from creation start, complete cancellation/
disconnect/release62ms within5000ms retirement. Original event Completion2,
HRESULT0, output0bytes, CurrentPageTotal0. JobId0 is observed, NOT used as ownership
or absence-of-job proof. Default-printer snapshots match; no unexplained native
child, no retained process/package leases; owner16536 retired0 guarded.

SDK compiler3854ms/120s and ABI executable684ms/30s pass through the unchanged
owned launch primitive; original exits0/Job0/output, no timeout/kill, max224ms/5s
retirement. Inputs unchanged. Parent runtime10.0.14, x64 PowerShell STA, not the
browser's .NET Framework4.8 runtime. Native RPC budgets are cooperative observation
and measured retirement, NOT preemptive/hard real-time cancellation guarantees.

V3 initial supported-target cancellation also passes46E73C48FF311478C8A3F46F93E5777D0EDA3B94FBB186D7AD0E26F30311E5E0:
creation125ms, total148ms, cancel phase16ms, no output/children. V4 adds explicit
driver/port/default snapshots and includes disconnect/release in measured budget.
One V4 launch stopped before output creation/API calls at600MiB disk reserve;
owner62288 retired0, free space later recovered without attribution. A new launch
occurred only after verified reserve recovery; no test failure retried unchanged.

## Compiler Failures And Remedy

V1 8252E228 / V2 D16505A2 produced the native executable and compiler exit0 but
needed owned-job termination afterward, so both are FAILED qualifications. V2's
inverse-verified read-only observer identifies vctip.exe in the retained compiler
job. Installed file metadata identifies it as the VC tools experience uploader.
No engine print target was created in either attempt.

V3 used a NEW private47,274,827-byte/56-file copy of installed compiler tools,
excluding only optional vctip.exe. Every copied byte matched the installed tool;
no installed file, registry/privacy setting, compiler flag or isolation rule was
modified. Original compiler120s/ABI30s/retirement5s gates remain intact. V3 and V4
both have natural exits/Job0/no termination. V4 reuses that verified private kit,
not another copy. Manifest SHA256
A7BF4C74CBD4EAC8E4D6099835B9B9520E1DDA59CCB952CD8C8DE14420EE4D19.
Private tools are not engine dependencies or a publicly redistributed SDK.

## Remaining Work

CURRENT2026-10-28 .NET4.8 native-bridge failure boundary v4 passes19 checks,
receipt8BC457809303BFE4264EBBEB1600C82283AC95956825347F4107F01F66CDA6F3.
Adds null output/target/input, zero/oversized length, unrelated COM object, and
a public managed COM fixture with SDK target IID that refuses GetPackageTarget.
Confirms exact E_POINTER/E_INVALIDARG/E_NOINTERFACE/E_ACCESSDENIED and zero page
counts, one request to refused target and no implicit Cancel/type enumeration.
Fixture uses actual native DLL/COM ABI but creates NO Windows package target/job.
Original target lifetime/cancel-under-load/driver faults remain unproven.
Compiler633ms/120s/run703ms/30s, exit0/Job0/drained output/no timeout/kill,
max176ms retirement, inputs unchanged, owner79816 retired0. Intermediate v3
17-check receipt98F70D4B60644042B6373ADB1861096EB79A83CFDD0B4777B96E9E3B975F66AD
retained; owner19249 retired0. No browser/backend or printer preference changes.
Local source/evidence checkpoint print-fault-source-20261028.zip verified143
entries/990126bytes, SHA256
47A23681E7A321EE641014911F751B50CC8FBAEF8B09D3C0CEE8C0EEF6993FE8.
Includes error fixture/runner and v3/v4 evidence with preceding source/render/
submission evidence. Local supplement only; this index postdates archive.

CURRENT2026-10-28 safety boundary follow-up: PrinterSafety captures the current
user's default through GetDefaultPrinterW plus read-only LegacyDefaultPrinterMode,
rejects unknown/automatic/invalid modes and missing names, detects name/preference
changes. PackageLease.Start applies it before activating a transfer-enabled target;
runner also checks before native activation and records API before/after snapshots.
CIM diagnostic checks remain conservative; prior failed safety receipt unchanged.
Read-only18-check PS7 test E57AEE11B9DED3A7A01107B0C3C8446A8AA8F5CEBA817BFA10DE6FFC5C8897E2
passes, HP default/mode0 unchanged. Predicates/API only; no target created.

.NET4.8 x64 safety v2 receipt
A53A235E095E691D454545E8A1B8D098E916911A5285ADBC6F7D8BDE23FE901B
passes11 checks: mode rejection/acceptance, change detection,24-byte ABI, through-
EOF stream copy and original v5 bridge load/unload in STA without target creation.
Compile554ms/120s, run673ms/30s, original exits0/Job0/output/no timeout/kill,
max133ms retirement, unchanged inputs, owner25853 retired0. .NET4.8 v1 compilation
failed on STATSTG ambiguity; explicit ComTypes alias fixes it, failure preserved.
PS7 predicate18 proof predates only that alias addition. No actual .NET4.8 native
target lifecycle/submission or browser integration proven. TOCTOU/external user
changes remain possible; capture/checks do not lock global printer preferences.
Do not submit again on this automatic-managed profile or alter preference for a
green result. Next qualify fault handling and host parity under safe test setup.
Local checkpoint print-safety-source-20261028.zip verified119 entries/939072bytes,
SHA2561BBBD417757F439BC7C6672AF2E79B61FC7501AEBAD9C082FCE63BFC5CA511BB.
Includes current source, guarded runner, .NET4.8 failed/pass evidence and prior
submission/render evidence. Local supplement only; this index postdates ZIP.

CURRENT2026-10-28: native v5 actual virtual submission completed3 pages, HRESULT0,
46ms creation/1899ms submission/66ms terminal-disconnect-release;85131 output
bytes SHA FEF0644FEB569C8E4C3AB847248B986B630077DAF96266084255A0F775C3476D.
However overall qualification FAILED: default changed from
HP06D9BB (HP ENVY4520 series; actual display includes space: HP ENVY 4520 series)
to Microsoft XPS Document Writer. Receipt SHA256
F7DDA9DBBBA0AE6375B628942A7EB8EC0D26D9ED7629F93847E42D3F2644270B.
Owner18331 retired0, original target released only after Completed; no unknown
children/leases, inputs unchanged, default NOT restored without user approval.
User subsequently authorized restoration: SetDefaultPrinterW succeeded, initial
CIM-only check still reported XPS (failure receipt retained). Direct current-user
GetDefaultPrinterW and registry confirm HP restored, preference mode0 unchanged.
Readback SHA D0408FCEF62791F5096B717A8C43BA0DC0134AB6979E57CDF368269DB9CC9F1D.
No physical job submitted; this does not prove the HP hardware printer works.
Registry read LegacyDefaultPrinterMode0 is consistent with automatic management;
Microsoft documents last-used printer selection when Windows manages defaults:
https://support.microsoft.com/en-us/windows/hardware/printer/set-a-default-printer-in-windows
This is a likely explanation, not a proven exclusive cause. Current runner now
blocks document submission when auto-management is enabled/unknown; never changes
that preference automatically. No further print tests or production adoption.

Independent output-readback audit2982CFE8D6CFB530597C8DD93FD407BA1C065A54BDE40D040323DC8ABB948E06
passes3 page dimensions/132 ordered glyph texts/one embedded font byte hash.
No visual/layout equivalence or fault recovery proof. All native compiler/ABI
jobs exited0/Job0/drained output with original limits, no timeout/kill; maximum
208ms retirement. Source v5 snapshots remain exact in its evidence folder;
later auto-management preflight guard is not covered by that run.
Next verify guard without submitting another job, use direct current-user
default API rather than CIM alone, then qualify cancellation/faults and actual
.NET4.8 host parity.

Independent .NET4.8 WPF output renderer v4 receipt SHA256
4C4EC912834C2AE43B560F7EF53DADE75A2A31E53D116831177DA238AC2F57FC
renders original/returned3 pages at816x1056, requires body ink (not just headers),
identical per-page body ink counts45755/46538/29071;31/29/34 differing pixels.
Returned page1 visually inspected with46 body lines/header/footer. NOT exact
pixel equivalence, printer/fault/host integration or default-safety qualification.
Compiler627ms/120s, render2955ms/30s, Job0/output/exit0/no kill/timeout, maximum
223ms retirement; unchanged inputs, owner7662 retired0. No print calls.
V1 refused by existing PowerShell execution policy (unchanged); V2 compile failed
duplicate default/reference assemblies; V3 first render exposed rounding-noise
width difference and unflushed WPF layout (headers only). V4 uses explicit .NET4.8
references, rounds dimension noise to6 decimals, measures/arranges/drains layout
before rasterization and checks body content. Failed receipts/PNGs retained.
Local source/evidence checkpoint print-render-source-20261028.zip verified95
entries/892605bytes, SHA256
7932D42905E427ECEFFDDCC1931DD3DCE119BCFAD53D343125C7230F30F63D58.
Includes source, restoration evidence, all four render attempts and PNGs; not a
complete SDK or off-device backup. This index postdates archived documentation.
Local source/evidence checkpoint print-submission-source-20261028.zip verified47
entries/648755bytes, SHA256
DC69766D830383A3A19E56D822B823DBF6741B47D9A48F7C1DE96E7B09220E6D.
Includes tested v5 snapshots and current guarded source, output and failed safety
receipt; local only, no complete SDK/off-device claim. This index postdates ZIP.

2026-10-28 native transfer bridge added in Lea.Tests/PrintPackageTransfer.cpp.
LeaWriteXps uses SDK COM declarations and retains the caller's original target;
it prepares all pages before acquiring the package writer, preserves document/
page tickets, structure, story and thumbnail resources, rejects sequence-level
tickets, signatures/restricted fonts instead of silently dropping them, and
requires caller-owned cancellation/terminal notification on failure. Input is
bounded4MiB/256 pages and TRUSTED Lea fixture only, not an untrusted importer.
The initial preflight below did not exercise submission; v5 above now does.

Native preflight v1 receipt SHA256
6C54038B79C22B19BEDFB39C164ADF1295E061036869544D1C7700870EAF5A88
passes one document/three prepared-ticket.xps pages and3 invalid-input checks.
Compile6998ms/120s, probe606ms/30s, original exits0/empty jobs/drained output,
no timeout/termination, inputs unchanged; owner25723 retired0. Private compiler
kit reused with56-file hash verification; no installed tools modified.
No target, default printer or browser involved. The executable's standalone
factory validates parsing, not target-supplied factory or writer behavior.
That preflight led to v5 DLL/target integration above. Faults/cancellation,
rendered output and .NET4.8 parity remain unqualified.
Local source checkpoint print-transfer-source-20261028.zip verified23 entries/
364908bytes, SHA256
7A48CC399F6B44BA4B5BD6E679FC074652728E74409132397DFAEEA45109191A.
Includes bridge, stream/tests, owned launch helpers, fixture and v1 evidence;
not a complete SDK or off-device backup. This archive index postdates its contents.

2026-10-28 stream preparation follow-up: BoundedStream now rejects input larger
than4MiB, copies borrowed input, exposes detached Snapshot bytes and clears native
read/write count outputs before operations. Test-PrintPackageStream.ps1 passed16
managed checks on PS7 (source SHA256
7795E15FCA498391198E1D69C4E6DDC96044B59F0F6CAB1BFC79D345C57B2BAA).
Follow-up CopyTo repair permits requests beyond EOF without lifting the4MiB
storage cap, reports separate consumed/accepted byte counts, rejects self-copy
and preserves counts on short writes. Expanded managed test passed26 checks,
exit0, source SHA256
829790392870744F305C880C5685D2C29E37BD4155B15D5163A8E468BEE9470B.
No native target or child process was created by this test. V4 cancellation proof
belongs to the earlier source revision, not this changed source. COM-marshaled CopyTo/Clone,
actual output and native lifecycle requalification remain open. Current browser
and production print library are unchanged.

V4 sent no pages; v5 above sent an actual document. COM stream clone support and
partial writes remain unqualified. Rendered output, cancel-under-load/faults, creation-to-event subscription gap,
RPC stalls/crash recovery, .NET Framework parity, other printers/architectures/OS
versions, browser adoption and full release remain open. The prototype may retain
an unknown target indefinitely; that is diagnostic safety, not production recovery.

Next resolve the default-printer safeguard failure, verify rendered virtual
output and fault cleanup, then validate the actual .NET4.8 host before
any backend adoption. Do not send fixtures to the default hardware printer or
replace a release gate with the simpler empty-cancel case.

Local checkpoint print-package-probe-source-20261028.zip verified56 files/
505214bytes, SHA2564736E18D1523582A766777BD28ADC1BCCC488E86C05E68D19E2AABA93A3C53C0.
Source/recipes/ABI binaries/pass and failed receipts retained; private compiler
binaries excluded (manifest retained), not complete SDK or off-device backup.
