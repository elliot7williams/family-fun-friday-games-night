param(
    [Parameter(Mandatory=$true)][string]$ResponseFile,
    [Parameter(Mandatory=$true)][ValidatePattern('^[A-F0-9]{64}$')][string]$ExpectedResponseSha256,
    [Parameter(Mandatory=$true)][ValidatePattern('^[A-F0-9]{64}$')][string]$ExpectedSourceSha256
)
$ErrorActionPreference='Stop'
$directory='C:\Program Files\PowerShell\7'
$identities=@{
 'Microsoft.CodeAnalysis.CSharp.dll'='CE8DFED4EB61F1B6FECB6D06E3AADFC21B36C478FD863ED014BAD2CB01987DFD'
 'Microsoft.CodeAnalysis.dll'='1AB0A49C07AF467D16EC723C5D95DAE3F243AD2ECEA840312228ECDEA8D6E021'
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
