$ErrorActionPreference='Stop'
$template=Join-Path $PSScriptRoot 'Test-DeterministicHostSmokes-v2.generated.ps1'
if((Get-FileHash $template).Hash -cne 'D7E21BDABCBD872FD98D8983BA14B630783F207B7EFB7161C67AD1413E147825'){throw 'Extraction template differs'}
$to=Join-Path $PSScriptRoot 'Test-DeterministicExtraction-v1.generated.ps1'
if(Test-Path $to){throw 'Preserve generated extraction test'}
$text=[IO.File]::ReadAllText($template).Replace('continuity-evidence/deterministic-host-smokes-v2','continuity-evidence/deterministic-package-extraction-v1')
$start=$text.IndexOf('    $manifestPath=')
$end=$text.IndexOf('    $result.before=', $start)
if($start -lt 0 -or $end -le $start){throw 'Extraction staging boundaries differ'}
$body=@'
    $base='I:\LeaBuildStorage\deterministic-firespider-package-20261102-v1'
    $proofPath=Join-Path $base 'result.json'
    Hold $proofPath 'D6087D9CF774E9424916F632E13DD3DFF17EFA87232B8E44F23F969A13BAE5AE'
    $proof=Get-Content $proofPath -Raw|ConvertFrom-Json
    $archive=Join-Path $base 'Firespider-Lea-deterministic-development-20261102.zip'
    Hold $archive '73FDDCC2B2F4B1EC41C2C89691D57F91A89F56A845AE54C7A4CC0486F1E7A121'
    if(!$proof.verified -or $proof.members.Count -ne 402 -or $proof.entries -ne 402){throw 'ZIP proof incomplete'}
    if((Get-Volume -DriveLetter C).FileSystem -cne 'NTFS'){throw 'Native NTFS required'}
    $bytes=($proof.members|Measure-Object bytes -Sum).Sum
    if((Get-PSDrive C).Free -lt (600MB+$bytes)){throw 'Extraction plus reserve required'}
    $names=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try{
        if($zip.Entries.Count -ne 402){throw 'ZIP inventory differs'}
        foreach($entry in $zip.Entries){
            $name=$entry.FullName
            if([IO.Path]::IsPathRooted($name) -or $name.Contains(':') -or $name.Contains('\') -or $name.Split('/') -contains '..' -or $name.Split('/') -contains '' -or !$names.Add($name)){throw 'Unsafe ZIP member'}
            $row=@($proof.members|Where-Object path -CEQ $name)
            if($row.Count -ne 1 -or $entry.Length -ne $row[0].bytes){throw 'ZIP member inventory differs'}
            $s=$entry.Open();try{if([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s)) -cne $row[0].sha256){throw 'ZIP member hash differs'}}finally{$s.Dispose()}
        }
    }finally{$zip.Dispose()}
    [IO.Compression.ZipFile]::ExtractToDirectory($archive,$runtime)
    if(@(Get-ChildItem $runtime -Recurse -File).Count -ne 402){throw 'Extracted inventory differs'}
    foreach($row in $proof.members){Hold (Join-Path $runtime $row.path) $row.sha256}
    $manifest=Get-Content (Join-Path $runtime 'lea-development-manifest.json') -Raw|ConvertFrom-Json
    if(!$manifest.developmentOnly -or $manifest.sourceIncluded -or $manifest.runtimeVersion -cne '8.0.33' -or $manifest.rollForward -cne 'Disable' -or $manifest.hostSha256 -cne 'B1B290CA5E90B0F5DD2460BFA3A90DCB81EF8F0ADF5DA96599D437B36B3F416E' -or $manifest.files.Count -ne 400){throw 'Extracted package metadata differs'}
'@
$text=$text.Substring(0,$start)+$body+[Environment]::NewLine+$text.Substring($end)
$text=$text.Replace('$result.before.Count -eq 400','$result.before.Count -eq 402')
$text=$text.Replace('Exact deterministic host settings/navigation/actual clean EXE smokes on fresh NTFS; not full host campaign, clean-PC or release qualification','Fresh deterministic ZIP extraction settings/navigation/actual EXE smokes on NTFS; not all 15 extracted workflows, clean-PC, reliability or release qualification')
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Extraction syntax invalid'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
