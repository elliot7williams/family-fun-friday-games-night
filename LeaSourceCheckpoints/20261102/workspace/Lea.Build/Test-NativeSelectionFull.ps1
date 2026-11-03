param([switch]$KeepForegroundHost,[switch]$DocumentSelection,[ValidateRange(1,9)][int]$Version=1)
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace 'continuity-evidence/native-selection-full-v1'
if($DocumentSelection){$out=Join-Path $workspace ('continuity-evidence/document-selection-native-full-v'+$Version)}
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
$result=[ordered]@{passed=$false;behaviorPassed=$false;candidate=$runtime;inputs=$inputs;runs=$runs;before=@();fixtureBefore=@();scope='Full retained native engine/view suite with experimental Sx view; no full scripting/host/release qualification';adopted=$false}
$null=New-Item -ItemType Directory -Path $runtime
try{
    Hold $PSCommandPath
    Copy-Item $PSCommandPath (Join-Path $out 'runner.ps1')
    Hold (Join-Path $PSScriptRoot 'OwnedTestProcess.cs') 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA'
    Hold (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1') 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00'
    Add-Type -Path (Join-Path $PSScriptRoot 'OwnedTestProcess.cs')
    . (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1')
    if($DocumentSelection){
        $focusedPath=Join-Path $workspace 'continuity-evidence/document-uia-v5/result.json'
        Hold $focusedPath '23C13323A10803C7879A487A9FE640E6BEFFCB38E682026E3CB112C4023E4DE7'
        $qualified=Get-Content $focusedPath -Raw|ConvertFrom-Json
        if(!$qualified.passed -or !$qualified.inputsUnchanged -or $qualified.ownerAttention){throw 'Document selection focused proof incomplete'}
        $focused=[pscustomobject]@{viewSha256='2E4264F2008C451D0F8FB42DE3F5229AF9D9817142E4ED02F785905820687CE7'}
        $viewPath=Join-Path $workspace 'continuity-evidence/document-uia-v5/Lea.Win.dll'
        $result.scope='Full retained native engine/view suite with document selection view; scripting/host/release qualification remains separate'
    }else{
    $focusedPath=Join-Path $workspace 'continuity-evidence/native-selection-view-v2/result.json'
    Hold $focusedPath '3E31B307B6A9B0224E783C68B50DD044E41F06FBA570BB57549DD7326AF8102F'
    $focused=Get-Content $focusedPath -Raw|ConvertFrom-Json
    if(!$focused.passed -or !$focused.inputsUnchanged -or !$focused.dialogFooterVerified -or $focused.ownerAttention -or $focused.viewSha256 -cne '2BEB3321AB5C70B18303E092451DB4D6B3299A56C232FF9374644C20CE2402CC'){throw 'Focused Sx proof incomplete'}
    $viewPath=Join-Path $workspace 'continuity-evidence/native-selection-view-v1/Lea.Win.dll'
    }
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
        if($DocumentSelection -and $name -ceq 'Lea.Tests.exe'){continue}
        if($DocumentSelection -and $name -ceq 'Lea.Engine.dll'){
            CopyPinned (Join-Path $workspace 'continuity-evidence/selection-image-live-v5/Lea.Engine.dll') (Join-Path $runtime $name) 'A85B02C678862D75FA764397C6BB6B3D7B964ECB0C8E13722F6D17DC44AA3791'
            $result.coreSha256='A85B02C678862D75FA764397C6BB6B3D7B964ECB0C8E13722F6D17DC44AA3791'
            continue
        }
        CopyPinned $file.path (Join-Path $runtime $name) $file.sha256
    }
    CopyPinned $viewPath (Join-Path $runtime 'Lea.Win.dll') $focused.viewSha256
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
    if($DocumentSelection){
        $projection=Join-Path $PSScriptRoot 'Project-DocumentSelectionNativeSmoke.ps1';Hold $projection;. $projection
        $original=[IO.File]::ReadAllText($source)
        $projected=Project-DocumentSelectionNativeSmoke $original
        foreach($bad in @($original.Replace('Eq(SupportedTextSelection.None,','Eq(SupportedTextSelection.Single,'),($original+"`n"+$original))){
            $denied=$false;try{$null=Project-DocumentSelectionNativeSmoke $bad}catch{$denied=$true}
            if(!$denied){throw 'Native projection negative control accepted'}
        }
        $testSource=Join-Path $out 'Smoke.cs';[IO.File]::WriteAllText($testSource,$projected,[Text.UTF8Encoding]::new($false));Hold $testSource
        $recipe=Join-Path $baseRoot 'tests.rsp';Hold $recipe 'E06440605480B364CCDA297247CC7DCAF671EF306E5A04BCEC18B2FD35908781'
        $options=@(foreach($line in Get-Content $recipe){
            if($line.StartsWith('/out:')){'/out:"'+(Join-Path $runtime 'Lea.Tests.exe')+'"'}
            elseif($line -ceq ('"'+$source+'"')){'"'+$testSource+'"'}
            elseif($line -match '^/reference:"(.+)"$' -and [IO.Path]::GetFileName($Matches[1]) -in @('Lea.Engine.dll','Lea.Win.dll')){'/reference:"'+(Join-Path $runtime ([IO.Path]::GetFileName($Matches[1])))+'"'}
            else{$line}
        })
        $response=Join-Path $out 'tests.rsp';[IO.File]::WriteAllLines($response,$options);Hold $response
        $wrapper=Join-Path $PSScriptRoot 'Invoke-PublicCompiler.20261026.ps1';Hold $wrapper 'C3EBB3801FAFB80DA031AAB69FE4837EEB4E4EECD3907F9704F9D6186FA42F0A'
        Hold (Join-Path $PSScriptRoot 'PublicCompiler.cs') 'D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB'
        $compiled=Invoke-OwnedTestRun -Name 'compile-native-contract' -Application (Get-Process -Id $PID).Path -Arguments @('-NoProfile','-File',('"'+$wrapper+'"'),'-ResponseFile',('"'+$response+'"'),'-ExpectedResponseSha256',(Get-FileHash $response).Hash,'-ExpectedSourceSha256','D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB') -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
        if(!$compiled.passed){throw 'Native contract compilation failed'}
        $result.testProjectionInverseVerified=$true;$result.testProjectionNegativeControls=2
        Hold (Join-Path $runtime 'Lea.Tests.exe')
    }
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
