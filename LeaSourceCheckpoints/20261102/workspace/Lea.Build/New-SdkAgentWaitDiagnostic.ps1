function New-SdkAgentWaitDiagnostic {
    param([Parameter(Mandatory)][string]$Text)
    $old='Check(view.Xo != null && view.Xo.Ok, "native nonblocking fixture loads");'
    $new='Check(view.Xo != null && view.Xo.Ok, "native nonblocking fixture loads" + (view.Xo == null ? "; result=null" : view.Xo.Ok ? "" : "; status=" + view.Xo.S + "; message=" + view.Xo.M));'
    function Project([string]$value){
        if([regex]::Matches($value,[regex]::Escape($old)).Count -ne 1 -or $value.Contains($new)){throw 'Unique original nonblocking fixture assertion required'}
        $updated=$value.Replace($old,$new)
        if($updated.Replace($new,$old) -cne $value){throw 'Agent wait failure diagnostic inverse differs'}
        return $updated
    }
    $updated=Project $Text;$negative=0
    foreach($invalid in @($Text.Replace($old,''),($Text+$old),($Text+$new))){try{$null=Project $invalid}catch{$negative++}}
    if($negative -ne 3){throw 'Agent wait diagnostic negative controls failed'}
    return @{text=$updated;inverseVerified=$true;negativeControls=3;assertionConditionUnchanged=$true;successMessageUnchanged=$true;failureOnlyStatusMessage=$true;newRuntimeCalls=$false;budgetsChanged=$false}
}
