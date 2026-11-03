param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$outRoot=Join-Path $workspace 'continuity-evidence/title-metadata-regression-v2'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained foreground TTY required'}
if(Test-Path -LiteralPath $outRoot){throw 'Preserve regression'}
$utility=Join-Path $workspace 'continuity-evidence/native-launch-timing-v1/Test-CompositeCandidate.v2.ps1'
if((Get-FileHash -LiteralPath $utility).Hash -cne 'EB00AD17B8078A6CC2F418733BA5D675FC84EF49EED433A527FAC5498DD5ECEF'){throw 'Utility changed'}
$t=$null;$e=$null;$ast=[Management.Automation.Language.Parser]::ParseFile($utility,[ref]$t,[ref]$e)
foreach($name in @('Space','Pin','CopyExact','NewOutputReader','PumpOutput','Run')){
    $defs=@($ast.FindAll({param($n)$n -is [Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -ceq $name},$false))
    if($e.Count -or $defs.Count -ne 1){throw 'Utility shape changed'}
    . ([scriptblock]::Create($defs[0].Extent.Text))
}
if(Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue){throw 'Fresh owner required'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
$result=[ordered]@{passed=$false;candidate=(Join-Path $outRoot 'runtime');inputs=$inputs;runs=$runs;error=$null;scope='Full retained native engine/view smoke suite with new core compiler output; not full scripting/host/release validation';strictCoreEquivalence=$false}
Space;$null=New-Item -ItemType Directory -Path $result.candidate
try{
    $native=Join-Path $workspace 'continuity-evidence/request-diagnostics-v1/verified-build-20261020-150037/native'
    $receipt=Join-Path $native 'result.json';$null=Pin $receipt '09690529339278525ACE678608BBD702A2AA3A70846B7EA3D5698EDE4843CD5D'
    $build=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json
    foreach($name in @('Lea.Tests.exe','Lea.Image.exe')){
        $row=@($build.binaries|Where-Object{[IO.Path]::GetFileName($_.path) -ceq $name})
        if($row.Count -ne 1){throw 'Ambiguous test runtime'}
        CopyExact (Join-Path $native $row[0].path) (Join-Path $result.candidate $name) $row[0].sha256
    }
    CopyExact (Join-Path $workspace 'continuity-evidence/title-metadata-visuals-v1/runtime/Lea.Win.dll') (Join-Path $result.candidate 'Lea.Win.dll') 'FF99C552ABD3AEFB2F6DD12ECF83A3B7EAF2A3FDD392571B286F9F804D2939EC'
    CopyExact (Join-Path $workspace 'continuity-evidence/ordered-flow-v6/candidate/Lea.Engine.dll') (Join-Path $result.candidate 'Lea.Engine.dll') '52CE235E37B10243D618E4BDF0031E215145974526DF8F758E167C16E791F4BC'
    CopyExact (Join-Path $workspace 'continuity-evidence/temporal-startup-v1/host-process-state-v1/runtime/Lea.Image.Handshake.exe') (Join-Path $result.candidate 'Lea.Image.Handshake.exe') '79EAE62C778644F11468C079C8B2E7DCFEAE9643CDA55383E7EB7F76D2742C5E'
    $oldSource=Join-Path $native 'source/Lea.Tests/Smoke.cs'
    $sourcePin=@($build.sourceInputs|Where-Object path -ceq 'Lea.Tests\Smoke.cs')
    if($sourcePin.Count -ne 1){throw 'Smoke source proof missing'}
    $null=Pin $oldSource $sourcePin[0].sha256
    $original=[IO.File]::ReadAllText($oldSource);$updated=$original
    $edits=@(
        @('var waitCount = typeof(SemaphoreSlim).GetField("m_waitCount",','var asyncHead = typeof(SemaphoreSlim).GetField("m_asyncHead",'),
        @('Ok(waitCount != null, "Framework semaphore waiter accounting','Ok(asyncHead != null, "Framework asynchronous semaphore waiter accounting'),
        @('(int)waitCount.GetValue(gate) > 0, "image decoder is blocked on the held gate"','asyncHead.GetValue(gate) != null, "image decoder is asynchronously queued on the held gate"')
    )
    foreach($pair in $edits){if([regex]::Matches($updated,[regex]::Escape($pair[0])).Count -ne 1){throw 'Ambiguous test overlay'};$updated=$updated.Replace($pair[0],$pair[1])}
    $inverse=$updated
    for($i=$edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($edits[$i][1],$edits[$i][0])}
    if($inverse -cne $original){throw 'Test overlay inverse differs'}
    $visualProof=Join-Path $workspace 'continuity-evidence/nested-flow-visuals-v1/result.json';$null=Pin $visualProof 'CABAC7356A80E3464C95B49106B934930E97C9F705DB23FE3CC815A7CD444660'
    $v=Get-Content -LiteralPath $visualProof -Raw|ConvertFrom-Json
    if(!$v.passed -or !$v.inputsUnchanged -or !$v.candidateUnchanged -or $v.ownerAttention){throw 'Nested model/geometry proof incomplete'}
    $helper=Join-Path $PSScriptRoot 'New-OrderedFlowSmokeSource.ps1';$null=Pin $helper;. $helper
    $updated=New-OrderedFlowSmokeSource $updated
    $result.nestedModelOverlayInverseVerified=$true
    $titleProof=Join-Path $workspace 'continuity-evidence/title-metadata-visuals-v1/result.json';$null=Pin $titleProof '04CB8E02EAC3FDEF82D2006C7E6C504C3A8913467DB4926EDFA5701A6D2103D4'
    $title=Get-Content -LiteralPath $titleProof -Raw|ConvertFrom-Json
    if(!$title.passed -or !$title.inputsUnchanged -or !$title.candidateUnchanged -or $title.ownerAttention -or !$title.baselineRegressionReproduced -or !$title.viewBuild.titleOverlayInverseVerified){throw 'Title behavior proof incomplete'}
    $titleHelper=Join-Path $PSScriptRoot 'New-TitleMetadataSmokeSource.ps1';$null=Pin $titleHelper;. $titleHelper
    $updated=New-TitleMetadataSmokeSource $updated
    $result.titleCountOverlayInverseVerified=$true
    $overlay=Join-Path $outRoot 'Smoke.cs';[IO.File]::WriteAllText($overlay,$updated,[Text.UTF8Encoding]::new($false));$null=Pin $overlay
    $options=[Collections.Generic.List[string]]::new()
    foreach($line in Get-Content -LiteralPath (Join-Path $native 'Lea.Tests.rsp')){
        if($line.StartsWith('/out:')){$line='/out:"'+(Join-Path $result.candidate 'Lea.Tests.exe')+'"'}
        elseif($line.StartsWith('"')){
            $path=$line.Trim('"');$relative=[IO.Path]::GetRelativePath((Join-Path $native 'source'),$path)
            $pin=@($build.sourceInputs|Where-Object path -ceq $relative)
            if($pin.Count -ne 1){throw 'Missing compiler source pin'};$null=Pin $path $pin[0].sha256
            if($path -ceq $oldSource){$line='"'+$overlay+'"'}
        }elseif($line.StartsWith('/reference:')){
            $path=$line.Substring(11).Trim('"');$name=[IO.Path]::GetFileName($path)
            if($name -cin @('Lea.Engine.dll','Lea.Win.dll')){$path=Join-Path $result.candidate $name}
            if(!$path.StartsWith($workspace+'\',[StringComparison]::OrdinalIgnoreCase)){
                $observed=(Get-FileHash -LiteralPath $path).Hash
                $local=Join-Path $outRoot ('references/'+$name)
                $null=New-Item -ItemType Directory -Path ([IO.Path]::GetDirectoryName($local)) -Force
                Copy-Item -LiteralPath $path -Destination $local
                $null=Pin $local $observed;$path=$local
            }else{$null=Pin $path}
            $line='/reference:"'+$path+'"'
        }
        $options.Add($line)
    }
    $rsp=Join-Path $outRoot 'tests.rsp';[IO.File]::WriteAllLines($rsp,$options);$hash=Pin $rsp
    $wrapper=Join-Path $PSScriptRoot 'Invoke-PublicCompiler.20261022.ps1';$null=Pin $wrapper '1A04EE9A191D522955C633F97A24C9C0921A5738F76D0DF36DB1C92A574EEA03'
    $compile=Run 'compile-tests' (Get-Process -Id $PID).Path @('-NoProfile','-File',('"'+$wrapper+'"'),'-ResponseFile',('"'+$rsp+'"'),'-ExpectedResponseSha256',$hash,'-ExpectedSourceSha256','D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB') 120
    if(!$compile.passed){throw 'Matched tests failed compilation'}
    $result.testOverlayInverseVerified=$true
    $fixtures=Join-Path $outRoot 'fixtures'
    foreach($file in $build.sourceInputs|Where-Object{$_.path.StartsWith('LeaForge\test-pages\',[StringComparison]::Ordinal) -or $_.path.StartsWith('assets\',[StringComparison]::Ordinal)}){
        CopyExact (Join-Path $native ('source/'+$file.path)) (Join-Path $fixtures $file.path) $file.sha256
    }
    if(!(Test-Path -LiteralPath (Join-Path $fixtures 'assets/lea.ico'))){throw 'Required pinned image fixture missing'}
    $policy=Join-Path $PSScriptRoot 'Test-DecoderAclDelta.ps1';$null=Pin $policy '15F46DBC7F4D4AD5B816DDFA1FE7213D02CAE1EDE79844AFBD904F281299E71E';. $policy
    $result.before=@(Get-ChildItem -LiteralPath $result.candidate -File|ForEach-Object{@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash;sddl=(Get-Acl -LiteralPath $_.FullName).Sddl}})
    $run=Run 'full-core' (Join-Path $result.candidate 'Lea.Tests.exe') @(('"'+$fixtures+'"')) 300
    $lines=@(Get-Content -LiteralPath $run.stdout)
    $result.fullSuiteFooter=$lines.Count -gt 0 -and $lines[-1].StartsWith('Lea smoke passed: bounded sticky positioning with compact k,')
    $result.passed=$run.passed -and $result.fullSuiteFooter
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.behaviorPassed=$result.passed
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -gt 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.inputsUnchanged=@($inputs|Where-Object{(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.bytesUnchanged=if($result.before){@($result.before|Where-Object{(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0}else{$false}
    $changes=@(); if($result.before){$changes=@($result.before|Where-Object{(Get-Acl -LiteralPath $_.path).Sddl -cne $_.sddl})}
    $result.aclPolicyAccepted=$changes.Count -eq 0
    if($changes.Count -eq 1){$result.aclPolicyAccepted=Test-DecoderAclDelta -Path $changes[0].path -ExpectedDecoderPath (Join-Path $result.candidate 'Lea.Image.exe') -Before $changes[0].sddl -After (Get-Acl -LiteralPath $changes[0].path).Sddl -ProfileName 'Lea.Image.Decoder'}
    $result.aclDeltaPaths=@($changes|ForEach-Object path)
    $result.passed=$result.passed -and !$result.ownerAttention -and $result.inputsUnchanged -and $result.bytesUnchanged -and $result.aclPolicyAccepted
    $global:LeaRebuiltCoreResult=$result
    [IO.File]::WriteAllText((Join-Path $outRoot 'result.json'),($result|ConvertTo-Json -Depth 7))
}
[pscustomobject]$result|Select-Object passed,ownerAttention,error|ConvertTo-Json
