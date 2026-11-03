function Get-PrintImageKeyBaseline([string]$Text){
    $text=$Text.Replace("`r",'')
    $edits=@(
        @('string image = "", imagePolicy = "", cacheKey = "";','string image = "", imagePolicy = "";'),
        @("                        if (friendly && uri != null && uri.Scheme == `"data`") cacheKey = Take(uri.AbsoluteUri, ref characters);`n",''),
        @('computed == null ? null : Py.Cp(computed, ref characters), cacheKey));','computed == null ? null : Py.Cp(computed, ref characters)));'),
        @("        internal string CacheKey { get; private set; }`n",''),
        @('string link, string image, string policy, Py style, string cacheKey = "")','string link, string image, string policy, Py style)'),
        @("            CacheKey = cacheKey;`n",'')
    )
    foreach($edit in $edits){
        if([regex]::Matches($text,[regex]::Escape($edit[0])).Count -ne 1){throw 'Unique image-key source anchor required'}
        $text=$text.Replace($edit[0],$edit[1])
    }
    return $text
}
