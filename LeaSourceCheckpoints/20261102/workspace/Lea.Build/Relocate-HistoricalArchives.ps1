param([switch]$Apply,[switch]$SourceBackups,[switch]$FeatureSourceBackups,[switch]$SdkReserve,[switch]$LegacySdkReserve,[switch]$FullSuiteReserve,[switch]$EntrySuiteReserve,[switch]$StartupReserve)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
$sourceRoot=Join-Path $root 'dist'
$destination='I:\LeaBuildStorage\historical-archives-20261031'
if($StartupReserve){
    if($SourceBackups -or $FeatureSourceBackups -or $SdkReserve -or $LegacySdkReserve -or $FullSuiteReserve -or $EntrySuiteReserve){throw 'Choose one startup reserve batch'}
    $sourceRoot=Join-Path $root 'continuity-evidence';$destination='I:\LeaBuildStorage\startup-reserve-source-zips-20261101-v1'
}
if($EntrySuiteReserve){
    if($SourceBackups -or $FeatureSourceBackups -or $SdkReserve -or $LegacySdkReserve -or $FullSuiteReserve){throw 'Choose one entry-suite reserve batch'}
    $sourceRoot=Join-Path $root 'continuity-evidence';$destination='I:\LeaBuildStorage\entry-suite-reserve-source-zips-20261101-v1'
}
if($SourceBackups){$sourceRoot=Join-Path $root 'continuity-evidence';$destination='I:\LeaBuildStorage\historical-source-backups-20261031'}
if($FeatureSourceBackups){if($SourceBackups){throw 'Choose one explicit archive batch'};$sourceRoot=Join-Path $root 'continuity-evidence';$destination='I:\LeaBuildStorage\historical-feature-source-backups-20261031'}
if($SdkReserve){if($SourceBackups -or $FeatureSourceBackups){throw 'Choose one batch'};$sourceRoot=Join-Path $root 'continuity-evidence';$destination='I:\LeaBuildStorage\sdk-reserve-source-backups-20261031'}
if($LegacySdkReserve){if($SourceBackups -or $FeatureSourceBackups -or $SdkReserve){throw 'Choose one explicit legacy batch'};$sourceRoot=Join-Path $root 'dist';$destination='I:\LeaBuildStorage\legacy-project-zips-20261101-v1'}
if($FullSuiteReserve){if($SourceBackups -or $FeatureSourceBackups -or $SdkReserve -or $LegacySdkReserve){throw 'Choose one full-suite reserve batch'};$sourceRoot=Join-Path $root 'continuity-evidence';$destination='I:\LeaBuildStorage\full-suite-reserve-source-zips-20261101-v1'}
if(Test-Path -LiteralPath $destination){throw 'Fresh destination required'}
$files=@(Get-ChildItem -LiteralPath $sourceRoot -File -Recurse -Filter '*.zip' | Where-Object {
    $_.Name -match '^Lea-runtime-(20[1-9]|21[0-9]|22[0-4])-ready_2026-09-' -or
    $_.Name -in @('Archived-runtime186-work.zip','Archived-Lea-runtime-185-host-stage.zip','Archived-Lea-runtime-185-host-stage2.zip')
})
if($SourceBackups){$files=@(Get-ChildItem -LiteralPath $sourceRoot -File -Filter 'print-data-source-*.zip' | Where-Object {$_.Name -match '^print-data-source-\d{8}-v(\d+)\.zip$' -and [int]$Matches[1] -le 60})}
if($FeatureSourceBackups){
    $names=@('print-preview-host-source-20261027.zip','runtime-8033-host-source-20261027.zip','storage-and-overlap-source-20261026.zip','current-hit-geometry-source-v1.zip','print-cache-source-20261028-v2.zip','print-html-source-20261028-v2.zip','print-preview-regression-source-20261027.zip','title-metadata-contracts-v4.zip','print-command-regression-source-20261028.zip')
    $files=@(foreach($name in $names){Get-Item -LiteralPath (Join-Path $sourceRoot $name)})
}
if(!$SdkReserve -and !$LegacySdkReserve -and !$FullSuiteReserve -and !$EntrySuiteReserve -and !$StartupReserve -and !$files.Count){throw 'No explicit historical archives found'}
if($SdkReserve){$files=@(foreach($name in @('print-cache-source-20261028-v1.zip','title-metadata-contracts-v2.zip','title-metadata-source-v1.zip','current-bootstrap-evidence-v1.zip')){Get-Item -LiteralPath (Join-Path $sourceRoot $name)})}
if($LegacySdkReserve){
    $files=@(Get-ChildItem -LiteralPath $sourceRoot -File -Recurse -Filter '*.zip'|Where-Object {$_.Name -match '^Lea-runtime-\d+(-ready)?_2026-(08|09)-' -or $_.Name -ceq 'Archived-verify-m111-host_2026-09-19_20-13-59.zip' -or $_.Name -match '^(Firespider-Lea-146-|Lea-js-146-native_).*2026-10-'})
    if($files.Count -ne 133 -or ($files|Measure-Object Length -Sum).Sum -ne 238126329){throw 'Inspected legacy ZIP inventory changed'}
}
if($FullSuiteReserve){
    $names=@('print-command-source-20261028.zip','native-selection-host-source-20261027.zip','page-color-source-20261026.zip','print-friendly-project-source-20261028.zip','print-ticket-source-20261028.zip','print-dialog-source-20261028.zip','print-device-source-20261027.zip','print-range-source-20261027.zip','print-project-source-20261027.zip','print-suite-timing-source-20261028-v1.zip','library-and-trace-source-20261026.zip')
    $files=@(foreach($name in $names){Get-Item -LiteralPath (Join-Path $sourceRoot $name)})
}
if($EntrySuiteReserve){
    $names=@('print-html-source-20261028-v1.zip','print-friendly-source-20261028-v7.zip','full-script-paint-source-20261026.zip','native-milestone-20261026.zip','print-images-source-20261028-v1.zip','print-data-source-20261028-v63.zip')
    $files=@(foreach($name in $names){Get-Item -LiteralPath (Join-Path $sourceRoot $name)})
}
if($StartupReserve){$files=@(foreach($name in @('about-and-cleanup-20261026.zip','decoder-launch-source-v1.zip','decoder-launch-source-v2.zip')){Get-Item -LiteralPath (Join-Path $sourceRoot $name)})}
if(!$Apply){$files|Select-Object FullName,Length;return}
if((Get-PSDrive I).Free -lt (($files|Measure-Object Length -Sum).Sum+256MB)){throw 'Destination reserve required'}
New-Item -ItemType Directory -Path $destination | Out-Null
$rows=[Collections.Generic.List[object]]::new()
$manifest=Join-Path $destination 'relocation.json'
foreach($file in $files){
    $full=[IO.Path]::GetFullPath($file.FullName)
    if(!$full.StartsWith($sourceRoot+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Outside allowed source'}
    if($file.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse file refused'}
    $ancestor=$file.Directory
    while($ancestor){if($ancestor.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse path refused'};$ancestor=$ancestor.Parent}
    if(@(Get-Item -LiteralPath $full -Stream * | Where-Object Stream -ne ':$DATA').Count){throw 'Alternate stream requires separate preservation'}
    $relative=[IO.Path]::GetRelativePath($sourceRoot,$full)
    $target=Join-Path $destination $relative
    New-Item -ItemType Directory -Path (Split-Path $target -Parent) -Force | Out-Null
    $hold=[IO.File]::Open($full,'Open','Read','Read')
    try{
        $hash=(Get-FileHash -LiteralPath $full -Algorithm SHA256).Hash
        Copy-Item -LiteralPath $full -Destination $target
        if((Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash -cne $hash -or (Get-Item -LiteralPath $target).Length -ne $file.Length){throw 'Copy mismatch; source retained'}
        $row=[pscustomobject]@{source=$full;destination=$target;bytes=$file.Length;sha256=$hash;sourceRemoved=$false}
        $rows.Add($row)
        $rows | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $manifest -Encoding utf8
    }finally{$hold.Dispose()}
    if((Get-FileHash -LiteralPath $full).Hash -cne $hash){throw 'Source changed; retained'}
    $ancestor=Get-Item -LiteralPath $full
    while($ancestor){if($ancestor.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Source path changed before removal'};$ancestor=if($ancestor -is [IO.FileInfo]){$ancestor.Directory}else{$ancestor.Parent}}
    Remove-Item -LiteralPath $full
    $row.sourceRemoved=$true
    $rows | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $manifest -Encoding utf8
}
$rows | Select-Object source,bytes,sourceRemoved
Write-Output ('Verified relocated bytes: '+($rows|Measure-Object bytes -Sum).Sum)
