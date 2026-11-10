Imports System.Text

Partial Class j
    Sub qfeatures5()
        fl.DropDownItems.Add(qnewitem("Move Current Tab To New Window", AddressOf q913, AddressOf q914))
        fl.DropDownItems.Add(qnewitem("Duplicate Current Tab To New Window", AddressOf q915, AddressOf q916))
        v.DropDownItems.Add(qnewitem("Suspend Or Resume Current Tab", AddressOf q917, AddressOf q918))
        v.DropDownItems.Add(qnewitem("Suspend Background Tabs", AddressOf q919, AddressOf q920))
        v.DropDownItems.Add(qnewitem("Resume All Suspended Tabs", AddressOf q921, AddressOf q922))
        v.DropDownItems.Add(qnewitem("Open Printer-Friendly Page", AddressOf q945, AddressOf q946))
        qfeatures6()
        qfeatures7()
        qfeatures8()
        qfeatures9()
        qfeatures10()
    End Sub

    ' --- Move And Duplicate Tabs Between Windows ---
    Sub q913(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If k.SelectedTab Is Nothing OrElse k.SelectedTab.Controls.Count = 0 Then Exit Sub
            Dim tab As TabPage = k.SelectedTab
            Dim wb As WebBrowser = TryCast(tab.Controls(0), WebBrowser)
            Dim url As String = qpersistedtaburl(tab, wb)
            If url = "" Then
                su.Visible = True
                su.Text = "Current tab has no address"
                Exit Sub
            End If
            qnewwindowwithurl(url)
            suspendedGroupUrls.Remove(tab)
            If k.TabPages.Count > 1 Then
                k.TabPages.Remove(tab)
                tab.Dispose()
                I = k.TabPages.Count
            Else
                q1(qntu())
            End If
            su.Visible = True
            su.Text = "Tab moved to a new window"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q914(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q913(sender, e)
    End Sub

    Sub q915(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If k.SelectedTab Is Nothing OrElse k.SelectedTab.Controls.Count = 0 Then Exit Sub
            Dim wb As WebBrowser = TryCast(k.SelectedTab.Controls(0), WebBrowser)
            Dim url As String = qpersistedtaburl(k.SelectedTab, wb)
            If url = "" Then
                su.Visible = True
                su.Text = "Current tab has no address"
                Exit Sub
            End If
            qnewwindowwithurl(url)
            su.Visible = True
            su.Text = "Tab duplicated in a new window"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q916(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q915(sender, e)
    End Sub

    Sub qnewwindowwithurl(ByVal url As String)
        Dim browserWindow As New j
        browserWindow.Show()
        browserWindow.q1(url)
        browserWindow.Activate()
    End Sub

    ' --- Direct Tab Suspension ---
    Sub q917(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If k.SelectedTab Is Nothing OrElse k.SelectedTab.Controls.Count = 0 Then Exit Sub
            Dim tab As TabPage = k.SelectedTab
            Dim wb As WebBrowser = TryCast(tab.Controls(0), WebBrowser)
            If suspendedGroupUrls.ContainsKey(tab) Then
                If Not qfeature6resumetab(tab) Then Exit Sub
                su.Visible = True
                su.Text = "Tab resumed"
            ElseIf qsuspendtab(tab) Then
                su.Visible = True
                su.Text = "Tab suspended"
            End If
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q918(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q917(sender, e)
    End Sub

    Sub q919(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim current As TabPage = k.SelectedTab
            Dim suspended As Integer = 0
            For Each tab As TabPage In k.TabPages
                If tab IsNot current AndAlso qsuspendtab(tab) Then suspended += 1
            Next tab
            su.Visible = True
            su.Text = suspended.ToString & " background tab(s) suspended"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q920(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q919(sender, e)
    End Sub

    Sub q921(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim resumed As Integer = 0
            For Each tab As TabPage In suspendedGroupUrls.Keys.ToArray
                If qfeature6resumetab(tab) Then resumed += 1
            Next tab
            su.Visible = True
            su.Text = resumed.ToString & " suspended tab(s) resumed"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q922(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q921(sender, e)
    End Sub

    ' --- Printer-Friendly Page ---
    Sub q945(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If k.SelectedTab Is Nothing OrElse k.SelectedTab.Controls.Count = 0 Then Exit Sub
            Dim source As WebBrowser = TryCast(k.SelectedTab.Controls(0), WebBrowser)
            If source Is Nothing OrElse source.Document Is Nothing OrElse source.Document.Body Is Nothing Then
                su.Visible = True
                su.Text = "The current page is not ready to print"
                Exit Sub
            End If

            Dim options As New PrinterOptionsForm
            If options.ShowDialog(Me) <> DialogResult.OK Then Exit Sub

            Dim body As String = ""
            If options.SelectionOnly Then
                Dim selected As String = qprinterselection(source)
                If selected.Trim = "" Then
                    su.Visible = True
                    su.Text = "Select page text before using selected-content printing"
                    Exit Sub
                End If
                body = "<div class=""selected-content"">" & qh(selected).Replace(vbCrLf, "<br>").Replace(vbLf, "<br>") & "</div>"
            Else
                body = source.Document.Body.InnerHtml
                body = System.Text.RegularExpressions.Regex.Replace(body, "<script\b[^>]*>.*?</script\s*>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase Or System.Text.RegularExpressions.RegexOptions.Singleline)
            End If
            Dim title As String = If(source.DocumentTitle = "", "Printer-Friendly Page", source.DocumentTitle)
            Dim sourceUrl As String = If(source.Url Is Nothing, "", source.Url.ToString)
            Dim pageSize As String = If(options.UseLandscape, "landscape", "portrait")
            Dim margin As String = options.MarginInches.ToString(System.Globalization.CultureInfo.InvariantCulture)
            Dim scale As String = (options.ScalePercent / 100.0R).ToString(System.Globalization.CultureInfo.InvariantCulture)
            Dim colorStyle As String = If(options.Grayscale, "filter:grayscale(100%);-ms-filter:grayscale(100%);", "")
            Dim pageHeader As String = If(options.IncludeHeader, "<div class=""print-header""><h1>" & qh(title) & "</h1><p>" & qh(sourceUrl) & "</p></div>", "")
            Dim pageFooter As String = If(options.IncludeFooter, "<div class=""print-footer"">Firespider Internet - <span class=""page-number"">Page </span></div>", "")
            Dim html As New StringBuilder
            html.AppendLine("<!doctype html><html><head><meta charset=""utf-8""><title>" & qh(title) & "</title>" & If(sourceUrl = "", "", "<base href=""" & qh(sourceUrl) & """>") )
            html.AppendLine("<style>@page{size:" & pageSize & ";margin:" & margin & "in;}html{background:#fff}body{background:#fff;color:#111;font:12pt Georgia,'Times New Roman',serif;line-height:1.5;margin:0 auto;max-width:780px;padding:28px;}body," & "#print-content{" & colorStyle & "}#print-content{zoom:" & scale & ";width:100%;}.print-header{border-bottom:1px solid #999;margin-bottom:24px;padding-bottom:12px}.print-header h1{font:700 22pt Arial,sans-serif;margin:0 0 6px}.print-header p{color:#555;font:9pt Arial,sans-serif;margin:0;overflow-wrap:anywhere}.print-footer{color:#555;font:9pt Arial,sans-serif;margin-top:24px;border-top:1px solid #999;padding-top:8px}.page-number:after{content:counter(page)}article{width:100%}article img,article video,article audio,article iframe,article canvas{max-width:100%;height:auto}article table{border-collapse:collapse;max-width:100%}article th,article td{border:1px solid #999;padding:4px 7px}nav,aside,footer,form,button,input,select,textarea,menu,dialog,video,audio,iframe,canvas,[role='navigation'],[role='banner'],[role='complementary'],.ad,.ads,.advert,.advertisement,.cookie,.cookies,.popup,.modal,.share,.social{display:none!important}h1,h2,h3,h4,h5,h6{page-break-after:avoid}img,table,blockquote,pre{page-break-inside:avoid}@media print{body{font-size:11pt;padding:0;max-width:none}.print-footer{position:fixed;bottom:0;left:0;right:0;margin:0;background:#fff}a{color:#111;text-decoration:none}a[href]:after{content:' (' attr(href) ')';font-size:8pt;overflow-wrap:anywhere}}</style></head><body>")
            html.AppendLine(pageHeader & "<article id=""print-content"">" & body & "</article>" & pageFooter & "</body></html>")

            Dim folder As String = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Firespider Internet")
            System.IO.Directory.CreateDirectory(folder)
            Dim path As String = System.IO.Path.Combine(folder, "PrinterFriendly.html")
            System.IO.File.WriteAllText(path, html.ToString, Encoding.UTF8)
            q3(New Uri(path).AbsoluteUri)
            su.Visible = True
            su.Text = "Printer-friendly page opened with print options"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    Sub q946(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q945(sender, e)
    End Sub

    Friend Function qprinterselection(ByVal source As WebBrowser) As String
        Try
            Dim script As String = "(function(){try{if(window.getSelection){return window.getSelection().toString();}if(document.selection){return document.selection.createRange().text;}return ''; }catch(e){return '';}})()"
            Dim result As Object = source.Document.InvokeScript("eval", New Object() {script})
            If result IsNot Nothing Then Return result.ToString
        Catch ex As Exception
            qlog(ex)
        End Try
        Return ""
    End Function

    Function qsuspendtab(ByVal tab As TabPage) As Boolean
        If tab Is Nothing OrElse tab.Controls.Count = 0 OrElse suspendedGroupUrls.ContainsKey(tab) Then Return False
        If qleaHost(tab) IsNot Nothing Then Return qleaSuspend(tab)
        Dim wb As WebBrowser = TryCast(tab.Controls(0), WebBrowser)
        If wb Is Nothing OrElse wb.Url Is Nothing OrElse wb.Url.ToString = "about:blank" Then Return False
        Dim title As String = If(tab.Text = "", wb.Url.Host, tab.Text)
        suspendedGroupUrls(tab) = wb.Url.ToString
        wb.Stop()
        wb.Navigate("about:blank")
        tab.Text = "[Suspended] " & title
        Return True
    End Function
End Class

Friend Class PrinterOptionsForm
    Inherits Form

    Dim orientationBox As New ComboBox
    Dim marginBox As New ComboBox
    Dim scaleBox As New ComboBox
    Dim grayscaleBox As New CheckBox
    Dim headerBox As New CheckBox
    Dim footerBox As New CheckBox
    Dim selectionBox As New CheckBox
    Public UseLandscape As Boolean
    Public MarginInches As Double
    Public ScalePercent As Integer
    Public Grayscale As Boolean
    Public IncludeHeader As Boolean
    Public IncludeFooter As Boolean
    Public SelectionOnly As Boolean

    Sub New()
        Text = "Printer-Friendly Print Options"
        StartPosition = FormStartPosition.CenterParent
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        ShowInTaskbar = False
        ClientSize = New Size(500, 355)

        Dim description As New Label With {.Text = "Choose how the printer-friendly copy should be prepared. The normal print dialog still controls the printer and page range.", .Left = 14, .Top = 12, .Width = 470, .Height = 38}
        Dim orientationLabel As New Label With {.Text = "Orientation:", .Left = 14, .Top = 62, .Width = 145}
        Dim marginLabel As New Label With {.Text = "Margins:", .Left = 14, .Top = 98, .Width = 145}
        Dim scaleLabel As New Label With {.Text = "Scale:", .Left = 14, .Top = 134, .Width = 145}

        orientationBox.Left = 165
        orientationBox.Top = 58
        orientationBox.Width = 150
        orientationBox.DropDownStyle = ComboBoxStyle.DropDownList
        orientationBox.Items.Add("Portrait")
        orientationBox.Items.Add("Landscape")
        orientationBox.SelectedIndex = 0

        marginBox.Left = 165
        marginBox.Top = 94
        marginBox.Width = 150
        marginBox.DropDownStyle = ComboBoxStyle.DropDownList
        For Each value As String In New String() {"0.35 in", "0.5 in", "0.75 in", "1.0 in", "1.25 in"}
            marginBox.Items.Add(value)
        Next value
        marginBox.SelectedIndex = 2

        scaleBox.Left = 165
        scaleBox.Top = 130
        scaleBox.Width = 150
        scaleBox.DropDownStyle = ComboBoxStyle.DropDownList
        For Each value As String In New String() {"75%", "90%", "100%", "110%", "125%"}
            scaleBox.Items.Add(value)
        Next value
        scaleBox.SelectedIndex = 2

        grayscaleBox.Text = "Print in grayscale"
        grayscaleBox.Left = 14
        grayscaleBox.Top = 174
        grayscaleBox.Width = 220

        headerBox.Text = "Include page header (title and URL)"
        headerBox.Left = 14
        headerBox.Top = 202
        headerBox.Width = 300
        headerBox.Checked = True

        footerBox.Text = "Include footer and page numbering"
        footerBox.Left = 14
        footerBox.Top = 230
        footerBox.Width = 300
        footerBox.Checked = True

        selectionBox.Text = "Print selected text only"
        selectionBox.Left = 14
        selectionBox.Top = 258
        selectionBox.Width = 300

        Dim note As New Label With {.Text = "Selected-content printing uses readable text from the current selection. Leave it unchecked to print the full page.", .Left = 14, .Top = 286, .Width = 470, .Height = 32}
        Dim okButton As New Button With {.Text = "Create Print Page", .Left = 294, .Top = 320, .Width = 105, .DialogResult = DialogResult.None}
        Dim cancelButton As New Button With {.Text = "Cancel", .Left = 409, .Top = 320, .Width = 75, .DialogResult = DialogResult.Cancel}

        Controls.Add(description)
        Controls.Add(orientationLabel)
        Controls.Add(marginLabel)
        Controls.Add(scaleLabel)
        Controls.Add(orientationBox)
        Controls.Add(marginBox)
        Controls.Add(scaleBox)
        Controls.Add(grayscaleBox)
        Controls.Add(headerBox)
        Controls.Add(footerBox)
        Controls.Add(selectionBox)
        Controls.Add(note)
        Controls.Add(okButton)
        Controls.Add(cancelButton)
        AcceptButton = okButton
        CancelButton = cancelButton
        AddHandler okButton.Click, AddressOf qaccept
    End Sub

    Sub qaccept(ByVal sender As Object, ByVal e As EventArgs)
        Dim margin As Double = 0
        Dim marginText As String = marginBox.SelectedItem.ToString.Replace(" in", "")
        If Double.TryParse(marginText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, margin) = False Then Exit Sub
        Dim scaleText As String = scaleBox.SelectedItem.ToString.Replace("%", "")
        Dim scale As Integer = 0
        If Integer.TryParse(scaleText, scale) = False Then Exit Sub
        UseLandscape = orientationBox.SelectedIndex = 1
        MarginInches = margin
        ScalePercent = scale
        Grayscale = grayscaleBox.Checked
        IncludeHeader = headerBox.Checked
        IncludeFooter = footerBox.Checked
        SelectionOnly = selectionBox.Checked
        DialogResult = DialogResult.OK
        Close()
    End Sub
End Class
