$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$baseline=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/current-native-script-suite-v10/runtime'
$out=Join-Path $workspace 'continuity-evidence/runtime-8033-configs-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve previous preparation'}
$root=Join-Path $env:ProgramW6432 'dotnet'
$required=@('dotnet.exe','host/fxr/8.0.33/hostfxr.dll','shared/Microsoft.NETCore.App/8.0.33/coreclr.dll')
$inventory=@(foreach($name in $required){
    $path=Join-Path $root $name
    if(!(Test-Path -LiteralPath $path -PathType Leaf)){throw "Missing prerequisite: $path"}
    $item=Get-Item -LiteralPath $path
    @{path=$path;sha256=(Get-FileHash -LiteralPath $path).Hash;version=$item.VersionInfo.FileVersion;length=$item.Length}
})
$configs=@(Get-ChildItem -LiteralPath $baseline -Recurse -File -Filter '*.runtimeconfig.json')
if($configs.Count -ne 8){throw 'Expected eight baseline configs'}
$prepared=@(foreach($file in $configs){
    $relative=[IO.Path]::GetRelativePath($baseline,$file.FullName)
    if([IO.Path]::IsPathRooted($relative) -or $relative.Split('\') -contains '..'){throw 'Path escape'}
    $text=[IO.File]::ReadAllText($file.FullName)
    $config=$text|ConvertFrom-Json -AsHashtable
    if($config.runtimeOptions.framework.version -cne '8.0.32' -or
       $config.runtimeOptions.framework.name -cne 'Microsoft.NETCore.App' -or
       $config.runtimeOptions.rollForward -cne 'Disable' -or
       [regex]::Matches($text,'8\.0\.32').Count -ne 1){throw 'Baseline config contract changed'}
    # Keep exact formatting and all unrelated settings; parse both sides as well.
    $updated=$text.Replace('8.0.32','8.0.33')
    if($updated.Replace('8.0.33','8.0.32') -cne $text){throw 'Inverse projection failed'}
    $parsed=$updated|ConvertFrom-Json -AsHashtable
    if($parsed.runtimeOptions.framework.version -cne '8.0.33' -or $parsed.runtimeOptions.rollForward -cne 'Disable'){throw 'Target config invalid'}
    @{relative=$relative;source=$file.FullName;sourceSha256=(Get-FileHash -LiteralPath $file.FullName).Hash;text=$updated}
})
$null=New-Item -ItemType Directory -Path $out
Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'runner.ps1')
$records=@(foreach($entry in $prepared){
    $target=Join-Path $out ('configs/'+$entry.relative)
    $null=New-Item -ItemType Directory -Path (Split-Path $target) -Force
    [IO.File]::WriteAllText($target,$entry.text,[Text.UTF8Encoding]::new($false))
    if([IO.File]::ReadAllText($target) -cne $entry.text -or
       (Get-FileHash -LiteralPath $entry.source).Hash -cne $entry.sourceSha256){throw 'Readback or original changed'}
    @{relative=$entry.relative;source=$entry.source;sourceSha256=$entry.sourceSha256;target=$target;targetSha256=(Get-FileHash -LiteralPath $target).Hash;inverseVerified=$true}
})
$result=@{passed=$true;configs=$records;installedInputs=$inventory;runtimeExecuted=$false;candidateQualified=$false;originalsUnchanged=$true;manifestsUpdated=$false;scope='Config-only exact 8.0.33 projections, not runnable suite or production adoption';next='Rebind manifests, prerequisite controls and source/worker identities in a fresh matched candidate before executing tests.'}
[IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 8))
[pscustomobject]@{passed=$true;configs=$records.Count;runtimeExecuted=$false;candidateQualified=$false}|ConvertTo-Json
