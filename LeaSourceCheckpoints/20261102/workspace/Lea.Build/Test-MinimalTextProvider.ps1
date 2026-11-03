param([switch]$KeepForegroundHost,[ValidateRange(1,9)][int]$Version=1,[switch]$ManagedOnly,[switch]$CrossProcess,[switch]$PairControl,[switch]$LeaProvider,[switch]$TypedArrays,[switch]$NativeArrays,[switch]$ColdLegacy,[switch]$WpfBaseline,[switch]$ClientX86,[switch]$OriginalVariant,[switch]$NativeClient,[switch]$NativeEdit)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent;$out=Join-Path $root ('continuity-evidence/minimal-text-provider-v'+$Version)
if($ManagedOnly){$out=Join-Path $root ('continuity-evidence/minimal-managed-provider-v'+$Version)}
if($CrossProcess){if($ManagedOnly){throw 'Cross-process mode already uses stock managed client'};$out=Join-Path $root ('continuity-evidence/minimal-cross-process-provider-v'+$Version)}
if($PairControl){if(!$CrossProcess){throw 'Pair control requires process pair'};$out=Join-Path $root ('continuity-evidence/minimal-pair-control-v'+$Version)}
if($LeaProvider){if(!$CrossProcess -or $PairControl){throw 'Lea provider requires full process pair'};$out=Join-Path $root ('continuity-evidence/lea-cross-process-provider-v'+$Version)}
if($TypedArrays){if(!$CrossProcess -or $LeaProvider -or $PairControl){throw 'Typed-array comparison requires independent provider pair'};$out=Join-Path $root ('continuity-evidence/minimal-typed-array-provider-v'+$Version)}
if($NativeArrays){if(!$TypedArrays){throw 'Native arrays require typed-array comparison'};$out=Join-Path $root ('continuity-evidence/minimal-native-array-provider-v'+$Version)}
if($ColdLegacy){if(!$CrossProcess -or $PairControl -or $LeaProvider -or $TypedArrays){throw 'Cold comparison requires independent SDK provider pair'};$out=Join-Path $root ('continuity-evidence/minimal-cold-legacy-provider-v'+$Version)}
if($WpfBaseline){if(!$ColdLegacy){throw 'WPF baseline requires cold legacy comparison'};$out=Join-Path $root ('continuity-evidence/wpf-cold-legacy-provider-v'+$Version)}
if($ClientX86){if(!$WpfBaseline){throw 'x86 client requires WPF baseline comparison'};$out=Join-Path $root ('continuity-evidence/wpf-cold-legacy-x86-client-v'+$Version)}
if($OriginalVariant){if(!$ColdLegacy -or $WpfBaseline -or $ClientX86){throw 'Original VARIANT requires independent cold provider'};$out=Join-Path $root ('continuity-evidence/minimal-original-variant-provider-v'+$Version)}
if($NativeClient){if(!$ColdLegacy -or $WpfBaseline -or $OriginalVariant){throw 'Native client requires independent cold provider'};$out=Join-Path $root ('continuity-evidence/minimal-native-legacy-client-v'+$Version)}
if($NativeEdit){if(!$NativeClient){throw 'Native EDIT requires native client'};$out=Join-Path $root ('continuity-evidence/native-edit-legacy-client-v'+$Version)}
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin owner required'}
if((Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner required'}
if((Get-PSDrive C).Free -lt 600MB){throw 'Storage reserve required'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Firespider.Boot','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$Path,[string]$Hash){
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$holds.Add($stream)
    $actual=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))
    if($Hash -and $Hash -cne $actual){throw 'Pinned input differs'};$inputs.Add(@{path=$Path;sha256=$actual})
}
$null=New-Item -ItemType Directory -Path $out
$result=@{passed=$false;inputs=$inputs;runs=$runs;leaAssemblyLoaded=$null;runtimeAssemblyAuditPassed=$false;productionChanged=$false;fullReleaseQualified=$false}
try{
    Hold $PSCommandPath;Copy-Item $PSCommandPath (Join-Path $out 'runner.ps1')
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA'
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00'
    Add-Type -Path $owned;. $helper
    $compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe';Hold $compiler
    $exe=Join-Path $out 'MinimalTextProvider.exe'
    $arguments=@('/nologo','/noconfig','/nostdlib+','/target:exe','/platform:x64','/warnaserror+',('/out:"'+$exe+'"'))
    if($ClientX86){$arguments=@($arguments|Where-Object {$_ -ne '/platform:x64'})+'/platform:x86';$result.clientX86=$true}
    if($TypedArrays){$arguments+='/define:TYPED_TEXT_ARRAYS';$result.typedArrayProbe=$true}
    if($NativeArrays){$arguments+='/define:NATIVE_TEXT_ARRAYS';$result.nativeArrayProbe=$true}
    if($ColdLegacy){$arguments+='/define:COLD_LEGACY_CLIENT';$result.coldLegacyProbe=$true}
    if($WpfBaseline){$arguments+='/define:WPF_BASELINE_CLIENT';$result.wpfBaseline=$true}
    if($OriginalVariant){$arguments+='/define:ORIGINAL_VARIANT_CLIENT';$result.originalVariantProbe=$true}
    $refs='C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8'
    foreach($name in @('mscorlib.dll','System.dll','System.Core.dll','System.Drawing.dll','System.Windows.Forms.dll','WindowsBase.dll','UIAutomationClient.dll','UIAutomationProvider.dll','UIAutomationTypes.dll')){
        $path=Join-Path $refs $name;Hold $path;$arguments+=('/reference:"'+$path+'"')
    }
    foreach($name in @('MinimalTextProviderMain.cs','MinimalTextProviderPairModes.cs','ProviderArrayMetadata.cs','UiaComponentInventory.cs','DocumentLegacyHandleChecks.cs','DocumentNativeClientChecks.cs')){
        $path=Join-Path $root ('Lea.Tests/'+$name);Hold $path
        $to=Join-Path $out $name;Copy-Item $path $to;Hold $to;$arguments+=('"'+$to+'"')
    }
    if($TypedArrays){
        $path=Join-Path $root 'Lea.Tests/TypedTextProviderContract.cs';Hold $path
        $to=Join-Path $out 'TypedTextProviderContract.cs';Copy-Item $path $to;Hold $to;$arguments+=('"'+$to+'"')
    }
    if($NativeArrays){
        $path=Join-Path $root 'Lea.Tests/NativeProviderArray.cs';Hold $path
        $to=Join-Path $out 'NativeProviderArray.cs';Copy-Item $path $to;Hold $to;$arguments+=('"'+$to+'"')
    }
    $compiled=Invoke-OwnedTestRun -Name 'compile-minimal-provider' -Application $compiler -Arguments $arguments -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$compiled.passed){throw 'Minimal provider compilation failed'}
    Hold $exe
    $serverExe=''
    if($WpfBaseline){
        $serverExe=Join-Path $out 'WpfTextProviderServer.exe'
        $serverArguments=@('/nologo','/noconfig','/nostdlib+','/target:exe','/platform:x64','/warnaserror+',('/out:"'+$serverExe+'"'))
        $serverArguments+=@($arguments|Where-Object {$_.StartsWith('/reference:')})
        foreach($name in @('PresentationCore.dll','PresentationFramework.dll','System.Xaml.dll')){
            $path=Join-Path $refs $name;Hold $path;$serverArguments+=('/reference:"'+$path+'"')
        }
        $path=Join-Path $root 'Lea.Tests/WpfTextProviderServerMain.cs';Hold $path
        $to=Join-Path $out 'WpfTextProviderServerMain.cs';Copy-Item $path $to;Hold $to;$serverArguments+=('"'+$to+'"')
        $build=Invoke-OwnedTestRun -Name 'compile-wpf-server' -Application $compiler -Arguments $serverArguments -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
        if(!$build.passed){throw 'WPF provider compilation failed'}
        Hold $serverExe
    }
    if($LeaProvider){
        $qualification=Join-Path $root 'continuity-evidence/temporal-startup-v1/shutdown-script-owned-run-v1/result.json'
        Hold $qualification '5F4C37A3C7858E1C8EB95E57147A82FCED3D19D8AAE9E5C97EA1C0C29F39C5E0'
        $qualified=Get-Content $qualification -Raw|ConvertFrom-Json
        if(!$qualified.passed -or !$qualified.inputsUnchanged -or $qualified.ownerAttention -or $qualified.changedFiles.Count){throw 'Exact Lea qualification incomplete'}
        $runtime=Join-Path $root 'continuity-evidence/temporal-startup-v1/shutdown-script-suite-v1/runtime'
        $serverArguments=@('/nologo','/noconfig','/nostdlib+','/target:exe','/platform:x64','/warnaserror+')
        $serverExe=Join-Path $out 'LeaTextProviderServer.exe';$serverArguments+=('/out:"'+$serverExe+'"')
        $serverArguments+=@($arguments|Where-Object {$_.StartsWith('/reference:')})
        foreach($binding in @(@('Lea.Win.dll','2225EC83D0DB503FE7778F896E7FC41475F5B50C690FE89B4FE9604B46E856CA'),@('Lea.Engine.dll','A85B02C678862D75FA764397C6BB6B3D7B964ECB0C8E13722F6D17DC44AA3791'))){
            $path=Join-Path $runtime $binding[0];Hold $path $binding[1]
            $to=Join-Path $out $binding[0];Copy-Item $path $to;Hold $to $binding[1];$serverArguments+=('/reference:"'+$to+'"')
        }
        $serverSource=Join-Path $root 'Lea.Tests/LeaTextProviderServerMain.cs';Hold $serverSource
        $serverCopy=Join-Path $out 'LeaTextProviderServerMain.cs';Copy-Item $serverSource $serverCopy;Hold $serverCopy;$serverArguments+=('"'+$serverCopy+'"')
        $serverBuild=Invoke-OwnedTestRun -Name 'compile-lea-server' -Application $compiler -Arguments $serverArguments -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
        if(!$serverBuild.passed){throw 'Lea provider server compilation failed'}
        Hold $serverExe;$result.exactLeaProvider=$true;$result.currentFullNativeQualified=$false
    }
    if($CrossProcess){
        if($NativeClient){$builder=Join-Path $PSScriptRoot 'Build-NativeLegacyUiaClient.ps1';Hold $builder;. $builder;$result.nativeClient=$true}
        $pairHelper=Join-Path $PSScriptRoot 'Invoke-UiaProcessPair.ps1';Hold $pairHelper;. $pairHelper
        $result.pair=Invoke-UiaProcessPair $exe $out $runs ([bool]$PairControl) $serverExe ([bool]$LeaProvider)
        $clientLines=@(Get-Content (Join-Path $out 'pair-client.stdout.txt'))
        $result.runtimeAssemblyAuditPassed=@($clientLines|Where-Object {$_ -ceq 'MINIMAL_UIA no-Lea assembly audit passed'}).Count -eq 1
        if($result.runtimeAssemblyAuditPassed){$result.leaAssemblyLoaded=$false}
        $pairFooter=if($LeaProvider){'MINIMAL_UIA PASS 12'}else{'MINIMAL_UIA PASS 9'}
        if($ColdLegacy){$pairFooter='MINIMAL_UIA PASS 4'}
        if($OriginalVariant){$pairFooter='MINIMAL_UIA PASS 7'}
        $result.passed=$result.pair.passed -and $result.runtimeAssemblyAuditPassed -and @($clientLines|Where-Object {$_ -ceq $pairFooter}).Count -eq 1
        if($PairControl){$result.pairControlOnly=$true;$result.passed=$result.pair.passed -and @($clientLines|Where-Object {$_ -ceq 'MINIMAL_UIA PAIR_CONTROL_PASS'}).Count -eq 1}
        if($NativeClient){$result.runtimeAssemblyAuditPassed=$false;$result.nativeModuleAuditPassed=@($clientLines|Where-Object {$_ -ceq 'NATIVE_LEGACY no CLR or Lea modules before visible read'}).Count -eq 1;$result.passed=$result.pair.passed -and $result.nativeModuleAuditPassed -and @($clientLines|Where-Object {$_ -ceq 'NATIVE_LEGACY PASS 4'}).Count -eq 1}
        $result.customLegacyProbeBypassed=!$OriginalVariant
    }else{
    $testArguments=@()
    if($ManagedOnly){$testArguments=@('--managed-only')}
    $tested=Invoke-OwnedTestRun -Name 'minimal-provider' -Application $exe -Arguments $testArguments -WorkingDirectory $out -OutputDirectory $out -Seconds 30 -Records $runs
    $footer=if($ManagedOnly){'MINIMAL_UIA PASS 9'}else{'MINIMAL_UIA PASS 12'}
    $outputLines=@(Get-Content $tested.stdout)
    $result.runtimeAssemblyAuditPassed=@($outputLines|Where-Object {$_ -ceq 'MINIMAL_UIA no-Lea assembly audit passed'}).Count -eq 1
    if($result.runtimeAssemblyAuditPassed){$result.leaAssemblyLoaded=$false}
    $result.passed=$tested.passed -and $result.runtimeAssemblyAuditPassed -and @($outputLines|Where-Object {$_ -ceq $footer}).Count -eq 1
    $result.customLegacyProbeBypassed=[bool]$ManagedOnly
    }
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($hold in $holds){$hold.Dispose()}}else{$global:LeaMinimalProviderInputLeases=$holds}
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 8))
    $result|Select-Object passed,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
