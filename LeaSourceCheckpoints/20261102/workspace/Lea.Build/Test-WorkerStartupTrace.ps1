$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace 'continuity-evidence/all-startup-trace-controls-v2'
if(Test-Path -LiteralPath $out){throw 'Preserve trace controls'}
. (Join-Path $PSScriptRoot 'Invoke-WorkerStartupTrace.ps1')
$original=[Environment]::GetEnvironmentVariable('LEA_SCRIPT_STARTUP_TRACE')
$rows=[Collections.Generic.List[object]]::new()
try{
    foreach($state in @(@{name='unset';value=$null},@{name='empty';value=''},@{name='disabled';value='0'},@{name='custom';value='retained-value'})){
        foreach($mode in @('return','throw','nested')){
            if($null -eq $state.value){[Environment]::SetEnvironmentVariable('LEA_SCRIPT_STARTUP_TRACE',[NullString]::Value)}
            else{[Environment]::SetEnvironmentVariable('LEA_SCRIPT_STARTUP_TRACE',$state.value)}
            $before=[Environment]::GetEnvironmentVariable('LEA_SCRIPT_STARTUP_TRACE')
            if($before -cne $state.value){throw 'Initial environment state differs'}
            $caught=$false;$value=$null
            try{
                $value=Invoke-WorkerStartupTrace {
                    if([Environment]::GetEnvironmentVariable('LEA_SCRIPT_STARTUP_TRACE') -cne '1'){throw 'Trace not enabled'}
                    if($mode -eq 'throw'){throw 'Expected callback failure'}
                    if($mode -eq 'nested'){
                        $inner=Invoke-WorkerStartupTrace {'nested-result'}
                        if($inner -cne 'nested-result' -or [Environment]::GetEnvironmentVariable('LEA_SCRIPT_STARTUP_TRACE') -cne '1'){throw 'Nested trace scope differs'}
                    }
                    'callback-result'
                }
            }catch{if($_.Exception.Message -cne 'Expected callback failure'){throw};$caught=$true}
            if([Environment]::GetEnvironmentVariable('LEA_SCRIPT_STARTUP_TRACE') -cne $before -or ($mode -eq 'throw') -ne $caught -or (!$caught -and $value -cne 'callback-result')){throw 'Trace restoration or return control failed'}
            $rows.Add(@{name=($state.name+'-'+$mode);passed=$true})
        }
    }
}finally{
    if($null -eq $original){[Environment]::SetEnvironmentVariable('LEA_SCRIPT_STARTUP_TRACE',[NullString]::Value)}
    else{[Environment]::SetEnvironmentVariable('LEA_SCRIPT_STARTUP_TRACE',$original)}
}
if([Environment]::GetEnvironmentVariable('LEA_SCRIPT_STARTUP_TRACE') -cne $original){throw 'Outer environment restoration failed'}
$null=New-Item -ItemType Directory -Path $out
$result=@{passed=$true;controls=$rows;runtimeExecuted=$false;environmentRestored=([Environment]::GetEnvironmentVariable('LEA_SCRIPT_STARTUP_TRACE') -ceq $original);inputs=@(Get-FileHash $PSCommandPath,(Join-Path $PSScriptRoot 'Invoke-WorkerStartupTrace.ps1')|Select-Object Path,Hash)}
[IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 6))
@{passed=$true;controls=$rows.Count;environmentRestored=$result.environmentRestored}|ConvertTo-Json
