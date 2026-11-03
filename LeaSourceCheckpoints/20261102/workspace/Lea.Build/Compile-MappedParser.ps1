param([Parameter(Mandatory)][string]$Map,[Parameter(Mandatory)][string]$ArchiveRoot,[Parameter(Mandatory)][string]$OutputDirectory)
$ErrorActionPreference='Stop'
if((Get-FileHash -LiteralPath $Map).Hash -cne 'CA40B407F75DD28DA97CF4570CB4D52B6ACD66D01631D98FD576659E4D6054DF'){throw 'Parser manifest changed'}
$recipe=Get-Content -LiteralPath $Map -Raw|ConvertFrom-Json
$leases=[Collections.Generic.List[IDisposable]]::new()
function Hold([string]$Path,[string]$Hash){
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($stream)
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) -cne $Hash){throw 'Parser compiler input changed'}
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
    $references=@($recipe.references|ForEach-Object{Hold $_.path $_.sha256;$_.path})
    $resources=@($recipe.resources|ForEach-Object{ResolveArchived $_})
    if($sources.Count -ne 210 -or $references.Count -ne 163 -or $resources.Count -ne 3){throw 'Parser input coverage differs'}
    $compiler=ResolveArchived $recipe.compiler
    $key=ResolveArchived $recipe.extra.'src/Karambolo.Public.snk'
    $project=ResolveArchived $recipe.extra.'src/Acornima/Acornima.csproj'
    $hostRefs=@(Get-ChildItem (Join-Path $PSHOME 'ref') -Filter '*.dll'|ForEach-Object FullName)
    foreach($pair in @(@('Microsoft.CodeAnalysis.dll','1AB0A49C07AF467D16EC723C5D95DAE3F243AD2ECEA840312228ECDEA8D6E021'),@('Microsoft.CodeAnalysis.CSharp.dll','CE8DFED4EB61F1B6FECB6D06E3AADFC21B36C478FD863ED014BAD2CB01987DFD'))){
        $path=Join-Path $PSHOME $pair[0];Hold $path $pair[1];$null=[Reflection.Assembly]::LoadFrom($path);$hostRefs+=$path
    }
    Add-Type -Path $compiler -ReferencedAssemblies $hostRefs -CompilerOptions '/nowarn:1701'
    $null=New-Item -ItemType Directory -Path $OutputDirectory
    $binary=Join-Path $OutputDirectory 'Acornima.dll'
    $check=[ParserVariantCompiler]::Build($sources,$references,$resources,$key,$project,$binary,(Join-Path $OutputDirectory 'AssemblyInfo.cs'))
    $report=@{emitted=$check.Emitted;sourceCount=$check.SourceCount;resourceCount=$check.ResourceCount;resourceStringCount=$check.ResourceStringCount;diagnostics=$check.Diagnostics;binary=$binary;sha256=if(Test-Path -LiteralPath $binary){(Get-FileHash -LiteralPath $binary).Hash}else{$null};runtimeExecuted=$false}
    [IO.File]::WriteAllText((Join-Path $OutputDirectory 'compile-result.json'),($report|ConvertTo-Json -Depth 5))
    if(!$check.Emitted -or $check.SourceCount -ne 211 -or $check.ResourceCount -ne 3 -or $check.ResourceStringCount -ne 146){throw 'Parser emission incomplete'}
    [pscustomobject]$report|Select-Object emitted,sourceCount,resourceCount,resourceStringCount,sha256|ConvertTo-Json
}finally{foreach($lease in $leases){$lease.Dispose()}}
