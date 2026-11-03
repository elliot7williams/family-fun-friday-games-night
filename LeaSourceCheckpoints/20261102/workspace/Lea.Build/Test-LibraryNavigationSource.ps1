$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace 'continuity-evidence/library-navigation-source-controls-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve library source controls'}
. (Join-Path $PSScriptRoot 'New-LibraryNavigationSource.ps1')
. (Join-Path $PSScriptRoot 'New-AboutHarnessSource.ps1')
. (Join-Path $PSScriptRoot 'New-LibraryHarnessSource.ps1')
$rows=[Collections.Generic.List[object]]::new();$inputs=[Collections.Generic.List[string]]::new()
foreach($kind in @('History','Bookmarks')){
    $file=if($kind -eq 'History'){'6.vb'}else{'7.vb'}
    $path=Join-Path $workspace ('continuity-evidence/about-navigation-v2/host-source/'+$file);$inputs.Add($path)
    $original=[IO.File]::ReadAllText($path)
    $candidate=New-LibraryNavigationSource $original $kind
    if(!$candidate.inverseVerified){throw 'Library source inverse failed'}
    $rows.Add(@{name=($kind+'-inverse');passed=$true})
    foreach($case in @(@{name='missing';text=$original.Replace('WebBrowser','Object')},@{name='duplicate';text=($original+"`n"+$original)},@{name='repeated';text=$candidate.text})){
        $rejected=$false
        try{$null=New-LibraryNavigationSource $case.text $kind}catch{$rejected=$_.Exception.Message -ceq 'Library navigation anchor differs'}
        if(!$rejected){throw ('Library source control failed: '+$kind+'-'+$case.name)}
        $rows.Add(@{name=($kind+'-'+$case.name);passed=$true})
    }
}
$boot=Join-Path $workspace 'continuity-evidence/split-host-diagnostic-v2/Boot.cs';$inputs.Add($boot)
$harness=New-LibraryHarnessSource ([IO.File]::ReadAllText($boot))
if(!$harness.inverseVerified){throw 'Library harness inverse failed'}
$rows.Add(@{name='harness-inverse';passed=$true})
foreach($name in @('New-LibraryNavigationSource.ps1','New-LibraryHarnessSource.ps1','New-AboutHarnessSource.ps1','Test-LibraryNavigationSource.ps1')){$inputs.Add((Join-Path $PSScriptRoot $name))}
$null=New-Item -ItemType Directory -Path $out
[IO.File]::WriteAllText((Join-Path $out 'result.json'),(@{passed=$true;controls=$rows;runtimeExecuted=$false;inputs=@($inputs|ForEach-Object{Get-FileHash -LiteralPath $_}|Select-Object Path,Hash)}|ConvertTo-Json -Depth 6))
@{passed=$true;controls=$rows.Count;runtimeExecuted=$false}|ConvertTo-Json
