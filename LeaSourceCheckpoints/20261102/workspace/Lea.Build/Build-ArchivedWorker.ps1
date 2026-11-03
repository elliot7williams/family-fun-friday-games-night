param([switch]$KeepForegroundHost,[switch]$ProjectReferences,[string]$ExpectedReferenceReceipt,[switch]$NetworkProbeDiagnostic)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$outRoot=Join-Path $workspace $(if($NetworkProbeDiagnostic){'continuity-evidence/worker-network-probe-il-v1'}elseif($ProjectReferences){'continuity-evidence/worker-local-reference-build-v1'}else{'continuity-evidence/worker-archive-build-v1'})
if($NetworkProbeDiagnostic -and !$ProjectReferences){throw 'Diagnostic requires pinned project-local references'}
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained foreground owner required'}
if(Test-Path -LiteralPath $outRoot){throw 'Preserve worker build'}
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
$leases=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$Path,[string]$Hash){
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($stream)
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) -cne $Hash){throw "Worker input changed: $Path"}
    $inputs.Add(@{path=$Path;sha256=$Hash})
}
$result=[ordered]@{passed=$false;candidate=$outRoot;runs=$runs;inputs=$inputs;error=$null;runtimeExecuted=$false;releaseCertified=$false}
Space;$null=New-Item -ItemType Directory -Path $outRoot
try{
    $archive=Join-Path $workspace 'continuity-evidence/tested-worker-source-v1.zip'
    Hold $archive '25516AAAEDDEE8CF5FF058F343D8485F1AB8565C87FB546A7DC5F2A440863A79'
    $source=Join-Path $outRoot 'archive';[IO.Compression.ZipFile]::ExtractToDirectory($archive,$source)
    $map=Get-Content -LiteralPath (Join-Path $source 'manifest.json') -Raw|ConvertFrom-Json
    if($map.ilSha256 -cne '83FE295571FAFF00D4864A13C37D270B998B339EA27A61A950A3572A494AC2DB' -or $map.references.Count -ne 165 -or (Get-ChildItem -LiteralPath $source -Recurse -File).Count -ne 26){throw 'Worker archive inventory differs'}
    $known=@{};$copied=@{}
    foreach($file in $map.files){
        $path=[IO.Path]::GetFullPath((Join-Path $source $file.archivePath))
        if(!$path.StartsWith($source+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Archive path escapes source'}
        Hold $path $file.sha256
        if($copied.ContainsKey($file.originalPath)){throw 'Duplicate original input'}
        $copied[$file.originalPath]=$path
    }
    $relocated=@{}
    if($ProjectReferences){
        if($ExpectedReferenceReceipt -cnotmatch '^[A-F0-9]{64}$'){throw 'Exact restored-reference receipt required'}
        $refReceipt=Join-Path $workspace 'continuity-evidence/worker-reference-pack-8032-v1/result.json';Hold $refReceipt $ExpectedReferenceReceipt
        $refs=Get-Content $refReceipt -Raw|ConvertFrom-Json
        if(!$refs.passed -or $refs.files.Count -ne 163 -or $refs.systemInstallationChanged -or $refs.runtimeExecuted){throw 'Restored reference proof incomplete'}
        foreach($file in $refs.files){
            if($relocated.ContainsKey($file.originalPath)){throw 'Duplicate reference mapping'}
            $relocated[$file.originalPath]=$file
        }
    }
    foreach($reference in $map.references){
        $path=$reference.path
        if($relocated.ContainsKey($path)){
            $replacement=$relocated[$path]
            if($replacement.sha256 -cne $reference.sha256){throw 'Restored reference is not byte-identical'}
            $path=$replacement.path
        }
        Hold $path $reference.sha256;$known[$reference.path]=$reference.sha256
    }
    if($NetworkProbeDiagnostic){
        $helper=Join-Path $PSScriptRoot 'New-WorkerNetworkProbeSource.ps1';Hold $helper (Get-FileHash $helper).Hash
        . $helper
        $program=@($copied.Keys|Where-Object{[IO.Path]::GetFileName($_) -ceq 'Program.cs'})
        $probe=@($copied.Keys|Where-Object{[IO.Path]::GetFileName($_) -ceq 'NetworkProbe.cs'})
        if($program.Count -ne 1 -or $probe.Count -ne 1){throw 'Diagnostic source identities differ'}
        $overlay=New-WorkerNetworkProbeSource ([IO.File]::ReadAllText($copied[$program[0]])) ([IO.File]::ReadAllText($copied[$probe[0]]))
        if(!$overlay.inverseVerified){throw 'Diagnostic inverse missing'}
        $diagnostic=Join-Path $outRoot 'diagnostic-source';$null=New-Item -ItemType Directory $diagnostic
        foreach($pair in @(@{original=$program[0];leaf='Program.cs';text=$overlay.program},@{original=$probe[0];leaf='NetworkProbe.cs';text=$overlay.probe})){
            $target=Join-Path $diagnostic $pair.leaf
            [IO.File]::WriteAllText($target,$pair.text,[Text.UTF8Encoding]::new($false));Hold $target (Get-FileHash $target).Hash
            $copied[$pair.original]=$target
        }
        $result.diagnosticSourceInverseVerified=$true
        $result.scope=$overlay.scope
    }
    $binary=Join-Path $outRoot 'Lea.Script.Worker.dll';$options=[Collections.Generic.List[string]]::new();$sourceCount=0;$resources=0
    foreach($line in Get-Content -LiteralPath (Join-Path $source 'recipes/worker.rsp')){
        if($line.StartsWith('/out:')){$options.Add('/out:"'+$binary+'"')}
        elseif($line -match '^"(.+\.cs)"$'){
            if(!$copied.ContainsKey($Matches[1])){throw 'Unmapped worker source'}
            $options.Add('"'+$copied[$Matches[1]]+'"');$sourceCount++
        }elseif($line -match '^/resource:"([^"]+)",Lea.Script.Dom.js$'){
            if(!$copied.ContainsKey($Matches[1])){throw 'Unmapped worker resource'}
            $options.Add('/resource:"'+$copied[$Matches[1]]+'",Lea.Script.Dom.js');$resources++
        }elseif($line -match '^/reference:"([^"]+)"$'){
            if(!$known.ContainsKey($Matches[1])){throw 'Unpinned worker reference'}
            $path=$Matches[1]
            if($relocated.ContainsKey($path)){$line='/reference:"'+$relocated[$path].path+'"'}
            $options.Add($line)
        }else{$options.Add($line)}
    }
    if($sourceCount -ne 17 -or $resources -ne 1){throw 'Worker source/resource mapping differs'}
    $rsp=Join-Path $outRoot 'compile.rsp';[IO.File]::WriteAllLines($rsp,$options);$hash=(Get-FileHash -LiteralPath $rsp).Hash;Hold $rsp $hash
    $wrapper=Join-Path $PSScriptRoot $(if($ProjectReferences){'Invoke-PublicCompiler.20261026.ps1'}else{'Invoke-PublicCompiler.20261022.ps1'})
    Hold $wrapper $(if($ProjectReferences){'C3EBB3801FAFB80DA031AAB69FE4837EEB4E4EECD3907F9704F9D6186FA42F0A'}else{'1A04EE9A191D522955C633F97A24C9C0921A5738F76D0DF36DB1C92A574EEA03'})
    $run=Run 'compile-worker' (Get-Process -Id $PID).Path @('-NoProfile','-File',('"'+$wrapper+'"'),'-ResponseFile',('"'+$rsp+'"'),'-ExpectedResponseSha256',$hash,'-ExpectedSourceSha256','D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB') 120
    if(!$run.passed){throw 'Worker compilation failed'}
    $il=Join-Path $workspace 'continuity-evidence/bootstrap-descriptor-batches-v1/ParentResourceIlAudit.cs'
    $api=Join-Path $workspace 'continuity-evidence/modern-worker-v1/InterpreterAssemblyAudit.cs'
    Hold $il 'B2438457F0CA2F9F54A9EB866335DF7EF23D50FB84F134F13B968C4FB6F557CC'
    Hold $api '372098D022AF9E29B14CFF5AE57574F101B333AFFE4F94019720D315E443DAB9'
    Add-Type -Path @($il,$api)
    $original=Join-Path $workspace 'continuity-evidence/bootstrap-descriptor-batches-v1/paired-worker-il-v2/Lea.Script.Worker.dll'
    Hold $original $map.ilSha256
    if(!$NetworkProbeDiagnostic){$result.il=[ParentResourceIlAudit]::Compare($original,$binary)}
    $a=[InterpreterAssemblyAudit]::Read($original);$b=[InterpreterAssemblyAudit]::Read($binary)
    $properties=if($NetworkProbeDiagnostic){@('Resources','AssemblyReferences')}else{@('Api','Resources','AllMethods','AssemblyReferences','OptimizationMethods')}
    foreach($property in $properties){
        if(@(Compare-Object -ReferenceObject $a.$property -DifferenceObject $b.$property -CaseSensitive).Count){throw "Worker metadata/resource mismatch: $property"}
    }
    if($a.Identity -cne $b.Identity){throw 'Worker identity mismatch'}
    $result.binary=$binary;$result.sha256=(Get-FileHash -LiteralPath $binary).Hash
    $result.byteIdentical=$result.sha256 -ceq $map.ilSha256;$result.apiResourcesReferencesIdentical=!$NetworkProbeDiagnostic;$result.sources=$sourceCount;$result.passed=$true
    if($NetworkProbeDiagnostic){$result.resourcesAndReferencesIdentical=$true;$result.productionAdopted=$false}
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -gt 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if($result.ownerAttention){$global:LeaWorkerInputLeases=$leases}else{foreach($lease in $leases){$lease.Dispose()}}
    $global:LeaWorkerArchiveBuild=$result
    [IO.File]::WriteAllText((Join-Path $outRoot 'result.json'),($result|ConvertTo-Json -Depth 7))
}
[pscustomobject]$result|Select-Object passed,sha256,byteIdentical,ownerAttention,error|ConvertTo-Json
