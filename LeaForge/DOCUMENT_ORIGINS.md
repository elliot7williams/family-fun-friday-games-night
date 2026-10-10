# Document origin foundation

Lea now creates one immutable `P.Og` from the final response URL when a
document is constructed. Redirects determine that URL; authored `<base>`
elements only affect resource resolution. Fragment navigation, retained
history and DOM/style rebuilds keep the same origin object. Reloads create
a new document and origin.

## Compact API

| Alias | Meaning |
| --- | --- |
| `P.Og` | The document's fixed origin |
| `Og.Of(uri)` | Create an origin from an absolute `System.Uri` |
| `Og.Eq(other)` | Same-origin comparison, including opaque identity |
| `Og.Tx` | Serialized origin, or `"null"` for opaque origins |
| `Og.Op` | Whether this origin is opaque |
| `Og.Lc` | Existing local-script eligibility, not network authority |

```csharp
var page = engine.Go("local-test.html");
var origin = page.Og;
var text = origin.Tx;
var same = origin.Eq(otherPage.Og);
```

Tuple origins use canonical scheme, ASCII host and effective port. Default
ports are omitted from serialization; credentials, path, query and fragment
are excluded. The supported tuple schemes are `ftp/http/https/ws/wss`; this
does not enable new navigation or networking protocols. DNS trailing dots
remain significant. IPv6 output is bracketed; zone-qualified IPv6 fails
closed to an opaque origin.

Files, standalone `about:blank` and unsupported schemes get distinct opaque
identities. Two origins serializing as `"null"` are **not** thereby equal.
Do not recreate an opaque document's authority from its URL; retain `P.Og`.
`Equals` and `GetHashCode` follow the same identity rules as `Eq`.

This foundation uses Lea's existing `System.Uri` parser, not a complete
WHATWG URL parser. Blob URL stores, inherited blank/iframe origins,
`document.domain` and browsing-context origin inheritance are not implemented.
The tuple/opaque model follows the
[HTML origin model](https://html.spec.whatwg.org/multipage/browsers.html#origins)
and the [URL origin rules](https://url.spec.whatwg.org/#origin), with the
unsupported cases above deliberately restricted rather than claimed complete.

## Optional JavaScript

`window.origin` and global `origin` read `P.Og.Tx` through the parent-owned
two-letter `og` bridge operation. The read has no DOM mutations and obeys
the existing bridge and scripting-permission limits. Bootstrap merely installs
the accessor; it does not call the bridge before the native handshake.

Page assignment replaces the visible accessor with a writable data property
without converting the assigned value. Deletion/redefinition are allowed.
The captured getter and engine's origin are unaffected by such replacements;
public JavaScript strings are never used as host permission authority.
This implements replacement behavior for Lea's single global window, not a
complete Window/WindowProxy prototype hierarchy. See the
[HTML global origin attribute](https://html.spec.whatwg.org/multipage/webappapis.html#dom-origin)
and [Web IDL replacement behavior](https://webidl.spec.whatwg.org/#Replaceable).

`W` checks the document's fixed `Og.Lc`, rather than deriving eligibility
from a later visible URL or resource base. Only non-UNC local files and
standalone `about:blank` are eligible. Remote automatic inline scripts,
adapter-supplied code, events and timers remain refused without starting
a worker. External scripts remain unfetched, including on local documents
with an HTTP base. Explicit direct `Js/Jp` host execution remains a separate
embedding capability, not permission for scripts obtained from websites.

## Verification

The first candidate passes 39 core-origin checks, a one-check accessor probe,
four exact embedded-resource checks and 30 isolated/adapter-origin checks.
The latter cover remote host-supplied document serialization, replacements,
private bridge captures, permission revocation, native isolation, local/base
behavior, external-script non-fetch, remote refusal and worker retirement.
Its 800x600 native capture is visually inspected and nonblank.

The trusted-only origin gate instead times out during bootstrap. The old
runtime negative probe also times out before its intended missing-accessor
assertion, so that control is inconclusive. Their helper invocations overlapped
for about three seconds; neither result supports a performance comparison.
They remain retained, with no same-binary retry. A passing isolated run does
not replace the failed trusted gate.

The initial full renderer attempt is stopped after the new test blocks on
an async redirect under a WinForms synchronization context. The test is
corrected to use the existing message-pumping completion helper and its
cross-thread fixture markup is made volatile. Both test executables are
rebuilt in a fresh folder; all 19 candidate runtime files remain identical.
The initial compile typo, stopped renderer run and subsequent corrected
verification are recorded separately.

The corrected 39-check core-origin gate and complete renderer smoke pass.
The corrected broad scripting suite fails after 198 passing assertions at
fresh-path worker startup: wall 5,012 ms, CPU 62 ms, launch 275 ms and probe
4 ms. Its last trace is `startup network`, before VM/parse checkpoints.
No same-binary retry is made. The standalone browser smoke passes with actual
exit code 0; this does not replace the failed broad or trusted gates.
One separate copied Firespider script case also passes, including lifecycle,
events, fields, recovery, split isolation/disposal and remote refusal. Its
helper verifies the child result; the outer report does not capture a numeric
host exit code. The full 43-case host matrix and installed EXE startup are
not reverified or claimed fixed. All five fresh captures are inspected.

Evidence: `H:\Lea Evidence\Lea-146-document-origins-2026-10-10`.
All normal production outputs and the original Firespider source/installed
binaries are preserved. Startup remains 5 seconds, adapter execution 2
seconds, worker CPU 2,500 ms and Job memory 128 MiB. No startup fix, speedup,
default adoption, release or milestone completion is claimed. Broader
readiness remains about 65%, an estimate rather than conformance coverage.

## Still Required

- Parent-owned script/resource fetching with bounded redirects, schemes,
  MIME handling, credentials and executable-resource limits.
- Explicit CORS/fetch policy and cross-origin classic/module distinctions,
  with tests for denied resources and every redirect hop.
- Browsing-context, sandbox, inherited blank and blob origin handling before
  those document types can gain script/network capabilities.
- Partitioned storage/cookies and CSP integration before general website
  execution; no authority decisions based on page-visible origin strings.
- Passing repeatable native startup, full scripting and complete Firespider
  migration gates before broad adoption.
