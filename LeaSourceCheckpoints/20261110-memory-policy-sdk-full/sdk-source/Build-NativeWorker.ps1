#requires -Version 7.4
param(
    [Parameter(Mandatory)][Alias('tc')][string]$Toolchain,
    [Parameter(Mandatory)][Alias('th')][ValidatePattern('^[0-9A-Fa-f]{64}$')][string]$ToolchainSha256,
    [Parameter(Mandatory)][Alias('il')][string]$IlDirectory,
    [Parameter(Mandatory)][Alias('o')][string]$OutputDirectory
)
$ErrorActionPreference='Stop'
if(!$IsWindows -or [IntPtr]::Size -ne 8){throw 'Windows x64 PowerShell required'}
$leases=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new()
function Plain([string]$path){
    $full=[IO.Path]::GetFullPath($path)
    if($full.StartsWith('\\',[StringComparison]::Ordinal)){throw 'Local native build paths required'}
    $walk=$full
    while($walk){if((Test-Path -LiteralPath $walk) -and ((Get-Item -LiteralPath $walk -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)){throw 'Reparse native build path refused'};$walk=[IO.Path]::GetDirectoryName($walk)}
    return $full.TrimEnd('\')
}
function Hold([string]$path,[string]$expected){
    $full=Plain $path;$stream=[IO.File]::Open($full,'Open','Read','Read');$leases.Add($stream)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))
    if($expected -and $hash -cne $expected.ToUpperInvariant()){throw ('Native build input hash differs: '+$full)}
    $inputs.Add([pscustomobject]@{path=$full;sha256=$hash})
    return $hash
}
function Inventory([string]$directory,$members,[bool]$runtime){
    $seen=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    foreach($file in $members){
        if(!$file.path -or [IO.Path]::IsPathRooted($file.path) -or $file.path.Contains(':') -or $file.path.Split([char[]]@('/','\')) -contains '..' -or !$seen.Add($file.path) -or $file.sha256 -cnotmatch '^[0-9A-F]{64}$'){throw 'Toolchain member escape/duplicate/hash rejected'}
        if($runtime -and ([IO.Path]::GetFileName($file.path) -cne $file.path -or [IO.Path]::GetExtension($file.path) -cne '.dll')){throw 'Runtime references must be top-level DLLs'}
        $null=Hold (Join-Path $directory $file.path) $file.sha256
    }
    $actual=if($runtime){@(Get-ChildItem -LiteralPath $directory -File -Filter '*.dll')}else{@(Get-ChildItem -LiteralPath $directory -Recurse -File -Force)}
    if($actual.Count -ne $seen.Count){throw 'Toolchain inventory changed'}
    foreach($file in $actual){$null=Plain $file.FullName;if(!$seen.Contains([IO.Path]::GetRelativePath($directory,$file.FullName).Replace('\','/'))){throw 'Unrecorded toolchain member'}}
}
try{
    $manifest=Plain $Toolchain;$null=Hold $manifest $ToolchainSha256
    $config=Get-Content -LiteralPath $manifest -Raw|ConvertFrom-Json
    if($config.schemaVersion -ne 1 -or $config.compilerVersion -cne '8.0.32' -or $config.runtimeVersion -cne '8.0.33' -or $config.compilerFiles.Count -lt 1 -or $config.compilerFiles.Count -gt 128 -or $config.runtimeFiles.Count -lt 150 -or $config.runtimeFiles.Count -gt 256){throw 'Qualified toolchain schema/version/inventory required'}
    $compilerRoot=Plain $config.compilerRoot;$runtimeRoot=Plain $config.runtimeRoot
    Inventory $compilerRoot $config.compilerFiles $false;Inventory $runtimeRoot $config.runtimeFiles $true
    $compiler=Join-Path $compilerRoot 'crossgen2.exe'
    if(@($config.compilerFiles|Where-Object path -ceq 'crossgen2.exe').Count -ne 1){throw 'Recorded native compiler required'}
    $ilRoot=Plain $IlDirectory;$output=Plain $OutputDirectory
    if([IO.Path]::GetFileName($output) -cne 'ScriptWorker' -or $output -ieq $ilRoot -or $output.StartsWith($ilRoot+'\',[StringComparison]::OrdinalIgnoreCase) -or $ilRoot.StartsWith($output+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Separate explicit ScriptWorker output required'}
    $names=@('Lea.Script.Worker.exe','Lea.Script.Worker.dll','Lea.Script.Worker.runtimeconfig.json','Lea.Script.Worker.deps.json','Jint.dll','Acornima.dll')
    foreach($name in $names){$null=Hold (Join-Path $ilRoot $name)}
    $policy=Get-Content -LiteralPath (Join-Path $ilRoot 'Lea.Script.Worker.runtimeconfig.json') -Raw|ConvertFrom-Json
    if($policy.runtimeOptions.framework.name -cne 'Microsoft.NETCore.App' -or $policy.runtimeOptions.framework.version -cne $config.runtimeVersion -or $policy.runtimeOptions.rollForward -cne 'Disable'){throw 'Exact worker runtime policy required'}
    $null=Hold $PSCommandPath
    $tools=Join-Path $PSScriptRoot 'NativeWorker.Tools.cs';$toolsHash=Hold $tools
    if('Lea.Sdk.NativeWorkerTools' -as [type]){if($global:LeaNativeToolsSourceHash -cne $toolsHash){throw 'Use fresh PowerShell for changed native tools'}}else{Add-Type -Path $tools;$global:LeaNativeToolsSourceHash=$toolsHash}
    foreach($name in @('Jint.dll','Acornima.dll')){if(![Lea.Sdk.NativeWorkerTools]::IsX64ReadyToRun((Join-Path $ilRoot $name))){throw 'ReadyToRun interpreter dependencies required; no IL fallback'}}
    $ordered=@($inputs|Sort-Object path)
    $fingerprint=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes(($ordered|ConvertTo-Json -Depth 4 -Compress))))
    $receipt=$output+'.build.json'
    if(Test-Path -LiteralPath $output){
        $null=Plain $receipt
        if(!(Test-Path -LiteralPath $receipt)){throw 'Existing native output has no receipt; preserve it and choose fresh output'}
        $cached=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
        if(!$cached.succeeded -or $cached.fingerprint -cne $fingerprint -or $cached.outputs.Count -ne 6){throw 'Native inputs changed; preserve old output and choose a fresh ScriptWorker directory'}
        $members=@(Get-ChildItem -LiteralPath $output -Force)
        if($members.Count -ne 6 -or @($members|Where-Object PSIsContainer).Count){throw 'Existing native output inventory differs'}
        foreach($name in $names){$entry=@($cached.outputs|Where-Object name -ceq $name);if($entry.Count -ne 1){throw 'Cached output identity absent'};$null=Hold (Join-Path $output $name) $entry[0].sha256}
        foreach($name in @('Lea.Script.Worker.dll','Jint.dll','Acornima.dll')){if(![Lea.Sdk.NativeWorkerTools]::IsX64ReadyToRun((Join-Path $output $name))){throw 'Cached native image invalid'}}
        Write-Output 'LEA_NATIVE_UP_TO_DATE';return
    }
    if(Test-Path -LiteralPath $receipt){throw 'Preserve existing native build receipt'}
    $parent=Split-Path $output;New-Item -ItemType Directory -Path $parent -Force|Out-Null
    $work=Join-Path $parent ('.lea-native-'+[guid]::NewGuid().ToString('N'))
    $stage=Join-Path $work 'ScriptWorker';New-Item -ItemType Directory -Path $stage|Out-Null
    foreach($name in $names|Where-Object {$_ -cne 'Lea.Script.Worker.dll'}){Copy-Item -LiteralPath (Join-Path $ilRoot $name) -Destination (Join-Path $stage $name)}
    $options=@(('"'+(Join-Path $ilRoot 'Lea.Script.Worker.dll')+'"'),('-o:"'+(Join-Path $stage 'Lea.Script.Worker.dll')+'"'),'--targetos:windows','--targetarch:x64','-O')
    foreach($file in $config.runtimeFiles|Sort-Object path){$options+=('-r:"'+(Join-Path $runtimeRoot $file.path)+'"')}
    foreach($name in @('Jint.dll','Acornima.dll')){$options+=('-r:"'+(Join-Path $stage $name)+'"')}
    $rsp=Join-Path $work 'native.rsp';[IO.File]::WriteAllLines($rsp,$options,[Text.UTF8Encoding]::new($false))
    & $compiler ('@'+$rsp)
    if($LASTEXITCODE -ne 0){throw ('Native compiler failed: '+$LASTEXITCODE+'; retained '+$work)}
    foreach($name in @('Lea.Script.Worker.dll','Jint.dll','Acornima.dll')){if(![Lea.Sdk.NativeWorkerTools]::IsX64ReadyToRun((Join-Path $stage $name))){throw 'Native output verification failed; no IL fallback'}}
    foreach($input in $inputs){if((Get-FileHash -LiteralPath $input.path).Hash -cne $input.sha256){throw 'Native input changed during compilation'}}
    $products=@(foreach($name in $names){@{name=$name;sha256=(Get-FileHash -LiteralPath (Join-Path $stage $name)).Hash}})
    if((Get-ChildItem -LiteralPath $stage -Force).Count -ne 6){throw 'Exact six-file native closure required'}
    $stage=Plain $stage;$output=Plain $output
    if(!$stage.StartsWith((Plain $work)+'\',[StringComparison]::OrdinalIgnoreCase) -or [IO.Path]::GetDirectoryName($output) -cne (Plain $parent) -or (Test-Path -LiteralPath $output)){throw 'Native publication target changed'}
    Move-Item -LiteralPath $stage -Destination $output
    foreach($file in $products){if((Get-FileHash -LiteralPath (Join-Path $output $file.name)).Hash -cne $file.sha256){throw 'Published native output differs'}}
    @{succeeded=$true;fingerprint=$fingerprint;inputs=$ordered;outputs=$products;toolchainSha256=$ToolchainSha256.ToUpperInvariant();runtimeVersion=$config.runtimeVersion;compilerVersion=$config.compilerVersion;runtimeQualified=$false;scope='Native compilation and PE header readback only; requires isolated execution, lifecycle and host qualification.'}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath $receipt -Encoding utf8
    Write-Output 'LEA_NATIVE_BUILT'
}finally{foreach($lease in $leases){$lease.Dispose()}}
