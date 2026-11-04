<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class F7
    Inherits System.Windows.Forms.Form
    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub
    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer
    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(F7))
        Me.x1 = New System.Windows.Forms.ListBox()
        Me.E = New System.Windows.Forms.Button()
        Me.re = New System.Windows.Forms.Button()
        Me.ca = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'x1
        '
        Me.x1.BackColor = System.Drawing.Color.Black
        Me.x1.ForeColor = System.Drawing.Color.White
        Me.x1.FormattingEnabled = True
        Me.x1.Location = New System.Drawing.Point(12, 12)
        Me.x1.Margin = New System.Windows.Forms.Padding(0)
        Me.x1.Name = "x1"
        Me.x1.Size = New System.Drawing.Size(485, 277)
        Me.x1.TabIndex = 0
        '
        'E
        '
        Me.E.BackColor = System.Drawing.Color.Black
        Me.E.BackgroundImage = Global.Firespider_Internet.My.Resources.F2
        Me.E.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.E.ForeColor = System.Drawing.Color.White
        Me.E.Location = New System.Drawing.Point(13, 293)
        Me.E.Name = "E"
        Me.E.Size = New System.Drawing.Size(75, 23)
        Me.E.TabIndex = 1
        Me.E.Text = "Exit"
        Me.E.UseVisualStyleBackColor = False
        '
        're
        '
        Me.re.BackColor = System.Drawing.Color.Black
        Me.re.BackgroundImage = Global.Firespider_Internet.My.Resources.F2
        Me.re.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.re.ForeColor = System.Drawing.Color.White
        Me.re.Location = New System.Drawing.Point(422, 293)
        Me.re.Name = "re"
        Me.re.Size = New System.Drawing.Size(75, 23)
        Me.re.TabIndex = 2
        Me.re.Text = "Remove"
        Me.re.UseVisualStyleBackColor = False
        '
        'ca
        '
        Me.ca.BackColor = System.Drawing.Color.Black
        Me.ca.BackgroundImage = Global.Firespider_Internet.My.Resources.F2
        Me.ca.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.ca.ForeColor = System.Drawing.Color.White
        Me.ca.Location = New System.Drawing.Point(217, 293)
        Me.ca.Name = "ca"
        Me.ca.Size = New System.Drawing.Size(75, 23)
        Me.ca.TabIndex = 3
        Me.ca.Text = "Clear All"
        Me.ca.UseVisualStyleBackColor = False
        '
        'F7
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.BackgroundImage = Global.Firespider_Internet.My.Resources.F2
        Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.ClientSize = New System.Drawing.Size(509, 323)
        Me.Controls.Add(Me.ca)
        Me.Controls.Add(Me.re)
        Me.Controls.Add(Me.E)
        Me.Controls.Add(Me.x1)
        Me.DoubleBuffered = True
        Me.ForeColor = System.Drawing.Color.Black
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "F7"
        Me.Opacity = 0.97R
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Bookmarks"
        Me.TopMost = True
        Me.ResumeLayout(False)

    End Sub
    Public WithEvents x1 As System.Windows.Forms.ListBox
    Friend WithEvents E As System.Windows.Forms.Button
    Friend WithEvents re As System.Windows.Forms.Button
    Friend WithEvents ca As System.Windows.Forms.Button
End Class