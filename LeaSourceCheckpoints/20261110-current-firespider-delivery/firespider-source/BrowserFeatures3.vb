Imports System.Configuration
Imports System.Text

Partial Class j
    Dim hiddenGroupTabs As New Dictionary(Of String, List(Of TabPage))(StringComparer.OrdinalIgnoreCase)
    Dim suspendedGroupUrls As New Dictionary(Of TabPage, String)
    Dim backupTimer As New Timer
    Dim formRecoveryTimer As New Timer
    Dim groupLoadTimer As New Timer
    Dim startupFormRecords As New HashSet(Of String)
    Dim consideredFormUrls As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

    Sub qfeatures3()
        v.DropDownItems.Add(qnewitem("Tab Group Manager", AddressOf q876, AddressOf q877))
        he.DropDownItems.Add(qnewitem("Keyboard Shortcut Editor", AddressOf q878, AddressOf q879))
        ot.DropDownItems.Add(qnewitem("Content Filter Manager", AddressOf q880, AddressOf q881))
        he.DropDownItems.Add(qnewitem("Site Identity Report", AddressOf q882, AddressOf q883))
        ot.DropDownItems.Add(qnewitem("Encrypted Autofill Profiles", AddressOf q884, AddressOf q885))
        ot.DropDownItems.Add(qnewitem("Scheduled Rotating Backups", AddressOf q886, AddressOf q887))
        ot.DropDownItems.Add(qnewitem("Crash Form Recovery", AddressOf q888, AddressOf q889))
        he.DropDownItems.Add(qnewitem("Page Resource Inspector", AddressOf q890, AddressOf q891))
        qfeatures4()
    End Sub

    Sub qfeature3loaded()
        Try
            startupFormRecords.Clear()
            For Each raw As String In My.Settings.fr
                startupFormRecords.Add(raw)
            Next raw
            qverticaltabs()
            backupTimer.Interval = 60000
            formRecoveryTimer.Interval = 15000
            groupLoadTimer.Interval = 1200
            AddHandler backupTimer.Tick, AddressOf qbackuptick
            AddHandler formRecoveryTimer.Tick, AddressOf qformtick
            AddHandler groupLoadTimer.Tick, AddressOf qgrouploadtick
            If My.Settings.backupEnabled = "e" Then backupTimer.Start()
            If My.Settings.frEnabled = "e" AndAlso pv = False Then formRecoveryTimer.Start()
            groupLoadTimer.Start()
            qfeature4loaded()
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    Sub qfeature3closing(ByVal finalWindow As Boolean)
        backupTimer.Stop()
        formRecoveryTimer.Stop()
        groupLoadTimer.Stop()
        For Each tabs As List(Of TabPage) In hiddenGroupTabs.Values.ToArray
            For Each tab As TabPage In tabs
                If k.TabPages.Contains(tab) = False Then k.TabPages.Add(tab)
            Next tab
        Next tabs
        hiddenGroupTabs.Clear()
        I = k.TabPages.Count
        qfeature4closing(finalWindow)
        If finalWindow Then My.Settings.fr.Clear()
    End Sub

    Friend Function qpersistedtaburl(ByVal tab As TabPage, ByVal wb As WebBrowser) As String
        If tab IsNot Nothing AndAlso suspendedGroupUrls.ContainsKey(tab) Then Return suspendedGroupUrls(tab)
        Dim host = qleaHost(tab)
        If host IsNot Nothing AndAlso host.U IsNot Nothing Then Return host.U.AbsoluteUri
        If wb IsNot Nothing AndAlso wb.Url IsNot Nothing Then Return wb.Url.ToString
        Return ""
    End Function

    Friend Function qalltabs() As List(Of TabPage)
        Dim tabs As New List(Of TabPage)
        For Each tab As TabPage In k.TabPages
            tabs.Add(tab)
        Next tab
        For Each hidden As List(Of TabPage) In hiddenGroupTabs.Values
            For Each tab As TabPage In hidden
                If tabs.Contains(tab) = False Then tabs.Add(tab)
            Next tab
        Next hidden
        Return tabs
    End Function

    ' --- Tab Groups And Vertical Tabs ---
    Sub q876(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim names As List(Of String) = qtabgroupnames()
            Dim action As String = InputBox("Enter CREATE, COLLAPSE, EXPAND, SUSPEND, RESUME, DELETE, or VERTICAL." & vbCrLf & "Groups: " & If(names.Count = 0, "none", String.Join(", ", names.ToArray)), "Tab Group Manager", "CREATE").Trim.ToUpperInvariant
            If action = "" Then Exit Sub
            If action = "VERTICAL" Then
                My.Settings.tv = If(My.Settings.tv = "e", "", "e")
                My.Settings.Save()
                qverticaltabs()
                su.Visible = True
                su.Text = "Vertical tabs " & If(My.Settings.tv = "e", "enabled", "disabled")
                Exit Sub
            End If
            Dim name As String = InputBox("Enter the tab group name:", "Tab Group Manager", If(names.Count = 0, "Group 1", names(0))).Trim.Replace("|", "-")
            If name = "" Then Exit Sub
            If action = "CREATE" Then
                Dim colorText As String = InputBox("Enter a color name or HTML color (for example, Orange or #ff8800):", "Create Tab Group", "Orange").Trim
                Dim color As Color = qgroupcolor(colorText)
                Dim selection As String = InputBox("Enter CURRENT, DOMAIN, or ALL to choose tabs:", "Create Tab Group", "CURRENT").Trim.ToUpperInvariant
                Dim urls As New List(Of String)
                Dim currentHost As String = ""
                Dim current As Uri = qleaDomain()
                If current IsNot Nothing Then currentHost = current.Host
                For Each tab As TabPage In k.TabPages
                    Dim url As String = qleaSessionTarget(qleaUrl(tab))
                    If url = "" Then Continue For
                    Dim address As New Uri(url)
                    If selection = "ALL" OrElse (selection = "CURRENT" AndAlso tab Is k.SelectedTab) OrElse (selection = "DOMAIN" AndAlso currentHost <> "" AndAlso Not address.IsFile AndAlso address.Host.Equals(currentHost, StringComparison.OrdinalIgnoreCase)) Then urls.Add(url)
                Next tab
                If urls.Count = 0 Then Exit Sub
                qdeletegrouprecords(name)
                For i As Integer = My.Settings.tg.Count - 1 To 0 Step -1
                    Dim fields As String() = My.Settings.tg(i).Split(New Char() {"|"c}, 4)
                    If fields.Length = 4 AndAlso urls.Any(Function(url) url.Equals(qunb64(fields(3)), StringComparison.OrdinalIgnoreCase)) Then My.Settings.tg.RemoveAt(i)
                Next i
                For Each url As String In urls
                    My.Settings.tg.Add(name & "|" & color.ToArgb.ToString & "|active|" & qb64(url))
                Next url
                My.Settings.Save()
                qapplyalltabgroups()
                su.Visible = True
                su.Text = urls.Count.ToString & " tab(s) added to " & name
            ElseIf action = "COLLAPSE" Then
                qcollapsegroup(name)
            ElseIf action = "EXPAND" Then
                qexpandgroup(name)
            ElseIf action = "SUSPEND" Then
                qsuspendgroup(name)
            ElseIf action = "RESUME" Then
                qresumegroup(name)
            ElseIf action = "DELETE" Then
                qexpandgroup(name)
                qresumegroup(name)
                qdeletegrouprecords(name)
                My.Settings.Save()
                qapplyalltabgroups()
                su.Visible = True
                su.Text = "Tab group deleted"
            End If
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q877(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q876(sender, e)
    End Sub

    Function qgroupcolor(ByVal value As String) As Color
        Try
            If value.StartsWith("#") Then Return ColorTranslator.FromHtml(value)
            Dim named As Color = Color.FromName(value)
            If named.IsKnownColor OrElse named.IsNamedColor Then Return named
        Catch
        End Try
        Return Color.Orange
    End Function

    Function qtabgroupnames() As List(Of String)
        Dim names As New List(Of String)
        For Each raw As String In My.Settings.tg
            Dim fields As String() = raw.Split(New Char() {"|"c}, 4)
            If fields.Length = 4 AndAlso names.Contains(fields(0)) = False Then names.Add(fields(0))
        Next raw
        Return names
    End Function

    Function qgroupurls(ByVal name As String) As List(Of String)
        Dim urls As New List(Of String)
        For Each raw As String In My.Settings.tg
            Dim fields As String() = raw.Split(New Char() {"|"c}, 4)
            If fields.Length = 4 AndAlso fields(0).Equals(name, StringComparison.OrdinalIgnoreCase) Then urls.Add(qunb64(fields(3)))
        Next raw
        Return urls
    End Function

    Function qgroupstate(ByVal name As String) As String
        For Each raw As String In My.Settings.tg
            Dim fields As String() = raw.Split(New Char() {"|"c}, 4)
            If fields.Length = 4 AndAlso fields(0).Equals(name, StringComparison.OrdinalIgnoreCase) Then Return fields(2)
        Next raw
        Return "active"
    End Function

    Sub qsetgroupstate(ByVal name As String, ByVal state As String)
        For i As Integer = 0 To My.Settings.tg.Count - 1
            Dim fields As String() = My.Settings.tg(i).Split(New Char() {"|"c}, 4)
            If fields.Length = 4 AndAlso fields(0).Equals(name, StringComparison.OrdinalIgnoreCase) Then My.Settings.tg(i) = fields(0) & "|" & fields(1) & "|" & state & "|" & fields(3)
        Next i
        My.Settings.Save()
    End Sub

    Sub qdeletegrouprecords(ByVal name As String)
        For i As Integer = My.Settings.tg.Count - 1 To 0 Step -1
            If My.Settings.tg(i).StartsWith(name & "|", StringComparison.OrdinalIgnoreCase) Then My.Settings.tg.RemoveAt(i)
        Next i
    End Sub

    Function qtabingroup(ByVal tab As TabPage, ByVal name As String) As Boolean
        If tab Is Nothing OrElse tab.IsDisposed Then Return False
        Dim url As String = qleaUrl(tab)
        If url = "" Then Return False
        Return qgroupurls(name).Any(Function(groupUrl) groupUrl.Equals(url, StringComparison.OrdinalIgnoreCase))
    End Function

    Sub qcollapsegroup(ByVal name As String)
        qexpandgroup(name)
        Dim matching As New List(Of TabPage)
        For Each tab As TabPage In k.TabPages
            If qtabingroup(tab, name) Then matching.Add(tab)
        Next tab
        If matching.Count = 0 Then Exit Sub
        If matching.Count >= k.TabPages.Count Then qleaNew(qleaStart())
        hiddenGroupTabs(name) = New List(Of TabPage)
        qleaGroupChange = True
        Try
            For Each tab As TabPage In matching
                If k.TabPages.Contains(tab) Then
                    hiddenGroupTabs(name).Add(tab)
                    k.TabPages.Remove(tab)
                End If
            Next tab
        Finally
            qleaGroupChange = False
        End Try
        I = k.TabPages.Count
        qsetgroupstate(name, "collapsed")
        su.Visible = True
        su.Text = name & " collapsed (" & matching.Count.ToString & " tab(s) hidden)"
    End Sub

    Sub qexpandgroup(ByVal name As String)
        If hiddenGroupTabs.ContainsKey(name) Then
            For Each tab As TabPage In hiddenGroupTabs(name)
                If Not tab.IsDisposed AndAlso k.TabPages.Contains(tab) = False Then k.TabPages.Add(tab)
            Next tab
            hiddenGroupTabs.Remove(name)
            I = k.TabPages.Count
        End If
        If qgroupstate(name) = "collapsed" Then qsetgroupstate(name, "active")
        qapplyalltabgroups()
    End Sub

    Sub qsuspendgroup(ByVal name As String)
        qexpandgroup(name)
        Dim suspended As Integer = 0
        For Each tab As TabPage In k.TabPages
            If qtabingroup(tab, name) AndAlso qsuspendtab(tab) Then suspended += 1
        Next tab
        qsetgroupstate(name, "suspended")
        su.Visible = True
        su.Text = name & " suspended (" & suspended.ToString & " tab(s) unloaded)"
    End Sub

    Sub qresumegroup(ByVal name As String)
        Dim resumed As Integer = 0
        For Each tab As TabPage In suspendedGroupUrls.Keys.ToArray
            If qgroupurls(name).Any(Function(url) url.Equals(suspendedGroupUrls(tab), StringComparison.OrdinalIgnoreCase)) Then
                If qfeature6resumetab(tab) Then resumed += 1
            End If
        Next tab
        If qgroupstate(name) = "suspended" Then qsetgroupstate(name, "active")
        su.Visible = True
        su.Text = name & " resumed (" & resumed.ToString & " tab(s))"
    End Sub

    Friend Sub qapplytabgroup(ByVal wb As WebBrowser)
        If wb IsNot Nothing Then qapplygrouptab(TryCast(wb.Parent, TabPage))
    End Sub

    Private Sub qapplygrouptab(ByVal tab As TabPage)
        Try
            If tab Is Nothing OrElse tab.IsDisposed Then Exit Sub
            tab.BackColor = SystemColors.Control
            tab.ToolTipText = ""
            If tab.Text.StartsWith("● ") Then tab.Text = tab.Text.Substring(2)
            Dim url As String = qleaUrl(tab)
            If url = "" Then Return
            For Each raw As String In My.Settings.tg
                Dim fields As String() = raw.Split(New Char() {"|"c}, 4)
                If fields.Length <> 4 OrElse qunb64(fields(3)).Equals(url, StringComparison.OrdinalIgnoreCase) = False Then Continue For
                Dim argb As Integer = Color.Orange.ToArgb
                Integer.TryParse(fields(1), argb)
                tab.BackColor = Color.FromArgb(argb)
                tab.ToolTipText = "Tab group: " & fields(0)
                If tab.Text.StartsWith("● ") = False Then tab.Text = "● " & tab.Text
                Exit For
            Next raw
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    Sub qapplyalltabgroups()
        For Each tab As TabPage In k.TabPages
            qapplygrouptab(tab)
        Next tab
    End Sub

    Sub qverticaltabs()
        If My.Settings.tv = "e" Then
            k.Alignment = TabAlignment.Left
            k.Multiline = True
            k.SizeMode = TabSizeMode.Fixed
            k.ItemSize = New Size(30, 220)
        Else
            k.Alignment = TabAlignment.Top
            k.Multiline = False
            k.SizeMode = TabSizeMode.Fixed
            k.ItemSize = New Size(240, 22)
        End If
    End Sub

    Sub qgrouploadtick(ByVal sender As Object, ByVal e As EventArgs)
        groupLoadTimer.Stop()
        qapplyalltabgroups()
        For Each name As String In qtabgroupnames().ToArray
            If qgroupstate(name) = "collapsed" Then qcollapsegroup(name)
            If qgroupstate(name) = "suspended" Then qsuspendgroup(name)
        Next name
    End Sub

    ' --- Custom Keyboard Shortcuts ---
    Sub q878(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim editor As New ShortcutEditorForm(Me)
            editor.Show(Me)
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q879(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q878(sender, e)
    End Sub

    Friend Function qcustomshortcut(ByVal e As KeyEventArgs) As Boolean
        For Each raw As String In My.Settings.sc
            Dim fields As String() = raw.Split(New Char() {"|"c}, 2)
            Dim keyValue As Integer = 0
            If fields.Length = 2 AndAlso Integer.TryParse(fields(0), keyValue) AndAlso keyValue = CInt(e.KeyData) Then
                Dim command As FirespiderCommand = qcommandlist().FirstOrDefault(Function(item) item.Id = fields(1))
                If command Is Nothing Then Return False
                e.SuppressKeyPress = True
                command.Item.PerformClick()
                Return True
            End If
        Next raw
        Return False
    End Function

    Friend Sub qassignshortcut(ByVal commandId As String, ByVal keyValue As Integer)
        For i As Integer = My.Settings.sc.Count - 1 To 0 Step -1
            Dim fields As String() = My.Settings.sc(i).Split(New Char() {"|"c}, 2)
            If fields.Length = 2 AndAlso (fields(0) = keyValue.ToString OrElse fields(1) = commandId) Then My.Settings.sc.RemoveAt(i)
        Next i
        My.Settings.sc.Add(keyValue.ToString & "|" & commandId)
        My.Settings.Save()
    End Sub
    Friend Sub qremoveshortcut(ByVal commandId As String)
        For i As Integer = My.Settings.sc.Count - 1 To 0 Step -1
            Dim fields As String() = My.Settings.sc(i).Split(New Char() {"|"c}, 2)
            If fields.Length = 2 AndAlso fields(1) = commandId Then My.Settings.sc.RemoveAt(i)
        Next i
        My.Settings.Save()
    End Sub
    Friend Function qshortcutfor(ByVal commandId As String) As Integer
        For Each raw As String In My.Settings.sc
            Dim fields As String() = raw.Split(New Char() {"|"c}, 2)
            Dim keyValue As Integer = 0
            If fields.Length = 2 AndAlso fields(1) = commandId AndAlso Integer.TryParse(fields(0), keyValue) Then Return keyValue
        Next raw
        Return 0
    End Function

    ' --- Content Filters ---
    Sub q880(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim action As String = InputBox(qfiltersummary() & vbCrLf & vbCrLf & "Enter DOMAIN, CONTAINS, CSS, EXCEPTION, UNEXCEPT, REMOVE, DEFAULTS, or TOGGLE:", "Content Filter Manager", "DEFAULTS").Trim.ToUpperInvariant
            If action = "" Then Exit Sub
            If action = "TOGGLE" Then
                My.Settings.cfEnabled = If(My.Settings.cfEnabled = "e", "", "e")
            ElseIf action = "DEFAULTS" Then
                For Each rule As String In New String() {"domain|doubleclick.net", "domain|googlesyndication.com", "domain|google-analytics.com", "contains|/adserver/", "contains|/tracking/", "contains|/analytics/", "css|.ad,.ads,.advertisement,.sponsored,[class*='ad-slot'],[id*='ad-slot']"}
                    If My.Settings.cf.Contains(rule) = False Then My.Settings.cf.Add(rule)
                Next rule
            ElseIf action = "EXCEPTION" Then
                Dim wb As WebBrowser = CType(k.SelectedTab.Controls(0), WebBrowser)
                Dim domain As String = qdomain(InputBox("Enter the site that should bypass filtering:", "Content Filter Exception", If(wb.Url Is Nothing, "", wb.Url.Host)))
                If domain <> "" AndAlso My.Settings.cfx.Contains(domain) = False Then My.Settings.cfx.Add(domain)
            ElseIf action = "UNEXCEPT" Then
                Dim domain As String = qdomain(InputBox("Enter the site exception to remove:", "Remove Content Filter Exception", If(My.Settings.cfx.Count = 0, "", My.Settings.cfx(0))))
                For i As Integer = My.Settings.cfx.Count - 1 To 0 Step -1
                    If qdomain(My.Settings.cfx(i)).Equals(domain, StringComparison.OrdinalIgnoreCase) Then My.Settings.cfx.RemoveAt(i)
                Next i
            ElseIf action = "REMOVE" Then
                Dim number As Integer = 0
                If Integer.TryParse(InputBox(qfiltersummary() & vbCrLf & vbCrLf & "Enter the rule number to remove:", "Remove Content Filter", "1"), number) AndAlso number > 0 AndAlso number <= My.Settings.cf.Count Then My.Settings.cf.RemoveAt(number - 1)
            ElseIf New String() {"DOMAIN", "CONTAINS", "CSS"}.Contains(action) Then
                Dim value As String = InputBox("Enter the " & action.ToLowerInvariant & " filter value:", "Add Content Filter", "").Trim
                If action = "DOMAIN" Then value = qdomain(value)
                If value <> "" Then My.Settings.cf.Add(action.ToLowerInvariant & "|" & value)
            End If
            My.Settings.Save()
            su.Visible = True
            su.Text = "Content filters updated; filtering is " & If(My.Settings.cfEnabled = "e", "enabled", "disabled")
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q881(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q880(sender, e)
    End Sub

    Function qfiltersummary() As String
        Dim lines As New List(Of String)
        For i As Integer = 0 To My.Settings.cf.Count - 1
            lines.Add((i + 1).ToString & ". " & My.Settings.cf(i))
        Next i
        Return "Filtering: " & If(My.Settings.cfEnabled = "e", "on", "off") & vbCrLf & "Rules:" & vbCrLf & If(lines.Count = 0, "none", String.Join(vbCrLf, lines.ToArray)) & vbCrLf & "Exceptions: " & If(My.Settings.cfx.Count = 0, "none", String.Join(", ", My.Settings.cfx.Cast(Of String)().ToArray))
    End Function

    Function qfilterexception(ByVal host As String) As Boolean
        host = qdomain(host)
        For Each allowed As String In My.Settings.cfx
            allowed = qdomain(allowed)
            If host.Equals(allowed, StringComparison.OrdinalIgnoreCase) OrElse host.EndsWith("." & allowed, StringComparison.OrdinalIgnoreCase) Then Return True
        Next allowed
        Return False
    End Function

    Friend Function qcontentnavigationblocked(ByVal url As Uri) As Boolean
        If My.Settings.cfEnabled <> "e" OrElse url Is Nothing OrElse qfilterexception(url.Host) Then Return False
        For Each raw As String In My.Settings.cf
            Dim fields As String() = raw.Split(New Char() {"|"c}, 2)
            If fields.Length <> 2 Then Continue For
            If fields(0) = "domain" AndAlso (url.Host.Equals(fields(1), StringComparison.OrdinalIgnoreCase) OrElse url.Host.EndsWith("." & fields(1), StringComparison.OrdinalIgnoreCase)) Then Return True
            If fields(0) = "contains" AndAlso url.AbsoluteUri.IndexOf(fields(1), StringComparison.OrdinalIgnoreCase) >= 0 Then Return True
        Next raw
        Return False
    End Function

    Friend Sub qapplycontentfilters(ByVal wb As WebBrowser)
        Try
            If My.Settings.cfEnabled <> "e" OrElse wb Is Nothing OrElse wb.Url Is Nothing OrElse wb.Document Is Nothing OrElse qfilterexception(wb.Url.Host) Then Exit Sub
            For Each tag As String In New String() {"iframe", "img", "script", "link"}
                For Each element As HtmlElement In wb.Document.GetElementsByTagName(tag)
                    Dim target As String = If(tag = "link", element.GetAttribute("href"), element.GetAttribute("src"))
                    If target = "" Then Continue For
                    Dim parsed As Uri = Nothing
                    If Uri.TryCreate(target, UriKind.Absolute, parsed) AndAlso qcontentnavigationblocked(parsed) Then element.Style &= ";display:none !important;visibility:hidden !important;"
                Next element
            Next tag
            For Each raw As String In My.Settings.cf
                Dim fields As String() = raw.Split(New Char() {"|"c}, 2)
                If fields.Length = 2 AndAlso fields(0) = "css" Then
                    Dim script As String = "(function(){try{var n=document.querySelectorAll('" & qjs(fields(1)) & "');for(var i=0;i<n.length;i++){n[i].style.display='none';n[i].style.visibility='hidden';}}catch(e){}})();"
                    wb.Document.InvokeScript("eval", New Object() {script})
                End If
            Next raw
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    Function qjs(ByVal value As String) As String
        Return value.Replace("\", "\\").Replace("'", "\'").Replace(vbCr, "").Replace(vbLf, "\n")
    End Function

    ' --- Site Identity Report ---
    Sub q882(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls(0), WebBrowser)
            If wb.Url Is Nothing OrElse wb.Document Is Nothing Then Exit Sub
            Dim uri As Uri = wb.Url
            Dim cookieText As String = If(wb.Document.Cookie, "")
            Dim cookieCount As Integer = If(cookieText.Trim = "", 0, cookieText.Split(";"c).Length)
            Dim mixed As Integer = 0
            If uri.Scheme = Uri.UriSchemeHttps Then
                For Each tag As String In New String() {"img", "script", "iframe", "link", "audio", "video", "source"}
                    For Each element As HtmlElement In wb.Document.GetElementsByTagName(tag)
                        Dim target As String = If(tag = "link", element.GetAttribute("href"), element.GetAttribute("src"))
                        If target.StartsWith("http://", StringComparison.OrdinalIgnoreCase) Then mixed += 1
                    Next element
                Next tag
            End If
            Dim domain As String = qdomain(uri.Host)
            Dim permissions As String() = qpermissionvalues(domain)
            Dim customContent As Integer = My.Settings.us.Cast(Of String)().Count(Function(raw) raw.StartsWith(domain & "|", StringComparison.OrdinalIgnoreCase))
            Dim domainPreferences As Boolean = My.Settings.dp.Cast(Of String)().Any(Function(raw) raw.StartsWith(domain & "|", StringComparison.OrdinalIgnoreCase))
            Dim html As String = "<!doctype html><html><head><meta charset=""utf-8""><title>Site Identity Report</title><style>body{max-width:820px;margin:35px auto;padding:0 24px;font:17px/1.55 Segoe UI,Arial;color:#222}dt{font-weight:bold;margin-top:14px}dd{margin:3px 0 0 20px}.good{color:#267326}.warn{color:#a34b00}</style></head><body><h1>Site Identity Report</h1><dl>" &
                "<dt>Address</dt><dd>" & qh(uri.ToString) & "</dd><dt>Host</dt><dd>" & qh(domain) & "</dd><dt>Protocol</dt><dd>" & qh(uri.Scheme.ToUpperInvariant) & "</dd>" &
                "<dt>Transport status</dt><dd class=""" & If(uri.Scheme = Uri.UriSchemeHttps, "good", "warn") & """>" & If(uri.Scheme = Uri.UriSchemeHttps, "HTTPS; certificate validation is handled by Windows and the embedded browser engine.", "This page is not using HTTPS.") & "</dd>" &
                "<dt>Mixed-content indicators</dt><dd>" & mixed.ToString & " insecure resource reference(s) found in the completed document.</dd>" &
                "<dt>Visible first-party cookies</dt><dd>" & cookieCount.ToString & " cookie name/value pair(s). HttpOnly and third-party cookies are not exposed to page script.</dd>" &
                "<dt>Saved permission rules</dt><dd>Camera " & permissions(0) & "; microphone " & permissions(1) & "; location " & permissions(2) & "; clipboard " & permissions(3) & "; notifications " & permissions(4) & ".</dd>" &
                "<dt>Content filtering</dt><dd>" & If(My.Settings.cfEnabled = "e", If(qfilterexception(domain), "Site exception", "Enabled"), "Disabled") & "</dd>" &
                "<dt>Saved custom content</dt><dd>" & customContent.ToString & " user script/style rule(s).</dd><dt>Reading preferences</dt><dd>" & If(domainPreferences, "Saved for this domain", "None saved") & "</dd></dl></body></html>"
            Dim path As String = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Firespider Identity " & Guid.NewGuid().ToString("N") & ".html")
            System.IO.File.WriteAllText(path, html, Encoding.UTF8)
            q3(New Uri(path).AbsoluteUri)
            su.Visible = True
            su.Text = "Site identity report opened"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q883(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q882(sender, e)
    End Sub

    ' --- Encrypted Autofill Profiles ---
    Sub q884(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim action As String = InputBox(qautofillsummary() & vbCrLf & vbCrLf & "Enter CREATE, FILL, DOMAINS, or DELETE:", "Encrypted Autofill Profiles", "CREATE").Trim.ToUpperInvariant
            If action = "" Then Exit Sub
            If action = "CREATE" Then
                Dim profile As String = InputBox("Enter the profile name:", "Create Autofill Profile", "Personal").Trim.Replace("|", "-")
                Dim wb As WebBrowser = CType(k.SelectedTab.Controls(0), WebBrowser)
                Dim domains As String = InputBox("Enter allowed domains separated by commas:", "Create Autofill Profile", If(wb.Url Is Nothing, "", wb.Url.Host)).Trim.ToLowerInvariant.Replace("|", "")
                If profile = "" OrElse domains = "" Then Exit Sub
                Dim values As New List(Of String)
                For Each field As String In New String() {"name", "email", "address", "city", "region", "postal-code", "country", "phone"}
                    Dim value As String = InputBox("Enter " & field & " (leave blank to skip):", "Create Autofill Profile", "")
                    If value <> "" Then values.Add(field & ":" & qb64(value))
                Next field
                If values.Count = 0 Then Exit Sub
                My.Settings.af.Add(qvaultprotect("autofill|" & profile & "|" & domains & "|" & String.Join("|", values.ToArray)))
            Else
                Dim selected As Integer = qautofillindex()
                If selected < 0 Then Exit Sub
                If action = "DELETE" Then
                    My.Settings.af.RemoveAt(selected)
                Else
                    Dim fields As String() = qvaultunprotect(My.Settings.af(selected)).Split("|"c)
                    If fields.Length < 4 Then Exit Sub
                    If action = "DOMAINS" Then
                        Dim domains As String = InputBox("Edit allowed domains separated by commas:", "Autofill Domain Allowlist", fields(2)).Trim.ToLowerInvariant.Replace("|", "")
                        If domains <> "" Then
                            fields(2) = domains
                            My.Settings.af(selected) = qvaultprotect(String.Join("|", fields))
                        End If
                    ElseIf action = "FILL" Then
                        qfillprofile(fields)
                    End If
                End If
            End If
            My.Settings.Save()
            su.Visible = True
            su.Text = "Encrypted autofill profiles updated"
        Catch ex As Exception
            qlog(ex)
            MessageBox.Show("The autofill profile could not be decrypted for this Windows user.", "Encrypted Autofill", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub
    Sub q885(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q884(sender, e)
    End Sub

    Function qautofillsummary() As String
        Dim lines As New List(Of String)
        For i As Integer = 0 To My.Settings.af.Count - 1
            Try
                Dim fields As String() = qvaultunprotect(My.Settings.af(i)).Split("|"c)
                If fields.Length >= 4 Then lines.Add((i + 1).ToString & ". " & fields(1) & " [" & fields(2) & "]")
            Catch
                lines.Add((i + 1).ToString & ". [unavailable encrypted profile]")
            End Try
        Next i
        Return "Profiles:" & vbCrLf & If(lines.Count = 0, "none", String.Join(vbCrLf, lines.ToArray))
    End Function
    Function qautofillindex() As Integer
        Dim answer As String = InputBox(qautofillsummary() & vbCrLf & vbCrLf & "Enter the profile number:", "Encrypted Autofill Profiles", "1")
        Dim index As Integer = 0
        If Integer.TryParse(answer, index) = False Then Return -1
        index -= 1
        If index < 0 OrElse index >= My.Settings.af.Count Then Return -1
        Return index
    End Function
    Sub qfillprofile(ByVal fields As String())
        Dim wb As WebBrowser = CType(k.SelectedTab.Controls(0), WebBrowser)
        If wb.Url Is Nothing OrElse wb.Document Is Nothing Then Exit Sub
        Dim host As String = qdomain(wb.Url.Host)
        Dim allowed As Boolean = fields(2).Split(","c).Any(Function(value) host.Equals(qdomain(value), StringComparison.OrdinalIgnoreCase) OrElse host.EndsWith("." & qdomain(value), StringComparison.OrdinalIgnoreCase))
        If allowed = False Then
            MessageBox.Show("This profile is not allowed on " & host & ".", "Autofill Domain Allowlist", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If
        Dim filled As Integer = 0
        For i As Integer = 3 To fields.Length - 1
            Dim split As Integer = fields(i).IndexOf(":"c)
            If split <= 0 Then Continue For
            Dim key As String = fields(i).Substring(0, split)
            Dim value As String = qunb64(fields(i).Substring(split + 1))
            If MessageBox.Show("Fill the " & key & " field on " & host & "?", "Confirm Autofill Field", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Continue For
            For Each input As HtmlElement In wb.Document.GetElementsByTagName("input")
                Dim inputType As String = input.GetAttribute("type").ToLowerInvariant
                If inputType = "password" Then Continue For
                Dim identity As String = (input.GetAttribute("name") & " " & input.GetAttribute("id") & " " & input.GetAttribute("autocomplete") & " " & input.GetAttribute("placeholder")).ToLowerInvariant
                If identity.Contains(key.ToLowerInvariant) Then
                    input.SetAttribute("value", value)
                    Try
                        CallByName(input.DomElement, "value", CallType.Set, value)
                    Catch
                    End Try
                    filled += 1
                    Exit For
                End If
            Next input
        Next i
        su.Visible = True
        su.Text = filled.ToString & " autofill field(s) completed"
    End Sub

    ' --- Scheduled Rotating Backups ---
    Sub q886(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim action As String = InputBox("Status: " & If(My.Settings.backupEnabled = "e", "enabled", "disabled") & vbCrLf & "Interval: " & My.Settings.backupInterval & vbCrLf & "Folder: " & qbackupfolder() & vbCrLf & "Keep: " & My.Settings.backupKeep & vbCrLf & vbCrLf & "Enter ENABLE, DISABLE, FOLDER, INTERVAL, KEEP, NOW, or OPEN:", "Scheduled Rotating Backups", "NOW").Trim.ToUpperInvariant
            If action = "ENABLE" Then
                My.Settings.backupEnabled = "e"
                backupTimer.Start()
            ElseIf action = "DISABLE" Then
                My.Settings.backupEnabled = ""
                backupTimer.Stop()
            ElseIf action = "FOLDER" Then
                Dim folder As New FolderBrowserDialog
                folder.Description = "Choose the rotating backup folder"
                folder.SelectedPath = qbackupfolder()
                If folder.ShowDialog(Me) = DialogResult.OK Then My.Settings.backupFolder = folder.SelectedPath
            ElseIf action = "INTERVAL" Then
                Dim interval As String = InputBox("Enter HOURLY, DAILY, or WEEKLY:", "Backup Interval", My.Settings.backupInterval).Trim.ToLowerInvariant
                If New String() {"hourly", "daily", "weekly"}.Contains(interval) Then My.Settings.backupInterval = interval
            ElseIf action = "KEEP" Then
                Dim keep As Integer = 0
                If Integer.TryParse(InputBox("How many backup files should be kept?", "Rotating Backup Count", My.Settings.backupKeep), keep) AndAlso keep >= 1 AndAlso keep <= 100 Then My.Settings.backupKeep = keep.ToString
            ElseIf action = "NOW" Then
                qperformbackup()
            ElseIf action = "OPEN" Then
                System.IO.Directory.CreateDirectory(qbackupfolder())
                Process.Start(qbackupfolder())
            End If
            My.Settings.Save()
            su.Visible = True
            su.Text = "Scheduled backup settings updated"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q887(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q886(sender, e)
    End Sub

    Function qbackupfolder() As String
        If System.IO.Directory.Exists(My.Settings.backupFolder) Then Return My.Settings.backupFolder
        Return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Firespider Backups")
    End Function
    Sub qbackuptick(ByVal sender As Object, ByVal e As EventArgs)
        If qbackupdue() Then qperformbackup()
    End Sub
    Function qbackupdue() As Boolean
        If My.Settings.backupEnabled <> "e" Then Return False
        Dim lastBackup As DateTime = DateTime.MinValue
        If DateTime.TryParse(My.Settings.backupTime, lastBackup) = False Then Return True
        If My.Settings.backupInterval = "hourly" Then Return Now.Subtract(lastBackup).TotalHours >= 1
        If My.Settings.backupInterval = "weekly" Then Return Now.Subtract(lastBackup).TotalDays >= 7
        Return Now.Subtract(lastBackup).TotalDays >= 1
    End Function
    Sub qperformbackup()
        Try
            Dim folder As String = qbackupfolder()
            System.IO.Directory.CreateDirectory(folder)
            Dim data As New StringBuilder
            data.AppendLine("FIRESPIDER_SETTINGS_BACKUP_V1")
            data.AppendLine("Created|" & Now.ToString("o"))
            For Each propertyValue As SettingsProperty In My.Settings.Properties
                Dim value As Object = My.Settings(propertyValue.Name)
                If TypeOf value Is System.Collections.Specialized.StringCollection Then
                    Dim collection As System.Collections.Specialized.StringCollection = CType(value, System.Collections.Specialized.StringCollection)
                    If collection.Count = 0 Then data.AppendLine("E|" & qb64(propertyValue.Name) & "|")
                    For Each item As String In collection
                        data.AppendLine("C|" & qb64(propertyValue.Name) & "|" & qb64(item))
                    Next item
                Else
                    data.AppendLine("S|" & qb64(propertyValue.Name) & "|" & qb64(If(value Is Nothing, "", value.ToString)))
                End If
            Next propertyValue
            Dim path As String = System.IO.Path.Combine(folder, "Firespider Backup " & Now.ToString("yyyyMMdd HHmmss fff") & ".txt")
            Dim temporary As String = path & ".tmp"
            System.IO.File.WriteAllText(temporary, data.ToString, Encoding.UTF8)
            System.IO.File.Move(temporary, path)
            My.Settings.backupTime = Now.ToString("o")
            My.Settings.Save()
            Dim keep As Integer = 7
            Integer.TryParse(My.Settings.backupKeep, keep)
            keep = Math.Max(1, keep)
            Dim files As String() = System.IO.Directory.GetFiles(folder, "Firespider Backup *.txt").OrderByDescending(Function(file) System.IO.File.GetCreationTimeUtc(file)).ToArray
            For i As Integer = keep To files.Length - 1
                System.IO.File.Delete(files(i))
            Next i
            su.Visible = True
            su.Text = "Rotating backup saved"
        Catch ex As Exception
            qlog(ex)
            su.Visible = True
            su.Text = "Scheduled backup could not be saved"
        End Try
    End Sub

    ' --- Crash Form Recovery ---
    Sub q888(ByVal sender As Object, ByVal e As EventArgs)
        My.Settings.frEnabled = If(My.Settings.frEnabled = "e", "", "e")
        If My.Settings.frEnabled = "e" AndAlso pv = False Then formRecoveryTimer.Start() Else formRecoveryTimer.Stop()
        My.Settings.Save()
        su.Visible = True
        su.Text = "Crash form recovery " & If(My.Settings.frEnabled = "e", "enabled", "disabled")
    End Sub
    Sub q889(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q888(sender, e)
    End Sub
    Sub qformtick(ByVal sender As Object, ByVal e As EventArgs)
        If pv OrElse My.Settings.frEnabled <> "e" Then Exit Sub
        For Each tab As TabPage In k.TabPages
            If tab.Controls.Count > 0 Then qcaptureform(TryCast(tab.Controls(0), WebBrowser))
        Next tab
        My.Settings.Save()
    End Sub
    Sub qcaptureform(ByVal wb As WebBrowser)
        Try
            If wb Is Nothing OrElse wb.Url Is Nothing OrElse wb.Document Is Nothing Then Exit Sub
            Dim url As String = wb.Url.ToString
            For i As Integer = My.Settings.fr.Count - 1 To 0 Step -1
                Dim fields As String() = My.Settings.fr(i).Split("|"c)
                If startupFormRecords.Contains(My.Settings.fr(i)) = False AndAlso fields.Length = 4 AndAlso qunb64(fields(0)).Equals(url, StringComparison.OrdinalIgnoreCase) Then My.Settings.fr.RemoveAt(i)
            Next i
            Dim saved As Integer = 0
            For Each tag As String In New String() {"input", "textarea"}
                For Each element As HtmlElement In wb.Document.GetElementsByTagName(tag)
                    Dim inputType As String = element.GetAttribute("type").ToLowerInvariant
                    If inputType = "password" OrElse inputType = "hidden" OrElse inputType = "file" OrElse inputType = "submit" OrElse inputType = "button" OrElse inputType = "checkbox" OrElse inputType = "radio" Then Continue For
                    Dim key As String = element.GetAttribute("name")
                    If key = "" Then key = element.GetAttribute("id")
                    If key = "" Then key = element.GetAttribute("autocomplete")
                    Dim value As String = element.GetAttribute("value")
                    If tag = "textarea" Then value = element.InnerText
                    If key = "" OrElse value = "" Then Continue For
                    If value.Length > 5000 Then value = value.Substring(0, 5000)
                    My.Settings.fr.Add(qb64(url) & "|" & Now.ToString("o") & "|" & qb64(key) & "|" & qb64(value))
                    saved += 1
                    If saved >= 100 Then Exit For
                Next element
                If saved >= 100 Then Exit For
            Next tag
            While My.Settings.fr.Count > 500
                My.Settings.fr.RemoveAt(0)
            End While
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    Friend Sub qrestoreform(ByVal wb As WebBrowser)
        Try
            If pv OrElse My.Settings.frEnabled <> "e" OrElse wb Is Nothing OrElse wb.Url Is Nothing OrElse wb.Document Is Nothing Then Exit Sub
            Dim url As String = wb.Url.ToString
            If consideredFormUrls.Contains(url) Then Exit Sub
            Dim records As New List(Of String)
            For Each raw As String In My.Settings.fr
                Dim fields As String() = raw.Split("|"c)
                If startupFormRecords.Contains(raw) AndAlso fields.Length = 4 AndAlso qunb64(fields(0)).Equals(url, StringComparison.OrdinalIgnoreCase) Then records.Add(raw)
            Next raw
            If records.Count = 0 Then Exit Sub
            consideredFormUrls.Add(url)
            If MessageBox.Show("Restore " & records.Count.ToString & " unsent form field(s) recovered after the previous interruption? Password fields were never stored.", "Crash Form Recovery", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Exit Sub
            Dim restored As Integer = 0
            For Each raw As String In records
                Dim fields As String() = raw.Split("|"c)
                Dim key As String = qunb64(fields(2))
                Dim value As String = qunb64(fields(3))
                For Each tag As String In New String() {"input", "textarea"}
                    For Each element As HtmlElement In wb.Document.GetElementsByTagName(tag)
                        If element.GetAttribute("type").Equals("password", StringComparison.OrdinalIgnoreCase) Then Continue For
                        If element.GetAttribute("name").Equals(key, StringComparison.OrdinalIgnoreCase) OrElse element.GetAttribute("id").Equals(key, StringComparison.OrdinalIgnoreCase) OrElse element.GetAttribute("autocomplete").Equals(key, StringComparison.OrdinalIgnoreCase) Then
                            If tag = "textarea" Then element.InnerText = value Else element.SetAttribute("value", value)
                            Try
                                CallByName(element.DomElement, "value", CallType.Set, value)
                            Catch
                            End Try
                            restored += 1
                            Exit For
                        End If
                    Next element
                Next tag
                My.Settings.fr.Remove(raw)
                startupFormRecords.Remove(raw)
            Next raw
            My.Settings.Save()
            su.Visible = True
            su.Text = restored.ToString & " form field(s) restored"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    ' --- Page Resource Inspector ---
    Sub q890(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls(0), WebBrowser)
            If wb.Document Is Nothing Then Exit Sub
            Dim resources As New List(Of ResourceRecord)
            qaddresources(wb, resources, "Script", "script", "src")
            qaddresources(wb, resources, "Stylesheet", "link", "href")
            qaddresources(wb, resources, "Frame", "iframe", "src")
            qaddresources(wb, resources, "Image", "img", "src")
            qaddresources(wb, resources, "Audio", "audio", "src")
            qaddresources(wb, resources, "Video", "video", "src")
            qaddresources(wb, resources, "Media source", "source", "src")
            Dim possibleFailures As Integer = 0
            For Each image As HtmlElement In wb.Document.GetElementsByTagName("img")
                Dim width As Integer = 0
                Dim height As Integer = 0
                Try
                    width = CInt(CallByName(image.DomElement, "naturalWidth", CallType.Get))
                    height = CInt(CallByName(image.DomElement, "naturalHeight", CallType.Get))
                Catch
                    Integer.TryParse(image.GetAttribute("width"), width)
                    Integer.TryParse(image.GetAttribute("height"), height)
                End Try
                If image.GetAttribute("src") <> "" AndAlso width = 0 AndAlso height = 0 Then possibleFailures += 1
            Next image
            Dim timing As String = "Unavailable in this document mode"
            Try
                Dim result As Object = wb.Document.InvokeScript("eval", New Object() {"(function(){try{var t=window.performance&&window.performance.timing;if(!t)return 'Unavailable';var end=t.loadEventEnd||new Date().getTime();return 'Total '+(end-t.navigationStart)+' ms; DOM ready '+(t.domContentLoadedEventEnd-t.navigationStart)+' ms; response '+(t.responseEnd-t.requestStart)+' ms';}catch(e){return 'Unavailable';}})();"})
                If result IsNot Nothing Then timing = result.ToString
            Catch
            End Try
            Dim html As New StringBuilder
            html.Append("<!doctype html><html><head><meta charset=""utf-8""><title>Page Resource Inspector</title><style>body{font:15px Segoe UI,Arial;margin:28px;color:#222}table{border-collapse:collapse;width:100%}th,td{border:1px solid #ccc;padding:7px;text-align:left;word-break:break-all}th{background:#eee}.warn{color:#a34b00}</style></head><body><h1>Page Resource Inspector</h1><p><strong>Page:</strong> " & qh(If(wb.Url Is Nothing, "", wb.Url.ToString)) & "</p><p><strong>Load timing:</strong> " & qh(timing) & "</p><p><strong>Resources:</strong> " & resources.Count.ToString & " · <span class=""warn"">Potential zero-size image failures: " & possibleFailures.ToString & "</span></p><table><thead><tr><th>Type</th><th>Security</th><th>Address</th></tr></thead><tbody>")
            For Each resource As ResourceRecord In resources.Take(500)
                html.Append("<tr><td>" & qh(resource.Kind) & "</td><td>" & qh(resource.Security) & "</td><td>" & qh(resource.Url) & "</td></tr>")
            Next resource
            html.Append("</tbody></table><p>Failed scripts and stylesheets are not directly exposed by the legacy browser engine; the report flags observable zero-size images and lists every completed-document resource for inspection.</p></body></html>")
            Dim path As String = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Firespider Resources " & Guid.NewGuid().ToString("N") & ".html")
            System.IO.File.WriteAllText(path, html.ToString, Encoding.UTF8)
            q3(New Uri(path).AbsoluteUri)
            su.Visible = True
            su.Text = resources.Count.ToString & " page resource(s) inspected"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q891(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q890(sender, e)
    End Sub
    Sub qaddresources(ByVal wb As WebBrowser, ByVal resources As List(Of ResourceRecord), ByVal kind As String, ByVal tag As String, ByVal attribute As String)
        For Each element As HtmlElement In wb.Document.GetElementsByTagName(tag)
            Dim target As String = element.GetAttribute(attribute)
            If target = "" Then Continue For
            Dim parsed As Uri = Nothing
            Dim security As String = "Relative or embedded"
            If Uri.TryCreate(target, UriKind.Absolute, parsed) Then security = parsed.Scheme.ToUpperInvariant
            resources.Add(New ResourceRecord(kind, target, security))
        Next element
    End Sub
End Class

Friend Class ResourceRecord
    Public Kind As String
    Public Url As String
    Public Security As String
    Sub New(ByVal resourceKind As String, ByVal resourceUrl As String, ByVal resourceSecurity As String)
        Kind = resourceKind
        Url = resourceUrl
        Security = resourceSecurity
    End Sub
End Class

Friend Class ShortcutEditorForm
    Inherits Form
    Dim ownerForm As j
    Dim commands As New ListBox
    Dim capture As New TextBox
    Dim current As New Label
    Dim assignButton As New Button
    Dim removeButton As New Button
    Dim resetButton As New Button
    Dim capturedKey As Integer = 0
    Sub New(ByVal browser As j)
        ownerForm = browser
        Text = "Keyboard Shortcut Editor"
        StartPosition = FormStartPosition.CenterParent
        Size = New Size(760, 540)
        commands.Dock = DockStyle.Fill
        commands.Font = New Font("Segoe UI", 10.0!)
        For Each command As FirespiderCommand In ownerForm.qcommandlist()
            commands.Items.Add(command)
        Next command
        Dim bottom As New TableLayoutPanel
        bottom.Dock = DockStyle.Bottom
        bottom.Height = 105
        bottom.ColumnCount = 3
        bottom.RowCount = 3
        bottom.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 55.0!))
        bottom.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 22.5!))
        bottom.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 22.5!))
        capture.Dock = DockStyle.Fill
        capture.ReadOnly = True
        capture.Text = "Click here, then press a shortcut"
        current.Dock = DockStyle.Fill
        current.TextAlign = ContentAlignment.MiddleLeft
        assignButton.Text = "Assign"
        removeButton.Text = "Remove"
        resetButton.Text = "Reset All"
        assignButton.Dock = DockStyle.Fill
        removeButton.Dock = DockStyle.Fill
        resetButton.Dock = DockStyle.Fill
        bottom.Controls.Add(capture, 0, 0)
        bottom.SetColumnSpan(capture, 3)
        bottom.Controls.Add(current, 0, 1)
        bottom.SetColumnSpan(current, 3)
        bottom.Controls.Add(assignButton, 0, 2)
        bottom.Controls.Add(removeButton, 1, 2)
        bottom.Controls.Add(resetButton, 2, 2)
        Controls.Add(commands)
        Controls.Add(bottom)
        AddHandler commands.SelectedIndexChanged, AddressOf qselection
        AddHandler capture.KeyDown, AddressOf qcapture
        AddHandler assignButton.Click, AddressOf qassign
        AddHandler removeButton.Click, AddressOf qremove
        AddHandler resetButton.Click, AddressOf qreset
        If commands.Items.Count > 0 Then commands.SelectedIndex = 0
    End Sub
    Function qselected() As FirespiderCommand
        Return TryCast(commands.SelectedItem, FirespiderCommand)
    End Function
    Sub qselection(ByVal sender As Object, ByVal e As EventArgs)
        Dim command As FirespiderCommand = qselected()
        If command Is Nothing Then Exit Sub
        Dim keyValue As Integer = ownerForm.qshortcutfor(command.Id)
        current.Text = "Current custom shortcut: " & If(keyValue = 0, "none", New KeysConverter().ConvertToString(CType(keyValue, Keys)))
    End Sub
    Sub qcapture(ByVal sender As Object, ByVal e As KeyEventArgs)
        If e.KeyCode = Keys.ControlKey OrElse e.KeyCode = Keys.ShiftKey OrElse e.KeyCode = Keys.Menu Then Exit Sub
        capturedKey = CInt(e.KeyData)
        capture.Text = New KeysConverter().ConvertToString(e.KeyData)
        e.SuppressKeyPress = True
    End Sub
    Sub qassign(ByVal sender As Object, ByVal e As EventArgs)
        Dim command As FirespiderCommand = qselected()
        If command Is Nothing OrElse capturedKey = 0 Then Exit Sub
        Dim keys As Keys = CType(capturedKey, Keys)
        Dim keyCode As Keys = keys And Keys.KeyCode
        Dim functionKey As Boolean = keyCode >= Keys.F1 AndAlso keyCode <= Keys.F12
        If (keys And Keys.Control) = 0 AndAlso (keys And Keys.Alt) = 0 AndAlso functionKey = False Then
            MessageBox.Show("Use Ctrl, Alt, or a function key so normal typing is not intercepted.", "Keyboard Shortcut Editor", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If
        ownerForm.qassignshortcut(command.Id, capturedKey)
        qselection(Nothing, EventArgs.Empty)
    End Sub
    Sub qremove(ByVal sender As Object, ByVal e As EventArgs)
        Dim command As FirespiderCommand = qselected()
        If command IsNot Nothing Then ownerForm.qremoveshortcut(command.Id)
        qselection(Nothing, EventArgs.Empty)
    End Sub
    Sub qreset(ByVal sender As Object, ByVal e As EventArgs)
        If MessageBox.Show("Remove every custom keyboard shortcut?", "Keyboard Shortcut Editor", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            My.Settings.sc.Clear()
            My.Settings.Save()
            qselection(Nothing, EventArgs.Empty)
        End If
    End Sub
End Class
