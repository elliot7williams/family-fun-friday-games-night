param([switch]$KeepForegroundHost,[switch]$ProjectReferences,[string]$ExpectedManagedReceipt,[switch]$NetworkProbeDiagnostic)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/temporal-startup-v1'
$outRoot=Join-Path $workspace $(if($NetworkProbeDiagnostic){'continuity-evidence/worker-network-probe-native-v1'}elseif($ProjectReferences){'continuity-evidence/worker-local-reference-native-v1'}else{'continuity-evidence/worker-archive-native-v2'})
if($NetworkProbeDiagnostic -and !$ProjectReferences){throw 'Diagnostic requires pinned project-local managed build'}
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained foreground owner required'}
if(Test-Path -LiteralPath $outRoot){throw 'Preserve native rebuild'}
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
function Hold([string]$Path,[string]$Expected){
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($stream)
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) -cne $Expected){throw "Changed native input: $Path"}
    $inputs.Add(@{path=$Path;sha256=$Expected})
}
$result=[ordered]@{passed=$false;candidate=$outRoot;inputs=$inputs;runs=$runs;error=$null;runtimeExecuted=$false;releaseCertified=$false}
Space;$null=New-Item -ItemType Directory -Path $outRoot
try{
    $verification=Join-Path $workspace 'continuity-evidence/worker-archive-build-v1/result.json'
    Hold $verification '15A9162F4B9E06F0D8455FC05D2250C62DC57127BACF56F9CAB6FC39FCADA75D'
    $proofPath=Join-Path $workspace 'continuity-evidence/worker-archive-build-v1/archive/proofs/native-build.json'
    Hold $proofPath '896375B3D0BA4DD7C26EEB9B933C5548201B15AC5669F363039EBD7EA8C3AFFF'
    $proof=Get-Content -LiteralPath $proofPath -Raw|ConvertFrom-Json
    if(!$proof.passed -or !$proof.inputsUnchanged -or $proof.ownerAttention -or $proof.inputs.Count -ne 200){throw 'Native provenance incomplete'}
    foreach($file in $proof.inputs){Hold $file.path $file.sha256}
    if($ProjectReferences){
        if($ExpectedManagedReceipt -cnotmatch '^[A-F0-9]{64}$'){throw 'Exact managed build receipt required'}
        $managedPath=Join-Path $workspace $(if($NetworkProbeDiagnostic){'continuity-evidence/worker-network-probe-il-v1/result.json'}else{'continuity-evidence/worker-local-reference-build-v1/result.json'});Hold $managedPath $ExpectedManagedReceipt
        $managed=Get-Content $managedPath -Raw|ConvertFrom-Json
        if(!$managed.passed -or !$managed.inputsUnchanged -or $managed.ownerAttention -or $managed.sources -ne 17){throw 'Local-reference managed build incomplete'}
        if($NetworkProbeDiagnostic){if(!$managed.diagnosticSourceInverseVerified -or !$managed.resourcesAndReferencesIdentical -or $managed.productionAdopted){throw 'Diagnostic provenance incomplete'}}
        elseif(!$managed.byteIdentical){throw 'Exact original managed worker required'}
    }
    $input=Join-Path $workspace $(if($NetworkProbeDiagnostic){'continuity-evidence/worker-network-probe-il-v1/Lea.Script.Worker.dll'}elseif($ProjectReferences){'continuity-evidence/worker-local-reference-build-v1/Lea.Script.Worker.dll'}else{'continuity-evidence/worker-archive-build-v1/Lea.Script.Worker.dll'})
    Hold $input $(if($NetworkProbeDiagnostic){$managed.sha256}else{'83FE295571FAFF00D4864A13C37D270B998B339EA27A61A950A3572A494AC2DB'})
    $original=Join-Path $workspace 'continuity-evidence/bootstrap-descriptor-batches-v1/paired-worker-native-v1/Lea.Script.Worker.dll'
    Hold $original '60FF26D8357FEA52C802BAC562C30C6CE28F72F735E78281D0908E94FAB530D6'
    $recipe=Join-Path $workspace 'continuity-evidence/worker-archive-build-v1/archive/recipes/native.rsp'
    Hold $recipe 'AABDDE8A810BA4143501045AA128E4500479EE2273269F08353F88143AF83F03'
    $lines=[IO.File]::ReadAllLines($recipe);$options=[Collections.Generic.List[string]]::new();$outputs=0
    $binary=Join-Path $outRoot 'Lea.Script.Worker.dll'
    for($i=0;$i -lt $lines.Length;$i++){
        $line=$lines[$i]
        if($i -eq 0){$line='"'+$input+'"'}
        elseif($line.StartsWith('-o:')){$line='-o:"'+$binary+'"';$outputs++}
        $options.Add($line)
    }
    if($outputs -ne 1){throw 'Native output count differs'}
    $rsp=Join-Path $outRoot 'native.rsp';[IO.File]::WriteAllLines($rsp,$options,[Text.UTF8Encoding]::new($false));Hold $rsp (Get-FileHash -LiteralPath $rsp).Hash
    $compiler=Join-Path $workspace 'continuity-evidence/modern-worker-v1/crossgen2-package-v4/extracted/tools/crossgen2.exe'
    $compile=Run 'native-rebuild' $compiler @(('"@'+$rsp+'"')) 120
    if(!$compile.passed){throw 'Native compilation failed'}
    $helper=Join-Path $workspace 'continuity-evidence/bootstrap-descriptor-batches-v1/ParentResourceIlAudit.cs'
    Hold $helper 'B2438457F0CA2F9F54A9EB866335DF7EF23D50FB84F134F13B968C4FB6F557CC'
    Add-Type -Path $helper
    $result.ilComparison=[ParentResourceIlAudit]::Compare($input,$binary)
    $result.sha256=(Get-FileHash -LiteralPath $binary).Hash
    $result.byteIdenticalToTestedNative=[Convert]::ToBase64String([IO.File]::ReadAllBytes($original)) -ceq [Convert]::ToBase64String([IO.File]::ReadAllBytes($binary))
    $result.maps=@(Get-ChildItem -LiteralPath $outRoot -Filter '*.csv' -File|ForEach-Object{@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash}})
    if($NetworkProbeDiagnostic){
        $result.diagnosticManagedReceiptSha256=$ExpectedManagedReceipt;$result.productionAdopted=$false
        $result.passed=$result.ilComparison.ChangedMethods.Count -eq 0 -and $result.ilComparison.TokenMapsIdentical -and $result.ilComparison.UserStringsIdentical -and $result.maps.Count -eq 0
    }else{$result.passed=$result.byteIdenticalToTestedNative -and $result.sha256 -ceq '60FF26D8357FEA52C802BAC562C30C6CE28F72F735E78281D0908E94FAB530D6' -and $result.maps.Count -eq 0}
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object{(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -gt 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if($result.ownerAttention){$global:LeaMappedNativeInputLeases=$leases}else{foreach($lease in $leases){$lease.Dispose()}}
    $global:LeaMappedNativeResult=$result
    [IO.File]::WriteAllText((Join-Path $outRoot 'result.json'),($result|ConvertTo-Json -Depth 7))
}
[pscustomobject]$result|Select-Object passed,sha256,byteIdenticalToTestedNative,ownerAttention,error|ConvertTo-Json
