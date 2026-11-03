# HTML Named References

Later visual work found and fixed literal ampersand painting in a separate view
candidate9D9C; see LITERAL_TEXT.md for exact new-view evidence and remaining issues.
Host/scripting passes below remain tied to E261, not transferred to that new view.

Candidate 2026-10-25; not adopted into a released Firespider build.

`Hc.D` now performs one-pass numeric and named reference decoding. `Hn.T` uses
ordinal binary searches over a generated sorted table, tries longest matches
first and avoids substring allocation during lookup. Attribute parsing passes an
explicit context flag so a missing-semicolon match followed by ASCII alphanumeric
or equals remains literal. Text/RCDATA use the text rule; script/style remain raw.
Names are case-sensitive. Supplementary and two-codepoint replacements are kept.
Decoded output is never fed back into reference decoding.

Data comes from [WHATWG entities.json](https://html.spec.whatwg.org/entities.json),
downloaded 2026-10-25, SHA256
`D741D877AC77C4194C4AD526B5B4A19AEF8DFE411AB840A466891CDBB9F362E6`.
Rules: [named character reference state](https://html.spec.whatwg.org/multipage/parsing.html#named-character-reference-state).
`Generate-HtmlNames.ps1` pins that input, checks all 2231 scalar mappings against
their character strings, sorts ordinally, and emits `Hn.Data.cs` and test vectors.
Input and generated files are retained in source; runtime lookup needs no network.
Maximum matched name is 32 characters. This is not full HTML-tokenizer conformance
or a legal review of the complete distribution.

Targeted validation: all 5119 real-parser named-reference checks pass, covering
every mapping, legacy suffix ambiguity, case sensitivity, raw text, RCDATA,
unquoted attributes, nonrecursive output and a 10000-character unknown name.
The prior numeric candidate fails 4392 of those checks. All 141 prior numeric
checks still pass. Original input bounds and interpreter/security budgets unchanged.
Receipt `continuity-evidence/named-references-v1/result.json`, SHA256
`D2B7756C2DE57DA73F6F32C7097CAE52F3FF0000D9F4DD4B05C6FF6D6205FF70`.
Candidate core SHA256
`9208520EE6A067FF38D39368B2E50E6B099E5120C977A071E8D3E09733A3AF17`.
Size472576bytes, +47616 vs numeric candidate; no speed or compression claim.
The mature E.cs receives five inverse-checked substitutions; older root source
does not replace mature mutation/diagnostic behavior.

Exact compiled-source archive `named-references-v1/candidate-source.zip` verified
45 entries, 33 compiled engine sources,205806bytes, SHA256
`765D37355B434AEACA854E0AB5B8FCBDA92729DF75C0DE2200E07D2E3BCCE4C5`.
Includes probes, data and local recipes; external references/toolchain and complete
browser sources are not all included. Local preservation, not off-device backup
or a standalone portable browser build kit.

Broader retained engine/view behavior passes in123975ms under the unchanged300s
deadline, exit0/no timeout/kill, inputs and candidate bytes held. Receipt
`named-references-regression-v1/result.json`, SHA256
`89E80A57BFBDF83EBCDCAE79F92984ECD8EE631FE0BBC83A11C0D32CF2A0304C`.
The raw aggregate audit remains failed for three fixture grants. Read-only
`acl-observation.json`, SHA256
`DFCD907F3B02328A605453DD2A82EED0682ECF26460ACB30509A1B2743E06353`,
classifies only one exact explicit fixed-decoder SID RX+Synchronize ACE on each
of Lea.Image.exe, Lea.Image.Handshake.exe and Lea.Tests.exe, reconstructing each
baseline descriptor in memory. No filesystem ACL or policy change by classifier;
no profile deletion/cleanup certification. Raw failed receipt retained.
Owner session99979 exited0/zero leases/unverifiedfalse.

Actual Firespider host15/15 checks pass with this core: settings-path, eight host
cases including script/navigation/multiplewindows/edit/style, and six actual EXE
startup scenarios (clean/restored/locked/oversized/lazy-background/lazy-all).
Receipt `temporal-startup-v1/host-named-references-v1/result.json`, SHA256
`90CB255B21C0C062AB03D4FC6AB7F7DEC0C46890DD85423850CDE0C04DA75743`.
All exit0/drained/no timeout/kill, input hashes and candidate bytes unchanged,
decoder policy accepted; owner74810 exited0/zero leases/unverifiedfalse.
Host950F/adapterAD4A/parentE685/viewE261/worker60FF unchanged; only core9208 swapped.
This is the retained15-stage subset, not full browser/release or visual approval.
Matched scripting build60sources/65runtimefiles passes, receipt
`87E291382607C8CACAA716D022D97A1A374B1A44EA2E91A4714AAD4C6CA94F09`;
testEXE `F5BC92DD30FE167F09B89D779631B320882301DAE3E6FDFCD775F3156E21C1D4`.
Binding verification `895FF5CEA53490CBD7ECF20081CEF4B5EBD134F2FF363194B32420BEEBF45336`
checks six dispatcher inverses, seven fixture pins,46entrymethods/42bodies and
three negative IL tests. Owner15969 exited0/zero leases/unverifiedfalse.

The full matched scripting behavior suite passes3320checks, all89stages and178
ordered phase lines, exit0/no timeout/kill in295752ms under the unchanged300s
deadline and1000ms trusted interpreter budget. Raw receipt
`temporal-startup-v1/named-references-script-run-v1/result.json`, SHA256
`D37C971EE714141BCC203DA2315D8ABA9F979CA95367A856F46C5CCFAA129C7F`,
remains aggregate-failed solely for one exact fixed-decoder grant on Lea.Image.exe.
Read-only `verification.json`, SHA256
`31F18F822FF9326ADA9794AC75709D7B12D42DBF6C16C3F95E7F52EAF712EEF7`,
classifies that existing narrowly defined ACE and verifies phases, lifecycle,
unchanged runtime bytes and all protected inputs. No filesystem ACL/policy change
by verifier or cleanup certification. Owner65466 exited0/zero leases/unverifiedfalse.
Only4248ms outer deadline margin remains; this run is not cold-start reliability
proof or a causal fix for the earlier bootstrap/events timeout60205... .

Qualification sources/receipts are backed up in
`temporal-startup-v1/named-reference-qualification-v1/source-and-qualification.zip`,
82entries/545009bytes, SHA256
`BC9F9608678BC7A131173DFBA1F31B38879B2B6720984DBEEDC7220C0619E731`.
All archived bytes verified against retained inputs. Includes60matchedtest sources,
nested exact engine-source archive and local recipes/receipts; not all browser
sources/dependencies/logs/toolchain, not a portable build kit or off-device backup.
Next: native visual qualification and coherent development delivery. Reliability,
source/toolchain portability and release integration remain open.
