# Native Class-List Paint Contract

2026-10-26. Development verification, not a release qualification.

Receiver diagnostic B892A912 reproduces the same300-sample failure; desktop PNG
A5EAB8B7 is byte-identical to the ClassLists capture. Focused receiver contract
41B2E049 passes89 checks, including all receiver/field/accessibility/isolation
assertions and blank/erased-region controls at940/420px. PNGs inspected. No root
ElementReceivers source exists: changes are scoped mature-source overlays, not
a wholesale prototype replacement. Normal60-source build098EB988 audits pass;
fullV5 fails earlier at tree startup, so these are focused paint results only.

The unchanged full script run V2 exited1 at phase36: the generic paint assertion
requires more than300 nonwhite samples on a4px grid. DiagnosticE008A9D7 retained
that assertion and captured a visibly populated940x749 page with exactly300
samples. The native editor identity, edited value and190px styled width passed
before painting. This demonstrates a false blank-page classification, not a
proved engine-rendering defect or a proved cause attributable solely to title
metadata removal. Original failure and bitmap are retained.

ClassLists.Native and the mature ElementReceivers.Native overlay now call
View.PaintAuthored for their identical attribute fixture. The original shared Paint
method and its threshold remain unchanged for other fixtures. The scoped check
requires no synthetic title box, a primary visible box for each of seven known
authored text regions, and nonwhite ink in every region. Block text may have an
inline representation too; inline-only nested text remains eligible. This is a
pixel/model presence check, not OCR, glyph correctness or complete CSS fidelity.
Native field identity/value/style and narrow editor-bound assertions remain.

Every width also rejects a white bitmap and seven copies with one authored
region erased. ContractV1 wrongly assumed every text had a noninline box;
V2 wrongly assumed all representations were unique. Both failures are retained.
V3 selects a unique primary block when present, otherwise its inline box.

Focused receipt6BF6D0DDDCA50275ED9D832AABA614663AADA0F6AE13381A2470021E3F440A00
passes51checks at940/420px. Both PNGs inspected: heading/icon, authored labels,
native field valueElliot, checkbox and trailing text are legible and contained.
Desktop SHA256A5EAB8B7876CE3C4D08C907A95BED3FDDCF53B10EDD25468257132E367BC00E3;
narrow SHA256CF8D202A4F8561451F964FEA37A6AAEF49107FA64DB5BF2491D0DEE67F2155CE.
Inputs/bytes held; raw audit retains one decoder ACL delta. Foreground owner
retired exit0 with no leases/unverified child. No ACL reset, isolation bypass,
runtime substitution or production engine change. Full suite still requires
a fresh private candidate; focused results cannot certify remaining phases.

Fresh60-source/65-file full-suite build0200372E and binding4DE2A4D4 pass; new
testEXE52F241C4. Full runV3 receipt23E8711E exits1 after68307ms at earlier phase28:
native removal inline initialization worker startup timeout, wall5012ms under
unchanged5000ms. Trace reaches startup evaluate, phasehello. No outer timeout/
kill; inputs/bytes held, one decoder ACL delta retained, owner retired. It does
not reach the changed classList phase. No full-suite or startup reliability pass.
