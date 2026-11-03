$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$base=Join-Path $workspace 'continuity-evidence/bootstrap-descriptor-batches-v1/Test-SourceCrossModuleProbes.ps1'
if((Get-FileHash -LiteralPath $base).Hash -cne '07558FC4ABF4F8A8E0136DACCA18104546382B644FA994CDEC54D53ECA3961B1'){throw 'Baseline probe builder changed'}
$target=Join-Path (Split-Path $base) 'Test-TemporalComponentProbes.generated.ps1'
if(Test-Path $target){throw 'Preserve component builder'}
$text=[IO.File]::ReadAllText($base).Replace("`r`n","`n")
$start=$text.IndexOf('    $stagePath=')
$end=$text.IndexOf('    $priorPath=',$start)
if($start -lt 0 -or $end -le $start){throw 'Stage block missing'}
$replacement=@'
    $nativeProof=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/native-pair-v2/literal-input-verification.json'
    $null=PinInput $nativeProof '98A773D0A4B40B2D116F8BAEA1B4CF1F20D5D9C6607C1FEB297EE4805C9428C8'
    $native=Get-Content -LiteralPath $nativeProof -Raw|ConvertFrom-Json
    if(!$native.passed){throw 'Native verification incomplete'}
    $hostProof=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/host-lazy-v1/result.json'
    $null=PinInput $hostProof 'A5B520E4A9D117854F49A105E2496DDF31796463851B373ACF87D6B571E86DE2'
    $hostRun=Get-Content -LiteralPath $hostProof -Raw|ConvertFrom-Json
    if(!$hostRun.completed -or !$hostRun.hostStagesPassed -or !$hostRun.originalInputsUnchanged -or !$hostRun.candidateBytesUnchanged -or $hostRun.ownerAttention){throw 'Candidate host behavior incomplete; raw decoder ACL failure retained'}
    $directory=Join-Path $hostRun.candidate 'ScriptWorker'
    $files=@(foreach($file in $hostRun.before|Where-Object {(Split-Path $_.path) -ceq $directory}){
        $null=PinInput $file.path $file.sha256
        @{name=[IO.Path]::GetFileName($file.path);sha256=$file.sha256}
    })
    if($files.Count -ne 6){throw 'Six-file worker binding required'}
    if(@($files|Where-Object { $_.name -ceq 'Lea.Script.Worker.dll' -and $_.sha256 -ceq '1C338F548D9F70B16F640A28BA7943F05DB0278260711674A62C5A2A8B36F436' }).Count -ne 1){throw 'Timestamp worker mismatch'}
    if(@($files|Where-Object { $_.name -ceq 'Jint.dll' -and $_.sha256 -ceq '67F3E7D75EDADCBC0110FD6BB7AC26E56784D3650A83C2AE0AA23DD67EB03E6D' }).Count -ne 1){throw 'Lazy interpreter mismatch'}
    $stage=@{variants=@(@{name='temporal-lazy';directory=$directory;files=$files})}
'@
$text=$text.Remove($start,$end-$start).Insert($start,$replacement+"`n")
$text=$text.Replace("`$outRoot=Join-Path `$PSScriptRoot 'source-cross-module-probes-v1'","`$outRoot=Join-Path `$workspace 'continuity-evidence/temporal-startup-v1/component-probes-v1'")
$text=$text.Replace("`$newHash='60FF26D8357FEA52C802BAC562C30C6CE28F72F735E78281D0908E94FAB530D6'","`$newHash='1C338F548D9F70B16F640A28BA7943F05DB0278260711674A62C5A2A8B36F436'")
$tokens=$null;$errors=$null;[void][Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Generated syntax invalid'}
if(!$text.Contains("`$newHash='1C338") -or !$text.Contains('component-probes-v1')){throw 'Required rebinding missing'}
[IO.File]::WriteAllText($target,$text,[Text.UTF8Encoding]::new($false))
[pscustomobject]@{sourceSha256=(Get-FileHash -LiteralPath $base).Hash;generatedSha256=(Get-FileHash -LiteralPath $target).Hash;workerRebound=$true;interpreterRebound=$true;probeAssertionsChanged=$false;executed=$false}|ConvertTo-Json
