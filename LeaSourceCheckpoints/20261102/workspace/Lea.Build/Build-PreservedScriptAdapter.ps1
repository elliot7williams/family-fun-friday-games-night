param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$outRoot=Join-Path $workspace 'continuity-evidence/script-adapter-source-build-v2'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained foreground owner required'}
if(Test-Path -LiteralPath $outRoot){throw 'Preserve adaptor rebuild'}
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
$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new()
function Hold([string]$Path,[string]$Hash){
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($stream)
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) -cne $Hash){throw "Adaptor build input changed: $Path"}
    $inputs.Add(@{path=$Path;sha256=$Hash})
}
$result=[ordered]@{passed=$false;candidate=$outRoot;inputs=$inputs;runs=$runs;error=$null;runtimeExecuted=$false;releaseCertified=$false;metadataReconstructedFromTestedAssembly=$true}
Space;$null=New-Item -ItemType Directory -Path $outRoot
try{
    $sourceRoot=Join-Path $workspace 'continuity-evidence/small-engine-v1/source/Lea.Script.Win'
    foreach($pair in @(@('W.cs','197F77D2A0085A0D00AE2B912E1C3ED06EC373B538392A81E3CBEB1B088B7509'),@('Lea.Script.Win.csproj','734C9E76E11B088A64BB16BF2DD1A1947AB25063B9335DB9310D15AD0FCD0880'),@('packages.lock.json','08CA1336C999E01F8802CAF4C4A453C1473E3796BAFAAB5AF3CC9CAEB51AEF06'))){
        $path=Join-Path $sourceRoot $pair[0];Hold $path $pair[1]
        Copy-Item -LiteralPath $path -Destination (Join-Path $outRoot $pair[0])
        Hold (Join-Path $outRoot $pair[0]) $pair[1]
    }
    $metadata=Join-Path $PSScriptRoot 'AdapterAssemblyInfo.cs';Hold $metadata (Get-FileHash -LiteralPath $metadata).Hash
    Copy-Item -LiteralPath $metadata -Destination (Join-Path $outRoot 'AssemblyInfo.cs')
    $parentRoot=Join-Path $workspace 'continuity-evidence/permission-contract-v1/early-limits-v1'
    $proofPath=Join-Path $parentRoot 'result.json'
    $proof=Get-Content -LiteralPath $proofPath -Raw|ConvertFrom-Json
    if(!$proof.passed -or !$proof.inputsUnchanged){throw 'Reference provenance incomplete'}
    Hold $proofPath (Get-FileHash -LiteralPath $proofPath).Hash
    $recipe=Join-Path $parentRoot 'compile.rsp';Hold $recipe (Get-FileHash -LiteralPath $recipe).Hash
    $options=[Collections.Generic.List[string]]::new()
    foreach($line in @('/nologo','/nostdlib+','/target:library','/langversion:12.0','/optimize+','/debug:portable','/deterministic+','/define:TRACE;RELEASE;NETFRAMEWORK;NET462')){$options.Add($line)}
    $binary=Join-Path $outRoot 'Lea.Script.Win.dll'
    $options.Add('/out:"'+$binary+'"');$options.Add('/pdb:"'+(Join-Path $outRoot 'adapter.pdb')+'"')
    foreach($line in Get-Content -LiteralPath $recipe|Where-Object{$_.StartsWith('/reference:')}){
        $path=$line.Substring(11).Trim('"')
        if([IO.Path]::GetFileName($path) -ceq 'Lea.Engine.dll'){continue}
        $pins=@($proof.inputs|Where-Object path -ceq $path|ForEach-Object sha256|Sort-Object -Unique)
        if($pins.Count -ne 1){throw 'Missing reference identity'}
        Hold $path $pins[0];$options.Add($line)
    }
    $runtime=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/host-source-rebuilt-v1/runtime'
    foreach($pair in @(@('Lea.Engine.dll','6D89A8F5953F2E5A197C5D789D8D828BF8FC084836EB30F22EB00D228B0591F8'),@('Lea.Win.dll','E26135F8CE6BA76A76515D2874DFE219E172B212037CD567AD37042DA516289B'),@('Lea.Script.dll','E68504B515405FA2C75B92904277A7A8074945BDCC02E1CAD10A57EB9A6FC1E5'))){
        $path=Join-Path $runtime $pair[0];Hold $path $pair[1];$options.Add('/reference:"'+$path+'"')
    }
    $options.Add('"'+(Join-Path $outRoot 'W.cs')+'"');$options.Add('"'+(Join-Path $outRoot 'AssemblyInfo.cs')+'"')
    $rsp=Join-Path $outRoot 'compile.rsp';[IO.File]::WriteAllLines($rsp,$options);$hash=(Get-FileHash -LiteralPath $rsp).Hash;Hold $rsp $hash
    $wrapper=Join-Path $PSScriptRoot 'Invoke-PublicCompiler.20261022.ps1';Hold $wrapper '1A04EE9A191D522955C633F97A24C9C0921A5738F76D0DF36DB1C92A574EEA03'
    $run=Run 'compile-adapter' (Get-Process -Id $PID).Path @('-NoProfile','-File',('"'+$wrapper+'"'),'-ResponseFile',('"'+$rsp+'"'),'-ExpectedResponseSha256',$hash,'-ExpectedSourceSha256','D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB') 120
    if(!$run.passed){throw 'Adaptor compilation failed'}
    $result.binary=$binary;$result.sha256=(Get-FileHash -LiteralPath $binary).Hash;$result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object{(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -gt 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if($result.ownerAttention){$global:LeaAdapterInputLeases=$leases}else{foreach($lease in $leases){$lease.Dispose()}}
    $global:LeaAdapterBuildResult=$result
    [IO.File]::WriteAllText((Join-Path $outRoot 'result.json'),($result|ConvertTo-Json -Depth 7))
}
[pscustomobject]$result|Select-Object passed,sha256,ownerAttention,error|ConvertTo-Json
