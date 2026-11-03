$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$runtime=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/host-process-state-v1/runtime'
$parent=Join-Path $runtime 'Lea.Script.dll'
if((Get-FileHash -LiteralPath $parent).Hash -cne 'D5B66F9E05D422040A49EC3EE3F16B99C9731EB036D6369B3C758EEE5BBD2916'){throw 'Parent changed'}
$out=Join-Path $workspace 'continuity-evidence/process-state-parent-build-v1/report-regression.json'
if(Test-Path -LiteralPath $out){throw 'Preserve report tests'}
$type=[Reflection.Assembly]::LoadFrom($parent).GetType('Lea.Script.Jp',$true)
$flags=[Reflection.BindingFlags]'NonPublic,Static'
$failure=$type.GetMethod('StartupFailure',$flags)
$report=$type.GetMethod('StartupReport',$flags)
if(!$failure -or !$report){throw 'Compiled report bindings missing'}
$checks=[Collections.Generic.List[string]]::new()
foreach($state in @('running','exited=23','exited=259','unavailable','unknown')){
    $summary='phase=hello; child='+$state+'; launch=422; probe=71; wall=5011; cpu=62 ms'
    foreach($trace in @('',('startup document: wall=1900; cpu=62 ms'+"`n"+('later trace noise '+"`n")*200))){
        $text=[string]$failure.Invoke($null,[object[]]@(('x'*300),$summary,$trace))
        if($text.Length -gt 512 -or !$text.Contains($summary) -or !$text.StartsWith(('x'*125)+'... ')){throw "Failure report lost summary or bounds: $state"}
        if($trace.Length -gt 0 -and !$text.Contains('startup document:')){throw 'Lost final bootstrap checkpoint'}
        $checks.Add('bounded failure preserves '+$state+'; trace='+($trace.Length -gt 0))
    }
}
foreach($length in @(0,510,511,512,513,2048)){
    $prefix='a'*$length
    $text=[string]$report.Invoke($null,[object[]]@($prefix,''))
    if($text.Length -ne [Math]::Min($length,512) -or $text -cne $prefix.Substring(0,[Math]::Min($length,512))){throw "Report bound mismatch: $length"}
    $checks.Add('empty trace boundary '+$length)
}
if((Get-FileHash -LiteralPath $parent).Hash -cne 'D5B66F9E05D422040A49EC3EE3F16B99C9731EB036D6369B3C758EEE5BBD2916'){throw 'Parent changed during tests'}
$result=@{passed=$true;checks=$checks;binary=$parent;sha256=(Get-FileHash -LiteralPath $parent).Hash;scope='Actual compiled private report formatters; synthetic failure inputs, no worker launch, lifecycle or timeout-reproduction claim'}
[IO.File]::WriteAllText($out,($result|ConvertTo-Json -Depth 4))
"PASS compiled startup report regression: $($checks.Count) checks."
