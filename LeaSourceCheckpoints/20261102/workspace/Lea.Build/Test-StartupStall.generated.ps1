param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$outRoot=Join-Path $workspace 'continuity-evidence/startup-stall-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained foreground TTY required'}
if(Test-Path -LiteralPath $outRoot){throw 'Preserve attempt'}
$utility=Join-Path $workspace 'continuity-evidence/native-launch-timing-v1/Test-CompositeCandidate.v2.ps1'
if((Get-FileHash -LiteralPath $utility).Hash -cne 'EB00AD17B8078A6CC2F418733BA5D675FC84EF49EED433A527FAC5498DD5ECEF'){throw 'Utility changed'}
$t=$null;$e=$null;$ast=[Management.Automation.Language.Parser]::ParseFile($utility,[ref]$t,[ref]$e)
foreach($name in @('Space','Pin','CopyExact','NewOutputReader','PumpOutput','Run')){
    $defs=@($ast.FindAll({param($n)$n -is [Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -ceq $name},$false))
    if($e.Count -or $defs.Count -ne 1){throw 'Utility shape changed'}
    . ([scriptblock]::Create($defs[0].Extent.Text))
}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
$result=[ordered]@{passed=$false;candidate=(Join-Path $outRoot 'runtime');inputs=$inputs;runs=$runs;error=$null;intentionalHandshakeWithheld=$true;scope='Withheld parent handshake transport with live confined worker; not actual worker stall causality or release certification'}
Space;$null=New-Item -ItemType Directory -Path $result.candidate
try{
    $priorPath=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/host-process-state-v1/result.json'
    $null=Pin $priorPath 'F429A465AE4F4E37A251FE95A17157DE0B5D2AF2E3A0D331D86676601EF913F5'
    $prior=Get-Content -LiteralPath $priorPath -Raw|ConvertFrom-Json
    foreach($file in $prior.before|Where-Object{ $_.path -match '\.dll$' -or $_.path.Contains('\ScriptWorker\') }){
        $relative=[IO.Path]::GetRelativePath($prior.candidate,$file.path)
        CopyExact $file.path (Join-Path $result.candidate $relative) $file.sha256
    }
    $source=Join-Path $PSScriptRoot 'StartupStallProbe.cs';$null=Pin $source
    Copy-Item -LiteralPath $source -Destination (Join-Path $outRoot 'StartupStallProbe.cs')
    $buildPath=Join-Path $workspace 'continuity-evidence/process-state-parent-build-v1/result.json'
    $null=Pin $buildPath '80B4315137D9596B39477C8B1731AAB77737589FDD4889FFB6748AEFDB678D91'
    $build=Get-Content -LiteralPath $buildPath -Raw|ConvertFrom-Json
    $recipe=Join-Path $workspace 'continuity-evidence/process-state-parent-build-v1/parent/compile.rsp'
    $row=@($build.inputs|Where-Object path -ceq $recipe)
    if($row.Count -ne 1){throw 'Recipe pin missing'};$null=Pin $recipe $row[0].sha256
    $lines=[Collections.Generic.List[string]]::new()
    foreach($line in Get-Content -LiteralPath $recipe){
        if($line.StartsWith('"') -or $line.StartsWith('/resource:') -or $line.StartsWith('/out:') -or $line.StartsWith('/pdb:')){continue}
        if($line.StartsWith('/reference:')){$path=$line.Substring(11).Trim('"');$pin=@($build.inputs|Where-Object path -ceq $path|Select-Object -First 1);if($pin.Count -ne 1){throw 'Reference pin missing'};$null=Pin $path $pin[0].sha256}
        $lines.Add($line.Replace('/target:library','/target:exe'))
    }
    $binary=Join-Path $result.candidate 'Lea.Script.Tests.exe'
    $lines.Add('/out:"'+$binary+'"');$lines.Add('/pdb:"'+(Join-Path $result.candidate 'probe.pdb')+'"')
    $lines.Add('/reference:"'+(Join-Path $result.candidate 'Lea.Script.dll')+'"');$lines.Add('"'+$source+'"')
    $rsp=Join-Path $outRoot 'compile.rsp';[IO.File]::WriteAllLines($rsp,$lines)
    $hash=Pin $rsp
    $wrapper=Join-Path $PSScriptRoot 'Invoke-PublicCompiler.20261022.ps1';$null=Pin $wrapper '1A04EE9A191D522955C633F97A24C9C0921A5738F76D0DF36DB1C92A574EEA03'
    $compile=Run 'compile' (Get-Process -Id $PID).Path @('-NoProfile','-File',('"'+$wrapper+'"'),'-ResponseFile',('"'+$rsp+'"'),'-ExpectedResponseSha256',$hash,'-ExpectedSourceSha256','D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB') 120
    if(!$compile.passed){throw 'Compilation failed'}
    $fixture=Join-Path $outRoot 'fixture.html';[IO.File]::WriteAllText($fixture,'<!doctype html><title>Retained</title><body>Startup failure fixture</body>');$null=Pin $fixture
    $result.before=@(Get-ChildItem -LiteralPath $result.candidate -Recurse -File|ForEach-Object{@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash;sddl=(Get-Acl -LiteralPath $_.FullName).Sddl}})
    $run=Run 'withheld-handshake' $binary @(('"'+$fixture+'"'),('"'+(Join-Path $result.candidate 'ScriptWorker')+'"')) 30
    if(!$run.passed){throw 'Injected exit check failed'}
    $result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -gt 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.inputsUnchanged=@($inputs|Where-Object{(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.runtimeUnchanged=if($result.before){@($result.before|Where-Object{(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256 -or (Get-Acl -LiteralPath $_.path).Sddl -cne $_.sddl}).Count -eq 0}else{$false}
    $result.passed=$result.passed -and !$result.ownerAttention -and $result.inputsUnchanged -and $result.runtimeUnchanged
    $global:LeaStartupExitResult=$result
    [IO.File]::WriteAllText((Join-Path $outRoot 'result.json'),($result|ConvertTo-Json -Depth 7))
}
[pscustomobject]$result|Select-Object passed,ownerAttention,error|ConvertTo-Json
