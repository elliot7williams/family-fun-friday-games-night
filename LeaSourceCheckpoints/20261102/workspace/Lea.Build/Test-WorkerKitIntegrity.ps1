$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$out=Join-Path $workspace 'continuity-evidence/worker-kit-integrity-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve kit integrity tests'}
$null=New-Item -ItemType Directory -Path $out
$kit=Join-Path $workspace 'continuity-evidence/worker-kit-relocation-v1/relocated-kit'
$builder=Join-Path $kit 'Build.ps1'
if((Get-FileHash -LiteralPath $builder).Hash -cne 'FBDE12C067FE4E3646A97121CD7957B406DA5381B433E930168BEDEA5A747338'){throw 'Kit builder differs'}
$manifest=Join-Path $kit 'manifest.json'
if((Get-FileHash -LiteralPath $manifest).Hash -cne '64853AE5B43313FF5DE9EE394C69F6DFC8154415FE599F193C990D2378E46E16'){throw 'Kit manifest differs'}
$map=Get-Content -LiteralPath $manifest -Raw|ConvertFrom-Json
$expected=[Collections.Generic.List[string]]::new()
foreach($option in $map.options){$expected.Add($option)}
$emit=Join-Path $workspace 'continuity-evidence/worker-kit-relocation-v1/emit'
$expected.Add('/out:"'+(Join-Path $emit 'Lea.Script.Worker.dll')+'"')
$expected.Add('/resource:"'+(Join-Path $kit $map.resource)+'",Lea.Script.Dom.js')
$known=@{};foreach($file in $map.files){$known[$file.path]=$file.sha256}
foreach($reference in $map.references){if(!$known.ContainsKey($reference)){throw 'Unhashed reference'};$expected.Add('/reference:"'+(Join-Path $kit $reference)+'"')}
foreach($source in $map.sources){if(!$known.ContainsKey($source)){throw 'Unhashed source'};$expected.Add('"'+(Join-Path $kit $source)+'"')}
if(!$known.ContainsKey($map.resource)){throw 'Unhashed resource'}
$actual=[IO.File]::ReadAllLines((Join-Path $emit 'compile.rsp'))
if($actual.Count -ne 194 -or $actual.Count -ne $expected.Count){throw 'Unexpected compiler response inventory'}
for($i=0;$i -lt $actual.Count;$i++){if($actual[$i] -cne $expected[$i]){throw 'Compiler response escaped the kit mapping'}}
$checks=[Collections.Generic.List[object]]::new()
$checks.Add(@{case='all-194-response-lines';passed=$true;targetInputsInsideKit=183;hashed=$true})
foreach($case in @('manifest-hash','input-hash','escape','existing-output')){
    $testRoot=$kit;$digest='64853AE5B43313FF5DE9EE394C69F6DFC8154415FE599F193C990D2378E46E16'
    $destination=Join-Path $out ($case+'-output')
    if($case -eq 'manifest-hash'){$digest='0'*64}
    if($case -in @('input-hash','escape')){
        $testRoot=Join-Path $out ($case+'-kit');$null=New-Item -ItemType Directory -Path $testRoot
        $relative=if($case -eq 'escape'){'../escape.txt'}else{'altered.cs'}
        if($case -eq 'input-hash'){[IO.File]::WriteAllText((Join-Path $testRoot $relative),'altered fixture')}
        $fixture=@{format=1;sources=@(1..17);references=@(1..165);files=@(@{path=$relative;sha256='0'*64})}
        $path=Join-Path $testRoot 'manifest.json';[IO.File]::WriteAllText($path,($fixture|ConvertTo-Json -Depth 5));$digest=(Get-FileHash -LiteralPath $path).Hash
    }
    if($case -eq 'existing-output'){$null=New-Item -ItemType Directory -Path $destination}
    $message=$null
    try{& $builder -KitRoot $testRoot -OutputDirectory $destination -ExpectedManifestSha256 $digest|Out-Null}catch{$message=$_.Exception.Message}
    $pattern=switch($case){'manifest-hash'{'Kit input identity differs:*'}'input-hash'{'Kit input identity differs:*'}'escape'{'Kit input escapes root'}'existing-output'{'Build output must be new'}}
    $checks.Add(@{case=$case;passed=$message -like $pattern -and ($case -eq 'existing-output' -or !(Test-Path -LiteralPath $destination));error=$message})
}
$result=@{passed=@($checks|Where-Object {!$_.passed}).Count -eq 0;checks=$checks;compilerExecuted=$false;runtimeExecuted=$false;scope='Relocated compiler response closure plus rejection before compiler load; not clean-PC certification'}
[IO.File]::WriteAllText((Join-Path $out 'result.json'),($result|ConvertTo-Json -Depth 5))
[pscustomobject]$result|ConvertTo-Json -Depth 5
