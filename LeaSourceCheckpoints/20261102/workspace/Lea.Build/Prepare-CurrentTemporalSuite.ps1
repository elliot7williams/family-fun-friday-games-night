$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$oldHelper=Join-Path $PSScriptRoot 'Initialize-SuiteRelocationV2.generated.ps1'
$oldBuilder=Join-Path $PSScriptRoot 'Build-RelocatedSuitePhaseV2.generated.ps1'
if((Get-FileHash -LiteralPath $oldHelper).Hash -cne '75739AD7F77F82A22D455225438D15995F8C7D9C47ABB6924FB0B3DCA4FBF449' -or (Get-FileHash -LiteralPath $oldBuilder).Hash -cne 'E49E10DA07F2329086FF1BFA2701A830CFA6B4085E1E8F0A7F85A4BAA6F32241'){throw 'Baseline generator inputs changed'}
$helper=Join-Path $PSScriptRoot 'Initialize-CurrentTemporalSuite.generated.ps1'
$builder=Join-Path $PSScriptRoot 'Build-CurrentTemporalSuite.generated.ps1'
if((Test-Path $helper) -or (Test-Path $builder)){throw 'Preserve current suite builders'}
function ReplaceOnce([string]$old,[string]$new){
    if($script:text.IndexOf($old,[StringComparison]::Ordinal) -lt 0 -or $script:text.IndexOf($old,[StringComparison]::Ordinal) -ne $script:text.LastIndexOf($old,[StringComparison]::Ordinal)){throw ('Ambiguous anchor: '+$old)}
    $script:text=$script:text.Replace($old,$new)
}
$text=[IO.File]::ReadAllText($oldHelper).Replace("`r`n","`n").Replace('function Initialize-SuiteRelocationV2 {','function Initialize-CurrentTemporalSuite {')
$insert=@'
    $replacement=@{
        'Lea.Script.dll'=@('continuity-evidence/permission-contract-v1/early-limits-v1/Lea.Script.dll','E68504B515405FA2C75B92904277A7A8074945BDCC02E1CAD10A57EB9A6FC1E5')
        'Lea.Win.dll'=@('continuity-evidence/permission-contract-v1/retry-view-v1/Lea.Win.dll','E26135F8CE6BA76A76515D2874DFE219E172B212037CD567AD37042DA516289B')
        'WorkerEntryProbe\Lea.Script.dll'=@('continuity-evidence/permission-contract-v1/early-limits-v1/Lea.Script.dll','E68504B515405FA2C75B92904277A7A8074945BDCC02E1CAD10A57EB9A6FC1E5')
        'ScriptWorker\Jint.dll'=@('continuity-evidence/temporal-startup-v1/native-pair-v2/lazy/Jint.dll','67F3E7D75EDADCBC0110FD6BB7AC26E56784D3650A83C2AE0AA23DD67EB03E6D')
    }
    $replaced=[Collections.Generic.HashSet[string]]::new()
    $componentBindings=[Collections.Generic.List[object]]::new()
'@
ReplaceOnce '    foreach($file in $Build.files){' ($insert+"`n    foreach(`$file in `$Build.files){")
ReplaceOnce '        if($relative -cin $skip){continue}' '        if($relative -cin $skip -or $relative.StartsWith(''ComponentProbes'',[StringComparison]::Ordinal)){continue}'
ReplaceOnce '        Copy-Item -LiteralPath $file.path -Destination $target' @'
        $from=$file.path;$expected=$file.sha256
        if($replacement.ContainsKey($relative)){$from=Join-Path $Workspace $replacement[$relative][0];$expected=$replacement[$relative][1];Hold $from $expected;$null=$replaced.Add($relative)}
        Copy-Item -LiteralPath $from -Destination $target
'@
ReplaceOnce '        Hold $target $file.sha256' '        Hold $target $expected'
$insert=@'
    if($replaced.Count -ne 4){throw 'Current component replacements incomplete'}
    $componentProof=Join-Path $Workspace 'continuity-evidence/temporal-startup-v1/normal-component-probes-v1/result.json'
    Hold $componentProof 'CF6A3FBAFC0D965FFA0F8A279BE9BD3F619F0B42EF0E417FFC6735C3067021CF'
    $components=Get-Content -LiteralPath $componentProof -Raw|ConvertFrom-Json
    if(!$components.passed -or !$components.inputsUnchanged -or $components.ownerAttention -or $components.reports.Count -ne 5){throw 'Normal worker component gates incomplete'}
    foreach($probe in $components.reports){
        if($probe.name -cnotin @('markers','frames','clock','wire','bootstrap') -or $probe.workerSha256 -cne '60FF26D8357FEA52C802BAC562C30C6CE28F72F735E78281D0908E94FAB530D6'){throw 'Wrong component binding'}
        $manifestPath=Join-Path $probe.directory 'manifest.json';Hold $manifestPath $probe.manifestSha256
        $spec=Get-Content -LiteralPath $manifestPath -Raw|ConvertFrom-Json -AsHashtable
        if($spec.files['Jint.dll'] -cne '67F3E7D75EDADCBC0110FD6BB7AC26E56784D3650A83C2AE0AA23DD67EB03E6D'){throw 'Wrong component interpreter'}
        $targetDir=Join-Path $runtime ('ComponentProbes/'+$probe.name);$null=New-Item -ItemType Directory $targetDir -Force
        foreach($name in $spec.files.Keys){
            if([IO.Path]::GetFileName($name) -cne $name){throw 'Component leaf escape'}
            $from=Join-Path $probe.directory $name;$to=Join-Path $targetDir $name
            Hold $from $spec.files[$name];Copy-Item -LiteralPath $from -Destination $to;Hold $to $spec.files[$name]
        }
        $to=Join-Path $targetDir 'manifest.json';Copy-Item -LiteralPath $manifestPath -Destination $to;Hold $to $probe.manifestSha256
        $oldManifestPath=Join-Path $Build.candidate ('ComponentProbes/'+$probe.name+'/manifest.json')
        $componentBindings.Add(@{name=$probe.name;old=OriginalHash $oldManifestPath;replacement=$probe.manifestSha256})
    }
'@
ReplaceOnce '    $resourcePath=Join-Path' ($insert+"`n    `$resourcePath=Join-Path")
ReplaceOnce "    `$manifest.files['WorkerEntryNativeHost.exe']=" "    `$manifest.files['Lea.Script.dll']='E68504B515405FA2C75B92904277A7A8074945BDCC02E1CAD10A57EB9A6FC1E5'`n    `$manifest.files['WorkerEntryNativeHost.exe']="
ReplaceOnce 'fixturePinsUnchanged=$true;workerChanged=$false}' 'fixturePinsUnchanged=$true;workerChanged=$false;componentBindings=$componentBindings;currentParentAndView=$true;lazyInterpreter=$true}'
$t=$null;$e=$null;[void][Management.Automation.Language.Parser]::ParseInput($text,[ref]$t,[ref]$e);if($e.Count){throw 'Current helper syntax invalid'}
[IO.File]::WriteAllText($helper,$text,[Text.UTF8Encoding]::new($false));$helperHash=(Get-FileHash -LiteralPath $helper).Hash
$text=[IO.File]::ReadAllText($oldBuilder).Replace("`r`n","`n").Replace("'suite-phase-relocated-v2'","'current-lazy-suite-v1'").Replace('Initialize-SuiteRelocationV2.generated.ps1','Initialize-CurrentTemporalSuite.generated.ps1').Replace('75739AD7F77F82A22D455225438D15995F8C7D9C47ABB6924FB0B3DCA4FBF449',$helperHash).Replace('=Initialize-SuiteRelocationV2 $build','=Initialize-CurrentTemporalSuite $build')
$start=$text.IndexOf('                if([regex]::Matches($original,$result.relocation.oldManifestSha256)')
$end=$text.IndexOf('                $selected=Join-Path $outRoot ''ModernComponents.overlay.cs''',$start)
if($start -lt 0 -or $end -le $start){throw 'Dispatcher binding block missing'}
$updated=@'
                $bindings=@($result.relocation.componentBindings)+@(@{name='entry';old=$result.relocation.oldManifestSha256;replacement=$result.relocation.newManifestSha256})
                if($bindings.Count -ne 6){throw 'Six dispatcher bindings required'}
                $updated=$original
                foreach($binding in $bindings){if([regex]::Matches($updated,$binding.old).Count -ne 1){throw 'Dispatcher anchor differs'};$updated=$updated.Replace($binding.old,$binding.replacement)}
                $inverse=$updated
                for($i=$bindings.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($bindings[$i].replacement,$bindings[$i].old)}
                if($inverse -cne $original){throw 'Dispatcher source changed beyond digests'}
'@
$text=$text.Remove($start,$end-$start).Insert($start,$updated+"`n")
ReplaceOnce '                Hold $newPath $expected' @'
                if($newPath -cne $path){
                    $pins=@($inputs|Where-Object path -ceq $newPath|ForEach-Object sha256|Sort-Object -Unique)
                    if($pins.Count -ne 1){throw 'Relocated reference lacks pinned provenance'}
                    $expected=$pins[0]
                }
                Hold $newPath $expected
'@
$t=$null;$e=$null;[void][Management.Automation.Language.Parser]::ParseInput($text,[ref]$t,[ref]$e);if($e.Count){throw 'Current builder syntax invalid'}
[IO.File]::WriteAllText($builder,$text,[Text.UTF8Encoding]::new($false))
[pscustomobject]@{helperSha256=$helperHash;builderSha256=(Get-FileHash -LiteralPath $builder).Hash;workerHashUnchanged=$true;dispatcherDigestBindings=6;syntaxValidated=$true;executed=$false}|ConvertTo-Json
