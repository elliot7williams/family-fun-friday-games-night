$ErrorActionPreference='Stop'
$from=Join-Path $PSScriptRoot 'Test-IsConnectedTrusted.ps1'
if((Get-FileHash $from).Hash -cne 'F48FB93BD317970D30CB3861AFE183E60A26A8F6D87893B6EFB9BFC513C449FF'){throw 'Isolated test template differs'}
$to=Join-Path $PSScriptRoot 'Test-IsConnectedNative-v1.generated.ps1'
if(Test-Path $to){throw 'Preserve native connectivity runner'}
$text=[IO.File]::ReadAllText($from).Replace('continuity-evidence/is-connected-isolated-v1','continuity-evidence/is-connected-native-v1').Replace("'Lea.Tests/IsConnectedMain.cs'","'Lea.Tests/IsConnectedNativeMain.cs'").Replace('continuity-evidence/deterministic-full-replay-v1/fixtures/LeaForge/test-pages/javascript-contains.html','LeaForge/test-pages/is-connected-native.html')
$needle='    $fixture=Join-Path'
if([regex]::Matches($text,[regex]::Escape($needle)).Count -ne 1){throw 'Unique native dependency boundary required'}
$dependencies=@'
    if(!$Isolated){throw 'Native view uses isolated scripting only'}
    $sdkPath='I:\LeaBuildStorage\sdk-reproducibility-20261101-v1/result.json';Pin $sdkPath '39955D0AC2D23B60360000414FDB3702F928B4CA1537C074D2C6007BD174B199'
    $sdk=Get-Content $sdkPath -Raw|ConvertFrom-Json
    foreach($name in @('Lea.Win.dll','Lea.Script.Win.dll')){
        $product=@($sdk.products|Where-Object path -CEQ ('bin/Release/'+$name))
        if($product.Count -ne 1 -or !$product[0].equal){throw 'Exact native-view dependency required'}
        Pin $product[0].first $product[0].sha256;Copy-Item -LiteralPath $product[0].first -Destination $runtime
    }
'@
$text=$text.Replace($needle,$dependencies+[Environment]::NewLine+$needle)
$text=$text.Replace("@('Lea.Engine.dll','Lea.Script.dll')","@('Lea.Engine.dll','Lea.Script.dll','Lea.Win.dll','Lea.Script.Win.dll')")
$text=$text.Replace('$mode=if($Isolated){''isolated''}else{''trusted''}', '$mode=''native''')
$text=$text.Replace("@(('`"'+`$fixture+'`"'),('`"'+`$cases+'`"'),`$mode)","@(('`"'+`$fixture+'`"'),('`"'+`$out+'`"'))")
$tokens=$null;$errors=$null;$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count -or !$text.Contains("`$mode='native'")){throw 'Native runner syntax/mode invalid'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
