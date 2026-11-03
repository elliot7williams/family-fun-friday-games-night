param([switch]$KeepForegroundHost,[ValidateRange(1,3)][int]$Version=1,[switch]$PrivateBindings,[ValidatePattern('^[A-F0-9]{64}$')][string]$PrivateBindingSourceSha256)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$stage='I:\LeaBuildStorage\sdk-entry-core-source-20261101-v'+$Version
$out='I:\LeaBuildStorage\sdk-entry-core-20261101-v'+$Version
if($PrivateBindings){if($Version -ne 3 -or !$PrivateBindingSourceSha256){throw 'Private bindings require separate pinned source mode'};$stage='I:\LeaBuildStorage\private-binding-entry-core-source-20261103-v1';$out='I:\LeaBuildStorage\private-binding-entry-core-20261103-v1'}elseif($PrivateBindingSourceSha256){throw 'Unexpected private binding pin'}
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Get-PSDrive C).Free -lt 600MB -or (Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/evidence/reserve required'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$path,[string]$pin){
    $item=Get-Item -LiteralPath $path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse build input refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s);$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($pin -and $hash -cne $pin){throw 'Entry core build input differs'}
    $inputs.Add(@{path=$path;sha256=$hash})
}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;inputs=$inputs;runs=$runs;executed=$false;assertionsChanged=$false;referencePackProjection='8.0.32 to installed8.0.33';completeSdk=$false}
try{
    Hold $PSCommandPath
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owned
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $sourcePin=if($Version -eq 2){'55499C1D1959A08377AD2648DA9ECD29272FBD5FDB5908930A4186683829ACD9'}else{'9FC12C48C31CFD02AF205496A1C09E44EC85053B3C874A6BDF8C2E9C84A3358B'}
    if($Version -eq 3){$sourcePin='62A0530A1CF84947D657779795047EB6BF10599357091EDC37C4CB4A011A2054'}
    if($PrivateBindings){$sourcePin=$PrivateBindingSourceSha256}
    $proof=Join-Path $stage 'source.json';Hold $proof $sourcePin;$source=Get-Content $proof -Raw|ConvertFrom-Json
    if(!$source.verified -or !$source.inverseVerified -or $source.identityReplacements -ne 1 -or $source.negativeControls -ne 3){throw 'Verified companion source required'}
    Hold $source.source $source.sha256
    if($PrivateBindings -and ([regex]::Matches([IO.File]::ReadAllText($source.source),'F61B6858822F8F7BB0350B91D21862C6D22EC955EE94E2E4F4060FA01FB344F0').Count -ne 1 -or $source.assertionsChanged)){throw 'Private worker core identity differs'}
    $result.privateBindingPair=[bool]$PrivateBindings
    $recipe=Join-Path $stage 'original-core.rsp';Hold $recipe $source.recipeSha256
    $options=[Collections.Generic.List[string]]::new();$sources=0;$outputs=0;$refs=0
    $dll=Join-Path $out 'WorkerEntryCore.dll'
    foreach($line in Get-Content $recipe){
        if($line -match '^"(.+\.cs)"$'){
            if([IO.Path]::GetFileName($Matches[1]) -cne 'WorkerEntryCore.cs'){throw 'Unexpected companion source'}
            $options.Add('"'+$source.source+'"');$sources++;continue
        }
        if($line -match '^/reference:"(.+)"$'){
            $from=$Matches[1];$prefix='C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.32\ref\net8.0\'
            if(!$from.StartsWith($prefix,[StringComparison]::OrdinalIgnoreCase)){throw 'Unexpected companion reference'}
            $from='C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.33\ref\net8.0\'+$from.Substring($prefix.Length);Hold $from
            $options.Add('/reference:"'+$from+'"');$refs++;continue
        }
        if($line.StartsWith('/out:')){$options.Add('/out:"'+$dll+'"');$outputs++;continue}
        if(!$line.StartsWith('/')){throw 'Unexpected companion compiler option'}
        $options.Add($line)
    }
    if($sources -ne 1 -or $outputs -ne 1 -or !$refs){throw 'Companion compiler inventory differs'}
    $compiler='C:\Program Files\dotnet\dotnet.exe';Hold $compiler
    $toolchain='C:\Program Files\dotnet\sdk\8.0.427\Roslyn\bincore'
    foreach($name in @('csc.dll','csc.deps.json','csc.runtimeconfig.json','Microsoft.CodeAnalysis.dll','Microsoft.CodeAnalysis.CSharp.dll')){Hold (Join-Path $toolchain $name)}
    $rsp=Join-Path $out 'core.rsp';[IO.File]::WriteAllLines($rsp,$options,[Text.UTF8Encoding]::new($false));Hold $rsp
    $run=Invoke-OwnedTestRun -Name 'compile-entry-core' -Application $compiler -Arguments @(('"'+(Join-Path $toolchain 'csc.dll')+'"'),('@"'+$rsp+'"')) -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$run.passed){throw 'Entry core compilation failed'}
    $fixture='I:\LeaBuildStorage\sdk-full-script-assembly-20261101-v2\runtime\WorkerEntryFixture'
    foreach($pair in @(@('WorkerEntryCore.exe','C3AB2E109090D2AE680D62A6E9403FF5E5A46C5C23FC4E987E978E0FCED17DBD'),@('WorkerEntryCore.runtimeconfig.json','6346264D0ED421CB89893F36E6760048A72CCCEDC5A4B8BDB26B6048D6F766F6'))){
        $from=Join-Path $fixture $pair[0];Hold $from $pair[1];Copy-Item -LiteralPath $from -Destination (Join-Path $out $pair[0]);Hold (Join-Path $out $pair[0]) $pair[1]
    }
    Hold $dll;$result.references=$refs
    $result.outputs=@(foreach($name in @('WorkerEntryCore.dll','WorkerEntryCore.exe','WorkerEntryCore.runtimeconfig.json')){$path=Join-Path $out $name;@{path=$path;sha256=(Get-FileHash $path).Hash}})
    $result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($s in $holds){$s.Dispose()}}else{$global:LeaSdkEntryCoreBuildLeases=$holds}
    $result|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
