param([switch]$Apply,[switch]$BootstrapProbes,[switch]$LegacyNativeSuites,[switch]$LegacyNativeTail,[switch]$HistoricalStableTail,[switch]$HistoricalModernTail,[switch]$HistoricalBootstrapTail)
$ErrorActionPreference='Stop'
if(@($BootstrapProbes,$LegacyNativeSuites,$LegacyNativeTail,$HistoricalStableTail,$HistoricalModernTail,$HistoricalBootstrapTail|Where-Object {$_}).Count -gt 1){throw 'Select one archive batch'}
$root=[IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
$base=Join-Path $root 'continuity-evidence/temporal-startup-v1'
$out='I:\LeaBuildStorage\historical-script-dependencies-20261101-v1'
$cases=@(
    @('suite-phase-relocated-v2','1DF0860AAEF98A61FDBE997A31889228061A2E30D02E9D705C7F9503CA352A18'),
    @('origins-lazy-v1','DB754FA7E380A758C3DBC81900BF563BAA18BC35E7330611BD1404425CF1B751'),
    @('named-references-script-suite-v1','87E291382607C8CACAA716D022D97A1A374B1A44EA2E91A4714AAD4C6CA94F09'),
    @('numeric-references-script-suite-v1','8992F34CA977FA5DF8F65501E8495342F53D5538EC524682FC826B6B4C192071'),
    @('rebuilt-core-script-suite-v1','A261868CAA8BE939B4124874FAAF98B6AD921DACB4E26F79855C4C221B4A2127'),
    @('font-script-suite-v1','E87409ED36E23F7C968490DDF206AAEF2D9D990AC9FAED2E144C1712A72EEF7E'),
    @('document-selection-script-suite-v1','02356A28411B30ADE710F4ECC4579A32CF9991DB8A30216D3FFFFEA8B348DD4C')
)
if($BootstrapProbes){
    $base=Join-Path $root 'continuity-evidence/bootstrap-descriptor-batches-v1'
    $out='I:\LeaBuildStorage\historical-script-dependencies-20261101-v2'
    $cases=@(
        @('trusted-test-v1','13E5BCF215542B8BCDDEBE1F8E3661DF976DDAD3D62C8C57482E4B295413DC79',14),
        @('current-options-diagnostic-probes-v1','0CF43D49981762A9583A7E2052A1123E595A93DB1B5BF9B46DED4DEC50E2F396',10),
        @('paired-probes-v1','0C4205D7DD4B45E3096DA34204A467EC459EB042C4F3F07D5EF1A3A5BB9D35F9',10)
    )
}
if($LegacyNativeSuites){
    $out='I:\LeaBuildStorage\historical-script-dependencies-20261101-v3'
    $cases=@(
        @('current-native-script-suite-v1','916925B96F63ECFE75526F6BCBF742326441FF840FFD137C3CCE9CC56DA6C7CB'),
        @('current-native-script-suite-v2','0200372EDD91869A82344137C6393D767A6B4019299BF18B3FD6AE9A1CDFCD03'),
        @('current-native-script-suite-v3','A5B8F08614628C6A39BEC653140BCD0D3F6BB8AF709E2B945FD5F2D6341881CC'),
        @('current-native-script-suite-v4','098EB9883BB7F406DC1C0FD1EE846E39379595F18EDA8EAFAC4247A2CF8A7FAC'),
        @('current-native-script-suite-v5','EAC23539FB685DA9234C53CB40ECADC7642EFBED359B2943771174A231BC9698'),
        @('current-native-script-suite-v6','B04C363A4B7243A647C1A56F7B039E40D55E5414471454D5704540474CD454F4')
    )
}
if($LegacyNativeTail){
    $out='I:\LeaBuildStorage\historical-script-dependencies-20261101-v4'
    $cases=@(
        @('current-native-script-suite-v7','CF5B99D80FF4DFED679B1871AC1D08BD3268BD9BFAC5B142063A2103193AECF9'),
        @('current-native-script-suite-v8','FC4AD1E5E9249348B6A4A8D70609E3BB90ABD38E3458A52D5E52FDAB2F6543C1'),
        @('current-native-script-suite-v9','5EE6B967BAF69F336C393F6AED5E90BE8D2C69D203FA1A24B13B64A92BC53537')
    )
}
if($HistoricalStableTail){
    $out='I:\LeaBuildStorage\historical-script-dependencies-20261103-v5'
    $cases=@(
        @('current-flow-script-suite-v1','6FC8EE19097D2EA29FCC0FCFEE0F53A9C57611C706A22483801B91938FB47087'),
        @('current-lazy-suite-v1','8D446126AB56CF9656A9A2BEF9DC6FB4BDDAAB3D597765F7FB7DCC84000FD5CF'),
        @('current-lazy-suite-v2','0D71CCA8558E48610A8B90C8A330D6D0EC1B7338E1174086E10B9AE5EEF395BA'),
        @('current-native-script-suite-v10','C68D683ECC718974A7F814FF60BFEB22C62E3247B49092EF1B54C66CF1A41F63'),
        @('current-native-script-suite-v11','A8D581FBF133F59ADF238C5B65C33F4A360CC6971399300F84698A75FDCFA9FA'),
        @('relative-font-script-suite-v1','39B11B7FFB696DED434C9E0313DEED6744E0A3A08590BB8B14977C064EA6608C')
    )
}
if($HistoricalModernTail){
    $base=Join-Path $root 'continuity-evidence/modern-worker-v1'
    $out='I:\LeaBuildStorage\historical-script-dependencies-20261103-v6'
    $cases=@(
        @('generic-r2r-probes-v1','8726EC7754B1689A985EAE8060A65315780F0E5B21F7905FC9DA65A5C52F32F7',10),
        @('interpreter-suite-v1','F9158390A93D2FAC54DE0CF0B562C8E84863917849FCC0C16C7A547618143434',14),
        @('jint-cross-module-components-v1','C63CFDEDBF24F62248FA553939C6DCAD4CE7815F2FBC9F2B87E90AF5E7D68E9A',10),
        @('jint-cross-module-components-v2','10F3DBBE7BCE3A125A01A4E65B0E1D7BCC0F01A400E304856FC687880DE73DFB',10),
        @('lazy-events-binding-suite-v1','7480006ADEB365B6A712D4D9C1641C15BA4FB0EBBA41B52EB5485C502CB81C32',14),
        @('parser-probes-v1','909870CFD1B411F742DACD9BC8DC8FD0BAEB749EC3549E574B0189D52E52BD62',20),
        @('parser-production-probes-v1','7ABC97DF0382E53FD46A0718F7F8DE8F20AE98E6193FB0E01A7A30087384D5AF',10),
        @('parser-production-suite-v1','C99E8253AF8EBEBF1CC80DC16FE0CE21823C89A110B40FBCE4316627ACC0A8E3',14),
        @('r2r-probes-build-v1','2152DBD66F876026A7241B3CA1ECAA5B65E6F8F58C39C17570AD5B6A1F3C35E5',10),
        @('worker-entry-suite-v1','18C89716E575A066D38845250266B3C127C9342B59F5AE0B18EB041E06641E6E',14)
    )
}
if($HistoricalBootstrapTail){
    $base=Join-Path $root 'continuity-evidence/bootstrap-descriptor-batches-v1'
    $out='I:\LeaBuildStorage\historical-script-dependencies-20261103-v7'
    $cases=@(
        @('paired-suite-v3','D3A10CAA917C2D2D652F1B1D3D53C8E7CBE62ABA497E258A38DC4993E99DEE89',14),
        @('source-cross-module-probes-v1','93BE3A4BD4434131AC6CBCA065EAA0966339891D79CBA66FB69554C473F6BD7A',10),
        @('source-cross-module-suite-v2','779061CB3422B80038653503E7AA375521E5A868DBA30FA88A0509CC206069F5',14)
    )
}
function SafePath([string]$path){
    $full=[IO.Path]::GetFullPath($path)
    if(!$full.StartsWith($base+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Historical target outside explicit workspace'}
    $item=Get-Item -LiteralPath $full
    while($item){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse ancestor refused'};$item=if($item -is [IO.FileInfo]){$item.Directory}else{$item.Parent}}
}
$plan=@(foreach($case in $cases){
    $target=Join-Path $base $case[0];SafePath $target
    $receipt=Join-Path $target 'result.json'
    if((Get-FileHash $receipt).Hash -cne $case[1]){throw 'Historical receipt changed'}
    $r=Get-Content $receipt -Raw|ConvertFrom-Json
    if(!$r.passed -or !$r.inputsUnchanged -or $r.ownerAttention){throw 'Stable retired historical snapshot required'}
    $tree=@(Get-ChildItem -LiteralPath $target -Recurse -Force)
    if(@($tree|Where-Object {$_.Attributes -band [IO.FileAttributes]::ReparsePoint}).Count){throw 'Reparse member refused'}
    $files=@($tree|Where-Object {!$_.PSIsContainer -and $_.Name -in 'Jint.dll','Acornima.dll'})
    $expected=if($case.Count -ge 3){$case[2]}else{14}
    if($files.Count -ne $expected){throw 'Inspected historical dependency count changed'}
    @{name=$case[0];target=$target;receipt=$receipt;receiptSha256=$case[1];files=$files}
})
if(!$Apply){$plan|ForEach-Object {[pscustomobject]@{name=$_.name;files=$_.files.Count;bytes=($_.files|Measure-Object Length -Sum).Sum}};return}
if(Test-Path -LiteralPath $out){throw 'Preserve existing archive evidence'}
if((Get-PSDrive I).Free -lt 1GB){throw 'External archive reserve required'}
$processes=@(Get-CimInstance Win32_Process)
foreach($p in $processes){foreach($entry in $plan){if(($p.ExecutablePath -and $p.ExecutablePath.StartsWith($entry.target+'\',[StringComparison]::OrdinalIgnoreCase)) -or ($p.CommandLine -and $p.CommandLine.Contains($entry.target))){throw 'Historical snapshot in use'}}}
New-Item -ItemType Directory -Path $out|Out-Null
$records=[Collections.Generic.List[object]]::new()
$result=@{verified=$false;records=$records;bytesRemoved=0;freeBefore=(Get-PSDrive C).Free;githubUploaded=$false;sourceRemoved=$false;failureLogsRemoved=$false}
try{
    foreach($entry in $plan){
        $holds=[Collections.Generic.List[IDisposable]]::new()
        $rows=@()
        try{
            foreach($file in $entry.files){
                SafePath $file.FullName
                if(@(Get-Item -LiteralPath $file.FullName -Stream *|Where-Object Stream -ne ':$DATA').Count){throw 'Alternate stream preservation requires separate review'}
                $holds.Add([IO.File]::Open($file.FullName,'Open','Read','Read'))
                $rows+=@{path=$file.FullName;relative=[IO.Path]::GetRelativePath($entry.target,$file.FullName).Replace('\','/');bytes=$file.Length;sha256=(Get-FileHash $file.FullName).Hash;sddl=(Get-Acl $file.FullName).Sddl;lastWriteUtc=$file.LastWriteTimeUtc.ToString('o')}
            }
            $zip=Join-Path $out ($entry.name+'.zip')
            $archive=[IO.Compression.ZipFile]::Open($zip,'Create')
            try{foreach($row in $rows){[IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive,$row.path,$row.relative,[IO.Compression.CompressionLevel]::Optimal)|Out-Null}}finally{$archive.Dispose()}
            $archive=[IO.Compression.ZipFile]::OpenRead($zip)
            try{
                if($archive.Entries.Count -ne $rows.Count){throw 'Archive inventory differs'}
                foreach($row in $rows){
                    $matches=@($archive.Entries|Where-Object FullName -ceq $row.relative)
                    if($matches.Count -ne 1 -or $matches[0].Length -ne $row.bytes){throw 'Archive member differs'}
                    $s=$matches[0].Open();try{$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($s))}finally{$s.Dispose()}
                    if($hash -cne $row.sha256){throw 'Archive readback differs'}
                }
            }finally{$archive.Dispose()}
            $record=@{target=$entry.target;receipt=$entry.receipt;receiptSha256=$entry.receiptSha256;archive=$zip;archiveSha256=(Get-FileHash $zip).Hash;files=$rows;removed=$false}
            $records.Add($record)
            $record|ConvertTo-Json -Depth 7|Set-Content (Join-Path $out ($entry.name+'-verified.json')) -Encoding utf8
        }finally{foreach($hold in $holds){$hold.Dispose()}}
        foreach($row in $rows){
            SafePath $row.path
            if((Get-FileHash $row.path).Hash -cne $row.sha256 -or (Get-Acl $row.path).Sddl -cne $row.sddl){throw 'Source changed before removal'}
            Remove-Item -LiteralPath $row.path
            $result.bytesRemoved+=$row.bytes
        }
        $record.removed=$true
        $record|ConvertTo-Json -Depth 7|Set-Content (Join-Path $entry.target 'dependency-archive.json') -Encoding utf8
        if((Get-FileHash $zip).Hash -cne $record.archiveSha256 -or (Get-FileHash $entry.receipt).Hash -cne $entry.receiptSha256){throw 'Post-removal archive/receipt changed'}
    }
    $result.verified=$true
}catch{$result.error=$_.Exception.ToString()}
finally{
    $result.freeAfter=(Get-PSDrive C).Free
    $result|ConvertTo-Json -Depth 8|Set-Content (Join-Path $out 'result.json') -Encoding utf8
    $result|Select-Object verified,bytesRemoved,freeBefore,freeAfter,error|ConvertTo-Json
}
