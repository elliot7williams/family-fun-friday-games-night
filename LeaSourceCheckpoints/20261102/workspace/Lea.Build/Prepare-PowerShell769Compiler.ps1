$ErrorActionPreference='Stop'
$old=Join-Path $PSScriptRoot 'Invoke-PublicCompiler.20261022.ps1'
if((Get-FileHash -LiteralPath $old).Hash -cne '1A04EE9A191D522955C633F97A24C9C0921A5738F76D0DF36DB1C92A574EEA03'){throw 'Old compiler recipe differs'}
$new=Join-Path $PSScriptRoot 'Invoke-PublicCompiler.20261026.ps1'
if(Test-Path -LiteralPath $new){throw 'Preserve compiler migration'}
$rows=@(
    @('Microsoft.CodeAnalysis.CSharp.dll','CE8DFED4EB61F1B6FECB6D06E3AADFC21B36C478FD863ED014BAD2CB01987DFD','5E9323E98D99F669ACD10705C40D985DBABEAF0984285ED32922F91DF223124B'),
    @('Microsoft.CodeAnalysis.dll','1AB0A49C07AF467D16EC723C5D95DAE3F243AD2ECEA840312228ECDEA8D6E021','E7A25CBDEC95390E9672659872189DA368032625D784C25569617617932C145F')
)
$text=[IO.File]::ReadAllText($old)
foreach($row in $rows){
    $path=Join-Path 'C:\Program Files\PowerShell\7' $row[0]
    $signature=Get-AuthenticodeSignature -LiteralPath $path
    if($signature.Status -ne 'Valid' -or $signature.SignerCertificate.Subject -cne 'CN=Microsoft 3rd Party Application Component, O=Microsoft Corporation, L=Redmond, S=Washington, C=US' -or (Get-FileHash -LiteralPath $path).Hash -cne $row[2] -or (Get-Item -LiteralPath $path).VersionInfo.FileVersion -cne '5.0.25.56712'){throw 'New Microsoft compiler identity differs'}
    if([regex]::Matches($text,$row[1]).Count -ne 1){throw 'Compiler pin anchor differs'}
    $text=$text.Replace($row[1],$row[2])
}
[IO.File]::WriteAllText($new,$text,[Text.UTF8Encoding]::new($false))
$hash=(Get-FileHash -LiteralPath $new).Hash
foreach($pair in @(
    @('Build-CurrentHitGeometry.generated.ps1','Build-CurrentHitGeometryV2.generated.ps1'),
    @('Test-CurrentHitGeometry.generated.ps1','Test-CurrentHitGeometryV2.generated.ps1')
)){
    $target=Join-Path $PSScriptRoot $pair[1]
    if(Test-Path -LiteralPath $target){throw 'Preserve migrated candidate recipe'}
    $source=[IO.File]::ReadAllText((Join-Path $PSScriptRoot $pair[0]))
    $source=$source.Replace('Invoke-PublicCompiler.20261022.ps1','Invoke-PublicCompiler.20261026.ps1').Replace('1A04EE9A191D522955C633F97A24C9C0921A5738F76D0DF36DB1C92A574EEA03',$hash)
    $source=$source.Replace('current-hit-geometry-v1','current-hit-geometry-v2').Replace('Build-CurrentHitGeometry.generated.ps1','Build-CurrentHitGeometryV2.generated.ps1')
    [IO.File]::WriteAllText($target,$source,[Text.UTF8Encoding]::new($false))
}
Get-FileHash -LiteralPath $new
