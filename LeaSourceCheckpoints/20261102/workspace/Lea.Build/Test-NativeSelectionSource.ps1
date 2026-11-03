$ErrorActionPreference='Stop'
$workspace=Split-Path $PSScriptRoot -Parent
$out=Join-Path $workspace 'continuity-evidence/native-selection-source-v1'
if(Test-Path $out){throw 'Preserve source controls'}
$baseline=Join-Path $workspace 'continuity-evidence/current-hit-geometry-v3/view-source/V.cs'
$fragmentPath=Join-Path $PSScriptRoot 'NativeSelection.fragment.cs'
$helper=Join-Path $PSScriptRoot 'New-NativeSelectionSource.ps1'
$leases=[Collections.Generic.List[IDisposable]]::new()
try{
    foreach($path in @($baseline,$fragmentPath,$helper,$PSCommandPath)){$leases.Add([IO.File]::Open($path,'Open','Read','Read'))}
    if((Get-FileHash $baseline).Hash -cne '1B61471C8962CC238BBE94AABBC3FEB4455EB42049CEAC02B1BABB9BF21F8396'){throw 'Mature view source differs'}
    . $helper
    $original=[IO.File]::ReadAllText($baseline);$fragment=[IO.File]::ReadAllText($fragmentPath)
    $candidate=New-NativeSelectionSource $original $fragment
    $rows=[Collections.Generic.List[object]]::new()
    if(!$candidate.inverseVerified){throw 'Inverse missing'}
    $rows.Add(@{name='exact-source-inverse';passed=$true})
    foreach($case in @(
        @{name='missing-anchor';text=$original.Replace('private bool ReadyEdit','private bool RenamedEdit')},
        @{name='duplicate-anchor';text=($original+[Environment]::NewLine+$original)},
        @{name='already-applied';text=$candidate.text}
    )){
        $rejected=$false;try{$null=New-NativeSelectionSource $case.text $fragment}catch{$rejected=$_.Exception.Message -ceq 'Native selection source anchor differs'}
        if(!$rejected){throw ('Source negative failed: '+$case.name)}
        $rows.Add(@{name=$case.name;passed=$true})
    }
    foreach($case in @(
        @{name='copy-permission-bypass';text=$fragment.Replace('!Ce("cp")','false')},
        @{name='full-editor-text-leak';text=$fragment.Replace('ed.SelectedText','ed.Text')},
        @{name='missing-edit-lock';text=$fragment.Replace('editLock || ','')},
        @{name='clipboard-side-effect';text=($fragment+'// Clipboard.SetText')}
    )){
        $rejected=$false;try{$null=New-NativeSelectionSource $original $case.text}catch{$rejected=$_.Exception.Message -ceq 'Native selection policy fragment differs'}
        if(!$rejected){throw ('Policy negative failed: '+$case.name)}
        $rows.Add(@{name=$case.name;passed=$true})
    }
    foreach($name in @('Microsoft.CodeAnalysis.dll','Microsoft.CodeAnalysis.CSharp.dll')){
        $null=[Reflection.Assembly]::LoadFrom((Join-Path $PSHOME $name))
    }
    $tree=[Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($candidate.text,[Microsoft.CodeAnalysis.CSharp.CSharpParseOptions]::Default,'V.cs',[Text.Encoding]::UTF8,[Threading.CancellationToken]::None)
    if(@($tree.GetDiagnostics([Threading.CancellationToken]::None)|Where-Object{$_.Severity.ToString() -eq 'Error'}).Count){throw 'Candidate C# syntax errors'}
    $rows.Add(@{name='candidate-csharp-syntax';passed=$true})
    $null=New-Item -ItemType Directory -Path $out
    [IO.File]::WriteAllText((Join-Path $out 'V.cs'),$candidate.text,[Text.UTF8Encoding]::new($false))
    foreach($path in @($fragmentPath,$helper,$PSCommandPath)){Copy-Item -LiteralPath $path -Destination $out}
    $pins=@($baseline,$fragmentPath,$helper,$PSCommandPath|ForEach-Object{Get-FileHash $_}|Select-Object Path,Hash)
    @{passed=$true;controls=$rows;inputs=$pins;candidateSha256=(Get-FileHash (Join-Path $out 'V.cs')).Hash;compiled=$false;runtimeExecuted=$false;scope='Source inverse, policy mutation rejection and C# syntax only; no privacy/runtime/full-view qualification';fullReleaseQualified=$false}|ConvertTo-Json -Depth 6|Set-Content (Join-Path $out 'result.json')
    @{passed=$true;controls=$rows.Count;compiled=$false;runtimeExecuted=$false}|ConvertTo-Json -Compress
}finally{foreach($s in $leases){$s.Dispose()}}
