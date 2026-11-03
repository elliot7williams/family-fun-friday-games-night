$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$out=Join-Path $workspace 'continuity-evidence/worker-launcher-rebuild-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve launcher build'}
if((Get-PSDrive C).Free -lt 512MB){throw 'Working reserve required'}
$leases=[Collections.Generic.List[IDisposable]]::new();$inputs=[Collections.Generic.List[object]]::new()
function Hold([string]$Path,[string]$Hash){
    $walk=[IO.Path]::GetFullPath($Path)
    while($walk){if((Get-Item -LiteralPath $walk -Force).Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Linked launcher input refused'};$walk=[IO.Path]::GetDirectoryName($walk)}
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($stream)
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) -cne $Hash){throw "Launcher input changed: $Path"}
    $inputs.Add(@{path=$Path;sha256=$Hash})
}
$result=@{passed=$false;error=$null;inputs=$inputs;workerExecuted=$false;browserChanged=$false;releaseCertified=$false}
$null=New-Item -ItemType Directory -Path $out
try{
    $tested=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/host-package-extracted-v1/extracted/runtime/ScriptWorker'
    $proof=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/host-package-extracted-v1/result.json'
    Hold $proof '0FBA544D311F22A84E046CFE99F97D381AB85D1FE936851CDDFB3833386823BD'
    $baseline=Get-Content -LiteralPath $proof -Raw|ConvertFrom-Json
    if(!$baseline.passed -or !$baseline.candidateBytesUnchanged -or $baseline.ownerAttention){throw 'Tested package proof incomplete'}
    $originalRecipe=Join-Path $workspace 'continuity-evidence/modern-worker-v1/Stage-AppHost.ps1'
    Hold $originalRecipe '2ECF790EBA420E1F97108B4296947C48ADE42A4B82759050FEB2887B3E17FCA5'
    Copy-Item -LiteralPath $originalRecipe -Destination (Join-Path $out 'original-launcher-recipe.ps1')
    $model='C:\Program Files\dotnet\sdk\8.0.426\Microsoft.NET.HostModel.dll'
    $template='C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Host.win-x64\8.0.32\runtimes\win-x64\native\apphost.exe'
    Hold $model 'F152FB21F0CF29398C9FB747A241D152315AA8E09BF85F6E4518D53A88DA3760'
    Hold $template '4CBDCC94704E8A8AFA4902C34225FEB4657ED5400A852F7327869BBECEEAA33C'
    foreach($spec in @(
        @('worker-archive-native-v2/result.json','41E53BFC9FDE20D00CD2398832DD4F397A862C7D6E8A198E90D35D1FC7AF1404'),
        @('interpreter-mapped-native-v1/result.json','CEB088149D4ED8C7103AFCD31C714E3245517E9C1DF8C5F1A486030EF0331F90'),
        @('parser-mapped-native-v1/result.json','81D8921C4B76A15A671DA06C547558D4E8123590E82603301467CCC7BE304999')
    )){
        $path=Join-Path $workspace ('continuity-evidence/'+$spec[0]);Hold $path $spec[1]
        $build=Get-Content -LiteralPath $path -Raw|ConvertFrom-Json
        if(!$build.passed -or !$build.inputsUnchanged -or $build.ownerAttention -or !$build.byteIdenticalToTestedNative){throw 'Native source chain incomplete'}
    }
    $runtime=Join-Path $out 'ScriptWorker';$null=New-Item -ItemType Directory -Path $runtime
    foreach($spec in @(
        @('worker-archive-native-v2/Lea.Script.Worker.dll','Lea.Script.Worker.dll','60FF26D8357FEA52C802BAC562C30C6CE28F72F735E78281D0908E94FAB530D6'),
        @('interpreter-mapped-native-v1/Jint.dll','Jint.dll','67F3E7D75EDADCBC0110FD6BB7AC26E56784D3650A83C2AE0AA23DD67EB03E6D'),
        @('parser-mapped-native-v1/Acornima.dll','Acornima.dll','CE23BFEFCDD287E0DE38CD818DF643287CD4C6CDCAACB20C46C379631D5C8821')
    )){
        $path=Join-Path $workspace ('continuity-evidence/'+$spec[0]);Hold $path $spec[2]
        Copy-Item -LiteralPath $path -Destination (Join-Path $runtime $spec[1])
    }
    foreach($spec in @(
        @('Lea.Script.Worker.runtimeconfig.json','658DE72465526C187EF01042E9411A867E3A4ADB7FAF5A8BA3FA8D5CFE1BABEA'),
        @('Lea.Script.Worker.deps.json','F0C2BC2BFC761A1866FF0471AE25BF0E634469382108C9496BE72E338ABFFD0D')
    )){
        $path=Join-Path $tested $spec[0];Hold $path $spec[1]
        $configSource=Join-Path $out ('configuration-source/'+$spec[0]);$null=New-Item -ItemType Directory -Path (Split-Path $configSource) -Force
        Copy-Item -LiteralPath $path -Destination $configSource
        Copy-Item -LiteralPath $configSource -Destination (Join-Path $runtime $spec[0])
    }
    $options=(Get-Content -LiteralPath (Join-Path $runtime 'Lea.Script.Worker.runtimeconfig.json') -Raw|ConvertFrom-Json).runtimeOptions
    if($options.rollForward -cne 'Disable' -or $options.framework.name -cne 'Microsoft.NETCore.App' -or $options.framework.version -cne '8.0.32'){throw 'Runtime policy differs'}
    $assembly=[Reflection.Assembly]::LoadFrom($model)
    $methods=@($assembly.GetType('Microsoft.NET.HostModel.AppHost.HostWriter').GetMethods([Reflection.BindingFlags]'Public,Static')|Where-Object Name -ceq CreateAppHost)
    if($methods.Count -ne 1 -or $methods[0].GetParameters().Count -ne 6){throw 'HostWriter API differs'}
    [Microsoft.NET.HostModel.AppHost.HostWriter]::CreateAppHost($template,(Join-Path $runtime 'Lea.Script.Worker.exe'),'Lea.Script.Worker.dll',$false,(Join-Path $runtime 'Lea.Script.Worker.dll'),$false)
    $files=[Collections.Generic.List[object]]::new()
    foreach($file in Get-ChildItem -LiteralPath $runtime -File){
        $original=Join-Path $tested $file.Name
        $expected=@($baseline.before|Where-Object path -ceq $original)
        if($expected.Count -ne 1){throw 'Unrecorded runtime member'}
        Hold $original $expected[0].sha256
        $actual=(Get-FileHash -LiteralPath $file.FullName).Hash
        $files.Add(@{name=$file.Name;sha256=$actual;byteIdentical=$actual -ceq $expected[0].sha256 -and [Convert]::ToBase64String([IO.File]::ReadAllBytes($original)) -ceq [Convert]::ToBase64String([IO.File]::ReadAllBytes($file.FullName))})
    }
    $result.files=$files;$result.passed=$files.Count -eq 6 -and @($files|Where-Object {!$_.byteIdentical}).Count -eq 0
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.passed=$result.passed -and $result.inputsUnchanged
    foreach($lease in $leases){$lease.Dispose()}
    [IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 7))
}
[pscustomobject]$result|Select-Object passed,files,inputsUnchanged,error|ConvertTo-Json -Depth 5
