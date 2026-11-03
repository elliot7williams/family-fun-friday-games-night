function Read-LeaSuiteFooter {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true)][AllowEmptyCollection()][AllowEmptyString()][string[]]$Lines,
        [ValidateRange(0,2147483647)][int]$ExpectedChecks=0
    )
    $pattern='^PASS Lea\.Script: ([1-9][0-9]*) checks; JavaScript, isolated worker, and typed DOM\.$'
    $footers=@($Lines|Where-Object {$_ -cmatch $pattern})
    if($footers.Count -ne 1 -or $Lines.Count -eq 0 -or $footers[0] -cne $Lines[-1]){
        throw 'Expected exactly one terminal Lea.Script completion footer'
    }
    $null=$footers[0] -cmatch $pattern
    $checks=0
    if(![int]::TryParse($Matches[1],[ref]$checks) -or $checks -lt 1){throw 'Invalid suite check count'}
    if($ExpectedChecks -gt 0 -and $checks -ne $ExpectedChecks){throw 'Unexpected suite check count'}
    $passes=@($Lines|Where-Object {$_.StartsWith('PASS ',[StringComparison]::Ordinal)}).Count
    if([long]$passes -ne [long]$checks+1){throw 'Footer count disagrees with assertion output'}
    return [pscustomobject]@{Checks=$checks;AssertionLines=$passes-1;Footer=$footers[0]}
}
