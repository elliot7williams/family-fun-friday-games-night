function New-SdkBootstrapTimeline([string]$Parent,[string]$Markers){
    $field='        private int next;'
    $fields=@'
        private int next;
        private readonly System.Diagnostics.Stopwatch traceClock = System.Diagnostics.Stopwatch.StartNew();
        private readonly System.Text.StringBuilder trace = new System.Text.StringBuilder(384);
        internal void Stamp(string name)
        {
            if (trace.Length > 320) return;
            trace.Append(name).Append('=').Append(traceClock.ElapsedTicks * 1000 / System.Diagnostics.Stopwatch.Frequency).Append(';');
        }
        internal string Report { get { return trace.ToString(); } }
'@
    $edits=@(
        @('else startupStage = stage;', 'else startupStage = markers.Report;'),
        @('var options = factory == null ? Vo.Of(1000) : null;', 'var options = factory == null ? Vo.Of(1000) : null; markers.Stamp("options");'),
        @('factory(settings => Vo.At(settings, 1000));', 'factory(settings => Vo.At(settings, 1000)); markers.Stamp("vm");'),
        @('new Dc().Bind(vm);', 'new Dc().Bind(vm); markers.Stamp("bridge");'),
        @('var syntax = prepared.Value;', 'var syntax = prepared.Value; markers.Stamp("prepared");')
    )
    $original=$Parent
    foreach($edit in $edits){if([regex]::Matches($Parent,[regex]::Escape($edit[0])).Count -ne 1){throw 'Unique parent timeline anchor required'};$Parent=$Parent.Replace($edit[0],$edit[1])}
    foreach($anchor in @($field,'return stages[next++];')){if([regex]::Matches($Markers,[regex]::Escape($anchor)).Count -ne 1){throw 'Unique marker timeline anchor required'}}
    if($Markers.Contains('traceClock') -or $original.Contains('markers.Stamp(')){throw 'Already timed candidate refused'}
    $timed=$Markers.Replace($field,$fields).Replace('return stages[next++];','Stamp(stages[next]); return stages[next++];')
    $inverse=$Parent
    foreach($edit in $edits){$inverse=$inverse.Replace($edit[1],$edit[0])}
    if($inverse -cne $original -or $timed.Replace($fields,$field).Replace('Stamp(stages[next]); return stages[next++];','return stages[next++];') -cne $Markers){throw 'Timeline inverse differs'}
    return @{parent=$Parent;markers=$timed;inverseVerified=$true;budgetMs=1000;diagnosticOnly=$true}
}
