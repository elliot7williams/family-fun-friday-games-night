$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/suite-footer-proof-v1'
if(Test-Path -LiteralPath $root){throw 'Preserve footer proof'}
$null=New-Item -ItemType Directory -Path $root
$helper=Join-Path $PSScriptRoot 'Test-LeaSuiteFooter.ps1';. $helper
$checks=0
function Check([bool]$value,[string]$name){if(!$value){throw $name};$script:checks++;Write-Output ('PASS '+$name)}
foreach($row in @(
    @('progress',"progress`nSUCCESS`n",$true),
    @('only',"SUCCESS`n",$true),
    @('early',"SUCCESS`nprogress`n",$false),
    @('duplicate',"SUCCESS`nSUCCESS`n",$false),
    @('prefix',"SUCCESS extra`n",$false),
    @('empty','',$false)
)){
    $path=Join-Path $root ($row[0]+'.txt');[IO.File]::WriteAllText($path,$row[1])
    Check ((Test-LeaSuiteFooter $path 'SUCCESS') -eq $row[2]) $row[0]
}
$threw=$false;try{Test-LeaSuiteFooter (Join-Path $root 'only.txt') ''}catch{$threw=$true}
Check $threw 'empty expected footer rejected'
$full=Join-Path $workspace 'continuity-evidence/decoder-launch-full-v2/full-engine-traced.stdout.txt'
$source=Join-Path $workspace 'continuity-evidence/ordered-flow-regression-v4/Smoke.cs'
$expected=[regex]::Matches([IO.File]::ReadAllText($source),'Console\.WriteLine\("(Lea smoke passed: [^"\r\n]*)"\);')
if($expected.Count -ne 1 -or $expected[0].Groups[1].Value.Contains('\')){throw 'Simple literal source footer differs'}
Check (Test-LeaSuiteFooter $full $expected[0].Groups[1].Value) 'retained successful full output recognized'
$failed=Join-Path $workspace 'continuity-evidence/decoder-launch-full-v1/full-engine-traced.stdout.txt'
Check (!(Test-LeaSuiteFooter $failed $expected[0].Groups[1].Value)) 'retained failed full output rejected'
$inputs=@($helper,$PSCommandPath,$full,$source,$failed)|ForEach-Object {@{path=$_;sha256=(Get-FileHash -LiteralPath $_).Hash}}
[IO.File]::WriteAllText((Join-Path $root 'result.json'),(@{passed=$true;checks=$checks;inputs=$inputs;nativeWorkerLaunched=$false;scope='Terminal exact footer validation only, not suite execution or release qualification'}|ConvertTo-Json -Depth 5))
"PASS suite footer: $checks checks"
