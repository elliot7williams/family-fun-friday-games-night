param([switch]$Command)
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$source=Join-Path $PSScriptRoot 'Test-Runtime8033HostCampaign.generated.ps1'
$target=Join-Path $PSScriptRoot $(if($Command){'Test-PrintCommandHostCampaign.generated.ps1'}else{'Test-PrintPreviewHostCampaign.generated.ps1'})
$out=Join-Path $workspace $(if($Command){'continuity-evidence/print-command-host-campaign-preparation-v1'}else{'continuity-evidence/print-preview-host-campaign-preparation-v1'})
if((Test-Path -LiteralPath $target) -or (Test-Path -LiteralPath $out)){throw 'Preserve campaign preparation'}
if((Get-FileHash -LiteralPath $source).Hash -cne 'AE8DDE63D812659712A58CC2FB1FCFB8CE17821BA0AF561D724E0D9B26E7A49A'){throw 'Original campaign changed'}
$original=[IO.File]::ReadAllText($source).Replace("`r",'')
$proof=@'
    $previewPath=Join-Path $workspace 'continuity-evidence/print-preview-host-v2/result.json'
    Hold $previewPath 'F046E375FE0277CDDB2433116BF293C8B577DDA91311D0C0B8EDF169C3BA014F'
    $previewProof=Get-Content -LiteralPath $previewPath -Raw|ConvertFrom-Json
    if(!$previewProof.passed -or !$previewProof.behaviorPassed -or !$previewProof.inputsUnchanged -or $previewProof.ownerAttention -or $previewProof.hostSha256 -cne 'F5EBF8F4593FACD4CA33083DEF1B854951DC21232674EAB6C9F3DC0361DD8FBE' -or $previewProof.byteChanges.Count -or $previewProof.aclChanges.Count){throw 'Exact preview qualification incomplete'}
    $result.previewProbeReceiptSha256='F046E375FE0277CDDB2433116BF293C8B577DDA91311D0C0B8EDF169C3BA014F'
'@
$library=@'
    $print=Join-Path $workspace 'continuity-evidence/print-live-capture-v1/Lea.Print.dll'
    Hold $print 'FC867957DF7CD7029BF15455C5163027755A79EA3CAECF5EF55963A6F3FE8A6A'
    Copy-Item -LiteralPath $print -Destination (Join-Path $runtime 'Lea.Print.dll')
    if((Get-FileHash -LiteralPath (Join-Path $runtime 'Lea.Print.dll')).Hash -cne 'FC867957DF7CD7029BF15455C5163027755A79EA3CAECF5EF55963A6F3FE8A6A'){throw 'Print library copy differs'}
    $result.previewHost=$true
'@
if($Command){
    $proof=$proof.Replace('print-preview-host-v2','print-command-host-v2').Replace('F046E375FE0277CDDB2433116BF293C8B577DDA91311D0C0B8EDF169C3BA014F','1C60031947F0486701B0369A490BFE8B5052936E332C2CC4CE7DFEC8E160FECD').Replace('F5EBF8F4593FACD4CA33083DEF1B854951DC21232674EAB6C9F3DC0361DD8FBE','FCFAA635B4403BC4F449761C49BA17F6ED9CFEF443D7164C56C924DDB09FA7C7').Replace('previewProbeReceiptSha256','commandProbeReceiptSha256').Replace('Exact preview qualification','Exact print command qualification')
    $library=$library.Replace('print-live-capture-v1/Lea.Print.dll','print-ticket-project-v1/bin/Lea.Print.dll').Replace('FC867957DF7CD7029BF15455C5163027755A79EA3CAECF5EF55963A6F3FE8A6A','3C014359204CEEDD11E8164A28467D46CB3BA83E5DF6A81802C0F3A7D3AF1795').Replace('previewHost','printCommandHost')
}
$campaignName=if($Command){'host-print-command-owned-v1'}else{'host-print-preview-owned-v1'}
$description=if($Command){'Own 15-stage FCFAA print-command host coreA85/view568/print3C014 exact8.0.33 campaign'}else{'Own 15-stage F5 preview host coreA85/view568/printFC exact8.0.33 campaign'}
$hostPath=if($Command){'continuity-evidence/print-command-host-v2/host-bin/Firespider Internet 19.exe'}else{'continuity-evidence/print-preview-host-v1/host-bin/Firespider Internet 19.exe'}
$hostHash=if($Command){'FCFAA635B4403BC4F449761C49BA17F6ED9CFEF443D7164C56C924DDB09FA7C7'}else{'F5EBF8F4593FACD4CA33083DEF1B854951DC21232674EAB6C9F3DC0361DD8FBE'}
$edits=@(
    @('host-runtime8033-owned-v1',$campaignName),
    @('Own 15-stage FBBD host coreA85/view568 exact8.0.33 campaign',$description),
    @('    $boundaryPath=Join-Path $workspace',($proof.TrimEnd("`n")+"`n    `$boundaryPath=Join-Path `$workspace")),
    @("    `$exe=Join-Path `$workspace 'continuity-evidence/page-color-host-v5/host-bin/Firespider Internet 19.exe'",("    `$exe=Join-Path `$workspace '"+$hostPath+"'")),
    @("    Hold `$exe 'FBBD913CF0B0C654706844DAF078B01B1DFC4A06DF8687678296DE261B24E818'",("    Hold `$exe '"+$hostHash+"'")),
    @("    `$boot=Join-Path `$runtime 'Lea.Firespider.Boot.exe'",($library.TrimEnd("`n")+"`n    `$boot=Join-Path `$runtime 'Lea.Firespider.Boot.exe'")),
    @("if(`$result.before.Count -ne 398){throw 'Candidate inventory differs'}","if(`$result.before.Count -ne 399){throw 'Candidate inventory differs'}"),
    @('$result.decoderPolicyAccepted=$result.before.Count -eq 398 -and $result.aclChanges.Count -eq 0','$result.decoderPolicyAccepted=$result.before.Count -eq 399 -and $result.aclChanges.Count -eq 0')
)
$text=$original
foreach($edit in $edits){
    if([regex]::Matches($text,[regex]::Escape($edit[0])).Count -ne 1){throw ('Unique anchor required: '+$edit[0])}
    $text=$text.Replace($edit[0],$edit[1])
}
$inverse=$text
for($i=$edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($edits[$i][1],$edits[$i][0])}
if($inverse -cne $original){throw 'Campaign source inverse differs'}
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Campaign syntax differs'}
$null=New-Item -ItemType Directory -Path $out
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
foreach($path in @($PSCommandPath,$target)){Copy-Item -LiteralPath $path -Destination (Join-Path $out ([IO.Path]::GetFileName($path)))}
@{passed=$true;inverseVerified=$true;syntaxVerified=$true;sourceSha256=(Get-FileHash -LiteralPath $source).Hash;targetSha256=(Get-FileHash -LiteralPath $target).Hash;expectedRuns=15;expectedRuntimeFiles=399;limitsChanged=$false;runtimeExecuted=$false;scope=($description+'; same15 cases/budgets, old boundary evidence retained only for unchanged source, not transferred to new host')}|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $out 'result.json')
Get-Content -LiteralPath (Join-Path $out 'result.json')
