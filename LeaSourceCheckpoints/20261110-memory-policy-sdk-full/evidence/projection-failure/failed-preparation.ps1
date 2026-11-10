$ErrorActionPreference='Stop'
$out='I:\LeaBuildStorage\memory-policy-full-source-20261110-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve full-suite projections'}
$specs=@(
 @{template='Prepare-CurrentSdkFullSuite.ps1';name='Prepare-MemoryPolicyFullSuite.ps1';pairs=@(
 @('I:\LeaBuildStorage\sdk-worker-source-graph-20261109-v1\source','I:\LeaBuildStorage\memory-policy-sdk-source-20261110-v1\source'),
 @('I:\LeaBuildStorage\sdk-source-full-preparation-20261109-v1','I:\LeaBuildStorage\memory-policy-full-preparation-20261110-v1'),
 @('continuity-evidence/sdk-source-full-suite-v1','continuity-evidence/memory-policy-full-suite-v1'),
 @('I:\LeaBuildStorage\sdk-worker-source-graph-audit-20261109-v1\audit.json','I:\LeaBuildStorage\memory-policy-sdk-audit-20261110-v1\audit.json'),
 @('BBE9F917F32EE3B2B5162B1644FA157B1735BA343F5A943168358D7600D522E7','FD6BBBC7479A5F26F983E180D0C4BBA1EE75582C28F1F692F3D61C63DE52B3DC')
 )},
 @{template='Test-OwnedSdkFullScript.ps1';name='Test-MemoryPolicyFullScript.ps1';pairs=@(
 @('continuity-evidence/sdk-source-full-suite-v1','continuity-evidence/memory-policy-full-suite-v1'),
 @('continuity-evidence/sdk-source-full-owned-v1','continuity-evidence/memory-policy-full-owned-v1')
 )}
)
New-Item -ItemType Directory -Path $out|Out-Null
$rows=@(foreach($spec in $specs){
 $template=Join-Path $PSScriptRoot $spec.template;$original=[IO.File]::ReadAllText($template);$text=$original
 $rootOld='$root=Split-Path $PSScriptRoot -Parent'
 $rootNew="`$root='"+(Split-Path $PSScriptRoot -Parent).Replace("'","''")+"'"
 $pairs=@(@($rootOld,$rootNew))+@($spec.pairs)
 foreach($pair in $pairs){$count=[regex]::Matches($text,[regex]::Escape($pair[0])).Count;if($count -lt 1 -or $text.Contains($pair[1])){throw 'Projection identity boundary missing/colliding'};$text=$text.Replace($pair[0],$pair[1])}
 $literal="'"+$PSScriptRoot.Replace("'","''")+"'";$text=$text.Replace('$PSScriptRoot',$literal)
 $inverse=$text.Replace($literal,'$PSScriptRoot');for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
 if($inverse -cne $original){throw 'Full-suite identity/helper-path inverse differs'}
 $tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors);if($errors.Count){throw 'Full-suite generated syntax invalid'}
 $target=Join-Path $out $spec.name;[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false));if([IO.File]::ReadAllText($target) -cne $text){throw 'Generated source readback differs'}
 @{source=$target;sha256=(Get-FileHash -LiteralPath $target).Hash;template=$template;templateSha256=(Get-FileHash -LiteralPath $template).Hash;pairs=$pairs;inverseVerified=$true;assertionsChanged=$false;limitsChanged=$false}
})
@{verified=$true;scripts=$rows;executed=$false;fullBudgetMs=300000;retirementBudgetMs=5000;compiledSources=60;references=193;requiredChecks=3412;requiredPhases=89}|ConvertTo-Json -Depth 8|Set-Content -LiteralPath (Join-Path $out 'source.json') -Encoding utf8
Get-FileHash -LiteralPath (Join-Path $out 'source.json')
