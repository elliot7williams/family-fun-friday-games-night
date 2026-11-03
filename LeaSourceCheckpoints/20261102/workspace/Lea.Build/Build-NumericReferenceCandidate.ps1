param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$outRoot=Join-Path $workspace 'continuity-evidence/numeric-references-v3'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained foreground owner required'}
if(Test-Path -LiteralPath $outRoot){throw 'Preserve numeric reference candidate'}
$utility=Join-Path $workspace 'continuity-evidence/native-launch-timing-v1/Test-CompositeCandidate.v2.ps1'
if((Get-FileHash -LiteralPath $utility).Hash -cne 'EB00AD17B8078A6CC2F418733BA5D675FC84EF49EED433A527FAC5498DD5ECEF'){throw 'Utility changed'}
$t=$null;$e=$null;$ast=[Management.Automation.Language.Parser]::ParseFile($utility,[ref]$t,[ref]$e)
foreach($name in @('Space','NewOutputReader','PumpOutput','Run')){
    $defs=@($ast.FindAll({param($n)$n -is [Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -ceq $name},$false))
    if($e.Count -or $defs.Count -ne 1){throw 'Utility shape changed'}
    . ([scriptblock]::Create($defs[0].Extent.Text))
}
if(Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue){throw 'Fresh owner required'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new();$leases=[Collections.Generic.List[IDisposable]]::new()
function Hold([string]$Path,[string]$Hash=''){
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($stream)
    $actual=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))
    if($Hash -and $actual -cne $Hash){throw 'Numeric candidate input differs'}
    $inputs.Add(@{path=$Path;sha256=$actual});return $actual
}
$result=[ordered]@{passed=$false;candidate=(Join-Path $outRoot 'candidate');runs=$runs;inputs=$inputs;error=$null;browserInstalled=$false;releaseCertified=$false}
Space;$null=New-Item -ItemType Directory -Path $result.candidate
try{
    $base=Join-Path $workspace 'continuity-evidence/archived-native-build-v1'
    $null=Hold (Join-Path $base 'result.json') 'C72B87A88BB5D164EA178BBAD36329C120142AF432D9A206240B2DBCF36AFA23'
    $proof=Get-Content -LiteralPath (Join-Path $base 'result.json') -Raw|ConvertFrom-Json
    if(!$proof.passed -or !$proof.inputsUnchanged -or $proof.ownerAttention){throw 'Base compilation proof incomplete'}
    $known=@{};foreach($file in $proof.inputs){$known[$file.path]=$file.sha256}
    $edits=@(
        @('token.D ? WebUtility.HtmlDecode(token.X) : token.X','token.D ? Hc.D(token.X) : token.X'),
        @('node.A[name] = WebUtility.HtmlDecode(value);','node.A[name] = Hc.D(value);'),
        @('TextSpace(GeneratedRun(run) ? run.X ?? "" : WebUtility.HtmlDecode(run.X ?? ""), block.S)','TextSpace(run.X ?? "", block.S)'),
        @('var value = GeneratedRun(run) ? run.X ?? "" : WebUtility.HtmlDecode(run.X ?? "");','var value = run.X ?? "";'),
        @('Regex.Replace(WebUtility.HtmlDecode(text ?? ""),','Regex.Replace(text ?? "",')
    )
    $options=[Collections.Generic.List[string]]::new();$refs=[Collections.Generic.List[string]]::new();$sources=0
    foreach($line in Get-Content -LiteralPath (Join-Path $base 'Lea.Engine/compile.rsp')){
        if($line.StartsWith('/out:')){$options.Add('/out:"'+(Join-Path $result.candidate 'Lea.Engine.dll')+'"')}
        elseif($line -match '^"(.+\.cs)"$'){
            $from=$Matches[1];if(!$known.ContainsKey($from)){throw 'Unpinned engine source'};$null=Hold $from $known[$from]
            $to=Join-Path $outRoot ('source/'+[IO.Path]::GetFileName($from));$null=New-Item -ItemType Directory -Path (Split-Path $to) -Force
            if([IO.Path]::GetFileName($from) -ceq 'E.cs'){
                $original=[IO.File]::ReadAllText($from);$updated=$original
                foreach($edit in $edits){if([regex]::Matches($updated,[regex]::Escape($edit[0])).Count -ne 1){throw 'Ambiguous numeric overlay'};$updated=$updated.Replace($edit[0],$edit[1])}
                $inverse=$updated;for($i=$edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($edits[$i][1],$edits[$i][0])}
                if($inverse -cne $original){throw 'Numeric overlay inverse differs'}
                [IO.File]::WriteAllText($to,$updated,[Text.UTF8Encoding]::new($false));$result.overlayInverseVerified=$true
            }else{Copy-Item -LiteralPath $from -Destination $to}
            $null=Hold $to;$options.Add('"'+$to+'"');$sources++
        }elseif($line -match '^/reference:"([^"]+)"$'){
            $path=$Matches[1];if(!$known.ContainsKey($path)){throw 'Unpinned engine reference'};$null=Hold $path $known[$path]
            $options.Add($line);$refs.Add($line)
        }else{$options.Add($line)}
    }
    if($sources -ne 30){throw 'Engine source inventory differs'}
    $helper=Join-Path $workspace 'Lea.Engine/Hc.cs';$null=Hold $helper '3D90FEECFAD3B62C92C2F28CD37E3B68DEDD68314EA8B314076A90EB81082627'
    $copy=Join-Path $outRoot 'source/Hc.cs';Copy-Item -LiteralPath $helper -Destination $copy;$null=Hold $copy;$options.Add('"'+$copy+'"')
    $wrapper=Join-Path $PSScriptRoot 'Invoke-PublicCompiler.20261022.ps1';$null=Hold $wrapper '1A04EE9A191D522955C633F97A24C9C0921A5738F76D0DF36DB1C92A574EEA03'
    function Compile([string]$Name,$Options){
        $rsp=Join-Path $outRoot ($Name+'.rsp');[IO.File]::WriteAllLines($rsp,$Options);$hash=Hold $rsp
        $row=Run $Name (Get-Process -Id $PID).Path @('-NoProfile','-File',('"'+$wrapper+'"'),'-ResponseFile',('"'+$rsp+'"'),'-ExpectedResponseSha256',$hash,'-ExpectedSourceSha256','D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB') 120
        if(!$row.passed){throw 'Numeric candidate compilation failed'}
    }
    Compile 'compile-engine' $options
    $result.binary=Join-Path $result.candidate 'Lea.Engine.dll';$result.sha256=(Get-FileHash -LiteralPath $result.binary).Hash
    $baseline=Join-Path $outRoot 'baseline';$null=New-Item -ItemType Directory -Path $baseline
    $old=Join-Path $base 'Lea.Engine/Lea.Engine.dll';$null=Hold $old '6D89A8F5953F2E5A197C5D789D8D828BF8FC084836EB30F22EB00D228B0591F8'
    Copy-Item -LiteralPath $old -Destination (Join-Path $baseline 'Lea.Engine.dll')
    $test=Join-Path $workspace 'Lea.Tests/NumericReferences.cs';$null=Hold $test
    $testCopy=Join-Path $outRoot 'NumericReferences.cs';Copy-Item -LiteralPath $test -Destination $testCopy;$null=Hold $testCopy
    $probe=Join-Path $result.candidate 'NumericReferences.exe'
    $args=@('/nologo','/nostdlib+','/target:exe','/langversion:12.0','/optimize+',('/out:"'+$probe+'"'))+@($refs)+@('/reference:"'+$old+'"',('"'+$testCopy+'"'))
    Compile 'compile-probe' $args
    Copy-Item -LiteralPath $probe -Destination (Join-Path $baseline 'NumericReferences.exe')
    $fixtures=Join-Path $outRoot 'fixtures';$null=New-Item -ItemType Directory -Path $fixtures
    $a=Run 'baseline-numeric' (Join-Path $baseline 'NumericReferences.exe') @(('"'+$fixtures+'"')) 30
    $b=Run 'candidate-numeric' $probe @(('"'+$fixtures+'"')) 30
    $result.baselineRegressionReproduced=$a.exitCode -eq 1 -and !$a.timedOut -and !$a.killRequested -and $a.stdoutComplete -and $a.stderrComplete
    $result.candidateChecksPassed=$b.passed -and ([IO.File]::ReadAllText($b.stdout).Trim().EndsWith('Numeric references: checks=141; failures=0',[StringComparison]::Ordinal))
    $result.passed=$result.baselineRegressionReproduced -and $result.candidateChecksPassed
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object {(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -gt 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    if($result.ownerAttention){$global:LeaNumericInputLeases=$leases}else{foreach($lease in $leases){$lease.Dispose()}}
    $global:LeaNumericResult=$result
    [IO.File]::WriteAllText((Join-Path $outRoot 'result.json'),($result|ConvertTo-Json -Depth 7))
}
[pscustomobject]$result|Select-Object passed,sha256,baselineRegressionReproduced,candidateChecksPassed,ownerAttention,error|ConvertTo-Json
