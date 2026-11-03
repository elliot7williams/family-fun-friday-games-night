$proof=Join-Path $root 'continuity-evidence/decoder-owner-retirement-v10/result.json'
Hold $proof 'EE71940D73E7A63839DF9806155090858225BC254A6370360ACC7F96EC1B3EC6'
$qualified=Get-Content $proof -Raw|ConvertFrom-Json
if(!$qualified.passed -or !$qualified.inputsUnchanged -or $qualified.ownerAttention){throw 'Qualified view source proof incomplete'}
$recipe=Join-Path $root 'continuity-evidence/decoder-owner-retirement-v10/view.rsp'
Hold $recipe '28F2269D6DA30433528F72EC12F1E4C8C7E8157E79F037EAC8E691B909566B64'
$view=Join-Path $out 'Lea.Win.dll';$stage=Join-Path $out 'view-source';$null=New-Item -ItemType Directory -Path $stage
$options=[Collections.Generic.List[string]]::new()
foreach($line in Get-Content $recipe){
    if($line.StartsWith('/out:')){$line='/out:"'+$view+'"'}
    elseif($line -match '^/reference:"(.+)"$'){
        $from=$Matches[1]
        if([IO.Path]::GetFileName($from) -eq 'Lea.Engine.dll'){$from=$copy}
        else{$pins=@($qualified.inputs|Where-Object path -ceq $from|Select-Object -ExpandProperty sha256 -Unique);if($pins.Count -ne 1){throw 'View reference pin missing'};Hold $from $pins[0]}
        $line='/reference:"'+$from+'"'
    }elseif($line -match '^"(.+\.cs)"$'){
        $from=$Matches[1];$pins=@($qualified.inputs|Where-Object path -ceq $from|Select-Object -ExpandProperty sha256 -Unique)
        if($pins.Count -ne 1){throw 'View source pin missing'};Hold $from $pins[0]
        $to=Join-Path $stage ([IO.Path]::GetFileName($from));Copy-Item $from $to;Hold $to $pins[0];$line='"'+$to+'"'
    }
    $options.Add($line)
}
$response=Join-Path $out 'font-view.rsp';[IO.File]::WriteAllLines($response,$options);Hold $response
$wrapper=Join-Path $PSScriptRoot 'Invoke-PublicCompiler.20261026.ps1';Hold $wrapper 'C3EBB3801FAFB80DA031AAB69FE4837EEB4E4EECD3907F9704F9D6186FA42F0A'
Hold (Join-Path $PSScriptRoot 'PublicCompiler.cs') 'D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB'
$built=Invoke-OwnedTestRun -Name 'build-font-view' -Application (Get-Process -Id $PID).Path -Arguments @('-NoProfile','-File',('"'+$wrapper+'"'),'-ResponseFile',('"'+$response+'"'),'-ExpectedResponseSha256',(Get-FileHash $response).Hash,'-ExpectedSourceSha256','D237E7BDD665521269B91824A8250F87C59156D04C26449C88EB429CB36AA1EB') -WorkingDirectory $out -OutputDirectory $out -Seconds 120 -Records $runs
if(!$built.passed){throw 'Matched font view compilation failed'}
Hold $view;$result.candidateViewSha256=(Get-FileHash $view).Hash
