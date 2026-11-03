$ErrorActionPreference='Stop'
$from=Join-Path $PSScriptRoot 'Test-GlobalInstallFeature-v1.generated.ps1'
$to=Join-Path $PSScriptRoot 'Test-PrivateBindingIsolatedFeature-v1.generated.ps1'
if(Test-Path $to){throw 'Preserve private binding isolated campaign'}
if((Get-FileHash $from).Hash -cne 'DE5C80EEF528EAAD40735274122013A7A9E9A94E0670E3403B14FE97CABC4F5D'){throw 'Feature template changed'}
$original=[IO.File]::ReadAllText($from)
$a=$original.IndexOf("    `$runtime=Join-Path `$out 'runtime';")
$worker=$original.IndexOf('        $workerProof=',$a)
$b=$original.LastIndexOf('    if($Isolated){',$worker)
if($a -lt 0 -or $b -le $a){throw 'Explicit closure staging boundary required'}
$old=$original.Substring($a,$b-$a)
$shared=@'
    if(!$Isolated){throw 'This campaign qualifies only the matching isolated feature'}
    $runtime=Join-Path $root 'continuity-evidence/private-binding-trusted-v1/runtime'
    $previousPath=Join-Path $root 'continuity-evidence/private-binding-trusted-v1/result.json'
    Pin $previousPath 'D62490714583EC7956AAFCE575579BF81F4EF8F9D1C1288A6C271B3930A43952'
    $previous=Get-Content $previousPath -Raw|ConvertFrom-Json
    if(!$previous.passed -or !$previous.inputsUnchanged -or $previous.ownerAttention -or $previous.changedFiles.Count -or $previous.before.Count -ne 13){throw 'Trusted closure qualification incomplete'}
    if(@(Get-ChildItem $runtime -Recurse -File).Count -ne 13){throw 'Trusted closure inventory changed'}
    foreach($file in $previous.before){Pin $file.path $file.sha256;if((Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Trusted closure ACL changed'}}
    if(Test-Path (Join-Path $runtime 'ScriptWorker')){throw 'Preserve existing worker staging'}
    $reserveProof='I:/LeaBuildStorage/global-install-worker-build-20261102-v1/result.json';Pin $reserveProof '21EF3E9F03E594F562596DA7008A97A282CFE49D8CEC6C1D8DBFBDA2E73B112C'
    $reserveWorker=Get-Content $reserveProof -Raw|ConvertFrom-Json
    if(!$reserveWorker.passed -or !$reserveWorker.inputsUnchanged -or $reserveWorker.ownerAttention -or $reserveWorker.products.Count -ne 6){throw 'Matching worker reserve proof required'}
    $stageBytes=[long](($reserveWorker.products|Measure-Object bytes -Sum).Sum)+256KB
    if((Get-PSDrive C).Free -lt (600MB+$stageBytes)){throw 'Reserve plus all new worker/harness bytes required'}
'@
$pairs=@(
    @('global-install-feature-trusted-v1','private-binding-feature-unused-v1',1),
    @('global-install-feature-isolated-v1','private-binding-feature-isolated-v1',1),
    @('global-install-build-20261102-v1','private-binding-build-20261102-v1',1),
    @('FA61813EBDA435DFF4DC1138E450FE2C385AC3156534B507DB9216AA9878B21F','D32732C0BF3B548DB649966100D4B074663E07544BC9C25A648A80D6723AB02C',1),
    @('F5BDB1FF4CAD596781E91D4FDAEED421E8B22C199EF97852BD96481D778E3B7B','406C5634638D75C66892BD95429760CB4A5C74C94277913D1DF467147E30E75F',1),
    @($old,($shared+"`n"),1),
    @('global-install-worker-build-20261102-v1','private-binding-worker-build-20261102-v1',2),
    @('21EF3E9F03E594F562596DA7008A97A282CFE49D8CEC6C1D8DBFBDA2E73B112C','302C3003416F9095BA56C214FF6FD8B4F05A82F102CBAAF53B23DC3FAEE6E398',2),
    @('Lea.IsConnected.Tests.exe','Lea.PrivateBindings.Connectivity.exe',1)
)
$text=$original
foreach($pair in $pairs){if($pair.Count -ne 3 -or [regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne $pair[2]){throw 'Unique isolated feature boundary required'};$text=$text.Replace($pair[0],$pair[1])}
$inverse=$text;for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
if($inverse -cne $original){throw 'Isolated feature inverse differs'}
foreach($critical in @('foreach($file in $worker.products)','Lea.Tests/IsConnectedMain.cs','Lea.Tests/IsConnectedCases.js','-Seconds 30','-Seconds 120')){if([regex]::Matches($text,[regex]::Escape($critical)).Count -ne [regex]::Matches($original,[regex]::Escape($critical)).Count){throw 'Feature safety operation changed'}}
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Isolated feature syntax invalid'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
