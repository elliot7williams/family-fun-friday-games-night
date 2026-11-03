param([switch]$KeepForegroundHost,[switch]$LibraryNavigation)
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$outRoot=Join-Path $workspace $(if($LibraryNavigation){'continuity-evidence/library-navigation-v1'}else{'continuity-evidence/about-navigation-v2'})
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained owner required'}
if(Test-Path -LiteralPath $outRoot){throw 'Preserve About regression attempt'}
$identity=[Security.Principal.WindowsIdentity]::GetCurrent()
if(!$identity.User.Value.EndsWith('-1001') -or ([Security.Principal.WindowsPrincipal]::new($identity)).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Normal nonadmin required'}
if(Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue){throw 'Fresh owner required'}
if(@(Get-Process 'Lea.Script.Worker','Lea.Script.Tests','Lea.Firespider.Boot','Firespider*' -ErrorAction SilentlyContinue).Count){throw 'Do not overlap existing tests/browser'}
$utility=Join-Path $workspace 'continuity-evidence/native-launch-timing-v1/Test-CompositeCandidate.v2.ps1'
if((Get-FileHash -LiteralPath $utility).Hash -cne 'EB00AD17B8078A6CC2F418733BA5D675FC84EF49EED433A527FAC5498DD5ECEF'){throw 'Runner differs'}
$t=$null;$e=$null;$ast=[Management.Automation.Language.Parser]::ParseFile($utility,[ref]$t,[ref]$e)
foreach($name in @('Space','NewOutputReader','PumpOutput','Run')){$defs=@($ast.FindAll({param($n)$n -is [Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -ceq $name},$false));if($e.Count -or $defs.Count -ne 1){throw 'Runner shape differs'};. ([scriptblock]::Create($defs[0].Extent.Text))}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new()
function Hold([string]$Path,[string]$Expected){$stream=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($stream);if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) -cne $Expected){throw ('Input differs: '+$Path)};$inputs.Add(@{path=$Path;sha256=$Expected})}
function CopyPinned([string]$From,[string]$To,[string]$Hash){Hold $From $Hash;$null=New-Item -ItemType Directory -Path (Split-Path $To) -Force;Copy-Item -LiteralPath $From -Destination $To}
$result=[ordered]@{passed=$false;candidate=(Join-Path $outRoot 'runtime');inputs=$inputs;runs=$runs;scope='Scoped About link/hover cast regression with real host and blocked external navigation; full changed-EXE campaign still required';installedBrowserChanged=$false;limitsChanged=$false}
Space;$null=New-Item -ItemType Directory -Path $result.candidate
try{
    CopyPinned $PSCommandPath (Join-Path $outRoot 'runner.ps1') (Get-FileHash -LiteralPath $PSCommandPath).Hash
    $proof=Join-Path $workspace $(if($LibraryNavigation){'continuity-evidence/temporal-startup-v1/host-about-current-native-v1/result.json'}else{'continuity-evidence/temporal-startup-v1/host-current-native-v1/result.json'})
    Hold $proof $(if($LibraryNavigation){'9D82D7C17CA855462830B57F0723BF6AE8FC9E8312FDE4130C783E505B88C5EA'}else{'74BAD2AFA9ADE431B9B8630AEB222DFBE238818ADC616F27FF3768A165448DE7'})
    $hostRun=Get-Content -LiteralPath $proof -Raw|ConvertFrom-Json
    if(!$hostRun.passed -or !$hostRun.originalInputsUnchanged -or !$hostRun.candidateBytesUnchanged -or $hostRun.ownerAttention){throw 'Current host proof incomplete'}
    $known=@{}
    foreach($file in $hostRun.before){$relative=[IO.Path]::GetRelativePath($hostRun.candidate,$file.path);$to=Join-Path $result.candidate $relative;CopyPinned $file.path $to $file.sha256;$known[$to]=$file.sha256}
    $diagnosticRoot=Join-Path $workspace 'continuity-evidence/split-host-diagnostic-v2'
    $diagnosticProof=Join-Path $diagnosticRoot 'result.json';Hold $diagnosticProof '7999E9A2CE4AA3B834FB1E731CDBC8C091F00F9E79F82CCDC0202D1745A1FFBC'
    $diagnostic=Get-Content -LiteralPath $diagnosticProof -Raw|ConvertFrom-Json
    foreach($file in $diagnostic.inputs){$known[$file.path]=$file.sha256}
    $harnessHelper=Join-Path $PSScriptRoot 'New-AboutHarnessSource.ps1';Hold $harnessHelper (Get-FileHash -LiteralPath $harnessHelper).Hash;. $harnessHelper
    $source=Join-Path $diagnosticRoot 'Boot.cs';Hold $source $known[$source]
    $harnessText=New-AboutHarnessSource ([IO.File]::ReadAllText($source))
    if($LibraryNavigation){
        $libraryHarnessHelper=Join-Path $PSScriptRoot 'New-LibraryHarnessSource.ps1';Hold $libraryHarnessHelper (Get-FileHash $libraryHarnessHelper).Hash;. $libraryHarnessHelper
        $libraryHarness=New-LibraryHarnessSource ([IO.File]::ReadAllText($source))
        if(!$libraryHarness.inverseVerified){throw 'Library harness inverse missing'}
        $harnessText=$libraryHarness.text
        $result.scope='Real history/bookmark list handler regression: baseline silent navigation failure, current-tab dispatch, original policy, local loads and no external I/O. Full changed-EXE campaign separate.'
    }
    $harness=Join-Path $outRoot 'Boot.cs';[IO.File]::WriteAllText($harness,$harnessText,[Text.UTF8Encoding]::new($false));Hold $harness (Get-FileHash -LiteralPath $harness).Hash
    $options=[Collections.Generic.List[string]]::new()
    $recipe=Join-Path $diagnosticRoot 'compile.rsp';Hold $recipe $known[$recipe]
    foreach($line in Get-Content -LiteralPath $recipe){
        if($line.StartsWith('/out:')){$line='/out:"'+(Join-Path $result.candidate 'Lea.Firespider.Boot.exe')+'"'}
        elseif($line -match '^"(.+\.cs)"$'){$from=$Matches[1];if([IO.Path]::GetFileName($from) -ceq 'Boot.cs'){$line='"'+$harness+'"'}else{Hold $from $known[$from]}}
        elseif($line -match '^/reference:"([^"]+)"$'){
            $from=$Matches[1];$local=Join-Path $result.candidate ([IO.Path]::GetFileName($from))
            if(Test-Path -LiteralPath $local){$from=$local}
            Hold $from $known[$from];$line='/reference:"'+$from+'"'
        }
        $options.Add($line)
    }
    $rsp=Join-Path $outRoot 'harness.rsp';[IO.File]::WriteAllLines($rsp,$options);$hash=(Get-FileHash -LiteralPath $rsp).Hash;Hold $rsp $hash
    $wrapper=Join-Path $PSScriptRoot 'Invoke-PublicCompiler.20261026.ps1';Hold $wrapper 'C3EBB3801FAFB80DA031AAB69FE4837EEB4E4EECD3907F9704F9D6186FA42F0A'
    $compile=Run 'compile-about-harness' (Get-Process -Id $PID).Path @('-NoProfile','-File',('"'+$wrapper+'"'),'-ResponseFile',('"'+$rsp+'"'),'-ExpectedResponseSha256',$hash,'-ExpectedSourceSha256','D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB') 120
    if(!$compile.passed){throw 'About harness compilation failed'}
    $boot=Join-Path $result.candidate 'Lea.Firespider.Boot.exe'
    $mode=if($LibraryNavigation){'library'}else{'about'}
    $baseline=Run ('old-'+$mode+'-regression') $boot @(('"'+$result.candidate+'"'),$mode) 90
    $expectedFailure=if($LibraryNavigation){'Library routes through blocked-site policy: history'}else{'InvalidCastException'}
    $result.baselineRegressionReproduced=$baseline.exitCode -eq 1 -and !$baseline.timedOut -and !$baseline.killRequested -and $baseline.stdoutComplete -and $baseline.stderrComplete -and ([IO.File]::ReadAllText($baseline.stderr).Contains($expectedFailure))
    if(!$result.baselineRegressionReproduced){throw 'Original About cast failure not reproduced'}
    $hostBuildRoot=Join-Path $workspace 'continuity-evidence/firespider-source-rebuild-v1'
    $hostBuildProof=Join-Path $hostBuildRoot 'result.json';Hold $hostBuildProof '9DDF92B7273943C6CB5481A8CCC8033F09053BE2C5968DC5E53EDA14E0A5F3DA'
    $build=Get-Content -LiteralPath $hostBuildProof -Raw|ConvertFrom-Json
    $sourcePins=@{};foreach($file in $build.inputs){$sourcePins[$file.path]=$file.sha256}
    $hostSource=Join-Path $hostBuildRoot 'source';$stage=Join-Path $outRoot 'host-source'
    foreach($file in Get-ChildItem -LiteralPath $hostSource -Recurse -File){
        $relative=[IO.Path]::GetRelativePath($hostSource,$file.FullName)
        if($relative.StartsWith('obj\',[StringComparison]::OrdinalIgnoreCase)){continue}
        CopyPinned $file.FullName (Join-Path $stage $relative) $sourcePins[$file.FullName]
    }
    $helper=Join-Path $PSScriptRoot 'New-AboutNavigationSource.ps1';Hold $helper (Get-FileHash -LiteralPath $helper).Hash;. $helper
    $about=Join-Path $stage '2.vb';[IO.File]::WriteAllText($about,(New-AboutNavigationSource ([IO.File]::ReadAllText($about))),[Text.UTF8Encoding]::new($false));Hold $about (Get-FileHash -LiteralPath $about).Hash
    $route=Join-Path $stage 'FirespiderLea.vb';[IO.File]::WriteAllText($route,(Add-AboutNewTabRoute ([IO.File]::ReadAllText($route))),[Text.UTF8Encoding]::new($false));Hold $route (Get-FileHash -LiteralPath $route).Hash
    $result.aboutOverlayInverseVerified=$true
    if($LibraryNavigation){
        $libraryHelper=Join-Path $PSScriptRoot 'New-LibraryNavigationSource.ps1';Hold $libraryHelper (Get-FileHash $libraryHelper).Hash;. $libraryHelper
        foreach($kind in @('History','Bookmarks')){
            $name=if($kind -eq 'History'){'6.vb'}else{'7.vb'}
            $path=Join-Path $stage $name;$edit=New-LibraryNavigationSource ([IO.File]::ReadAllText($path)) $kind
            if(!$edit.inverseVerified){throw 'Library source inverse missing'}
            [IO.File]::WriteAllText($path,$edit.text,[Text.UTF8Encoding]::new($false));Hold $path (Get-FileHash $path).Hash
        }
        $result.libraryOverlayInverseVerified=$true
    }
    $compiler=$build.compiler.path
    if((Get-FileHash -LiteralPath $compiler).Hash -cne $build.compiler.sha256){throw 'Host compiler identity differs'}
    $bin=Join-Path $outRoot 'host-bin'
    $rebuild=Run 'build-about-host' $compiler @(('"'+(Join-Path $stage 'Firespider Internet.vbproj')+'"'),'/nologo','/m:1','/nodeReuse:false','/t:Rebuild','/v:minimal','/p:Configuration=Release','/p:GenerateManifests=false','/p:AutoGenerateBindingRedirects=true',('/p:OutputPath="'+$bin+'"')) 120
    if(!$rebuild.passed){throw 'Changed host build failed'}
    $newExe=Join-Path $bin 'Firespider Internet 19.exe';$result.executableSha256=(Get-FileHash -LiteralPath $newExe).Hash
    CopyPinned $newExe (Join-Path $result.candidate 'Firespider Internet 19.exe') $result.executableSha256
    $result.before=@(Get-ChildItem -LiteralPath $result.candidate -Recurse -File|ForEach-Object{@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash;sddl=(Get-Acl -LiteralPath $_.FullName).Sddl}})
    $positive=Run ('fixed-'+$mode+'-regression') $boot @(('"'+$result.candidate+'"'),$mode) 90
    $expectedPass=if($LibraryNavigation){'PASS Library navigation: history and bookmarks'}else{'PASS About navigation: four click/hover paths'}
    $result.passed=$positive.passed -and ([IO.File]::ReadAllText($positive.stdout).Contains($expectedPass))
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object{(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -gt 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.runtimeBytesAndAclsHeld=if($result.before){@($result.before|Where-Object{(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256 -or (Get-Acl -LiteralPath $_.path).Sddl -cne $_.sddl}).Count -eq 0}else{$false}
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention -and $result.runtimeBytesAndAclsHeld
    if($result.ownerAttention){$global:LeaLibraryInputLeases=$leases}else{foreach($lease in $leases){$lease.Dispose()}}
    $result|ConvertTo-Json -Depth 7|Set-Content -LiteralPath (Join-Path $outRoot 'result.json')
}
[pscustomobject]$result|Select-Object passed,baselineRegressionReproduced,ownerAttention,error|ConvertTo-Json
