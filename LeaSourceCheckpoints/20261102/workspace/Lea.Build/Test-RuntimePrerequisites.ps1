function Test-LeaPrerequisiteInventory {
    param([Parameter(Mandatory)]$Inventory)
    $problems=[Collections.Generic.List[string]]::new()
    if(!$Inventory.windows){$problems.Add('Windows is required.')}
    if($Inventory.workerMachine -ne 34404){$problems.Add('This candidate requires the recorded x64 worker apphost.')}
    if(!$Inventory.os64){$problems.Add('A 64-bit Windows OS is required for this worker.')}
    if(!$Inventory.frameworkVersion -or [version]$Inventory.frameworkVersion -lt [version]'4.8'){$problems.Add('.NET Framework 4.8 or later was not found.')}
    if($Inventory.frameworkName -cne 'Microsoft.NETCore.App' -or $Inventory.frameworkRequired -cne '8.0.32' -or $Inventory.rollForward -cne 'Disable'){$problems.Add('Worker runtime configuration differs from this candidate.')}
    if('8.0.32' -cnotin @($Inventory.coreVersions)){$problems.Add('Exact x64 Microsoft.NETCore.App 8.0.32 was not found.')}
    if(!$Inventory.hostFxrPresent){$problems.Add('The x64 .NET host resolver was not found.')}
    if(@($Inventory.missingFiles).Count){$problems.Add('Required candidate files are missing: '+($Inventory.missingFiles -join ', '))}
    return [pscustomobject]@{prerequisitesPresent=$problems.Count -eq 0;problems=$problems.ToArray();inventory=$Inventory;runtimeExecuted=$false;confinementVerified=$false;cleanPcValidated=$false}
}

function Get-LeaRuntimePrerequisites {
    param([Parameter(Mandatory)][string]$Root)
    $ErrorActionPreference='Stop'
    $rootPath=[IO.Path]::GetFullPath($Root)
    $required=@('Firespider Internet 19.exe','Firespider Internet 19.exe.config','Lea.Engine.dll','Lea.Win.dll','Lea.Script.dll','Lea.Script.Win.dll','Lea.Image.exe',
        'ScriptWorker/Lea.Script.Worker.exe','ScriptWorker/Lea.Script.Worker.dll','ScriptWorker/Lea.Script.Worker.runtimeconfig.json','ScriptWorker/Lea.Script.Worker.deps.json','ScriptWorker/Jint.dll','ScriptWorker/Acornima.dll')
    $missing=@($required|Where-Object {!(Test-Path -LiteralPath (Join-Path $rootPath $_) -PathType Leaf)})
    $inventory=[ordered]@{root=$rootPath;windows=[Environment]::OSVersion.Platform -eq 'Win32NT';os64=[Environment]::Is64BitOperatingSystem;workerMachine=0;frameworkVersion=$null;frameworkName=$null;frameworkRequired=$null;rollForward=$null;coreVersions=@();hostFxrPresent=$false;missingFiles=$missing}
    $configPath=Join-Path $rootPath 'ScriptWorker/Lea.Script.Worker.runtimeconfig.json'
    if(Test-Path -LiteralPath $configPath -PathType Leaf){
        $options=(Get-Content -LiteralPath $configPath -Raw|ConvertFrom-Json).runtimeOptions
        $inventory.frameworkName=$options.framework.name;$inventory.frameworkRequired=$options.framework.version;$inventory.rollForward=$options.rollForward
    }
    $workerPath=Join-Path $rootPath 'ScriptWorker/Lea.Script.Worker.exe'
    if(Test-Path -LiteralPath $workerPath -PathType Leaf){
        $stream=[IO.File]::OpenRead($workerPath);$reader=[IO.BinaryReader]::new($stream)
        try{
            if($stream.Length -lt 64 -or $reader.ReadUInt16() -ne 23117){throw 'Invalid worker DOS header'}
            $stream.Position=60;$offset=$reader.ReadInt32()
            if($offset -lt 64 -or $offset -gt $stream.Length-6){throw 'Invalid worker PE offset'}
            $stream.Position=$offset
            if($reader.ReadUInt32() -ne 17744){throw 'Invalid worker PE signature'}
            $inventory.workerMachine=$reader.ReadUInt16()
        }finally{$reader.Dispose();$stream.Dispose()}
    }
    if($inventory.windows){
        $registry=[Microsoft.Win32.RegistryKey]::OpenBaseKey([Microsoft.Win32.RegistryHive]::LocalMachine,[Microsoft.Win32.RegistryView]::Registry64)
        try{
            $framework=$registry.OpenSubKey('SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full')
            if($framework){try{if($framework.GetValue('Install',0) -eq 1){$inventory.frameworkVersion=[string]$framework.GetValue('Version',$null)}}finally{$framework.Dispose()}}
        }finally{$registry.Dispose()}
        $registry=[Microsoft.Win32.RegistryKey]::OpenBaseKey([Microsoft.Win32.RegistryHive]::LocalMachine,[Microsoft.Win32.RegistryView]::Registry32)
        try{
            $dotnet=$registry.OpenSubKey('SOFTWARE\dotnet\Setup\InstalledVersions\x64')
            if($dotnet){try{$location=[string]$dotnet.GetValue('InstallLocation',$null)}finally{$dotnet.Dispose()}}else{$location=$null}
        }finally{$registry.Dispose()}
        if($location){
            $inventory.dotnetLocation=$location
            $shared=Join-Path $location 'shared/Microsoft.NETCore.App'
            if(Test-Path -LiteralPath $shared){$inventory.coreVersions=@(Get-ChildItem -LiteralPath $shared -Directory|Where-Object {Test-Path -LiteralPath (Join-Path $_.FullName 'coreclr.dll')}|ForEach-Object Name)}
            $fxr=Join-Path $location 'host/fxr'
            if(Test-Path -LiteralPath $fxr){$inventory.hostFxrPresent=@(Get-ChildItem -LiteralPath $fxr -Directory|Where-Object {Test-Path -LiteralPath (Join-Path $_.FullName 'hostfxr.dll')}).Count -gt 0}
        }
    }
    Test-LeaPrerequisiteInventory ([pscustomobject]$inventory)
}
