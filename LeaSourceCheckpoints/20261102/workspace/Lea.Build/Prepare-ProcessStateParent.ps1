$ErrorActionPreference='Stop'
$base=Join-Path $PSScriptRoot 'Build-ArchivedParentView.ps1'
if((Get-FileHash -LiteralPath $base).Hash -cne '5F48FB7E813C3F0F5C923BCE8C588CDA636D2F24C42F6BF2BB4ECE792EADD665'){throw 'Builder changed'}
$target=Join-Path $PSScriptRoot 'Build-ProcessStateParent.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve generated builder'}
$text=[IO.File]::ReadAllText($base).Replace("`r`n","`n")
$text=$text.Replace('archived-parent-view-build-v3','process-state-parent-build-v1')
$anchor='            $text=$text.Replace(''"''+$dependency.originalPath+''"'',''"''+$target+''"'')'
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Source substitution anchor differs'}
$insertion=@'
            if($module.module -ceq 'parent' -and $dependency.kind -ceq 'source'){
                $original=[IO.File]::ReadAllText($target)
                $updated=$original
                $name=[IO.Path]::GetFileName($target)
                if($name -ceq '13.cs'){
                    $rootRp=Join-Path $workspace 'Lea.Win/Rp.cs'
                    Hold $rootRp 'DF50F83C8BF1B0B2B91BFA24EDC05712691555E7035CEA61AB80BE14EA81B7CA'
                    $rootText=[IO.File]::ReadAllText($rootRp)
                    $start=$rootText.IndexOf('        public string Ps {',[StringComparison]::Ordinal)
                    $end=$rootText.IndexOf('        private Rp(', $start,[StringComparison]::Ordinal)
                    if($start -lt 0 -or $end -lt 0){throw 'Diagnostic implementation shape changed'}
                    $addition=$rootText.Substring($start,$end-$start)
                    $anchor='        public string E { get { lock (errorGate) return errorText.ToString(); } }'
                    if([regex]::Matches($original,[regex]::Escape($anchor)).Count -ne 1){throw 'Rp insertion anchor differs'}
                    $updated=$original.Replace($anchor,$anchor+"`r`n"+$addition)
                    if($updated.Replace($anchor+"`r`n"+$addition,$anchor) -cne $original){throw 'Rp inverse differs'}
                }elseif($name -ceq '6.cs'){
                    $old='startupSummary = "phase=" + phase + "; launch="'
                    $new='startupSummary = "phase=" + phase + "; child=" + (endpoint == null ? "unavailable" : endpoint.Ps) + "; launch="'
                    if([regex]::Matches($original,[regex]::Escape($old)).Count -ne 1){throw 'Summary anchor differs'}
                    $updated=$original.Replace($old,$new)
                    if($updated.Replace($new,$old) -cne $original){throw 'Summary inverse differs'}
                }
                if($updated -cne $original){
                    $target=Join-Path $directory $name
                    [IO.File]::WriteAllText($target,$updated,[Text.UTF8Encoding]::new($false))
                    Hold $target (Get-FileHash -LiteralPath $target).Hash
                }
            }
'@
$text=$text.Replace($anchor,$insertion+"`n"+$anchor)
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Generated builder syntax invalid'}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
