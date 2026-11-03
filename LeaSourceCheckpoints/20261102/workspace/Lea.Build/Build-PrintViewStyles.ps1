$viewProofPath=Join-Path $workspace 'continuity-evidence/native-selection-view-v1/result.json'
Hold $viewProofPath 'FEA23AD0926870E0A0B77B2C16450DC802A09B385F0E28F67D01AAC77A3BB5FC'
$viewProof=Get-Content $viewProofPath -Raw|ConvertFrom-Json
if(!$viewProof.passed -or !$viewProof.inputsUnchanged -or $viewProof.ownerAttention){throw 'Mature view proof incomplete'}
$viewKnown=@{};foreach($pin in $viewProof.inputs){$viewKnown[$pin.path]=$pin.sha256}
$recipe=Join-Path $workspace 'continuity-evidence/native-selection-view-v1/view.rsp'
Hold $recipe '788A7ACB5906115BE0FF86BA17152E4941C8CFA40E327644593D088876742F9C'
$preparation=Join-Path $workspace 'continuity-evidence/print-view-styles-source-v1/result.json';Hold $preparation
$viewProjection=Get-Content $preparation -Raw|ConvertFrom-Json
if(!$viewProjection.inverseVerified -or $viewProjection.source.Hash -cne '3DBCDB0363BA5C291807D395D5F3FFB50956F697536A71FD3C0A2B00EE1744AD' -or $viewProjection.candidate.Hash -cne '24C58C887DAD9DC69E310C1B35A4299B918F14818D552126DA21182B14D2AC51'){throw 'View projection differs'}
Hold $viewProjection.fragment.Path $viewProjection.fragment.Hash
$view=Join-Path $out 'Lea.Win.dll'
$viewOptions=[Collections.Generic.List[string]]::new()
$null=New-Item -ItemType Directory -Path (Join-Path $out 'view-source')
foreach($line in Get-Content $recipe){
    if($line.StartsWith('/out:')){$line='/out:"'+$view+'"'}
    elseif($line -match '^"(.+\.cs)"$'){
        $from=$Matches[1];if(!$viewKnown.ContainsKey($from)){throw 'Unpinned mature view source'}
        Hold $from $viewKnown[$from]
        if([IO.Path]::GetFileName($from) -ceq 'V.cs'){$from=$viewProjection.candidate.Path;Hold $from $viewProjection.candidate.Hash}
        $to=Join-Path $out ('view-source/'+[IO.Path]::GetFileName($from))
        Copy-Item -LiteralPath $from -Destination $to;Hold $to (Get-FileHash $from).Hash;$line='"'+$to+'"'
    }elseif($line -match '^/reference:"([^"]+)"$'){
        $from=$Matches[1]
        if([IO.Path]::GetFileName($from) -ceq 'Lea.Engine.dll'){$from=$core;Hold $from 'A85B02C678862D75FA764397C6BB6B3D7B964ECB0C8E13722F6D17DC44AA3791'}
        else{if(!$viewKnown.ContainsKey($from)){throw 'Unpinned mature view reference'};Hold $from $viewKnown[$from]}
        $line='/reference:"'+$from+'"'
    }
    $viewOptions.Add($line)
}
Compile 'compile-print-view' @($viewOptions)
Hold $view
