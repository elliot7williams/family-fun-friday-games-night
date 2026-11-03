function New-OrderedFlowSmokeSource {
    param([string]$Original)
    $edits=@(
        @('Eq(12, nestedFlowPage.Bs.Count, "nested flow block count");','Eq(13, nestedFlowPage.Bs.Count, "nested flow block count");'),
        @('"h1,section,p,div,article,h2,p,p,p,section,p,p", BlockKinds(nestedFlowPage)','"h1,section,p,div,article,h2,p,p,p,section,body,p,p", BlockKinds(nestedFlowPage)'),
        @('Ok(Object.ReferenceEquals(nestedFlowPage.Bs[10].Up, nestedFlowPage.Bs[9]), "mixed-content visual context owns child");','Ok(Object.ReferenceEquals(nestedFlowPage.Bs[11].Up, nestedFlowPage.Bs[9]), "mixed-content visual context owns child");'),
        @('Ok(nestedFlowPage.Bs[11].Up == null, "following root block leaves visual contexts");', ('Ok(nestedFlowPage.Bs[12].Up == null, "following root block leaves visual contexts");'+"`r`n"+'                Ok(nestedFlowPage.Bs[9].X == "" && nestedFlowPage.Bs[10].X == "Loose text before a nested block." && Object.ReferenceEquals(nestedFlowPage.Bs[10].Up, nestedFlowPage.Bs[9]), "anonymous prefix belongs to mixed context without duplicate text");')),
        @('var mixedChild = BlockRect(view, view.At.Bs[10]);','var mixedChild = BlockRect(view, view.At.Bs[11]);'),
        @('var after = BlockRect(view, view.At.Bs[11]);','var after = BlockRect(view, view.At.Bs[12]);'),
        @('var mixedLines = TextLines(view, view.At.Bs[9]);','var mixedLines = TextLines(view, view.At.Bs[10]);'),
        @('Ok(BlockRect(view, view.At.Bs[11]).Top > BlockOuterRect(view, view.At.Bs[9]).Bottom, "narrow root flow follows expanded mixed container");','Ok(BlockRect(view, view.At.Bs[12]).Top > BlockOuterRect(view, view.At.Bs[9]).Bottom, "narrow root flow follows expanded mixed container");')
    )
    $updated=$Original
    foreach($pair in $edits){
        if($pair.Count -ne 2 -or [regex]::Matches($updated,[regex]::Escape($pair[0])).Count -ne 1){throw 'Nested flow test anchor differs'}
        $updated=$updated.Replace($pair[0],$pair[1])
    }
    $inverse=$updated
    for($i=$edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($edits[$i][1],$edits[$i][0])}
    if($inverse -cne $Original){throw 'Nested flow test inverse differs'}
    return $updated
}
