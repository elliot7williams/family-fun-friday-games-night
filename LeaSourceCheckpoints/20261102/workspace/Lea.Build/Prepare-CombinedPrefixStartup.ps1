param([ValidateRange(1,9)][int]$Version=3)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$stage='I:\LeaBuildStorage\sdk-full-script-assembly-20261101-v7'
$out='I:\LeaBuildStorage\combined-prefix-startup-source-20261101-v'+$Version
if(Test-Path -LiteralPath $out){throw 'Preserve prefix source evidence'}
$inputs=[Collections.Generic.List[object]]::new();$rows=[Collections.Generic.List[object]]::new()
function Pin([string]$path,[string]$expected){if((Get-FileHash -LiteralPath $path).Hash -cne $expected){throw ('Prefix source differs: '+$path)};$inputs.Add(@{path=$path;sha256=$expected})}
Pin (Join-Path $stage 'assembly.json') 'DD8BAED18A7A189FD339A3D925669F4CAB076F58F717D2369AC37253C365AEFC'
$assembly=Get-Content (Join-Path $stage 'assembly.json') -Raw|ConvertFrom-Json
New-Item -ItemType Directory -Path $out|Out-Null
function Project([string]$name,[string]$from,[string]$hash,$edits){
    Pin $from $hash;$original=[IO.File]::ReadAllText($from);$text=$original
    foreach($edit in $edits){if($edit.Count -ne 2 -or [regex]::Matches($text,[regex]::Escape($edit[0])).Count -ne 1){throw ('Unique prefix projection anchor required: '+$name)};$text=$text.Replace($edit[0],$edit[1])}
    function Inverse([string]$value){
        for($i=$edits.Count-1;$i -ge 0;$i--){if([regex]::Matches($value,[regex]::Escape($edits[$i][1])).Count -ne 1){return $false};$value=$value.Replace($edits[$i][1],$edits[$i][0])}
        return $value -ceq $original
    }
    if(!(Inverse $text)){throw 'Prefix projection inverse differs'}
    foreach($bad in @($original,($text+$text),($text+"`n// unrelated edit"))){if(Inverse $bad){throw 'Prefix source negative accepted'}}
    $to=Join-Path $out $name;[IO.File]::WriteAllText($to,$text,[Text.UTF8Encoding]::new($false))
    $rows.Add(@{path=$to;sha256=(Get-FileHash $to).Hash;origin=$from;originalSha256=$hash;inverseVerified=$true;negativeControls=3;replacements=$edits.Count})
}
$from=Join-Path $PSScriptRoot 'CombinedWorkerStartupProbe.cs'
$nl=if([IO.File]::ReadAllText($from).Contains("`r`n")){"`r`n"}else{"`n"}
$setup=('        var engine = new E();','        var page = engine.Go(fixture);','        var script = new Jp(engine, page, Path.Combine(runtime, "ScriptWorker"));') -join $nl
$edits=@(
    @('internal static class CombinedWorkerStartupProbe','internal static class CombinedPrefixStartupProbe'),
    @('try { return Run(Path.GetFullPath(args[1])); }','try { throw new InvalidOperationException("Full-prefix diagnostic only."); }'),
    @('private static int Run(string fixture)','internal static void Observe(Jp script, P page, Func<Jr> operation, Action<Jr> assertion)'),
    @($setup,'        // The original Worker.Run owns the engine, page and provider.'),
    @('var result = script.Ex(engine.Q(page, "script").C[0].X);','var result = operation();'),
    @('return valid ? 0 : 1;',('if (!valid) throw new InvalidOperationException("Prefix startup capture invalid.");'+$nl+'            assertion(result);')),
    @('Single computed-script diagnostic; no full prefix equivalence, no VM instrumentation, no PID reopen.','Instrumented original prefix through first isolated computed assertion; not full suite. No VM instrumentation or PID reopen.')
)
Project 'CombinedPrefixStartupProbe.cs' $from '0A7B34D6268A898DB090F1574282EFA4C3CA0095B228C1A3CAE5753E18A8582A' $edits
foreach($name in @('Worker.cs','Program.cs')){
    $file=@($assembly.files|Where-Object {$_.path.StartsWith('compiled-source/') -and [IO.Path]::GetFileName($_.path) -ceq $name})
    if($file.Count -ne 1){throw 'Unique original suite source required'}
    $from=Join-Path $stage $file[0].path
    $nl=if([IO.File]::ReadAllText($from).Contains("`r`n")){"`r`n"}else{"`n"}
    if($name -ceq 'Worker.cs'){
        $nl=[regex]::Match([IO.File]::ReadAllText($from),'var execution = script.Ex[^\r\n]+(\r?\n)').Groups[1].Value
        if(!$nl){throw 'Computed execution local newline missing'}
        $old=('                var execution = script.Ex(engine.Q(page, "script").C[0].X);','                Check(execution.Ok && execution.N == 6, "isolated computed script: " + execution.S + "; " + execution.M + "; " + script.Sd);') -join $nl
        $new=('                CombinedPrefixStartupProbe.Observe(script, page,','                    () => script.Ex(engine.Q(page, "script").C[0].X),','                    execution => Check(execution.Ok && execution.N == 6, "isolated computed script: " + execution.S + "; " + execution.M + "; " + script.Sd));','                CombinedPrefixComplete.Stop();') -join $nl
    }else{
        $nl=[regex]::Match([IO.File]::ReadAllText($from),'            catch \(Exception error\)(\r?\n)').Groups[1].Value
        if(!$nl){throw 'Main catch local newline missing'}
        $old='            catch (Exception error)'+$nl+'            {'
        $new=('            catch (CombinedPrefixComplete)','            {','                Console.WriteLine("Diagnostic stop after isolated computed-script assertion; not full suite.");','                return 0;','            }',$old) -join $nl
    }
    Project $name $from $file[0].sha256 (,@($old,$new))
}
foreach($input in $inputs){if((Get-FileHash $input.path).Hash -cne $input.sha256){throw 'Prefix projection input changed'}}
@{verified=$true;files=$rows;inputs=$inputs;originalPrefixPhases=29;targetPhase=29;originalAssertionPreserved=$true;diagnosticOnly=$true;engineChanged=$false;compiled=$false;executed=$false;testBudgetMs=300000;startupBudgetMs=5000;cleanupBudgetMs=5000}|ConvertTo-Json -Depth 6|Set-Content (Join-Path $out 'source.json') -Encoding utf8
Get-FileHash (Join-Path $out 'source.json')
