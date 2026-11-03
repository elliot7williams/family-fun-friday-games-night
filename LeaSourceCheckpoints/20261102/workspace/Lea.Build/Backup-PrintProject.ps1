param([switch]$Range,[switch]$Device,[switch]$Dialog,[switch]$Ticket,[switch]$CommandHost,[switch]$Friendly)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
if($Device -and !$Range){throw 'Device checkpoint includes range sources'}
if($Dialog -and !$Device){throw 'Dialog checkpoint includes device sources'}
if($Ticket -and !$Dialog){throw 'Ticket checkpoint includes native selection sources'}
if($CommandHost -and !$Ticket){throw 'Command host checkpoint includes prepared printer sources'}
if($Friendly -and (!$Ticket -or $CommandHost)){throw 'Friendly project checkpoint requires ticket coverage and excludes an unqualified command host'}
$archiveName=if($Friendly){'print-friendly-project-source-20261028.zip'}elseif($CommandHost){'print-command-source-20261028.zip'}elseif($Ticket){'print-ticket-source-20261028.zip'}elseif($Dialog){'print-dialog-source-20261028.zip'}elseif($Device){'print-device-source-20261027.zip'}elseif($Range){'print-range-source-20261027.zip'}else{'print-project-source-20261027.zip'}
$archivePath=Join-Path $root ('continuity-evidence/'+$archiveName)
$receiptPath=[IO.Path]::ChangeExtension($archivePath,'.json')
if((Test-Path $archivePath) -or (Test-Path $receiptPath)){throw 'Preserve existing checkpoint'}
if((Get-PSDrive C).Free -lt 512MB){throw 'Storage reserve required'}
$files=@(Get-ChildItem (Join-Path $root 'Lea.Print') -File)
$directories=if($Friendly){@('print-friendly-project-v1','print-friendly-v2')}elseif($CommandHost){@('print-command-host-v2')}elseif($Ticket){@('print-ticket-project-v1')}elseif($Dialog){@('print-dialog-project-v6')}elseif($Device){@('print-device-project-v1')}elseif($Range){@('print-range-project-v1','print-range-project-v2')}else{@('print-project-v1')}
if($Friendly){
    $proof=Join-Path $root 'continuity-evidence/print-friendly-project-v1/result.json'
    if((Get-FileHash $proof).Hash -cne '120081FE1A8B946D16D36BCD4EA889AD24F43FAF80FEAFAB12EAD6986F094541'){throw 'Friendly project proof differs'}
    foreach($name in @('Lea.Tests/PrintFriendlyMain.cs','Lea.Tests/PrintSnapshot.cs','Lea.Build/Test-PrintFriendly.ps1',
        'Lea.Build/Get-PrintFriendlyBaseline.ps1','Lea.Build/Get-PrintFriendlyViewBaseline.ps1',
        'Lea.Build/Test-PrintFriendlyBaseline.ps1','Lea.Build/Test-PrintFriendlyViewBaseline.ps1','LeaForge/PRINT_FRIENDLY.md')){$files+=Get-Item (Join-Path $root $name)}
}
foreach($directory in $directories){$files+=@(Get-ChildItem (Join-Path $root ('continuity-evidence/'+$directory)) -File -Recurse)}
if($Range){foreach($name in @('Lea.Tests/PrintRange.cs','Lea.Tests/Lea.Print.Range.Tests.csproj','LeaForge/FINISH_PLAN.md')){$files+=Get-Item (Join-Path $root $name)}}
if($Device){$files+=Get-Item (Join-Path $root 'Lea.Tests/PrintDevice.cs')}
if($Ticket){$files+=Get-Item (Join-Path $root 'Lea.Tests/PrintTicket.cs')}
if($CommandHost){
    foreach($name in @('Lea.Firespider/PrintCommand.vb','Lea.Firespider/PrintPreview.vb','Lea.Build/Test-PrintPreviewHost.ps1','Lea.Build/New-PrintCommandHostSource.ps1','Lea.Build/New-PrintCommandHarness.ps1','Lea.Build/PrintCommandHostProbe.fragment.cs','Lea.Build/New-PrintPreviewHostSource.ps1','Lea.Build/New-PrintPreviewHarness.ps1','Lea.Build/PrintPreviewHostProbe.fragment.cs','LeaForge/CURRENT_HOST.md','LeaForge/history/PRINT_NEXT_ACTION_20261028_before_command.md','continuity-evidence/print-ticket-project-v1/result.json')){$files+=Get-Item (Join-Path $root $name)}
    $files+=Get-ChildItem (Join-Path $root 'continuity-evidence/print-command-host-v1') -File
    $files+=Get-Item (Join-Path $root 'continuity-evidence/print-command-host-v1/host-source/PrintCommand.vb')
}
if($Dialog){
    $files+=Get-Item (Join-Path $root 'Lea.Tests/PrintDialog.cs')
    foreach($version in 1..5){
        $prior=Join-Path $root ('continuity-evidence/print-dialog-project-v'+$version)
        $files+=Get-ChildItem $prior -File
        foreach($name in @('source/Pj.cs','test-source/PrintDialog.cs','test-source/Lea.Print.Range.Tests.csproj')){$files+=Get-Item (Join-Path $prior $name)}
    }
    $files+=Get-ChildItem (Join-Path $root 'continuity-evidence/project-compression-20261027-historicaltext-native-dialog') -File
}
$files+=@(Get-ChildItem (Join-Path $root 'continuity-evidence') -Directory -Filter 'windows-component-*' | Get-ChildItem -File)
foreach($name in @('Lea.Build/Test-PrintProject.ps1','Lea.Build/Backup-PrintProject.ps1','Lea.Build/Invoke-WindowsComponentMaintenance.ps1','Lea.Build/OwnedTestProcess.cs','Lea.Build/Invoke-OwnedTestRun.ps1','PROJECT_CONTEXT.md','LeaForge/PRINTING.md','LeaForge/PROGRESS.md','LeaForge/STORAGE_CLEANUP.md')){$files+=Get-Item (Join-Path $root $name)}
$files=@($files|Sort-Object FullName -Unique)
$members=[Collections.Generic.List[object]]::new()
$archive=[IO.Compression.ZipFile]::Open($archivePath,'Create')
try{
    foreach($file in $files){
        $name=[IO.Path]::GetRelativePath($root,$file.FullName).Replace('\','/')
        if($name.StartsWith('../') -or [IO.Path]::IsPathFullyQualified($name)){throw 'Archive path escape'}
        $hash=(Get-FileHash -LiteralPath $file.FullName).Hash
        $null=[IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive,$file.FullName,$name,'Optimal')
        $members.Add(@{entry=$name;path=$file.FullName;sha256=$hash;bytes=$file.Length})
    }
}finally{$archive.Dispose()}
$archive=[IO.Compression.ZipFile]::OpenRead($archivePath)
try{
    if($archive.Entries.Count -ne $members.Count){throw 'Entry count mismatch'}
    foreach($member in $members){
        $entry=$archive.GetEntry($member.entry);$stream=$entry.Open()
        try{$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))}finally{$stream.Dispose()}
        if($entry.Length -ne $member.bytes -or $hash -cne $member.sha256 -or (Get-FileHash $member.path).Hash -cne $hash){throw 'Checkpoint content mismatch'}
    }
}finally{$archive.Dispose()}
$result=@{verified=$true;files=$members.Count;bytes=(Get-Item $archivePath).Length;sha256=(Get-FileHash $archivePath).Hash;members=$members;scope='Direct project source/build/focused evidence plus system-maintenance receipts; supplements existing mature engine/view source checkpoints, not complete SDK or off-device backup'}
[IO.File]::WriteAllText($receiptPath,($result|ConvertTo-Json -Depth 6))
$result|Select-Object verified,files,bytes,sha256|ConvertTo-Json -Compress
