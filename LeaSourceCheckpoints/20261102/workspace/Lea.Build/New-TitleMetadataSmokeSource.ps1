function New-TitleMetadataSmokeSource {
    param([string]$Original)
    $helpers=@'
        private static void AuthoredBoxes(V view, int expected, string name)
        {
            Eq(expected, view.Bc, name);
            foreach (var box in LayoutBoxes(view))
                Ok(!(bool)box.GetType().GetField("Title").GetValue(box), name + " contains only authored content");
        }

        private static void WidgetBoxes(V view)
        {
            foreach (var field in view.At.Fs)
            {
                var found = false;
                foreach (var box in LayoutBoxes(view))
                {
                    var type = box.GetType();
                    var block = type.GetField("B").GetValue(box) as B;
                    if (block != null && Object.ReferenceEquals(block.I, field) && !(bool)type.GetField("Inline").GetValue(box))
                    { found = true; break; }
                }
                Ok(found, "richer widget retains authored layout box: " + field.N);
            }
        }

'@
    $edits=@(
        @('Eq(12, view.Bc, "richer widget layout boxes");',('AuthoredBoxes(view, 11, "richer widget layout boxes");'+"`r`n"+'                        WidgetBoxes(view);')),
        @('Eq(5, view.Bc, "box-model boxes");','AuthoredBoxes(view, 4, "box-model boxes");'),
        @('Eq(17, view.Bc, "table layout boxes");','AuthoredBoxes(view, 16, "table layout boxes");'),
        @('Eq(8, view.Bc, "positioning layout boxes");','AuthoredBoxes(view, 7, "positioning layout boxes");'),
        @('Eq(16, view.Bc, "absolute and fixed layout boxes");','AuthoredBoxes(view, 15, "absolute and fixed layout boxes");'),
        @('Eq(42, view.Bc, "nested context layout boxes");','AuthoredBoxes(view, 41, "nested context layout boxes");'),
        @('Eq(16, view.Bc, "scroll boxes");','AuthoredBoxes(view, 15, "scroll boxes");'),
        @('Check(view, root, "css.html", 3, 4);','Check(view, root, "css.html", 3, 3);'),
        @('Check(view, root, "images.html", 5, 6);','Check(view, root, "images.html", 5, 5);'),
        @('Check(view, root, "layout.html", 3, 4);','Check(view, root, "layout.html", 3, 3);'),
        @('Eq(boxes, view.Bc, file + " boxes");','AuthoredBoxes(view, boxes, file + " boxes");'),
        @('        private static string Kinds(P page)',($helpers+'        private static string Kinds(P page)'))
    )
    $updated=$Original
    foreach($pair in $edits){
        if($pair.Count -ne 2 -or [regex]::Matches($updated,[regex]::Escape($pair[0])).Count -ne 1){throw 'Title metadata test anchor differs'}
        $updated=$updated.Replace($pair[0],$pair[1])
    }
    $inverse=$updated
    for($i=$edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($edits[$i][1],$edits[$i][0])}
    if($inverse -cne $Original){throw 'Title test overlay inverse differs'}
    return $updated
}
