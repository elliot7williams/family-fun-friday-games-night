Imports System
Imports System.IO
Imports System.Text
Imports System.Collections.Generic

Partial Class j
    Private Function qh(ByVal value As String) As String
        Return System.Security.SecurityElement.Escape(value)
    End Function

    Public Shared Function Main() As Integer
        Try
            Dim fixture = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "access-fixture.html")
            File.WriteAllText(fixture, "<!doctype html><title>&lt;unsafe&gt;</title><main><h1>Test</h1><img><img alt='Named'><a href='x'> </a><a href='x'><span>Named</span></a><a href='x'><img alt='Icon'></a><a>No href</a><input><input type='hidden'><input aria-label='Name'></main>")
            Dim engine As New Global.Lea.Engine.E()
            Dim page = engine.Go(New Uri(fixture))
            Dim report As New j()
            Dim issues As Integer = -1
            Dim html = report.qleaaccesshtml(page, issues)
            Check(issues = 3, "issue count")
            Check(html.Contains("Headings: 1"), "headings")
            Check(html.Contains("Images: 3; missing alt text: 1"), "image alt")
            Check(html.Contains("Links: 3; potentially unlabeled: 1"), "link descendants")
            Check(html.Contains("Inputs: 3; potentially unlabeled: 1"), "input labels")
            Check(html.Contains("Semantic landmarks: 1"), "landmarks")
            Check(html.Contains("&lt;unsafe&gt;") AndAlso Not html.Contains("<unsafe>"), "escaped title")
            Check(html.Contains("Manual checks"), "manual checks retained")
            Dim failed As Boolean = False
            Try
                report.qleaaccesshtml(Nothing, issues)
            Catch ex As InvalidOperationException
                failed = True
            End Try
            Check(failed, "null document guard")
            page.D.C.Add(page.D)
            failed = False
            Try
                report.qleaaccesshtml(page, issues)
            Catch ex As InvalidOperationException
                failed = True
            End Try
            Check(failed, "cycle guard")
            Console.WriteLine("PASS Lea accessibility report: 10 model checks")
            Return 0
        Catch ex As Exception
            Console.Error.WriteLine(ex.ToString())
            Return 1
        End Try
    End Function

    Private Shared Sub Check(ByVal ok As Boolean, ByVal name As String)
        If Not ok Then Throw New Exception(name)
    End Sub
End Class
