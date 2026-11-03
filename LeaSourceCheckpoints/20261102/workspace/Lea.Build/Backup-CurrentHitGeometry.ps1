$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$archive=Join-Path $workspace 'continuity-evidence/current-hit-geometry-source-v1.zip'
if(Test-Path -LiteralPath $archive){throw 'Preserve hit geometry source archive'}
$members=[Collections.Generic.List[object]]::new()
$leases=[Collections.Generic.List[IDisposable]]::new()
try{
    foreach($folder in @('focus-click-probe-v1','focus-click-probe-v2','current-hit-geometry-v1','current-hit-geometry-v2','current-hit-geometry-v3','current-hit-full-regression-v1','current-hit-full-regression-v2')){
        $path=Join-Path $workspace ('continuity-evidence/'+$folder)
        if(!(Test-Path -LiteralPath (Join-Path $path 'result.json'))){throw ('Run not terminal: '+$folder)}
        $result=Get-Content -LiteralPath (Join-Path $path 'result.json') -Raw|ConvertFrom-Json
        if($result.ownerAttention){throw ('Unresolved child ownership: '+$folder)}
        foreach($file in Get-ChildItem -LiteralPath $path -File){$members.Add(@{path=$file.FullName;entry=$folder+'/'+$file.Name})}
        $source=Join-Path $path 'view-source'
        if(Test-Path -LiteralPath $source){foreach($file in Get-ChildItem -LiteralPath $source -File){$members.Add(@{path=$file.FullName;entry=$folder+'/view-source/'+$file.Name})}}
    }
    foreach($name in @('New-CurrentHitGeometrySource.ps1','Prepare-CurrentHitGeometry.ps1','Prepare-CurrentHitGeometryV3.ps1','Prepare-PowerShell769Compiler.ps1','Invoke-PublicCompiler.20261026.ps1','PublicCompiler.cs','Test-FocusClickProbe.ps1','Build-CurrentHitGeometryV2.generated.ps1','Test-CurrentHitGeometryV3.generated.ps1','Prepare-CurrentHitFullRegression.ps1','Prepare-CurrentHitFullRegressionV2.ps1','Test-CurrentHitFullRegressionV2.generated.ps1','Backup-CurrentHitGeometry.ps1')){
        $members.Add(@{path=(Join-Path $PSScriptRoot $name);entry='recipes/'+$name})
    }
    foreach($name in @('Lea.Win/V.cs','Lea.Tests/FocusClickProbe.cs','PROJECT_CONTEXT.md','LeaForge/PROGRESS.md','LeaForge/FINISH_PLAN.md','LeaForge/TITLE_METADATA.md','LeaForge/HIT_GEOMETRY.md')){$members.Add(@{path=(Join-Path $workspace $name);entry='working-source/'+$name})}
    $prior=Join-Path $workspace 'continuity-evidence/title-metadata-contracts-v4.zip'
    if((Get-FileHash -LiteralPath $prior).Hash -cne '595950A5B5EA22C5B9B21A6AAD7E24A55B5B41CB27FAD4874B4CD313DA201D83'){throw 'Prior matched source archive differs'}
    $members.Add(@{path=$prior;entry='prior-matched-source.zip'})
    foreach($member in $members){
        $stream=[IO.File]::Open($member.path,'Open','Read','Read');$leases.Add($stream)
        $member.sha256=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream));$stream.Position=0
        $member.stream=$stream
    }
    $zip=[IO.Compression.ZipFile]::Open($archive,[IO.Compression.ZipArchiveMode]::Create)
    try{
        foreach($member in $members){$output=$zip.CreateEntry($member.entry,[IO.Compression.CompressionLevel]::Optimal).Open();try{$member.stream.CopyTo($output)}finally{$output.Dispose()}}
        $manifest=@($members|ForEach-Object{@{entry=$_.entry;sha256=$_.sha256}})
        $writer=[IO.StreamWriter]::new($zip.CreateEntry('manifest.json').Open())
        try{$writer.Write((@{members=$manifest;scope='Local source/evidence snapshot, not portable or release-qualified. Failed runs intentionally retained.'}|ConvertTo-Json -Depth 5))}finally{$writer.Dispose()}
    }finally{$zip.Dispose()}
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{
        if($zip.Entries.Count -ne $members.Count+1){throw 'Archive inventory differs'}
        foreach($member in $members){$stream=$zip.GetEntry($member.entry).Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) -cne $member.sha256){throw 'Archive member differs'}}finally{$stream.Dispose()}}
    }finally{$zip.Dispose()}
    @{archive=$archive;sha256=(Get-FileHash -LiteralPath $archive).Hash;entries=$members.Count+1;verified=$true}|ConvertTo-Json
}finally{foreach($lease in $leases){$lease.Dispose()}}
