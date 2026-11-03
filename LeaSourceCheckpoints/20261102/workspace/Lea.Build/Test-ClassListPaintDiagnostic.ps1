param([switch]$KeepForegroundHost,[ValidateSet('diagnostic','contract','startup','receiver-diagnostic','receiver-contract')][string]$Mode='diagnostic',[ValidateRange(1,9)][int]$Attempt=1,[switch]$NetworkProbeDiagnostic,[string]$ExpectedDiagnosticNativeReceipt)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
$outRoot=Join-Path $workspace $(if($NetworkProbeDiagnostic){'continuity-evidence/network-probe-startup-v'+$Attempt}else{'continuity-evidence/classlist-paint-'+$Mode+'-v'+$Attempt})
$receiverMode=$Mode -in @('receiver-diagnostic','receiver-contract')
$contractMode=$Mode -in @('contract','receiver-contract')
if($NetworkProbeDiagnostic -and $Mode -ne 'startup'){throw 'Network diagnostic is startup-only'}
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(Test-Path -LiteralPath $outRoot){throw 'Preserve diagnostic attempt'}
$id=[Security.Principal.WindowsIdentity]::GetCurrent()
if(([Security.Principal.WindowsPrincipal]::new($id)).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Normal nonadmin required'}
$utility=Join-Path $workspace 'continuity-evidence/native-launch-timing-v1/Test-CompositeCandidate.v2.ps1'
if((Get-FileHash $utility).Hash -cne 'EB00AD17B8078A6CC2F418733BA5D675FC84EF49EED433A527FAC5498DD5ECEF'){throw 'Runner differs'}
$t=$null;$e=$null;$ast=[Management.Automation.Language.Parser]::ParseFile($utility,[ref]$t,[ref]$e)
foreach($name in @('Space','NewOutputReader','PumpOutput','Run')){
    $defs=@($ast.FindAll({param($n)$n -is [Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -ceq $name},$false))
    if($e.Count -or $defs.Count -ne 1){throw 'Runner shape differs'}
    . ([scriptblock]::Create($defs[0].Extent.Text))
}
if(Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue){throw 'Fresh owner required'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new()
function Hold([string]$path,[string]$expected){
    $s=[IO.File]::Open($path,'Open','Read','Read');$leases.Add($s)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if(!$expected -or $hash -cne $expected){throw ('Input differs: '+$path)}
    $inputs.Add(@{path=$path;sha256=$hash})
}
$result=[ordered]@{completed=$false;inverseVerified=$false;assertionPreserved=$false;mode=$Mode;error=$null;inputs=$inputs;runs=$runs;scope='Diagnostic or scoped classList authored paint contract; unchanged engine/view/worker/shared paint assertion; not full qualification'}
Space;$null=New-Item -ItemType Directory -Path $outRoot
try{
    Hold $PSCommandPath (Get-FileHash $PSCommandPath).Hash
    Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $outRoot 'runner.ps1')
    if(@(Get-Process 'Lea.Script.Worker','Lea.Script.Tests','Lea.Firespider.Boot','Firespider*' -ErrorAction SilentlyContinue).Count){throw 'No overlapping tests/browser'}
    $buildPath=Join-Path $workspace $(if($Mode -eq 'startup'){'continuity-evidence/temporal-startup-v1/current-native-script-suite-v2/result.json'}else{'continuity-evidence/temporal-startup-v1/current-native-script-suite-v1/result.json'})
    $buildHash=if($Mode -eq 'startup'){'0200372EDD91869A82344137C6393D767A6B4019299BF18B3FD6AE9A1CDFCD03'}else{'916925B96F63ECFE75526F6BCBF742326441FF840FFD137C3CCE9CC56DA6C7CB'}
    Hold $buildPath $buildHash
    $build=Get-Content $buildPath -Raw|ConvertFrom-Json
    if(!$build.passed -or !$build.inputsUnchanged -or $build.ownerAttention){throw 'Build provenance incomplete'}
    if($NetworkProbeDiagnostic){
        if($ExpectedDiagnosticNativeReceipt -cnotmatch '^[A-F0-9]{64}$'){throw 'Exact diagnostic native receipt required'}
        $nativePath=Join-Path $workspace 'continuity-evidence/worker-network-probe-native-v1/result.json';Hold $nativePath $ExpectedDiagnosticNativeReceipt
        $native=Get-Content $nativePath -Raw|ConvertFrom-Json
        if(!$native.passed -or !$native.inputsUnchanged -or $native.ownerAttention -or $native.productionAdopted -or !$native.ilComparison.TokenMapsIdentical -or !$native.ilComparison.UserStringsIdentical -or $native.ilComparison.ChangedMethods.Count -ne 0){throw 'Diagnostic native proof incomplete'}
        $result.scope='Focused removal with timing-only network diagnostic worker; original confinement and deadlines, not production/full qualification'
        $result.diagnosticNativeReceiptSha256=$ExpectedDiagnosticNativeReceipt
    }
    function KnownHash([string]$path){
        $values=@(@($build.inputs)+@($build.files)|Where-Object path -ceq $path|ForEach-Object sha256|Sort-Object -Unique)
        if($values.Count -ne 1){throw ('Missing provenance: '+$path)}
        return $values[0]
    }
    $runtime=Join-Path $outRoot 'runtime';$null=New-Item -ItemType Directory $runtime
    foreach($file in $build.files){
        Hold $file.path $file.sha256
        $relative=[IO.Path]::GetRelativePath($build.runtime,$file.path)
        if([IO.Path]::IsPathRooted($relative) -or $relative.Split('\') -contains '..'){throw 'Runtime path escape'}
        $to=Join-Path $runtime $relative;$null=New-Item -ItemType Directory (Split-Path $to) -Force
        $from=$file.path;$expected=$file.sha256
        if($NetworkProbeDiagnostic -and $relative -ceq 'ScriptWorker\Lea.Script.Worker.dll'){
            $from=Join-Path $workspace 'continuity-evidence/worker-network-probe-native-v1/Lea.Script.Worker.dll';$expected=$native.sha256
            Hold $from $expected;$result.diagnosticWorkerSha256=$expected
        }
        Copy-Item -LiteralPath $from -Destination $to
        if((Get-FileHash $to).Hash -cne $expected){throw 'Copied runtime differs'}
    }
    $image=Join-Path $runtime 'Lea.Image.exe'
    if($NetworkProbeDiagnostic -and !$result.diagnosticWorkerSha256){throw 'Diagnostic worker was not selected'}
    if((Get-Acl $image).Sddl.Contains('S-1-15-2-3597768014-2038560105-1077951562-531713394-1456559389-3372869253-1355503848')){throw 'Fresh fixture must not inherit decoder grant'}
    $viewSource=Join-Path $build.candidate 'source/Lea.Script.Tests/View.cs'
    Hold $viewSource (KnownHash $viewSource)
    $original=[IO.File]::ReadAllText($viewSource)
    $anchor='                Check(colored > 300, "native event page renders nonblank at " + view.Width + "px");'
    if([regex]::Matches($original,[regex]::Escape($anchor)).Count -ne 1){throw 'Expected one paint assertion'}
    $diagnostic=@'
                Console.WriteLine("PAINT_DIAGNOSTIC width=" + view.Width + " height=" + view.Height + " colored=" + colored + " scroll=" + view.Sy);
                foreach (Control control in view.Controls)
                    Console.WriteLine("PAINT_CONTROL " + control.GetType().Name + " visible=" + control.Visible + " bounds=" + control.Bounds);
                if (output != null) bitmap.Save(output, System.Drawing.Imaging.ImageFormat.Png);
'@
    $updated=$original.Replace($anchor,$diagnostic+[Environment]::NewLine+$anchor)
    if($updated.Replace($diagnostic+[Environment]::NewLine+$anchor,$anchor) -cne $original){throw 'Diagnostic inverse differs'}
    $result.inverseVerified=$true;$result.assertionPreserved=$true
    $classSource=Join-Path $build.candidate $(if($receiverMode){'source/Lea.Script.Tests/ElementReceivers.cs'}else{'source/Lea.Script.Tests/ClassLists.cs'})
    if($contractMode){
        Hold $classSource (KnownHash $classSource)
        $helper=Join-Path $workspace 'Lea.Build/New-ClassListPaintContractSource.ps1';Hold $helper (Get-FileHash $helper).Hash
        $rootView=Join-Path $workspace 'Lea.Script.Tests/View.cs';Hold $rootView (Get-FileHash $rootView).Hash
        . $helper
        $contract=New-ClassListPaintContractSource $original ([IO.File]::ReadAllText($classSource)) ([IO.File]::ReadAllText($rootView))
        if(!$contract.inverseVerified){throw 'Contract inverse missing'}
        $updated=$contract.view
        $classOverlay=Join-Path $outRoot ([IO.Path]::GetFileName($classSource));[IO.File]::WriteAllText($classOverlay,$contract.classes,[Text.UTF8Encoding]::new($false));Hold $classOverlay (Get-FileHash $classOverlay).Hash
    }
    $overlay=Join-Path $outRoot 'View.cs';[IO.File]::WriteAllText($overlay,$updated,[Text.UTF8Encoding]::new($false));Hold $overlay (Get-FileHash $overlay).Hash
    if($Mode -eq 'startup'){
        $removalSource=Join-Path $build.candidate 'source/Lea.Script.Tests/Removal.cs';Hold $removalSource (KnownHash $removalSource)
        $removal=[IO.File]::ReadAllText($removalSource)
        $anchors=@('                W.On(view, null, 5000);','                var page = await view.GoA(fixture);')
        $replacements=@(($anchors[0]+[Environment]::NewLine+'                using var observation = new WorkerStartupObserver(view);'),($anchors[1]+[Environment]::NewLine+'                observation.Stop();'))
        $observed=$removal
        for($i=0;$i -lt 2;$i++){if([regex]::Matches($observed,[regex]::Escape($anchors[$i])).Count -ne 1){throw 'Startup observer anchor differs'};$observed=$observed.Replace($anchors[$i],$replacements[$i])}
        $inverse=$observed;for($i=1;$i -ge 0;$i--){$inverse=$inverse.Replace($replacements[$i],$anchors[$i])}
        if($inverse -cne $removal){throw 'Startup observer inverse differs'}
        $result.startupObserverInverseVerified=$true
        $removalOverlay=Join-Path $outRoot 'Removal.cs';[IO.File]::WriteAllText($removalOverlay,$observed,[Text.UTF8Encoding]::new($false));Hold $removalOverlay (Get-FileHash $removalOverlay).Hash
    }
    $recipe=Join-Path $build.candidate 'tests.rsp';Hold $recipe (KnownHash $recipe)
    $options=[Collections.Generic.List[string]]::new();$sources=0;$changed=0
    foreach($line in [IO.File]::ReadAllLines($recipe)){
        if($line.StartsWith('"') -and $line.EndsWith('.cs"')){
            $from=$line.Trim('"');Hold $from (KnownHash $from);$sources++
            if($Mode -ne 'startup' -and $from -ceq $viewSource){$line='"'+$overlay+'"';$changed++}
            elseif($contractMode -and $from -ceq $classSource){$line='"'+$classOverlay+'"';$changed++}
            elseif($Mode -eq 'startup' -and $from -ceq $removalSource){$line='"'+$removalOverlay+'"';$changed++}
        }elseif($line.StartsWith('/out:')){$line='/out:"'+(Join-Path $runtime 'Lea.Script.Tests.exe')+'"'}
        elseif($line.StartsWith('/pdb:')){$line='/pdb:"'+(Join-Path $runtime 'Lea.Script.Tests.pdb')+'"'}
        elseif($line.StartsWith('/reference:')){
            $from=$line.Substring(11).Trim('"');Hold $from (KnownHash $from)
            $line='/reference:"'+$from.Replace($build.runtime,$runtime)+'"'
        }
        $options.Add($line)
    }
    $expectedChanges=if($contractMode){2}else{1}
    if($sources -ne 60 -or $changed -ne $expectedChanges){throw 'Source/overlay coverage differs'}
    if($Mode -eq 'startup'){
        foreach($name in @('WorkerStartupObserver.cs','ExternalProcessCounters.cs')){
            $path=Join-Path $workspace ('Lea.Build/'+$name);Hold $path (Get-FileHash $path).Hash
            $selected=Join-Path $outRoot $name;Copy-Item -LiteralPath $path -Destination $selected;Hold $selected (Get-FileHash $path).Hash
            $options.Add('"'+$selected+'"')
        }
    }
    $rsp=Join-Path $outRoot 'tests.rsp';[IO.File]::WriteAllLines($rsp,$options,[Text.UTF8Encoding]::new($false));$hash=(Get-FileHash $rsp).Hash;Hold $rsp $hash
    $wrapper=Join-Path $workspace 'Lea.Build/Invoke-PublicCompiler.20261026.ps1';Hold $wrapper 'C3EBB3801FAFB80DA031AAB69FE4837EEB4E4EECD3907F9704F9D6186FA42F0A'
    $compile=Run 'compile' (Get-Process -Id $PID).Path @('-NoProfile','-File',('"'+$wrapper+'"'),'-ResponseFile',('"'+$rsp+'"'),'-ExpectedResponseSha256',$hash,'-ExpectedSourceSha256','D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB') 120
    if(!$compile.passed){throw 'Diagnostic compilation failed'}
    foreach($file in $build.fixtureFiles){Hold $file.path $file.sha256}
    $result.before=@(Get-ChildItem $runtime -File -Recurse|ForEach-Object{@{path=$_.FullName;sha256=(Get-FileHash $_.FullName).Hash;sddl=(Get-Acl $_.FullName).Sddl}})
    $arguments=if($Mode -eq 'startup'){@(('"'+$build.fixtureSource+'"'),'--removal-view')}elseif($receiverMode){@(('"'+$build.fixtureSource+'"'),'--element-receivers',('"'+(Join-Path $outRoot 'receivers')+'"'))}else{@(('"'+$build.fixtureSource+'"'),'--classes-view',('"'+(Join-Path $outRoot 'classlist')+'"'))}
    $run=Run $(if($Mode -eq 'startup'){'native-removal-startup'}elseif($receiverMode){'element-receivers'}else{'native-classlist'}) (Join-Path $runtime 'Lea.Script.Tests.exe') $arguments 90
    $result.completed=$true;$result.behaviorPassed=$run.passed
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object{(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.changedFiles=@(foreach($file in $result.before){if((Get-FileHash $file.path).Hash -cne $file.sha256 -or (Get-Acl $file.path).Sddl -cne $file.sddl){$file}})
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -gt 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    if($result.ownerAttention){$global:LeaDiagnosticInputLeases=$leases}else{foreach($lease in $leases){$lease.Dispose()}}
    $result|ConvertTo-Json -Depth 8|Set-Content (Join-Path $outRoot 'result.json')
    [pscustomobject]$result|Select-Object completed,inverseVerified,assertionPreserved,behaviorPassed,inputsUnchanged,ownerAttention,error|ConvertTo-Json -Compress
}
