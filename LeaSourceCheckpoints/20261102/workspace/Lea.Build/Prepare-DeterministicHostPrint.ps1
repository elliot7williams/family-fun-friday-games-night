param([ValidateRange(1,9999)][int]$Version=2)
$ErrorActionPreference='Stop'
$template=Join-Path $PSScriptRoot 'Test-DeterministicHostSmokes-v2.generated.ps1'
if((Get-FileHash $template).Hash -cne 'D7E21BDABCBD872FD98D8983BA14B630783F207B7EFB7161C67AD1413E147825'){throw 'Print staging template differs'}
$to=Join-Path $PSScriptRoot "Test-DeterministicHostPrint-v$Version.generated.ps1"
if(Test-Path $to){throw 'Preserve generated print test'}
$text=[IO.File]::ReadAllText($template).Replace('continuity-evidence/deterministic-host-smokes-v2',"continuity-evidence/deterministic-host-print-v$Version")
$start=$text.IndexOf('    $result.before=')
$end=$text.IndexOf('    $result.completed=', $start)
if($start -lt 0 -or $end -le $start){throw 'Print test replacement boundaries differ'}
$body=@'
    $proofPath=Join-Path $workspace 'continuity-evidence/relative-font-host-v1/result.json'
    Hold $proofPath '452492446FCAE2FD9F2DFCC5A6185ED4F05B977B89FADD584A2B6DC83150A6F6'
    $proof=Get-Content $proofPath -Raw|ConvertFrom-Json
    $probe=@($proof.before|Where-Object {$_.path.EndsWith('\Lea.Firespider.Boot.exe',[StringComparison]::OrdinalIgnoreCase)})
    if($probe.Count -ne 1 -or $probe[0].sha256 -cne '84985AC08FD5A7DC018CF6AD75006633734298153D327D53834C6B37DF30839B'){throw 'Print test binary provenance differs'}
    Hold $probe[0].path $probe[0].sha256
    $boot=Join-Path $runtime 'Lea.Firespider.PrintProbe.exe'
    Copy-Item -LiteralPath $probe[0].path -Destination $boot
    Hold $boot $probe[0].sha256
    foreach($name in @('print-styled-document.html','print-preview-image.html')){
        $fixture=@($proof.before|Where-Object {$_.path.EndsWith(('\test-pages\'+$name),[StringComparison]::OrdinalIgnoreCase)})
        if($fixture.Count -ne 1){throw 'Print fixture provenance incomplete'}
        Hold $fixture[0].path $fixture[0].sha256
        $fixtureTo=Join-Path $runtime ('test-pages/'+$name)
        if(Test-Path $fixtureTo){throw 'Do not overwrite a runtime fixture'}
        Copy-Item -LiteralPath $fixture[0].path -Destination $fixtureTo
        Hold $fixtureTo $fixture[0].sha256
    }
    $result.before=@(Get-ChildItem $runtime -Recurse -File|ForEach-Object{@{path=$_.FullName;sha256=(Get-FileHash $_.FullName).Hash;sddl=(Get-Acl $_.FullName).Sddl}})
    if($result.before.Count -ne 403){throw 'Print runtime inventory differs'}
    $run=Run 'print-command-preview' $boot @(('"'+$runtime+'"'),'print-command') 90
    if(!$run.passed -or @(Get-Content $run.stdout|Where-Object {$_.StartsWith('PASS Lea browser print command:')}).Count -ne 1){throw 'Current host print command/preview failed'}
'@
$text=$text.Substring(0,$start)+$body+[Environment]::NewLine+$text.Substring($end)
$text=$text.Replace('$runs.Count -eq 3','$runs.Count -eq 1').Replace('$result.before.Count -eq 400','$result.before.Count -eq 403')
$text=$text.Replace('Exact deterministic host settings/navigation/actual clean EXE smokes on fresh NTFS; not full host campaign, clean-PC or release qualification','Exact current host print-command/preview with pinned preserved test harness; native dialog cancellation only, no queue submission; not physical output, clean-PC, fidelity or release proof')
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Print test syntax invalid'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
