param([Parameter(Mandatory=$true)][ValidatePattern('^[A-F0-9]{64}$')][string]$ResultSha256,[switch]$Workflows)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$storage='I:\LeaBuildStorage'
$kind=if($Workflows){'workflow'}else{'smoke'}
$out=Join-Path $storage ('guarded-sdk-host-'+$kind+'-audit-20261110-v1')
if(Test-Path -LiteralPath $out){throw 'Preserve campaign audit'}
function Proof([string]$Path,[string]$Hash){if((Get-FileHash -LiteralPath $Path).Hash -cne $Hash){throw ('Proof differs: '+$Path)};Get-Content -LiteralPath $Path -Raw|ConvertFrom-Json}
$buildPath=Join-Path $storage 'guarded-sdk-host-build-20261110-v1/result.json'
$b=Proof $buildPath '1399CDED2BB6634A2B339F3B1BDC9FEFC68580F840BD2A866AAAE968A3BC83A2'
$manifestPath=Join-Path $storage 'guarded-sdk-host-runtime-20261110-v1/runtime-manifest.json'
$sourcePath=Join-Path $storage ('guarded-sdk-host-'+$kind+'-source-20261110-v1/source.json')
$folder=Join-Path $root $(if($Workflows){'continuity-evidence/guarded-sdk-host-workflows-v1'}else{'continuity-evidence/guarded-sdk-host-smokes-v1'})
$r=Proof (Join-Path $folder 'result.json') $ResultSha256
$m=Get-Content -LiteralPath $manifestPath -Raw|ConvertFrom-Json
$s=Get-Content -LiteralPath $sourcePath -Raw|ConvertFrom-Json
if(!$b.passed -or !$b.guardedSdkProductsVerified -or !$b.inputsUnchanged -or $b.ownerAttention -or $b.products.Count -ne 22 -or $b.hostSha256 -cne '1E390343125011CAA3F78582E91FAFC036E07A31F7E81B4B727ED8503FC39606'){throw 'Guarded host build differs'}
if(!$m.verified -or !$m.currentSourceSdk -or $m.files.Count -ne 400 -or $m.buildOutputsBound -ne 22 -or $m.hostSha256 -cne $b.hostSha256 -or !$s.verified -or !$s.inverseVerified -or $s.assertionsChanged -or $s.manifestSha256 -cne (Get-FileHash -LiteralPath $manifestPath).Hash){throw 'Composed runtime/projection differs'}
$count=if($Workflows){15}else{3}
if(!$r.passed -or !$r.completed -or !$r.inputsUnchanged -or $r.ownerAttention -or !$r.decoderPolicyAccepted -or $r.before.Count -ne 400 -or $r.runs.Count -ne $count -or $r.byteChanges.Count -or $r.aclChanges.Count -or $s.requiredCases -ne $count -or [bool]$s.workflows -ne [bool]$Workflows){throw 'Campaign incomplete'}
foreach($state in @($b,$m,$r)){foreach($f in $state.inputs){if((Get-FileHash -LiteralPath $f.path).Hash -cne $f.sha256){throw 'Input drift'}}}
foreach($f in $b.products){if((Get-FileHash -LiteralPath $f.path).Hash -cne $f.sha256){throw 'Build product drift'}}
foreach($f in $m.files){if((Get-FileHash -LiteralPath (Join-Path (Split-Path $manifestPath -Parent) ('runtime/'+$f.path))).Hash -cne $f.sha256){throw 'Composed runtime drift'}}
foreach($f in $r.before){if((Get-FileHash -LiteralPath $f.path).Hash -cne $f.sha256 -or (Get-Acl -LiteralPath $f.path).Sddl -cne $f.sddl){throw 'Runtime byte/ACL drift'}}
$template=Join-Path $PSScriptRoot $(if($Workflows){'Test-PrivateBindingHostWorkflows-v1.generated.ps1'}else{'Test-PrivateBindingHostSmokes-v1.generated.ps1'})
$templateHash=if($Workflows){'AE4F6F346E736C28D60291FEBF0EA578D2CEF331CAC2260294F5BEA37564BFE0'}else{'0AC6D53BA9C051AA359805F8C5938D269E1F69960FEEA9CB3D8A26BA9BD24D7C'}
if((Get-FileHash -LiteralPath $template).Hash -cne $templateHash -or $s.templateSha256 -cne $templateHash -or (Get-FileHash -LiteralPath $s.source).Hash -cne $s.sha256){throw 'Campaign source drift'}
$inverse=[IO.File]::ReadAllText($s.source).Replace(("'"+$PSScriptRoot.Replace("'","''")+"'"),'$PSScriptRoot')
for($i=$s.replacements.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($s.replacements[$i][1],$s.replacements[$i][0])}
if($inverse -cne [IO.File]::ReadAllText($template)){throw 'Original assertions changed'}
$budgets=@{'compile-accessibility-host'=120000;'settings-path'=30000}
if($Workflows){foreach($n in @('clean','lea','navigation','second','split','edit','style-start','script')){$budgets.Add('host-'+$n,90000)};foreach($n in @('clean','restored','locked','oversized','lazy-background','lazy-all')){$budgets.Add('exe-'+$n,100000)}}else{$budgets.Add('navigation',90000);$budgets.Add('exe-clean',100000)}
$seen=[Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach($run in @($b.runs)+@($r.runs)){
 if(!$seen.Add($run.name) -or !$budgets.ContainsKey($run.name) -or !$run.passed -or $run.exitCode -ne 0 -or !$run.originalHandleCaptured -or !$run.ownedJob -or !$run.exitObserved -or !$run.childRetirementIndependentlyAudited -or !$run.outputDrained -or !$run.stdoutComplete -or !$run.stderrComplete -or $run.activeAfterRetirement -ne 0 -or $run.timedOut -or $run.killRequested -or $run.unsafeToContinue -or $run.retainedForOwner -or $run.captureOverflow -or $run.error -or $run.parentBudgetMs -ne $budgets[$run.name] -or $run.sharedRetirementDrainBudgetMs -ne 5000 -or $run.retirementAndDrainElapsedMs -gt 5000){throw 'Owned process/deadline contract differs'}
}
if($Workflows){foreach($run in $r.runs|Where-Object {$_.name.StartsWith('host-')}){if(!$run.exactFooterVerified -or @(Get-Content -LiteralPath $run.stdout|Where-Object {$_.StartsWith('PASS Lea boot: '+$run.name.Substring(5)+';')}).Count -ne 1){throw 'Host workflow footer differs'}}}
elseif(($r.runs.name -join ',') -cne 'settings-path,navigation,exe-clean' -or @(Get-Content -LiteralPath $r.runs[1].stdout|Where-Object {$_.StartsWith('PASS Lea boot: navigation;')}).Count -ne 1){throw 'Smoke order/footer differs'}
$scenarios=if($Workflows){@('clean','restored','locked','oversized','lazy-background','lazy-all')}else{@('clean')}
foreach($name in $scenarios){
 $case=Get-Content -LiteralPath (Join-Path $folder ($name+'.json')) -Raw|ConvertFrom-Json
 if(!$case.Passed -or !$case.Responsive -or $case.LegacyEngine -or $case.ForcedCleanup -or $case.CleanupErrors.Count -or $case.Scenario -cne $name -or $case.SourceConfigSHA256 -cne (Get-FileHash -LiteralPath (Join-Path $r.runtime 'Firespider Internet 19.exe.config')).Hash){throw 'Actual EXE readiness/cleanup differs'}
 if($name -ceq 'lazy-all'){if($case.Lea -or !$case.DeferredSelection -or !$case.Resumed -or $case.Tabs -ne 2){throw 'Deferred/Resume contract differs'}}elseif(!$case.Lea -or $case.DeferredSelection -or $case.Resumed){throw 'Immediate Lea contract differs'}
}
New-Item -ItemType Directory -Path $out|Out-Null
$copies=@(foreach($f in Get-ChildItem -LiteralPath $folder -File){$to=Join-Path $out $f.Name;Copy-Item -LiteralPath $f.FullName -Destination $to;$hash=(Get-FileHash -LiteralPath $f.FullName).Hash;if((Get-FileHash -LiteralPath $to).Hash -cne $hash){throw 'Evidence copy differs'};@{path=$to;sha256=$hash}})
@{verified=$true;hostSha256=$b.hostSha256;resultSha256=$ResultSha256;manifestSha256=(Get-FileHash -LiteralPath $manifestPath).Hash;sourceSha256=(Get-FileHash -LiteralPath $sourcePath).Hash;cases=$count;ownedRuns=($count+1);runtimeFiles=400;bytesAndAclStable=$true;assertionsChanged=$false;actualExeResponsive=$true;legacyEngineLoaded=$false;forcedCleanup=$false;all15WorkflowsQualified=[bool]$Workflows;printQualified=$false;startupReliabilityFixed=$false;releaseQualified=$false;hostAdopted=$false;copies=$copies;scriptSha256=(Get-FileHash -LiteralPath $PSCommandPath).Hash}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $out 'audit.json') -Encoding utf8
Get-FileHash -LiteralPath (Join-Path $out 'audit.json')
