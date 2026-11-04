Imports System.IO
Imports System.Net
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading.Tasks
Imports System.Xml

Partial Class j
    Dim q8watchTimer As New System.Windows.Forms.Timer
    Dim q8watchTimerReady As Boolean = False
    Dim q8watchCursor As Integer = 0
    Shared q8watchBusy As Integer = 0
    Friend Event q8watchchanged()

    Sub qfeatures8()
        ot.DropDownItems.Add(qnewitem("Site Data And Privacy Dashboard", AddressOf q935, AddressOf q936))
        bo.DropDownItems.Add(qnewitem("Feed And Page Watchlist", AddressOf q937, AddressOf q938))
        ot.DropDownItems.Add(qnewitem("Address-Bar Alias Manager", AddressOf q939, AddressOf q940))
    End Sub

    Sub qfeature8loaded()
        Try
            q8ensurestorage()
            If q8watchTimerReady = False Then
                q8watchTimerReady = True
                q8watchTimer.Interval = 15000
                AddHandler q8watchTimer.Tick, AddressOf q8watchtick
            End If
            If My.Settings.watchEnabled = "e" AndAlso pv = False Then q8watchTimer.Start()
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub

    Sub qfeature8closing(ByVal finalWindow As Boolean)
        q8watchTimer.Stop()
    End Sub

    Sub q8ensurestorage()
        If My.Settings.watchSources Is Nothing Then My.Settings.watchSources = New System.Collections.Specialized.StringCollection
        If My.Settings.watchItems Is Nothing Then My.Settings.watchItems = New System.Collections.Specialized.StringCollection
        If My.Settings.aliases Is Nothing Then My.Settings.aliases = New System.Collections.Specialized.StringCollection
    End Sub

    ' --- Site Data And Privacy Dashboard ---
    Sub q935(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim dashboard As New Feature8SiteDataForm(Me, q8currentdomain())
            dashboard.Show(Me)
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q936(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q935(sender, e)
    End Sub

    Friend Function q8currentdomain() As String
        Try
            If k.SelectedTab Is Nothing OrElse k.SelectedTab.Controls.Count = 0 Then Return ""
            Dim wb As WebBrowser = TryCast(k.SelectedTab.Controls(0), WebBrowser)
            If wb IsNot Nothing AndAlso wb.Url IsNot Nothing Then Return qdomain(wb.Url.Host)
        Catch ex As Exception
            qlog(ex)
        End Try
        Return ""
    End Function

    Friend Function q8normalizedomain(ByVal value As String) As String
        value = If(value, "").Trim
        If value = "" Then Return ""
        Dim parsed As Uri = Nothing
        If Uri.TryCreate(value, UriKind.Absolute, parsed) = False Then
            If Uri.TryCreate("http://" & value, UriKind.Absolute, parsed) = False Then Return ""
        End If
        If parsed.Host = "" OrElse Uri.CheckHostName(parsed.Host) = UriHostNameType.Unknown Then Return ""
        Return qdomain(parsed.Host)
    End Function

    Function q8hostrelated(ByVal host As String, ByVal domain As String) As Boolean
        host = qdomain(host)
        domain = qdomain(domain)
        Return host <> "" AndAlso domain <> "" AndAlso (host.Equals(domain, StringComparison.OrdinalIgnoreCase) OrElse host.EndsWith("." & domain, StringComparison.OrdinalIgnoreCase))
    End Function

    Function q8ruleapplies(ByVal storedDomain As String, ByVal domain As String) As Boolean
        storedDomain = qdomain(storedDomain)
        domain = qdomain(domain)
        Return storedDomain <> "" AndAlso domain <> "" AndAlso (domain.Equals(storedDomain, StringComparison.OrdinalIgnoreCase) OrElse domain.EndsWith("." & storedDomain, StringComparison.OrdinalIgnoreCase))
    End Function

    Function q8domainexact(ByVal storedDomain As String, ByVal domain As String) As Boolean
        Return qdomain(storedDomain).Equals(qdomain(domain), StringComparison.OrdinalIgnoreCase)
    End Function

    Function q8urlrelated(ByVal value As String, ByVal domain As String) As Boolean
        Dim parsed As Uri = Nothing
        Return Uri.TryCreate(If(value, ""), UriKind.Absolute, parsed) AndAlso q8hostrelated(parsed.Host, domain)
    End Function

    Friend Function q8siteprivacysummary(ByVal rawDomain As String) As String
        Dim domain As String = q8normalizedomain(rawDomain)
        If domain = "" Then Return "Enter a valid domain or HTTP/HTTPS address."
        Try
            Dim permissions As String() = qpermissionvalues(domain)
            Dim permissionRecords As Integer = q8sitecategorycount(domain, "permissions")
            Dim exceptionExact As Integer = q8sitecategorycount(domain, "exceptions")
            Dim exceptionApplies As Boolean = False
            For Each value As String In My.Settings.cfx
                If q8ruleapplies(value, domain) Then exceptionApplies = True
            Next value
            Dim blockedExact As Integer = q8sitecategorycount(domain, "blocked")
            Dim blockedApplies As Boolean = False
            For Each value As String In My.Settings.bs
                If q8ruleapplies(value, domain) Then blockedApplies = True
            Next value
            Dim userRelevant As Integer = 0
            Dim userExact As Integer = 0
            For Each raw As String In My.Settings.us
                Dim fields As String() = raw.Split(New Char() {"|"c}, 4)
                If fields.Length = 4 AndAlso q8ruleapplies(fields(0), domain) Then userRelevant += 1
                If fields.Length = 4 AndAlso q8domainexact(fields(0), domain) Then userExact += 1
            Next raw
            Dim autofillRelevant As Integer = 0
            Dim autofillExact As Integer = 0
            Dim autofillUnavailable As Integer = 0
            For Each raw As String In My.Settings.af
                Try
                    Dim fields As String() = qvaultunprotect(raw).Split("|"c)
                    If fields.Length >= 4 Then
                        Dim profileRelevant As Boolean = False
                        Dim profileExact As Boolean = False
                        For Each allowed As String In fields(2).Split(","c)
                            If q8ruleapplies(allowed, domain) Then profileRelevant = True
                            If q8domainexact(allowed, domain) Then profileExact = True
                        Next allowed
                        If profileRelevant Then autofillRelevant += 1
                        If profileExact Then autofillExact += 1
                    End If
                Catch
                    autofillUnavailable += 1
                End Try
            Next raw
            Dim cookieSummary As String = q8visiblecookies(domain)
            Dim result As New StringBuilder
            result.AppendLine("Site Data And Privacy Dashboard")
            result.AppendLine("Domain: " & domain)
            result.AppendLine()
            result.AppendLine("Document-visible cookies: " & cookieSummary)
            result.AppendLine("  Only cookie names exposed by the current document are shown. HttpOnly, third-party, and other Windows/WinINet cookies may not be visible.")
            result.AppendLine("Saved permissions: " & If(permissionRecords = 0, "no custom record (ASK defaults)", permissionRecords.ToString & " custom record(s)"))
            result.AppendLine("  Camera=" & permissions(0) & "; microphone=" & permissions(1) & "; location=" & permissions(2) & "; clipboard=" & permissions(3) & "; notifications=" & permissions(4))
            result.AppendLine("Content-filter exception: " & If(exceptionApplies, "applies", "does not apply") & "; exact removable record(s): " & exceptionExact.ToString)
            result.AppendLine("Blocked-site rule: " & If(blockedApplies, "applies", "does not apply") & "; exact removable record(s): " & blockedExact.ToString)
            result.AppendLine("User scripts/styles applying here: " & userRelevant.ToString & "; exact removable record(s): " & userExact.ToString)
            result.AppendLine("Domain reading preferences: " & q8sitecategorycount(domain, "preferences").ToString)
            result.AppendLine("Autofill profiles allowed here: " & autofillRelevant.ToString & "; exact removable domain access: " & autofillExact.ToString & If(autofillUnavailable = 0, "", "; encrypted profiles unavailable: " & autofillUnavailable.ToString))
            result.AppendLine("Crash form-recovery fields for this domain/subdomains: " & q8sitecategorycount(domain, "forms").ToString)
            result.AppendLine("Local analytics records for this domain/subdomains: " & q8sitecategorycount(domain, "analytics").ToString)
            result.AppendLine("Bookmarks for this domain/subdomains: " & q8sitecategorycount(domain, "bookmarks").ToString)
            result.AppendLine("Reading-list entries for this domain/subdomains: " & q8sitecategorycount(domain, "reading").ToString)
            result.AppendLine("History entries for this domain/subdomains: " & q8sitecategorycount(domain, "history").ToString)
            result.AppendLine()
            result.AppendLine("Clear/reset actions below affect only the selected Firespider records. This dashboard does not perform or claim complete Windows/WinINet cookie deletion.")
            Return result.ToString
        Catch ex As Exception
            qlog(ex)
            Return "The site-data summary could not be completed."
        End Try
    End Function

    Function q8visiblecookies(ByVal domain As String) As String
        Try
            If k.SelectedTab Is Nothing OrElse k.SelectedTab.Controls.Count = 0 Then Return "not available (no current document)"
            Dim wb As WebBrowser = TryCast(k.SelectedTab.Controls(0), WebBrowser)
            If wb Is Nothing OrElse wb.Url Is Nothing OrElse wb.Document Is Nothing OrElse q8hostrelated(wb.Url.Host, domain) = False Then Return "not available unless a page from this domain is current"
            Dim cookieText As String = If(wb.Document.Cookie, "").Trim
            If cookieText = "" Then Return "none exposed by the current document"
            Dim names As New List(Of String)
            For Each cookie As String In cookieText.Split(";"c)
                Dim split As Integer = cookie.IndexOf("="c)
                Dim name As String = If(split < 0, cookie, cookie.Substring(0, split)).Trim
                If name <> "" AndAlso names.Contains(name) = False Then names.Add(name)
                If names.Count >= 25 Then Exit For
            Next cookie
            Return names.Count.ToString & " visible name(s): " & String.Join(", ", names.ToArray)
        Catch
            Return "not available from the current document"
        End Try
    End Function

    Friend Function q8sitecategorycount(ByVal domain As String, ByVal category As String) As Integer
        Dim count As Integer = 0
        domain = q8normalizedomain(domain)
        If domain = "" Then Return 0
        If category = "permissions" Then
            For Each raw As String In My.Settings.perm
                Dim fields As String() = raw.Split("|"c)
                If fields.Length > 0 AndAlso q8domainexact(fields(0), domain) Then count += 1
            Next raw
        ElseIf category = "exceptions" Then
            For Each raw As String In My.Settings.cfx
                If q8domainexact(raw, domain) Then count += 1
            Next raw
        ElseIf category = "blocked" Then
            For Each raw As String In My.Settings.bs
                If q8domainexact(raw, domain) Then count += 1
            Next raw
        ElseIf category = "usercontent" Then
            For Each raw As String In My.Settings.us
                Dim fields As String() = raw.Split(New Char() {"|"c}, 4)
                If fields.Length = 4 AndAlso q8domainexact(fields(0), domain) Then count += 1
            Next raw
        ElseIf category = "preferences" Then
            For Each raw As String In My.Settings.dp
                Dim fields As String() = raw.Split("|"c)
                If fields.Length > 0 AndAlso q8domainexact(fields(0), domain) Then count += 1
            Next raw
        ElseIf category = "autofill" Then
            For Each raw As String In My.Settings.af
                Try
                    Dim fields As String() = qvaultunprotect(raw).Split("|"c)
                    If fields.Length >= 4 Then
                        For Each allowed As String In fields(2).Split(","c)
                            If q8domainexact(allowed, domain) Then count += 1
                        Next allowed
                    End If
                Catch
                End Try
            Next raw
        ElseIf category = "forms" Then
            For Each raw As String In My.Settings.fr
                Dim fields As String() = raw.Split("|"c)
                If fields.Length = 4 AndAlso q8urlrelated(qunb64(fields(0)), domain) Then count += 1
            Next raw
        ElseIf category = "analytics" Then
            For Each raw As String In My.Settings.analytics
                Dim fields As String() = raw.Split("|"c)
                If fields.Length = 4 AndAlso q8hostrelated(qunb64(fields(1)), domain) Then count += 1
            Next raw
        ElseIf category = "bookmarks" Then
            For Each raw As String In My.Settings.bm
                If q8urlrelated(qbu(raw), domain) Then count += 1
            Next raw
        ElseIf category = "reading" Then
            For Each raw As String In My.Settings.rl
                Dim fields As String() = raw.Split(New Char() {"|"c}, 4)
                If fields.Length = 4 AndAlso q8urlrelated(fields(3), domain) Then count += 1
            Next raw
        ElseIf category = "history" Then
            For i As Integer = 0 To f6.l0.Items.Count - 1
                If q8urlrelated(f6.l0.Items(i).ToString, domain) Then count += 1
            Next i
        End If
        Return count
    End Function

    Friend Function q8clearsitecategory(ByVal rawDomain As String, ByVal category As String, ByVal label As String) As Boolean
        Dim domain As String = q8normalizedomain(rawDomain)
        If domain = "" Then Return False
        Dim count As Integer = q8sitecategorycount(domain, category)
        If count = 0 Then
            su.Visible = True
            su.Text = "No " & label.ToLowerInvariant & " records found for " & domain
            Return False
        End If
        Dim question As String = "Clear/reset " & count.ToString & " " & label.ToLowerInvariant & " record(s) for " & domain & "?" & vbCrLf & vbCrLf &
            "Only this targeted Firespider category will be changed. Windows/WinINet cookies will not be cleared."
        If MessageBox.Show(question, "Confirm Targeted Site Data Clear", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Return False
        Try
            If category = "permissions" Then
                qremovepermission(domain)
            ElseIf category = "exceptions" Then
                For i As Integer = My.Settings.cfx.Count - 1 To 0 Step -1
                    If q8domainexact(My.Settings.cfx(i), domain) Then My.Settings.cfx.RemoveAt(i)
                Next i
            ElseIf category = "blocked" Then
                For i As Integer = My.Settings.bs.Count - 1 To 0 Step -1
                    If q8domainexact(My.Settings.bs(i), domain) Then My.Settings.bs.RemoveAt(i)
                Next i
            ElseIf category = "usercontent" Then
                For i As Integer = My.Settings.us.Count - 1 To 0 Step -1
                    Dim fields As String() = My.Settings.us(i).Split(New Char() {"|"c}, 4)
                    If fields.Length = 4 AndAlso q8domainexact(fields(0), domain) Then My.Settings.us.RemoveAt(i)
                Next i
            ElseIf category = "preferences" Then
                For i As Integer = My.Settings.dp.Count - 1 To 0 Step -1
                    Dim fields As String() = My.Settings.dp(i).Split("|"c)
                    If fields.Length > 0 AndAlso q8domainexact(fields(0), domain) Then My.Settings.dp.RemoveAt(i)
                Next i
            ElseIf category = "autofill" Then
                For i As Integer = 0 To My.Settings.af.Count - 1
                    Try
                        Dim fields As String() = qvaultunprotect(My.Settings.af(i)).Split("|"c)
                        If fields.Length < 4 Then Continue For
                        Dim remaining As New List(Of String)
                        Dim removed As Boolean = False
                        For Each allowed As String In fields(2).Split(","c)
                            If q8domainexact(allowed, domain) Then
                                removed = True
                            ElseIf allowed.Trim <> "" Then
                                remaining.Add(allowed.Trim)
                            End If
                        Next allowed
                        If removed Then
                            fields(2) = String.Join(",", remaining.ToArray)
                            My.Settings.af(i) = qvaultprotect(String.Join("|", fields))
                        End If
                    Catch ex As Exception
                        qlog(ex)
                    End Try
                Next i
            ElseIf category = "forms" Then
                For i As Integer = My.Settings.fr.Count - 1 To 0 Step -1
                    Dim fields As String() = My.Settings.fr(i).Split("|"c)
                    If fields.Length = 4 AndAlso q8urlrelated(qunb64(fields(0)), domain) Then
                        startupFormRecords.Remove(My.Settings.fr(i))
                        My.Settings.fr.RemoveAt(i)
                    End If
                Next i
                For Each value As String In consideredFormUrls.ToArray
                    If q8urlrelated(value, domain) Then consideredFormUrls.Remove(value)
                Next value
            ElseIf category = "analytics" Then
                For i As Integer = My.Settings.analytics.Count - 1 To 0 Step -1
                    Dim fields As String() = My.Settings.analytics(i).Split("|"c)
                    If fields.Length = 4 AndAlso q8hostrelated(qunb64(fields(1)), domain) Then My.Settings.analytics.RemoveAt(i)
                Next i
            ElseIf category = "bookmarks" Then
                For i As Integer = My.Settings.bm.Count - 1 To 0 Step -1
                    If q8urlrelated(qbu(My.Settings.bm(i)), domain) Then My.Settings.bm.RemoveAt(i)
                Next i
                qrb()
            ElseIf category = "reading" Then
                For i As Integer = My.Settings.rl.Count - 1 To 0 Step -1
                    Dim fields As String() = My.Settings.rl(i).Split(New Char() {"|"c}, 4)
                    If fields.Length = 4 AndAlso q8urlrelated(fields(3), domain) Then My.Settings.rl.RemoveAt(i)
                Next i
            ElseIf category = "history" Then
                For i As Integer = f6.l0.Items.Count - 1 To 0 Step -1
                    If q8urlrelated(f6.l0.Items(i).ToString, domain) Then f6.l0.Items.RemoveAt(i)
                Next i
                For i As Integer = c.Items.Count - 1 To 0 Step -1
                    If q8urlrelated(c.Items(i).ToString, domain) Then c.Items.RemoveAt(i)
                Next i
            End If
            My.Settings.Save()
            su.Visible = True
            su.Text = "Cleared " & count.ToString & " Firespider " & label.ToLowerInvariant & " record(s) for " & domain & "; Windows/WinINet cookies were unchanged"
            Return True
        Catch ex As Exception
            qlog(ex)
            su.Visible = True
            su.Text = "The targeted site-data clear could not be completed"
            Return False
        End Try
    End Function

    ' --- Feed And Page Watchlist ---
    Sub q937(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim manager As New Feature8WatchlistForm(Me)
            manager.Show(Me)
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q938(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q937(sender, e)
    End Sub

    Sub q8watchtick(ByVal sender As Object, ByVal e As EventArgs)
        q8watchTimer.Interval = 900000
        q8requestwatchcheck("", False)
    End Sub

    Friend Sub q8setwatchenabled(ByVal enabled As Boolean)
        My.Settings.watchEnabled = If(enabled, "e", "")
        My.Settings.Save()
        If enabled AndAlso pv = False Then
            q8watchTimer.Interval = 5000
            q8watchTimer.Start()
            MessageBox.Show("Watchlist checks contact the saved feed/page sites while Firespider is running. Checks are skipped in private mode.", "Feed And Page Watchlist", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Else
            q8watchTimer.Stop()
        End If
        su.Visible = True
        su.Text = "Feed and page watchlist " & If(enabled, "enabled", "disabled")
        RaiseEvent q8watchchanged()
    End Sub

    Friend Function q8watchsources() As List(Of Feature8WatchSource)
        q8ensurestorage()
        Dim records As New List(Of Feature8WatchSource)
        For i As Integer = 0 To My.Settings.watchSources.Count - 1
            Dim record As Feature8WatchSource = q8parsewatchsource(My.Settings.watchSources(i))
            If record IsNot Nothing Then
                record.SettingIndex = i
                records.Add(record)
            End If
        Next i
        Return records
    End Function

    Friend Function q8watchitems() As List(Of Feature8WatchItem)
        q8ensurestorage()
        Dim records As New List(Of Feature8WatchItem)
        For i As Integer = 0 To My.Settings.watchItems.Count - 1
            Dim record As Feature8WatchItem = q8parsewatchitem(My.Settings.watchItems(i))
            If record IsNot Nothing Then
                record.SettingIndex = i
                records.Add(record)
            End If
        Next i
        Return records
    End Function

    Function q8parsewatchsource(ByVal raw As String) As Feature8WatchSource
        Dim fields As String() = If(raw, "").Split("|"c)
        If (fields.Length <> 8 AndAlso fields.Length <> 9) OrElse (fields(0) <> "v1" AndAlso fields(0) <> "v2") Then Return Nothing
        Dim record As New Feature8WatchSource
        record.Id = qunb64(fields(1))
        record.Kind = fields(2).ToLowerInvariant
        record.Url = qunb64(fields(3))
        record.Title = qunb64(fields(4))
        record.Marker = qunb64(fields(5))
        record.LastCheck = qunb64(fields(6))
        record.Status = qunb64(fields(7))
        If fields.Length = 9 Then record.Workspace = qunb64(fields(8))
        If record.Id = "" OrElse (record.Kind <> "feed" AndAlso record.Kind <> "page") OrElse q8httpurl(record.Url) = False Then Return Nothing
        Return record
    End Function

    Function q8serializewatchsource(ByVal record As Feature8WatchSource) As String
        Return "v2|" & qb64(record.Id) & "|" & record.Kind & "|" & qb64(record.Url) & "|" & qb64(record.Title) & "|" & qb64(record.Marker) & "|" & qb64(record.LastCheck) & "|" & qb64(record.Status) & "|" & qb64(record.Workspace)
    End Function

    Function q8parsewatchitem(ByVal raw As String) As Feature8WatchItem
        Dim fields As String() = If(raw, "").Split("|"c)
        If fields.Length <> 9 OrElse fields(0) <> "v1" Then Return Nothing
        Dim record As New Feature8WatchItem
        record.SourceId = qunb64(fields(1))
        record.ItemId = qunb64(fields(2))
        record.Url = qunb64(fields(3))
        record.Title = qunb64(fields(4))
        record.Published = qunb64(fields(5))
        record.Unread = fields(6) = "u"
        record.Discovered = qunb64(fields(7))
        record.Kind = qunb64(fields(8))
        If record.SourceId = "" OrElse record.ItemId = "" OrElse q8httpurl(record.Url) = False Then Return Nothing
        Return record
    End Function

    Function q8serializewatchitem(ByVal record As Feature8WatchItem) As String
        Return "v1|" & qb64(record.SourceId) & "|" & qb64(record.ItemId) & "|" & qb64(record.Url) & "|" & qb64(record.Title) & "|" & qb64(record.Published) & "|" & If(record.Unread, "u", "r") & "|" & qb64(record.Discovered) & "|" & qb64(record.Kind)
    End Function

    Function q8httpurl(ByVal value As String) As Boolean
        Dim parsed As Uri = Nothing
        Return Uri.TryCreate(If(value, ""), UriKind.Absolute, parsed) AndAlso (parsed.Scheme = Uri.UriSchemeHttp OrElse parsed.Scheme = Uri.UriSchemeHttps) AndAlso parsed.UserInfo = ""
    End Function

    Friend Function q8addwatchsource(ByVal url As String, ByVal kind As String, ByVal title As String) As String
        url = If(url, "").Trim
        kind = If(kind, "").Trim.ToLowerInvariant
        If kind <> "feed" AndAlso kind <> "page" Then Return "Choose FEED or PAGE."
        Dim parsed As Uri = Nothing
        If Uri.TryCreate(url, UriKind.Absolute, parsed) = False OrElse q8httpurl(url) = False Then Return "Enter an HTTP or HTTPS address without embedded credentials."
        Dim builder As New UriBuilder(parsed)
        builder.Fragment = ""
        url = builder.Uri.AbsoluteUri
        For Each existing As Feature8WatchSource In q8watchsources()
            If existing.Url.Equals(url, StringComparison.OrdinalIgnoreCase) Then Return "That address is already watched."
        Next existing
        If My.Settings.watchSources.Count >= 100 Then Return "The watch-source limit of 100 has been reached."
        title = If(title, "").Trim
        If title = "" Then title = parsed.Host
        If title.Length > 200 Then title = title.Substring(0, 200)
        Dim record As New Feature8WatchSource
        record.Id = q8sha256(url.ToLowerInvariant)
        record.Kind = kind
        record.Url = url
        record.Title = title
        record.Marker = ""
        record.LastCheck = ""
        record.Status = "Not checked"
        record.Workspace = ""
        My.Settings.watchSources.Add(q8serializewatchsource(record))
        My.Settings.Save()
        RaiseEvent q8watchchanged()
        su.Visible = True
        su.Text = title & " added to the watchlist"
        Return ""
    End Function

    Friend Function q8discoverfeeds() As List(Of Feature8DiscoveredFeed)
        Dim feeds As New List(Of Feature8DiscoveredFeed)
        Try
            If k.SelectedTab Is Nothing OrElse k.SelectedTab.Controls.Count = 0 Then Return feeds
            Dim wb As WebBrowser = TryCast(k.SelectedTab.Controls(0), WebBrowser)
            If wb Is Nothing OrElse wb.Url Is Nothing OrElse wb.Document Is Nothing Then Return feeds
            Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            For Each element As HtmlElement In wb.Document.GetElementsByTagName("link")
                Dim rel As String = element.GetAttribute("rel").ToLowerInvariant
                Dim contentType As String = element.GetAttribute("type").ToLowerInvariant
                If rel.Contains("alternate") = False OrElse (contentType.Contains("rss") = False AndAlso contentType.Contains("atom") = False) Then Continue For
                Dim href As String = element.GetAttribute("href").Trim
                Dim target As Uri = Nothing
                If Uri.TryCreate(href, UriKind.Absolute, target) = False Then Uri.TryCreate(wb.Url, href, target)
                If target Is Nothing OrElse q8httpurl(target.AbsoluteUri) = False OrElse seen.Add(target.AbsoluteUri) = False Then Continue For
                Dim title As String = element.GetAttribute("title").Trim
                If title = "" Then title = target.Host & " feed"
                feeds.Add(New Feature8DiscoveredFeed(title, target.AbsoluteUri))
            Next element
        Catch ex As Exception
            qlog(ex)
        End Try
        Return feeds
    End Function

    Friend Sub q8removewatchsource(ByVal sourceId As String)
        Dim source As Feature8WatchSource = q8watchsources().FirstOrDefault(Function(value As Feature8WatchSource) value.Id = sourceId)
        If source Is Nothing Then Exit Sub
        If MessageBox.Show("Remove " & source.Title & " and its stored watch items?", "Remove Watch Source", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Exit Sub
        For i As Integer = My.Settings.watchSources.Count - 1 To 0 Step -1
            Dim current As Feature8WatchSource = q8parsewatchsource(My.Settings.watchSources(i))
            If current IsNot Nothing AndAlso current.Id = sourceId Then My.Settings.watchSources.RemoveAt(i)
        Next i
        For i As Integer = My.Settings.watchItems.Count - 1 To 0 Step -1
            Dim item As Feature8WatchItem = q8parsewatchitem(My.Settings.watchItems(i))
            If item IsNot Nothing AndAlso item.SourceId = sourceId Then My.Settings.watchItems.RemoveAt(i)
        Next i
        My.Settings.Save()
        RaiseEvent q8watchchanged()
    End Sub

    Friend Sub q8assignwatchworkspace(ByVal sourceId As String)
        Dim names As List(Of String) = qworkspacenames()
        Dim answer As String = InputBox("Enter a workspace name, or NONE to remove the assignment:" & vbCrLf & If(names.Count = 0, "No workspaces are currently saved.", String.Join(", ", names.ToArray)), "Assign Watch Source Workspace", "NONE").Trim
        If answer = "" Then Exit Sub
        Dim workspace As String = ""
        If answer.Equals("NONE", StringComparison.OrdinalIgnoreCase) = False Then
            workspace = names.FirstOrDefault(Function(value) value.Equals(answer, StringComparison.OrdinalIgnoreCase))
            If workspace Is Nothing Then
                MessageBox.Show("That workspace does not exist.", "Assign Watch Source Workspace", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Exit Sub
            End If
        End If
        For i As Integer = 0 To My.Settings.watchSources.Count - 1
            Dim source As Feature8WatchSource = q8parsewatchsource(My.Settings.watchSources(i))
            If source IsNot Nothing AndAlso source.Id = sourceId Then
                source.Workspace = workspace
                My.Settings.watchSources(i) = q8serializewatchsource(source)
                My.Settings.Save()
                RaiseEvent q8watchchanged()
                su.Visible = True
                su.Text = If(workspace = "", "Watch source workspace assignment removed", "Watch source assigned to " & workspace)
                Exit Sub
            End If
        Next i
    End Sub

    Friend Async Sub q8requestwatchcheck(ByVal sourceId As String, ByVal manual As Boolean)
        If pv Then
            su.Visible = True
            su.Text = "Watchlist check skipped in private mode"
            Return
        End If
        If manual = False AndAlso My.Settings.watchEnabled <> "e" Then Return
        If System.Threading.Interlocked.CompareExchange(q8watchBusy, 1, 0) <> 0 Then
            If manual Then
                su.Visible = True
                su.Text = "A watchlist check is already running"
            End If
            Return
        End If
        q8watchTimer.Stop()
        Try
            Dim allSources As List(Of Feature8WatchSource) = q8watchsources()
            Dim batch As New List(Of Feature8WatchSource)
            If sourceId <> "" Then
                For Each source As Feature8WatchSource In allSources
                    If source.Id = sourceId Then batch.Add(source) : Exit For
                Next source
            ElseIf allSources.Count > 0 Then
                Dim maximum As Integer = Math.Min(5, allSources.Count)
                Dim start As Integer = q8watchCursor Mod allSources.Count
                For offset As Integer = 0 To maximum - 1
                    batch.Add(allSources((start + offset) Mod allSources.Count))
                Next offset
                q8watchCursor = (start + batch.Count) Mod allSources.Count
            End If
            If batch.Count = 0 Then
                If manual Then
                    su.Visible = True
                    su.Text = "The watchlist has no source to check"
                End If
                Return
            End If
            su.Visible = True
            su.Text = "Checking " & batch.Count.ToString & " watch source(s) sequentially..."
            Dim results As List(Of Feature8WatchCheckResult) = Await Task.Run(Function() As List(Of Feature8WatchCheckResult)
                                                                                 Return q8checkwatchbatch(batch)
                                                                             End Function)
            Dim newItems As Integer = q8applywatchresults(results)
            Dim failures As Integer = results.Where(Function(value As Feature8WatchCheckResult) value.ErrorText <> "").Count
            su.Visible = True
            su.Text = "Watchlist check complete: " & newItems.ToString & " new item(s), " & failures.ToString & " failure(s)"
            RaiseEvent q8watchchanged()
        Catch ex As Exception
            qlog(ex)
            su.Visible = True
            su.Text = "Watchlist check failed"
        Finally
            System.Threading.Interlocked.Exchange(q8watchBusy, 0)
            If My.Settings.watchEnabled = "e" AndAlso IsDisposed = False Then
                q8watchTimer.Interval = 900000
                q8watchTimer.Start()
            End If
        End Try
    End Sub

    Function q8checkwatchbatch(ByVal sources As List(Of Feature8WatchSource)) As List(Of Feature8WatchCheckResult)
        Dim results As New List(Of Feature8WatchCheckResult)
        For Each source As Feature8WatchSource In sources
            results.Add(q8checkwatchsource(source))
        Next source
        Return results
    End Function

    Function q8checkwatchsource(ByVal source As Feature8WatchSource) As Feature8WatchCheckResult
        Dim result As New Feature8WatchCheckResult
        result.SourceId = source.Id
        result.Title = source.Title
        result.NewMarker = source.Marker
        result.LastCheck = DateTime.Now.ToString("o")
        Try
            Dim finalUrl As String = source.Url
            Dim text As String = q8watchdownload(source.Url, finalUrl)
            If source.Kind = "feed" Then
                Dim feedTitle As String = ""
                Dim entries As List(Of Feature8WatchEntry) = Nothing
                If q8parsefeed(text, finalUrl, feedTitle, entries) = False Then Throw New InvalidDataException("The response was not a readable RSS or Atom feed.")
                If feedTitle <> "" Then result.Title = feedTitle
                If entries.Count > 0 Then result.NewMarker = entries(0).Id
                For i As Integer = 0 To Math.Min(entries.Count, 20) - 1
                    result.Entries.Add(entries(i))
                Next i
                result.Status = "Feed checked"
            Else
                Dim normalized As String = q8normalizepagetext(text)
                Dim pageHash As String = q8sha256(normalized)
                If source.Marker = "" Then
                    result.NewMarker = pageHash
                    result.Status = "Page baseline saved"
                ElseIf source.Marker.Equals(pageHash, StringComparison.OrdinalIgnoreCase) Then
                    result.Status = "Page unchanged"
                Else
                    result.NewMarker = pageHash
                    result.Status = "Page changed"
                    Dim pageTitle As String = q8htmltitle(text)
                    If pageTitle = "" Then pageTitle = source.Title
                    result.Entries.Add(New Feature8WatchEntry(pageHash, source.Url, pageTitle & " changed", DateTime.Now.ToString("o"), "page"))
                End If
            End If
        Catch ex As Exception
            result.ErrorText = q8shortstatus(ex.Message)
            result.Status = "Check failed: " & result.ErrorText
        End Try
        Return result
    End Function

    Function q8watchdownload(ByVal url As String, ByRef finalUrl As String) As String
        Dim request As HttpWebRequest = CType(WebRequest.Create(url), HttpWebRequest)
        request.Method = "GET"
        request.Timeout = 7000
        request.ReadWriteTimeout = 7000
        request.MaximumAutomaticRedirections = 3
        request.AllowAutoRedirect = True
        request.AutomaticDecompression = DecompressionMethods.GZip Or DecompressionMethods.Deflate
        request.UserAgent = "Firespider Internet Watchlist/1.0 (.NET Framework WebBrowser companion)"
        request.Accept = "application/atom+xml, application/rss+xml, application/xml, text/xml, text/html;q=0.8, */*;q=0.5"
        request.Headers(HttpRequestHeader.CacheControl) = "no-cache"
        Using response As HttpWebResponse = CType(request.GetResponse(), HttpWebResponse)
            finalUrl = response.ResponseUri.AbsoluteUri
            If response.ContentLength > 2000000 Then Throw New InvalidDataException("The response exceeds the 2 MB watch limit.")
            Dim responseEncoding As Encoding = Encoding.UTF8
            Try
                If response.CharacterSet <> "" Then responseEncoding = Encoding.GetEncoding(response.CharacterSet)
            Catch
            End Try
            Using stream As Stream = response.GetResponseStream()
                Using reader As New StreamReader(stream, responseEncoding, True)
                    Dim data As New StringBuilder
                    Dim buffer(4095) As Char
                    Do
                        Dim read As Integer = reader.Read(buffer, 0, buffer.Length)
                        If read <= 0 Then Exit Do
                        If data.Length + read > 2000000 Then Throw New InvalidDataException("The response exceeds the 2 MB watch limit.")
                        data.Append(buffer, 0, read)
                    Loop
                    Return data.ToString
                End Using
            End Using
        End Using
    End Function

    Function q8parsefeed(ByVal text As String, ByVal baseUrl As String, ByRef feedTitle As String, ByRef entries As List(Of Feature8WatchEntry)) As Boolean
        entries = New List(Of Feature8WatchEntry)
        Try
            Dim settings As New XmlReaderSettings
            settings.DtdProcessing = DtdProcessing.Prohibit
            settings.XmlResolver = Nothing
            settings.MaxCharactersInDocument = 2000000
            Dim document As New XmlDocument
            document.XmlResolver = Nothing
            Using reader As XmlReader = XmlReader.Create(New StringReader(text), settings)
                document.Load(reader)
            End Using
            If document.DocumentElement Is Nothing Then Return False
            Dim rootName As String = document.DocumentElement.LocalName.ToLowerInvariant
            Dim nodes As XmlNodeList = document.SelectNodes("//*[local-name()='item' or local-name()='entry']")
            If rootName <> "rss" AndAlso rootName <> "feed" AndAlso rootName <> "rdf" AndAlso nodes.Count = 0 Then Return False
            Dim titleNode As XmlNode = document.SelectSingleNode("//*[local-name()='channel']/*[local-name()='title']")
            If titleNode Is Nothing AndAlso rootName = "feed" Then titleNode = q8xmlchild(document.DocumentElement, New String() {"title"})
            If titleNode IsNot Nothing Then feedTitle = q8cleantext(titleNode.InnerText, 200)
            Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            For Each node As XmlNode In nodes
                Dim title As String = q8xmltext(node, New String() {"title"})
                Dim link As String = q8xmllink(node)
                If link <> "" Then
                    Dim absolute As Uri = Nothing
                    If Uri.TryCreate(link, UriKind.Absolute, absolute) = False Then Uri.TryCreate(New Uri(baseUrl), link, absolute)
                    If absolute IsNot Nothing Then link = absolute.AbsoluteUri
                End If
                If q8httpurl(link) = False Then link = baseUrl
                Dim published As String = q8xmltext(node, New String() {"pubDate", "published", "updated", "date"})
                Dim identity As String = q8xmltext(node, New String() {"guid", "id"})
                If identity = "" Then identity = link
                If identity = "" OrElse identity.Equals(baseUrl, StringComparison.OrdinalIgnoreCase) Then identity = q8sha256(title & "|" & published & "|" & link)
                If seen.Add(identity) = False Then Continue For
                If title = "" Then title = link
                entries.Add(New Feature8WatchEntry(identity, link, q8cleantext(title, 300), q8cleantext(published, 100), "feed"))
                If entries.Count >= 50 Then Exit For
            Next node
            Return True
        Catch
            Return False
        End Try
    End Function

    Function q8xmlchild(ByVal node As XmlNode, ByVal names As String()) As XmlNode
        If node Is Nothing Then Return Nothing
        For Each child As XmlNode In node.ChildNodes
            For Each name As String In names
                If child.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase) Then Return child
            Next name
        Next child
        Return Nothing
    End Function

    Function q8xmltext(ByVal node As XmlNode, ByVal names As String()) As String
        Dim child As XmlNode = q8xmlchild(node, names)
        Return If(child Is Nothing, "", child.InnerText.Trim)
    End Function

    Function q8xmllink(ByVal node As XmlNode) As String
        If node Is Nothing Then Return ""
        For Each child As XmlNode In node.ChildNodes
            If child.LocalName.Equals("link", StringComparison.OrdinalIgnoreCase) = False Then Continue For
            If child.Attributes IsNot Nothing AndAlso child.Attributes("href") IsNot Nothing Then
                Dim rel As String = ""
                If child.Attributes("rel") IsNot Nothing Then rel = child.Attributes("rel").Value
                If rel = "" OrElse rel.Equals("alternate", StringComparison.OrdinalIgnoreCase) Then Return child.Attributes("href").Value.Trim
            ElseIf child.InnerText.Trim <> "" Then
                Return child.InnerText.Trim
            End If
        Next child
        Return ""
    End Function

    Function q8normalizepagetext(ByVal html As String) As String
        Dim value As String = Regex.Replace(If(html, ""), "(?is)<(script|style|noscript)[^>]*>.*?</\1>", " ")
        value = Regex.Replace(value, "(?is)<!--.*?-->", " ")
        value = Regex.Replace(value, "(?s)<[^>]+>", " ")
        value = WebUtility.HtmlDecode(value)
        value = Regex.Replace(value, "\s+", " ").Trim
        Return value
    End Function

    Function q8htmltitle(ByVal html As String) As String
        Dim match As Match = Regex.Match(If(html, ""), "(?is)<title[^>]*>(.*?)</title>")
        If match.Success Then Return q8cleantext(WebUtility.HtmlDecode(Regex.Replace(match.Groups(1).Value, "\s+", " ")), 200)
        Return ""
    End Function

    Function q8cleantext(ByVal value As String, ByVal maximum As Integer) As String
        value = Regex.Replace(If(value, ""), "\s+", " ").Trim
        If value.Length > maximum Then value = value.Substring(0, maximum)
        Return value
    End Function

    Function q8shortstatus(ByVal value As String) As String
        value = q8cleantext(value, 160)
        Return If(value = "", "Unknown network or parse error", value)
    End Function

    Function q8sha256(ByVal value As String) As String
        Using hash As SHA256 = SHA256.Create()
            Return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(If(value, "")))).Replace("-", "").ToLowerInvariant
        End Using
    End Function

    Function q8applywatchresults(ByVal results As List(Of Feature8WatchCheckResult)) As Integer
        Dim known As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each item As Feature8WatchItem In q8watchitems()
            known.Add(item.SourceId & "|" & item.ItemId)
        Next item
        Dim added As Integer = 0
        For Each result As Feature8WatchCheckResult In results
            Dim sourceIndex As Integer = -1
            Dim source As Feature8WatchSource = Nothing
            For i As Integer = 0 To My.Settings.watchSources.Count - 1
                Dim candidate As Feature8WatchSource = q8parsewatchsource(My.Settings.watchSources(i))
                If candidate IsNot Nothing AndAlso candidate.Id = result.SourceId Then sourceIndex = i : source = candidate : Exit For
            Next i
            If source Is Nothing Then Continue For
            Dim sourceAdded As Integer = 0
            For i As Integer = result.Entries.Count - 1 To 0 Step -1
                Dim entry As Feature8WatchEntry = result.Entries(i)
                Dim key As String = result.SourceId & "|" & entry.Id
                If known.Add(key) = False OrElse q8httpurl(entry.Url) = False Then Continue For
                Dim item As New Feature8WatchItem
                item.SourceId = result.SourceId
                item.ItemId = entry.Id
                item.Url = entry.Url
                item.Title = entry.Title
                item.Published = entry.Published
                item.Unread = True
                item.Discovered = DateTime.Now.ToString("o")
                item.Kind = entry.Kind
                My.Settings.watchItems.Add(q8serializewatchitem(item))
                added += 1
                sourceAdded += 1
            Next i
            source.Marker = result.NewMarker
            source.LastCheck = result.LastCheck
            If result.Title <> "" Then source.Title = result.Title
            If result.ErrorText <> "" Then
                source.Status = result.Status
            ElseIf sourceAdded > 0 Then
                source.Status = sourceAdded.ToString & " new item(s)"
            Else
                source.Status = result.Status
            End If
            My.Settings.watchSources(sourceIndex) = q8serializewatchsource(source)
        Next result
        While My.Settings.watchItems.Count > 500
            My.Settings.watchItems.RemoveAt(0)
        End While
        My.Settings.Save()
        Return added
    End Function

    Friend Sub q8openwatchitem(ByVal sourceId As String, ByVal itemId As String)
        For i As Integer = 0 To My.Settings.watchItems.Count - 1
            Dim item As Feature8WatchItem = q8parsewatchitem(My.Settings.watchItems(i))
            If item Is Nothing OrElse item.SourceId <> sourceId OrElse item.ItemId <> itemId Then Continue For
            item.Unread = False
            My.Settings.watchItems(i) = q8serializewatchitem(item)
            My.Settings.Save()
            RaiseEvent q8watchchanged()
            q1(item.Url)
            Exit Sub
        Next i
    End Sub

    Friend Sub q8markwatchitemread(ByVal sourceId As String, ByVal itemId As String)
        For i As Integer = 0 To My.Settings.watchItems.Count - 1
            Dim item As Feature8WatchItem = q8parsewatchitem(My.Settings.watchItems(i))
            If item IsNot Nothing AndAlso item.SourceId = sourceId AndAlso item.ItemId = itemId Then
                item.Unread = False
                My.Settings.watchItems(i) = q8serializewatchitem(item)
                My.Settings.Save()
                RaiseEvent q8watchchanged()
                Exit Sub
            End If
        Next i
    End Sub

    Friend Sub q8openunreadwatchitems()
        Dim urls As New List(Of String)
        Dim records As New List(Of Feature8WatchItem)
        Dim seenUrls As New HashSet(Of String)(StringComparer.Ordinal)
        For i As Integer = My.Settings.watchItems.Count - 1 To 0 Step -1
            If My.Settings.watchItems.Count - i > 500 Then Exit For
            Dim item As Feature8WatchItem = q8parsewatchitem(My.Settings.watchItems(i))
            If item Is Nothing OrElse item.Unread = False Then Continue For
            Dim target As String = qleaSessionTarget(item.Url)
            If target = "" Then Continue For
            item.SettingIndex = i
            records.Add(item)
            If urls.Count < 20 AndAlso seenUrls.Add(target) Then urls.Add(target)
        Next i
        If urls.Count = 0 Then
            su.Visible = True
            su.Text = "The watchlist has no supported unread URLs in this batch"
            Exit Sub
        End If
        Dim targets As List(Of String) = qleaTargets(urls)
        Dim opened As Integer = qopenurls(targets)
        Dim accepted As New HashSet(Of String)(targets.Take(opened), StringComparer.Ordinal)
        Dim marked As Integer = 0
        For Each item As Feature8WatchItem In records
            If Not accepted.Contains(qleaSessionTarget(item.Url)) Then Continue For
            item.Unread = False
            My.Settings.watchItems(item.SettingIndex) = q8serializewatchitem(item)
            marked += 1
        Next
        My.Settings.Save()
        RaiseEvent q8watchchanged()
        su.Visible = True
        su.Text = opened.ToString & " Lea watch tab(s) opened; " & marked.ToString & " item(s) marked read"
    End Sub

    ' --- Address-Bar Alias Manager ---
    Sub q939(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim manager As New Feature8AliasManagerForm(Me)
            manager.Show(Me)
        Catch ex As Exception
            qlog(ex)
        End Try
    End Sub
    Sub q940(ByVal sender As Object, ByVal e As EventArgs)
        If My.Settings.t = "e" Then q939(sender, e)
    End Sub

    Friend Function q8aliases() As List(Of Feature8AliasRecord)
        q8ensurestorage()
        Dim records As New List(Of Feature8AliasRecord)
        For i As Integer = 0 To My.Settings.aliases.Count - 1
            Dim record As Feature8AliasRecord = q8parsealias(My.Settings.aliases(i))
            If record IsNot Nothing Then
                record.SettingIndex = i
                If records.Any(Function(value As Feature8AliasRecord) value.Name.Equals(record.Name, StringComparison.OrdinalIgnoreCase)) = False Then records.Add(record)
            End If
        Next i
        Return records.OrderBy(Function(value As Feature8AliasRecord) value.Name).ToList
    End Function

    Function q8parsealias(ByVal raw As String) As Feature8AliasRecord
        Dim fields As String() = If(raw, "").Split("|"c)
        If fields.Length <> 3 OrElse fields(0) <> "v1" Then Return Nothing
        Dim record As New Feature8AliasRecord
        record.Name = qunb64(fields(1)).Trim.ToLowerInvariant
        record.Template = qunb64(fields(2)).Trim
        If q8validaliasname(record.Name) = False OrElse q8validaliastemplate(record.Template) = False Then Return Nothing
        Return record
    End Function

    Function q8serializealias(ByVal record As Feature8AliasRecord) As String
        Return "v1|" & qb64(record.Name.ToLowerInvariant) & "|" & qb64(record.Template)
    End Function

    Function q8validaliasname(ByVal name As String) As Boolean
        Return Regex.IsMatch(If(name, ""), "^[A-Za-z][A-Za-z0-9_-]{0,31}$")
    End Function

    Function q8validaliastemplate(ByVal template As String) As Boolean
        template = If(template, "").Trim
        If template = "" Then Return False
        Dim candidate As String = Regex.Replace(template, "\{query\}", "test", RegexOptions.IgnoreCase)
        Dim parsed As Uri = Nothing
        If Uri.TryCreate(candidate, UriKind.Absolute, parsed) = False Then Return False
        If parsed.Scheme <> Uri.UriSchemeHttp AndAlso parsed.Scheme <> Uri.UriSchemeHttps Then Return False
        If parsed.UserInfo <> "" Then Return False
        Dim extension As String = Path.GetExtension(parsed.AbsolutePath).ToLowerInvariant
        If New String() {".exe", ".msi", ".bat", ".cmd", ".com", ".scr", ".ps1", ".vbs"}.Contains(extension) Then Return False
        Return True
    End Function

    Friend Function q8savealias(ByVal oldName As String, ByVal newName As String, ByVal template As String) As String
        newName = If(newName, "").Trim.ToLowerInvariant
        template = If(template, "").Trim
        If q8validaliasname(newName) = False Then Return "Alias names must start with a letter and contain only letters, numbers, underscores, or hyphens (maximum 32 characters)."
        If q8validaliastemplate(template) = False Then Return "Use a safe HTTP/HTTPS URL template without credentials or executable targets. Search aliases should include {query}."
        Dim replaceIndex As Integer = -1
        For Each record As Feature8AliasRecord In q8aliases()
            If oldName <> "" AndAlso record.Name.Equals(oldName, StringComparison.OrdinalIgnoreCase) Then replaceIndex = record.SettingIndex
            If record.Name.Equals(newName, StringComparison.OrdinalIgnoreCase) AndAlso record.Name.Equals(oldName, StringComparison.OrdinalIgnoreCase) = False Then Return "That alias name already exists."
        Next record
        Dim updated As New Feature8AliasRecord
        updated.Name = newName
        updated.Template = template
        If replaceIndex >= 0 Then My.Settings.aliases(replaceIndex) = q8serializealias(updated) Else My.Settings.aliases.Add(q8serializealias(updated))
        My.Settings.Save()
        Return ""
    End Function

    Friend Sub q8deletealias(ByVal name As String)
        If MessageBox.Show("Delete the alias '" & name & "'?", "Delete Address-Bar Alias", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Exit Sub
        For i As Integer = My.Settings.aliases.Count - 1 To 0 Step -1
            Dim record As Feature8AliasRecord = q8parsealias(My.Settings.aliases(i))
            If record IsNot Nothing AndAlso record.Name.Equals(name, StringComparison.OrdinalIgnoreCase) Then My.Settings.aliases.RemoveAt(i)
        Next i
        My.Settings.Save()
    End Sub

    Friend Function q8exportaliases(ByVal path As String) As Integer
        Dim lines As New List(Of String)
        lines.Add("FIRESPIDER_ALIASES_V1")
        For Each record As Feature8AliasRecord In q8aliases()
            lines.Add(q8serializealias(record))
        Next record
        File.WriteAllLines(path, lines.ToArray, Encoding.UTF8)
        Return lines.Count - 1
    End Function

    Friend Function q8importaliases(ByVal path As String) As String
        Dim lines As String() = File.ReadAllLines(path, Encoding.UTF8)
        If lines.Length = 0 OrElse lines(0).Trim <> "FIRESPIDER_ALIASES_V1" Then Return "The selected file is not a Firespider alias export."
        Dim names As New HashSet(Of String)(q8aliases().Select(Function(value As Feature8AliasRecord) value.Name), StringComparer.OrdinalIgnoreCase)
        Dim imported As Integer = 0
        Dim skipped As Integer = 0
        For i As Integer = 1 To lines.Length - 1
            If lines(i).Trim = "" Then Continue For
            Dim record As Feature8AliasRecord = q8parsealias(lines(i).Trim)
            If record Is Nothing OrElse names.Add(record.Name) = False Then
                skipped += 1
            Else
                My.Settings.aliases.Add(q8serializealias(record))
                imported += 1
            End If
        Next i
        My.Settings.Save()
        Return imported.ToString & " alias(es) imported; " & skipped.ToString & " invalid or duplicate record(s) skipped."
    End Function

    Friend Function qresolvealias(ByVal input As String, ByRef resolved As String) As Boolean
        resolved = ""
        Try
            Dim value As String = If(input, "").Trim
            If value = "" Then Return False
            Dim split As Integer = -1
            For i As Integer = 0 To value.Length - 1
                If Char.IsWhiteSpace(value(i)) Then split = i : Exit For
            Next i
            Dim name As String = If(split < 0, value, value.Substring(0, split)).ToLowerInvariant
            Dim query As String = If(split < 0, "", value.Substring(split + 1).Trim)
            For Each record As Feature8AliasRecord In q8aliases()
                If record.Name.Equals(name, StringComparison.OrdinalIgnoreCase) = False Then Continue For
                Dim hasQuery As Boolean = Regex.IsMatch(record.Template, "\{query\}", RegexOptions.IgnoreCase)
                If hasQuery Then
                    If query = "" Then Return False
                    Dim escaped As String = Uri.EscapeDataString(query)
                    resolved = Regex.Replace(record.Template, "\{query\}", escaped, RegexOptions.IgnoreCase)
                Else
                    If query <> "" Then Return False
                    resolved = record.Template
                End If
                If q8validaliastemplate(resolved) = False Then resolved = "" : Return False
                Return True
            Next record
        Catch ex As Exception
            qlog(ex)
            resolved = ""
        End Try
        Return False
    End Function
End Class

Friend Class Feature8SiteDataForm
    Inherits Form
    Dim ownerForm As j
    Dim domainBox As New TextBox
    Dim summaryBox As New TextBox
    Dim categoryBox As New ComboBox

    Sub New(ByVal owner As j, ByVal initialDomain As String)
        ownerForm = owner
        Text = "Site Data And Privacy Dashboard"
        StartPosition = FormStartPosition.CenterParent
        MinimumSize = New Size(720, 520)
        Size = New Size(820, 620)
        Dim layout As New TableLayoutPanel
        layout.Dock = DockStyle.Fill
        layout.RowCount = 4
        layout.ColumnCount = 1
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 48.0!))
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0!))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 52.0!))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 48.0!))

        Dim top As New FlowLayoutPanel
        top.Dock = DockStyle.Fill
        top.Padding = New Padding(8)
        top.Controls.Add(New Label With {.Text = "Domain or address:", .AutoSize = True, .Margin = New Padding(3, 7, 3, 3)})
        domainBox.Width = 430
        domainBox.Text = initialDomain
        top.Controls.Add(domainBox)
        Dim refreshButton As New Button With {.Text = "Refresh", .AutoSize = True}
        AddHandler refreshButton.Click, AddressOf qrefresh
        top.Controls.Add(refreshButton)

        summaryBox.Dock = DockStyle.Fill
        summaryBox.Multiline = True
        summaryBox.ReadOnly = True
        summaryBox.ScrollBars = ScrollBars.Both
        summaryBox.Font = New Font("Consolas", 9.5!)
        summaryBox.WordWrap = False

        Dim actions As New FlowLayoutPanel
        actions.Dock = DockStyle.Fill
        actions.Padding = New Padding(8)
        categoryBox.DropDownStyle = ComboBoxStyle.DropDownList
        categoryBox.Width = 350
        categoryBox.Items.Add(New Feature8PrivacyCategory("permissions", "Saved Permissions"))
        categoryBox.Items.Add(New Feature8PrivacyCategory("exceptions", "Content-Filter Exception"))
        categoryBox.Items.Add(New Feature8PrivacyCategory("blocked", "Blocked-Site Rule"))
        categoryBox.Items.Add(New Feature8PrivacyCategory("usercontent", "User Scripts And Styles"))
        categoryBox.Items.Add(New Feature8PrivacyCategory("preferences", "Domain Reading Preference"))
        categoryBox.Items.Add(New Feature8PrivacyCategory("autofill", "Autofill Domain Access"))
        categoryBox.Items.Add(New Feature8PrivacyCategory("forms", "Form-Recovery Field"))
        categoryBox.Items.Add(New Feature8PrivacyCategory("analytics", "Local Analytics"))
        categoryBox.Items.Add(New Feature8PrivacyCategory("bookmarks", "Bookmark"))
        categoryBox.Items.Add(New Feature8PrivacyCategory("reading", "Reading-List Entry"))
        categoryBox.Items.Add(New Feature8PrivacyCategory("history", "History Entry"))
        categoryBox.SelectedIndex = 0
        actions.Controls.Add(categoryBox)
        Dim clearButton As New Button With {.Text = "Clear/Reset Selected Category", .AutoSize = True}
        AddHandler clearButton.Click, AddressOf qclear
        actions.Controls.Add(clearButton)

        Dim disclaimer As New Label
        disclaimer.Dock = DockStyle.Fill
        disclaimer.Padding = New Padding(10, 4, 10, 4)
        disclaimer.Text = "Cookie scope note: this view reports only cookies exposed by the current document. It does not claim or perform global Windows/WinINet cookie deletion."
        disclaimer.AutoEllipsis = True

        layout.Controls.Add(top, 0, 0)
        layout.Controls.Add(summaryBox, 0, 1)
        layout.Controls.Add(actions, 0, 2)
        layout.Controls.Add(disclaimer, 0, 3)
        Controls.Add(layout)
        qrefresh(Nothing, EventArgs.Empty)
    End Sub

    Sub qrefresh(ByVal sender As Object, ByVal e As EventArgs)
        Dim domain As String = ownerForm.q8normalizedomain(domainBox.Text)
        If domain <> "" Then domainBox.Text = domain
        summaryBox.Text = ownerForm.q8siteprivacysummary(domainBox.Text)
    End Sub

    Sub qclear(ByVal sender As Object, ByVal e As EventArgs)
        Dim category As Feature8PrivacyCategory = TryCast(categoryBox.SelectedItem, Feature8PrivacyCategory)
        If category Is Nothing Then Exit Sub
        If ownerForm.q8clearsitecategory(domainBox.Text, category.Key, category.Label) Then qrefresh(Nothing, EventArgs.Empty)
    End Sub
End Class

Friend Class Feature8PrivacyCategory
    Public Key As String
    Public Label As String
    Sub New(ByVal categoryKey As String, ByVal categoryLabel As String)
        Key = categoryKey
        Label = categoryLabel
    End Sub
    Public Overrides Function ToString() As String
        Return Label
    End Function
End Class

Friend Class Feature8WatchlistForm
    Inherits Form
    Dim ownerForm As j
    Dim sourceList As New ListBox
    Dim itemList As New ListBox
    Dim statusLabel As New Label

    Sub New(ByVal owner As j)
        ownerForm = owner
        Text = "Feed And Page Watchlist"
        StartPosition = FormStartPosition.CenterParent
        MinimumSize = New Size(820, 500)
        Size = New Size(980, 620)
        statusLabel.Dock = DockStyle.Top
        statusLabel.Height = 42
        statusLabel.Padding = New Padding(8)

        Dim split As New SplitContainer
        split.Dock = DockStyle.Fill
        split.SplitterDistance = 460
        Dim sourceLabel As New Label With {.Text = "Watch sources", .Dock = DockStyle.Top, .Height = 24, .Padding = New Padding(5, 4, 0, 0)}
        sourceList.Dock = DockStyle.Fill
        split.Panel1.Controls.Add(sourceList)
        split.Panel1.Controls.Add(sourceLabel)
        Dim itemLabel As New Label With {.Text = "Unread items", .Dock = DockStyle.Top, .Height = 24, .Padding = New Padding(5, 4, 0, 0)}
        itemList.Dock = DockStyle.Fill
        split.Panel2.Controls.Add(itemList)
        split.Panel2.Controls.Add(itemLabel)

        Dim buttons As New FlowLayoutPanel
        buttons.Dock = DockStyle.Bottom
        buttons.Height = 82
        buttons.Padding = New Padding(6)
        buttons.AutoScroll = True
        qbutton(buttons, "Add Feed/Page", AddressOf qadd)
        qbutton(buttons, "Discover Current Page", AddressOf qdiscover)
        qbutton(buttons, "Enable/Disable", AddressOf qtoggle)
        qbutton(buttons, "Assign Workspace", AddressOf qworkspace)
        qbutton(buttons, "Remove Source", AddressOf qremove)
        qbutton(buttons, "Check Selected", AddressOf qcheckselected)
        qbutton(buttons, "Check Batch", AddressOf qcheckbatch)
        qbutton(buttons, "Open Selected Item", AddressOf qopenitem)
        qbutton(buttons, "Mark Selected Read", AddressOf qmarkread)
        qbutton(buttons, "Open Unread", AddressOf qopenunread)
        qbutton(buttons, "Close", AddressOf qclose)

        Controls.Add(split)
        Controls.Add(buttons)
        Controls.Add(statusLabel)
        AddHandler ownerForm.q8watchchanged, AddressOf qchanged
        AddHandler FormClosed, AddressOf qclosed
        qchanged()
    End Sub

    Sub qbutton(ByVal panel As FlowLayoutPanel, ByVal label As String, ByVal handler As EventHandler)
        Dim button As New Button With {.Text = label, .AutoSize = True}
        AddHandler button.Click, handler
        panel.Controls.Add(button)
    End Sub

    Sub qchanged()
        Dim selectedSourceId As String = ""
        Dim oldSource As Feature8WatchSource = TryCast(sourceList.SelectedItem, Feature8WatchSource)
        If oldSource IsNot Nothing Then selectedSourceId = oldSource.Id
        sourceList.Items.Clear()
        Dim sources As List(Of Feature8WatchSource) = ownerForm.q8watchsources()
        Dim items As List(Of Feature8WatchItem) = ownerForm.q8watchitems()
        For Each source As Feature8WatchSource In sources
            source.UnreadCount = items.Where(Function(value As Feature8WatchItem) value.SourceId = source.Id AndAlso value.Unread).Count
            sourceList.Items.Add(source)
            If source.Id = selectedSourceId Then sourceList.SelectedItem = source
        Next source
        itemList.Items.Clear()
        For Each item As Feature8WatchItem In items.Where(Function(value As Feature8WatchItem) value.Unread).OrderByDescending(Function(value As Feature8WatchItem) value.Discovered)
            itemList.Items.Add(item)
        Next item
        statusLabel.Text = "Scheduled checks: " & If(My.Settings.watchEnabled = "e", "enabled while Firespider is running", "disabled") &
            "  |  Sources: " & sources.Count.ToString & "  |  Unread: " & itemList.Items.Count.ToString & "  |  Private mode always skips network checks"
    End Sub

    Sub qclosed(ByVal sender As Object, ByVal e As FormClosedEventArgs)
        RemoveHandler ownerForm.q8watchchanged, AddressOf qchanged
    End Sub

    Sub qadd(ByVal sender As Object, ByVal e As EventArgs)
        Dim url As String = InputBox("Enter the HTTP/HTTPS feed or page address:", "Add Watch Source", "").Trim
        If url = "" Then Exit Sub
        Dim kind As String = InputBox("Enter FEED for RSS/Atom or PAGE for normalized page-text change detection:", "Watch Source Type", "FEED").Trim.ToLowerInvariant
        If kind = "" Then Exit Sub
        Dim title As String = InputBox("Enter a display name (optional):", "Watch Source Name", "").Trim
        Dim problem As String = ownerForm.q8addwatchsource(url, kind, title)
        If problem <> "" Then MessageBox.Show(problem, "Add Watch Source", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        qchanged()
    End Sub

    Sub qdiscover(ByVal sender As Object, ByVal e As EventArgs)
        Dim feeds As List(Of Feature8DiscoveredFeed) = ownerForm.q8discoverfeeds()
        If feeds.Count = 0 Then
            MessageBox.Show("No RSS/Atom <link> discovery entries were found in the current document. You can still add the feed address manually.", "Discover Feeds", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Exit Sub
        End If
        Dim lines As New List(Of String)
        For i As Integer = 0 To feeds.Count - 1
            lines.Add((i + 1).ToString & ". " & feeds(i).Title & " — " & feeds(i).Url)
        Next i
        Dim answer As String = InputBox(String.Join(vbCrLf, lines.ToArray) & vbCrLf & vbCrLf & "Enter a number, comma-separated numbers, or ALL:", "Discovered RSS/Atom Feeds", If(feeds.Count = 1, "1", "ALL")).Trim
        If answer = "" Then Exit Sub
        Dim indexes As New HashSet(Of Integer)
        If answer.Equals("ALL", StringComparison.OrdinalIgnoreCase) Then
            For i As Integer = 0 To feeds.Count - 1
                indexes.Add(i)
            Next i
        Else
            For Each token As String In answer.Split(","c)
                Dim index As Integer = 0
                If Integer.TryParse(token.Trim, index) AndAlso index >= 1 AndAlso index <= feeds.Count Then indexes.Add(index - 1)
            Next token
        End If
        Dim messages As New List(Of String)
        For Each index As Integer In indexes
            Dim problem As String = ownerForm.q8addwatchsource(feeds(index).Url, "feed", feeds(index).Title)
            If problem <> "" Then messages.Add(feeds(index).Title & ": " & problem)
        Next index
        If messages.Count > 0 Then MessageBox.Show(String.Join(vbCrLf, messages.ToArray), "Discovered Feeds", MessageBoxButtons.OK, MessageBoxIcon.Information)
        qchanged()
    End Sub

    Sub qtoggle(ByVal sender As Object, ByVal e As EventArgs)
        ownerForm.q8setwatchenabled(My.Settings.watchEnabled <> "e")
        qchanged()
    End Sub

    Sub qremove(ByVal sender As Object, ByVal e As EventArgs)
        Dim source As Feature8WatchSource = TryCast(sourceList.SelectedItem, Feature8WatchSource)
        If source Is Nothing Then Exit Sub
        ownerForm.q8removewatchsource(source.Id)
        qchanged()
    End Sub

    Sub qworkspace(ByVal sender As Object, ByVal e As EventArgs)
        Dim source As Feature8WatchSource = TryCast(sourceList.SelectedItem, Feature8WatchSource)
        If source Is Nothing Then Exit Sub
        ownerForm.q8assignwatchworkspace(source.Id)
        qchanged()
    End Sub

    Sub qcheckselected(ByVal sender As Object, ByVal e As EventArgs)
        Dim source As Feature8WatchSource = TryCast(sourceList.SelectedItem, Feature8WatchSource)
        If source Is Nothing Then Exit Sub
        ownerForm.q8requestwatchcheck(source.Id, True)
    End Sub

    Sub qcheckbatch(ByVal sender As Object, ByVal e As EventArgs)
        ownerForm.q8requestwatchcheck("", True)
    End Sub

    Sub qopenitem(ByVal sender As Object, ByVal e As EventArgs)
        Dim item As Feature8WatchItem = TryCast(itemList.SelectedItem, Feature8WatchItem)
        If item IsNot Nothing Then ownerForm.q8openwatchitem(item.SourceId, item.ItemId)
    End Sub

    Sub qmarkread(ByVal sender As Object, ByVal e As EventArgs)
        Dim item As Feature8WatchItem = TryCast(itemList.SelectedItem, Feature8WatchItem)
        If item IsNot Nothing Then ownerForm.q8markwatchitemread(item.SourceId, item.ItemId)
    End Sub

    Sub qopenunread(ByVal sender As Object, ByVal e As EventArgs)
        ownerForm.q8openunreadwatchitems()
    End Sub

    Sub qclose(ByVal sender As Object, ByVal e As EventArgs)
        Close()
    End Sub
End Class

Friend Class Feature8WatchSource
    Public Id As String
    Public Kind As String
    Public Url As String
    Public Title As String
    Public Marker As String
    Public LastCheck As String
    Public Status As String
    Public Workspace As String
    Public SettingIndex As Integer
    Public UnreadCount As Integer
    Public Overrides Function ToString() As String
        Dim checked As String = If(LastCheck = "", "never", LastCheck)
        Dim parsed As DateTime
        If DateTime.TryParse(LastCheck, parsed) Then checked = parsed.ToString("yyyy-MM-dd HH:mm")
        Return "[" & Kind.ToUpperInvariant & "] " & Title & "  |  unread " & UnreadCount.ToString & "  |  " & Status & If(Workspace = "", "", "  |  workspace " & Workspace) & "  |  last " & checked & "  |  " & Url
    End Function
End Class

Friend Class Feature8WatchItem
    Public SourceId As String
    Public ItemId As String
    Public Url As String
    Public Title As String
    Public Published As String
    Public Unread As Boolean
    Public Discovered As String
    Public Kind As String
    Public SettingIndex As Integer
    Public Overrides Function ToString() As String
        Dim dateText As String = If(Published = "", Discovered, Published)
        Return "● " & Title & If(dateText = "", "", " — " & dateText)
    End Function
End Class

Friend Class Feature8WatchEntry
    Public Id As String
    Public Url As String
    Public Title As String
    Public Published As String
    Public Kind As String
    Sub New(ByVal entryId As String, ByVal entryUrl As String, ByVal entryTitle As String, ByVal entryPublished As String, ByVal entryKind As String)
        Id = entryId
        Url = entryUrl
        Title = entryTitle
        Published = entryPublished
        Kind = entryKind
    End Sub
End Class

Friend Class Feature8WatchCheckResult
    Public SourceId As String
    Public Title As String
    Public NewMarker As String
    Public LastCheck As String
    Public Status As String
    Public ErrorText As String = ""
    Public Entries As New List(Of Feature8WatchEntry)
End Class

Friend Class Feature8DiscoveredFeed
    Public Title As String
    Public Url As String
    Sub New(ByVal feedTitle As String, ByVal feedUrl As String)
        Title = feedTitle
        Url = feedUrl
    End Sub
End Class

Friend Class Feature8AliasManagerForm
    Inherits Form
    Dim ownerForm As j
    Dim aliasList As New ListBox

    Sub New(ByVal owner As j)
        ownerForm = owner
        Text = "Address-Bar Alias Manager"
        StartPosition = FormStartPosition.CenterParent
        MinimumSize = New Size(680, 430)
        Size = New Size(800, 520)
        Dim note As New Label
        note.Dock = DockStyle.Top
        note.Height = 52
        note.Padding = New Padding(8)
        note.Text = "Examples: w cats with https://en.wikipedia.org/wiki/Special:Search?search={query}; or an exact alias with a fixed HTTP/HTTPS address. Queries are escaped before navigation."
        aliasList.Dock = DockStyle.Fill
        Dim buttons As New FlowLayoutPanel
        buttons.Dock = DockStyle.Bottom
        buttons.Height = 48
        buttons.Padding = New Padding(6)
        qbutton(buttons, "Add", AddressOf qadd)
        qbutton(buttons, "Edit", AddressOf qedit)
        qbutton(buttons, "Delete", AddressOf qdelete)
        qbutton(buttons, "Test", AddressOf qtest)
        qbutton(buttons, "Import", AddressOf qimport)
        qbutton(buttons, "Export", AddressOf qexport)
        qbutton(buttons, "Close", AddressOf qclose)
        Controls.Add(aliasList)
        Controls.Add(buttons)
        Controls.Add(note)
        qrefresh()
    End Sub

    Sub qbutton(ByVal panel As FlowLayoutPanel, ByVal label As String, ByVal handler As EventHandler)
        Dim button As New Button With {.Text = label, .AutoSize = True}
        AddHandler button.Click, handler
        panel.Controls.Add(button)
    End Sub

    Sub qrefresh()
        aliasList.Items.Clear()
        For Each record As Feature8AliasRecord In ownerForm.q8aliases()
            aliasList.Items.Add(record)
        Next record
    End Sub

    Sub qadd(ByVal sender As Object, ByVal e As EventArgs)
        Dim name As String = InputBox("Enter the short alias name (no spaces):", "Add Address-Bar Alias", "w").Trim
        If name = "" Then Exit Sub
        Dim template As String = InputBox("Enter an HTTP/HTTPS URL template. Include {query} for a search alias:", "Add Address-Bar Alias", "https://en.wikipedia.org/wiki/Special:Search?search={query}").Trim
        If template = "" Then Exit Sub
        Dim problem As String = ownerForm.q8savealias("", name, template)
        If problem <> "" Then MessageBox.Show(problem, "Add Address-Bar Alias", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        qrefresh()
    End Sub

    Sub qedit(ByVal sender As Object, ByVal e As EventArgs)
        Dim record As Feature8AliasRecord = TryCast(aliasList.SelectedItem, Feature8AliasRecord)
        If record Is Nothing Then Exit Sub
        Dim name As String = InputBox("Edit the alias name:", "Edit Address-Bar Alias", record.Name).Trim
        If name = "" Then Exit Sub
        Dim template As String = InputBox("Edit the HTTP/HTTPS URL template:", "Edit Address-Bar Alias", record.Template).Trim
        If template = "" Then Exit Sub
        Dim problem As String = ownerForm.q8savealias(record.Name, name, template)
        If problem <> "" Then MessageBox.Show(problem, "Edit Address-Bar Alias", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        qrefresh()
    End Sub

    Sub qdelete(ByVal sender As Object, ByVal e As EventArgs)
        Dim record As Feature8AliasRecord = TryCast(aliasList.SelectedItem, Feature8AliasRecord)
        If record Is Nothing Then Exit Sub
        ownerForm.q8deletealias(record.Name)
        qrefresh()
    End Sub

    Sub qtest(ByVal sender As Object, ByVal e As EventArgs)
        Dim record As Feature8AliasRecord = TryCast(aliasList.SelectedItem, Feature8AliasRecord)
        Dim example As String = If(record Is Nothing, "w red fox", record.Name & If(Regex.IsMatch(record.Template, "\{query\}", RegexOptions.IgnoreCase), " red fox", ""))
        Dim input As String = InputBox("Enter address-bar text to resolve:", "Test Address-Bar Alias", example).Trim
        If input = "" Then Exit Sub
        Dim resolved As String = ""
        If ownerForm.qresolvealias(input, resolved) = False Then
            MessageBox.Show("No alias resolution was produced. Unknown aliases continue through normal address/search handling.", "Test Address-Bar Alias", MessageBoxButtons.OK, MessageBoxIcon.Information)
        ElseIf MessageBox.Show("Resolved address:" & vbCrLf & resolved & vbCrLf & vbCrLf & "Open it through normal Firespider navigation?", "Test Address-Bar Alias", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            ownerForm.q1(resolved)
        End If
    End Sub

    Sub qimport(ByVal sender As Object, ByVal e As EventArgs)
        Dim dialog As New OpenFileDialog
        dialog.Filter = "Firespider Alias Export [*.txt*]|*.txt|All Files [*.*]|*.*"
        If dialog.ShowDialog(Me) <> DialogResult.OK Then Exit Sub
        Try
            MessageBox.Show(ownerForm.q8importaliases(dialog.FileName), "Import Address-Bar Aliases", MessageBoxButtons.OK, MessageBoxIcon.Information)
            qrefresh()
        Catch ex As Exception
            ownerForm.qlog(ex)
            MessageBox.Show("The alias file could not be imported.", "Import Address-Bar Aliases", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Sub qexport(ByVal sender As Object, ByVal e As EventArgs)
        Dim dialog As New SaveFileDialog
        ownerForm.qsavefolder(dialog)
        dialog.FileName = "Firespider Aliases.txt"
        dialog.Filter = "Firespider Alias Export [*.txt*]|*.txt|All Files [*.*]|*.*"
        If dialog.ShowDialog(Me) <> DialogResult.OK Then Exit Sub
        Try
            Dim count As Integer = ownerForm.q8exportaliases(dialog.FileName)
            MessageBox.Show(count.ToString & " alias(es) exported.", "Export Address-Bar Aliases", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            ownerForm.qlog(ex)
            MessageBox.Show("The aliases could not be exported.", "Export Address-Bar Aliases", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Sub qclose(ByVal sender As Object, ByVal e As EventArgs)
        Close()
    End Sub
End Class

Friend Class Feature8AliasRecord
    Public Name As String
    Public Template As String
    Public SettingIndex As Integer
    Public Overrides Function ToString() As String
        Return Name & "  →  " & Template
    End Function
End Class
