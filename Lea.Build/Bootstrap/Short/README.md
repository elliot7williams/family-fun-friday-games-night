# Short Bootstrap Experiment

Developer-only private-binding mangling experiment. The normal .NET generator,
committed `Dom.compact.js`, hash properties, Visual Studio builds and runtime
dependencies remain unchanged. This tool always writes a fresh output directory
and never replaces normal runtime artifacts.

Install the exact development dependencies with the committed pnpm lockfile,
using `pnpm install --frozen-lockfile --ignore-scripts` in this directory. Node
and these packages are needed only to run the experiment, not by a browser or
normal MSBuild build. An existing absolute node_modules directory can be given
with `-d/--deps` instead of installing beside the tool.

```powershell
node Short.cjs -t
node Short.cjs -i "L:\Lea Development\Lea.Script\Dom.compact.js" -o "H:\Lea Evidence\fresh-short-output"
```

Short aliases: `-t` self-test, `-i` input, `-o` fresh output directory, `-d`
dependency directory. Long forms are `--self-test`, `--input`, `--out`, `--deps`.
Input/output paths must be absolute. Input is bounded UTF-8 without a BOM.

The pinned tools are Terser 5.46.1, Acorn 8.15.0 and ESLint Scope 8.4.0.
Compression/optimization and property mangling are disabled. Function/class
names are preserved; module and top-level modes are disabled. The output uses
short private local bindings and remains ordinary JavaScript.

Verification parses both inputs and resolves their scopes using the proven
analyzer. It compares complete normalized structure, declaration/reference
binding identities, unresolved globals, strict scopes, operators, literals,
public property names, explicit function/class names and inferred declaration
function names. Only printing positions, raw literal spelling and object
shorthand spelling are omitted. Direct eval/with are rejected. Deterministic
generation is checked independently; runtime regressions are still required.
Twelve controls cover valid renaming/shadowing/shorthand and reject mutation,
wrong binding, changed public/function names, changed globals and direct eval.

This is an experimental verifier, not a universal JavaScript equivalence proof.
Function source text, stack source positions and private spelling differ, as
they already do after formatting. Do not replace the normal generator's exact
AST check with this one or claim reliable startup from a smaller payload alone.

See [Terser options](https://terser.org/docs/options/) and
[ESLint Scope usage](https://github.com/eslint/js/blob/main/packages/eslint-scope/README.md).
