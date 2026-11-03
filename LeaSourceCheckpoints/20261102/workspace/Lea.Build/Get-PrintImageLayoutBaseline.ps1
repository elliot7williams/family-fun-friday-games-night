function Get-PrintImageLayoutBaseline([string]$Text,[string]$Name){
    $text=$Text.Replace("`r",'')
    if($Name -eq 'Pm.cs'){
        $blocks=@('(?ms)^        public static FlowDocument Ci\(.*?(?=^        private sealed class WhiteSpace)', '(?ms)^                    if \(images != null\)\n.*?(?=^                    string alt;)')
        $edits=@(
            @("            private readonly IReadOnlyDictionary<int, Pi> images;`n",''),
            @("            private readonly double imageWidth, imageHeight;`n",''),
            @('internal Builder(Ps snapshot, bool grayscale, IReadOnlyDictionary<int, Pi> images = null, double imageWidth = 0, double imageHeight = 0)','internal Builder(Ps snapshot, bool grayscale)'),
            @("                this.images = images; this.imageWidth = imageWidth; this.imageHeight = imageHeight;`n",'')
        )
    }elseif($Name -eq 'Pw.cs'){
        $blocks=@('(?ms)^        public static Pw Di\(.*?(?=^        private static double Size\()');$edits=@()
    }else{throw 'Unknown image layout source'}
    foreach($block in $blocks){if([regex]::Matches($text,$block).Count -ne 1){throw 'Unique image layout block required'};$text=[regex]::Replace($text,$block,'')}
    foreach($edit in $edits){if([regex]::Matches($text,[regex]::Escape($edit[0])).Count -ne 1){throw 'Unique image layout anchor required'};$text=$text.Replace($edit[0],$edit[1])}
    return $text
}
