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

    Private Sub qleaInit(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If qleaMenu IsNot Nothing Then Return
        N.Text = "New Lea Tab"
        N.ToolTipText = "Open a new Lea tab"
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
        If Not qleaSessionRestored Then
            qleaSessionRestored = True
            qleaRestoreSession()
        End If
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
        AddHandler k.MouseDown, AddressOf qleaTabMouseDown
        AddHandler Me.FormClosing, AddressOf qleaClosing
    End Sub

    Private Function qleaAt() As Global.Lea.Win.V
        If k.SelectedTab Is Nothing OrElse k.SelectedTab.Controls.Count = 0 Then Return Nothing
        Dim host = TryCast(k.SelectedTab.Controls(0), Global.Lea.Win.V)
        If host IsNot Nothing Then host.Pv = pv
        Return host
    End Function

    Private Function qleaHost(ByVal tab As TabPage) As Global.Lea.Win.V
        If tab Is Nothing OrElse tab.Controls.Count = 0 Then Return Nothing
        Return TryCast(tab.Controls(0), Global.Lea.Win.V)
    End Function

    Private Function qleaDo(ByVal command As String, Optional ByVal value As String = "") As Boolean
        Dim host = qleaAt()
        If host Is Nothing Then Return False
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
                qleaLoad(host, qleaStart())
            Case "reload"
                If host.Busy Then host.St() Else qleaReload(host)
            Case "stop"
                host.St()
        End Select
        qleaState(host)
        Return True
    End Function

    Private Function qleaHover(ByVal command As String, Optional ByVal value As String = "") As Boolean
        If qleaAt() Is Nothing Then Return False
        If My.Settings.t = "e" Then qleaDo(command, value)
        Return True
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
        Dim tab As New TabPage("Lea")
        Dim host As New Global.Lea.Win.V()
        host.Pv = pv
        host.Dock = DockStyle.Fill
        tab.Tag = "lea"
        tab.Controls.Add(host)
        k.TabPages.Add(tab)
        k.SelectedTab = tab
        I += 1
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
        If String.IsNullOrWhiteSpace(target) Then target = qleaStart()
        If Visible Then qleaLoad(host, target) Else host.Go(target)
        Return host
    End Function

    Private Function qleaStart() As String
        Dim localPage = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Lea", "start.html")
        If File.Exists(localPage) Then Return localPage
        If Not String.IsNullOrWhiteSpace(My.Settings.h) Then Return My.Settings.h
        Return "https://example.com"
    End Function

    Private Async Sub qleaOpen(ByVal host As Global.Lea.Win.V, ByVal input As String, ByVal searchOnly As Boolean)
        Dim value = If(input, "").Trim()
        Dim aliasAddress As String = ""
        If qresolvealias(value, aliasAddress) Then value = aliasAddress
        If value.Length = 0 Then Return
        If searchOnly OrElse value.Contains(" ") OrElse Not value.Contains(".") Then
            value = My.Settings.se & Uri.EscapeDataString(value)
        ElseIf Not value.Contains("://") AndAlso Not value.StartsWith("about:") AndAlso Not value.StartsWith("file:") Then
            value = "http://" & value
        End If
        Await qleaLoadA(host, value)
    End Sub

    Private Async Sub qleaLoad(ByVal host As Global.Lea.Win.V, ByVal target As String)
        Await qleaLoadA(host, target)
    End Sub

    Private Async Function qleaLoadA(ByVal host As Global.Lea.Win.V, ByVal target As String) As Threading.Tasks.Task
        Try
            su.Visible = True
            su.Text = "Lea loading"
            r.Image = My.Resources.F7
            Await host.GoA(target)
        Catch ex As Exception
            su.Visible = True
            su.Text = "Lea: " & ex.Message
            r.Image = My.Resources.F8
        End Try
    End Function

    Private Async Sub qleaReload(ByVal host As Global.Lea.Win.V)
        Try
            su.Visible = True
            su.Text = "Lea loading"
            r.Image = My.Resources.F7
            Await host.RlA()
        Catch ex As Exception
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
        If tab IsNot Nothing Then tab.Text = If(String.IsNullOrWhiteSpace(host.T), "Lea", host.T)
        If k.SelectedTab IsNot tab Then Return
        c.Text = If(host.U Is Nothing, "", host.U.ToString())
        Text = ProductName & " - " & If(tab Is Nothing, "Lea", tab.Text)
        b.Enabled = host.CanBk
        BT.Enabled = host.CanBk
        f.Enabled = host.CanFw
        FT.Enabled = host.CanFw
    End Sub

    Private Sub qleaTabChanged(ByVal sender As Object, ByVal e As EventArgs) Handles k.SelectedIndexChanged
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
        If host Is Nothing Then
            tk(sender, e)
        Else
            su.Text = If(host.Busy, "Lea loading", "Lea ready")
            qleaState(host)
        End If
    End Sub

    Private Sub qleaCloseMenuClick(ByVal sender As Object, ByVal e As EventArgs)
        If qleaAt() Is Nothing Then
            h5(sender, e)
            Return
        End If
        qleaClose(k.SelectedTab)
    End Sub

    Private Sub qleaTabMouseDown(ByVal sender As Object, ByVal e As MouseEventArgs)
        If qleaAt() Is Nothing Then Return
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
        For Each tab As TabPage In k.TabPages
            Dim host = qleaHost(tab)
            If host IsNot Nothing AndAlso host.U IsNot Nothing Then urls.Add(host.U.AbsoluteUri)
        Next
        If urls.Count = 0 Then
            If File.Exists(sessionFile) Then File.Delete(sessionFile)
            Return
        End If
        Dim folder = System.IO.Path.GetDirectoryName(sessionFile)
        If Not String.IsNullOrWhiteSpace(folder) Then Directory.CreateDirectory(folder)
        File.WriteAllLines(sessionFile, urls.ToArray(), New System.Text.UTF8Encoding(False))
    End Sub

    Private Sub qleaRestoreSession()
        If pv Then Return
        Dim path = qleaSessionPath()
        If Not File.Exists(path) Then Return
        Dim restored = 0
        For Each line In File.ReadAllLines(path)
            If restored >= 12 Then Exit For
            Dim target = If(line, "").Trim()
            Dim address As Uri = Nothing
            If Not Uri.TryCreate(target, UriKind.Absolute, address) Then Continue For
            If address.Scheme <> Uri.UriSchemeHttp AndAlso address.Scheme <> Uri.UriSchemeHttps AndAlso address.Scheme <> Uri.UriSchemeFile Then Continue For
            qleaNew(address.AbsoluteUri)
            restored += 1
        Next
        If restored > 0 Then
            su.Visible = True
            su.Text = "Restored " & restored.ToString() & " Lea tab(s)"
        End If
    End Sub

    Private Sub qleaClosing(ByVal sender As Object, ByVal e As FormClosingEventArgs)
        Try
            If qleaInspectorWindow IsNot Nothing Then qleaInspectorWindow.Close()
            qleaSaveSession()
            qfeatureclosing()
            If pv = False Then
                My.Settings.ls.Clear()
                My.Settings.lcw.Clear()
                For Each tab As TabPage In k.TabPages
                    If tab.Controls.Count = 0 Then Continue For
                    Dim browser = TryCast(tab.Controls(0), WebBrowser)
                    If browser Is Nothing Then Continue For
                    Dim savedUrl As String = qpersistedtaburl(tab, browser)
                    If savedUrl <> "" Then
                        My.Settings.ls.Add(savedUrl)
                        My.Settings.lcw.Add(savedUrl)
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
        If tab Is Nothing OrElse Not k.TabPages.Contains(tab) Then Return
        If k.TabCount = 1 Then
            Close()
            Return
        End If
        k.TabPages.Remove(tab)
        tab.Dispose()
        I = Math.Max(0, I - 1)
        qleaTabChanged(k, EventArgs.Empty)
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
