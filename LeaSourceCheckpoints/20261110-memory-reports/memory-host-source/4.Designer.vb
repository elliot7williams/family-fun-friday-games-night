<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class F4
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(F4))
        Me.TB1 = New System.Windows.Forms.TabControl()
        Me.TP1 = New System.Windows.Forms.TabPage()
        Me.L03 = New System.Windows.Forms.Label()
        Me.L02 = New System.Windows.Forms.Label()
        Me.B01 = New System.Windows.Forms.Button()
        Me.CB1 = New System.Windows.Forms.ComboBox()
        Me.L4 = New System.Windows.Forms.Label()
        Me.L3 = New System.Windows.Forms.Label()
        Me.L2 = New System.Windows.Forms.Label()
        Me.HSB2 = New System.Windows.Forms.HScrollBar()
        Me.Search = New System.Windows.Forms.CheckedListBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.F1 = New System.Windows.Forms.Label()
        Me.O2 = New System.Windows.Forms.Label()
        Me.O1 = New System.Windows.Forms.Label()
        Me.HSB1 = New System.Windows.Forms.HScrollBar()
        Me.B1 = New System.Windows.Forms.Button()
        Me.TP2 = New System.Windows.Forms.TabPage()
        Me.TP3 = New System.Windows.Forms.TabPage()
        Me.TP4 = New System.Windows.Forms.TabPage()
        Me.TP5 = New System.Windows.Forms.TabPage()
        Me.TP9 = New System.Windows.Forms.TabPage()
        Me.TP7 = New System.Windows.Forms.TabPage()
        Me.TB1.SuspendLayout()
        Me.TP1.SuspendLayout()
        Me.SuspendLayout()
        '
        'TB1
        '
        Me.TB1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TB1.Controls.Add(Me.TP1)
        Me.TB1.Controls.Add(Me.TP2)
        Me.TB1.Controls.Add(Me.TP3)
        Me.TB1.Controls.Add(Me.TP4)
        Me.TB1.Controls.Add(Me.TP5)
        Me.TB1.Controls.Add(Me.TP9)
        Me.TB1.Controls.Add(Me.TP7)
        Me.TB1.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed
        Me.TB1.ItemSize = New System.Drawing.Size(80, 16)
        Me.TB1.Location = New System.Drawing.Point(0, 0)
        Me.TB1.Margin = New System.Windows.Forms.Padding(0)
        Me.TB1.Multiline = True
        Me.TB1.Name = "TB1"
        Me.TB1.Padding = New System.Drawing.Point(0, 0)
        Me.TB1.SelectedIndex = 0
        Me.TB1.Size = New System.Drawing.Size(500, 451)
        Me.TB1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed
        Me.TB1.TabIndex = 0
        '
        'TP1
        '
        Me.TP1.BackColor = System.Drawing.Color.White
        Me.TP1.BackgroundImage = Global.Firespider_Internet.My.Resources.Resources.F2
        Me.TP1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.TP1.Controls.Add(Me.L03)
        Me.TP1.Controls.Add(Me.L02)
        Me.TP1.Controls.Add(Me.B01)
        Me.TP1.Controls.Add(Me.CB1)
        Me.TP1.Controls.Add(Me.L4)
        Me.TP1.Controls.Add(Me.L3)
        Me.TP1.Controls.Add(Me.L2)
        Me.TP1.Controls.Add(Me.HSB2)
        Me.TP1.Controls.Add(Me.Search)
        Me.TP1.Controls.Add(Me.Label1)
        Me.TP1.Controls.Add(Me.F1)
        Me.TP1.Controls.Add(Me.O2)
        Me.TP1.Controls.Add(Me.O1)
        Me.TP1.Controls.Add(Me.HSB1)
        Me.TP1.Controls.Add(Me.B1)
        Me.TP1.Location = New System.Drawing.Point(4, 36)
        Me.TP1.Margin = New System.Windows.Forms.Padding(0)
        Me.TP1.Name = "TP1"
        Me.TP1.Size = New System.Drawing.Size(492, 411)
        Me.TP1.TabIndex = 2
        Me.TP1.Text = "General"
        '
        'L03
        '
        Me.L03.AutoSize = True
        Me.L03.ForeColor = System.Drawing.Color.Black
        Me.L03.Location = New System.Drawing.Point(38, 36)
        Me.L03.Name = "L03"
        Me.L03.Size = New System.Drawing.Size(117, 13)
        Me.L03.TabIndex = 18
        Me.L03.Text = "Set Web Browser Color"
        '
        'L02
        '
        Me.L02.AutoSize = True
        Me.L02.ForeColor = System.Drawing.Color.Black
        Me.L02.Location = New System.Drawing.Point(59, 109)
        Me.L02.Name = "L02"
        Me.L02.Size = New System.Drawing.Size(82, 13)
        Me.L02.TabIndex = 17
        Me.L02.Text = "Set Home Page"
        '
        'B01
        '
        Me.B01.BackColor = System.Drawing.Color.Black
        Me.B01.BackgroundImage = Global.Firespider_Internet.My.Resources.Resources.F2
        Me.B01.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.B01.Location = New System.Drawing.Point(59, 150)
        Me.B01.Name = "B01"
        Me.B01.Size = New System.Drawing.Size(57, 23)
        Me.B01.TabIndex = 16
        Me.B01.Text = "Set"
        Me.B01.UseVisualStyleBackColor = False
        '
        'CB1
        '
        Me.CB1.FormattingEnabled = True
        Me.CB1.Location = New System.Drawing.Point(59, 125)
        Me.CB1.Margin = New System.Windows.Forms.Padding(0)
        Me.CB1.Name = "CB1"
        Me.CB1.Size = New System.Drawing.Size(379, 21)
        Me.CB1.TabIndex = 15
        '
        'L4
        '
        Me.L4.AutoSize = True
        Me.L4.BackColor = System.Drawing.Color.Transparent
        Me.L4.ForeColor = System.Drawing.Color.White
        Me.L4.Location = New System.Drawing.Point(35, 320)
        Me.L4.Margin = New System.Windows.Forms.Padding(0)
        Me.L4.Name = "L4"
        Me.L4.Size = New System.Drawing.Size(23, 13)
        Me.L4.TabIndex = 14
        Me.L4.Text = "ON"
        '
        'L3
        '
        Me.L3.AutoSize = True
        Me.L3.BackColor = System.Drawing.Color.Transparent
        Me.L3.ForeColor = System.Drawing.Color.Black
        Me.L3.Location = New System.Drawing.Point(180, 320)
        Me.L3.Margin = New System.Windows.Forms.Padding(0)
        Me.L3.Name = "L3"
        Me.L3.Size = New System.Drawing.Size(27, 13)
        Me.L3.TabIndex = 13
        Me.L3.Text = "OFF"
        '
        'L2
        '
        Me.L2.AutoSize = True
        Me.L2.BackColor = System.Drawing.Color.Transparent
        Me.L2.ForeColor = System.Drawing.Color.Black
        Me.L2.Location = New System.Drawing.Point(57, 296)
        Me.L2.Margin = New System.Windows.Forms.Padding(0)
        Me.L2.Name = "L2"
        Me.L2.Size = New System.Drawing.Size(137, 13)
        Me.L2.TabIndex = 12
        Me.L2.Text = "Turn Hover Technology Off"
        '
        'HSB2
        '
        Me.HSB2.LargeChange = 1
        Me.HSB2.Location = New System.Drawing.Point(69, 322)
        Me.HSB2.Maximum = 1
        Me.HSB2.Name = "HSB2"
        Me.HSB2.Size = New System.Drawing.Size(111, 17)
        Me.HSB2.TabIndex = 11
        '
        'Search
        '
        Me.Search.BackColor = System.Drawing.Color.Black
        Me.Search.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.Search.ForeColor = System.Drawing.Color.White
        Me.Search.FormattingEnabled = True
        Me.Search.Items.AddRange(New Object() {"Bing", "Google", "Yahoo", "Ask", "DuckDuckGo"})
        Me.Search.Location = New System.Drawing.Point(358, 322)
        Me.Search.Margin = New System.Windows.Forms.Padding(0)
        Me.Search.Name = "Search"
        Me.Search.Size = New System.Drawing.Size(80, 60)
        Me.Search.TabIndex = 5
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.ForeColor = System.Drawing.Color.Black
        Me.Label1.Location = New System.Drawing.Point(356, 303)
        Me.Label1.Margin = New System.Windows.Forms.Padding(0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(81, 13)
        Me.Label1.TabIndex = 10
        Me.Label1.Text = "Change Search"
        '
        'F1
        '
        Me.F1.AutoSize = True
        Me.F1.BackColor = System.Drawing.Color.Transparent
        Me.F1.ForeColor = System.Drawing.Color.Black
        Me.F1.Location = New System.Drawing.Point(86, 344)
        Me.F1.Margin = New System.Windows.Forms.Padding(0)
        Me.F1.Name = "F1"
        Me.F1.Size = New System.Drawing.Size(73, 13)
        Me.F1.TabIndex = 4
        Me.F1.Text = "Turn Siera Off"
        '
        'O2
        '
        Me.O2.AutoSize = True
        Me.O2.BackColor = System.Drawing.Color.Transparent
        Me.O2.ForeColor = System.Drawing.Color.White
        Me.O2.Location = New System.Drawing.Point(35, 370)
        Me.O2.Margin = New System.Windows.Forms.Padding(0)
        Me.O2.Name = "O2"
        Me.O2.Size = New System.Drawing.Size(23, 13)
        Me.O2.TabIndex = 3
        Me.O2.Text = "ON"
        '
        'O1
        '
        Me.O1.AutoSize = True
        Me.O1.BackColor = System.Drawing.Color.Transparent
        Me.O1.ForeColor = System.Drawing.Color.Black
        Me.O1.Location = New System.Drawing.Point(180, 370)
        Me.O1.Margin = New System.Windows.Forms.Padding(0)
        Me.O1.Name = "O1"
        Me.O1.Size = New System.Drawing.Size(27, 13)
        Me.O1.TabIndex = 2
        Me.O1.Text = "OFF"
        '
        'HSB1
        '
        Me.HSB1.LargeChange = 1
        Me.HSB1.Location = New System.Drawing.Point(69, 369)
        Me.HSB1.Maximum = 1
        Me.HSB1.Name = "HSB1"
        Me.HSB1.Size = New System.Drawing.Size(111, 17)
        Me.HSB1.TabIndex = 1
        '
        'B1
        '
        Me.B1.BackColor = System.Drawing.Color.Black
        Me.B1.BackgroundImage = Global.Firespider_Internet.My.Resources.Resources.F2
        Me.B1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.B1.ForeColor = System.Drawing.Color.White
        Me.B1.Location = New System.Drawing.Point(41, 55)
        Me.B1.Name = "B1"
        Me.B1.Size = New System.Drawing.Size(75, 23)
        Me.B1.TabIndex = 0
        Me.B1.Text = "Change"
        Me.B1.UseVisualStyleBackColor = False
        '
        'TP2
        '
        Me.TP2.BackColor = System.Drawing.Color.White
        Me.TP2.BackgroundImage = Global.Firespider_Internet.My.Resources.Resources.F2
        Me.TP2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.TP2.Location = New System.Drawing.Point(4, 20)
        Me.TP2.Margin = New System.Windows.Forms.Padding(0)
        Me.TP2.Name = "TP2"
        Me.TP2.Size = New System.Drawing.Size(492, 427)
        Me.TP2.TabIndex = 3
        Me.TP2.Text = "Tabs"
        '
        'TP3
        '
        Me.TP3.BackColor = System.Drawing.Color.White
        Me.TP3.BackgroundImage = Global.Firespider_Internet.My.Resources.Resources.F2
        Me.TP3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.TP3.Location = New System.Drawing.Point(4, 20)
        Me.TP3.Margin = New System.Windows.Forms.Padding(0)
        Me.TP3.Name = "TP3"
        Me.TP3.Size = New System.Drawing.Size(492, 427)
        Me.TP3.TabIndex = 4
        Me.TP3.Text = "Content"
        '
        'TP4
        '
        Me.TP4.BackColor = System.Drawing.Color.White
        Me.TP4.BackgroundImage = Global.Firespider_Internet.My.Resources.Resources.F2
        Me.TP4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.TP4.Location = New System.Drawing.Point(4, 20)
        Me.TP4.Margin = New System.Windows.Forms.Padding(0)
        Me.TP4.Name = "TP4"
        Me.TP4.Size = New System.Drawing.Size(492, 427)
        Me.TP4.TabIndex = 5
        Me.TP4.Text = "Applications"
        '
        'TP5
        '
        Me.TP5.BackColor = System.Drawing.Color.White
        Me.TP5.BackgroundImage = Global.Firespider_Internet.My.Resources.Resources.F2
        Me.TP5.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.TP5.Location = New System.Drawing.Point(4, 20)
        Me.TP5.Margin = New System.Windows.Forms.Padding(0)
        Me.TP5.Name = "TP5"
        Me.TP5.Size = New System.Drawing.Size(492, 427)
        Me.TP5.TabIndex = 6
        Me.TP5.Text = "Privacy"
        '
        'TP9
        '
        Me.TP9.BackColor = System.Drawing.Color.White
        Me.TP9.BackgroundImage = Global.Firespider_Internet.My.Resources.Resources.F2
        Me.TP9.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.TP9.Location = New System.Drawing.Point(4, 20)
        Me.TP9.Margin = New System.Windows.Forms.Padding(0)
        Me.TP9.Name = "TP9"
        Me.TP9.Size = New System.Drawing.Size(492, 427)
        Me.TP9.TabIndex = 7
        Me.TP9.Text = "Security"
        '
        'TP7
        '
        Me.TP7.BackColor = System.Drawing.Color.White
        Me.TP7.BackgroundImage = Global.Firespider_Internet.My.Resources.Resources.F2
        Me.TP7.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.TP7.Location = New System.Drawing.Point(4, 36)
        Me.TP7.Name = "TP7"
        Me.TP7.Size = New System.Drawing.Size(492, 411)
        Me.TP7.TabIndex = 8
        Me.TP7.Text = "Themes"
        '
        'F4
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.BackgroundImage = Global.Firespider_Internet.My.Resources.Resources.F2
        Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.ClientSize = New System.Drawing.Size(500, 451)
        Me.Controls.Add(Me.TB1)
        Me.DoubleBuffered = True
        Me.ForeColor = System.Drawing.Color.White
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "F4"
        Me.Opacity = 0.97R
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Options"
        Me.TopMost = True
        Me.TB1.ResumeLayout(False)
        Me.TP1.ResumeLayout(False)
        Me.TP1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Public WithEvents TB1 As System.Windows.Forms.TabControl
    Public WithEvents TP1 As System.Windows.Forms.TabPage
    Public WithEvents TP2 As System.Windows.Forms.TabPage
    Public WithEvents TP3 As System.Windows.Forms.TabPage
    Public WithEvents TP4 As System.Windows.Forms.TabPage
    Public WithEvents TP5 As System.Windows.Forms.TabPage
    Public WithEvents B1 As System.Windows.Forms.Button
    Public WithEvents HSB1 As System.Windows.Forms.HScrollBar
    Public WithEvents O2 As System.Windows.Forms.Label
    Public WithEvents O1 As System.Windows.Forms.Label
    Public WithEvents F1 As System.Windows.Forms.Label
    Public WithEvents Search As System.Windows.Forms.CheckedListBox
    Public WithEvents Label1 As System.Windows.Forms.Label
    Public WithEvents L4 As System.Windows.Forms.Label
    Public WithEvents L3 As System.Windows.Forms.Label
    Public WithEvents L2 As System.Windows.Forms.Label
    Public WithEvents HSB2 As System.Windows.Forms.HScrollBar
    Public WithEvents CB1 As System.Windows.Forms.ComboBox
    Friend WithEvents TP9 As System.Windows.Forms.TabPage
    Friend WithEvents B01 As System.Windows.Forms.Button
    Friend WithEvents L02 As System.Windows.Forms.Label
    Friend WithEvents L03 As System.Windows.Forms.Label
    Friend WithEvents TP7 As TabPage
End Class