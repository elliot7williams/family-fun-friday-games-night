param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$outRoot=Join-Path $workspace 'continuity-evidence/decoder-owner-proof-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained foreground owner required'}
if(Test-Path -LiteralPath $outRoot){throw 'Preserve visual evidence'}
$utility=Join-Path $workspace 'continuity-evidence/native-launch-timing-v1/Test-CompositeCandidate.v2.ps1'
if((Get-FileHash -LiteralPath $utility).Hash -cne 'EB00AD17B8078A6CC2F418733BA5D675FC84EF49EED433A527FAC5498DD5ECEF'){throw 'Process utility differs'}
$tokens=$null;$errors=$null;$ast=[Management.Automation.Language.Parser]::ParseFile($utility,[ref]$tokens,[ref]$errors)
foreach($name in @('Space','NewOutputReader','PumpOutput','Run')){
    $defs=@($ast.FindAll({param($node)$node -is [Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -ceq $name},$false))
    if($errors.Count -or $defs.Count -ne 1){throw 'Process utility shape differs'}
    . ([scriptblock]::Create($defs[0].Extent.Text))
}
if(Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue){throw 'Fresh owner required'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new()
function Hold([string]$Path,[string]$Expected){
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($stream)
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) -cne $Expected){throw 'Visual input differs'}
    $inputs.Add(@{path=$Path;sha256=$Expected})
}
$result=[ordered]@{passed=$false;candidate=(Join-Path $outRoot 'runtime');inputs=$inputs;runs=$runs;error=$null;images=[Collections.Generic.List[object]]::new();humanVisualReviewComplete=$false;releaseCertified=$false;scope='Controlled decoder queue phase diagnostic, no native worker launch, not full suite or reliability proof'}
Space;$null=New-Item -ItemType Directory -Path $result.candidate
try{
    if(@(Get-Process -Name 'Firespider*','Lea.Firespider.Boot','Lea.Script.Tests','Lea.Script.Worker' -ErrorAction SilentlyContinue).Count){throw 'Do not overlap browser tests'}
    $base=Join-Path $workspace 'continuity-evidence/named-references-regression-v1'
    $proof=Join-Path $base 'result.json';Hold $proof '89E80A57BFBDF83EBCDCAE79F92984ECD8EE631FE0BBC83A11C0D32CF2A0304C'
    $b=Get-Content -LiteralPath $proof -Raw|ConvertFrom-Json
    if(!$b.behaviorPassed -or !$b.inputsUnchanged -or !$b.bytesUnchanged -or $b.ownerAttention){throw 'Core behavior proof incomplete'}
    $known=@{};foreach($file in @($b.inputs)+@($b.before)){$known[$file.path]=$file.sha256}
    foreach($pair in ,@('Lea.Engine.dll','D0A8061162EF716A9B5F940AC80119BF84E98F6EFFF3821737AA80541262E7F6')){
        $from=Join-Path $workspace 'continuity-evidence/ordered-flow-v4/candidate/Lea.Engine.dll';Hold $from $pair[1]
        $to=Join-Path $result.candidate $pair[0];Copy-Item -LiteralPath $from -Destination $to;Hold $to $pair[1]
    }
    $builder=Join-Path $PSScriptRoot 'Build-DecoderOwnerView.generated.ps1';Hold $builder (Get-FileHash -LiteralPath $builder).Hash
    . $builder
    $result.viewBuild=Build-DecoderQueueTraceView $workspace $outRoot $result.candidate
    Hold $result.viewBuild.binary $result.viewBuild.sha256
    $options=[Collections.Generic.List[string]]::new()
    $binary=Join-Path $result.candidate 'DecoderQueueTraceProof.exe'
    foreach($flag in @('/nologo','/nostdlib+','/target:exe','/langversion:12.0','/optimize+',('/out:"'+$binary+'"'))){$options.Add($flag)}
    $referenceCount=0
    foreach($line in Get-Content -LiteralPath (Join-Path $base 'tests.rsp')){
        if($line -match '^/reference:"([^"]+)"$'){
            $path=$Matches[1]
            if(!$known.ContainsKey($path)){throw 'Unpinned visual reference'}
            Hold $path $known[$path]
            if([IO.Path]::GetFileName($path) -cin @('Lea.Engine.dll','Lea.Win.dll')){$path=Join-Path $result.candidate ([IO.Path]::GetFileName($path))}
            $options.Add('/reference:"'+$path+'"');$referenceCount++
        }
    }
    if($referenceCount -ne 11){throw 'Visual reference inventory differs'}
    $source=Join-Path $workspace 'Lea.Tests/DecoderQueueTraceProof.cs';Hold $source (Get-FileHash -LiteralPath $source).Hash
    $copy=Join-Path $outRoot 'DecoderQueueTraceProof.cs';Copy-Item -LiteralPath $source -Destination $copy;Hold $copy (Get-FileHash -LiteralPath $source).Hash;$options.Add('"'+$copy+'"')
    $compat=Join-Path $workspace 'continuity-evidence/decoder-scheduling-v3/QueueProof.cs';Hold $compat (Get-FileHash -LiteralPath $compat).Hash
    $copy=Join-Path $outRoot 'QueueProof.cs';Copy-Item -LiteralPath $compat -Destination $copy;Hold $copy (Get-FileHash -LiteralPath $compat).Hash
    $options.Add('"'+$copy+'"')
    $retirement=Join-Path $workspace 'Lea.Tests/DecoderRetirementProof.cs';Hold $retirement (Get-FileHash -LiteralPath $retirement).Hash
    $copy=Join-Path $outRoot 'DecoderRetirementProof.cs';Copy-Item -LiteralPath $retirement -Destination $copy;Hold $copy (Get-FileHash -LiteralPath $retirement).Hash
    $options.Add('"'+$copy+'"')
    $ownerProof=Join-Path $workspace 'Lea.Tests/DecoderOwnerProof.cs';Hold $ownerProof (Get-FileHash -LiteralPath $ownerProof).Hash
    $copy=Join-Path $outRoot 'DecoderOwnerProof.cs';Copy-Item -LiteralPath $ownerProof -Destination $copy;Hold $copy (Get-FileHash -LiteralPath $ownerProof).Hash
    $options.Add('"'+$copy+'"');$options.Add('/main:DecoderOwnerProof')
    Hold $PSCommandPath (Get-FileHash -LiteralPath $PSCommandPath).Hash;Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $outRoot 'runner.ps1')
    $rsp=Join-Path $outRoot 'compile.rsp';[IO.File]::WriteAllLines($rsp,$options);$hash=(Get-FileHash -LiteralPath $rsp).Hash;Hold $rsp $hash
    $wrapper=Join-Path $PSScriptRoot 'Invoke-PublicCompiler.20261022.ps1';Hold $wrapper '1A04EE9A191D522955C633F97A24C9C0921A5738F76D0DF36DB1C92A574EEA03'
    $compile=Run 'compile-visuals' (Get-Process -Id $PID).Path @('-NoProfile','-File',('"'+$wrapper+'"'),'-ResponseFile',('"'+$rsp+'"'),'-ExpectedResponseSha256',$hash,'-ExpectedSourceSha256','D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB') 120
    if(!$compile.passed){throw 'Visual probe compilation failed'}
    $result.before=@(Get-ChildItem -LiteralPath $result.candidate -File|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash;sddl=(Get-Acl -LiteralPath $_.FullName).Sddl}})
    $run=Run 'queue-trace-proof' $binary @(('"'+(Join-Path $result.candidate 'Lea.Win.dll')+'"')) 30
    $lines=@(Get-Content -LiteralPath $run.stdout)
    $result.compatibilityProofPassed=$lines -ccontains 'PASS decoder queue: 21 checks; no native worker launched.'
    $result.passed=$run.passed -and $result.compatibilityProofPassed -and ($lines -ccontains 'Decoder queue trace: checks=32; failures=0; no native worker launched') -and ($lines -ccontains 'Decoder retirement: checks=11; failures=0; no native worker launched') -and $lines[-1] -ceq 'Decoder owner: checks=7; failures=0; no native worker launched'
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.candidateUnchanged=if($result.before){@($result.before|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256 -or (Get-Acl -LiteralPath $_.path).Sddl -cne $_.sddl}).Count -eq 0}else{$false}
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -gt 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and $result.candidateUnchanged -and !$result.ownerAttention
    if($result.ownerAttention){$global:LeaVisualInputLeases=$leases}else{foreach($lease in $leases){$lease.Dispose()}}
    $global:LeaVisualResult=$result
    [IO.File]::WriteAllText((Join-Path $outRoot 'result.json'),($result|ConvertTo-Json -Depth 7))
}
[pscustomobject]$result|Select-Object passed,inputsUnchanged,candidateUnchanged,ownerAttention,error|ConvertTo-Json
