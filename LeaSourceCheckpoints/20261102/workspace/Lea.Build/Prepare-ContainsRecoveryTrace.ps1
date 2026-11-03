$ErrorActionPreference='Stop'
$out='I:\LeaBuildStorage\contains-recovery-trace-source-20261102-v1'
$source='I:\LeaBuildStorage\sdk-full-script-assembly-20261101-v8/compiled-source/source/Lea.Script.Tests/Contains.cs'
$pin='809837EC846B255428FA99FF6A25452D29CAD31F9EBDDECE90D34BA11DA9600B'
if(Test-Path $out){throw 'Preserve recovery trace preparation'}
$hold=[IO.File]::Open($source,'Open','Read','Read')
try{
    if((Get-FileHash $source).Hash -cne $pin){throw 'Exact Contains source required'}
    $text=[IO.File]::ReadAllText($source);$nl=if($text.Contains("`r`n")){"`r`n"}else{"`n"}
    $pairs=@(
        @{
            old=('        private static void Suite(E engine, P page, Func<string, Jr> ex, string mode)'+$nl+'        {')
            replacement=('        private static void Suite(E engine, P page, Func<string, Jr> ex, string mode)'+$nl+'        {'+$nl+'            var recoveryTrace = Stopwatch.StartNew();'+$nl+'            var recoveryStep = 0;')
        },
        @{
            old='                var result = ex(code);'
            replacement=('                var traceStep = ++recoveryStep;'+$nl+'                Console.Error.WriteLine("LEA_CONTAINS_RECOVERY " + mode + " code " + traceStep + " begin " + recoveryTrace.ElapsedMilliseconds);'+$nl+'                var result = ex(code);'+$nl+'                Console.Error.WriteLine("LEA_CONTAINS_RECOVERY " + mode + " code " + traceStep + " end " + recoveryTrace.ElapsedMilliseconds + " status=" + result.S);')
        },
        @{
            old='                var bounded = ex("document.contains(child);");'
            replacement=('                Console.Error.WriteLine("LEA_CONTAINS_RECOVERY " + mode + " cycle begin " + recoveryTrace.ElapsedMilliseconds);'+$nl+'                var bounded = ex("document.contains(child);");'+$nl+'                Console.Error.WriteLine("LEA_CONTAINS_RECOVERY " + mode + " cycle end " + recoveryTrace.ElapsedMilliseconds + " status=" + bounded.S);')
        },
        @{
            old='            var exhausted = ex("for(var i=0;i<300;i++)group.contains(child);");'
            replacement=('            Console.Error.WriteLine("LEA_CONTAINS_RECOVERY " + mode + " bridge begin " + recoveryTrace.ElapsedMilliseconds);'+$nl+'            var exhausted = ex("for(var i=0;i<300;i++)group.contains(child);");'+$nl+'            Console.Error.WriteLine("LEA_CONTAINS_RECOVERY " + mode + " bridge end " + recoveryTrace.ElapsedMilliseconds + " status=" + exhausted.S);')
        }
    )
    function Project([string]$inputText){
        if($inputText.Contains('LEA_CONTAINS_RECOVERY') -or $inputText.Contains('var recoveryTrace =')){throw 'Already instrumented source refused'}
        $value=$inputText
        foreach($pair in $pairs){
            if([regex]::Matches($value,[regex]::Escape($pair.old)).Count -ne 1){throw 'Unique exact original boundary required'}
            $value=$value.Replace($pair.old,$pair.replacement)
        }
        $inverse=$value
        for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i].replacement,$pairs[$i].old)}
        if($inverse -cne $inputText){throw 'Trace source inverse differs'}
        return $value
    }
    $updated=Project $text;$negative=0
    foreach($bad in @($text.Replace($pairs[1].old,'MISSING_CALL'),($text+$text),$updated)){
        $rejected=$false;try{$null=Project $bad}catch{$rejected=$true}
        if(!$rejected){throw 'Invalid trace source accepted'};$negative++
    }
    New-Item -ItemType Directory -Path $out|Out-Null
    $to=Join-Path $out 'Contains.cs';[IO.File]::WriteAllText($to,$updated,[Text.UTF8Encoding]::new($false))
    $readback=[IO.File]::ReadAllText($to)
    for($i=$pairs.Count-1;$i -ge 0;$i--){$readback=$readback.Replace($pairs[$i].replacement,$pairs[$i].old)}
    if($readback -cne $text -or (Get-FileHash $source).Hash -cne $pin){throw 'Written inverse/original differs'}
    @{verified=$true;original=$source;originalSha256=$pin;source=$to;sha256=(Get-FileHash $to).Hash;inverseVerified=$true;negativeControls=$negative;replacements=$pairs.Count;assertionsChanged=$false;engineChanged=$false;budgetsChanged=$false;compiled=$false;executed=$false;diagnosticOnly=$true;scope='Reversible bounded test-only elapsed/status checkpoints; no page code/content printed. Extra stderr I/O can perturb timing. Requires fresh full-prefix integration; no release qualification';runnerSha256=(Get-FileHash $PSCommandPath).Hash}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'source.json') -Encoding utf8
    Get-FileHash (Join-Path $out 'source.json')
}finally{$hold.Dispose()}
