param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$outRoot=Join-Path $workspace 'continuity-evidence/worker-kit-relocation-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained foreground owner required'}
if(Test-Path -LiteralPath $outRoot){throw 'Preserve relocated kit test'}
$utility=Join-Path $workspace 'continuity-evidence/native-launch-timing-v1/Test-CompositeCandidate.v2.ps1'
if((Get-FileHash -LiteralPath $utility).Hash -cne 'EB00AD17B8078A6CC2F418733BA5D675FC84EF49EED433A527FAC5498DD5ECEF'){throw 'Utility changed'}
$t=$null;$e=$null;$ast=[Management.Automation.Language.Parser]::ParseFile($utility,[ref]$t,[ref]$e)
foreach($name in @('Space','NewOutputReader','PumpOutput','Run')){
    $defs=@($ast.FindAll({param($n)$n -is [Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -ceq $name},$false))
    if($e.Count -or $defs.Count -ne 1){throw 'Utility shape changed'}
    . ([scriptblock]::Create($defs[0].Extent.Text))
}
if(Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue){throw 'Fresh owner required'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$runs=[Collections.Generic.List[object]]::new();$inputs=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new()
function Hold([string]$Path,[string]$Hash){
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($stream)
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) -cne $Hash){throw 'Kit test input differs'}
    $inputs.Add(@{path=$Path;sha256=$Hash})
}
$result=[ordered]@{passed=$false;candidate=(Join-Path $outRoot 'relocated-kit');inputs=$inputs;runs=$runs;runtimeExecuted=$false;cleanPcValidated=$false;error=$null}
Space;$null=New-Item -ItemType Directory -Path $outRoot
try{
    $archive=Join-Path $workspace 'continuity-evidence/worker-build-kit-v1/Lea-worker-IL-build-kit.zip'
    Hold $archive 'B4B056AE959E1A1DEFBA0438C3629CAE4A6F7B92B498439DC9B066C8DFC9D6B3'
    [IO.Compression.ZipFile]::ExtractToDirectory($archive,$result.candidate)
    $manifest=Join-Path $result.candidate 'manifest.json';Hold $manifest '64853AE5B43313FF5DE9EE394C69F6DFC8154415FE599F193C990D2378E46E16'
    $map=Get-Content -LiteralPath $manifest -Raw|ConvertFrom-Json
    if($map.files.Count -ne 186 -or (Get-ChildItem -LiteralPath $result.candidate -Recurse -File).Count -ne 187){throw 'Kit extraction inventory differs'}
    foreach($file in $map.files){
        $path=[IO.Path]::GetFullPath((Join-Path $result.candidate $file.path))
        if(!$path.StartsWith($result.candidate+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Kit path escapes extraction'}
        Hold $path $file.sha256
    }
    $emit=Join-Path $outRoot 'emit'
    $run=Run 'relocated-worker-build' (Get-Process -Id $PID).Path @('-NoProfile','-File',('"'+(Join-Path $result.candidate 'Build.ps1')+'"'),'-KitRoot',('"'+$result.candidate+'"'),'-OutputDirectory',('"'+$emit+'"'),'-ExpectedManifestSha256','64853AE5B43313FF5DE9EE394C69F6DFC8154415FE599F193C990D2378E46E16') 120
    if(!$run.passed){throw 'Relocated worker compilation failed'}
    $binary=Join-Path $emit 'Lea.Script.Worker.dll'
    $original=Join-Path $workspace 'continuity-evidence/worker-archive-build-v1/Lea.Script.Worker.dll'
    Hold $original '83FE295571FAFF00D4864A13C37D270B998B339EA27A61A950A3572A494AC2DB'
    $result.sha256=(Get-FileHash -LiteralPath $binary).Hash
    $result.byteIdentical=[Convert]::ToBase64String([IO.File]::ReadAllBytes($original)) -ceq [Convert]::ToBase64String([IO.File]::ReadAllBytes($binary))
    $rsp=[IO.File]::ReadAllText((Join-Path $emit 'compile.rsp'))
    $result.noOriginalCheckoutOrSdkPaths=!$rsp.Contains($workspace+'\continuity-evidence\modern-worker-v1') -and !$rsp.Contains('C:\Program Files\dotnet\')
    $result.passed=$result.byteIdentical -and $result.noOriginalCheckoutOrSdkPaths -and $result.sha256 -ceq '83FE295571FAFF00D4864A13C37D270B998B339EA27A61A950A3572A494AC2DB'
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -gt 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if($result.ownerAttention){$global:LeaKitInputLeases=$leases}else{foreach($lease in $leases){$lease.Dispose()}}
    $global:LeaKitBuildResult=$result
    [IO.File]::WriteAllText((Join-Path $outRoot 'result.json'),($result|ConvertTo-Json -Depth 7))
}
[pscustomobject]$result|Select-Object passed,sha256,byteIdentical,noOriginalCheckoutOrSdkPaths,ownerAttention,error|ConvertTo-Json
