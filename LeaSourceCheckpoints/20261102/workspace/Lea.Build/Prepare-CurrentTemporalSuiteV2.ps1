$ErrorActionPreference='Stop'
$oldHelper=Join-Path $PSScriptRoot 'Initialize-CurrentTemporalSuite.generated.ps1'
$oldBuilder=Join-Path $PSScriptRoot 'Build-CurrentTemporalSuite.generated.ps1'
if((Get-FileHash -LiteralPath $oldHelper).Hash -cne '1FBE4373961625AFF08C4A618A293A16DF8217FADD38554CC557347AE8692EE8' -or (Get-FileHash -LiteralPath $oldBuilder).Hash -cne '9006DB3F8C916456A5B501D01ACAC4A3FB85CFCDF215995677A31346EB3DB4BF'){throw 'Retain current V1 builders'}
$helper=Join-Path $PSScriptRoot 'Initialize-CurrentTemporalSuiteV2.generated.ps1'
$builder=Join-Path $PSScriptRoot 'Build-CurrentTemporalSuiteV2.generated.ps1'
if((Test-Path $helper) -or (Test-Path $builder)){throw 'Preserve V2 builders'}
$text=[IO.File]::ReadAllText($oldHelper).Replace('function Initialize-CurrentTemporalSuite {','function Initialize-CurrentTemporalSuiteV2 {')
$old='elseif($line.StartsWith(''"'') -and $line.EndsWith(''.cs"'')){Hold $line.Trim(''"'') ''BD06A62CEF747C8617ED7217507397B46C241472079403194BCEAC2EF0D47646'';$sources++}'
$new=@'
elseif($line.StartsWith('"') -and $line.EndsWith('.cs"')){
                $originalPath=$line.Trim('"');Hold $originalPath 'BD06A62CEF747C8617ED7217507397B46C241472079403194BCEAC2EF0D47646'
                $originalText=[IO.File]::ReadAllText($originalPath)
                $oldParent='7C46E97E6F836716687ED7F2822F3024F9E73A006A9CCD0A7366E7DD484432A1'
                $currentParent='E68504B515405FA2C75B92904277A7A8074945BDCC02E1CAD10A57EB9A6FC1E5'
                if([regex]::Matches($originalText,$oldParent).Count -ne 1){throw 'Expected one confinement parent digest'}
                $updated=$originalText.Replace($oldParent,$currentParent)
                if($updated.Replace($currentParent,$oldParent) -cne $originalText){throw 'Entry host changed beyond parent identity binding'}
                $hostSource=Join-Path $OutputRoot 'WorkerEntryNativeHost.cs'
                [IO.File]::WriteAllText($hostSource,$updated,[Text.UTF8Encoding]::new($false))
                Hold $hostSource (Get-FileHash -LiteralPath $hostSource).Hash
                $line='"'+$hostSource+'"';$sources++
            }
'@
if(!$text.Contains($old)){throw 'Host source anchor missing'}
$text=$text.Replace($old,$new)
[IO.File]::WriteAllText($helper,$text,[Text.UTF8Encoding]::new($false));$hash=(Get-FileHash -LiteralPath $helper).Hash
$text=[IO.File]::ReadAllText($oldBuilder).Replace("'current-lazy-suite-v1'","'current-lazy-suite-v2'").Replace('Initialize-CurrentTemporalSuite.generated.ps1','Initialize-CurrentTemporalSuiteV2.generated.ps1').Replace('1FBE4373961625AFF08C4A618A293A16DF8217FADD38554CC557347AE8692EE8',$hash).Replace('=Initialize-CurrentTemporalSuite $build','=Initialize-CurrentTemporalSuiteV2 $build')
foreach($s in @([IO.File]::ReadAllText($helper),$text)){$t=$null;$e=$null;[void][Management.Automation.Language.Parser]::ParseInput($s,[ref]$t,[ref]$e);if($e.Count){throw 'V2 syntax invalid'}}
[IO.File]::WriteAllText($builder,$text,[Text.UTF8Encoding]::new($false))
[pscustomobject]@{helperSha256=$hash;builderSha256=(Get-FileHash -LiteralPath $builder).Hash;hostSourceDelta='One confinement-parent hash; inverse checked';assertionsRemoved=0}|ConvertTo-Json
