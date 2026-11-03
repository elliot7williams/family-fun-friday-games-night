param([Parameter(Mandatory=$true)][ValidateSet('control','lazy')][string]$Variant)
$ErrorActionPreference='Stop'
if([IO.DriveInfo]::new('C:\').AvailableFreeSpace -lt 512MB){throw 'Working reserve required'}
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/temporal-startup-v1'
$receipt=Join-Path $root ('contract-'+$Variant+'.json')
if(Test-Path $receipt){throw 'Preserve previous test'}
$buildPath=Join-Path $root 'build-v1/result.json'
if((Get-FileHash $buildPath).Hash -cne '7D82A029AA5977798CF8B323920D606E21DFE4D803243914378EC1CAD8761D29'){throw 'Build receipt differs'}
$build=Get-Content $buildPath -Raw|ConvertFrom-Json
if(!$build.passed -or !$build.inputsUnchanged){throw 'Build incomplete'}
$row=@($build.runs|Where-Object name -ceq $Variant)
if($row.Count -ne 1 -or (Get-FileHash $row[0].binary).Hash -cne $row[0].sha256){throw 'Candidate differs'}
$lease=[IO.File]::Open($row[0].binary,'Open','Read','Read')
try{
    $parser=Join-Path $workspace 'continuity-evidence/modern-worker-v1/apphost-v5/Acornima.dll'
    if((Get-FileHash $parser).Hash -cne 'A11417B8542B4E55330E751FCA44DA5CBDE1D95E79B1519C2ABA59BE7A7B941C'){throw 'Parser differs'}
    $null=[Reflection.Assembly]::LoadFrom($parser)
    Add-Type -Path (Join-Path $root 'TemporalLazyContract.cs')
    $result=[TemporalLazyContract]::Run($row[0].binary,($Variant -ceq 'lazy'))
    $result|ConvertTo-Json -Depth 6|Set-Content $receipt
    $result|ConvertTo-Json -Depth 6
}finally{$lease.Dispose()}
