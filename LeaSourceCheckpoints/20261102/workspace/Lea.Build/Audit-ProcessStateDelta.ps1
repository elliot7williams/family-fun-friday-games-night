$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $workspace 'continuity-evidence/process-state-parent-build-v1'
$out=Join-Path $root 'scoped-method-delta.json'
if(Test-Path -LiteralPath $out){throw 'Preserve audit'}
$baseline=Join-Path $workspace 'continuity-evidence/archived-parent-view-build-v3/parent/Lea.Script.dll'
$candidate=Join-Path $root 'parent/Lea.Script.dll'
$resolved=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/TemporalResolvedIl.cs'
$instructions=Join-Path $workspace 'continuity-evidence/temporal-startup-v1/TemporalInstructionShape.cs'
foreach($pin in @(@($baseline,'3AA3B8E81372738DA30A75C538A81266EED0F24E19A4EF7A8540B331A811A8AE'),@($candidate,'D5B66F9E05D422040A49EC3EE3F16B99C9731EB036D6369B3C758EEE5BBD2916'),@($resolved,'37FE2EBD678C512E888DF2A7381588F01D2B8284B7AE29C27D472536D956524F'),@($instructions,'ABE29EB2A31DF368EF8A8A887D83F54910E9E9DACFEC5B2DFE3A241737FCDAEB'))){
    if((Get-FileHash -LiteralPath $pin[0]).Hash -cne $pin[1]){throw 'Changed audit input'}
}
$helper=Join-Path $PSScriptRoot 'DiagnosticDeltaAudit.cs'
$pins=@($baseline,$candidate,$resolved,$instructions,$helper|ForEach-Object{Get-FileHash -LiteralPath $_})
$refs=@(Get-ChildItem (Join-Path $PSHOME 'ref') -Filter '*.dll'|ForEach-Object FullName)
Add-Type -Path @($instructions,$resolved,$helper) -ReferencedAssemblies $refs -CompilerOptions '/nowarn:1701'
$tests=[TemporalResolvedIl]::SelfTest($baseline)
$self=[DiagnosticDeltaAudit]::Compare($baseline,$baseline)
if($self.added.Count -or $self.removed.Count -or $self.instructionChanges.Count -or $self.headerLocalExceptionChanges.Count -or !$self.fieldsIdentical){throw 'Self comparison failed'}
$delta=[DiagnosticDeltaAudit]::Compare($baseline,$candidate)
$scoped=[DiagnosticDeltaAudit]::CompareKnownClosures($baseline,$candidate)
$expectedChanged='method:self:Lea.Script/Jp+/<Start>d__99::MoveNext:32:0:0:Void()'
$expectedAdded=@(
    'method:self:Lea.Win/Rp::GetExitCodeProcess:0:0:2:Boolean(18:assembly:System:4.0.0.0::0:B77A5C561934E089/Microsoft.Win32.SafeHandles/SafeProcessHandle,UInt32&)',
    'method:self:Lea.Win/Rp::ProcessState:0:0:1:String(18:assembly:System:4.0.0.0::0:B77A5C561934E089/Microsoft.Win32.SafeHandles/SafeProcessHandle)',
    'method:self:Lea.Win/Rp::get_Ps:32:0:0:String()'
)
$passed=$scoped.added.Count -eq 3 -and @(Compare-Object $expectedAdded $scoped.added -CaseSensitive).Count -eq 0 -and
    $scoped.removed.Count -eq 0 -and $scoped.methodMetadataChanges.Count -eq 0 -and $scoped.bodyPresenceChanges.Count -eq 0 -and
    $scoped.fieldsIdentical -and $scoped.instructionChanges.Count -eq 1 -and $scoped.instructionChanges[0] -ceq $expectedChanged -and
    $scoped.headerLocalExceptionChanges.Count -eq 1 -and $scoped.headerLocalExceptionChanges[0] -ceq $expectedChanged
foreach($pin in $pins){if((Get-FileHash -LiteralPath $pin.Path).Hash -cne $pin.Hash){throw 'Audit input changed'}}
[IO.File]::WriteAllText($out,(@{scopedMethodDeltaPassed=$passed;rawDelta=$delta;scopedDelta=$scoped;exactClosureMap=@('Rp/DisplayClass92_0->96_0','Rp/DisplayClass95_0->99_0');decoderNegativeChecks=$tests;selfComparisonPassed=$true;inputs=$pins;releaseCertified=$false}|ConvertTo-Json -Depth 7))
$scoped|ConvertTo-Json -Depth 5
if(!$passed){throw 'Scoped method delta differs; retain raw comparison'}
