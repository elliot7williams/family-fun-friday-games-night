$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
$from='L:\Lea-source-backup-archive-20261029-uia\runtime-8033-matched-source-20261027.zip'
$to=Join-Path $root 'continuity-evidence/runtime-8033-matched-source-20261027.zip'
$hash='84179CBDF6C3674E43C886FCF72B5C048056425B2D029E2CE3DBBE4CD719AEB9'
foreach($path in @($from,$to)){
    $walk=[IO.Path]::GetFullPath($path)
    while($walk){
        if((Test-Path -LiteralPath $walk) -and ((Get-Item -LiteralPath $walk -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)){throw 'Linked archive path refused'}
        $walk=[IO.Path]::GetDirectoryName($walk)
    }
}
if(Test-Path -LiteralPath $to){
    if((Get-FileHash $to).Hash -cne $hash){throw 'Preserve differing local archive'}
    Write-Output 'Verified archive already staged';return
}
if((Get-FileHash $from).Hash -cne $hash){throw 'Relocated archive differs'}
if([IO.DriveInfo]::new('C:\').AvailableFreeSpace -lt (Get-Item $from).Length+600MB){throw 'Restaging must preserve build reserve'}
$archiveStream=[IO.File]::Open($from,'Open','Read','Read')
try{
    $output=[IO.File]::Open($to,'CreateNew','Write','None')
    try{$archiveStream.CopyTo($output);$output.Flush($true)}finally{$output.Dispose()}
}finally{$archiveStream.Dispose()}
if((Get-FileHash $to).Hash -cne $hash){throw 'Restaged archive differs; preserve evidence'}
Write-Output 'Verified archive staged for historical package helpers; L: copy retained'
