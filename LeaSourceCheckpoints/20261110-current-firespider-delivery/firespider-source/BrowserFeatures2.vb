Imports System.Security.Cryptography
Imports System.Text

Partial Class j
    Sub qfeatures2()
        fl.DropDownItems.Add(qnewitem("Workspace Profile Manager", AddressOf q860, AddressOf q861))
        fl.DropDownItems.Add(qnewitem("Download Details And Checksums", AddressOf q864, AddressOf q865))
        fl.DropDownItems.Add(qnewitem("Archive Current Page Locally", AddressOf q872, AddressOf q873))
        v.DropDownItems.Add(qnewitem("Side-By-Side Tab View", AddressOf q862, AddressOf q863))
        v.DropDownItems.Add(qnewitem("Accessibility Inspector", AddressOf q870, AddressOf q871))
        bo.DropDownItems.Add(qnewitem("Encrypted Local Vault", AddressOf q868, AddressOf q869))
        bo.DropDownItems.Add(qnewitem("Local Archive Library", AddressOf q874, AddressOf q875))
        ot.DropDownItems.Add(qnewitem("Site Permission Center", AddressOf q858, AddressOf q859))
        ot.DropDownItems.Add(qnewitem("User Script And Style Manager", AddressOf q866, AddressOf q867))
        qfeatures3()
    End Sub

    ' --- Site Permission Center ---
    Sub q858(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls(0), WebBrowser)
            Dim defaultDomain As String = If(wb.Url Is Nothing, "", wb.Url.Host)
            Dim domain As String = qdomain(InputBox("Enter the domain to configure:", "Site Permission Center", defaultDomain))
            If domain = "" Then Exit Sub
            Dim values As String() = qpermissionvalues(domain)
            Dim summary As String = "Current rules for " & domain & ":" & vbCrLf &
                "camera=" & values(0) & ", microphone=" & values(1) & ", location=" & values(2) &
                ", clipboard=" & values(3) & ", notifications=" & values(4)
            Dim resource As String = InputBox(summary & vbCrLf & vbCrLf & "Enter CAMERA, MICROPHONE, LOCATION, CLIPBOARD, NOTIFICATIONS, ALL, or RESET:", "Site Permission Center", "ALL").Trim.ToLowerInvariant
            If resource = "" Then Exit Sub
            If resource = "reset" Then
                qremovepermission(domain)
                My.Settings.Save()
                su.Visible = True
                su.Text = "Permission rules reset for " & domain
                Exit Sub
            End If
            Dim allowed As String() = {"camera", "microphone", "location", "clipboard", "notifications", "all"}
            If allowed.Contains(resource) = False Then
                su.Visible = True
                su.Text = "Unknown permission name"
                Exit Sub
            End If
            Dim decision As String = InputBox("Enter ALLOW, BLOCK, or ASK:", "Site Permission Center", "BLOCK").Trim.ToLowerInvariant
            If New String() {"allow", "block", "ask"}.Contains(decision) = False Then Exit Sub
            If resource = "all" Then
                For i As Integer = 0 To values.Length - 1
                    values(i) = decision
                Next i
            Else
                Dim index As Integer = Array.IndexOf(New String() {"camera", "microphone", "location", "clipboard", "notifications"}, resource)
                values(index) = decision
            End If
            qremovepermission(domain)
            My.Settings.perm.Add(domain & "|" & String.Join("|", values))
            My.Settings.Save()
            su.Visible = True
            su.Text = "Permission rules updated for " & domain
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q859(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q858(sender, e)
    End Sub

    Function qpermissionvalues(ByVal domain As String) As String()
        For Each raw As String In My.Settings.perm
            Dim fields As String() = raw.Split("|"c)
            If fields.Length = 6 AndAlso fields(0).Equals(domain, StringComparison.OrdinalIgnoreCase) Then Return fields.Skip(1).ToArray
        Next raw
        Return New String() {"ask", "ask", "ask", "ask", "ask"}
    End Function

    Sub qremovepermission(ByVal domain As String)
        For i As Integer = My.Settings.perm.Count - 1 To 0 Step -1
            If My.Settings.perm(i).StartsWith(domain & "|", StringComparison.OrdinalIgnoreCase) Then My.Settings.perm.RemoveAt(i)
        Next i
    End Sub

    Friend Sub qapplypermissions(ByVal wb As WebBrowser)
        Try
            If wb Is Nothing OrElse wb.Url Is Nothing OrElse wb.Document Is Nothing Then Exit Sub
            Dim rules As String() = qpermissionvalues(qdomain(wb.Url.Host))
            Dim script As New StringBuilder
            script.Append("(function(){")
            If rules(0) = "block" OrElse rules(1) = "block" Then
                Dim cameraBlocked As String = If(rules(0) = "block", "true", "false")
                Dim microphoneBlocked As String = If(rules(1) = "block", "true", "false")
                script.Append("try{var oldGum=navigator.getUserMedia||navigator.webkitGetUserMedia||navigator.msGetUserMedia;navigator.getUserMedia=function(c,s,f){if((" & cameraBlocked & "&&c&&c.video)||(" & microphoneBlocked & "&&c&&c.audio)){if(f)f({name:'NotAllowedError'});return;}if(oldGum)return oldGum.call(navigator,c,s,f);};}catch(e){}")
                script.Append("try{if(navigator.mediaDevices&&navigator.mediaDevices.getUserMedia){var oldMediaGum=navigator.mediaDevices.getUserMedia;navigator.mediaDevices.getUserMedia=function(c){if((" & cameraBlocked & "&&c&&c.video)||(" & microphoneBlocked & "&&c&&c.audio)){return {then:function(){return this;},catch:function(f){if(f)f({name:'NotAllowedError'});}};}return oldMediaGum.call(navigator.mediaDevices,c);};}}catch(e){}")
            End If
            If rules(2) = "block" Then
                script.Append("try{navigator.geolocation.getCurrentPosition=function(a,b){if(b)b({code:1,message:'Blocked by Firespider'});};navigator.geolocation.watchPosition=navigator.geolocation.getCurrentPosition;}catch(e){}")
            End If
            If rules(3) = "block" Then
                script.Append("try{document.execCommand=function(c){if(String(c).toLowerCase().indexOf('copy')>=0||String(c).toLowerCase().indexOf('paste')>=0)return false;return false;};}catch(e){}")
            End If
            If rules(4) = "block" Then
                script.Append("try{window.Notification=function(){};window.Notification.permission='denied';window.Notification.requestPermission=function(f){if(f)f('denied');};}catch(e){}")
            End If
            script.Append("})();")
            wb.Document.InvokeScript("eval", New Object() {script.ToString})
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    ' --- Workspace Profiles ---
    Sub q860(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim names As List(Of String) = qworkspacenames()
            Dim action As String = InputBox("Enter SAVE, OPEN, RENAME, or DELETE." & vbCrLf & "Profiles: " & If(names.Count = 0, "none", String.Join(", ", names.ToArray)), "Workspace Profile Manager", "SAVE").Trim.ToUpperInvariant
            If action = "" Then Exit Sub
            Dim name As String = InputBox("Enter the workspace name:", "Workspace Profile Manager", If(names.Count = 0, "Workspace " & Now.ToString("yyyy-MM-dd"), names(0))).Trim.Replace("|", "-")
            If name = "" Then Exit Sub
            If action = "SAVE" Then
                qsaveworkspace(name)
            ElseIf action = "OPEN" Then
                qopenworkspace(name)
            ElseIf action = "DELETE" Then
                qremoveworkspace(name)
                My.Settings.Save()
                su.Visible = True
                su.Text = "Workspace profile deleted"
            ElseIf action = "RENAME" Then
                Dim replacement As String = InputBox("Enter the new workspace name:", "Rename Workspace", name).Trim.Replace("|", "-")
                If replacement = "" Then Exit Sub
                For i As Integer = 0 To My.Settings.wp.Count - 1
                    Dim split As Integer = My.Settings.wp(i).IndexOf("|"c)
                    If split > 0 AndAlso My.Settings.wp(i).Substring(0, split).Equals(name, StringComparison.OrdinalIgnoreCase) Then My.Settings.wp(i) = replacement & My.Settings.wp(i).Substring(split)
                Next i
                My.Settings.Save()
                su.Visible = True
                su.Text = "Workspace profile renamed"
            End If
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q861(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q860(sender, e)
    End Sub

    Function qworkspacenames() As List(Of String)
        Dim names As New List(Of String)
        For Each raw As String In My.Settings.wp
            Dim split As Integer = raw.IndexOf("|"c)
            If split > 0 Then
                Dim name As String = raw.Substring(0, split)
                If names.Contains(name) = False Then names.Add(name)
            End If
        Next raw
        Return names
    End Function

    Sub qremoveworkspace(ByVal name As String)
        For i As Integer = My.Settings.wp.Count - 1 To 0 Step -1
            If My.Settings.wp(i).StartsWith(name & "|", StringComparison.OrdinalIgnoreCase) Then My.Settings.wp.RemoveAt(i)
        Next i
    End Sub

    Sub qsaveworkspace(ByVal name As String)
        qremoveworkspace(name)
        For Each tab As TabPage In qalltabs()
            If tab.Controls.Count = 0 Then Continue For
            Dim wb As WebBrowser = TryCast(tab.Controls(0), WebBrowser)
            Dim url As String = qpersistedtaburl(tab, wb)
            If url <> "" Then My.Settings.wp.Add(name & "|T|" & qb64(url))
        Next tab
        For Each value As String In My.Settings.bm
            My.Settings.wp.Add(name & "|M|" & qb64(value))
        Next value
        For Each value As String In My.Settings.rl
            My.Settings.wp.Add(name & "|R|" & qb64(value))
        Next value
        For Each value As String In My.Settings.dp
            My.Settings.wp.Add(name & "|D|" & qb64(value))
        Next value
        For Each value As String In My.Settings.bs
            My.Settings.wp.Add(name & "|B|" & qb64(value))
        Next value
        My.Settings.Save()
        su.Visible = True
        su.Text = "Workspace profile saved: " & name
    End Sub

    Sub qopenworkspace(ByVal name As String)
        Dim urls As New List(Of String)
        Dim found As Boolean = False
        For Each raw As String In My.Settings.wp
            Dim fields As String() = raw.Split(New Char() {"|"c}, 3)
            If fields.Length <> 3 OrElse fields(0).Equals(name, StringComparison.OrdinalIgnoreCase) = False Then Continue For
            found = True
            Dim value As String = qunb64(fields(2))
            If fields(1) = "T" Then urls.Add(value)
            If fields(1) = "M" AndAlso My.Settings.bm.Contains(value) = False Then My.Settings.bm.Add(value)
            If fields(1) = "R" AndAlso My.Settings.rl.Contains(value) = False Then My.Settings.rl.Add(value)
            If fields(1) = "D" AndAlso My.Settings.dp.Contains(value) = False Then My.Settings.dp.Add(value)
            If fields(1) = "B" AndAlso My.Settings.bs.Contains(value) = False Then My.Settings.bs.Add(value)
        Next raw
        If found = False Then
            su.Visible = True
            su.Text = "Workspace profile not found"
            Exit Sub
        End If
        My.Settings.Save()
        qrb()
        Dim opened As Integer = qleaRestoreSaved(urls, True)
        su.Visible = True
        su.Text = "Workspace profile opened: " & name & "; " & opened.ToString & " Lea tab(s) restored"
        If opened < urls.Count Then su.Text &= "; " & (urls.Count - opened).ToString & " URL(s) skipped by Lea limits"
    End Sub

    ' --- Side-By-Side Tab View ---
    Sub q862(ByVal sender As Object, ByVal e As EventArgs)
        Try
            qleaSplit()
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q863(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q862(sender, e)
    End Sub

    ' --- Download Details And Checksums ---
    Friend Sub qtrackdownload(ByVal path As String)
        Try
            Dim source As String = ""
            If k.SelectedTab IsNot Nothing AndAlso k.SelectedTab.Controls.Count > 0 Then
                Dim wb As WebBrowser = TryCast(k.SelectedTab.Controls(0), WebBrowser)
                If wb IsNot Nothing AndAlso wb.Url IsNot Nothing Then source = wb.Url.ToString
            End If
            Dim records As List(Of DownloadQueueRecord) = qdownloadrecords()
            For i As Integer = My.Settings.dq.Count - 1 To 0 Step -1
                Dim existing As DownloadQueueRecord = qparsedownload(My.Settings.dq(i))
                If existing IsNot Nothing AndAlso existing.Path.Equals(path, StringComparison.OrdinalIgnoreCase) Then My.Settings.dq.RemoveAt(i)
            Next i
            My.Settings.dq.Add(qdownloadraw(New DownloadQueueRecord(path, source, "complete", Now.ToString("yyyy-MM-dd HH:mm:ss"))))
            While My.Settings.dq.Count > 50
                My.Settings.dq.RemoveAt(0)
            End While
            My.Settings.Save()
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    Friend Function qdownloadrecords() As List(Of DownloadQueueRecord)
        Dim records As New List(Of DownloadQueueRecord)
        Dim paths As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each raw As String In My.Settings.dq
            Dim record As DownloadQueueRecord = qparsedownload(raw)
            If record IsNot Nothing Then
                records.Add(record)
                paths.Add(record.Path)
            End If
        Next raw
        For Each path As String In My.Settings.dl
            If paths.Add(path) Then records.Add(New DownloadQueueRecord(path, "", "complete", "Legacy download record"))
        Next path
        Return records
    End Function

    Function qparsedownload(ByVal raw As String) As DownloadQueueRecord
        Try
            Dim fields As String() = raw.Split("|"c)
            If fields.Length <> 4 Then Return Nothing
            Return New DownloadQueueRecord(qunb64(fields(0)), qunb64(fields(1)), fields(2), fields(3))
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Function qdownloadraw(ByVal record As DownloadQueueRecord) As String
        Return qb64(record.Path) & "|" & qb64(record.Source) & "|" & record.Status & "|" & record.DateSaved
    End Function

    Friend Sub qupdatedownload(ByVal record As DownloadQueueRecord, ByVal remove As Boolean)
        For i As Integer = My.Settings.dq.Count - 1 To 0 Step -1
            Dim existing As DownloadQueueRecord = qparsedownload(My.Settings.dq(i))
            If existing IsNot Nothing AndAlso existing.Path.Equals(record.Path, StringComparison.OrdinalIgnoreCase) Then My.Settings.dq.RemoveAt(i)
        Next i
        If remove = False Then
            My.Settings.dq.Add(qdownloadraw(record))
        Else
            My.Settings.dl.Remove(record.Path)
        End If
        My.Settings.Save()
    End Sub

    Sub q864(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim center As New DownloadCenterForm(Me)
            center.Show(Me)
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q865(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q864(sender, e)
    End Sub

    ' --- User Scripts And Styles ---
    Sub q866(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim action As String = InputBox(qusercontentsummary() & vbCrLf & vbCrLf & "Enter ADD, EDIT, TOGGLE, PREVIEW, DELETE, or SAFE:", "User Script And Style Manager", "ADD").Trim.ToUpperInvariant
            If action = "" Then Exit Sub
            If action = "SAFE" Then
                My.Settings.scriptPreview = If(My.Settings.scriptPreview = "e", "", "e")
                My.Settings.Save()
                su.Visible = True
                su.Text = "Safe preview mode " & If(My.Settings.scriptPreview = "e", "enabled", "disabled")
                Exit Sub
            End If
            If action = "ADD" Then
                Dim wb As WebBrowser = CType(k.SelectedTab.Controls(0), WebBrowser)
                Dim domain As String = qdomain(InputBox("Enter the target domain:", "Add User Content", If(wb.Url Is Nothing, "", wb.Url.Host)))
                Dim kind As String = InputBox("Enter STYLE or SCRIPT:", "Add User Content", "STYLE").Trim.ToLowerInvariant
                If domain = "" OrElse New String() {"style", "script"}.Contains(kind) = False Then Exit Sub
                Dim code As String = InputBox("Enter the CSS or JavaScript code. It may be edited later:", "Add User Content", If(kind = "style", "body { font-family: Segoe UI !important; }", "document.title = document.title;"))
                If code = "" Then Exit Sub
                Dim enabled As String = If(My.Settings.scriptPreview = "e", "", "e")
                My.Settings.us.Add(domain & "|" & kind & "|" & enabled & "|" & qb64(code))
                My.Settings.Save()
                qpreviewusercontent(My.Settings.us.Count - 1)
                If My.Settings.scriptPreview = "e" AndAlso MessageBox.Show("The new " & kind & " was previewed on the current page. Enable it for future visits?", "Safe Preview", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                    Dim fields As String() = My.Settings.us(My.Settings.us.Count - 1).Split(New Char() {"|"c}, 4)
                    My.Settings.us(My.Settings.us.Count - 1) = fields(0) & "|" & fields(1) & "|e|" & fields(3)
                    My.Settings.Save()
                End If
            Else
                Dim selected As Integer = qusercontentindex()
                If selected < 0 Then Exit Sub
                Dim fields As String() = My.Settings.us(selected).Split(New Char() {"|"c}, 4)
                If fields.Length <> 4 Then Exit Sub
                If action = "EDIT" Then
                    Dim code As String = InputBox("Edit the " & fields(1) & " code:", "Edit User Content", qunb64(fields(3)))
                    If code <> "" Then My.Settings.us(selected) = fields(0) & "|" & fields(1) & "|" & fields(2) & "|" & qb64(code)
                ElseIf action = "TOGGLE" Then
                    My.Settings.us(selected) = fields(0) & "|" & fields(1) & "|" & If(fields(2) = "e", "", "e") & "|" & fields(3)
                ElseIf action = "PREVIEW" Then
                    qpreviewusercontent(selected)
                ElseIf action = "DELETE" Then
                    My.Settings.us.RemoveAt(selected)
                End If
                My.Settings.Save()
            End If
            su.Visible = True
            su.Text = "User scripts and styles updated"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q867(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q866(sender, e)
    End Sub

    Function qusercontentsummary() As String
        Dim lines As New List(Of String)
        For i As Integer = 0 To My.Settings.us.Count - 1
            Dim fields As String() = My.Settings.us(i).Split(New Char() {"|"c}, 4)
            If fields.Length = 4 Then lines.Add((i + 1).ToString & ". " & fields(0) & " " & fields(1) & " [" & If(fields(2) = "e", "enabled", "disabled") & "]")
        Next i
        Return "Saved content:" & vbCrLf & If(lines.Count = 0, "none", String.Join(vbCrLf, lines.ToArray)) & vbCrLf & "Safe preview: " & If(My.Settings.scriptPreview = "e", "on", "off")
    End Function

    Function qusercontentindex() As Integer
        Dim selected As String = InputBox(qusercontentsummary() & vbCrLf & vbCrLf & "Enter the item number:", "User Script And Style Manager", "1")
        Dim index As Integer = 0
        If Integer.TryParse(selected, index) = False Then Return -1
        index -= 1
        If index < 0 OrElse index >= My.Settings.us.Count Then Return -1
        Return index
    End Function

    Sub qpreviewusercontent(ByVal index As Integer)
        If My.Settings.us Is Nothing OrElse index < 0 OrElse index >= My.Settings.us.Count Then Exit Sub
        Dim raw As String = My.Settings.us(index)
        If raw Is Nothing Then Exit Sub
        Dim fields As String() = raw.Split(New Char() {"|"c}, 4)
        If fields.Length <> 4 Then Exit Sub
        Dim tab As TabPage = qleaSelectedTab()
        Dim lea As Global.Lea.Win.V = qleaHost(tab)
        If lea IsNot Nothing Then
            If fields(1) = "style" Then
                qleaPreviewStyle(lea, fields(3))
            Else
                qleaStyleStatus(lea, "Saved script preview requires a legacy tab; Lea page unchanged")
            End If
            Exit Sub
        End If
        Dim wb As WebBrowser = If(tab Is Nothing OrElse tab.Controls.Count = 0, Nothing, TryCast(tab.Controls(0), WebBrowser))
        If wb Is Nothing OrElse wb.IsDisposed Then
            su.Visible = True
            su.Text = "Saved style/script preview requires a legacy tab; Lea page unchanged"
            Exit Sub
        End If
        qrunusercontent(wb, fields(1), qunb64(fields(3)))
    End Sub

    Friend Sub qapplyusercontent(ByVal wb As WebBrowser)
        Try
            If wb Is Nothing OrElse wb.Url Is Nothing OrElse wb.Document Is Nothing Then Exit Sub
            Dim host As String = qdomain(wb.Url.Host)
            For Each raw As String In My.Settings.us
                Dim fields As String() = raw.Split(New Char() {"|"c}, 4)
                If fields.Length = 4 AndAlso fields(2) = "e" AndAlso (host.Equals(fields(0), StringComparison.OrdinalIgnoreCase) OrElse host.EndsWith("." & fields(0), StringComparison.OrdinalIgnoreCase)) Then qrunusercontent(wb, fields(1), qunb64(fields(3)))
            Next raw
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    Sub qrunusercontent(ByVal wb As WebBrowser, ByVal kind As String, ByVal code As String)
        Try
            If wb Is Nothing OrElse wb.Document Is Nothing Then Exit Sub
            If kind = "style" Then
                Dim style As HtmlElement = wb.Document.CreateElement("style")
                style.SetAttribute("type", "text/css")
                style.InnerText = code
                Dim heads As HtmlElementCollection = wb.Document.GetElementsByTagName("head")
                If heads.Count > 0 Then
                    heads(0).AppendChild(style)
                ElseIf wb.Document.Body IsNot Nothing Then
                    wb.Document.Body.AppendChild(style)
                End If
            ElseIf kind = "script" Then
                wb.Document.InvokeScript("eval", New Object() {code})
            End If
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    ' --- Encrypted Local Vault ---
    Sub q868(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim action As String = InputBox(qvaultsummary() & vbCrLf & vbCrLf & "Enter BOOKMARK, NOTE, SESSION, OPEN, or DELETE:", "Encrypted Local Vault", "BOOKMARK").Trim.ToUpperInvariant
            If action = "" Then Exit Sub
            If action = "BOOKMARK" Then
                Dim wb As WebBrowser = CType(k.SelectedTab.Controls(0), WebBrowser)
                If wb.Url Is Nothing Then Exit Sub
                Dim title As String = InputBox("Enter a protected bookmark title:", "Encrypted Local Vault", If(wb.DocumentTitle = "", wb.Url.Host, wb.DocumentTitle)).Replace("|", "-")
                If title <> "" Then My.Settings.vault.Add(qvaultprotect("bookmark|" & title & "|" & wb.Url.ToString))
            ElseIf action = "NOTE" Then
                Dim title As String = InputBox("Enter a protected note title:", "Encrypted Local Vault", "Private Note").Replace("|", "-")
                Dim value As String = InputBox("Enter the protected note:", "Encrypted Local Vault", "")
                If title <> "" AndAlso value <> "" Then My.Settings.vault.Add(qvaultprotect("note|" & title & "|" & value))
            ElseIf action = "SESSION" Then
                Dim title As String = InputBox("Enter a protected session title:", "Encrypted Local Vault", "Protected Session").Replace("|", "-")
                Dim urls As New List(Of String)
                For Each tab As TabPage In qalltabs()
                    If tab.Controls.Count = 0 Then Continue For
                    Dim wb As WebBrowser = TryCast(tab.Controls(0), WebBrowser)
                    Dim url As String = qpersistedtaburl(tab, wb)
                    If url <> "" Then urls.Add(url)
                Next tab
                If title <> "" AndAlso urls.Count > 0 Then My.Settings.vault.Add(qvaultprotect("session|" & title & "|" & String.Join(ControlChars.Tab, urls.ToArray)))
            Else
                Dim selected As Integer = qvaultindex()
                If selected < 0 Then Exit Sub
                If action = "DELETE" Then
                    My.Settings.vault.RemoveAt(selected)
                ElseIf action = "OPEN" Then
                    Dim fields As String() = qvaultunprotect(My.Settings.vault(selected)).Split(New Char() {"|"c}, 3)
                    If fields.Length = 3 Then
                        If fields(0) = "bookmark" Then q1(fields(2))
                        If fields(0) = "note" Then Clipboard.SetText(fields(2)) : MessageBox.Show("Protected note copied to the clipboard.", fields(1), MessageBoxButtons.OK, MessageBoxIcon.Information)
                        If fields(0) = "session" Then qleaRestoreSaved(fields(2).Split(ControlChars.Tab), True)
                    End If
                End If
            End If
            My.Settings.Save()
            su.Visible = True
            su.Text = "Encrypted vault updated"
        Catch ex As Exception
            qlog(ex)
            MessageBox.Show("The encrypted vault could not be opened for this Windows user.", "Encrypted Local Vault", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub
    Sub q869(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q868(sender, e)
    End Sub

    Function qvaultprotect(ByVal value As String) As String
        Dim plain As Byte() = Encoding.UTF8.GetBytes(value)
        Return Convert.ToBase64String(ProtectedData.Protect(plain, Nothing, DataProtectionScope.CurrentUser))
    End Function
    Function qvaultunprotect(ByVal value As String) As String
        Dim encrypted As Byte() = Convert.FromBase64String(value)
        Return Encoding.UTF8.GetString(ProtectedData.Unprotect(encrypted, Nothing, DataProtectionScope.CurrentUser))
    End Function
    Function qvaultsummary() As String
        Dim lines As New List(Of String)
        For i As Integer = 0 To My.Settings.vault.Count - 1
            Try
                Dim fields As String() = qvaultunprotect(My.Settings.vault(i)).Split(New Char() {"|"c}, 3)
                If fields.Length = 3 Then lines.Add((i + 1).ToString & ". [" & fields(0) & "] " & fields(1))
            Catch
                lines.Add((i + 1).ToString & ". [unavailable encrypted record]")
            End Try
        Next i
        Return "Protected items for this Windows user:" & vbCrLf & If(lines.Count = 0, "none", String.Join(vbCrLf, lines.ToArray))
    End Function
    Function qvaultindex() As Integer
        Dim answer As String = InputBox(qvaultsummary() & vbCrLf & vbCrLf & "Enter the item number:", "Encrypted Local Vault", "1")
        Dim index As Integer = 0
        If Integer.TryParse(answer, index) = False Then Return -1
        index -= 1
        If index < 0 OrElse index >= My.Settings.vault.Count Then Return -1
        Return index
    End Function

    ' --- Accessibility Inspector ---
    Sub q870(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls(0), WebBrowser)
            If wb.Document Is Nothing Then Exit Sub
            Dim headings As Integer = 0
            For level As Integer = 1 To 6
                headings += wb.Document.GetElementsByTagName("h" & level.ToString).Count
            Next level
            Dim images As HtmlElementCollection = wb.Document.GetElementsByTagName("img")
            Dim missingAlt As Integer = 0
            For Each image As HtmlElement In images
                If image.GetAttribute("alt").Trim = "" Then missingAlt += 1
            Next image
            Dim links As HtmlElementCollection = wb.Document.Links
            Dim unlabeledLinks As Integer = 0
            For Each link As HtmlElement In links
                If link.InnerText.Trim = "" AndAlso link.GetAttribute("title").Trim = "" AndAlso link.GetElementsByTagName("img").Count = 0 Then unlabeledLinks += 1
            Next link
            Dim inputs As HtmlElementCollection = wb.Document.GetElementsByTagName("input")
            Dim unlabeledInputs As Integer = 0
            For Each input As HtmlElement In inputs
                Dim inputType As String = input.GetAttribute("type").ToLowerInvariant
                If inputType <> "hidden" AndAlso input.GetAttribute("title").Trim = "" AndAlso input.GetAttribute("aria-label").Trim = "" AndAlso input.GetAttribute("id").Trim = "" Then unlabeledInputs += 1
            Next input
            Dim landmarks As Integer = 0
            For Each tag As String In New String() {"main", "nav", "header", "footer", "aside"}
                landmarks += wb.Document.GetElementsByTagName(tag).Count
            Next tag
            Dim title As String = If(wb.DocumentTitle = "", "Accessibility Report", wb.DocumentTitle)
            Dim issues As Integer = missingAlt + unlabeledLinks + unlabeledInputs + If(headings = 0, 1, 0) + If(landmarks = 0, 1, 0)
            Dim html As String = "<!doctype html><html><head><meta charset=""utf-8""><title>Accessibility Inspector</title><style>body{max-width:820px;margin:35px auto;padding:0 24px;font:17px/1.55 Segoe UI,Arial;color:#222}h1{color:#b33}.ok{color:#267326}.warn{color:#a34b00}li{margin:9px 0}</style></head><body>" &
                "<h1>Accessibility Inspector</h1><p><strong>Page:</strong> " & qh(title) & "</p><p><a href=""" & qh(If(wb.Url Is Nothing, "", wb.Url.ToString)) & """>" & qh(If(wb.Url Is Nothing, "", wb.Url.ToString)) & "</a></p>" &
                "<h2>Summary</h2><p class=""" & If(issues = 0, "ok", "warn") & """>" & issues.ToString & " potential issue(s) found.</p><ul>" &
                "<li>Headings: " & headings.ToString & If(headings = 0, " — add a logical heading structure.", "") & "</li>" &
                "<li>Images: " & images.Count.ToString & "; missing alt text: " & missingAlt.ToString & "</li>" &
                "<li>Links: " & links.Count.ToString & "; potentially unlabeled: " & unlabeledLinks.ToString & "</li>" &
                "<li>Inputs: " & inputs.Count.ToString & "; potentially unlabeled: " & unlabeledInputs.ToString & "</li>" &
                "<li>Semantic landmarks: " & landmarks.ToString & If(landmarks = 0, " — consider main, nav, header, footer, or aside regions.", "") & "</li></ul>" &
                "<h2>Manual checks</h2><ul><li>Verify text and control color contrast.</li><li>Navigate the page using only Tab, Shift+Tab, Enter, and Space.</li><li>Confirm focus order follows the visual reading order.</li><li>Confirm dynamic status messages are announced appropriately.</li></ul></body></html>"
            Dim path As String = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Firespider Accessibility " & Guid.NewGuid().ToString("N") & ".html")
            System.IO.File.WriteAllText(path, html, Encoding.UTF8)
            q3(New Uri(path).AbsoluteUri)
            su.Visible = True
            su.Text = issues.ToString & " potential accessibility issue(s) reported"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q871(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q870(sender, e)
    End Sub

    ' --- Local Archive Library ---
    Sub q872(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim wb As WebBrowser = CType(k.SelectedTab.Controls(0), WebBrowser)
            If wb.Document Is Nothing Then Exit Sub
            Dim folder As String = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Firespider Internet", "Archive")
            System.IO.Directory.CreateDirectory(folder)
            Dim title As String = If(wb.DocumentTitle = "", "Archived Page", wb.DocumentTitle)
            Dim stem As String = qsf(title) & " " & Now.ToString("yyyyMMdd HHmmss")
            Dim htmlPath As String = System.IO.Path.Combine(folder, stem & ".html")
            Dim textPath As String = System.IO.Path.Combine(folder, stem & ".txt")
            System.IO.File.WriteAllText(htmlPath, wb.DocumentText, Encoding.UTF8)
            System.IO.File.WriteAllText(textPath, If(wb.Document.Body Is Nothing, "", wb.Document.Body.InnerText), Encoding.UTF8)
            Dim url As String = If(wb.Url Is Nothing, "", wb.Url.ToString)
            My.Settings.archives.Add(Now.ToString("yyyy-MM-dd HH:mm:ss") & "|" & qb64(title) & "|" & qb64(url) & "|" & qb64(htmlPath) & "|" & qb64(textPath))
            My.Settings.Save()
            su.Visible = True
            su.Text = "Page added to the local archive library"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q873(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q872(sender, e)
    End Sub
    Sub q874(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim library As New ArchiveLibraryForm(Me)
            library.Show(Me)
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q875(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q874(sender, e)
    End Sub

    Friend Function qarchiverecords() As List(Of ArchiveRecord)
        Dim records As New List(Of ArchiveRecord)
        For i As Integer = 0 To My.Settings.archives.Count - 1
            Try
                Dim fields As String() = My.Settings.archives(i).Split("|"c)
                If fields.Length = 5 Then records.Add(New ArchiveRecord(i, fields(0), qunb64(fields(1)), qunb64(fields(2)), qunb64(fields(3)), qunb64(fields(4))))
            Catch ex As Exception
                qlog(ex)
            End Try
        Next i
        Return records
    End Function

    Friend Sub qopenarchive(ByVal record As ArchiveRecord)
        If System.IO.File.Exists(record.HtmlPath) Then
            q3(New Uri(record.HtmlPath).AbsoluteUri)
        Else
            su.Visible = True
            su.Text = "Archived page file is missing"
        End If
    End Sub
    Friend Sub qremovearchive(ByVal record As ArchiveRecord, ByVal deleteFiles As Boolean)
        If record.Index >= 0 AndAlso record.Index < My.Settings.archives.Count Then My.Settings.archives.RemoveAt(record.Index)
        If deleteFiles Then
            Try
                If System.IO.File.Exists(record.HtmlPath) Then System.IO.File.Delete(record.HtmlPath)
                If System.IO.File.Exists(record.TextPath) Then System.IO.File.Delete(record.TextPath)
            Catch ex As Exception
                qlog(ex)
            End Try
        End If
        My.Settings.Save()
    End Sub

    Function qb64(ByVal value As String) As String
        Return Convert.ToBase64String(Encoding.UTF8.GetBytes(If(value, "")))
    End Function
    Function qunb64(ByVal value As String) As String
        Try
            Return Encoding.UTF8.GetString(Convert.FromBase64String(value))
        Catch ex As Exception
            Return ""
        End Try
    End Function
End Class

Friend Class DownloadQueueRecord
    Public Path As String
    Public Source As String
    Public Status As String
    Public DateSaved As String
    Sub New(ByVal filePath As String, ByVal sourceUrl As String, ByVal queueStatus As String, ByVal saved As String)
        Path = filePath
        Source = sourceUrl
        Status = queueStatus
        DateSaved = saved
    End Sub
    Public Overrides Function ToString() As String
        Return "[" & Status & "] " & System.IO.Path.GetFileName(Path) & " — " & DateSaved
    End Function
End Class

Friend Class DownloadCenterForm
    Inherits Form
    Dim ownerForm As j
    Dim downloads As New ListBox
    Dim details As New TextBox
    Dim openButton As New Button
    Dim verifyButton As New Button
    Dim retryButton As New Button
    Dim pauseButton As New Button
    Dim cleanButton As New Button
    Sub New(ByVal browser As j)
        ownerForm = browser
        Text = "Download Details And Checksums"
        StartPosition = FormStartPosition.CenterParent
        Size = New Size(800, 520)
        downloads.Dock = DockStyle.Left
        downloads.Width = 360
        details.Dock = DockStyle.Fill
        details.Multiline = True
        details.ReadOnly = True
        details.ScrollBars = ScrollBars.Vertical
        Dim buttons As New FlowLayoutPanel
        buttons.Dock = DockStyle.Bottom
        buttons.Height = 45
        openButton.Text = "Open"
        verifyButton.Text = "SHA-256"
        retryButton.Text = "Retry Source"
        pauseButton.Text = "Pause / Resume"
        cleanButton.Text = "Remove Missing"
        buttons.Controls.AddRange(New Control() {openButton, verifyButton, retryButton, pauseButton, cleanButton})
        Controls.Add(details)
        Controls.Add(downloads)
        Controls.Add(buttons)
        AddHandler downloads.SelectedIndexChanged, AddressOf qdetails
        AddHandler openButton.Click, AddressOf qopen
        AddHandler verifyButton.Click, AddressOf qverify
        AddHandler retryButton.Click, AddressOf qretry
        AddHandler pauseButton.Click, AddressOf qpause
        AddHandler cleanButton.Click, AddressOf qclean
        qrefresh()
    End Sub
    Sub qrefresh()
        downloads.Items.Clear()
        For Each record As DownloadQueueRecord In ownerForm.qdownloadrecords()
            downloads.Items.Add(record)
        Next record
        If downloads.Items.Count > 0 Then downloads.SelectedIndex = 0 Else details.Text = "No tracked downloads or saved files."
    End Sub
    Function qselected() As DownloadQueueRecord
        Return TryCast(downloads.SelectedItem, DownloadQueueRecord)
    End Function
    Sub qdetails(ByVal sender As Object, ByVal e As EventArgs)
        Dim record As DownloadQueueRecord = qselected()
        If record Is Nothing Then Exit Sub
        Dim exists As Boolean = System.IO.File.Exists(record.Path)
        Dim sizeText As String = "Unavailable"
        Dim modified As String = "Unavailable"
        If exists Then
            Dim info As New System.IO.FileInfo(record.Path)
            sizeText = info.Length.ToString("N0") & " bytes"
            modified = info.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")
        End If
        details.Text = "File: " & record.Path & vbCrLf & "Exists: " & If(exists, "Yes", "No") & vbCrLf & "Size: " & sizeText & vbCrLf & "Modified: " & modified & vbCrLf & "Source page: " & record.Source & vbCrLf & "Queue state: " & record.Status & vbCrLf & "Tracked: " & record.DateSaved
    End Sub
    Sub qopen(ByVal sender As Object, ByVal e As EventArgs)
        Dim record As DownloadQueueRecord = qselected()
        If record IsNot Nothing AndAlso System.IO.File.Exists(record.Path) Then Process.Start(record.Path)
    End Sub
    Sub qverify(ByVal sender As Object, ByVal e As EventArgs)
        Dim record As DownloadQueueRecord = qselected()
        If record Is Nothing OrElse System.IO.File.Exists(record.Path) = False Then Exit Sub
        Try
            Dim hash As String
            Using stream As System.IO.FileStream = System.IO.File.OpenRead(record.Path)
                Using sha As SHA256 = SHA256.Create()
                    hash = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant
                End Using
            End Using
            Clipboard.SetText(hash)
            MessageBox.Show("SHA-256 copied to the clipboard:" & vbCrLf & vbCrLf & hash, "Checksum Verified", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show("The checksum could not be calculated.", "Checksum", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub
    Sub qretry(ByVal sender As Object, ByVal e As EventArgs)
        Dim record As DownloadQueueRecord = qselected()
        If record Is Nothing OrElse record.Status = "paused" Then Exit Sub
        If record.Source <> "" Then ownerForm.q1(record.Source)
    End Sub
    Sub qpause(ByVal sender As Object, ByVal e As EventArgs)
        Dim record As DownloadQueueRecord = qselected()
        If record Is Nothing Then Exit Sub
        record.Status = If(record.Status = "paused", "complete", "paused")
        ownerForm.qupdatedownload(record, False)
        qrefresh()
    End Sub
    Sub qclean(ByVal sender As Object, ByVal e As EventArgs)
        For Each record As DownloadQueueRecord In ownerForm.qdownloadrecords().ToArray
            If System.IO.File.Exists(record.Path) = False AndAlso System.IO.Directory.Exists(record.Path) = False Then ownerForm.qupdatedownload(record, True)
        Next record
        qrefresh()
    End Sub
End Class

Friend Class ArchiveRecord
    Public Index As Integer
    Public DateSaved As String
    Public Title As String
    Public Url As String
    Public HtmlPath As String
    Public TextPath As String
    Sub New(ByVal recordIndex As Integer, ByVal saved As String, ByVal pageTitle As String, ByVal pageUrl As String, ByVal htmlFile As String, ByVal textFile As String)
        Index = recordIndex
        DateSaved = saved
        Title = pageTitle
        Url = pageUrl
        HtmlPath = htmlFile
        TextPath = textFile
    End Sub
    Public Overrides Function ToString() As String
        Return DateSaved & " — " & Title
    End Function
End Class

Friend Class ArchiveLibraryForm
    Inherits Form
    Dim ownerForm As j
    Dim searchBox As New TextBox
    Dim results As New ListBox
    Dim details As New TextBox
    Dim openButton As New Button
    Dim removeButton As New Button
    Dim folderButton As New Button
    Sub New(ByVal browser As j)
        ownerForm = browser
        Text = "Local Archive Library"
        StartPosition = FormStartPosition.CenterParent
        Size = New Size(850, 560)
        searchBox.Dock = DockStyle.Top
        searchBox.Font = New Font("Segoe UI", 12.0!)
        results.Dock = DockStyle.Left
        results.Width = 420
        details.Dock = DockStyle.Fill
        details.Multiline = True
        details.ReadOnly = True
        details.ScrollBars = ScrollBars.Vertical
        Dim buttons As New FlowLayoutPanel
        buttons.Dock = DockStyle.Bottom
        buttons.Height = 44
        openButton.Text = "Open Archive"
        removeButton.Text = "Remove"
        folderButton.Text = "Open Folder"
        buttons.Controls.AddRange(New Control() {openButton, removeButton, folderButton})
        Controls.Add(details)
        Controls.Add(results)
        Controls.Add(searchBox)
        Controls.Add(buttons)
        AddHandler searchBox.TextChanged, AddressOf qfilter
        AddHandler results.SelectedIndexChanged, AddressOf qdetails
        AddHandler results.DoubleClick, AddressOf qopen
        AddHandler openButton.Click, AddressOf qopen
        AddHandler removeButton.Click, AddressOf qremove
        AddHandler folderButton.Click, AddressOf qfolder
        qfilter(Nothing, EventArgs.Empty)
    End Sub
    Sub qfilter(ByVal sender As Object, ByVal e As EventArgs)
        Dim query As String = searchBox.Text.Trim
        results.Items.Clear()
        For Each record As ArchiveRecord In ownerForm.qarchiverecords()
            Dim match As Boolean = query = "" OrElse record.Title.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 OrElse record.Url.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 OrElse record.DateSaved.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0
            If match = False AndAlso System.IO.File.Exists(record.TextPath) Then
                Try
                    match = System.IO.File.ReadAllText(record.TextPath).IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0
                Catch
                End Try
            End If
            If match Then results.Items.Add(record)
        Next record
        If results.Items.Count > 0 Then results.SelectedIndex = 0 Else details.Text = "No archived pages match this search."
    End Sub
    Function qselected() As ArchiveRecord
        Return TryCast(results.SelectedItem, ArchiveRecord)
    End Function
    Sub qdetails(ByVal sender As Object, ByVal e As EventArgs)
        Dim record As ArchiveRecord = qselected()
        If record Is Nothing Then Exit Sub
        details.Text = "Title: " & record.Title & vbCrLf & "Saved: " & record.DateSaved & vbCrLf & "Original URL: " & record.Url & vbCrLf & "HTML snapshot: " & record.HtmlPath & vbCrLf & "Text index: " & record.TextPath & vbCrLf & "Available: " & If(System.IO.File.Exists(record.HtmlPath), "Yes", "No")
    End Sub
    Sub qopen(ByVal sender As Object, ByVal e As EventArgs)
        Dim record As ArchiveRecord = qselected()
        If record IsNot Nothing Then ownerForm.qopenarchive(record)
    End Sub
    Sub qremove(ByVal sender As Object, ByVal e As EventArgs)
        Dim record As ArchiveRecord = qselected()
        If record Is Nothing Then Exit Sub
        Dim answer As DialogResult = MessageBox.Show("Yes: remove the record and delete its local files." & vbCrLf & "No: remove only the library record." & vbCrLf & "Cancel: keep everything.", "Remove Archive", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)
        If answer = DialogResult.Cancel Then Exit Sub
        Dim deleteFiles As Boolean = answer = DialogResult.Yes
        ownerForm.qremovearchive(record, deleteFiles)
        qfilter(Nothing, EventArgs.Empty)
    End Sub
    Sub qfolder(ByVal sender As Object, ByVal e As EventArgs)
        Dim record As ArchiveRecord = qselected()
        If record IsNot Nothing AndAlso System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(record.HtmlPath)) Then Process.Start(System.IO.Path.GetDirectoryName(record.HtmlPath))
    End Sub
End Class
