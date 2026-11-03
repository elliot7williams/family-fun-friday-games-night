function New-NestedOverlapSmokeSource {
    param([string]$Original)
    $newline=if($Original.Contains("`r`n")){"`r`n"}else{"`n"}
    $helper=@'
        private static void AlignNestedOverlap(V view)
        {
            // Establish the stacking scenario independently of document-title paint.
            using (var initial = Render(view)) { }
            var escape = BlockRect(view, BlockById(view.At, "escape"));
            var cover = BlockRect(view, BlockById(view.At, "cover"));
            var high = BlockById(view.At, "high");
            Ok(high.S.Tp.HasValue, "overlap fixture has explicit high-context inset");
            Eq(escape.Size, cover.Size, "overlap fixture layer sizes match before setup");
            Eq(escape.Left, cover.Left, "overlap fixture horizontal placement matches before setup");
            var top = high.S.Tp.Value + escape.Top - cover.Top;
            Eq(1, view.Ss("test-nested-overlap", "#high{top:" + top.ToString(System.Globalization.CultureInfo.InvariantCulture) + "px}"), "install title-independent stacking overlap setup");
        }

'@
    $helper=$helper.Replace("`r`n","`n").Replace("`n",$newline)
    $anchor='                    using (var nestedPaint = Render(view))'
    $helperAnchor='        private static string Kinds(P page)'
    $edits=@(
        @($anchor,('                    AlignNestedOverlap(view);'+$newline+$anchor)),
        @($helperAnchor,($helper+$helperAnchor))
    )
    $updated=$Original
    foreach($pair in $edits){
        if([regex]::Matches($updated,[regex]::Escape($pair[0])).Count -ne 1){throw 'Nested overlap setup anchor differs'}
        $updated=$updated.Replace($pair[0],$pair[1])
    }
    $inverse=$updated
    for($index=$edits.Count-1;$index -ge 0;$index--){$inverse=$inverse.Replace($edits[$index][1],$edits[$index][0])}
    if($inverse -cne $Original){throw 'Nested overlap setup inverse differs'}
    return $updated
}
