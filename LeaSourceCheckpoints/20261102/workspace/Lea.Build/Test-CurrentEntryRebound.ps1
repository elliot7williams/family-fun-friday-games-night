$ErrorActionPreference='Stop'
if(!$global:LeaSuitePhaseBuildResult.passed -or $global:LeaCompositeProcessLeases.Count -or (Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)){throw 'Require clean retained V2 build owner'}
$build=$global:LeaSuitePhaseBuildResult
if(!$build.runtime.EndsWith('\current-lazy-suite-v2\runtime',[StringComparison]::OrdinalIgnoreCase)){throw 'Wrong candidate'}
if($build.sha256 -cne '86D79BE8D3D853134705BA3BA55FB13652DFA1F0B4425F2D9582D7653B48E13C'){throw 'Wrong suite binary'}
$outRoot=Split-Path $build.runtime
$out=Join-Path $outRoot 'entry-rebound-audit.json'
if(Test-Path -LiteralPath $out){throw 'Preserve targeted test evidence'}
$before=@(foreach($file in $build.files){
    if((Get-FileHash -LiteralPath $file.path).Hash -cne $file.sha256){throw 'Candidate changed'}
    @{path=$file.path;sha256=$file.sha256;sddl=(Get-Acl -LiteralPath $file.path).Sddl}
})
$runs=[Collections.Generic.List[object]]::new()
$result=[ordered]@{candidate=$build.runtime;runs=$runs;passed=$false;error=$null;before=$before;limitsChanged=$false;sourceAssertionsRemoved=0}
try{
    $run=Run 'entry-rebound' $build.binary @(('"'+$build.fixtureSource+'"'),'--worker-entry') 60
    $footer=@(Get-Content -LiteralPath $run.stdout|Where-Object {$_.StartsWith('PASS Lea.Script worker entry: ')})
    $result.passed=$run.passed -and $footer.Count -eq 1
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.changedFiles=@(foreach($file in $before){$acl=(Get-Acl -LiteralPath $file.path).Sddl;$hash=(Get-FileHash -LiteralPath $file.path).Hash;if($acl -cne $file.sddl -or $hash -cne $file.sha256){@{path=$file.path;bytesChanged=$hash -cne $file.sha256;before=$file.sddl;after=$acl}}})
    $result.ownerAttention=$global:LeaCompositeProcessLeases.Count -gt 0 -or [bool](Get-Variable LeaCompositeUnverifiedChild -Scope Global -ErrorAction SilentlyContinue)
    if($result.changedFiles.Count -or $result.ownerAttention){$result.passed=$false}
    $result|ConvertTo-Json -Depth 8|Set-Content -LiteralPath $out
    $global:LeaCurrentEntryResult=$result
    [pscustomobject]$result|Select-Object passed,error,ownerAttention,changedFiles|ConvertTo-Json -Depth 5
}
