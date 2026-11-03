$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/temporal-startup-v1'
$out=Join-Path $root 'decoder-profile-policy-v1.json'
if(Test-Path -LiteralPath $out){throw 'Preserve policy evidence'}
$receipt=Join-Path $workspace 'continuity-evidence/permission-contract-v1/retry-view-v1/result.json'
if((Get-FileHash -LiteralPath $receipt).Hash -cne '48ACDD2FB42179D66418E80EE0640A26B3B6FA9439E26CFEB4FEAF4E3A44BFAB'){throw 'View build receipt changed'}
$build=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
if(!$build.passed -or !$build.inputsUnchanged){throw 'View build incomplete'}
$source=Join-Path (Split-Path $receipt) 'source/Rp.cs'
$proof=@($build.sourceProofs|Where-Object copy -ceq $source)
if($proof.Count -ne 1){throw 'Frozen source proof missing'}
if((Get-FileHash -LiteralPath $source).Hash -cne $proof[0].sha256){throw 'Frozen Rp source changed'}
$text=[IO.File]::ReadAllText($source)
$default='return Open(path, args, directory, memory, cpu, ContainerName, new[] { path }, false);'
$release='if (temporaryProfile) worker.release = () => Release(profile, files, identity);'
foreach($statement in @($default,$release)){if($text.IndexOf($statement) -lt 0 -or $text.IndexOf($statement) -ne $text.LastIndexOf($statement)){throw 'Expected unique profile policy source statements'}}
$binary=Join-Path (Split-Path $receipt) 'Lea.Win.dll'
if((Get-FileHash -LiteralPath $binary).Hash -cne 'E26135F8CE6BA76A76515D2874DFE219E172B212037CD567AD37042DA516289B'){throw 'View binary changed'}
$assembly=[Reflection.Assembly]::LoadFrom($binary)
$type=$assembly.GetType('Lea.Win.Rp',$true)
$methods=@($type.GetMethods([Reflection.BindingFlags]'Public,Static')|Where-Object {$_.Name -ceq 'Open' -and $_.GetParameters().Count -eq 5})
if($methods.Count -ne 1){throw 'Default Open ambiguous'}
$method=$methods[0];$il=$method.GetMethodBody().GetILAsByteArray()
if($il.Length -ne 28 -or [Convert]::ToHexString($il[0..6]) -cne '020304050E0472' -or
    [Convert]::ToHexString($il[11..12]) -cne '178D' -or
    [Convert]::ToHexString($il[17..22]) -cne '251602A21628' -or $il[27] -ne 0x2a){throw 'Unexpected default launch IL shape'}
$profile=$method.Module.ResolveString([BitConverter]::ToInt32($il,7))
$arrayElement=$method.Module.ResolveType([BitConverter]::ToInt32($il,13))
$target=$method.Module.ResolveMethod([BitConverter]::ToInt32($il,23))
if($profile -cne 'Lea.Image.Decoder' -or $arrayElement -ne [string] -or $target.DeclaringType -ne $type -or $target.Name -cne 'Open' -or $target.GetParameters().Count -ne 8 -or $target.GetParameters()[7].ParameterType -ne [bool]){throw 'Unexpected launch target/profile'}
$result=@{
    verified=$true;viewSha256=(Get-FileHash -LiteralPath $binary).Hash;sourceSha256=$proof[0].sha256
    defaultOpenMetadataToken=$method.MetadataToken;defaultOpenIl=[Convert]::ToHexString($il)
    fixedProfileName=$profile;defaultTemporaryProfile=$false;grantedFiles='Only the supplied decoder executable path'
    compiledDefaultArgumentsVerified=$true;temporaryOnlyReleaseAssignmentVerifiedInPinnedSource=$true
    workersLaunched=0;filesystemAclChanged=$false;profileDeleted=$false;concurrentNativeDecodersTested=$false
    conclusion='Persistent fixed-profile grant follows current policy; deleting/revoking it after one worker exits is not a valid shared-lifetime fix'
    next='Test two simultaneous decoder workers on a private copied executable, disposing one while the other continues; keep original evidence untouched'
}
[IO.File]::WriteAllText($out,($result|ConvertTo-Json -Depth 5),[Text.UTF8Encoding]::new($false))
$result|ConvertTo-Json -Depth 5
(Get-FileHash -LiteralPath $out).Hash
