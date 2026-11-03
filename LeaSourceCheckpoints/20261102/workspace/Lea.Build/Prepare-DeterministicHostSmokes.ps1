param([ValidateRange(1,9999)][int]$Version=1)
$ErrorActionPreference='Stop'
$template=Join-Path $PSScriptRoot 'Test-RelativeFontExtraction.generated.ps1'
if((Get-FileHash $template).Hash -cne 'EB03EDDA78D4A984AE169914283C352A704C260284F5F40BF5F990BF6D703A4D'){throw 'Smoke template differs'}
$leaf=if($Version -eq 1){'Test-DeterministicHostSmokes.generated.ps1'}else{"Test-DeterministicHostSmokes-v$Version.generated.ps1"}
$destination=Join-Path $PSScriptRoot $leaf
if(Test-Path $destination){throw 'Preserve generated smoke source'}
$text=[IO.File]::ReadAllText($template)
$text=$text.Replace('continuity-evidence/relative-font-package-extraction-v1',"continuity-evidence/deterministic-host-smokes-v$Version")
$start=$text.IndexOf('    $proofPath=')
$end=$text.IndexOf('    $result.before=', $start)
if($start -lt 0 -or $end -le $start){throw 'Template staging boundaries differ'}
$stage=@'
    $manifestPath='I:\LeaBuildStorage\deterministic-firespider-runtime-20261102-v1\runtime-manifest.json'
    Hold $manifestPath '72D5699F0A392633EF9B967BCDE5E6AB7961FC8405C5F5CB76DE15E342BA2C17'
    $manifest=Get-Content $manifestPath -Raw|ConvertFrom-Json
    if(!$manifest.verified -or $manifest.files.Count -ne 400 -or $manifest.hostSha256 -cne 'B1B290CA5E90B0F5DD2460BFA3A90DCB81EF8F0ADF5DA96599D437B36B3F416E'){throw 'Current runtime manifest differs'}
    if((Get-Volume -DriveLetter C).FileSystem -cne 'NTFS'){throw 'Native NTFS required'}
    if((Get-PSDrive C).Free -lt (600MB+$manifest.bytes)){throw 'Staging plus reserve required'}
    $source=Join-Path (Split-Path $manifestPath) 'runtime'
    foreach($file in $manifest.files){
        $name=$file.path
        if([IO.Path]::IsPathRooted($name) -or $name.Contains(':') -or $name.Contains('\') -or $name.Split('/') -contains '..'){throw 'Unsafe runtime member'}
        $from=Join-Path $source $name
        $item=Get-Item -LiteralPath $from
        while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse source refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
        Hold $from $file.sha256
        $to=Join-Path $runtime $name
        New-Item -ItemType Directory -Path (Split-Path $to) -Force|Out-Null
        Copy-Item -LiteralPath $from -Destination $to
        Hold $to $file.sha256
    }
    if(@(Get-ChildItem $runtime -Recurse -File).Count -ne 400){throw 'Staged inventory differs'}
'@
$text=$text.Substring(0,$start)+$stage+[Environment]::NewLine+$text.Substring($end)
$start=$text.IndexOf('    $result.decoderPolicyAccepted=')
$end=$text.IndexOf('    $result.ownerAttention=', $start)
if($start -lt 0 -or $end -le $start){throw 'Template ACL boundaries differ'}
$text=$text.Substring(0,$start)+'    $result.decoderPolicyAccepted=$result.before.Count -eq 400 -and !$result.aclChanges.Count'+[Environment]::NewLine+$text.Substring($end)
$text=$text.Replace('Fresh verified ZIP extraction settings/navigation/actual clean EXE smokes; current machine only, not all15 extracted, clean-PC or full release','Exact deterministic host settings/navigation/actual clean EXE smokes on fresh NTFS; not full host campaign, clean-PC or release qualification')
$tokens=$null;$errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($text,[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Generated smoke syntax invalid'}
foreach($required in @("'settings-path'","'navigation'","'exe-clean'",') 30',') 90',') 100','600MB','LeaCompositeUnverifiedChild')){if(!$text.Contains($required)){throw ('Missing original safeguard: '+$required)}}
[IO.File]::WriteAllText($destination,$text)
Get-FileHash $destination
