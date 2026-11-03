param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$outRoot=Join-Path $workspace 'continuity-evidence/interpreter-mapped-build-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained foreground owner required'}
if(Test-Path -LiteralPath $outRoot){throw 'Preserve build attempt'}
$utility=Join-Path $workspace 'continuity-evidence/native-launch-timing-v1/Test-CompositeCandidate.v2.ps1'
if((Get-FileHash -LiteralPath $utility).Hash -cne 'EB00AD17B8078A6CC2F418733BA5D675FC84EF49EED433A527FAC5498DD5ECEF'){throw 'Utility changed'}
$tokens=$null;$errors=$null;$ast=[Management.Automation.Language.Parser]::ParseFile($utility,[ref]$tokens,[ref]$errors)
foreach($name in @('Space','Pin','NewOutputReader','PumpOutput','Run')){
    $definitions=@($ast.FindAll({param($n)$n -is [Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -ceq $name},$false))
    if($errors.Count -or $definitions.Count -ne 1){throw 'Utility shape changed'}
    . ([scriptblock]::Create($definitions[0].Extent.Text))
}
if(Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue){throw 'Fresh owner required'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$runs=[Collections.Generic.List[object]]::new();$inputs=[Collections.Generic.List[object]]::new()
$result=[ordered]@{passed=$false;candidate=$outRoot;runs=$runs;inputs=$inputs;error=$null;runtimeExecuted=$false}
Space;$null=New-Item -ItemType Directory -Path $outRoot
try{
    $archive=Join-Path $workspace 'continuity-evidence/tested-interpreter-inputs-v1.zip';$null=Pin $archive 'E8F3D3CDF7F25589A8E7FDBFEEC15732C46310444147648C7F8DF0EF84F58E1C'
    $map=Join-Path $workspace 'continuity-evidence/interpreter-ordered-map-v1/build-map.json';$null=Pin $map '08C28F248AE2CC7BEC293E4EECEB455F0A530ADE1370B1298D59BA1EE1B9DC91'
    $child=Join-Path $PSScriptRoot 'Compile-MappedInterpreter.ps1';$null=Pin $child
    Copy-Item -LiteralPath $child -Destination (Join-Path $outRoot 'compile-source.ps1')
    $source=Join-Path $outRoot 'archive';[IO.Compression.ZipFile]::ExtractToDirectory($archive,$source)
    $emit=Join-Path $outRoot 'emit'
    $run=Run 'compile-archived-interpreter' (Get-Process -Id $PID).Path @('-NoProfile','-File',('"'+$child+'"'),'-Map',('"'+$map+'"'),'-ArchiveRoot',('"'+$source+'"'),'-OutputDirectory',('"'+$emit+'"')) 120
    $result.passed=$run.passed
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -gt 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.inputsUnchanged=@($inputs|Where-Object{(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.passed=$result.passed -and !$result.ownerAttention -and $result.inputsUnchanged
    $global:LeaMappedInterpreterResult=$result
    [IO.File]::WriteAllText((Join-Path $outRoot 'result.json'),($result|ConvertTo-Json -Depth 7))
}
[pscustomobject]$result|Select-Object passed,ownerAttention,error|ConvertTo-Json
