function Initialize-SuiteRelocation {
    param($Build,[string]$Base,[string]$OutputRoot,[string]$Workspace)
    if((Get-PSDrive C).Free -lt 600MB){throw 'Relocation staging reserve required'}
    $runtime=Join-Path $OutputRoot 'runtime'
    $null=New-Item -ItemType Directory $runtime
    $helper=Join-Path $Workspace 'Lea.Build/New-EntryFixtureRelocation.ps1'
    Hold $helper '2DD060255306D4B19A7805EA9CC3EA350396F4379036C2F09006995289AB3B0E'
    . $helper
    $skip=@('Lea.Script.Tests.exe','Lea.Script.Tests.pdb','WorkerEntryProbe\WorkerEntryNativeHost.exe','WorkerEntryProbe\manifest.json')
    foreach($file in $Build.files){
        $relative=[IO.Path]::GetRelativePath($Build.candidate,$file.path)
        if([IO.Path]::IsPathRooted($relative) -or $relative.Split('\') -contains '..'){throw 'Runtime path escape'}
        Hold $file.path $file.sha256
        if($relative -cin $skip){continue}
        $target=Join-Path $runtime $relative
        $null=New-Item -ItemType Directory (Split-Path $target) -Force
        Copy-Item -LiteralPath $file.path -Destination $target
        Hold $target $file.sha256
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
    $host=Join-Path $runtime 'WorkerEntryProbe/WorkerEntryNativeHost.exe'
    $arguments=[Collections.Generic.List[string]]::new();$outputs=0;$resources=0;$sources=0
    foreach($line in [IO.File]::ReadAllLines($recipe)){
        if($line.StartsWith('/out:')){$arguments.Add('/out:"'+$host+'"');$outputs++}
        elseif($line.StartsWith('/resource:')){$arguments.Add('/resource:"'+$resource+'",Lea.EntryFixture.json');$resources++}
        else{
            if($line.StartsWith('/reference:')){$path=$line.Substring(11).Trim('"');Hold $path (OriginalHash $path)}
            elseif($line.StartsWith('"') -and $line.EndsWith('.cs"')){Hold $line.Trim('"') 'BD06A62CEF747C8617ED7217507397B46C241472079403194BCEAC2EF0D47646';$sources++}
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
    $manifest.files['WorkerEntryNativeHost.exe']=(Get-FileHash -LiteralPath $host).Hash
    foreach($name in $manifest.files.Keys){Hold (Join-Path (Split-Path $host) $name) $manifest.files[$name]}
    foreach($name in $manifest.fixtureFiles.Keys){if($manifest.fixtureFiles[$name] -cne $relocated.files[$name]){throw 'Fixture manifest pin changed'}}
    $newManifest=Join-Path $runtime 'WorkerEntryProbe/manifest.json'
    [IO.File]::WriteAllText($newManifest,($manifest|ConvertTo-Json -Depth 5),[Text.UTF8Encoding]::new($false))
    $newHash=(Get-FileHash -LiteralPath $newManifest).Hash;Hold $newManifest $newHash
    return @{runtime=$runtime;oldManifestSha256=$oldHash;newManifestSha256=$newHash;resource=$resource;host=$host;hostSha256=$manifest.files['WorkerEntryNativeHost.exe'];fixturePinsUnchanged=$true;workerChanged=$false}
}
