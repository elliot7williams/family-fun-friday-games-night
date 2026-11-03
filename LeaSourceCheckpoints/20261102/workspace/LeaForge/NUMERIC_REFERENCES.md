# Numeric HTML References

Later named-reference candidate work is documented in NAMED_REFERENCES.md.
The original numeric candidate and its evidence below remain retained unchanged.

Candidate work, 2026-10-25. Implements HTML numeric-reference recovery in the
compact internal `Hc.D` decoder, called at text/attribute parsing boundaries.
Named references still use the existing .NET decoder; this is not full HTML
tokenizer or named-entity conformance.

Behavior follows the [HTML numeric reference states](https://html.spec.whatwg.org/multipage/parsing.html#numeric-character-reference-state):

- Decimal/hexadecimal ASCII digits; either hex-prefix case; optional semicolon.
- Null, surrogate, and out-of-range values become U+FFFD.
- The specified C1 replacement table is applied; other controls/noncharacters
  retain their code points. Parse-error diagnostics are not exposed yet.
- Long digit runs saturate without integer overflow while consuming every digit.
- Missing digits, signs and leading whitespace remain literal.
- Decoded output is not reparsed. DOM text is no longer HTML-decoded again during
  title/block/inline normalization. Script/style raw text stays raw; textarea
  RCDATA is decoded. Existing document/attribute bounds remain unchanged.

`Lea.Tests/NumericReferences.cs` checks real parser nodes, attributes and a loaded
page's presentation. The existing 6D89 core fails 96 of 141 checks; candidate
515121C4 passes all 141, including 10,000-digit inputs. The V2 candidate's four
malformed-reference failures remain retained; V3 corrects that fallback issue.
V1 stopped before compilation on an incorrect source inventory expectation.

`Build-NumericReferenceCandidate.ps1` overlays five exact changes onto the mature
preserved E.cs, verifies their inverse, and adds Hc.cs. It does not replace that
source with the older root E.cs; mature attribute/mutation/diagnostic work remains
intact. Root E.cs and its project also carry the scoped changes for development.

Targeted receipt: `numeric-references-v3/result.json`, SHA256
`9DA5BD190720348FF6A63631BEA5C51C2F220527683FE63F03CA5984C0AA9332`.
Full retained engine/view behavior passed in 128.1 seconds with its unchanged
300-second deadline, fixture set and assertions. Receipt:
`numeric-references-regression-v1/result.json`, SHA256
`D4196CC2DB96AD7318704E6EC7BB30BC730D0E595DCB55B5205AA535D297A473`.
Inputs/candidate bytes were unchanged; the original owner retired cleanly. The raw
audit remains failed for three exact private fixed-decoder grants. Read-only
attribution (`acl-observation.json`, 5838A67A...) reconstructs the prior SDDL;
no permissions were edited and no production permission policy was broadened.

The candidate DLL is 424,960 bytes versus 423,936 baseline bytes. This is not a
performance claim. Scripting, actual Firespider, visual, security, and broad
standards qualification remain separate. Do not adopt this candidate or transfer
old runtime passes to it based on these engine/view checks.

The exact numeric-core Firespider candidate passed all 15 retained host/startup
cases, including scripting and six actual-EXE startup scenarios. Receipt:
`host-numeric-references-v1/result.json`, SHA256
`D9DEAECD30748B3819B55D164182C788E74C26006F9805B5A421E97584A8FA47`.
All parents exited zero with drained output, no timeout/kill, unchanged input/
candidate bytes and only the existing narrowly checked decoder permission delta.
This is not cold-start reliability or a numeric-page visual comparison.

The matched 60-source/65-file full scripting suite built and passed binding
verification with core5151 and adaptorAD4A. Its runtime attempt FAILED after 89
early passing assertions at phase 8: computed trusted-Js bootstrap timed out at
`events`, under the original 1000ms interpreter budget. This was not the outer
300-second deadline or isolated-worker hello deadline. Later stages did not run.
Failure receipt: `numeric-references-script-run-v1/result.json`, SHA256
`60205BDC951F064A13EAE715F20B42F7998C1EE38F7AF7513613F20CFB70B9DE`.
The parent exited 1 in 9789ms with drained output, no timeout/kill; all 65 runtime
bytes/ACLs and inputs stayed unchanged (FD69F0D3... failure verification).

The historical baseline has failures in this same budget/stage class, but that
does not prove the new failure's cause. Adoption is held pending same-context
scope/events cost attribution and a verified fix. No warm retry, budget increase,
assertion removal or transfer of the older 3320-check pass is allowed.
