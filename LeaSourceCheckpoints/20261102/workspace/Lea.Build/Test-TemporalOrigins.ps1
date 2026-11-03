param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/temporal-startup-v1'
$outRoot=Join-Path $root 'origins-lazy-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Original NoExit TTY required'}
$id=[Security.Principal.WindowsIdentity]::GetCurrent()
if(!$id.User.Value.EndsWith('-1001') -or ([Security.Principal.WindowsPrincipal]::new($id)).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Normal nonadmin required'}
if(Test-Path -LiteralPath $outRoot){throw 'Preserve origin evidence'}
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
$result=[ordered]@{completed=$false;passed=$false;behaviorPassed=$false;candidate=(Join-Path $outRoot 'runtime');inputFiles=$inputs;runs=$runs;error=$null;fullSuitePassed=$false;productionChanged=$false;scope='Frozen document-origin targeted diagnostic with current lazy candidate, not full-suite validation'}
Space
if((Get-PSDrive C).Free -lt 600MB){throw 'Staging reserve required'}
$null=New-Item -ItemType Directory $outRoot
try{
    $null=Pin $PSCommandPath;Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $outRoot 'runner.ps1')
    if(@(Get-Process 'Lea.Script.Worker','Lea.Script.Tests','Firespider*','Lea.Firespider.Boot' -ErrorAction SilentlyContinue).Count){throw 'Do not overlap browser/tests'}
    $buildPath=Join-Path $workspace 'continuity-evidence/bootstrap-descriptor-batches-v1/source-cross-module-suite-v2/result.json'
    $null=Pin $buildPath '779061CB3422B80038653503E7AA375521E5A868DBA30FA88A0509CC206069F5'
    $build=Get-Content -LiteralPath $buildPath -Raw|ConvertFrom-Json
    if(!$build.passed -or !$build.inputsUnchanged -or $build.ownerAttention -or !$build.fullSuiteBindingsComplete -or $build.files.Count -ne 65){throw 'Frozen suite build incomplete'}
    $nativeProof=Join-Path $root 'native-pair-v2/literal-input-verification.json'
    $null=Pin $nativeProof '98A773D0A4B40B2D116F8BAEA1B4CF1F20D5D9C6607C1FEB297EE4805C9428C8'
    $replacements=@{
        'Lea.Script.dll'=@('continuity-evidence/permission-contract-v1/early-limits-v1/Lea.Script.dll','E68504B515405FA2C75B92904277A7A8074945BDCC02E1CAD10A57EB9A6FC1E5')
        'Lea.Win.dll'=@('continuity-evidence/permission-contract-v1/retry-view-v1/Lea.Win.dll','E26135F8CE6BA76A76515D2874DFE219E172B212037CD567AD37042DA516289B')
        'ScriptWorker\Jint.dll'=@('continuity-evidence/temporal-startup-v1/native-pair-v2/lazy/Jint.dll','67F3E7D75EDADCBC0110FD6BB7AC26E56784D3650A83C2AE0AA23DD67EB03E6D')
        'ScriptWorker\Lea.Script.Worker.dll'=@('continuity-evidence/bootstrap-descriptor-batches-v1/timed-worker-native-v2/Lea.Script.Worker.dll','1C338F548D9F70B16F640A28BA7943F05DB0278260711674A62C5A2A8B36F436')
    }
    $seen=[Collections.Generic.HashSet[string]]::new()
    foreach($file in $build.files){
        $relative=$file.path.Substring($build.candidate.Length+1)
        if([IO.Path]::IsPathRooted($relative) -or $relative.Split('\') -contains '..'){throw 'Unsafe relative path'}
        $from=$file.path;$hash=$file.sha256
        if($replacements.ContainsKey($relative)){$from=Join-Path $workspace $replacements[$relative][0];$hash=$replacements[$relative][1];$null=$seen.Add($relative)}
        CopyExact $from (Join-Path $result.candidate $relative) $hash
    }
    if($seen.Count -ne 4){throw 'Candidate replacements incomplete'}
    foreach($file in $build.fixtureFiles){$null=Pin $file.path $file.sha256}
    $fixtureRoot=$build.fixtureSource
    foreach($file in Get-ChildItem (Join-Path $fixtureRoot 'LeaForge/test-pages') -File -Recurse){$null=Pin $file.FullName}
    $result.before=@(Get-ChildItem $result.candidate -File -Recurse|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash;sddl=(Get-Acl -LiteralPath $_.FullName).Sddl}})
    foreach($input in $inputs){$leases.Add([IO.File]::Open($input.path,'Open','Read','Read'))}
    $run=Run 'origins-isolated' (Join-Path $result.candidate 'Lea.Script.Tests.exe') @(('"'+$fixtureRoot+'"'),'--origins-isolated',('"'+(Join-Path $outRoot 'origins.png')+'"')) 90
    $footer=@(Get-Content -LiteralPath $run.stdout|Where-Object {$_.StartsWith('PASS Lea.Script document origins isolated: ')})
    $result.behaviorPassed=$run.passed -and $footer.Count -eq 1
    $result.completed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.changedFiles=@(foreach($file in $result.before){$sddl=(Get-Acl -LiteralPath $file.path).Sddl;$hash=(Get-FileHash -LiteralPath $file.path).Hash;if($sddl -cne $file.sddl -or $hash -cne $file.sha256){@{path=$file.path;bytesChanged=$hash -cne $file.sha256;before=$file.sddl;after=$sddl}}})
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -gt 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.behaviorPassed -and $result.inputsUnchanged -and !$result.ownerAttention -and $result.changedFiles.Count -eq 0
    foreach($lease in $leases){$lease.Dispose()}
    $result|ConvertTo-Json -Depth 9|Set-Content -LiteralPath (Join-Path $outRoot 'result.json')
    $global:LeaTemporalOriginsResult=$result
    [pscustomobject]$result|Select-Object completed,passed,behaviorPassed,inputsUnchanged,ownerAttention,changedFiles,error|ConvertTo-Json -Depth 5
}
