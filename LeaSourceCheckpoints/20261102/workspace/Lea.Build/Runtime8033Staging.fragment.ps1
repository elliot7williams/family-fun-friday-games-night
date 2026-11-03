    $configReceipt=Join-Path $Workspace 'continuity-evidence/runtime-8033-configs-v1/result.json'
    Hold $configReceipt '430D79B9C4800BF19015300D60F5FDE781191EA372F0388393B7DF554C9C4318'
    $configProof=Get-Content -LiteralPath $configReceipt -Raw|ConvertFrom-Json
    if(!$configProof.passed -or !$configProof.originalsUnchanged -or $configProof.configs.Count -ne 8){throw 'Config projection incomplete'}
    $configMap=@{}
    $configUsed=[Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach($config in $configProof.configs){
        $relative=$config.relative.Replace('/','\')
        if($configMap.ContainsKey($relative) -or [IO.Path]::IsPathRooted($relative) -or $relative.Split('\') -contains '..'){throw 'Config projection path invalid'}
        Hold $config.target $config.targetSha256
        $options=(Get-Content -LiteralPath $config.target -Raw|ConvertFrom-Json).runtimeOptions
        if($options.framework.version -cne '8.0.33' -or $options.framework.name -cne 'Microsoft.NETCore.App' -or $options.rollForward -cne 'Disable'){throw 'Config runtime policy differs'}
        $configMap[$relative]=$config
    }
    function Stage-Runtime8033Input([string]$From,[string]$To,[string]$Expected){
        Hold $From $Expected
        $relative=[IO.Path]::GetRelativePath($runtime,$To).Replace('/','\')
        if([IO.Path]::IsPathRooted($relative) -or $relative.Split('\') -contains '..'){throw 'Staging path escape'}
        if($configMap.ContainsKey($relative)){
            $config=$configMap[$relative]
            if($Expected -cne $config.sourceSha256 -or !$configUsed.Add($relative)){throw 'Config original identity or coverage differs'}
            $From=$config.target;$Expected=$config.targetSha256
        }
        Copy-Item -LiteralPath $From -Destination $To
        Hold $To $Expected
        return $Expected
    }
