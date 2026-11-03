param([switch]$KeepForegroundHost,[ValidateSet(1,2)][int]$Version=2)
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace ('continuity-evidence/native-selection-view-v'+$Version)
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
$identity=[Security.Principal.WindowsIdentity]::GetCurrent()
if(([Security.Principal.WindowsPrincipal]::new($identity)).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Normal nonadmin required'}
if(Test-Path $out){throw 'Preserve native selection attempt'}
if((Get-PSDrive C).Free -lt 600MB){throw 'Storage reserve required'}
if(Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue){throw 'Fresh owner required'}
if(@(Get-Process 'Firespider*','Lea.Firespider.Boot','Lea.Script.Worker','Lea.Script.Tests','NativeSelectionProbe' -ErrorAction SilentlyContinue).Count){throw 'Do not overlap native/browser tests'}
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
}
function Compile([string]$Name,[string]$Response){
    Hold $Response
    $row=Invoke-OwnedTestRun -Name $Name -Application (Get-Process -Id $PID).Path -Arguments @('-NoProfile','-File',('"'+$wrapper+'"'),'-ResponseFile',('"'+$Response+'"'),'-ExpectedResponseSha256',(Get-FileHash $Response).Hash,'-ExpectedSourceSha256','D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB') -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$row.passed){throw ('Compilation failed: '+$Name)}
}
$result=[ordered]@{passed=$false;compiled=$false;behaviorPassed=$false;inputs=$inputs;runs=$runs;before=@();scope='New Sx native-field selection read only; no ordinary page-text selection, printer UI migration, broad view/host qualification or adoption';adopted=$false}
$null=New-Item -ItemType Directory -Path $out
try{
    Hold $PSCommandPath
    Hold (Join-Path $PSScriptRoot 'OwnedTestProcess.cs') 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA'
    Hold (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1') 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00'
    Add-Type -Path (Join-Path $PSScriptRoot 'OwnedTestProcess.cs')
    . (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1')
    $baseline=Join-Path $workspace 'continuity-evidence/current-hit-geometry-v3'
    $receipt=Join-Path $baseline 'result.json';Hold $receipt '7EF54C6150CB19A89AF32A04E756C95F8C0D2EC411711B010B3AC8D794C523E0'
    $proof=Get-Content $receipt -Raw|ConvertFrom-Json
    if(!$proof.passed -or !$proof.inputsUnchanged -or $proof.ownerAttention){throw 'Mature view baseline incomplete'}
    $known=@{};foreach($file in $proof.inputs){$known[$file.path]=$file.sha256}
    $controls=Join-Path $workspace 'continuity-evidence/native-selection-source-v1/result.json';Hold $controls 'AE5EF542D1887F2E4FD7E3C983C52634F8FB5370D0F936DFE5A96A2EF207C92E'
    $control=Get-Content $controls -Raw|ConvertFrom-Json
    if(!$control.passed -or $control.controls.Count -ne 9){throw 'Selection source controls incomplete'}
    foreach($pin in $control.inputs){Hold $pin.Path $pin.Hash}
    $core=Join-Path $out 'Lea.Engine.dll'
    CopyPinned (Join-Path $baseline 'Lea.Engine.dll') $core '52CE235E37B10243D618E4BDF0031E215145974526DF8F758E167C16E791F4BC'
    $wrapper=Join-Path $PSScriptRoot 'Invoke-PublicCompiler.20261026.ps1';Hold $wrapper 'C3EBB3801FAFB80DA031AAB69FE4837EEB4E4EECD3907F9704F9D6186FA42F0A'
    $options=[Collections.Generic.List[string]]::new()
    $recipe=Join-Path $baseline 'view.rsp';Hold $recipe '0550B794CD09AAC0A3274B0206D973D63FA4856C9B513AB6AB38842236742851'
    $view=Join-Path $out 'Lea.Win.dll'
    foreach($line in Get-Content $recipe){
        if($line.StartsWith('/out:')){$line='/out:"'+$view+'"'}
        elseif($line -match '^"(.+\.cs)"$'){
            $from=$Matches[1];$name=[IO.Path]::GetFileName($from);$to=Join-Path $out ('view-source/'+$name)
            if($name -ceq 'V.cs'){
                CopyPinned (Join-Path $workspace 'continuity-evidence/native-selection-source-v1/V.cs') $to $control.candidateSha256
            }else{
                if(!$known.ContainsKey($from)){throw 'Unpinned mature view source'}
                CopyPinned $from $to $known[$from]
            }
            Hold $to;$line='"'+$to+'"'
        }elseif($line -match '^/reference:"([^"]+)"$'){
            $from=$Matches[1]
            if([IO.Path]::GetFileName($from) -ceq 'Lea.Engine.dll'){$from=$core}
            elseif(!$known.ContainsKey($from)){throw 'Unpinned view reference'}
            Hold $from $(if($from -ceq $core){'52CE235E37B10243D618E4BDF0031E215145974526DF8F758E167C16E791F4BC'}else{$known[$from]})
            $line='/reference:"'+$from+'"'
        }
        $options.Add($line)
    }
    $viewRsp=Join-Path $out 'view.rsp';[IO.File]::WriteAllLines($viewRsp,$options)
    if($Version -eq 2){
        $prior=Join-Path $workspace 'continuity-evidence/native-selection-view-v1'
        $priorReceipt=Join-Path $prior 'result.json';Hold $priorReceipt 'FEA23AD0926870E0A0B77B2C16450DC802A09B385F0E28F67D01AAC77A3BB5FC'
        foreach($file in Get-ChildItem (Join-Path $out 'view-source') -File){Hold (Join-Path $prior ('view-source/'+$file.Name)) (Get-FileHash $file.FullName).Hash}
        CopyPinned (Join-Path $prior 'Lea.Win.dll') $view '2BEB3321AB5C70B18303E092451DB4D6B3299A56C232FF9374644C20CE2402CC'
        $result.viewRebuiltThisRun=$false
    }else{Compile 'compile-selection-view' $viewRsp;$result.viewRebuiltThisRun=$true}
    $result.compiled=$true;$result.viewSha256=(Get-FileHash $view).Hash
    $probeSource=Join-Path $workspace 'Lea.Tests/NativeSelectionProbe.cs'
    CopyPinned $probeSource (Join-Path $out 'NativeSelectionProbe.cs') (Get-FileHash $probeSource).Hash
    Hold (Join-Path $out 'NativeSelectionProbe.cs')
    $fixture=Join-Path $out 'native-selection.html'
    CopyPinned (Join-Path $workspace 'LeaForge/test-pages/native-selection.html') $fixture (Get-FileHash (Join-Path $workspace 'LeaForge/test-pages/native-selection.html')).Hash
    Hold $fixture
    $probeRecipe=Join-Path $baseline 'compile.rsp';Hold $probeRecipe '0D8A980F1586A24D3D5030CDA3A42A3C362AB66496715F7147A6748786CF3E4C'
    $options.Clear()
    foreach($line in Get-Content $probeRecipe){
        if($line.StartsWith('/out:')){$line='/out:"'+(Join-Path $out 'NativeSelectionProbe.exe')+'"'}
        elseif($line -match '^"(.+\.cs)"$'){$line='"'+(Join-Path $out 'NativeSelectionProbe.cs')+'"'}
        elseif($line -match '^/reference:"([^"]+)"$'){
            $from=$Matches[1];$name=[IO.Path]::GetFileName($from)
            if($name -ceq 'Lea.Engine.dll'){$from=$core}
            elseif($name -ceq 'Lea.Win.dll'){$from=$view}
            elseif(!$known.ContainsKey($from)){throw 'Unpinned probe reference'}
            Hold $from $(if($from -ceq $core){'52CE235E37B10243D618E4BDF0031E215145974526DF8F758E167C16E791F4BC'}elseif($from -ceq $view){$result.viewSha256}else{$known[$from]})
            $line='/reference:"'+$from+'"'
        }
        $options.Add($line)
    }
    $probeRsp=Join-Path $out 'probe.rsp';[IO.File]::WriteAllLines($probeRsp,$options);Compile 'compile-selection-probe' $probeRsp
    $probe=Join-Path $out 'NativeSelectionProbe.exe'
    $result.before=@(Get-ChildItem $out -File|Where-Object Extension -in @('.exe','.dll')|ForEach-Object{@{path=$_.FullName;sha256=(Get-FileHash $_.FullName).Hash;sddl=(Get-Acl $_.FullName).Sddl}})
    foreach($file in $result.before){Hold $file.path $file.sha256}
    $arguments=@(('"'+([Uri]$fixture).AbsoluteUri+'"'));if($Version -eq 2){$arguments+='dialog'}
    $row=Invoke-OwnedTestRun -Name 'native-selection' -Application $probe -Arguments $arguments -WorkingDirectory $out -OutputDirectory $out -Seconds 90 -Records $runs
    $result.behaviorPassed=$row.passed -and @(Get-Content $row.stdout|Where-Object{$_.StartsWith('PASS Native selection: ')}).Count -eq 1
    if($Version -eq 2){$result.dialogFooterVerified=@(Get-Content $row.stdout|Where-Object{$_.StartsWith('PASS Native selection dialog: ')}).Count -eq 1;$result.behaviorPassed=$result.behaviorPassed -and $result.dialogFooterVerified}
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object{(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.changedFiles=@($result.before|Where-Object{(Get-FileHash $_.path).Hash -cne $_.sha256 -or (Get-Acl $_.path).Sddl -cne $_.sddl})
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.behaviorPassed -and !$result.error -and $result.inputsUnchanged -and !$result.changedFiles.Count -and !$result.ownerAttention
    if($result.ownerAttention){$global:LeaNativeSelectionInputLeases=$leases}else{foreach($s in $leases){$s.Dispose()}}
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 8))
    [pscustomobject]$result|Select-Object passed,compiled,behaviorPassed,ownerAttention,error|ConvertTo-Json -Compress
}
