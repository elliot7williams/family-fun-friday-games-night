function New-ExternalBootstrapProgram {
    param([Parameter(Mandatory)][string]$Original)
    $call='var execution = script.Ex(code);'
    $check='Check(execution.Ok && execution.N == 6, "computed script: " + execution.S + "; " + execution.M);'
    $stop='if (args.Length > 1 && args[1] == "--bootstrap-cost-diagnostic") { Console.WriteLine("Diagnostic stop after computed-script assertion; not full suite."); return 0; }'
    if([regex]::Matches($Original,[regex]::Escape($call)).Count -ne 1 -or [regex]::Matches($Original,[regex]::Escape($check)).Count -ne 1 -or $Original.Contains($stop)){throw 'External diagnostic overlay shape differs'}
    $replacement=$check+"`r`n                    "+$stop
    $text=$Original.Replace($check,$replacement)
    if($text.Replace($replacement,$check) -cne $Original -or [regex]::Matches($text,[regex]::Escape($stop)).Count -ne 1){throw 'External diagnostic inverse differs'}
    return [pscustomobject]@{text=$text;inverseVerified=$true;assertionPreserved=$true;originalCallPreserved=$text.Contains($call);stopCount=1}
}
