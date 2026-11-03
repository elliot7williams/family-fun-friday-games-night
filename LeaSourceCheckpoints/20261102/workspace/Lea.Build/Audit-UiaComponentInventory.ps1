$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$directory=Join-Path $root 'continuity-evidence/wpf-cold-legacy-provider-v2'
$receipt=Join-Path $directory 'result.json'
if((Get-FileHash $receipt).Hash -cne '948865CE4ECB2D49B0CA0DF3EA1A982F9D015B394369CFCE5B4C573149A01F9C'){throw 'Exact diagnostic receipt differs'}
$result=Get-Content $receipt -Raw|ConvertFrom-Json
if(!$result.inputsUnchanged -or $result.ownerAttention -or !$result.runtimeAssemblyAuditPassed){throw 'Diagnostic ownership/input audit incomplete'}
$output=Join-Path $directory 'component-signatures.json'
if(Test-Path $output){throw 'Preserve prior component audit'}
$log=Join-Path $directory 'pair-client.stdout.txt'
$lines=@(Get-Content $log)
if(@($lines|Where-Object {$_ -ceq 'UIA_COMPONENT_INVENTORY_COMPLETE'}).Count -ne 1){throw 'Runtime inventory incomplete'}
$records=@(foreach($line in $lines|Where-Object {$_.StartsWith('UIA_COMPONENT|')}){
    $parts=$line.Split('|')
    if($parts.Count -ne 5){throw 'Inventory row malformed'}
    $path=[IO.Path]::GetFullPath($parts[2])
    if(!$path.StartsWith([IO.Path]::GetFullPath($env:WINDIR)+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Non-Windows runtime component'}
    $before=(Get-FileHash -LiteralPath $path).Hash
    if($before -cne $parts[4]){throw 'Runtime component changed since capture'}
    $signature=Get-AuthenticodeSignature -LiteralPath $path
    if((Get-FileHash -LiteralPath $path).Hash -cne $before){throw 'Component changed during signature query'}
    [pscustomobject]@{kind=$parts[1];path=$path;version=$parts[3];sha256=$before;signatureStatus=$signature.Status.ToString();signatureMessage=$signature.StatusMessage;signer=$signature.SignerCertificate.Subject}
})
if($records.Count -ne 5 -or @($records|Where-Object kind -eq 'native').Count -ne 1){throw 'Expected four managed and one native component'}
$audit=[ordered]@{diagnosticReceiptSha256=(Get-FileHash $receipt).Hash;runtimeLogSha256=(Get-FileHash $log).Hash;runnerSha256=(Get-FileHash $PSCommandPath).Hash;components=$records;readOnlySystemAudit=$true;systemRepaired=$false;legacyGatePassed=$false}
[IO.File]::WriteAllText($output,($audit|ConvertTo-Json -Depth 6))
$readback=Get-Content $output -Raw|ConvertFrom-Json
if($readback.components.Count -ne 5){throw 'Audit readback differs'}
$records|Select-Object kind,version,signatureStatus,signer|ConvertTo-Json
