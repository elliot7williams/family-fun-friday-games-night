# Runtime 8.0.33 Upgrade Plan

2026-10-27: user selected a separately validated upgrade, not reinstalling8.0.32.
Status: experimental matched scripting campaign verified; not browser-adopted or
release qualified. Preserve historical candidates.

## Baseline

The live-print coreA85/view568 suite10 compiled and passed binding verification,
but its full-script run stopped at prerequisite checking: exact8.0.32 coreclr and
hostfxr were absent. No scripting tests launched. Only8.0.33 was observed installed.
Evidence: continuity-evidence/temporal-startup-v1/print-live-script-owned-run-v1.
Old passing script receipts do not qualify this pair or a changed runtime.

## Work And Acceptance

1. Inventory8.0.33 x64 launcher, hostfxr, runtime, apphost and reference inputs;
   record versions, provenance, hashes and available build space. Identify every
   active runtimeconfig, worker build recipe, prerequisite check and digest pin.
   Do not mechanically rewrite frozen evidence or unrelated projects.
2. Create separate versioned recipes/candidates with exact8.0.33 runtime pins.
   Preserve rollForward=Disable in qualification candidates so the tested runtime
   is explicit; do not use a global environment override to bypass the old check.
3. Rebuild affected worker/apphost and managed inputs, regenerate manifests and
   binding checks from verified sources, retain old artifacts and failure logs.
   Keep normal/diagnostic workers and Framework/native Jint identities distinct.
4. Run prerequisite negative controls, startup/exit/fault/retirement controls,
   isolation and file-grant checks under original retained process/Job handles.
   Keep deadlines, CPU/memory/capability limits and cleanup reporting unchanged.
5. Run focused and full matched scripting/native suites, then launch a newly
   matched Firespider candidate for startup/navigation and regression checks.
   Preserve failed runs; no unchanged retries merely to obtain a passing result.
6. Package matching source, recipes, notices and EXEs only after required checks
   pass; verify extraction and clean-machine prerequisites. Update progress with
   actual results. No installed/default browser replacement as part of planning.

## Preparation Evidence

Prepare-Runtime8033Configs.ps1 passed: eight config-only projections from suite10,
JSON parsing, exact inverse version-only verification and unchanged-original
readback. Installed launcher/coreclr/hostfxr hashes recorded. Receipt:
continuity-evidence/runtime-8033-configs-v1/result.json SHA256
430D79B9C4800BF19015300D60F5FDE781191EA372F0388393B7DF554C9C4318.
No runtime executed; manifests unchanged; these are NOT a runnable candidate.

Additional version-sensitive inputs found: Build-PreservedWorkerLauncher.ps1
(apphost template and exact config guard), Build-TemporalNativePair.ps1 (compiler
attestation), Test-ComponentProbePrerequisites.ps1 and its synthetic controls,
Restore-WorkerReferencePack.ps1 and Build-ArchivedWorker.ps1 (compile references).
Runtime patch selection does not automatically require replacing compile
references or NativeAOT output: establish the actual deployment/build relationship
first, then document every retained or changed input explicitly.

Versioned prerequisite preparation now passed inverse/syntax verification against
the pinned original helpers. New helper SHA256
7B2FC0FE6EFA950A72DCFFC63F2251AAD37FC266BED76E4492D821643AE6BC42.
All seven unchanged synthetic prerequisite controls pass; actual machine inventory
finds x64 CLI/coreclr/hostfxr8.0.33. This is not runtime/security verification.
Receipt: continuity-evidence/component-host-prerequisites-8033-v1/result.json
SHA256560FE1890E5D373ADC3412336B5609A1B6057CC31EF2AA61210840BB6483F50D.
Old8.0.32 prerequisite helpers remain byte-identical. An initial preparer attempt
stopped before outputs on a PowerShell single-array shape mismatch; corrected
projection preserves explicit edit tuples and passes the original count guards.

Deployment mapping: Build-TemporalNativePair.ps1 uses crossgen2 and keeps managed
IL comparisons; Build-PreservedWorkerLauncher.ps1 stages Worker.dll/Jint/Acornima,
deps/runtimeconfig and apphost. These are ReadyToRun shared-runtime deployments,
not standalone NativeAOT. Worker deps target net8.0 without a8.0.32 runtime pin.
Keep this distinction when deciding whether recompilation is necessary; installed
runtime existence alone does not qualify the existing native images under8.0.33.

Suite10 has five component manifests plus one entry manifest pinned in
source/Lea.Script.Tests/ModernComponents.cs. Entry manifest fixtureFiles and the
entry host's embedded relocated-fixture.json both pin two affected runtimeconfigs.
Therefore regenerate the entry resource/host, six manifests and dispatcher digest
bindings together in the new suite; do not edit configs in an old tested runtime.

Fresh suite11 initializer/builder projections now pass inverse/syntax checks.
Prepare-Runtime8033Suite.ps1 uses Runtime8033Staging.fragment.ps1 to project only
eight pinned configs before target read leases, rebuild five component manifests,
rebind the entry fixture resource/manifest and regenerate all six dispatcher pins.
Worker/parent/adapter/core/view identities and source assertions remain unchanged.
Receipt runtime-8033-suite-preparation-v1/result.json SHA256
9362A5DFEF4FC15EE4A18DBE10EF926755670E621A7AA3A2674998E534C3DB71.
Initializer03451A26; builderC40660BB. Not yet compiled.

Exact production staging fragment passes12 focused controls (eight config staging
and four negative cases: duplicate/path escape/hash/original identity), originals
unchanged. This is file staging only, not engine/runtime/security validation.
Receipt runtime-8033-staging-controls-v1/result.json SHA256
95F4DA18833C40F545164CA8CCDA71D7C7E555BF9584D4F981E3CFDC7E61DF07.

Prepare-Runtime8033Verification.ps1 projects versioned suite11 verifierD0EBB6D6
and owned runner6059390C, inverse/syntax verified. Verifier requires eight exact
8.0.33/Disable configs, unchanged worker identity and existing full binding checks.
Runner retains300s/5000ms ownership/deadline limits and new prerequisite checker.
Receipt runtime-8033-verification-preparation-v1/result.json SHA256
0F174703DA7EEDAEAF35144076C8636A9C77F3B42A67BFFA89C7DAF4402DE69B.

## Suite11 Build And Binding

After verified historical compression restored the original600MiB reserve, fresh
suite11 compiled successfully. Build receipt:
continuity-evidence/temporal-startup-v1/current-native-script-suite-v11/result.json
SHA256A8D581FBF133F59ADF238C5B65C33F4A360CC6971399300F84698A75FDCFA9FA.
Test EXE52F0EFD25DF08450871B6BFE15965D368ACA893DD0E7C996C390BFB310342DA5.
Both owned compiler runs passed11813/17247ms under120s, original exits/Job0/output,
no timeout/kill;131/8ms retirement within5000ms. Inputs unchanged, ownerAttention
false; retained owner61873 retired0 after zero-leases/no-unverified-child guard.

Binding verification passes60 source/65 runtime files, eight exact8.0.33 configs,
six dispatcher digests/inverse and seven embedded fixture pins. Entry IL46methods/
42bodies/token maps/body flags unchanged except original parent-digest projection;
three existing self-tests pass. Receipt binding-verification.json SHA256
149183F5D044E04544831BA39C683D87D45F1E94242F5769CA8F0C36FD49B4B9.
Old suite10 unchanged. No DOTNET environment overrides found; fresh suite11 image
has no existing exact decoder grant. These prechecks are not runtime qualification.

## Matched Full Scripting Campaign

After StartupStudies compression restored the unchanged600MiB test reserve,
suite11 completed3412 checks/all89 phases in293447ms under300000ms. Original
retained process exit0/Job active0/output completion verified, no timeout/kill,
135ms retirement/drain under5000ms. Inputs/runtime bytes unchanged, trace enabled
and environment restored, ownerAttention false. Owner73429 retired0 after guard.
Run: temporal-startup-v1/runtime-8033-script-owned-run-v1/result.json SHA256
B651EC32E7B3EA5ABE2E6616437AF3C858A178732EDC7982A60F3DDC72696DC5.
Process receiptA761DBBC76F221787E4D11F2A0E7D42DFDEFA296E4D90AC74DEA19AC5A0C096F.

Raw passed remainsfalse due one metadata-only decoder grant. Existing unchanged
Test-DecoderAclDelta policy classified only that exact delta with3 negative
controls. No ACL edits/profile deletion/policy broadening. Separate read-only
outcome SHA2568FDBA6D0F5BB19D776C033C33663DC03769215F12D252A7297823B1E42F498D9.
All runtimeconfigs still exact8.0.33/Disable. Audit source691D3304 is an inverse/
syntax-verified projection of the old audit plus exact config checks.

This campaign qualifies its matched functional scope only. Prior intermittent
startup failures, cold/clean-PC reliability, confinement/profile cleanup, all-site
compatibility and final release gates remain open. No performance improvement
claim from a single timing. Actual Firespider/browser ZIP still unchanged.

2026-10-29 location update: this checkpoint was verified and relocated to
L:/Lea-source-backup-archive-20261029-uia/ to recover C: build headroom. Its hash
below is unchanged. Restage with Lea.Build/Restore-UiaSourceBackup.ps1 before
historical package helpers that require the original C: path;STORAGE_CLEANUP.md.

Verified source checkpoint runtime-8033-matched-source-20261027.zip contains263
files/28294937bytes, SHA25684179CBDF6C3674E43C886FCF72B5C048056425B2D029E2CE3DBBE4CD719AEB9.
Every ZIP entry matched the retained original. Includes exact candidate source/
runtime/recipes/receipts plus core/view/print sources; external worker/reference
source archives remain separate. Local checkpoint, not complete portable SDK.

## Actual Firespider Campaign And Package

Fresh host-runtime8033-owned-v1 passes all15 stages: settings, eight host cases
(clean/Lea/navigation/second/split/edit/style-start/script), six actual-EXE scenarios
(clean/restored/locked/oversized/lazy-background/lazy-all). Host script27033ms/90s.
All original30/90/100s budgets/retained handles/Job0/output/no timeout/kill verified;
max318ms/5s drain. Inputs/bytes unchanged, one decoder ACL delta accepted by
unchanged policy. Owner66093 retired0. Receipt SHA256
A1B1A96C9358525A7704BE283B434AD946D4906C47025E1E01D2C768BD0A12CA.
Host FBBD stays source-identical; only coreA85/view568/worker config6346264D
replace the three pinned older inputs. All398 runtime files bound. No print DLL
or browser print-command integration in this campaign, no installed adoption.

New development ZIP: dist/LeaExes/Firespider-Lea-runtime8033-development-20261027.zip,
7466014bytes,398 runtime+two generated notes/manifest, full member readback passed.
SHA25697CC8B8EAC78FC8A07D909786D780C5FC0824F58AA19D1FA2287070DC7A9CC9A.
Requires net48/exact8.0.33 x64, no runtime installer/default registration. Source
references84179CBD andD4F8FB82, runtime-only development archive, not source SDK.

Fresh extraction6D55AF1D804AF0A714C2C0FECA8972575B91C26BD17D2FE8B9B1BDE6A2972C88
passes settings/navigation/actual clean EXE3/3, all400 members held, zero byte/ACL
deltas, original handles/Job0/output/no timeout/kill, max139ms/5s drain. Owner14921
retired0. Not all15 on extraction, clean-PC or reliability/security qualification.
Package/extraction projections B6842B46/D4191273 retain inverse/syntax proofs.

Verified host/delivery source checkpoint runtime-8033-host-source-20261027.zip:
717files/12348489bytes, SHA2569D653C6CAABC9BE15265636709E8D30C2CAB2CC29657D63A825DF2875782BD78.
All entries match retained originals. Includes actual host source/runtime/campaign,
new helper/package/extraction evidence and core/view/print sources. Prior suite11
checkpoint84179CBD retained separately. Local scope, not portable SDK/off-device.
All required compiler/test/backup/move sessions from this work are terminal.

Next: actual browser print integration with own exact-candidate tests, plus
fidelity/resources/ordinary selection and all broader gates.

Microsoft runtime selection reference:
https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet
Disable binds only the specified version; production servicing policy remains a
separate release decision, not a reason to broaden qualification silently.
