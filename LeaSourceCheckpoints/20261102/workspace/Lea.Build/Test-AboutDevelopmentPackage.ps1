param([switch]$KeepForegroundHost,[switch]$LibraryHost,[ValidatePattern('^[A-F0-9]{64}$')][string]$ExpectedHostReceipt,[ValidatePattern('^[A-F0-9]{64}$')][string]$ExpectedArchiveSha256)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
$outRoot=Join-Path $workspace $(if($LibraryHost){'continuity-evidence/library-package-extraction-v1'}else{'continuity-evidence/about-package-extraction-v1'})
if($LibraryHost -and (!$ExpectedHostReceipt -or !$ExpectedArchiveSha256)){throw 'Library extraction requires exact campaign and archive pins'}
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(Test-Path $outRoot){throw 'Preserve package test'}
$id=[Security.Principal.WindowsIdentity]::GetCurrent()
if(([Security.Principal.WindowsPrincipal]::new($id)).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Normal nonadmin required'}
$utility=Join-Path $workspace 'continuity-evidence/native-launch-timing-v1/Test-CompositeCandidate.v2.ps1'
if((Get-FileHash $utility).Hash -cne 'EB00AD17B8078A6CC2F418733BA5D675FC84EF49EED433A527FAC5498DD5ECEF'){throw 'Utility differs'}
$t=$null;$e=$null;$ast=[Management.Automation.Language.Parser]::ParseFile($utility,[ref]$t,[ref]$e)
foreach($name in @('Space','Pin','NewOutputReader','PumpOutput','Run')){
    $defs=@($ast.FindAll({param($n)$n -is [Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -ceq $name},$false))
    if($e.Count -or $defs.Count -ne 1){throw 'Utility shape differs'}
    . ([scriptblock]::Create($defs[0].Extent.Text))
}
if(Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue){throw 'Fresh owner required'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new()
$result=[ordered]@{passed=$false;completed=$false;error=$null;runs=$runs;inputs=$inputs;runtime=Join-Path $outRoot 'runtime';scope='Fresh ZIP extraction settings/navigation/actual clean-EXE smoke on current machine; not full extraction campaign, clean-PC install or full scripting'}
Space;$null=New-Item -ItemType Directory $outRoot
try{
    $null=Pin $PSCommandPath;Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $outRoot 'runner.ps1')
    if(@(Get-Process 'Firespider*','Lea.Firespider.Boot','Lea.Script.Worker','Lea.Script.Tests' -ErrorAction SilentlyContinue).Count){throw 'No overlapping browser/tests'}
    $receipt=Join-Path $workspace $(if($LibraryHost){'continuity-evidence/temporal-startup-v1/host-library-current-native-v1/result.json'}else{'continuity-evidence/temporal-startup-v1/host-about-current-native-v1/result.json'})
    $null=Pin $receipt $(if($LibraryHost){$ExpectedHostReceipt}else{'9D82D7C17CA855462830B57F0723BF6AE8FC9E8312FDE4130C783E505B88C5EA'})
    $hostProof=Get-Content $receipt -Raw|ConvertFrom-Json
    if(!$hostProof.passed -or $hostProof.ownerAttention){throw 'Host proof incomplete'}
    $archive=Join-Path $workspace $(if($LibraryHost){'dist/LeaExes/Firespider-Lea-library-development-20261026.zip'}else{'dist/LeaExes/Firespider-Lea-development-20261026.zip'})
    $null=Pin $archive $(if($LibraryHost){$ExpectedArchiveSha256}else{'A3B64730267912591E099D0A85DF95D01A49149CE89AEF12579FBF4F77C7349B'})
    $leases.Add([IO.File]::Open($archive,'Open','Read','Read'))
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{
        $names=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
        foreach($entry in $zip.Entries){$relative=$entry.FullName.Replace('/','\');if([IO.Path]::IsPathRooted($relative) -or $relative.Contains(':') -or $relative.Split('\') -contains '..' -or !$names.Add($relative)){throw 'Unsafe ZIP entry'}}
    }finally{$zip.Dispose()}
    [IO.Compression.ZipFile]::ExtractToDirectory($archive,$result.runtime)
    if(@(Get-ChildItem $result.runtime -Recurse -File).Count -ne 400){throw 'Expected398 runtime +2 generated package files'}
    foreach($file in $hostProof.before){
        $relative=[IO.Path]::GetRelativePath($hostProof.candidate,$file.path)
        if([IO.Path]::IsPathRooted($relative) -or $relative.Split('\') -contains '..'){throw 'Host receipt path escape'}
        $null=Pin (Join-Path $result.runtime $relative) $file.sha256
    }
    $policy=Join-Path $PSScriptRoot 'Test-DecoderAclDelta.ps1';$null=Pin $policy '15F46DBC7F4D4AD5B816DDFA1FE7213D02CAE1EDE79844AFBD904F281299E71E';. $policy
    $result.before=@(Get-ChildItem $result.runtime -Recurse -File|ForEach-Object{@{path=$_.FullName;sha256=(Get-FileHash $_.FullName).Hash;sddl=(Get-Acl $_.FullName).Sddl}})
    $boot=Join-Path $result.runtime 'Lea.Firespider.Boot.exe'
    $settings=Run 'settings-path' $boot @('--settings-path') 30
    if(!$settings.passed){throw 'Extracted settings-path check failed'}
    $navigation=Run 'navigation' $boot @(('"'+$result.runtime+'"'),'navigation') 90
    if(!$navigation.passed -or @(Get-Content $navigation.stdout|Where-Object{$_.StartsWith('PASS Lea boot: navigation;')}).Count -ne 1){throw 'Extracted navigation failed'}
    $startupScript=Join-Path $workspace 'continuity-evidence/small-engine-v1/source/Lea.Build/Test-FirespiderStartup.ps1'
    $launcher=Join-Path $PSScriptRoot 'Invoke-StartupBody.ps1'
    foreach($path in @($startupScript,$launcher)){
        $hashes=@($hostProof.inputFiles|Where-Object path -ceq $path|ForEach-Object sha256|Sort-Object -Unique)
        if($hashes.Count -ne 1){throw 'Missing startup provenance'}
        $null=Pin $path $hashes[0]
    }
    $startupHash=(Get-FileHash $startupScript).Hash
    $command="& {`n"+[IO.File]::ReadAllText($launcher)+"`n} -TestScript '"+$startupScript.Replace("'","''")+"' -ExpectedSha256 '"+$startupHash+"' -Root '"+$result.runtime.Replace("'","''")+"' -Scenario 'clean' -Evidence '"+$outRoot.Replace("'","''")+"'"
    $encoded=[Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($command))
    $clean=Run 'exe-clean' (Join-Path $env:WINDIR 'System32/WindowsPowerShell/v1.0/powershell.exe') @('-NoProfile','-STA','-EncodedCommand',$encoded) 100
    if(!$clean.passed){throw 'Extracted actual clean EXE startup failed'}
    $result.completed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object{(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.bytesUnchanged=@($result.before|Where-Object{(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $changes=@($result.before|Where-Object{(Get-Acl $_.path).Sddl -cne $_.sddl})
    $result.aclDeltaPaths=@($changes|ForEach-Object path)
    $result.decoderPolicyAccepted=$changes.Count -eq 0
    if($changes.Count -eq 1){$result.decoderPolicyAccepted=Test-DecoderAclDelta -Path $changes[0].path -ExpectedDecoderPath (Join-Path $result.runtime 'Lea.Image.exe') -Before $changes[0].sddl -After (Get-Acl $changes[0].path).Sddl -ProfileName 'Lea.Image.Decoder'}
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -gt 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.completed -and $runs.Count -eq 3 -and $result.inputsUnchanged -and $result.bytesUnchanged -and $result.decoderPolicyAccepted -and !$result.ownerAttention
    if($result.ownerAttention){$global:LeaPackageInputLeases=$leases}else{foreach($s in $leases){$s.Dispose()}}
    $result|ConvertTo-Json -Depth 8|Set-Content (Join-Path $outRoot 'result.json')
    [pscustomobject]$result|Select-Object completed,passed,inputsUnchanged,bytesUnchanged,decoderPolicyAccepted,ownerAttention,error|ConvertTo-Json -Compress
}
