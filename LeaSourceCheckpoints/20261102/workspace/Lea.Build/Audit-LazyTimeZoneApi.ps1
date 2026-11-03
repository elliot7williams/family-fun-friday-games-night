$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/temporal-startup-v1'
$out=Join-Path $root 'api-audit.json'
if(Test-Path $out){throw 'Preserve API audit'}
$buildPath=Join-Path $root 'build-v1/result.json'
if((Get-FileHash $buildPath).Hash -cne '7D82A029AA5977798CF8B323920D606E21DFE4D803243914378EC1CAD8761D29'){throw 'Build differs'}
$build=Get-Content $buildPath -Raw|ConvertFrom-Json
if(!$build.passed -or !$build.inputsUnchanged -or $build.runs.Count -ne 2){throw 'Build incomplete'}
$helper=Join-Path $workspace 'continuity-evidence/modern-worker-v1/InterpreterAssemblyAudit.cs'
$helperHash=(Get-FileHash $helper).Hash
$refs=@(Get-ChildItem (Join-Path $PSHOME 'ref') -Filter '*.dll' -File|ForEach-Object FullName)
Add-Type -Path $helper -ReferencedAssemblies $refs -CompilerOptions '/nowarn:1701'
$leases=[Collections.Generic.List[IDisposable]]::new()
$snapshots=@()
try{
    foreach($name in @('control','lazy')){
        $row=@($build.runs|Where-Object name -ceq $name)
        if($row.Count -ne 1){throw 'Variant missing'}
        $leases.Add([IO.File]::Open($row[0].binary,'Open','Read','Read'))
        if((Get-FileHash $row[0].binary).Hash -cne $row[0].sha256){throw 'Binary differs'}
        $snapshots+=,[InterpreterAssemblyAudit]::Read($row[0].binary)
    }
    $a=$snapshots[0];$b=$snapshots[1]
    foreach($property in @('Api','Resources','AllMethods','AssemblyReferences')){
        if($a.$property.Count -ne $b.$property.Count -or [string]::Join("`n",$a.$property) -cne [string]::Join("`n",$b.$property)){throw ('Contract differs: '+$property)}
    }
    if($a.Identity -cne $b.Identity -or $a.MethodCount -ne $b.MethodCount -or $a.AggressiveOptimizationCount -ne $b.AggressiveOptimizationCount){throw 'Identity/method policy differs'}
    if((Get-FileHash $helper).Hash -cne $helperHash){throw 'Auditor changed'}
    @{passed=$true;helperSha256=$helperHash;identity=$a.Identity;apiEntries=$a.Api.Count;
      methodCount=$a.MethodCount;resourceEntries=$a.Resources.Count;assemblyReferences=$a.AssemblyReferences.Count;
      optimizationCount=$a.AggressiveOptimizationCount;resources=$a.Resources;
      publicContractIdentical=$true;allMethodSignaturesIdentical=$true;methodBodiesVerified=$false;
      workerExecuted=$false;scope='Control/candidate public API/resources/references/signatures; not complete IL or native/runtime equivalence. Public signing is not vendor authentication.'}|ConvertTo-Json -Depth 5|Set-Content $out
}finally{foreach($lease in $leases){$lease.Dispose()}}
Get-Content $out
