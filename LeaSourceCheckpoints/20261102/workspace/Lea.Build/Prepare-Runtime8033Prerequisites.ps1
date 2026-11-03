$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace 'continuity-evidence/runtime-8033-prerequisite-preparation-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve preparation evidence'}
$specs=@(
    @{source='Test-ComponentProbePrerequisites.ps1';hash='21DF74BFC400ABDE2C5119C4CB8870E8D791521655568546AEA3CDB5DA3A26D5';target='Test-ComponentProbePrerequisites8033.generated.ps1';edits=@(,@('8.0.32','8.0.33',4))},
    @{source='Test-ComponentProbePrerequisites.Tests.ps1';hash='69ACA30D867C9E4C5F7A0F29A919C1C640C6329BFE6F79E5F6DBAF846D19353E';target='Test-ComponentProbePrerequisites8033.Tests.generated.ps1';edits=@(@('Test-ComponentProbePrerequisites.ps1','Test-ComponentProbePrerequisites8033.generated.ps1',2),@('component-host-prerequisites-v1','component-host-prerequisites-8033-v1',1))}
)
$prepared=@(foreach($spec in $specs){
    $source=Join-Path $PSScriptRoot $spec.source
    $target=Join-Path $PSScriptRoot $spec.target
    if(Test-Path -LiteralPath $target){throw 'Preserve generated helper'}
    if((Get-FileHash -LiteralPath $source).Hash -cne $spec.hash){throw 'Source helper changed'}
    $original=[IO.File]::ReadAllText($source)
    $text=$original
    foreach($edit in $spec.edits){
        if([regex]::Matches($text,[regex]::Escape($edit[0])).Count -ne $edit[2]){throw 'Projection count changed'}
        $text=$text.Replace($edit[0],$edit[1])
    }
    $inverse=$text
    for($i=$spec.edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($spec.edits[$i][1],$spec.edits[$i][0])}
    if($inverse -cne $original){throw 'Projection inverse changed'}
    $tokens=$null;$errors=$null
    $null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
    if($errors.Count){throw 'Projection syntax invalid'}
    @{source=$source;sourceSha256=$spec.hash;target=$target;text=$text}
})
$null=New-Item -ItemType Directory -Path $out
Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'runner.ps1')
$records=@(foreach($entry in $prepared){
    [IO.File]::WriteAllText($entry.target,$entry.text,[Text.UTF8Encoding]::new($false))
    Copy-Item -LiteralPath $entry.target -Destination (Join-Path $out ([IO.Path]::GetFileName($entry.target)))
    if((Get-FileHash -LiteralPath $entry.source).Hash -cne $entry.sourceSha256){throw 'Original helper changed'}
    @{source=$entry.source;sourceSha256=$entry.sourceSha256;target=$entry.target;sha256=(Get-FileHash -LiteralPath $entry.target).Hash;inverseVerified=$true;syntaxVerified=$true}
})
@{passed=$true;helpers=$records;runtimeExecuted=$false;productionChanged=$false;scope='Versioned exact8.0.33 prerequisite checker and unchanged seven synthetic controls; old helpers untouched'}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $out 'result.json')
$records|ConvertTo-Json
