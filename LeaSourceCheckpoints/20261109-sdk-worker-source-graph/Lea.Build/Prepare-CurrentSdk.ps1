param([Alias('o')][string]$Destination='I:\LeaBuildStorage\sdk-worker-source-graph-20261109-v1\source')
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$base='I:\LeaBuildStorage\sdk-print-source-graph-20261109-v1\source'
$original='I:\LeaBuildStorage\github-source-checkpoint-20261109-v1\LeaSourceCheckpoints\20261102\mature-sdk'
$out=[IO.Path]::GetFullPath($Destination)
if(Test-Path -LiteralPath $out){throw 'Preserve existing SDK export'}
$inputs=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new();$files=[Collections.Generic.Dictionary[string,object]]::new([StringComparer]::OrdinalIgnoreCase)
function Pin([string]$Path,[string]$Expected){
    $item=Get-Item -LiteralPath $Path
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse source refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
    $s=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($s)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($Expected -and $hash -cne $Expected){throw ('Source differs: '+$Path)};$inputs.Add(@{path=$Path;sha256=$hash});return $hash
}
function Save([string]$From,[string]$Relative,[string]$Expected){
    if([IO.Path]::IsPathRooted($Relative) -or $Relative.Contains(':') -or $Relative.Split('/') -contains '..' -or $files.ContainsKey($Relative)){throw 'SDK path escape/collision'}
    $hash=Pin $From $Expected;$to=Join-Path $out $Relative
    New-Item -ItemType Directory -Path (Split-Path $to -Parent) -Force|Out-Null;Copy-Item -LiteralPath $From -Destination $to
    if((Get-FileHash -LiteralPath $to).Hash -cne $hash){throw 'Export readback differs'}
    $files.Add($Relative,@{path=$Relative;sha256=$hash;origin=$From})
}
try{
    $null=Pin $PSCommandPath
    $auditPath='I:\LeaBuildStorage\sdk-print-source-graph-audit-20261109-v1\audit.json'
    $null=Pin $auditPath '24A2A32A5FAAB645E3A44E7915725AC4EB9C42CC6CE6DC9C56DDEDEB201A8E03'
    $audit=Get-Content -LiteralPath $auditPath -Raw|ConvertFrom-Json
    if(!$audit.verified -or $audit.sourceFiles.Count -ne 98){throw 'Current source graph required'}
    foreach($file in $audit.sourceFiles){Save (Join-Path $base $file.path) $file.path $file.sha256}
    $manifest=Join-Path $original 'source-manifest.json';$null=Pin $manifest 'B5A0283803C7062DA98A7F3331D94940360E00045A06038168FCA94214CD114A'
    $old=Get-Content -LiteralPath $manifest -Raw|ConvertFrom-Json
    $worker=@($old.files|Where-Object {$_.path.StartsWith('Lea.Script.Worker/',[StringComparison]::Ordinal)})
    if($worker.Count -ne 20){throw 'Original worker source coverage differs'}
    foreach($file in $worker){
        $from=Join-Path $original $file.path;$null=Pin $from $file.sha256;$hash=$file.sha256
        if($file.path -ceq 'Lea.Script.Worker/Dom.compact.js'){
            if($hash -cne 'EC4B6513A77230C3B8439568CC3FED41B7712CA3779182D38076880DCC83BC8A'){throw 'Original worker resource differs'}
            $from=Join-Path $base 'Lea.Script/Dom.compact.js';$hash='196BCE962F2C2F375C77644D0AE19485820F599170D4D80013829A6ECD2886E3'
        }
        Save $from $file.path $hash
    }
    $priorPath='I:\LeaBuildStorage\sdk-image-source-graph-20261109-v1\result.json'
    $null=Pin $priorPath '177FDA95290C6AEC605325072AEB598CC7A39A5F552910BA3FD1083DD093648F'
    $prior=Get-Content -LiteralPath $priorPath -Raw|ConvertFrom-Json
    foreach($file in Get-ChildItem -LiteralPath 'I:\LeaBuildStorage\sdk-image-source-graph-20261109-v1\source\lib' -File){
        $pins=@($prior.inputs|Where-Object path -CEQ $file.FullName|Select-Object -ExpandProperty sha256 -Unique)
        if($pins.Count -ne 1){throw 'Framework dependency pin ambiguous'};Save $file.FullName ('lib/'+$file.Name) $pins[0]
    }
    $runtime=Join-Path $root 'continuity-evidence/private-binding-full-suite-v5/runtime/ScriptWorker'
    foreach($file in $old.files|Where-Object {$_.path.StartsWith('lib/worker/')} ){
        Save (Join-Path $runtime ([IO.Path]::GetFileName($file.path))) $file.path $file.sha256
    }
    foreach($name in @('Build-NativeWorker.ps1','NativeWorker.Tools.cs','New-NativeToolchain.ps1')){Save (Join-Path $root ('Lea.Sdk/'+$name)) $name ''}
    Save (Join-Path $root 'Lea.Sdk/SDK.Full.Build.proj') 'SDK.Build.proj' ''
    foreach($name in @('ScriptEmbeddingSample.cs','ScriptEmbeddingSample.csproj','ScriptHost.config','script-embedding.html')){Save (Join-Path $root ('Lea.Sdk/'+$name)) ('ScriptSample/'+$name) ''}
    foreach($name in @('SOURCE_GRAPH.md','SDK_FULL.md')){Save (Join-Path $root ('Lea.Sdk/'+$name)) $name ''}
    $members=@($files.Values|Sort-Object path)
    if($members.Count -ne 136 -or @($members|Where-Object {$_.path -notmatch '^lib/' -and [IO.Path]::GetExtension($_.path) -in @('.dll','.exe')}).Count){throw 'Expected128source members/eight dependencies, no Lea binaries'}
    foreach($file in $members){if((Get-FileHash -LiteralPath (Join-Path $out $file.path)).Hash -cne $file.sha256){throw 'Final source drift'}}
    foreach($file in $inputs){if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256){throw 'Input drift'}}
    @{verified=$true;sourceMembers=128;dependencyMembers=8;files=$members;inputs=$inputs;workerSourceChanges='Only current private-binding DOM resource; 17 original C# sources unchanged.';runtimeVersion='8.0.33';rollForward='Disable';compiled=$false;nativeQualified=$false;fullSdk=$false;hostAdopted=$false}|ConvertTo-Json -Depth 7|Set-Content -LiteralPath (Join-Path (Split-Path $out -Parent) 'source.json') -Encoding utf8
    Get-FileHash -LiteralPath (Join-Path (Split-Path $out -Parent) 'source.json')|Format-List Hash
}finally{foreach($lease in $leases){$lease.Dispose()}}
