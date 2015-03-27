<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
	Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
	Private Sub InitializeComponent()
		Me.components = New System.ComponentModel.Container()
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
		Me.cmbPorts = New System.Windows.Forms.ComboBox()
		Me.Button2 = New System.Windows.Forms.Button()
		Me.Button3 = New System.Windows.Forms.Button()
		Me.Button4 = New System.Windows.Forms.Button()
		Me.Button5 = New System.Windows.Forms.Button()
		Me.Label1 = New System.Windows.Forms.Label()
		Me.Label2 = New System.Windows.Forms.Label()
		Me.ListBox1 = New System.Windows.Forms.ListBox()
		Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
		Me.ToolTip2 = New System.Windows.Forms.ToolTip(Me.components)
		Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
		Me.SuspendLayout()
		'
		'cmbPorts
		'
		Me.cmbPorts.FormattingEnabled = True
		Me.cmbPorts.Location = New System.Drawing.Point(12, 12)
		Me.cmbPorts.Name = "cmbPorts"
		Me.cmbPorts.Size = New System.Drawing.Size(88, 21)
		Me.cmbPorts.TabIndex = 1
		'
		'Button2
		'
		Me.Button2.Location = New System.Drawing.Point(260, 9)
		Me.Button2.Name = "Button2"
		Me.Button2.Size = New System.Drawing.Size(129, 23)
		Me.Button2.TabIndex = 3
		Me.Button2.Text = "On/Off"
		Me.ToolTip1.SetToolTip(Me.Button2, "Toggles whether the LED lights will be on or off.")
		Me.Button2.UseVisualStyleBackColor = True
		'
		'Button3
		'
		Me.Button3.Location = New System.Drawing.Point(260, 40)
		Me.Button3.Name = "Button3"
		Me.Button3.Size = New System.Drawing.Size(129, 23)
		Me.Button3.TabIndex = 4
		Me.Button3.Text = "Toggle Cycling"
		Me.ToolTip2.SetToolTip(Me.Button3, "Toggles Cycling, when enabled, you will see the LEDs" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "flash white once, the Ardui" &
		"no will automatically cycle through" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "the patterns every 30 seconds. The LEDs wil" &
		"l flash white twice " & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "when disabled.")
		Me.Button3.UseVisualStyleBackColor = True
		'
		'Button4
		'
		Me.Button4.Location = New System.Drawing.Point(339, 97)
		Me.Button4.Name = "Button4"
		Me.Button4.Size = New System.Drawing.Size(50, 23)
		Me.Button4.TabIndex = 5
		Me.Button4.Text = "+"
		Me.Button4.UseVisualStyleBackColor = True
		'
		'Button5
		'
		Me.Button5.Location = New System.Drawing.Point(260, 97)
		Me.Button5.Name = "Button5"
		Me.Button5.Size = New System.Drawing.Size(50, 23)
		Me.Button5.TabIndex = 6
		Me.Button5.Text = "-"
		Me.Button5.UseVisualStyleBackColor = True
		'
		'Label1
		'
		Me.Label1.AutoSize = True
		Me.Label1.Location = New System.Drawing.Point(296, 81)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(56, 13)
		Me.Label1.TabIndex = 7
		Me.Label1.Text = "Brightness"
		'
		'Label2
		'
		Me.Label2.AutoSize = True
		Me.Label2.Location = New System.Drawing.Point(107, 19)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(83, 13)
		Me.Label2.TabIndex = 8
		Me.Label2.Text = "Select Com Port"
		'
		'ListBox1
		'
		Me.ListBox1.FormattingEnabled = True
		Me.ListBox1.Items.AddRange(New Object() {"Rainbow", "Rainbow With Glitter", "Confetti", "Sinelon", "Juggle", "BPM"})
		Me.ListBox1.Location = New System.Drawing.Point(13, 40)
		Me.ListBox1.Name = "ListBox1"
		Me.ListBox1.Size = New System.Drawing.Size(177, 82)
		Me.ListBox1.TabIndex = 9
		'
		'ToolTip1
		'
		Me.ToolTip1.AutoPopDelay = 10000
		Me.ToolTip1.InitialDelay = 500
		Me.ToolTip1.ReshowDelay = 100
		'
		'ToolTip2
		'
		Me.ToolTip2.AutoPopDelay = 10000
		Me.ToolTip2.InitialDelay = 500
		Me.ToolTip2.ReshowDelay = 100
		'
		'Timer1
		'
		Me.Timer1.Interval = 1000
		'
		'Form1
		'
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
		Me.ClientSize = New System.Drawing.Size(400, 131)
		Me.Controls.Add(Me.ListBox1)
		Me.Controls.Add(Me.Label2)
		Me.Controls.Add(Me.Label1)
		Me.Controls.Add(Me.Button5)
		Me.Controls.Add(Me.Button4)
		Me.Controls.Add(Me.Button3)
		Me.Controls.Add(Me.Button2)
		Me.Controls.Add(Me.cmbPorts)
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
		Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
		Me.MaximizeBox = False
		Me.Name = "Form1"
		Me.Text = "Arduino Commander v1.1"
		Me.ResumeLayout(False)
		Me.PerformLayout()

	End Sub
	Friend WithEvents cmbPorts As ComboBox
	Friend WithEvents Button2 As Button
	Friend WithEvents Button3 As Button
	Friend WithEvents Button4 As Button
	Friend WithEvents Button5 As Button
	Friend WithEvents Label1 As Label
	Friend WithEvents Label2 As Label
	Friend WithEvents ListBox1 As ListBox
	Friend WithEvents ToolTip1 As ToolTip
	Friend WithEvents ToolTip2 As ToolTip
	Friend WithEvents Timer1 As Timer
End Class
