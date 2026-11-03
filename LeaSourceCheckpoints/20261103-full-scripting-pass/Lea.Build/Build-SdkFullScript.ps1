param([switch]$KeepForegroundHost,[ValidateRange(1,8)][int]$Version=1,[switch]$RecoveryTrace,[switch]$Replay,[switch]$PrivateBindings,[ValidatePattern('^[A-F0-9]{64}$')][string]$PrivateBindingAssemblySha256,[ValidateRange(1,3)][int]$PrivateBindingVersion=1)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$stage='I:\LeaBuildStorage\sdk-full-script-assembly-20261101-v'+$Version
$preparation='I:\LeaBuildStorage\sdk-full-script-preparation-20261101-v1'
$out='I:\LeaBuildStorage\sdk-full-script-compile-20261101-v'+$Version
if($RecoveryTrace){if($Version -ne 8){throw 'Recovery trace requires deterministic candidate'};$stage='I:\LeaBuildStorage\contains-recovery-assembly-20261102-v1';$out='I:\LeaBuildStorage\contains-recovery-compile-20261102-v1'}
if($Replay){if($Version -ne 8 -or $RecoveryTrace){throw 'Replay requires separate deterministic mode'};$stage='I:\LeaBuildStorage\deterministic-replay-assembly-20261102-v1';$out='I:\LeaBuildStorage\deterministic-replay-compile-20261102-v1'}
if($PrivateBindings){
    if($Version -ne 8 -or $RecoveryTrace -or $Replay -or !$PrivateBindingAssemblySha256){throw 'Private bindings require separate pinned full-suite mode'}
    $stage='I:\LeaBuildStorage\private-binding-full-assembly-20261103-v'+$PrivateBindingVersion
    if($PrivateBindingVersion -ge 2){$stage+='-assets'}
    $out='I:\LeaBuildStorage\private-binding-full-compile-20261103-v'+$PrivateBindingVersion
}elseif($PrivateBindingAssemblySha256){throw 'Unexpected private-binding assembly pin'}
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin owner required'}
if((Get-PSDrive C).Free -lt 600MB -or (Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/evidence and reserve required'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$path,[string]$pin){
    $item=Get-Item -LiteralPath $path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse compiler input refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($pin -and $pin -cne $hash){throw ('Compile input differs: '+$path)}
    $inputs.Add(@{path=$path;sha256=$hash})
}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;inputs=$inputs;runs=$runs;executed=$false;completeSdk=$false;requiredChecks=3412;requiredPhases=89}
$result.diagnosticOnly=$Version -eq 4 -or [bool]$RecoveryTrace
$result.uninstrumentedReplay=[bool]$Replay
try{
    Hold $PSCommandPath
    Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'build.ps1')
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owned
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $assemblyPin=if($Version -eq 5){'ED384D347A9AB5E912B8A175244BCB9198731B4AFB8A71F461115A24678D3571'}elseif($Version -eq 4){'54CACB2CA1D0939791C101A0FB97D79CF0D0C2B123633EF5B5A017F5874B9A3F'}elseif($Version -eq 3){'045F8EAF988199AC86119C0EAB552EF4A9E90C02ACAB5ACE2CBB8C3CD00E9992'}elseif($Version -eq 2){'EA20FF493514B93CCD3BB5501D7FDECD9C295E8C571B960E359CE28B88BC8C9A'}else{'ABB3D08DDAB7FE8068E79F81B0ED0256FE6564EAE4C099CF77238EA8E91A5CF3'}
    if($Version -eq 6){$assemblyPin='C53A9E35544CD5675BACEF86B3FFDB44293FD0B92377BFDF71911FAC2BE5EFBD'}
    if($Version -eq 7){$assemblyPin='DD8BAED18A7A189FD339A3D925669F4CAB076F58F717D2369AC37253C365AEFC'}
    if($Version -eq 8){$assemblyPin='D1E745E50AFE789D845BB43B27216BF52A555D0A29FF8E65F511AE5874430BCB'}
    if($RecoveryTrace){$assemblyPin='AFFDEA3E82AE42779C1ABA5292D8E40847D72CB4A8C4E8B61F0B1E99E2BE196F'}
    if($Replay){$assemblyPin='6E8A9EBAEB14BC046E2DEDACC514FA0B5439DE8F261F4A2CE63209B2ED75A29F'}
    if($PrivateBindings){$assemblyPin=$PrivateBindingAssemblySha256}
    $manifest=Join-Path $stage 'assembly.json';Hold $manifest $assemblyPin
    $assembly=Get-Content $manifest -Raw|ConvertFrom-Json
    $privateAssetsMatch=$PrivateBindings -and $PrivateBindingVersion -ge 2 -and $assembly.testFixturesUnchanged -and $assembly.productAssetChanges.Count -eq 2 -and @($assembly.productAssetChanges|Where-Object {$_.path -ceq 'fixtures/Lea.Script/Dom.js' -and $_.sha256 -ceq 'A228494E1D20D2669DC2FED38B93B7549EF8849FA2C7997F45EE32678117239A'}).Count -eq 1 -and @($assembly.productAssetChanges|Where-Object {$_.path -ceq 'fixtures/Lea.Script/Dom.compact.js' -and $_.sha256 -ceq '196BCE962F2C2F375C77644D0AE19485820F599170D4D80013829A6ECD2886E3'}).Count -eq 1
    if($PrivateBindings -and (!$assembly.readyForSuiteCompilation -or $assembly.assertionsChanged -or (!$assembly.fixturesUnchanged -and !$privateAssetsMatch) -or $assembly.preparationChanges.Count -ne 17 -or $assembly.entryChanges.Count -ne 6 -or $assembly.parentSha256 -cne '406C5634638D75C66892BD95429760CB4A5C74C94277913D1DF467147E30E75F' -or $assembly.workerSha256 -cne 'F61B6858822F8F7BB0350B91D21862C6D22EC955EE94E2E4F4060FA01FB344F0')){throw 'Matching private-binding assembly boundary differs'}
    $result.privateBindingPair=[bool]$PrivateBindings
    if($PrivateBindings -and $PrivateBindingVersion -ge 2 -and ($assembly.privateBindingVersion -ne $PrivateBindingVersion -or $assembly.probeChanges.Count -ne 10)){throw 'Ten rebuilt probe products required for v2'}
    if($RecoveryTrace -and (!$assembly.diagnosticOnly -or !$assembly.fullPrefixRetained -or !$assembly.containsInverseVerified -or !$assembly.dispatcherInverseVerified -or $assembly.changes.Count -ne 4)){throw 'Recovery trace source boundary incomplete'}
    if($Replay -and ($assembly.diagnosticOnly -or !$assembly.uninstrumentedReplay -or !$assembly.fullPrefixRetained -or !$assembly.dispatcherInverseVerified -or $assembly.changes.Count -ne 3)){throw 'Uninstrumented replay source boundary incomplete'}
    if(!$assembly.verified -or !$assembly.baselineTestBinariesExcluded -or $assembly.compiledSources -ne 60 -or $assembly.fixtures -ne 182){throw 'Assembly incomplete'}
    $members=[Collections.Generic.Dictionary[string,object]]::new([StringComparer]::OrdinalIgnoreCase)
    foreach($file in $assembly.files){if($members.ContainsKey($file.path)){throw 'Duplicate assembly member'};$members.Add($file.path,$file);Hold (Join-Path $stage $file.path) $file.sha256}
    $proof=Join-Path $preparation 'evidence/baseline-build.json';Hold $proof '39B11B7FFB696DED434C9E0313DEED6744E0A3A08590BB8B14977C064EA6608C'
    $baseline=Get-Content $proof -Raw|ConvertFrom-Json
    $old=Split-Path $baseline.runtime -Parent
    $recipe=Join-Path $preparation 'evidence/baseline-tests.rsp';Hold $recipe 'DF5DEEA4C810B46BB1A8D7925E9BC676EDF5EFBDA201EFC29431B1FE6C64F048'
    $options=[Collections.Generic.List[string]]::new();$sources=0;$references=0;$outputs=0;$symbols=0
    foreach($line in Get-Content $recipe){
        if($line -match '^"(.+\.cs)"$'){
            $relative='compiled-source/'+[IO.Path]::GetRelativePath($old,$Matches[1]).Replace('\','/')
            if(!$members.ContainsKey($relative)){throw 'Compiled source absent from assembly'}
            $options.Add('"'+(Join-Path $stage $relative)+'"');$sources++;continue
        }
        if($line -match '^/reference:"(.+)"$'){
            $from=$Matches[1]
            if($from.StartsWith($baseline.runtime+'\',[StringComparison]::OrdinalIgnoreCase)){
                $relative='runtime/'+[IO.Path]::GetRelativePath($baseline.runtime,$from).Replace('\','/')
                if(!$members.ContainsKey($relative)){throw 'Runtime reference absent'}
                $from=Join-Path $stage $relative
            }else{
                $pins=@($baseline.inputs|Where-Object path -ceq $from|Select-Object -ExpandProperty sha256 -Unique)
                if($pins.Count -ne 1){throw ('Reference provenance absent: '+$from)}
                Hold $from $pins[0]
            }
            $options.Add('/reference:"'+$from+'"');$references++;continue
        }
        if($line.StartsWith('/out:')){$options.Add('/out:"'+(Join-Path $out 'Lea.Script.Tests.exe')+'"');$outputs++;continue}
        if($line.StartsWith('/pdb:')){$options.Add('/pdb:"'+(Join-Path $out 'Lea.Script.Tests.pdb')+'"');$symbols++;continue}
        if(!$line.StartsWith('/')){throw 'Unrecognized compiler recipe member'}
        $options.Add($line)
    }
    if($sources -ne 60 -or $references -lt 1 -or $outputs -ne 1 -or $symbols -ne 1){throw 'Compiler recipe inventory differs'}
    $result.compiledSources=$sources;$result.references=$references;$result.assertionsChanged=$false
    $rsp=Join-Path $out 'tests.rsp';[IO.File]::WriteAllLines($rsp,$options,[Text.UTF8Encoding]::new($false));Hold $rsp
    $compiler='C:\Program Files\dotnet\dotnet.exe';Hold $compiler
    $toolchain='C:\Program Files\dotnet\sdk\8.0.427\Roslyn\bincore'
    foreach($name in @('csc.dll','csc.deps.json','csc.runtimeconfig.json','Microsoft.CodeAnalysis.dll','Microsoft.CodeAnalysis.CSharp.dll')){Hold (Join-Path $toolchain $name)}
    $run=Invoke-OwnedTestRun -Name 'compile-full-suite' -Application $compiler -Arguments @(('"'+(Join-Path $toolchain 'csc.dll')+'"'),('@"'+$rsp+'"')) -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$run.passed){throw 'Matched SDK full suite compilation failed'}
    $result.outputs=@(foreach($name in @('Lea.Script.Tests.exe','Lea.Script.Tests.pdb')){$path=Join-Path $out $name;Hold $path;@{path=$path;sha256=(Get-FileHash $path).Hash}})
    $result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($s in $holds){$s.Dispose()}}else{$global:LeaSdkFullCompileLeases=$holds}
    $result|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
