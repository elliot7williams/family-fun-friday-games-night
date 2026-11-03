function Test-LeaSuiteFooter {
    param([string]$Path,[string]$Expected)
    if([String]::IsNullOrEmpty($Expected)){throw 'Exact expected suite footer is required'}
    $reader=[IO.File]::OpenText($Path)
    $last=$null;$matches=0
    try{
        while($null -ne ($line=$reader.ReadLine())){
            $last=$line
            if([String]::Equals($line,$Expected,[StringComparison]::Ordinal)){$matches++}
        }
    }finally{$reader.Dispose()}
    return $matches -eq 1 -and [String]::Equals($last,$Expected,[StringComparison]::Ordinal)
}
