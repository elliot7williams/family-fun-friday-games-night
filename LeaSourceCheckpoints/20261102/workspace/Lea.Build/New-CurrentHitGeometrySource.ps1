function New-CurrentHitGeometrySource {
    param([string]$Original)
    $newline=if($Original.Contains("`r`n")){"`r`n"}else{"`n"}
    $edits=@(
        @('        private readonly List<Hit> hits = new List<Hit>();',('        private readonly List<Hit> hits = new List<Hit>();'+$newline+'        private bool hitsDirty = true;')),
        @('            layoutDirty = true;',('            layoutDirty = true;'+$newline+'            hitsDirty = true;')),
        @('            sy = Math.Max(0, Math.Min(MaxScroll(), value));',('            var previousScroll = sy;'+$newline+'            sy = Math.Max(0, Math.Min(MaxScroll(), value));'+$newline+'            if (sy != previousScroll) hitsDirty = true;')),
        @(('            base.OnPaint(pe);'+$newline+'            hits.Clear();'),('            base.OnPaint(pe);'+$newline+'            PaintDocument(pe);'+$newline+'        }'+$newline+$newline+'        private void PaintDocument(PaintEventArgs pe)'+$newline+'        {'+$newline+'            hits.Clear();')),
        @('            PaintKeyFocus(pe.Graphics);',('            PaintKeyFocus(pe.Graphics);'+$newline+'            hitsDirty = false;')),
        @(('        private Hit HitAt(Point point)'+$newline+'        {'),('        private Hit HitAt(Point point)'+$newline+'        {'+$newline+'            if (closed || At == null) return null;'+$newline+'            // Input can precede the queued paint after scrolling or restyling.'+$newline+'            if (hitsDirty || layoutDirty || layoutWidth != ClientSize.Width)'+$newline+'            {'+$newline+'                if (!IsHandleCreated) CreateControl();'+$newline+'                using (var graphics = CreateGraphics())'+$newline+'                using (var paint = new PaintEventArgs(graphics, ClientRectangle)) PaintDocument(paint);'+$newline+'            }'))
    )
    $updated=$Original
    foreach($pair in $edits){
        if([regex]::Matches($updated,[regex]::Escape($pair[0])).Count -ne 1){throw ('Hit geometry anchor differs: '+$pair[0])}
        $updated=$updated.Replace($pair[0],$pair[1])
    }
    $inverse=$updated
    for($i=$edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($edits[$i][1],$edits[$i][0])}
    if($inverse -cne $Original){throw 'Hit geometry inverse differs'}
    return $updated
}
