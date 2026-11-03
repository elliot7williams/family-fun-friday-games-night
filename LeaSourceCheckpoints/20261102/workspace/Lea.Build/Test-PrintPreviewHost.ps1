param([switch]$KeepForegroundHost,[ValidateSet(1,2)][int]$Version=1,[switch]$Command,[ValidateRange(1,9)][int]$CommandVersion=1,[switch]$InitialFontPair,[switch]$RelativeFontPair)
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
if($Command -and $Version -ne 1){throw 'Print command builds a new projected host; do not reuse preview binary'}
$out=Join-Path $workspace $(if($Command){'continuity-evidence/print-command-host-v'+$CommandVersion}else{'continuity-evidence/print-preview-host-v'+$Version})
if($InitialFontPair){if(!$Command -or $Version -ne 1){throw 'Initial font host requires fresh command build'};$out=Join-Path $workspace ('continuity-evidence/initial-font-host-v'+$CommandVersion)}
if($RelativeFontPair){if(!$InitialFontPair){throw 'Relative font host requires complete font qualification mode'};$out=Join-Path $workspace ('continuity-evidence/relative-font-host-v'+$CommandVersion)}
$runtime=Join-Path $out 'runtime'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
$identity=[Security.Principal.WindowsIdentity]::GetCurrent()
if(([Security.Principal.WindowsPrincipal]::new($identity)).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Normal nonadmin owner required'}
if(Test-Path -LiteralPath $out){throw 'Preserve preview host attempt'}
if((Get-PSDrive C).Free -lt 600MB){throw 'Storage reserve required'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Firespider.Boot','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'Do not overlap browser/tests'}
if(Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue){throw 'Fresh owner required'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$Path,[string]$Hash){
    $s=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($s)
    if(!$Hash){$Hash=(Get-FileHash -LiteralPath $Path).Hash}
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $Hash){throw ('Input differs: '+$Path)}
    $inputs.Add(@{path=$Path;sha256=$Hash})
}
function Run([string]$Name,[string]$Application,[string[]]$Arguments,[int]$Seconds){
    Invoke-OwnedTestRun -Name $Name -Application $Application -Arguments $Arguments -WorkingDirectory $workspace -OutputDirectory $out -Seconds $Seconds -Records $runs
}
$result=[ordered]@{passed=$false;candidate=$runtime;inputs=$inputs;runs=$runs;before=@();installedBrowserChanged=$false;physicalPrintChanged=$false;scope='Experimental actual browser print preview routing/modal lifetime with exact tested print library and core/view; not physical printing, full fidelity or release qualification'}
if($Command){$result.physicalPrintChanged=$true;$result.scope='New actual Print click/hover routing, native cancel/changed-tab guards and preview regression on395-check library; valid queue submission/physical output/full host campaign NOT verified'}
$null=New-Item -ItemType Directory -Path $runtime
try{
    Hold $PSCommandPath;Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $out 'runner.ps1')
    Hold (Join-Path $PSScriptRoot 'OwnedTestProcess.cs') 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA'
    Hold (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1') 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00'
    Add-Type -Path (Join-Path $PSScriptRoot 'OwnedTestProcess.cs')
    . (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1')
    $baselinePath=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/host-runtime8033-owned-v1/result.json'
    Hold $baselinePath 'A1B1A96C9358525A7704BE283B434AD946D4906C47025E1E01D2C768BD0A12CA'
    $baseline=Get-Content -LiteralPath $baselinePath -Raw|ConvertFrom-Json
    if(!$baseline.passed -or !$baseline.inputsUnchanged -or $baseline.ownerAttention -or $baseline.before.Count -ne 398){throw 'Matched host baseline incomplete'}
    foreach($file in $baseline.before){
        Hold $file.path $file.sha256
        $relative=[IO.Path]::GetRelativePath($baseline.candidate,$file.path)
        if([IO.Path]::IsPathRooted($relative) -or $relative.Split('\') -contains '..'){throw 'Runtime path escape'}
        $to=Join-Path $runtime $relative;$null=New-Item -ItemType Directory -Path (Split-Path $to) -Force
        Copy-Item -LiteralPath $file.path -Destination $to
        if((Get-FileHash -LiteralPath $to).Hash -cne $file.sha256){throw 'Runtime copy differs'}
    }
    if($Command){
        $printProof=Join-Path $workspace 'continuity-evidence/print-ticket-project-v1/result.json'
        Hold $printProof '39527111558E9FD59C210E8779B6F1B96182ED9E145E44083C750BE70352C6AD'
        $proof=Get-Content $printProof -Raw|ConvertFrom-Json
        if(!$proof.passed -or !$proof.ticketPassed -or !$proof.inputsUnchanged -or $proof.ownerAttention){throw 'Prepared printer library proof incomplete'}
        $print=Join-Path $workspace 'continuity-evidence/print-ticket-project-v1/bin/Lea.Print.dll'
        Hold $print '3C014359204CEEDD11E8164A28467D46CB3BA83E5DF6A81802C0F3A7D3AF1795'
    }else{
        $print=Join-Path $workspace 'continuity-evidence/print-live-capture-v1/Lea.Print.dll'
        Hold $print 'FC867957DF7CD7029BF15455C5163027755A79EA3CAECF5EF55963A6F3FE8A6A'
    }
    if($InitialFontPair){
        $proofSpecs=@(@('initial-font-print-project-v1','20BE9ED2377F96857A062F3616FAD3841BB3ABD62A001335DE44A402E66F8259'),@('initial-font-image-live-v1','B78969A7390CA2F167CBE4CBFEED3D0D3920E282FC22B575CCA5239521149D08'),@('temporal-startup-v1/initial-font-script-owned-run-v1','2990322C7945B11249BFE198F8296ECFF3345CF35BBD6CB48E888BF6AC36F27B'))
        $fontFolder='font-initial-render-v1';$printFolder='initial-font-print-project-v1';$printPin='271BA491D0496E990DC67B8B849310241BCE277F4E0CDF6070C1854305812C1C'
        $pairs=@(@('Lea.Engine.dll','1474123BA6B52A3857B32D29FC85A462AA10AD1ABB933C19E93FD63DCFFCEEBA'),@('Lea.Win.dll','6E342B9B68C54803B91CF796F082C788DC86E29C1A7CEFAA571302E5E4636B4F'))
        if($RelativeFontPair){
            $proofSpecs=@(@('relative-font-print-project-v1','A71A3BC94E9584876DCE5480027EBAD4585A2900E6F01651C65D4869FA2D8002'),@('relative-font-image-live-v1','41EB69CB05327FF8FE6C4226543EBE2ED75B8662E3DBB8B709B528B19D4BF1CA'),@('temporal-startup-v1/relative-font-script-owned-run-v1','7F1448D51CF16421602D7D02DDC58F066BDAF5DB9E22A8E8B3728C525B5BA205'))
            $fontFolder='font-relative-render-v1';$printFolder='relative-font-print-project-v1';$printPin='0C16B3D0113A91F83274F37D48C4D7B5F9BAC8DC1E292FBBBF318BF908711CFB'
            $pairs=@(@('Lea.Engine.dll','4DA8BF1968218A97AF3EC053DDE0E22EEB218669EF1F3B4BCFEA3A39F8ABDFBA'),@('Lea.Win.dll','CA51FC818B119529EA8F7104D0753C5779070A7CCF0AD157675A564C4755C584'))
        }
        foreach($spec in $proofSpecs){
            $proofPath=Join-Path $workspace ('continuity-evidence/'+$spec[0]+'/result.json');Hold $proofPath $spec[1]
            $verified=Get-Content $proofPath -Raw|ConvertFrom-Json
            if(!$verified.passed -or !$verified.inputsUnchanged -or $verified.ownerAttention){throw 'Current font qualification incomplete'}
        }
        foreach($pair in $pairs){
            $from=Join-Path $workspace ('continuity-evidence/'+$fontFolder+'/'+$pair[0]);Hold $from $pair[1]
            $to=Join-Path $runtime $pair[0];Copy-Item -LiteralPath $from -Destination $to
            if((Get-FileHash $to).Hash -cne $pair[1]){throw 'Current font host component copy differs'}
        }
        $print=Join-Path $workspace ('continuity-evidence/'+$printFolder+'/bin/Lea.Print.dll');Hold $print $printPin
        $result.currentFullNativeQualified=$false;$result.legacyUiAUnresolved=$true;$result.fullHostCampaignQualified=$false
    }
    Copy-Item -LiteralPath $print -Destination (Join-Path $runtime 'Lea.Print.dll')
    foreach($name in @('print-styled-document.html','print-preview-image.html')){
        $from=Join-Path $workspace ('LeaForge/test-pages/'+$name);Hold $from
        Copy-Item -LiteralPath $from -Destination (Join-Path $runtime ('test-pages/'+$name))
    }
    if($Version -eq 1){
    $archivePath=Join-Path $workspace 'continuity-evidence/native-selection-host-source-20261027.zip'
    Hold $archivePath 'D4F8FB82153418B2584C27E6D6A50C9D99499119B28DE7A64C755DACD8D0FC80'
    $archive=[IO.Compression.ZipFile]::OpenRead($archivePath)
    try{$reader=[IO.StreamReader]::new($archive.GetEntry('manifest.json').Open());try{$manifest=$reader.ReadToEnd()|ConvertFrom-Json}finally{$reader.Dispose()}}finally{$archive.Dispose()}
    $prefix='continuity-evidence/page-color-host-v5/host-source/'
    $sources=@($manifest.members|Where-Object entry -Like ($prefix+'*'))
    if($sources.Count -ne 116){throw 'Host source inventory differs'}
    $stage=Join-Path $out 'host-source'
    foreach($source in $sources){
        $from=Join-Path $workspace $source.entry;Hold $from $source.sha256
        $relative=$source.entry.Substring($prefix.Length)
        if([IO.Path]::IsPathRooted($relative) -or $relative.Split('/') -contains '..'){throw 'Source path escape'}
        $to=Join-Path $stage $relative;$null=New-Item -ItemType Directory -Path (Split-Path $to) -Force
        Copy-Item -LiteralPath $from -Destination $to
    }
    foreach($name in @('New-PrintPreviewHostSource.ps1','New-PrintPreviewHarness.ps1')){Hold (Join-Path $PSScriptRoot $name);. (Join-Path $PSScriptRoot $name)}
    $main=Join-Path $stage '1.vb';$targets=Join-Path $stage 'Lea/FirespiderLea.targets'
    $oldMain=[IO.File]::ReadAllText($main);$oldTargets=[IO.File]::ReadAllText($targets)
    $edit=New-PrintPreviewHostSource $oldMain $oldTargets
    $result.hostSourceInverseVerified=$edit.inverseVerified
    $negativeControls=0
    foreach($bad in @($oldMain.Replace('Handles PP.Click','Handles OTHER.Click'),($oldMain+$oldMain),$edit.main)){
        $denied=$false;try{$null=New-PrintPreviewHostSource $bad $oldTargets}catch{$denied=$true}
        if(!$denied){throw 'Host source projection negative control failed'};$negativeControls++
    }
    $result.sourceNegativeControls=$negativeControls
    if($Command){
        $projection=Join-Path $PSScriptRoot 'New-PrintCommandHostSource.ps1';Hold $projection;. $projection
        $previewMain=$edit.main;$previewTargets=$edit.targets
        $edit=New-PrintCommandHostSource $previewMain $previewTargets
        $result.commandSourceInverseVerified=$edit.inverseVerified
        $negative=0
        foreach($bad in @($previewMain.Replace('Handles P.Click','Handles OTHER.Click'),($previewMain+$previewMain),$edit.main)){
            $denied=$false;try{$null=New-PrintCommandHostSource $bad $previewTargets}catch{$denied=$true}
            if(!$denied){throw 'Print command projection negative failed'};$negative++
        }
        $result.commandSourceNegativeControls=$negative
        $bridge=Join-Path $workspace 'Lea.Firespider/PrintCommand.vb';Hold $bridge
        Copy-Item -LiteralPath $bridge -Destination (Join-Path $stage 'PrintCommand.vb')
    }
    [IO.File]::WriteAllText($main,$edit.main,[Text.UTF8Encoding]::new($false))
    [IO.File]::WriteAllText($targets,$edit.targets,[Text.UTF8Encoding]::new($false))
    $bridge=Join-Path $workspace 'Lea.Firespider/PrintPreview.vb';Hold $bridge
    Copy-Item -LiteralPath $bridge -Destination (Join-Path $stage 'PrintPreview.vb')
    foreach($name in @('Lea.Engine.dll','Lea.Win.dll','Lea.Print.dll','Lea.Script.dll','Lea.Script.Win.dll','Jint.dll','Acornima.dll','Lea.Image.exe','System.Memory.dll','System.Buffers.dll','System.Numerics.Vectors.dll','System.Runtime.CompilerServices.Unsafe.dll')){
        $from=Join-Path $runtime $name;Hold $from
        Copy-Item -LiteralPath $from -Destination (Join-Path $stage ('Lea/'+$name))
    }
    foreach($file in Get-ChildItem -LiteralPath (Join-Path $runtime 'ScriptWorker') -File){
        $to=Join-Path $stage ('Lea/ScriptWorker/'+$file.Name);$null=New-Item -ItemType Directory -Path (Split-Path $to) -Force
        Copy-Item -LiteralPath $file.FullName -Destination $to
    }
    $buildProof=Join-Path $workspace 'continuity-evidence/firespider-source-rebuild-v1/result.json'
    Hold $buildProof '9DDF92B7273943C6CB5481A8CCC8033F09053BE2C5968DC5E53EDA14E0A5F3DA'
    $compiler=(Get-Content -LiteralPath $buildProof -Raw|ConvertFrom-Json).compiler;Hold $compiler.path $compiler.sha256
    $bin=Join-Path $out 'host-bin'
    $build=Run 'compile-preview-host' $compiler.path @(('"'+(Join-Path $stage 'Firespider Internet.vbproj')+'"'),'/nologo','/m:1','/nodeReuse:false','/t:Rebuild','/v:minimal','/p:Configuration=Release','/p:GenerateManifests=false','/p:AutoGenerateBindingRedirects=true',('/p:OutputPath="'+$bin+'"')) 120
    if(!$build.passed){throw 'Preview host compilation failed'}
    $exe=Join-Path $bin 'Firespider Internet 19.exe';Hold $exe
    Copy-Item -LiteralPath $exe -Destination (Join-Path $runtime 'Firespider Internet 19.exe')
    $result.hostSha256=(Get-FileHash -LiteralPath $exe).Hash
    }else{
        $priorPath=Join-Path $workspace 'continuity-evidence/print-preview-host-v1/result.json'
        Hold $priorPath '67240841A7A85B95CB438E214051BE3B865F4C04D021DBE0157012BFBD8B8449'
        $prior=Get-Content -LiteralPath $priorPath -Raw|ConvertFrom-Json
        if(!$prior.inputsUnchanged -or $prior.ownerAttention -or $prior.passed -or $prior.hostSha256 -cne 'F5EBF8F4593FACD4CA33083DEF1B854951DC21232674EAB6C9F3DC0361DD8FBE' -or @($prior.runs|Where-Object {$_.name -like 'compile-*' -and $_.passed -and !$_.timedOut -and !$_.killRequested -and $_.activeAfterRetirement -eq 0}).Count -ne 2){throw 'Prior unchanged compiled host proof incomplete'}
        foreach($input in $prior.inputs){
            if($input.path -cin @($PSCommandPath,(Join-Path $PSScriptRoot 'PrintPreviewHostProbe.fragment.cs'))){continue}
            Hold $input.path $input.sha256
        }
        $stage=Join-Path $workspace 'continuity-evidence/print-preview-host-v1/host-source'
        $exe=Join-Path $workspace 'continuity-evidence/print-preview-host-v1/host-bin/Firespider Internet 19.exe'
        Hold $exe $prior.hostSha256
        Copy-Item -LiteralPath $exe -Destination (Join-Path $runtime 'Firespider Internet 19.exe')
        Hold (Join-Path $PSScriptRoot 'New-PrintPreviewHarness.ps1');. (Join-Path $PSScriptRoot 'New-PrintPreviewHarness.ps1')
        $result.hostSha256=$prior.hostSha256;$result.reusedCompiledHost=$true
        $result.hostSourceInverseVerified=$prior.hostSourceInverseVerified;$result.sourceNegativeControls=$prior.sourceNegativeControls
        $result.priorBehaviorFailureRetained=$true
    }
    $bootSource=Join-Path $workspace 'continuity-evidence/split-host-diagnostic-v2/Boot.cs';Hold $bootSource 'A23876A4FC73B31A4CB48E784B5BF90F8D6C3F73AEFBDA21CC8B08BB931BDB31'
    $probe=Join-Path $PSScriptRoot 'PrintPreviewHostProbe.fragment.cs';Hold $probe
    $harness=New-PrintPreviewHarness ([IO.File]::ReadAllText($bootSource)) ([IO.File]::ReadAllText($probe))
    if($Command){
        $builder=Join-Path $PSScriptRoot 'New-PrintCommandHarness.ps1';Hold $builder;. $builder
        $commandProbe=Join-Path $PSScriptRoot 'PrintCommandHostProbe.fragment.cs';Hold $commandProbe
        $harness=New-PrintCommandHarness ([IO.File]::ReadAllText($bootSource)) ([IO.File]::ReadAllText($probe)) ([IO.File]::ReadAllText($commandProbe))
    }
    $result.harnessInverseVerified=$harness.inverseVerified
    $harnessPath=Join-Path $out 'Boot.cs';[IO.File]::WriteAllText($harnessPath,$harness.text,[Text.UTF8Encoding]::new($false));Hold $harnessPath
    $recipe=Join-Path $workspace 'continuity-evidence/split-host-diagnostic-v2/compile.rsp';Hold $recipe
    $options=[Collections.Generic.List[string]]::new()
    foreach($line in [IO.File]::ReadAllLines($recipe)){
        if($line.StartsWith('/out:')){$line='/out:"'+(Join-Path $runtime 'Lea.Firespider.Boot.exe')+'"'}
        elseif($line -match '^"(.+\.cs)"$'){
            $path=$Matches[1];if([IO.Path]::GetFileName($path) -ceq 'Boot.cs'){$path=$harnessPath}else{Hold $path}
            $line='"'+$path+'"'
        }elseif($line -match '^/reference:"([^"]+)"$'){
            $path=$Matches[1];$local=Join-Path $runtime ([IO.Path]::GetFileName($path));if(Test-Path -LiteralPath $local){$path=$local}
            Hold $path;$line='/reference:"'+$path+'"'
        }
        $options.Add($line)
    }
    $frameworks=@('PresentationCore','PresentationFramework','WindowsBase','System.Xaml','WindowsFormsIntegration')
    if($Command){$frameworks+=@('System.Printing','ReachFramework')}
    foreach($name in $frameworks){
        $path=Join-Path $env:WINDIR ('Microsoft.NET/Framework64/v4.0.30319/WPF/'+$name+'.dll')
        if(!(Test-Path -LiteralPath $path)){$path=Join-Path $env:WINDIR ('Microsoft.NET/Framework64/v4.0.30319/'+$name+'.dll')}
        Hold $path;$options.Add('/reference:"'+$path+'"')
    }
    $options.Add('/reference:"'+(Join-Path $runtime 'Lea.Print.dll')+'"')
    $rsp=Join-Path $out 'harness.rsp';[IO.File]::WriteAllLines($rsp,$options,[Text.UTF8Encoding]::new($false));Hold $rsp
    $wrapper=Join-Path $PSScriptRoot 'Invoke-PublicCompiler.20261026.ps1';Hold $wrapper 'C3EBB3801FAFB80DA031AAB69FE4837EEB4E4EECD3907F9704F9D6186FA42F0A'
    $compile=Run 'compile-preview-probe' (Get-Process -Id $PID).Path @('-NoProfile','-File',('"'+$wrapper+'"'),'-ResponseFile',('"'+$rsp+'"'),'-ExpectedResponseSha256',(Get-FileHash -LiteralPath $rsp).Hash,'-ExpectedSourceSha256','D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB') 120
    if(!$compile.passed){throw 'Preview probe compilation failed'}
    foreach($file in Get-ChildItem -LiteralPath $stage -File -Recurse){Hold $file.FullName}
    $policy=Join-Path $PSScriptRoot 'Test-DecoderAclDelta.ps1';Hold $policy '15F46DBC7F4D4AD5B816DDFA1FE7213D02CAE1EDE79844AFBD904F281299E71E';. $policy
    $result.before=@(Get-ChildItem -LiteralPath $runtime -File -Recurse|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash;sddl=(Get-Acl -LiteralPath $_.FullName).Sddl}})
    $mode=if($Command){'print-command'}else{'print-preview'}
    $run=Run 'browser-preview' (Join-Path $runtime 'Lea.Firespider.Boot.exe') @(('"'+$runtime+'"'),$mode) 90
    $footer=if($Command){'PASS Lea browser print command:'}else{'PASS Lea browser print preview:'}
    $result.behaviorPassed=$run.passed -and @((Get-Content -LiteralPath $run.stdout)|Where-Object { $_.StartsWith($footer) }).Count -eq 1
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.byteChanges=@($result.before|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256})
    $result.aclChanges=@($result.before|Where-Object {(Get-Acl -LiteralPath $_.path).Sddl -cne $_.sddl})
    $result.decoderPolicyAccepted=$result.before.Count -gt 0 -and !$result.aclChanges.Count
    if($result.aclChanges.Count -eq 1 -and (Get-Command Test-DecoderAclDelta -ErrorAction SilentlyContinue)){
        $file=$result.aclChanges[0]
        $result.decoderPolicyAccepted=Test-DecoderAclDelta -Path $file.path -ExpectedDecoderPath (Join-Path $runtime 'Lea.Image.exe') -Before $file.sddl -After (Get-Acl -LiteralPath $file.path).Sddl -ProfileName 'Lea.Image.Decoder'
    }
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.behaviorPassed -and !$result.error -and $result.inputsUnchanged -and !$result.byteChanges.Count -and $result.decoderPolicyAccepted -and !$result.ownerAttention
    if($result.ownerAttention){$global:LeaPreviewHostInputLeases=$leases}else{foreach($s in $leases){$s.Dispose()}}
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 8))
    [pscustomobject]$result|Select-Object passed,behaviorPassed,inputsUnchanged,decoderPolicyAccepted,ownerAttention,error|ConvertTo-Json -Compress
}
