param([switch]$KeepForegroundHost,[ValidateRange(1,8)][int]$Version=1,[switch]$RecoveryTrace,[switch]$Replay,[switch]$PrivateBindings,[ValidatePattern('^[A-F0-9]{64}$')][string]$PrivateBindingStagingSha256,[ValidatePattern('^[A-F0-9]{64}$')][string]$PrivateBindingAuditSha256)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$candidate=Join-Path $root ('continuity-evidence/sdk-full-script-suite-v'+$Version)
$out=Join-Path $root ('continuity-evidence/sdk-full-script-owned-run-v'+$Version)
if($RecoveryTrace){if($Version -ne 8){throw 'Recovery trace requires deterministic candidate'};$candidate=Join-Path $root 'continuity-evidence/contains-recovery-full-prefix-v1';$out=Join-Path $root 'continuity-evidence/contains-recovery-full-prefix-owned-v1'}
if($Replay){if($Version -ne 8 -or $RecoveryTrace){throw 'Replay requires separate deterministic mode'};$candidate=Join-Path $root 'continuity-evidence/deterministic-full-replay-v1';$out=Join-Path $root 'continuity-evidence/deterministic-full-replay-owned-v1'}
if($PrivateBindings){
    if($Version -ne 8 -or $RecoveryTrace -or $Replay -or !$PrivateBindingStagingSha256 -or !$PrivateBindingAuditSha256){throw 'Private binding full run requires separate pinned mode'}
    $candidate=Join-Path $root 'continuity-evidence/private-binding-full-suite-v1'
    $out=Join-Path $root 'continuity-evidence/private-binding-full-owned-v1'
}elseif($PrivateBindingStagingSha256 -or $PrivateBindingAuditSha256){throw 'Unexpected private binding pins'}
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Get-PSDrive C).Free -lt 600MB -or (Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/evidence and reserve required'}
if(@(Get-Process 'Lea.Script.Tests','Lea.Script.Worker','Lea.Firespider.Boot','Firespider*' -ErrorAction SilentlyContinue).Count){throw 'Do not overlap browser/tests'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$path,[string]$pin){
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($pin -and $pin -cne $hash){throw ('Full script input differs: '+$path)}
    $inputs.Add(@{path=$path;sha256=$hash})
}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;completed=$false;inputs=$inputs;runs=$runs;before=@();requiredChecks=3412;requiredPhases=89;limitsChanged=$false;hostAdopted=$false;currentFullNativeQualified=$false;legacyUiAUnresolved=$true}
$result.diagnosticOnly=$Version -eq 4 -or [bool]$RecoveryTrace
$result.uninstrumentedReplay=[bool]$Replay
$result.privateBindingPair=[bool]$PrivateBindings
$result.releaseQualified=$false
try{
    Hold $PSCommandPath
    Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'runner.ps1')
    $controlPath=Join-Path $root 'continuity-evidence/owned-run-adapter-controls-v2/result.json';Hold $controlPath 'A97DA64E805F92F13D5F6B822AF0EFAB96051BE5E581367B213C98AF2B185020'
    $control=Get-Content $controlPath -Raw|ConvertFrom-Json
    if(!$control.passed -or !$control.inputsUnchanged -or $control.ownerAttention -or $control.controls.Count -ne 5){throw 'Owned adapter controls incomplete'}
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owned
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $stagingPin=if($Version -eq 4){'82D6BD02492F8D97B4AE5FD692E8035E601D72C69FA4F5303DC91AC582B4DC03'}elseif($Version -eq 3){'8630A1522CB258464E9993B7CD2EC661D4D8182C1E846F103CF7DE7595C91629'}elseif($Version -eq 2){'3F3AE0A44A1803C78D746AFB9FB2BBA49A05C90091D334D120832CF83A59C7D0'}else{'DA7C68B40333CB9CF5DC0B8D0B2B9E9C0AB0DEAB2F153B1BD45FC704ECBE6073'}
    $auditPin=if($Version -eq 4){'96B345575CA05FCCAF4D999FCE869CE89B970A88D9FF7D0379A390F10E4ACEE6'}elseif($Version -eq 3){'956AE0C953173B6DFA2E36F3BD2185659F72AF6BABB256F9E88850DFA3FC1164'}elseif($Version -eq 2){'F3E9C5232F2C2413087E77DE182444C8482A3DDBA4532C6208A0FA055610E4E5'}else{'E7964B26DDA9776E19033D6FA061118EA6D469A1A2DE6C9BC0A2473E25AEFA6F'}
    if($Version -eq 5){$stagingPin='03BC4310AEEBF6BD87387E4BA26A10B1D76D4D823064E3AA0C637A85B83DF353';$auditPin='54970F1F1D652628156B7EBEB6E60F13E12D72D4E3D9ADD216C4305EA2DD538A'}
    if($Version -eq 6){$stagingPin='68EE47715CFA033B76921A74813E2E33CCD52059516427B627306C332DDA6FC1';$auditPin='461A67197901870B786E70D9897821253A1C69AEACE6B96643575F218597C340'}
    if($Version -eq 7){$stagingPin='F693258746A0E08024A72011F9E5E6EE242766FAD468BE4849FF4BCC1C0BFBE8';$auditPin='0C5A7E6CE310807FBA80FF6FC74B5555FBF5AFE2389D46527D7C0A23A7476C14'}
    if($Version -eq 8){$stagingPin='90FD0D60BA1CCE04C0923D6E18E7FB5D218F4F04C129DD49F97477AFFD2384CA';$auditPin='59A3BEE3EBE04C37B69075A9C29BDD91E3A1158835A38B9C9FAB2D0524331E82'}
    if($RecoveryTrace){$stagingPin='18C161A100C9991379C1C85925B37212B60F711D83BCBBDC5964F93DD62B9C5F';$auditPin='AC4F55FD88AF47270B9C8BFE6841674368CC4472EA393006C492BF57310670F3'}
    if($Replay){$stagingPin='E39D9685FED2430B972A1D6900C13D07F645C252E3DFD88A9B031F4323556DB9';$auditPin='A09F8250C6A8423718A32422E574B1354FD8CCD265CBEB5142E6485B5C0156EF'}
    if($PrivateBindings){$stagingPin=$PrivateBindingStagingSha256;$auditPin=$PrivateBindingAuditSha256}
    $stagingPath=Join-Path $candidate 'staging.json';Hold $stagingPath $stagingPin
    $auditPath=Join-Path $candidate 'staging-audit.json';Hold $auditPath $auditPin
    $staging=Get-Content $stagingPath -Raw|ConvertFrom-Json;$audit=Get-Content $auditPath -Raw|ConvertFrom-Json
    if($PrivateBindings -and (!$staging.privateBindingPair -or !$audit.privateBindingPair -or $staging.diagnosticOnly -or $audit.diagnosticOnly)){throw 'Private binding full suite scope missing'}
    if($RecoveryTrace -and (!$staging.diagnosticOnly -or !$audit.diagnosticOnly)){throw 'Recovery diagnostic scope missing'}
    if($Replay -and ($staging.diagnosticOnly -or $audit.diagnosticOnly -or !$staging.uninstrumentedReplay -or !$audit.uninstrumentedReplay)){throw 'Uninstrumented replay scope missing'}
    if($Version -eq 4 -and !$staging.diagnosticOnly){throw 'Diagnostic candidate scope missing'}
    if(!$staging.verified -or !$audit.verified -or !$audit.embeddedFixtureReadWithoutLoadingHost -or $audit.embeddedFiles -ne 7 -or $audit.stagingReceiptSha256 -cne (Get-FileHash $stagingPath).Hash){throw 'Fresh staged candidate audit incomplete'}
    $result.candidate=$staging.runtime
    foreach($file in $staging.files){Hold (Join-Path $candidate $file.path) $file.sha256}
    $preflightHelper=Join-Path $PSScriptRoot 'Test-ComponentProbePrerequisites8033.generated.ps1';Hold $preflightHelper '7B2FC0FE6EFA950A72DCFFC63F2251AAD37FC266BED76E4492D821643AE6BC42';. $preflightHelper
    $preflight=Get-LeaComponentProbePrerequisites
    $preflight|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'component-host-prerequisites.json') -Encoding utf8
    if(!$preflight.prerequisitesPresent){throw 'Exact runtime prerequisites missing'}
    $image=Join-Path $staging.runtime 'Lea.Image.exe'
    $sid=[Security.Principal.SecurityIdentifier]::new('S-1-15-2-3597768014-2038560105-1077951562-531713394-1456559389-3372869253-1355503848')
    if(@((Get-Acl $image).GetAccessRules($true,$true,[Security.Principal.SecurityIdentifier])|Where-Object IdentityReference -eq $sid).Count){throw 'Existing decoder grant requires fresh candidate'}
    $footerHelper=Join-Path $PSScriptRoot 'Read-LeaSuiteFooter.ps1';Hold $footerHelper 'CE0005069067DDA4810317C0B9BFAF83339CD0DC929713458F76336FAC82D3B2';. $footerHelper
    $traceHelper=Join-Path $PSScriptRoot 'Read-AllStartupTraceProof.ps1';Hold $traceHelper '6454310B2E41C38B25C9A7085687BCADF9C7D7F95B3DF7D96E7F702DC9241CDF';. $traceHelper
    $result.before=@(foreach($file in $staging.files|Where-Object {$_.path.StartsWith('runtime/')} ){$path=Join-Path $candidate $file.path;@{path=$path;sha256=$file.sha256;sddl=(Get-Acl -LiteralPath $path).Sddl}})
    $run=Invoke-OwnedTestRun -Name 'full-script-timed' -Application $staging.binary -Arguments @(('"'+$staging.fixtureSource+'"')) -WorkingDirectory $staging.runtime -OutputDirectory $out -Seconds 300 -Records $runs
    $result.completed=$true;$result.behaviorPassed=$run.passed
    if(!$run.passed){throw 'Owned full script child failed; retain original behavior/deadline result'}
    $footer=Read-LeaSuiteFooter -Lines @(Get-Content -LiteralPath $run.stdout) -ExpectedChecks 3412
    $result.checks=$footer.Checks;$result.footerVerified=$true
    if($Replay){$result.recoveryMarkersAbsent=@(Get-Content $run.stderr|Where-Object {$_.StartsWith('LEA_CONTAINS_RECOVERY ')}).Count -eq 0;if(!$result.recoveryMarkersAbsent){throw 'Unexpected recovery instrumentation in replay'}}
    $phases=@(Get-Content -LiteralPath $run.stderr|Where-Object {$_ -cmatch '^LEA_SUITE_PHASE [0-9]+ [0-9]+ (begin|end)$'})
    if($phases.Count -ne 178){throw 'Expected all89 phase pairs'}
    for($i=0;$i -lt 89;$i++){
        if($phases[2*$i] -cnotmatch ('^LEA_SUITE_PHASE [0-9]+ '+$i+' begin$') -or $phases[2*$i+1] -cnotmatch ('^LEA_SUITE_PHASE [0-9]+ '+$i+' end$')){throw 'Phase sequence differs'}
    }
    $result.phasesVerified=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    try{
        if($runs.Count -eq 1 -and (Test-Path -LiteralPath $runs[0].stderr) -and (Get-Command Read-AllStartupTraceProof -ErrorAction SilentlyContinue)){
            $proof=Read-AllStartupTraceProof -Lines @(Get-Content -LiteralPath $runs[0].stderr) -StreamsComplete ([bool]($runs[0].stdoutComplete -and $runs[0].stderrComplete)) -ExitObserved ([bool]$runs[0].exitObserved) -TimedOut ([bool]$runs[0].timedOut) -KillRequested ([bool]$runs[0].killRequested)
            $result.traceActivationVerified=$proof.traceActivationVerified;$result.traceEnvironmentRestored=$proof.traceEnvironmentRestored
        }
        $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
        $result.changedFiles=@(foreach($file in $result.before){$sddl=(Get-Acl -LiteralPath $file.path).Sddl;if($sddl -cne $file.sddl -or (Get-FileHash $file.path).Hash -cne $file.sha256){$file.path}})
    }catch{$result.validationError=$_.Exception.ToString()}
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.completed -and $result.behaviorPassed -and $result.footerVerified -and $result.phasesVerified -and $result.traceActivationVerified -and $result.traceEnvironmentRestored -and $result.inputsUnchanged -and !$result.changedFiles.Count -and !$result.ownerAttention -and !$result.error -and !$result.validationError
    if(!$result.ownerAttention){foreach($s in $holds){$s.Dispose()}}else{$global:LeaSdkFullRunLeases=$holds}
    $result|ConvertTo-Json -Depth 8|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object completed,passed,ownerAttention,error,validationError|ConvertTo-Json
}
