param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/temporal-startup-v1'
$outRoot=Join-Path $root 'trusted-stage-probe-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(Test-Path $outRoot){throw 'Preserve probe evidence'}
if(Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue){throw 'Fresh owner required'}
$utility=Join-Path $workspace 'continuity-evidence/native-launch-timing-v1/Test-CompositeCandidate.v2.ps1'
if((Get-FileHash -LiteralPath $utility).Hash -cne 'EB00AD17B8078A6CC2F418733BA5D675FC84EF49EED433A527FAC5498DD5ECEF'){throw 'Runner changed'}
$t=$null;$e=$null;$ast=[Management.Automation.Language.Parser]::ParseFile($utility,[ref]$t,[ref]$e)
foreach($name in @('Space','Pin','CopyExact','NewOutputReader','PumpOutput','Run')){
    $defs=@($ast.FindAll({param($n)$n -is [Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -ceq $name},$false))
    if($e.Count -or $defs.Count -ne 1){throw 'Runner shape changed'}
    . ([scriptblock]::Create($defs[0].Extent.Text))
}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new()
$result=[ordered]@{completed=$false;candidate=(Join-Path $outRoot 'runtime');inputFiles=$inputs;runs=$runs;error=$null;report=$null;limitsChanged=$false;productionChanged=$false;performanceGatePassed=$false}
Space;$null=New-Item -ItemType Directory $result.candidate
try{
    $null=Pin $PSCommandPath;Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $outRoot 'runner.ps1')
    $buildPath=Join-Path $root 'suite-phase-relocated-v2/result.json';$null=Pin $buildPath '1DF0860AAEF98A61FDBE997A31889228061A2E30D02E9D705C7F9503CA352A18'
    $build=Get-Content -LiteralPath $buildPath -Raw|ConvertFrom-Json
    if(!$build.passed -or !$build.inputsUnchanged -or $build.ownerAttention){throw 'Baseline build incomplete'}
    foreach($file in $build.files){
        if((Split-Path $file.path) -cne $build.runtime){continue}
        $name=[IO.Path]::GetFileName($file.path)
        if([IO.Path]::GetExtension($name) -ceq '.dll'){CopyExact $file.path (Join-Path $result.candidate $name) $file.sha256}
        elseif($name -ceq 'Lea.Script.Tests.exe.config'){CopyExact $file.path (Join-Path $result.candidate 'TrustedBootstrapProbe.exe.config') $file.sha256}
    }
    $source=Join-Path $root 'TrustedBootstrapProbe.cs';$null=Pin $source
    Copy-Item -LiteralPath $source -Destination (Join-Path $outRoot 'TrustedBootstrapProbe.cs')
    $oldBuildPath=Join-Path $workspace 'continuity-evidence/bootstrap-descriptor-batches-v1/source-cross-module-suite-v2/result.json'
    $null=Pin $oldBuildPath '779061CB3422B80038653503E7AA375521E5A868DBA30FA88A0509CC206069F5'
    $old=Get-Content -LiteralPath $oldBuildPath -Raw|ConvertFrom-Json
    $recipe=Join-Path (Split-Path $oldBuildPath) 'tests.rsp';$null=Pin $recipe '804234F6C0058ABE81D0914A79630DC9FEC0B942FCE6521B899CB5DE8F527E9F'
    $binary=Join-Path $result.candidate 'TrustedBootstrapProbe.exe'
    $options=[Collections.Generic.List[string]]::new()
    foreach($flag in @('/nologo','/nostdlib+','/target:exe','/platform:anycpu','/langversion:12.0','/optimize+','/warnaserror+')){$options.Add($flag)}
    $options.Add('/out:"'+$binary+'"')
    foreach($line in [IO.File]::ReadAllLines($recipe)){
        if(!$line.StartsWith('/reference:')){continue}
        $path=$line.Substring(11).Trim('"');$hashes=@(@($old.inputs)+@($old.files)|Where-Object path -ceq $path|ForEach-Object sha256|Sort-Object -Unique)
        if($hashes.Count -ne 1){throw 'Reference provenance missing'}
        if($path.StartsWith($old.candidate+'\',[StringComparison]::OrdinalIgnoreCase)){$path=Join-Path $result.candidate ([IO.Path]::GetFileName($path));$null=Pin $path $hashes[0]}
        else{if((Get-FileHash -LiteralPath $path).Hash -cne $hashes[0]){throw 'Framework reference changed'}}
        $options.Add('/reference:"'+$path+'"')
    }
    $options.Add('"'+$source+'"')
    $rsp=Join-Path $outRoot 'probe.rsp';[IO.File]::WriteAllLines($rsp,$options,[Text.UTF8Encoding]::new($false));$hash=Pin $rsp
    $wrapper=Join-Path $workspace 'Lea.Build/Invoke-PublicCompiler.20261022.ps1';$null=Pin $wrapper '1A04EE9A191D522955C633F97A24C9C0921A5738F76D0DF36DB1C92A574EEA03'
    foreach($input in $inputs){$leases.Add([IO.File]::Open($input.path,'Open','Read','Read'))}
    $compile=Run 'compile-trusted-probe' (Get-Process -Id $PID).Path @('-NoProfile','-File',('"'+$wrapper+'"'),'-ResponseFile',('"'+$rsp+'"'),'-ExpectedResponseSha256',$hash,'-ExpectedSourceSha256','D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB') 120
    if(!$compile.passed){throw 'Trusted probe compile failed'}
    $fixture=Join-Path $build.fixtureSource 'LeaForge/test-pages/javascript-runtime.html'
    $row=@($build.fixtureFiles|Where-Object path -ceq $fixture)
    if($row.Count -ne 1){throw 'Fixture pin missing'}
    $null=Pin $fixture $row[0].sha256
    $result.before=@(Get-ChildItem $result.candidate -File|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash;sddl=(Get-Acl -LiteralPath $_.FullName).Sddl}})
    $run=Run 'trusted-bootstrap' $binary @(('"'+$fixture+'"')) 30
    if(!$run.passed){throw 'Probe observation failed'}
    $result.report=Get-Content -LiteralPath $run.stdout -Raw|ConvertFrom-Json
    if(!$result.report.observerRetired -or $result.report.observerError -or $result.report.samplesOverflow){throw 'Observer incomplete'}
    $result.completed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.runtimeUnchanged=@($result.before|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256 -or (Get-Acl -LiteralPath $_.path).Sddl -cne $_.sddl}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -gt 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    if($result.ownerAttention){$global:LeaTrustedProbeInputLeases=$leases}else{foreach($lease in $leases){$lease.Dispose()}}
    $result|ConvertTo-Json -Depth 9|Set-Content -LiteralPath (Join-Path $outRoot 'result.json')
    $global:LeaTrustedProbeResult=$result
    [pscustomobject]$result|Select-Object completed,inputsUnchanged,runtimeUnchanged,ownerAttention,error,report|ConvertTo-Json -Depth 5
}
