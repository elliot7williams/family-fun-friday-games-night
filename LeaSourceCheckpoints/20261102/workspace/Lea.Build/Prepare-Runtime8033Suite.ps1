$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace 'continuity-evidence/runtime-8033-suite-preparation-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve suite preparation'}
function Read-Pinned([string]$Name,[string]$Hash){
    $path=Join-Path $PSScriptRoot $Name
    if((Get-FileHash -LiteralPath $path).Hash -cne $Hash){throw "Helper changed: $Name"}
    return [IO.File]::ReadAllText($path).Replace("`r",'')
}
function Project([string]$Original,[object[]]$Edits){
    $text=$Original
    foreach($edit in $Edits){
        if([regex]::Matches($text,[regex]::Escape($edit[0])).Count -ne 1){throw ('Unique anchor required: '+$edit[0])}
        $text=$text.Replace($edit[0],$edit[1])
    }
    $inverse=$text
    for($i=$Edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($Edits[$i][1],$Edits[$i][0])}
    if($inverse -cne $Original){throw 'Source projection inverse failed'}
    $tokens=$null;$errors=$null
    $null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
    if($errors.Count){throw 'Source projection syntax failed'}
    return $text
}
$fragmentPath=Join-Path $PSScriptRoot 'Runtime8033Staging.fragment.ps1'
$fragment=[IO.File]::ReadAllText($fragmentPath).Replace("`r",'').TrimEnd("`n")
$original=Read-Pinned 'Initialize-PrintLiveScriptSuite.generated.ps1' '4C164AACE690D2A7225DE66E84F48F47ABBFB4D983C21BF509C5A6B202222372'
$oldManifest=@'
        $to=Join-Path $targetDir 'manifest.json';Copy-Item -LiteralPath $manifestPath -Destination $to;Hold $to $probe.manifestSha256
'@
$newManifest=@'
        $to=Join-Path $targetDir 'manifest.json'
        [IO.File]::WriteAllText($to,($spec|ConvertTo-Json -Depth 5),[Text.UTF8Encoding]::new($false))
        $newComponentHash=(Get-FileHash -LiteralPath $to).Hash;Hold $to $newComponentHash
'@
$oldFixture=@'
    foreach($name in $relocated.files.Keys){if((Get-FileHash -LiteralPath (Join-Path $relocated.directory $name)).Hash -cne $relocated.files[$name]){throw 'Relocated fixture bytes differ'}}
'@
$newFixture=@'
    foreach($name in @($relocated.files.Keys)){
        $relative='WorkerEntryFixture\'+$name
        if($configMap.ContainsKey($relative)){
            $config=$configMap[$relative]
            if($relocated.files[$name] -cne $config.sourceSha256 -or !$configUsed.Contains($relative)){throw 'Entry config original identity differs'}
            $relocated.files[$name]=$config.targetSha256
        }
        if((Get-FileHash -LiteralPath (Join-Path $relocated.directory $name)).Hash -cne $relocated.files[$name]){throw 'Relocated fixture bytes differ'}
    }
'@
$oldFixtureManifest=@'
    foreach($name in $manifest.fixtureFiles.Keys){if($manifest.fixtureFiles[$name] -cne $relocated.files[$name]){throw 'Fixture manifest pin changed'}}
'@
$newFixtureManifest=@'
    foreach($name in @($manifest.fixtureFiles.Keys)){
        $expectedOriginal=$relocated.files[$name]
        $relative='WorkerEntryFixture\'+$name
        if($configMap.ContainsKey($relative)){$expectedOriginal=$configMap[$relative].sourceSha256}
        if($manifest.fixtureFiles[$name] -cne $expectedOriginal){throw 'Fixture original manifest pin changed'}
        $manifest.fixtureFiles[$name]=$relocated.files[$name]
    }
    if($configUsed.Count -ne 8){throw 'All eight runtime configs must be projected'}
'@
$init=Project $original @(
    @('    $replacement=@{',($fragment+"`n    `$replacement=@{")),
    @("        Copy-Item -LiteralPath `$from -Destination `$target`n        Hold `$target `$expected",'        $expected=Stage-Runtime8033Input $from $target $expected'),
    @('        foreach($name in $spec.files.Keys){','        foreach($name in @($spec.files.Keys)){'),
    @('            Hold $from $spec.files[$name];Copy-Item -LiteralPath $from -Destination $to;Hold $to $spec.files[$name]','            $spec.files[$name]=Stage-Runtime8033Input $from $to $spec.files[$name]'),
    @($oldManifest.TrimEnd("`n"),$newManifest.TrimEnd("`n")),
    @('replacement=$probe.manifestSha256','replacement=$newComponentHash'),
    @($oldFixture.TrimEnd("`n"),$newFixture.TrimEnd("`n")),
    @($oldFixtureManifest.TrimEnd("`n"),$newFixtureManifest.TrimEnd("`n")),
    @('fixturePinsUnchanged=$true;workerChanged=$false','fixturePinsUnchanged=$false;runtimeConfigVersion=''8.0.33'';configProjections=8;fixtureNonConfigPinsUnchanged=$true;workerChanged=$false')
)
$target=Join-Path $PSScriptRoot 'Initialize-Runtime8033Suite.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve initializer'}
[IO.File]::WriteAllText($target,$init,[Text.UTF8Encoding]::new($false))
$initHash=(Get-FileHash -LiteralPath $target).Hash
$buildOriginal=Read-Pinned 'Build-PrintLiveScriptSuite.generated.ps1' 'F672442C979CAB4C5DDE6F7895B89B7E7E6D3DC5E036AF22B3852A8F7BAD5861'
$build=Project $buildOriginal @(
    @('current-native-script-suite-v10','current-native-script-suite-v11'),
    @('Lea.Build/Initialize-PrintLiveScriptSuite.generated.ps1','Lea.Build/Initialize-Runtime8033Suite.generated.ps1'),
    @('4C164AACE690D2A7225DE66E84F48F47ABBFB4D983C21BF509C5A6B202222372',$initHash),
    @('Matched60-source print coreA85/view568 suite with unchanged contracts and owned compiler processes; runtime verification separate','Matched60-source print coreA85/view568 suite with exact8.0.33 config projections and unchanged contracts; runtime compatibility verification separate')
)
$buildTarget=Join-Path $PSScriptRoot 'Build-Runtime8033Suite.generated.ps1'
if(Test-Path -LiteralPath $buildTarget){throw 'Preserve builder'}
[IO.File]::WriteAllText($buildTarget,$build,[Text.UTF8Encoding]::new($false))
$null=New-Item -ItemType Directory -Path $out
foreach($path in @($PSCommandPath,$fragmentPath,$target,$buildTarget)){Copy-Item -LiteralPath $path -Destination (Join-Path $out ([IO.Path]::GetFileName($path)))}
@{passed=$true;inverseVerified=$true;syntaxVerified=$true;initializerSha256=$initHash;builderSha256=(Get-FileHash -LiteralPath $buildTarget).Hash;fragmentSha256=(Get-FileHash -LiteralPath $fragmentPath).Hash;compiled=$false;runtimeExecuted=$false;limitsChanged=$false;scope='Fresh suite11 staging/build source projection only; eight configs, six regenerated manifest/dispatcher bindings and new embedded entry resource; no runtime qualification'}|ConvertTo-Json -Depth 5|Set-Content -LiteralPath (Join-Path $out 'result.json')
Get-Content -LiteralPath (Join-Path $out 'result.json')
