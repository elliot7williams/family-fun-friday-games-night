$ErrorActionPreference='Stop'
$out='I:\LeaBuildStorage\sdk-identity-reader-controls-20261101-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve identity reader controls'}
$proof='I:\LeaBuildStorage\sdk-build-identity-20261101-v1/result.json'
if((Get-FileHash $proof).Hash -cne '1459B734A83DB5C498642696EF562F1653D1A97974242EACB7DCDA3009AFAB90'){throw 'Identity inspection differs'}
$data=Get-Content $proof -Raw|ConvertFrom-Json;$reader=Join-Path $PSScriptRoot 'SdkBuildIdentity.cs'
if((Get-FileHash $reader).Hash -cne $data.readerSha256){throw 'Identity reader differs'}
Add-Type -Path $reader
$core=@($data.images|Where-Object name -ceq 'Lea.Engine.dll')[0].current
$native=@($data.images|Where-Object name -ceq 'Lea.Script.Worker.exe')[0].current
if((Get-FileHash $core.path).Hash -cne $core.sha256 -or (Get-FileHash $native.path).Hash -cne $native.sha256){throw 'Controlled images differ'}
New-Item -ItemType Directory -Path $out|Out-Null
$bytes=[IO.File]::ReadAllBytes($core.path)
$stream=[IO.MemoryStream]::new($bytes,$false);$pe=[Reflection.PortableExecutable.PEReader]::new($stream)
try{$stamp=$pe.PEHeaders.CoffHeaderStartOffset+4}finally{$pe.Dispose();$stream.Dispose()}
$positive=0;$negative=0
$copy=Join-Path $out 'timestamp.dll';$modified=[byte[]]$bytes.Clone();$modified[$stamp]=$modified[$stamp] -bxor 1;[IO.File]::WriteAllBytes($copy,$modified)
$stampOnly=[SdkBuildIdentity]::Read($copy)
if($stampOnly.sha256 -ceq $core.sha256 -or $stampOnly.timestampAndMvidMaskedSha256 -cne $core.timestampAndMvidMaskedSha256){throw 'Timestamp-only control failed'};$positive++
$copy=Join-Path $out 'outside-mask.dll';$modified=[byte[]]$bytes.Clone();$modified[32]=$modified[32] -bxor 1;[IO.File]::WriteAllBytes($copy,$modified)
$outside=[SdkBuildIdentity]::Read($copy)
if($outside.timestampAndMvidMaskedSha256 -ceq $core.timestampAndMvidMaskedSha256){throw 'Unmasked change was hidden'};$positive++
$nativeInfo=[SdkBuildIdentity]::Read($native.path)
if($nativeInfo.hasMetadata -or $nativeInfo.sha256 -cne $native.sha256){throw 'Native apphost control failed'};$positive++
$copy=Join-Path $out 'invalid.dll';[IO.File]::WriteAllBytes($copy,[byte[]](1,2,3))
$denied=$false;try{$null=[SdkBuildIdentity]::Read($copy)}catch{$denied=$true};if(!$denied){throw 'Invalid image accepted'};$negative++
$copy=Join-Path $out 'duplicate-mvid.dll';$modified=[byte[]]$bytes.Clone();$guid=[guid]::Parse($core.moduleId).ToByteArray();[Array]::Copy($guid,0,$modified,64,16);[IO.File]::WriteAllBytes($copy,$modified)
$denied=$false;try{$null=[SdkBuildIdentity]::Read($copy)}catch{$denied=$_.Exception.ToString().Contains('Unique module identity occurrence required')};if(!$denied){throw 'Duplicate MVID control failed'};$negative++
if((Get-FileHash $reader).Hash -cne $data.readerSha256 -or (Get-FileHash $core.path).Hash -cne $core.sha256 -or (Get-FileHash $native.path).Hash -cne $native.sha256){throw 'Controlled source changed'}
@{passed=$true;positiveControls=$positive;negativeControls=$negative;targetCodeExecuted=$false;readerSha256=$data.readerSha256;scope='Masked hash is diagnostic only; actual reproducibility requires full raw product SHA equality.'}|ConvertTo-Json|Set-Content (Join-Path $out 'result.json') -Encoding utf8
Get-FileHash (Join-Path $out 'result.json')
