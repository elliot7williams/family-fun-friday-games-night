param(
    [ValidateSet('Trial','Modern','Bootstrap','SuiteHistory','EarlierSuites','StartupStudies','OtherStudies','LegacyLarge','HistoricalText','ClasslistHistory','EarlierRuntimeHistory')][string]$Scope='Trial',
    [ValidatePattern('^$|^[a-z0-9-]{1,48}$')][string]$Attempt='',
    [switch]$OlderRuntimeStages
)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
$scopes=@{Trial='modern-worker-v1/parser-probes-v1';Modern='modern-worker-v1';Bootstrap='bootstrap-descriptor-batches-v1';SuiteHistory='temporal-startup-v1';EarlierSuites='temporal-startup-v1';StartupStudies='temporal-startup-v1';OtherStudies='temporal-startup-v1'}
$relativeRoot=if($Scope -eq 'LegacyLarge'){'dist'}elseif($Scope -in @('HistoricalText','ClasslistHistory','EarlierRuntimeHistory')){'continuity-evidence'}else{'continuity-evidence/'+$scopes[$Scope]}
$root=[IO.Path]::GetFullPath((Join-Path $workspace $relativeRoot))
$allowed=if($Scope -in @('LegacyLarge','HistoricalText','ClasslistHistory','EarlierRuntimeHistory')){$workspace+'\'}else{[IO.Path]::GetFullPath((Join-Path $workspace 'continuity-evidence'))+'\'}
if(!$root.StartsWith($allowed,[StringComparison]::OrdinalIgnoreCase)){throw 'Target outside evidence'}
$out=Join-Path $workspace ('continuity-evidence/project-compression-20261027-'+$Scope.ToLowerInvariant())
if($Attempt){$out+='-'+$Attempt}
if(Test-Path $out){throw 'Preserve compression attempt'}
if(@(Get-Process 'Firespider*','Lea.Script.Worker','Lea.Script.Tests','Lea.Firespider.Boot' -ErrorAction SilentlyContinue).Count){throw 'Do not overlap browser tests'}
$ancestor=Get-Item -LiteralPath $root
while($ancestor){if($ancestor.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse ancestor'};$ancestor=$ancestor.Parent}
$legacyFolders=@('LeaExes/Lea-runtime-185-host-stage','LeaExes/Lea-runtime-185-host-stage2','LeaExes/verify-m111-host_2026-09-19_20-13-59','LeaExes/verify-runtime210-final_2026-09-19_16-01-25','.runtime186-work','.m108-host-check-2026-09-13_20-36-44','.m109-host-check-2026-09-19_05-28-50')
if($OlderRuntimeStages){
    if($Scope -ne 'LegacyLarge'){throw 'Older runtime stages require LegacyLarge scope'}
    $legacyFolders=@(Get-ChildItem -LiteralPath $root -Directory -Force | Where-Object {
        $_.Name -match '^\.runtime(15[7-9]|1[6-7][0-9]|18[0-4])-(work|check-.+|stage-.+|final-check-.+|final-verify(?:-.+)?|verify)$'
    } | ForEach-Object Name)
    if(!$legacyFolders.Count){throw 'No older runtime stages'}
}
$classlistFolders=@('classlist-paint-contract-v2','classlist-paint-contract-v3','classlist-paint-diagnostic-v1','classlist-paint-startup-v1')
if($Scope -eq 'EarlierRuntimeHistory'){
    $classlistFolders=@('permission-contract-v1','network-probe-startup-v1',
        'classlist-paint-startup-v3','classlist-paint-receiver-contract-v1',
        'classlist-paint-contract-v1','classlist-paint-receiver-diagnostic-v1',
        'classlist-paint-startup-v2','process-state-parent-build-v1','archived-parent-view-build-v3')
}
$items=@(if($Scope -in @('ClasslistHistory','EarlierRuntimeHistory')){
    foreach($name in $classlistFolders){
        $folder=[IO.Path]::GetFullPath((Join-Path $root $name))
        if(!$folder.StartsWith($root+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Historical folder escape'}
        $walk=Get-Item -LiteralPath $folder
        while($walk){if($walk.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Historical folder link'};$walk=$walk.Parent}
        $tree=@(Get-ChildItem -LiteralPath $folder -Recurse -Force)
        if(@($tree|Where-Object {$_.Attributes -band [IO.FileAttributes]::ReparsePoint}).Count){throw 'Historical tree link'}
        $tree|Where-Object {!$_.PSIsContainer -and $_.Length -ge 256KB -and !($_.Attributes -band [IO.FileAttributes]::Compressed)}
    }
}elseif($Scope -eq 'HistoricalText'){
    $types=@('.cs','.vb','.json','.txt','.ps1','.rsp','.xml','.html','.css')
    Get-ChildItem -LiteralPath $root -Recurse -Force -File|Where-Object {
        $_.Length -ge 64KB -and $_.Extension.ToLowerInvariant() -in $types -and
        !($_.Attributes -band [IO.FileAttributes]::Compressed) -and
        $_.LastWriteTimeUtc -lt [datetime]'2026-10-27T00:00:00Z' -and
        $_.FullName -notmatch '\\(?:print-|runtime-8033|host-runtime8033|host-print-preview|project-compression|current-native-script-suite-v(?:9|10|11))'
    }|Sort-Object Length -Descending|Select-Object -First 600
}elseif($Scope -eq 'LegacyLarge'){
    foreach($name in $legacyFolders){
        $folder=[IO.Path]::GetFullPath((Join-Path $root $name))
        if(!$folder.StartsWith($root+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Historical folder escape'}
        $walk=Get-Item -LiteralPath $folder
        while($walk){if($walk.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Historical folder link'};$walk=$walk.Parent}
        Get-ChildItem -LiteralPath $folder -Recurse -Force
    }
}else{Get-ChildItem -LiteralPath $root -Recurse -Force})
if(@($items|Where-Object {$_.Attributes -band [IO.FileAttributes]::ReparsePoint}).Count){throw 'Reparse tree'}
if($Scope -eq 'HistoricalText'){
    $checked=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    foreach($file in $items){
        $walk=$file.Directory
        while($walk -and $checked.Add($walk.FullName)){
            if($walk.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Historical text ancestor link'}
            $walk=$walk.Parent
        }
    }
}
$files=@($items|Where-Object {!$_.PSIsContainer})
if($Scope -in @('ClasslistHistory','EarlierRuntimeHistory')){
    $selectedClasslistPaths=[Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach($file in $files){if(!$selectedClasslistPaths.Add($file.FullName)){throw 'Duplicate historical file'}}
}
if($Scope -eq 'HistoricalText'){
    $selectedTextPaths=[Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach($file in $files){if(!$selectedTextPaths.Add($file.FullName)){throw 'Duplicate historical text'}}
}
if($Scope -eq 'LegacyLarge'){
    $files=@($files|Where-Object {$_.Length -ge 256KB -and !($_.Attributes -band [IO.FileAttributes]::Compressed)})
    $selectedLegacyPaths=[Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach($file in $files){if(!$selectedLegacyPaths.Add($file.FullName)){throw 'Duplicate historical file'}}
}
$earlierSuites=@('current-flow-bootstrap-cost-v1','current-flow-script-suite-v1','current-lazy-suite-v1','current-lazy-suite-v2','named-references-script-suite-v1','suite-phase-relocated-v1','suite-phase-relocated-v2')
$otherStudies=@('bootstrap-exception-cost-v1','external-bootstrap-cost-v1','numeric-references-script-suite-v1','rebuilt-core-script-suite-v1')
if($Scope -eq 'OtherStudies'){
    $files=@($files|Where-Object {$_.FullName.Substring($root.Length+1).Split('\')[0] -cin $otherStudies})
}
if($Scope -eq 'StartupStudies'){
    $files=@($files|Where-Object {$_.FullName.Substring($root.Length+1).Split('\')[0] -cmatch '^same-context-bootstrap-cost-v[1-4]$'})
}
if($Scope -eq 'EarlierSuites'){
    $files=@($files|Where-Object {$_.FullName.Substring($root.Length+1).Split('\')[0] -cin $earlierSuites})
}
if($Scope -eq 'SuiteHistory'){
    # Only superseded suites; leave the latest candidates and run evidence untouched.
    $files=@($files|Where-Object {
        $_.FullName.Substring($root.Length+1).Split('\')[0] -match '^current-native-script-suite-v[1-8]$'
    })
}
if(!$files.Count){throw 'Empty target'}
$largest=($files|Measure-Object Length -Maximum).Maximum
if([IO.DriveInfo]::new('C:\').AvailableFreeSpace -lt $largest+8MB){throw 'Insufficient per-file compression workspace'}
if(@(Get-CimInstance Win32_Process|Where-Object {$_.ExecutablePath -and $_.ExecutablePath.StartsWith($root+'\',[StringComparison]::OrdinalIgnoreCase)}).Count){throw 'Historical executable in use'}
Add-Type @'
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
public static class LeaStoredBytes {
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]
 private static extern uint GetCompressedFileSizeW(string path,out uint high);
 public static long Read(string path) { uint high;uint low=GetCompressedFileSizeW(path,out high);
  if(low==uint.MaxValue && Marshal.GetLastWin32Error()!=0)throw new Win32Exception();
  return checked((long)(((ulong)high<<32)|low)); }
}
'@
function Snapshot([string]$Path){
    $item=Get-Item -LiteralPath $Path -Force
    $streams=@(foreach($stream in Get-Item -LiteralPath $Path -Stream * -Force){
        if($stream.Stream -eq ':$DATA'){continue}
        $s=[IO.File]::Open($Path+':'+$stream.Stream,'Open','Read','Read')
        try{@{name=$stream.Stream;length=$s.Length;hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))}}finally{$s.Dispose()}
    })
    @{path=$Path;length=$item.Length;hash=(Get-FileHash -LiteralPath $Path).Hash;sddl=(Get-Acl -LiteralPath $Path).Sddl;
      written=$item.LastWriteTimeUtc.ToString('o');attributes=[int]$item.Attributes;stored=[LeaStoredBytes]::Read($Path);streams=$streams}
}
$null=New-Item -ItemType Directory -Path $out
Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'runner.ps1')
$result=[ordered]@{passed=$false;scope=$Scope;root=$root;deletedFiles=0;before=@();after=@();compactExitCodes=@();error=$null}
try{
    $result.freeBefore=[IO.DriveInfo]::new('C:\').AvailableFreeSpace
    $result.before=@(foreach($file in $files){Snapshot $file.FullName})
    [IO.File]::WriteAllText((Join-Path $out 'before.json'),($result.before|ConvertTo-Json -Depth 6))
    $candidates=@($result.before|Where-Object {!($_.attributes -band [int][IO.FileAttributes]::Compressed) -and !($_.attributes -band [int][IO.FileAttributes]::Encrypted) -and $_.length -gt 0})
    $compact=Join-Path $env:WINDIR 'System32/compact.exe'
    for($index=0;$index -lt $candidates.Count;$index+=20){
        $batch=@($candidates|Select-Object -Skip $index -First 20|ForEach-Object path)
        $lines=& $compact /C /I /Q @batch 2>&1
        $exit=$LASTEXITCODE;$result.compactExitCodes+=@($exit)
        [IO.File]::AppendAllText((Join-Path $out 'compact.log'),(($lines|Out-String)+[Environment]::NewLine))
        if($exit -ne 0){throw 'Compression reported an error; preserve evidence and verify remaining state'}
    }
    $current=@(if($Scope -in @('ClasslistHistory','EarlierRuntimeHistory')){
        foreach($name in $classlistFolders){Get-ChildItem -LiteralPath (Join-Path $root $name) -Recurse -Force -File|Where-Object {$selectedClasslistPaths.Contains($_.FullName)}}
    }elseif($Scope -eq 'HistoricalText'){
        Get-ChildItem -LiteralPath $root -Recurse -Force -File|Where-Object {$selectedTextPaths.Contains($_.FullName)}
    }elseif($Scope -eq 'LegacyLarge'){
        foreach($name in $legacyFolders){Get-ChildItem -LiteralPath (Join-Path $root $name) -Recurse -Force -File|Where-Object {$selectedLegacyPaths.Contains($_.FullName)}}
    }else{Get-ChildItem -LiteralPath $root -Recurse -Force -File})
    if($Scope -eq 'OtherStudies'){
        $current=@($current|Where-Object {$_.FullName.Substring($root.Length+1).Split('\')[0] -cin $otherStudies})
    }
    if($Scope -eq 'StartupStudies'){
        $current=@($current|Where-Object {$_.FullName.Substring($root.Length+1).Split('\')[0] -cmatch '^same-context-bootstrap-cost-v[1-4]$'})
    }
    if($Scope -eq 'EarlierSuites'){
        $current=@($current|Where-Object {$_.FullName.Substring($root.Length+1).Split('\')[0] -cin $earlierSuites})
    }
    if($Scope -eq 'SuiteHistory'){
        $current=@($current|Where-Object {
            $_.FullName.Substring($root.Length+1).Split('\')[0] -match '^current-native-script-suite-v[1-8]$'
        })
    }
    if($current.Count -ne $files.Count){throw 'File inventory changed'}
    $result.after=@(foreach($prior in $result.before){
        $now=Snapshot $prior.path
        if($now.length -ne $prior.length -or $now.hash -cne $prior.hash -or $now.sddl -cne $prior.sddl -or $now.written -cne $prior.written){throw ('File content/permissions/time changed: '+$prior.path)}
        $mask=-bnot ([int][IO.FileAttributes]::Compressed -bor [int][IO.FileAttributes]::Archive)
        if(($now.attributes -band $mask) -ne ($prior.attributes -band $mask)){throw 'Unexpected attribute change'}
        if(($now.streams|ConvertTo-Json -Depth 4 -Compress) -cne ($prior.streams|ConvertTo-Json -Depth 4 -Compress)){throw 'Alternate streams changed'}
        $now
    })
    $result.storedBefore=($result.before|Measure-Object stored -Sum).Sum
    $result.storedAfter=($result.after|Measure-Object stored -Sum).Sum
    $result.reclaimedStoredBytes=$result.storedBefore-$result.storedAfter
    $result.files=$files.Count;$result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.freeAfter=[IO.DriveInfo]::new('C:\').AvailableFreeSpace
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 7))
    [pscustomobject]$result|Select-Object passed,scope,files,deletedFiles,reclaimedStoredBytes,freeBefore,freeAfter,error|ConvertTo-Json
}
