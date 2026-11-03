$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/temporal-startup-v1'
$out=Join-Path $root 'suite-phase-source-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve source preparation'}
if((Get-PSDrive C).Free -lt 512MB){throw 'Working reserve required'}
$buildPath=Join-Path $workspace 'continuity-evidence/bootstrap-descriptor-batches-v1/source-cross-module-suite-v2/result.json'
if((Get-FileHash -LiteralPath $buildPath).Hash -cne '779061CB3422B80038653503E7AA375521E5A868DBA30FA88A0509CC206069F5'){throw 'Frozen build receipt changed'}
$source=Join-Path $workspace 'continuity-evidence/bootstrap-descriptor-batches-v1/source-cross-module-suite-v2/source/Lea.Script.Tests/Program.cs'
if((Get-FileHash -LiteralPath $source).Hash -cne 'DBDA0E6EAD53271FC636DEE6336E68A24490EC9B3A55CF79BE03460AF1E9F26F'){throw 'Frozen Program differs from build input'}
$original=[IO.File]::ReadAllText($source)
$compiler=@{'Microsoft.CodeAnalysis.dll'='1AB0A49C07AF467D16EC723C5D95DAE3F243AD2ECEA840312228ECDEA8D6E021';'Microsoft.CodeAnalysis.CSharp.dll'='CE8DFED4EB61F1B6FECB6D06E3AADFC21B36C478FD863ED014BAD2CB01987DFD'}
foreach($name in $compiler.Keys){if((Get-FileHash -LiteralPath (Join-Path $PSHOME $name)).Hash -cne $compiler[$name]){throw 'Roslyn changed'};Add-Type -Path (Join-Path $PSHOME $name)}
$helper=Join-Path $root 'SuitePhaseInstrumentation.cs'
$refs=@(Get-ChildItem (Join-Path $PSHOME 'ref') -Filter '*.dll'|ForEach-Object FullName)+@((Join-Path $PSHOME 'Microsoft.CodeAnalysis.dll'),(Join-Path $PSHOME 'Microsoft.CodeAnalysis.CSharp.dll'))
Add-Type -Path $helper -ReferencedAssemblies $refs -CompilerOptions '/nowarn:1701'
$prepared=[SuitePhaseInstrumentation]::Prepare($original)
if(!$prepared.InverseVerified -or $prepared.Stages.Count -lt 60){throw 'Instrumentation incomplete'}
$null=New-Item -ItemType Directory $out
[IO.File]::WriteAllText((Join-Path $out 'Program.cs'),$prepared.Source,[Text.UTF8Encoding]::new($false))
$receipt=@{source=$source;sourceSha256=(Get-FileHash -LiteralPath $source).Hash;helperSha256=(Get-FileHash -LiteralPath $helper).Hash;candidateSha256=(Get-FileHash -LiteralPath (Join-Path $out 'Program.cs')).Hash;stages=$prepared.Stages;edits=$prepared.Edits;inverseVerified=$prepared.InverseVerified;compiler=$compiler;compiled=$false;executed=$false;limitsChanged=$false;scope='Insertion-only frozen main-suite phase timing; not a rebuilt or rebound full-suite candidate'}
[IO.File]::WriteAllText((Join-Path $out 'result.json'),($receipt|ConvertTo-Json -Depth 6),[Text.UTF8Encoding]::new($false))
[pscustomobject]@{stageCount=$prepared.Stages.Count;insertions=$prepared.Edits.Count;inverseVerified=$prepared.InverseVerified;candidateSha256=$receipt.candidateSha256;receipt=(Get-FileHash -LiteralPath (Join-Path $out 'result.json')).Hash}|ConvertTo-Json
