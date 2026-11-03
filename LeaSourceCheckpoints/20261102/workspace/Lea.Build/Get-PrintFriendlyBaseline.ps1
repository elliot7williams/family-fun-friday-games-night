function Get-PrintFriendlyBaseline([string]$Text) {
    $text=$Text.Replace("`r",'')
    $patterns=@(
        '(?ms)^        public static Ps Cf\(P page\)\n.*?(?=^        public static Ps Cs\(P page,)',
        '(?ms)^        private static bool FriendlyExcluded\(N node, string kind, ref int characters\)\n.*?(?=^        private static bool Excluded\(string kind\))'
    )
    foreach($pattern in $patterns){
        if([regex]::Matches($text,$pattern).Count -ne 1){throw 'Unique friendly source block required'}
        $text=[regex]::Replace($text,$pattern,'')
    }
    $edits=@(
        @('private static Ps Capture(P page, IReadOnlyDictionary<N, S> styles, bool friendly = false)','private static Ps Capture(P page, IReadOnlyDictionary<N, S> styles)'),
        @('Excluded(kind) || friendly && FriendlyExcluded(node, kind, ref characters) ||','Excluded(kind) ||')
    )
    foreach($edit in $edits){
        if([regex]::Matches($text,[regex]::Escape($edit[0])).Count -ne 1){throw 'Unique friendly source anchor required'}
        $text=$text.Replace($edit[0],$edit[1])
    }
    return $text
}
