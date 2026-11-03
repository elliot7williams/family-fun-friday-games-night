param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$outRoot=Join-Path $workspace 'continuity-evidence/ordered-flow-v2'
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
    $flowHelper=Join-Path $PSScriptRoot 'New-OrderedFlowSource.ps1';$null=Hold $flowHelper
    . $flowHelper
    $flowRoot=Join-Path $workspace 'Lea.Engine/E.cs';$null=Hold $flowRoot
    $flowText=[IO.File]::ReadAllText($flowRoot)
    $edits=@(
        @('token.D ? WebUtility.HtmlDecode(token.X) : token.X','token.D ? Hc.D(token.X) : token.X'),
        @('node.A[name] = WebUtility.HtmlDecode(value);','node.A[name] = Hc.D(value, true);'),
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
                $updated=New-OrderedFlowSource $updated $flowText
                [IO.File]::WriteAllText($to,$updated,[Text.UTF8Encoding]::new($false));$result.overlayInverseVerified=$true
            }elseif([IO.Path]::GetFileName($from) -ceq 'B.cs'){
                $original=[IO.File]::ReadAllText($from)
                $old='        public bool Hd { get; internal set; }'
                $new=$old+"`r`n        internal bool An { get; set; }"
                if([regex]::Matches($original,[regex]::Escape($old)).Count -ne 1){throw 'Anonymous block anchor differs'}
                $updated=$original.Replace($old,$new)
                if($updated.Replace($new,$old) -cne $original){throw 'Anonymous block inverse differs'}
                [IO.File]::WriteAllText($to,$updated,[Text.UTF8Encoding]::new($false))
            }else{Copy-Item -LiteralPath $from -Destination $to}
            $null=Hold $to;$options.Add('"'+$to+'"');$sources++
        }elseif($line -match '^/reference:"([^"]+)"$'){
            $path=$Matches[1];if(!$known.ContainsKey($path)){throw 'Unpinned engine reference'};$null=Hold $path $known[$path]
            $options.Add($line);$refs.Add($line)
        }else{$options.Add($line)}
    }
    if($sources -ne 30){throw 'Engine source inventory differs'}
    $helper=Join-Path $workspace 'Lea.Engine/Hc.cs';$null=Hold $helper 'BFE89C49863A0A50B527A16E0DF64857C4EA7CEC29EE9FD1CE441902048D3357'
    $copy=Join-Path $outRoot 'source/Hc.cs';Copy-Item -LiteralPath $helper -Destination $copy;$null=Hold $copy;$options.Add('"'+$copy+'"')
    foreach($name in @('Hn.cs','Hn.Data.cs')){
        $helper=Join-Path $workspace ('Lea.Engine/'+$name);$null=Hold $helper
        $copy=Join-Path $outRoot ('source/'+$name);Copy-Item -LiteralPath $helper -Destination $copy;$null=Hold $copy;$options.Add('"'+$copy+'"')
    }
    foreach($name in @('entities.whatwg.json','entities.expected.tsv')){
        $data=Join-Path $workspace ('Lea.Engine/Data/'+$name);$null=Hold $data
        $copy=Join-Path $outRoot $name;Copy-Item -LiteralPath $data -Destination $copy;$null=Hold $copy
    }
    $wrapper=Join-Path $PSScriptRoot 'Invoke-PublicCompiler.20261022.ps1';$null=Hold $wrapper '1A04EE9A191D522955C633F97A24C9C0921A5738F76D0DF36DB1C92A574EEA03'
    function Compile([string]$Name,$Options){
        $rsp=Join-Path $outRoot ($Name+'.rsp');[IO.File]::WriteAllLines($rsp,$Options);$hash=Hold $rsp
        $row=Run $Name (Get-Process -Id $PID).Path @('-NoProfile','-File',('"'+$wrapper+'"'),'-ResponseFile',('"'+$rsp+'"'),'-ExpectedResponseSha256',$hash,'-ExpectedSourceSha256','D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB') 120
        if(!$row.passed){throw 'Numeric candidate compilation failed'}
    }
    Compile 'compile-engine' $options
    $result.binary=Join-Path $result.candidate 'Lea.Engine.dll';$result.sha256=(Get-FileHash -LiteralPath $result.binary).Hash
    $baseline=Join-Path $outRoot 'baseline';$null=New-Item -ItemType Directory -Path $baseline
    $old=Join-Path $workspace 'continuity-evidence/named-references-v1/candidate/Lea.Engine.dll';$null=Hold $old '9208520EE6A067FF38D39368B2E50E6B099E5120C977A071E8D3E09733A3AF17'
    Copy-Item -LiteralPath $old -Destination (Join-Path $baseline 'Lea.Engine.dll')
    $test=Join-Path $workspace 'Lea.Tests/OrderedFlow.cs';$null=Hold $test
    $testCopy=Join-Path $outRoot 'OrderedFlow.cs';Copy-Item -LiteralPath $test -Destination $testCopy;$null=Hold $testCopy
    $probe=Join-Path $result.candidate 'OrderedFlow.exe'
    $args=@('/nologo','/nostdlib+','/target:exe','/langversion:12.0','/optimize+',('/out:"'+$probe+'"'))+@($refs)+@('/reference:"'+$old+'"',('"'+$testCopy+'"'))
    Compile 'compile-probe' $args
    Copy-Item -LiteralPath $probe -Destination (Join-Path $baseline 'OrderedFlow.exe')
    $fixtures=Join-Path $outRoot 'fixtures';$null=New-Item -ItemType Directory -Path $fixtures
    $a=Run 'baseline-numeric' (Join-Path $baseline 'OrderedFlow.exe') @(('"'+$fixtures+'"'),('"'+(Join-Path $outRoot 'entities.expected.tsv')+'"')) 60
    $b=Run 'candidate-numeric' $probe @(('"'+$fixtures+'"'),('"'+(Join-Path $outRoot 'entities.expected.tsv')+'"')) 60
    $result.baselineRegressionReproduced=$a.exitCode -eq 1 -and !$a.timedOut -and !$a.killRequested -and $a.stdoutComplete -and $a.stderrComplete
    $result.candidateChecksPassed=$b.passed -and ([IO.File]::ReadAllText($b.stdout).Trim().EndsWith('Ordered flow: checks=36; failures=0',[StringComparison]::Ordinal))
    $numeric=Join-Path $workspace 'Lea.Tests/NumericReferences.cs';$null=Hold $numeric
    $numericCopy=Join-Path $outRoot 'NumericReferences.cs';Copy-Item -LiteralPath $numeric -Destination $numericCopy;$null=Hold $numericCopy
    $numericProbe=Join-Path $result.candidate 'NumericReferences.exe'
    Compile 'compile-numeric-regression' (@('/nologo','/nostdlib+','/target:exe','/langversion:12.0','/optimize+',('/out:"'+$numericProbe+'"'))+@($refs)+@('/reference:"'+$old+'"',('"'+$numericCopy+'"')))
    $numericRun=Run 'numeric-regression' $numericProbe @(('"'+$fixtures+'"')) 30
    $result.numericRegressionPassed=$numericRun.passed -and ([IO.File]::ReadAllText($numericRun.stdout).Trim().EndsWith('Numeric references: checks=141; failures=0',[StringComparison]::Ordinal))
    $named=Join-Path $workspace 'Lea.Tests/NamedReferences.cs';$null=Hold $named
    $namedCopy=Join-Path $outRoot 'NamedReferences.cs';Copy-Item -LiteralPath $named -Destination $namedCopy;$null=Hold $namedCopy
    $namedProbe=Join-Path $result.candidate 'NamedReferences.exe'
    Compile 'compile-named-regression' (@('/nologo','/nostdlib+','/target:exe','/langversion:12.0','/optimize+',('/out:"'+$namedProbe+'"'))+@($refs)+@('/reference:"'+$old+'"',('"'+$namedCopy+'"')))
    $namedRun=Run 'named-regression' $namedProbe @(('"'+$fixtures+'"'),('"'+(Join-Path $outRoot 'entities.expected.tsv')+'"')) 60
    $result.namedRegressionPassed=$namedRun.passed -and ([IO.File]::ReadAllText($namedRun.stdout).Trim().EndsWith('Named references: checks=5119; failures=0',[StringComparison]::Ordinal))
    $result.passed=$result.baselineRegressionReproduced -and $result.candidateChecksPassed -and $result.numericRegressionPassed -and $result.namedRegressionPassed
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
