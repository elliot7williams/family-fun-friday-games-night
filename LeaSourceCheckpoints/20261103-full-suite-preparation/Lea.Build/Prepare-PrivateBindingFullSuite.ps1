$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$baseline='I:\LeaBuildStorage\sdk-full-script-assembly-20261101-v8'
$entry='I:\LeaBuildStorage\sdk-full-script-entry-20261101-v8'
$out='I:\LeaBuildStorage\private-binding-full-preparation-20261103-v1'
$oldParent='D2685E2F9AC15A72373CEE7B2B5EA69A7DD46605E1C058CB32406A085D7F6518'
$newParent='406C5634638D75C66892BD95429760CB4A5C74C94277913D1DF467147E30E75F'
$oldWorker='E777E0CAE71D694BBA96E43E01BEED102B2B512B0B24B4ABE11AF2CDB3E7C2C6'
$newWorker='F61B6858822F8F7BB0350B91D21862C6D22EC955EE94E2E4F4060FA01FB344F0'
if(Test-Path -LiteralPath $out){throw 'Preserve existing full-suite preparation'}
$inputs=[Collections.Generic.List[object]]::new()
$holds=[Collections.Generic.List[IDisposable]]::new()
$changes=[Collections.Generic.List[object]]::new()
$rows=[Collections.Generic.Dictionary[string,object]]::new([StringComparer]::OrdinalIgnoreCase)
function Pin([string]$path,[string]$hash){
    $item=Get-Item -LiteralPath $path
    while($item){
        if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse preparation input refused'}
        $item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}
    }
    $stream=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($stream)
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) -cne $hash){throw ('Preparation input differs: '+$path)}
    $inputs.Add(@{path=$path;sha256=$hash})
}
function Proof([string]$path,[string]$hash){Pin $path $hash;Get-Content -LiteralPath $path -Raw|ConvertFrom-Json}
function Store([string]$relative,[string]$kind){
    $path=Join-Path $out $relative
    $hash=(Get-FileHash -LiteralPath $path).Hash
    $changes.Add(@{path=$relative;kind=$kind;originalSha256=$rows[$relative].sha256;sha256=$hash})
    $rows[$relative]=@{path=$relative;sha256=$hash}
}
function Project([string]$text,[string]$old,[string]$new){
    if([regex]::Matches($text,[regex]::Escape($old)).Count -ne 1 -or $text.Contains($new)){throw 'Unique source identity required'}
    $updated=$text.Replace($old,$new)
    if($updated.Replace($new,$old) -cne $text){throw 'Source identity inverse failed'}
    return $updated
}
try{
    $original=Proof (Join-Path $baseline 'assembly.json') 'D1E745E50AFE789D845BB43B27216BF52A555D0A29FF8E65F511AE5874430BCB'
    $parent=Proof 'I:\LeaBuildStorage\private-binding-build-20261102-v1\result.json' 'D32732C0BF3B548DB649966100D4B074663E07544BC9C25A648A80D6723AB02C'
    $worker=Proof 'I:\LeaBuildStorage\private-binding-worker-build-20261102-v1\result.json' '302C3003416F9095BA56C214FF6FD8B4F05A82F102CBAAF53B23DC3FAEE6E398'
    if(!$original.verified -or $original.files.Count -ne 305 -or $original.compiledSources -ne 60 -or $original.fixtures -ne 182){throw 'Full baseline inventory differs'}
    foreach($proof in @($parent,$worker)){if(!$proof.passed -or !$proof.inputsUnchanged -or $proof.ownerAttention){throw 'Candidate build incomplete'}}
    Pin $parent.binary $newParent
    $workerDll=@($worker.products|Where-Object {[IO.Path]::GetFileName($_.path) -ceq 'Lea.Script.Worker.dll'})
    if($workerDll.Count -ne 1 -or $workerDll[0].sha256 -cne $newWorker -or $worker.products.Count -ne 6){throw 'Matching worker closure differs'}
    foreach($product in $worker.products){Pin $product.path $product.sha256}
    foreach($file in $original.files){
        if([IO.Path]::IsPathRooted($file.path) -or $file.path.Contains(':') -or $file.path.Split('/') -contains '..' -or $rows.ContainsKey($file.path)){throw 'Invalid original member'}
        Pin (Join-Path $baseline $file.path) $file.sha256
        $rows.Add($file.path,@{path=$file.path;sha256=$file.sha256})
    }
    New-Item -ItemType Directory -Path $out|Out-Null
    foreach($file in $original.files){
        $to=Join-Path $out $file.path
        New-Item -ItemType Directory -Path (Split-Path $to) -Force|Out-Null
        Copy-Item -LiteralPath (Join-Path $baseline $file.path) -Destination $to
    }
    $parents=@($original.files|Where-Object {$_.path.StartsWith('runtime/') -and [IO.Path]::GetFileName($_.path) -ceq 'Lea.Script.dll'})
    $workers=@($original.files|Where-Object {$_.path.StartsWith('runtime/') -and [IO.Path]::GetFileName($_.path) -ceq 'Lea.Script.Worker.dll'})
    if($parents.Count -ne 2 -or $workers.Count -ne 7){throw 'Exact parent/worker roles required'}
    foreach($file in $parents){if($file.sha256 -cne $oldParent){throw 'Prior parent role differs'};Copy-Item -LiteralPath $parent.binary -Destination (Join-Path $out $file.path);Store $file.path 'parent'}
    foreach($file in $workers){if($file.sha256 -cne $oldWorker){throw 'Prior worker role differs'};Copy-Item -LiteralPath $workerDll[0].path -Destination (Join-Path $out $file.path);Store $file.path 'worker'}
    $dispatcher='compiled-source/source/Lea.Script.Tests/ModernComponents.cs'
    $dispatcherText=[IO.File]::ReadAllText((Join-Path $out $dispatcher))
    $manifests=@($original.files|Where-Object {$_.path.StartsWith('runtime/ComponentProbes/') -and [IO.Path]::GetFileName($_.path) -ceq 'manifest.json'})
    if($manifests.Count -ne 5){throw 'Five component manifests required'}
    foreach($file in $manifests){
        $path=Join-Path $out $file.path
        $spec=Get-Content -LiteralPath $path -Raw|ConvertFrom-Json -AsHashtable
        if($spec.files['Lea.Script.Worker.dll'] -cne $oldWorker){throw 'Prior component worker differs'}
        $spec.files['Lea.Script.Worker.dll']=$newWorker
        $spec|ConvertTo-Json -Depth 8|Set-Content -LiteralPath $path -Encoding utf8
        Store $file.path 'component-manifest'
        $dispatcherText=Project $dispatcherText $file.sha256 $rows[$file.path].sha256
    }
    [IO.File]::WriteAllText((Join-Path $out $dispatcher),$dispatcherText,[Text.UTF8Encoding]::new($false))
    Store $dispatcher 'five-component-identities; entry identity pending host build'
    foreach($name in @('CoreProbeBridge.cs','WorkerEntryBridge.cs')){
        $relative='compiled-source/source/Lea.Script.Tests/'+$name
        $path=Join-Path $out $relative
        [IO.File]::WriteAllText($path,(Project ([IO.File]::ReadAllText($path)) $oldWorker $newWorker),[Text.UTF8Encoding]::new($false))
        Store $relative 'worker-identity-only; inverse verified'
    }
    $hostSource=Join-Path $entry 'WorkerEntryNativeHost.cs'
    Pin $hostSource 'BBE3BE124B14C73125C06D44DE6648EF696C85F2CD28D639AB4288C6099F9BF4'
    $text=Project ([IO.File]::ReadAllText($hostSource)) $oldWorker $newWorker
    $text=Project $text $oldParent $newParent
    [IO.File]::WriteAllText((Join-Path $out 'WorkerEntryNativeHost.cs'),$text,[Text.UTF8Encoding]::new($false))
    $resourcePath=Join-Path $entry 'entry-fixture.json'
    Pin $resourcePath 'F3848128462D29A0301519D30D7B6D8D3D46F41DFF6281516C97EBADF44C482D'
    $resource=Get-Content -LiteralPath $resourcePath -Raw|ConvertFrom-Json -AsHashtable
    if($resource.files.Count -ne 7 -or $resource.files['Lea.Script.Worker.dll'] -cne $oldWorker){throw 'Entry fixture differs'}
    $resource.directory=Join-Path $root 'continuity-evidence/private-binding-full-suite-v1/runtime/WorkerEntryFixture'
    $resource.files['Lea.Script.Worker.dll']=$newWorker
    foreach($name in $resource.files.Keys){
        if([IO.Path]::GetFileName($name) -cne $name -or (Get-FileHash (Join-Path $out ('runtime/WorkerEntryFixture/'+$name))).Hash -cne $resource.files[$name]){throw 'Prepared entry fixture differs'}
    }
    $resource|ConvertTo-Json -Depth 8|Set-Content (Join-Path $out 'entry-fixture.json') -Encoding utf8
    foreach($row in $rows.Values){if((Get-FileHash (Join-Path $out $row.path)).Hash -cne $row.sha256){throw 'Prepared member readback differs'}}
    foreach($input in $inputs){if((Get-FileHash $input.path).Hash -cne $input.sha256){throw 'Preparation input changed'}}
    $generated=@(foreach($name in @('WorkerEntryNativeHost.cs','entry-fixture.json')){@{path=$name;sha256=(Get-FileHash (Join-Path $out $name)).Hash}})
    @{verified=$true;inputs=$inputs;files=@($rows.Values|Sort-Object path);changes=$changes;generated=$generated;parentSha256=$newParent;workerSha256=$newWorker;readyForHostCompilation=$true;readyForSuiteCompilation=$false;compiled=$false;executed=$false;notRunnable=$true;assertionsChanged=$false;fixturesUnchanged=$true;requiredChecks=3412;requiredPhases=89;compileBudgetMs=120000;testBudgetMs=300000;cleanupBudgetMs=5000;pending=@('compile new entry host with prepared embedded fixture','regenerate entry manifest and bind its dispatcher constant','compile all60 original suite sources','fresh NTFS staging and embedded resource readback','owned full3412/89 campaign');runnerSha256=(Get-FileHash $PSCommandPath).Hash}|ConvertTo-Json -Depth 8|Set-Content (Join-Path $out 'preparation.json') -Encoding utf8
    Get-FileHash (Join-Path $out 'preparation.json')
}finally{foreach($hold in $holds){$hold.Dispose()}}
