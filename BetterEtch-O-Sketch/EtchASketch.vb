Option Strict On
Option Explicit On
Imports System.Threading ' For Thread.Sleep
Imports System.IO.Ports ' For SerialPort.GetPortNames()
Imports System.Drawing ' For Graphics

' NOTE: The QyHandler class must be added as a separate file/class to the project.

Public Class EtchASketch
    ' --- Qy Board Integration Fields ---
    Private Shared ReadOnly QyHandlerInstance As New QyHandler()

    Private Enum DrawingMode
        Mouse
        QyBoard
    End Enum

    Private _drawingMode As DrawingMode
    Private _connectedPortName As String = "n/a"

    Private _currentX As Integer = 0
    Private _currentY As Integer = 0

    Private _isDigitalInputHandled As Boolean = False

    ' --- Existing Fields ---
    Private DrawColor As Color = Color.Black
    Private ToolTipProvider As New ToolTip()

    Private Sub EtchASketch_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        InitializeToolTips()
        SetupAccessibility()

        AddHandler QyHandlerInstance.DataUpdated, AddressOf QyHandler_DataUpdated

        PopulateComPortMenu()

        _currentX = DisplayPictureBox.Width \ 2
        _currentY = DisplayPictureBox.Height \ 2

        rbtnMouseMode.Checked = True
        SetDrawingMode(DrawingMode.Mouse)
        UpdateConnectionStateUI()
    End Sub

    Private Sub EtchASketch_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If QyHandlerInstance.IsConnected Then
            QyHandlerInstance.Disconnect()
        End If
        RemoveHandler QyHandlerInstance.DataUpdated, AddressOf QyHandler_DataUpdated
    End Sub

    ' ----------------------------------------------------
    ' --- Drawing Mode Control (Radio Buttons) ---
    ' ----------------------------------------------------

    Private Sub rbtnMouseMode_CheckedChanged(sender As Object, e As EventArgs) Handles rbtnMouseMode.CheckedChanged
        If rbtnMouseMode.Checked Then
            SetDrawingMode(DrawingMode.Mouse)
        End If
    End Sub

    Private Sub rbtnExternalMode_CheckedChanged(sender As Object, e As EventArgs) Handles rbtnExternalMode.CheckedChanged
        If rbtnExternalMode.Checked Then
            If Not QyHandlerInstance.IsConnected Then
                MessageBox.Show("Cannot switch to External Mode: Qy Board is Disconnected.", "Connection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                rbtnMouseMode.Checked = True
                Exit Sub
            End If
            SetDrawingMode(DrawingMode.QyBoard)
        End If
    End Sub

    Private Sub SetDrawingMode(mode As DrawingMode)
        If mode = DrawingMode.Mouse Then
            _currentX = -1
            _currentY = -1
            AddHandler DisplayPictureBox.MouseMove, AddressOf DisplayPictureBox_MouseMove
            _currentX = DisplayPictureBox.Width \ 2
            _currentY = DisplayPictureBox.Height \ 2
        Else ' DrawingMode.QyBoard
            RemoveHandler DisplayPictureBox.MouseMove, AddressOf DisplayPictureBox_MouseMove
            _currentX = DisplayPictureBox.Width \ 2
            _currentY = DisplayPictureBox.Height \ 2
        End If

        _drawingMode = mode
    End Sub

    ' ----------------------------------------------------
    ' --- Qy Board Connection and Menu Management Logic ---
    ' ----------------------------------------------------

    Private Sub UpdateConnectionStateUI()
        Dim isConnected As Boolean = QyHandlerInstance.IsConnected

        Dim portName As String = If(isConnected, _connectedPortName, "n/a")

        ' Update Status Labels
        lblStatus.Text = If(isConnected, "Connected", "Disconnected")
        lblStatus.ForeColor = If(isConnected, Color.Green, Color.Red)
        lblPortValue.Text = portName

        ' Menu State
        mnuConnect.Enabled = Not isConnected
        mnuDisconnect.Enabled = isConnected
        mnuDiag.Enabled = True

        ' External Mode Radio Button availability
        rbtnExternalMode.Enabled = isConnected

        If Not isConnected AndAlso rbtnExternalMode.Checked Then
            rbtnMouseMode.Checked = True
            SetDrawingMode(DrawingMode.Mouse)
        End If
    End Sub

    Private Sub PopulateComPortMenu()
        mnuConnect.DropDownItems.Clear()

        Dim refreshItem As New ToolStripMenuItem("Refresh")
        AddHandler refreshItem.Click, AddressOf RefreshMenuItem_Click
        mnuConnect.DropDownItems.Add(refreshItem)
        mnuConnect.DropDownItems.Add(New ToolStripSeparator())

        Dim detectedPorts As String() = SerialPort.GetPortNames()
        If detectedPorts.Length = 0 Then
            Dim noPortsItem As New ToolStripMenuItem("(No Ports Detected)")
            noPortsItem.Enabled = False
            mnuConnect.DropDownItems.Add(noPortsItem)
        Else
            For Each portName As String In detectedPorts
                Dim portItem As New ToolStripMenuItem(portName)
                AddHandler portItem.Click, AddressOf ConnectPortMenuItem_Click
                mnuConnect.DropDownItems.Add(portItem)
            Next
        End If
    End Sub

    Private Sub RefreshMenuItem_Click(sender As Object, e As EventArgs)
        PopulateComPortMenu()
    End Sub

    Private Sub ConnectPortMenuItem_Click(sender As Object, e As EventArgs)
        Dim portItem As ToolStripMenuItem = DirectCast(sender, ToolStripMenuItem)
        Dim portName As String = portItem.Text

        If QyHandlerInstance.IsConnected Then Return

        Try
            QyHandlerInstance.Connect(portName)
            _connectedPortName = portName
            MessageBox.Show($"Successfully connected to {portName}", "Connected", MessageBoxButtons.OK, MessageBoxIcon.Information)
            rbtnExternalMode.Checked = True
        Catch ex As Exception
            _connectedPortName = "n/a"
            MessageBox.Show($"Failed to connect to {portName}: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            UpdateConnectionStateUI()
        End Try
    End Sub

    Private Sub mnuDisconnect_Click(sender As Object, e As EventArgs) Handles mnuDisconnect.Click
        QyHandlerInstance.Disconnect()
        _connectedPortName = "n/a"
        rbtnMouseMode.Checked = True
        UpdateConnectionStateUI()
    End Sub

    Private Sub mnuDiag_Click(sender As Object, e As EventArgs) Handles mnuDiag.Click
        Dim diagForm As New QyUI(QyHandlerInstance)
        diagForm.Show()
    End Sub

    ' ----------------------------------------------
    ' --- Qy Board ADC Drawing and Input Logic ---
    ' ----------------------------------------------

    Private Sub QyHandler_DataUpdated(analog1Raw As String, analog2Raw As String, digitalInRaw As String, digitalOutRaw As String)
        If Me.InvokeRequired Then
            Me.Invoke(New Action(Of String, String, String, String)(AddressOf QyHandler_DataUpdated), analog1Raw, analog2Raw, digitalInRaw, digitalOutRaw)
            Return
        End If

        If rbtnExternalMode.Checked Then

            ' 1. Convert Analog 1 (X-axis) and Analog 2 (Y-axis) from Hex String to Integer
            Dim analog1Value As Integer = ConvertAnalogHexToValue(analog1Raw)
            Dim analog2Value As Integer = ConvertAnalogHexToValue(analog2Raw)

            ' 2. Map ADC values (0-1023) to PictureBox dimensions
            Dim maxX As Integer = DisplayPictureBox.Width
            Dim maxY As Integer = DisplayPictureBox.Height

            If maxX <= 0 OrElse maxY <= 0 Then Return

            Dim newX As Integer = CInt(analog1Value * maxX / 1023.0)
            Dim newY As Integer = CInt(analog2Value * maxY / 1023.0)

            newX = Math.Min(Math.Max(0, newX), maxX - 1)
            newY = Math.Min(Math.Max(0, newY), maxY - 1)

            ' 3. Draw the line segment
            If _currentX >= 0 AndAlso _currentY >= 0 Then
                Using g As Graphics = DisplayPictureBox.CreateGraphics()
                    Using pen As New Pen(DrawColor, 2)
                        g.DrawLine(pen, _currentX, _currentY, newX, newY)
                    End Using
                End Using
            End If

            ' 4. Update the current position for the next segment
            _currentX = newX
            _currentY = newY

            ' 5. Handle Digital Input (for Clear/Color switch)
            Dim diValue As Byte = 0
            If Byte.TryParse(digitalInRaw, System.Globalization.NumberStyles.HexNumber, Nothing, diValue) Then

                If (diValue And 3) <> 0 Then
                    If Not _isDigitalInputHandled Then

                        If (diValue And 1) <> 0 Then
                            ShakeAndClear()
                            _isDigitalInputHandled = True
                        End If

                        If (diValue And 2) <> 0 Then
                            PresentColorDialog()
                            _isDigitalInputHandled = True
                        End If
                    End If
                Else
                    _isDigitalInputHandled = False
                End If
            End If
        End If
    End Sub

    Private Function ConvertAnalogHexToValue(rawHex As String) As Integer
        Dim parts As String() = rawHex.Split(" "c)
        If parts.Length <> 2 Then Return 0

        Dim msb As Byte = 0
        Dim lsb As Byte = 0

        If Byte.TryParse(parts(0), System.Globalization.NumberStyles.HexNumber, Nothing, msb) AndAlso
           Byte.TryParse(parts(1), System.Globalization.NumberStyles.HexNumber, Nothing, lsb) Then

            Dim fullValue As Integer = (CInt(msb) << 8) Or CInt(lsb)

            Return fullValue And &H3FF
        Else
            Return 0
        End If
    End Function

    ' ------------------------------------
    ' --- Existing Functions ---
    ' ------------------------------------

    Private Sub InitializeToolTips()
        ToolTipProvider.SetToolTip(DisplayPictureBox, "Draw within this area using the mouse.")
        ToolTipProvider.SetToolTip(SelectColorButton, "Select a new color for drawing.")
        ToolTipProvider.SetToolTip(DrawWaveformsButton, "Plot sine, cosine, and tangent waveforms.")
        ToolTipProvider.SetToolTip(ClearButton, "Clear the drawing area.")
        ToolTipProvider.SetToolTip(ExitButton, "Exit the application.")
        ToolTipProvider.SetToolTip(rbtnMouseMode, "Use mouse to draw.")
        ToolTipProvider.SetToolTip(rbtnExternalMode, "Use Qy board inputs to draw.")
    End Sub

    Private Sub SetupAccessibility()
        SelectColorButton.TabIndex = 0
        DrawWaveformsButton.TabIndex = 1
        ClearButton.TabIndex = 2
        ExitButton.TabIndex = 3

        Me.AcceptButton = DrawWaveformsButton
        Me.CancelButton = ClearButton
    End Sub

    Private Sub PresentColorDialog()
        Using colorDialog As New ColorDialog()
            If colorDialog.ShowDialog() = DialogResult.OK Then
                DrawColor = colorDialog.Color
            End If
        End Using
    End Sub

    Private Sub ShakeAndClear()
        Dim offset As Integer = 10
        For i As Integer = 0 To 5
            Me.Left += offset
            Thread.Sleep(50)
            Me.Left -= offset
        Next
        DisplayPictureBox.Refresh()

        _currentX = DisplayPictureBox.Width \ 2
        _currentY = DisplayPictureBox.Height \ 2
    End Sub

    Private Sub DrawWaveforms()
        Try
            Using g As Graphics = DisplayPictureBox.CreateGraphics()
                Dim width As Integer = DisplayPictureBox.Width
                Dim height As Integer = DisplayPictureBox.Height

                g.Clear(Color.White)

                Dim pen As New Pen(Color.LightGray)
                For x As Integer = 0 To width Step Math.Max(1, width \ 10)
                    g.DrawLine(pen, x, 0, x, height)
                Next
                For y As Integer = 0 To height Step Math.Max(1, height \ 10)
                    g.DrawLine(pen, 0, y, width, y)
                Next

                DrawWave(g, Color.Red, Function(x) Math.Sin(x))
                DrawWave(g, Color.Green, Function(x) Math.Cos(x))
                DrawWave(g, Color.Blue, Function(x) SafeTan(x) / 10)
            End Using
        Catch ex As Exception
            MessageBox.Show($"An error occurred while drawing waveforms: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub DrawWave(g As Graphics, color As Color, waveFunc As Func(Of Double, Double))
        Dim pen As New Pen(color)
        Dim width As Integer = DisplayPictureBox.Width
        Dim height As Integer = DisplayPictureBox.Height
        Dim centerY As Integer = height \ 2

        Dim oldX As Integer = 0
        Dim oldY As Integer = centerY

        For x As Integer = 1 To width
            Try
                Dim y As Integer = centerY - CInt(waveFunc(x * 2 * Math.PI / width) * (height \ 3))
                y = Math.Min(Math.Max(0, y), height - 1)
                g.DrawLine(pen, oldX, oldY, x, y)
                oldX = x
                oldY = y
            Catch ex As OverflowException
                oldX = x
                oldY = centerY
            End Try
        Next
    End Sub

    Private Function SafeTan(x As Double) As Double
        Try
            Return Math.Tan(x)
        Catch ex As Exception
            Return 0
        End Try
    End Function

    Private Sub DisplayPictureBox_MouseMove(sender As Object, e As MouseEventArgs) Handles DisplayPictureBox.MouseMove
        Static oldX As Integer = -1
        Static oldY As Integer = -1

        If e.Button = MouseButtons.Left AndAlso oldX >= 0 AndAlso oldY >= 0 Then
            Using g As Graphics = DisplayPictureBox.CreateGraphics()
                Using pen As New Pen(DrawColor, 2)
                    g.DrawLine(pen, oldX, oldY, e.X, e.Y)
                End Using
            End Using
        End If
        oldX = e.X
        oldY = e.Y
    End Sub

    Private Sub ExitButton_Click(sender As Object, e As EventArgs) Handles ExitButton.Click, ExitMenuItem.Click
        Me.Close()
    End Sub

    Private Sub DrawWaveformsButton_Click(sender As Object, e As EventArgs) Handles DrawWaveformsButton.Click, DrawWaveformsMenuItem.Click
        DrawWaveforms()
    End Sub

    Private Sub DisplayPictureBox_MouseDown(sender As Object, e As MouseEventArgs) Handles DisplayPictureBox.MouseDown
        If e.Button = MouseButtons.Middle Then
            PresentColorDialog()
        End If
    End Sub

    Private Sub ClearButton_Click(sender As Object, e As EventArgs) Handles ClearButton.Click, ClearMenuItem.Click
        ShakeAndClear()
    End Sub

    Private Sub SelectColorButton_Click(sender As Object, e As EventArgs) Handles SelectColorButton.Click, SelectColorMenuItem.Click
        PresentColorDialog()
    End Sub

    Private Sub AboutMenuItem_Click(sender As Object, e As EventArgs) Handles AboutMenuItem.Click
        Dim about As New AboutForm()
        about.ShowDialog()
    End Sub

End Class