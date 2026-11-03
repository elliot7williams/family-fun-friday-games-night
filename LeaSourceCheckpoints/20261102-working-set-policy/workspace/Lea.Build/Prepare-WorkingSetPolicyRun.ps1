$ErrorActionPreference='Stop'
$from=Join-Path $PSScriptRoot 'Test-TrustedBootstrapProfile-v4.generated.ps1'
$to=Join-Path $PSScriptRoot 'Test-WorkingSetPolicy-v1.generated.ps1'
if(Test-Path $to){throw 'Preserve working-set observation'}
if((Get-FileHash $from).Hash -cne '0FB476532A795EBAF192F0197A92C4B3C6D7BB0BA773C9A328F85C20F39BF68E'){throw 'Resource profile template changed'}
$original=[IO.File]::ReadAllText($from)
$begin=$original.IndexOf("    `$runtime=Join-Path `$out 'runtime';")
$end=$original.IndexOf('    if($Isolated){',$begin)
if($begin -lt 0 -or $end -le $begin){throw 'Explicit staged closure boundary required'}
$old=$original.Substring($begin,$end-$begin)
$shared=@'
    $runtime=Join-Path $root 'continuity-evidence/trusted-bootstrap-profile-v4/runtime'
    $previousPath=Join-Path $root 'continuity-evidence/trusted-bootstrap-profile-v4/result.json'
    Pin $previousPath '2B084ABE6DD48B6C3B7D5AE7CA5EA4EEAFAD5C09CAAADD888E837D04854A59FA'
    $previous=Get-Content $previousPath -Raw|ConvertFrom-Json
    if(!$previous.passed -or !$previous.inputsUnchanged -or $previous.ownerAttention -or $previous.changedFiles.Count -or $previous.before.Count -ne 10){throw 'Prior shared runtime not verified'}
    if(@(Get-ChildItem $runtime -Recurse -File).Count -ne 10){throw 'Prior shared runtime inventory changed'}
    foreach($file in $previous.before){Pin $file.path $file.sha256;if((Get-Acl $file.path).Sddl -cne $file.sddl){throw 'Shared runtime ACL changed'}}
    if((Get-PSDrive C).Free -lt (600MB+256KB)){throw 'Reserve plus bounded harness staging required'}
'@
$pairs=@(
    @('continuity-evidence/trusted-bootstrap-profile-v4','continuity-evidence/trusted-working-set-policy-v1'),
    @('Lea.Tests/BootstrapComparisonMain.cs','Lea.Tests/WorkingSetPolicyMain.cs'),
    @('Lea.IsConnected.Tests.exe','Lea.WorkingSetPolicy.Tests.exe'),
    @($old,($shared+"`n"))
)
$text=$original
foreach($pair in $pairs){if($pair.Count -ne 2 -or [regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Unique policy observation boundary required'};$text=$text.Replace($pair[0],$pair[1])}
$inverse=$text;for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
if($inverse -cne $original){throw 'Policy observation inverse differs'}
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Policy observation syntax invalid'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
