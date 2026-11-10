<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class f6
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(f6))
        Me.l0 = New System.Windows.Forms.ListBox()
        Me.z = New System.Windows.Forms.Button()
        Me.b = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'l0
        '
        Me.l0.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.l0.BackColor = System.Drawing.Color.Black
        Me.l0.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.l0.Font = New System.Drawing.Font("Arial", 6.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.l0.ForeColor = System.Drawing.Color.White
        Me.l0.FormattingEnabled = True
        Me.l0.HorizontalScrollbar = True
        Me.l0.ItemHeight = 11
        Me.l0.Location = New System.Drawing.Point(2, 2)
        Me.l0.Name = "l0"
        Me.l0.ScrollAlwaysVisible = True
        Me.l0.Size = New System.Drawing.Size(467, 253)
        Me.l0.TabIndex = 0
        '
        'z
        '
        Me.z.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.z.BackColor = System.Drawing.Color.Black
        Me.z.BackgroundImage = Global.Firespider_Internet.My.Resources.F2
        Me.z.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.z.Font = New System.Drawing.Font("Arial", 6.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.z.ForeColor = System.Drawing.Color.White
        Me.z.Location = New System.Drawing.Point(391, 257)
        Me.z.Name = "z"
        Me.z.Size = New System.Drawing.Size(78, 23)
        Me.z.TabIndex = 1
        Me.z.Text = "Clear History"
        Me.z.UseVisualStyleBackColor = False
        '
        'b
        '
        Me.b.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.b.BackColor = System.Drawing.Color.Black
        Me.b.BackgroundImage = Global.Firespider_Internet.My.Resources.F2
        Me.b.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.b.Font = New System.Drawing.Font("Arial", 6.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.b.ForeColor = System.Drawing.Color.White
        Me.b.Location = New System.Drawing.Point(0, 257)
        Me.b.Name = "b"
        Me.b.Size = New System.Drawing.Size(34, 23)
        Me.b.TabIndex = 2
        Me.b.Text = "Exit"
        Me.b.UseVisualStyleBackColor = False
        '
        'f6
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.BackgroundImage = Global.Firespider_Internet.My.Resources.F2
        Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.ClientSize = New System.Drawing.Size(461, 274)
        Me.ControlBox = False
        Me.Controls.Add(Me.b)
        Me.Controls.Add(Me.z)
        Me.Controls.Add(Me.l0)
        Me.DoubleBuffered = True
        Me.ForeColor = System.Drawing.Color.White
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "f6"
        Me.Opacity = 0.97R
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "History"
        Me.TopMost = True
        Me.ResumeLayout(False)

    End Sub
    Public WithEvents l0 As System.Windows.Forms.ListBox
    Public WithEvents z As System.Windows.Forms.Button
    Public WithEvents b As System.Windows.Forms.Button
End Class