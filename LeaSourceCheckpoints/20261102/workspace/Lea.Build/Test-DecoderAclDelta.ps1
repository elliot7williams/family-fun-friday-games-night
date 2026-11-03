function Test-DecoderAclDelta {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$ExpectedDecoderPath,
        [Parameter(Mandatory)][string]$Before,
        [Parameter(Mandatory)][string]$After,
        [Parameter(Mandatory)][string]$ProfileName,
        [switch]$TemporaryProfile
    )
    # This exception is only for the established shared image profile, never scripts.
    if ($TemporaryProfile -or $ProfileName -cne 'Lea.Image.Decoder') { return $false }
    if (![IO.Path]::IsPathFullyQualified($Path) -or
        ![IO.Path]::IsPathFullyQualified($ExpectedDecoderPath) -or
        ![string]::Equals([IO.Path]::GetFullPath($Path), [IO.Path]::GetFullPath($ExpectedDecoderPath), [StringComparison]::OrdinalIgnoreCase) -or
        [IO.Path]::GetFileName($Path) -cne 'Lea.Image.exe' -or $Before -ceq $After) { return $false }
    $sid = 'S-1-15-2-3597768014-2038560105-1077951562-531713394-1456559389-3372869253-1355503848'
    try {
        # FileSecurity can normalize inheritance flags on files; inspect raw ACEs first.
        $raw = [Security.AccessControl.RawSecurityDescriptor]::new($After)
        $rawRules = @($raw.DiscretionaryAcl | Where-Object { $_ -is [Security.AccessControl.KnownAce] -and $_.SecurityIdentifier.Value -ceq $sid })
        if ($rawRules.Count -ne 1 -or $rawRules[0].AceFlags -ne [Security.AccessControl.AceFlags]::None) { return $false }
        $original = [Security.AccessControl.FileSecurity]::new()
        $original.SetSecurityDescriptorSddlForm($Before)
        if (@($original.GetAccessRules($true, $true, [Security.Principal.SecurityIdentifier]) | Where-Object { $_.IdentityReference.Value -ceq $sid }).Count) { return $false }
        $security = [Security.AccessControl.FileSecurity]::new()
        $security.SetSecurityDescriptorSddlForm($After)
        $rules = @($security.GetAccessRules($true, $true, [Security.Principal.SecurityIdentifier]) | Where-Object { $_.IdentityReference.Value -ceq $sid })
        $rights = [Security.AccessControl.FileSystemRights]::ReadAndExecute -bor [Security.AccessControl.FileSystemRights]::Synchronize
        if ($rules.Count -ne 1 -or $rules[0].FileSystemRights -ne $rights -or
            $rules[0].AccessControlType -ne 'Allow' -or $rules[0].IsInherited -or
            $rules[0].InheritanceFlags -ne 'None' -or $rules[0].PropagationFlags -ne 'None') { return $false }
        $security.RemoveAccessRuleSpecific($rules[0])
        return $security.GetSecurityDescriptorSddlForm([Security.AccessControl.AccessControlSections]::All) -ceq $Before
    } catch { return $false }
}
