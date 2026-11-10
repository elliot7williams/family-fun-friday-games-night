$ErrorActionPreference='Stop'
$base='I:\LeaBuildStorage\sdk-worker-source-graph-20261109-v1'
$patched='I:\LeaBuildStorage\memory-document-policy-view-20261110-v1'
$out='I:\LeaBuildStorage\memory-policy-sdk-source-20261110-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve matched SDK preparation'}
$basePath=Join-Path $base 'source.json'
if((Get-FileHash -LiteralPath $basePath).Hash -cne '51A823667E96D3ED4C3EC526E0747B8C19B1EBBE59A8A05324503DBEB4FED9C9'){throw 'Baseline SDK manifest differs'}
$audit='I:\LeaBuildStorage\memory-script-policy-audit-20261110-v1\audit.json'
if((Get-FileHash -LiteralPath $audit).Hash -cne 'A5B80CC913EA43CBC5D9EA1AAD97AB851B4B3A082858533A076671353A0CCC4A'){throw 'Guarded policy audit differs'}
$proofPath=Join-Path $patched 'result.json'
if((Get-FileHash -LiteralPath $proofPath).Hash -cne '4E40211A9FD553BA9F2536ADB24BE05BCAE0B35CB39AB4DD50142A5AE0E2FDE8'){throw 'Guarded source proof differs'}
$proof=Get-Content -LiteralPath $proofPath -Raw|ConvertFrom-Json
if(!$proof.passed -or !$proof.inputsUnchanged -or $proof.ownerAttention){throw 'Guarded source candidate incomplete'}
$manifest=Get-Content -LiteralPath $basePath -Raw|ConvertFrom-Json
if($manifest.files.Count -ne 136 -or $manifest.sourceMembers -ne 128 -or $manifest.dependencyMembers -ne 8){throw 'Complete source closure required'}
$overrides=@('Lea.Engine/E.cs','Lea.Engine/P.cs','Lea.Win/V.cs')
$stage=Join-Path $out 'source'
$rows=@(foreach($file in $manifest.files){
 $from=Join-Path $base ('source/'+$file.path);$hash=$file.sha256
 if((Get-FileHash -LiteralPath $from).Hash -cne $hash){throw 'Baseline source drift'}
 if($file.path -cin $overrides){$from=Join-Path $patched ('source/'+$file.path);$pin=@($proof.inputs|Where-Object path -CEQ $from);if($pin.Count -ne 1){throw 'Unique guarded source pin required'};$hash=$pin[0].sha256;if((Get-FileHash -LiteralPath $from).Hash -cne $hash -or $hash -ceq $file.sha256){throw 'Guarded source override differs'}}
 $to=Join-Path $stage $file.path;New-Item -ItemType Directory -Path (Split-Path $to -Parent) -Force|Out-Null;Copy-Item -LiteralPath $from -Destination $to
 if((Get-FileHash -LiteralPath $to).Hash -cne $hash){throw 'SDK source readback differs'}
 @{path=$file.path;origin=$from;sha256=$hash;baselineSha256=$file.sha256;changed=$hash -cne $file.sha256}
})
if(@($rows|Where-Object changed).Count -ne 3){throw 'Only three guarded source changes permitted'}
$sourcePath=Join-Path $out 'source.json'
@{verified=$true;sourceMembers=128;dependencyMembers=8;files=$rows;inputs=@(@{path=$basePath;sha256=(Get-FileHash -LiteralPath $basePath).Hash},@{path=$audit;sha256=(Get-FileHash -LiteralPath $audit).Hash},@{path=$proofPath;sha256=(Get-FileHash -LiteralPath $proofPath).Hash});overrides=$overrides;seededLeaProducts=$false;compiled=$false;completeSdkQualified=$false}|ConvertTo-Json -Depth 7|Set-Content -LiteralPath $sourcePath -Encoding utf8
$template=Join-Path $PSScriptRoot 'Test-CurrentSdkWorker.ps1'
$original=[IO.File]::ReadAllText($template);$text=$original
$pairs=@(
 @("`$root=Split-Path `$PSScriptRoot -Parent",("`$root='"+(Split-Path $PSScriptRoot -Parent).Replace("'","''")+"'")),
 @('I:\LeaBuildStorage\sdk-worker-source-graph-20261109-v1',$out),
 @('continuity-evidence/sdk-worker-source-graph-v1','continuity-evidence/memory-policy-sdk-v1'),
 @('51A823667E96D3ED4C3EC526E0747B8C19B1EBBE59A8A05324503DBEB4FED9C9',(Get-FileHash -LiteralPath $sourcePath).Hash)
)
foreach($pair in $pairs){if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1 -or $text.Contains($pair[1])){throw 'Unique SDK identity projection required'};$text=$text.Replace($pair[0],$pair[1])}
$literal="'"+$PSScriptRoot.Replace("'","''")+"'"
$text=$text.Replace('$PSScriptRoot',$literal)
$inverse=$text.Replace($literal,'$PSScriptRoot');for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
if($inverse -cne $original){throw 'SDK assertion/helper-path inverse differs'}
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors);if($errors.Count){throw 'SDK runner syntax invalid'}
$runner=Join-Path $out 'Test-MemoryPolicySdk.ps1';[IO.File]::WriteAllText($runner,$text,[Text.UTF8Encoding]::new($false))
@{verified=$true;templateSha256=(Get-FileHash -LiteralPath $template).Hash;sourceSha256=(Get-FileHash -LiteralPath $sourcePath).Hash;runner=$runner;runnerSha256=(Get-FileHash -LiteralPath $runner).Hash;inverseVerified=$true;assertionsChanged=$false;deadlinesChanged=$false;nativeFallbackAllowed=$false;executed=$false}|ConvertTo-Json -Depth 4|Set-Content -LiteralPath (Join-Path $out 'preparation.json') -Encoding utf8
Get-FileHash -LiteralPath $sourcePath,(Join-Path $out 'preparation.json')
