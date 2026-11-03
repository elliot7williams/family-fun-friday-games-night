param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/temporal-startup-v1'
$outRoot=Join-Path $root 'numeric-references-script-suite-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
$id=[Security.Principal.WindowsIdentity]::GetCurrent()
if(!$id.User.Value.EndsWith('-1001') -or ([Security.Principal.WindowsPrincipal]::new($id)).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Normal nonadmin required'}
if(Test-Path -LiteralPath $outRoot){throw 'Preserve build attempt'}
if(Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue){throw 'Fresh owner required'}
$utility=Join-Path $workspace 'continuity-evidence/native-launch-timing-v1/Test-CompositeCandidate.v2.ps1'
if((Get-FileHash -LiteralPath $utility).Hash -cne 'EB00AD17B8078A6CC2F418733BA5D675FC84EF49EED433A527FAC5498DD5ECEF'){throw 'Runner changed'}
$t=$null;$e=$null;$ast=[Management.Automation.Language.Parser]::ParseFile($utility,[ref]$t,[ref]$e)
foreach($name in @('Space','NewOutputReader','PumpOutput','Run')){
    $defs=@($ast.FindAll({param($n)$n -is [Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -ceq $name},$false))
    if($e.Count -or $defs.Count -ne 1){throw 'Runner shape changed'}
    . ([scriptblock]::Create($defs[0].Extent.Text))
}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new()
function Hold([string]$path,[string]$expected){
    $full=[IO.Path]::GetFullPath($path);$stream=[IO.File]::Open($full,'Open','Read','Read');$leases.Add($stream)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))
    if(!$expected -or $hash -cne $expected){throw ('Input differs: '+$full)}
    $inputs.Add(@{path=$full;sha256=$hash})
}
$result=[ordered]@{passed=$false;candidate=$outRoot;inputs=$inputs;runs=$runs;sources=[Collections.Generic.List[object]]::new();error=$null;suiteExecuted=$false;productionChanged=$false;scope='Compile-only timing diagnostic, not rebound full-suite validation'}
Space;if((Get-PSDrive C).Free -lt 600MB){throw 'Relocation reserve required'};$null=New-Item -ItemType Directory $outRoot
try{
    Hold $PSCommandPath (Get-FileHash -LiteralPath $PSCommandPath).Hash
    Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $outRoot 'build.ps1')
    $base=Join-Path $workspace 'continuity-evidence/bootstrap-descriptor-batches-v1/source-cross-module-suite-v2'
    $buildPath=Join-Path $base 'result.json';Hold $buildPath '779061CB3422B80038653503E7AA375521E5A868DBA30FA88A0509CC206069F5'
    $build=Get-Content -LiteralPath $buildPath -Raw|ConvertFrom-Json
    if(!$build.passed -or !$build.inputsUnchanged -or $build.ownerAttention -or $build.sourceCount -ne 60){throw 'Frozen suite incomplete'}
    function OriginalHash([string]$path){
        $hashes=@(@($build.inputs)+@($build.files)|Where-Object path -ceq $path|ForEach-Object sha256|Sort-Object -Unique)
        if($hashes.Count -ne 1){throw ('Missing provenance: '+$path)}
        return $hashes[0]
    }
    $relocationHelper=Join-Path $workspace 'Lea.Build/Initialize-NumericScriptSuite.generated.ps1'
    Hold $relocationHelper 'C6848C0BD9344E11F7298749251980E18F4012BB040FEE03F135ABFD15E4061E'
    . $relocationHelper
    $result.relocation=Initialize-NumericScriptSuite $build $base $outRoot $workspace
    $result.runtime=$result.relocation.runtime
    $overlayPath=Join-Path $root 'suite-phase-source-v1/result.json';Hold $overlayPath '22948AC8AD6303C1D1FEAEFFE15CB30FBB03871619445D45F6606E176628C8C3'
    $overlay=Get-Content -LiteralPath $overlayPath -Raw|ConvertFrom-Json
    if(!$overlay.inverseVerified -or $overlay.stages.Count -ne 89 -or $overlay.executed -or $overlay.compiled){throw 'Overlay proof incomplete'}
    $recipe=Join-Path $base 'tests.rsp';Hold $recipe '804234F6C0058ABE81D0914A79630DC9FEC0B942FCE6521B899CB5DE8F527E9F'
    $options=[Collections.Generic.List[string]]::new();$oldSource=Join-Path $base 'source';$outputs=0;$pdbs=0
    foreach($line in [IO.File]::ReadAllLines($recipe)){
        if($line.StartsWith('"') -and $line.EndsWith('.cs"')){
            $from=$line.Trim('"');Hold $from (OriginalHash $from)
            if(!$from.StartsWith($oldSource+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Source path escapes'}
            $relative=$from.Substring($oldSource.Length+1);$selected=$from
            $changed=$relative -ceq 'Lea.Script.Tests\Program.cs'
            if($changed){$selected=Join-Path $root 'suite-phase-source-v1/Program.cs';Hold $selected $overlay.candidateSha256}
            if($relative -ceq 'Lea.Script.Tests\ModernComponents.cs'){
                $original=[IO.File]::ReadAllText($from)
                $bindings=@($result.relocation.componentBindings)+@(@{name='entry';old=$result.relocation.oldManifestSha256;replacement=$result.relocation.newManifestSha256})
                if($bindings.Count -ne 6){throw 'Six dispatcher bindings required'}
                $updated=$original
                foreach($binding in $bindings){if([regex]::Matches($updated,$binding.old).Count -ne 1){throw 'Dispatcher anchor differs'};$updated=$updated.Replace($binding.old,$binding.replacement)}
                $inverse=$updated
                for($i=$bindings.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($bindings[$i].replacement,$bindings[$i].old)}
                if($inverse -cne $original){throw 'Dispatcher source changed beyond digests'}
                $selected=Join-Path $outRoot 'ModernComponents.overlay.cs'
                [IO.File]::WriteAllText($selected,$updated,[Text.UTF8Encoding]::new($false))
                Hold $selected (Get-FileHash -LiteralPath $selected).Hash
                $changed=$true
            }
            $to=Join-Path (Join-Path $outRoot 'source') $relative
            $null=New-Item -ItemType Directory (Split-Path $to) -Force
            Copy-Item -LiteralPath $selected -Destination $to
            $hash=(Get-FileHash -LiteralPath $selected).Hash;Hold $to $hash
            $result.sources.Add(@{path=$relative;sha256=$hash;changed=$changed;originalSha256=(Get-FileHash -LiteralPath $from).Hash})
            $options.Add('"'+$to+'"')
        }elseif($line.StartsWith('/out:')){$outputs++;$options.Add('/out:"'+(Join-Path $result.runtime 'Lea.Script.Tests.exe')+'"')}
        elseif($line.StartsWith('/pdb:')){$pdbs++;$options.Add('/pdb:"'+(Join-Path $result.runtime 'Lea.Script.Tests.pdb')+'"')}
        else{
            if($line.StartsWith('/reference:')){
                $path=$line.Substring(11).Trim('"');$expected=OriginalHash $path
                $newPath=$path.Replace($build.candidate,$result.runtime)
                if($newPath -cne $path){
                    $pins=@($inputs|Where-Object path -ceq $newPath|ForEach-Object sha256|Sort-Object -Unique)
                    if($pins.Count -ne 1){throw 'Relocated reference lacks pinned provenance'}
                    $expected=$pins[0]
                }
                Hold $newPath $expected
                $line='/reference:"'+$newPath+'"'
            }
            $options.Add($line)
        }
    }
    if($result.sources.Count -ne 60 -or @($result.sources|Where-Object changed).Count -ne 2 -or $outputs -ne 1 -or $pdbs -ne 1){throw 'Recipe/source coverage differs'}
    $rsp=Join-Path $outRoot 'tests.rsp';[IO.File]::WriteAllLines($rsp,$options,[Text.UTF8Encoding]::new($false));$rspHash=(Get-FileHash -LiteralPath $rsp).Hash;Hold $rsp $rspHash
    $wrapper=Join-Path $workspace 'Lea.Build/Invoke-PublicCompiler.20261022.ps1';Hold $wrapper '1A04EE9A191D522955C633F97A24C9C0921A5738F76D0DF36DB1C92A574EEA03'
    $compile=Run 'compile-timed-suite' (Get-Process -Id $PID).Path @('-NoProfile','-File',('"'+$wrapper+'"'),'-ResponseFile',('"'+$rsp+'"'),'-ExpectedResponseSha256',$rspHash,'-ExpectedSourceSha256','D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB') 120
    if(!$compile.passed){throw 'Diagnostic compilation failed'}
    $result.binary=Join-Path $result.runtime 'Lea.Script.Tests.exe';$result.sha256=(Get-FileHash -LiteralPath $result.binary).Hash
    $result.files=@(Get-ChildItem -LiteralPath $result.runtime -File -Recurse|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash}})
    if($result.files.Count -ne 65){throw 'Expected65 relocated runtime files'}
    $result.fixtureSource=$build.fixtureSource;$result.fixtureFiles=$build.fixtureFiles
    $result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -gt 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    if(!$result.inputsUnchanged -or $result.ownerAttention){$result.passed=$false}
    if($result.ownerAttention){$global:LeaSuitePhaseInputLeases=$leases}else{foreach($lease in $leases){$lease.Dispose()}}
    $result|ConvertTo-Json -Depth 9|Set-Content -LiteralPath (Join-Path $outRoot 'result.json')
    $global:LeaSuitePhaseBuildResult=$result
    [pscustomobject]$result|Select-Object passed,inputsUnchanged,ownerAttention,error,sha256|ConvertTo-Json -Compress
}
