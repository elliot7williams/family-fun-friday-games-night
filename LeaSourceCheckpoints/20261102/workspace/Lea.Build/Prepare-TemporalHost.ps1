$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/temporal-startup-v1'
$base=Join-Path $workspace 'continuity-evidence/permission-contract-v1/Test-FirespiderEarlyHost.generated.ps1'
if((Get-FileHash -LiteralPath $base).Hash -cne '01404AB896532F877B8400FC1FEE1539E1850A80D997CE830D6F20762CE1BE78'){throw 'Host runner changed'}
$proofPath=Join-Path $root 'native-pair-v2/literal-input-verification.json'
if((Get-FileHash -LiteralPath $proofPath).Hash -cne '98A773D0A4B40B2D116F8BAEA1B4CF1F20D5D9C6607C1FEB297EE4805C9428C8'){throw 'Native proof changed'}
$proof=Get-Content -LiteralPath $proofPath -Raw|ConvertFrom-Json
if(!$proof.passed -or $proof.variants.Count -ne 2){throw 'Native pair incomplete'}
$generation=Join-Path $root 'host-runner-generation.json'
if(Test-Path -LiteralPath $generation){throw 'Preserve generation'}
$rows=@(foreach($variant in $proof.variants){
    if($variant.name -cnotin @('control','lazy')){throw 'Unexpected variant'}
    $target=Join-Path (Split-Path $base) ('Test-TemporalHost-'+$variant.name+'.generated.ps1')
    if(Test-Path -LiteralPath $target){throw 'Preserve runner'}
    $text=[IO.File]::ReadAllText($base).Replace("`r`n","`n")
    function ReplaceOnce([string]$old,[string]$new){
        if($script:text.IndexOf($old,[StringComparison]::Ordinal) -lt 0 -or $script:text.IndexOf($old,[StringComparison]::Ordinal) -ne $script:text.LastIndexOf($old,[StringComparison]::Ordinal)){throw ('Ambiguous substitution: '+$old)}
        $script:text=$script:text.Replace($old,$new)
    }
    ReplaceOnce "`$outRoot=`$workspace+'\continuity-evidence\permission-contract-v1\firespider-early-host-v1'" ("`$outRoot=`$workspace+'\continuity-evidence\temporal-startup-v1\host-"+$variant.name+"-v1'")
    ReplaceOnce '    Space;$null=Pin $PSCommandPath' @'
    Space;$null=Pin $PSCommandPath
    $proofPath=$workspace+'\continuity-evidence\temporal-startup-v1\native-pair-v2\literal-input-verification.json'
    $null=Pin $proofPath '98A773D0A4B40B2D116F8BAEA1B4CF1F20D5D9C6607C1FEB297EE4805C9428C8'
'@
    $old='        CopyExact (Join-Path $stage.variants[0].directory $file.name) (Join-Path $result.candidate (''ScriptWorker/''+$file.name)) $file.sha256'
    $new=@'
        $from=Join-Path $stage.variants[0].directory $file.name;$hash=$file.sha256
        if($file.name -ceq 'Lea.Script.Worker.dll'){$from=$workspace+'\continuity-evidence\bootstrap-descriptor-batches-v1\timed-worker-native-v2\Lea.Script.Worker.dll';$hash='1C338F548D9F70B16F640A28BA7943F05DB0278260711674A62C5A2A8B36F436'}
'@
    $new+="`n        if(`$file.name -ceq 'Jint.dll'){`$from='"+$variant.binary.Replace("'","''")+"';`$hash='"+$variant.sha256+"'}`n"
    $new+='        CopyExact $from (Join-Path $result.candidate (''ScriptWorker/''+$file.name)) $hash'
    ReplaceOnce $old $new
    # Keep all15 stages and original process deadlines/security untouched.
    $tokens=$null;$errors=$null;[void][Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
    if($errors.Count){throw 'Generated syntax invalid'}
    [IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
    @{variant=$variant.name;runner=$target;sha256=(Get-FileHash -LiteralPath $target).Hash;workerJintSha256=$variant.sha256}
})
[IO.File]::WriteAllText($generation,(@{baseSha256=(Get-FileHash -LiteralPath $base).Hash;variants=$rows;productionChanged=$false}|ConvertTo-Json -Depth 5),[Text.UTF8Encoding]::new($false))
$rows|ConvertTo-Json -Depth 4
