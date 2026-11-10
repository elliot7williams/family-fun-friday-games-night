    Private Function qleaaccesshtml(ByVal page As Global.Lea.Engine.P, ByRef issues As Integer) As String
        If page Is Nothing OrElse page.D Is Nothing Then Throw New InvalidOperationException("No Lea document is available")
        Dim nodes As New List(Of Global.Lea.Engine.N)
        Dim seen As New HashSet(Of Global.Lea.Engine.N)
        Dim pending As New Stack(Of Global.Lea.Engine.N)
        pending.Push(page.D)
        Dim queued As Integer = 1
        While pending.Count > 0
            Dim node = pending.Pop()
            If node Is Nothing OrElse Not seen.Add(node) Then Throw New InvalidOperationException("Invalid Lea document tree")
            nodes.Add(node)
            If node.C.Count > 65536 - queued Then Throw New InvalidOperationException("Accessibility report document limit exceeded")
            queued += node.C.Count
            For index As Integer = node.C.Count - 1 To 0 Step -1
                pending.Push(node.C(index))
            Next
        End While

        Dim hasText As New Dictionary(Of Global.Lea.Engine.N, Boolean)
        Dim hasImage As New Dictionary(Of Global.Lea.Engine.N, Boolean)
        Dim headings As Integer = 0, images As Integer = 0, missingAlt As Integer = 0
        Dim links As Integer = 0, unlabeledLinks As Integer = 0, inputs As Integer = 0
        Dim unlabeledInputs As Integer = 0, landmarks As Integer = 0
        ' Reverse preorder computes descendant summaries once, without recursive walks.
        For index As Integer = nodes.Count - 1 To 0 Step -1
            Dim node = nodes(index)
            Dim text As Boolean = node.K = "#text" AndAlso Not String.IsNullOrWhiteSpace(node.X)
            Dim image As Boolean = node.K = "img"
            For Each child In node.C
                text = text OrElse hasText(child)
                image = image OrElse hasImage(child)
            Next
            hasText.Add(node, text)
            hasImage.Add(node, image)
            Select Case node.K
                Case "h1", "h2", "h3", "h4", "h5", "h6"
                    headings += 1
                Case "img"
                    images += 1
                    If node.Av("alt").Trim() = "" Then missingAlt += 1
                Case "a", "area"
                    If node.A.ContainsKey("href") Then
                        links += 1
                        If Not text AndAlso node.Av("title").Trim() = "" AndAlso Not image Then unlabeledLinks += 1
                    End If
                Case "input"
                    inputs += 1
                    If Not String.Equals(node.Av("type"), "hidden", StringComparison.OrdinalIgnoreCase) AndAlso node.Av("title").Trim() = "" AndAlso node.Av("aria-label").Trim() = "" AndAlso node.Av("id").Trim() = "" Then unlabeledInputs += 1
                Case "main", "nav", "header", "footer", "aside"
                    landmarks += 1
            End Select
        Next
        issues = missingAlt + unlabeledLinks + unlabeledInputs + If(headings = 0, 1, 0) + If(landmarks = 0, 1, 0)
        Dim title As String = If(String.IsNullOrEmpty(page.T), "Accessibility Report", page.T)
        Dim address As String = If(page.U Is Nothing, "", page.U.AbsoluteUri)
        Dim html As New StringBuilder()
        html.Append("<!doctype html><html><head><meta charset=""utf-8""><title>Accessibility Inspector</title><style>body{max-width:820px;margin:35px auto;padding:0 24px;font:17px/1.55 Segoe UI,Arial;color:#222}h1{color:#b33}.ok{color:#267326}.warn{color:#a34b00}li{margin:9px 0}</style></head><body><h1>Accessibility Inspector</h1>")
        html.Append("<p><strong>Page:</strong> ").Append(qh(title)).Append("</p><p><a href=""").Append(qh(address)).Append(""">").Append(qh(address)).Append("</a></p>")
        html.Append("<h2>Summary</h2><p class=""").Append(If(issues = 0, "ok", "warn")).Append(""">").Append(issues).Append(" potential issue(s) found.</p><ul>")
        html.Append("<li>Headings: ").Append(headings).Append(If(headings = 0, " - add a logical heading structure.", "")).Append("</li>")
        html.Append("<li>Images: ").Append(images).Append("; missing alt text: ").Append(missingAlt).Append("</li>")
        html.Append("<li>Links: ").Append(links).Append("; potentially unlabeled: ").Append(unlabeledLinks).Append("</li>")
        html.Append("<li>Inputs: ").Append(inputs).Append("; potentially unlabeled: ").Append(unlabeledInputs).Append("</li>")
        html.Append("<li>Semantic landmarks: ").Append(landmarks).Append(If(landmarks = 0, " - consider main, nav, header, footer, or aside regions.", "")).Append("</li></ul>")
        html.Append("<h2>Manual checks</h2><ul><li>Verify text and control color contrast.</li><li>Navigate the page using only Tab, Shift+Tab, Enter, and Space.</li><li>Confirm focus order follows the visual reading order.</li><li>Confirm dynamic status messages are announced appropriately.</li></ul></body></html>")
        Return html.ToString()
    End Function
