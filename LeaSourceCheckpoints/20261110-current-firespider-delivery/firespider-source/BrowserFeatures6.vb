Imports System.Collections.Specialized

Partial Class j
    Dim feature6HibernateTimer As New Timer
    Dim feature6IdleSince As New Dictionary(Of TabPage, DateTime)
    Dim feature6MruTabs As New List(Of TabPage)
    Dim feature6CycleTabs As New List(Of TabPage)
    Dim feature6LazyTabs As New HashSet(Of TabPage)
    Dim feature6WiredBrowsers As New HashSet(Of WebBrowser)
    Dim feature6WiredTabs As New HashSet(Of TabPage)
    Dim feature6ActiveTab As TabPage
    Dim feature6Loaded As Boolean = False
    Dim feature6Cycling As Boolean = False

    Sub qfeatures6()
        Try
            v.DropDownItems.Add(qnewitem("Hibernate Eligible Tabs Now", AddressOf q923, AddressOf q924))
            ot.DropDownItems.Add(qnewitem("Automatic Tab Hibernation Policy", AddressOf q925, AddressOf q926))
            ot.DropDownItems.Add(qnewitem("Lazy Session Restore Mode", AddressOf q927, AddressOf q928))
            v.DropDownItems.Add(qnewitem("Most-Recently-Used Tab Switcher (Ctrl+Tab)", AddressOf q929, AddressOf q930))
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    Sub qfeature6loaded()
        Try
            If feature6Loaded Then Exit Sub
            feature6Loaded = True
            feature6HibernateTimer.Interval = 30000
            AddHandler feature6HibernateTimer.Tick, AddressOf qfeature6hibernatetick
            AddHandler k.SelectedIndexChanged, AddressOf qfeature6selectedtabchanged
            AddHandler k.ControlAdded, AddressOf qfeature6tabcontroladded
            AddHandler Me.KeyDown, AddressOf qfeature6keydown
            feature6IdleSince.Clear()
            feature6MruTabs.Clear()
            feature6ActiveTab = k.SelectedTab
            If feature6ActiveTab IsNot Nothing Then feature6MruTabs.Add(feature6ActiveTab)
            For Each tab As TabPage In k.TabPages
                feature6IdleSince(tab) = Now
                If feature6MruTabs.Contains(tab) = False Then feature6MruTabs.Add(tab)
                qfeature6wiretab(tab)
            Next tab
            If My.Settings.hibernateExceptions Is Nothing Then My.Settings.hibernateExceptions = New StringCollection
            If My.Settings.hibernateEnabled = "e" AndAlso pv = False Then feature6HibernateTimer.Start()
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    Sub qfeature6closing(ByVal finalWindow As Boolean)
        Try
            feature6HibernateTimer.Stop()
            If feature6Loaded Then
                RemoveHandler feature6HibernateTimer.Tick, AddressOf qfeature6hibernatetick
                RemoveHandler k.SelectedIndexChanged, AddressOf qfeature6selectedtabchanged
                RemoveHandler k.ControlAdded, AddressOf qfeature6tabcontroladded
                RemoveHandler Me.KeyDown, AddressOf qfeature6keydown
            End If
            For Each tab As TabPage In feature6WiredTabs.ToArray
                If tab IsNot Nothing AndAlso tab.IsDisposed = False Then RemoveHandler tab.ControlAdded, AddressOf qfeature6tabchildadded
            Next tab
            For Each wb As WebBrowser In feature6WiredBrowsers.ToArray
                If wb IsNot Nothing AndAlso wb.IsDisposed = False Then RemoveHandler wb.DocumentCompleted, AddressOf qfeature6suspendeddocumentcompleted
            Next wb
            feature6WiredBrowsers.Clear()
            feature6WiredTabs.Clear()
            feature6IdleSince.Clear()
            feature6MruTabs.Clear()
            feature6CycleTabs.Clear()
            feature6LazyTabs.Clear()
            feature6ActiveTab = Nothing
            feature6Loaded = False
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    ' --- Automatic Tab Hibernation ---
    Sub q923(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If pv Then
                su.Visible = True
                su.Text = "Tab hibernation is skipped in private mode"
                Exit Sub
            End If
            Dim hibernated As Integer = qfeature6hibernateeligible(False)
            su.Visible = True
            su.Text = hibernated.ToString & " eligible background tab(s) hibernated"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q924(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q923(sender, e)
    End Sub

    Sub q925(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If My.Settings.hibernateExceptions Is Nothing Then My.Settings.hibernateExceptions = New StringCollection
            Dim action As String = InputBox(qfeature6policysummary() & vbCrLf & vbCrLf & "Enter TOGGLE, MINUTES, ADD, REMOVE, or CLEAR:", "Automatic Tab Hibernation Policy", "TOGGLE").Trim.ToUpperInvariant
            If action = "" Then Exit Sub
            If action = "TOGGLE" Then
                My.Settings.hibernateEnabled = If(My.Settings.hibernateEnabled = "e", "", "e")
                If My.Settings.hibernateEnabled = "e" AndAlso pv = False AndAlso feature6Loaded Then feature6HibernateTimer.Start() Else feature6HibernateTimer.Stop()
            ElseIf action = "MINUTES" Then
                Dim minutes As Integer = 0
                Dim answer As String = InputBox("Hibernate eligible background tabs after how many idle minutes? Enter 1 through 10080.", "Hibernation Idle Time", qfeature6idleminutes().ToString).Trim
                If Integer.TryParse(answer, minutes) = False OrElse minutes < 1 OrElse minutes > 10080 Then
                    MessageBox.Show("Enter a whole number from 1 through 10080.", "Automatic Tab Hibernation Policy", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Exit Sub
                End If
                My.Settings.hibernateMinutes = minutes.ToString
            ElseIf action = "ADD" Then
                Dim domain As String = qfeature6normalizedomain(InputBox("Enter a domain that must never be hibernated automatically:", "Add Hibernation Exception", qfeature6currentdomain()).Trim)
                If domain = "" Then Exit Sub
                If qfeature6exceptionindex(domain) < 0 Then My.Settings.hibernateExceptions.Add(domain)
            ElseIf action = "REMOVE" Then
                If My.Settings.hibernateExceptions.Count = 0 Then
                    su.Visible = True
                    su.Text = "There are no hibernation exceptions to remove"
                    Exit Sub
                End If
                Dim answer As String = InputBox("Enter an exception number or domain to remove:" & vbCrLf & qfeature6exceptiontext(), "Remove Hibernation Exception", "1").Trim
                If answer = "" Then Exit Sub
                Dim index As Integer = 0
                If Integer.TryParse(answer, index) Then
                    index -= 1
                Else
                    index = qfeature6exceptionindex(qfeature6normalizedomain(answer))
                End If
                If index >= 0 AndAlso index < My.Settings.hibernateExceptions.Count Then
                    My.Settings.hibernateExceptions.RemoveAt(index)
                Else
                    su.Visible = True
                    su.Text = "Hibernation exception was not found"
                    Exit Sub
                End If
            ElseIf action = "CLEAR" Then
                If MessageBox.Show("Remove all automatic hibernation exceptions?", "Automatic Tab Hibernation Policy", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Exit Sub
                My.Settings.hibernateExceptions.Clear()
            Else
                su.Visible = True
                su.Text = "No hibernation policy change was made"
                Exit Sub
            End If
            My.Settings.Save()
            su.Visible = True
            su.Text = "Automatic tab hibernation policy updated"
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q926(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q925(sender, e)
    End Sub

    Sub qfeature6hibernatetick(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If pv OrElse My.Settings.hibernateEnabled <> "e" Then Exit Sub
            Dim hibernated As Integer = qfeature6hibernateeligible(True)
            If hibernated > 0 Then
                su.Visible = True
                su.Text = hibernated.ToString & " idle background tab(s) hibernated"
            End If
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    Function qfeature6hibernateeligible(ByVal enforceIdle As Boolean) As Integer
        Dim hibernated As Integer = 0
        If pv Then Return hibernated
        qfeature6prunestate()
        Dim idleMinutes As Integer = qfeature6idleminutes()
        For Each tab As TabPage In k.TabPages
            Try
                If tab Is k.SelectedTab Then Continue For
                If feature6IdleSince.ContainsKey(tab) = False Then
                    feature6IdleSince(tab) = Now
                    If enforceIdle Then Continue For
                End If
                If enforceIdle AndAlso Now.Subtract(feature6IdleSince(tab)).TotalMinutes < idleMinutes Then Continue For
                If qfeature6tabeligible(tab) Then
                    Dim wb As WebBrowser = TryCast(tab.Controls(0), WebBrowser)
                    qfeature6wirebrowser(wb)
                    If qsuspendtab(tab) Then hibernated += 1
                End If
            Catch ex As Exception
                qlog(ex)
            End Try
        Next tab
        Return hibernated
    End Function

    Function qfeature6tabeligible(ByVal tab As TabPage) As Boolean
        Try
            If pv OrElse tab Is Nothing OrElse tab.IsDisposed OrElse tab Is k.SelectedTab Then Return False
            If k.TabPages.Contains(tab) = False OrElse tab.Controls.Count = 0 OrElse suspendedGroupUrls.ContainsKey(tab) Then Return False
            Dim protection As String = If(tab.Tag Is Nothing, "", tab.Tag.ToString).Trim.ToLowerInvariant
            If protection = "locked" OrElse protection = "pinned" Then Return False
            If tab.ToolTipText.StartsWith("Tab group:", StringComparison.OrdinalIgnoreCase) Then Return False
            Dim lea As Global.Lea.Win.V = qleaHost(tab)
            If lea IsNot Nothing Then Return qleaHibernateEligible(lea)
            Dim wb As WebBrowser = TryCast(tab.Controls(0), WebBrowser)
            If wb Is Nothing OrElse wb.IsBusy OrElse wb.ReadyState <> WebBrowserReadyState.Complete OrElse wb.Url Is Nothing OrElse wb.Document Is Nothing Then Return False
            If wb.Url.ToString = "about:blank" OrElse qfeature6domainexcepted(wb.Url) Then Return False
            If qfeature6haseditabledata(wb) OrElse qfeature6hasactivemedia(wb) Then Return False
            Return True
        Catch
            Return False
        End Try
    End Function

    Function qfeature6haseditabledata(ByVal wb As WebBrowser) As Boolean
        Try
            If wb Is Nothing OrElse wb.Document Is Nothing Then Return True
            Dim script As String = "(function(){try{var i,e,t,v,items=document.getElementsByTagName('input');for(i=0;i<items.length;i++){e=items[i];t=(e.type||'text').toLowerCase();if(e.disabled||e.readOnly)continue;if(t==='checkbox'||t==='radio'){if(e.checked!==e.defaultChecked)return true;continue;}if(t==='hidden'||t==='button'||t==='submit'||t==='reset'||t==='image')continue;v=String(e.value||'').replace(/^\s+|\s+$/g,'');if(v.length)return true;}items=document.getElementsByTagName('textarea');for(i=0;i<items.length;i++){e=items[i];if(!e.disabled&&!e.readOnly&&String(e.value||'').replace(/^\s+|\s+$/g,'').length)return true;}items=document.getElementsByTagName('select');for(i=0;i<items.length;i++){e=items[i];if(e.disabled)continue;for(var o=0;o<e.options.length;o++){if(e.options[o].selected!==e.options[o].defaultSelected)return true;}}items=document.getElementsByTagName('*');for(i=0;i<items.length;i++){e=items[i];if((e.contentEditable==='true'||e.isContentEditable)&&String(e.innerText||e.textContent||'').replace(/^\s+|\s+$/g,'').length)return true;}return false;}catch(x){return null;}})()"
            Dim result As Object = wb.Document.InvokeScript("eval", New Object() {script})
            If result IsNot Nothing Then Return CBool(result)
        Catch
        End Try
        Try
            For Each input As HtmlElement In wb.Document.GetElementsByTagName("input")
                If qfeature6attributeon(input, "disabled") OrElse qfeature6attributeon(input, "readOnly") Then Continue For
                Dim inputType As String = input.GetAttribute("type").Trim.ToLowerInvariant
                If inputType = "hidden" OrElse inputType = "button" OrElse inputType = "submit" OrElse inputType = "reset" OrElse inputType = "image" Then Continue For
                If inputType = "checkbox" OrElse inputType = "radio" Then
                    Try
                        Dim checked As Boolean = CBool(CallByName(input.DomElement, "checked", CallType.Get))
                        Dim defaultChecked As Boolean = CBool(CallByName(input.DomElement, "defaultChecked", CallType.Get))
                        If checked <> defaultChecked Then Return True
                    Catch
                    End Try
                ElseIf input.GetAttribute("value").Trim <> "" Then
                    Return True
                End If
            Next input
            For Each area As HtmlElement In wb.Document.GetElementsByTagName("textarea")
                If qfeature6attributeon(area, "disabled") = False AndAlso qfeature6attributeon(area, "readOnly") = False Then
                    If area.GetAttribute("value").Trim <> "" OrElse If(area.InnerText, "").Trim <> "" Then Return True
                End If
            Next area
            For Each element As HtmlElement In wb.Document.GetElementsByTagName("*")
                If element.GetAttribute("contentEditable").Equals("true", StringComparison.OrdinalIgnoreCase) AndAlso If(element.InnerText, "").Trim <> "" Then Return True
            Next element
            Return False
        Catch
            Return True
        End Try
    End Function

    Function qfeature6attributeon(ByVal element As HtmlElement, ByVal name As String) As Boolean
        Try
            Dim value As String = element.GetAttribute(name).Trim.ToLowerInvariant
            Return value = "true" OrElse value = "-1" OrElse value = name.ToLowerInvariant
        Catch
            Return False
        End Try
    End Function

    Function qfeature6hasactivemedia(ByVal wb As WebBrowser) As Boolean
        Try
            If wb Is Nothing OrElse wb.Document Is Nothing Then Return True
            Dim script As String = "(function(){try{var tags=['audio','video'];for(var t=0;t<tags.length;t++){var items=document.getElementsByTagName(tags[t]);for(var i=0;i<items.length;i++){var e=items[i];if(typeof e.paused==='undefined')return true;if(!e.paused&&!e.ended)return true;}}return false;}catch(x){return null;}})()"
            Dim result As Object = wb.Document.InvokeScript("eval", New Object() {script})
            If result IsNot Nothing Then Return CBool(result)
        Catch
        End Try
        Try
            Return wb.Document.GetElementsByTagName("audio").Count > 0 OrElse wb.Document.GetElementsByTagName("video").Count > 0
        Catch
            Return True
        End Try
    End Function

    Function qfeature6idleminutes() As Integer
        Dim minutes As Integer = 30
        If Integer.TryParse(My.Settings.hibernateMinutes, minutes) = False OrElse minutes < 1 Then minutes = 30
        Return minutes
    End Function

    Function qfeature6domainexcepted(ByVal address As Uri) As Boolean
        If address Is Nothing OrElse address.Host = "" OrElse My.Settings.hibernateExceptions Is Nothing Then Return False
        Dim host As String = qfeature6normalizedomain(address.Host)
        For Each raw As String In My.Settings.hibernateExceptions
            Dim exceptionDomain As String = qfeature6normalizedomain(raw)
            If exceptionDomain <> "" AndAlso (host.Equals(exceptionDomain, StringComparison.OrdinalIgnoreCase) OrElse host.EndsWith("." & exceptionDomain, StringComparison.OrdinalIgnoreCase)) Then Return True
        Next raw
        Return False
    End Function

    Function qfeature6normalizedomain(ByVal value As String) As String
        Try
            value = value.Trim.ToLowerInvariant
            While value.StartsWith("*.")
                value = value.Substring(2)
            End While
            Dim address As Uri = Nothing
            If Uri.TryCreate(value, UriKind.Absolute, address) AndAlso address.Host <> "" Then value = address.Host
            If value.Contains("/") OrElse value.Contains(":") Then
                If Uri.TryCreate("http://" & value, UriKind.Absolute, address) AndAlso address.Host <> "" Then value = address.Host
            End If
            Return value.Trim().Trim("."c)
        Catch
            Return ""
        End Try
    End Function

    Function qfeature6currentdomain() As String
        Try
            If k.SelectedTab Is Nothing OrElse k.SelectedTab.Controls.Count = 0 Then Return ""
            Dim wb As WebBrowser = TryCast(k.SelectedTab.Controls(0), WebBrowser)
            Dim url As String = qpersistedtaburl(k.SelectedTab, wb)
            Dim address As Uri = Nothing
            If Uri.TryCreate(url, UriKind.Absolute, address) Then Return address.Host
        Catch
        End Try
        Return ""
    End Function

    Function qfeature6exceptionindex(ByVal domain As String) As Integer
        If My.Settings.hibernateExceptions Is Nothing Then Return -1
        For i As Integer = 0 To My.Settings.hibernateExceptions.Count - 1
            If qfeature6normalizedomain(My.Settings.hibernateExceptions(i)).Equals(domain, StringComparison.OrdinalIgnoreCase) Then Return i
        Next i
        Return -1
    End Function

    Function qfeature6exceptiontext() As String
        If My.Settings.hibernateExceptions Is Nothing OrElse My.Settings.hibernateExceptions.Count = 0 Then Return "None"
        Dim lines As New List(Of String)
        For i As Integer = 0 To My.Settings.hibernateExceptions.Count - 1
            lines.Add((i + 1).ToString & ". " & My.Settings.hibernateExceptions(i))
        Next i
        Return String.Join(vbCrLf, lines.ToArray)
    End Function

    Function qfeature6policysummary() As String
        Return "Automatic hibernation: " & If(My.Settings.hibernateEnabled = "e", "enabled", "disabled") & vbCrLf &
               "Idle time: " & qfeature6idleminutes().ToString & " minute(s)" & vbCrLf &
               "Domain exceptions:" & vbCrLf & qfeature6exceptiontext()
    End Function

    ' --- Lazy Session Restore ---
    Sub q927(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim mode As String = InputBox("Current mode: " & qfeature6lazymode().ToUpperInvariant & vbCrLf & vbCrLf & "OFF loads every restored tab immediately." & vbCrLf & "BACKGROUND loads the first tab and keeps background tabs suspended." & vbCrLf & "ALL creates every restored tab as a suspended placeholder." & vbCrLf & vbCrLf & "Enter OFF, BACKGROUND, or ALL:", "Lazy Session Restore Mode", qfeature6lazymode().ToUpperInvariant).Trim.ToLowerInvariant
            If mode = "" Then Exit Sub
            If mode <> "off" AndAlso mode <> "background" AndAlso mode <> "all" Then
                MessageBox.Show("Enter OFF, BACKGROUND, or ALL.", "Lazy Session Restore Mode", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Exit Sub
            End If
            My.Settings.lazyRestoreMode = mode
            My.Settings.Save()
            su.Visible = True
            su.Text = "Lazy session restore mode set to " & mode
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q928(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q927(sender, e)
    End Sub

    Friend Function qlazyopenurls(ByVal urls As List(Of String)) As Integer
        Try
            If urls Is Nothing OrElse urls.Count = 0 Then Return 0
            Dim mode As String = qfeature6lazymode()
            Dim restored As Integer = qleaRestoreSaved(urls, True)
            Dim deferred As Integer = If(mode = "all", restored, If(mode = "background", Math.Max(0, restored - 1), 0))
            su.Visible = True
            su.Text = restored.ToString & " Lea tab(s) restored; " & deferred.ToString & " deferred"
            If restored < urls.Count Then su.Text &= "; " & (urls.Count - restored).ToString & " URL(s) skipped by Lea limits"
            Return restored
        Catch ex As Exception
            qlog(ex)
            Return 0
        End Try
    End Function

    Function qfeature6lazymode() As String
        Dim mode As String = If(My.Settings.lazyRestoreMode, "").Trim.ToLowerInvariant
        If mode <> "background" AndAlso mode <> "all" Then mode = "off"
        Return mode
    End Function

    Function qfeature6newbrowser() As WebBrowser
        Dim wb As New WebBrowser
        wb.Dock = DockStyle.Fill
        AddHandler wb.Navigating, AddressOf nn
        AddHandler wb.Navigated, AddressOf nv
        AddHandler wb.DocumentCompleted, AddressOf n0
        AddHandler wb.ProgressChanged, AddressOf q14
        AddHandler wb.NewWindow, AddressOf qpopup
        qfeature6wirebrowser(wb)
        Return wb
    End Function

    Function qfeature6resumetab(ByVal tab As TabPage) As Boolean
        Try
            If tab Is Nothing OrElse tab.IsDisposed OrElse suspendedGroupUrls.ContainsKey(tab) = False Then Return False
            If qleaPaused.ContainsKey(tab) Then Return qleaResume(tab, True) IsNot Nothing
            Dim url As String = suspendedGroupUrls(tab)
            If tab.Controls.Count = 0 Then
                tab.Controls.Add(qfeature6newbrowser())
            ElseIf TryCast(tab.Controls(0), WebBrowser) Is Nothing Then
                tab.Controls.Clear()
                tab.Controls.Add(qfeature6newbrowser())
            End If
            Dim wb As WebBrowser = TryCast(tab.Controls(0), WebBrowser)
            If wb Is Nothing Then Return False
            suspendedGroupUrls.Remove(tab)
            feature6LazyTabs.Remove(tab)
            feature6IdleSince(tab) = Now
            wb.Navigate(url)
            Return True
        Catch ex As Exception
            qlog(ex)
            Return False
        End Try
    End Function

    Sub qfeature6wirebrowser(ByVal wb As WebBrowser)
        If wb Is Nothing OrElse wb.IsDisposed OrElse feature6WiredBrowsers.Contains(wb) Then Exit Sub
        feature6WiredBrowsers.Add(wb)
        AddHandler wb.DocumentCompleted, AddressOf qfeature6suspendeddocumentcompleted
    End Sub

    Sub qfeature6wiretab(ByVal tab As TabPage)
        If tab Is Nothing OrElse tab.IsDisposed Then Exit Sub
        If feature6WiredTabs.Add(tab) Then AddHandler tab.ControlAdded, AddressOf qfeature6tabchildadded
        For Each control As Control In tab.Controls
            qfeature6wirebrowser(TryCast(control, WebBrowser))
        Next control
    End Sub

    Sub qfeature6tabcontroladded(ByVal sender As Object, ByVal e As ControlEventArgs)
        Dim tab As TabPage = TryCast(e.Control, TabPage)
        If tab Is Nothing Then Exit Sub
        qfeature6wiretab(tab)
        feature6IdleSince(tab) = Now
        If feature6MruTabs.Contains(tab) = False Then feature6MruTabs.Add(tab)
    End Sub

    Sub qfeature6tabchildadded(ByVal sender As Object, ByVal e As ControlEventArgs)
        qfeature6wirebrowser(TryCast(e.Control, WebBrowser))
    End Sub

    Sub qfeature6suspendeddocumentcompleted(ByVal sender As Object, ByVal e As WebBrowserDocumentCompletedEventArgs)
        Try
            Dim wb As WebBrowser = TryCast(sender, WebBrowser)
            Dim tab As TabPage = If(wb Is Nothing, Nothing, TryCast(wb.Parent, TabPage))
            If tab IsNot Nothing AndAlso suspendedGroupUrls.ContainsKey(tab) AndAlso (wb.Url Is Nothing OrElse wb.Url.ToString = "about:blank") Then
                tab.Text = "[Suspended] " & qfeature6tablabel(suspendedGroupUrls(tab))
            End If
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    Function qfeature6tablabel(ByVal url As String) As String
        Try
            Dim address As Uri = Nothing
            If Uri.TryCreate(url, UriKind.Absolute, address) AndAlso address.Host <> "" Then Return address.Host
            If url.Length > 50 Then Return url.Substring(0, 50) & "..."
            Return url
        Catch
            Return "Restored Tab"
        End Try
    End Function

    ' --- Most-Recently-Used Tab Switcher ---
    Sub q929(ByVal sender As Object, ByVal e As EventArgs)
        Try
            qfeature6prunestate()
            qfeature6addmissingmrutabs()
            If feature6MruTabs.Count < 2 Then
                su.Visible = True
                su.Text = "There is no other recent tab to select"
                Exit Sub
            End If
            Dim lines As New List(Of String)
            Dim shown As Integer = Math.Min(15, feature6MruTabs.Count)
            For i As Integer = 0 To shown - 1
                lines.Add((i + 1).ToString & ". " & qfeature6mrulabel(feature6MruTabs(i)))
            Next i
            If feature6MruTabs.Count > shown Then lines.Add("... " & (feature6MruTabs.Count - shown).ToString & " more tab(s)")
            Dim answer As String = InputBox("Most-recently-used tabs:" & vbCrLf & String.Join(vbCrLf, lines.ToArray) & vbCrLf & vbCrLf & "Enter a number, NEXT, or PREVIOUS:", "Most-Recently-Used Tab Switcher", "NEXT").Trim.ToUpperInvariant
            If answer = "" Then Exit Sub
            If answer = "NEXT" Then
                qfeature6cycletab(1)
            ElseIf answer = "PREVIOUS" Then
                qfeature6cycletab(-1)
            Else
                Dim index As Integer = 0
                If Integer.TryParse(answer, index) = False OrElse index < 1 OrElse index > shown Then
                    su.Visible = True
                    su.Text = "Recent tab selection was not recognized"
                    Exit Sub
                End If
                feature6CycleTabs.Clear()
                qfeature6selectrecenttab(feature6MruTabs(index - 1))
            End If
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q930(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q929(sender, e)
    End Sub

    Sub qfeature6selectedtabchanged(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim selected As TabPage = k.SelectedTab
            If feature6ActiveTab IsNot Nothing AndAlso feature6ActiveTab.IsDisposed = False Then feature6IdleSince(feature6ActiveTab) = Now
            feature6ActiveTab = selected
            If selected IsNot Nothing Then
                feature6IdleSince(selected) = Now
                feature6MruTabs.Remove(selected)
                feature6MruTabs.Insert(0, selected)
                If Not qleaGroupChange AndAlso suspendedGroupUrls.ContainsKey(selected) Then
                    If qfeature6resumetab(selected) Then
                        su.Visible = True
                        su.Text = "Suspended tab resumed"
                    End If
                End If
            End If
            If feature6Cycling = False Then feature6CycleTabs.Clear()
            qfeature6prunestate()
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    Sub qfeature6keydown(ByVal sender As Object, ByVal e As KeyEventArgs)
        Try
            If e.Handled OrElse e.SuppressKeyPress OrElse e.Control = False OrElse e.Alt OrElse e.KeyCode <> Keys.Tab Then Exit Sub
            qfeature6cycletab(If(e.Shift, -1, 1))
            e.Handled = True
            e.SuppressKeyPress = True
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    Sub qfeature6cycletab(ByVal direction As Integer)
        qfeature6prunestate()
        qfeature6addmissingmrutabs()
        If feature6MruTabs.Count < 2 Then Exit Sub
        Dim current As TabPage = k.SelectedTab
        If feature6CycleTabs.Count = 0 OrElse feature6CycleTabs.Contains(current) = False Then
            feature6CycleTabs.Clear()
            For Each tab As TabPage In feature6MruTabs
                feature6CycleTabs.Add(tab)
            Next tab
        End If
        For i As Integer = feature6CycleTabs.Count - 1 To 0 Step -1
            Dim tab As TabPage = feature6CycleTabs(i)
            If tab Is Nothing OrElse tab.IsDisposed OrElse k.TabPages.Contains(tab) = False Then feature6CycleTabs.RemoveAt(i)
        Next i
        If feature6CycleTabs.Count < 2 Then Exit Sub
        Dim index As Integer = feature6CycleTabs.IndexOf(current)
        If index < 0 Then index = 0
        index += If(direction < 0, -1, 1)
        If index < 0 Then index = feature6CycleTabs.Count - 1
        If index >= feature6CycleTabs.Count Then index = 0
        qfeature6selectrecenttab(feature6CycleTabs(index))
    End Sub

    Sub qfeature6selectrecenttab(ByVal tab As TabPage)
        If tab Is Nothing OrElse tab.IsDisposed OrElse k.TabPages.Contains(tab) = False Then Exit Sub
        feature6Cycling = True
        Try
            k.SelectedTab = tab
        Finally
            feature6Cycling = False
        End Try
        qfeature6resumetab(tab)
        su.Visible = True
        su.Text = "Switched to " & qfeature6mrulabel(tab)
    End Sub

    Function qfeature6mrulabel(ByVal tab As TabPage) As String
        Try
            Dim title As String = tab.Text
            If title.StartsWith("[Suspended] ") Then title = title.Substring(12)
            Dim url As String = ""
            If tab.Controls.Count > 0 Then url = qpersistedtaburl(tab, TryCast(tab.Controls(0), WebBrowser))
            Dim address As Uri = Nothing
            Dim domain As String = ""
            If Uri.TryCreate(url, UriKind.Absolute, address) Then domain = address.Host
            If title = "" Then title = If(domain = "", "New Tab", domain)
            If title.Length > 55 Then title = title.Substring(0, 55) & "..."
            Return title & If(domain = "" OrElse title.Equals(domain, StringComparison.OrdinalIgnoreCase), "", " — " & domain)
        Catch
            Return "Tab"
        End Try
    End Function

    Sub qfeature6addmissingmrutabs()
        For Each tab As TabPage In k.TabPages
            If feature6MruTabs.Contains(tab) = False Then feature6MruTabs.Add(tab)
        Next tab
    End Sub

    Sub qfeature6prunestate()
        For Each tab As TabPage In qleaPaused.Keys.ToArray()
            If tab.IsDisposed OrElse Not qalltabs().Contains(tab) Then
                qleaPaused.Remove(tab)
                suspendedGroupUrls.Remove(tab)
            End If
        Next
        For i As Integer = feature6MruTabs.Count - 1 To 0 Step -1
            Dim tab As TabPage = feature6MruTabs(i)
            If tab Is Nothing OrElse tab.IsDisposed OrElse k.TabPages.Contains(tab) = False Then feature6MruTabs.RemoveAt(i)
        Next i
        For Each tab As TabPage In feature6IdleSince.Keys.ToArray
            If tab Is Nothing OrElse tab.IsDisposed OrElse k.TabPages.Contains(tab) = False Then feature6IdleSince.Remove(tab)
        Next tab
        For Each tab As TabPage In feature6LazyTabs.ToArray
            If tab Is Nothing OrElse tab.IsDisposed OrElse k.TabPages.Contains(tab) = False OrElse suspendedGroupUrls.ContainsKey(tab) = False Then feature6LazyTabs.Remove(tab)
        Next tab
        For Each wb As WebBrowser In feature6WiredBrowsers.ToArray
            If wb Is Nothing OrElse wb.IsDisposed Then feature6WiredBrowsers.Remove(wb)
        Next wb
        For Each tab As TabPage In feature6WiredTabs.ToArray
            If tab Is Nothing OrElse tab.IsDisposed OrElse k.TabPages.Contains(tab) = False Then
                If tab IsNot Nothing AndAlso tab.IsDisposed = False Then RemoveHandler tab.ControlAdded, AddressOf qfeature6tabchildadded
                feature6WiredTabs.Remove(tab)
            End If
        Next tab
    End Sub
End Class
