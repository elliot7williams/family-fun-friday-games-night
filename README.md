# Lea browser engine

Lea is a small browser engine with a custom Windows Forms renderer. The mature implementation is **Lea.Engine + Lea.Win**. Required Lea rendering does not use Chromium or WebView. JavaScript is an optional, isolated component built around Jint. The Firespider host retains explicit legacy fallback and some unmigrated tools.

This repository also contains historical prototypes and unrelated projects. `Lea.Core`, `Lea`, `Lea.Micro`, `LeaBrowser`, and `LeaPlus` are separate from the mature engine.

## Development status

The scoped Lea baseline is implemented. Broader browser development continues, with milestone 146 open. This is a development project, not a verified stable browser release.

Implemented bounded features include:

- Local and HTTP navigation, history, cookies, caching, forms, and common document elements.
- HTML parsing, CSS cascade and layout, image handling, editing, keyboard interaction, and accessibility.
- Lea integration with the Firespider host, including tabs, split views, and session workflows.
- Optional isolated scripting, DOM operations, native events, microtasks, timers, and animation frames for permitted local/about documents. HTTP/HTTPS/UNC script execution remains disabled.

Support is intentionally bounded; these features do not imply complete web-standard compatibility.

**Unfinished:** intermittent five-second JavaScript worker startup failures, remaining exact-build worker/UI regression checks, independent host rebuild and extracted-package verification, and broader DOM/CSS/layout compatibility. Passing startup samples and a smaller-bootstrap prototype do not establish a production startup fix.

## Source map

| Location | Purpose |
| --- | --- |
| `Lea.Engine/` | Document models, HTML/CSS, navigation, and engine commands |
| `Lea.Win/` | Windows Forms layout, rendering, input, and accessibility |
| `Lea.Image/` | Isolated raster decoding |
| `Lea.Script/`, `Lea.Script.Worker/`, `Lea.Script.Win/` | Optional scripting, worker isolation, and native adapter |
| `Lea.Tests/`, `Lea.Win.Test/`, `Lea.Script.Tests/` | Regression harnesses |
| `Lea.Firespider/`, `Lea.Build/` | Host integration, build tools, candidate checks, and packaging |
| `LeaForge/` | Feature documentation, fixtures, progress, and evidence |

The required engine/view target .NET Framework 4.6.1. Optional scripting targets .NET Framework 4.6.2 and uses locked Jint 4.16.4 / Acornima 1.7.0 dependencies. Development-only normal bootstrap generation uses .NET 8; the short-bootstrap experiment has separate pinned Node tooling. The external Firespider host has its own requirements.

See [build and validation instructions](Lea.Build/README.md), [Firespider candidate instructions](Lea.Firespider/CANDIDATE.md), and [optional scripting documentation](Lea.Script/README.md). Follow the instructions for the intended component; historical projects have different build paths.

## Packages

Lea remains experimental. There is no verified stable browser release. Candidate runtime and host-source packages must retain their exact revision, SHA-256 manifests and validation limits. Host-source packages use the selected prebuilt Lea dependencies; the complete Lea source is separate. Passing results from one candidate do not validate another.

## Continue development

Read [LeaForge/PROGRESS.md](LeaForge/PROGRESS.md) and the linked feature documentation. Check Git status and diffs before editing, preserve existing work and retained failure evidence, and keep validation tied to the tested revision. Worker isolation and existing deadlines remain required.
