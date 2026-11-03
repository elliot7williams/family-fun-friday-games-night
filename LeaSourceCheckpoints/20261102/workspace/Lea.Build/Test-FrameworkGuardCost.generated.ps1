param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$outRoot=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/framework-guard-cost-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained foreground owner required'}
if(Test-Path -LiteralPath $outRoot){throw 'Preserve calibration'}
$utility=Join-Path $workspace 'continuity-evidence/native-launch-timing-v1/Test-CompositeCandidate.v2.ps1'
if((Get-FileHash -LiteralPath $utility).Hash -cne 'EB00AD17B8078A6CC2F418733BA5D675FC84EF49EED433A527FAC5498DD5ECEF'){throw 'Utility changed'}
$t=$null;$e=$null;$ast=[Management.Automation.Language.Parser]::ParseFile($utility,[ref]$t,[ref]$e)
foreach($name in @('Space','NewOutputReader','PumpOutput','Run')){
    $defs=@($ast.FindAll({param($n)$n -is [Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -ceq $name},$false))
    if($e.Count -or $defs.Count -ne 1){throw 'Utility shape differs'}
    . ([scriptblock]::Create($defs[0].Extent.Text))
}
if(Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue){throw 'Fresh owner required'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$runs=[Collections.Generic.List[object]]::new();$inputs=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new()
function Hold([string]$Path,[string]$Hash){
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($stream)
    if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) -cne $Hash){throw 'Counter calibration input differs'}
    $inputs.Add(@{path=$Path;sha256=$Hash})
}
$result=[ordered]@{passed=$false;candidate=(Join-Path $outRoot 'runtime');inputs=$inputs;runs=$runs;interpreterExecuted=$false;releaseQualified=$false;error=$null}
Space;$null=New-Item -ItemType Directory -Path $result.candidate
try{
    $base=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/numeric-references-script-suite-v1'
    Hold (Join-Path $base 'result.json') '8992F34CA977FA5DF8F65501E8495342F53D5538EC524682FC826B6B4C192071'
    $build=Get-Content -LiteralPath (Join-Path $base 'result.json') -Raw|ConvertFrom-Json
    if(!$build.passed -or !$build.inputsUnchanged -or $build.ownerAttention){throw 'Reference proof incomplete'}
    $known=@{};foreach($input in $build.inputs){$known[$input.path]=$input.sha256}
    $binary=Join-Path $result.candidate 'CounterCalibration.exe'
    $options=[Collections.Generic.List[string]]::new()
    foreach($flag in @('/nologo','/nostdlib+','/target:exe','/langversion:12.0','/optimize+',('/out:"'+$binary+'"'))){$options.Add($flag)}
    $refs=[Collections.Generic.HashSet[string]]::new()
    foreach($line in Get-Content -LiteralPath (Join-Path $base 'tests.rsp')){
        if($line -match '^/reference:"([^"]+)"$'){
            $path=$Matches[1]
            if([IO.Path]::GetFileName($path) -cin @('mscorlib.dll','System.dll','System.Core.dll','System.Web.Extensions.dll') -and $refs.Add($path)){
                if(!$known.ContainsKey($path)){throw 'Unpinned framework reference'}
                Hold $path $known[$path];$options.Add($line)
            }
        }
    }
    if($refs.Count -ne 4){throw 'Reference inventory differs'}
    foreach($pair in @(@('NativeProcessSample.cs','E5C089C8E91CFBA9B7A56FBB27ACF57DB67CDCF723641EF7884A9563BA01B3FF'),@('FrameworkGuardCost.cs','D4199497A770CCFEEE14639744A2625A5D265C2E2F9306B1C69AB15715B6D349'))){
        $source=Join-Path $PSScriptRoot $pair[0];Hold $source $pair[1]
        $copy=Join-Path $outRoot $pair[0];Copy-Item -LiteralPath $source -Destination $copy;Hold $copy $pair[1];$options.Add('"'+$copy+'"')
    }
    $rsp=Join-Path $outRoot 'compile.rsp';[IO.File]::WriteAllLines($rsp,$options);$hash=(Get-FileHash -LiteralPath $rsp).Hash;Hold $rsp $hash
    $wrapper=Join-Path $PSScriptRoot 'Invoke-PublicCompiler.20261022.ps1';Hold $wrapper '1A04EE9A191D522955C633F97A24C9C0921A5738F76D0DF36DB1C92A574EEA03'
    $compile=Run 'compile-calibration' (Get-Process -Id $PID).Path @('-NoProfile','-File',('"'+$wrapper+'"'),'-ResponseFile',('"'+$rsp+'"'),'-ExpectedResponseSha256',$hash,'-ExpectedSourceSha256','D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB') 120
    if(!$compile.passed){throw 'Calibration compilation failed'}
    $fixtures=Join-Path $outRoot 'fixtures';$null=New-Item -ItemType Directory -Path $fixtures
    $probe=Run 'counter-calibration' $binary @(('"'+$fixtures+'"')) 30
    $report=Get-Content -LiteralPath $probe.stdout -Raw|ConvertFrom-Json
    $result.observation=$report;$result.passed=$probe.passed -and $report.passed
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -gt 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if($result.ownerAttention){$global:LeaCounterInputLeases=$leases}else{foreach($lease in $leases){$lease.Dispose()}}
    $global:LeaCounterResult=$result
    [IO.File]::WriteAllText((Join-Path $outRoot 'result.json'),($result|ConvertTo-Json -Depth 8))
}
[pscustomobject]$result|Select-Object passed,inputsUnchanged,ownerAttention,error|ConvertTo-Json
