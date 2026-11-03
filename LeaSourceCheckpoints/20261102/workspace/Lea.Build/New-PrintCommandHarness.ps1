function New-PrintCommandHarness {
    param([string]$Original,[string]$PreviewProbe,[string]$CommandProbe)
    $preview=New-PrintPreviewHarness $Original ($PreviewProbe.Replace("`r",'')+"`n"+$CommandProbe.Replace("`r",''))
    $edits=@(
        @('"cache", "script", "print-preview" }.Contains(mode)','"cache", "script", "print-preview", "print-command" }.Contains(mode)'),
        @('                if (mode == "print-preview") CheckPrintPreview(form, settings, root);',"                if (mode == `"print-command`") CheckPrintCommand(form, settings, root);`n                if (mode == `"print-preview`") CheckPrintPreview(form, settings, root);")
    )
    $text=$preview.text
    foreach($edit in $edits){if([regex]::Matches($text,[regex]::Escape($edit[0])).Count -ne 1){throw 'Print harness anchor differs'};$text=$text.Replace($edit[0],$edit[1])}
    $inverse=$text
    for($i=$edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($edits[$i][1],$edits[$i][0])}
    if($inverse -cne $preview.text){throw 'Print harness inverse differs'}
    @{text=$text;inverseVerified=$preview.inverseVerified}
}
