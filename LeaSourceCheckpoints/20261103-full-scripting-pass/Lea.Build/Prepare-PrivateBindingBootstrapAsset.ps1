$ErrorActionPreference='Stop'
$base='I:\LeaBuildStorage\private-binding-component-sources-20261103-v1'
$out='I:\LeaBuildStorage\private-binding-component-sources-20261103-v2'
if(Test-Path -LiteralPath $out){throw 'Preserve existing source revision'}
$receipt=Join-Path $base 'sources.json'
if((Get-FileHash $receipt).Hash -cne 'BE90716A9237699ED45E07EC69DC5CCC649E5F9E189C1E78C6C0EF325ABF436F'){throw 'Prior probe source proof differs'}
$prior=Get-Content -LiteralPath $receipt -Raw|ConvertFrom-Json -AsHashtable
if(!$prior.verified -or $prior.files.Count -ne 6 -or $prior.assertionsChanged){throw 'Prior projection incomplete'}
$asset='I:\LeaBuildStorage\private-binding-source-20261102-v1\Dom.compact.js'
$old='EC4B6513A77230C3B8439568CC3FED41B7712CA3779182D38076880DCC83BC8A'
$new='196BCE962F2C2F375C77644D0AE19485820F599170D4D80013829A6ECD2886E3'
if((Get-FileHash $asset).Hash -cne $new){throw 'Current generated asset differs'}
$holds=[Collections.Generic.List[IDisposable]]::new()
try{
    foreach($file in $prior.files){
        $holds.Add([IO.File]::Open($file.path,'Open','Read','Read'))
        if((Get-FileHash $file.path).Hash -cne $file.sha256){throw 'Prior source changed'}
    }
    $bootstrap=@($prior.files|Where-Object {[IO.Path]::GetFileName($_.path) -ceq 'BootstrapBundleHost.cs'})
    if($bootstrap.Count -ne 1){throw 'Unique bootstrap probe required'}
    $original=[IO.File]::ReadAllText($bootstrap[0].path)
    function Project([string]$text){
        if([regex]::Matches($text,$old).Count -ne 1 -or $text.Contains($new)){throw 'Unique prior resource pin required'}
        $updated=$text.Replace($old,$new)
        if($updated.Replace($new,$old) -cne $text){throw 'Resource pin inverse differs'}
        return $updated
    }
    $updated=Project $original;$negative=0
    foreach($invalid in @($original.Replace($old,'MISSING'),($original+$old),($original+$new))){try{$null=Project $invalid}catch{$negative++}}
    if($negative -ne 3){throw 'Resource pin negative controls failed'}
    $rows=[Collections.Generic.List[object]]::new()
    foreach($file in $prior.files){
        $relative=[IO.Path]::GetRelativePath($base,$file.path)
        if([IO.Path]::IsPathRooted($relative) -or $relative.Replace('\','/').Split('/') -contains '..'){throw 'Source path escape'}
        $to=Join-Path $out $relative;New-Item -ItemType Directory -Path (Split-Path $to) -Force|Out-Null
        if($file.path -ceq $bootstrap[0].path){[IO.File]::WriteAllText($to,$updated,[Text.UTF8Encoding]::new($false));if([IO.File]::ReadAllText($to).Replace($new,$old) -cne $original){throw 'Written source inverse differs'}}
        else{Copy-Item -LiteralPath $file.path -Destination $to;if((Get-FileHash $to).Hash -cne $file.sha256){throw 'Unrelated source changed'}}
        $row=[ordered]@{};foreach($key in $file.Keys){$row[$key]=$file[$key]};$row.path=$to;$row.sha256=(Get-FileHash $to).Hash;$rows.Add($row)
    }
    $prior.files=$rows
    $prior.resourceIdentityChange=@{verified=$true;old=$old;replacement=$new;asset=$asset;inverseVerified=$true;negativeControls=3;changedSources=1;priorProofSha256=(Get-FileHash $receipt).Hash}
    $prior.runnerSha256=(Get-FileHash $PSCommandPath).Hash
    $prior|ConvertTo-Json -Depth 8|Set-Content (Join-Path $out 'sources.json') -Encoding utf8
    Get-FileHash (Join-Path $out 'sources.json')
}finally{foreach($hold in $holds){$hold.Dispose()}}
