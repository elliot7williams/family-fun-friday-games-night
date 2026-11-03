param([ValidateRange(1,8)][int]$Version=1,[switch]$RecoveryTrace,[switch]$Replay)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$candidate=Join-Path $root ('continuity-evidence/sdk-full-script-suite-v'+$Version)
if($RecoveryTrace){if($Version -ne 8){throw 'Recovery trace requires deterministic candidate'};$candidate=Join-Path $root 'continuity-evidence/contains-recovery-full-prefix-v1'}
if($Replay){if($Version -ne 8 -or $RecoveryTrace){throw 'Replay requires separate deterministic mode'};$candidate=Join-Path $root 'continuity-evidence/deterministic-full-replay-v1'}
$receipt=Join-Path $candidate 'staging.json'
$out=Join-Path $candidate 'staging-audit.json'
if(Test-Path -LiteralPath $out){throw 'Preserve staging audit'}
function Pin([string]$path,[string]$hash){if((Get-FileHash -LiteralPath $path).Hash -cne $hash){throw ('Staging audit differs: '+$path)}}
$stagingPin=if($Version -eq 4){'82D6BD02492F8D97B4AE5FD692E8035E601D72C69FA4F5303DC91AC582B4DC03'}elseif($Version -eq 3){'8630A1522CB258464E9993B7CD2EC661D4D8182C1E846F103CF7DE7595C91629'}elseif($Version -eq 2){'3F3AE0A44A1803C78D746AFB9FB2BBA49A05C90091D334D120832CF83A59C7D0'}else{'DA7C68B40333CB9CF5DC0B8D0B2B9E9C0AB0DEAB2F153B1BD45FC704ECBE6073'}
if($Version -eq 5){$stagingPin='03BC4310AEEBF6BD87387E4BA26A10B1D76D4D823064E3AA0C637A85B83DF353'}
if($Version -eq 6){$stagingPin='68EE47715CFA033B76921A74813E2E33CCD52059516427B627306C332DDA6FC1'}
if($Version -eq 7){$stagingPin='F693258746A0E08024A72011F9E5E6EE242766FAD468BE4849FF4BCC1C0BFBE8'}
if($Version -eq 8){$stagingPin='90FD0D60BA1CCE04C0923D6E18E7FB5D218F4F04C129DD49F97477AFFD2384CA'}
if($RecoveryTrace){$stagingPin='18C161A100C9991379C1C85925B37212B60F711D83BCBBDC5964F93DD62B9C5F'}
if($Replay){$stagingPin='E39D9685FED2430B972A1D6900C13D07F645C252E3DFD88A9B031F4323556DB9'}
Pin $receipt $stagingPin
$staging=Get-Content $receipt -Raw|ConvertFrom-Json
if(!$staging.verified -or $staging.executed -or $staging.compiledSources -ne 60 -or $staging.componentManifests -ne 6){throw 'Staging incomplete'}
if($RecoveryTrace -and !$staging.diagnosticOnly){throw 'Diagnostic staging scope missing'}
if($Replay -and ($staging.diagnosticOnly -or !$staging.uninstrumentedReplay)){throw 'Replay staging scope missing'}
foreach($file in $staging.files){Pin (Join-Path $candidate $file.path) $file.sha256}
$reader=Join-Path $root 'continuity-evidence/temporal-startup-v1/ReadEmbeddedEntryResource.cs'
Pin $reader '0F4A2FA89F91C0142435A975FEE1EE0331803E6EFFDD4896D7BDFA9E1B874644'
Add-Type -Path $reader
$hostPath=Join-Path $staging.runtime 'WorkerEntryProbe/WorkerEntryNativeHost.exe'
$hostPin=if($Version -eq 4){'EC1F219D26215211F865243353C75B1A170DB73C3AD20C4A66E3960C30262961'}elseif($Version -eq 3){'F94B18A9C9324247A0CEA6E029F6351503A0272E2CF04C9CAE2CB16257369589'}elseif($Version -eq 2){'CD654048DA0B7F834AA181E3C24D6B116772FA8894A4E89838A7EEE6E3783DF3'}else{'042B7A022F4C8C8D5D22041931C9A721E25440EA763E84724902C98AF9CF1F6A'}
if($Version -eq 5){$hostPin='14B51A92BEDB5D74725E0889EBE12B1B1F488892DBD16F0F6CD2409FD3124A4E'}
if($Version -eq 6){$hostPin='EC963A3F2ADDF26BFF5B07C6EF69F31BADC81CC0B9029F9915BB73FFC8CDA31A'}
if($Version -eq 7){$hostPin='4A0B402E08A6F00CCC04EB07771A828BEB96B8D2B015458E52D9B8792A5A2C3A'}
if($Version -eq 8){$hostPin='6C1B57552ABEB9382C4EC0BC505AF2E7C7358C95419BA1D18BEE6AC4EE10F95E'}
if($RecoveryTrace){$hostPin='474BEA6438815C31BC9C5ED4CA8B1283ACBF60BF3D6A8E7965DF987D0BDAA475'}
if($Replay){$hostPin='A7913830E84A5F1E158FD23ED4672E081EA0E410004DB2869DC00745A0334C84'}
Pin $hostPath $hostPin
$resource=[ReadEmbeddedEntryResource]::Read($hostPath)|ConvertFrom-Json -AsHashtable
$expected=Join-Path $staging.runtime 'WorkerEntryFixture'
if($resource.directory -cne $expected -or $resource.files.Count -ne 7){throw 'Embedded fixture location/inventory differs'}
$entry=Get-Content (Join-Path $staging.runtime 'WorkerEntryProbe/manifest.json') -Raw|ConvertFrom-Json -AsHashtable
foreach($name in $resource.files.Keys){
    if([IO.Path]::GetFileName($name) -cne $name -or $entry.fixtureFiles[$name] -cne $resource.files[$name]){throw 'Embedded/manifest fixture identity differs'}
    Pin (Join-Path $expected $name) $resource.files[$name]
}
foreach($file in $staging.files){Pin (Join-Path $candidate $file.path) $file.sha256}
@{verified=$true;engineExecuted=$false;diagnosticOnly=[bool]$RecoveryTrace;uninstrumentedReplay=[bool]$Replay;stagingReceiptSha256=(Get-FileHash $receipt).Hash;embeddedFixtureReadWithoutLoadingHost=$true;embeddedDirectory=$expected;embeddedFiles=7;selectedFiles=$staging.files.Count;testBinarySha256=(Get-FileHash $staging.binary).Hash;pending=@('owned launch preflight and unchanged ACL/byte checks','full300s/5s campaign');runnerSha256=(Get-FileHash $PSCommandPath).Hash}|ConvertTo-Json -Depth 5|Set-Content $out -Encoding utf8
Get-FileHash $out
