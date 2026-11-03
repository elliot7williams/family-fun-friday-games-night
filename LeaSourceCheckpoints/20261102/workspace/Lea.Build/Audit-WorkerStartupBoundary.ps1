$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace 'continuity-evidence/worker-startup-boundary-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve boundary audit'}
$null=New-Item -ItemType Directory $out
$leases=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new()
function Hold([string]$path,[string]$expected){
    $s=[IO.File]::Open($path,'Open','Read','Read');$leases.Add($s)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))
    if($hash -cne $expected){throw 'Audit input differs'}
    $inputs.Add(@{path=$path;sha256=$hash})
}
try{
    $binary=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/current-native-script-suite-v2/runtime/ScriptWorker/Lea.Script.Worker.dll'
    Hold $binary '60FF26D8357FEA52C802BAC562C30C6CE28F72F735E78281D0908E94FAB530D6'
    $receipt=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/current-native-script-run-v3/result.json'
    Hold $receipt '23E8711EFA033D5F4E10F8FC4FF6313913460166F815CF998301CF04DFF71656'
    $reader=Join-Path $PSScriptRoot 'ReadFailureIL.cs';Hold $reader 'A27FC129FABD195808AA2564EB78CF275E8E0AF325D4D01EE6C8B95DC009F6B0'
    $helper=Join-Path $PSScriptRoot 'WorkerStartupIl.cs';Hold $helper (Get-FileHash $helper).Hash
    Add-Type -Path @($reader,$helper)
    $audit=[WorkerStartupIl]::Read($binary)
    $lines=@($audit.instructions)
    $positions=@(foreach($pattern in @('ldstr "evaluate"','ldstr "debugger"','callvirt add_BeforeEvaluate','ldstr "subscribed"','callvirt Evaluate')){
        $matches=@(0..($lines.Count-1)|Where-Object{$lines[$_].EndsWith($pattern,[StringComparison]::Ordinal)})
        if($matches.Count -ne 1){throw 'Startup boundary shape differs'}
        $matches[0]
    })
    for($i=1;$i -lt $positions.Count;$i++){if($positions[$i] -le $positions[$i-1]){throw 'Startup instruction order differs'}}
    $wrong=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/current-native-script-suite-v2/runtime/Lea.Engine.dll'
    Hold $wrong '52CE235E37B10243D618E4BDF0031E215145974526DF8F758E167C16E791F4BC'
    $rejected=$false
    try{$null=[WorkerStartupIl]::Read($wrong)}catch{$rejected=$_.Exception.InnerException.Message -ceq 'Expected one worker Program type.'}
    if(!$rejected){throw 'Wrong-assembly negative control failed'}
    [IO.File]::WriteAllLines((Join-Path $out 'startup-il.txt'),[string[]]$lines)
    $result=@{passed=$true;inputs=$inputs;methodToken=$audit.methodToken;ilBytes=$audit.ilBytes;boundaryInstructions=$audit.boundaryInstructions;wrongAssemblyRejected=$rejected;runtimeExecuted=$false;scope='Read-only exact worker IL. Evaluate trace precedes callback construction/debugger subscription and actual Evaluate call. Captured stderr is asynchronous, not a native stack or proof of where execution stalled.'}
    $result|ConvertTo-Json -Depth 6|Set-Content (Join-Path $out 'result.json')
    $result|Select-Object passed,methodToken,ilBytes,wrongAssemblyRejected,runtimeExecuted|ConvertTo-Json
}finally{foreach($s in $leases){$s.Dispose()}}
