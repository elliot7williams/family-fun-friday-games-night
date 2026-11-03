param([Parameter(Mandatory)][ValidateSet('Stage','Audit','Run')][string]$Kind,[ValidatePattern('^[A-F0-9]{64}$')][string]$StagingHash,[ValidatePattern('^[A-F0-9]{64}$')][string]$AuditHash,[switch]$MainOnly)
$ErrorActionPreference='Stop'
$spec=@{
    Stage=@('Stage-SdkFullScriptRuntime.ps1','B80AAA7346DF6890AD8D02E0D20A89F44F37B3757A6974FC59B486DD20CD3DB8')
    Audit=@('Audit-SdkFullScriptStaging.ps1','E356CD62CD5A3212613ADA2777D80E0AFEAEAECCE62DD39CECC3E27B52300511')
    Run=@('Test-OwnedSdkFullScript.ps1','4209E6B3732C06FAC72DFE54CD9F42ABCDCDA512283E58D2DA93AB13ECC137B2')
}[$Kind]
$from=Join-Path $PSScriptRoot $spec[0]
if((Get-FileHash $from).Hash -cne $spec[1]){throw 'Runtime tool template differs'}
$to=Join-Path $PSScriptRoot ($Kind+'-RetirementRuntime-v'+$(if($MainOnly){4}else{3})+'.generated.ps1')
if(Test-Path $to){throw 'Preserve generated runtime tool'}
$pairs=[Collections.Generic.List[object]]::new()
$pairs.Add(@('continuity-evidence/contains-recovery-full-prefix-v1','continuity-evidence/retirement-full-prefix-v1'))
if($Kind -ceq 'Stage'){
    $pairs.Add(@('I:\LeaBuildStorage\contains-recovery-assembly-20261102-v1','I:\LeaBuildStorage\retirement-prefix-diagnostic-20261102-v3'))
    $pairs.Add(@('I:\LeaBuildStorage\contains-recovery-compile-20261102-v1','I:\LeaBuildStorage\retirement-prefix-compile-20261102-v3'))
    $pairs.Add(@('AFFDEA3E82AE42779C1ABA5292D8E40847D72CB4A8C4E8B61F0B1E99E2BE196F','CF629A10368624EB10E144D5C8D861DC86E097246DAFD6EA7EE4631B5468FEBD'))
    $pairs.Add(@('E46CFBB42E468672220B4197AFC9CE6F85AE2832CD025D86A52C9FA01D8BCCC8','376889BA29A128651989BD328958740380B2F53BC4A7DA52F2BE6DF1B947CEC4'))
    $pairs.Add(@('$assembly.changes.Count -ne 4','$assembly.changes.Count -ne 5'))
}else{
    if(!$StagingHash){throw 'Exact staging receipt required'}
    $pairs.Add(@('18C161A100C9991379C1C85925B37212B60F711D83BCBBDC5964F93DD62B9C5F',$StagingHash))
    if($Kind -ceq 'Audit'){$pairs.Add(@('474BEA6438815C31BC9C5ED4CA8B1283ACBF60BF3D6A8E7965DF987D0BDAA475','E3662C64687A7F224293BD77E61C373BB5CA2532517BD374C6D85E8E96A57212'))}
    else{if(!$AuditHash){throw 'Exact embedded audit required'};$pairs.Add(@('AC4F55FD88AF47270B9C8BFE6841674368CC4472EA393006C492BF57310670F3',$AuditHash));$pairs.Add(@('continuity-evidence/contains-recovery-full-prefix-owned-v1','continuity-evidence/retirement-full-prefix-owned-v1'))}
}
if($MainOnly){foreach($pair in $pairs){
    $pair[1]=$pair[1].Replace('continuity-evidence/retirement-full-prefix-v1','continuity-evidence/retirement-full-prefix-v2').Replace('continuity-evidence/retirement-full-prefix-owned-v1','continuity-evidence/retirement-full-prefix-owned-v2').Replace('retirement-prefix-diagnostic-20261102-v3','retirement-prefix-main-only-20261102-v4').Replace('retirement-prefix-compile-20261102-v3','retirement-prefix-compile-20261102-v4').Replace('CF629A10368624EB10E144D5C8D861DC86E097246DAFD6EA7EE4631B5468FEBD','043777669F93998DC2D7FA51F66DBAAB7FD6B9612CD568AEFAE65C512B2D2C6C').Replace('376889BA29A128651989BD328958740380B2F53BC4A7DA52F2BE6DF1B947CEC4','DF32874875EF2F080A1DBD6B09C4B2AA131D8E5EA34E225B26683082D2B32222').Replace('E3662C64687A7F224293BD77E61C373BB5CA2532517BD374C6D85E8E96A57212','A16FAEBDA9768980825D6C05065862AB5DB68E5F7C61B771BB00CC80C1CE192E').Replace('$assembly.changes.Count -ne 5','$assembly.changes.Count -ne 4')
}}
$original=[IO.File]::ReadAllText($from);$text=$original
foreach($pair in $pairs){if([regex]::Matches($text,[regex]::Escape($pair[0])).Count -ne 1){throw ('Unique runtime projection required: '+$pair[0])};$text=$text.Replace($pair[0],$pair[1])}
$inverse=$text;for($i=$pairs.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($pairs[$i][1],$pairs[$i][0])}
if($inverse -cne $original){throw 'Runtime tool inverse differs'}
[IO.File]::WriteAllText($to,$text)
Get-FileHash $to
