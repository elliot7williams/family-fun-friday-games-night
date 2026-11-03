param(
    [Parameter(Mandatory=$true)][string]$ResponseFile,
    [Parameter(Mandatory=$true)][ValidatePattern('^[A-F0-9]{64}$')][string]$ExpectedResponseSha256,
    [Parameter(Mandatory=$true)][ValidatePattern('^[A-F0-9]{64}$')][string]$ExpectedSourceSha256
)
$ErrorActionPreference='Stop'
$directory='C:\Program Files\PowerShell\7'
$identities=@{
 'Microsoft.CodeAnalysis.CSharp.dll'='5E9323E98D99F669ACD10705C40D985DBABEAF0984285ED32922F91DF223124B'
 'Microsoft.CodeAnalysis.dll'='E7A25CBDEC95390E9672659872189DA368032625D784C25569617617932C145F'
}
$leases=[Collections.Generic.List[IDisposable]]::new()
function Lease([string]$path,[string]$expected){
 $file=[IO.File]::Open([IO.Path]::GetFullPath($path),'Open','Read','Read')
 $leases.Add($file)
 if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($file))-cne$expected){throw ('Compiler input identity differs: '+$path)}
}
try{
 foreach($name in $identities.Keys){
  $path=Join-Path $directory $name
  Lease $path $identities[$name]
  $signature=Get-AuthenticodeSignature -LiteralPath $path
  if($signature.Status -ne 'Valid' -or $signature.SignerCertificate.Subject -cne 'CN=Microsoft 3rd Party Application Component, O=Microsoft Corporation, L=Redmond, S=Washington, C=US'){
   throw 'Compiler Microsoft signature differs.'
  }
 }
 $response=[IO.Path]::GetFullPath($ResponseFile)
 Lease $response $ExpectedResponseSha256
 $source=Join-Path $PSScriptRoot 'PublicCompiler.cs'
 Lease $source $ExpectedSourceSha256
 foreach($name in @('Microsoft.CodeAnalysis.dll','Microsoft.CodeAnalysis.CSharp.dll')){
  $assembly=[Reflection.Assembly]::LoadFrom((Join-Path $directory $name))
  if($assembly.Location-cne(Join-Path $directory $name)){throw 'Unexpected loaded compiler assembly.'}
 }
 $hostReferences=@(Get-ChildItem (Join-Path $directory 'ref') -File -Filter '*.dll'|ForEach-Object FullName)
 $hostReferences+=@((Join-Path $directory 'Microsoft.CodeAnalysis.dll'),(Join-Path $directory 'Microsoft.CodeAnalysis.CSharp.dll'))
 # Host-only unification; response-file target options remain untouched.
 Add-Type -Path $source -CompilerOptions '/nowarn:1701' -ReferencedAssemblies $hostReferences
 exit ([Lea.Build.PublicCompiler]::Run($response))
}catch{[Console]::Error.WriteLine($_.Exception.ToString());exit 1}
finally{foreach($lease in $leases){$lease.Dispose()}}
