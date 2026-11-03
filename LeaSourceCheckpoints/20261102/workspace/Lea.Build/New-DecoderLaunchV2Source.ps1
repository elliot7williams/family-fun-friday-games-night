function Add-DecoderLaunchStages {
    param([string]$Original,[string]$Kind)
    if($Kind -eq 'Di'){
        $edits=@(
            @('            var full = Rp.Prepare(P(path));', "            MarkOwner(`"prep`" );`r`n            var full = Rp.Prepare(P(path));`r`n            MarkOwner(`"prepared`" );"),
            @('            lock (state)', "            MarkOwner(`"lock`" );`r`n            lock (state)"),
            @('                    Drop();', "                    MarkOwner(`"drop`" );`r`n                    Drop();`r`n                    MarkOwner(`"dropped`" );"),
            @('                    created = Rp.Open(full, args ?? "--pipe", Path.GetDirectoryName(full), limit, cpu);', ('                    var observer = Rp.StageObserver;'+"`r`n"+'                    try { Rp.StageObserver = MarkOwner; created = Rp.Open(full, args ?? "--pipe", Path.GetDirectoryName(full), limit, cpu); }'+"`r`n"+'                    finally { Rp.StageObserver = observer; }'))
        )
        # Both Open and Stop take this lock; distinguish lock waiting in either path.
        $counts=@(1,2,1,1)
    }elseif($Kind -eq 'Rp'){
        $observer=@'
        [ThreadStatic] internal static Action<string> StageObserver;
        internal static void ObserveStage(string stage)
        {
            var observer = StageObserver;
            if (observer != null) try { observer(stage); } catch (Exception) { }
        }

'@
        $edits=@(
            @('        public static Rp Open(string path, string args, string directory, long memory, int cpu)', ($observer+'        public static Rp Open(string path, string args, string directory, long memory, int cpu)')),
            @('                appSid = ContainerSid(profile);', "                ObserveStage(`"sid`" );`r`n                appSid = ContainerSid(profile);`r`n                ObserveStage(`"sid-ok`" );"),
            @('                var fileAcl = true;', "                ObserveStage(`"acl`" );`r`n                var fileAcl = true;"),
            @('                var security = new SecurityAttributes', "                ObserveStage(`"pipe`" );`r`n                var security = new SecurityAttributes"),
            @('                IntPtr attributeSize = IntPtr.Zero;', "                ObserveStage(`"attrs`" );`r`n                IntPtr attributeSize = IntPtr.Zero;"),
            @('                if (!CreateProcessAsUser(', "                ObserveStage(`"create`" );`r`n                if (!CreateProcessAsUser("),
            @('                created = true;', "                created = true;`r`n                ObserveStage(`"token`" );"),
            @('                job = Jb.Add(info.Process, memory, cpu);', "                ObserveStage(`"job`" );`r`n                job = Jb.Add(info.Process, memory, cpu);"),
            @('                if (ResumeThread(info.Thread)', "                ObserveStage(`"resume`" );`r`n                if (ResumeThread(info.Thread)"),
            @('                return worker;', "                ObserveStage(`"returned`" );`r`n                return worker;")
        )
        $counts=@(1,1,1,1,1,1,1,1,1,1)
    }else{throw 'Unknown launch source kind'}
    $updated=$Original
    for($i=0;$i -lt $edits.Count;$i++){
        $pair=$edits[$i]
        if($pair.Count -ne 2 -or [regex]::Matches($updated,[regex]::Escape($pair[0])).Count -ne $counts[$i]){throw ('Launch source anchor differs: '+$Kind+'/'+$i)}
        $updated=$updated.Replace($pair[0],$pair[1])
    }
    $inverse=$updated
    for($i=$edits.Count-1;$i -ge 0;$i--){$inverse=$inverse.Replace($edits[$i][1],$edits[$i][0])}
    if($inverse -cne $Original){throw 'Launch stage inverse differs'}
    return $updated
}
