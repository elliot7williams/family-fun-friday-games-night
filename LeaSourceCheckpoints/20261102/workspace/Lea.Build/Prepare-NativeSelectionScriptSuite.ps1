param([ValidateSet(1,2)][int]$Version=2)
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace ('continuity-evidence/native-selection-script-preparation-v'+$Version)
if(Test-Path $out){throw 'Preserve script preparation'}
$lf=[string][char]10
function Read([string]$Name){[IO.File]::ReadAllText((Join-Path $PSScriptRoot $Name)).Replace([string][char]13,'')}
function Line([string]$Text,[string]$Prefix){
    $matches=[regex]::Matches($Text,('(?m)^'+[regex]::Escape($Prefix)+'.*$'))
    if($matches.Count -ne 1){throw ('Unique line required: '+$Prefix)}
    $matches[0].Value
}
function Apply([string]$Original,[object[]]$Edits){
    $text=$Original
    foreach($pair in $Edits){
        if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw ('Projection anchor differs: '+$pair[0])}
        $text=$text.Replace($pair[0],$pair[1])
    }
    $inverse=$text
    for($index=$Edits.Count-1;$index -ge 0;$index--){$inverse=$inverse.Replace($Edits[$index][1],$Edits[$index][0])}
    if($inverse -cne $Original){throw 'Projection inverse differs'}
    return $text
}
$names=@('Initialize-NativeSelectionScriptSuite.generated.ps1','Build-NativeSelectionScriptSuite.generated.ps1','Verify-NativeSelectionScriptSuite.generated.ps1','Test-OwnedNativeSelectionScriptSuite.generated.ps1')
foreach($name in $names){
    $path=Join-Path $PSScriptRoot $name
    if(Test-Path $path){
        if($Version -ne 2){throw 'Preserve generated source'}
        $prior=Get-Content (Join-Path $workspace 'continuity-evidence/native-selection-script-preparation-v1/result.json') -Raw|ConvertFrom-Json
        $pins=@($prior.generated|Where-Object Path -ceq $path|ForEach-Object Hash)
        if($pins.Count -ne 1 -or (Get-FileHash $path).Hash -cne $pins[0]){throw 'Generated source was changed; preserve it'}
    }
}
$initialize=Read 'Initialize-CurrentNativeScriptSuite.generated.ps1'
if((Get-FileHash (Join-Path $PSScriptRoot 'Initialize-CurrentNativeScriptSuite.generated.ps1')).Hash -cne '53DC071BE123CBD7386E05DAE9FE55635934F1121740DE62910DF8182F63F1D4'){throw 'Original relocation helper differs'}
$oldView="        'Lea.Win.dll'=@('continuity-evidence/current-hit-geometry-v3/Lea.Win.dll','74956DA3FBBF28BC1DF281691FE7F8215C74CA67608AA61F6A6540E3BB0714AC')"
$newView="        'Lea.Win.dll'=@('continuity-evidence/native-selection-view-v1/Lea.Win.dll','2BEB3321AB5C70B18303E092451DB4D6B3299A56C232FF9374644C20CE2402CC')"
$init=Apply $initialize (,@($oldView,$newView))
$null=New-Item -ItemType Directory -Path $out
[IO.File]::WriteAllText((Join-Path $PSScriptRoot $names[0]),$init,[Text.UTF8Encoding]::new($false))
$initHash=(Get-FileHash (Join-Path $PSScriptRoot $names[0])).Hash
$build=Read 'Build-CurrentNativeScriptSuite.generated.ps1'
$setup=@'
function Run([string]$name,[string]$exe,[string[]]$arguments,[int]$seconds){
    Invoke-OwnedTestRun -Name $name -Application $exe -Arguments $arguments -WorkingDirectory $workspace -OutputDirectory $outRoot -Seconds $seconds -Records $runs
}
'@
$proof=@'
    $nativeProofPath=Join-Path $workspace 'continuity-evidence/native-selection-full-v1/outcome-observation.json'
    Hold $nativeProofPath '47761CBADBADA54A4AEDB45E50A5E784F702380006ECED300BC7540E59BEFCBD'
    $nativeProof=Get-Content $nativeProofPath -Raw|ConvertFrom-Json
    if(!$nativeProof.nativeBehaviorPassed -or !$nativeProof.exactTerminalFooterVerified -or !$nativeProof.originalExitAndOwnedJobZero -or $nativeProof.productionPolicyBroadened -or $nativeProof.viewSha256 -cne '2BEB3321AB5C70B18303E092451DB4D6B3299A56C232FF9374644C20CE2402CC'){throw 'Sx native qualification incomplete'}
    Hold (Join-Path $PSScriptRoot 'OwnedTestProcess.cs') 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA'
    Hold (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1') 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00'
    Add-Type -Path (Join-Path $PSScriptRoot 'OwnedTestProcess.cs')
    . (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1')
'@
$owned='if($OwnedRunCandidate){$result.ownedRunRequested=$true}'
$b=Apply $build @(
    @((Line $build '$outRoot='),'$outRoot=Join-Path $root ''current-native-script-suite-v9'''),
    @('$ErrorActionPreference=''Stop''',('$ErrorActionPreference=''Stop'''+$lf+'if(!$OwnedRunCandidate){throw ''Sx suite requires the owned timed candidate flags''}')),
    @("@('Space','NewOutputReader','PumpOutput','Run')","@('Space')"),
    @('$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()',('$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()'+$lf+$setup)),
    @($owned,($owned+$lf+'$result.nativeSelectionView=$true;$result.productionChanged=$true;$result.scope=''Matched60-source Sx view suite with relocated fixtures and owned compiler processes; runtime verification separate''')),
    @('    Hold $PSCommandPath (Get-FileHash -LiteralPath $PSCommandPath).Hash',('    Hold $PSCommandPath (Get-FileHash -LiteralPath $PSCommandPath).Hash'+$lf+$proof)),
    @('Lea.Build/Initialize-CurrentNativeScriptSuite.generated.ps1','Lea.Build/Initialize-NativeSelectionScriptSuite.generated.ps1'),
    @('53DC071BE123CBD7386E05DAE9FE55635934F1121740DE62910DF8182F63F1D4',$initHash)
)
$verify=Read 'Verify-CurrentNativeScriptSuite.generated.ps1'
$v=Apply $verify @(
    @((Line $verify 'param('),('param([Parameter(Mandatory)][ValidatePattern(''^[A-F0-9]{64}$'')][string]$ExpectedBuildReceipt)'+$lf+'$SuiteVersion=9')),
    @('$SuiteVersion -eq 8','$SuiteVersion -ge 8'),
    @('74956DA3FBBF28BC1DF281691FE7F8215C74CA67608AA61F6A6540E3BB0714AC','2BEB3321AB5C70B18303E092451DB4D6B3299A56C232FF9374644C20CE2402CC'),
    @('if(Test-Path $out){throw ''Preserve binding verification''}',('if(!$b.nativeSelectionView){throw ''Sx view build flag missing''}'+$lf+'if(Test-Path $out){throw ''Preserve binding verification''}'))
)
$run=Read 'Test-OwnedFullScriptSuite.ps1'
$r=Apply $run @(
    @("'current-native-script-owned-run-v1'","'native-selection-script-owned-run-v1'"),
    @("'current-native-script-suite-v8'","'current-native-script-suite-v9'"),
    @('productionChanged=$false','productionChanged=$true'),
    @('Exact60-source timed full script suite under owned Run; original300s/5000ms budgets and production components unchanged','Exact60-source timed full script suite with Sx view2BEB under owned Run; original300s/5000ms budgets, other production components unchanged'),
    @('    if($binding.buildReceiptSha256 -cne $ExpectedBuildReceipt){throw ''Binding does not identify this build''}',('    if($binding.buildReceiptSha256 -cne $ExpectedBuildReceipt){throw ''Binding does not identify this build''}'+$lf+'    if(!$build.nativeSelectionView -or (Get-FileHash (Join-Path $build.runtime ''Lea.Win.dll'')).Hash -cne ''2BEB3321AB5C70B18303E092451DB4D6B3299A56C232FF9374644C20CE2402CC''){throw ''Wrong Sx candidate''}'))
)
$texts=@($init,$b,$v,$r)
for($index=0;$index -lt $names.Count;$index++){
    $tokens=$null;$errors=$null
    $null=[Management.Automation.Language.Parser]::ParseInput($texts[$index],[ref]$tokens,[ref]$errors)
    if($errors.Count){throw 'Generated PowerShell parse errors'}
    if($index){[IO.File]::WriteAllText((Join-Path $PSScriptRoot $names[$index]),$texts[$index],[Text.UTF8Encoding]::new($false))}
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot $names[$index]) -Destination (Join-Path $out $names[$index])
}
$inputs=@('Initialize-CurrentNativeScriptSuite.generated.ps1','Build-CurrentNativeScriptSuite.generated.ps1','Verify-CurrentNativeScriptSuite.generated.ps1','Test-OwnedFullScriptSuite.ps1','Prepare-NativeSelectionScriptSuite.ps1')|ForEach-Object{Get-FileHash (Join-Path $PSScriptRoot $_)}|Select-Object Path,Hash
@{passed=$true;inverseVerified=$true;generatedSyntaxVerified=$true;inputs=@($inputs);generated=@($names|ForEach-Object{Get-FileHash (Join-Path $PSScriptRoot $_)}|Select-Object Path,Hash);suiteExecuted=$false;scope='Fresh relocated suite9 with only Sx view substitution and owned compiler adapter; original contracts, isolation and deadlines retained'}|ConvertTo-Json -Depth 6|Set-Content (Join-Path $out 'result.json')
@{passed=$true;generated=$names.Count}|ConvertTo-Json -Compress
