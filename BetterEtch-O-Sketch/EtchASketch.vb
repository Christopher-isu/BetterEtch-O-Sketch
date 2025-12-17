'Christopher Z
'Fall 2025
'Better Etch-O-Sketch Project
'https://github.com/Christopher-isu/BetterEtch-O-Sketch.git

Option Strict On ' Enforces rigorous type checking to prevent implicit data loss
Option Explicit On ' Requires all variables to be declared prior to use

Imports System.Threading ' Necessary for thread-based pauses (Shake effect)
Imports System.IO.Ports  ' Access to serial port enumeration
Imports System.Drawing   ' Core GDI+ library for all canvas drawing operations

' =========================================================================================
' CLASS: EtchASketch
' PURPOSE: The primary Controller for the application. It manages the drawing canvas,
'          processes hardware inputs via the QyHandler, and handles UI state logic 
'          for both Mouse and External (PIC) control modes.
' =========================================================================================
Public Class EtchASketch
    ' --- SHARED RESOURCES ---
    ' Shared instance of the Serial Handler to ensure consistent communication
    Private Shared ReadOnly QyHandlerInstance As New QyHandler()
    ' ToolTip manager to provide contextual help on UI elements
    Private ToolTipProvider As New ToolTip()

    ' --- CALIBRATED HARDWARE CONSTRAINTS ---
    ' RawMin/Max define the physical potentiometer range (0240 - FDC0 Hex).
    ' These are used to map 16-bit analog values to screen coordinates.
    Private Const RawMin As Integer = &H240
    Private Const RawMax As Integer = &HFDC0

    ' --- DISPLAY RESOLUTION CONSTRAINTS ---
    ' Defines the virtual drawing surface area (800 pixels wide by 403 pixels high).
    Private Const MaxWidth As Integer = 800
    Private Const MaxHeight As Integer = 403

    ' --- DRAWING STATE ---
    ' Tracking variables for the "Pen" position. Set to -1 to avoid initial drift lines.
    Private _currentX As Integer = -1
    Private _currentY As Integer = -1
    ' Logic lock to prevent single button presses from triggering multiple events (Debounce)
    Private _isButtonLocked As Boolean = False
    ' Stores the current user-selected pen color
    Private DrawColor As Color = Color.Black

    ' -------------------------------------------------------------------------------------
    ' SECTION 1: FORM LIFE CYCLE & INITIALIZATION
    ' -------------------------------------------------------------------------------------

    Private Sub EtchASketch_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Attach the event listener for incoming hardware data packets
        AddHandler QyHandlerInstance.DataUpdated, AddressOf QyHandler_DataUpdated

        ' Initialize UI components and state
        PopulateComPortMenu()
        InitializeToolTips()
        SetupAccessibility()
        UpdateUIState()

        ' Default the interface to Mouse Mode for standalone testing
        rbtnMouseMode.Checked = True
    End Sub

    Private Sub InitializeToolTips()
        ' Set up hover-text for better user discoverability
        ToolTipProvider.SetToolTip(DisplayPictureBox, "Draw within this area using the mouse or knobs.")
        ToolTipProvider.SetToolTip(SelectColorButton, "Select a new color for drawing.")
        ToolTipProvider.SetToolTip(DrawWaveformsButton, "Plot sine, cosine, and tangent waveforms.")
        ToolTipProvider.SetToolTip(ClearButton, "Clear the drawing area.")
        ToolTipProvider.SetToolTip(ExitButton, "Exit the application.")
    End Sub

    Private Sub SetupAccessibility()
        ' Configure keyboard tab indices and default form triggers (Enter/Escape)
        SelectColorButton.TabIndex = 0
        DrawWaveformsButton.TabIndex = 1
        ClearButton.TabIndex = 2
        ExitButton.TabIndex = 3
        Me.AcceptButton = DrawWaveformsButton
        Me.CancelButton = ClearButton
    End Sub

    ' -------------------------------------------------------------------------------------
    ' SECTION 2: WAVEFORM DRAWING LOGIC (MATHEMATICAL PLOTTING)
    ' -------------------------------------------------------------------------------------

    ''' <summary>
    ''' Clears the canvas and renders a 10x10 graticule followed by Red/Sine, 
    ''' Green/Cosine, and Blue/Tangent waveforms.
    ''' </summary>
    Private Sub DrawWaveforms() Handles DrawWaveformsButton.Click, DrawWaveformsMenuItem.Click
        Try
            Using g As Graphics = DisplayPictureBox.CreateGraphics()
                Dim width As Integer = DisplayPictureBox.Width
                Dim height As Integer = DisplayPictureBox.Height

                ' Wipe surface to white before drawing grid
                g.Clear(Color.White)
                Using pGrid As New Pen(Color.LightGray)
                    ' Draw vertical graticule lines
                    For x As Integer = 0 To width Step Math.Max(1, width \ 10)
                        g.DrawLine(pGrid, x, 0, x, height)
                    Next
                    ' Draw horizontal graticule lines
                    For y As Integer = 0 To height Step Math.Max(1, height \ 10)
                        g.DrawLine(pGrid, 0, y, width, y)
                    Next
                End Using

                ' Iterate through the three primary trigonometric functions
                ' Tangent is scaled (/10) to stay visible on the Y-axis
                DrawWaveSegment(g, Color.Red, Function(x) Math.Sin(x))
                DrawWaveSegment(g, Color.Green, Function(x) Math.Cos(x))
                DrawWaveSegment(g, Color.Blue, Function(x) Math.Tan(x) / 10)
            End Using
        Catch ex As Exception
            MessageBox.Show($"Error drawing waveforms: {ex.Message}")
        End Try
    End Sub

    ''' <summary>
    ''' Generic plotter that maps a mathematical function across the PictureBox width.
    ''' </summary>
    Private Sub DrawWaveSegment(g As Graphics, color As Color, waveFunc As Func(Of Double, Double))
        Dim width As Integer = DisplayPictureBox.Width
        Dim height As Integer = DisplayPictureBox.Height
        Dim centerY As Integer = height \ 2

        Dim oldX As Integer = 0
        Dim oldY As Integer = centerY

        Using pWave As New Pen(color, 1)
            For x As Integer = 1 To width
                Try
                    ' Convert X pixel to Radians (0 to 2PI)
                    Dim yVal As Double = waveFunc(x * 2 * Math.PI / width)
                    ' Scale Y result to 1/3 of the height for visual balance
                    Dim y As Integer = centerY - CInt(yVal * (height \ 3))

                    ' CLAMPING: Ensures lines don't wrap or draw outside bounds
                    y = Math.Max(0, Math.Min(y, height - 1))

                    g.DrawLine(pWave, oldX, oldY, x, y)
                    oldX = x
                    oldY = y
                Catch
                    ' Handle potential overflows (e.g., Tan approaching infinity)
                    oldX = x
                    oldY = centerY
                End Try
            Next
        End Using
    End Sub

    ' -------------------------------------------------------------------------------------
    ' SECTION 3: DATA & DRAWING LOGIC (EXTERNAL MODE)
    ' -------------------------------------------------------------------------------------

    ''' <summary>
    ''' Event handler triggered at 50ms intervals by the QyHandler background thread.
    ''' </summary>
    Private Sub QyHandler_DataUpdated(a1 As String, a2 As String, di As String, dou As String)
        ' Thread Marshalling: Ensure graphics calls happen on the UI thread
        If Me.InvokeRequired Then
            Me.Invoke(New Action(Of String, String, String, String)(AddressOf QyHandler_DataUpdated), a1, a2, di, dou)
            Return
        End If

        ' Only process coordinate changes if External Mode is physically selected
        If rbtnExternalMode.Checked Then
            ' Convert raw Hex "XX XX" to 16-bit Integers
            Dim valX As Integer = HexToFullValue(a1)
            Dim valY As Integer = HexToFullValue(a2)

            ' Interpolate hardware values into pixel-space (0-800, 0-403)
            Dim targetX As Integer = MapToDimension(valX, RawMin, RawMax, MaxWidth)
            Dim targetY As Integer = MapToDimension(valY, RawMin, RawMax, MaxHeight)

            ' Optimization: Only draw if the movement spans at least one pixel
            If targetX <> _currentX OrElse targetY <> _currentY Then
                If _currentX <> -1 AndAlso _currentY <> -1 Then
                    Using g As Graphics = DisplayPictureBox.CreateGraphics()
                        g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                        Using p As New Pen(DrawColor, 2)
                            g.DrawLine(p, _currentX, _currentY, targetX, targetY)
                        End Using
                    End Using
                End If
                _currentX = targetX
                _currentY = targetY
            End If
            ' Process button inputs (Clear/Color)
            HandleButtonsActiveLow(di)
        End If
    End Sub

    ' -------------------------------------------------------------------------------------
    ' SECTION 4: MOUSE SUPPORT & COMMON ACTIONS
    ' -------------------------------------------------------------------------------------

    Private Sub DisplayPictureBox_MouseMove(sender As Object, e As MouseEventArgs) Handles DisplayPictureBox.MouseMove
        ' logic isolation: Mouse only tracks if Mouse Mode is active
        If rbtnMouseMode.Checked Then
            If e.Button = MouseButtons.Left AndAlso _currentX <> -1 Then
                Using g = DisplayPictureBox.CreateGraphics(), p = New Pen(DrawColor, 2)
                    g.DrawLine(p, _currentX, _currentY, e.X, e.Y)
                End Using
            End If
            ' Update tracking variables only for Mouse-specific use
            _currentX = e.X
            _currentY = e.Y
        End If
    End Sub

    ''' <summary>
    ''' Executes the window-shaking effect and erases the canvas.
    ''' </summary>
    Private Sub ShakeAndClear() Handles ClearButton.Click, ClearMenuItem.Click
        Dim start = Me.Left
        ' Physical feedback: Mimics shaking the real Etch A Sketch toy
        For i = 0 To 5
            Me.Left = start + 10 : Thread.Sleep(35)
            Me.Left = start - 10 : Thread.Sleep(35)
        Next
        Me.Left = start
        DisplayPictureBox.Refresh() ' Force a redraw of the background, clearing the ink
    End Sub

    Private Sub PromptForColor() Handles SelectColorButton.Click, SelectColorMenuItem.Click
        Using cd As New ColorDialog()
            If cd.ShowDialog() = DialogResult.OK Then DrawColor = cd.Color
        End Using
    End Sub

    Private Sub AboutMenuItem_Click(sender As Object, e As EventArgs) Handles AboutMenuItem.Click
        ' Launches the project-specific About window
        Using frmAbout As New AboutForm()
            frmAbout.ShowDialog()
        End Using
    End Sub

    Private Sub ExitApp(sender As Object, e As EventArgs) Handles ExitButton.Click, ExitMenuItem.Click
        Me.Close()
    End Sub

    Private Sub rbtnMode_CheckedChanged(sender As Object, e As EventArgs) Handles rbtnMouseMode.CheckedChanged, rbtnExternalMode.CheckedChanged
        ' SAFETY: Prevent "Teleport Lines" when switching from Mouse to Knobs
        _currentX = -1 : _currentY = -1
    End Sub

    ' -------------------------------------------------------------------------------------
    ' SECTION 5: HELPERS & CONNECTION LOGIC
    ' -------------------------------------------------------------------------------------

    ''' <summary>
    ''' Maps a value from a hardware input range to a UI output range.
    ''' </summary>
    Private Function MapToDimension(val As Integer, inMin As Integer, inMax As Integer, outMax As Integer) As Integer
        Dim clamped = Math.Max(inMin, Math.Min(val, inMax))
        ' Standard Linear Interpolation: (value - min) / (max - min) * targetRange
        Return CInt(Math.Round((clamped - inMin) * outMax / (inMax - inMin)))
    End Function

    ''' <summary>
    ''' Concatenates High/Low Hex bytes from the Serial stream into a single Integer.
    ''' </summary>
    Private Function HexToFullValue(hexPair As String) As Integer
        Try
            Dim parts() As String = hexPair.Split(" "c)
            Return (Convert.ToInt32(parts(0), 16) << 8) Or Convert.ToInt32(parts(1), 16)
        Catch
            Return 0
        End Try
    End Function

    ''' <summary>
    ''' Processes Digital Input bitmasks where 0 = Pressed (Active-Low).
    ''' </summary>
    Private Sub HandleButtonsActiveLow(hex As String)
        Try
            ' Invert bits: 0 becomes 1 for logical evaluation
            Dim inverted = Convert.ToByte(hex, 16) Xor &HFF
            If (inverted And 3) <> 0 Then ' Check if either Button 0 or 1 is pressed
                If Not _isButtonLocked Then
                    If (inverted And 1) <> 0 Then ShakeAndClear() ' Button 1 triggers Shake
                    If (inverted And 2) <> 0 Then PromptForColor() ' Button 2 triggers Color Picker
                    _isButtonLocked = True ' Lock until physical release
                End If
            Else
                _isButtonLocked = False ' Unlock once buttons are released
            End If
        Catch
        End Try
    End Sub

    Public Sub PopulateComPortMenu()
        mnuConnect.DropDownItems.Clear()
        For Each p In SerialPort.GetPortNames()
            mnuConnect.DropDownItems.Add(p, Nothing, AddressOf PortItem_Click)
        Next
    End Sub

    Private Sub PortItem_Click(sender As Object, e As EventArgs)
        Try
            Dim portName = DirectCast(sender, ToolStripItem).Text
            QyHandlerInstance.Connect(portName)
            lblPortValue.Text = portName
            UpdateUIState()
            ' Auto-toggle to External mode upon successful handshake
            rbtnExternalMode.Checked = True
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    Private Sub mnuDisconnect_Click(sender As Object, e As EventArgs) Handles mnuDisconnect.Click
        QyHandlerInstance.Disconnect()
        lblPortValue.Text = "n/a"
        UpdateUIState()
        rbtnMouseMode.Checked = True
    End Sub

    Private Sub UpdateUIState()
        ' Reflect connectivity in the UI status bar
        Dim isConn = QyHandlerInstance.IsConnected
        lblStatus.Text = If(isConn, "Connected", "Disconnected")
        lblStatus.ForeColor = If(isConn, Color.Green, Color.Red)
        mnuConnect.Enabled = Not isConn
        mnuDisconnect.Enabled = isConn
    End Sub

    Private Sub mnuDiag_Click(sender As Object, e As EventArgs) Handles mnuDiag.Click
        ' Opens the diagnostic monitor, passing the shared handler reference
        Dim f As New QyUI(QyHandlerInstance)
        f.Show()
    End Sub
End Class