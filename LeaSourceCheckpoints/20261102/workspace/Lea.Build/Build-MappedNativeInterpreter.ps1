param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/temporal-startup-v1'
$outRoot=Join-Path $workspace 'continuity-evidence/interpreter-mapped-native-v1'
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
    $verification=Join-Path $workspace 'continuity-evidence/interpreter-mapped-build-v1/rebuild-verification.json'
    Hold $verification '86199E378206B2398B934DF76DB4D1F90905FB39DD22CE32B6D8316874598DE7'
    $proofPath=Join-Path $root 'native-pair-v2/literal-input-verification.json'
    Hold $proofPath '98A773D0A4B40B2D116F8BAEA1B4CF1F20D5D9C6607C1FEB297EE4805C9428C8'
    $proof=Get-Content -LiteralPath $proofPath -Raw|ConvertFrom-Json
    if(!$proof.passed -or $proof.verified.Count -ne 382){throw 'Native provenance incomplete'}
    foreach($file in $proof.verified){Hold $file.path $file.sha256}
    $input=Join-Path $workspace 'continuity-evidence/interpreter-mapped-build-v1/emit/Jint.dll'
    Hold $input '13B3EF3BA07B3DE50572D07D1B485A1ACDEBE307BE935C6E6F5D5771C492E32F'
    $original=Join-Path $root 'native-pair-v2/lazy/Jint.dll'
    Hold $original '67F3E7D75EDADCBC0110FD6BB7AC26E56784D3650A83C2AE0AA23DD67EB03E6D'
    $recipe=Join-Path $root 'native-pair-v2/lazy/native.rsp'
    $lines=[IO.File]::ReadAllLines($recipe);$options=[Collections.Generic.List[string]]::new();$outputs=0
    $binary=Join-Path $outRoot 'Jint.dll'
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
    $result.passed=$result.byteIdenticalToTestedNative -and $result.sha256 -ceq '67F3E7D75EDADCBC0110FD6BB7AC26E56784D3650A83C2AE0AA23DD67EB03E6D' -and $result.maps.Count -eq 2
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
