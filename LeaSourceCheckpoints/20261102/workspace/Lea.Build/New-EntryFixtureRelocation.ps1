function New-EntryFixtureRelocation {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true)][System.Collections.IDictionary]$Manifest,
        [Parameter(Mandatory=$true)][string]$ExpectedOriginalDirectory,
        [Parameter(Mandatory=$true)][string]$DestinationRoot
    )
    $ErrorActionPreference='Stop'
    $workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..')).TrimEnd('\')
    if($Manifest.Count -ne 2 -or !$Manifest.Contains('files') -or !$Manifest.Contains('directory')){throw 'Unexpected entry resource schema'}
    if($Manifest['files'] -isnot [System.Collections.IDictionary]){throw 'Entry file pins must be a dictionary'}
    $original=[IO.Path]::GetFullPath([string]$Manifest['directory']).TrimEnd('\')
    $expected=[IO.Path]::GetFullPath($ExpectedOriginalDirectory).TrimEnd('\')
    $destination=[IO.Path]::GetFullPath($DestinationRoot).TrimEnd('\')
    if($original -cne $expected -or !$original.StartsWith($workspace+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Unexpected original fixture directory'}
    if(!$destination.StartsWith($workspace+'\',[StringComparison]::OrdinalIgnoreCase) -or $destination -eq $workspace){throw 'Destination outside workspace'}
    if($destination -eq $original -or $destination.StartsWith($original+'\',[StringComparison]::OrdinalIgnoreCase) -or $original.StartsWith($destination+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Destination overlaps retained fixture'}
    $walk=$destination
    while($walk){
        if((Test-Path -LiteralPath $walk) -and ((Get-Item -LiteralPath $walk -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)){throw 'Linked destination refused'}
        $walk=[IO.Path]::GetDirectoryName($walk)
    }
    $required=@('Lea.Script.Worker.deps.json','Lea.Script.Worker.dll','Lea.Script.Worker.exe','Lea.Script.Worker.runtimeconfig.json','WorkerEntryCore.dll','WorkerEntryCore.exe','WorkerEntryCore.runtimeconfig.json')
    if($Manifest['files'].Count -ne $required.Count){throw 'Expected seven fixture files'}
    $pins=[ordered]@{}
    foreach($name in $required){
        if(!$Manifest['files'].Contains($name) -or [string]$Manifest['files'][$name] -cnotmatch '^[A-F0-9]{64}$'){throw 'Missing or malformed fixture pin'}
        $pins[$name]=[string]$Manifest['files'][$name]
    }
    # Return a fresh resource; never mutate the original parsed manifest or its files.
    return [ordered]@{files=$pins;directory=(Join-Path $destination 'WorkerEntryFixture')}
}
