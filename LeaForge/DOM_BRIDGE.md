# Lea Literal DOM Bridge

Milestone 74 / runtime 174 added a familiar browser-shaped surface without
adding a JavaScript virtual machine or a live DOM object model. Lea recognizes
one literal `document.getElementById` lookup per top-level statement and maps it
to the existing compact command engine. Milestone 75 / runtime 175 adds bounded
`style.display` translation and familiar literal location requests.

## Supported Forms

| Familiar form | Lea command |
| --- | --- |
| `.textContent = "text"` or `.innerText = "text"` | `tx(id,text)` |
| `.value = "value"` | `vl(id,value)` |
| `.checked = true/false` | `ck(id,1/0)` |
| `.disabled = true/false` | `ds(id)` / `en(id)` |
| `.hidden = true/false` | `hd(id)` / `sh(id)` |
| `.open = true/false` | `op(id)` / `cl(id)` |
| `.focus()` | `fo(id)` |
| `.classList.add/remove/toggle("name")` | `ac/rc/tc(id,name)` |
| `.style.background` or `.style.backgroundColor` | `bg(id,color)` |
| `.style.color` | `fg(id,color)` |
| `.style.fontSize` | `fs(id,size)` |
| `.style.display` | `dp(id,display)` |

For example:

```javascript
document.getElementById("status").textContent = "Ready";
document.getElementById("name").value = "Lea";
document.getElementById("agree").checked = true;
document.getElementById("status").classList.add("ready");
document.getElementById("name").focus();
```

## Boundary

The ID and class name must be quoted literals. Text/style/value operands use
the existing bounded literal reader, and state accepts only literal `true` or
`false`. Every translated command then passes through Lea's existing ID, target,
class, color, font, field, disclosure, focus, navigation-independent rebuild,
and data-retention checks.

The global script limits remain eight inline blocks, 16 KiB per block, and 32
accepted commands per run. IDs remain at most 64 characters, class tokens at
most 64 characters, class lists at most 32 tokens/512 characters, field values
at most 1,024 characters, and inline style text at most 4,096 characters.

Lea does not run `innerHTML`, `querySelector`, computed targets or values,
property reads, variables as object references, event listeners, timers,
promises, loops, functions, arbitrary method calls, or control flow. Unknown or
malformed statements are ignored without partially mutating the page.

Literal `location`, `location.href`, and `location.assign(...)` forms map to the
existing host-controlled `go` request. `window.location` and
`document.location` spellings are accepted. They retain the same URL and host
policy boundary; `location.replace`, history APIs, computed values, and unsafe
schemes remain inert. See `DISPLAY_NAVIGATION.md`.

## Integration

The bridge is inside `Lea.Engine.J`; no Firespider adapter change is required.
It works through inline script blocks, `data-lea`/`onclick` via `E.Ck`/`V.Ck`,
and host-provided `E.Js`/`V.Js` strings. The short aliases remain the stable API
for small Lea pages and embedders.

`dom-bridge.html` covers page and click translation, retained form identity,
layout/styles, accessibility restoration, disclosure state, focus consumption,
and rejected APIs. The full engine and WinForms suite passes. The compiled
Firespider smoke exercises the same syntax through a real hosted `V` control.
The benchmark has 70 passing probes over 67 fixtures.

All 42 off-screen Firespider Boot cases pass in one combined run. The recovery
case pumps the real 30-second timer for 35 seconds without the historical Lea to
`WebBrowser` cast dialog. Clean, restored, locked, oversized, lazy-background,
and lazy-all actual EXE launches remain responsive, expose the expected Lea or
deferred surface, and load no MSHTML module.

This is a compatibility convenience, not full DOM or JavaScript support.
