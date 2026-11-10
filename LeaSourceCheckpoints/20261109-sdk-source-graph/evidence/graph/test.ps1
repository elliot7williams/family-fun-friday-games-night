param([switch]$KeepForegroundHost,[ValidateRange(1,2)][int]$Version=1)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out="I:\LeaBuildStorage\sdk-source-graph-20261109-v$Version"
$sdk='I:\LeaBuildStorage\github-source-checkpoint-20261109-v1\LeaSourceCheckpoints\20261102\mature-sdk'
$runtime=Join-Path $root "continuity-evidence/sdk-source-graph-embedding-v$Version/runtime"
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Get-PSDrive C).Free -lt 620MB -or (Get-PSDrive I).Free -lt 128MB -or (Test-Path $out) -or (Test-Path $runtime) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/outputs and original reserve required'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Pin([string]$Path,[string]$Expected){
    $walk=Get-Item -LiteralPath $Path
    while($walk){if($walk.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse input refused'};$walk=if($walk -is [IO.FileInfo]){$walk.Directory}else{$walk.Parent}}
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$holds.Add($stream)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))
    if($Expected -and $hash -cne $Expected){throw ('Input differs: '+$Path)}
    $inputs.Add(@{path=$Path;sha256=$hash})
}
function Copy-Pinned([string]$From,[string]$To,[string]$Expected){
    Pin $From $Expected
    New-Item -ItemType Directory -Path (Split-Path $To -Parent) -Force|Out-Null
    Copy-Item -LiteralPath $From -Destination $To
    Pin $To (Get-FileHash -LiteralPath $From).Hash
}
function Set-Property($Xml,[string]$Name,[string]$Value){
    $group=$Xml.DocumentElement.SelectSingleNode("*[local-name()='PropertyGroup']")
    $node=$group.SelectSingleNode("*[local-name()='$Name']")
    if(!$node){$node=$Xml.CreateElement($Name,$Xml.DocumentElement.NamespaceURI);$null=$group.AppendChild($node)}
    $node.InnerText=$Value
}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{passed=$false;inputs=$inputs;runs=$runs;before=@();completeSdk=$false;startupFixed=$false;hostAdopted=$false;productBinariesUsedForSourceBuild=$false;sourceGraphCompiled=$false;binaryModeCompiled=$false;embeddingPassed=$false}
$oldTemp=$env:TEMP;$oldTmp=$env:TMP
try{
    Pin $PSCommandPath;Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'test.ps1')
    $manifestPath=Join-Path $sdk 'source-manifest.json';Pin $manifestPath 'B5A0283803C7062DA98A7F3331D94940360E00045A06038168FCA94214CD114A'
    $manifest=Get-Content -LiteralPath $manifestPath -Raw|ConvertFrom-Json
    function Sdk-Pin([string]$Relative){$rows=@($manifest.files|Where-Object path -CEQ $Relative);if($rows.Count -ne 1){throw ('SDK member ambiguous: '+$Relative)};return $rows[0].sha256}
    $coreProof=Join-Path $root 'continuity-evidence/font-net48-relative-candidate-v1/result.json';Pin $coreProof 'E1A27B9A2ACCCAC6820393FEEBBD6145911D30E9ECF50459DE28E9AB5B4FC46A'
    $viewProof=Join-Path $root 'continuity-evidence/font-net48-signed-render-v1/result.json';Pin $viewProof '952D427EBE4CC04653F22C23833A7D1845BC323AF931ED5181A9D72236181337'
    $core=Get-Content -LiteralPath $coreProof -Raw|ConvertFrom-Json;$view=Get-Content -LiteralPath $viewProof -Raw|ConvertFrom-Json
    if(!$core.passed -or !$view.passed -or $core.candidateCoreSha256 -cne 'C5CC0B99CFA54C090C0E553B2C6080186E4E412BD254A533307860D5522C67E0' -or $view.candidateViewSha256 -cne '4EE7E01AF6F24A87BF3B4BE873B895539108C8792A281962F54AF4A3D03DF344'){throw 'Current mature sources required'}
    $parentProof='I:\LeaBuildStorage\signed-font-startup-investigation-20261108-v1\parent-build.json';Pin $parentProof 'D32732C0BF3B548DB649966100D4B074663E07544BC9C25A648A80D6723AB02C'
    $parent=Get-Content -LiteralPath $parentProof -Raw|ConvertFrom-Json
    $stageProof=Join-Path $root 'continuity-evidence/private-binding-full-suite-v5/staging.json';Pin $stageProof '591FBFFC8D0A57136FEC81442A392BE1B8F14ABC9931D7DCC9E41FD14B3F7B5B'
    $stage=Get-Content -LiteralPath $stageProof -Raw|ConvertFrom-Json
    $cases=@{}
    foreach($name in @('original','source','binary')){
        $case=Join-Path $out $name;$cases[$name]=$case;New-Item -ItemType Directory -Path $case|Out-Null
        foreach($module in @(@{folder='Lea.Engine';proof=$core;source=(Join-Path (Split-Path $coreProof) 'core-source')},@{folder='Lea.Win';proof=$view;source=(Join-Path (Split-Path $viewProof) 'view-source')})){
            $files=@(Get-ChildItem -LiteralPath $module.source -Filter '*.cs' -File)
            if(!$files.Count){throw 'Mature source set missing'}
            foreach($file in $files){$pins=@($module.proof.inputs|Where-Object path -CEQ $file.FullName|Select-Object -ExpandProperty sha256 -Unique);if($pins.Count -ne 1){throw 'Mature source pin ambiguous'};Copy-Pinned $file.FullName (Join-Path $case ($module.folder+'/'+$file.Name)) $pins[0]}
        }
        foreach($n in 1..15){
            $pins=@($parent.inputs|Where-Object {$_.path.EndsWith('\Lea.Script\'+$n+'.cs',[StringComparison]::OrdinalIgnoreCase)}|Select-Object -ExpandProperty sha256 -Unique)
            if($pins.Count -ne 1){throw 'Original parent source pin ambiguous'}
            Copy-Pinned ('I:\LeaBuildStorage\signed-font-startup-investigation-20261108-v1\parent-source\'+$n+'.cs') (Join-Path $case ('Lea.Script/'+$n+'.cs')) $pins[0]
        }
        foreach($file in @('W.cs','AssemblyInfo.cs')){$relative='Lea.Script.Win/'+$file;Copy-Pinned (Join-Path $sdk $relative) (Join-Path $case $relative) (Sdk-Pin $relative)}
        foreach($entry in @(@{name='Dom.js';hash='A228494E1D20D2669DC2FED38B93B7549EF8849FA2C7997F45EE32678117239A'},@{name='Dom.compact.js';hash='196BCE962F2C2F375C77644D0AE19485820F599170D4D80013829A6ECD2886E3'})){
            Copy-Pinned ('I:\LeaBuildStorage\private-binding-full-assembly-20261103-v5-assets\fixtures\Lea.Script\'+$entry.name) (Join-Path $case ('Lea.Script/'+$entry.name)) $entry.hash
        }
        foreach($dll in @('Jint.dll','Acornima.dll','System.Buffers.dll','System.Memory.dll','System.Numerics.Vectors.dll','System.Runtime.CompilerServices.Unsafe.dll')){
            $member=@($stage.files|Where-Object path -CEQ ('runtime/'+$dll));if($member.Count -ne 1){throw 'Dependency identity ambiguous'}
            Copy-Pinned (Join-Path $stage.runtime $dll) (Join-Path $case ('lib/'+$dll)) $member[0].sha256
        }
        Copy-Pinned (Join-Path $root 'Lea.Sdk/Directory.Build.targets') (Join-Path $case 'Directory.Build.targets') ''
        foreach($module in @('Lea.Engine','Lea.Win','Lea.Script','Lea.Script.Win')){
            $from=Join-Path $root ('Lea.Sdk/'+$module+'.csproj');$pin=''
            if($name -eq 'original' -and $module -in @('Lea.Script','Lea.Script.Win')){$relative=$module+'/'+$module+'.csproj';$from=Join-Path $sdk $relative;$pin=Sdk-Pin $relative}
            Pin $from $pin;$xml=[xml](Get-Content -LiteralPath $from -Raw)
            Set-Property $xml 'TargetFrameworkVersion' 'v4.8'
            Set-Property $xml 'GenerateTargetFrameworkAttribute' 'false'
            if($module -eq 'Lea.Script'){Set-Property $xml 'DefineConstants' 'TRACE;RELEASE;NETFRAMEWORK;NET48'}
            $project=Join-Path $case ($module+'/'+$module+'.csproj');$xml.Save($project);Pin $project
        }
        if(@(Get-ChildItem -LiteralPath $case -Recurse -File|Where-Object {$_.Extension -in @('.dll','.exe') -and !$_.FullName.StartsWith((Join-Path $case 'lib')+'\',[StringComparison]::OrdinalIgnoreCase)}).Count){throw 'Product binary seeded into source case'}
    }
    # Explicit binary mode must not compile the deliberately invalid source dependency.
    $poison=Join-Path $cases.binary 'Lea.Engine/SourceGraphPoison.cs'
    [IO.File]::WriteAllText($poison,'#error Binary-reference mode must not compile the core source project'+[Environment]::NewLine);Pin $poison
    $owner=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Pin $owner 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owner
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Pin $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $dotnet='C:\Program Files\dotnet\dotnet.exe';Pin $dotnet
    $msbuild='C:\Program Files\dotnet\sdk\8.0.427\MSBuild.dll';Pin $msbuild
    $env:TEMP=Join-Path $out 'temp';$env:TMP=$env:TEMP;New-Item -ItemType Directory -Path $env:TEMP|Out-Null
    function Build-Case([string]$Name,[string]$Project,[string]$Case,[string[]]$Extra){
        $args=@(('"'+$msbuild+'"'),('"'+$Project+'"'),'/nologo','/m:1','/nodeReuse:false','/t:Rebuild','/v:minimal','/p:Configuration=Release','/p:UseSharedCompilation=false')+@($Extra)
        return Invoke-OwnedTestRun -Name $Name -Application $dotnet -Arguments $args -WorkingDirectory $Case -OutputDirectory $out -Seconds 120 -Records $runs
    }
    if($Version -eq 1){
        $negative=Build-Case 'original-missing-dependencies' (Join-Path $cases.original 'Lea.Script.Win/Lea.Script.Win.csproj') $cases.original @()
    }else{
        $priorFolder='I:\LeaBuildStorage\sdk-source-graph-20261109-v1'
        $priorReceipt=Join-Path $priorFolder 'result.json';Pin $priorReceipt 'F201D5EF8C4C7C9FEA719D63AFCB73AE604786B12B01E84216ED4A2584DF1ACF'
        $prior=Get-Content -LiteralPath $priorReceipt -Raw|ConvertFrom-Json
        if($prior.passed -or !$prior.inputsUnchanged -or $prior.ownerAttention -or $prior.runs.Count -ne 1){throw 'Stopped harness receipt differs'}
        $recipe=@($prior.inputs|Where-Object path -CEQ $PSCommandPath|Select-Object -ExpandProperty sha256 -Unique)
        if($recipe.Count -ne 1){throw 'Original harness recipe identity ambiguous'}
        Pin (Join-Path $priorFolder 'test.ps1') $recipe[0]
        $oldCase=Join-Path $priorFolder 'original'
        foreach($entry in @($prior.inputs|Where-Object {$_.path.StartsWith($oldCase+'\',[StringComparison]::OrdinalIgnoreCase)})){
            Pin $entry.path $entry.sha256
            $matching=Join-Path $cases.original $entry.path.Substring($oldCase.Length+1)
            if((Get-FileHash -LiteralPath $matching).Hash -cne $entry.sha256){throw 'Reused negative control source differs'}
        }
        $negative=$prior.runs[0];Pin $negative.stdout;Pin $negative.stderr
        $result.negativeReusedFromReceipt='F201D5EF8C4C7C9FEA719D63AFCB73AE604786B12B01E84216ED4A2584DF1ACF'
        $result.negativeRun=$negative
    }
    $diagnostics=(Get-Content -LiteralPath $negative.stdout -Raw)+(Get-Content -LiteralPath $negative.stderr -Raw)
    if($negative.passed -or $negative.exitCode -ne 1 -or $negative.timedOut -or !$negative.ownedJob -or !$negative.originalHandleCaptured -or !$negative.childRetirementIndependentlyAudited -or !$negative.exitObserved -or !$negative.outputDrained -or !$negative.stdoutComplete -or !$negative.stderrComplete -or $negative.activeAfterRetirement -ne 0 -or $negative.unsafeToContinue -or $negative.error -or $diagnostics -notmatch 'error CS0234:.*Engine.*Lea' -or $diagnostics -notmatch 'error CS0246'){throw 'Original clean-build defect not reproduced safely'}
    $result.originalDefectReproduced=$true;$result.negativeCleanupRequested=$negative.killRequested
    $positive=Build-Case 'automatic-source-graph' (Join-Path $cases.source 'Lea.Script.Win/Lea.Script.Win.csproj') $cases.source @()
    if(!$positive.passed){throw 'Automatic source graph build failed'}
    $result.outputs=@(foreach($module in @('Lea.Engine','Lea.Win','Lea.Script','Lea.Script.Win')){$p=Join-Path $cases.source ('bin/Release/'+$module+'.dll');Pin $p;@{name=$module;path=$p;sha256=(Get-FileHash -LiteralPath $p).Hash}})
    $result.sourceGraphCompiled=$true
    foreach($module in @('Lea.Engine','Lea.Win')){Copy-Pinned (Join-Path $cases.source ('bin/Release/'+$module+'.dll')) (Join-Path $cases.binary ('bin/Release/'+$module+'.dll')) ''}
    $binary=Build-Case 'explicit-binary-mode' (Join-Path $cases.binary 'Lea.Script.Win/Lea.Script.Win.csproj') $cases.binary @('/p:LeaBuildFromSource=false')
    if(!$binary.passed){throw 'Explicit binary-reference mode failed'}
    $result.binaryModeCompiled=$true
    foreach($file in @('EmbeddingSample.cs','EmbeddingSample.csproj','embedding.html')){Copy-Pinned (Join-Path $root ('Lea.Sdk/'+$file)) (Join-Path $cases.source ('Sample/'+$file)) ''}
    $consumer=Build-Case 'build-embedding-consumer' (Join-Path $cases.source 'Sample/EmbeddingSample.csproj') $cases.source @('/p:LeaSdkBin="'+(Join-Path $cases.source 'bin/Release')+'"')
    if(!$consumer.passed){throw 'Embedding consumer build failed'}
    $sampleBin=Join-Path $cases.source 'Sample/bin/Release';$closure=@(Get-ChildItem -LiteralPath $sampleBin -File)
    if((Get-PSDrive C).Free -lt (600MB+($closure|Measure-Object Length -Sum).Sum+512KB)){throw 'Actual embedding allocation plus original reserve required'}
    New-Item -ItemType Directory -Path $runtime|Out-Null
    foreach($file in $closure){Copy-Pinned $file.FullName (Join-Path $runtime $file.Name) ''}
    $result.before=@(Get-ChildItem -LiteralPath $runtime -File|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash;sddl=(Get-Acl -LiteralPath $_.FullName).Sddl}})
    $sample=Invoke-OwnedTestRun -Name 'current-source-embedding' -Application (Join-Path $runtime 'Lea.EmbeddingSample.exe') -Arguments @('--test',('"'+(Join-Path $runtime 'embedding.html')+'"'),('"'+$out+'"')) -WorkingDirectory $runtime -OutputDirectory $out -Seconds 30 -Records $runs
    if(!$sample.passed -or @(Get-Content -LiteralPath $sample.stdout|Where-Object {$_ -ceq 'PASS LEA SDK EMBEDDING 14'}).Count -ne 1){throw 'Current source embedding failed'}
    $result.embeddingPassed=$true;$result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $env:TEMP=$oldTemp;$env:TMP=$oldTmp
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.changedFiles=@($result.before|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256 -or (Get-Acl -LiteralPath $_.path).Sddl -cne $_.sddl})
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.changedFiles.Count -and !$result.ownerAttention -and !$result.error
    if(!$result.ownerAttention){foreach($hold in $holds){$hold.Dispose()}}else{$global:LeaSdkGraphLeases=$holds}
    $result|ConvertTo-Json -Depth 8|Set-Content -LiteralPath (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object passed,originalDefectReproduced,sourceGraphCompiled,binaryModeCompiled,embeddingPassed,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
