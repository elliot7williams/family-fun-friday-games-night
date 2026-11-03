param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$outRoot=Join-Path $workspace 'continuity-evidence/current-hit-geometry-v2'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained foreground TTY required'}
if(Test-Path -LiteralPath $outRoot){throw 'Preserve focus evidence'}
$utility=Join-Path $workspace 'continuity-evidence/native-launch-timing-v1/Test-CompositeCandidate.v2.ps1'
if((Get-FileHash -LiteralPath $utility).Hash -cne 'EB00AD17B8078A6CC2F418733BA5D675FC84EF49EED433A527FAC5498DD5ECEF'){throw 'Utility changed'}
$tokens=$null;$errors=$null;$ast=[Management.Automation.Language.Parser]::ParseFile($utility,[ref]$tokens,[ref]$errors)
foreach($name in @('Space','Pin','CopyExact','NewOutputReader','PumpOutput','Run')){
    $defs=@($ast.FindAll({param($node)$node -is [Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -ceq $name},$false))
    if($errors.Count -or $defs.Count -ne 1){throw 'Utility shape differs'}
    . ([scriptblock]::Create($defs[0].Extent.Text))
}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
$result=[ordered]@{passed=$false;scope='Focused diagnostic only, no full-suite/release qualification';inputs=$inputs;runs=$runs}
Space;$null=New-Item -ItemType Directory -Path $outRoot
try{
    $base=Join-Path $workspace 'continuity-evidence/title-metadata-regression-v8'
    $receipt=Join-Path $base 'result.json';$null=Pin $receipt 'CD615712B42B5AFBA14E2271E42878ED241574ECAA800C42154B052EF89F40AD'
    $prior=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
    if(!$prior.inputsUnchanged -or !$prior.bytesUnchanged -or $prior.ownerAttention){throw 'Retained candidate proof differs'}
    $known=@{};foreach($row in @($prior.inputs)+@($prior.before)){$known[$row.path]=$row.sha256}
    foreach($name in @('Lea.Engine.dll','Lea.Win.dll','Lea.Image.exe','Lea.Image.Handshake.exe')){
        $path=Join-Path $base ('runtime/'+$name);CopyExact $path (Join-Path $outRoot $name) $known[$path]
    }
    function Hold([string]$Path,[string]$Expected){$null=Pin $Path $Expected}
    $viewBuilder=Join-Path $PSScriptRoot 'Build-CurrentHitGeometryV2.generated.ps1';$null=Pin $viewBuilder
    . $viewBuilder
    $result.viewBuild=Build-DecoderQueueTraceView $workspace $outRoot $outRoot
    $fixtureRows=@($prior.inputs|Where-Object {$_.path.EndsWith('\LeaForge\test-pages\keyboard-focus.html',[StringComparison]::Ordinal)})
    if($fixtureRows.Count -ne 1){throw 'Fixture identity ambiguous'}
    $fixture=$fixtureRows[0].path
    if(!$known.ContainsKey($fixture)){throw 'Fixture not pinned'}
    CopyExact $fixture (Join-Path $outRoot 'keyboard-focus.html') $known[$fixture]
    $rspInput=Join-Path $base 'tests.rsp';$null=Pin $rspInput $known[$rspInput]
    $binary=Join-Path $outRoot 'FocusClickProbe.exe'
    $options=[Collections.Generic.List[string]]::new()
    foreach($flag in @('/nologo','/nostdlib+','/optimize+','/target:exe',('/out:"'+$binary+'"'))){$options.Add($flag)}
    $count=0
    foreach($line in Get-Content -LiteralPath $rspInput){
        if($line -match '^/reference:"([^"]+)"$'){
            $path=$Matches[1];if(!$known.ContainsKey($path)){throw 'Reference not pinned'}
            $null=Pin $path $known[$path];$options.Add($line);$count++
        }
    }
    if($count -ne 11){throw 'Reference inventory differs'}
    $source=Join-Path $workspace 'Lea.Tests/FocusClickProbe.cs';$hash=Pin $source
    CopyExact $source (Join-Path $outRoot 'FocusClickProbe.cs') $hash
    $options.Add('"'+(Join-Path $outRoot 'FocusClickProbe.cs')+'"')
    $rsp=Join-Path $outRoot 'compile.rsp';[IO.File]::WriteAllLines($rsp,$options);$hash=Pin $rsp
    $wrapper=Join-Path $PSScriptRoot 'Invoke-PublicCompiler.20261026.ps1';$null=Pin $wrapper 'C3EBB3801FAFB80DA031AAB69FE4837EEB4E4EECD3907F9704F9D6186FA42F0A'
    $compile=Run 'compile-probe' (Get-Process -Id $PID).Path @('-NoProfile','-File',('"'+$wrapper+'"'),'-ResponseFile',('"'+$rsp+'"'),'-ExpectedResponseSha256',$hash,'-ExpectedSourceSha256','D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB') 120
    if(!$compile.passed){throw 'Probe compilation failed'}
    $result.before=@(Get-ChildItem -LiteralPath $outRoot -File|ForEach-Object{@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash}})
    $run=Run 'focus-probe' $binary @(('"'+(Join-Path $outRoot 'keyboard-focus.html')+'"'),'require-current') 60
    $result.passed=$run.passed
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -gt 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.inputsUnchanged=@($inputs|Where-Object{(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.bytesUnchanged=if($result.before){@($result.before|Where-Object{(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0}else{$false}
    $result.passed=$result.passed -and !$result.ownerAttention -and $result.inputsUnchanged -and $result.bytesUnchanged
    [IO.File]::WriteAllText((Join-Path $outRoot 'result.json'),($result|ConvertTo-Json -Depth 7))
}
$result|ConvertTo-Json -Depth 2
