$coreProofPath=Join-Path $workspace 'continuity-evidence/ordered-flow-v6/result.json'
Hold $coreProofPath '5C93311ACDA6D4509580C30E6EB8D85C458C28C3CBA6BC6247D93100EBAB9D7E'
$coreProof=Get-Content $coreProofPath -Raw|ConvertFrom-Json
if(!$coreProof.passed -or !$coreProof.inputsUnchanged -or $coreProof.ownerAttention){throw 'Mature core proof incomplete'}
$coreKnown=@{};foreach($pin in $coreProof.inputs){$coreKnown[$pin.path]=$pin.sha256}
$recipe=Join-Path $workspace 'continuity-evidence/ordered-flow-v6/compile-engine.rsp'
Hold $recipe '97CD4568C0691EF7A77D92B8619548557BE1DACB669C67A3C9B850CBF1D80430'
$priorPreparation=Join-Path $workspace 'continuity-evidence/print-style-core-source-v1/result.json';Hold $priorPreparation
$priorProjection=Get-Content $priorPreparation -Raw|ConvertFrom-Json
if(!$priorProjection.inverseVerified -or $priorProjection.source.Hash -cne 'E07F8014A89FD7758A927C2EC36DE379BF8097E93DA18D23968AABA4EB60A905' -or $priorProjection.candidate.Hash -cne 'CE230E646EB9A47743E90186C83D91FBD45B3C5A3429EBDAB0D20B0BBBDB7540'){throw 'Original style source projection differs'}
Hold $priorProjection.candidate.Path $priorProjection.candidate.Hash
$corePreparation=Join-Path $workspace 'continuity-evidence/print-style-core-source-v2/result.json';Hold $corePreparation
$projection=Get-Content $corePreparation -Raw|ConvertFrom-Json
if(!$projection.inverseVerified -or $projection.source.Hash -cne 'CE230E646EB9A47743E90186C83D91FBD45B3C5A3429EBDAB0D20B0BBBDB7540' -or $projection.candidate.Hash -cne '859BC6D6F3922903C4AC4CEE33144BBAED9F27625951E1DFC498BD54DB451D77'){throw 'Style source projection differs'}
Hold $projection.fragment.Path $projection.fragment.Hash
Hold $projection.candidate.Path $projection.candidate.Hash
$fontPreparation=Join-Path $workspace 'continuity-evidence/print-style-core-source-v3/result.json';Hold $fontPreparation
$projection=Get-Content $fontPreparation -Raw|ConvertFrom-Json
if(!$projection.inverseVerified -or $projection.source.Hash -cne '859BC6D6F3922903C4AC4CEE33144BBAED9F27625951E1DFC498BD54DB451D77' -or $projection.candidate.Hash -cne 'D8B38DB6B18DBF12EF94A93F31AFA856D79968E5A6BECE39166F0B3EC05C5681'){throw 'Font source projection differs'}
Hold $projection.fragment.Path $projection.fragment.Hash
$core=Join-Path $out 'Lea.Engine.dll'
$coreOptions=[Collections.Generic.List[string]]::new()
$null=New-Item -ItemType Directory -Path (Join-Path $out 'core-source')
foreach($line in Get-Content $recipe){
    if($line.StartsWith('/out:')){$line='/out:"'+$core+'"'}
    elseif($line -match '^"(.+\.cs)"$'){
        $from=$Matches[1]
        if(!$coreKnown.ContainsKey($from)){throw 'Unpinned mature core source'}
        Hold $from $coreKnown[$from]
        if([IO.Path]::GetFileName($from) -ceq 'E.cs'){$from=$projection.candidate.Path;Hold $from $projection.candidate.Hash}
        $to=Join-Path $out ('core-source/'+[IO.Path]::GetFileName($from))
        Copy-Item -LiteralPath $from -Destination $to;Hold $to (Get-FileHash $from).Hash
        $line='"'+$to+'"'
    }elseif($line -match '^/reference:"([^"]+)"$'){
        $from=$Matches[1];if(!$coreKnown.ContainsKey($from)){throw 'Unpinned mature core reference'}
        Hold $from $coreKnown[$from]
    }
    $coreOptions.Add($line)
}
Compile 'compile-style-core' @($coreOptions)
Hold $core
