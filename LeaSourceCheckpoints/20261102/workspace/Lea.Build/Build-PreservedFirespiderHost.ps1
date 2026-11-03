param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$outRoot=Join-Path $workspace 'continuity-evidence/firespider-source-rebuild-v1'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained foreground owner required'}
if(Test-Path -LiteralPath $outRoot){throw 'Preserve host rebuild'}
$utility=Join-Path $workspace 'continuity-evidence/native-launch-timing-v1/Test-CompositeCandidate.v2.ps1'
if((Get-FileHash -LiteralPath $utility).Hash -cne 'EB00AD17B8078A6CC2F418733BA5D675FC84EF49EED433A527FAC5498DD5ECEF'){throw 'Utility changed'}
$t=$null;$e=$null;$ast=[Management.Automation.Language.Parser]::ParseFile($utility,[ref]$t,[ref]$e)
foreach($name in @('Space','Pin','NewOutputReader','PumpOutput','Run')){
    $defs=@($ast.FindAll({param($n)$n -is [Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -ceq $name},$false))
    if($e.Count -or $defs.Count -ne 1){throw 'Utility shape changed'}
    . ([scriptblock]::Create($defs[0].Extent.Text))
}
if(Get-Variable LeaCompositeProcessLeases -Scope Global -ErrorAction SilentlyContinue){throw 'Fresh owner required'}
$global:LeaCompositeProcessLeases=[Collections.Generic.List[object]]::new()
$inputs=[Collections.Generic.List[object]]::new();$runs=[Collections.Generic.List[object]]::new()
$result=[ordered]@{passed=$false;candidate=$outRoot;inputs=$inputs;runs=$runs;error=$null;browserExecuted=$false;sourceModified=$false;installedBrowserChanged=$false;releaseCertified=$false}
Space;$null=New-Item -ItemType Directory -Path $outRoot
try{
    $proofPath=Join-Path $workspace 'continuity-evidence/small-engine-v1/extraction-result.json'
    $null=Pin $proofPath 'EFB784EE4B70B75EFC7D0B910D97AEB1D26ED61A8410071989ED978BF6544D6D'
    $proof=Get-Content -LiteralPath $proofPath -Raw|ConvertFrom-Json
    if(!$proof.completed -or $proof.hostSourceFiles.Count -ne 103){throw 'Host extraction proof incomplete'}
    $archive=@($proof.archives|Where-Object sha256 -ceq '1E6AA708C94F4D1E19B8EC832258F605E08310FEFAA646E01B12C894E27919F4')
    if($archive.Count -ne 1){throw 'Host source archive ambiguous'}
    $null=Pin $archive[0].path $archive[0].sha256
    $stage=Join-Path $outRoot 'source';[IO.Compression.ZipFile]::ExtractToDirectory($archive[0].path,$stage)
    $result.source=$stage
    foreach($file in @($proof.hostSourceFiles)+@($proof.hostSourceManifestFile)){
        $path=[IO.Path]::GetFullPath((Join-Path $stage $file.path))
        if(!$path.StartsWith($stage+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Host source path escape'}
        $null=Pin $path $file.sha256
    }
    $project=Join-Path $stage 'Firespider Internet.vbproj'
    $compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework/v4.0.30319/MSBuild.exe'
    if(!(Test-Path -LiteralPath $compiler)){throw 'Framework MSBuild missing'}
    $result.compiler=@{path=$compiler;sha256=(Get-FileHash -LiteralPath $compiler).Hash;version=(Get-Item -LiteralPath $compiler).VersionInfo.FileVersion}
    $output=Join-Path $outRoot 'bin'
    $run=Run 'build-host' $compiler @(('"'+$project+'"'),'/nologo','/t:Rebuild','/v:minimal','/p:Configuration=Release','/p:GenerateManifests=false','/p:AutoGenerateBindingRedirects=true',('/p:OutputPath="'+$output+'"')) 120
    $result.compilerPassed=$run.passed
    $result.output=$output
    $result.binaries=@(Get-ChildItem -LiteralPath $output -Filter '*.exe' -File -ErrorAction SilentlyContinue|ForEach-Object{@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash}})
    $result.passed=$run.passed -and @($result.binaries|Where-Object{[IO.Path]::GetFileName($_.path) -ceq 'Firespider Internet 19.exe'}).Count -eq 1
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.inputsUnchanged=@($inputs|Where-Object{(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -gt 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.passed=$result.passed -and $result.inputsUnchanged -and !$result.ownerAttention
    $global:LeaHostRebuildResult=$result
    [IO.File]::WriteAllText((Join-Path $outRoot 'result.json'),($result|ConvertTo-Json -Depth 7))
}
[pscustomobject]$result|Select-Object passed,compilerPassed,inputsUnchanged,ownerAttention,error|ConvertTo-Json
