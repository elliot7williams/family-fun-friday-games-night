Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Windows.Forms

Partial Class j
    Private Sub qleaSplit()
        Dim choices As New List(Of SplitTabChoice)()
        Dim bytes As Integer = 0
        For Each tab As TabPage In k.TabPages.Cast(Of TabPage)().Take(256)
            Dim target As String = qleaSessionTarget(qleaUrl(tab))
            If target = "" Then Continue For
            Dim size As Integer = Encoding.UTF8.GetByteCount(target)
            If bytes + size > qleaSessionBytes Then Continue For
            choices.Add(New SplitTabChoice(qleaDomainLine(tab.Text, 256), target, pv OrElse qleaPrivate(tab)))
            bytes += size
        Next
        su.Visible = True
        If choices.Count = 0 Then
            su.Text = "No supported tabs are available for split view"
            Return
        End If
        Dim window As New SplitBrowserForm(Me, choices)
        Try
            window.Show(Me)
            su.Text = "Lea split view opened"
        Catch
            window.Dispose()
            Throw
        End Try
    End Sub

    Friend Function qleaSplitPrivate() As Boolean
        Return pv
    End Function

    Friend Function qleaSplitAllowed(ByVal address As Uri) As Boolean
        If IsDisposed OrElse Disposing OrElse address Is Nothing Then Return False
        If qleaSessionTarget(address.AbsoluteUri) = "" Then Return False
        Return Not qnavigationblocked(New WebBrowserNavigatingEventArgs(address, ""))
    End Function
End Class

Friend Class SplitTabChoice
    Public ReadOnly Label As String
    Public ReadOnly Url As String
    Public ReadOnly Pv As Boolean
    Sub New(ByVal tabLabel As String, ByVal tabUrl As String, ByVal privateView As Boolean)
        Label = tabLabel
        Url = tabUrl
        Pv = privateView
    End Sub
    Public Overrides Function ToString() As String
        Return Label & " - " & Url
    End Function
End Class

Friend Class SplitBrowserForm
    Inherits Form
    Private ownerForm As j
    Private leftChoice As New ComboBox()
    Private rightChoice As New ComboBox()
    Private syncScroll As New CheckBox()
    Private differenceButton As New Button()
    Private leftBrowser As Global.Lea.Win.V
    Private rightBrowser As Global.Lea.Win.V
    Private split As New SplitContainer()
    Private status As New Label()
    Private syncTimer As New Timer()
    Private differenceWindow As Form
    Private lastLeftY As Integer
    Private lastRightY As Integer
    Private started As Boolean
    Private closed As Boolean

    Sub New(ByVal browser As j, ByVal choices As List(Of SplitTabChoice))
        ownerForm = browser
        Text = "Side-By-Side Tab View"
        Icon = browser.Icon
        StartPosition = FormStartPosition.CenterParent
        Size = New Size(1100, 700)
        MinimumSize = New Size(760, 420)
        Dim top As New TableLayoutPanel With {.Dock = DockStyle.Top, .Height = 38, .ColumnCount = 4}
        top.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
        top.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
        top.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 100))
        top.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150))
        For Each box As ComboBox In New ComboBox() {leftChoice, rightChoice}
            box.Dock = DockStyle.Fill
            box.DropDownStyle = ComboBoxStyle.DropDownList
            box.DropDownWidth = 580
            For Each choice As SplitTabChoice In choices
                box.Items.Add(choice)
            Next
        Next
        leftChoice.AccessibleName = "Left source tab"
        rightChoice.AccessibleName = "Right source tab"
        syncScroll.Text = "Sync scroll"
        syncScroll.Dock = DockStyle.Fill
        syncScroll.Checked = My.Settings.splitSync = "e"
        differenceButton.Text = "Text Differences"
        differenceButton.Dock = DockStyle.Fill
        differenceButton.Enabled = False
        top.Controls.Add(leftChoice, 0, 0)
        top.Controls.Add(rightChoice, 1, 0)
        top.Controls.Add(syncScroll, 2, 0)
        top.Controls.Add(differenceButton, 3, 0)
        split.Dock = DockStyle.Fill
        split.Size = New Size(1080, 620)
        split.SplitterDistance = 540
        split.Panel1MinSize = 180
        split.Panel2MinSize = 180
        status.Dock = DockStyle.Bottom
        status.Height = 26
        status.AutoEllipsis = True
        status.Padding = New Padding(6, 4, 6, 0)
        Controls.Add(split)
        Controls.Add(top)
        Controls.Add(status)
        AddHandler leftChoice.SelectedIndexChanged, AddressOf qleft
        AddHandler rightChoice.SelectedIndexChanged, AddressOf qright
        AddHandler syncScroll.CheckedChanged, AddressOf qsyncchanged
        AddHandler differenceButton.Click, AddressOf qdifference
        syncTimer.Interval = 350
        AddHandler syncTimer.Tick, AddressOf qsynctick
        leftChoice.SelectedIndex = 0
        rightChoice.SelectedIndex = If(choices.Count > 1, 1, 0)
    End Sub

    Protected Overrides Sub OnShown(ByVal e As EventArgs)
        MyBase.OnShown(e)
        If started OrElse closed Then Return
        started = True
        qleft(Me, EventArgs.Empty)
        qright(Me, EventArgs.Empty)
        syncTimer.Start()
    End Sub

    Private Sub qleft(ByVal sender As Object, ByVal e As EventArgs)
        If started AndAlso Not closed Then qshow(True, TryCast(leftChoice.SelectedItem, SplitTabChoice))
    End Sub

    Private Sub qright(ByVal sender As Object, ByVal e As EventArgs)
        If started AndAlso Not closed Then qshow(False, TryCast(rightChoice.SelectedItem, SplitTabChoice))
    End Sub

    Private Async Sub qshow(ByVal left As Boolean, ByVal choice As SplitTabChoice)
        If choice Is Nothing OrElse closed Then Return
        qrelease(If(left, leftBrowser, rightBrowser))
        Dim view As New Global.Lea.Win.V With {.Dock = DockStyle.Fill, .Pv = choice.Pv OrElse ownerForm.qleaSplitPrivate()}
        view.AccessibleName = If(left, "Left Lea page", "Right Lea page")
        view.Ng = Function(address) qallow(view, address)
        AddHandler view.Done, AddressOf qdone
        AddHandler view.Changed, AddressOf qstate
        AddHandler view.Al, AddressOf qalert
        AddHandler view.Gr, AddressOf qgo
        AddHandler view.Dr, AddressOf qdownload
        If left Then
            leftBrowser = view
            split.Panel1.Controls.Add(view)
        Else
            rightBrowser = view
            split.Panel2.Controls.Add(view)
        End If
        differenceButton.Enabled = False
        Await qload(view, choice.Url)
    End Sub

    Private Function qallow(ByVal view As Global.Lea.Win.V, ByVal address As Uri) As Boolean
        If closed OrElse view.IsDisposed OrElse ownerForm Is Nothing OrElse ownerForm.IsDisposed Then Return False
        If ownerForm.qleaSplitPrivate() Then view.Pv = True
        Return ownerForm.qleaSplitAllowed(address)
    End Function

    Private Async Function qload(ByVal view As Global.Lea.Win.V, ByVal url As String) As Threading.Tasks.Task
        If closed OrElse view Is Nothing OrElse view.IsDisposed Then Return
        status.Text = "Lea loading"
        Try
            Await view.GoA(url)
        Catch ex As Exception
            If Not closed AndAlso Not view.IsDisposed Then status.Text = "Lea: " & qline(ex.Message, 256)
        Finally
            If Not closed AndAlso Not view.IsDisposed Then qstate(view, EventArgs.Empty)
        End Try
    End Function

    Private Sub qstate(ByVal sender As Object, ByVal e As EventArgs)
        If closed Then Return
        differenceButton.Enabled = leftBrowser IsNot Nothing AndAlso rightBrowser IsNot Nothing AndAlso
            Not leftBrowser.Busy AndAlso Not rightBrowser.Busy AndAlso leftBrowser.At IsNot Nothing AndAlso rightBrowser.At IsNot Nothing
        Text = "Side-By-Side Tab View" & If((leftBrowser IsNot Nothing AndAlso leftBrowser.Pv) OrElse (rightBrowser IsNot Nothing AndAlso rightBrowser.Pv), " - Private", "")
    End Sub

    Private Sub qdone(ByVal sender As Object, ByVal e As EventArgs)
        If closed Then Return
        lastLeftY = If(leftBrowser Is Nothing, 0, leftBrowser.Sy)
        lastRightY = If(rightBrowser Is Nothing, 0, rightBrowser.Sy)
        status.Text = "Lea ready"
        qstate(sender, e)
    End Sub

    Private Sub qalert(ByVal sender As Object, ByVal e As EventArgs)
        Dim view As Global.Lea.Win.V = TryCast(sender, Global.Lea.Win.V)
        If Not closed AndAlso view IsNot Nothing Then status.Text = "Lea: " & qline(view.Am, 256)
    End Sub

    Private Async Sub qgo(ByVal sender As Object, ByVal e As EventArgs)
        Dim view As Global.Lea.Win.V = TryCast(sender, Global.Lea.Win.V)
        If view IsNot Nothing AndAlso view.Gu IsNot Nothing Then Await qload(view, view.Gu.AbsoluteUri)
    End Sub

    Private Async Sub qdownload(ByVal sender As Object, ByVal e As EventArgs)
        Dim view As Global.Lea.Win.V = TryCast(sender, Global.Lea.Win.V)
        If view Is Nothing OrElse view.Du Is Nothing OrElse Not qallow(view, view.Du) Then Return
        Using dialog As New SaveFileDialog()
            dialog.Title = "Save Lea Download"
            dialog.Filter = "All files (*.*)|*.*"
            Dim name As String = qline(view.Dn, 160)
            For Each invalid As Char In Path.GetInvalidFileNameChars()
                name = name.Replace(invalid, "_"c)
            Next
            dialog.FileName = If(name = "", "Lea download", name)
            If dialog.ShowDialog(Me) <> DialogResult.OK OrElse closed OrElse view.IsDisposed Then Return
            Try
                Await view.DlA(view.Du.AbsoluteUri, dialog.FileName)
                If Not closed Then status.Text = "Lea download saved"
            Catch ex As Exception
                If Not closed AndAlso Not view.IsDisposed Then status.Text = "Lea: " & qline(ex.Message, 256)
            End Try
        End Using
    End Sub

    Private Sub qsyncchanged(ByVal sender As Object, ByVal e As EventArgs)
        My.Settings.splitSync = If(syncScroll.Checked, "e", "")
        My.Settings.Save()
        lastLeftY = If(leftBrowser Is Nothing, 0, leftBrowser.Sy)
        lastRightY = If(rightBrowser Is Nothing, 0, rightBrowser.Sy)
    End Sub

    Private Sub qsynctick(ByVal sender As Object, ByVal e As EventArgs)
        If closed OrElse Not syncScroll.Checked OrElse leftBrowser Is Nothing OrElse rightBrowser Is Nothing Then Return
        If leftBrowser.Busy OrElse rightBrowser.Busy OrElse leftBrowser.At Is Nothing OrElse rightBrowser.At Is Nothing Then Return
        If leftBrowser.Sy <> lastLeftY Then
            rightBrowser.Sc(leftBrowser.Sy - rightBrowser.Sy)
        ElseIf rightBrowser.Sy <> lastRightY Then
            leftBrowser.Sc(rightBrowser.Sy - leftBrowser.Sy)
        End If
        lastLeftY = leftBrowser.Sy
        lastRightY = rightBrowser.Sy
    End Sub

    Private Shared Function qline(ByVal text As String, ByVal limit As Integer) As String
        text = If(text, "")
        Dim size As Integer = Math.Min(text.Length, limit)
        If size > 0 AndAlso Char.IsHighSurrogate(text(size - 1)) Then size -= 1
        Return Regex.Replace(text.Substring(0, size), "[\s\p{Cc}]+", " ").Trim()
    End Function

    Private Shared Function qtext(ByVal view As Global.Lea.Win.V, ByRef limited As Boolean) As List(Of String)
        Dim lines As New List(Of String)()
        Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Dim pending As New Queue(Of Global.Lea.Engine.B)(view.At.Bs)
        Dim bytes As Integer = 0
        Dim visits As Integer = 0
        While pending.Count > 0 AndAlso visits < 4096
            Dim block As Global.Lea.Engine.B = pending.Dequeue()
            visits += 1
            Dim visible As Boolean = block.S Is Nothing OrElse (block.S.Vi <> "h" AndAlso block.S.Vi <> "c")
            Dim current As Global.Lea.Engine.B = block
            While current IsNot Nothing
                If current.Hd OrElse (current.S IsNot Nothing AndAlso current.S.Op = 0) Then visible = False
                current = current.Up
            End While
            If Not visible OrElse block.I IsNot Nothing Then Continue While
            If block.Tb IsNot Nothing Then
                For Each row As Global.Lea.Engine.Tr In block.Tb.Rs
                    For Each cell As Global.Lea.Engine.Tc In row.Cs
                        For Each part As Global.Lea.Engine.B In cell.Bs
                            If pending.Count + visits < 4096 Then pending.Enqueue(part) Else limited = True
                        Next
                    Next
                Next
            End If
            Dim raw As String = If(block.X, "")
            If raw.Length > 4096 Then limited = True
            Dim line As String = qline(raw, 4096)
            If line = "" OrElse Not seen.Add(line) Then Continue While
            Dim size As Integer = Encoding.UTF8.GetByteCount(line & Environment.NewLine)
            If lines.Count >= 2048 OrElse bytes + size > 65536 - 256 Then
                limited = True
                Exit While
            End If
            bytes += size
            lines.Add(line)
        End While
        If pending.Count > 0 Then limited = True
        Return lines
    End Function

    Private Sub qdifference(ByVal sender As Object, ByVal e As EventArgs)
        qstate(sender, e)
        If Not differenceButton.Enabled Then Return
        Dim limitedLeft As Boolean = False
        Dim limitedRight As Boolean = False
        Dim left As List(Of String) = qtext(leftBrowser, limitedLeft)
        Dim right As List(Of String) = qtext(rightBrowser, limitedRight)
        Dim leftSet As New HashSet(Of String)(left, StringComparer.OrdinalIgnoreCase)
        Dim rightSet As New HashSet(Of String)(right, StringComparer.OrdinalIgnoreCase)
        Dim leftOnly As List(Of String) = left.Where(Function(line) Not rightSet.Contains(line)).ToList()
        Dim rightOnly As List(Of String) = right.Where(Function(line) Not leftSet.Contains(line)).ToList()
        If differenceWindow IsNot Nothing Then differenceWindow.Dispose()
        differenceWindow = New Form With {.Text = "Text Differences", .Size = New Size(900, 600), .MinimumSize = New Size(600, 360), .StartPosition = FormStartPosition.CenterParent, .Icon = Icon}
        Dim panes As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 2, .RowCount = 2, .Padding = New Padding(8)}
        panes.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
        panes.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
        panes.RowStyles.Add(New RowStyle(SizeType.Absolute, 30))
        panes.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        For column As Integer = 0 To 1
            Dim values As List(Of String) = If(column = 0, leftOnly, rightOnly)
            Dim clipped As Boolean = limitedLeft OrElse limitedRight OrElse values.Count > 150
            Dim label As String = If(column = 0, "Left", "Right") & " only (" & Math.Min(150, values.Count).ToString() & ")" & If(clipped, " - limited", "")
            Dim heading As New Label With {.Text = label, .Dock = DockStyle.Fill, .AutoEllipsis = True, .Padding = New Padding(0, 5, 0, 0)}
            Dim text As New TextBox With {.Name = If(column = 0, "LeftOnly", "RightOnly"), .AccessibleName = label, .Multiline = True, .ReadOnly = True, .WordWrap = True, .ScrollBars = ScrollBars.Vertical, .Dock = DockStyle.Fill}
            text.Text = String.Join(Environment.NewLine, values.Take(150).ToArray())
            panes.Controls.Add(heading, column, 0)
            panes.Controls.Add(text, column, 1)
        Next
        differenceWindow.Controls.Add(panes)
        differenceWindow.Show(Me)
    End Sub

    Private Sub qrelease(ByVal view As Global.Lea.Win.V)
        If view Is Nothing Then Return
        RemoveHandler view.Done, AddressOf qdone
        RemoveHandler view.Changed, AddressOf qstate
        RemoveHandler view.Al, AddressOf qalert
        RemoveHandler view.Gr, AddressOf qgo
        RemoveHandler view.Dr, AddressOf qdownload
        view.Ng = Nothing
        If view.Parent IsNot Nothing Then view.Parent.Controls.Remove(view)
        view.Dispose()
    End Sub

    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso Not closed Then
            closed = True
            syncTimer.Stop()
            RemoveHandler syncTimer.Tick, AddressOf qsynctick
            syncTimer.Dispose()
            If differenceWindow IsNot Nothing Then differenceWindow.Dispose()
            qrelease(leftBrowser)
            qrelease(rightBrowser)
            ownerForm = Nothing
        End If
        MyBase.Dispose(disposing)
    End Sub
End Class
