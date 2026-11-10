Imports System.Drawing

Partial Class j
    Sub qfeatures10()
        ot.DropDownItems.Add(qnewitem("Browser Themes And Site Colors", AddressOf q947, AddressOf q948))
        AddHandler k.SelectedIndexChanged, AddressOf qthemechanged
        qapplybrowserappearance()
    End Sub

    ' --- Browser Themes And Site Colors ---
    Sub q947(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim appearance As New BrowserAppearanceForm(Me)
            appearance.Show(Me)
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q948(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q947(sender, e)
    End Sub

    Friend Sub qsetbrowserappearance(ByVal mode As String, ByVal color As Color)
        Try
            mode = mode.ToLowerInvariant
            If mode = "custom" Then My.Settings.i = color
            If mode = "original" Then My.Settings.i = Color.Black
            My.Settings.chromeMode = mode
            My.Settings.Save()
            qapplybrowserappearance()
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    Friend Sub qusecurrentpagecolor()
        Try
            Dim browser As WebBrowser = qthemebrowser()
            Dim color As Color = qsitechromecolor(browser)
            If color.IsEmpty Then
                su.Visible = True
                su.Text = "This page does not provide a usable color"
                Exit Sub
            End If
            qsetbrowserappearance("custom", color)
            su.Visible = True
            su.Text = "Current page color applied to the browser toolbar"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    Friend Function qthememode() As String
        Dim mode As String = My.Settings.chromeMode.Trim.ToLowerInvariant
        If mode <> "original" AndAlso mode <> "custom" AndAlso mode <> "site" Then mode = "legacy"
        Return mode
    End Function

    Friend Sub qapplybrowserappearance()
        Try
            Dim mode As String = qthememode()
            If mode = "original" Then
                qapplyoriginalappearance()
            ElseIf mode = "custom" Then
                qapplychromecolor(My.Settings.i)
            ElseIf mode = "site" Then
                qapplysitechrome(qthemebrowser())
            ElseIf My.Settings.i <> Color.Black Then
                qapplychromecolor(My.Settings.i)
            End If
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    Sub qthemechanged(ByVal sender As Object, ByVal e As EventArgs)
        If qthememode() = "site" Then qapplysitechrome(qthemebrowser())
    End Sub

    Friend Sub qapplysitechrome(ByVal browser As WebBrowser)
        Try
            If qthememode() <> "site" Then Exit Sub
            Dim color As Color = qsitechromecolor(browser)
            If color.IsEmpty Then
                qapplychromecolor(qthemefallbackcolor())
            Else
                qapplychromecolor(color)
            End If
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    Friend Function qthemebrowser() As WebBrowser
        Try
            If k.SelectedTab Is Nothing OrElse k.SelectedTab.Controls.Count = 0 Then Return Nothing
            Return TryCast(k.SelectedTab.Controls(0), WebBrowser)
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Friend Function qleapagecolor(ByVal host As Global.Lea.Win.V) As Color
        If host Is Nothing OrElse host.IsDisposed OrElse host.Busy OrElse host.Le IsNot Nothing OrElse host.At Is Nothing OrElse host.At.D Is Nothing Then Return Color.Empty
        Dim page As Global.Lea.Engine.P = host.At
        Dim pending As New Stack(Of Global.Lea.Engine.N)
        Dim seen As New HashSet(Of Global.Lea.Engine.N)
        Dim body As Global.Lea.Engine.N = Nothing
        pending.Push(page.D)
        Dim queued As Integer = 1
        ' Read the parsed tree only. No script execution, resource load or recursive walk.
        While pending.Count > 0
            Dim node As Global.Lea.Engine.N = pending.Pop()
            If node Is Nothing OrElse Not seen.Add(node) Then Continue While
            If node.K = "meta" AndAlso node.Av("media").Trim = "" Then
                Dim name As String = node.Av("name")
                If name = "" Then name = node.Av("property")
                If String.Equals(name, "theme-color", StringComparison.OrdinalIgnoreCase) OrElse String.Equals(name, "msapplication-tilecolor", StringComparison.OrdinalIgnoreCase) Then
                    Dim color As Color = qparsechromecolor(node.Av("content"))
                    If Not color.IsEmpty Then Return color
                End If
            End If
            If body Is Nothing AndAlso node.K = "body" Then body = node
            If node.C.Count > 65536 - queued Then Return Color.Empty
            queued += node.C.Count
            For index As Integer = node.C.Count - 1 To 0 Step -1
                pending.Push(node.C(index))
            Next
        End While
        If page.S IsNot Nothing Then
            Dim color As Color = qparsechromecolor(page.S.Bg)
            If Not color.IsEmpty Then Return color
        End If
        If body IsNot Nothing Then Return qparsechromecolor(body.Av("bgcolor"))
        Return Color.Empty
    End Function

    Friend Function qsitechromecolor(ByVal browser As WebBrowser) As Color
        Try
            If browser Is Nothing AndAlso qleaAt() IsNot Nothing Then Return qleapagecolor(qleaAt())
            If browser Is Nothing OrElse browser.Document Is Nothing OrElse browser.Document.Body Is Nothing Then Return Color.Empty
            Dim script As String = "(function(){try{var a=document.getElementsByTagName('meta'),i,m,n;for(i=0;i<a.length;i++){m=a[i];n=(m.getAttribute('name')||m.getAttribute('property')||'').toLowerCase();if(n==='theme-color'||n==='msapplication-tilecolor'){if(m.content)return m.content;}}var b=document.body,s=window.getComputedStyle?window.getComputedStyle(b,null):b.currentStyle;return (s&&s.backgroundColor)?s.backgroundColor:(b.bgColor||'');}catch(e){return '';}})()"
            Dim value As Object = browser.Document.InvokeScript("eval", New Object() {script})
            Dim siteColor As Color = qparsechromecolor(If(value Is Nothing, "", value.ToString))
            If siteColor.IsEmpty Then siteColor = qparsechromecolor(browser.Document.Body.GetAttribute("bgColor"))
            Return siteColor
        Catch ex As Exception
            Return Color.Empty
        End Try
    End Function

    Friend Function qparsechromecolor(ByVal value As String) As Color
        Try
            value = value.Trim
            If value = "" OrElse value.Equals("transparent", StringComparison.OrdinalIgnoreCase) Then Return Color.Empty
            If value.StartsWith("rgb", StringComparison.OrdinalIgnoreCase) Then
                Dim openAt As Integer = value.IndexOf("("c)
                Dim closeAt As Integer = value.IndexOf(")"c)
                If openAt < 0 OrElse closeAt <= openAt Then Return Color.Empty
                Dim parts As String() = value.Substring(openAt + 1, closeAt - openAt - 1).Split(","c)
                If parts.Length < 3 Then Return Color.Empty
                Dim red As Integer = CInt(Val(parts(0)))
                Dim green As Integer = CInt(Val(parts(1)))
                Dim blue As Integer = CInt(Val(parts(2)))
                If red < 0 OrElse red > 255 OrElse green < 0 OrElse green > 255 OrElse blue < 0 OrElse blue > 255 Then Return Color.Empty
                Return Color.FromArgb(red, green, blue)
            End If
            Return ColorTranslator.FromHtml(value)
        Catch ex As Exception
            Return Color.Empty
        End Try
    End Function

    Friend Function qthemefallbackcolor() As Color
        If My.Settings.i <> Color.Black Then Return My.Settings.i
        Return Color.FromArgb(0, 204, 255)
    End Function

    Friend Sub qapplychromecolor(ByVal color As Color)
        Try
            If color.IsEmpty Then Exit Sub
            Dim foreground As Color = qthemeforeground(color)
            q.BackgroundImage = Nothing
            q.BackColor = color
            q.ForeColor = foreground
            m.BackColor = color
            m.ForeColor = foreground
            For Each root As ToolStripMenuItem In New ToolStripMenuItem() {fl, Me.e, v, hi, bo, ot, he}
                root.BackColor = color
                root.ForeColor = foreground
            Next root
            c.BackColor = Color.White
            c.ForeColor = Color.Black
            x.BackColor = Color.White
            x.ForeColor = Color.Black
            su.BackColor = color
            su.ForeColor = foreground
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    Friend Sub qapplyoriginalappearance()
        Try
            q.BackgroundImage = Nothing
            q.BackColor = Color.FromArgb(0, 204, 255)
            q.ForeColor = Color.White
            m.BackColor = Color.Transparent
            m.ForeColor = Color.White
            For Each root As ToolStripMenuItem In New ToolStripMenuItem() {fl, Me.e, v, hi, bo, ot, he}
                root.BackColor = Color.Black
                root.ForeColor = Color.White
            Next root
            c.BackColor = Color.White
            c.ForeColor = Color.Black
            x.BackColor = Color.White
            x.ForeColor = Color.Black
            su.BackColor = Color.Black
            su.ForeColor = Color.White
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    Friend Function qthemeforeground(ByVal color As Color) As Color
        Dim brightness As Double = (color.R * 0.299R + color.G * 0.587R + color.B * 0.114R) / 255.0R
        If brightness >= 0.58R Then Return Color.Black
        Return Color.White
    End Function
End Class

Friend Class BrowserAppearanceForm
    Inherits Form

    Dim ownerBrowser As j
    Dim themeBox As New ComboBox
    Dim preview As New Panel
    Dim previewText As New Label
    Dim chooseColor As New Button
    Dim useCurrentPage As New Button
    Dim applyTheme As New Button
    Dim closeForm As New Button
    Dim selectedColor As Color

    Sub New(ByVal browser As j)
        ownerBrowser = browser
        Text = "Browser Themes And Site Colors"
        StartPosition = FormStartPosition.CenterParent
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        ShowInTaskbar = False
        ClientSize = New Size(520, 290)

        Dim description As New Label With {.Left = 15, .Top = 14, .Width = 490, .Height = 42, .Text = "Choose a browser color or let Firespider match the current website. Site matching uses the page's declared theme color first, then its visible page background."}
        Dim label As New Label With {.Left = 15, .Top = 69, .Width = 130, .Text = "Browser appearance:"}
        themeBox.Left = 150
        themeBox.Top = 65
        themeBox.Width = 340
        themeBox.DropDownStyle = ComboBoxStyle.DropDownList
        themeBox.Items.Add("Firespider Original")
        themeBox.Items.Add("Ocean Blue")
        themeBox.Items.Add("Ember Orange")
        themeBox.Items.Add("Forest Green")
        themeBox.Items.Add("Violet")
        themeBox.Items.Add("Custom Color")
        themeBox.Items.Add("Match Website Colors Automatically")

        preview.Left = 15
        preview.Top = 105
        preview.Width = 475
        preview.Height = 58
        preview.BorderStyle = BorderStyle.FixedSingle
        previewText.Dock = DockStyle.Fill
        previewText.TextAlign = ContentAlignment.MiddleCenter
        preview.Controls.Add(previewText)

        chooseColor.Left = 15
        chooseColor.Top = 180
        chooseColor.Width = 145
        chooseColor.Text = "Choose Custom Color"
        useCurrentPage.Left = 170
        useCurrentPage.Top = 180
        useCurrentPage.Width = 155
        useCurrentPage.Text = "Use Current Page Color"
        applyTheme.Left = 335
        applyTheme.Top = 180
        applyTheme.Width = 155
        applyTheme.Text = "Apply"
        closeForm.Left = 390
        closeForm.Top = 230
        closeForm.Width = 100
        closeForm.Text = "Close"

        Controls.Add(description)
        Controls.Add(label)
        Controls.Add(themeBox)
        Controls.Add(preview)
        Controls.Add(chooseColor)
        Controls.Add(useCurrentPage)
        Controls.Add(applyTheme)
        Controls.Add(closeForm)
        AcceptButton = applyTheme
        CancelButton = closeForm
        AddHandler themeBox.SelectedIndexChanged, AddressOf qappearancechanged
        AddHandler chooseColor.Click, AddressOf qchoosecolor
        AddHandler useCurrentPage.Click, AddressOf qusecurrentcolor
        AddHandler applyTheme.Click, AddressOf qapplytheme
        AddHandler closeForm.Click, AddressOf qcloseappearance
        selectedColor = ownerBrowser.qthemefallbackcolor()
        qsetselectedtheme()
    End Sub

    Sub qsetselectedtheme()
        Dim mode As String = ownerBrowser.qthememode()
        If mode = "original" Then
            themeBox.SelectedItem = "Firespider Original"
        ElseIf mode = "site" Then
            themeBox.SelectedItem = "Match Website Colors Automatically"
        ElseIf mode = "custom" Then
            themeBox.SelectedItem = "Custom Color"
            selectedColor = My.Settings.i
        Else
            themeBox.SelectedItem = "Firespider Original"
        End If
        If themeBox.SelectedIndex < 0 Then themeBox.SelectedIndex = 0
        qupdatepreview()
    End Sub

    Sub qappearancechanged(ByVal sender As Object, ByVal e As EventArgs)
        Select Case themeBox.Text
            Case "Ocean Blue"
                selectedColor = Color.FromArgb(0, 120, 212)
            Case "Ember Orange"
                selectedColor = Color.FromArgb(194, 65, 12)
            Case "Forest Green"
                selectedColor = Color.FromArgb(21, 128, 61)
            Case "Violet"
                selectedColor = Color.FromArgb(109, 40, 217)
            Case "Custom Color"
                selectedColor = My.Settings.i
        End Select
        chooseColor.Enabled = themeBox.Text = "Custom Color"
        qupdatepreview()
    End Sub

    Sub qupdatepreview()
        Dim color As Color = selectedColor
        If themeBox.Text = "Firespider Original" Then color = Color.FromArgb(0, 204, 255)
        If themeBox.Text = "Match Website Colors Automatically" Then
            Dim siteColor As Color = ownerBrowser.qsitechromecolor(ownerBrowser.qthemebrowser())
            If siteColor.IsEmpty = False Then color = siteColor
        End If
        preview.BackColor = color
        previewText.ForeColor = ownerBrowser.qthemeforeground(color)
        previewText.Text = "Toolbar preview - navigation buttons keep their existing appearance"
    End Sub

    Sub qchoosecolor(ByVal sender As Object, ByVal e As EventArgs)
        Dim dialog As New ColorDialog
        dialog.Color = selectedColor
        If dialog.ShowDialog(Me) <> DialogResult.OK Then Exit Sub
        selectedColor = dialog.Color
        qupdatepreview()
    End Sub

    Sub qusecurrentcolor(ByVal sender As Object, ByVal e As EventArgs)
        ownerBrowser.qusecurrentpagecolor()
        selectedColor = My.Settings.i
        themeBox.SelectedItem = "Custom Color"
        qupdatepreview()
    End Sub

    Sub qapplytheme(ByVal sender As Object, ByVal e As EventArgs)
        If themeBox.Text = "Firespider Original" Then
            ownerBrowser.qsetbrowserappearance("original", Color.Black)
        ElseIf themeBox.Text = "Match Website Colors Automatically" Then
            ownerBrowser.qsetbrowserappearance("site", selectedColor)
        Else
            ownerBrowser.qsetbrowserappearance("custom", selectedColor)
        End If
        qupdatepreview()
    End Sub

    Sub qcloseappearance(ByVal sender As Object, ByVal e As EventArgs)
        Close()
    End Sub
End Class
