$ErrorActionPreference='Stop'
$base=Join-Path $PSScriptRoot 'Test-RebuiltCore.ps1'
if((Get-FileHash -LiteralPath $base).Hash -cne 'E9566BC86AE65DCE3C47108A1C567ADD2A1BD2AAD914C557ACAAAE0615B5FB13'){throw 'Runner changed'}
$target=Join-Path $PSScriptRoot 'Test-AsyncCancellationSuiteV2.generated.ps1'
if(Test-Path -LiteralPath $target){throw 'Preserve runner'}
$text=[IO.File]::ReadAllText($base).Replace('rebuilt-core-regression-v2','rebuilt-core-regression-v4')
$anchor="    `$fixtures=Join-Path `$outRoot 'fixtures'"
$addition=@'
    $oldSource=Join-Path $native 'source/Lea.Tests/Smoke.cs'
    $sourcePin=@($build.sourceInputs|Where-Object path -ceq 'Lea.Tests\Smoke.cs')
    if($sourcePin.Count -ne 1){throw 'Smoke source proof missing'}
    $null=Pin $oldSource $sourcePin[0].sha256
    $original=[IO.File]::ReadAllText($oldSource);$updated=$original
    $edits=@(
        @('var waitCount = typeof(SemaphoreSlim).GetField("m_waitCount",','var asyncHead = typeof(SemaphoreSlim).GetField("m_asyncHead",'),
        @('Ok(waitCount != null, "Framework semaphore waiter accounting','Ok(asyncHead != null, "Framework asynchronous semaphore waiter accounting'),
        @('(int)waitCount.GetValue(gate) > 0, "image decoder is blocked on the held gate"','asyncHead.GetValue(gate) != null, "image decoder is asynchronously queued on the held gate"')
    )
    foreach($pair in $edits){if([regex]::Matches($updated,[regex]::Escape($pair[0])).Count -ne 1){throw 'Ambiguous test overlay'};$updated=$updated.Replace($pair[0],$pair[1])}
    $inverse=$updated
    for($i=$edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($edits[$i][1],$edits[$i][0])}
    if($inverse -cne $original){throw 'Test overlay inverse differs'}
    $overlay=Join-Path $outRoot 'Smoke.cs';[IO.File]::WriteAllText($overlay,$updated,[Text.UTF8Encoding]::new($false));$null=Pin $overlay
    $options=[Collections.Generic.List[string]]::new()
    foreach($line in Get-Content -LiteralPath (Join-Path $native 'Lea.Tests.rsp')){
        if($line.StartsWith('/out:')){$line='/out:"'+(Join-Path $result.candidate 'Lea.Tests.exe')+'"'}
        elseif($line.StartsWith('"')){
            $path=$line.Trim('"');$relative=[IO.Path]::GetRelativePath((Join-Path $native 'source'),$path)
            $pin=@($build.sourceInputs|Where-Object path -ceq $relative)
            if($pin.Count -ne 1){throw 'Missing compiler source pin'};$null=Pin $path $pin[0].sha256
            if($path -ceq $oldSource){$line='"'+$overlay+'"'}
        }elseif($line.StartsWith('/reference:')){
            $path=$line.Substring(11).Trim('"');$name=[IO.Path]::GetFileName($path)
            if($name -cin @('Lea.Engine.dll','Lea.Win.dll')){$path=Join-Path $result.candidate $name}
            if(!$path.StartsWith($workspace+'\',[StringComparison]::OrdinalIgnoreCase)){
                $observed=(Get-FileHash -LiteralPath $path).Hash
                $local=Join-Path $outRoot ('references/'+$name)
                $null=New-Item -ItemType Directory -Path ([IO.Path]::GetDirectoryName($local)) -Force
                Copy-Item -LiteralPath $path -Destination $local
                $null=Pin $local $observed;$path=$local
            }else{$null=Pin $path}
            $line='/reference:"'+$path+'"'
        }
        $options.Add($line)
    }
    $rsp=Join-Path $outRoot 'tests.rsp';[IO.File]::WriteAllLines($rsp,$options);$hash=Pin $rsp
    $wrapper=Join-Path $PSScriptRoot 'Invoke-PublicCompiler.20261022.ps1';$null=Pin $wrapper '1A04EE9A191D522955C633F97A24C9C0921A5738F76D0DF36DB1C92A574EEA03'
    $compile=Run 'compile-tests' (Get-Process -Id $PID).Path @('-NoProfile','-File',('"'+$wrapper+'"'),'-ResponseFile',('"'+$rsp+'"'),'-ExpectedResponseSha256',$hash,'-ExpectedSourceSha256','D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB') 120
    if(!$compile.passed){throw 'Matched tests failed compilation'}
    $result.testOverlayInverseVerified=$true
'@
if([regex]::Matches($text,[regex]::Escape($anchor)).Count -ne 1){throw 'Fixture anchor changed'}
$text=$text.Replace($anchor,$addition+"`r`n"+$anchor)
$text=$text.Replace('$changes=@($result.before|Where-Object{(Get-Acl -LiteralPath $_.path).Sddl -cne $_.sddl})','$changes=@(); if($result.before){$changes=@($result.before|Where-Object{(Get-Acl -LiteralPath $_.path).Sddl -cne $_.sddl})}')
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Invalid runner syntax'}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
Get-FileHash -LiteralPath $target
