$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace 'continuity-evidence/all-startup-trace-source-controls-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve trace source controls'}
. (Join-Path $PSScriptRoot 'New-AllStartupTraceSource.ps1')
$path=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/current-native-script-suite-v6/source/Lea.Script.Tests/Program.cs'
$original=[IO.File]::ReadAllText($path);$candidate=New-AllStartupTraceSource $original
if(!$candidate.inverseVerified){throw 'Trace inverse missing'}
$rows=[Collections.Generic.List[object]]::new();$rows.Add(@{name='exact-inverse';passed=$true})
foreach($case in @(
    @{name='missing-main';text=$original.Replace('private static int Main(string[] args)','private static int Other(string[] args)')},
    @{name='duplicate-root';text=($original+"`n"+'                var root = args.Length == 0 ? Directory.GetCurrentDirectory() : Path.GetFullPath(args[0]);')},
    @{name='missing-return';text=$original.Replace('                return 1;','                return 2;')},
    @{name='repeated';text=$candidate.text}
)){
    $rejected=$false;try{$null=New-AllStartupTraceSource $case.text}catch{$rejected=$_.Exception.Message -in @('All-startup trace anchor differs','All-startup trace already applied')}
    if(!$rejected){throw ('Trace source control failed: '+$case.name)}
    $rows.Add(@{name=$case.name;passed=$true})
}
$null=New-Item -ItemType Directory -Path $out
[IO.File]::WriteAllText((Join-Path $out 'result.json'),(@{passed=$true;controls=$rows;runtimeExecuted=$false;inputs=@(Get-FileHash $path,$PSCommandPath,(Join-Path $PSScriptRoot 'New-AllStartupTraceSource.ps1')|Select-Object Path,Hash)}|ConvertTo-Json -Depth 6))
@{passed=$true;controls=$rows.Count}|ConvertTo-Json
