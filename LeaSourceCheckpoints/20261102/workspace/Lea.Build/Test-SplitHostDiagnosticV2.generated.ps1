param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$outRoot=Join-Path $workspace 'continuity-evidence/split-host-diagnostic-v2'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained foreground owner required'}
if(Test-Path -LiteralPath $outRoot){throw 'Preserve split host attempt'}
$utility=Join-Path $workspace 'continuity-evidence/native-launch-timing-v1/Test-CompositeCandidate.v2.ps1'
if((Get-FileHash -LiteralPath $utility).Hash -cne 'EB00AD17B8078A6CC2F418733BA5D675FC84EF49EED433A527FAC5498DD5ECEF'){throw 'Process utility differs'}
$tokens=$null;$errors=$null;$ast=[Management.Automation.Language.Parser]::ParseFile($utility,[ref]$tokens,[ref]$errors)
foreach($name in @('Space','NewOutputReader','PumpOutput','Run')){
    $defs=@($ast.FindAll({param($n)$n -is [Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -ceq $name},$false))
    if($errors.Count -or $defs.Count -ne 1){throw 'Process utility shape differs'}
    . ([scriptblock]::Create($defs[0].Extent.Text))
}
if(Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue){throw 'Fresh owner required'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new()
function Hold([string]$path,[string]$expected){
    $stream=[IO.File]::Open($path,'Open','Read','Read');$leases.Add($stream)
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) -cne $expected){throw ('Input differs: '+$path)}
    $inputs.Add(@{path=$path;sha256=$expected})
}
$result=[ordered]@{passed=$false;candidate=(Join-Path $outRoot 'runtime');inputs=$inputs;runs=$runs;error=$null;productionChanged=$false;limitsChanged=$false;scope='Failure-only diagnostic launcher overlay on the failed actual-host candidate; not a replacement host campaign or reliability proof'}
Space;$null=New-Item -ItemType Directory -Path $result.candidate
try{
    if(@(Get-Process -Name 'Firespider*','Lea.Firespider.Boot','Lea.Script.Tests','Lea.Script.Worker','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'Do not overlap host/script tests'}
    Hold $PSCommandPath (Get-FileHash -LiteralPath $PSCommandPath).Hash
    Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $outRoot 'runner.ps1')
    $proof=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/host-current-flow-v1/result.json'
    Hold $proof 'EF3B0F8835FC3B882E5E42B79F2F9F0085663EF72396A70F7F87D0B29B4358D7'
    $hostRun=Get-Content -LiteralPath $proof -Raw|ConvertFrom-Json
    if(!$hostRun.originalInputsUnchanged -or !$hostRun.candidateBytesUnchanged -or $hostRun.ownerAttention -or $hostRun.passed){throw 'Failed host binding incomplete'}
    foreach($file in $hostRun.before){
        Hold $file.path $file.sha256
        $relative=[IO.Path]::GetRelativePath($hostRun.candidate,$file.path)
        if([IO.Path]::IsPathRooted($relative) -or $relative.Split('\') -contains '..'){throw 'Runtime path escape'}
        $to=Join-Path $result.candidate $relative;$null=New-Item -ItemType Directory -Path (Split-Path $to) -Force
        $input=[IO.File]::OpenRead($file.path);$output=[IO.File]::Open($to,'CreateNew','Write','None')
        try{$input.CopyTo($output)}finally{$input.Dispose();$output.Dispose()}
    }
    $buildProof=Join-Path $workspace 'continuity-evidence/small-engine-v1/harness-build-result.json'
    Hold $buildProof '7D16D055C6138A86D92495D6B34E137FDEDDBED885596DBA9DEA17FD9AE3DF0F'
    $build=Get-Content -LiteralPath $buildProof -Raw|ConvertFrom-Json
    if(!$build.completed -or !$build.compilerExitObserved -or $build.compilerExitCode -ne 0){throw 'Original launcher source binding incomplete'}
    $source=Join-Path $workspace 'continuity-evidence/small-engine-v1/harness-source/Boot.cs'
    Hold $source '55F6F5F0F0C9036020B5133A9A2788B9E72A2FB5FFDAB1DE9DDF79EF8B4EA1AA'
    $original=[IO.File]::ReadAllText($source)
    $old=@'
                Check(splitPids.All(value => value > 0) && splitPids[0] != splitPids[1] && !splitPids.Contains(ScriptPid(view)),
                    "split panes do not have independent isolated sessions");
'@
    $new=@'
                var independent = splitPids.All(value => value > 0) && splitPids[0] != splitPids[1] && !splitPids.Contains(ScriptPid(view));
                if (!independent)
                {
                    Console.WriteLine("Script split diagnostic PIDs: left=" + splitPids[0] + "; right=" + splitPids[1] + "; source=" + ScriptPid(view));
                    foreach (var pane in new[] { left, right, view })
                    {
                        try
                        {
                            var state = ScriptState(pane);
                            Console.WriteLine("Script split diagnostic state: " + state.Substring(0, Math.Min(state.Length, 1024)));
                        }
                        catch (Exception diagnostic) { Console.WriteLine("Script split diagnostic unavailable: " + diagnostic.GetType().Name); }
                    }
                }
                Check(independent, "split panes do not have independent isolated sessions");
'@
    $old=$old.Replace("`r`n","`n");$new=$new.Replace("`r`n","`n")
    $original=$original.Replace("`r`n","`n")
    if([regex]::Matches($original,[regex]::Escape($old)).Count -ne 1){throw 'Split check overlay anchor differs'}
    $updated=$original.Replace($old,$new)
    if($updated.Replace($new,$old) -cne $original){throw 'Split diagnostic inverse differs'}
    $overlay=Join-Path $outRoot 'Boot.cs';[IO.File]::WriteAllText($overlay,$updated,[Text.UTF8Encoding]::new($false));Hold $overlay (Get-FileHash -LiteralPath $overlay).Hash
    $options=[Collections.Generic.List[string]]::new()
    $corlib=Join-Path (Split-Path $build.compiler.path) 'mscorlib.dll'
    Hold $corlib (Get-FileHash -LiteralPath $corlib).Hash
    $options.Add('/nostdlib+')
    $options.Add('/reference:"'+$corlib+'"')
    foreach($arg in $build.compilerArguments){
        if($arg -ceq '/noconfig'){continue}
        if($arg.StartsWith('/out:')){$arg='/out:"'+(Join-Path $result.candidate 'Lea.Firespider.Boot.exe')+'"'}
        elseif($arg.StartsWith('/reference:')){
            $path=$arg.Substring(11)
            $name=[IO.Path]::GetFileName($path)
            if($name -cin @('Lea.Engine.dll','Lea.Win.dll')){$path=Join-Path $result.candidate $name}
            Hold $path (Get-FileHash -LiteralPath $path).Hash;$arg='/reference:"'+$path+'"'
        }elseif($arg.EndsWith('.cs')){
            if($arg -ceq $source){$arg=$overlay}else{
                $pin=@($build.protectedInputs|Where-Object path -ceq $arg)
                if($pin.Count -ne 1){throw 'Launcher source pin missing'};Hold $arg $pin[0].sha256
            }
            $arg='"'+$arg+'"'
        }
        $options.Add($arg)
    }
    $rsp=Join-Path $outRoot 'compile.rsp';[IO.File]::WriteAllLines($rsp,$options);$hash=(Get-FileHash -LiteralPath $rsp).Hash;Hold $rsp $hash
    $wrapper=Join-Path $PSScriptRoot 'Invoke-PublicCompiler.20261022.ps1';Hold $wrapper '1A04EE9A191D522955C633F97A24C9C0921A5738F76D0DF36DB1C92A574EEA03'
    $compile=Run 'compile-launcher' (Get-Process -Id $PID).Path @('-NoProfile','-File',('"'+$wrapper+'"'),'-ResponseFile',('"'+$rsp+'"'),'-ExpectedResponseSha256',$hash,'-ExpectedSourceSha256','D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB') 120
    if(!$compile.passed){throw 'Diagnostic launcher compilation failed'}
    $result.testOverlayInverseVerified=$true
    $result.before=@(Get-ChildItem -LiteralPath $result.candidate -File -Recurse|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash;sddl=(Get-Acl -LiteralPath $_.FullName).Sddl}})
    $run=Run 'host-script-diagnostic' (Join-Path $result.candidate 'Lea.Firespider.Boot.exe') @(('"'+$result.candidate+'"'),'script') 90
    $result.behaviorPassed=$run.passed -and @((Get-Content -LiteralPath $run.stdout)|Where-Object {$_.StartsWith('PASS Lea boot: script;')}).Count -eq 1
    $result.passed=$result.behaviorPassed
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.candidateBytesUnchanged=if($result.before){@($result.before|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0}else{$false}
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -gt 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and $result.candidateBytesUnchanged -and !$result.ownerAttention
    if($result.ownerAttention){$global:LeaSplitDiagnosticInputLeases=$leases}else{foreach($lease in $leases){$lease.Dispose()}}
    [IO.File]::WriteAllText((Join-Path $outRoot 'result.json'),($result|ConvertTo-Json -Depth 7))
    [pscustomobject]$result|Select-Object passed,ownerAttention,error|ConvertTo-Json
}
