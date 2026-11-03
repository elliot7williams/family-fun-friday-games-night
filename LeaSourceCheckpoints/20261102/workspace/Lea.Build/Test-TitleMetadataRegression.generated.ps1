param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$outRoot=Join-Path $workspace 'continuity-evidence/title-metadata-regression-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained foreground owner required'}
if(Test-Path -LiteralPath $outRoot){throw 'Preserve visual evidence'}
$utility=Join-Path $workspace 'continuity-evidence/native-launch-timing-v1/Test-CompositeCandidate.v2.ps1'
if((Get-FileHash -LiteralPath $utility).Hash -cne 'EB00AD17B8078A6CC2F418733BA5D675FC84EF49EED433A527FAC5498DD5ECEF'){throw 'Process utility differs'}
$tokens=$null;$errors=$null;$ast=[Management.Automation.Language.Parser]::ParseFile($utility,[ref]$tokens,[ref]$errors)
foreach($name in @('Space','NewOutputReader','PumpOutput','Run')){
    $defs=@($ast.FindAll({param($node)$node -is [Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -ceq $name},$false))
    if($errors.Count -or $defs.Count -ne 1){throw 'Process utility shape differs'}
    . ([scriptblock]::Create($defs[0].Extent.Text))
}
if(Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue){throw 'Fresh owner required'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new()
function Hold([string]$Path,[string]$Expected){
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($stream)
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) -cne $Expected){throw 'Visual input differs'}
    $inputs.Add(@{path=$Path;sha256=$Expected})
}
$result=[ordered]@{passed=$false;candidate=(Join-Path $outRoot 'runtime');inputs=$inputs;runs=$runs;error=$null;images=[Collections.Generic.List[object]]::new();humanVisualReviewComplete=$false;releaseCertified=$false;scope='Full retained engine/view sequence with queue phase diagnostics; not scripting, host, or cold reliability qualification'}
Space;$null=New-Item -ItemType Directory -Path $result.candidate
try{
    function FreshCopy([string]$from,[string]$to,[string]$hash){
        Hold $from $hash
        $null=New-Item -ItemType Directory -Path (Split-Path $to) -Force
        $input=[IO.File]::OpenRead($from);$output=[IO.File]::Open($to,'CreateNew','Write','None')
        try{$input.CopyTo($output)}finally{$input.Dispose();$output.Dispose()}
        Hold $to $hash
    }
    if(@(Get-Process -Name 'Firespider*','Lea.Firespider.Boot','Lea.Tests','Lea.Script.Tests','Lea.Script.Worker','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'Do not overlap native tests'}
    $tracePath=Join-Path $workspace 'continuity-evidence/title-metadata-visuals-v1/result.json';Hold $tracePath '04CB8E02EAC3FDEF82D2006C7E6C504C3A8913467DB4926EDFA5701A6D2103D4'
    $trace=Get-Content -LiteralPath $tracePath -Raw|ConvertFrom-Json
    if(!$trace.passed -or !$trace.inputsUnchanged -or !$trace.candidateUnchanged -or $trace.ownerAttention -or !$trace.viewBuild.titleOverlayInverseVerified -or !$trace.baselineRegressionReproduced){throw 'Queue trace proof incomplete'}
    $basePath=Join-Path $workspace 'continuity-evidence/ordered-flow-regression-v2/result.json';Hold $basePath '921E6A0C44CA04EF0CD96C228B02C0231A1B9B66571EA877F664E47F4FC5D7ED'
    $b=Get-Content -LiteralPath $basePath -Raw|ConvertFrom-Json
    $testProof=Join-Path $workspace 'continuity-evidence/ordered-flow-regression-v4/result.json';Hold $testProof '9A1C1B073D72892D652542D9CEAF32C2D5620696B217C5C103309802A597C9C3'
    $test=Get-Content -LiteralPath $testProof -Raw|ConvertFrom-Json
    if(!$test.inputsUnchanged -or !$test.bytesUnchanged -or $test.ownerAttention -or !$test.nestedModelOverlayInverseVerified -or !$test.testOverlayInverseVerified){throw 'Updated test source binding incomplete'}
    $testRows=@($test.before|Where-Object {[IO.Path]::GetFileName($_.path) -ceq 'Lea.Tests.exe'})
    if($testRows.Count -ne 1){throw 'Updated test binary ambiguous'}
    $coreProof=Join-Path $workspace 'continuity-evidence/ordered-flow-v6/result.json';Hold $coreProof '5C93311ACDA6D4509580C30E6EB8D85C458C28C3CBA6BC6247D93100EBAB9D7E'
    $core=Get-Content -LiteralPath $coreProof -Raw|ConvertFrom-Json
    if(!$core.passed -or !$core.inputsUnchanged -or $core.ownerAttention){throw 'V5 core binding incomplete'}
    if(!$b.inputsUnchanged -or !$b.bytesUnchanged -or $b.ownerAttention -or $b.behaviorPassed -or $b.before.Count -ne 5){throw 'Failed candidate binding differs'}
    foreach($file in $b.before){
        $from=$file.path;$hash=$file.sha256;$name=[IO.Path]::GetFileName($from)
        if($name -ceq 'Lea.Win.dll'){$from=$trace.viewBuild.binary;$hash=$trace.viewBuild.sha256}
        if($name -ceq 'Lea.Engine.dll'){$from=$core.binary;$hash=$core.sha256}
        if($name -ceq 'Lea.Tests.exe'){$from=$testRows[0].path;$hash=$testRows[0].sha256}
        FreshCopy $from (Join-Path $result.candidate $name) $hash
    }
    $fixture=Join-Path $outRoot 'fixtures'
    $sourceRoot=Join-Path $workspace 'continuity-evidence/request-diagnostics-v1/verified-build-20261020-150037/native/source'
    $count=0
    foreach($file in $b.inputs|Group-Object path|ForEach-Object {$_.Group[0]}){
        if(!$file.path.StartsWith($sourceRoot+'\',[StringComparison]::OrdinalIgnoreCase)){continue}
        $relative=[IO.Path]::GetRelativePath($sourceRoot,$file.path)
        if(!$relative.StartsWith('LeaForge\test-pages\',[StringComparison]::Ordinal) -and !$relative.StartsWith('assets\',[StringComparison]::Ordinal)){continue}
        FreshCopy $file.path (Join-Path $fixture $relative) $file.sha256;$count++
    }
    if($count -eq 0 -or !(Test-Path -LiteralPath (Join-Path $fixture 'assets/lea.ico')) -or !(Test-Path -LiteralPath (Join-Path $fixture 'LeaForge/test-pages/second.html'))){throw 'Picture fixtures incomplete'}
    $result.fixtureFiles=$count
    $sid=[Security.Principal.SecurityIdentifier]::new('S-1-15-2-3597768014-2038560105-1077951562-531713394-1456559389-3372869253-1355503848')
    foreach($file in Get-ChildItem -LiteralPath $result.candidate -File){
        $acl=Get-Acl -LiteralPath $file.FullName
        if(@($acl.GetAccessRules($true,$true,[Security.Principal.SecurityIdentifier])|Where-Object {$_.IdentityReference -eq $sid}).Count){throw 'Fresh private baseline contains prior decoder grant'}
    }
    Hold $PSCommandPath (Get-FileHash -LiteralPath $PSCommandPath).Hash;Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $outRoot 'runner.ps1')
    $result.before=@(Get-ChildItem -LiteralPath $result.candidate -File|ForEach-Object {@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash;sddl=(Get-Acl -LiteralPath $_.FullName).Sddl}})
    $footerHelper=Join-Path $PSScriptRoot 'Test-LeaSuiteFooter.ps1';Hold $footerHelper 'BFD1DD8FC9DACBCA1E7177AA740F8D7EA8974E5B4C4BB344C1DCD16055EE47AE';. $footerHelper
    $run=Run 'full-engine-traced' (Join-Path $result.candidate 'Lea.Tests.exe') @(('"'+$fixture+'"')) 300
    $result.fullBehaviorPassed=$run.passed -and (Test-LeaSuiteFooter $run.stdout 'Lea smoke passed: bounded sticky positioning with compact k, shared paint/hit/editor/accessibility geometry, normal-flow retention, and ancestor clamps, bounded has relational selectors with strict lists, maximum specificity, descendant/child/adjacent/following and complex chains, queries, dynamic focus rebuilds, retained fields, WinForms geometry, and traversal/part limits, bounded nth-child of-S filters with strict compound lists, maximum specificity, forward/reverse counting, queries, dynamic form-state rebuilds, retained fields, WinForms geometry, and nesting/test limits, bounded precompiled An+B and reverse structural selectors with stylesheet/query parity, specificity, selector-list composition, type filtering, WinForms geometry, and invalid limits, bounded is, where, and list-not selector functions with maximum/zero specificity, forgiving/strict parsing, queries, dynamic state, WinForms geometry, and retained fields, bounded CSS important cascade with compiled priority, G.Ic diagnostics, stylesheet/inline tiers, custom properties, hover/media rebuilds, and retained fields, bounded inherited CSS custom properties with compiled declarations, S.Vc/Cv and G.Vc aliases, nested and cycle fallbacks, hover/media rebuilds, and retained fields, bounded responsive CSS media queries with viewport aliases, live resize, diagnostics, and retained fields, optional bounded persistent cache with restart reuse, validators, private isolation, atomic recovery, LRU entry/byte ceilings and host aliases, bounded content-box and border-box geometry, body and ID-scoped style commands, ordered row and column flex layout with around/evenly distribution, item alignment, wrapping, growth, shrink, reverse flow, and resize, semantic accessibility with WinForms MSAA and native UI Automation fragments, patterns and notifications, protected values, live states, hierarchy and rendered bounds, bounded autofocus and access keys, tabindex focus, visible rings, keyboard link and shared image-map activation, compact output/data values, bounded host aliases, safe rebuilds, inline and table ownership, resize reflow, compact progress and meter values, quality regions, inherited custom painting, bounded retained fragment history, encoded and named targets, hidden reveal, bounded details disclosures with retained state, keyboard and table interaction, bounded ordered and unordered list markers, nested list scope and resize reflow, bounded host-controlled go requests, class mutations, document base URLs and Id/Q/Qa queries, bounded HTML tokenization, raw text, RCDATA, comments, entities and malformed delimiters, bounded developer diagnostics and inspector data, safe click commands and bounded variables/text updates, streamed downloads and highlighted find-in-page, bounded script commands and alert bridging, inset stretching and translated stacking contexts, nested forms, media, tables, and clipped editors, explicit z-index and opacity/isolation contexts, two-pass positioned auto height, nested visual flow, bounded multipart file uploads, range and color widgets, temporal fields, absolute and fixed positioning, clipped viewport hits, form validation and typed inputs, responsive and embedded images, live hover and focus restyling, visibility and opacity, general sibling and negation selectors, min/max sizing, long-word breaking, compiled attribute and pseudo selectors, fonts, overflow clipping, malformed HTML recovery, semantic blocks, tables, inline flow, external CSS, private cookies, cache revalidation, typed errors, POST, compression, layout, and scrolling.')
    $result.passed=$result.fullBehaviorPassed
    $result.queueDiagnosticLines=@(Get-Content -LiteralPath $run.stderr|Where-Object {$_ -like '*queue=*'})
    $result.diagnosticCaptured=$run.exitObserved -and !$run.timedOut -and !$run.killRequested -and $run.stdoutComplete -and $run.stderrComplete
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.candidateUnchanged=if($result.before){@($result.before|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256 -or (Get-Acl -LiteralPath $_.path).Sddl -cne $_.sddl}).Count -eq 0}else{$false}
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -gt 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.candidateBytesUnchanged=if($result.before){@($result.before|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0}else{$false}
    $result.diagnosticCaptured=$result.diagnosticCaptured -and $result.inputsUnchanged -and $result.candidateBytesUnchanged -and !$result.ownerAttention
    $result.scriptingSuiteExecuted=$false
    $result.passed=$result.passed -and $result.inputsUnchanged -and $result.candidateUnchanged -and !$result.ownerAttention
    if($result.ownerAttention){$global:LeaVisualInputLeases=$leases}else{foreach($lease in $leases){$lease.Dispose()}}
    $global:LeaVisualResult=$result
    [IO.File]::WriteAllText((Join-Path $outRoot 'result.json'),($result|ConvertTo-Json -Depth 7))
}
[pscustomobject]$result|Select-Object passed,inputsUnchanged,candidateUnchanged,ownerAttention,error|ConvertTo-Json
