function Remove-DocumentTitleSource {
    param([string]$Original)
    $block=@'
            using (var titleFont = BoxFont("#title", style, true))
            {
                var height = Measure(graphics, At.T, titleFont, width);
                boxes.Add(new Bx("#title", At.T, null, style, new Rectangle(x, y, width, height), true));
                y += height + 14;
            }

'@
    $newline=if($Original.Contains("`r`n")){"`r`n"}else{"`n"}
    $block=$block.Replace("`r`n","`n").Replace("`n",$newline)+$newline
    $at=$Original.IndexOf($block,[StringComparison]::Ordinal)
    if($at -lt 0 -or $Original.LastIndexOf($block,[StringComparison]::Ordinal) -ne $at){throw 'Synthetic title layout anchor differs'}
    $updated=$Original.Remove($at,$block.Length)
    if($updated.Insert($at,$block) -cne $Original){throw 'Synthetic title removal inverse differs'}
    return $updated
}
