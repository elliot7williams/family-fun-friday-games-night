param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace 'continuity-evidence/print-live-native-v1'
$runtime=Join-Path $out 'runtime';$fixtures=Join-Path $out 'fixtures'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
$identity=[Security.Principal.WindowsIdentity]::GetCurrent()
if(([Security.Principal.WindowsPrincipal]::new($identity)).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Normal nonadmin required'}
if(Test-Path $out){throw 'Preserve full regression attempt'}
if((Get-PSDrive C).Free -lt 600MB){throw 'Storage reserve required'}
if(Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue){throw 'Fresh owner required'}
if(@(Get-Process 'Firespider*','Lea.Firespider.Boot','Lea.Script.Worker','Lea.Script.Tests','Lea.Tests','Lea.Image','NativeSelectionProbe' -ErrorAction SilentlyContinue).Count){throw 'Do not overlap native/browser tests'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$Path,[string]$Hash){
    $s=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($s)
    if(!$Hash){$Hash=(Get-FileHash $Path).Hash}
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $Hash){throw ('Input differs: '+$Path)}
    $inputs.Add(@{path=$Path;sha256=$Hash})
}
function CopyPinned([string]$From,[string]$To,[string]$Hash){
    Hold $From $Hash
    $null=New-Item -ItemType Directory -Path (Split-Path $To) -Force
    Copy-Item -LiteralPath $From -Destination $To
    Hold $To $Hash
}
$result=[ordered]@{passed=$false;behaviorPassed=$false;candidate=$runtime;inputs=$inputs;runs=$runs;before=@();fixtureBefore=@();scope='Full retained native suite with live print capture view568A2639 and coreA85B02C6; no full scripting/host/release qualification';adopted=$false}
$null=New-Item -ItemType Directory -Path $runtime
try{
    Hold $PSCommandPath
    Hold (Join-Path $PSScriptRoot 'OwnedTestProcess.cs') 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA'
    Hold (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1') 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00'
    Add-Type -Path (Join-Path $PSScriptRoot 'OwnedTestProcess.cs')
    . (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1')
    $focusedPath=Join-Path $workspace 'continuity-evidence/native-selection-view-v2/result.json'
    Hold $focusedPath '3E31B307B6A9B0224E783C68B50DD044E41F06FBA570BB57549DD7326AF8102F'
    $focused=Get-Content $focusedPath -Raw|ConvertFrom-Json
    if(!$focused.passed -or !$focused.inputsUnchanged -or !$focused.dialogFooterVerified -or $focused.ownerAttention -or $focused.viewSha256 -cne '2BEB3321AB5C70B18303E092451DB4D6B3299A56C232FF9374644C20CE2402CC'){throw 'Focused Sx proof incomplete'}
    $liveRoot=Join-Path $workspace 'continuity-evidence/print-live-capture-v1'
    $livePath=Join-Path $liveRoot 'result.json';Hold $livePath '751F1EF75A7E184DFE245D1DEFE7732EBBB4A58C5E34AD284DFB716AAE57F123'
    $liveProof=Get-Content $livePath -Raw|ConvertFrom-Json
    if(!$liveProof.passed -or !$liveProof.inputsUnchanged -or $liveProof.ownerAttention){throw 'Live view focused proof required'}
    $styleRoot=Join-Path $workspace 'continuity-evidence/print-styles-v3'
    $stylePath=Join-Path $styleRoot 'result.json';Hold $stylePath '445574DF56FB88273F314D07496DF63656C7BCAB4F448AF88463F29DA0750188'
    $styleProof=Get-Content $stylePath -Raw|ConvertFrom-Json
    if(!$styleProof.passed -or !$styleProof.inputsUnchanged -or $styleProof.ownerAttention){throw 'Style core focused proof required'}
    $baseRoot=Join-Path $workspace 'continuity-evidence/nested-overlap-regression-v1'
    $basePath=Join-Path $baseRoot 'result.json'
    Hold $basePath 'E60A49F48671E92B80279171E5FC274BB4D7CFF2020D185A86729B7A38F04B6E'
    $base=Get-Content $basePath -Raw|ConvertFrom-Json
    if(!$base.behaviorPassed -or !$base.fullSuiteFooter -or !$base.inputsUnchanged -or !$base.bytesUnchanged -or $base.ownerAttention -or $base.before.Count -ne 5){throw 'Native baseline incomplete; raw ACL audit remains separate'}
    $observedPath=Join-Path $baseRoot 'outcome-observation.json'
    Hold $observedPath '89AD895CA8DD8A560A9A3DB217AA1FD7955FE42D9D0F147898F13C60149E0F2A'
    $observed=Get-Content $observedPath -Raw|ConvertFrom-Json
    if(!$observed.nativeBehaviorPassed -or !$observed.exactTerminalFooterVerified -or $observed.originalParentBudgetMs -ne 300000 -or $observed.productionPolicyBroadened){throw 'Baseline output binding incomplete'}
    foreach($pin in $base.inputs){Hold $pin.path $pin.sha256}
    foreach($file in $base.before){
        $name=[IO.Path]::GetFileName($file.path)
        if($name -ceq 'Lea.Win.dll'){continue}
        if($name -ceq 'Lea.Engine.dll'){
            CopyPinned (Join-Path $styleRoot 'Lea.Engine.dll') (Join-Path $runtime $name) 'A85B02C678862D75FA764397C6BB6B3D7B964ECB0C8E13722F6D17DC44AA3791'
            continue
        }
        CopyPinned $file.path (Join-Path $runtime $name) $file.sha256
    }
    CopyPinned (Join-Path $liveRoot 'Lea.Win.dll') (Join-Path $runtime 'Lea.Win.dll') '568A26396DA540C56546447DDC6277878ED5A5A71AF9712559BF8E3A894DADF8'
    $baseFixtures=Join-Path $baseRoot 'fixtures'
    foreach($file in Get-ChildItem $baseFixtures -Recurse -File){
        $relative=[IO.Path]::GetRelativePath($baseFixtures,$file.FullName)
        if([IO.Path]::IsPathRooted($relative) -or $relative.Split('\') -contains '..'){throw 'Fixture path escape'}
        $hashes=@($base.inputs|Where-Object{$_.path.EndsWith(('\'+$relative),[StringComparison]::OrdinalIgnoreCase)}|ForEach-Object sha256|Sort-Object -Unique)
        if($hashes.Count -ne 1){throw ('Fixture source pin ambiguous: '+$relative)}
        $to=Join-Path $fixtures $relative
        CopyPinned $file.FullName $to $hashes[0]
        $result.fixtureBefore+=@{path=$to;sha256=$hashes[0]}
    }
    if($result.fixtureBefore.Count -ne 179){throw 'Original fixture inventory differs'}
    $source=Join-Path $baseRoot 'Smoke.cs';Hold $source
    $matches=[regex]::Matches([IO.File]::ReadAllText($source),'Console\.WriteLine\("(?<footer>Lea smoke passed: bounded sticky positioning with compact k,[^"\r\n]*)"\);')
    if($matches.Count -ne 1 -or $matches[0].Groups['footer'].Value.Contains('\')){throw 'Exact unescaped native footer required'}
    $footer=$matches[0].Groups['footer'].Value
    $footerHelper=Join-Path $PSScriptRoot 'Test-LeaSuiteFooter.ps1';Hold $footerHelper 'BFD1DD8FC9DACBCA1E7177AA740F8D7EA8974E5B4C4BB344C1DCD16055EE47AE';. $footerHelper
    $result.before=@(Get-ChildItem $runtime -File|ForEach-Object{@{path=$_.FullName;sha256=(Get-FileHash $_.FullName).Hash;sddl=(Get-Acl $_.FullName).Sddl}})
    if($result.before.Count -ne 5){throw 'Native runtime inventory differs'}
    $run=Invoke-OwnedTestRun -Name 'full-core' -Application (Join-Path $runtime 'Lea.Tests.exe') -Arguments @(('"'+$fixtures+'"')) -WorkingDirectory $runtime -OutputDirectory $out -Seconds 300 -Records $runs
    $result.fullSuiteFooter=Test-LeaSuiteFooter $run.stdout $footer
    $result.behaviorPassed=$run.passed -and $result.fullSuiteFooter
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object{(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.byteChanges=@($result.before|Where-Object{(Get-FileHash $_.path).Hash -cne $_.sha256})
    $result.aclChanges=@($result.before|Where-Object{(Get-Acl $_.path).Sddl -cne $_.sddl})
    $result.bytesUnchanged=$result.before.Count -eq 5 -and !$result.byteChanges.Count
    $result.fixtureBytesUnchanged=@($result.fixtureBefore|Where-Object{(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.behaviorPassed -and !$result.error -and $result.inputsUnchanged -and $result.bytesUnchanged -and $result.fixtureBytesUnchanged -and !$result.aclChanges.Count -and !$result.ownerAttention
    if($result.ownerAttention){$global:LeaNativeSelectionFullInputLeases=$leases}else{foreach($s in $leases){$s.Dispose()}}
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 8))
    [pscustomobject]$result|Select-Object passed,behaviorPassed,fullSuiteFooter,inputsUnchanged,fixtureBytesUnchanged,ownerAttention,error|ConvertTo-Json -Compress
}
