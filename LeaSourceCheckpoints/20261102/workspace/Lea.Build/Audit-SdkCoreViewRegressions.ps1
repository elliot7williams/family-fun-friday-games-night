param([switch]$AggregateSdkPair,[switch]$PrintSdk)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'continuity-evidence/sdk-core-view-regression-20261031-v1'
if(!$AggregateSdkPair -and (Test-Path $out)){throw 'Preserve SDK regression audit'}
$core='772A51ADD8C8A9CE80964BEDB043D912B0DFB95422E3DC84FAC6F49B93366270'
$view='80085FA1CD380CDF2E7AB0F8BBE5AC8982A11E95F82C43F776EE16ADC1A0D0C0'
$cases=@(
    @('sdk-font-shorthand-v1','2C1929B1C62E1C5BBBC28410E1E19F0A320A902026C457C8955BA971BF347300','FONT_SHORTHAND controls=12 cases=14 failures=0'),
    @('sdk-font-initial-v1','947B3804D49395A2D843485BFAE4BE80F140CCC8117D489755EB229220DAABCD','FONT_INITIAL controls=5 cases=7 failures=0'),
    @('sdk-font-relative-v1','E2E69E21EB43C298F78C2F5E5201DE1895279FC8FB94B8FCC91A030E2211A969','FONT_RELATIVE controls=6 cases=12 failures=0'),
    @('sdk-font-initial-render-v1','FAECC04163FD26A124A33CCF4EE120C66E404ED030294511FA026341435C596F','FONT_RENDER PASS 142'),
    @('sdk-font-relative-render-v1','5AB2DB74D7DB67E37B728B21649913E8CC9F16359057790BEBC88A28C41F6704','FONT_RENDER PASS 142')
)
if($AggregateSdkPair -and !$PrintSdk){
    $out=Join-Path $root 'continuity-evidence/sdk-aggregate-core-view-regression-20261031-v1'
    if(Test-Path $out){throw 'Preserve aggregate regression audit'}
    $core='DC71E2B9DA704135CA022E5BBC2F1C57BA8046621C41E40AB98C7DC354BD6213'
    $view='6F907862F57C7640A8B0E7BE2C22DFEDD18B3966D472958FCFCEDA53A00F0F52'
    $cases=@(
        @('sdk-aggregate-font-shorthand-v1','209E800D102E2501AFA3265384B7A73067300DC4ED78659AE360CDB1C40E9D7B','FONT_SHORTHAND controls=12 cases=14 failures=0'),
        @('sdk-aggregate-font-initial-v1','86F651490A5209D2FA28BF1A472D4A03D943567C18CD600D376B3BB509084259','FONT_INITIAL controls=5 cases=7 failures=0'),
        @('sdk-aggregate-font-relative-v1','B78D1CA57E4005002B5475FB047D5C1E71558FA3D2D9AA898BBDA5A2BD3ABD5F','FONT_RELATIVE controls=6 cases=12 failures=0'),
        @('sdk-aggregate-font-initial-render-v1','BEF9D2AC5C23DE7539CE7F66F4FAFF2868514C027DB67B05938C09D0D7D50B77','FONT_RENDER PASS 142'),
        @('sdk-aggregate-font-relative-render-v1','AF9CD0CBC6441FE88788C775D5C5C0DACD94D157B2391524A6E0FC44134F4514','FONT_RENDER PASS 142')
    )
}
if($PrintSdk){
    if(!$AggregateSdkPair){throw 'Print SDK audit requires aggregate mode'}
    $out=Join-Path $root 'continuity-evidence/sdk-print-aggregate-core-view-regression-v1'
    if(Test-Path $out){throw 'Preserve print SDK audit'}
    $core='5B8A501DA627467A6C261A355917DE5F73DF8F00758B49C1D240CF7D596E8A4A'
    $view='A622A1C266AD7CA27F437832E390E594F7E419CAFED700C9F466F0517970CE0D'
    $cases=@(
        @('sdk-print-aggregate-font-shorthand-v1','B7B6D65BAB9214D6E81D9A33DC6151010DC8B2FD295649DAAD5F5C0133CE97DF','FONT_SHORTHAND controls=12 cases=14 failures=0'),
        @('sdk-print-aggregate-font-initial-v1','C991BBD1EE868413FE7939B4D23F8905C0B668D294AB8B6D42E44A8B50834AC1','FONT_INITIAL controls=5 cases=7 failures=0'),
        @('sdk-print-aggregate-font-relative-v1','9926132FC1B5AE0A35DC11190C4C9D7C2F56C4DD9099EEEA02BFEB82169815D1','FONT_RELATIVE controls=6 cases=12 failures=0'),
        @('sdk-print-aggregate-font-initial-render-v1','0E7C975A0E68AFF8697E4F52BC0A39FB261585FD920B6DE9B8EA494A6F354FC2','FONT_RENDER PASS 142'),
        @('sdk-print-aggregate-font-relative-render-v1','4D486CDB1C3F9CA3E70E9963A43C94C0D5DEF9A8FFC6757C6783C40C25F16271','FONT_RENDER PASS 142')
    )
}
$rows=[Collections.Generic.List[object]]::new()
foreach($case in $cases){
    $path=Join-Path $root ('continuity-evidence/'+$case[0]+'/result.json')
    if((Get-FileHash $path).Hash -cne $case[1]){throw 'SDK regression receipt differs'}
    $r=Get-Content $path -Raw|ConvertFrom-Json
    if($PrintSdk -and !$r.printSdk){throw 'Print SDK identity mode missing'}
    if($AggregateSdkPair -and !$r.aggregateSdkPair){throw 'Aggregate identity mode missing'}
    if(!$r.passed -or !$r.sdkPair -or !$r.inputsUnchanged -or $r.ownerAttention -or $r.error -or $r.candidateCoreSha256 -cne $core -or $r.runs.Count -ne 2){throw 'SDK regression gate incomplete'}
    if($case[0].Contains('render') -and $r.candidateViewSha256 -cne $view){throw 'SDK rendered view differs'}
    foreach($run in $r.runs){
        if(!$run.passed -or $run.exitCode -ne 0 -or $run.timedOut -or $run.killRequested -or !$run.exitObserved -or $run.activeAfterRetirement -ne 0 -or !$run.outputDrained -or $run.retirementAndDrainElapsedMs -gt 5000 -or $run.elapsedMs -gt $run.parentBudgetMs){throw 'SDK execution/retirement gate failed'}
    }
    if($r.runs[0].parentBudgetMs -ne 120000 -or $r.runs[1].parentBudgetMs -ne 30000){throw 'SDK original budgets differ'}
    $lines=@(Get-Content $r.runs[1].stdout)
    if($lines[-1] -cne $case[2] -or @($lines|Where-Object {$_ -ceq $case[2]}).Count -ne 1){throw 'SDK exact terminal footer differs'}
    foreach($inputFile in $r.inputs){if((Get-FileHash -LiteralPath $inputFile.path).Hash -cne $inputFile.sha256){throw 'SDK input changed after execution'}}
    $rows.Add(@{suite=$case[0];receiptSha256=$case[1];elapsedMs=$r.runs[1].elapsedMs;footer=$case[2]})
}
$binaryRoot='I:\LeaBuildStorage\core-view-sdk-20261031-v3\bin\Release'
if($AggregateSdkPair){$binaryRoot=Join-Path $root 'continuity-evidence/sdk-portable-source-v1/bundle/Sample/bin/Release'}
if($PrintSdk){$binaryRoot=Join-Path $root 'continuity-evidence/sdk-print-aggregate-source-v1/bundle/Sample/bin/Release'}
foreach($pair in @(@('Lea.Engine.dll',$core),@('Lea.Win.dll',$view))){if((Get-FileHash (Join-Path $binaryRoot $pair[0])).Hash -cne $pair[1]){throw 'SDK original binary changed'}}
New-Item -ItemType Directory -Path $out | Out-Null
@{verified=$true;suites=$rows;coreSha256=$core;viewSha256=$view;completeSdk=$false;imageWorkerTested=$false;scriptWorkerTested=$false;releaseQualified=$false;runnerSha256=(Get-FileHash $PSCommandPath).Hash}|ConvertTo-Json -Depth 5|Set-Content -LiteralPath (Join-Path $out 'result.json') -Encoding utf8
Get-FileHash (Join-Path $out 'result.json')
