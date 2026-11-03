$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$folder = Join-Path $root 'continuity-evidence/title-metadata-regression-v7'
$receipt = Join-Path $folder 'result.json'
$result = Get-Content -LiteralPath $receipt -Raw | ConvertFrom-Json
$binary = Join-Path $folder 'runtime/Lea.Tests.exe'
$expected = @($result.before | Where-Object { $_.path -eq $binary })
if ($expected.Count -ne 1 -or (Get-FileHash -LiteralPath $binary).Hash -cne $expected[0].sha256) {
    throw 'Failure binary differs from the retained test receipt.'
}
Add-Type -Path (Join-Path $PSScriptRoot 'ReadFailureIL.cs')
$instructions = [ReadFailureIL]::Read($binary, 100663299, 213717)
if (-not ($instructions -match '0342D0 call Click') -or
    -not ($instructions -match '0342EA callvirt get_Ke') -or
    -not ($instructions -match 'click focus retains generic tabindex region')) {
    throw 'Failure instruction contract differs.'
}
$report = [ordered]@{
    receiptSha256 = (Get-FileHash -LiteralPath $receipt).Hash
    binarySha256 = $expected[0].sha256
    methodToken = 100663299
    reportedIlOffset = 213717
    instructions = $instructions
    interpretation = 'Main resumes after Click(view, view.Kb); the following assertion dereferences view.Ke. Focus was cleared. This does not establish why hit testing lost the target.'
    nextAction = 'Capture focus bounds, scroll offset, BoxAt/HitAt/FocusAt identities around the generic-region click in a matched focused reproduction. Preserve the focus assertion and security limits.'
    engineFixVerified = $false
}
$output = Join-Path $folder 'failure-location.json'
if (Test-Path -LiteralPath $output) { throw 'Preserve the existing failure-location report.' }
$report | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $output -Encoding utf8
$report | ConvertTo-Json -Depth 5
