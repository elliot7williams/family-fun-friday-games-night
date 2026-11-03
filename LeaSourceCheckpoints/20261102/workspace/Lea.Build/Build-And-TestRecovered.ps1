param(
    [Parameter(Mandatory=$true)][string]$Source,
    [Parameter(Mandatory=$true)][string]$Out,
    [ValidateSet('diagnostic-urls')][string[]]$ExtraSmokeModes=@()
)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..')).TrimEnd('\')
$output=[IO.Path]::GetFullPath($Out).TrimEnd('\')
if(-not$output.StartsWith($workspace+'\',[StringComparison]::OrdinalIgnoreCase)-or(Test-Path -LiteralPath $output)){
    throw 'Choose a new workspace output folder; existing candidates are preserved.'
}
$parent=[IO.Path]::GetDirectoryName($output)
while($parent){
    if((Get-Item -LiteralPath $parent -Force).Attributes-band[IO.FileAttributes]::ReparsePoint){throw 'Linked output ancestor refused.'}
    $parent=[IO.Path]::GetDirectoryName($parent)
}
if([IO.DriveInfo]::new('C:\').AvailableFreeSpace-lt1GB){throw 'At least 1 GiB working space is required for build plus browser validation.'}
$null=New-Item -ItemType Directory -Path $output
$native=Join-Path $output 'native'
$browser=Join-Path $output 'browser'
$summary=[ordered]@{passed=$false;nativeReceipt=$native+'\result.json';browserReceipt=$browser+'\result.json';
    scope='Recovered Firespider host with newly compiled native Lea components; optional script parent/worker remain the pinned numeric candidate.';
    hostRecompiled=$false;fullReleaseCertified=$false;error=$null}
try {
    & (Join-Path $PSScriptRoot 'Build-NativeCheck.ps1') -Source $Source -Out $native -ExtraSmokeModes $ExtraSmokeModes
    & (Join-Path $PSScriptRoot 'Test-RecoveredCandidate.ps1') -NativeBuild $native -Out $browser
    $summary.passed=$true
} catch {$summary.error=$_.Exception.ToString()}
finally {$summary|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $output 'build-and-test.json') -Encoding UTF8}
if(-not$summary.passed){throw 'Build/test gates failed or are incomplete. Retain the candidate and inspect the summary; do not publish it.'}
