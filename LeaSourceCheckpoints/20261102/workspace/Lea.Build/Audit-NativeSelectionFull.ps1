param([Parameter(Mandatory)][ValidatePattern('^[A-F0-9]{64}$')][string]$ExpectedReceipt)
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$root=Join-Path $workspace 'continuity-evidence/native-selection-full-v1'
$generated=Join-Path $root 'read-only-acl-audit.ps1'
if(Test-Path $generated){throw 'Preserve generated audit'}
$originalPath=Join-Path $PSScriptRoot 'Audit-NestedOverlapAcls.generated.ps1'
if((Get-FileHash $originalPath).Hash -cne '91140EA99FF42DA1BD8C7231E2CC9B33CCF1702980AC5172418B0CF7EAF133A1'){throw 'Original read-only classifier differs'}
$original=[IO.File]::ReadAllText($originalPath)
$pairs=@(
    @('$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot ''..''))',('$workspace='''+$workspace.Replace("'","''")+'''')),
    @("'continuity-evidence/nested-overlap-regression-v1'","'continuity-evidence/native-selection-full-v1'"),
    @('E60A49F48671E92B80279171E5FC274BB4D7CFF2020D185A86729B7A38F04B6E',$ExpectedReceipt)
)
$text=$original
foreach($pair in $pairs){
    if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Read-only audit anchor differs'}
    $text=$text.Replace($pair[0],$pair[1])
}
$inverse=$text
for($index=$pairs.Count-1;$index -ge 0;$index--){$inverse=$inverse.Replace($pairs[$index][1],$pairs[$index][0])}
if($inverse -cne $original){throw 'Read-only classifier inverse differs'}
[IO.File]::WriteAllText($generated,$text,[Text.UTF8Encoding]::new($false))
& $generated
