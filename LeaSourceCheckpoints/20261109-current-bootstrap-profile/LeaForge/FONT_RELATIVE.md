# Relative Font Size Increment

## Net48 Candidate, 2026-11-08

2026-11-09 follow-up on exact C5CC/4EE7: fresh prior model/render regressions
PASS26shorthand/12initial/142initial-render/142relative-render. Receipts under
continuity-evidence/font-net48-prior-*-v1/result.json respectively have hashes
580F2300FBC44D135CBF2FC4996C8A924EF9952D1651664DA840186DE1DE96D7,
C161F3B08D373EAC53A5869A2386A200158AEB3E0DD058A1D39FA65B5060A7E8,
5A2800ABE77AF4971EDE3340CFDDB40E042E87042411A03D31BBCBD19E3E1071,
4E4C92E2C4A3710940BDBB58A837369ED7413DB619C24E8BCE8B63A75E5B7AE7.
All eight owned runs clean, stable inputs, no owner attention. Current total464
is independently verified, superseding the earlier142-only boundary below.
Fresh full-suite compile A3E1DDC2 PASS on prepared assembly F647679D; full native
scripting qualification, print and Firespider qualification still pending.
The fresh v5 stage591FBFFC/auditE01AC763 passed. Full v5 F099EC00 FAILED in
phase8 computed script: bootstrap TimeoutException, last stage events. Child
exit1/12785ms; retirement190ms/Job0/drained/no outer timeout or kill. All65runtime
byte/SDDL and input hashes stable, trace restored, owner retired. Independent
audit68075D65696BE154CA3F74A75D12C4982395C6A5E1A0DA5E294F331D8ACD055E
at I:/LeaBuildStorage/font-net48-full-failure-20261109-v1 retains raw logs.
Not a proven font regression or metadata fix; no changed binaries adopted.

Production SDK baseline core/view declare .NETFramework v4.8; the earlier staged
13076D91/65B87DAE pair did not. Net48TargetFramework.cs and explicit opt-in staged
builder switches now restore those declarations without touching frozen source,
the prototype projects or previously tested binaries. Test-FontShorthand
-Net48Metadata uses separate outputs and verifies the exact target declaration
in both images for the visual mode. This is SDK metadata consistency, NOT a
proven explanation or fix for the full-suite timeout.

New core C5CC0B99CFA54C090C0E553B2C6080186E4E412BD254A533307860D5522C67E0
passes28relative model checks; receipt
continuity-evidence/font-net48-relative-candidate-v1/result.json SHA
E1A27B9A2ACCCAC6820393FEEBBD6145911D30E9ECF50459DE28E9AB5B4FC46A.
Matched view4EE7E01AF6F24A87BF3B4BE873B895539108C8792A281962F54AF4A3D03DF344
passes114signed rendering/selection checks at720/360; receipt
font-net48-signed-render-v1/result.json SHA
952D427EBE4CC04653F22C23833A7D1845BC323AF931ED5181A9D72236181337.
All six owned compile/test runs clean; unchanged120s/30s/5s budgets and input
hashes stable at execution. Core build26760ms/test1268; view24277/test6117ms.
Metadata read without loading assemblies: audit9E7EF0EA on I: startup investigation.
Exact staged source/binaries/receipts126files copied/hash verified to
I:/LeaBuildStorage/font-net48-candidate-20261108-v1. This142check scope does not
inherit464older checks. Prior regressions, fresh fullscript, print/host/package
qualification remain pending; no installed adoption or release gate closure.

Diagnostic12-session old/new/new/old startup pilot tested the PREVIOUS pair,
not this metadata-corrected pair. PASS receipt69440043; sample auditE46D8B52:
baseline mean1798.83ms/median1840, candidate mean2058/median1420.5. Large spread,
six samples per side, no speed or broad reliability conclusion. All5owned jobs
clean,43runtime bytes/ACL unchanged at execution. Np1or2 both denote controlled
network denial; Mg15 and other isolation/default budget checks retained. Harness
compile/API error88AB8C32 and strict-denial-code failure74E1E3F6 preserved.
Completed diagnostic C: stages archived to I: with hash/SDDL locators; restore
before replay. Full-v4 quarantine remains untouched. Detailed history:
history/signed-font-20261108-history.md and PROJECT_CONTEXT.md.

## Signed Relative Sizes: Focused Verification, 2026-11-08

### Full Scripting Boundary

All60historical test sources recovered by SHA; new entry fixture compiled and
audited. Five-change305member assemblyFE74A59B on I: retains182fixtures; compiler
PASS9823ms/retire143, receipt7CB5CAF1. New247file stage6129AD4C/resource audit
513A0491 passed for exact13076D91/65B87DAE pair plus406C/F61B scripting pair.
Full owned v4 FAILED300074ms during phase70(VmOptions), phases0..69 complete,
no3412footer. Receipt6A1DAC82, original300s/5s; exit57005/kill requested, Job0/
drained/retire1967, inputs/65runtime bytes stable. Image-decoder ACL delta and
missing trace-restoration proof mean stage QUARANTINED; retain without repair
or unchanged rerun. Independent audit0B9F49C4 on I: copies raw evidence and verifies
boundaries. Distributed timing increases versus one historical pass do not prove
cause or a functional regression. Next fresh startup/runtime timing diagnostics,
then full qualification before print/host/adoption. Earlier464checks remain
bounded model/render proof, not full native/browser or release proof.

### Prior Rendering Regressions

Exact same13076D91/65B87DAE pair now passes142prior shorthand/initial render
checks (font-signed-prior-initial-render-v2/result.json SHA
F4AA7DA69A274161DC6516D5674A87456BFEEA5023D283204BDD6203253D00A8)
and142unsigned-relative render checks (font-signed-prior-relative-render-v1/
result.json SHA3C099DD7098B5663460D24502F3BE5716F6A8BABC2BA6C98E08D0396220FEFFC).
Total464bounded model/render checks; both720/360px,40PNGs per regression.
Representative relative-order-720-short-selected.png inspected. All4owned runs
clean, input hashes unchanged at execution, original120s/30s/5s limits;
test8959/8718ms. Explicit -ReuseSignedRenderPair pins exact view and core proofs.

Initial-render-v1 timed out30024ms near final fixture, exit57005/kill requested,
Job0/drained/input stable; retained receiptA84A87F6C4CD7114D1B6EADD083FB177E79E43A0AAC7127942058149CB492E91.
Harness now uses fullARGB LockBits row copies instead of repeated GetPixel calls.
No assertions/thresholds/deadlines removed or relaxed; parity verifies24/32bit
images, alpha, padded rows, equal pixels and a last-pixel mismatch. All39PNGs
retained from timeout match the new output SHA256. This is test-harness work,
not improved engine speed or erased failure history.

Fullscript/print/host remain pending. Fullscript staging needs a newly compiled
entry fixture because its embedded resource hardcodes an absolute fixture path;
copying the old stage alone is not a fresh isolated qualification. Preserve that
resource check and existing process ownership/deadline/ACL checks.

Candidate core13076D911C0710DCF9848AB81BA456BBA7F5DD7560CD18F6DEDDD302E2BA5011
now passes28relative model checks,26prior shorthand checks and12initial checks.
Receipts in continuity-evidence: font-relative-candidate-v3/result.json SHA
5D2C8AFF82277B9BA3D1C1BC982DCA7A30DAAE29DA1E861634E3879F4EAB6502;
font-signed-prior-shorthand-v1/result.json SHA
519729904FDEC7F4F8B58982127DD2DDA7DA32C821B2C9FBA14A1C27FBCAFB06;
font-signed-prior-initial-v1/result.json SHA
08094ACCB0B0BE3294D6B0B33FEAD07960DDC88860F036D2868E1D0535039BFB.

Matched view65B87DAEA42728F35A5040760AC9CD05635FF9D0C28F5FD24820294BB8EE9772
passes114 signed em/percentage shorthand/longhand render/selection checks at
720/360px, compared to30px equivalents. Full wrapped text, nonblank ink and
selection, line/selection geometry, exact plain/selected pixels;32PNGs retained.
Representative signed-long-percent-360-short-selected.png inspected. Receipt
font-signed-prior-signed-render-v1/result.json SHA
C9E9A4CC286BDFE60407F31D29ABA14528E056B1F44837F54DDED4AF7D85CEF5.
View compile23282ms/test22596ms, original120s/30s/5s limits. All10owned runs
across these four receipts exit0, Job0, drained, no timeout/kill; input hashes
unchanged at execution and all owner shells retired. Total180 focused checks.

Test-FontShorthand -ReuseSignedRelativeCore explicitly pins this proof/core for
prior model checks or signed-render mode, without changing old baseline paths.
Initial storage preflight failure below is superseded for these focused checks.
Still pending: prior rendering regression, matched script/print/host campaign,
delivery adoption and broad standards/site compatibility. No all-CSS/DPI/speed
claim; current browser package remains unchanged.

CSS numbers permit a leading plus sign ([CSS Values](https://www.w3.org/TR/css-values/#numbers)).
New tests cover shorthand/longhand +1.5em/+150%, +.5em and uppercase units.
Four additional controls cover doubled/separated/mixed signs and invalid
line-height atomic rejection; the prior12cases/6controls are retained.
Unchanged parser75059879 built from frozen mature sources reproduces six new
failures, with all prior positives and10controls passing. Receipt
continuity-evidence/font-relative-candidate-v2/result.json SHA
54F93DE5168243C5667B462F4CBC79F67B1AEBBFB730E37D8161BB14B917D653.
All runs exit/Job0/drained; negative test exit1 requested kill, so this is not
positive cleanup proof. Input bytes unchanged, ownerAttention false.

FontShorthand.cs now permits one leading+ and invariant AllowLeadingSign parsing.
The first candidate-v3 attempt refused on600MiB storage preflight, before
output/leases; the later successful run above supersedes it. Source SHA04EEE4E4...;
test9E50B996.../fixture43543DB4.... Current packaged browser remains unchanged.
Next prior rendering, scripting and actual-host qualification. Zero, negative,
out-of-range and scientific-notation sizes remain outside this increment.
This is not full CSS numeric/font conformance or release gate closure.

2026-10-31 E7F49/4DA8/CA51/0C16 developmentdelivery verified:
I:\LeaBuildStorage\Firespider-Lea-relative-font-development-20261031.zip,
7508693bytes,SHA5203EC96A59E31D0F0F6CACA237209A6C8AAEE7BD7B880F6190225DD32C4FE5F.
Readback399runtime+2metadata;manifestpinscurrentI:sourcev14checkpoint andhistorical
sourcearchives. Runtime-only,notcompleteSDK/GitHubupload. SameZIPverifiedonL:\Lea Builds.
FreshNTFSextraction62C9DAEB... passes3/3settings/navigation/actualEXEclean,
all0/no timeout/kill/Job0/drain,maxretire137ms/401bytes+ACLstable,inputsstable,
owner29831retired0. Not all15extracted/cleanPC/release proof. Supersedes pending
package/extraction notes below. Old1474package unchanged;all6finalgatesOPEN.

2026-10-31 currentpair now has actualFirespiderhostqualification:
hostE7F49DB9D59A6C73D92A3B40CA2101C61005C8A5B1BDB583D24946D6B8A5EF67,
core4DA8/viewCA51/print0C16/exact8.0.33. Focusedprint/preview45249244... passes
with actualhandlers/nativecancel/owner/reentry/changedtabguards;build12825/
13252ms,behavior13801ms. Own15stagecampaign
D9EEB35C3B9AB747640505EF07CBD03BB6D27FBD491F66F253E44F76D3D72A3B
passes allworkflow+6actualEXEstartups under original30/90/100s+5sbudgets.
All0/no timeout/kill/Job0/drain,maxretire151ms,399bytes+ACLstable,inputsstable;
owners2614/6832guardedretired0. New optionalhostmode andinversecheckedcampaign
keep old paths/evidence. Supersedes missinghost notes below,not releaseproof.
Next newpackage/freshNTFSextraction. OldD10DBpackageunchanged/all6gatesOPEN.

2026-10-31 matchedprinting now PASS on4DA8/CA51 withprintlibrary
0C16B3D0113A91F83274F37D48C4D7B5F9BAC8DC1E292FBBBF318BF908711CFB.
relative-font-print-project-v1 receipt
A71A3BC94E9584876DCE5480027EBAD4585A2900E6F01651C65D4869FA2D8002:
directMSBuild/priorstyledcapture/range37/device/dialog/ticketchecks pass.
relative-font-image-live-v1 receipt
41EB69CB05327FF8FE6C4226543EBE2ED75B8662E3DBB8B709B528B19D4BF1CA:
40baseline/46friendly/250live overlappingchecks pass,realdecoder,13246ms,
all0/no timeout/kill/Job0/drain/inputstable/decoderACLrestored. No printjob or
browserrouting/physicaloutputqualification. Owners83416/37879guardedretired0.
New optional modes preserve old paths/pins and requirefullshutdown/imagecoverage.
Supersedes missingcurrentprint notes below,not fullnative/host/releaseproof.
Next currentpair Firespiderrebuild+focused+15stagecampaign/package/extraction.

2026-10-31 fullmatched scripting now PASS on4DA8/CA51,not a startupfix:
relative-font-script-owned-run-v1 receipt
7F1448D51CF16421602D7D02DDC58F066BDAF5DB9E22A8E8B3728C525B5BA205.
3412checks/all89phases,296694ms within300000 (3306msmargin),cleanup1294ms
within5000,exit0/no timeout/kill/Job0/drain/65bytes+ACLstable/inputsunchanged/
trace restored. Independent outcomeaudit verifies89orderedpairs/footer/count/
currenthash+ACL. Owner50853guardedretired0. Firstactualrelativeexecution;
earlier74885rejectedbeforelaunchonstorage. C:recoveredwithoutcleanup,no attributed
growthcause. Timingmargin is small,not reliability proof. Prior142renderchecks
also pass onEXACTpair (88CD0656...). Supersedes missingpriorrender/fullscript
notes below. NEXT matchedprint/host beforeadoption;packaged1474/D10DBunchanged.

2026-10-31 current4DA8core +matchedview
CA51FC818B119529EA8F7104D0753C5779070A7CCF0AD157675A564C4755C584
passes142relative rendering checks at720/360 (font-relative-render-v1),receipt
5FCA5A4730ECA3F2894533665FC22CAA9BA520FA49A2B71622B8B45D295E03A0.
Fivepairs:shorthandem/percent,longhandem/percent,inheritedbasis/order+style+lineheight.
Parent20px/expected30px ensures notdefaultfallback. Completewrappedtext,nonblankink,
selectionpayload/geometry/highlight,exactplain+selectedpixelpairs checked;40PNGs.
Representative360orderselected and720percentplain inspected. Run24370ms under
unchanged30s,build20770ms,all0/no timeout/kill/Job0/drain,inputsunchanged;
owner24056guardedretired0. Not independentDPI/unitfidelity proof.

Unchanged1474 baseline with samefixtures failsfirst text-equivalence comparison
(wrapping differs),receipt76904E40D7A31DC573E3CF726CDC7D1F915B69CD3628C8A27DED43799F1AFD2F.
Exit1,killRequestedTRUE,Job0/drain/inputsunchanged,owner36852retired0. This is
negativebehavior evidence,NOT normalcleanup proof. No positive gate weakened.
Next prior-font render regression on same4DA8/CA51pair,then matchedscript/print/
hostqualification. PackagedD10DB/1474 untouched. Below missingrender note is
superseded ONLY for these142relative comparisons;all6finalgates OPEN.

2026-10-31 staged core
4DA8BF1968218A97AF3EC053DDE0E22EEB218669EF1F3B4BCFEA3A39F8ABDFBA
adds positive bounded `em` and percentage sizes to `font` and `font-size`.
Shared TryRelativeFontSize resolves against inherited Fs (16px default medium),
not earlier declarations on the same element. Decimal parsing is invariant;
case-insensitive units and leading decimals supported. Shorthand remains atomic.

The staged projection retains frozen mature sources. Build-FontShorthandCore adds
one inverse-checked longhand hook before the existing viewport fallback. Root
prototype E.cs is not replaced; FontShorthand.cs is not automatically adopted by
the root project. Existing pixel and viewport branches remain intact.

## Verification

FontRelativeMain/font-relative.html exercise public E.Go: twelve cases cover
longhand/shorthand em/percent,nested inheritance,order,decimal,uppercase,leading
decimal,default basis and style/lineheight. Six controls preserve prior font state
for negative/zero/oversized/malformed/unit/invalid-lineheight forms.

- Exact1474 baseline:6controls pass/12cases fail. Receipt
  BAF29F804166DA8BEBC3EA8CFD0D6388EF62D85782C3EDF26AB04CE80A3AAEBB.
- font-relative-candidate-v1:all18 pass. Receipt
  D828D1B49F3E9DC6C30E2C8186004834F6B8929172A19D645592AC02B06306DB.
- Same4DA8 DLL original26checks pass:font-shorthand-candidate-v6 receipt
  C238F6A329D008F0BDF9E06D5C01B824587A7F68BE9EEC4322EB968250216275.
- Same4DA8 DLL initial12checks pass:font-initial-candidate-v3 receipt
  7052507D5A680BBE53FEE8AD98CC63DB617467FF66F39D2D971DC9CC0D937598.

Inputs unchanged/ownerAttentionfalse;all original owned runs exited0,no timeout/
kill,Job0/drain. Foreground owners17167/20236/74231/71026 retired0 after guard.

## Boundaries

Lea's integer size model rounds computed values away from zero at midpoints;
only resulting sizes1..199 are accepted. Tokens bounded to four integer/fraction
digits. Zero is valid CSS but unsupported by this renderer model,not an invalid
CSS claim. Negative sizes are invalid. rem,calc,absolute-size keywords,ex/ch,
fractional-pixel fidelity,fullnumeric syntax and complete CSS sizing remain open.
Not a new browser binary or current4DA8 render/script/host qualification. Existing
packaged1474/D10DB build stays unchanged. Next matched view/render comparisons,
then scripting/print/host before adoption. No release gate/percentage change.

Specification basis: https://www.w3.org/TR/css-fonts-3/#font-size-prop
