function New-PrintPreviewHarness {
    param([string]$Original,[string]$Probe)
    $original=$Original.Replace("`r",'');$probe=$Probe.Replace("`r",'')
    $edits=@(
        @('"cache", "script" }.Contains(mode)','"cache", "script", "print-preview" }.Contains(mode)'),
        @('                if (mode == "navigation") CheckNavigation(form, settings, page);',"                if (mode == `"print-preview`") CheckPrintPreview(form, settings, root);`n                if (mode == `"navigation`") CheckNavigation(form, settings, page);"),
        @('        private static object Call(Form form, string name, params object[] args)',($probe+"`n        private static object Call(Form form, string name, params object[] args)"))
    )
    $text=$original
    foreach($edit in $edits){
        if([regex]::Matches($text,[regex]::Escape($edit[0])).Count -ne 1){throw 'Preview harness anchors differ'}
        $text=$text.Replace($edit[0],$edit[1])
    }
    $inverse=$text
    for($i=$edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($edits[$i][1],$edits[$i][0])}
    if($inverse -cne $original){throw 'Preview harness inverse differs'}
    @{text=$text;inverseVerified=$true}
}
