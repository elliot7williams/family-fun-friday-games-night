$ErrorActionPreference='Stop'
$source=Join-Path $PSScriptRoot 'Build-NumericReferenceCandidate.ps1'
$target=Join-Path $PSScriptRoot 'Build-NamedReferenceCandidate.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve named candidate builder'}
$text=[IO.File]::ReadAllText($source)
$text=$text.Replace('numeric-references-v3','named-references-v1').Replace('NumericReferences','NamedReferences')
$text=$text.Replace('Hc.D(value);','Hc.D(value, true);')
$text=$text.Replace('3D90FEECFAD3B62C92C2F28CD37E3B68DEDD68314EA8B314076A90EB81082627',(Get-FileHash -LiteralPath (Join-Path $PSScriptRoot '../Lea.Engine/Hc.cs')).Hash)
$anchor="    `$wrapper=Join-Path `$PSScriptRoot 'Invoke-PublicCompiler.20261022.ps1';"
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Named helper insertion differs'}
$helpers=@'
    foreach($name in @('Hn.cs','Hn.Data.cs')){
        $helper=Join-Path $workspace ('Lea.Engine/'+$name);$null=Hold $helper
        $copy=Join-Path $outRoot ('source/'+$name);Copy-Item -LiteralPath $helper -Destination $copy;$null=Hold $copy;$options.Add('"'+$copy+'"')
    }
    foreach($name in @('entities.whatwg.json','entities.expected.tsv')){
        $data=Join-Path $workspace ('Lea.Engine/Data/'+$name);$null=Hold $data
        $copy=Join-Path $outRoot $name;Copy-Item -LiteralPath $data -Destination $copy;$null=Hold $copy
    }
'@
$text=$text.Replace($anchor,$helpers+"`r`n"+$anchor)
$text=$text.Replace("`$old=Join-Path `$base 'Lea.Engine/Lea.Engine.dll'", "`$old=Join-Path `$workspace 'continuity-evidence/numeric-references-v3/candidate/Lea.Engine.dll'")
$text=$text.Replace('6D89A8F5953F2E5A197C5D789D8D828BF8FC084836EB30F22EB00D228B0591F8','515121C456FEE03257695BB34BA7302CF11AF0FF868186A14FBB5E9CADF9DFC1')
$text=$text.Replace("@(('`"'+`$fixtures+'`"')) 30", "@(('`"'+`$fixtures+'`"'),('`"'+(Join-Path `$outRoot 'entities.expected.tsv')+'`"')) 60")
$text=$text.Replace("([IO.File]::ReadAllText(`$b.stdout).Trim().EndsWith('Numeric references: checks=141; failures=0',[StringComparison]::Ordinal))", "([IO.File]::ReadAllText(`$b.stdout).Trim().EndsWith('Named references: checks=5119; failures=0',[StringComparison]::Ordinal))")
$anchor='    $result.passed=$result.baselineRegressionReproduced -and $result.candidateChecksPassed'
$extra=@'
    $numeric=Join-Path $workspace 'Lea.Tests/NumericReferences.cs';$null=Hold $numeric
    $numericCopy=Join-Path $outRoot 'NumericReferences.cs';Copy-Item -LiteralPath $numeric -Destination $numericCopy;$null=Hold $numericCopy
    $numericProbe=Join-Path $result.candidate 'NumericReferences.exe'
    Compile 'compile-numeric-regression' (@('/nologo','/nostdlib+','/target:exe','/langversion:12.0','/optimize+',('/out:"'+$numericProbe+'"'))+@($refs)+@('/reference:"'+$old+'"',('"'+$numericCopy+'"')))
    $numericRun=Run 'numeric-regression' $numericProbe @(('"'+$fixtures+'"')) 30
    $result.numericRegressionPassed=$numericRun.passed -and ([IO.File]::ReadAllText($numericRun.stdout).Trim().EndsWith('Numeric references: checks=141; failures=0',[StringComparison]::Ordinal))
'@
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Numeric regression anchor differs'}
$text=$text.Replace($anchor,$extra+"`r`n"+$anchor+' -and $result.numericRegressionPassed')
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
