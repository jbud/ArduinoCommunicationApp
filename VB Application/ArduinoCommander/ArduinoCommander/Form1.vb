Option Explicit On

Imports System.IO.Ports
Public Class Form1
	Dim myComPort As New SerialPort
	Dim time As Integer
	Dim sleeping As Boolean
    Private CommonDialog1 As Object
    Dim clicked As Boolean
    Sub init()

        Dim bitRates(9) As Integer
        Dim nameArray() As String

        ' Find the COM ports on the system.
        clicked = False
        nameArray = SerialPort.GetPortNames
        Array.Sort(nameArray)

        ' Fill a combo box with the port names.

        cmbPorts.DataSource = nameArray
        cmbPorts.DropDownStyle = ComboBoxStyle.DropDownList

        ' Select a default port.

        cmbPorts.SelectedIndex = 1
        time = 0
        Timer1.Start()
        Diagnostics.Debug.WriteLine("Initialized...")

    End Sub

    Sub OpenComPort()
		Try
			If Not myComPort.IsOpen Then
				myComPort.PortName = cmbPorts.SelectedItem.ToString	' Get the selected COM port from the combo box.

				myComPort.BaudRate = 9600 'default for Arduino, please set your arduino back to 9600 if not already set.

                ' Set other port parameters.
                myComPort.DataBits = 8
                myComPort.Parity = Parity.None
                myComPort.StopBits = StopBits.One
                myComPort.Handshake = Handshake.None
                myComPort.Encoding = System.Text.Encoding.Default
                myComPort.ReadTimeout = 10000
                myComPort.WriteTimeout = 5000
                myComPort.RtsEnable = True
                myComPort.DtrEnable = True


                myComPort.Open() ' Open the port.
				sleeping = False
				Diagnostics.Debug.WriteLine("Connection Established")
			End If

		Catch ex As InvalidOperationException
			MessageBox.Show(ex.Message)

		Catch ex As UnauthorizedAccessException
			MessageBox.Show(ex.Message)

		Catch ex As System.IO.IOException
			MessageBox.Show(ex.Message)

		End Try
	End Sub

	Sub CloseComPort()
		Try
			Using myComPort
				If (Not (myComPort Is Nothing)) Then
                    ' The COM port exists.
                    If myComPort.IsOpen Then
                        ' Wait for the transmit buffer to empty.
                        Do While (myComPort.BytesToWrite > 0)
						Loop
						myComPort.Close()
						Diagnostics.Debug.WriteLine("Disconnected.")
					End If
				End If
			End Using
		Catch ex As UnauthorizedAccessException
            ' The port may have been removed. Ignore.
        End Try
	End Sub

	Sub SendCommand(ByVal command As String)

        Try
            myComPort.WriteLine(command)

            Diagnostics.Debug.WriteLine("Sent Command: " + command)



        Catch ex As TimeoutException
			MessageBox.Show(ex.Message)
		Catch ex As InvalidOperationException
			MessageBox.Show(ex.Message)
		Catch ex As UnauthorizedAccessException
			MessageBox.Show(ex.Message)
		End Try

	End Sub
	Private Sub Button1_Click(sender As Object, e As EventArgs)
		unsleep()
		SendCommand("*")
	End Sub
	Private Sub cmbPorts_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbPorts.SelectedIndexChanged
		unsleep()
		CloseComPort()
		myComPort.PortName = cmbPorts.SelectedItem.ToString
		OpenComPort()
	End Sub
	Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        init()
        OpenComPort()
    End Sub

	Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
		unsleep()
		SendCommand("-")
	End Sub

	Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox1.SelectedIndexChanged
		Dim send As String
		unsleep()
        If ListBox1.SelectedIndex = 9 Then
            send = "K"
        Else
            send = CType(ListBox1.SelectedIndex + 1, String)
        End If

        SendCommand(send)
	End Sub

	Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
		unsleep()
		SendCommand("C")
	End Sub



    Private Sub colorchange(ByVal c As Color)
        Dim cc As String

        cc = String.Format("{0:X2}{1:X2}{2:X2}",
                     c.R,
                     c.G,
                     c.B)
        unsleep()
        SendCommand("RGB " + cc)

        Diagnostics.Debug.WriteLine("Color: " + cc)
    End Sub
    Private Sub sleep()
		If sleeping = False Then
			CloseComPort()
			sleeping = True
			Timer1.Stop()
		End If
	End Sub

	Private Sub unsleep()
		If sleeping = True Then
			Timer1.Start()
			OpenComPort()
			sleeping = False
			time = 0
		Else
			time = 0 ' reset timer
		End If
	End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        time = time + 1
        Dim Incoming As String
        ' Diagnostics.Debug.WriteLine("Timer: {0}", time)
        Try
            Incoming = myComPort.ReadExisting()
            If Incoming Is Nothing Then
                Diagnostics.Debug.WriteLine("nothing" + vbCrLf)
            Else
                Diagnostics.Debug.WriteLine(Incoming)
            End If
        Catch ex As TimeoutException
            Diagnostics.Debug.WriteLine("Error: Serial Port read timed out.")
        End Try
        If time > 30 Then ' if the idle timer is longer than 30 seconds, initiate sleep.
            sleep()
            Diagnostics.Debug.WriteLine("sleep mode")
            time = 0
			sleeping = True
		End If
	End Sub

    Private Sub Button1_Click_1(sender As Object, e As EventArgs) Handles Button1.Click
        Dim cDialog As New ColorDialog()
        cDialog.Color = Color.Red

        If (cDialog.ShowDialog() = DialogResult.OK) Then
            colorchange(cDialog.Color)
        End If
    End Sub

    Private Sub TrackBar1_Scroll(sender As Object, e As EventArgs) Handles TrackBar1.Scroll

    End Sub
    Private Sub down(sender As Object, e As EventArgs) Handles TrackBar1.MouseDown
        clicked = True
    End Sub
    Private Sub up(sender As Object, e As EventArgs) Handles TrackBar1.MouseUp
        If (clicked) Then
            Dim xx As String
            xx = TrackBar1.Value
            unsleep()
            SendCommand("B " + xx)
            clicked = False
        End If

    End Sub


End Class
