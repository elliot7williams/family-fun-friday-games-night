param([switch]$KeepForegroundHost,[ValidateSet(1,2,3,4,5,6,7)][int]$Version=7)
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace ('continuity-evidence/page-color-host-v'+$Version)
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
$identity=[Security.Principal.WindowsIdentity]::GetCurrent()
if(([Security.Principal.WindowsPrincipal]::new($identity)).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Normal nonadmin required'}
if(Test-Path -LiteralPath $out){throw 'Preserve prior attempt'}
if((Get-PSDrive C).Free -lt 600MB){throw 'Storage reserve required'}
if(@(Get-Process 'Lea.Script.Tests','Lea.Script.Worker','Lea.Firespider.Boot','Firespider*' -ErrorAction SilentlyContinue).Count){throw 'Do not overlap browser/tests'}
if(Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue){throw 'Fresh owner required'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$Path,[string]$Hash){
    $s=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($s)
    if(!$Hash){$Hash=(Get-FileHash $Path).Hash}
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $Hash){throw ('Input differs: '+$Path)}
    $inputs.Add(@{path=$Path;sha256=$Hash})
}
function CopyPinned([string]$From,[string]$To,[string]$Hash){
    Hold $From $Hash
    $null=New-Item -ItemType Directory -Path (Split-Path $To) -Force
    Copy-Item -LiteralPath $From -Destination $To
}
function Run([string]$Name,[string]$App,[string[]]$Arguments,[int]$Seconds){
    Invoke-OwnedTestRun -Name $Name -Application $App -Arguments $Arguments -WorkingDirectory $workspace -OutputDirectory $out -Seconds $Seconds -Records $runs
}
$runtime=Join-Path $out 'runtime'
$result=[ordered]@{passed=$false;candidate=$runtime;runs=$runs;inputs=$inputs;scope='Selected-page colors only: parsed Lea document, legacy fallback retained, unchanged engine/worker and limits; not full host qualification';installedBrowserChanged=$false}
$null=New-Item -ItemType Directory -Path $runtime
try{
    Hold $PSCommandPath
    Hold (Join-Path $PSScriptRoot 'OwnedTestProcess.cs') 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA'
    Hold (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1') 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00'
    Add-Type -Path (Join-Path $PSScriptRoot 'OwnedTestProcess.cs')
    . (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1')
    $proof=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/host-library-current-native-v1/result.json'
    Hold $proof 'C2731D13019074BDB7E82353DDE0B1DDC11D47FFF1AA58ABA8D1A157468008FA'
    $hostProof=Get-Content $proof -Raw|ConvertFrom-Json
    if(!$hostProof.passed -or !$hostProof.originalInputsUnchanged -or !$hostProof.candidateBytesUnchanged -or $hostProof.ownerAttention){throw 'Host receipt incomplete'}
    $known=@{}
    foreach($file in $hostProof.before){
        $relative=[IO.Path]::GetRelativePath($hostProof.candidate,$file.path)
        $to=Join-Path $runtime $relative;CopyPinned $file.path $to $file.sha256;$known[$to]=$file.sha256
    }
    $audit=Join-Path $workspace 'continuity-evidence/legacy-host-tools-audit-v1/result.json'
    Hold $audit '41C7B4B93652E93FFC5836D05C0CA7BF697E1D419374CC7527ABF5913DDC2BF4'
    $checkpoint=Join-Path $workspace 'continuity-evidence/library-and-trace-source-20261026.zip'
    Hold $checkpoint 'A1515801B6A9696951BA07F874CAAD976E4A7896DAAD18F85D8138F6B8E38AC0'
    $zip=[IO.Compression.ZipFile]::OpenRead($checkpoint)
    try{
        $reader=[IO.StreamReader]::new($zip.GetEntry('manifest.json').Open());try{$manifest=$reader.ReadToEnd()|ConvertFrom-Json}finally{$reader.Dispose()}
    }finally{$zip.Dispose()}
    $prefix='continuity-evidence/library-navigation-v1/host-source/'
    $pins=@($manifest.members|Where-Object{$_.entry.StartsWith($prefix,[StringComparison]::Ordinal)})
    if($pins.Count -ne 104){throw 'Source inventory differs'}
    $stage=Join-Path $out 'host-source'
    foreach($file in $pins){CopyPinned (Join-Path $workspace $file.entry) (Join-Path $stage $file.entry.Substring($prefix.Length)) $file.sha256}
    foreach($name in @('New-PageColorSource.ps1','New-PageColorHarness.ps1','New-AboutHarnessSource.ps1')){Hold (Join-Path $PSScriptRoot $name);. (Join-Path $PSScriptRoot $name)}
    $helperPath=Join-Path $workspace 'Lea.Firespider/PageColor.vb';Hold $helperPath
    $themePath=Join-Path $stage 'BrowserFeatures10.vb';$routesPath=Join-Path $stage 'FirespiderLea.vb'
    $theme=[IO.File]::ReadAllText($themePath);$routes=[IO.File]::ReadAllText($routesPath);$helper=[IO.File]::ReadAllText($helperPath)
    $edit=New-PageColorSource $theme $routes $helper
    $result.sourceInverseVerified=$edit.inverseVerified
    $controls=0
    foreach($bad in @($theme.Replace('qsitechromecolor','renamed'),($theme+[Environment]::NewLine+$theme),$edit.theme)){
        $rejected=$false;try{$null=New-PageColorSource $bad $routes $helper}catch{$rejected=$_.Exception.Message -ceq 'Page-color source anchors differ'}
        if(!$rejected){throw 'Theme source negative control failed'};$controls++
    }
    $result.sourceNegativeControls=$controls
    [IO.File]::WriteAllText($themePath,$edit.theme,[Text.UTF8Encoding]::new($false))
    [IO.File]::WriteAllText($routesPath,$edit.routes,[Text.UTF8Encoding]::new($false))
    Hold $themePath;Hold $routesPath
    $diagnostic=Join-Path $workspace 'continuity-evidence/split-host-diagnostic-v2'
    $diagProof=Join-Path $diagnostic 'result.json';Hold $diagProof '7999E9A2CE4AA3B834FB1E731CDBC8C091F00F9E79F82CCDC0202D1745A1FFBC'
    foreach($file in (Get-Content $diagProof -Raw|ConvertFrom-Json).inputs){$known[$file.path]=$file.sha256}
    $bootSource=Join-Path $diagnostic 'Boot.cs';Hold $bootSource $known[$bootSource]
    $harness=New-PageColorHarness ([IO.File]::ReadAllText($bootSource))
    $result.harnessInverseVerified=$harness.inverseVerified
    $harnessPath=Join-Path $out 'Boot.cs';[IO.File]::WriteAllText($harnessPath,$harness.text,[Text.UTF8Encoding]::new($false));Hold $harnessPath
    $recipe=Join-Path $diagnostic 'compile.rsp';Hold $recipe $known[$recipe]
    $options=[Collections.Generic.List[string]]::new()
    foreach($line in Get-Content $recipe){
        if($line.StartsWith('/out:')){$line='/out:"'+(Join-Path $runtime 'Lea.Firespider.Boot.exe')+'"'}
        elseif($line -match '^"(.+\.cs)"$'){
            $from=$Matches[1]
            if([IO.Path]::GetFileName($from) -ceq 'Boot.cs'){$line='"'+$harnessPath+'"'}else{Hold $from $known[$from]}
        }elseif($line -match '^/reference:"([^"]+)"$'){
            $from=$Matches[1];$local=Join-Path $runtime ([IO.Path]::GetFileName($from));if(Test-Path $local){$from=$local}
            Hold $from $known[$from];$line='/reference:"'+$from+'"'
        }
        $options.Add($line)
    }
    $rsp=Join-Path $out 'harness.rsp';[IO.File]::WriteAllLines($rsp,$options);Hold $rsp
    $wrapper=Join-Path $PSScriptRoot 'Invoke-PublicCompiler.20261026.ps1';Hold $wrapper 'C3EBB3801FAFB80DA031AAB69FE4837EEB4E4EECD3907F9704F9D6186FA42F0A'
    $compile=Run 'compile-theme-harness' (Get-Process -Id $PID).Path @('-NoProfile','-File',('"'+$wrapper+'"'),'-ResponseFile',('"'+$rsp+'"'),'-ExpectedResponseSha256',(Get-FileHash $rsp).Hash,'-ExpectedSourceSha256','D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB') 120
    if(!$compile.passed){throw 'Harness compile failed'}
    $boot=Join-Path $runtime 'Lea.Firespider.Boot.exe'
    if($Version -eq 1){
        $baseline=Run 'baseline-theme' $boot @(('"'+$runtime+'"'),'about') 90
    }else{
        $baselinePath=Join-Path $workspace 'continuity-evidence/page-color-host-v1/baseline-theme.result.json'
        Hold $baselinePath 'BB11A4FDB7C08B4C8AD0D875DE7489B421300194EF028E179D82C3D57DF0427F'
        $baseline=Get-Content $baselinePath -Raw|ConvertFrom-Json
        Hold $baseline.stderr 'B52173CA2532AC3E49C1CBB1EF86A2FB4C866ACA0406E6197D4B7D5BBA298EE4'
        $result.baselineEvidence=$baselinePath
    }
    $result.baselineForcedCleanup=$baseline.killRequested
    $result.baselineRegressionReproduced=$baseline.exitCode -eq 1 -and $baseline.exitObserved -and $baseline.originalHandleCaptured -and !$baseline.timedOut -and $baseline.childRetirementIndependentlyAudited -and $baseline.activeAfterRetirement -eq 0 -and $baseline.stdoutComplete -and $baseline.stderrComplete -and ([IO.File]::ReadAllText($baseline.stderr).Contains('Lea parsed document page color 0'))
    if(!$result.baselineRegressionReproduced){throw 'Baseline theme failure not reproduced'}
    $buildProof=Join-Path $workspace 'continuity-evidence/firespider-source-rebuild-v1/result.json'
    Hold $buildProof '9DDF92B7273943C6CB5481A8CCC8033F09053BE2C5968DC5E53EDA14E0A5F3DA'
    $build=Get-Content $buildProof -Raw|ConvertFrom-Json
    Hold $build.compiler.path $build.compiler.sha256
    $bin=Join-Path $out 'host-bin'
    if($Version -in @(3,4,6,7)){
        $prior=Join-Path $workspace $(if($Version -in @(6,7)){'continuity-evidence/page-color-host-v5'}else{'continuity-evidence/page-color-host-v2'})
        $priorReceipt=Join-Path $prior 'result.json';Hold $priorReceipt $(if($Version -in @(6,7)){'E4AF5A590D9CDB24CFEA9E59587AC7705AE873045E577A7BF7E4FC2A38F15F3C'}else{$null})
        $priorResult=Get-Content $priorReceipt -Raw|ConvertFrom-Json
        foreach($name in @('BrowserFeatures10.vb','FirespiderLea.vb')){
            $original=Join-Path $prior ('host-source/'+$name);Hold $original (Get-FileHash (Join-Path $stage $name)).Hash
        }
        $exe=Join-Path $prior 'host-bin/Firespider Internet 19.exe';Hold $exe $priorResult.executableSha256
        $result.reusedHostSourceVerified=$true
        $result.reusedHostReceipt=$priorReceipt
    }else{
        $compile=Run 'build-theme-host' $build.compiler.path @(('"'+(Join-Path $stage 'Firespider Internet.vbproj')+'"'),'/nologo','/m:1','/nodeReuse:false','/t:Rebuild','/v:minimal','/p:Configuration=Release','/p:GenerateManifests=false','/p:AutoGenerateBindingRedirects=true',('/p:OutputPath="'+$bin+'"')) 120
        if(!$compile.passed){throw 'Changed host build failed'}
        $exe=Join-Path $bin 'Firespider Internet 19.exe'
    }
    $result.executableSha256=(Get-FileHash $exe).Hash
    CopyPinned $exe (Join-Path $runtime 'Firespider Internet 19.exe') $result.executableSha256
    $result.before=@(Get-ChildItem $runtime -Recurse -File|ForEach-Object{@{path=$_.FullName;sha256=(Get-FileHash $_.FullName).Hash;sddl=(Get-Acl $_.FullName).Sddl}})
    $positive=Run 'fixed-theme' $boot @(('"'+$runtime+'"'),'about') 90
    $result.behaviorPassed=$positive.passed -and ([IO.File]::ReadAllText($positive.stdout).Contains('PASS Lea page colors: six parsed-document fixtures'))
    if($Version -in @(6,7)){$result.boundaryFooterVerified=([IO.File]::ReadAllText($positive.stdout).Contains('PASS Lea page color boundaries: null host, cycle, exact and over-limit traversal, restored tree, background completion and original tabs.'));$result.behaviorPassed=$result.behaviorPassed -and $result.boundaryFooterVerified}
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object{(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.changedFiles=if($result.before){@($result.before|Where-Object{(Get-FileHash $_.path).Hash -cne $_.sha256 -or (Get-Acl $_.path).Sddl -cne $_.sddl})}else{@()}
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.behaviorPassed -and $result.before -and !$result.error -and $result.inputsUnchanged -and !$result.changedFiles.Count -and !$result.ownerAttention
    if($result.ownerAttention){$global:LeaPageColorInputLeases=$leases}else{foreach($s in $leases){$s.Dispose()}}
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 8))
    [pscustomobject]$result|Select-Object passed,baselineRegressionReproduced,behaviorPassed,ownerAttention,error|ConvertTo-Json -Compress
}
