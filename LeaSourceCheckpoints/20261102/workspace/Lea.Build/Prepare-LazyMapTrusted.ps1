param([ValidateSet(1,2)][int]$Version=2)
$ErrorActionPreference='Stop'
$from=Join-Path $PSScriptRoot 'Test-GlobalInstallScheduling-v2.generated.ps1'
if((Get-FileHash $from).Hash -cne '6217862EF77C9554702F2036A0085397FF6403D7F6017290B8C292623A039280'){throw 'Trusted scheduling template differs'}
$to=Join-Path $PSScriptRoot "Test-LazyMapTrusted-v$Version.generated.ps1"
if(Test-Path $to){throw 'Preserve cold lazy map test'}
$original=[IO.File]::ReadAllText($from)
$pairs=@(
    @('global-install-scheduling-v2','lazy-map-trusted-v1'),
    @('global-install-build-20261102-v1','lazy-map-build-20261102-v1'),
    @('FA61813EBDA435DFF4DC1138E450FE2C385AC3156534B507DB9216AA9878B21F','F4194E982B87835E9EC08F553136E558C0C0652C48961D97E1FA3A794547007C'),
    @('F5BDB1FF4CAD596781E91D4FDAEED421E8B22C199EF97852BD96481D778E3B7B','A17246FBF5F0EA68B931E7125ECD41189CC21ABA8D798396CB339C74F7728098'),
    @('Lea.Tests/SchedulingRegressionMain.cs','Lea.Tests/LazyMapTrustedMain.cs'),
    @('fixtures/LeaForge/test-pages/javascript-view.html','fixtures/LeaForge/test-pages/javascript-attributes.html'),
    @("@('Timers','Frames')","@('ClassLists','FirstUse')"),
    @('PASS Lea scheduling ','PASS Lea lazy-map trusted '),
    @('Original compiled trusted Timers.Suite and Frames.Run(trusted), unchanged assertions and budgets. Does not run timer scheduler/native/isolated branches or frame scheduler/protocol/native/isolated tests. No retry or release qualification.','Cold original trusted ClassLists.Suite followed by separate fresh-process captured-constructor/early-brand/revocation probe. No warmup or changed assertions/deadlines. Not isolated, timing, full-suite, host or release qualification.')
)
$text=$original
foreach($pair in $pairs){if($pair.Count -ne 2 -or [regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Unique lazy map test boundary required'};$text=$text.Replace($pair[0],$pair[1])}
$inverse=$text;for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
if($inverse -cne $original){throw 'Lazy map test inverse differs'}
if($Version -eq 2){$text=$text.Replace('continuity-evidence/lazy-map-trusted-v1','continuity-evidence/lazy-map-trusted-v2')}
foreach($critical in @('600MB+$stageBytes','foreach($file in $dlls)','Copy-Item -LiteralPath $file.FullName','-Seconds 120','-Seconds 30')){if([regex]::Matches($text,[regex]::Escape($critical)).Count -ne [regex]::Matches($original,[regex]::Escape($critical)).Count){throw 'Critical lazy map staging/ownership operation lost'}}
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Lazy map runner syntax invalid'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
