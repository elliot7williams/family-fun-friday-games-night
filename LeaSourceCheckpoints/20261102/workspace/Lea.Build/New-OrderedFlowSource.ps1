function New-OrderedFlowSource {
    param([string]$Original,[string]$RootSource)
    $begin=$RootSource.IndexOf('        // Ordered anonymous flow keeps mixed inline/block children in DOM order.')
    $end=$RootSource.IndexOf('        private void Blocks(', $begin)
    if($begin -lt 0 -or $end -le $begin){throw 'Ordered flow helper source missing'}
    $helpers=$RootSource.Substring($begin,$end-$begin)
    $branch=@'
                if (IsContainerBlock(kind) && !Flex(style) && !Grid(style))
                {
                    var split = new HashSet<N>();
                    if (FlowParts(node, split)) { OrderedBlocks(page, node, baseUrl, rules, style, split, inherited); return; }
                }
'@
    $anchor='                var text = TextValue(Flex(style) || Grid(style) ? DirectText(node)'
    $edits=@(
        @('for (var node = block.El == null ? null : block.El.Up; node != null; node = node.Up)','for (var node = block.El == null ? null : block.An ? block.El : block.El.Up; node != null; node = node.Up)'),
        @('        private void Blocks(',($helpers+'        private void Blocks(')),
        @($anchor,($branch+"`r`n"+$anchor))
    )
    $text=$Original
    foreach($edit in $edits){if($edit.Count -ne 2 -or [regex]::Matches($text,[regex]::Escape($edit[0])).Count -ne 1){throw 'Ordered source anchor differs'};$text=$text.Replace($edit[0],$edit[1])}
    $inverse=$text;for($i=$edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($edits[$i][1],$edits[$i][0])}
    if($inverse -cne $Original){throw 'Ordered source inverse differs'}
    return $text
}
