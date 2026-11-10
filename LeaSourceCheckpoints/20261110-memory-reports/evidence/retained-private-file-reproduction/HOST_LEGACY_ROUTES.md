# Remaining Host Routes

2026-11-10 source audit of the exact105input current Firespider build closure,
including87preserved host members. No source/binary/installed browser changed.
Recipe: Lea.Build/Audit-CurrentHostLegacyRoutes.ps1.

Evidence: I:/LeaBuildStorage/current-host-legacy-routes-20261110-v1/audit.json,
SHA256CC6DD691B357BA7263682B494E253F702F19EBBCCBBAB393759E8D10D6DDEB6C.
All105input hashes matched the build manifest. XML project inventory identifies
27compiled VB files. Lexical index records522legacy references and64menu bindings;
these counts are not counts of reachable bugs or completed migrations.

## Confirmed Source Gaps

- BrowserFeatures2.vb:10 binds Accessibility Inspector click/hover to q870/q871.
  q870 at517 casts the selected control to WebBrowser at519. Lea V is not that
  type. The handler catches/logs exceptions; no q870 replacement appears in
  FirespiderLea.vb. Clicking behavior and log outcome still need a runtime test.
- BrowserFeatures2.vb:8 binds Archive Current Page Locally to q872/q873. q872
  at570 uses the same cast at572, then legacy DocumentText/body and archive
  settings. Preserve HTML/text/metadata and private-mode policy when migrating;
  simply swallowing the cast or opening an inspector is not equivalent.
- FirespiderLea.vb:47 creates New Legacy Tab, binds qleaLegacyNewClick at53,
  and its handler at566 invokes q3(qntu()). This remains an explicit old-engine
  path. Removing the menu alone does not prove all creation factories migrated.

## Next Implementation

2026-11-10 report logic now implemented in Lea.Firespider/AccessibilityReport.vb,
as a partial-class method fragment like PageColor.vb. Uses current Lea P/N DOM,
iterative traversal and bottom-up link text/image summaries,65536entry bound,
null/repeated-node rejection and escaped report values. Retains the prior six
summary categories, potential-issue calculation and manual checks. This is a
DOM heuristic, not an accessibility conformance audit or computed-visible-text
equivalence proof (hidden/script text and associated-label semantics need review).

Owned VB compile/model run PASS10checks against exact11B154A1 engine. Receipt
I:/LeaBuildStorage/accessibility-report-model-20261110-v1/result.json,
SHA25642BF9138448B13E5849176297E9A0BE500E4727766BE7846418B53652E538618.
Compile1684ms/retire167, model1240ms/retire41, original60s/30s and5scleanup;
both exit0/Job0/drained/no timeout/kill, inputs unchanged, owner76386 exit0.
Model fixtures cover counts, descendant links, escaping, manual checks and
null/cycle guards. Source/driver/runner hash-copied beside receipt. Model-only
test on I: uses no image/script workers; it does not qualify sandbox execution
on exFAT. No UI/host route wiring yet, no installed browser or SDK changes.
New code/evidence locally preserved, not yet GitHub readback verified.

Fresh host candidate now wires q870 inside its existing Try block to
qleaaccessroute(). AccessibilityRoute.vb uses active Lea host, unavailable/busy/
error guards and qleaNew for report tabs. Legacy selected tabs return to the
original handler. Report file/status output follows the old workflow; private
temporary-file retention is explicitly unresolved, not a privacy guarantee.
Prepare-AccessibilityHostSource.ps1 copies/validates105baseline inputs; only
BrowserFeatures2.vb and appended FirespiderLea.vb partial class differ. Original
route inverse and adapter prefix verified. Source receiptB9CD62FD under
I:/LeaBuildStorage/accessibility-host-source-20261110-v1.

Build-AccessibilityHost.ps1 PASS: receipt4FE81C3234D9D6CCBDBBA6AF3BCA3D1B4AD21E2CC7CB77CDD43C90BB75AB5806,
9161ms/retire198ms,120s/5s,exit0/Job0/drained/inputs stable,22products. Owner21796
guarded exit0. New host61986B5044C5D0FED6B77C6895386813184177E4B5DB9F76C8FC5DA40FF90A1A
on I:/LeaBuildStorage/accessibility-host-build-20261110-v1/bin is compiled only;
do not launch sandbox workers from exFAT or transfer9513runtime qualifications.
Fresh NTFS UI v2 passes bounded route checks. No installed adoption.

UI receipt8CB24FF244CAD1D714AC9AD6D64CE5BF5675588B0A04703E9169227A95ADE53B,
audit3F2BF8F2B191AC478137C8582218BB7E7E68147CA7E0B8A45D97AC81C6FCFFC1
on I:/LeaBuildStorage/accessibility-host-ui-audit-20261110-v2. Compile4461ms/
retire242,UI18993/retire27,60s/90s and5scleanup,exit0/Job0/drained,401runtime
bytes+ACL/input stable,owner89937 guarded exit0. Actual click, hover off/on,
summary/escaping, empty-tab guard and no MSHTML during report case pass. Original
navigation checks unchanged afterward intentionally create legacy tabs later;
not a whole-run no-legacy claim. PNG viewed: heading/escaped source title and
zero-issue summary visible, not pixel/DPI fidelity qualification.

V1receipt546877F6 failed because test used V.Q(query), not At.X(document text).
Generated HTML inspected and correct; same expected output/limits/host bytes,
fresh v2 after test correction. V1source/probe/logs retained on I: under
accessibility-host-ui-failure-20261110-v1;exit1/no timeout/kill/401bytes+ACL stable,
owner19736 exit0. Private report retention/loading/disposed/paused/split remain
untested at v2; no inherited full15/print/package qualification from9513.

## State Tests And Privacy Defect

V3receiptB1C72E3C3AD9587511057D2ED6F667310629B133B0AC5FBC40E61633EDA1BC81,
audit0B7890ED09FF0A94150EDEC0EF46B60DFC45B3181048301158049F92CF9DBF9F
on I:/LeaBuildStorage/accessibility-host-ui-audit-20261110-v3. Compile3986ms/
retire241,UI21852/retire16,60s/90s and5scleanup,exit0/Job0/drained,401runtime
bytes+ACL/input stable,owner20005 exit0. Same61986B50host. Gated local HTTP load
does not produce a report; suspended tab remains the same placeholder without
resumption; no selected tab produces no report. Click/hover/navigation checks
retained. Disposed/split behavior remains untested.

Private-view probe reproduces a privacy defect: report tab is Lea/private, but
after disposal the generated HTML still exists and contains the source URL.
private-report-retention.json explicitly records privacyFixed=false. Diagnostic
reproduction is not privacy qualification; only synthetic fixture data was used.
Do not promote this host on that basis.

Next implement an origin-isolated in-memory HTML report route, preserving report
content/navigation policy without writing plaintext report files. Keep remote
scripting restrictions, resource/document limits, navigation cancellation and
lifecycle behavior intact; no reflective page replacement. Current local reader
uses FileShare.Read, so delete-on-close would conflict with it. Do not broaden
access or claim ad hoc file cleanup fully resolves privacy. New candidates need
model/security/UI/private/disposal/split and matched qualification.

Use a fresh source candidate copied from the exact preserved host closure;
never edit retained build inputs or overwrite the live L: project. Start with
Accessibility Inspector and Archive Current Page, preserving their existing
outputs/behavior on Lea DOM. Add owned runtime tests for click/hover, empty,
loading/disposed/paused/split tabs and privacy; prove no legacy control appears.
Then audit remaining menu factories, About, UIA and lifecycle/recovery routes.
Qualify changed host bytes before publishing another development package.

This inventory is not a runtime reproduction, full reachability proof or gate
closure. All six final release gates remain open. Existing9513/ZIP81CB qualification
and historical failure evidence remain unchanged. New audit is locally backed up;
it is not claimed to be in the earlier365file GitHub checkpoint.
