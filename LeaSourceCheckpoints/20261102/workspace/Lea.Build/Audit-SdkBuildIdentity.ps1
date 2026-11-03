$ErrorActionPreference='Stop'
$out='I:\LeaBuildStorage\sdk-build-identity-20261101-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve identity inspection'}
$paths=@(
    @('I:\LeaBuildStorage\full-sdk-libraries-build-20261101-v1/result.json','3AF0DA862CF1FA7C1E3E53E949B7EA0F37DE6593BEE8DDC5A02EFA8653522E6B'),
    @('I:\LeaBuildStorage\full-sdk-native-build-20261101-v1/result.json','20E2EE1F8A87BFC2B14CD581E48B59D1DE834E87EBA729DCB5A45CCF4E1A7893'),
    @('I:\LeaBuildStorage\portable-full-sdk-build-20261101-v1/result.json','17AAF573A110588C3D9A0C43944465B0504B44865CBBAFDFBE6C9D319C29C2C7')
)
$proofs=@(foreach($pair in $paths){if((Get-FileHash $pair[0]).Hash -cne $pair[1]){throw 'Identity campaign receipt differs'};Get-Content $pair[0] -Raw|ConvertFrom-Json})
foreach($proof in $proofs){if(!$proof.passed -or !$proof.inputsUnchanged -or $proof.ownerAttention){throw 'Passing retired build required'}}
$reader=Join-Path $PSScriptRoot 'SdkBuildIdentity.cs';$readerHash=(Get-FileHash $reader).Hash
Add-Type -Path $reader
$old=@($proofs[0].libraryProducts)+@($proofs[1].nativeProducts)
$new=@($proofs[2].libraryProducts)+@($proofs[2].nativeProducts)
$holds=[Collections.Generic.List[IDisposable]]::new();$comparisons=[Collections.Generic.List[object]]::new()
try{
    foreach($file in @($old)+@($new)){$holds.Add([IO.File]::Open($file.path,'Open','Read','Read'));if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Identity image differs from built receipt'}}
    foreach($file in $new){
        $name=[IO.Path]::GetFileName($file.path);$previous=@($old|Where-Object {[IO.Path]::GetFileName($_.path) -ceq $name})
        if($previous.Count -ne 1){throw 'Unique previous identity required'}
        if($file.path -notmatch '\.(dll|exe)$'){continue}
        $a=[SdkBuildIdentity]::Read($previous[0].path);$b=[SdkBuildIdentity]::Read($file.path)
        $comparisons.Add(@{name=$name;previous=$a;current=$b;byteIdentical=($a.sha256 -ceq $b.sha256);identicalAfterOnlyTimestampAndMvidMask=($a.timestampAndMvidMaskedSha256 -ceq $b.timestampAndMvidMaskedSha256)})
    }
    if((Get-FileHash $reader).Hash -cne $readerHash){throw 'Identity reader source changed'}
    New-Item -ItemType Directory -Path $out|Out-Null
    @{verified=$true;images=$comparisons;targetAssembliesExecuted=$false;readerSha256=$readerHash;scope='Metadata-only identity comparison, not permission to transfer passing results or explanation of runtime timing failures.'}|ConvertTo-Json -Depth 8|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    Get-FileHash (Join-Path $out 'result.json')
}finally{foreach($s in $holds){$s.Dispose()}}
