param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace 'continuity-evidence/print-styles-v2'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
$identity=[Security.Principal.WindowsIdentity]::GetCurrent()
if(([Security.Principal.WindowsPrincipal]::new($identity)).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Normal nonadmin required'}
if(Test-Path $out){throw 'Preserve Framework attempt'}
if((Get-PSDrive C).Free -lt 512MB){throw 'Build reserve required'}
if(Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue){throw 'Fresh owner required'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$Path,[string]$Hash){
    $s=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($s)
    if(!$Hash){$Hash=(Get-FileHash $Path).Hash}
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $Hash){throw ('Input differs: '+$Path)}
    $inputs.Add(@{path=$Path;sha256=$Hash})
}
function Compile([string]$Name,[string[]]$Options){
    $rsp=Join-Path $out ($Name+'.rsp');[IO.File]::WriteAllLines($rsp,$Options);Hold $rsp
    $row=Invoke-OwnedTestRun -Name $Name -Application (Get-Process -Id $PID).Path -Arguments @('-NoProfile','-File',('"'+$wrapper+'"'),'-ResponseFile',('"'+$rsp+'"'),'-ExpectedResponseSha256',(Get-FileHash $rsp).Hash,'-ExpectedSourceSha256','D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB') -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$row.passed){throw ('Compilation failed: '+$Name)}
}
$result=[ordered]@{passed=$false;completed=$false;inputs=$inputs;runs=$runs;before=@();scope='Owned new core print-media style query plus prior print checks; style snapshot/renderer adoption and full regressions unfinished'}
$null=New-Item -ItemType Directory -Path $out
try{
    Hold $PSCommandPath
    Hold (Join-Path $PSScriptRoot 'OwnedTestProcess.cs') 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA'
    Hold (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1') 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00'
    Add-Type -Path (Join-Path $PSScriptRoot 'OwnedTestProcess.cs')
    . (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1')
    $wrapper=Join-Path $PSScriptRoot 'Invoke-PublicCompiler.20261026.ps1';Hold $wrapper 'C3EBB3801FAFB80DA031AAB69FE4837EEB4E4EECD3907F9704F9D6186FA42F0A'
    Hold (Join-Path $PSScriptRoot 'PublicCompiler.cs') 'D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB'
    $baseline=Join-Path $workspace 'continuity-evidence/current-hit-geometry-v3/result.json';Hold $baseline '7EF54C6150CB19A89AF32A04E756C95F8C0D2EC411711B010B3AC8D794C523E0'
    $proof=Get-Content $baseline -Raw|ConvertFrom-Json
    $references=[Collections.Generic.List[string]]::new()
    foreach($name in @('mscorlib.dll','System.dll','System.Core.dll','System.Drawing.dll','System.Windows.Forms.dll','Accessibility.dll','UIAutomationClient.dll','UIAutomationTypes.dll','WindowsBase.dll')){
        $reference=Join-Path $workspace ('continuity-evidence/title-metadata-regression-v8/references/'+$name)
        $pin=@($proof.inputs|Where-Object path -ceq $reference)
        if($pin.Count -ne 1){throw 'Reference pin missing'}
        Hold $reference $pin[0].sha256;$references.Add('/reference:"'+$reference+'"')
    }
    $coreBuilder=Join-Path $PSScriptRoot 'Build-PrintStylesCore.ps1';Hold $coreBuilder
    . $coreBuilder
    $references.Add('/reference:"'+$core+'"')
    foreach($name in @('PresentationCore.dll','PresentationFramework.dll','System.Xaml.dll','ReachFramework.dll','System.Printing.dll','System.Xml.dll','System.Xml.Linq.dll','WindowsFormsIntegration.dll')){
        $reference=Join-Path ${env:ProgramFiles(x86)} ('Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/'+$name)
        Hold $reference
        $references.Add('/reference:"'+$reference+'"')
    }
    $view=Join-Path $workspace 'continuity-evidence/native-selection-view-v1/Lea.Win.dll'
    Hold $view '2BEB3321AB5C70B18303E092451DB4D6B3299A56C232FF9374644C20CE2402CC'
    Copy-Item -LiteralPath $view -Destination (Join-Path $out 'Lea.Win.dll')
    $view=Join-Path $out 'Lea.Win.dll';Hold $view '2BEB3321AB5C70B18303E092451DB4D6B3299A56C232FF9374644C20CE2402CC'
    $references.Add('/reference:"'+$view+'"')
    $fixture=Join-Path $workspace 'LeaForge/test-pages/native-selection.html';Hold $fixture
    Copy-Item -LiteralPath $fixture -Destination (Join-Path $out 'native-selection.html')
    $fixture=Join-Path $out 'native-selection.html';Hold $fixture
    $documentFixture=Join-Path $workspace 'LeaForge/test-pages/print-document.html';Hold $documentFixture
    Copy-Item -LiteralPath $documentFixture -Destination (Join-Path $out 'print-document.html')
    Hold (Join-Path $out 'print-document.html')
    $styleFixture=Join-Path $workspace 'LeaForge/test-pages/print-styles.html';Hold $styleFixture
    Copy-Item -LiteralPath $styleFixture -Destination (Join-Path $out 'print-styles.html')
    Hold (Join-Path $out 'print-styles.html')
    $sources=@('Lea.Print/Ps.cs','Lea.Print/Pc.cs','Lea.Print/Po.cs','Lea.Tests/PrintCaptureProbe.cs','Lea.Tests/PrintSnapshot.cs','Lea.Tests/PrintGeometry.cs','Lea.Tests/PrintUnifiedMain.cs','Lea.Print/Pt.cs','Lea.Tests/PrintTextMain.cs','Lea.Tests/PrintFontProbe.cs','Lea.Tests/PrintWpfProbe.cs','Lea.Print/Pw.cs','Lea.Tests/PrintWpfLayoutMain.cs','Lea.Print/Pf.cs','Lea.Tests/PrintFrameMain.cs','Lea.Print/Pp.cs','Lea.Tests/PrintPaginatorMain.cs','Lea.Print/Px.cs','Lea.Tests/PrintXpsMain.cs','Lea.Print/Pv.cs','Lea.Tests/PrintPreviewMain.cs','Lea.Print/Ph.cs','Lea.Tests/PrintHostMain.cs','Lea.Print/Pm.cs','Lea.Tests/PrintDocumentMain.cs','Lea.Tests/PrintStylesMain.cs','Lea.Print/Lea.Print.csproj')
    foreach($relative in $sources){$path=Join-Path $workspace $relative;Hold $path;Copy-Item -LiteralPath $path -Destination (Join-Path $out ([IO.Path]::GetFileName($path)))}
    $library=Join-Path $out 'Lea.Print.dll'
    $common=@('/nologo','/nostdlib+','/optimize+','/deterministic+')+@($references)
    Compile 'compile-print-library' ($common+@('/target:library',('/out:"'+$library+'"'),('"'+(Join-Path $out 'Ps.cs')+'"'),('"'+(Join-Path $out 'Pc.cs')+'"'),('"'+(Join-Path $out 'Po.cs')+'"'),('"'+(Join-Path $out 'Pt.cs')+'"'),('"'+(Join-Path $out 'Pw.cs')+'"'),('"'+(Join-Path $out 'Pf.cs')+'"'),('"'+(Join-Path $out 'Pp.cs')+'"'),('"'+(Join-Path $out 'Px.cs')+'"'),('"'+(Join-Path $out 'Pv.cs')+'"'),('"'+(Join-Path $out 'Ph.cs')+'"'),('"'+(Join-Path $out 'Pm.cs')+'"')))
    Hold $library
    $exe=Join-Path $out 'PrintTextChecks.exe'
    Compile 'compile-print-tests' ($common+@('/target:exe',('/out:"'+$exe+'"'),('/reference:"'+$library+'"'),'/main:PrintStylesMain',('"'+(Join-Path $out 'PrintCaptureProbe.cs')+'"'),('"'+(Join-Path $out 'PrintSnapshot.cs')+'"'),('"'+(Join-Path $out 'PrintGeometry.cs')+'"'),('"'+(Join-Path $out 'PrintUnifiedMain.cs')+'"'),('"'+(Join-Path $out 'PrintTextMain.cs')+'"'),('"'+(Join-Path $out 'PrintFontProbe.cs')+'"'),('"'+(Join-Path $out 'PrintWpfProbe.cs')+'"'),('"'+(Join-Path $out 'PrintWpfLayoutMain.cs')+'"'),('"'+(Join-Path $out 'PrintFrameMain.cs')+'"'),('"'+(Join-Path $out 'PrintPaginatorMain.cs')+'"'),('"'+(Join-Path $out 'PrintXpsMain.cs')+'"'),('"'+(Join-Path $out 'PrintPreviewMain.cs')+'"'),('"'+(Join-Path $out 'PrintHostMain.cs')+'"'),('"'+(Join-Path $out 'PrintDocumentMain.cs')+'"'),('"'+(Join-Path $out 'PrintStylesMain.cs')+'"')))
    foreach($file in Get-ChildItem $out -File|Where-Object Extension -in @('.cs','.dll','.exe')){Hold $file.FullName}
    $result.before=@(Get-ChildItem $out -File|Where-Object Extension -in @('.dll','.exe')|ForEach-Object{@{path=$_.FullName;sha256=(Get-FileHash $_.FullName).Hash}})
    $row=Invoke-OwnedTestRun -Name 'view-print-capture' -Application $exe -Arguments @(('"'+$fixture+'"')) -WorkingDirectory $out -OutputDirectory $out -Seconds 30 -Records $runs
    $result.footerVerified=@(Get-Content $row.stdout|Where-Object{$_ -ceq 'PASS Lea print styles and foundation'}).Count -eq 1
    $result.completed=$row.passed -and $result.footerVerified
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object{(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.byteChanges=@($result.before|Where-Object{(Get-FileHash $_.path).Hash -cne $_.sha256})
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.completed -and !$result.error -and $result.inputsUnchanged -and !$result.byteChanges.Count -and !$result.ownerAttention
    if($result.ownerAttention){$global:LeaPrintFrameworkInputLeases=$leases}else{foreach($s in $leases){$s.Dispose()}}
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 7))
    [pscustomobject]$result|Select-Object passed,completed,footerVerified,ownerAttention,error|ConvertTo-Json -Compress
}
