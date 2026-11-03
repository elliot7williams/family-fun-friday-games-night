$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$modern=Join-Path $workspace 'continuity-evidence/modern-worker-v1'
$root=Join-Path $workspace 'continuity-evidence/temporal-startup-v1'
$out=Join-Path $root 'build-v1'
if(Test-Path $out){throw 'Preserve experimental build'}
if([IO.DriveInfo]::new('C:\').AvailableFreeSpace -lt 768MB){throw 'Full interpreter build reserve required'}
$inputs=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new()
function Pin($path,$hash){
    $file=[IO.File]::Open($path,'Open','Read','Read');$leases.Add($file)
    if((Get-FileHash $path).Hash -cne $hash){throw "Input differs: $path"}
    $inputs.Add(@{path=$path;sha256=$hash})
}
$null=New-Item -ItemType Directory -Path $out
$runs=[Collections.Generic.List[object]]::new()
$result=[ordered]@{passed=$false;inputs=$inputs;runs=$runs;workerExecuted=$false;interpreterExecuted=$false;productionChanged=$false;error=$null}
try{
    $baselinePath=Join-Path $modern 'interpreter-baseline-v1/result.json'
    Pin $baselinePath 'B700E343AE9240B61323CC58BA4325658AA230B8C9C0EDFFF4EBC8755FDF5942'
    $baseline=Get-Content $baselinePath -Raw|ConvertFrom-Json
    if(!$baseline.passed -or !$baseline.inputsUnchanged){throw 'Prior source baseline incomplete'}
    foreach($item in $baseline.inputs){Pin $item.path $item.sha256}
    $snapshot=Get-Content (Join-Path $modern 'interpreter-source-v1/result.json') -Raw|ConvertFrom-Json
    $generation=Get-Content (Join-Path $modern 'interpreter-generation-check-v5/result.json') -Raw|ConvertFrom-Json
    $sources=[Collections.Generic.List[string]]::new()
    $target=Join-Path $modern 'interpreter-source-v1/source/Jint/Native/Temporal/DefaultTimeZoneProvider.cs'
    foreach($file in $snapshot.files|Where-Object {$_.relative.StartsWith('Jint/') -and $_.relative.EndsWith('.cs')}|Sort-Object relative){
        $original=Join-Path $modern ('interpreter-source-v1/source/'+$file.relative)
        $doc=Join-Path $modern ('interpreter-generation-check-v5/doc-overlay/'+$file.relative.Substring(5))
        $sources.Add($(if(Test-Path $doc){$doc}else{$original}))
    }
    if($sources.Count -ne 586 -or @($sources|Where-Object {$_ -ceq $target}).Count -ne 1){throw 'Runtime source inventory differs'}
    $sources.Add((Join-Path $modern 'InterpreterRuntimeUsings.cs'))
    foreach($file in $generation.generated|Sort-Object name){$sources.Add((Join-Path $modern ('interpreter-generation-check-v5/generated/'+$file.name)))}
    if($sources.Count -ne 698){throw 'Generated source inventory differs'}
    $overlay=Join-Path $root 'DefaultTimeZoneProvider.cs'
    Pin $overlay 'C8C82155789ECC8530F8854DD5956F93A46C0F064D1F15BE251ABCAE926297B7'
    $sourceHelper=Join-Path $modern 'InterpreterBaselineCompiler.cs'
    $before=[IO.File]::ReadAllText($sourceHelper)
    $after=$before.Replace('InterpreterBaselineCompiler','TemporalStartupCompiler').Replace('lea.source-baseline.','lea.temporal-startup-experiment.').Replace('baseline;public-sign;','temporal-startup-experiment;public-sign;')
    if($after.Replace('TemporalStartupCompiler','InterpreterBaselineCompiler').Replace('lea.temporal-startup-experiment.','lea.source-baseline.').Replace('temporal-startup-experiment;public-sign;','baseline;public-sign;') -cne $before){throw 'Compiler provenance inverse failed'}
    $helper=Join-Path $out 'TemporalStartupCompiler.cs';[IO.File]::WriteAllText($helper,$after,[Text.UTF8Encoding]::new($false))
    Pin $helper (Get-FileHash $helper).Hash
    $hostRefs=@(Get-ChildItem (Join-Path $PSHOME 'ref') -File -Filter '*.dll'|ForEach-Object FullName)
    foreach($spec in @(
        @{name='Microsoft.CodeAnalysis.dll';hash='1AB0A49C07AF467D16EC723C5D95DAE3F243AD2ECEA840312228ECDEA8D6E021'},
        @{name='Microsoft.CodeAnalysis.CSharp.dll';hash='CE8DFED4EB61F1B6FECB6D06E3AADFC21B36C478FD863ED014BAD2CB01987DFD'}
    )){$path=Join-Path $PSHOME $spec.name;Pin $path $spec.hash;$null=[Reflection.Assembly]::LoadFrom($path);$hostRefs+=$path}
    Add-Type -Path $helper -ReferencedAssemblies $hostRefs -CompilerOptions '/nowarn:1701'
    $references=@(Get-ChildItem 'C:/Program Files/dotnet/packs/Microsoft.NETCore.App.Ref/8.0.32/ref/net8.0' -File -Filter '*.dll'|Sort-Object Name|ForEach-Object FullName)
    $references+=Join-Path $modern 'apphost-v5/Acornima.dll'
    $key=Join-Path $modern 'interpreter-source-v1/source/Jint/Jint.snk'
    $resource=Join-Path $modern 'interpreter-source-v1/source/Jint/Native/Intl/Data/LikelySubtags.txt'
    foreach($name in @('control','lazy')){
        if([IO.DriveInfo]::new('C:\').AvailableFreeSpace -lt 512MB){throw 'Working reserve required before emit'}
        $directory=Join-Path $out $name;$null=New-Item -ItemType Directory -Path $directory
        $units=@($sources|ForEach-Object {if($name -ceq 'lazy' -and $_ -ceq $target){$overlay}else{$_}})
        $binary=Join-Path $directory 'Jint.dll'
        $check=[TemporalStartupCompiler]::Build([string[]]$units,[string[]]$references,$key,$resource,$binary)
        $runs.Add(@{name=$name;check=$check;binary=$binary;sha256=$(if(Test-Path $binary){(Get-FileHash $binary).Hash}else{$null})})
        if(!$check.Emitted){throw 'Interpreter emit failed'}
    }
    $result.passed=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash $_.path).Hash -cne $_.sha256}).Count -eq 0
    if(!$result.inputsUnchanged){$result.passed=$false}
    $result|ConvertTo-Json -Depth 8|Set-Content (Join-Path $out 'result.json')
    foreach($lease in $leases){$lease.Dispose()}
}
[pscustomobject]$result|Select-Object passed,inputsUnchanged,error,runs|ConvertTo-Json -Depth 5
if(!$result.passed){throw 'Temporal interpreter build incomplete; retain evidence'}
