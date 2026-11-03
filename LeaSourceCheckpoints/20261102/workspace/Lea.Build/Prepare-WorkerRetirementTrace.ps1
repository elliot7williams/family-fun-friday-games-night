param([ValidateRange(1,9999)][int]$Version=2)
$ErrorActionPreference='Stop'
$source='I:\LeaBuildStorage\sdk-reproducibility-20261101-v1\a\Lea.Script'
$out="I:\LeaBuildStorage\worker-retirement-trace-source-20261102-v$Version"
if(Test-Path $out){throw 'Preserve diagnostic preparation'}
$specs=@(
    @{name='6.cs';hash='5CC754F6DDF643811F57637D0BBBC1526955AFC47D210EC00AC2FFD19F1E3FAC';points=@(
        @{before='            StopIo();';label='reset-io'},
        @{before='                    retiring.Dispose();';label='reset-endpoint'},
        @{before='                foreach (var lease in leases) lease.Dispose();';label='reset-leases'}
    )},
    @{name='13.cs';hash='FD5BD356DEC82D38C9B3199036EFAE24F2B06931E5DCEAA30607958DE22C19B6';points=@(
        @{before='            if (J != null) J.Dispose();';label='retire-job'},
        @{before='            if (I != null) try { I.Dispose(); } catch (IOException) { }';label='retire-input'},
        @{before='            if (O != null) try { O.Dispose(); } catch (IOException) { }';label='retire-output'},
        @{before='            if (error != null) try { error.Dispose(); } catch (IOException) { }';label='retire-stderr'},
        @{before='            if (cleanup != null && cleanup()) release = null;';label='retire-permissions'}
    )}
)
$holds=[Collections.Generic.List[IDisposable]]::new();$rows=[Collections.Generic.List[object]]::new();$negative=0
function Project([string]$text,$points){
    if($text.Contains('LEA_RETIREMENT_TRACE')){throw 'Already instrumented source'}
    $nl=if($text.Contains("`r`n")){"`r`n"}else{"`n"}
    $updated=$text
    foreach($point in $points){
        if([regex]::Matches($updated,[regex]::Escape($point.before)).Count -ne 1){throw 'Unique original retirement boundary required'}
        $prefix='            Console.Error.WriteLine("LEA_RETIREMENT_TRACE '+$point.label+' begin " + Stopwatch.GetTimestamp());'
        $suffix='            Console.Error.WriteLine("LEA_RETIREMENT_TRACE '+$point.label+' end " + Stopwatch.GetTimestamp());'
        $updated=$updated.Replace($point.before,$prefix+$nl+$point.before+$nl+$suffix)
    }
    return $updated
}
try{
    foreach($spec in $specs){
        $path=Join-Path $source $spec.name
        $hold=[IO.File]::Open($path,'Open','Read','Read');$holds.Add($hold)
        if((Get-FileHash $path).Hash -cne $spec.hash){throw 'Exact SDK source identity required'}
        $original=[IO.File]::ReadAllText($path)
        if($spec.name -ceq '13.cs'){
            $begin=$original.IndexOf('            if (J != null) J.Dispose();')
            $end=$original.IndexOf('            if (I != null)', $begin)
            if($begin -lt 0 -or $end -le $begin){throw 'Complete retirement conditional absent'}
            $spec.points[0].before=$original.Substring($begin,$end-$begin).TrimEnd([char[]]"`r`n")
        }
        $updated=Project $original $spec.points
        $inverse=$updated
        $nl=if($original.Contains("`r`n")){"`r`n"}else{"`n"}
        foreach($point in $spec.points){
            $prefix='            Console.Error.WriteLine("LEA_RETIREMENT_TRACE '+$point.label+' begin " + Stopwatch.GetTimestamp());'
            $suffix='            Console.Error.WriteLine("LEA_RETIREMENT_TRACE '+$point.label+' end " + Stopwatch.GetTimestamp());'
            $inverse=$inverse.Replace($prefix+$nl+$point.before+$nl+$suffix,$point.before)
        }
        if($inverse -cne $original){throw 'Instrumentation inverse differs'}
        foreach($bad in @($updated,($original+$original),$original.Replace($spec.points[0].before,'MISSING_BOUNDARY'))){
            $rejected=$false;try{$null=Project $bad $spec.points}catch{$rejected=$true}
            if(!$rejected){throw 'Invalid projection accepted'};$negative++
        }
        $rows.Add(@{name=$spec.name;original=$path;originalSha256=$spec.hash;text=$updated;points=@($spec.points.label)})
    }
    New-Item -ItemType Directory -Path $out|Out-Null
    foreach($row in $rows){$path=Join-Path $out $row.name;[IO.File]::WriteAllText($path,$row.text,[Text.UTF8Encoding]::new($false));$row.Remove('text');$row.path=$path;$row.sha256=(Get-FileHash $path).Hash}
    @{verified=$true;diagnosticOnly=$true;compiled=$false;executed=$false;inverseVerified=$true;negativeControls=$negative;files=$rows;budgetsChanged=$false;isolationChanged=$false;testedBuildChanged=$false;scope='Eight before/after native timestamp pairs around existing retirement operations; no page data. Diagnostic I/O can perturb timing; neither a fix nor runtime qualification. Compile only in fresh diagnostic candidate.'}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'source.json') -Encoding utf8
    Get-FileHash (Join-Path $out 'source.json')
}finally{foreach($hold in $holds){$hold.Dispose()}}
