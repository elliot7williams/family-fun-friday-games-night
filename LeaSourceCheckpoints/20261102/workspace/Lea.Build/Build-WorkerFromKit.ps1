param([Parameter(Mandatory)][string]$KitRoot,[Parameter(Mandatory)][string]$OutputDirectory,
    [Parameter(Mandatory)][ValidatePattern('^[A-F0-9]{64}$')][string]$ExpectedManifestSha256)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath($KitRoot).TrimEnd('\')
$output=[IO.Path]::GetFullPath($OutputDirectory)
if(Test-Path -LiteralPath $output){throw 'Build output must be new'}
$leases=[Collections.Generic.List[IDisposable]]::new()
function Hold([string]$Path,[string]$Hash){
    $walk=$Path
    while($walk){if((Get-Item -LiteralPath $walk -Force).Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Linked build input refused'};$walk=[IO.Path]::GetDirectoryName($walk)}
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($stream)
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) -cne $Hash){throw "Kit input identity differs: $Path"}
}
function ResolveKit([string]$Relative){
    $path=[IO.Path]::GetFullPath((Join-Path $root $Relative))
    if(!$path.StartsWith($root+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Kit input escapes root'}
    return $path
}
try{
    $manifest=ResolveKit 'manifest.json';Hold $manifest $ExpectedManifestSha256
    $map=Get-Content -LiteralPath $manifest -Raw|ConvertFrom-Json
    if($map.format -ne 1 -or $map.sources.Count -ne 17 -or $map.references.Count -ne 165){throw 'Unexpected kit format'}
    foreach($file in $map.files){Hold (ResolveKit $file.path) $file.sha256}
    foreach($pair in @(
        @('Microsoft.CodeAnalysis.dll','1AB0A49C07AF467D16EC723C5D95DAE3F243AD2ECEA840312228ECDEA8D6E021'),
        @('Microsoft.CodeAnalysis.CSharp.dll','CE8DFED4EB61F1B6FECB6D06E3AADFC21B36C478FD863ED014BAD2CB01987DFD')
    )){
        $path=Join-Path $PSHOME $pair[0];Hold $path $pair[1]
        $signature=Get-AuthenticodeSignature -LiteralPath $path
        if($signature.Status -ne 'Valid' -or $signature.SignerCertificate.Subject -cne 'CN=Microsoft 3rd Party Application Component, O=Microsoft Corporation, L=Redmond, S=Washington, C=US'){throw 'Compiler signature differs'}
        $assembly=[Reflection.Assembly]::LoadFrom($path)
        if($assembly.Location -cne $path){throw 'Unexpected compiler load path'}
    }
    $hostReferences=@(Get-ChildItem -LiteralPath (Join-Path $PSHOME 'ref') -Filter '*.dll' -File|ForEach-Object FullName)
    $hostReferences+=@((Join-Path $PSHOME 'Microsoft.CodeAnalysis.dll'),(Join-Path $PSHOME 'Microsoft.CodeAnalysis.CSharp.dll'))
    Add-Type -Path (ResolveKit 'compiler/PublicCompiler.cs') -ReferencedAssemblies $hostReferences -CompilerOptions '/nowarn:1701'
    $null=New-Item -ItemType Directory -Path $output
    $args=[Collections.Generic.List[string]]::new()
    foreach($option in $map.options){$args.Add($option)}
    $binary=Join-Path $output 'Lea.Script.Worker.dll'
    $args.Add('/out:"'+$binary+'"')
    $args.Add('/resource:"'+(ResolveKit $map.resource)+'",Lea.Script.Dom.js')
    foreach($reference in $map.references){$args.Add('/reference:"'+(ResolveKit $reference)+'"')}
    foreach($source in $map.sources){$args.Add('"'+(ResolveKit $source)+'"')}
    $rsp=Join-Path $output 'compile.rsp';[IO.File]::WriteAllLines($rsp,$args,[Text.UTF8Encoding]::new($false))
    $code=[Lea.Build.PublicCompiler]::Run($rsp)
    if($code -ne 0){throw 'Kit compilation failed'}
    $hash=(Get-FileHash -LiteralPath $binary).Hash
    $result=@{passed=$hash -ceq '83FE295571FAFF00D4864A13C37D270B998B339EA27A61A950A3572A494AC2DB';sha256=$hash;sourceCount=$map.sources.Count;referenceCount=$map.references.Count;runtimeExecuted=$false;scope='Relocated IL build only; pinned PowerShell compiler host still required'}
    [IO.File]::WriteAllText((Join-Path $output 'compile-result.json'),($result|ConvertTo-Json))
    [pscustomobject]$result|ConvertTo-Json
    if(!$result.passed){throw 'Relocated build is not byte-identical'}
}finally{foreach($lease in $leases){$lease.Dispose()}}
