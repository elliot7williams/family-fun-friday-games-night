param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$outRoot=Join-Path $workspace 'continuity-evidence/current-title-visuals-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained foreground owner required'}
if(Test-Path -LiteralPath $outRoot){throw 'Preserve current visual evidence'}
$utility=Join-Path $workspace 'continuity-evidence/native-launch-timing-v1/Test-CompositeCandidate.v2.ps1'
if((Get-FileHash -LiteralPath $utility).Hash -cne 'EB00AD17B8078A6CC2F418733BA5D675FC84EF49EED433A527FAC5498DD5ECEF'){throw 'Utility differs'}
$tokens=$null;$errors=$null;$ast=[Management.Automation.Language.Parser]::ParseFile($utility,[ref]$tokens,[ref]$errors)
foreach($name in @('Space','Pin','CopyExact','NewOutputReader','PumpOutput','Run')){
    $defs=@($ast.FindAll({param($node)$node -is [Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -ceq $name},$false))
    if($errors.Count -or $defs.Count -ne 1){throw 'Utility shape differs'}
    . ([scriptblock]::Create($defs[0].Extent.Text))
}
$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$result=[ordered]@{passed=$false;inputs=$inputs;runs=$runs;scope='Current matched DLLs and original 18-check title/native-field visual probe; not whole-browser qualification'}
Space;$null=New-Item -ItemType Directory -Path $outRoot
try{
    $proof=Join-Path $workspace 'continuity-evidence/nested-overlap-regression-v1/result.json';$null=Pin $proof 'E60A49F48671E92B80279171E5FC274BB4D7CFF2020D185A86729B7A38F04B6E'
    $native=Get-Content -LiteralPath $proof -Raw|ConvertFrom-Json
    if(!$native.behaviorPassed -or !$native.bytesUnchanged -or !$native.inputsUnchanged -or $native.ownerAttention){throw 'Matched native proof incomplete'}
    foreach($name in @('Lea.Engine.dll','Lea.Win.dll')){
        $rows=@($native.before|Where-Object {[IO.Path]::GetFileName($_.path) -ceq $name})
        if($rows.Count -ne 1){throw 'Native binary ambiguous'}
        CopyExact $rows[0].path (Join-Path $outRoot $name) $rows[0].sha256
    }
    $old=Join-Path $workspace 'continuity-evidence/title-metadata-visuals-v1'
    $null=Pin (Join-Path $old 'result.json') '04CB8E02EAC3FDEF82D2006C7E6C504C3A8913467DB4926EDFA5701A6D2103D4'
    CopyExact (Join-Path $old 'runtime/TitleMetadataVisuals.exe') (Join-Path $outRoot 'TitleMetadataVisuals.exe') 'EED7881FDD67CD3A7F9A186C7D8F57BA37964F44F30D98DD6AADFAB14F0B3BE9'
    CopyExact (Join-Path $old 'title-metadata.html') (Join-Path $outRoot 'title-metadata.html') '53925CB518EDA89A1BD9761341AF659E327A48FB56E1D9ED1602814B8C38697E'
    $result.before=@(Get-ChildItem -LiteralPath $outRoot -File|ForEach-Object{@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash;sddl=(Get-Acl -LiteralPath $_.FullName).Sddl}})
    $images=Join-Path $outRoot 'images';$null=New-Item -ItemType Directory -Path $images
    $run=Run 'title-visuals' (Join-Path $outRoot 'TitleMetadataVisuals.exe') @(('"'+(Join-Path $outRoot 'title-metadata.html')+'"'),('"'+$images+'"')) 60
    $lines=@(Get-Content -LiteralPath $run.stdout)
    $result.passed=$run.passed -and $lines[-1] -ceq 'Title metadata visuals: checks=18; failures=0'
    $result.images=@(foreach($name in @('title-720.png','title-420.png')){$path=Join-Path $images $name;@{path=$path;sha256=(Get-FileHash -LiteralPath $path).Hash}})
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -gt 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.inputsUnchanged=@($inputs|Where-Object{(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.bytesAndAclsUnchanged=if($result.before){@($result.before|Where-Object{(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256 -or (Get-Acl -LiteralPath $_.path).Sddl -cne $_.sddl}).Count -eq 0}else{$false}
    $result.passed=$result.passed -and !$result.ownerAttention -and $result.inputsUnchanged -and $result.bytesAndAclsUnchanged
    $result|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $outRoot 'result.json')
}
$result|Select-Object passed,ownerAttention,error|ConvertTo-Json
