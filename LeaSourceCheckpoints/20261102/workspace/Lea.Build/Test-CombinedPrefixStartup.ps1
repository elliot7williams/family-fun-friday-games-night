param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$stage='I:\LeaBuildStorage\sdk-full-script-assembly-20261101-v7'
$projected='I:\LeaBuildStorage\combined-prefix-startup-source-20261101-v3'
$out='I:\LeaBuildStorage\combined-prefix-startup-build-20261101-v1'
$candidate=Join-Path $root 'continuity-evidence/combined-prefix-startup-runtime-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Nonadmin required'}
if((Get-PSDrive C).Free -lt 600MB -or (Test-Path $out) -or (Test-Path $candidate) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/output/candidate and reserve required'}
if(@(Get-Process 'Lea.Script.Tests','Lea.Script.Worker','CombinedWorkerStartupProbe','Firespider*' -ErrorAction SilentlyContinue).Count){throw 'No native test/browser overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$path,[string]$pin){
    $item=Get-Item -LiteralPath $path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse prefix input refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s);$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($pin -and $hash -cne $pin){throw ('Prefix input differs: '+$path)};$inputs.Add(@{path=$path;sha256=$hash})
}
function Proof([string]$path,[string]$hash){Hold $path $hash;return Get-Content -LiteralPath $path -Raw|ConvertFrom-Json}
New-Item -ItemType Directory -Path $out|Out-Null
$result=@{diagnosticCaptured=$false;fullSuitePassed=$false;startupFailureResolved=$false;engineChanged=$false;limitsChanged=$false;inputs=$inputs;runs=$runs;before=@();originalBefore=@();entryFixtureBindingNotRequalified=$true}
try{
    Hold $PSCommandPath;Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'runner.ps1')
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA';Add-Type -Path $owned
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00';. $helper
    $assembly=Proof (Join-Path $stage 'assembly.json') 'DD8BAED18A7A189FD339A3D925669F4CAB076F58F717D2369AC37253C365AEFC'
    $projection=Proof (Join-Path $projected 'source.json') 'B04B2DEE1525FE6F3A8FF81A572FCA727F62CD3FD6F99262580040118DA2A040'
    $priorBuild=Proof 'I:\LeaBuildStorage\sdk-full-script-compile-20261101-v7\result.json' '0995B44F7B12515DCEA7D98D8E1C3971961B63D3524F405B705463A9990518F4'
    $audit=Proof 'I:\LeaBuildStorage\combined-worker-startup-audit-20261101-v1\result.json' '3F5C097B3BB9834979267949F10603121838D7AEE4812BEF28A83721E4CA5ADB'
    $original=Proof (Join-Path $root 'continuity-evidence/sdk-full-script-owned-run-v7/result.json') 'CBECBD7932C23EF6FF187ED115A43D792528FE36B92675D4B1456302D2DBEB65'
    if(!$assembly.verified -or !$projection.verified -or !$projection.originalAssertionPreserved -or !$projection.diagnosticOnly -or $projection.engineChanged -or $projection.files.Count -ne 3 -or $projection.originalPrefixPhases -ne 29 -or $projection.targetPhase -ne 29 -or $projection.testBudgetMs -ne 300000 -or $projection.startupBudgetMs -ne 5000 -or $projection.cleanupBudgetMs -ne 5000 -or !$priorBuild.passed -or !$priorBuild.inputsUnchanged -or $priorBuild.ownerAttention -or !$audit.verified -or $audit.nativeControls -ne 13 -or $audit.reportNegativeControls -ne 5 -or $audit.samplerNegativeControls -ne 3 -or $original.passed -or !$original.inputsUnchanged -or $original.ownerAttention -or $original.changedFiles.Count -or $original.before.Count -ne 65){throw 'Prefix prerequisites incomplete'}
    foreach($file in $projection.files){Hold $file.path $file.sha256;Hold $file.origin $file.originalSha256;if(!$file.inverseVerified -or $file.negativeControls -ne 3){throw 'Source inverse/negative proof missing'}}
    $result.originalBefore=$original.before
    foreach($file in $original.before){Hold $file.path $file.sha256;if((Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){throw 'Original failure ACL differs'}}
    foreach($file in $assembly.files){Hold (Join-Path $stage $file.path) $file.sha256}
    $recipe='I:\LeaBuildStorage\sdk-full-script-compile-20261101-v7\tests.rsp';Hold $recipe '1559FFC3DC09FAAE7A0602BB4534C3DEE1CD823A8F6CFC527B7049E3A63DC381'
    $options=[Collections.Generic.List[string]]::new();$sourceCount=0;$replaced=0;$references=0
    foreach($line in Get-Content $recipe){
        if($line -match '^"(.+\.cs)"$'){
            $from=$Matches[1];$name=[IO.Path]::GetFileName($from)
            $swap=@($projection.files|Where-Object {[IO.Path]::GetFileName($_.path) -ceq $name -and $name -in 'Worker.cs','Program.cs'})
            if($swap.Count -eq 1){$from=$swap[0].path;$replaced++}elseif($swap.Count){throw 'Ambiguous suite projection'}
            $options.Add('"'+$from+'"');$sourceCount++;continue
        }
        if($line -match '^/reference:"(.+)"$'){
            $from=$Matches[1];$pin=@($priorBuild.inputs|Where-Object path -ceq $from|Select-Object -ExpandProperty sha256 -Unique)
            if($pin.Count -ne 1){throw 'Reference pin missing'};Hold $from $pin[0];$references++;$options.Add($line);continue
        }
        if($line.StartsWith('/out:')){$options.Add('/out:"'+(Join-Path $out 'Lea.Script.Tests.exe')+'"');continue}
        if($line.StartsWith('/pdb:')){$options.Add('/pdb:"'+(Join-Path $out 'Lea.Script.Tests.pdb')+'"');continue}
        if($line.StartsWith('/define:')){$options.Add($line+';LEA_THREAD_V2');continue}
        if(!$line.StartsWith('/')){throw 'Unknown compiler recipe member'};$options.Add($line)
    }
    if($sourceCount -ne 60 -or $replaced -ne 2 -or !$references){throw 'Original 60 source inventory differs'}
    foreach($entry in @(@((Join-Path $projected 'CombinedPrefixStartupProbe.cs'),''),@((Join-Path $PSScriptRoot 'CombinedPrefixComplete.cs'),''),@((Join-Path $PSScriptRoot 'ExternalProcessCounters.cs'),'128CCB2A4B1E97D6D98B37A6E6A9B70AF7FF15F3DFCBA66CAA274E2D8C95B886'),@((Join-Path $PSScriptRoot 'OwnedThreadSnapshotV2.cs'),'B1904ABBD7E56BC546BBAECFB88113374135825873FBD97222CF31CEA3D30858'))){Hold $entry[0] $entry[1];$options.Add('"'+$entry[0]+'"')}
    $options.Add('/main:Lea.Script.Tests.Program')
    $rsp=Join-Path $out 'tests.rsp';[IO.File]::WriteAllLines($rsp,$options,[Text.UTF8Encoding]::new($false));Hold $rsp
    $dotnet='C:\Program Files\dotnet\dotnet.exe';Hold $dotnet;$tools='C:\Program Files\dotnet\sdk\8.0.427\Roslyn\bincore'
    foreach($name in @('csc.dll','csc.deps.json','csc.runtimeconfig.json','Microsoft.CodeAnalysis.dll','Microsoft.CodeAnalysis.CSharp.dll')){Hold (Join-Path $tools $name)}
    $compiled=Invoke-OwnedTestRun -Name 'compile-prefix' -Application $dotnet -Arguments @(('"'+(Join-Path $tools 'csc.dll')+'"'),('@"'+$rsp+'"')) -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$compiled.passed){throw 'Prefix compilation failed'}
    $stagingPath=Join-Path $root 'continuity-evidence/sdk-full-script-suite-v7/staging.json'
    $staging=Proof $stagingPath 'F693258746A0E08024A72011F9E5E6EE242766FAD468BE4849FF4BCC1C0BFBE8'
    $rows=[Collections.Generic.List[object]]::new()
    foreach($file in $staging.files){if($file.path -notin 'runtime/Lea.Script.Tests.exe','runtime/Lea.Script.Tests.pdb'){$rows.Add(@{path=$file.path;origin=(Join-Path (Split-Path $stagingPath) $file.path);sha256=$file.sha256})}}
    foreach($name in @('Lea.Script.Tests.exe','Lea.Script.Tests.pdb')){$path=Join-Path $out $name;Hold $path;$rows.Add(@{path=('runtime/'+$name);origin=$path;sha256=(Get-FileHash $path).Hash})}
    $bytes=0L;foreach($row in $rows){Hold $row.origin $row.sha256;$bytes+=(Get-Item -LiteralPath $row.origin).Length}
    if((Get-Volume -DriveLetter C).FileSystem -cne 'NTFS' -or (Get-PSDrive C).Free -lt (600MB+$bytes)){throw 'Original post-staging reserve required'}
    foreach($row in $rows){
        if([IO.Path]::IsPathRooted($row.path) -or $row.path.Contains(':') -or $row.path.Split('/') -contains '..'){throw 'Fresh candidate path escape'}
        $to=Join-Path $candidate $row.path;New-Item -ItemType Directory -Path (Split-Path $to) -Force|Out-Null
        Copy-Item -LiteralPath $row.origin -Destination $to;Hold $to $row.sha256
    }
    $result.before=@(foreach($row in $rows|Where-Object {$_.path.StartsWith('runtime/')}){$path=Join-Path $candidate $row.path;@{path=$path;sha256=$row.sha256;sddl=(Get-Acl -LiteralPath $path).Sddl}})
    $result.stagedFiles=$rows.Count;$result.stagedBytes=$bytes;$result.compiledSources=64
    $tested=Invoke-OwnedTestRun -Name 'observe-prefix' -Application (Join-Path $candidate 'runtime/Lea.Script.Tests.exe') -Arguments @(('"'+(Join-Path $candidate 'fixtures')+'"')) -WorkingDirectory (Join-Path $candidate 'runtime') -OutputDirectory $out -Seconds 300 -Records $runs
    $lines=@(Get-Content $tested.stdout);$stderr=@(Get-Content $tested.stderr)
    $reports=@($lines|Where-Object {$_.StartsWith('COMBINED_WORKER_STARTUP ',[StringComparison]::Ordinal)})
    if($reports.Count -ne 1){throw 'Prefix did not reach a unique startup capture'}
    $report=$reports[0].Substring('COMBINED_WORKER_STARTUP '.Length)|ConvertFrom-Json
    $result.observation=$report
    if(!$report.valid -or !$report.observerRetired -or !$report.exited -or !$report.traceRestored -or $report.sampleError -or $report.overflow -or !$report.samples.Count -or $report.samples.Count -gt 60){throw 'Prefix capture incomplete'}
    $phases=@($stderr|Where-Object {$_ -cmatch '^LEA_SUITE_PHASE '})
    if($phases.Count -ne 59){throw 'Exact original prefix phase count differs'}
    for($i=0;$i -lt 29;$i++){if($phases[2*$i] -cnotmatch ('^LEA_SUITE_PHASE [0-9]+ '+$i+' begin$') -or $phases[2*$i+1] -cnotmatch ('^LEA_SUITE_PHASE [0-9]+ '+$i+' end$')){throw 'Prefix phase order differs'}}
    if($phases[-1] -cnotmatch '^LEA_SUITE_PHASE [0-9]+ 29 begin$' -or @($stderr|Where-Object {$_ -ceq 'LEA_ALL_STARTUP_TRACE enabled'}).Count -ne 1 -or @($stderr|Where-Object {$_ -ceq 'LEA_ALL_STARTUP_TRACE restored=true'}).Count -ne 1){throw 'Target phase or trace restoration differs'}
    $stops=@($lines|Where-Object {$_ -ceq 'Diagnostic stop after isolated computed-script assertion; not full suite.'}).Count
    if($report.scriptPassed){if(!$tested.passed -or $stops -ne 1 -or $report.mutations -ne 6 -or $report.title -cne 'Lea 7'){throw 'Successful diagnostic terminal differs'}}
    elseif($tested.exitCode -ne 1 -or $tested.timedOut -or $stops -or @($stderr|Where-Object {$_ -cmatch '^System.InvalidOperationException: FAIL isolated computed script:'}).Count -ne 1){throw 'Failed computed assertion terminal differs'}
    $result.prefixVerified=$true;$result.computedPassed=$report.scriptPassed;$result.diagnosticCaptured=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.changedFiles=@(foreach($file in @($result.before)+@($result.originalBefore)){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256 -or (Get-Acl -LiteralPath $file.path).Sddl -cne $file.sddl){$file.path}})
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.diagnosticCaptured=$result.diagnosticCaptured -and $result.inputsUnchanged -and !$result.changedFiles.Count -and !$result.ownerAttention
    if(!$result.ownerAttention){foreach($s in $holds){$s.Dispose()}}else{$global:LeaCombinedPrefixInputLeases=$holds}
    $result|ConvertTo-Json -Depth 15|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object diagnosticCaptured,prefixVerified,computedPassed,inputsUnchanged,ownerAttention,error|ConvertTo-Json
}
