$ErrorActionPreference='Stop'
$oldHelper=Join-Path $PSScriptRoot 'Initialize-SuiteRelocation.ps1'
if((Get-FileHash -LiteralPath $oldHelper).Hash -cne '6FF8A77CADD98EA3DCA56EC406522620040E9984E3A2A15325646ED0DF52D661'){throw 'Retain original helper'}
$helper=Join-Path $PSScriptRoot 'Initialize-SuiteRelocationV2.generated.ps1'
$builder=Join-Path $PSScriptRoot 'Build-RelocatedSuitePhaseV2.generated.ps1'
if((Test-Path $helper) -or (Test-Path $builder)){throw 'Preserve V2 files'}
$source=[IO.File]::ReadAllText($oldHelper)
$updated=[regex]::Replace($source,'\$host\b','$entryHost').Replace('Initialize-SuiteRelocation {','Initialize-SuiteRelocationV2 {')
if($updated.Replace('$entryHost','$host').Replace('Initialize-SuiteRelocationV2 {','Initialize-SuiteRelocation {') -cne $source){throw 'Unexpected helper delta'}
[IO.File]::WriteAllText($helper,$updated,[Text.UTF8Encoding]::new($false))
$hash=(Get-FileHash -LiteralPath $helper).Hash
$oldBuilder=Join-Path $PSScriptRoot 'Build-RelocatedSuitePhase.generated.ps1'
if((Get-FileHash -LiteralPath $oldBuilder).Hash -cne '0805D449EDEDAFB571B3882E67655B59AEE07771E39AD1B88AEE3E07444A4022'){throw 'Retain original builder'}
$text=[IO.File]::ReadAllText($oldBuilder).Replace("'suite-phase-relocated-v1'","'suite-phase-relocated-v2'").Replace("'Lea.Build/Initialize-SuiteRelocation.ps1'","'Lea.Build/Initialize-SuiteRelocationV2.generated.ps1'").Replace('6FF8A77CADD98EA3DCA56EC406522620040E9984E3A2A15325646ED0DF52D661',$hash).Replace('=Initialize-SuiteRelocation $build','=Initialize-SuiteRelocationV2 $build')
foreach($s in @($updated,$text)){$t=$null;$e=$null;[void][Management.Automation.Language.Parser]::ParseInput($s,[ref]$t,[ref]$e);if($e.Count){throw 'Generated syntax invalid'}}
if([regex]::IsMatch($updated,'(?i)\$host\s*=')){throw 'Reserved Host assignment remains'}
[IO.File]::WriteAllText($builder,$text,[Text.UTF8Encoding]::new($false))
[pscustomobject]@{helperSha256=$hash;builderSha256=(Get-FileHash -LiteralPath $builder).Hash;helperInverseVerified=$true}|ConvertTo-Json
