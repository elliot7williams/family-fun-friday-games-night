param(
    [Parameter(Mandatory=$true)][string]$ResponseFile,
    [Parameter(Mandatory=$true)][string]$ExpectedResponseSha256
)
$ErrorActionPreference = 'Stop'
$directory = 'C:\Program Files\dotnet\sdk\8.0.425\Roslyn\bincore'
$identities = @{
    'csc.dll'='D2959AC91DD420E6F912823504A47CE0357A2F491887C6A7CFE61A3F3C067FA3'
    'Microsoft.CodeAnalysis.dll'='C67ECCA9CABD11D3BEC805982B0D2D8EC895A69873171FBCB4E0F8F96B45225D'
    'Microsoft.CodeAnalysis.CSharp.dll'='79E7665038ABE430A96AF11A72552ADDBA2BA0D750E50283EE874EE1BF0F9C33'
}
$leases = [Collections.Generic.List[IDisposable]]::new()
$context = $null
try {
    foreach ($name in $identities.Keys) {
        $stream = [IO.File]::Open((Join-Path $directory $name), 'Open', 'Read', 'Read')
        $leases.Add($stream)
        if ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) -cne $identities[$name]) {
            throw "Compiler identity differs: $name"
        }
    }
    $response = [IO.File]::Open([IO.Path]::GetFullPath($ResponseFile), 'Open', 'Read', 'Read')
    $leases.Add($response)
    if ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($response)) -cne $ExpectedResponseSha256) {
        throw 'Response identity differs.'
    }
    $context = [Runtime.Loader.AssemblyLoadContext]::new('LeaNativeCompiler', $true)
    foreach ($name in @('Microsoft.CodeAnalysis.dll','Microsoft.CodeAnalysis.CSharp.dll')) {
        [void]$context.LoadFromAssemblyPath((Join-Path $directory $name))
    }
    $entry = $context.LoadFromAssemblyPath((Join-Path $directory 'csc.dll')).EntryPoint
    $arguments = [object[]]::new(1)
    $arguments[0] = [string[]]@('/noconfig', ('@' + [IO.Path]::GetFullPath($ResponseFile)))
    exit ([int]$entry.Invoke($null, $arguments))
} catch {
    [Console]::Error.WriteLine($_.Exception.ToString())
    exit 1
} finally {
    foreach ($lease in $leases) { $lease.Dispose() }
    if ($context) { $context.Unload() }
}
