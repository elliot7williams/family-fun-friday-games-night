param([ValidateRange(1,4)][int]$Version=1)
$ErrorActionPreference='Stop'
$base='I:\LeaBuildStorage\historical-script-dependencies-20261101-v'+$Version
$receipt=Join-Path $base 'result.json'
$hold=[IO.File]::Open($receipt,'Open','Read','Read')
try{
    $r=Get-Content $receipt -Raw|ConvertFrom-Json
    if(!$r.verified -or $r.error){throw 'Archive batch incomplete'}
    $count=0;$bytes=0
    foreach($record in $r.records){
        if(!$record.removed -or (Get-FileHash $record.archive).Hash -cne $record.archiveSha256 -or (Get-FileHash $record.receipt).Hash -cne $record.receiptSha256){throw 'Archive/retained receipt changed'}
        $z=[IO.Compression.ZipFile]::OpenRead($record.archive)
        try{
            if($z.Entries.Count -ne $record.files.Count){throw 'Archive inventory changed'}
            foreach($f in $record.files){
                if(Test-Path -LiteralPath $f.path){throw 'Removed copy still present'}
                if(!$f.sddl -or !$f.lastWriteUtc){throw 'Restore metadata missing'}
                $m=@($z.Entries|Where-Object FullName -ceq $f.relative)
                if($m.Count -ne 1 -or $m[0].Length -ne $f.bytes){throw 'Member length/uniqueness differs'}
                $s=$m[0].Open();try{$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))}finally{$s.Dispose()}
                if($hash -cne $f.sha256){throw 'Archived member hash differs'}
                $count++;$bytes+=$f.bytes
            }
        }finally{$z.Dispose()}
    }
    if($bytes -ne $r.bytesRemoved){throw 'Removed-byte total differs'}
    @{verified=$true;receiptSha256=(Get-FileHash $receipt).Hash;archives=$r.records.Count;members=$count;bytes=$bytes;scope='Post-removal archive member readback, original receipt hashes, source absence, restore metadata presence; not engine runtime qualification'}|ConvertTo-Json|Set-Content (Join-Path $base 'audit.json') -Encoding utf8
    Get-Content (Join-Path $base 'audit.json')
}finally{$hold.Dispose()}
