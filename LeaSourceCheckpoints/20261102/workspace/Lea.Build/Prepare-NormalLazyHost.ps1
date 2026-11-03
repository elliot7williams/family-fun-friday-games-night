$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$base=Join-Path $workspace 'continuity-evidence/permission-contract-v1/Test-FirespiderEarlyHost.generated.ps1'
if((Get-FileHash -LiteralPath $base).Hash -cne '01404AB896532F877B8400FC1FEE1539E1850A80D997CE830D6F20762CE1BE78'){throw 'Base changed'}
$target=Join-Path $PSScriptRoot 'Test-NormalLazyHost.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve generated runner'}
$text=[IO.File]::ReadAllText($base).Replace("`r`n","`n")
function ReplaceOnce([string]$Old,[string]$New){
    if($script:text.IndexOf($Old,[StringComparison]::Ordinal) -lt 0 -or $script:text.IndexOf($Old,[StringComparison]::Ordinal) -ne $script:text.LastIndexOf($Old,[StringComparison]::Ordinal)){throw 'Ambiguous substitution'}
    $script:text=$script:text.Replace($Old,$New)
}
ReplaceOnce "`$outRoot=`$workspace+'\continuity-evidence\permission-contract-v1\firespider-early-host-v1'" "`$outRoot=`$workspace+'\continuity-evidence\temporal-startup-v1\host-normal-lazy-v1'"
ReplaceOnce '    Space;$null=Pin $PSCommandPath' @'
    Space;$null=Pin $PSCommandPath
    $policy=$workspace+'\Lea.Build\Test-DecoderAclDelta.ps1'
    $null=Pin $policy '15F46DBC7F4D4AD5B816DDFA1FE7213D02CAE1EDE79844AFBD904F281299E71E'
    . $policy
'@
ReplaceOnce '        CopyExact (Join-Path $stage.variants[0].directory $file.name) (Join-Path $result.candidate (''ScriptWorker/''+$file.name)) $file.sha256' @'
        $from=Join-Path $stage.variants[0].directory $file.name;$hash=$file.sha256
        if($file.name -ceq 'Lea.Script.Worker.dll' -and $hash -cne '60FF26D8357FEA52C802BAC562C30C6CE28F72F735E78281D0908E94FAB530D6'){throw 'Normal worker required'}
        if($file.name -ceq 'Jint.dll'){$from=$workspace+'\continuity-evidence\temporal-startup-v1\native-pair-v2\lazy\Jint.dll';$hash='67F3E7D75EDADCBC0110FD6BB7AC26E56784D3650A83C2AE0AA23DD67EB03E6D'}
        CopyExact $from (Join-Path $result.candidate ('ScriptWorker/'+$file.name)) $hash
'@
ReplaceOnce '    $result.passed=$result.passed-and$result.completed-and$result.originalInputsUnchanged-and$result.candidateBytesUnchanged-and$result.privateFilesUnchanged' @'
    $result.decoderPolicyAccepted=$false
    if($result.before){
        $changes=@($result.before|Where-Object{(Get-Acl -LiteralPath $_.path).Sddl -cne $_.sddl})
        $result.decoderPolicyAccepted=$changes.Count -eq 0
        if($changes.Count -eq 1){
            $result.decoderPolicyAccepted=Test-DecoderAclDelta -Path $changes[0].path -ExpectedDecoderPath ($result.candidate+'\Lea.Image.exe') -Before $changes[0].sddl -After (Get-Acl -LiteralPath $changes[0].path).Sddl -ProfileName 'Lea.Image.Decoder'
        }
        $result.aclDeltaPaths=@($changes|ForEach-Object{$_.path})
    }
    $result.passed=$result.passed-and$result.completed-and$result.originalInputsUnchanged-and$result.candidateBytesUnchanged-and$result.decoderPolicyAccepted
'@
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Invalid generated syntax'}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
