$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$source=Join-Path $PSScriptRoot 'Move-VerifiedSourceBackups.ps1'
$target=Join-Path $PSScriptRoot 'Move-UiaSourceBackup.generated.ps1'
if(Test-Path $target){throw 'Preserve relocation helper'}
if((Get-FileHash $source).Hash -cne 'D89873DEC0547F02FA78860DA0BEE0EAEBE91C79A43F8A1061754EA2493298AD'){throw 'Original relocation helper differs'}
$original=[IO.File]::ReadAllText($source)
$tokens=$null;$errors=$null
$ast=[Management.Automation.Language.Parser]::ParseInput($original,[ref]$tokens,[ref]$errors)
$assignments=@($ast.FindAll({param($node) $node -is [Management.Automation.Language.AssignmentStatementAst] -and $node.Left.Extent.Text -ceq '$names'},$true))
if($errors.Count -or $assignments.Count -ne 1){throw 'Unique archive allowlist required'}
$anchor='$sourceRoot=Join-Path $workspace ''continuity-evidence'''
$pin='if((Get-FileHash (Join-Path $sourceRoot ''runtime-8033-matched-source-20261027.zip'')).Hash -cne ''84179CBDF6C3674E43C886FCF72B5C048056425B2D029E2CE3DBBE4CD719AEB9''){throw ''Pinned source archive differs''}'
$edits=@(
    @($assignments[0].Right.Extent.Text,"@('runtime-8033-matched-source-20261027.zip')"),
    @('L:\Lea-source-backup-archive-20261027','L:\Lea-source-backup-archive-20261029-uia'),
    @('$result.records.Count -ne 8','$result.records.Count -ne 1'),
    @('All eight copies must verify before removal','The single copy must verify before removal'),
    @($anchor,($anchor+[Environment]::NewLine+$pin))
)
$candidate=$original
foreach($edit in $edits){if(!$candidate.Contains($edit[0])){throw 'Relocation projection anchor missing'};$candidate=$candidate.Replace($edit[0],$edit[1])}
$inverse=$candidate
for($i=$edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($edits[$i][1],$edits[$i][0])}
if($inverse -cne $original){throw 'Relocation inverse differs'}
$null=[Management.Automation.Language.Parser]::ParseInput($candidate,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Relocation syntax failed'}
[IO.File]::WriteAllText($target,$candidate,[Text.UTF8Encoding]::new($false))
if([IO.File]::ReadAllText($target) -cne $candidate){throw 'Relocation helper readback differs'}
& $target
