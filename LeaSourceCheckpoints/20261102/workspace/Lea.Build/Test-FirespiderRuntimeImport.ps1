$ErrorActionPreference='Stop'
$root=Join-Path ([IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))) 'continuity-evidence/runtime-import-v1'
if(Test-Path -LiteralPath $root){throw 'Preserve previous test output'}
$null=New-Item -ItemType Directory -Path $root
$msbuild=Join-Path $env:WINDIR 'Microsoft.NET/Framework/v4.0.30319/MSBuild.exe'
$source=Join-Path $PSScriptRoot '../Lea.Firespider/FirespiderLea.targets'
$base=@('Lea.Engine.dll','Lea.Win.dll','Lea.Image.exe')
$scriptFiles=@('Lea.Script.Win.dll','Lea.Script.dll','Jint.dll','Acornima.dll','System.Memory.dll','System.Buffers.dll','System.Numerics.Vectors.dll','System.Runtime.CompilerServices.Unsafe.dll','ScriptWorker/Jint.dll','ScriptWorker/Acornima.dll')
$modern=@('ScriptWorker/Lea.Script.Worker.dll','ScriptWorker/Lea.Script.Worker.deps.json','ScriptWorker/Lea.Script.Worker.runtimeconfig.json')
$cases=@(
    @{name='base';enabled=$false;files=$base;ok=$true},
    @{name='missing-image';enabled=$false;files=@('Lea.Engine.dll','Lea.Win.dll');ok=$false},
    @{name='missing-worker';enabled=$true;files=$base+$scriptFiles;ok=$false},
    @{name='framework-worker';enabled=$true;files=$base+$scriptFiles+@('ScriptWorker/Lea.Script.Worker.exe');ok=$true},
    @{name='modern-worker';enabled=$true;files=$base+$scriptFiles+$modern;ok=$true},
    @{name='missing-runtimeconfig';enabled=$true;files=$base+$scriptFiles+$modern[0..1];ok=$false},
    @{name='missing-parser';enabled=$true;files=@($base+$scriptFiles+$modern|Where-Object{$_ -cne 'ScriptWorker/Acornima.dll'});ok=$false}
)
$results=@(foreach($case in $cases){
    $directory=Join-Path $root $case.name
    $null=New-Item -ItemType Directory -Path $directory
    Copy-Item -LiteralPath $source -Destination (Join-Path $directory 'Lea.targets')
    foreach($file in $case.files){
        $path=Join-Path $directory $file
        $null=New-Item -ItemType Directory -Path ([IO.Path]::GetDirectoryName($path)) -Force
        [IO.File]::WriteAllBytes($path,[byte[]]@(0))
    }
    $project='<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003"><PropertyGroup><LeaScriptEnabled>'+([string]$case.enabled).ToLowerInvariant()+'</LeaScriptEnabled><TargetFrameworkVersion>v4.8</TargetFrameworkVersion></PropertyGroup><Import Project="Lea.targets" /></Project>'
    $projectPath=Join-Path $directory 'Test.proj'
    [IO.File]::WriteAllText($projectPath,$project)
    $output=@(& $msbuild $projectPath /nologo /v:minimal /t:CheckLeaRuntime 2>&1)
    $exit=$LASTEXITCODE
    [IO.File]::WriteAllLines((Join-Path $directory 'output.txt'),[string[]]$output)
    if(($exit -eq 0) -ne $case.ok){throw "Unexpected MSBuild result: $($case.name)"}
    if(!$case.ok -and ($output -join "`n") -notmatch 'Incomplete Lea runtime:'){throw 'Wrong rejection reason'}
    @{name=$case.name;exitCode=$exit;expectedSuccess=$case.ok;passed=$true}
})
[IO.File]::WriteAllText((Join-Path $root 'result.json'),(@{checks=$results;passed=$true;scope='Real MSBuild import validation with placeholder files; not compilation or runtime compatibility'}|ConvertTo-Json -Depth 5))
$results|ForEach-Object{"PASS runtime import: $($_.name)"}
