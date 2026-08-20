# LeaForge Codex Workbench

This is the new Codex work area for continuing LeaForge.

The full staged roadmap is in [ENGINE_PLAN.md](ENGINE_PLAN.md).
Visual Studio embedding details are in [INTEGRATION.md](INTEGRATION.md).
Weighted completion is tracked in [PROGRESS.md](PROGRESS.md).

## Goal

Build Lea into a more complete, self-owned browser engine while keeping the tiny Lea spirit:

- understandable source
- fast startup
- separate experiments for big features
- no Chromium/WebView dependency for the custom renderer path

## Next Build Targets

1. Add a compact compatibility and performance report.
2. Expand the safe command set only through bounded, fixture-backed aliases.
3. Route useful developer diagnostics into Firespider without coupling its UI to engine internals.

## Rules For Future Codex Work

- Keep `LeaForge` separate from `Lea.Micro`, `LeaBrowser`, and `LeaPlus`.
- Do not replace the custom renderer with WebView unless we create a separate comparison project.
- Prefer small working steps and compile an exe after each meaningful milestone.
- Keep local test pages in `test-pages`.
- Keep public docs in `README.md`; keep working notes here.
- Keep new public Lea features within the alias rules in `ALIASES.md`.
