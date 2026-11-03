$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/temporal-startup-v1'
$base=Join-Path $workspace 'continuity-evidence/permission-contract-v1/Test-StartupBoundaryV4.generated.ps1'
if((Get-FileHash -LiteralPath $base).Hash -cne '0FD0EE68A52BAB42C9E6302DF29A8D6EEA229160FEF39D38F4C7E33BA6CF5AB0'){throw 'Base runner changed'}
$receipt=Join-Path $root 'native-pair-v2/literal-input-verification.json'
if((Get-FileHash -LiteralPath $receipt).Hash -cne '98A773D0A4B40B2D116F8BAEA1B4CF1F20D5D9C6607C1FEB297EE4805C9428C8'){throw 'Native verification changed'}
$pair=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
if(!$pair.passed -or $pair.variants.Count -ne 2){throw 'Verified native pair required'}
$original=[IO.File]::ReadAllText($base)
$generation=Join-Path $root 'startup-runner-generation.json'
if(Test-Path -LiteralPath $generation){throw 'Preserve generation receipt'}
$rows=@(foreach($variant in $pair.variants){
    $name=$variant.name
    if($name -cnotin @('control','lazy')){throw 'Unknown variant'}
    $target=Join-Path (Split-Path $base) ('Test-TemporalStartup-'+$name+'.generated.ps1')
    if(Test-Path -LiteralPath $target){throw 'Preserve generated runner'}
    $text=$original.Replace("`r`n","`n")
    function ReplaceOnce([string]$old,[string]$new){
        if($script:text.IndexOf($old,[StringComparison]::Ordinal) -lt 0 -or $script:text.IndexOf($old,[StringComparison]::Ordinal) -ne $script:text.LastIndexOf($old,[StringComparison]::Ordinal)){throw ('Ambiguous substitution: '+$old)}
        $script:text=$script:text.Replace($old,$new)
    }
    ReplaceOnce "`$outRoot=Join-Path `$PSScriptRoot 'startup-boundary-run-v4'" ("`$outRoot=Join-Path `$workspace 'continuity-evidence/temporal-startup-v1/startup-"+$name+"-v1'")
    $injection=@'
    $proofPath=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/native-pair-v2/literal-input-verification.json'
    if((Get-FileHash -LiteralPath $proofPath).Hash -cne '98A773D0A4B40B2D116F8BAEA1B4CF1F20D5D9C6607C1FEB297EE4805C9428C8'){throw 'Native proof differs'}
    $leases.Add([IO.File]::Open($proofPath,'Open','Read','Read'))
    $inputs.Add(@{path=$proofPath;sha256=(Get-FileHash -LiteralPath $proofPath).Hash})
    $leases.Add([IO.File]::Open($PSCommandPath,'Open','Read','Read'))
    $inputs.Add(@{path=$PSCommandPath;sha256=(Get-FileHash -LiteralPath $PSCommandPath).Hash})
'@
    ReplaceOnce "`ntry{`n    `$priorPath=" ("`ntry{`n"+$injection+"`n    `$priorPath=")
    $old='        CopyPinned $from (Join-Path $runtime (''ScriptWorker/''+$name)) $hash'
    $new='        if($name -ceq ''Jint.dll''){$from='''+$variant.binary.Replace("'","''")+''';$hash='''+$variant.sha256+'''}'+"`r`n"+$old
    ReplaceOnce $old $new
    ReplaceOnce '    $result.before=@(' @'
    $leases.Add([IO.File]::Open($fixture,'Open','Read','Read'))
    if((Get-FileHash -LiteralPath $fixture).Hash -cne 'DFC37D26AE20C3A6BFA0BB5B091635C4F23E2C316637943AC093F9948035C234'){throw 'Fixture differs'}
    $inputs.Add(@{path=$fixture;sha256='DFC37D26AE20C3A6BFA0BB5B091635C4F23E2C316637943AC093F9948035C234'})
    $result.before=@(
'@
    # Mechanical fork: retain observer, security settings, deadlines and ownership paths.
    $text=$text.Replace('Get-FileHash $from','Get-FileHash -LiteralPath $from').Replace('Get-FileHash $to','Get-FileHash -LiteralPath $to').Replace('Get-FileHash $_.path','Get-FileHash -LiteralPath $_.path')
    $tokens=$null;$errors=$null;[void][Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
    if($errors.Count){throw 'Generated runner syntax invalid'}
    [IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
    @{variant=$name;runner=$target;sha256=(Get-FileHash -LiteralPath $target).Hash;workerJint=$variant.binary;workerJintSha256=$variant.sha256}
})
[IO.File]::WriteAllText($generation,(@{base=$base;baseSha256=(Get-FileHash -LiteralPath $base).Hash;variants=$rows;productionChanged=$false}|ConvertTo-Json -Depth 5),[Text.UTF8Encoding]::new($false))
$rows|ConvertTo-Json -Depth 4
