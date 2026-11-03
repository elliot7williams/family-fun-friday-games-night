function New-TitleGeometrySmokeSource {
    param([string]$Original)
    $helpers=@'
        private static int PaintedIndex(V view, B block, N element = null)
        {
            var index = 0;
            foreach (var box in LayoutBoxes(view))
            {
                var type = box.GetType();
                var matches = block != null ? Object.ReferenceEquals(type.GetField("B").GetValue(box), block) :
                    Object.ReferenceEquals(type.GetField("El").GetValue(box), element);
                if (matches && !(bool)type.GetField("Inline").GetValue(box)) return index;
                index++;
            }
            throw new InvalidOperationException("Authored geometry target is not painted.");
        }

        private static int ClassIndex(V view, string name)
        {
            B match = null;
            foreach (var block in view.At.Bs)
                if (block.El.Av("class") == name)
                {
                    if (match != null) throw new InvalidOperationException("Duplicate authored class target.");
                    match = block;
                }
            if (match == null) throw new InvalidOperationException("Missing authored class target.");
            return PaintedIndex(view, match);
        }

        private static int TableIndex(V view, int row = -1, int column = 0, int child = -1)
        {
            B table = null;
            foreach (var block in view.At.Bs)
                if (block.Tb != null)
                {
                    if (table != null) throw new InvalidOperationException("Duplicate authored table target.");
                    table = block;
                }
            if (table == null) throw new InvalidOperationException("Missing authored table target.");
            if (row < 0) return PaintedIndex(view, table);
            var cell = table.Tb.Rs[row].Cs[column];
            return child < 0 ? PaintedIndex(view, null, cell.El) : PaintedIndex(view, cell.Bs[child]);
        }

'@
    $edits=@(
        @('Eq(280, view.Rc(2).Width, "solid content width");','Eq(280, view.Rc(ClassIndex(view, "solid")).Width, "solid content width");'),
        @('Eq(308, view.Rb(2).Width, "solid border-box width");','Eq(308, view.Rb(ClassIndex(view, "solid")).Width, "solid border-box width");'),
        @('Eq(54, view.Rc(3).Height, "fixed content height");','Eq(54, view.Rc(ClassIndex(view, "dashed")).Height, "fixed content height");'),
        @('Eq(76, view.Rb(3).Height, "fixed border-box height");','Eq(76, view.Rb(ClassIndex(view, "dashed")).Height, "fixed border-box height");'),
        @('Eq(320, view.Rc(4).Width, "sided content width");','Eq(320, view.Rc(ClassIndex(view, "sides")).Width, "sided content width");'),
        @('Eq(347, view.Rb(4).Width, "sided border-box width");','Eq(347, view.Rb(ClassIndex(view, "sides")).Width, "sided border-box width");'),
        @('Eq(480, view.Rc(2).Width, "table content width");','Eq(480, view.Rc(TableIndex(view)).Width, "table content width");'),
        @('Eq(484, view.Rb(2).Width, "table border-box width");','Eq(484, view.Rb(TableIndex(view)).Width, "table border-box width");'),
        @('Eq(view.Rb(4).Y, view.Rb(5).Y, "header cells share a row");','Eq(view.Rb(TableIndex(view, 0, 0)).Y, view.Rb(TableIndex(view, 0, 1)).Y, "header cells share a row");'),
        @('Eq(160, view.Rb(4).Width, "equal table column width");','Eq(160, view.Rb(TableIndex(view, 0, 0)).Width, "equal table column width");'),
        @('Eq(320, view.Rb(10).Width, "spanning cell width");','Eq(320, view.Rb(TableIndex(view, 2, 0)).Width, "spanning cell width");'),
        @('Eq(160, view.Rb(11).Width, "cell after span width");','Eq(160, view.Rb(TableIndex(view, 2, 1)).Width, "cell after span width");'),
        @('Ok(view.Rb(13).Y >= view.Rb(12).Y && view.Rb(15).Bottom <= view.Rb(12).Bottom, "nested blocks stay in cell");','Ok(view.Rb(TableIndex(view, 3, 0, 0)).Y >= view.Rb(TableIndex(view, 3, 0)).Y && view.Rb(TableIndex(view, 3, 0, 2)).Bottom <= view.Rb(TableIndex(view, 3, 0)).Bottom, "nested blocks stay in cell");'),
        @('Ok(view.Rb(2).Width < 484, "table reflows to narrow view");','Ok(view.Rb(TableIndex(view)).Width < 484, "table reflows to narrow view");'),
        @('Eq(view.Rb(4).Y, view.Rb(5).Y, "narrow header row alignment");','Eq(view.Rb(TableIndex(view, 0, 0)).Y, view.Rb(TableIndex(view, 0, 1)).Y, "narrow header row alignment");'),
        @('Click(view, view.Rb(15));','Click(view, view.Rb(TableIndex(view, 3, 0, 2)));'),
        @('Ok(view.Bc > view.At.Bs.Count, "negation fields paint separately");',('AuthoredBoxes(view, 9, "negation authored layout boxes");'+"`r`n"+'                    WidgetBoxes(view);')),
        @('        private static string Kinds(P page)',($helpers+'        private static string Kinds(P page)'))
    )
    $updated=$Original
    foreach($pair in $edits){
        if($pair.Count -ne 2 -or [regex]::Matches($updated,[regex]::Escape($pair[0])).Count -ne 1){throw 'Title geometry test anchor differs'}
        $updated=$updated.Replace($pair[0],$pair[1])
    }
    $inverse=$updated
    for($i=$edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($edits[$i][1],$edits[$i][0])}
    if($inverse -cne $Original){throw 'Title geometry test inverse differs'}
    return $updated
}
