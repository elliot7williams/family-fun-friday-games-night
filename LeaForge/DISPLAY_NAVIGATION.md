# Lea Display And Location Compatibility

Milestone 75 / runtime 175 extends Lea's bounded script bridge with one compact
display command and familiar literal location requests. It does not add a
JavaScript virtual machine, live style object, history replacement, or direct
page-controlled loading.

## Display

The short form is:

```javascript
dp("panel", "none");
dp("panel", "flex");
dp("panel", "");
```

`dp(id,value)` writes or clears only the target element's inline `display`
declaration. Lea accepts `none`, `block`, `inline`, `inline-block`, `flex`,
`inline-flex`, `grid`, `inline-grid`, `list-item`, `table`, `table-row`, and
`table-cell`, plus the CSS-wide `initial`, `inherit`, `unset`, `revert`, and
`revert-layer` values. An empty value removes the inline declaration and reveals
the authored cascade. Other inline declarations are retained.

The familiar equivalent is:

```javascript
document.getElementById("panel").style.display = "none";
```

IDs remain quoted literals and pass the existing 64-character ID and safe-target
checks. Values remain bounded literals, the final inline style remains at most
4,096 characters, and every accepted mutation uses one retained rebuild. The
same rebuild updates layout, flex/grid mode, fields, focus, hit testing, and
accessibility. Unsupported display keywords are inert and do not replace an
earlier valid declaration.

## Location

The existing `go(url)` alias remains the stable compact command. Lea now also
recognizes these literal compatibility forms:

```javascript
location = "next.html";
location.href = "next.html#result";
window.location.href = "next.html";
document.location.href = "next.html";
location.assign("next.html");
window.location.assign("next.html");
```

Each form maps to `go`. Input is capped at 2,048 characters, resolves against
the document base, and produces at most one pending request per command run.
HTTP and HTTPS are allowed. A local file page may request another non-UNC local
file; remote pages cannot request local files. Other schemes remain rejected.

Headless hosts take the request with `E.Gt(page)`. `Lea.Win.V` exposes the URL as
`V.Gu` and raises `V.Gr`; Firespider then applies its existing host policy and
asynchronous loader. Page code never loads a document or invokes host objects
directly.

Relative file navigation now preserves real query and fragment components
instead of escaping `?` or `#` into the filename. This correction is shared by
the familiar forms and `go` because they use the same resolver.

## Boundary

Computed values, property reads, `location.replace`, `history`, popups,
`javascript:` URLs, expressions, callbacks, and arbitrary location methods stay
unsupported. Unknown or malformed statements are ignored without partial
mutation.

`display-navigation.html` covers compact and familiar display mutations,
empty reset, inline-style retention, invalid cascade preservation, flex reflow,
accessibility restoration, literal location requests, query/fragment handling,
one-pending-request behavior, and unsafe forms. Core, WinForms, benchmark, and
compiled Firespider smokes exercise the same paths.

The runtime 175 release verification passes 71 benchmark probes over 68
fixtures, all 42 off-screen Firespider Boot cases, and all six actual EXE startup
scenarios. Recovery pumps the real 30-second timer for 35 seconds without the
historical Lea-to-`WebBrowser` cast dialog. Required runtime files total 541.0
KiB with a 230.7 KiB GZip estimate. The measured clean-process, local-load, and
layout/paint medians are 512.230 ms, 3.203 ms, and 32.563 ms; these are local
diagnostic samples, not performance promises. Installed runtime 157 remains
unchanged.
