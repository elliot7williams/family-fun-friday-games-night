$ErrorActionPreference='Stop'
. (Join-Path (Split-Path $PSScriptRoot) 'Lea.Build/Read-BootstrapResources.ps1')
$a='LEA_BOOT_RESOURCE enter wall=10 thread=20 process=30 faults=40 read=50 write=60 other=70 cycles=80 working=90 commit=100 tid=2'
$b='LEA_BOOT_RESOURCE end wall=1010 thread=120 process=230 faults=45 read=55 write=66 other=77 cycles=180 working=80 commit=90 tid=2'
$result=Read-BootstrapResources @($a,$b) @('enter','end') 1000
$delta=$result.intervals[0]
if(!$result.verified -or $result.markers -ne 2 -or $delta.wallMs -ne 1000 -or $delta.threadCpuMs -ne 0.01 -or $delta.processCpuMs -ne 0.02 -or $delta.pageFaults -ne 5 -or $delta.workingDelta -ne -10){throw 'Synthetic resource deltas invalid'}
$bad=@($b.Replace('tid=2','tid=3'),$b.Replace('faults=45','faults=39'),$b.Replace('wall=1010','wall=9'),$b.Replace('process=230','process=10'),$b.Replace('cycles=180','cycles=79'),$b.Replace('LEA_BOOT_RESOURCE end','LEA_BOOT_RESOURCE unexpected'),($b+' trailing'))
$checks=1
foreach($line in $bad){$rejected=$false;try{$null=Read-BootstrapResources @($a,$line) @('enter','end') 1000}catch{$rejected=$true};if(!$rejected){throw 'Malformed resource trace accepted'};$checks++}
$rejected=$false;try{$null=Read-BootstrapResources @($a) @('enter','end') 1000}catch{$rejected=$true};if(!$rejected){throw 'Truncated resource trace accepted'};$checks++
"PASS resource parser: $checks cases"
