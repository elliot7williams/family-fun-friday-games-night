Imports System.IO
Imports System.Collections.Generic
Imports System.Text

Partial Class j
    Private qleaMenu As ToolStripMenuItem
    Private qleaInspectorMenu As ToolStripMenuItem
    Private qleaInspectorWindow As Form
    Private qleaInspectorTabs As TabControl
    Private qleaInspectorSummary As TextBox
    Private qleaInspectorNodes As TextBox
    Private qleaInspectorCss As TextBox
    Private qleaInspectorRequests As TextBox
    Private qleaInspectorMessages As TextBox
    Private qleaInspectorAccess As TextBox
    Private qleaInspectorTips As ToolTip
    Private Shared qleaSessionRestored As Boolean
    Private qleaStarted As Boolean
    Private qleaStarting As Boolean
    Private qleaInitial As List(Of String)
    Private ReadOnly qleaClock As System.Diagnostics.Stopwatch = If(Environment.GetEnvironmentVariable("LEA_FIRESPIDER_PROFILE") = "1", System.Diagnostics.Stopwatch.StartNew(), Nothing)
    Private qleaTimings As List(Of String)
    Private ReadOnly qleaPaused As New Dictionary(Of TabPage, Boolean)()
    Private ReadOnly qleaPending As New Dictionary(Of Global.Lea.Win.V, String)()
    Private qleaGroupChange As Boolean
    Private Const qleaSessionTabs As Integer = 12
    Private Const qleaSessionBytes As Integer = 65536
    Private Const qleaSessionUrl As Integer = 4096

    Private Sub qleaMark(ByVal phase As String)
        If qleaClock Is Nothing Then Return
        If qleaTimings Is Nothing Then qleaTimings = New List(Of String)()
        If qleaTimings.Count < 32 Then qleaTimings.Add(phase & "=" & qleaClock.ElapsedMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture))
    End Sub

    Private Sub qleaInit(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If qleaMenu IsNot Nothing Then Return
        N.Text = "New Lea Tab"
        N.ToolTipText = "Open a new Lea tab"
        CT.Available = True
        HO.Available = True
        HO.Enabled = True
        re.Available = True
        re.Enabled = True
        qleaMenu = New ToolStripMenuItem("New Legacy Tab")
        qleaMenu.Image = My.Resources.F9
        qleaMenu.BackColor = Color.White
        qleaMenu.ForeColor = Color.Black
        qleaMenu.Padding = New Padding(0)
        qleaMenu.ShortcutKeys = Keys.Control Or Keys.Alt Or Keys.L
        AddHandler qleaMenu.Click, AddressOf qleaLegacyNewClick
        fl.DropDownItems.Insert(Math.Min(1, fl.DropDownItems.Count), qleaMenu)
        qleaInspectorMenu = New ToolStripMenuItem("Lea Inspector")
        qleaInspectorMenu.Image = My.Resources.F9
        qleaInspectorMenu.BackColor = Color.White
        qleaInspectorMenu.ForeColor = Color.Black
        qleaInspectorMenu.Padding = New Padding(0)
        qleaInspectorMenu.ShortcutKeys = Keys.F12
        AddHandler qleaInspectorMenu.Click, AddressOf qleaInspectorClick
        Dim inspectorIndex As Integer = v.DropDownItems.IndexOf(S7)
        If inspectorIndex < 0 Then inspectorIndex = v.DropDownItems.Count
        v.DropDownItems.Insert(inspectorIndex, qleaInspectorMenu)
        qleaWire()
    End Sub

    Private Sub qleaShown(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Shown
        qleaStartup()
    End Sub

    Private Sub qleaStartup()
        If qleaStarted Then Return
        qleaStarted = True
        qleaStarting = True
        Try
            qleaInit(Me, EventArgs.Empty)
            If My.Settings.ps = "e" Then pv = True
            If qleaInitial IsNot Nothing Then
                Dim initial As List(Of String) = qleaInitial
                qleaInitial = Nothing
                If qleaRestoreSaved(initial, True) > 0 Then Return
            End If
            If Not qleaSessionRestored Then
                qleaSessionRestored = True
                If Not pv Then
                    If recoveryRequested AndAlso My.Settings.recovery IsNot Nothing Then
                        Dim recovered = qleaRestoreSaved(My.Settings.recovery.Cast(Of String)())
                        If recovered > 0 Then
                            su.Visible = True
                            su.Text = "Recovered " & recovered.ToString() & " Lea tab(s)"
                            Return
                        End If
                    End If
                    If qleaRestoreSession() > 0 Then Return
                    If My.Settings.rs = "e" AndAlso My.Settings.ls IsNot Nothing Then
                        Dim restored = qleaRestoreSaved(My.Settings.ls.Cast(Of String)())
                        If restored > 0 Then Return
                    End If
                End If
            End If
            qleaNew(qntu())
        Finally
            qleaStarting = False
        End Try
    End Sub

    Private Sub qleaWire()
        RemoveHandler b.Click, AddressOf a1
        RemoveHandler b.MouseHover, AddressOf a2
        RemoveHandler f.Click, AddressOf a3
        RemoveHandler f.MouseHover, AddressOf a4
        RemoveHandler h.Click, AddressOf a5
        RemoveHandler h.MouseHover, AddressOf a6
        RemoveHandler g.Click, AddressOf a7
        RemoveHandler g.MouseHover, AddressOf a8
        RemoveHandler r.Click, AddressOf a9
        RemoveHandler r.MouseHover, AddressOf b1
        RemoveHandler s.Click, AddressOf b3
        RemoveHandler s.MouseHover, AddressOf b4
        RemoveHandler BT.Click, AddressOf b5
        RemoveHandler BT.MouseHover, AddressOf b6
        RemoveHandler FT.Click, AddressOf b7
        RemoveHandler FT.MouseHover, AddressOf b8
        RemoveHandler HO.Click, AddressOf h6
        RemoveHandler HO.MouseHover, AddressOf h7
        RemoveHandler re.Click, AddressOf i3
        RemoveHandler re.MouseHover, AddressOf i4
        RemoveHandler st.Click, AddressOf i5
        RemoveHandler st.MouseHover, AddressOf i6
        RemoveHandler t.Tick, AddressOf tk
        RemoveHandler CT.Click, AddressOf h5
        RemoveHandler pt.Click, AddressOf e9
        RemoveHandler pt.MouseHover, AddressOf g1
        RemoveHandler c.KeyDown, AddressOf i9
        RemoveHandler x.KeyDown, AddressOf j1
        RemoveHandler OP.Click, AddressOf h1
        RemoveHandler OP.MouseHover, AddressOf h2
        RemoveHandler SP.Click, AddressOf h3
        RemoveHandler SP.MouseHover, AddressOf h4
        RemoveHandler SL.Click, AddressOf k4
        RemoveHandler FI.Click, AddressOf q10
        RemoveHandler FI.MouseHover, AddressOf q13
        RemoveHandler Me.FormClosing, AddressOf q9
        RemoveHandler N.Click, AddressOf nc
        RemoveHandler N.MouseHover, AddressOf nh
        RemoveHandler zx.Click, AddressOf zk
        RemoveHandler zx.MouseHover, AddressOf zh
        RemoveHandler U.Click, AddressOf c5
        RemoveHandler U.MouseHover, AddressOf c6
        RemoveHandler RD.Click, AddressOf c7
        RemoveHandler RD.MouseHover, AddressOf c8
        RemoveHandler CO.Click, AddressOf c9
        RemoveHandler CO.MouseHover, AddressOf d1
        RemoveHandler CU.Click, AddressOf d2
        RemoveHandler CU.MouseHover, AddressOf d3
        RemoveHandler PA.Click, AddressOf d4
        RemoveHandler PA.MouseHover, AddressOf d5
        RemoveHandler SA.Click, AddressOf d6
        RemoveHandler SA.MouseHover, AddressOf d7
        RemoveHandler D.Click, AddressOf d8
        RemoveHandler D.MouseHover, AddressOf d9
        For Each item As ToolStripItem In New ToolStripItem() {U, RD, CO, CU, PA, SA, D}
            AddHandler item.Click, AddressOf qleaEditClick
            AddHandler item.MouseHover, AddressOf qleaEditHover
        Next

        AddHandler b.Click, AddressOf qleaBackClick
        AddHandler b.MouseHover, AddressOf qleaBackHover
        AddHandler f.Click, AddressOf qleaForwardClick
        AddHandler f.MouseHover, AddressOf qleaForwardHover
        AddHandler h.Click, AddressOf qleaHomeClick
        AddHandler h.MouseHover, AddressOf qleaHomeHover
        AddHandler g.Click, AddressOf qleaGoClick
        AddHandler g.MouseHover, AddressOf qleaGoHover
        AddHandler r.Click, AddressOf qleaReloadClick
        AddHandler r.MouseHover, AddressOf qleaReloadHover
        AddHandler s.Click, AddressOf qleaSearchClick
        AddHandler s.MouseHover, AddressOf qleaSearchHover
        AddHandler BT.Click, AddressOf qleaBackMenuClick
        AddHandler BT.MouseHover, AddressOf qleaBackMenuHover
        AddHandler FT.Click, AddressOf qleaForwardMenuClick
        AddHandler FT.MouseHover, AddressOf qleaForwardMenuHover
        AddHandler HO.Click, AddressOf qleaHomeMenuClick
        AddHandler HO.MouseHover, AddressOf qleaHomeMenuHover
        AddHandler re.Click, AddressOf qleaReloadMenuClick
        AddHandler re.MouseHover, AddressOf qleaReloadMenuHover
        AddHandler st.Click, AddressOf qleaStopClick
        AddHandler st.MouseHover, AddressOf qleaStopHover
        AddHandler t.Tick, AddressOf qleaTick
        AddHandler CT.Click, AddressOf qleaCloseMenuClick
        AddHandler pt.Click, AddressOf qleaSourceClick
        AddHandler pt.MouseHover, AddressOf qleaSourceHover
        AddHandler c.KeyDown, AddressOf qleaAddressKeyDown
        AddHandler x.KeyDown, AddressOf qleaSearchKeyDown
        AddHandler OP.Click, AddressOf qleaOpenFileClick
        AddHandler OP.MouseHover, AddressOf qleaOpenFileHover
        AddHandler SP.Click, AddressOf qleaSavePageClick
        AddHandler SP.MouseHover, AddressOf qleaSavePageHover
        AddHandler SL.Click, AddressOf qleaSendLinkClick
        AddHandler FI.Click, AddressOf qleaFindClick
        AddHandler FI.MouseHover, AddressOf qleaFindHover
        AddHandler N.Click, AddressOf qleaNewClick
        AddHandler N.MouseHover, AddressOf qleaNewHover
        AddHandler zx.Click, AddressOf qleaNewClick
        AddHandler zx.MouseHover, AddressOf qleaNewHover
        AddHandler k.MouseDown, AddressOf qleaTabMouseDown
        AddHandler Me.FormClosing, AddressOf qleaClosing
    End Sub

    Private Function qleaAt() As Global.Lea.Win.V
        Dim tab = qleaSelectedTab()
        If tab Is Nothing OrElse tab.Controls.Count = 0 Then Return Nothing
        Dim host = TryCast(tab.Controls(0), Global.Lea.Win.V)
        If host IsNot Nothing AndAlso pv Then host.Pv = True
        Return host
    End Function

    Private Sub qleaEditClick(ByVal sender As Object, ByVal e As EventArgs)
        Dim code As String = If(sender Is U, "un", If(sender Is RD, "re", If(sender Is CO, "cp", If(sender Is CU, "cu", If(sender Is PA, "pa", If(sender Is SA, "sa", "de"))))))
        Try
            If c.ComboBox.ContainsFocus OrElse x.ComboBox.ContainsFocus OrElse ActiveControl Is c.ComboBox OrElse ActiveControl Is x.ComboBox Then
                su.Visible = True
                su.Text = "Toolbar edit unavailable"
                Return
            End If
            Dim view = qleaAt()
            If view IsNot Nothing Then
                If view.Ec(code) = 0 Then
                    su.Visible = True
                    su.Text = "No available Lea field edit for this command"
                End If
                Return
            End If
            Dim tab As TabPage = qleaSelectedTab()
            Dim browser As WebBrowser = If(tab Is Nothing OrElse tab.Controls.Count = 0, Nothing, TryCast(tab.Controls(0), WebBrowser))
            If browser Is Nothing OrElse browser.Document Is Nothing Then Return
            Dim command As String = If(code = "un", "Undo", If(code = "re", "Redo", If(code = "cp", "Copy", If(code = "cu", "Cut", If(code = "pa", "Paste", If(code = "sa", "SelectAll", "Delete"))))))
            browser.Document.ExecCommand(command, False, Nothing)
        Catch ex As Exception
            su.Visible = True
            su.Text = "Edit command could not complete"
            qlog(ex)
        End Try
    End Sub

    Private Sub qleaEditHover(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then qleaEditClick(sender, e)
    End Sub

    Private Function qleaSelectedTab() As TabPage
        If k Is Nothing OrElse k.TabPages.Count = 0 Then Return Nothing
        Dim index = k.SelectedIndex
        If index < 0 OrElse index >= k.TabPages.Count Then Return Nothing
        Return k.TabPages(index)
    End Function

    Private Function qleaHost(ByVal tab As TabPage) As Global.Lea.Win.V
        If tab Is Nothing OrElse tab.Controls.Count = 0 Then Return Nothing
        Return TryCast(tab.Controls(0), Global.Lea.Win.V)
    End Function

    Private Function qleaUrl(ByVal tab As TabPage) As String
        If tab Is Nothing OrElse tab.IsDisposed Then Return ""
        Dim browser = If(tab.Controls.Count = 0, Nothing, TryCast(tab.Controls(0), WebBrowser))
        Return qpersistedtaburl(tab, browser)
    End Function

    Private Sub qleaDuplicate()
        Dim tab = qleaSelectedTab()
        Dim target = qleaUrl(tab)
        If target.Length = 0 Then Return
        If qleaHost(tab) IsNot Nothing OrElse qleaPaused.ContainsKey(tab) Then
            qleaCreate(target, pv OrElse qleaPrivate(tab))
        Else
            q3(target)
        End If
    End Sub

    Private Sub qleaReopen()
        Dim target As String = ""
        If My.Settings.rc.Count = 0 Then
            target = qleaSessionTarget(lu)
            lu = ""
        Else
            For visited As Integer = 0 To 24
                If My.Settings.rc.Count = 0 Then Exit For
                target = qleaSessionTarget(My.Settings.rc(0))
                My.Settings.rc.RemoveAt(0)
                If target.Length > 0 Then Exit For
            Next
            lu = If(My.Settings.rc.Count = 0, "", My.Settings.rc(0))
        End If
        If target.Length > 0 Then qleaNew(target)
    End Sub

    Private Sub qleaCloseGroup(ByVal side As String)
        Dim selected = qleaSelectedTab()
        If selected Is Nothing Then Return
        Dim first = If(side = "r", k.SelectedIndex + 1, 0)
        Dim last = If(side = "l", k.SelectedIndex - 1, k.TabCount - 1)
        For index As Integer = last To first Step -1
            Dim tab = k.TabPages(index)
            If tab IsNot selected Then qleaClose(tab)
        Next
        k.SelectedTab = selected
        qleaTabChanged(k, EventArgs.Empty)
    End Sub

    Private Function qleaDomain() As Uri
        Dim address As Uri = Nothing
        If Not Uri.TryCreate(qleaUrl(qleaSelectedTab()), UriKind.Absolute, address) Then Return Nothing
        If address.Scheme <> Uri.UriSchemeHttp AndAlso address.Scheme <> Uri.UriSchemeHttps Then Return Nothing
        Return New UriBuilder(address.Scheme, address.Host, address.Port).Uri
    End Function

    Private Function qleaSite(ByVal target As String) As String
        Dim address As Uri = Nothing
        If Not Uri.TryCreate(qleaSessionTarget(target), UriKind.Absolute, address) Then Return ""
        If address.Scheme <> Uri.UriSchemeHttp AndAlso address.Scheme <> Uri.UriSchemeHttps Then Return ""
        Try
            Return address.IdnHost
        Catch ex As UriFormatException
            Return ""
        End Try
    End Function

    Private Function qleaDomainTabs() As List(Of TabPage)
        Dim result As New List(Of TabPage)()
        Dim site As String = qleaSite(qleaUrl(qleaSelectedTab()))
        If site = "" Then Return result
        For Each tab As TabPage In k.TabPages
            If String.Equals(site, qleaSite(qleaUrl(tab)), StringComparison.OrdinalIgnoreCase) Then result.Add(tab)
        Next
        Return result
    End Function

    Private Sub qleaDomainAction(ByVal command As String)
        Dim selected As TabPage = qleaSelectedTab()
        Dim matches As List(Of TabPage) = qleaDomainTabs()
        su.Visible = True
        If matches.Count = 0 Then
            su.Text = "No HTTP or HTTPS domain selected"
            Return
        End If
        Dim count As Integer = 0
        Select Case command
            Case "rl"
                For Each tab As TabPage In matches
                    If suspendedGroupUrls.ContainsKey(tab) Then Continue For
                    Dim host As Global.Lea.Win.V = qleaHost(tab)
                    If host IsNot Nothing Then
                        If host.Busy Then Continue For
                        If pv Then host.Pv = True
                        qleaReload(host)
                    Else
                        Dim browser As WebBrowser = TryCast(tab.Controls(0), WebBrowser)
                        If browser Is Nothing OrElse browser.IsBusy Then Continue For
                        browser.Refresh()
                    End If
                    count += 1
                Next
                su.Text = count.ToString & " same-domain tab(s) reloaded; " & (matches.Count - count).ToString & " busy or suspended tab(s) skipped"
            Case "cl"
                For Each tab As TabPage In matches.AsEnumerable().Reverse()
                    If tab Is selected Then Continue For
                    qleaClose(tab)
                    count += 1
                Next
                k.SelectedTab = selected
                qleaTabChanged(k, EventArgs.Empty)
                su.Text = count.ToString & " same-domain tab(s) closed"
            Case "bm"
                For Each tab As TabPage In matches
                    My.Settings.bm.Add(qleaUrl(tab))
                Next
                My.Settings.Save()
                qrb()
                su.Text = matches.Count.ToString & " same-domain tab(s) bookmarked"
            Case "dp"
                Dim bytes As Integer = 0
                Dim first As TabPage = Nothing
                For Each tab As TabPage In matches
                    If count >= qleaSessionTabs Then Exit For
                    Dim target As String = qleaSessionTarget(qleaUrl(tab))
                    If target = "" Then Continue For
                    Dim size As Integer = Encoding.UTF8.GetByteCount(target & Environment.NewLine)
                    If bytes + size > qleaSessionBytes Then Continue For
                    Dim host As Global.Lea.Win.V = qleaCreate(target, pv OrElse qleaPrivate(tab))
                    If first Is Nothing Then first = DirectCast(host.Parent, TabPage)
                    bytes += size
                    count += 1
                Next
                If first IsNot Nothing Then k.SelectedTab = first
                su.Text = count.ToString & " same-domain Lea tab(s) duplicated; " & (matches.Count - count).ToString & " skipped by Lea limits"
            Case "wn"
                Dim privateWindow As Boolean = matches.Any(Function(tab) qleaPrivate(tab))
                Dim window As j = qleaWindow(matches.Select(Function(tab) qleaUrl(tab)), privateWindow)
                count = If(window Is Nothing, 0, window.k.TabCount)
                su.Text = count.ToString & " same-domain Lea tab(s) opened in a new window; " & (matches.Count - count).ToString & " skipped by Lea limits"
            Case "nx"
                If matches.Count = 1 Then
                    su.Text = "No other tab found for this domain"
                    Return
                End If
                k.SelectedTab = matches((matches.IndexOf(selected) + 1) Mod matches.Count)
                qleaTabChanged(k, EventArgs.Empty)
                su.Text = "Jumped to next same-domain tab"
        End Select
    End Sub

    Private Function qleaDomainLine(ByVal value As String, ByVal limit As Integer) As String
        value = If(value, "")
        Dim length As Integer = Math.Min(value.Length, Math.Max(0, limit))
        If length > 0 AndAlso Char.IsHighSurrogate(value(length - 1)) Then length -= 1
        Dim chars As Char() = value.ToCharArray(0, length)
        For index As Integer = 0 To chars.Length - 1
            If Char.IsControl(chars(index)) Then chars(index) = " "c
        Next
        Return New String(chars)
    End Function

    Private Function qleaDomainText(ByVal snapshot As Boolean) As String
        Dim matches As List(Of TabPage) = qleaDomainTabs()
        If matches.Count = 0 Then Return ""
        Dim text As New StringBuilder()
        If snapshot Then
            text.AppendLine("Firespider Domain Snapshot")
            text.AppendLine("Current URL: " & qleaDomainLine(qleaUrl(qleaSelectedTab()), qleaSessionUrl))
            text.AppendLine("Domain Root: " & qleaDomain().AbsoluteUri)
            text.AppendLine("Homepage: " & qleaDomainLine(My.Settings.h, qleaSessionUrl))
            text.AppendLine()
            text.AppendLine("Matching Tabs")
        End If
        Dim bytes As Integer = Encoding.UTF8.GetByteCount(text.ToString())
        Dim count As Integer = 0
        For Each tab As TabPage In matches
            Dim line As String = qleaDomainLine(tab.Text, 256) & " - " & qleaUrl(tab) & Environment.NewLine
            Dim size As Integer = Encoding.UTF8.GetByteCount(line)
            If count >= 256 OrElse bytes + size > qleaSessionBytes - 128 Then Exit For
            text.Append(line)
            bytes += size
            count += 1
        Next
        If count < matches.Count Then text.AppendLine("[" & (matches.Count - count).ToString & " tab(s) omitted by export limits]")
        Return text.ToString()
    End Function

    Private Sub qleaPaste(ByVal value As String, ByVal multiple As Boolean)
        su.Visible = True
        value = If(value, "")
        If value.Length > qleaSessionBytes OrElse Encoding.UTF8.GetByteCount(value) > qleaSessionBytes Then
            su.Text = "Clipboard text is too large; no tabs opened"
            Return
        End If
        If String.IsNullOrWhiteSpace(value) Then
            su.Text = "Clipboard is empty"
            Return
        End If
        If Not multiple Then
            Dim target = qleaSessionTarget(value)
            If target = "" AndAlso value.Length <= qleaSessionUrl AndAlso qvl(value) Then target = qleaSessionTarget("http://" & value.Trim())
            If target = "" Then
                su.Text = "Clipboard is not a supported link"
                Return
            End If
            qleaNew(target)
            su.Text = "Clipboard link opened in Lea"
            Return
        End If
        Dim matches As System.Text.RegularExpressions.MatchCollection = System.Text.RegularExpressions.Regex.Matches(value, "(?:https?://|file:///)[^\s""'<>]+", System.Text.RegularExpressions.RegexOptions.IgnoreCase Or System.Text.RegularExpressions.RegexOptions.CultureInvariant)
        Dim opened = qleaRestoreUrls(matches.Cast(Of System.Text.RegularExpressions.Match)().Select(Function(match) match.Value), True)
        su.Text = opened.ToString() & " Lea link(s) opened from clipboard"
        If matches.Count > opened Then su.Text &= "; " & (matches.Count - opened).ToString() & " URL(s) skipped by Lea limits"
    End Sub

    Private Function qleaLegacyTab(ByVal browser As WebBrowser) As TabPage
        If browser Is Nothing OrElse browser.IsDisposed OrElse k Is Nothing Then Return Nothing
        Dim tab = TryCast(browser.Parent, TabPage)
        If tab Is Nothing OrElse tab.IsDisposed OrElse Not k.TabPages.Contains(tab) Then Return Nothing
        If tab.Controls.Count = 0 OrElse tab.Controls(0) IsNot browser Then Return Nothing
        Return tab
    End Function

    Private Function qleaDo(ByVal command As String, Optional ByVal value As String = "") As Boolean
        Dim host = qleaAt()
        Dim selected As TabPage = qleaSelectedTab()
        If host Is Nothing AndAlso selected IsNot Nothing AndAlso qleaPaused.ContainsKey(selected) Then
            If command = "reload" Then
                qleaResume(selected, True)
                Return True
            End If
            If command = "go" OrElse command = "search" OrElse command = "home" Then
                host = qleaResume(selected, False)
            End If
        End If
        If host Is Nothing Then
            Dim tab = qleaSelectedTab()
            Return tab Is Nothing OrElse tab.Controls.Count = 0 OrElse Not TypeOf tab.Controls(0) Is WebBrowser
        End If
        Select Case command
            Case "bk"
                host.Bk()
            Case "fw"
                host.Fw()
            Case "go"
                qleaOpen(host, value, False)
            Case "search"
                qleaOpen(host, value, True)
            Case "home"
                qleaOpen(host, qleaHome(), False)
            Case "reload"
                If host.Busy Then host.St() Else qleaReload(host)
            Case "stop"
                host.St()
        End Select
        qleaState(host)
        Return True
    End Function

    Private Function qleaHover(ByVal command As String, Optional ByVal value As String = "") As Boolean
        If My.Settings.t <> "e" Then Return True
        Return qleaDo(command, value)
    End Function

    Private Sub qleaNewClick(ByVal sender As Object, ByVal e As EventArgs)
        qleaNew(qntu())
    End Sub

    Private Sub qleaNewHover(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then qleaNew(qntu())
    End Sub

    Private Sub qleaLegacyNewClick(ByVal sender As Object, ByVal e As EventArgs)
        q3(qntu())
    End Sub

    Private Function qleaNew(ByVal target As String) As Global.Lea.Win.V
        Return qleaCreate(target, pv)
    End Function

    Private Function qleaCreate(ByVal target As String, ByVal privateView As Boolean) As Global.Lea.Win.V
        Dim tab As New TabPage("Lea")
        Dim host As Global.Lea.Win.V = qleaMount(tab, privateView)
        k.TabPages.Add(tab)
        k.SelectedTab = tab
        I += 1
        If String.IsNullOrWhiteSpace(target) Then target = qleaStart()
        If Visible OrElse qleaStarting Then qleaLoad(host, target) Else host.Go(target)
        Return host
    End Function

    Private Function qleaMount(ByVal tab As TabPage, ByVal privateView As Boolean) As Global.Lea.Win.V
        Dim host As New Global.Lea.Win.V()
        host.Pv = privateView
        host.Dock = DockStyle.Fill
        If tab.Tag Is Nothing Then tab.Tag = "lea"
        tab.Controls.Add(host)
        AddHandler host.Changed, AddressOf qleaChanged
        AddHandler host.Done, AddressOf qleaDone
        AddHandler host.Al, AddressOf qleaAlert
        AddHandler host.Dr, AddressOf qleaDownload
        AddHandler host.Gr, AddressOf qleaGoRequest

        Dim closeMenu As New ContextMenuStrip()
        Dim inspectorItem As New ToolStripMenuItem("Lea Inspector")
        Dim closeItem As New ToolStripMenuItem("Close Lea Tab")
        AddHandler inspectorItem.Click, Sub() qleaInspectorOpen()
        AddHandler closeItem.Click, Sub() qleaClose(tab)
        closeMenu.Items.Add(inspectorItem)
        closeMenu.Items.Add(New ToolStripSeparator())
        closeMenu.Items.Add(closeItem)
        host.ContextMenuStrip = closeMenu
        Return host
    End Function

    Private Function qleaSuspend(ByVal tab As TabPage) As Boolean
        If tab Is Nothing OrElse tab.IsDisposed OrElse qleaPaused.ContainsKey(tab) OrElse Not qalltabs().Contains(tab) Then Return False
        Dim host As Global.Lea.Win.V = qleaHost(tab)
        If host Is Nothing OrElse host.IsDisposed Then Return False
        Dim target As String = qleaUrl(tab)
        qleaPending.TryGetValue(host, target)
        If String.IsNullOrWhiteSpace(target) Then target = qleaUrl(tab)
        target = qleaSessionTarget(target)
        If target = "" Then Return False
        Dim privateView As Boolean = host.Pv OrElse pv
        qleaRelease(host)
        qleaPlaceholder(tab, target, privateView)
        qleaTabChanged(k, EventArgs.Empty)
        Return True
    End Function

    Private Sub qleaPlaceholder(ByVal tab As TabPage, ByVal target As String, ByVal privateView As Boolean)
        Dim placeholder As New Panel With {.Dock = DockStyle.Fill, .BackColor = SystemColors.Window, .AccessibleName = "Lea suspended tab"}
        Dim button As New Button With {.Text = "Resume", .AutoSize = True, .Location = New Point(16, 16)}
        AddHandler button.Click, Sub() qleaResume(tab, True)
        placeholder.Controls.Add(button)
        qleaPaused(tab) = privateView
        suspendedGroupUrls(tab) = target
        tab.Controls.Add(placeholder)
        tab.Text = "[Suspended] " & qfeature6tablabel(target)
        qapplygrouptab(tab)
    End Sub

    Private Sub qleaRelease(ByVal host As Global.Lea.Win.V)
        RemoveHandler host.Changed, AddressOf qleaChanged
        RemoveHandler host.Done, AddressOf qleaDone
        RemoveHandler host.Al, AddressOf qleaAlert
        RemoveHandler host.Dr, AddressOf qleaDownload
        RemoveHandler host.Gr, AddressOf qleaGoRequest
        qleaPending.Remove(host)
        If host.Parent IsNot Nothing Then host.Parent.Controls.Remove(host)
        Dim menu As ContextMenuStrip = host.ContextMenuStrip
        host.ContextMenuStrip = Nothing
        If menu IsNot Nothing Then menu.Dispose()
        host.Dispose()
    End Sub

    Private Function qleaResume(ByVal tab As TabPage, ByVal navigate As Boolean) As Global.Lea.Win.V
        If tab Is Nothing OrElse tab.IsDisposed OrElse Not qleaPaused.ContainsKey(tab) Then Return Nothing
        If Not qalltabs().Contains(tab) Then Return Nothing
        Dim target As String = ""
        If Not suspendedGroupUrls.TryGetValue(tab, target) Then Return Nothing
        target = qleaSessionTarget(target)
        If target = "" Then Return Nothing
        Dim privateView As Boolean = qleaPaused(tab) OrElse pv
        For Each control As Control In tab.Controls.Cast(Of Control)().ToArray()
            tab.Controls.Remove(control)
            control.Dispose()
        Next
        Dim host As Global.Lea.Win.V = qleaMount(tab, privateView)
        qleaPaused.Remove(tab)
        suspendedGroupUrls.Remove(tab)
        feature6LazyTabs.Remove(tab)
        feature6IdleSince(tab) = Now
        If navigate Then qleaLoad(host, target)
        Return host
    End Function

    Private Function qleaPrivate(ByVal tab As TabPage) As Boolean
        If qleaPaused.ContainsKey(tab) Then Return qleaPaused(tab)
        Dim host As Global.Lea.Win.V = qleaHost(tab)
        Return host IsNot Nothing AndAlso host.Pv
    End Function

    Private Function qleaStart() As String
        Dim localPage = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Lea", "start.html")
        If File.Exists(localPage) Then Return localPage
        If Not String.IsNullOrWhiteSpace(My.Settings.h) Then Return My.Settings.h
        Return "https://example.com"
    End Function

    Private Function qleaHome() As String
        If Not String.IsNullOrWhiteSpace(My.Settings.h) Then Return My.Settings.h.Trim()
        Return qleaStart()
    End Function

    Private Async Sub qleaOpen(ByVal host As Global.Lea.Win.V, ByVal input As String, ByVal searchOnly As Boolean)
        Dim value = If(input, "").Trim()
        Dim aliasAddress As String = ""
        If qresolvealias(value, aliasAddress) Then value = aliasAddress
        If value.Length = 0 Then Return
        Dim address As Uri = Nothing
        Dim absolute = Uri.TryCreate(value, UriKind.Absolute, address) AndAlso (address.Scheme = Uri.UriSchemeHttp OrElse address.Scheme = Uri.UriSchemeHttps OrElse address.Scheme = Uri.UriSchemeFile OrElse address.Scheme = "about")
        If searchOnly OrElse (Not absolute AndAlso (value.Contains(" ") OrElse Not value.Contains("."))) Then
            value = My.Settings.se & Uri.EscapeDataString(value)
        ElseIf absolute Then
            value = address.AbsoluteUri
        ElseIf Not value.Contains("://") AndAlso Not value.StartsWith("about:") AndAlso Not value.StartsWith("file:") Then
            value = "http://" & value
        End If
        Await qleaLoadA(host, value)
    End Sub

    Private Async Sub qleaLoad(ByVal host As Global.Lea.Win.V, ByVal target As String)
        Await qleaLoadA(host, target)
    End Sub

    Private Async Function qleaLoadA(ByVal host As Global.Lea.Win.V, ByVal target As String) As Threading.Tasks.Task
        If host Is Nothing OrElse host.IsDisposed OrElse IsDisposed Then Return
        Dim owned As Boolean = Not qleaPending.ContainsKey(host)
        If owned Then qleaPending(host) = qleaSessionTarget(target)
        Try
            su.Visible = True
            su.Text = "Lea loading"
            r.Image = My.Resources.F7
            Await host.GoA(target)
        Catch ex As Exception
            If host.IsDisposed OrElse IsDisposed Then Return
            su.Visible = True
            su.Text = "Lea: " & ex.Message
            r.Image = My.Resources.F8
        Finally
            If owned Then qleaPending.Remove(host)
        End Try
    End Function

    Private Async Sub qleaReload(ByVal host As Global.Lea.Win.V)
        Try
            su.Visible = True
            su.Text = "Lea loading"
            r.Image = My.Resources.F7
            Await host.RlA()
        Catch ex As Exception
            If host.IsDisposed OrElse IsDisposed Then Return
            su.Visible = True
            su.Text = "Lea: " & ex.Message
            r.Image = My.Resources.F8
        End Try
    End Sub

    Private Sub qleaChanged(ByVal sender As Object, ByVal e As EventArgs)
        Dim host = TryCast(sender, Global.Lea.Win.V)
        qleaState(host)
        qleaInspectorFollow(host)
    End Sub

    Private Sub qleaDone(ByVal sender As Object, ByVal e As EventArgs)
        r.Image = My.Resources.F8
        su.Visible = False
        Dim host = TryCast(sender, Global.Lea.Win.V)
        qleaState(host)
        If host IsNot Nothing Then qapplygrouptab(TryCast(host.Parent, TabPage))
        qleaInspectorFollow(host)
    End Sub

    Private Sub qleaAlert(ByVal sender As Object, ByVal e As EventArgs)
        Dim host = TryCast(sender, Global.Lea.Win.V)
        If host Is Nothing Then Return
        Dim message = If(host.Am, "")
        If message.Length > 160 Then message = message.Substring(0, 160)
        su.Visible = True
        su.Text = "Lea: " & message
    End Sub

    Private Sub qleaGoRequest(ByVal sender As Object, ByVal e As EventArgs)
        Dim host = TryCast(sender, Global.Lea.Win.V)
        If host Is Nothing OrElse host.Gu Is Nothing Then Return
        qleaLoad(host, host.Gu.AbsoluteUri)
    End Sub

    Private Function qleaFind(ByVal host As Global.Lea.Win.V, ByVal term As String, Optional ByVal advance As Boolean = False) As Integer
        If host Is Nothing Then Return 0
        Dim value = If(term, "").Trim()
        If value.Length = 0 Then
            host.Fc()
        ElseIf advance AndAlso String.Equals(host.Fq, value, StringComparison.OrdinalIgnoreCase) AndAlso host.Fn > 0 Then
            host.Nx()
        Else
            host.Fd(value)
        End If
        su.Visible = True
        If host.Fn = 0 Then
            su.Text = "Lea: no page match found"
        Else
            su.Text = "Lea: match " & host.Fx.ToString() & " of " & host.Fn.ToString()
        End If
        Return host.Fn
    End Function

    Private Sub qleaFindClick(ByVal sender As Object, ByVal e As EventArgs)
        Dim host = qleaAt()
        If host Is Nothing Then
            q10(sender, e)
            Return
        End If
        Dim term As String = InputBox("Enter text to find on this page:", "Find In Current Page", host.Fq)
        If term.Length = 0 Then Return
        qleaFind(host, term, True)
    End Sub

    Private Sub qleaFindHover(ByVal sender As Object, ByVal e As EventArgs)
        If qleaAt() Is Nothing Then
            q13(sender, e)
        ElseIf My.Settings.t = "e" Then
            qleaFindClick(sender, e)
        End If
    End Sub

    Private Function qleaDownloadPath(ByVal host As Global.Lea.Win.V, ByVal address As Uri, ByVal path As String) As Boolean
        If host Is Nothing OrElse address Is Nothing OrElse String.IsNullOrWhiteSpace(path) Then Return False
        Dim result = host.Dl(address.AbsoluteUri, path)
        su.Visible = True
        su.Text = "Lea: downloaded " & result.Sz.ToString() & " bytes to " & result.P
        Return File.Exists(result.P)
    End Function

    Private Async Sub qleaDownload(ByVal sender As Object, ByVal e As EventArgs)
        Dim host = TryCast(sender, Global.Lea.Win.V)
        If host Is Nothing OrElse host.Du Is Nothing Then Return
        Using dialog As New SaveFileDialog()
            qsavefolder(dialog)
            dialog.FileName = qleaSafeName(host.Dn)
            dialog.Filter = "All files (*.*)|*.*"
            dialog.CheckPathExists = True
            dialog.Title = "Save Lea Download"
            If dialog.ShowDialog(Me) <> DialogResult.OK Then Return
            su.Visible = True
            su.Text = "Lea: downloading to " & dialog.FileName
            Try
                Dim result = Await host.DlA(host.Du.AbsoluteUri, dialog.FileName)
                qad(result.P)
                su.Text = "Lea: downloaded " & result.Sz.ToString() & " bytes to " & result.P
            Catch ex As Exception
                su.Text = "Lea: " & ex.Message
            End Try
        End Using
    End Sub

    Private Sub qleaInspectorClick(ByVal sender As Object, ByVal e As EventArgs)
        qleaInspectorOpen()
    End Sub

    Private Function qleaInspectorOpen() As Form
        If qleaAt() Is Nothing Then
            su.Visible = True
            su.Text = "Select a Lea tab to open Lea Inspector"
            Return Nothing
        End If
        If qleaInspectorWindow Is Nothing OrElse qleaInspectorWindow.IsDisposed Then qleaInspectorBuild()
        qleaInspectorSnapshot()
        If Visible Then
            If Not qleaInspectorWindow.Visible Then qleaInspectorWindow.Show(Me)
            qleaInspectorWindow.BringToFront()
        Else
            qleaInspectorWindow.CreateControl()
        End If
        Return qleaInspectorWindow
    End Function

    Private Sub qleaInspectorBuild()
        qleaInspectorWindow = New Form()
        qleaInspectorWindow.Name = "LeaInspector"
        qleaInspectorWindow.Text = "Lea Inspector"
        qleaInspectorWindow.Icon = Icon
        qleaInspectorWindow.Size = New Size(760, 520)
        qleaInspectorWindow.MinimumSize = New Size(480, 320)
        qleaInspectorWindow.StartPosition = FormStartPosition.CenterParent
        qleaInspectorWindow.FormBorderStyle = FormBorderStyle.SizableToolWindow
        qleaInspectorWindow.ShowInTaskbar = False
        qleaInspectorWindow.KeyPreview = True
        AddHandler qleaInspectorWindow.KeyDown, AddressOf qleaInspectorKeyDown
        AddHandler qleaInspectorWindow.FormClosed, AddressOf qleaInspectorClosed

        Dim header As New TableLayoutPanel()
        header.Dock = DockStyle.Top
        header.Height = 34
        header.ColumnCount = 4
        header.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        header.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 32))
        header.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 32))
        header.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 32))
        Dim title As New Label()
        title.Text = "Lea Inspector"
        title.Dock = DockStyle.Fill
        title.Padding = New Padding(7, 8, 0, 0)
        Dim refresh As New Button()
        Dim clear As New Button()
        Dim closeButton As New Button()
        qleaInspectorButton(refresh, "R", AddressOf qleaInspectorRefreshClick)
        qleaInspectorButton(clear, "C", AddressOf qleaInspectorClearClick)
        qleaInspectorButton(closeButton, "X", AddressOf qleaInspectorCloseClick)
        qleaInspectorTips = New ToolTip()
        qleaInspectorTips.SetToolTip(refresh, "Refresh diagnostics")
        qleaInspectorTips.SetToolTip(clear, "Clear request activity")
        qleaInspectorTips.SetToolTip(closeButton, "Close inspector")
        header.Controls.Add(title, 0, 0)
        header.Controls.Add(refresh, 1, 0)
        header.Controls.Add(clear, 2, 0)
        header.Controls.Add(closeButton, 3, 0)

        qleaInspectorTabs = New TabControl()
        qleaInspectorTabs.Name = "LeaInspectorTabs"
        qleaInspectorTabs.Dock = DockStyle.Fill
        qleaInspectorSummary = qleaInspectorText("LeaInspectorSummary")
        qleaInspectorNodes = qleaInspectorText("LeaInspectorNodes")
        qleaInspectorCss = qleaInspectorText("LeaInspectorCss")
        qleaInspectorRequests = qleaInspectorText("LeaInspectorRequests")
        qleaInspectorMessages = qleaInspectorText("LeaInspectorMessages")
        qleaInspectorAccess = qleaInspectorText("LeaInspectorAccess")
        qleaInspectorPage("Summary", qleaInspectorSummary)
        qleaInspectorPage("Nodes", qleaInspectorNodes)
        qleaInspectorPage("CSS", qleaInspectorCss)
        qleaInspectorPage("Requests", qleaInspectorRequests)
        qleaInspectorPage("Messages", qleaInspectorMessages)
        qleaInspectorPage("Access", qleaInspectorAccess)
        qleaInspectorWindow.Controls.Add(qleaInspectorTabs)
        qleaInspectorWindow.Controls.Add(header)
    End Sub

    Private Sub qleaInspectorButton(ByVal button As Button, ByVal text As String, ByVal action As EventHandler)
        button.Text = text
        button.Dock = DockStyle.Fill
        button.FlatStyle = FlatStyle.Flat
        button.Margin = New Padding(2)
        button.TabStop = False
        AddHandler button.Click, action
    End Sub

    Private Function qleaInspectorText(ByVal name As String) As TextBox
        Dim box As New TextBox()
        box.Name = name
        box.Dock = DockStyle.Fill
        box.Multiline = True
        box.ReadOnly = True
        box.WordWrap = False
        box.ScrollBars = ScrollBars.Both
        box.BorderStyle = BorderStyle.None
        box.BackColor = Color.White
        box.Font = New Font("Consolas", 9.0F)
        Return box
    End Function

    Private Sub qleaInspectorPage(ByVal name As String, ByVal content As TextBox)
        Dim page As New TabPage(name)
        page.Padding = New Padding(4)
        page.Controls.Add(content)
        qleaInspectorTabs.TabPages.Add(page)
    End Sub

    Private Function qleaInspectorSnapshot() As Boolean
        If qleaInspectorSummary Is Nothing Then Return False
        Dim host = qleaAt()
        If host Is Nothing Then
            qleaInspectorSummary.Text = "Select a Lea tab"
            qleaInspectorNodes.Clear()
            qleaInspectorCss.Clear()
            qleaInspectorRequests.Clear()
            qleaInspectorMessages.Clear()
            qleaInspectorAccess.Clear()
            Return False
        End If
        Dim data = host.Dg()
        If data Is Nothing Then
            qleaInspectorSummary.Text = "No page"
            qleaInspectorNodes.Clear()
            qleaInspectorCss.Clear()
            qleaInspectorRequests.Clear()
            qleaInspectorMessages.Clear()
            qleaInspectorAccess.Clear()
            Return False
        End If
        qleaInspectorSummary.Lines = New String() {
            If(data.T, ""),
            If(data.U Is Nothing, "", data.U.ToString()),
            "",
            "Response   " & data.St.ToString() & "  " & If(data.Ct, "") & "  " & data.Sz.ToString() & " bytes" & If(data.Ca, "  cache", ""),
            "Document   " & data.N.ToString() & " nodes  " & data.B.ToString() & " blocks  " & host.Axs.Count.ToString() & " access  " & data.Vw.ToString() & "px viewport",
            "Content    " & data.L.ToString() & " links  " & data.F.ToString() & " fields  " & data.C.ToString() & " CSS rules  " & data.Vc.ToString() & " custom  " & data.Ic.ToString() & " important",
            "Commands   " & data.J.ToString() & " accepted  " & data.V.ToString() & " variables  " & data.M.ToString() & " alerts",
            "Engine     " & data.Cc.ToString() & " memory  " & data.Ch.ToString() & " reuses  " & data.Kc.ToString() & " cookies",
            "Disk       " & data.Dc.ToString() & " entries  " & data.Db.ToString() & " bytes  " & data.Dh.ToString() & " hits  " & data.Dw.ToString() & " stores  " & data.Dr.ToString() & " recovers  " & data.De.ToString() & " evicts"
        }
        qleaInspectorNodes.Lines = qleaInspectorCopy(data.Ns)
        qleaInspectorCss.Lines = qleaInspectorCopy(data.Cs)
        qleaInspectorRequests.Lines = qleaInspectorCopy(data.Rs)
        Dim messages As New List(Of String)()
        For Each item As String In data.Ms
            messages.Add(item)
        Next
        If data.Vs.Count > 0 Then
            messages.Add("")
            messages.Add("variables")
            For Each item As String In data.Vs
                messages.Add(item)
            Next
        End If
        qleaInspectorMessages.Lines = messages.ToArray()
        qleaInspectorAccess.Lines = qleaInspectorAccessLines(host)
        Return True
    End Function

    Private Function qleaInspectorAccessLines(ByVal host As Global.Lea.Win.V) As String()
        Dim lines As New List(Of String)()
        For Each item As Global.Lea.Engine.Ax In host.Axs
            Dim depth As Integer = 0
            Dim parent = item.Up
            While parent IsNot Nothing AndAlso depth < 12
                depth += 1
                parent = parent.Up
            End While
            Dim line As New StringBuilder()
            line.Append(New String(" "c, depth * 2)).Append(item.K)
            If item.N.Length > 0 Then line.Append(" """).Append(item.N).Append("""")
            If item.V.Length > 0 Then line.Append(" = ").Append(item.V)
            If item.St <> 0 Then line.Append(" [").Append(item.St).Append("]")
            If item.Lv > 0 Then line.Append(" level ").Append(item.Lv)
            lines.Add(line.ToString())
        Next
        Return lines.ToArray()
    End Function

    Private Function qleaInspectorCopy(ByVal values As IList(Of String)) As String()
        If values.Count = 0 Then Return New String() {}
        Dim result(values.Count - 1) As String
        values.CopyTo(result, 0)
        Return result
    End Function

    Private Function qleaInspectorClear() As Boolean
        Dim host = qleaAt()
        If host Is Nothing Then Return False
        host.Dcl()
        qleaInspectorSnapshot()
        su.Visible = True
        su.Text = "Lea request activity cleared"
        Return True
    End Function

    Private Sub qleaInspectorRefreshClick(ByVal sender As Object, ByVal e As EventArgs)
        qleaInspectorSnapshot()
    End Sub

    Private Sub qleaInspectorClearClick(ByVal sender As Object, ByVal e As EventArgs)
        qleaInspectorClear()
    End Sub

    Private Sub qleaInspectorCloseClick(ByVal sender As Object, ByVal e As EventArgs)
        If qleaInspectorWindow IsNot Nothing Then qleaInspectorWindow.Close()
    End Sub

    Private Sub qleaInspectorKeyDown(ByVal sender As Object, ByVal e As KeyEventArgs)
        If e.KeyCode = Keys.F5 Then
            qleaInspectorSnapshot()
            e.SuppressKeyPress = True
        ElseIf e.KeyCode = Keys.Escape Then
            qleaInspectorWindow.Close()
            e.SuppressKeyPress = True
        End If
    End Sub

    Private Sub qleaInspectorClosed(ByVal sender As Object, ByVal e As FormClosedEventArgs)
        If qleaInspectorTips IsNot Nothing Then qleaInspectorTips.Dispose()
        qleaInspectorTips = Nothing
        qleaInspectorWindow = Nothing
        qleaInspectorTabs = Nothing
        qleaInspectorSummary = Nothing
        qleaInspectorNodes = Nothing
        qleaInspectorCss = Nothing
        qleaInspectorRequests = Nothing
        qleaInspectorMessages = Nothing
        qleaInspectorAccess = Nothing
    End Sub

    Private Sub qleaInspectorFollow(ByVal host As Global.Lea.Win.V)
        If host Is Nothing OrElse qleaInspectorWindow Is Nothing OrElse qleaInspectorWindow.IsDisposed Then Return
        If Object.ReferenceEquals(host, qleaAt()) Then qleaInspectorSnapshot()
    End Sub

    Private Sub qleaState(ByVal host As Global.Lea.Win.V)
        If host Is Nothing Then Return
        Dim tab = TryCast(host.Parent, TabPage)
        If tab IsNot Nothing Then tab.Text = If(tab.ToolTipText.StartsWith("Tab group:", StringComparison.OrdinalIgnoreCase), ChrW(&H25CF) & " ", "") & If(String.IsNullOrWhiteSpace(host.T), "Lea", host.T)
        If k.SelectedTab IsNot tab Then Return
        c.Text = If(host.U Is Nothing, "", host.U.ToString())
        Text = ProductName & " - " & If(tab Is Nothing, "Lea", tab.Text)
        b.Enabled = host.CanBk
        BT.Enabled = host.CanBk
        f.Enabled = host.CanFw
        FT.Enabled = host.CanFw
    End Sub

    Private Sub qleaTabChanged(ByVal sender As Object, ByVal e As EventArgs) Handles k.SelectedIndexChanged
        Dim selected As TabPage = qleaSelectedTab()
        If selected IsNot Nothing AndAlso qleaPaused.ContainsKey(selected) Then
            c.Text = qleaUrl(selected)
            Text = ProductName & " - " & selected.Text
            b.Enabled = False
            BT.Enabled = False
            f.Enabled = False
            FT.Enabled = False
            If qleaInspectorWindow IsNot Nothing Then qleaInspectorSnapshot()
            Return
        End If
        Dim host = qleaAt()
        If host IsNot Nothing Then
            qleaState(host)
            If qleaInspectorWindow IsNot Nothing Then qleaInspectorSnapshot()
            Return
        End If
        If qleaInspectorWindow IsNot Nothing Then qleaInspectorSnapshot()
        If k.SelectedTab Is Nothing OrElse k.SelectedTab.Controls.Count = 0 Then Return
        Dim browser = TryCast(k.SelectedTab.Controls(0), WebBrowser)
        If browser Is Nothing Then Return
        k.SelectedTab.Text = browser.DocumentTitle
        c.Text = If(browser.Url Is Nothing, "", browser.Url.ToString())
        b.Enabled = browser.CanGoBack
        BT.Enabled = browser.CanGoBack
        f.Enabled = browser.CanGoForward
        FT.Enabled = browser.CanGoForward
    End Sub

    Private Sub qleaTick(ByVal sender As Object, ByVal e As EventArgs)
        Dim host = qleaAt()
        If host IsNot Nothing Then
            su.Text = If(host.Busy, "Lea loading", "Lea ready")
            qleaState(host)
            Return
        End If

        Dim tab = qleaSelectedTab()
        If tab IsNot Nothing AndAlso qleaPaused.ContainsKey(tab) Then Return
        If tab Is Nothing OrElse tab.Controls.Count = 0 Then
            su.Text = "Ready"
            Return
        End If
        Dim browser = TryCast(tab.Controls(0), WebBrowser)
        su.Text = If(browser Is Nothing OrElse browser.StatusText Is Nothing, "Ready", browser.StatusText)
    End Sub

    Private Sub qleaCloseMenuClick(ByVal sender As Object, ByVal e As EventArgs)
        qleaClose(qleaSelectedTab())
    End Sub

    Private Sub qleaTabMouseDown(ByVal sender As Object, ByVal e As MouseEventArgs)
        Dim tab As TabPage = qleaSelectedTab()
        If tab Is Nothing OrElse (qleaAt() Is Nothing AndAlso Not qleaPaused.ContainsKey(tab)) Then Return
        If e.Button = MouseButtons.Middle Then
            qleaClose(k.SelectedTab)
            Return
        End If
        If e.Button <> MouseButtons.Left OrElse k.SelectedIndex < 0 Then Return
        Dim tabRect = k.GetTabRect(k.SelectedIndex)
        Dim closeRect As New RectangleF(tabRect.Left + tm, tabRect.Top + tm, tabRect.Width - 4 * tm, tabRect.Height - 4 * tm)
        If e.X >= closeRect.Right - mx AndAlso e.X <= closeRect.Right AndAlso e.Y >= closeRect.Top AndAlso e.Y <= closeRect.Top + mx Then qleaClose(k.SelectedTab)
    End Sub

    Private Function qleaSourceText() As String
        Dim host = qleaAt()
        If host Is Nothing OrElse host.At Is Nothing Then Return ""
        Return If(host.H, "")
    End Function

    Private Sub qleaSourceClick(ByVal sender As Object, ByVal e As EventArgs)
        If qleaAt() Is Nothing Then
            e9(sender, e)
            Return
        End If
        Dim source = qleaSourceText()
        If source.Length = 0 Then
            su.Visible = True
            su.Text = "No Lea page source to show"
            Return
        End If
        F3.Show()
        F3.RN.Text = source
        su.Visible = True
        su.Text = "Lea page source opened"
    End Sub

    Private Sub qleaSourceHover(ByVal sender As Object, ByVal e As EventArgs)
        If qleaAt() Is Nothing Then
            g1(sender, e)
        ElseIf My.Settings.t = "e" Then
            qleaSourceClick(sender, e)
        End If
    End Sub

    Private Sub qleaAddressKeyDown(ByVal sender As Object, ByVal e As KeyEventArgs)
        If qleaAt() Is Nothing Then
            i9(sender, e)
            Return
        End If
        If e.KeyCode <> Keys.Enter OrElse String.IsNullOrWhiteSpace(c.Text) Then Return
        qleaDo("go", c.Text)
        e.Handled = True
        e.SuppressKeyPress = True
    End Sub

    Private Sub qleaSearchKeyDown(ByVal sender As Object, ByVal e As KeyEventArgs)
        If qleaAt() Is Nothing Then
            j1(sender, e)
            Return
        End If
        If e.KeyCode <> Keys.Enter OrElse String.IsNullOrWhiteSpace(x.Text) OrElse x.Text = "search" Then Return
        qleaDo("search", x.Text)
        e.Handled = True
        e.SuppressKeyPress = True
    End Sub

    Private Function qleaOpenPath(ByVal path As String) As Boolean
        Dim host = qleaAt()
        If host Is Nothing OrElse String.IsNullOrWhiteSpace(path) OrElse Not File.Exists(path) Then Return False
        qleaLoad(host, System.IO.Path.GetFullPath(path))
        Return True
    End Function

    Private Sub qleaOpenFileClick(ByVal sender As Object, ByVal e As EventArgs)
        If qleaAt() Is Nothing Then
            h1(sender, e)
            Return
        End If
        Using dialog As New OpenFileDialog()
            dialog.Title = "Open"
            dialog.CheckFileExists = True
            dialog.Filter = "Web pages (*.html;*.htm)|*.html;*.htm|Text files (*.txt)|*.txt|All files (*.*)|*.*"
            If dialog.ShowDialog(Me) <> DialogResult.OK Then Return
            If qleaOpenPath(dialog.FileName) Then
                qad(dialog.FileName)
                su.Visible = True
                su.Text = "Lea file opened"
            End If
        End Using
    End Sub

    Private Sub qleaOpenFileHover(ByVal sender As Object, ByVal e As EventArgs)
        If qleaAt() Is Nothing Then
            h2(sender, e)
        ElseIf My.Settings.t = "e" Then
            qleaOpenFileClick(sender, e)
        End If
    End Sub

    Private Function qleaSafeName(ByVal title As String) As String
        Dim name = If(title, "").Trim()
        If name.Length = 0 Then name = "Lea page"
        For Each invalid As Char In Path.GetInvalidFileNameChars()
            name = name.Replace(invalid, "_"c)
        Next
        Return name
    End Function

    Private Function qleaSavePath(ByVal path As String) As Boolean
        Dim source = qleaSourceText()
        If source.Length = 0 OrElse String.IsNullOrWhiteSpace(path) Then Return False
        File.WriteAllText(path, source, New System.Text.UTF8Encoding(False))
        Return True
    End Function

    Private Sub qleaSavePageClick(ByVal sender As Object, ByVal e As EventArgs)
        Dim host = qleaAt()
        If host Is Nothing Then
            h3(sender, e)
            Return
        End If
        Using dialog As New SaveFileDialog()
            qsavefolder(dialog)
            dialog.FileName = qleaSafeName(host.T) & ".html"
            dialog.Filter = "HTML (*.html)|*.html|All files (*.*)|*.*"
            dialog.CheckPathExists = True
            dialog.Title = "Save Lea Page"
            If dialog.ShowDialog(Me) <> DialogResult.OK Then Return
            Try
                If qleaSavePath(dialog.FileName) Then
                    qad(dialog.FileName)
                    su.Visible = True
                    su.Text = "Lea page saved"
                End If
            Catch ex As Exception
                su.Visible = True
                su.Text = "Lea: " & ex.Message
            End Try
        End Using
    End Sub

    Private Sub qleaSavePageHover(ByVal sender As Object, ByVal e As EventArgs)
        If qleaAt() Is Nothing Then
            h4(sender, e)
        ElseIf My.Settings.t = "e" Then
            qleaSavePageClick(sender, e)
        End If
    End Sub

    Private Function qleaMailLink() As String
        Dim host = qleaAt()
        If host Is Nothing OrElse host.U Is Nothing Then Return ""
        Return "mailto:?subject=" & Uri.EscapeDataString(If(host.T, "")) & "&body=" & Uri.EscapeDataString(host.U.AbsoluteUri)
    End Function

    Private Sub qleaSendLinkClick(ByVal sender As Object, ByVal e As EventArgs)
        Dim link = qleaMailLink()
        If link.Length = 0 Then
            k4(sender, e)
            Return
        End If
        Try
            Process.Start(link)
            su.Visible = True
            su.Text = "Send Lea link opened"
        Catch ex As Exception
            su.Visible = True
            su.Text = "Lea: " & ex.Message
        End Try
    End Sub

    Private Function qleaSessionPath() As String
        Dim custom = Environment.GetEnvironmentVariable("LEA_FIRESPIDER_SESSION")
        If Not String.IsNullOrWhiteSpace(custom) Then Return custom
        Dim folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Firespider Internet")
        Return Path.Combine(folder, "Lea.session")
    End Function

    Private Sub qleaSaveSession()
        Dim sessionFile = qleaSessionPath()
        If pv Then
            If File.Exists(sessionFile) Then File.Delete(sessionFile)
            Return
        End If
        Dim urls As New List(Of String)()
        Dim bytes As Integer = 0
        For Each tab As TabPage In qalltabs()
            If urls.Count >= qleaSessionTabs Then Exit For
            Dim host = qleaHost(tab)
            If (host Is Nothing AndAlso Not qleaPaused.ContainsKey(tab)) OrElse qleaPrivate(tab) Then Continue For
            Dim target As String = qleaSessionTarget(qleaUrl(tab))
            If target = "" Then Continue For
            If target.Length > qleaSessionUrl Then Continue For
            Dim size = Encoding.UTF8.GetByteCount(target & Environment.NewLine)
            If bytes + size > qleaSessionBytes Then Continue For
            urls.Add(target)
            bytes += size
        Next
        If urls.Count = 0 Then
            If File.Exists(sessionFile) Then File.Delete(sessionFile)
            Return
        End If
        Dim folder = System.IO.Path.GetDirectoryName(sessionFile)
        If Not String.IsNullOrWhiteSpace(folder) Then Directory.CreateDirectory(folder)
        File.WriteAllLines(sessionFile, urls.ToArray(), New System.Text.UTF8Encoding(False))
    End Sub

    Private Sub qleaClearSession()
        Dim path = qleaSessionPath()
        If File.Exists(path) Then File.Delete(path)
        My.Settings.ls.Clear()
        My.Settings.Save()
    End Sub

    Private Function qleaSessionTarget(ByVal value As String) As String
        Dim target = If(value, "").Trim()
        If target.Length = 0 OrElse target.Length > qleaSessionUrl Then Return ""
        Dim address As Uri = Nothing
        If Not Uri.TryCreate(target, UriKind.Absolute, address) Then Return ""
        If address.Scheme <> Uri.UriSchemeHttp AndAlso address.Scheme <> Uri.UriSchemeHttps AndAlso address.Scheme <> Uri.UriSchemeFile Then Return ""
        If address.AbsoluteUri.Length > qleaSessionUrl Then Return ""
        Return address.AbsoluteUri
    End Function

    Private Function qleaRestoreUrls(ByVal values As IEnumerable(Of String), Optional ByVal allowPrivate As Boolean = False) As Integer
        Return qleaRestoreBatch(values, allowPrivate, "off", False)
    End Function

    Private Function qleaRestoreSaved(ByVal values As IEnumerable(Of String), Optional ByVal allowPrivate As Boolean = False, Optional ByVal replaceExisting As Boolean = False) As Integer
        Return qleaRestoreBatch(values, allowPrivate, qfeature6lazymode(), replaceExisting)
    End Function

    Private Function qleaRestoreBatch(ByVal values As IEnumerable(Of String), ByVal allowPrivate As Boolean, ByVal mode As String, ByVal replaceExisting As Boolean) As Integer
        If (pv AndAlso Not allowPrivate) OrElse values Is Nothing Then Return 0
        Dim targets As List(Of String) = qleaTargets(values)
        If targets.Count = 0 Then Return 0
        Dim previous As List(Of TabPage) = If(replaceExisting, qalltabs(), New List(Of TabPage)())
        Dim batch As Boolean = qleaGroupChange
        qleaGroupChange = True
        Try
            Dim first As TabPage = Nothing
            For Each target As String In targets
                Dim tab As TabPage
                If mode = "all" OrElse (mode = "background" AndAlso first IsNot Nothing) Then
                    tab = New TabPage("Lea") With {.Tag = "lea"}
                    qleaPlaceholder(tab, target, pv)
                    feature6LazyTabs.Add(tab)
                    k.TabPages.Add(tab)
                Else
                    Dim host As Global.Lea.Win.V = qleaNew(target)
                    tab = DirectCast(host.Parent, TabPage)
                End If
                If first Is Nothing Then first = tab
            Next
            k.SelectedTab = first
            For Each tab As TabPage In previous
                qleaDrop(tab)
            Next
            I = k.TabCount
            qfeature6prunestate()
            qleaTabChanged(k, EventArgs.Empty)
        Finally
            qleaGroupChange = batch
        End Try
        Return targets.Count
    End Function

    Private Function qleaTargets(ByVal values As IEnumerable(Of String)) As List(Of String)
        Dim targets As New List(Of String)()
        If values Is Nothing Then Return targets
        Dim visited As Integer = 0
        Dim bytes As Integer = 0
        For Each value In values
            If targets.Count >= qleaSessionTabs OrElse visited >= 256 Then Exit For
            visited += 1
            Dim target = qleaSessionTarget(value)
            If target.Length = 0 Then Continue For
            Dim size = Encoding.UTF8.GetByteCount(target & Environment.NewLine)
            If bytes + size > qleaSessionBytes Then Continue For
            targets.Add(target)
            bytes += size
        Next
        Return targets
    End Function

    Private Function qleaWindow(ByVal values As IEnumerable(Of String), Optional ByVal privateWindow As Boolean = False) As j
        Dim targets As List(Of String) = qleaTargets(values)
        If targets.Count = 0 Then Return Nothing
        Dim window As New j
        window.qleaInitial = targets
        window.pv = pv OrElse privateWindow
        Try
            window.Show()
            Return window
        Catch
            window.Dispose()
            Throw
        End Try
    End Function

    Private Function qleaSavedUrls() As List(Of String)
        If pv Then Return New List(Of String)()
        Return qleaTargets(qalltabs().Where(Function(tab) Not qleaPrivate(tab)).Select(Function(tab) qleaUrl(tab)))
    End Function

    Private Function qleaReadSession() As List(Of String)
        Dim urls As New List(Of String)()
        Try
            Dim path = qleaSessionPath()
            If Not File.Exists(path) Then Return urls
            Using stream As New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
                If stream.Length > qleaSessionBytes Then
                    qleaSessionError("Saved session is too large; skipped")
                    Return Nothing
                End If
                Using reader As New StreamReader(stream, Encoding.UTF8, True)
                    While Not reader.EndOfStream AndAlso urls.Count < 256
                        urls.Add(reader.ReadLine())
                    End While
                End Using
            End Using
        Catch ex As IOException
            qleaSessionError("Saved session could not be read; skipped")
            Return Nothing
        Catch ex As UnauthorizedAccessException
            qleaSessionError("Saved session is not accessible; skipped")
            Return Nothing
        Catch ex As System.Security.SecurityException
            qleaSessionError("Saved session is not accessible; skipped")
            Return Nothing
        End Try
        Return urls
    End Function

    Private Sub qleaPreviousSession()
        Dim urls = qleaReadSession()
        If urls Is Nothing Then Return
        Dim restored = qleaRestoreSaved(urls, True)
        If restored = 0 AndAlso My.Settings.ls IsNot Nothing Then restored = qleaRestoreSaved(My.Settings.ls.Cast(Of String)(), True)
        su.Visible = True
        su.Text = If(restored = 0, "No supported saved session", "Restored " & restored.ToString() & " Lea tab(s)")
    End Sub

    Private Function qleaRestoreSession() As Integer
        If pv Then Return 0
        Dim restored = qleaRestoreSaved(qleaReadSession())
        If restored > 0 Then
            su.Visible = True
            su.Text = "Restored " & restored.ToString() & " Lea tab(s)"
        End If
        Return restored
    End Function

    Private Sub qleaSessionError(ByVal message As String)
        su.Visible = True
        su.Text = message
    End Sub

    Private Sub qleaClosing(ByVal sender As Object, ByVal e As FormClosingEventArgs)
        Try
            If qleaInspectorWindow IsNot Nothing Then qleaInspectorWindow.Close()
            Try
                qleaSaveSession()
            Catch ex As IOException
                qlog(ex)
            Catch ex As UnauthorizedAccessException
                qlog(ex)
            Catch ex As System.Security.SecurityException
                qlog(ex)
            End Try
            qfeatureclosing()
            If pv = False Then
                My.Settings.ls.Clear()
                My.Settings.lcw.Clear()
                For Each url As String In qleaSavedUrls()
                    My.Settings.lcw.Add(url)
                Next
                For Each tab As TabPage In k.TabPages
                    If tab.Controls.Count = 0 Then Continue For
                    Dim browser = TryCast(tab.Controls(0), WebBrowser)
                    If browser Is Nothing Then Continue For
                    Dim savedUrl As String = qpersistedtaburl(tab, browser)
                    If savedUrl <> "" Then
                        My.Settings.ls.Add(savedUrl)
                    End If
                Next
            End If
            If My.Settings.rws = "e" And FormBorderStyle <> FormBorderStyle.None Then
                Dim savedBounds As Rectangle = If(WindowState = FormWindowState.Normal, Bounds, RestoreBounds)
                My.Settings.wl = savedBounds.Left.ToString()
                My.Settings.wt = savedBounds.Top.ToString()
                My.Settings.ww = savedBounds.Width.ToString()
                My.Settings.wh = savedBounds.Height.ToString()
                My.Settings.ws = If(WindowState = FormWindowState.Maximized, "Maximized", "Normal")
            End If
            qclearonexit()
            My.Settings.Save()
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    Private Sub qleaClose(ByVal tab As TabPage)
        If tab Is Nothing OrElse tab.IsDisposed OrElse Not qalltabs().Contains(tab) Then Return
        Dim legacy = tab.Controls.Count > 0 AndAlso TypeOf tab.Controls(0) Is WebBrowser
        If Not pv AndAlso Not qleaPrivate(tab) Then rca(qleaUrl(tab))
        If k.TabPages.Contains(tab) AndAlso k.TabCount = 1 Then
            Close()
            Return
        End If
        qleaDrop(tab)
        qfeature6prunestate()
        I = k.TabCount
        If legacy Then
            Dim size = k.ItemSize
            size.Width += 6
            k.ItemSize = size
        End If
        qleaTabChanged(k, EventArgs.Empty)
    End Sub

    Private Sub qleaDrop(ByVal tab As TabPage)
        Dim host As Global.Lea.Win.V = qleaHost(tab)
        If host IsNot Nothing Then qleaRelease(host)
        k.TabPages.Remove(tab)
        tab.Dispose()
        suspendedGroupUrls.Remove(tab)
        qleaPaused.Remove(tab)
        For Each hidden As List(Of TabPage) In hiddenGroupTabs.Values
            hidden.Remove(tab)
        Next
    End Sub

    Private Sub qleaBackClick(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaDo("bk") Then a1(s, e)
    End Sub
    Private Sub qleaBackHover(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaHover("bk") Then a2(s, e)
    End Sub
    Private Sub qleaForwardClick(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaDo("fw") Then a3(s, e)
    End Sub
    Private Sub qleaForwardHover(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaHover("fw") Then a4(s, e)
    End Sub
    Private Sub qleaHomeClick(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaDo("home") Then a5(s, e)
    End Sub
    Private Sub qleaHomeHover(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaHover("home") Then a6(s, e)
    End Sub
    Private Sub qleaGoClick(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaDo("go", c.Text) Then a7(s, e)
    End Sub
    Private Sub qleaGoHover(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaHover("go", c.Text) Then a8(s, e)
    End Sub
    Private Sub qleaSearchClick(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaDo("search", x.Text) Then b3(s, e)
    End Sub
    Private Sub qleaSearchHover(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaHover("search", x.Text) Then b4(s, e)
    End Sub
    Private Sub qleaReloadClick(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaDo("reload") Then a9(s, e)
    End Sub
    Private Sub qleaReloadHover(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaHover("reload") Then b1(s, e)
    End Sub
    Private Sub qleaBackMenuClick(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaDo("bk") Then b5(s, e)
    End Sub
    Private Sub qleaBackMenuHover(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaHover("bk") Then b6(s, e)
    End Sub
    Private Sub qleaForwardMenuClick(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaDo("fw") Then b7(s, e)
    End Sub
    Private Sub qleaForwardMenuHover(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaHover("fw") Then b8(s, e)
    End Sub
    Private Sub qleaHomeMenuClick(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaDo("home") Then h6(s, e)
    End Sub
    Private Sub qleaHomeMenuHover(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaHover("home") Then h7(s, e)
    End Sub
    Private Sub qleaReloadMenuClick(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaDo("reload") Then i3(s, e)
    End Sub
    Private Sub qleaReloadMenuHover(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaHover("reload") Then i4(s, e)
    End Sub
    Private Sub qleaStopClick(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaDo("stop") Then i5(s, e)
    End Sub
    Private Sub qleaStopHover(ByVal s As Object, ByVal e As EventArgs)
        If Not qleaHover("stop") Then i6(s, e)
    End Sub
End Class
