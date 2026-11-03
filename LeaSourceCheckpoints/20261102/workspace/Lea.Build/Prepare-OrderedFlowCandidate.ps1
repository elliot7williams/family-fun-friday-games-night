$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Build-OrderedFlowCandidate.v4.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve ordered candidate'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Build-NamedReferenceCandidate.generated.ps1'))
$text=$text.Replace('named-references-v1','ordered-flow-v4').Replace('NamedReferences','OrderedFlow')
$anchor='    $edits=@('
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Ordered helper anchor differs'}
$setup=@'
    $flowHelper=Join-Path $PSScriptRoot 'New-OrderedFlowSource.ps1';$null=Hold $flowHelper
    . $flowHelper
    $flowRoot=Join-Path $workspace 'Lea.Engine/E.cs';$null=Hold $flowRoot
    $flowText=[IO.File]::ReadAllText($flowRoot)
'@
$text=$text.Replace($anchor,$setup+"`r`n"+$anchor)
$anchor='                [IO.File]::WriteAllText($to,$updated,[Text.UTF8Encoding]::new($false));$result.overlayInverseVerified=$true'
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Ordered E overlay anchor differs'}
$text=$text.Replace($anchor,'                $updated=New-OrderedFlowSource $updated $flowText'+"`r`n"+$anchor)
$anchor='            }else{Copy-Item -LiteralPath $from -Destination $to}'
$replace=@'
            }elseif([IO.Path]::GetFileName($from) -ceq 'B.cs'){
                $original=[IO.File]::ReadAllText($from)
                $old='        public bool Hd { get; internal set; }'
                $new=$old+"`r`n        internal bool An { get; set; }"
                if([regex]::Matches($original,[regex]::Escape($old)).Count -ne 1){throw 'Anonymous block anchor differs'}
                $updated=$original.Replace($old,$new)
                if($updated.Replace($new,$old) -cne $original){throw 'Anonymous block inverse differs'}
                [IO.File]::WriteAllText($to,$updated,[Text.UTF8Encoding]::new($false))
            }else{Copy-Item -LiteralPath $from -Destination $to}
'@
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Ordered B source anchor differs'}
$text=$text.Replace($anchor,$replace)
$text=$text.Replace('continuity-evidence/numeric-references-v3/candidate/Lea.Engine.dll','continuity-evidence/named-references-v1/candidate/Lea.Engine.dll').Replace('515121C456FEE03257695BB34BA7302CF11AF0FF868186A14FBB5E9CADF9DFC1','9208520EE6A067FF38D39368B2E50E6B099E5120C977A071E8D3E09733A3AF17')
$text=$text.Replace('Named references: checks=5119; failures=0','Ordered flow: checks=40; failures=0')
$anchor='    $result.passed=$result.baselineRegressionReproduced -and $result.candidateChecksPassed -and $result.numericRegressionPassed'
$extra=@'
    $named=Join-Path $workspace 'Lea.Tests/NamedReferences.cs';$null=Hold $named
    $namedCopy=Join-Path $outRoot 'NamedReferences.cs';Copy-Item -LiteralPath $named -Destination $namedCopy;$null=Hold $namedCopy
    $namedProbe=Join-Path $result.candidate 'NamedReferences.exe'
    Compile 'compile-named-regression' (@('/nologo','/nostdlib+','/target:exe','/langversion:12.0','/optimize+',('/out:"'+$namedProbe+'"'))+@($refs)+@('/reference:"'+$old+'"',('"'+$namedCopy+'"')))
    $namedRun=Run 'named-regression' $namedProbe @(('"'+$fixtures+'"'),('"'+(Join-Path $outRoot 'entities.expected.tsv')+'"')) 60
    $result.namedRegressionPassed=$namedRun.passed -and ([IO.File]::ReadAllText($namedRun.stdout).Trim().EndsWith('Named references: checks=5119; failures=0',[StringComparison]::Ordinal))
'@
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Ordered regression result anchor differs'}
$text=$text.Replace($anchor,$extra+"`r`n"+$anchor+' -and $result.namedRegressionPassed')
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
