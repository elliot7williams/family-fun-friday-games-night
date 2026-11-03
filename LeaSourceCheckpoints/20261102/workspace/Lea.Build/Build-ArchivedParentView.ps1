param([switch]$KeepForegroundHost)
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$outRoot=Join-Path $workspace 'continuity-evidence/archived-parent-view-build-v3'
if(!$KeepForegroundHost -or '-NoExit' -notin [Environment]::GetCommandLineArgs() -or [Console]::IsInputRedirected -or [Console]::IsOutputRedirected){throw 'Retained NoExit TTY required'}
if(Test-Path -LiteralPath $outRoot){throw 'Preserve previous attempt'}
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
$runs=[Collections.Generic.List[object]]::new()
$leases=[Collections.Generic.List[IDisposable]]::new()
$inputs=[Collections.Generic.List[object]]::new()
function Hold([string]$Path,[string]$Expected){
    $stream=[IO.File]::Open($Path,'Open','Read','Read');$leases.Add($stream)
    $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream))
    if($hash -cne $Expected){throw "Input mismatch: $Path"}
    $inputs.Add(@{path=$Path;sha256=$hash})
}
$result=[ordered]@{passed=$false;candidate=$outRoot;runs=$runs;inputs=$inputs;modules=[Collections.Generic.List[object]]::new();companionModules=[Collections.Generic.List[object]]::new();error=$null;scope='Relocated archived parent/view compilation only; binary equivalence and runtime NOT verified'}
Space;$null=New-Item -ItemType Directory -Path $outRoot
try{
    $archive=Join-Path $workspace 'continuity-evidence/tested-parent-view-source-v1.zip'
    Hold $archive 'F6B770D3AB46AFC1FD6D9DF4D65CD272FA365D6494B9B647DD0DED77A5F12599'
    $extracted=Join-Path $outRoot 'archive'
    [IO.Compression.ZipFile]::ExtractToDirectory($archive,$extracted)
    $manifest=Get-Content -LiteralPath (Join-Path $extracted 'manifest.json') -Raw|ConvertFrom-Json
    $wrapper=Join-Path $PSScriptRoot 'Invoke-PublicCompiler.20261022.ps1'
    Hold $wrapper '1A04EE9A191D522955C633F97A24C9C0921A5738F76D0DF36DB1C92A574EEA03'
    Hold (Join-Path $PSScriptRoot 'PublicCompiler.cs') 'D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB'
    foreach($module in $manifest.modules){
        if($module.module -cnotin @('parent','view')){throw 'Unknown module'}
        $directory=Join-Path $outRoot $module.module
        $null=New-Item -ItemType Directory -Path $directory
        $recipe=Join-Path $extracted ($module.module+'/original-compile.rsp')
        Hold $recipe $module.responseSha256
        $text=[IO.File]::ReadAllText($recipe)
        foreach($dependency in $module.dependencies){
            if($dependency.kind -ceq 'reference'){
                Hold $dependency.originalPath $dependency.sha256
                $target=Join-Path $outRoot ('references/'+$dependency.sha256+'/'+[IO.Path]::GetFileName($dependency.originalPath))
                if(!(Test-Path -LiteralPath $target)){
                    $null=New-Item -ItemType Directory -Path ([IO.Path]::GetDirectoryName($target)) -Force
                    Copy-Item -LiteralPath $dependency.originalPath -Destination $target
                }
                # Multi-module framework references need metadata-declared siblings.
                $inputStream=[IO.File]::OpenRead($dependency.originalPath)
                $pe=[Reflection.PortableExecutable.PEReader]::new($inputStream)
                try{
                    $metadata=[Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe)
                    foreach($handle in $metadata.AssemblyFiles){
                        $file=$metadata.GetAssemblyFile($handle)
                        if(!$file.ContainsMetadata){continue}
                        $name=$metadata.GetString($file.Name)
                        if([IO.Path]::GetFileName($name) -cne $name){throw 'Companion path escapes reference directory'}
                        if($metadata.GetAssemblyDefinition().HashAlgorithm -ne [Reflection.AssemblyHashAlgorithm]::Sha1){throw 'Unsupported companion hash algorithm'}
                        $original=Join-Path ([IO.Path]::GetDirectoryName($dependency.originalPath)) $name
                        $expected=[Convert]::ToHexString($metadata.GetBlobBytes($file.HashValue))
                        if((Get-FileHash -LiteralPath $original -Algorithm SHA1).Hash -cne $expected){throw 'Companion differs from pinned assembly metadata'}
                        $sha256=(Get-FileHash -LiteralPath $original).Hash;Hold $original $sha256
                        $companion=Join-Path ([IO.Path]::GetDirectoryName($target)) $name
                        if(!(Test-Path -LiteralPath $companion)){Copy-Item -LiteralPath $original -Destination $companion}
                        Hold $companion $sha256
                        $result.companionModules.Add(@{path=$companion;sha256=$sha256;declaredSha1=$expected;declaringAssemblySha256=$dependency.sha256})
                    }
                }finally{$pe.Dispose();$inputStream.Dispose()}
            }else{$target=Join-Path $extracted $dependency.archivePath}
            Hold $target $dependency.sha256
            $text=$text.Replace('"'+$dependency.originalPath+'"','"'+$target+'"')
        }
        $lines=[Collections.Generic.List[string]]::new();$outputs=0
        foreach($line in ($text -split '\r?\n')){
            if($line -match '^/out:"([^"]+)"$'){
                $binary=Join-Path $directory ([IO.Path]::GetFileName($Matches[1]))
                $lines.Add('/out:"'+$binary+'"');$outputs++
            }elseif($line -match '^/pdb:'){$lines.Add('/pdb:"'+(Join-Path $directory 'symbols.pdb')+'"')}
            else{$lines.Add($line)}
        }
        if($outputs -ne 1){throw 'Expected one output'}
        $rsp=Join-Path $directory 'compile.rsp'
        [IO.File]::WriteAllLines($rsp,$lines,[Text.UTF8Encoding]::new($false))
        $hash=(Get-FileHash -LiteralPath $rsp).Hash;Hold $rsp $hash
        $run=Run ('compile-'+$module.module) (Get-Process -Id $PID).Path @('-NoProfile','-File',('"'+$wrapper+'"'),'-ResponseFile',('"'+$rsp+'"'),'-ExpectedResponseSha256',$hash,'-ExpectedSourceSha256','D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB') 120
        if(!$run.passed){throw 'Archived compilation failed'}
        $result.modules.Add(@{module=$module.module;binary=$binary;sha256=(Get-FileHash -LiteralPath $binary).Hash;originalTestedSha256=$module.testedBinarySha256})
    }
    $result.passed=$result.modules.Count -eq 2
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -gt 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    $result.inputsUnchanged=@($inputs|Where-Object{(Get-FileHash -LiteralPath $_.path).Hash -cne $_.sha256}).Count -eq 0
    $result.passed=$result.passed -and !$result.ownerAttention -and $result.inputsUnchanged
    if(!$result.ownerAttention){foreach($lease in $leases){$lease.Dispose()}}
    $global:LeaArchivedBuildResult=$result
    [IO.File]::WriteAllText((Join-Path $outRoot 'result.json'),($result|ConvertTo-Json -Depth 7))
}
[pscustomobject]$result|Select-Object passed,ownerAttention,error|ConvertTo-Json
