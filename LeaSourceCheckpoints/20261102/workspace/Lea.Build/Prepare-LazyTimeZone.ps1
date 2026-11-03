$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$from=Join-Path $workspace 'continuity-evidence/modern-worker-v1/interpreter-source-v1/source/Jint/Native/Temporal/DefaultTimeZoneProvider.cs'
$out=Join-Path $workspace 'continuity-evidence/temporal-startup-v1'
if(Test-Path -LiteralPath $out){throw 'Preserve prior overlay'}
$expected='DADF08FE2B0DAB62B1CAD61E11FAA021D5E82CDE5F4B50EA13890ABC0FFF288F'
$lease=[IO.File]::Open($from,'Open','Read','Read')
try{
    if((Get-FileHash $from).Hash -cne $expected){throw 'Pinned time-zone source differs'}
    $before=[IO.File]::ReadAllText($from);$after=$before
    $pairs=@(
        @{old='private static readonly Dictionary<string, string> IanaCanonicalCasing = BuildCanonicalCasingDict();';new='private static readonly Lazy<Dictionary<string, string>> IanaCanonicalCasing = new(BuildCanonicalCasingDict);'},
        @{old='IanaCanonicalCasing.TryGetValue(timeZoneId, out var canonicalIana)';new='IanaCanonicalCasing.Value.TryGetValue(timeZoneId, out var canonicalIana)'}
    )
    foreach($pair in $pairs){
        if($after.Split(@($pair.old),[StringSplitOptions]::None).Count -ne 2){throw 'Overlay anchor ambiguous'}
        $after=$after.Replace($pair.old,$pair.new)
    }
    $inverse=$after
    foreach($pair in $pairs){$inverse=$inverse.Replace($pair.new,$pair.old)}
    if($inverse -cne $before){throw 'Source inverse differs'}
    Add-Type -Path (Join-Path $PSHOME 'Microsoft.CodeAnalysis.dll')
    Add-Type -Path (Join-Path $PSHOME 'Microsoft.CodeAnalysis.CSharp.dll')
    $tree=[Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($after)
    if(@($tree.GetDiagnostics()|Where-Object Severity -eq Error).Count){throw 'Overlay syntax invalid'}
    $null=New-Item -ItemType Directory -Path $out
    Copy-Item -LiteralPath $from -Destination (Join-Path $out 'DefaultTimeZoneProvider.original.cs')
    $to=Join-Path $out 'DefaultTimeZoneProvider.cs'
    [IO.File]::WriteAllText($to,$after,[Text.UTF8Encoding]::new($false))
    @{source=$from;sourceSha256=$expected;overlay=$to;overlaySha256=(Get-FileHash $to).Hash;
      inverseVerified=$true;syntaxVerified=$true;compiled=$false;executed=$false;
      scope='Defer canonical casing table until lookup; same builder and key comparer. Changes initialization/exception timing; semantic and confined performance verification pending.'}|ConvertTo-Json|Set-Content (Join-Path $out 'overlay.json')
}finally{$lease.Dispose()}
