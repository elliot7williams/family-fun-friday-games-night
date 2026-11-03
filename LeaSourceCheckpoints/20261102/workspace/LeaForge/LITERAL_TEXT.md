# Literal Text Rendering

Later ordered-flow work corrects the observed heading/label order and adds top
viewport coverage; see ORDERED_FLOW.md for the new pair's evidence and failed
decoder-deadline regression gate. Results below remain tied to their exact core.

Candidate work 2026-10-25, not a released browser change.

Native visual inspection found that E261 painted `R&D` as `RD` with a mnemonic
underline, and removed the ampersands from literal `&copy;` / `&NotEqualTilde;`.
DOM decoding was correct; WinForms prefix handling was altering rendered text.
[TextFormatFlags.NoPrefix](https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.textformatflags?view=netframework-4.8.1)
disables that mnemonic interpretation for measurement and painting.

Six exact inverse-checked substitutions are applied to the preserved mature V.cs:
one wrapped measurement, four single-line measurements (grid fit, text width and
automation word geometry), and opaque PaintTextCore. Spaced and translucent paths
already preserve literal ampersands. Root V.cs carries the same scoped changes;
it does not replace the mature source used for compilation.

`Build-LiteralTextView.ps1` compiles all six preserved view sources against named
core9208520E. New view SHA256:
`9D9C7A3937B59206514D5631164A442744D9F5728CC43952AD643A86A5EB4867`.
The underlying archived view source is already independently verified equivalent
to E261 (comparison990EB454...,1314methods/1262bodies, no IL/API differences).
That equivalence does not claim equivalence for this intentional rendering change.

`NamedVisuals.cs` checks literal painting pixel-for-pixel against GDI NoPrefix and
checks widths for `R&D`, `&copy;`, `A&&B`, `&Z` and trailing ampersands. Old E261
fails the first paint contract. New view passes35checks including decoded DOM,
native field activation, field geometry, editing and identity across980/420px
resizes. Four native PNGs are saved and nonblank-pixel checked. Receipt:
`named-reference-visuals-v4/result.json`, SHA256
`168AA864BF848B97C75DAEDC72E1F595D231CB5121732F556840D64693C7DD14`.
All inputs/runtime bytes and ACLs held, no timeout/kill; owner33564 exited0 with
zero leases/unverifiedfalse. Old failure controls are retained, not green runs.

Earlier V1 failed because the probe expected a native editor before activation;
V2 adds public Kf/Ka activation and passes25checks but image inspection exposes the
paint bug. V3 stopped before compilation due a single-row PowerShell array shape;
V4 corrects that shape and adds the unchanged paint/width contracts. All evidence
remains retained. No production deadline or security change.

Assistant review `visual-review.json`, SHA256
`E500257D4FEE0B8F5190AE2A8DEB40B4BC11C149C4EEA81E488ECB107C56B7C5`,
confirms visible ampersands in all four reviewed images but does NOT approve full
visual behavior. Remaining observations:

- Native fields heading appears after both field labels, contrary to authored order.
- Combining relation sample appears missing-glyph-like; fallback needs verification.
- Images are scrolled to the activated field, so title/currency area is not covered.

Exact view source, probe, fixture, local recipes, four PNGs, review and nested
named-core-source archive are backed up in `literal-text-source.zip`,22entries,
385516bytes, SHA256
`483D3D7332E7C56974100FED98B300DC19850D6ECA2BFBEF84A9DA761E6D4A71`.
All entries verified. Local source preservation, not off-device backup, complete
browser sources or a portable toolchain/build kit.

Broader retained engine/view behavior passes127444ms/exit0/no timeout/kill under
the unchanged300sdeadline. Receipt `literal-text-regression-v1/result.json`, SHA256
`5955288E6E8507594FDB52D0A6F4AB7A0C0A80D91EDC4BB1C1C91B71D3566CE5`.
Inputs/runtime bytes held; owner36744 exited0/zero leases/unverifiedfalse.
Raw aggregate remains failed for three private fixture grants. Read-only exact
classification `acl-observation.json`, SHA256
`E40F74D7163BEEC6F1D8C281525F54EE6410D9016D5425B221DF5B811DBA642B`,
confirms one explicit fixed-decoder SID RX+Synchronize ACE on each of Image,
Handshake and testEXE, reconstructing baseline descriptors in memory. No filesystem
ACL or policy change by classifier; no profile deletion/cleanup certification.
Earlier15host/3320script passes bind old viewE261, not this new9D9C view.
Host/scripting requalification and the above visual problems remain open before
coherent delivery or adoption.
