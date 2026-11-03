$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/archived-native-build-v1'
$out=Join-Path $root 'iterator-behavior.json'
if(Test-Path -LiteralPath $out){throw 'Preserve behavior evidence'}
$original=Join-Path $workspace 'continuity-evidence/request-diagnostics-v1/verified-build-20261020-150037/native/runtime/Lea.Engine.dll'
$candidate=Join-Path $root 'Lea.Engine/Lea.Engine.dll'
if((Get-FileHash -LiteralPath $original).Hash -cne '88690DBF2B1244AF7D3EBF402954A0E41E46CA1917CCE4A57851E29BB710BF9F' -or (Get-FileHash -LiteralPath $candidate).Hash -cne '6D89A8F5953F2E5A197C5D789D8D828BF8FC084836EB30F22EB00D228B0591F8'){throw 'Engine input changed'}
$helper=Join-Path $PSScriptRoot 'IteratorDisposalProbe.cs'
$pins=@($original,$candidate,$helper|ForEach-Object{Get-FileHash -LiteralPath $_})
Add-Type -Path $helper
$a=[IteratorDisposalProbe]::Run($original)
$b=[IteratorDisposalProbe]::Run($candidate)
$rows=@(for($i=0;$i -lt $a.Count;$i++){
    if($a[$i].method -cne $b[$i].method){throw 'Case order mismatch'}
    $same=($a[$i].items|ConvertTo-Json -Compress) -ceq ($b[$i].items|ConvertTo-Json -Compress)
    @{method=$a[$i].method;normalOutputIdentical=$same;original=$a[$i];candidate=$b[$i];postDisposeBehaviorIdentical=$a[$i].movedAfterDispose -eq $b[$i].movedAfterDispose}
})
foreach($pin in $pins){if((Get-FileHash -LiteralPath $pin.Path).Hash -cne $pin.Hash){throw 'Input changed during probe'}}
[IO.File]::WriteAllText($out,(@{cases=$rows;inputs=$pins;scope='Three private iterator fixtures on .NET host runtime; not full engine regression, framework runtime or release validation';releaseCertified=$false}|ConvertTo-Json -Depth 8))
$rows|Select-Object method,normalOutputIdentical,postDisposeBehaviorIdentical|ConvertTo-Json
