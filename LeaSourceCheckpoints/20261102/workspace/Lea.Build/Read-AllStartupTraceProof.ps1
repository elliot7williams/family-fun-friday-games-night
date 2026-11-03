function Read-AllStartupTraceProof {
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][AllowEmptyString()][string[]]$Lines,
        [Parameter(Mandatory)][bool]$StreamsComplete,
        [Parameter(Mandatory)][bool]$ExitObserved,
        [Parameter(Mandatory)][bool]$TimedOut,
        [Parameter(Mandatory)][bool]$KillRequested
    )
    $markers=@($Lines|Where-Object {$_.StartsWith('LEA_ALL_STARTUP_TRACE ',[StringComparison]::Ordinal)})
    $enabled=@($markers|Where-Object {$_ -ceq 'LEA_ALL_STARTUP_TRACE enabled'}).Count
    $restored=@($markers|Where-Object {$_ -ceq 'LEA_ALL_STARTUP_TRACE restored=true'}).Count
    $shape=$markers.Count -eq 2 -and $enabled -eq 1 -and $restored -eq 1 -and
        $markers[0] -ceq 'LEA_ALL_STARTUP_TRACE enabled' -and $markers[1] -ceq 'LEA_ALL_STARTUP_TRACE restored=true'
    $retired=$StreamsComplete -and $ExitObserved -and !$TimedOut -and !$KillRequested
    @{
        traceActivationVerified=$StreamsComplete -and $enabled -eq 1
        traceEnvironmentRestored=$shape -and $retired
        markerShapeVerified=$shape
        processResultEligible=$retired
        scope='Diagnostic trace markers only; not descendant cleanup, full-suite success or release proof. Forced stops fail closed even if markers exist.'
    }
}
