$ErrorActionPreference='Stop'
$from=Join-Path $PSScriptRoot 'Test-IsConnectedTrusted.ps1'
if((Get-FileHash $from).Hash -cne 'F48FB93BD317970D30CB3861AFE183E60A26A8F6D87893B6EFB9BFC513C449FF'){throw 'Feature runtime template differs'}
$to=Join-Path $PSScriptRoot 'Test-GlobalInstallFeature-v1.generated.ps1'
if(Test-Path $to){throw 'Preserve matched feature runner'}
$original=[IO.File]::ReadAllText($from)
$pairs=@(
    @('[int]$Version=2','[int]$Version=3'),
    @('"continuity-evidence/is-connected-trusted-v$Version"',"'continuity-evidence/global-install-feature-trusted-v1'"),
    @('continuity-evidence/is-connected-isolated-v1','continuity-evidence/global-install-feature-isolated-v1'),
    @('is-connected-script-build-20261102-v1','global-install-build-20261102-v1'),
    @('2596ED5B2B5E5210324DBE403C1526E029E4F99240080C4434DCD028445D46F9','FA61813EBDA435DFF4DC1138E450FE2C385AC3156534B507DB9216AA9878B21F'),
    @('FBD50D216D724A4BFCA0488C53CB1B648BC659AAE19F800AE7E129B207EBBD4C','F5BDB1FF4CAD596781E91D4FDAEED421E8B22C199EF97852BD96481D778E3B7B'),
    @('is-connected-worker-build-20261102-v1','global-install-worker-build-20261102-v1'),
    @('DB8F6F426B4773E821A7930B46BA74B59E46802B540BFFB89DD775B5550AFCB6','21EF3E9F03E594F562596DA7008A97A282CFE49D8CEC6C1D8DBFBDA2E73B112C')
)
$text=$original
foreach($pair in $pairs){if($pair.Count -ne 2 -or [regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Unique feature projection required'};$text=$text.Replace($pair[0],$pair[1])}
$inverse=$text;for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
if($inverse -cne $original){throw 'Feature runner inverse differs'}
$anchor="    if(`$dlls.Count -ne 8){throw 'Eight-file trusted closure required'}"
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Feature reserve anchor differs'}
$reserve=@'
    $stageBytes=[long](($dlls|Measure-Object Length -Sum).Sum)+256KB
    if($Isolated){
        $reserveProof='I:/LeaBuildStorage/global-install-worker-build-20261102-v1/result.json';Pin $reserveProof '21EF3E9F03E594F562596DA7008A97A282CFE49D8CEC6C1D8DBFBDA2E73B112C'
        $reserveWorker=Get-Content $reserveProof -Raw|ConvertFrom-Json
        if(!$reserveWorker.passed -or !$reserveWorker.inputsUnchanged -or $reserveWorker.ownerAttention -or $reserveWorker.products.Count -ne 6){throw 'Matching worker reserve proof required'}
        $stageBytes+=[long](($reserveWorker.products|Measure-Object bytes -Sum).Sum)
    }
    if((Get-PSDrive C).Free -lt (600MB+$stageBytes)){throw 'Feature reserve plus complete staged closure required'}
'@
$text=$text.Replace($anchor,$anchor+[Environment]::NewLine+$reserve)
foreach($critical in @('foreach($file in $dlls)','foreach($file in $worker.products)','Lea.Tests/IsConnectedMain.cs','Lea.Tests/IsConnectedCases.js','-Seconds 30','-Seconds 120')){
    if([regex]::Matches($original,[regex]::Escape($critical)).Count -ne [regex]::Matches($text,[regex]::Escape($critical)).Count){throw 'Critical feature operation lost'}
}
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Feature runner syntax invalid'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
