param([Parameter(Mandatory=$true)][ValidateSet('control','lazy')][string]$Variant)
$ErrorActionPreference='Stop'
if([IO.DriveInfo]::new('C:\').AvailableFreeSpace -lt 512MB){throw 'Working reserve required'}
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/temporal-startup-v1'
$out=Join-Path $root ('javascript-'+$Variant+'.json')
if(Test-Path $out){throw 'Preserve test result'}
$buildPath=Join-Path $root 'build-v1/result.json'
if((Get-FileHash $buildPath).Hash -cne '7D82A029AA5977798CF8B323920D606E21DFE4D803243914378EC1CAD8761D29'){throw 'Build differs'}
$build=Get-Content $buildPath -Raw|ConvertFrom-Json
$row=@($build.runs|Where-Object name -ceq $Variant)
if(!$build.passed -or !$build.inputsUnchanged -or $row.Count -ne 1){throw 'Incomplete candidate'}
$lease=[IO.File]::Open($row[0].binary,'Open','Read','Read')
$result=[ordered]@{passed=$false;variant=$Variant;binarySha256=$row[0].sha256;workerExecuted=$false;performanceGate=$false;checks=@();error=$null}
try{
    if((Get-FileHash $row[0].binary).Hash -cne $row[0].sha256){throw 'Binary changed'}
    $parser=Join-Path $workspace 'continuity-evidence/modern-worker-v1/apphost-v5/Acornima.dll'
    if((Get-FileHash $parser).Hash -cne 'A11417B8542B4E55330E751FCA44DA5CBDE1D95E79B1519C2ABA59BE7A7B941C'){throw 'Parser differs'}
    $null=[Reflection.Assembly]::LoadFrom($parser)
    $source=Join-Path $root 'TemporalJavaScriptContract.cs'
    $result.sourceSha256=(Get-FileHash $source).Hash
    Add-Type -Path $source
    $result.checks=@([TemporalJavaScriptContract]::Run($row[0].binary))
    $result.inputsUnchanged=(Get-FileHash $row[0].binary).Hash -ceq $row[0].sha256 -and (Get-FileHash $source).Hash -ceq $result.sourceSha256
    $result.passed=$result.inputsUnchanged -and $result.checks.Count -eq 12 -and @($result.checks|Where-Object {!$_.passed}).Count -eq 0
}catch{$result.error=$_.Exception.ToString()}
finally{$lease.Dispose();$result|ConvertTo-Json -Depth 7|Set-Content $out}
[pscustomobject]$result|ConvertTo-Json -Depth 5
if(!$result.passed){throw 'JavaScript contract failed; preserve evidence'}
