#requires -Version 7.4
param(
    [Parameter(Mandatory)][Alias('cc')][string]$CompilerRoot,
    [Parameter(Mandatory)][Alias('rt')][string]$RuntimeRoot,
    [Parameter(Mandatory)][Alias('o')][string]$Output
)
$ErrorActionPreference='Stop'
function Plain([string]$path){
    $full=[IO.Path]::GetFullPath($path)
    if($full.StartsWith('\\',[StringComparison]::Ordinal)){throw 'Local toolchain paths required'}
    $walk=$full
    while($walk){if((Test-Path -LiteralPath $walk) -and ((Get-Item -LiteralPath $walk -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)){throw 'Reparse toolchain path refused'};$walk=[IO.Path]::GetDirectoryName($walk)}
    return $full
}
$compiler=Plain $CompilerRoot;$runtime=Plain $RuntimeRoot;$destination=Plain $Output
if((Test-Path -LiteralPath $destination) -or $destination.StartsWith($compiler+'\',[StringComparison]::OrdinalIgnoreCase) -or $destination.StartsWith($runtime+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Fresh manifest outside input trees required'}
if(!(Test-Path -LiteralPath (Join-Path $compiler 'crossgen2.exe')) -or !(Test-Path -LiteralPath (Join-Path $runtime 'System.Private.CoreLib.dll'))){throw 'Compiler/runtime roots incomplete'}
$tools=@(Get-ChildItem -LiteralPath $compiler -Recurse -File -Force)
$references=@(Get-ChildItem -LiteralPath $runtime -File -Filter '*.dll')
if($tools.Count -lt 1 -or $tools.Count -gt 128 -or $references.Count -lt 150 -or $references.Count -gt 256){throw 'Bounded toolchain inventory differs'}
function Members($files,[string]$directory){
    foreach($file in $files|Sort-Object FullName){
        $null=Plain $file.FullName
        @{path=[IO.Path]::GetRelativePath($directory,$file.FullName).Replace('\','/');sha256=(Get-FileHash -LiteralPath $file.FullName).Hash}
    }
}
$toolFiles=@(Members $tools $compiler);$runtimeFiles=@(Members $references $runtime)
foreach($pair in @(@($compiler,$toolFiles),@($runtime,$runtimeFiles))){foreach($file in $pair[1]){if((Get-FileHash -LiteralPath (Join-Path $pair[0] $file.path)).Hash -cne $file.sha256){throw 'Toolchain changed while recording'}}}
New-Item -ItemType Directory -Path (Split-Path $destination) -Force|Out-Null
@{schemaVersion=1;compilerVersion='8.0.32';runtimeVersion='8.0.33';compilerRoot=$compiler;runtimeRoot=$runtime;compilerFiles=$toolFiles;runtimeFiles=$runtimeFiles;scope='Local byte snapshot, not origin authentication. Obtain and authenticate Microsoft tools before recording.'}|ConvertTo-Json -Depth 5|Set-Content -LiteralPath $destination -Encoding utf8
Get-FileHash -LiteralPath $destination
