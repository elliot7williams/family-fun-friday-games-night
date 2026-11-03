function Test-LeaComponentHostInventory {
    param([Parameter(Mandatory)]$Inventory)
    $problems=[Collections.Generic.List[string]]::new()
    if(!$Inventory.windows -or !$Inventory.os64){$problems.Add('Component probes require 64-bit Windows.')}
    if(!$Inventory.launcherPresent){$problems.Add('The component-probe dotnet.exe launcher is missing; repair the .NET host installation. Shared runtime files alone are insufficient for this probe.')}
    elseif($Inventory.launcherMachine -ne 34404){$problems.Add('The component-probe launcher is not the expected x64 executable.')}
    if(!$Inventory.coreClrPresent){$problems.Add('The exact Microsoft.NETCore.App 8.0.32 runtime file was not found.')}
    if(!$Inventory.hostFxrPresent){$problems.Add('The exact 8.0.32 host resolver file was not found.')}
    [pscustomobject]@{prerequisitesPresent=$problems.Count -eq 0;problems=$problems.ToArray();inventory=$Inventory;runtimeExecuted=$false;securityOrConfinementVerified=$false;scope='Component-probe CLI prerequisites, not a requirement that production apphosts launch through dotnet.exe.'}
}

function Get-LeaComponentProbePrerequisites {
    $root=[Environment]::GetEnvironmentVariable('ProgramW6432')
    if(!$root){$root=[Environment]::GetFolderPath([Environment+SpecialFolder]::ProgramFiles)}
    $root=Join-Path $root 'dotnet'
    $launcher=Join-Path $root 'dotnet.exe'
    $inventory=[ordered]@{windows=[Environment]::OSVersion.Platform -eq 'Win32NT';os64=[Environment]::Is64BitOperatingSystem;root=$root;launcher=$launcher;launcherPresent=Test-Path -LiteralPath $launcher -PathType Leaf;launcherMachine=0;coreClrPresent=Test-Path -LiteralPath (Join-Path $root 'shared/Microsoft.NETCore.App/8.0.32/coreclr.dll') -PathType Leaf;hostFxrPresent=Test-Path -LiteralPath (Join-Path $root 'host/fxr/8.0.32/hostfxr.dll') -PathType Leaf}
    if($inventory.launcherPresent){
        $reader=[IO.BinaryReader]::new([IO.File]::OpenRead($launcher))
        try{
            if($reader.BaseStream.Length -lt 64 -or $reader.ReadUInt16() -ne 23117){throw 'Invalid component launcher DOS header'}
            $reader.BaseStream.Position=60;$offset=$reader.ReadInt32()
            if($offset -lt 64 -or $offset -gt $reader.BaseStream.Length-6){throw 'Invalid component launcher PE offset'}
            $reader.BaseStream.Position=$offset
            if($reader.ReadUInt32() -ne 17744){throw 'Invalid component launcher PE signature'}
            $inventory.launcherMachine=$reader.ReadUInt16()
        }finally{$reader.Dispose()}
    }
    Test-LeaComponentHostInventory ([pscustomobject]$inventory)
}
