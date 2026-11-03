param([ValidateRange(1,9)][int]$Version=1)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root ('continuity-evidence/temporal-startup-v1/font-framework-interpreter-map-v'+$Version)
if(Test-Path $out){throw 'Preserve interpreter map'}
$holds=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new()
function Hold([string]$Path,[string]$Expected){
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$holds.Add($stream)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))
    if($Expected -and $hash -cne $Expected){throw 'Interpreter map input differs'};$inputs.Add(@{path=$Path;sha256=$hash})
}
try{
    $runtime=Join-Path $root 'continuity-evidence/temporal-startup-v1/font-script-suite-v1/runtime'
    $dll=Join-Path $runtime 'Jint.dll';$jintHash='B54618A4E3FFE33344D478D3A1580658C4D91819B3B97A24326CB3626CB7FD1A';Hold $dll $jintHash
    $parent=Join-Path $runtime 'Lea.Script.dll';Hold $parent 'E68504B515405FA2C75B92904277A7A8074945BDCC02E1CAD10A57EB9A6FC1E5'
    $package='C:\Users\Elliot\.nuget\packages\jint\4.16.4\jint.4.16.4.nupkg';Hold $package 'E920276A0A398ECD58CB2F2C9F0B263354D29EB7246AD548A2E8D5872BF5B4D2'
    Add-Type -AssemblyName System.IO.Compression
    $zip=[IO.Compression.ZipFile]::OpenRead($package)
    try{
        $entry=$zip.GetEntry('lib/net462/Jint.dll');if(!$entry){throw 'Framework package entry absent'}
        $stream=$entry.Open();try{$entryHash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))}finally{$stream.Dispose()}
        if($entryHash -cne $jintHash){throw 'Package entry does not match runtime'}
        $entry=$zip.GetEntry('Jint.nuspec');if(!$entry){throw 'Package nuspec absent'}
        $stream=$entry.Open();$xml=[xml]::new();try{$xml.Load($stream)}finally{$stream.Dispose()}
        $ns=[Xml.XmlNamespaceManager]::new($xml.NameTable);$ns.AddNamespace('n',$xml.DocumentElement.NamespaceURI)
        $repository=$xml.SelectSingleNode('/n:package/n:metadata/n:repository',$ns)
        if(!$repository -or $repository.commit -cne '884bd0c4ebf7806f70041eae0370ee11a9c138bc'){throw 'Repository commit differs'}
    }finally{$zip.Dispose()}
    $sourceRoot=Join-Path $root 'continuity-evidence/modern-worker-v1/interpreter-source-v1'
    $proof=Join-Path $sourceRoot 'result.json';Hold $proof 'D013E5CCEDED3C5CC52C94EBE87E3E9CF2A4629CD4DCE750A6B234BCF06F3E28'
    $source=Get-Content $proof -Raw|ConvertFrom-Json
    if(!$source.passed -or $source.commit -cne $repository.commit){throw 'Retained source commit mismatch'}
    $null=New-Item -ItemType Directory -Path $out
    foreach($relative in @('Jint/Runtime/CallStack/StackGuard.cs','Jint/Constraints/TimeConstraint.cs','Jint/Constraints/MemoryLimitConstraint.cs','Jint/Options.cs','Jint/Options.Extensions.cs','Jint/Engine.cs','Jint/Extensions/Polyfills.cs')){
        $pins=@($source.files|Where-Object relative -ceq $relative)
        if($pins.Count -ne 1){throw ('Commit source pin missing: '+$relative)}
        $from=Join-Path (Join-Path $sourceRoot 'source') $relative;Hold $from $pins[0].sha256
        $to=Join-Path $out ('source/'+$relative);$null=New-Item -ItemType Directory -Path (Split-Path $to) -Force;Copy-Item $from $to;Hold $to $pins[0].sha256
    }
    foreach($file in @(@('8.cs','21792E7953FFEEDA20E6F151ACBC85B873B9BF4C0E5FEA9E01DCB46BF0BC2C9E'),@('10.cs','2DDAE2203C58AABEA824C1AC95C9F8A3630E188837A7659C4976D2C26643E8DF'))){
        $from=Join-Path $root ('continuity-evidence/permission-contract-v1/early-limits-v1/source/'+$file[0]);Hold $from $file[1]
        $to=Join-Path $out ('parent-'+$file[0]);Copy-Item $from $to;Hold $to $file[1]
    }
    $reader=Join-Path $PSScriptRoot 'ReadFailureIL.cs';Hold $reader 'A27FC129FABD195808AA2564EB78CF275E8E0AF325D4D01EE6C8B95DC009F6B0'
    $map=Join-Path $PSScriptRoot 'FrameworkInterpreterIlMap.cs';Hold $map;Hold $PSCommandPath
    Add-Type -Path @($reader,$map)
    $interpreter=[FrameworkInterpreterIlMap]::Read($dll);$parentMap=[FrameworkInterpreterIlMap]::Read($parent)
    if($interpreter.version -cne '4.16.4.0' -or $interpreter.methodCount -lt 31){throw 'Interpreter method map incomplete'}
    function Method($Inventory,[string]$Type,[string]$Name){
        $types=@($Inventory.types|Where-Object name -ceq $Type);if($types.Count -ne 1){throw ('Type not unique: '+$Type)}
        $methods=@($types[0].methods|Where-Object name -ceq $Name);if($methods.Count -ne 1){throw ('Method not unique: '+$Name)}
        return $methods[0]
    }
    $time=Method $interpreter 'Jint.Constraints.TimeConstraint' 'Check'
    if(!@($time.instructions|Where-Object {$_ -match ' call GetTimestamp$'}).Count -or @($time.instructions|Where-Object {$_ -match 'CancellationTokenSource|IsCancellationRequested'}).Count){throw 'Timeout implementation unexpected'}
    $memory=Method $interpreter 'Jint.Constraints.MemoryLimitConstraint' 'Check'
    if(!@($memory.instructions|Where-Object {$_ -match 'call GetAllocatedBytesForCurrentThread$'}).Count -or @($memory.instructions|Where-Object {$_ -match 'GetTotalMemory'}).Count){throw 'Memory accounting implementation unexpected'}
    $stack=Method $interpreter 'Jint.Runtime.CallStack.StackGuard' 'ProbeStackHeadroom'
    if(!@($stack.instructions|Where-Object {$_ -match 'call TryEnsureSufficientExecutionStack$'}).Count){throw 'Stack guard call absent'}
    $wait=Method $interpreter 'Jint.Runtime.CallStack.StackGuard' 'RunOnEmptyStackCore'
    if(!@($wait.instructions|Where-Object {$_ -match 'callvirt WaitOne$'}).Count){throw 'Stack handoff wait absent'}
    $start=Method $parentMap 'Lea.Script.Js' 'Start'
    if(!@($start.instructions|Where-Object {$_ -match 'callvirt Evaluate$'}).Count){throw 'Parent evaluation call missing'}
    $defaults=Method $interpreter 'Jint.Options+ConstraintOptions' '.ctor'
    if(($defaults.instructions -join "`n") -notmatch 'ldc.i4.m1[^\n]*\n[^\n]*stfld <MaxExecutionStackCount>k__BackingField'){throw 'Default stack handoff setting differs'}
    $settings=Method $parentMap 'Lea.Script.Vo' 'At'
    if(!@($settings.instructions|Where-Object {$_ -match 'callvirt set_StackOverflowGuard$'}).Count -or @($settings.instructions|Where-Object {$_ -match 'set_MaxExecutionStackCount'}).Count){throw 'Parent stack policy differs'}
    $recursion=Method $interpreter 'Jint.OptionsExtensions' 'LimitRecursion'
    if(!@($recursion.instructions|Where-Object {$_ -match 'set_MaxRecursionDepth$'}).Count -or @($recursion.instructions|Where-Object {$_ -match 'set_MaxExecutionStackCount'}).Count){throw 'Recursion option unexpectedly changes handoff setting'}
    foreach($file in $inputs){if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Map input changed'}}
    Copy-Item $PSCommandPath (Join-Path $out 'runner.ps1')
    $result=@{mapped=$true;packageEntrySha256=$entryHash;repositoryCommit=$repository.commit;sourceArchiveSha256=$source.archiveSha256;inputs=$inputs;interpreter=$interpreter;parent=$parentMap;selectedIlChecks=8;runtimeExecuted=$false;fullSuitePassed=$false;causeProven=$false;scope='Package-byte identity/repository-source pins plus selected compiled IL; NOT full reproducible source-to-binary proof'}
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 12))
    if(!(Get-Content (Join-Path $out 'result.json') -Raw|ConvertFrom-Json).mapped){throw 'Map receipt readback differs'}
    $result|Select-Object mapped,packageEntrySha256,repositoryCommit,selectedIlChecks,runtimeExecuted,causeProven|ConvertTo-Json
}finally{foreach($hold in $holds){$hold.Dispose()}}
