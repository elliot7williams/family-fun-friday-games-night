function Initialize-NumericScriptSuite {
    param($Build,[string]$Base,[string]$OutputRoot,[string]$Workspace)
    if((Get-PSDrive C).Free -lt 600MB){throw 'Relocation staging reserve required'}
    $runtime=Join-Path $OutputRoot 'runtime'
    $null=New-Item -ItemType Directory $runtime
    $helper=Join-Path $Workspace 'Lea.Build/New-EntryFixtureRelocation.ps1'
    Hold $helper '2DD060255306D4B19A7805EA9CC3EA350396F4379036C2F09006995289AB3B0E'
    . $helper
    $skip=@('Lea.Script.Tests.exe','Lea.Script.Tests.pdb','WorkerEntryProbe\WorkerEntryNativeHost.exe','WorkerEntryProbe\manifest.json')
    $replacement=@{
        'Lea.Script.Win.dll'=@('continuity-evidence/script-adapter-source-build-v2/Lea.Script.Win.dll','AD4A057F5E82C8A36FA8362E54D1F746AD6264B9DF8D5447491DE001E34A59EA')
        'Lea.Engine.dll'=@('continuity-evidence/numeric-references-v3/candidate/Lea.Engine.dll','515121C456FEE03257695BB34BA7302CF11AF0FF868186A14FBB5E9CADF9DFC1')
        'Lea.Script.dll'=@('continuity-evidence/permission-contract-v1/early-limits-v1/Lea.Script.dll','E68504B515405FA2C75B92904277A7A8074945BDCC02E1CAD10A57EB9A6FC1E5')
        'Lea.Win.dll'=@('continuity-evidence/permission-contract-v1/retry-view-v1/Lea.Win.dll','E26135F8CE6BA76A76515D2874DFE219E172B212037CD567AD37042DA516289B')
        'WorkerEntryProbe\Lea.Script.dll'=@('continuity-evidence/permission-contract-v1/early-limits-v1/Lea.Script.dll','E68504B515405FA2C75B92904277A7A8074945BDCC02E1CAD10A57EB9A6FC1E5')
        'ScriptWorker\Jint.dll'=@('continuity-evidence/temporal-startup-v1/native-pair-v2/lazy/Jint.dll','67F3E7D75EDADCBC0110FD6BB7AC26E56784D3650A83C2AE0AA23DD67EB03E6D')
    }
    $replaced=[Collections.Generic.HashSet[string]]::new()
    $componentBindings=[Collections.Generic.List[object]]::new()
    foreach($file in $Build.files){
        $relative=[IO.Path]::GetRelativePath($Build.candidate,$file.path)
        if([IO.Path]::IsPathRooted($relative) -or $relative.Split('\') -contains '..'){throw 'Runtime path escape'}
        Hold $file.path $file.sha256
        if($relative -cin $skip -or $relative.StartsWith('ComponentProbes',[StringComparison]::Ordinal)){continue}
        $target=Join-Path $runtime $relative
        $null=New-Item -ItemType Directory (Split-Path $target) -Force
        $from=$file.path;$expected=$file.sha256
        if($replacement.ContainsKey($relative)){$from=Join-Path $Workspace $replacement[$relative][0];$expected=$replacement[$relative][1];Hold $from $expected;$null=$replaced.Add($relative)}
        Copy-Item -LiteralPath $from -Destination $target
        Hold $target $expected
    }
    if($replaced.Count -ne 6){throw 'Current component replacements incomplete'}
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
    $resourcePath=Join-Path $Base 'fixture.json'
    Hold $resourcePath '11C24CC01F9BE738A43491B2F3377BDA0C89D44FCB730EC119EE4B69240A53D3'
    $old=Get-Content -LiteralPath $resourcePath -Raw|ConvertFrom-Json -AsHashtable
    $relocated=New-EntryFixtureRelocation $old (Join-Path $Build.candidate 'WorkerEntryFixture') $runtime
    foreach($name in $relocated.files.Keys){if((Get-FileHash -LiteralPath (Join-Path $relocated.directory $name)).Hash -cne $relocated.files[$name]){throw 'Relocated fixture bytes differ'}}
    $resource=Join-Path $OutputRoot 'relocated-fixture.json'
    [IO.File]::WriteAllText($resource,($relocated|ConvertTo-Json -Depth 5),[Text.UTF8Encoding]::new($false))
    Hold $resource (Get-FileHash -LiteralPath $resource).Hash
    $recipe=Join-Path $Base 'entry-host.rsp'
    Hold $recipe 'C7D1102AB7D2E221EF2C64530821DCE4F5E8A162BF9AD42379A969B05EAE66BE'
    $entryHost=Join-Path $runtime 'WorkerEntryProbe/WorkerEntryNativeHost.exe'
    $arguments=[Collections.Generic.List[string]]::new();$outputs=0;$resources=0;$sources=0
    foreach($line in [IO.File]::ReadAllLines($recipe)){
        if($line.StartsWith('/out:')){$arguments.Add('/out:"'+$entryHost+'"');$outputs++}
        elseif($line.StartsWith('/resource:')){$arguments.Add('/resource:"'+$resource+'",Lea.EntryFixture.json');$resources++}
        else{
            if($line.StartsWith('/reference:')){$path=$line.Substring(11).Trim('"');Hold $path (OriginalHash $path)}
            elseif($line.StartsWith('"') -and $line.EndsWith('.cs"')){
                $originalPath=$line.Trim('"');Hold $originalPath 'BD06A62CEF747C8617ED7217507397B46C241472079403194BCEAC2EF0D47646'
                $originalText=[IO.File]::ReadAllText($originalPath)
                $oldParent='7C46E97E6F836716687ED7F2822F3024F9E73A006A9CCD0A7366E7DD484432A1'
                $currentParent='E68504B515405FA2C75B92904277A7A8074945BDCC02E1CAD10A57EB9A6FC1E5'
                if([regex]::Matches($originalText,$oldParent).Count -ne 1){throw 'Expected one confinement parent digest'}
                $updated=$originalText.Replace($oldParent,$currentParent)
                if($updated.Replace($currentParent,$oldParent) -cne $originalText){throw 'Entry host changed beyond parent identity binding'}
                $hostSource=Join-Path $OutputRoot 'WorkerEntryNativeHost.cs'
                [IO.File]::WriteAllText($hostSource,$updated,[Text.UTF8Encoding]::new($false))
                Hold $hostSource (Get-FileHash -LiteralPath $hostSource).Hash
                $line='"'+$hostSource+'"';$sources++
            }
            $arguments.Add($line)
        }
    }
    if($outputs -ne 1 -or $resources -ne 1 -or $sources -ne 1){throw 'Entry host recipe shape differs'}
    $rsp=Join-Path $OutputRoot 'relocated-entry-host.rsp';[IO.File]::WriteAllLines($rsp,$arguments,[Text.UTF8Encoding]::new($false));$hash=(Get-FileHash -LiteralPath $rsp).Hash;Hold $rsp $hash
    $wrapper=Join-Path $Workspace 'Lea.Build/Invoke-PublicCompiler.20261022.ps1';Hold $wrapper '1A04EE9A191D522955C633F97A24C9C0921A5738F76D0DF36DB1C92A574EEA03'
    $compile=Run 'compile-relocated-entry' (Get-Process -Id $PID).Path @('-NoProfile','-File',('"'+$wrapper+'"'),'-ResponseFile',('"'+$rsp+'"'),'-ExpectedResponseSha256',$hash,'-ExpectedSourceSha256','D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB') 120
    if(!$compile.passed){throw 'Relocated entry compilation failed'}
    $oldManifest=Join-Path $Build.candidate 'WorkerEntryProbe/manifest.json'
    $oldHash=OriginalHash $oldManifest;Hold $oldManifest $oldHash
    $manifest=Get-Content -LiteralPath $oldManifest -Raw|ConvertFrom-Json -AsHashtable
    if($manifest.files.Count -ne 3 -or $manifest.fixtureFiles.Count -ne 7){throw 'Entry manifest schema differs'}
    $manifest.files['Lea.Script.dll']='E68504B515405FA2C75B92904277A7A8074945BDCC02E1CAD10A57EB9A6FC1E5'
    $manifest.files['WorkerEntryNativeHost.exe']=(Get-FileHash -LiteralPath $entryHost).Hash
    foreach($name in $manifest.files.Keys){Hold (Join-Path (Split-Path $entryHost) $name) $manifest.files[$name]}
    foreach($name in $manifest.fixtureFiles.Keys){if($manifest.fixtureFiles[$name] -cne $relocated.files[$name]){throw 'Fixture manifest pin changed'}}
    $newManifest=Join-Path $runtime 'WorkerEntryProbe/manifest.json'
    [IO.File]::WriteAllText($newManifest,($manifest|ConvertTo-Json -Depth 5),[Text.UTF8Encoding]::new($false))
    $newHash=(Get-FileHash -LiteralPath $newManifest).Hash;Hold $newManifest $newHash
    return @{runtime=$runtime;oldManifestSha256=$oldHash;newManifestSha256=$newHash;resource=$resource;host=$entryHost;hostSha256=$manifest.files['WorkerEntryNativeHost.exe'];fixturePinsUnchanged=$true;workerChanged=$false;componentBindings=$componentBindings;currentParentAndView=$true;lazyInterpreter=$true}
}
