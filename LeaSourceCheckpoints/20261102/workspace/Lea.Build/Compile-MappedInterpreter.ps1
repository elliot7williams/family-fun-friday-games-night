param([Parameter(Mandatory)][string]$Map,[Parameter(Mandatory)][string]$ArchiveRoot,[Parameter(Mandatory)][string]$OutputDirectory)
$ErrorActionPreference='Stop'
if((Get-FileHash -LiteralPath $Map).Hash -cne '08C28F248AE2CC7BEC293E4EECEB455F0A530ADE1370B1298D59BA1EE1B9DC91'){throw 'Build map changed'}
$recipe=Get-Content -LiteralPath $Map -Raw|ConvertFrom-Json
$leases=[Collections.Generic.List[IDisposable]]::new()
function Hold([string]$Path,[string]$Hash){
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($stream)
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) -cne $Hash){throw 'Mapped compiler input changed'}
}
function ResolveArchived($Entry){
    $root=[IO.Path]::GetFullPath($ArchiveRoot).TrimEnd('\')
    $path=[IO.Path]::GetFullPath((Join-Path $root $Entry.archivePath))
    if(!$path.StartsWith($root+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Archive path escape'}
    Hold $path $Entry.sha256
    return $path
}
try{
    $sources=@($recipe.orderedSources|ForEach-Object{ResolveArchived $_})
    if($sources.Count -ne 698){throw 'Mapped source count changed'}
    $references=@($recipe.references|ForEach-Object{Hold $_.path $_.sha256;$_.path})
    $compiler=ResolveArchived $recipe.compiler
    $key=ResolveArchived $recipe.publicSigningKey
    $resource=ResolveArchived $recipe.embeddedResource
    $hostRefs=@(Get-ChildItem (Join-Path $PSHOME 'ref') -Filter '*.dll'|ForEach-Object FullName)
    foreach($pair in @(@('Microsoft.CodeAnalysis.dll','1AB0A49C07AF467D16EC723C5D95DAE3F243AD2ECEA840312228ECDEA8D6E021'),@('Microsoft.CodeAnalysis.CSharp.dll','CE8DFED4EB61F1B6FECB6D06E3AADFC21B36C478FD863ED014BAD2CB01987DFD'))){
        $path=Join-Path $PSHOME $pair[0];Hold $path $pair[1]
        $null=[Reflection.Assembly]::LoadFrom($path);$hostRefs+=$path
    }
    Add-Type -Path $compiler -ReferencedAssemblies $hostRefs -CompilerOptions '/nowarn:1701'
    $null=New-Item -ItemType Directory -Path $OutputDirectory
    $binary=Join-Path $OutputDirectory 'Jint.dll'
    $check=[TemporalStartupCompiler]::Build($sources,$references,$key,$resource,$binary)
    $report=@{emitted=$check.Emitted;sourceCount=$check.SourceCount;diagnostics=$check.Diagnostics;binary=$binary;sha256=if(Test-Path -LiteralPath $binary){(Get-FileHash -LiteralPath $binary).Hash}else{$null};scope='Archive-extracted lazy interpreter rebuild; no equivalence/runtime/adoption claim'}
    [IO.File]::WriteAllText((Join-Path $OutputDirectory 'compile-result.json'),($report|ConvertTo-Json -Depth 5))
    if(!$check.Emitted -or $check.SourceCount -ne 699){throw 'Mapped compilation failed'}
    [pscustomobject]$report|Select-Object emitted,sourceCount,sha256|ConvertTo-Json
}finally{foreach($lease in $leases){$lease.Dispose()}}
