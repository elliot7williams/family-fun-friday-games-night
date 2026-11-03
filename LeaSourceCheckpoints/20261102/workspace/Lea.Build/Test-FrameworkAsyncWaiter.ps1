$ErrorActionPreference='Stop'
if($PSVersionTable.PSEdition -cne 'Desktop'){throw 'Run in Windows PowerShell/.NET Framework'}
$flags=[Reflection.BindingFlags]'Instance,NonPublic'
$sync=[Threading.SemaphoreSlim].GetField('m_waitCount',$flags)
$async=[Threading.SemaphoreSlim].GetField('m_asyncHead',$flags)
if(!$sync -or !$async){throw 'Framework semaphore layout changed'}
$gate=[Threading.SemaphoreSlim]::new(0,1)
$cancel=[Threading.CancellationTokenSource]::new()
try{
    $task=$gate.WaitAsync($cancel.Token)
    if($task.IsCompleted -or [int]$sync.GetValue($gate) -ne 0 -or $null -eq $async.GetValue($gate)){throw 'Async queue accounting differs'}
    $cancel.Cancel()
    try{$task.GetAwaiter().GetResult();throw 'Expected cancellation'}catch [System.Management.Automation.MethodInvocationException]{if($_.Exception.InnerException -isnot [OperationCanceledException]){throw}}
    if(!$task.IsCanceled -or $gate.CurrentCount -ne 0 -or $null -ne $async.GetValue($gate)){throw 'Canceled waiter altered owner permit or remained queued'}
    $null=$gate.Release()
    if($gate.CurrentCount -ne 1){throw 'Owner permit not restored'}
    'PASS Framework async waiter: pending async node; zero sync waiters; cancellation removes node without releasing permit; owner release restores permit.'
}finally{$cancel.Cancel();$cancel.Dispose();$gate.Dispose()}
