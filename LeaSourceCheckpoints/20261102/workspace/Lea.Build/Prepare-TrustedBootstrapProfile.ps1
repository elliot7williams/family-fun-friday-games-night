param([ValidateSet(1,2)][int]$Version=2)
$ErrorActionPreference='Stop'
$source='I:\LeaBuildStorage\sdk-reproducibility-20261101-v1\a\Lea.Script\8.cs'
$out="I:\LeaBuildStorage\trusted-bootstrap-profile-source-20261102-v$Version"
if(Test-Path $out){throw 'Preserve bootstrap profile source'}
$originalHash='21792E7953FFEEDA20E6F151ACBC85B873B9BF4C0E5FEA9E01DCB46BF0BC2C9E'
$hold=[IO.File]::Open($source,'Open','Read','Read')
try{
    if((Get-FileHash $source).Hash -cne $originalHash){throw 'Exact trusted runtime source required'}
    $original=[IO.File]::ReadAllText($source);$nl=if($original.Contains("`r`n")){"`r`n"}else{"`n"}
    $opening=@'
            var profileClock = System.Diagnostics.Stopwatch.StartNew();
            var profileProcess = System.Diagnostics.Process.GetCurrentProcess();
            var profileCpu = profileProcess.TotalProcessorTime;
            var profileGc = new[] { GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2) };
            var profileRows = new System.Collections.Generic.List<string>();
            Action<string> profileMark = label => profileRows.Add(label + " wall=" + profileClock.ElapsedTicks + " cpu=" + (profileProcess.TotalProcessorTime - profileCpu).Ticks + " gc0=" + (GC.CollectionCount(0) - profileGc[0]) + " gc1=" + (GC.CollectionCount(1) - profileGc[1]) + " gc2=" + (GC.CollectionCount(2) - profileGc[2]));
            profileMark("enter");
            try
            {
            var markers = new Bm();
'@
    $ending=@'
            startupStage = null;
            profileMark("complete");
            }
            finally
            {
                try
                {
                    profileMark("end");
                    foreach (var row in profileRows) Console.Error.WriteLine("LEA_TRUSTED_BOOT wallFrequency=" + System.Diagnostics.Stopwatch.Frequency + " cpuFrequency=" + TimeSpan.TicksPerSecond + " " + row);
                }
                finally { profileProcess.Dispose(); }
            }
'@
    $pairs=@(
        @('            var markers = new Bm();',$opening.Replace("`r`n","`n").Replace("`n",$nl)),
        @('            startupStage = factory == null ? "vm" : "factory";',('            startupStage = factory == null ? "vm" : "factory";'+$nl+'            profileMark("vm-begin");')),
        @('                factory(settings => Vo.At(settings, 1000));',('                factory(settings => Vo.At(settings, 1000));'+$nl+'            profileMark("vm-end");')),
        @('            var syntax = prepared.Value;',('            profileMark("prepare-begin");'+$nl+'            var syntax = prepared.Value;'+$nl+'            profileMark("prepare-end");')),
        @('            dispatch = vm.Evaluate(syntax);',('            profileMark("evaluate-begin");'+$nl+'            try { dispatch = vm.Evaluate(syntax); }'+$nl+'            finally { profileMark("evaluate-end"); }')),
        @('                else startupStage = stage;','                else { startupStage = stage; profileMark("stage-" + stage); }'),
        @('            startupStage = null;',$ending.Replace("`r`n","`n").Replace("`n",$nl))
    )
    function ValidatePairs($specs){foreach($pair in $specs){if($pair.Count -ne 2 -or $pair[0] -isnot [string] -or $pair[1] -isnot [string]){throw 'Exactly two string members per source projection required'}}}
    ValidatePairs $pairs
    $badPairs=[Collections.Generic.List[object]]::new();$badPairs.Add(@('old','replacement','unexpected'))
    $shapeRejected=$false;try{ValidatePairs $badPairs}catch{$shapeRejected=$true}
    if(!$shapeRejected){throw 'Malformed projection shape accepted'}
    function Project([string]$text){
        if($text.Contains('LEA_TRUSTED_BOOT')){throw 'Already profiled source'}
        $updated=$text
        foreach($pair in $pairs){if([regex]::Matches($updated,[regex]::Escape($pair[0])).Count -ne 1){throw 'Unique trusted bootstrap boundary required'};$updated=$updated.Replace($pair[0],$pair[1])}
        $inverse=$updated;for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
        if($inverse -cne $text){throw 'Buffered bootstrap profile inverse differs'}
        foreach($operation in @('var markers = new Bm();','Vo.Of(1000)','new Jint.Engine(options)','factory(settings => Vo.At(settings, 1000))','new Dc().Bind(vm);','var syntax = prepared.Value;','dispatch = vm.Evaluate(syntax);','markers.End();','bridge.Begin();')){
            if([regex]::Matches($updated,[regex]::Escape($operation)).Count -ne [regex]::Matches($text,[regex]::Escape($operation)).Count){throw 'Critical original bootstrap operation removed or duplicated'}
        }
        return $updated
    }
    $updated=Project $original;$negative=0
    $invalid=[Collections.Generic.List[string]]::new();$invalid.Add($updated)
    for($i=0;$i -lt 5;$i++){$invalid.Add($original.Replace($pairs[$i][0],'MISSING_PROFILE_BOUNDARY'))}
    foreach($bad in $invalid){$rejected=$false;try{$null=Project $bad}catch{$rejected=$true};if(!$rejected){throw 'Invalid profile input accepted'};$negative++}
    $featurePath='I:\LeaBuildStorage\is-connected-source-20261102-v1\source.json'
    if((Get-FileHash $featurePath).Hash -cne 'B4BEA8392042E88DE4D833AA158B35869811A95AC9D5D8CA5AF8D02B53FA359E'){throw 'Exact feature resource proof required'}
    $feature=Get-Content $featurePath -Raw|ConvertFrom-Json
    $resource=@($feature.files|Where-Object name -CEQ 'Dom.compact.js')
    if(!$feature.verified -or !$feature.inverseVerified -or $resource.Count -ne 1 -or (Get-FileHash $resource[0].path).Hash -cne $resource[0].sha256){throw 'Feature resource boundary incomplete'}
    New-Item -ItemType Directory -Path $out|Out-Null
    $path=Join-Path $out '8.cs';[IO.File]::WriteAllText($path,$updated,[Text.UTF8Encoding]::new($false))
    $to=Join-Path $out 'Dom.compact.js';Copy-Item -LiteralPath $resource[0].path -Destination $to
    if((Get-FileHash $to).Hash -cne $resource[0].sha256){throw 'Profile resource copy differs'}
    @{verified=$true;inverseVerified=$true;negativeControls=$negative;pairShapeControls=1;criticalOperationsVerified=$true;diagnosticOnly=$true;budgetsChanged=$false;pageCodeLogged=$false;compiled=$false;executed=$false;files=@(@{name='8.cs';path=$path;originalSha256=$originalHash;sha256=(Get-FileHash $path).Hash},@{name='Dom.compact.js';path=$to;originalSha256=$resource[0].originalSha256;sha256=$resource[0].sha256});scope='Buffered process CPU/wall/GC stamps for trusted bootstrap only; seven reversible source projections with critical-operation counts, original1000ms/guards unchanged. Metrics and buffering perturb timing; not a fix or release evidence.'}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $out 'source.json') -Encoding utf8
    Get-FileHash (Join-Path $out 'source.json')
}finally{$hold.Dispose()}
