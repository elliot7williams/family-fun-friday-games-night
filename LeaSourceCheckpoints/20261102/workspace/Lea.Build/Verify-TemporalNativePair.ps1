$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/temporal-startup-v1'
$out=Join-Path $root 'native-pair-v2/literal-input-verification.json'
if(Test-Path -LiteralPath $out){throw 'Preserve verification evidence'}
if((Get-PSDrive C).Free -lt 512MB){throw 'Working reserve required'}
$path=Join-Path $root 'native-pair-v2/result.json'
if((Get-FileHash -LiteralPath $path).Hash -cne '20835577F4B1470E96F63C91553F2F9D1C5F12F20EEA10EBD16167F53779DB92'){throw 'Native receipt changed'}
$build=Get-Content -LiteralPath $path -Raw|ConvertFrom-Json
if($build.passed -or $build.inputsUnchanged -or $build.error -or $build.ownerAttention -or $build.runs.Count -ne 2 -or $build.variants.Count -ne 2){throw 'Unexpected retained failure shape'}
$leases=[Collections.Generic.List[IDisposable]]::new()
$verified=[Collections.Generic.List[object]]::new()
function Hold([string]$path,[string]$expected){
    $stream=[IO.File]::Open([IO.Path]::GetFullPath($path),'Open','Read','Read');$leases.Add($stream)
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) -cne $expected){throw ('Changed input: '+$path)}
    $verified.Add(@{path=$path;sha256=$expected})
}
try{
    foreach($input in $build.inputs){Hold $input.path $input.sha256}
    foreach($run in $build.runs){if(!$run.passed -or $run.exitCode -ne 0 -or $run.killRequested -or $run.retainedForOwner -or !$run.stdoutComplete -or !$run.stderrComplete){throw 'Unclean compiler run'}}
    $ilHelper=Join-Path $workspace 'continuity-evidence/bootstrap-descriptor-batches-v1/ParentResourceIlAudit.cs'
    Hold $ilHelper 'B2438457F0CA2F9F54A9EB866335DF7EF23D50FB84F134F13B968C4FB6F557CC'
    $apiHelper=Join-Path $workspace 'continuity-evidence/modern-worker-v1/InterpreterAssemblyAudit.cs'
    Hold $apiHelper '372098D022AF9E29B14CFF5AE57574F101B333AFFE4F94019720D315E443DAB9'
    Add-Type -Path @($ilHelper,$apiHelper)
    $results=@(foreach($variant in $build.variants){
        Hold $variant.input $variant.inputSha256;Hold $variant.binary $variant.sha256
        foreach($map in $variant.maps){Hold $map.path $map.sha256}
        if($variant.maps.Count -ne 2){throw 'Missing maps'}
        $il=[ParentResourceIlAudit]::Compare($variant.input,$variant.binary)
        $a=[InterpreterAssemblyAudit]::Read($variant.input);$b=[InterpreterAssemblyAudit]::Read($variant.binary)
        foreach($property in @('Api','Resources','AllMethods','AssemblyReferences','OptimizationMethods')){
            if(@(Compare-Object -ReferenceObject $a.$property -DifferenceObject $b.$property -CaseSensitive).Count){throw ('Native metadata/resource mismatch: '+$property)}
        }
        if($a.Identity -cne $b.Identity){throw 'Assembly identity changed'}
        @{name=$variant.name;binary=$variant.binary;sha256=$variant.sha256;ilComparison=$il;apiResourcesReferencesIdentical=$true}
    })
    $result=@{passed=$true;originalBuildReceiptStillFailed=$true;reason='Literal-path revalidation of unchanged inputs after original wildcard hash error';verified=$verified;variants=$results;workerExecuted=$false;startupPassed=$false;productionChanged=$false}
    if((Get-PSDrive C).Free -lt 512MB){throw 'Reserve lost'}
    [IO.File]::WriteAllText($out,($result|ConvertTo-Json -Depth 10),[Text.UTF8Encoding]::new($false))
    $saved=Get-Content -LiteralPath $out -Raw|ConvertFrom-Json
    if(!$saved.passed -or $saved.variants.Count -ne 2){throw 'Receipt readback failed'}
    [pscustomobject]@{passed=$saved.passed;verifiedFiles=$saved.verified.Count;variants=$results;receipt=(Get-FileHash -LiteralPath $out).Hash}|ConvertTo-Json -Depth 7
}finally{foreach($lease in $leases){$lease.Dispose()}}
