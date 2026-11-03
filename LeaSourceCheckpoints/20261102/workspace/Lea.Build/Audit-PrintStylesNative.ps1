$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$root=Join-Path $workspace 'continuity-evidence/print-styles-native-v1'
$target=Join-Path $root 'read-only-acl-audit.ps1'
$source=Join-Path $PSScriptRoot 'Audit-NestedOverlapAcls.generated.ps1'
if(Test-Path $target){throw 'Preserve read-only classification'}
if((Get-FileHash $source).Hash -cne '91140EA99FF42DA1BD8C7231E2CC9B33CCF1702980AC5172418B0CF7EAF133A1'){throw 'Original classifier differs'}
$original=[IO.File]::ReadAllText($source);$text=$original
$pairs=@(
    @('$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot ''..''))',('$workspace='''+$workspace.Replace("'","''")+'''')),
    @('continuity-evidence/nested-overlap-regression-v1','continuity-evidence/print-styles-native-v1'),
    @('E60A49F48671E92B80279171E5FC274BB4D7CFF2020D185A86729B7A38F04B6E','01C7672A37F2EE7947358C832ABA8BCE87CEB68EE3D5BA686478E4119826721E')
)
foreach($pair in $pairs){if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Classifier anchor differs'};$text=$text.Replace($pair[0],$pair[1])}
$inverse=$text
for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
if($inverse -cne $original){throw 'Classifier inverse differs'}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
& $target
