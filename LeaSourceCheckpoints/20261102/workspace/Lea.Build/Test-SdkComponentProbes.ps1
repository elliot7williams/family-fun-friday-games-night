param([switch]$KeepForegroundHost,[ValidateRange(1,4)][int]$Version=1)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$build='I:\LeaBuildStorage\sdk-component-probes-20261101-v'+$Version
$assembly='I:\LeaBuildStorage\sdk-full-script-assembly-20261101-v1'
$out=Join-Path $root ('continuity-evidence/sdk-component-probes-runtime-v'+$Version)
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Get-PSDrive C).Free -lt 600MB -or (Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/evidence/reserve required'}
if($Version -ge 3 -and @(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new();$reports=[Collections.Generic.List[object]]::new()
function Hold([string]$path,[string]$pin){$s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s);$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s));if($pin -and $hash -cne $pin){throw 'Component runtime input differs'};$inputs.Add(@{path=$path;sha256=$hash})}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;inputs=$inputs;runs=$runs;reports=$reports;workerEntryPointExecuted=$false;pageCodeEvaluated=$false;nativeIsolationTest=$false;fullSuitePassed=$false;before=@()}
try{
    Hold $PSCommandPath
    $buildPin=if($Version -eq 2){'3A4F36BE459CC0CDB82AAE9D149C806909AF56062197BB1773DC6B817090CF7B'}else{'70C34D0AA31091B67BC76D04391320DCE8712542D3DC50DE55B28DA112135447'}
    if($Version -eq 3){$buildPin='436983AEF289AC8B205F8878FBF7BD31FCFF079FB14F9B99B6D3DCB26F7FC953'}
    if($Version -eq 4){$buildPin='97CD0F8118FF656CB7A14B5F2451EC87CE3F7A3B0627B22B5D757D3D76C352D7'}
    $proof=Join-Path $build 'result.json';Hold $proof $buildPin;$compiled=Get-Content $proof -Raw|ConvertFrom-Json
    if(!$compiled.passed -or !$compiled.inputsUnchanged -or $compiled.ownerAttention -or !$compiled.dispatcherInverseVerified -or $compiled.runs.Count -ne 5){throw 'Compiled probe set incomplete'}
    $workerHash='F8BE188734FB0B747C131E396269E9698A02E257ED2DCF6DCB1DB7DECADFC81C'
    if($Version -eq 3){$workerHash='5BF8B589731CB1A0A5DE38B0DAE1AF516DBF21DB5982141C9C013C55E6D39596';if($compiled.workerModule.sha256 -cne $workerHash){throw 'Combined component worker identity differs'};Hold $compiled.workerModule.path $workerHash}
    if($Version -eq 4){$workerHash='E777E0CAE71D694BBA96E43E01BEED102B2B512B0B24B4ABE11AF2CDB3E7C2C6';if($compiled.workerModule.sha256 -cne $workerHash){throw 'Deterministic component worker identity differs'};Hold $compiled.workerModule.path $workerHash}
    $assemblyProof=Join-Path $assembly 'assembly.json';Hold $assemblyProof 'ABB3D08DDAB7FE8068E79F81B0ED0256FE6564EAE4C099CF77238EA8E91A5CF3';$members=(Get-Content $assemblyProof -Raw|ConvertFrom-Json).files
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owned
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $hostPath='C:\Program Files\dotnet\dotnet.exe';Hold $hostPath
    $files=[Collections.Generic.List[object]]::new();$specs=[Collections.Generic.List[object]]::new()
    foreach($change in $compiled.dispatcherChanges){
        $component=$change.component;$directory=Join-Path $out $component;New-Item -ItemType Directory -Path $directory|Out-Null
        $manifestPath=Join-Path $build ($component+'/manifest.json');Hold $manifestPath $change.replacement
        $spec=Get-Content $manifestPath -Raw|ConvertFrom-Json -AsHashtable
        foreach($name in $spec.files.Keys){
            if([IO.Path]::GetFileName($name) -cne $name){throw 'Component member escape'}
            $from=if($name -ceq $spec.binary){Join-Path $build ($component+'/'+$name)}elseif($Version -ge 3 -and $name -ceq 'Lea.Script.Worker.dll'){$compiled.workerModule.path}else{Join-Path $assembly ('runtime/ComponentProbes/'+$component+'/'+$name)}
            Hold $from $spec.files[$name]
            $to=Join-Path $directory $name;Copy-Item -LiteralPath $from -Destination $to;Hold $to $spec.files[$name]
            $files.Add(@{path=$to;sha256=$spec.files[$name];sddl=(Get-Acl -LiteralPath $to).Sddl})
        }
        $specs.Add(@{component=$component;directory=$directory;spec=$spec})
    }
    $result.before=$files
    foreach($row in $specs){
        $spec=$row.spec;$binary=Join-Path $row.directory $spec.binary;$worker=Join-Path $row.directory 'Lea.Script.Worker.dll'
        $run=Invoke-OwnedTestRun -Name ('test-'+$row.component) -Application $hostPath -Arguments @(('"'+$binary+'"'),('"'+$worker+'"')) -WorkingDirectory $row.directory -OutputDirectory $out -Seconds 30 -Records $runs
        if(!$run.passed -or (Get-Item $run.stderr).Length -ne 0){throw ('Component runtime failed: '+$row.component)}
        $report=Get-Content $run.stdout -Raw|ConvertFrom-Json
        if(!$report.passed -or $report.componentSha256 -cne $workerHash -or $report.sourceSuiteSha256 -cne $spec.sourceSuiteSha256 -or !$report.componentOnly -or $report.workerEntryPointExecuted -or $report.pageCodeEvaluated -or $report.nativeIsolationTest -or $report.checks.Count -ne $spec.checks -or @($report.checks|Where-Object {!$_.passed -or !$_.name}).Count){throw 'Component report identity/coverage/scope differs'}
        $reports.Add(@{component=$row.component;checks=$report.checks.Count;componentSha256=$report.componentSha256;sourceSuiteSha256=$report.sourceSuiteSha256})
    }
    if($reports.Count -ne 5){throw 'All five probes required'}
    $result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.changedFiles=@($result.before|Where-Object {(Get-Acl -LiteralPath $_.path).Sddl -cne $_.sddl -or (Get-FileHash $_.path).Hash -cne $_.sha256})
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.changedFiles.Count -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($s in $holds){$s.Dispose()}}else{$global:LeaSdkProbeRuntimeLeases=$holds}
    $result|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
