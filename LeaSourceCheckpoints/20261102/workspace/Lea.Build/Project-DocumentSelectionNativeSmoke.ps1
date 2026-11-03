function Project-DocumentSelectionNativeSmoke([string]$Original){
    $edits=@(
        @('Eq(SupportedTextSelection.None, (SupportedTextSelection)data["text-selection"], "UIA read-only text selection mode");','Eq(SupportedTextSelection.Single, (SupportedTextSelection)data["text-selection"], "UIA single document selection mode");'),
        @('&& fullText.Contains("Hidden action") &&','&& !fullText.Contains("Hidden action") &&'),
        @('data["text-visible"] = visibleText != null && visibleText.Length == 0;', 'data["text-visible"] = visibleText != null && visibleText.Length > 0 && visibleText.Length <= 2048; if ((bool)data["text-visible"]) foreach (var visibleRange in visibleText) data["text-visible"] = (bool)data["text-visible"] && visibleRange.GetText(-1).Length > 0 && visibleRange.GetBoundingRectangles().Length > 0;')
    )
    $text=$Original
    foreach($edit in $edits){
        if([regex]::Matches($text,[regex]::Escape($edit[0])).Count -ne 1){throw 'Unique document selection test contract anchor required'}
        $text=$text.Replace($edit[0],$edit[1])
    }
    $inverse=$text
    for($i=$edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($edits[$i][1],$edits[$i][0])}
    if($inverse -cne $Original){throw 'Native selection test inverse differs'}
    return $text
}
