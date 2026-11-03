param(
    [Parameter(Mandatory=$true)][string]$ResponseFile,
    [Parameter(Mandatory=$true)][ValidatePattern('^[A-F0-9]{64}$')][string]$ExpectedResponseSha256,
    [Parameter(Mandatory=$true)][ValidatePattern('^[A-F0-9]{64}$')][string]$ExpectedSourceSha256
)
$ErrorActionPreference='Stop'
$directory='C:\Program Files\PowerShell\7'
$identities=@{
 'Microsoft.CodeAnalysis.CSharp.dll'='9DA18199A80C5892FE84F67F4FCE159A06259EBF9D3B73DF036B28249DBBB31F'
 'Microsoft.CodeAnalysis.dll'='0C5159274EF1790BEEF54DD796B51933D2B9C05A5568F968ACCF32A3CBF6D0C0'
}
$leases=[Collections.Generic.List[IDisposable]]::new()
function Lease([string]$path,[string]$expected){
 $file=[IO.File]::Open([IO.Path]::GetFullPath($path),'Open','Read','Read')
 $leases.Add($file)
 if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($file))-cne$expected){throw ('Compiler input identity differs: '+$path)}
}
try{
 foreach($name in $identities.Keys){Lease (Join-Path $directory $name) $identities[$name]}
 $response=[IO.Path]::GetFullPath($ResponseFile)
 Lease $response $ExpectedResponseSha256
 $source=Join-Path $PSScriptRoot 'PublicCompiler.cs'
 Lease $source $ExpectedSourceSha256
 foreach($name in @('Microsoft.CodeAnalysis.dll','Microsoft.CodeAnalysis.CSharp.dll')){
  $assembly=[Reflection.Assembly]::LoadFrom((Join-Path $directory $name))
  if($assembly.Location-cne(Join-Path $directory $name)){throw 'Unexpected loaded compiler assembly.'}
 }
 # Explicit references replace Add-Type's default set; include the host reference pack.
 $hostReferences=@(Get-ChildItem (Join-Path $directory 'ref') -File -Filter '*.dll'|ForEach-Object FullName)
 $hostReferences+=@((Join-Path $directory 'Microsoft.CodeAnalysis.dll'),(Join-Path $directory 'Microsoft.CodeAnalysis.CSharp.dll'))
 # Host-only assembly unification; the target compilation uses its own untouched options.
 Add-Type -Path $source -CompilerOptions '/nowarn:1701' -ReferencedAssemblies $hostReferences
 exit ([Lea.Build.PublicCompiler]::Run($response))
}catch{[Console]::Error.WriteLine($_.Exception.ToString());exit 1}
finally{foreach($lease in $leases){$lease.Dispose()}}
