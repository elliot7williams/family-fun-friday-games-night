param([switch]$KeepForegroundHost,[Parameter(Mandatory)][ValidatePattern('^[A-F0-9]{64}$')][string]$ExpectedBoundaryReceipt)
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/host-native-selection-owned-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
$identity=[Security.Principal.WindowsIdentity]::GetCurrent()
if(!$identity.User.Value.EndsWith('-1001') -or ([Security.Principal.WindowsPrincipal]::new($identity)).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Normal nonadmin owner required'}
if(Test-Path $out){throw 'Preserve campaign evidence'}
if((Get-PSDrive C).Free -lt 600MB){throw 'Storage reserve required'}
if(Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue){throw 'Fresh owner required'}
if(@(Get-Process 'Lea.Script.Tests','Lea.Script.Worker','Lea.Firespider.Boot','Firespider*','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'Do not overlap browser/tests'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$Path,[string]$Hash){
    $s=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($s)
    if(!$Hash){$Hash=(Get-FileHash $Path).Hash}
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $Hash){throw ('Input differs: '+$Path)}
    $inputs.Add(@{path=$Path;sha256=$Hash})
}
function Run([string]$Name,[string]$Application,[string[]]$Arguments,[int]$Seconds){
    Invoke-OwnedTestRun -Name $Name -Application $Application -Arguments $Arguments -WorkingDirectory $runtime -OutputDirectory $out -Seconds $Seconds -Records $runs
}
$runtime=Join-Path $out 'runtime'
$result=[ordered]@{passed=$false;completed=$false;candidate=$runtime;runs=$runs;inputs=$inputs;before=@();expectedRuns=15;scope='Own 15-stage FBBD host with new native-selection view2BEB campaign with original case budgets; original process/Job ownership; not clean-PC, all-sites, full release or profile cleanup proof';fullReleaseQualified=$false;installedBrowserChanged=$false}
$null=New-Item -ItemType Directory -Path $runtime
try{
    Hold $PSCommandPath
    Hold (Join-Path $PSScriptRoot 'OwnedTestProcess.cs') 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA'
    Hold (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1') 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00'
    Add-Type -Path (Join-Path $PSScriptRoot 'OwnedTestProcess.cs')
    . (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1')
    $boundaryPath=Join-Path $workspace 'continuity-evidence/page-color-host-v7/result.json';Hold $boundaryPath $ExpectedBoundaryReceipt
    $boundary=Get-Content $boundaryPath -Raw|ConvertFrom-Json
    if(!$boundary.passed -or !$boundary.boundaryFooterVerified -or !$boundary.inputsUnchanged -or $boundary.ownerAttention -or $boundary.executableSha256 -cne 'FBBD913CF0B0C654706844DAF078B01B1DFC4A06DF8687678296DE261B24E818'){throw 'Boundary source/host proof incomplete'}
    foreach($proof in @(
        @('continuity-evidence/native-selection-full-v1/outcome-observation.json','47761CBADBADA54A4AEDB45E50A5E784F702380006ECED300BC7540E59BEFCBD'),
        @('continuity-evidence/temporal-startup-v1/native-selection-script-owned-run-v1/outcome-observation.json','7387BB5A4FE461712DB314D7BAF65616DD5B047DFADF92AB09467EDDAF0458B0')
    )){Hold (Join-Path $workspace $proof[0]) $proof[1]}
    $newView=Join-Path $workspace 'continuity-evidence/native-selection-view-v1/Lea.Win.dll'
    $newViewHash='2BEB3321AB5C70B18303E092451DB4D6B3299A56C232FF9374644C20CE2402CC'
    Hold $newView $newViewHash
    $basePath=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/host-library-current-native-v1/result.json'
    Hold $basePath 'C2731D13019074BDB7E82353DDE0B1DDC11D47FFF1AA58ABA8D1A157468008FA'
    $base=Get-Content $basePath -Raw|ConvertFrom-Json
    if(!$base.passed -or !$base.originalInputsUnchanged -or !$base.candidateBytesUnchanged -or $base.ownerAttention -or $base.before.Count -ne 398){throw 'Base component manifest incomplete'}
    foreach($file in $base.before){
        Hold $file.path $file.sha256
        $relative=[IO.Path]::GetRelativePath($base.candidate,$file.path);$to=Join-Path $runtime $relative
        $null=New-Item -ItemType Directory -Path (Split-Path $to) -Force
        if($relative -ceq 'Lea.Win.dll'){
            if($file.sha256 -cne '74956DA3FBBF28BC1DF281691FE7F8215C74CA67608AA61F6A6540E3BB0714AC'){throw 'Original view differs'}
            Copy-Item -LiteralPath $newView -Destination $to
        }else{Copy-Item -LiteralPath $file.path -Destination $to}
        if((Get-FileHash $to).Hash -cne $(if($relative -ceq 'Lea.Win.dll'){$newViewHash}else{$file.sha256})){throw 'Copied component differs'}
    }
    $exe=Join-Path $workspace 'continuity-evidence/page-color-host-v5/host-bin/Firespider Internet 19.exe'
    Hold $exe 'FBBD913CF0B0C654706844DAF078B01B1DFC4A06DF8687678296DE261B24E818'
    Copy-Item -LiteralPath $exe -Destination (Join-Path $runtime 'Firespider Internet 19.exe')
    $boot=Join-Path $runtime 'Lea.Firespider.Boot.exe'
    Hold $boot '2C44FB4A92088FBF9B548B426B267163E5900A1BD04794BF9843EA869E9532D4'
    $policy=Join-Path $PSScriptRoot 'Test-DecoderAclDelta.ps1';Hold $policy '15F46DBC7F4D4AD5B816DDFA1FE7213D02CAE1EDE79844AFBD904F281299E71E';. $policy
    $startup=Join-Path $workspace 'continuity-evidence/small-engine-v1/source/Lea.Build/Test-FirespiderStartup.ps1'
    $startupHash='1042F5CB68AB16A1C66EFB8524B2C5FD6C1C5098A74F088161060692BD629414'
    Hold $startup $startupHash
    $launcher=Join-Path $PSScriptRoot 'Invoke-StartupBody.ps1';Hold $launcher 'BD2DB16A23A8C41B51BACC78AB9E6501928A44BDE72634CA79290B7EFAD68548'
    $result.before=@(Get-ChildItem $runtime -Recurse -File|ForEach-Object{@{path=$_.FullName;sha256=(Get-FileHash $_.FullName).Hash;sddl=(Get-Acl $_.FullName).Sddl}})
    if($result.before.Count -ne 398){throw 'Candidate inventory differs'}
    foreach($file in $result.before){Hold $file.path $file.sha256}
    $probe=Run 'settings-path' $boot @('--settings-path') 30
    if(!$probe.passed){throw 'Settings preflight failed'}
    foreach($case in @('clean','lea','navigation','second','split','edit','style-start','script')){
        $run=Run ('host-'+$case) $boot @(('"'+$runtime+'"'),$case) 90
        $run.exactFooterVerified=@(Get-Content $run.stdout|Where-Object{$_.StartsWith('PASS Lea boot: '+$case+';')}).Count -eq 1
    }
    $shell=Join-Path $env:WINDIR 'System32/WindowsPowerShell/v1.0/powershell.exe'
    foreach($scenario in @('clean','restored','locked','oversized','lazy-background','lazy-all')){
        $nl=[Environment]::NewLine
        $command='& {'+$nl+[IO.File]::ReadAllText($launcher)+$nl+"} -TestScript '"+$startup.Replace("'","''")+"' -ExpectedSha256 '"+$startupHash+"' -Root '"+$runtime.Replace("'","''")+"' -Scenario '"+$scenario+"' -Evidence '"+$out.Replace("'","''")+"'"
        $encoded=[Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($command))
        $null=Run ('exe-'+$scenario) $shell @('-NoProfile','-STA','-EncodedCommand',$encoded) 100
    }
    $result.hostStagesPassed=$runs.Count -eq 15 -and @($runs|Where-Object{!$_.passed -or ($_.ContainsKey('exactFooterVerified') -and !$_.exactFooterVerified)}).Count -eq 0
    $result.completed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object{(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.byteChanges=@($result.before|Where-Object{(Get-FileHash $_.path).Hash -cne $_.sha256})
    $result.aclChanges=@($result.before|Where-Object{(Get-Acl $_.path).Sddl -cne $_.sddl})
    $result.decoderPolicyAccepted=$result.before.Count -eq 398 -and $result.aclChanges.Count -eq 0
    if($result.aclChanges.Count -eq 1 -and (Get-Command Test-DecoderAclDelta -ErrorAction SilentlyContinue)){
        $file=$result.aclChanges[0]
        $result.decoderPolicyAccepted=Test-DecoderAclDelta -Path $file.path -ExpectedDecoderPath (Join-Path $runtime 'Lea.Image.exe') -Before $file.sddl -After (Get-Acl $file.path).Sddl -ProfileName 'Lea.Image.Decoder'
    }
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.completed -and $result.hostStagesPassed -and !$result.error -and $result.inputsUnchanged -and !$result.byteChanges.Count -and $result.decoderPolicyAccepted -and !$result.ownerAttention
    if($result.ownerAttention){$global:LeaPageColorCampaignInputLeases=$leases}else{foreach($s in $leases){$s.Dispose()}}
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 8))
    [pscustomobject]$result|Select-Object passed,completed,hostStagesPassed,decoderPolicyAccepted,ownerAttention,error|ConvertTo-Json -Compress
}
