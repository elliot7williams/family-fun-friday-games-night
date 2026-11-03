param([ValidateRange(1,8)][int]$Version=1,[switch]$RecoveryTrace,[switch]$Replay)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$stage='I:\LeaBuildStorage\sdk-full-script-assembly-20261101-v'+$Version
$compile='I:\LeaBuildStorage\sdk-full-script-compile-20261101-v'+$Version
$out=Join-Path $root ('continuity-evidence/sdk-full-script-suite-v'+$Version)
if($RecoveryTrace){if($Version -ne 8){throw 'Recovery trace requires deterministic candidate'};$stage='I:\LeaBuildStorage\retirement-prefix-main-only-20261102-v4';$compile='I:\LeaBuildStorage\retirement-prefix-compile-20261102-v4';$out=Join-Path $root 'continuity-evidence/retirement-full-prefix-v2'}
if($Replay){if($Version -ne 8 -or $RecoveryTrace){throw 'Replay requires separate deterministic mode'};$stage='I:\LeaBuildStorage\deterministic-replay-assembly-20261102-v1';$compile='I:\LeaBuildStorage\deterministic-replay-compile-20261102-v1';$out=Join-Path $root 'continuity-evidence/deterministic-full-replay-v1'}
if(Test-Path -LiteralPath $out){throw 'Preserve existing NTFS candidate'}
if((Get-Volume -DriveLetter C).FileSystem -cne 'NTFS' -or (Get-PSDrive C).Free -lt 600MB){throw 'NTFS and original staging reserve required'}
function Pin([string]$path,[string]$hash){if((Get-FileHash -LiteralPath $path).Hash -cne $hash){throw ('Runtime staging identity differs: '+$path)}}
$assemblyPin=if($Version -eq 4){'54CACB2CA1D0939791C101A0FB97D79CF0D0C2B123633EF5B5A017F5874B9A3F'}elseif($Version -eq 3){'045F8EAF988199AC86119C0EAB552EF4A9E90C02ACAB5ACE2CBB8C3CD00E9992'}elseif($Version -eq 2){'EA20FF493514B93CCD3BB5501D7FDECD9C295E8C571B960E359CE28B88BC8C9A'}else{'ABB3D08DDAB7FE8068E79F81B0ED0256FE6564EAE4C099CF77238EA8E91A5CF3'}
$compilePin=if($Version -eq 4){'D1EDE7EED5F3EB47710510F8FA916D3E662BB921C69B75E6D68E6E78ED3F712A'}elseif($Version -eq 3){'548A1EA10753DAFD362995B1260A2256502297BA14E7A4B165A4F212B701A33F'}elseif($Version -eq 2){'8644A85D5CE1180FF8212CA06177FA10289DD47FB331D9BA763B18009CC468C2'}else{'5BAAF8AE0EADCD5DD66F35C8BBF09870D0279AEB1F17885B6C28C7757CF28DEE'}
if($Version -eq 5){$assemblyPin='ED384D347A9AB5E912B8A175244BCB9198731B4AFB8A71F461115A24678D3571';$compilePin='FF5A3E89AF5B6B068B4CE00825B50E2143FF73E5CED83B7C35E91D980093FE6A'}
if($Version -eq 6){$assemblyPin='C53A9E35544CD5675BACEF86B3FFDB44293FD0B92377BFDF71911FAC2BE5EFBD';$compilePin='BBBEA75438D601C23A5C312D6A5DB5246268DAB9B21116F5D226F9F993243CCA'}
if($Version -eq 7){$assemblyPin='DD8BAED18A7A189FD339A3D925669F4CAB076F58F717D2369AC37253C365AEFC';$compilePin='0995B44F7B12515DCEA7D98D8E1C3971961B63D3524F405B705463A9990518F4'}
if($Version -eq 8){$assemblyPin='D1E745E50AFE789D845BB43B27216BF52A555D0A29FF8E65F511AE5874430BCB';$compilePin='B23E98B3A79762D253256925E40E57D24CB85ED4B197C506DB0847ECE67D642F'}
if($RecoveryTrace){$assemblyPin='043777669F93998DC2D7FA51F66DBAAB7FD6B9612CD568AEFAE65C512B2D2C6C';$compilePin='DF32874875EF2F080A1DBD6B09C4B2AA131D8E5EA34E225B26683082D2B32222'}
if($Replay){$assemblyPin='6E8A9EBAEB14BC046E2DEDACC514FA0B5439DE8F261F4A2CE63209B2ED75A29F';$compilePin='B6203137AF1DE07B691E36A53843BCB3F0E0961CF680A131D0A26AD974213DCD'}
Pin (Join-Path $stage 'assembly.json') $assemblyPin
Pin (Join-Path $compile 'result.json') $compilePin
$assembly=Get-Content (Join-Path $stage 'assembly.json') -Raw|ConvertFrom-Json
$build=Get-Content (Join-Path $compile 'result.json') -Raw|ConvertFrom-Json
if(!$assembly.verified -or !$build.passed -or !$build.inputsUnchanged -or $build.ownerAttention -or $build.compiledSources -ne 60){throw 'Matched compilation incomplete'}
if($RecoveryTrace -and (!$assembly.diagnosticOnly -or !$build.diagnosticOnly -or !$assembly.fullPrefixRetained -or $assembly.changes.Count -ne 4 -or $assembly.entryFixtureDirectory -cne (Join-Path $out 'runtime/WorkerEntryFixture'))){throw 'Diagnostic source/resource boundary differs'}
if($Replay -and ($assembly.diagnosticOnly -or $build.diagnosticOnly -or !$assembly.uninstrumentedReplay -or !$build.uninstrumentedReplay -or !$assembly.fullPrefixRetained -or $assembly.changes.Count -ne 3 -or $assembly.entryFixtureDirectory -cne (Join-Path $out 'runtime/WorkerEntryFixture'))){throw 'Replay source/resource boundary differs'}
$selected=@($assembly.files|Where-Object {$_.path.StartsWith('runtime/') -or $_.path.StartsWith('fixtures/')})
$rows=[Collections.Generic.List[object]]::new()
foreach($file in $selected){$rows.Add(@{path=$file.path;origin=(Join-Path $stage $file.path);sha256=$file.sha256})}
foreach($file in $build.outputs){$rows.Add(@{path=('runtime/'+[IO.Path]::GetFileName($file.path));origin=$file.path;sha256=$file.sha256})}
$bytes=0L
foreach($row in $rows){Pin $row.origin $row.sha256;$bytes+=(Get-Item -LiteralPath $row.origin).Length}
if((Get-PSDrive C).Free -lt (600MB+$bytes)){throw 'Preserve staging reserve after copying candidate'}
foreach($row in $rows){
    if([IO.Path]::IsPathRooted($row.path) -or $row.path.Contains(':') -or $row.path.Split('/') -contains '..'){throw 'Runtime path escape'}
    $to=Join-Path $out $row.path
    New-Item -ItemType Directory -Path (Split-Path $to) -Force|Out-Null
    Copy-Item -LiteralPath $row.origin -Destination $to
    Pin $to $row.sha256
}
$manifests=@(Get-ChildItem -LiteralPath (Join-Path $out 'runtime/ComponentProbes') -Recurse -Filter manifest.json)
$manifests+=Get-Item -LiteralPath (Join-Path $out 'runtime/WorkerEntryProbe/manifest.json')
if($manifests.Count -ne 6){throw 'Six component manifests required'}
$closure=0
foreach($manifest in $manifests){
    $spec=Get-Content -LiteralPath $manifest.FullName -Raw|ConvertFrom-Json -AsHashtable
    foreach($name in $spec.files.Keys){
        if([IO.Path]::GetFileName($name) -cne $name){throw 'Component member escape'}
        Pin (Join-Path $manifest.DirectoryName $name) $spec.files[$name];$closure++
    }
    if($spec.fixtureFiles){foreach($name in $spec.fixtureFiles.Keys){
        if([IO.Path]::GetFileName($name) -cne $name){throw 'Entry fixture escape'}
        Pin (Join-Path $out ('runtime/WorkerEntryFixture/'+$name)) $spec.fixtureFiles[$name];$closure++
    }}
}
foreach($row in $rows){Pin $row.origin $row.sha256;Pin (Join-Path $out $row.path) $row.sha256}
@{verified=$true;executed=$false;diagnosticOnly=($Version -eq 4 -or [bool]$RecoveryTrace);uninstrumentedReplay=[bool]$Replay;files=$rows;runtime=(Join-Path $out 'runtime');binary=(Join-Path $out 'runtime/Lea.Script.Tests.exe');fixtureSource=(Join-Path $out 'fixtures');componentManifests=6;componentClosureFiles=$closure;bytes=$bytes;compiledSources=60;requiredChecks=3412;requiredPhases=89;pending=@('embedded fixture resource readback','owned launch preflight and unchanged ACL/byte checks','full300s/5s campaign');runnerSha256=(Get-FileHash $PSCommandPath).Hash}|ConvertTo-Json -Depth 6|Set-Content (Join-Path $out 'staging.json') -Encoding utf8
Get-FileHash (Join-Path $out 'staging.json')
