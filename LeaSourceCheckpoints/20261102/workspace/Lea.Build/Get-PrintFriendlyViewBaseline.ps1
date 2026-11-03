function Get-PrintFriendlyViewBaseline([string]$Text) {
    $text=$Text.Replace("`r",'')
    $pattern='(?ms)^        public static Pc Cf\(V view, Pg geometry\)\n.*?(?=^        private static Pc Capture\()'
    if([regex]::Matches($text,$pattern).Count -ne 1){throw 'Unique friendly view method required'}
    $text=[regex]::Replace($text,$pattern,'')
    $edits=@(
        @('private static Pc Capture(V view, Pg geometry, bool friendly = false)','private static Pc Capture(V view, Pg geometry)'),
        @("Ps document = geometry == null ? Ps.Cp(page) : friendly`n                ? Ps.Cf(page, view.Pst(width, height)) : Ps.Cs(page, view.Pst(width, height));",'Ps document = geometry == null ? Ps.Cp(page) : Ps.Cs(page, view.Pst(width, height));')
    )
    foreach($edit in $edits){
        if([regex]::Matches($text,[regex]::Escape($edit[0])).Count -ne 1){throw 'Unique friendly view anchor required'}
        $text=$text.Replace($edit[0],$edit[1])
    }
    return $text
}
