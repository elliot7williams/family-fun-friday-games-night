param([switch]$KeepForegroundHost,[ValidateRange(1,9)][int]$Version=1,[switch]$Range,[switch]$Device,[switch]$Dialog,[switch]$Ticket,[switch]$Friendly,[switch]$Html,[switch]$CacheImages,[switch]$DataImages,[switch]$LayoutImages,[switch]$SelectionView,[switch]$DecoderView,[switch]$ShutdownView,[switch]$InitialFontPair,[switch]$RelativeFontPair)
$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
if($Device -and !$Range){throw 'Device verification includes range verification'}
if($Dialog -and !$Device){throw 'Dialog verification includes device verification'}
if($Ticket -and !$Dialog){throw 'Ticket verification includes accepted native selection'}
if($Friendly -and !$Ticket){throw 'Friendly project verification includes complete prior ticket/range/dialog coverage'}
if($Html -and !$Friendly){throw 'HTML project verification includes friendly and prior coverage'}
if($CacheImages -and !$Html){throw 'Cache image project includes HTML/friendly/prior verification'}
if($DataImages -and !$CacheImages){throw 'Data image project includes cache/HTML/friendly/prior verification'}
if($LayoutImages -and !$DataImages){throw 'Image layout includes all prior data/cache/HTML verification'}
$prefix=if($LayoutImages){'print-image-layout-project-v'}elseif($DataImages){'print-data-image-project-v'}elseif($CacheImages){'print-cache-project-v'}elseif($Html){'print-html-project-v'}elseif($Friendly){'print-friendly-project-v'}elseif($Ticket){'print-ticket-project-v'}elseif($Dialog){'print-dialog-project-v'}elseif($Device){'print-device-project-v'}elseif($Range){'print-range-project-v'}else{'print-project-v'}
$out=Join-Path $workspace ('continuity-evidence/'+$prefix+$Version)
if($DecoderView -and (!$SelectionView -or !$LayoutImages)){throw 'Decoder candidate requires full selection/image-layout project mode'}
if($SelectionView){
    if(!$LayoutImages){throw 'Selection view regression includes full image-layout print project coverage'}
    $out=Join-Path $workspace ('continuity-evidence/selection-print-project-v'+$Version)
}
if($DecoderView){$out=Join-Path $workspace ('continuity-evidence/decoder-print-project-v'+$Version)}
if($ShutdownView){if(!$DecoderView){throw 'Shutdown candidate requires decoder mode'};$out=Join-Path $workspace ('continuity-evidence/shutdown-print-project-v'+$Version)}
if($InitialFontPair){if(!$ShutdownView){throw 'Initial font pair requires full shutdown/image/selection regression'};$out=Join-Path $workspace ('continuity-evidence/initial-font-print-project-v'+$Version)}
if($RelativeFontPair){if(!$InitialFontPair){throw 'Relative font pair requires complete font regression mode'};$out=Join-Path $workspace ('continuity-evidence/relative-font-print-project-v'+$Version)}
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Normal nonadmin owner required'}
if(Test-Path $out){throw 'Preserve existing attempt'}
if((Get-PSDrive C).Free -lt 600MB){throw 'Storage reserve required'}
if(Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue){throw 'Fresh owner required'}
if(@(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Firespider.Boot','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'Do not overlap browser/tests'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$Path,[string]$Hash){
    $s=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($s)
    if(!$Hash){$Hash=(Get-FileHash -LiteralPath $Path).Hash}
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $Hash){throw ('Input differs: '+$Path)}
    $inputs.Add(@{path=$Path;sha256=$Hash})
}
$result=[ordered]@{passed=$false;inputs=$inputs;runs=$runs;scope='Direct net48 MSBuild print project with mature pinned core/view and matching prior293 functional checks; not full VS SDK or browser qualification'}
if($Range){$result.scope='New range adapter/XPS export, direct project build, prior293 matching print checks plus own range checks; no printer dispatch/browser adoption'}
if($Device){$result.scope='Printer capabilities to oriented geometry, prior293 print checks and37 range checks with new library; synthetic capabilities only, no queue/job/physical printer or browser adoption'}
if($Dialog){$result.scope='Native owned printer selection cancellation, device/range/prior293 checks on new library; no accepted selection, queue dispatch or physical output/browser adoption'}
if($Ticket){$result.scope='Accepted selection to validated driver ticket/capabilities/range and rejected dispatch guards, plus prior native dialog/range/geometry/293 checks; valid submission/physical output/browser adoption NOT tested'}
$null=New-Item -ItemType Directory -Path $out
try{
    Hold $PSCommandPath;Copy-Item $PSCommandPath (Join-Path $out 'runner.ps1')
    Hold (Join-Path $PSScriptRoot 'OwnedTestProcess.cs') 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA'
    Hold (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1') 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00'
    Add-Type -Path (Join-Path $PSScriptRoot 'OwnedTestProcess.cs')
    . (Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1')
    if($InitialFontPair){
        $fontFolder='font-initial-render-v1';$fontProofPin='9774F4B14D7F0E4D783961529A9A6F5A1969A81A6DFEC060F0A75696AC867649';$scriptFolder='initial-font-script-owned-run-v1';$scriptProofPin='2990322C7945B11249BFE198F8296ECFF3345CF35BBD6CB48E888BF6AC36F27B'
        $fontCorePin='1474123BA6B52A3857B32D29FC85A462AA10AD1ABB933C19E93FD63DCFFCEEBA';$fontViewPin='6E342B9B68C54803B91CF796F082C788DC86E29C1A7CEFAA571302E5E4636B4F'
        if($RelativeFontPair){$fontFolder='font-relative-render-v1';$fontProofPin='5FCA5A4730ECA3F2894533665FC22CAA9BA520FA49A2B71622B8B45D295E03A0';$scriptFolder='relative-font-script-owned-run-v1';$scriptProofPin='7F1448D51CF16421602D7D02DDC58F066BDAF5DB9E22A8E8B3728C525B5BA205';$fontCorePin='4DA8BF1968218A97AF3EC053DDE0E22EEB218669EF1F3B4BCFEA3A39F8ABDFBA';$fontViewPin='CA51FC818B119529EA8F7104D0753C5779070A7CCF0AD157675A564C4755C584'}
        $fontProofPath=Join-Path $workspace ('continuity-evidence/'+$fontFolder+'/result.json');Hold $fontProofPath $fontProofPin
        $fontProof=Get-Content $fontProofPath -Raw|ConvertFrom-Json
        $scriptProofPath=Join-Path $workspace ('continuity-evidence/temporal-startup-v1/'+$scriptFolder+'/result.json');Hold $scriptProofPath $scriptProofPin
        $scriptProof=Get-Content $scriptProofPath -Raw|ConvertFrom-Json
        if(!$fontProof.passed -or !$fontProof.inputsUnchanged -or $fontProof.ownerAttention -or !$scriptProof.passed -or !$scriptProof.inputsUnchanged -or $scriptProof.ownerAttention){throw 'Current pair prior verification incomplete'}
        $result.currentFullNativeQualified=$false;$result.legacyUiAUnresolved=$true;$result.hostAdopted=$false
    }
    $proofPath=Join-Path $workspace 'continuity-evidence/print-live-capture-v1/result.json'
    Hold $proofPath '751F1EF75A7E184DFE245D1DEFE7732EBBB4A58C5E34AD284DFB716AAE57F123'
    $proof=Get-Content $proofPath -Raw|ConvertFrom-Json
    if(!$proof.passed -or !$proof.inputsUnchanged -or $proof.ownerAttention){throw 'Live capture baseline incomplete'}
    $stage=Join-Path $out 'source';$bin=Join-Path $out 'bin'
    $null=New-Item -ItemType Directory -Path $stage,$bin
    foreach($source in Get-ChildItem (Join-Path $workspace 'Lea.Print') -File|Where-Object Extension -in @('.cs','.csproj')){
        $pin=@($proof.inputs|Where-Object path -ceq $source.FullName)
        if($source.Name -eq 'Ds.cs'){
            Hold $source.FullName
        }elseif($LayoutImages -and $source.Name -in @('Pm.cs','Pw.cs')){
            Hold $source.FullName
            $layoutHelper=Join-Path $PSScriptRoot 'Get-PrintImageLayoutBaseline.ps1';Hold $layoutHelper;. $layoutHelper
            $layoutProof=Join-Path $workspace 'continuity-evidence/print-data-image-project-v1/result.json';Hold $layoutProof '49ECE0540A0D9544B051728E250BA61C3910392499E3FE226B6D307B21BE58A5'
            $prior=Join-Path $workspace ('continuity-evidence/print-data-image-project-v1/source/'+$source.Name)
            $priorPins=@((Get-Content $layoutProof -Raw|ConvertFrom-Json).inputs|Where-Object path -ceq $prior|Select-Object -ExpandProperty sha256 -Unique)
            if($priorPins.Count -ne 1){throw 'Prior layout source pin missing'};Hold $prior $priorPins[0]
            $text=[IO.File]::ReadAllText($source.FullName);$expected=[IO.File]::ReadAllText($prior).Replace("`r",'')
            if((Get-PrintImageLayoutBaseline $text $source.Name) -cne $expected){throw 'Image layout inverse differs'}
            $unrelated=if($source.Name -eq 'Pm.cs'){$text.Replace('return new Builder(snapshot, grayscale).Build();','throw new Exception();')}else{$text.Replace('this.geometry = geometry;','this.geometry = null;')}
            $missing=$text.Replace('public static FlowDocument Ci','public static FlowDocument Renamed').Replace('public static Pw Di','public static Pw Renamed')
            foreach($bad in @($unrelated,$missing,($text+"`n"+$text))){
                $denied=$false;try{$denied=(Get-PrintImageLayoutBaseline $bad $source.Name) -cne $expected}catch{$denied=$true}
                if(!$denied){throw 'Image layout negative failed'}
            }
            if(!$result.Contains('layoutProjections')){$result.layoutProjections=[Collections.Generic.List[object]]::new()}
            $result.layoutProjections.Add(@{source=$source.Name;inverseVerified=$true;negativeControls=3})
        }elseif(($Html -and $source.Name -in @('Ht.cs','Pi.cs')) -or ($CacheImages -and $source.Name -eq 'Ci.cs') -or ($LayoutImages -and $source.Name -eq 'Mi.cs')){
            Hold $source.FullName
            $result.htmlSourceAdded=$true
        }elseif($Friendly -and $source.Name -in @('Ps.cs','Pc.cs')){
            Hold $source.FullName
            $viewSource=$source.Name -eq 'Pc.cs'
            $inverseFunction=if($viewSource){'Get-PrintFriendlyViewBaseline'}else{'Get-PrintFriendlyBaseline'}
            $projection=Join-Path $PSScriptRoot ($inverseFunction+'.ps1');Hold $projection;. $projection
            $baseline=Join-Path $workspace ('continuity-evidence/print-ticket-project-v1/source/'+$source.Name)
            if($pin.Count -ne 1){throw 'Snapshot baseline pin missing'}
            Hold $baseline $pin[0].sha256
            $text=[IO.File]::ReadAllText($source.FullName)
            if($DataImages -and !$viewSource){
                $keyProjection=Join-Path $PSScriptRoot 'Get-PrintImageKeyBaseline.ps1';Hold $keyProjection;. $keyProjection
                $prior=Join-Path $workspace 'continuity-evidence/print-cache-project-v2/source/Ps.cs'
                Hold $prior
                $expectedPrior=[IO.File]::ReadAllText($prior).Replace("`r",'')
                if((Get-PrintImageKeyBaseline $text) -cne $expectedPrior){throw 'Data image source inverse differs'}
                $negative=0
                foreach($bad in @($text.Replace('internal string CacheKey','internal string ChangedKey'),$text.Replace('Title = title, Source = source','Title = title, Source = "changed"'),($text+"`n"+$text))){
                    $denied=$false;try{$denied=(Get-PrintImageKeyBaseline $bad) -cne $expectedPrior}catch{$denied=$true}
                    if(!$denied){throw 'Image key inverse negative failed'};$negative++
                }
                $result.imageKeyInverseVerified=$true;$result.imageKeyInverseNegatives=$negative
                $text=Get-PrintImageKeyBaseline $text
            }
            if($CacheImages -and $viewSource){
                $pageLine="        internal Lea.Engine.P Page { get; private set; }`n"
                $text=$text.Replace("`r",'')
                if([regex]::Matches($text,[regex]::Escape($pageLine)).Count -ne 1 -or [regex]::Matches($text,[regex]::Escape(', Page = page')).Count -ne 1){throw 'Unique page identity additions required'}
                $text=$text.Replace($pageLine,'').Replace(', Page = page','')
                $result.pageIdentityInverseVerified=$true
            }
            $expected=[IO.File]::ReadAllText($baseline).Replace("`r",'')
            if((& $inverseFunction $text) -cne $expected){throw 'Friendly capture source inverse differs'}
            $negative=0
            $unrelated=if($viewSource){$text.Replace('string selection = view.Sx();','string selection = "changed";')}else{$text.Replace('Title = title, Source = source','Title = title, Source = "changed"')}
            $missing=if($viewSource){$text.Replace('public static Pc Cf','public static Pc Renamed')}else{$text.Replace('private static bool FriendlyExcluded','private static bool RenamedExcluded')}
            foreach($bad in @($unrelated,$missing,($text+"`n"+$text))){
                $denied=$false
                try{$denied=(& $inverseFunction $bad) -cne $expected}catch{$denied=$true}
                if(!$denied){throw 'Friendly inverse negative control failed'};$negative++
            }
            $result.friendlySourceInverseVerified=$true;$result.friendlySourceNegativeControls=$negative
            if(!$result.Contains('friendlySourceProjections')){$result.friendlySourceProjections=[Collections.Generic.List[object]]::new()}
            $result.friendlySourceProjections.Add(@{source=$source.Name;inverseVerified=$true;negativeControls=$negative})
        }elseif($Range -and $source.Name -in @('Pr.cs','Pd.cs','Pj.cs','Pk.cs','Pp.cs','Px.cs','Lea.Print.csproj')){
            Hold $source.FullName
            if($source.Name -notin @('Pr.cs','Pd.cs','Pj.cs','Pk.cs')){
                $original=Join-Path $workspace ('continuity-evidence/print-live-capture-v1/'+$source.Name)
                $oldPin=@($proof.inputs|Where-Object path -ceq $original|Select-Object -ExpandProperty sha256 -Unique)
                if($source.Name -eq 'Lea.Print.csproj'){$oldPin=@($pin|Select-Object -ExpandProperty sha256 -Unique)}
                if($oldPin.Count -ne 1){throw 'Original source pin missing'}
                Hold $original $oldPin[0]
                $text=[IO.File]::ReadAllText($source.FullName).Replace("`r`n","`n")
                if($source.Name -eq 'Lea.Print.csproj'){
                    $inverse=$text.Replace("    <Compile Include=`"Pr.cs`" />`n",'').Replace("    <Compile Include=`"Pd.cs`" />`n",'').Replace("    <Compile Include=`"Pj.cs`" />`n",'').Replace("    <Compile Include=`"Pk.cs`" />`n",'')
                    if($Html){$inverse=$inverse.Replace("    <Compile Include=`"Ht.cs`" />`n",'').Replace("    <Compile Include=`"Pi.cs`" />`n",'').Replace("    <Reference Include=`"System.Xml`" />`n",'')}
                    if($CacheImages){$inverse=$inverse.Replace("    <Compile Include=`"Ci.cs`" />`n",'')}
                    $inverse=$inverse.Replace("    <Compile Include=`"Ds.cs`" Condition=`"'`$(EnableDocumentSelection)' == 'true'`" />`n",'')
                    if($LayoutImages){$inverse=$inverse.Replace("    <Compile Include=`"Mi.cs`" />`n",'')}
                }
                elseif($source.Name -eq 'Pp.cs'){
                    $inverse=$text.Replace("        internal Pg Geometry { get { plan.Verify(); return plan.Geometry; } }`n",'').Replace("        internal bool HasImages { get { plan.Verify(); return plan.UnresolvedImages != 0; } }`n",'')
                }
                else{
                    $insertion=@'
        { return Write(pages, destination); }

        public static int Rg(Pr pages, Stream destination)
        { return Write(pages, destination); }

        private static int Write(DocumentPaginator pages, Stream destination)
'@
                    $inverse=$text.Replace("using System.Windows.Documents;`n",'').Replace($insertion+"`n",'')
                }
                if($inverse -cne [IO.File]::ReadAllText($original).Replace("`r`n","`n")){throw ('Range source inverse differs: '+$source.Name)}
            }
        }else{
            if($pin.Count -ne 1){throw ('Source not in baseline: '+$source.Name)}
            if($source.Extension -eq '.cs'){Hold $source.FullName $pin[0].sha256}else{Hold $source.FullName}
        }
        $destination=Join-Path $stage $source.Name;Copy-Item $source.FullName $destination;Hold $destination
    }
    foreach($name in @('Lea.Engine.dll','Lea.Win.dll','PrintTextChecks.exe','native-selection.html','print-document.html','print-styles.html','print-styled-document.html')){
        $from=Join-Path $workspace ('continuity-evidence/print-live-capture-v1/'+$name)
        $pin=@($proof.inputs|Where-Object path -ceq $from|Select-Object -ExpandProperty sha256 -Unique)
        if($CacheImages -and $name -eq 'Lea.Win.dll'){
            $cacheProof=Join-Path $workspace 'continuity-evidence/print-cache-images-v1/result.json';Hold $cacheProof '8FEAE4FB93C89AF62D774FDC59D247B1647A952F6033C3F094704F194AE7BC69'
            $cache=Get-Content $cacheProof -Raw|ConvertFrom-Json
            if(!$cache.passed -or !$cache.inputsUnchanged -or $cache.ownerAttention){throw 'Cache view focused proof incomplete'}
            $from=Join-Path $workspace 'continuity-evidence/print-cache-images-v1/Lea.Win.dll';$pin=@('A921DDF96C9648014305DE36D40A165E55C387125B6B73C41AB5CF4A3D0B0AEF')
        }
        if($SelectionView -and $name -eq 'Lea.Win.dll'){
            $selectionProof=Join-Path $workspace 'continuity-evidence/document-native-broker-v2/result.json'
            Hold $selectionProof '244799D50C787C6AC7D568EAF37BFAE300EE87226C37573E076730BC94E1D3B1'
            $selection=Get-Content $selectionProof -Raw|ConvertFrom-Json
            if(!$selection.passed -or !$selection.inputsUnchanged -or $selection.ownerAttention -or !$selection.selectionHooksInverseVerified){throw 'Selection view focused proof incomplete'}
            $from=Join-Path $workspace 'continuity-evidence/document-native-broker-v2/Lea.Win.dll'
            $pin=@('926C1A28093A7D7AB31BFA10A3252297ED76549BAB38D0045BD3A4DB4DABD855')
            $result.selectionViewSha256=$pin[0]
        }
        if($DecoderView -and $name -eq 'Lea.Win.dll'){
            $decoderProof=Join-Path $workspace 'continuity-evidence/decoder-owner-retirement-v9/result.json'
            Hold $decoderProof '0D796C789B451732C87161162BC32EF0890CB99414A22EA71AB8A96C6A5715B2'
            $decoder=Get-Content $decoderProof -Raw|ConvertFrom-Json
            if(!$decoder.passed -or !$decoder.inputsUnchanged -or $decoder.ownerAttention -or !$decoder.decoderAclRestored -or !$decoder.realWorkerCampaign -or !$decoder.selectionHooksInverseVerified){throw 'Decoder candidate lifecycle proof incomplete'}
            $from=Join-Path $workspace 'continuity-evidence/decoder-owner-retirement-v9/Lea.Win.dll'
            $pin=@('8CC105D4B983FC67E66EE97487BE0771AA42626838A8F668DDEC0E6FF99F183D')
            $result.selectionViewSha256=$pin[0]
            $result.decoderView=$true;$result.currentFullNativeQualified=$false;$result.legacyUiAUnresolved=$true
        }
        if($ShutdownView -and $name -eq 'Lea.Win.dll'){
            $shutdownProof=Join-Path $workspace 'continuity-evidence/decoder-owner-retirement-v10/result.json'
            Hold $shutdownProof 'EE71940D73E7A63839DF9806155090858225BC254A6370360ACC7F96EC1B3EC6'
            $shutdown=Get-Content $shutdownProof -Raw|ConvertFrom-Json
            if(!$shutdown.passed -or !$shutdown.inputsUnchanged -or $shutdown.ownerAttention -or !$shutdown.decoderAclRestored -or !$shutdown.realWorkerCampaign){throw 'Shutdown candidate focused proof incomplete'}
            $from=Join-Path $workspace 'continuity-evidence/decoder-owner-retirement-v10/Lea.Win.dll'
            $pin=@('2225EC83D0DB503FE7778F896E7FC41475F5B50C690FE89B4FE9604B46E856CA')
            $result.selectionViewSha256=$pin[0];$result.shutdownView=$true
        }
        if($InitialFontPair -and $name -in @('Lea.Engine.dll','Lea.Win.dll')){
            $from=Join-Path $workspace ('continuity-evidence/'+$fontFolder+'/'+$name)
            $pin=@($(if($name -eq 'Lea.Engine.dll'){$fontCorePin}else{$fontViewPin}))
            if($name -eq 'Lea.Win.dll'){$result.selectionViewSha256=$pin[0]}else{$result.coreSha256=$pin[0]}
        }
        if($pin.Count -ne 1){throw ('Runtime pin missing: '+$name)}
        Hold $from $pin[0];$to=Join-Path $bin $name;Copy-Item $from $to;Hold $to $pin[0]
    }
    $buildProof=Join-Path $workspace 'continuity-evidence/firespider-source-rebuild-v1/result.json'
    Hold $buildProof '9DDF92B7273943C6CB5481A8CCC8033F09053BE2C5968DC5E53EDA14E0A5F3DA'
    $compiler=(Get-Content $buildProof -Raw|ConvertFrom-Json).compiler;Hold $compiler.path $compiler.sha256
    $arguments=@(('"'+(Join-Path $stage 'Lea.Print.csproj')+'"'),'/nologo','/m:1','/nodeReuse:false','/t:Rebuild','/v:minimal','/p:Configuration=Release','/p:EnableWpfPrint=true',('/p:LeaCorePath="'+(Join-Path $bin 'Lea.Engine.dll')+'"'),('/p:LeaViewPath="'+(Join-Path $bin 'Lea.Win.dll')+'"'),('/p:OutputPath="'+$bin+'"'))
    $result.buildArguments=$arguments
    if($SelectionView){$arguments+='/p:EnableDocumentSelection=true';$result.buildArguments=$arguments}
    $build=Invoke-OwnedTestRun -Name 'build-print-project' -Application $compiler.path -Arguments $arguments -WorkingDirectory $stage -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$build.passed){throw 'Direct print project compilation failed'}
    $library=Join-Path $bin 'Lea.Print.dll';Hold $library
    $result.librarySha256=(Get-FileHash $library).Hash
    $run=Invoke-OwnedTestRun -Name 'project-print-capture' -Application (Join-Path $bin 'PrintTextChecks.exe') -Arguments @(('"'+(Join-Path $bin 'native-selection.html')+'"')) -WorkingDirectory $bin -OutputDirectory $out -Seconds 30 -Records $runs
    $result.footerVerified=@(Get-Content $run.stdout|Where-Object {$_ -ceq 'PASS Lea live styled capture and foundation'}).Count -eq 1
    $result.behaviorPassed=$run.passed -and $result.footerVerified
    if($Range){
        if(!$result.behaviorPassed){throw 'Prior matching print checks failed'}
        $testStage=Join-Path $out 'test-source';$null=New-Item -ItemType Directory -Path $testStage
        foreach($name in @('Lea.Print.Range.Tests.csproj','PrintRange.cs','PrintDevice.cs','PrintDialog.cs','PrintTicket.cs')){
            $from=Join-Path $workspace ('Lea.Tests/'+$name);Hold $from
            $to=Join-Path $testStage $name;Copy-Item $from $to;Hold $to
        }
        $testBuild=Invoke-OwnedTestRun -Name 'build-range-tests' -Application $compiler.path -Arguments @(('"'+(Join-Path $testStage 'Lea.Print.Range.Tests.csproj')+'"'),'/nologo','/m:1','/nodeReuse:false','/t:Rebuild','/v:minimal',('/p:LeaPrintPath="'+$library+'"'),('/p:LeaCorePath="'+(Join-Path $bin 'Lea.Engine.dll')+'"'),('/p:OutputPath="'+$bin+'"')) -WorkingDirectory $testStage -OutputDirectory $out -Seconds 120 -Records $runs
        if(!$testBuild.passed){throw 'Range test compilation failed'}
        $testExe=Join-Path $bin 'Lea.Print.Range.Tests.exe';Hold $testExe
        $rangeRun=Invoke-OwnedTestRun -Name 'print-range' -Application $testExe -Arguments @() -WorkingDirectory $bin -OutputDirectory $out -Seconds 30 -Records $runs
        $result.rangePassed=$rangeRun.passed -and @(Get-Content $rangeRun.stdout|Where-Object {$_ -match '^PASS Lea print range: [0-9]+$'}).Count -eq 1
        if(!$result.rangePassed){throw 'Range checks failed'}
        if($Device){
            $result.devicePassed=@(Get-Content $rangeRun.stdout|Where-Object {$_ -match '^PASS Lea printer geometry: [0-9]+$'}).Count -eq 1
            if(!$result.devicePassed){throw 'Printer geometry footer missing'}
        }
        if($Dialog){
            $result.dialogPassed=@(Get-Content $rangeRun.stdout|Where-Object {$_ -match '^PASS Lea native printer selection: [0-9]+$'}).Count -eq 1
            if(!$result.dialogPassed){throw 'Native selection footer missing'}
        }
        if($Ticket){
            $result.ticketPassed=@(Get-Content $rangeRun.stdout|Where-Object {$_ -match '^PASS Lea prepared printer ticket: [0-9]+$'}).Count -eq 1
            if(!$result.ticketPassed){throw 'Prepared ticket footer missing'}
        }
    }
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.behaviorPassed -and !$result.error -and $result.inputsUnchanged -and !$result.ownerAttention
    if($result.ownerAttention){$global:LeaPrintProjectInputLeases=$leases}else{foreach($s in $leases){$s.Dispose()}}
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 8))
    [pscustomobject]$result|Select-Object passed,behaviorPassed,inputsUnchanged,ownerAttention,error|ConvertTo-Json -Compress
}
