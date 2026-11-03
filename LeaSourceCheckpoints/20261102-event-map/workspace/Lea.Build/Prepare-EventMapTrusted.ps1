$ErrorActionPreference='Stop'
$from=Join-Path $PSScriptRoot 'Test-LazyMapTrusted-v2.generated.ps1'
$to=Join-Path $PSScriptRoot 'Test-EventMapTrusted-v1.generated.ps1'
if(Test-Path $to){throw 'Preserve event map trusted campaign'}
$original=[IO.File]::ReadAllText($from)
$pairs=@(
    @('lazy-map-trusted-v2','event-map-trusted-v1'),
    @('lazy-map-build-20261102-v1','event-map-build-20261102-v1'),
    @('F4194E982B87835E9EC08F553136E558C0C0652C48961D97E1FA3A794547007C','4EB46F1448E476A466B8DE01A179DF4EFDBA7DF8738DC5E00EE67878935F4202'),
    @('A17246FBF5F0EA68B931E7125ECD41189CC21ABA8D798396CB339C74F7728098','B245644C86BC706F1F54CB896B138419A7D9CECD9150F525E0F7790BF5C01A2E'),
    @('Lea.Tests/LazyMapTrustedMain.cs','Lea.Tests/EventMapTrustedMain.cs'),
    @("@('ClassLists','FirstUse')","@('FirstUse','ClassLists')"),
    @('PASS Lea lazy-map trusted ','PASS Lea event-map trusted '),
    @('Cold original trusted ClassLists.Suite followed by separate fresh-process captured-constructor/early-brand/revocation probe. No warmup or changed assertions/deadlines. Not isolated, timing, full-suite, host or release qualification.','Cold captured-constructor/event-target/receiver probe followed by separate fresh-process original trusted ClassLists.Suite. No warmup or changed assertions/deadlines. Not isolated, timing, full-suite, host or release qualification.')
)
$text=$original
foreach($pair in $pairs){if($pair.Count -ne 2 -or [regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw 'Unique event map trusted boundary required'};$text=$text.Replace($pair[0],$pair[1])}
$inverse=$text;for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
if($inverse -cne $original){throw 'Event map trusted inverse differs'}
foreach($critical in @('600MB+$stageBytes','foreach($file in $dlls)','Copy-Item -LiteralPath $file.FullName','-Seconds 120','-Seconds 30')){if([regex]::Matches($text,[regex]::Escape($critical)).Count -ne [regex]::Matches($original,[regex]::Escape($critical)).Count){throw 'Trusted staging/ownership operation changed'}}
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Event map runner syntax invalid'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
