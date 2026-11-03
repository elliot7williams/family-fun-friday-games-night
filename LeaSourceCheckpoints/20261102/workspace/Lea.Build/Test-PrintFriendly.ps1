param([switch]$KeepForegroundHost,[ValidateRange(1,99)][int]$Version=1,[switch]$Live,[switch]$Html,[switch]$ProjectHtml,[switch]$Images,[switch]$CacheView,[switch]$Bridge,[switch]$Decoder,[switch]$DataImages,[switch]$LayoutImages,[switch]$DocSelection,[switch]$DocGestures,[switch]$DocCopy,[switch]$DocUia,[switch]$SelectionLibrary,[switch]$DocBroker,[switch]$BuildBrokerView,[switch]$NativeBrokerOnly,[switch]$DecoderLibrary,[switch]$ShutdownLibrary,[switch]$ProviderThreadProbe,[switch]$RangeLeaseProbe,[switch]$InitialFontLibrary,[switch]$RelativeFontLibrary)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
if($Html -and !$Live){throw 'HTML checks require actual live snapshot library'}
if($ProjectHtml -and !$Html){throw 'Candidate library mode requires HTML checks'}
if($Images -and (!$Html -or ($ProjectHtml -and !$Bridge))){throw 'Image checks require source preparation or explicit bridge library mode'}
if($Bridge -and (!$ProjectHtml -or !$Images -or $CacheView)){throw 'Bridge checks require exact actual library mode, not source view rebuild'}
if($CacheView -and !$Images){throw 'Cache view checks require image preparation checks'}
if($Decoder -and !$Bridge){throw 'Decoder roundtrip requires exact bridge library mode'}
if($DataImages -and !$Decoder){throw 'Data image recapture requires real decoder checks'}
if($LayoutImages -and !$DataImages){throw 'Image layout checks include real data-image roundtrip'}
if($DocSelection -and !$CacheView){throw 'Selection foundation requires explicit mature view build'}
if($DocSelection -and !$DocGestures){throw 'Current selection source requires gesture hooks; use historical checkpoint for API-only build'}
if($DocGestures -and !$DocSelection){throw 'Gesture checks require selection API'}
if($DocCopy -and !$DocGestures){throw 'Document copy checks include selection gestures'}
$prefix=if($DocCopy){'document-copy-v'}elseif($DocGestures){'document-selection-gestures-v'}elseif($DocSelection){'document-selection-api-v'}elseif($LayoutImages){'print-image-layout-live-v'}elseif($DataImages){'print-data-image-live-v'}elseif($Decoder){'print-decoder-roundtrip-v'}elseif($Bridge){'print-cache-bridge-v'}elseif($CacheView){'print-cache-images-v'}elseif($Images){'print-html-images-v'}elseif($ProjectHtml){'print-html-library-live-v'}elseif($Html){'print-friendly-html-v'}elseif($Live){'print-friendly-live-v'}else{'print-friendly-v'}
$out=Join-Path $root ('continuity-evidence/'+$prefix+$Version)
if($SelectionLibrary){
    if(!$LayoutImages -or !$Bridge){throw 'Matched selection library requires full actual decoder/layout mode'}
    $out=Join-Path $root ('continuity-evidence/selection-image-live-v'+$Version)
}
if($DecoderLibrary){
    if(!$SelectionLibrary -or !$Decoder -or $CacheView -or $DocBroker){throw 'Decoder library requires actual matched selection/image library mode'}
    $out=Join-Path $root ('continuity-evidence/decoder-image-live-v'+$Version)
}
if($ShutdownLibrary){if(!$DecoderLibrary){throw 'Shutdown library requires decoder mode'};$out=Join-Path $root ('continuity-evidence/shutdown-image-live-v'+$Version)}
if($InitialFontLibrary){if(!$ShutdownLibrary){throw 'Initial font library requires full shutdown mode'};$out=Join-Path $root ('continuity-evidence/initial-font-image-live-v'+$Version)}
if($RelativeFontLibrary){if(!$InitialFontLibrary){throw 'Relative library requires complete font regression mode'};$out=Join-Path $root ('continuity-evidence/relative-font-image-live-v'+$Version)}
if($DocUia){
    if(!$DocCopy){throw 'UIA checks include copy/gesture regression'}
    $out=Join-Path $root ('continuity-evidence/document-uia-v'+$Version)
}
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY owner required'}
if($DocBroker){if(!$DocUia){throw 'Broker isolation requires document UIA checks'};$out=Join-Path $root ('continuity-evidence/document-broker-v'+$Version)}
if($BuildBrokerView -and !$DocBroker){throw 'Broker view rebuild requires explicit broker checks'}
if($NativeBrokerOnly){if(!$DocBroker){throw 'Native client campaign requires explicit broker fixture'};$out=Join-Path $root ('continuity-evidence/document-native-broker-v'+$Version)}
if($ProviderThreadProbe){
    if(!$BuildBrokerView -or !$DocBroker -or $NativeBrokerOnly){throw 'Threading diagnostic requires rebuilt full legacy broker fixture'}
    $out=Join-Path $root ('continuity-evidence/document-threading-probe-v'+$Version)
}
if($RangeLeaseProbe){
    if(!$BuildBrokerView -or !$DocBroker -or $NativeBrokerOnly -or $ProviderThreadProbe){throw 'Range lease diagnostic requires rebuilt full broker and no threading probe'}
    $out=Join-Path $root ('continuity-evidence/document-range-lease-probe-v'+$Version)
}
if((Test-Path $out) -or (Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue)){throw 'Fresh owner/evidence required'}
if((Get-PSDrive C).Free -lt 600MB){throw 'Storage reserve required'}
if($Live -and (([Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator))){throw 'Nonadmin live owner required'}
if($Live -and @(Get-Process 'Firespider*','Lea.Script.Tests','Lea.Script.Worker','Lea.Firespider.Boot','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'No browser/test overlap'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
function Hold([string]$path,[string]$hash){
    $s=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($s)
    $actual=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($hash -and $actual -cne $hash){throw "Pin differs: $path"}
    $inputs.Add(@{path=$path;sha256=$actual})
}
$null=New-Item -ItemType Directory -Path $out
$result=[ordered]@{passed=$false;inputs=$inputs;runs=$runs;browserAdopted=$false;printJobSubmitted=$false}
try{
    Hold $PSCommandPath;Copy-Item $PSCommandPath (Join-Path $out 'runner.ps1')
    $owned=Join-Path $PSScriptRoot 'OwnedTestProcess.cs';Hold $owned 'D553F33A48B9B7DD5CF97012E6974C4B4054C315B19FD0086C05543844242AFA'
    $helper=Join-Path $PSScriptRoot 'Invoke-OwnedTestRun.ps1';Hold $helper 'E5DD42C23546F58CEF425DE5602CC39D478409A824FDBB4C19F5CB454FC0FC00'
    Add-Type -Path $owned;. $helper
    $core=Join-Path $root 'continuity-evidence/print-ticket-project-v1/bin/Lea.Engine.dll'
    $coreHash='A85B02C678862D75FA764397C6BB6B3D7B964ECB0C8E13722F6D17DC44AA3791'
    if($InitialFontLibrary){$core=Join-Path $root 'continuity-evidence/font-initial-render-v1/Lea.Engine.dll';$coreHash='1474123BA6B52A3857B32D29FC85A462AA10AD1ABB933C19E93FD63DCFFCEEBA'}
    if($RelativeFontLibrary){$core=Join-Path $root 'continuity-evidence/font-relative-render-v1/Lea.Engine.dll';$coreHash='4DA8BF1968218A97AF3EC053DDE0E22EEB218669EF1F3B4BCFEA3A39F8ABDFBA'}
    Hold $core $coreHash
    Copy-Item $core (Join-Path $out 'Lea.Engine.dll');Hold (Join-Path $out 'Lea.Engine.dll') $coreHash
    $compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe';Hold $compiler
    $refs='C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8'
    $exe=Join-Path $out 'PrintFriendlyChecks.exe'
    $arguments=@('/nologo','/noconfig','/nostdlib+','/target:exe','/platform:x64','/warnaserror+',('/out:"'+$exe+'"'),('/reference:"'+$core+'"'))
    foreach($name in @('mscorlib.dll','System.dll','System.Core.dll','System.Drawing.dll')){
        $reference=Join-Path $refs $name;Hold $reference;$arguments+=('/reference:"'+$reference+'"')
    }
    if($Live){
        $project=if($LayoutImages){'print-image-layout-project-v1'}elseif($DataImages){'print-data-image-project-v1'}elseif($Bridge){'print-cache-project-v2'}elseif($ProjectHtml){'print-html-project-v1'}else{'print-friendly-project-v1'}
        $proof=Join-Path $root ('continuity-evidence/'+$project+'/result.json')
        if($SelectionLibrary){$project='selection-print-project-v5';$proof=Join-Path $root ('continuity-evidence/'+$project+'/result.json')}
        if($DecoderLibrary){$project='decoder-print-project-v1';$proof=Join-Path $root ('continuity-evidence/'+$project+'/result.json')}
        if($ShutdownLibrary){$project='shutdown-print-project-v1';$proof=Join-Path $root ('continuity-evidence/'+$project+'/result.json')}
        if($InitialFontLibrary){$project='initial-font-print-project-v1';$proof=Join-Path $root ('continuity-evidence/'+$project+'/result.json')}
        if($RelativeFontLibrary){$project='relative-font-print-project-v1';$proof=Join-Path $root ('continuity-evidence/'+$project+'/result.json')}
        $proofHash=if($DataImages){'49ECE0540A0D9544B051728E250BA61C3910392499E3FE226B6D307B21BE58A5'}elseif($Bridge){'3E1B7958720F1097FFC7063FFC6CDD6664EABBF995E0CBDDECBF4CA0FCD20109'}elseif($ProjectHtml){'7586782F4CFEBBA5893B75647CEF29B1C5966474714430A10233E02AC0FB79A6'}else{'120081FE1A8B946D16D36BCD4EA889AD24F43FAF80FEAFAB12EAD6986F094541'}
        if($LayoutImages){$proofHash='E1710956780923B633CF36BE5548C32440A8D00F56295446BF88DD4CDCB4AC0C'}
        if($SelectionLibrary){$proofHash='7C696BB40C353C25BB67513B23BB19AC613BD535014D058576C7C004FD4DDD63'}
        if($DecoderLibrary){$proofHash='9577146D8D08906833F3717D7047401E3B94FBA810382BA2F4319FCA85AC831D'}
        if($ShutdownLibrary){$proofHash='8B9A1DA0D1B25F6E9EC8BE95F9FC265BFB6E56644A99C71615595BFF1F91D09E'}
        if($InitialFontLibrary){$proofHash='20BE9ED2377F96857A062F3616FAD3841BB3ABD62A001335DE44A402E66F8259'}
        if($RelativeFontLibrary){$proofHash='A71A3BC94E9584876DCE5480027EBAD4585A2900E6F01651C65D4869FA2D8002'}
        Hold $proof $proofHash
        if($ProjectHtml){
            $buildProof=Get-Content $proof -Raw|ConvertFrom-Json
            if(!$buildProof.runs[0].passed -or !$buildProof.inputsUnchanged -or $buildProof.ownerAttention){throw 'Candidate compilation/ownership proof incomplete'}
            if($Bridge -and !$buildProof.passed){throw 'Exact bridge project prior regression incomplete'}
            $result.fullProjectRegressionPassed=[bool]$buildProof.passed
        }
        $printHash=if($DataImages){'4405C081AC9073FEA2F4940A9F49E54D7B14B11D907EA837B4C290FC7843CA3E'}elseif($Bridge){'4E88BDA503BACEAB0DB3BD748E33133810D05471548066B06451CF4BB99E0F9A'}elseif($ProjectHtml){'F14AD95878DC0824BA02DF146F8047D5BC750062B30A250CA42236E7C650ED6C'}else{'453968A8D218CBCEA15E417F9218713C1BC2496D6CE598AA92425E7D3E104B79'}
        if($LayoutImages){$printHash='750E606AD95F3AF0856BB8C73749A0F92F1A8D63ED77438325B87907749EAFCF'}
        if($SelectionLibrary){$printHash='BFCE52A29491D1CDE4F9E507CE467031C36474F391B5939198B7C0C61999D708'}
        $viewHash=if($Bridge){'A921DDF96C9648014305DE36D40A165E55C387125B6B73C41AB5CF4A3D0B0AEF'}else{'568A26396DA540C56546447DDC6277878ED5A5A71AF9712559BF8E3A894DADF8'}
        if($SelectionLibrary){$viewHash='926C1A28093A7D7AB31BFA10A3252297ED76549BAB38D0045BD3A4DB4DABD855'}
        if($DecoderLibrary -and !$ShutdownLibrary){
            if(!$buildProof.decoderView -or $buildProof.currentFullNativeQualified -ne $false -or !$buildProof.legacyUiAUnresolved){throw 'Explicit decoder candidate/native boundary required'}
            $printHash='E525BEC5530E4954F8BD9668E108A073B6208C81DC25E935AE24964FDECD3E46'
            $viewHash='8CC105D4B983FC67E66EE97487BE0771AA42626838A8F668DDEC0E6FF99F183D'
            if($buildProof.librarySha256 -cne $printHash -or $buildProof.selectionViewSha256 -cne $viewHash){throw 'Matched decoder print/view pair differs'}
            $result.currentFullNativeQualified=$false;$result.legacyUiAUnresolved=$true
        }
        if($ShutdownLibrary){
            if(!$buildProof.shutdownView -or !$buildProof.decoderView -or $buildProof.currentFullNativeQualified -ne $false -or !$buildProof.legacyUiAUnresolved){throw 'Shutdown candidate/native boundary required'}
            $printHash='D94B951071528562EB1F546E636196B1F02DF03C5965323F96E7A338E179AEC4'
            $viewHash='2225EC83D0DB503FE7778F896E7FC41475F5B50C690FE89B4FE9604B46E856CA'
            if($InitialFontLibrary){
                $printHash='271BA491D0496E990DC67B8B849310241BCE277F4E0CDF6070C1854305812C1C';$viewHash='6E342B9B68C54803B91CF796F082C788DC86E29C1A7CEFAA571302E5E4636B4F'
                if($RelativeFontLibrary){$printHash='0C16B3D0113A91F83274F37D48C4D7B5F9BAC8DC1E292FBBBF318BF908711CFB';$viewHash='CA51FC818B119529EA8F7104D0753C5779070A7CCF0AD157675A564C4755C584'}
                if($buildProof.coreSha256 -cne $coreHash){throw 'Initial font print core differs'}
            }
            if($buildProof.librarySha256 -cne $printHash -or $buildProof.selectionViewSha256 -cne $viewHash){throw 'Matched shutdown pair differs'}
            $result.currentFullNativeQualified=$false;$result.legacyUiAUnresolved=$true
        }
        foreach($binding in @(@('Lea.Print.dll',$printHash),@('Lea.Win.dll',$viewHash))){
            if($CacheView -and $binding[0] -eq 'Lea.Win.dll'){
                if($DocBroker -and !$BuildBrokerView){
                    $from=Join-Path $root 'continuity-evidence/document-uia-v5/Lea.Win.dll'
                    $hash='2E4264F2008C451D0F8FB42DE3F5229AF9D9817142E4ED02F785905820687CE7';Hold $from $hash
                    $to=Join-Path $out 'Lea.Win.dll';Copy-Item $from $to;Hold $to $hash;$arguments+=('/reference:"'+$to+'"');$result.exactBrokerViewSha256=$hash
                    continue
                }
                $builder=Join-Path $PSScriptRoot 'Build-PrintImageView.ps1';Hold $builder;. $builder
                $to=Join-Path $out 'Lea.Win.dll';Hold $to;$arguments+=('/reference:"'+$to+'"');$result.cacheViewSourceAddition=$true
                continue
            }
            $from=Join-Path $root ('continuity-evidence/'+$project+'/bin/'+$binding[0]);Hold $from $binding[1]
            $to=Join-Path $out $binding[0];Copy-Item $from $to;Hold $to $binding[1];$arguments+=('/reference:"'+$to+'"')
        }
        foreach($name in @('System.Xml.dll','System.Xml.Linq.dll','System.Windows.Forms.dll','WindowsBase.dll','PresentationCore.dll','PresentationFramework.dll','System.Xaml.dll','ReachFramework.dll')){
            $reference=Join-Path $refs $name;Hold $reference;$arguments+=('/reference:"'+$reference+'"')
        }
        $arguments+='/main:PrintFriendlyLiveMain'
        $fixture=Join-Path $root 'LeaForge/test-pages/print-friendly.html';Hold $fixture
        Copy-Item $fixture (Join-Path $out 'print-friendly.html')
    }
    $sources=if($Live){@('Lea.Tests/PrintSnapshot.cs','Lea.Tests/PrintFriendlyMain.cs','Lea.Tests/PrintFriendlyLiveMain.cs','Lea.Tests/PrintCaptureProbe.cs')}else{@('Lea.Print/Ps.cs','Lea.Print/Py.cs','Lea.Tests/PrintSnapshot.cs','Lea.Tests/PrintFriendlyMain.cs')}
    if($Html){
        $arguments+='/define:FRIENDLY_HTML';$sources+='Lea.Tests/PrintHtmlChecks.cs'
        if(!$ProjectHtml){$sources+='Lea.Print/Ht.cs'}
        $result.htmlSourceHarness=!$ProjectHtml
    }
    if($Html -and !$ProjectHtml){$sources+='Lea.Print/Pi.cs'}
    if($Images){$arguments+='/define:FRIENDLY_IMAGES';$sources+='Lea.Tests/PrintImageChecks.cs'}
    if($CacheView -or $Bridge){
        $arguments+='/define:FRIENDLY_CACHE';$sources+='Lea.Tests/PrintCacheImageChecks.cs'
        $fixture=Join-Path $root 'LeaForge/test-pages/print-cache-image.html';Hold $fixture;Copy-Item $fixture (Join-Path $out 'print-cache-image.html')
    }
    if($Bridge){$arguments+='/define:FRIENDLY_BRIDGE'}
    if($SelectionLibrary){$arguments+='/define:DOCUMENT_PRINT'}
    if($DocSelection -or $SelectionLibrary){
        $arguments+='/define:DOCUMENT_SELECTION';$sources+='Lea.Tests/DocumentSelectionChecks.cs'
        $fixture=Join-Path $root 'LeaForge/test-pages/document-selection.html';Hold $fixture;Copy-Item $fixture (Join-Path $out 'document-selection.html')
    }
    if($DocGestures -or $SelectionLibrary){$arguments+='/define:DOCUMENT_GESTURES';$sources+='Lea.Tests/DocumentSelectionGestures.cs'}
    if($DocCopy -or $SelectionLibrary){$arguments+='/define:DOCUMENT_COPY';$sources+='Lea.Tests/DocumentCopyChecks.cs'}
    if($DocUia -or $SelectionLibrary){
        $arguments+='/define:DOCUMENT_UIA';$sources+='Lea.Tests/DocumentSelectionAutomationChecks.cs'
        foreach($name in @('UIAutomationProvider.dll','UIAutomationTypes.dll')){
            $reference=Join-Path $refs $name;Hold $reference;$arguments+=('/reference:"'+$reference+'"')
        }
    }
    if($DataImages){$arguments+='/define:FRIENDLY_DATA'}
    if($DocBroker){
        if($NativeBrokerOnly){$arguments+='/define:NATIVE_BROKER_ONLY';$result.legacyBrokerQualified=$false}
        $arguments+='/define:DOCUMENT_BROKER';$sources+=@('Lea.Tests/DocumentBrokerChecks.cs','Lea.Tests/DocumentRangeAbiChecks.cs','Lea.Tests/DocumentRangeApartmentChecks.cs','Lea.Tests/DocumentNativeClientChecks.cs','Lea.Tests/DocumentLegacyHandleChecks.cs')
        $reference=Join-Path $refs 'UIAutomationClient.dll';Hold $reference;$arguments+=('/reference:"'+$reference+'"')
    }
    if($LayoutImages){$arguments+='/define:FRIENDLY_LAYOUT';$sources+='Lea.Tests/PrintWpfImageChecks.cs'}
    if($Decoder){
        $arguments+='/define:FRIENDLY_DECODER';$sources+='Lea.Tests/PrintDecoderRoundTrip.cs'
        $worker=Join-Path $root 'continuity-evidence/nested-overlap-regression-v1/runtime/Lea.Image.exe'
        Hold $worker 'FE744BE1D11E99B36A35F4E6CF12C45229AB7FF89134434E61438DE8DED3E535'
        $target=Join-Path $out 'Lea.Image.exe';Copy-Item $worker $target;Hold $target 'FE744BE1D11E99B36A35F4E6CF12C45229AB7FF89134434E61438DE8DED3E535'
        $result.decoderAclBefore=(Get-Acl $target).Sddl;$result.decoderSecurityQualified=$false
    }
    foreach($relative in $sources){
        $source=Join-Path $root $relative
        $pin=if($relative -eq 'Lea.Tests/PrintSnapshot.cs'){'C7E0E507FE1A94B000442834FA67CB05420AF65475FC26B50BF607BC6F84989B'}else{$null}
        Hold $source $pin
        Copy-Item $source (Join-Path $out ([IO.Path]::GetFileName($source)))
        $arguments+=('"'+$source+'"')
    }
    $build=Invoke-OwnedTestRun -Name 'compile-friendly' -Application $compiler -Arguments $arguments -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
    if(!$build.passed){throw 'Friendly capture compilation failed'}
    Hold $exe
    $runArguments=if($Live){@(('"'+(Join-Path $out 'print-friendly.html')+'"'),('"'+$out+'"'))}else{@('"'+$out+'"')}
    $run=Invoke-OwnedTestRun -Name 'friendly-capture' -Application $exe -Arguments $runArguments -WorkingDirectory $out -OutputDirectory $out -Seconds 30 -Records $runs
    if(!$run.passed){throw 'Friendly capture checks failed'}
    $result.checks=Get-Content (Join-Path $out 'friendly.json') -Raw|ConvertFrom-Json
    $result.passed=$result.checks.passed -and $result.checks.baselineChecks -gt 0 -and $result.checks.friendlyChecks -gt 0 -and !$result.checks.browserRouted
    if($Live){
        $result.live=Get-Content (Join-Path $out 'live.json') -Raw|ConvertFrom-Json
        $result.passed=$result.passed -and $result.live.passed -and $result.live.liveChecks -gt 0 -and !$result.live.printJobSubmitted -and !$result.live.hostRouted
    }
}catch{$result.error=$_.Exception.ToString()}
finally{
    if($Decoder -and (Test-Path (Join-Path $out 'Lea.Image.exe'))){$result.decoderAclAfter=(Get-Acl (Join-Path $out 'Lea.Image.exe')).Sddl;$result.decoderAclChanged=$result.decoderAclBefore -cne $result.decoderAclAfter}
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -ne 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if($DecoderLibrary){$result.passed=$result.passed -and !$result.decoderAclChanged;$result.decoderAclRestored=!$result.decoderAclChanged}
    if(!$result.ownerAttention){foreach($h in $holds){$h.Dispose()}}else{$global:LeaFriendlyInputLeases=$holds}
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 9))
    [pscustomobject]$result|Select-Object passed,error,ownerAttention,inputsUnchanged,checks|ConvertTo-Json -Depth 5
}
