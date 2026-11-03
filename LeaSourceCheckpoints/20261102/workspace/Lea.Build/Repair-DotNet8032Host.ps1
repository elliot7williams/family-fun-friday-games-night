$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace 'continuity-evidence/dotnet-host-repair-20261026-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve previous installer attempt'}
if([IO.DriveInfo]::new('C:\').AvailableFreeSpace -lt 1GB){throw 'Insufficient installer headroom'}
$null=New-Item -ItemType Directory -Path $out
Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'runner.ps1')
$result=[ordered]@{completed=$false;passed=$false;error=$null;scope='User-authorized Microsoft installer attempt for exact 8.0.32; no test configuration or runtime pin changes'}
$installer=Join-Path $out 'dotnet-runtime-8.0.32-win-x64.exe'
$expected='47E999865BEC424DA9D75B90F479204305BE5E7857F9EC27E1FB33F399819007E0791505096ACE36BE3F26EE4D66E260434250509597DFAFE7CAAEEE87ACF0E1'
try{
    . (Join-Path $PSScriptRoot 'Test-ComponentProbePrerequisites.ps1')
    $result.before=Get-LeaComponentProbePrerequisites
    if($result.before.prerequisitesPresent){throw 'Exact prerequisites already restored; do not run unnecessary installer'}
    Invoke-WebRequest -Uri 'https://builds.dotnet.microsoft.com/dotnet/Runtime/8.0.32/dotnet-runtime-8.0.32-win-x64.exe' -OutFile $installer
    $result.sha512=(Get-FileHash -LiteralPath $installer -Algorithm SHA512).Hash
    if($result.sha512 -cne $expected){throw 'Microsoft release metadata SHA512 differs'}
    $signature=Get-AuthenticodeSignature -LiteralPath $installer
    $result.signatureStatus=$signature.Status.ToString();$result.signer=$signature.SignerCertificate.Subject
    if($signature.Status -ne 'Valid' -or $result.signer -notmatch 'O=Microsoft Corporation'){throw 'Installer publisher/signature invalid'}
    $result|ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $out 'verified-installer.json')
    $log=Join-Path $out 'installer.log'
    $process=Start-Process -FilePath $installer -ArgumentList @('/install','/passive','/norestart','/log',('"'+$log+'"')) -Verb RunAs -WindowStyle Hidden -PassThru
    try{
        $result.processId=$process.Id
        $process.WaitForExit()
        $result.exitCode=$process.ExitCode
    }finally{$process.Dispose()}
    $result.completed=$true
    $result.after=Get-LeaComponentProbePrerequisites
    $result.passed=$result.exitCode -in @(0,3010) -and $result.after.prerequisitesPresent
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.availableAfter=[IO.DriveInfo]::new('C:\').AvailableFreeSpace
    $result|ConvertTo-Json -Depth 7 | Set-Content -LiteralPath (Join-Path $out 'result.json')
    $result|ConvertTo-Json -Depth 7
}
