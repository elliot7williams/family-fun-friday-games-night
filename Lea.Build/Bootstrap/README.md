# Bootstrap generator

This developer-only .NET 8 tool formats the readable `Lea.Script/Dom.js`
AST using Acornima.Extras 1.7.0, matching the runtime's pinned Acornima parser.
There is no identifier mangling, optimizer, new runtime dependency, compressed
stream, or decompression work. The output is compact JavaScript, not a new
language or a shortened web-platform API.

From the repository directory:

```powershell
dotnet restore Lea.Build/Bootstrap/Lea.Bootstrap.csproj --locked-mode
dotnet build Lea.Build/Bootstrap/Lea.Bootstrap.csproj -c Release --no-restore
Lea.Build/Bootstrap/bin/Release/net8.0/Lea.Bootstrap.exe -t
Lea.Build/Bootstrap/bin/Release/net8.0/Lea.Bootstrap.exe -w .
Lea.Build/Bootstrap/bin/Release/net8.0/Lea.Bootstrap.exe -c .
```

Review and commit the readable source, generated `Dom.compact.js`, and
`Dom.g.props` together. Source, generated JavaScript, and generated hashes use
CRLF via `.gitattributes`; generated files are UTF-8 without a BOM.
Short aliases are `-t` (test), `-w` (write), and `-c` (check); the descriptive
`--self-test`, `--write`, and `--check` forms remain available.

Generation reparses the output and compares the complete default AST JSON
(without locations), then checks fixed-point generation. This catches structural
changes, including literal, directive, identifier, and statement differences;
it does not replace runtime regressions or prove all observable equivalence.
For example, function source strings and source positions reflect compact text.
The self-test covers seven syntax cases and a changed-literal negative control.

Ordinary Visual Studio/MSBuild builds do not run or require this generator.
They embed the committed output under the existing `Lea.Script.Dom.js` name.
Both runtime projects use stock MSBuild SHA-256 tasks to reject source or
generated files that do not match the committed hashes. `--check` additionally
regenerates in memory and rejects stale JavaScript or hash properties.

Run `Lea.Script.Tests.exe . --bootstrap-assets` to inspect both compiled
resource payloads. This requires source files and is also part of the normal
full source test run. `--class-profile` profiles the actual embedded compact
bootstrap without statement markers. Supply `Lea.Script/Dom.js` explicitly for
the existing readable-source statement-marker diagnostic. Neither instrumented
diagnostic is the production startup gate.
