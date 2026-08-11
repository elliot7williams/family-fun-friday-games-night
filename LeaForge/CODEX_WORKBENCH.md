# LeaForge Codex Workbench

This is the new Codex work area for continuing LeaForge.

The full staged roadmap is in [ENGINE_PLAN.md](ENGINE_PLAN.md).
Visual Studio embedding details are in [INTEGRATION.md](INTEGRATION.md).

## Goal

Build Lea into a more complete, self-owned browser engine while keeping the tiny Lea spirit:

- understandable source
- fast startup
- separate experiments for big features
- no Chromium/WebView dependency for the custom renderer path

## Next Build Targets

1. Split `Program.cs` into engine, parser, renderer, and UI files.
2. Add a real CSS cascade: tag selectors, class selectors, id selectors, and inline style priority.
3. Add layout boxes instead of one-control-per-node rendering.
4. Add image downloading and bitmap display.
5. Add a small JavaScript interpreter layer or bridge.
6. Add cache storage for pages and images. (Page cache started: bounded 12-page memory cache.)
7. Add bookmarks saved to a JSON file.
8. Add downloads.
9. Add developer tools panel for parsed nodes, CSS rules, network events, and console messages.

## Rules For Future Codex Work

- Keep `LeaForge` separate from `Lea.Micro`, `LeaBrowser`, and `LeaPlus`.
- Do not replace the custom renderer with WebView unless we create a separate comparison project.
- Prefer small working steps and compile an exe after each meaningful milestone.
- Keep local test pages in `test-pages`.
- Keep public docs in `README.md`; keep working notes here.
- Keep new public Lea features within the alias rules in `ALIASES.md`.
