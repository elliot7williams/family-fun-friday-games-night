param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$outRoot=Join-Path $workspace 'continuity-evidence/embedding-modern-worker-v2'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained foreground owner required'}
if(Test-Path -LiteralPath $outRoot){throw 'Preserve embedding evidence'}
$utility=Join-Path $workspace 'continuity-evidence/native-launch-timing-v1/Test-CompositeCandidate.v2.ps1'
if((Get-FileHash -LiteralPath $utility).Hash -cne 'EB00AD17B8078A6CC2F418733BA5D675FC84EF49EED433A527FAC5498DD5ECEF'){throw 'Runner differs'}
$t=$null;$e=$null;$ast=[Management.Automation.Language.Parser]::ParseFile($utility,[ref]$t,[ref]$e)
foreach($name in @('Space','Pin','CopyExact','NewOutputReader','PumpOutput','Run')){
    $defs=@($ast.FindAll({param($n)$n -is [Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -ceq $name},$false))
    if($e.Count -or $defs.Count -ne 1){throw 'Runner shape differs'}
    . ([scriptblock]::Create($defs[0].Extent.Text))
}
$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$result=[ordered]@{passed=$false;inputs=$inputs;runs=$runs;checks=[Collections.Generic.List[object]]::new();scope='Actual VS MSBuild sample packaging/native smoke and layout-only missing-file controls, not isolated scripting startup or clean-PC certification'}
Space;$null=New-Item -ItemType Directory -Path $outRoot
try{
    CopyExact $PSCommandPath (Join-Path $outRoot 'runner.ps1') (Get-FileHash -LiteralPath $PSCommandPath).Hash
    $proof=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/host-current-native-v1/result.json';$null=Pin $proof '74BAD2AFA9ADE431B9B8630AEB222DFBE238818ADC616F27FF3768A165448DE7'
    $hostProof=Get-Content -LiteralPath $proof -Raw|ConvertFrom-Json
    if(!$hostProof.passed -or !$hostProof.candidateBytesUnchanged -or !$hostProof.originalInputsUnchanged -or $hostProof.ownerAttention){throw 'Selected runtime proof incomplete'}
    foreach($file in $hostProof.before){$null=Pin $file.path $file.sha256}
    $prior=Join-Path $workspace 'continuity-evidence/embedding-sample-v1/visual-studio-v2/result.json';$null=Pin $prior 'C16023264D782BEE1C595069B31F431C76D091DB63F539833D2403BBF766F005'
    $old=Get-Content -LiteralPath $prior -Raw|ConvertFrom-Json
    $msbuild='C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe'
    $pin=@($old.inputFiles|Where-Object path -ceq $msbuild)
    if($pin.Count -ne 1){throw 'MSBuild provenance ambiguous'}
    if((Get-FileHash -LiteralPath $msbuild).Hash -cne $pin[0].sha256){throw 'Installed MSBuild identity differs'}
    $inputs.Add(@{path=$msbuild;sha256=$pin[0].sha256})
    foreach($file in Get-ChildItem -LiteralPath (Join-Path $workspace 'Lea.Embed.Sample') -File){CopyExact $file.FullName (Join-Path $outRoot ('source/'+$file.Name)) (Get-FileHash -LiteralPath $file.FullName).Hash}
    $project=Join-Path $outRoot 'source/Lea.Embed.Sample.csproj';$output=Join-Path $outRoot 'runtime'
    if(Test-Path -LiteralPath $output){throw 'Require empty fresh output'}
    $args=@(('"'+$project+'"'),'/nologo','/m:1','/nodeReuse:false','/t:Build','/p:Configuration=Release','/p:UseSharedCompilation=false',('"/p:LeaRuntime='+$hostProof.candidate+'"'),('"/p:OutputPath='+$output+'/"'),('"/p:BaseIntermediateOutputPath='+$outRoot+'/obj/"'))
    $build=Run 'sample-build' $msbuild $args 120
    if(!$build.passed){throw 'Sample build failed'}
    foreach($name in @('Lea.Script.Worker.exe','Lea.Script.Worker.dll','Lea.Script.Worker.deps.json','Lea.Script.Worker.runtimeconfig.json','Jint.dll','Acornima.dll')){
        $from=Join-Path $hostProof.candidate ('ScriptWorker/'+$name);$to=Join-Path $output ('ScriptWorker/'+$name)
        if((Get-FileHash -LiteralPath $from).Hash -cne (Get-FileHash -LiteralPath $to).Hash){throw ('Worker copy mismatch: '+$name)}
    }
    $result.modernWorkerFilesVerified=6
    $required=@('Lea.Engine.dll','Lea.Win.dll','Lea.Script.dll','Lea.Script.Win.dll','Lea.Image.exe','ScriptWorker/Lea.Script.Worker.exe','ScriptWorker/Jint.dll','ScriptWorker/Acornima.dll','ScriptWorker/Lea.Script.Worker.deps.json','ScriptWorker/Lea.Script.Worker.runtimeconfig.json')
    $cases=@(@{name='complete-framework';missing=$null;modern=$false},@{name='complete-modern';missing=$null;modern=$true})+@($required|ForEach-Object{@{name=('missing-'+[IO.Path]::GetFileName($_));missing=$_;modern=$true}})
    foreach($case in $cases){
        $fixture=Join-Path $outRoot ('layout-fixtures/'+$case.name);$null=New-Item -ItemType Directory -Path (Join-Path $fixture 'ScriptWorker') -Force
        foreach($name in $required){if($name -ceq $case.missing -or (!$case.modern -and $name.EndsWith('.json'))){continue};[IO.File]::WriteAllBytes((Join-Path $fixture $name),[byte[]]@())}
        if($case.modern){[IO.File]::WriteAllBytes((Join-Path $fixture 'ScriptWorker/Lea.Script.Worker.dll'),[byte[]]@())}
        $test=Run $case.name $msbuild @(('"'+$project+'"'),'/nologo','/m:1','/nodeReuse:false','/t:RequireLeaRuntime',('"/p:LeaRuntime='+$fixture+'"')) 30
        $ok=if($case.missing){$test.exitCode -ne 0 -and !$test.timedOut -and !$test.killRequested -and ([IO.File]::ReadAllText($test.stdout).Contains('Incomplete Lea sample runtime: missing'))}else{$test.passed}
        $result.checks.Add(@{name=$case.name;passed=$ok;layoutOnly=$true})
        if(!$ok){throw ('Layout boundary failed: '+$case.name)}
    }
    $result.before=@(Get-ChildItem -LiteralPath $output -Recurse -File|ForEach-Object{@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash;sddl=(Get-Acl -LiteralPath $_.FullName).Sddl}})
    $smoke=Run 'embedding-smoke' (Join-Path $output 'Lea.Embed.Sample.exe') @('--smoke',('"'+(Join-Path $outRoot 'embedding.png')+'"')) 45
    if(!$smoke.passed -or @((Get-Content -LiteralPath $smoke.stdout)|Where-Object {$_ -like 'PASS Lea embedding smoke: 7 checks;*'}).Count -ne 1){throw 'Native sample smoke failed'}
    $result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -gt 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.inputsUnchanged=@($inputs|Where-Object{(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.outputBytesAndAclsHeld=if($result.before){@($result.before|Where-Object{(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256 -or (Get-Acl -LiteralPath $_.path).Sddl -cne $_.sddl}).Count -eq 0}else{$false}
    $result.passed=$result.passed -and !$result.ownerAttention -and $result.inputsUnchanged -and $result.outputBytesAndAclsHeld
    $result|ConvertTo-Json -Depth 7|Set-Content -LiteralPath (Join-Path $outRoot 'result.json')
}
[pscustomobject]$result|Select-Object passed,ownerAttention,error|ConvertTo-Json
