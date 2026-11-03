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

