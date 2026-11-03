$ErrorActionPreference='Stop'
$target=Join-Path $PSScriptRoot 'Test-DecoderQueueNative.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve native decoder diagnostic'}
$text=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Test-DecoderQueueTrace.generated.ps1'))
$text=$text.Replace('decoder-queue-trace-v1','decoder-queue-native-v1').Replace('Controlled decoder queue phase diagnostic, no native worker launch, not full suite or reliability proof','Original picture-test sequence with queue phase diagnostics, not the full suite or reliability qualification')
$start=[regex]::Match($text,'(?m)^try\{').Index
$end=$text.IndexOf('}catch{$result.error=',$start)
if($start -le 0 -or $end -le $start){throw 'Native diagnostic body span differs'}
$body=@'
try{
    function FreshCopy([string]$from,[string]$to,[string]$hash){
        Hold $from $hash
        $null=New-Item -ItemType Directory -Path (Split-Path $to) -Force
        $input=[IO.File]::OpenRead($from);$output=[IO.File]::Open($to,'CreateNew','Write','None')
        try{$input.CopyTo($output)}finally{$input.Dispose();$output.Dispose()}
        Hold $to $hash
    }
    if(@(Get-Process -Name 'Firespider*','Lea.Firespider.Boot','Lea.Tests','Lea.Script.Tests','Lea.Script.Worker','Lea.Image' -ErrorAction SilentlyContinue).Count){throw 'Do not overlap native tests'}
    $tracePath=Join-Path $workspace 'continuity-evidence/decoder-queue-trace-v1/result.json';Hold $tracePath '6AF56EF9D858E49A60FA050CED05A2430443C4AA8588B371CAB57BDA3E50BC0D'
    $trace=Get-Content -LiteralPath $tracePath -Raw|ConvertFrom-Json
    if(!$trace.passed -or !$trace.inputsUnchanged -or !$trace.candidateUnchanged -or $trace.ownerAttention -or !$trace.compatibilityProofPassed){throw 'Queue trace proof incomplete'}
    $basePath=Join-Path $workspace 'continuity-evidence/ordered-flow-regression-v2/result.json';Hold $basePath '921E6A0C44CA04EF0CD96C228B02C0231A1B9B66571EA877F664E47F4FC5D7ED'
    $b=Get-Content -LiteralPath $basePath -Raw|ConvertFrom-Json
    if(!$b.inputsUnchanged -or !$b.bytesUnchanged -or $b.ownerAttention -or $b.behaviorPassed -or $b.before.Count -ne 5){throw 'Failed candidate binding differs'}
    foreach($file in $b.before){
        $from=$file.path;$hash=$file.sha256;$name=[IO.Path]::GetFileName($from)
        if($name -ceq 'Lea.Win.dll'){$from=$trace.viewBuild.binary;$hash=$trace.viewBuild.sha256}
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
    $run=Run 'native-pictures' (Join-Path $result.candidate 'Lea.Tests.exe') @(('"'+$fixture+'"'),'--pics') 180
    $result.nativeBehaviorPassed=$run.passed -and ([IO.File]::ReadAllText($run.stdout).Trim().EndsWith('Lea picture limit smoke passed.',[StringComparison]::Ordinal))
    $result.passed=$result.nativeBehaviorPassed
    $result.queueDiagnosticLines=@(Get-Content -LiteralPath $run.stderr|Where-Object {$_ -like '*queue=*'})
    $result.diagnosticCaptured=$run.exitObserved -and !$run.timedOut -and !$run.killRequested -and $run.stdoutComplete -and $run.stderrComplete

'@
$text=$text.Remove($start,$end-$start).Insert($start,$body)
$anchor='    $result.passed=$result.passed -and $result.inputsUnchanged -and $result.candidateUnchanged -and !$result.ownerAttention'
$extra=@'
    $result.candidateBytesUnchanged=if($result.before){@($result.before|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0}else{$false}
    $result.diagnosticCaptured=$result.diagnosticCaptured -and $result.inputsUnchanged -and $result.candidateBytesUnchanged -and !$result.ownerAttention
    $result.fullSuitePassed=$false
'@
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Native finalization anchor differs'}
$text=$text.Replace($anchor,$extra+"`r`n"+$anchor)
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw ($errors|Out-String)}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
