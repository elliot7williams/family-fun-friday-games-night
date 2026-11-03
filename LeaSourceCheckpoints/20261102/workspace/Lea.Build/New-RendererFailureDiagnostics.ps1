function New-RendererFailureDiagnostics {
    param([string]$Original)
    $field="        private static string rendererCheckpoint = `"before renderer`";`r`n"
    $edits=[Collections.Generic.List[object]]::new()
    $edits.Add(@('        private static string Kinds(P page)',($field+'        private static string Kinds(P page)')))
    foreach($pair in @(
        @('view.Go(negationPath);','negation-load'),
        @('using (var combinedSelectorFont =','negation-combined-font'),
        @('using (var enabledSelectorFont =','negation-enabled-font'),
        @('using (var disabledSelectorFont =','negation-disabled-font'),
        @('view.Go(selectorFunctionPath);','functional-load'),
        @('var selectorViewField = FieldById(view.At, "field");','functional-field'),
        @('view.Go(selectorArithmeticPath);','arithmetic-load'),
        @('view.Go(selectorFilteredPath);','nth-filter-load'),
        @('view.Go(selectorRelationalPath);','relational-load'),
        @('view.Go(selectorStatePath);','attribute-state-load')
    )){
        $edits.Add(@($pair[0],('rendererCheckpoint = "'+$pair[1]+'";'+"`r`n                    "+$pair[0])))
    }
    $old='                Console.Error.WriteLine("Lea smoke failed: " + error);'
    $new=@'
                Console.Error.WriteLine("Lea smoke failed: " + error);
                Console.Error.WriteLine("Lea renderer last checkpoint: " + rendererCheckpoint);
                var frames = new StackTrace(error, false).GetFrames();
                if (frames != null)
                    foreach (var frame in frames)
                    {
                        var method = frame.GetMethod();
                        Console.Error.WriteLine("Lea failure frame: method=" + (method == null ? "unknown" : method.Name) +
                            "; token=" + (method == null ? 0 : method.MetadataToken) + "; il=" + frame.GetILOffset());
                    }
'@
    $edits.Add(@($old,$new))
    $updated=$Original
    foreach($pair in $edits){
        if($pair.Count -ne 2 -or [regex]::Matches($updated,[regex]::Escape($pair[0])).Count -ne 1){throw ('Renderer diagnostic anchor differs: '+$pair[0])}
        $updated=$updated.Replace($pair[0],$pair[1])
    }
    $inverse=$updated
    for($i=$edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($edits[$i][1],$edits[$i][0])}
    if($inverse -cne $Original){throw 'Renderer diagnostic inverse differs'}
    return $updated
}
