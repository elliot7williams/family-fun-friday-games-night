function New-BootstrapCostProgram {
    param([Parameter(Mandatory)][string]$Original)
    $call='var execution = script.Ex(code);'
    $observed='var execution = BootstrapCostObserver.Run(script, code);'
    $check='Check(execution.Ok && execution.N == 6, "computed script: " + execution.S + "; " + execution.M);'
    $stop='if (args.Length > 1 && args[1] == "--bootstrap-cost-diagnostic") { Console.WriteLine("Diagnostic stop after computed-script assertion; not full suite."); return 0; }'
    $checkAndStop=$check+"`r`n                    "+$stop
    $edits=@(@($call,$observed),@($check,$checkAndStop))
    $text=$Original
    foreach($edit in $edits){
        if($edit.Count -ne 2 -or [regex]::Matches($text,[regex]::Escape($edit[0])).Count -ne 1){throw 'Diagnostic overlay shape differs'}
        $text=$text.Replace($edit[0],$edit[1])
    }
    if([regex]::Matches($text,[regex]::Escape($stop)).Count -ne 1 -or [regex]::Matches($text,[regex]::Escape($observed)).Count -ne 1){throw 'Diagnostic stop or observation missing'}
    $inverse=$text
    for($i=$edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($edits[$i][1],$edits[$i][0])}
    if($inverse -cne $Original){throw 'Diagnostic overlay inverse differs'}
    return [pscustomobject]@{text=$text;inverseVerified=$true;edits=2;stopCount=1;assertionPreserved=$text.Contains($check)}
}
