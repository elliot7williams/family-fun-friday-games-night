param([switch]$KeepForegroundHost,[switch]$FoldConstants,[switch]$NativeWindowGlobals,[switch]$DetailedStages)
$ErrorActionPreference='Stop'
if($FoldConstants -and $NativeWindowGlobals){throw 'Keep independent candidate changes separate'}
if($DetailedStages -and ($FoldConstants -or $NativeWindowGlobals)){throw 'Detailed profiling must remain independent'}
$comparison=[bool]($FoldConstants -or $NativeWindowGlobals)
$root=Split-Path $PSScriptRoot -Parent
$out='I:\LeaBuildStorage\current-bootstrap-profile-run-20261109-v1'
$runtime=Join-Path $root 'continuity-evidence/current-bootstrap-profile-v1/runtime'
$baseline=Join-Path $root 'continuity-evidence/private-binding-full-suite-v5'
$recovery='I:\LeaBuildStorage\signed-font-startup-investigation-20261108-v1'
$profile='I:\LeaBuildStorage\current-bootstrap-profile-source-20261109-v1'
if($DetailedStages){
    $out='I:\LeaBuildStorage\detailed-bootstrap-profile-run-20261109-v1'
    $runtime=Join-Path $root 'continuity-evidence/detailed-bootstrap-profile-v1/runtime'
    $profile='I:\LeaBuildStorage\detailed-bootstrap-profile-source-20261109-v1'
}
if($FoldConstants){
    $out='I:\LeaBuildStorage\current-bootstrap-fold-abba-20261109-v1'
    $runtime=Join-Path $root 'continuity-evidence/current-bootstrap-fold-v1/runtime'
}
if($NativeWindowGlobals){
    $out='I:\LeaBuildStorage\native-window-globals-abba-20261109-v1'
    $runtime=Join-Path $root 'continuity-evidence/native-window-globals-v1/runtime'
}
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Test-Path $out) -or (Test-Path $runtime) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/evidence/runtime required'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Pin([string]$path,[string]$expected){
    $item=Get-Item -LiteralPath $path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse input refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $stream=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($stream)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))
    if($expected -and $hash -cne $expected){throw ('Profile input differs: '+$path)}
    $inputs.Add(@{path=$path;sha256=$hash})
}
if((Get-PSDrive C).Free -lt 604MB -or (Get-PSDrive I).Free -lt 64MB){throw 'Original600MiB reserve plus diagnostic closure required'}
New-Item -ItemType Directory -Path $out,$runtime|Out-Null
$result=@{passed=$false;diagnosticOnly=$true;runtimeQualified=$false;hostAdopted=$false;inputs=$inputs;runs=$runs;before=@();historicalMetadataRecovered=$false;metadataReplacementExplicit=$true;limitsChanged=$false}
try{
    Pin $PSCommandPath
    $sourceProof=Join-Path $profile 'source.json'
    $sourceHash=if($DetailedStages){'BFBEEF565BE2EEEE0683C8A729E754DEDB17B49C458BA3C7782CED5C33C6926F'}else{'D74BB7C6812012197DFC11A803C13E493796D40685E953E1B6194ED9624F2968'}
    Pin $sourceProof $sourceHash
    $plan=Get-Content $sourceProof -Raw|ConvertFrom-Json
    $expectedControls=if($DetailedStages){4}else{6}
    if(!$plan.verified -or !$plan.inverseVerified -or !$plan.criticalOperationsVerified -or $plan.negativeControls -ne $expectedControls -or (!$DetailedStages -and $plan.pairShapeControls -ne 1) -or $plan.budgetsChanged -or $plan.files.Count -ne 2){throw 'Operation-preserving diagnostic proof required'}
    if($DetailedStages){
        $nodePath=Join-Path $profile 'node-test.json';Pin $nodePath
        $node=Get-Content -LiteralPath $nodePath -Raw|ConvertFrom-Json
        if(!$node.passed -or $node.checks -ne 156 -or !$node.detailSequenceVerified -or !$node.hiddenHelperRemoved -or $node.sourceProofSha256 -cne $sourceHash){throw 'Detailed resource mock controls incomplete'}
        $result.detailedStages=$true;$result.historicalMetadataRecovered=$true;$result.metadataReplacementExplicit=$false
    }
    foreach($file in $plan.files){Pin $file.path $file.sha256}
    $foldedHost=$null;$baselineProfile=$null;$installerSource=$null
    $resourcePath=Join-Path $profile 'Dom.compact.js'
    if($comparison){
        if($FoldConstants){
        $oldHost=Join-Path $profile '8.cs';$original=[IO.File]::ReadAllText($oldHost)
        $old='FoldConstants = false,';$new='FoldConstants = true,'
        function Fold([string]$text){
            if([regex]::Matches($text,[regex]::Escape($old)).Count -ne 1 -or $text.Contains($new)){throw 'Unique original folding setting required'}
            $value=$text.Replace($old,$new)
            if($value.Replace($new,$old) -cne $text){throw 'Folding projection inverse differs'}
            return $value
        }
        $updated=Fold $original;$negative=0
        foreach($bad in @($original.Replace($old,'MISSING_FOLD_SETTING'),($original+$original),$updated)){
            $rejected=$false;try{$null=Fold $bad}catch{$rejected=$true};if(!$rejected){throw 'Malformed folding source accepted'};$negative++
        }
        $foldedHost=Join-Path $out '8.fold.cs';[IO.File]::WriteAllText($foldedHost,$updated,[Text.UTF8Encoding]::new($false));Pin $foldedHost
        $result.fixedBootstrapFolding=$true;$result.foldingInverseVerified=$true;$result.foldingNegativeControls=$negative
        $result.speedImprovementProven=$false
        }else{
            $native='I:\LeaBuildStorage\native-window-globals-source-20261109-v1/source.json'
            Pin $native '6E5B5FD89DCE987831D6BD0D190A27AC1EF76CE625537333F46BFC63579378E4'
            $nativePlan=Get-Content $native -Raw|ConvertFrom-Json
            if(!$nativePlan.verified -or !$nativePlan.inverseVerified -or $nativePlan.negativeControls -ne 8 -or $nativePlan.policiesChanged -or !$nativePlan.functionBodiesPreserved -or $nativePlan.files.Count -ne 2){throw 'Native window source boundary differs'}
            foreach($file in @($nativePlan.files)+@($nativePlan.host,$nativePlan.installer)){Pin $file.path $file.sha256}
            $nodeProof='I:\LeaBuildStorage\native-window-globals-source-20261109-v1/node-test.json'
            Pin $nodeProof '33905C1E52585A4A17CB7E718B5D89AF873215B26CAE02F217B990EA0B4A01E5'
            $node=Get-Content $nodeProof -Raw|ConvertFrom-Json
            if(!$node.passed -or $node.checks -ne 312 -or $node.apiMetadataComparisons -ne 2 -or !$node.installerMockOnly){throw 'Node projection checks incomplete'}
            $foldedHost=$nativePlan.host.path;$installerSource=$nativePlan.installer.path
            $resourcePath=@($nativePlan.files|Where-Object name -CEQ 'Dom.compact.js')[0].path
            $result.nativeWindowGlobals=$true;$result.nativeSourceProofSha256=(Get-FileHash $native).Hash
            $result.speedImprovementProven=$false
        }
        $proof='I:\LeaBuildStorage\current-bootstrap-profile-run-20261109-v1/result.json';Pin $proof 'E926C855BBAA2528E7FD113A2D1A337C616A1F0173C44B06B74C94EEA22D3B0F'
        $baselineProfile=Get-Content $proof -Raw|ConvertFrom-Json
        if(!$baselineProfile.passed -or !$baselineProfile.inputsUnchanged -or $baselineProfile.ownerAttention -or $baselineProfile.before.Count -ne 10 -or $baselineProfile.diagnosticParentSha256 -cne 'C56FB88B72F9D2492101B28B8E8B93FD6B82A94F95A7C1444F6D5E3E5112F21B'){throw 'Matched nonfolded diagnostic baseline required'}
        foreach($file in $baselineProfile.before){Pin $file.path $file.sha256;if((Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'Baseline ACL changed'}}
    }
    $buildProof=Join-Path $recovery 'parent-build.json';Pin $buildProof 'D32732C0BF3B548DB649966100D4B074663E07544BC9C25A648A80D6723AB02C'
    $build=Get-Content $buildProof -Raw|ConvertFrom-Json
    $stageProof=Join-Path $baseline 'staging.json';Pin $stageProof '591FBFFC8D0A57136FEC81442A392BE1B8F14ABC9931D7DCC9E41FD14B3F7B5B'
    $stage=Get-Content $stageProof -Raw|ConvertFrom-Json
    $dependencies=@('Acornima.dll','Jint.dll','Lea.Engine.dll','System.Buffers.dll','System.Memory.dll','System.Numerics.Vectors.dll','System.Runtime.CompilerServices.Unsafe.dll')
    $closureBytes=0L
    foreach($name in $dependencies){$closureBytes+=(Get-Item -LiteralPath (Join-Path $stage.runtime $name)).Length}
    if((Get-PSDrive C).Free -lt (600MB+$closureBytes+512KB)){throw 'Full actual diagnostic allocation plus reserve required'}
    foreach($name in $dependencies){
        $member=@($stage.files|Where-Object path -CEQ ('runtime/'+$name))
        if($member.Count -ne 1){throw 'Dependency identity ambiguous'}
        $from=Join-Path $stage.runtime $name;Pin $from $member[0].sha256
        $to=Join-Path $runtime $name;Copy-Item -LiteralPath $from -Destination $to;Pin $to $member[0].sha256
    }
    Pin (Join-Path $PSScriptRoot 'OwnedTestProcess.cs') 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path (Join-Path $PSScriptRoot 'OwnedTestProcess.cs')
    Pin (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1') 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1')
    $compiler='C:\Program Files\dotnet\dotnet.exe';Pin $compiler
    $toolchain='C:\Program Files\dotnet\sdk\8.0.427\Roslyn\bincore'
    foreach($name in @('csc.dll','csc.deps.json','csc.runtimeconfig.json','Microsoft.CodeAnalysis.dll','Microsoft.CodeAnalysis.CSharp.dll')){Pin (Join-Path $toolchain $name)}
    $refs=[Collections.Generic.List[string]]::new()
    $framework='C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8'
    foreach($name in @('mscorlib','System','System.Core','System.Drawing','System.Windows.Forms','System.Web.Extensions','System.Xml','System.Runtime.Serialization','System.Security','Facades/netstandard')){
        $path=Join-Path $framework ($name+'.dll');Pin $path;$refs.Add('/reference:"'+$path+'"')
    }
    foreach($name in $dependencies){$refs.Add('/reference:"'+(Join-Path $runtime $name)+'"')}
    $sources=[Collections.Generic.List[string]]::new()
    foreach($n in 1..15){
        if($n -eq 14 -and !$DetailedStages){continue}
        $old=Join-Path $recovery ('parent-source/'+$n+'.cs')
        $pins=@($build.inputs|Where-Object {$_.path.EndsWith('\Lea.Script\'+$n+'.cs',[StringComparison]::OrdinalIgnoreCase)}|Select-Object -ExpandProperty sha256 -Unique)
        if($pins.Count -ne 1){throw 'Recovered source identity ambiguous'}
        Pin $old $pins[0]
        $path=if($n -eq 8){Join-Path $profile '8.cs'}else{$old}
        if($n -eq 8 -and $comparison){$path=$foldedHost}
        $sources.Add('"'+$path+'"')
    }
    # Older experiments keep their explicit metadata; detailed mode uses recovered14.cs.
    if(!$DetailedStages){$metadata=Join-Path $PSScriptRoot 'DiagnosticScriptAssemblyInfo.cs';Pin $metadata;$sources.Add('"'+$metadata+'"')}
    if($installerSource){$sources.Add('"'+$installerSource+'"')}
    $dll=Join-Path $runtime 'Lea.Script.dll'
    $options=@('/nologo','/noconfig','/nostdlib+','/target:library','/optimize+','/langversion:12',('/out:"'+$dll+'"'))+@($refs.ToArray())+@($sources.ToArray())+@('/resource:"'+$resourcePath+'",Lea.Script.Dom.js')
    $rsp=Join-Path $out 'parent.rsp';[IO.File]::WriteAllLines($rsp,$options);Pin $rsp
    $compile=Invoke-OwnedTestRun -Name 'compile-current-profile' -Application $compiler -Arguments @(('"'+(Join-Path $toolchain 'csc.dll')+'"'),('@"'+$rsp+'"')) -WorkingDirectory $runtime -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$compile.passed){throw 'Diagnostic parent compile failed'}
    Pin $dll;$result.diagnosticParentSha256=(Get-FileHash $dll).Hash
    $main=Join-Path $root 'Lea.Tests/BootstrapComparisonMain.cs';Pin $main
    $exe=Join-Path $runtime 'Lea.CurrentBootstrap.Tests.exe'
    $options=@('/nologo','/noconfig','/nostdlib+','/target:exe','/optimize+',('/out:"'+$exe+'"'),('"'+$main+'"'),('/reference:"'+$dll+'"'))+@($refs.ToArray())
    $rsp=Join-Path $out 'harness.rsp';[IO.File]::WriteAllLines($rsp,$options);Pin $rsp
    $compile=Invoke-OwnedTestRun -Name 'compile-current-profile-harness' -Application $compiler -Arguments @(('"'+(Join-Path $toolchain 'csc.dll')+'"'),('@"'+$rsp+'"')) -WorkingDirectory $runtime -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$compile.passed){throw 'Diagnostic harness compile failed'}
    $config=Join-Path $stage.runtime 'Lea.Script.Tests.exe.config';Pin $config;Copy-Item -LiteralPath $config -Destination ($exe+'.config');Pin ($exe+'.config')
    if($NativeWindowGlobals){
        $contractSource=Join-Path $root 'Lea.Tests/WindowGlobalsMain.cs';Pin $contractSource
        $contractExe=Join-Path $runtime 'Lea.WindowGlobals.Tests.exe'
        $options=@('/nologo','/noconfig','/nostdlib+','/target:exe','/optimize+',('/out:"'+$contractExe+'"'),('"'+$contractSource+'"'),('/reference:"'+$dll+'"'))+@($refs.ToArray())
        $rsp=Join-Path $out 'contract.rsp';[IO.File]::WriteAllLines($rsp,$options);Pin $rsp
        $compile=Invoke-OwnedTestRun -Name 'compile-window-contract' -Application $compiler -Arguments @(('"'+(Join-Path $toolchain 'csc.dll')+'"'),('@"'+$rsp+'"')) -WorkingDirectory $runtime -OutputDirectory $out -Seconds 120 -Records $runs
        if(!$compile.passed){throw 'Native installer contract compile failed'}
        Copy-Item -LiteralPath $config -Destination ($contractExe+'.config');Pin $contractExe;Pin ($contractExe+'.config')
    }
    $fixture=Join-Path $baseline 'fixtures/LeaForge/test-pages/javascript-contains.html'
    $fixtureMember=@($stage.files|Where-Object path -CEQ 'fixtures/LeaForge/test-pages/javascript-contains.html')
    if($fixtureMember.Count -ne 1){throw 'Diagnostic fixture identity ambiguous'}
    Pin $fixture $fixtureMember[0].sha256;Pin $exe
    $result.before=@(Get-ChildItem -LiteralPath $runtime -File|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash $_.FullName).Hash;sddl=(Get-Acl -LiteralPath $_.FullName).Sddl}})
    if($comparison){
        $result.before+=@($baselineProfile.before)
        if($NativeWindowGlobals){
            $contract=Invoke-OwnedTestRun -Name 'window-globals-contract' -Application $contractExe -Arguments @() -WorkingDirectory $runtime -OutputDirectory $out -Seconds 30 -Records $runs
            $footer=@(Get-Content -LiteralPath $contract.stdout|Where-Object {$_ -cmatch '^WINDOW_GLOBALS_CONTRACT PASS [0-9]+$'})
            if(!$contract.passed -or $footer.Count -ne 1){throw 'Native window installer contract failed'}
            $result.contractFooter=$footer[0];$result.nativeContractPassed=$true
        }
        $result.observations=[Collections.Generic.List[object]]::new()
        . (Join-Path $PSScriptRoot 'Read-TrustedBootstrapProfile.ps1')
        Pin (Join-Path $PSScriptRoot 'Read-TrustedBootstrapProfile.ps1')
        $baselineExe=@($baselineProfile.before|Where-Object {$_.path.EndsWith('Lea.CurrentBootstrap.Tests.exe',[StringComparison]::OrdinalIgnoreCase)})
        if($baselineExe.Count -ne 1){throw 'Unique baseline executable required'}
        $order=@(@{label='baseline-1';exe=$baselineExe[0].path},@{label='folded-2';exe=$exe},@{label='folded-3';exe=$exe},@{label='baseline-4';exe=$baselineExe[0].path})
        if($NativeWindowGlobals){$order[1].label='native-2';$order[2].label='native-3'}
        foreach($case in $order){
            $sample=Invoke-OwnedTestRun -Name $case.label -Application $case.exe -Arguments @(('"'+$fixture+'"')) -WorkingDirectory ([IO.Path]::GetDirectoryName($case.exe)) -OutputDirectory $out -Seconds 30 -Records $runs
            $reports=@(Get-Content -LiteralPath $sample.stdout|Where-Object {$_.StartsWith('LEA_BOOTSTRAP_COMPARISON ')})
            if($reports.Count -ne 1){throw 'One bounded observation report required'}
            $report=$reports[0].Substring('LEA_BOOTSTRAP_COMPARISON '.Length)|ConvertFrom-Json
            $rows=@(Get-Content -LiteralPath $sample.stderr|Where-Object {$_.StartsWith('LEA_TRUSTED_BOOT ')})
            $parsed=Read-TrustedBootstrapProfile -Lines $rows -Failed:(!$report.ok)
            $result.observations.Add(@{label=$case.label;runPassed=$sample.passed;report=$report;profile=$parsed})
        }
        $result.measurementCaptured=$result.observations.Count -eq 4
        $result.passed=$result.measurementCaptured -and @($result.observations|Where-Object {!$_.runPassed -or !$_.report.ok}).Count -eq 0
    }else{
    $run=Invoke-OwnedTestRun -Name 'cold-current-profile' -Application $exe -Arguments @(('"'+$fixture+'"')) -WorkingDirectory $runtime -OutputDirectory $out -Seconds 30 -Records $runs
    $reports=@(Get-Content -LiteralPath $run.stdout|Where-Object {$_.StartsWith('LEA_BOOTSTRAP_COMPARISON ')})
    $rows=@(Get-Content -LiteralPath $run.stderr|Where-Object {$_.StartsWith('LEA_TRUSTED_BOOT ')})
    $result.measurementCaptured=$reports.Count -eq 1 -and $rows.Count -gt 0 -and $run.outputDrained -and $run.exitObserved
    if($reports.Count -eq 1){$result.report=$reports[0].Substring('LEA_BOOTSTRAP_COMPARISON '.Length)|ConvertFrom-Json}
    $result.profileRows=$rows.Count
    $result.passed=$run.passed -and $result.measurementCaptured -and $result.report.ok
    }
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.changedFiles=@($result.before|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256 -or (Get-Acl -LiteralPath $_.path).Sddl -cne $_.sddl})
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.changedFiles.Count -and !$result.ownerAttention -and !$result.error
    if(!$result.ownerAttention){foreach($hold in $holds){$hold.Dispose()}}else{$global:LeaCurrentProfileLeases=$holds}
    $result|ConvertTo-Json -Depth 7|Set-Content -LiteralPath (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,measurementCaptured,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
