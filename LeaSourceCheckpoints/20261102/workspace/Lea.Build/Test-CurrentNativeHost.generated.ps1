param([switch]$KeepForegroundHost,[Parameter(Mandatory=$true)][ValidatePattern('^[A-F0-9]{64}$')][string]$ExpectedNativeReceipt,[switch]$AboutHost,[switch]$LibraryHost)
$ErrorActionPreference='Stop'
$workspace='C:\Users\Elliot\Documents\Family Fun Friday Games Night copy'
$root=$workspace+'\continuity-evidence\decoder-scheduling-v3'
$outRoot=$workspace+$(if($LibraryHost){'\continuity-evidence\temporal-startup-v1\host-library-current-native-v1'}elseif($AboutHost){'\continuity-evidence\temporal-startup-v1\host-about-current-native-v1'}else{'\continuity-evidence\temporal-startup-v1\host-current-native-v1'})
if($LibraryHost -and !$AboutHost){throw 'Library host candidate must retain About fixes'}
$utility=$workspace+'\continuity-evidence\native-launch-timing-v1\Test-CompositeCandidate.v2.ps1'
$composite=$workspace+'\continuity-evidence\native-launch-timing-v1\composite-v2'
$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
$result=[ordered]@{completed=$false;passed=$false;candidate=$outRoot+'\runtime';runs=$runs;inputFiles=$inputs;error=$null;limitsChanged=$false;installedBrowserChanged=$false;adopted=$false;scope='15-stage host subset with rebuilt core; matched full scripting suite and release remain unverified';fullReleaseValidated=$false;scriptGatesRun=$false}
if(-not$KeepForegroundHost-or'-NoExit'-notin[Environment]::GetCommandLineArgs()-or[Console]::IsInputRedirected-or[Console]::IsOutputRedirected){throw 'Original retained NoExit TTY required.'}
$identity=[Security.Principal.WindowsIdentity]::GetCurrent()
try{if(-not$identity.User.Value.EndsWith('-1001')-or([Security.Principal.WindowsPrincipal]::new($identity)).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Normal nonadministrator Elliot required.'}}finally{$identity.Dispose()}
if(Test-Path $outRoot){throw 'Preserve original evidence; no retry.'}
if(Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue){throw 'Fresh owner required.'}
if((Get-FileHash $utility).Hash-cne'EB00AD17B8078A6CC2F418733BA5D675FC84EF49EED433A527FAC5498DD5ECEF'){throw 'Process utility differs.'}
$t=$null;$e=$null;$ast=[Management.Automation.Language.Parser]::ParseFile($utility,[ref]$t,[ref]$e)
if($e.Count){throw 'Utility parse failed.'}
foreach($name in @('Space','Pin','CopyExact','NewOutputReader','PumpOutput','Run')){
    $defs=@($ast.FindAll({param($n)$n-is[Management.Automation.Language.FunctionDefinitionAst]-and$n.Name-ceq$name},$false))
    if($defs.Count-ne1){throw 'Utility definition differs.'};. ([scriptblock]::Create($defs[0].Extent.Text))
}
function SafeRun([string]$name,[string]$exe,[string[]]$arguments,[int]$seconds){
    if($global:LeaCompositeProcessLeases.Count-or(Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)){throw 'Original process ownership unresolved.'}
    return Run $name $exe $arguments $seconds
}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$null=New-Item -ItemType Directory -Path $outRoot
try{
    Space;$null=Pin $PSCommandPath
    $coreProof=$workspace+'\continuity-evidence\nested-overlap-regression-v1\result.json'
    $null=Pin $coreProof 'E60A49F48671E92B80279171E5FC274BB4D7CFF2020D185A86729B7A38F04B6E'
    $core=Get-Content -LiteralPath $coreProof -Raw|ConvertFrom-Json
    $observation=$workspace+'\continuity-evidence\nested-overlap-regression-v1\outcome-observation.json'
    $null=Pin $observation '89AD895CA8DD8A560A9A3DB217AA1FD7955FE42D9D0F147898F13C60149E0F2A'
    $observed=Get-Content -LiteralPath $observation -Raw|ConvertFrom-Json
    if(!$core.behaviorPassed -or !$core.inputsUnchanged -or !$core.bytesUnchanged -or $core.ownerAttention -or !$observed.nativeBehaviorPassed -or !$observed.exactTerminalFooterVerified -or $observed.originalParentBudgetMs -ne 300000 -or $observed.productionPolicyBroadened){throw 'Current native execution evidence incomplete'}
    foreach($input in @($core.inputs)+@($core.before)){$null=Pin $input.path $input.sha256}
    $result.priorFullRawAuditPassed=$core.passed
    $policy=$workspace+'\Lea.Build\Test-DecoderAclDelta.ps1'
    $null=Pin $policy '15F46DBC7F4D4AD5B816DDFA1FE7213D02CAE1EDE79844AFBD904F281299E71E'
    . $policy
    Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $outRoot 'runner.ps1')
    if(@(Get-Process -Name 'Firespider*','Lea.Firespider.Boot','Lea.Script.Worker','Lea.Script.Tests','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'Do not overlap existing browser or native tests'}
    $permissionRoot=$workspace+'\continuity-evidence\permission-contract-v1'
    foreach($spec in @(
        @('early-limits-contract-v1/test-v2/result.json','10B7CFE85825305601161698507F402DDB135A49ED6295571B95FF7660E28C7A'),
        @('retry-view-contract-v1/test-v2/result.json','BB876D9E115732F4964CCEE161901F892690E6E6F25412B64ADF104DFFBD4243')
    )){
        $path=Join-Path $permissionRoot $spec[0];$null=Pin $path $spec[1]
        $p=Get-Content -LiteralPath $path -Raw|ConvertFrom-Json
        if(!$p.completed-or!$p.expectedOutcomeVerified-or!$p.assertionsPassed-or$p.ownerAttention){throw 'Actual DLL permission checks incomplete'}
    }
    $stagePath=$workspace+'\continuity-evidence\bootstrap-descriptor-batches-v1\source-cross-module-stage-v1\result.json'
    $null=Pin $stagePath '7AED0273C0D8D47BDC9C75DC3E9633EBE8A271D6C41572A6F0D4B2E4E2CB8D4A'
    $stage=Get-Content -LiteralPath $stagePath -Raw|ConvertFrom-Json
    if(!$stage.passed-or!$stage.inputsUnchanged-or$stage.variants.Count-ne1-or$stage.variants[0].files.Count-ne6){throw 'Six-file current worker stage required'}
    $nativePath=$workspace+'\continuity-evidence\bootstrap-descriptor-batches-v1\early-pair-native-v1\result.json'
    $null=Pin $nativePath $ExpectedNativeReceipt
    $native=Get-Content -LiteralPath $nativePath -Raw|ConvertFrom-Json
    if(!$native.completed-or$native.ownerAttention-or!$native.inputsUnchanged-or$native.reports.Count-ne1-or!$native.reports[0].behaviorPassed){throw 'Targeted pair behavior checks required; raw audit remains failed'}
    $result.priorNativeRawAuditPassed=$native.passed
    $null=Pin ($composite+'\result.json') 'CEDDECFB4ECD0116BB84DE3E9E655ADBFC1C7FA059AEAD65A4B94420BED739B5'
    $base=Get-Content ($composite+'\result.json') -Raw|ConvertFrom-Json
    if(-not$base.completed-or$base.ownerAttention-or-not$base.originalInputsUnchanged){throw 'Completed composite required.'}
    $binaryHashes=@{};foreach($row in $base.candidateBinaries){$binaryHashes[$row.path]=$row.sha256}
    foreach($file in Get-ChildItem $base.candidate -Recurse -File){
        $rel=$file.FullName.Substring($base.candidate.Length+1)
        if($rel.StartsWith('ScriptWorker\',[StringComparison]::OrdinalIgnoreCase)){continue}
        $hash=if($binaryHashes.ContainsKey($rel)){$binaryHashes[$rel]}else{(Get-FileHash $file.FullName).Hash}
        CopyExact $file.FullName (Join-Path $result.candidate $rel) $hash
    }
    CopyExact ($workspace+'\continuity-evidence\ordered-flow-v6\candidate\Lea.Engine.dll') ($result.candidate+'\Lea.Engine.dll') '52CE235E37B10243D618E4BDF0031E215145974526DF8F758E167C16E791F4BC'
    $hostBuildPath=$workspace+'\continuity-evidence\firespider-source-rebuild-v1\result.json'
    $null=Pin $hostBuildPath '9DDF92B7273943C6CB5481A8CCC8033F09053BE2C5968DC5E53EDA14E0A5F3DA'
    $hostBuild=Get-Content -LiteralPath $hostBuildPath -Raw|ConvertFrom-Json
    if(!$hostBuild.passed -or !$hostBuild.inputsUnchanged -or $hostBuild.ownerAttention){throw 'Source host build incomplete'}
    CopyExact ($workspace+'\continuity-evidence\firespider-source-rebuild-v1\bin\Firespider Internet 19.exe') ($result.candidate+'\Firespider Internet 19.exe') '950FEB59E55837BC84F42175B91104690CBFD75F7C492169F782A47CAD72EA67'
    CopyExact ($workspace+'\continuity-evidence\firespider-source-rebuild-v1\bin\Firespider Internet 19.exe.config') ($result.candidate+'\Firespider Internet 19.exe.config') 'E91AC2CC41148E4F9F2FA181E5B17C8A4A35E07A56055A9BA6567A6EEC92BFAF'
    if($AboutHost){
        $aboutPath=$workspace+'\continuity-evidence\about-navigation-v2\result.json'
        $null=Pin $aboutPath '6E90FF84B17140992FC2D9B3EFDAA36DD438B94490E261804B15E5C4DB9825C1'
        $about=Get-Content $aboutPath -Raw|ConvertFrom-Json
        if(!$about.passed -or !$about.inputsUnchanged -or !$about.runtimeBytesAndAclsHeld -or !$about.aboutOverlayInverseVerified -or !$about.baselineRegressionReproduced -or $about.ownerAttention){throw 'About host focused qualification incomplete'}
        CopyExact ($workspace+'\continuity-evidence\about-navigation-v2\runtime\Firespider Internet 19.exe') ($result.candidate+'\Firespider Internet 19.exe') '9615491532E758347AACBB0B6DCF0A9C023363C17C9962B60C0638B8EBABB5C0'
        $result.aboutHostReceiptSha256='6E90FF84B17140992FC2D9B3EFDAA36DD438B94490E261804B15E5C4DB9825C1'
    }
    if($LibraryHost){
        $libraryPath=$workspace+'\continuity-evidence\library-navigation-v1\result.json'
        $null=Pin $libraryPath '3739C0B1979EC72622E8030CBB49FAF50824B912DB2C2B24C14DD76D53F38021'
        $library=Get-Content -LiteralPath $libraryPath -Raw|ConvertFrom-Json
        if(!$library.passed -or !$library.inputsUnchanged -or !$library.runtimeBytesAndAclsHeld -or !$library.libraryOverlayInverseVerified -or !$library.aboutOverlayInverseVerified -or !$library.baselineRegressionReproduced -or $library.ownerAttention){throw 'Library host focused qualification incomplete'}
        CopyExact ($workspace+'\continuity-evidence\library-navigation-v1\runtime\Firespider Internet 19.exe') ($result.candidate+'\Firespider Internet 19.exe') '638F46BD41B791001861E1AEB50DE9A047277F4BAE475702E903E0EF4CA3A5D5'
        $result.libraryHostReceiptSha256='3739C0B1979EC72622E8030CBB49FAF50824B912DB2C2B24C14DD76D53F38021'
    }
    $adapterRoot=$workspace+'\continuity-evidence\script-adapter-source-build-v2'
    $null=Pin ($adapterRoot+'\result.json') '7178C265B7BCAF50738DC8F610432F4C6B282DAB77442D689D06437CD3BD3E35'
    $adapter=Get-Content -LiteralPath ($adapterRoot+'\result.json') -Raw|ConvertFrom-Json
    if(!$adapter.passed -or !$adapter.inputsUnchanged -or $adapter.ownerAttention){throw 'Adapter build incomplete'}
    CopyExact ($adapterRoot+'\Lea.Script.Win.dll') ($result.candidate+'\Lea.Script.Win.dll') 'AD4A057F5E82C8A36FA8362E54D1F746AD6264B9DF8D5447491DE001E34A59EA'
    CopyExact ($workspace+'\continuity-evidence\current-hit-geometry-v3\Lea.Win.dll') ($result.candidate+'\Lea.Win.dll') '74956DA3FBBF28BC1DF281691FE7F8215C74CA67608AA61F6A6540E3BB0714AC'
    CopyExact ($permissionRoot+'\early-limits-v1\Lea.Script.dll') ($result.candidate+'\Lea.Script.dll') 'E68504B515405FA2C75B92904277A7A8074945BDCC02E1CAD10A57EB9A6FC1E5'
    CopyExact ($permissionRoot+'\early-limits-v1\symbols.pdb') ($result.candidate+'\Lea.Script.pdb') (Get-FileHash ($permissionRoot+'\early-limits-v1\symbols.pdb')).Hash
    foreach($file in $stage.variants[0].files){
        $from=Join-Path $stage.variants[0].directory $file.name;$hash=$file.sha256
        if($file.name -ceq 'Lea.Script.Worker.dll' -and $hash -cne '60FF26D8357FEA52C802BAC562C30C6CE28F72F735E78281D0908E94FAB530D6'){throw 'Normal worker required'}
        if($file.name -ceq 'Jint.dll'){$from=$workspace+'\continuity-evidence\temporal-startup-v1\native-pair-v2\lazy\Jint.dll';$hash='67F3E7D75EDADCBC0110FD6BB7AC26E56784D3650A83C2AE0AA23DD67EB03E6D'}
        CopyExact $from (Join-Path $result.candidate ('ScriptWorker/'+$file.name)) $hash
    }
    if(@(Get-ChildItem ($result.candidate+'\ScriptWorker') -File).Count-ne6){throw 'Do not mix framework and net8 worker dependencies'}
    $diagnosticProof=$workspace+'\continuity-evidence\split-host-diagnostic-v2\result.json'
    $null=Pin $diagnosticProof '7999E9A2CE4AA3B834FB1E731CDBC8C091F00F9E79F82CCDC0202D1745A1FFBC'
    $diagnostic=Get-Content -LiteralPath $diagnosticProof -Raw|ConvertFrom-Json
    if(!$diagnostic.passed -or !$diagnostic.inputsUnchanged -or !$diagnostic.candidateBytesUnchanged -or $diagnostic.ownerAttention){throw 'Failure-only diagnostic launcher binding incomplete'}
    CopyExact ($workspace+'\continuity-evidence\split-host-diagnostic-v2\runtime\Lea.Firespider.Boot.exe') ($result.candidate+'\Lea.Firespider.Boot.exe') '2C44FB4A92088FBF9B548B426B267163E5900A1BD04794BF9843EA869E9532D4'
    foreach($file in Get-ChildItem $result.candidate -Recurse -File){$null=Pin $file.FullName}
    $result.candidateBinaries=@(Get-ChildItem $result.candidate -Recurse -File|Where-Object Extension -In '.exe','.dll','.pdb'|ForEach-Object{@{path=$_.FullName.Substring($result.candidate.Length+1);sha256=(Get-FileHash $_.FullName).Hash}})
    $source=$workspace+'\continuity-evidence\bootstrap-numeric-stores-v1\builds-resume-v2\candidate\source'
    foreach($file in Get-ChildItem ($source+'\LeaForge\test-pages') -File){$null=Pin $file.FullName}
    $result.before=@(Get-ChildItem $result.candidate -Recurse -File|ForEach-Object{@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash;sddl=(Get-Acl -LiteralPath $_.FullName).Sddl}})
    $boot=$result.candidate+'\Lea.Firespider.Boot.exe'
    $probe=SafeRun 'settings-path' $boot @('--settings-path') 30
    if(-not$probe.passed){throw 'Settings probe failed before opening forms.'}
    foreach($case in @('clean','lea','navigation','second','split','edit','style-start','script')){
        $row=SafeRun ('host-'+$case) $boot @(('"'+$result.candidate+'"'),$case) 90
        if($row.passed){$row.passed=@(Get-Content $row.stdout|Where-Object{$_.StartsWith('PASS Lea boot: '+$case+';')}).Count-eq1}
    }
    $startupScript=$workspace+'\continuity-evidence\small-engine-v1\source\Lea.Build\Test-FirespiderStartup.ps1'
    $startupLauncher=$workspace+'\Lea.Build\Invoke-StartupBody.ps1'
    $startupHash=Pin $startupScript;$null=Pin $startupLauncher
    $windowsPowerShell=Join-Path $env:WINDIR 'System32\WindowsPowerShell\v1.0\powershell.exe'
    foreach($scenario in @('clean','restored','locked','oversized','lazy-background','lazy-all')){
        $command="& {`n"+[IO.File]::ReadAllText($startupLauncher)+"`n} -TestScript '"+$startupScript.Replace("'","''")+"' -ExpectedSha256 '"+$startupHash+"' -Root '"+$result.candidate.Replace("'","''")+"' -Scenario '"+$scenario+"' -Evidence '"+$outRoot.Replace("'","''")+"'"
        $encoded=[Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($command))
        $null=SafeRun ('exe-'+$scenario) $windowsPowerShell @('-NoProfile','-STA','-EncodedCommand',$encoded) 100
    }
    $result.expectedRuns=15
    $result.passed=$runs.Count-eq15-and@($runs|Where-Object{-not$_.passed}).Count-eq0
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.originalInputsUnchanged=$true
    foreach($row in $inputs){if((Get-FileHash $row.path).Hash-cne$row.sha256){$result.originalInputsUnchanged=$false}}
    $result.hostStagesPassed=$result.passed
    $result.candidateBytesUnchanged=if($result.before){@($result.before|Where-Object{(Get-FileHash -LiteralPath $_.path).Hash-cne$_.sha256}).Count-eq0}else{$null}
    $result.privateFilesUnchanged=if($result.before){@($result.before|Where-Object{(Get-Acl -LiteralPath $_.path).Sddl-cne$_.sddl}).Count-eq0}else{$null}
    $result.ownerAttention=($global:LeaCompositeProcessLeases.Count-gt0)-or[bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.completed=-not$result.ownerAttention
    $result.decoderPolicyAccepted=$false
    if($result.before){
        $changes=@($result.before|Where-Object{(Get-Acl -LiteralPath $_.path).Sddl -cne $_.sddl})
        $result.decoderPolicyAccepted=$changes.Count -eq 0
        if($changes.Count -eq 1){
            $result.decoderPolicyAccepted=Test-DecoderAclDelta -Path $changes[0].path -ExpectedDecoderPath ($result.candidate+'\Lea.Image.exe') -Before $changes[0].sddl -After (Get-Acl -LiteralPath $changes[0].path).Sddl -ProfileName 'Lea.Image.Decoder'
        }
        $result.aclDeltaPaths=@($changes|ForEach-Object{$_.path})
    }
    $result.passed=$result.passed-and$result.completed-and$result.originalInputsUnchanged-and$result.candidateBytesUnchanged-and$result.decoderPolicyAccepted
    $global:LeaCompositeResult=$result
    $result|ConvertTo-Json -Depth 8|Set-Content ($outRoot+'\result.json') -Encoding utf8
}
Write-Output ($outRoot+'\result.json')
if(-not$result.passed){throw 'Integration gates failed; preserve evidence and inspect original owner.'}


