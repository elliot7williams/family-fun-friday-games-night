function Invoke-WorkerStartupTrace {
    param([Parameter(Mandatory)][scriptblock]$Action)
    $savedStartupTrace=[Environment]::GetEnvironmentVariable('LEA_SCRIPT_STARTUP_TRACE')
    try{
        [Environment]::SetEnvironmentVariable('LEA_SCRIPT_STARTUP_TRACE','1')
        & $Action
    }finally{
        if($null -eq $savedStartupTrace){[Environment]::SetEnvironmentVariable('LEA_SCRIPT_STARTUP_TRACE',[NullString]::Value)}
        else{[Environment]::SetEnvironmentVariable('LEA_SCRIPT_STARTUP_TRACE',$savedStartupTrace)}
        if([Environment]::GetEnvironmentVariable('LEA_SCRIPT_STARTUP_TRACE') -cne $savedStartupTrace){throw 'Worker startup trace environment was not restored'}
    }
}
