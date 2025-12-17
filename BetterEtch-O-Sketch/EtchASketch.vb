'ChristopherZ
'Fall 2025
'RCET3371
'Better Etch-O-Sketch
'https://github.com/Christopher-isu/BetterEtch-O-Sketch.git

Option Strict On ' Ensures type safety and prevents hidden data loss during conversions
Option Explicit On ' Forces variable declaration to prevent spelling-based logic bugs

Imports System.Threading ' Used for Thread.Sleep in the shake animation
Imports System.IO.Ports  ' Used for SerialPort.GetPortNames
Imports System.Drawing   ' Used for GDI+ drawing (Graphics, Pen, Color)

''' <summary>
''' Main UI Logic for the Better Etch-O-Sketch.
''' Handles coordinate mapping, trig waveform generation, and Serial communication events.
''' </summary>
Public Class EtchASketch

    ' =========================================================================
    ' SECTION 1: FIELD DEFINITIONS & CONSTANTS
    ' These values define the "Physical vs Virtual" mapping of the application.
    ' =========================================================================

    ' Shared communication engine (Singleton-style access)
    Private Shared ReadOnly QyHandlerInstance As New QyHandler()

    ' ToolTip provider to enhance User Experience (UX)
    Private ToolTipProvider As New ToolTip()

    ' --- HARDWARE CALIBRATION CONSTANTS ---
    ' RawMin/Max define the physical boundaries of your potentiometer sweep (Hex 0240 to FDC0).
    Private Const RawMin As Integer = &H240
    Private Const RawMax As Integer = &HFDC0

    ' --- DISPLAY BOUNDARIES ---
    ' Fixed resolution based on the PictureBox designer size (800x403).
    Private Const MaxWidth As Integer = 800
    Private Const MaxHeight As Integer = 403

    ' --- DRAWING STATE TRACKING ---
    ' _currentX/Y store the "last known point" so GDI+ can draw a line to the "new point".
    ' Initialized to -1 to prevent drawing a line from (0,0) on startup.
    Private _currentX As Integer = -1
    Private _currentY As Integer = -1

    ' _isButtonLocked prevents "rapid-fire" triggering of Clear or Color dialogs
    ' while a hardware button is held down.
    Private _isButtonLocked As Boolean = False

    ' Current pen color, defaults to black
    Private DrawColor As Color = Color.Black

    ' =========================================================================
    ' SECTION 2: FORM LIFE CYCLE
    ' Logic for initializing the UI, ToolTips, and event subscriptions.
    ' =========================================================================

    Private Sub EtchASketch_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Subscribe to the Serial Handler's data event
        AddHandler QyHandlerInstance.DataUpdated, AddressOf QyHandler_DataUpdated

        ' UI Setup routines
        PopulateComPortMenu()
        InitializeToolTips()
        SetupAccessibility()
        UpdateUIState()

        ' Default to Mouse Mode for immediate usability
        rbtnMouseMode.Checked = True
    End Sub

    Private Sub InitializeToolTips()
        ' Provides contextual help when the user hovers over buttons
        ToolTipProvider.SetToolTip(DisplayPictureBox, "Draw area: 800x403 resolution.")
        ToolTipProvider.SetToolTip(SelectColorButton, "Changes the active ink color.")
        ToolTipProvider.SetToolTip(DrawWaveformsButton, "Clears canvas and plots Trig functions.")
        ToolTipProvider.SetToolTip(ClearButton, "Shake the window and erase the drawing.")
        ToolTipProvider.SetToolTip(ExitButton, "Close the application.")
    End Sub

    Private Sub SetupAccessibility()
        ' Define logical tab flow for keyboard users
        SelectColorButton.TabIndex = 0
        DrawWaveformsButton.TabIndex = 1
        ClearButton.TabIndex = 2
        ExitButton.TabIndex = 3

        ' Map Enter and Escape keys to logical actions
        Me.AcceptButton = DrawWaveformsButton
        Me.CancelButton = ClearButton
    End Sub

    ' =========================================================================
    ' SECTION 3: WAVEFORM GENERATION (MATHEMATICAL PLOTTING)
    ' Logic for clearing the canvas and rendering Sine, Cosine, and Tangent.
    ' =========================================================================

    ''' <summary>
    ''' Renders a coordinate grid (graticule) and plots three trig waveforms.
    ''' </summary>
    Private Sub DrawWaveforms() Handles DrawWaveformsButton.Click, DrawWaveformsMenuItem.Click
        Try
            ' CreateGraphics provides a temporary drawing surface
            Using g As Graphics = DisplayPictureBox.CreateGraphics()
                Dim width As Integer = DisplayPictureBox.Width
                Dim height As Integer = DisplayPictureBox.Height

                ' STEP 1: Wipe the canvas clean
                g.Clear(Color.White)

                ' STEP 2: Draw the Graticule (10x10 Grid)
                Using pGrid As New Pen(Color.LightGray)
                    ' Draw Vertical grid lines
                    For x As Integer = 0 To width Step Math.Max(1, width \ 10)
                        g.DrawLine(pGrid, x, 0, x, height)
                    Next
                    ' Draw Horizontal grid lines
                    For y As Integer = 0 To height Step Math.Max(1, height \ 10)
                        g.DrawLine(pGrid, 0, y, width, y)
                    Next
                End Using

                ' STEP 3: Plot the waves using mathematical Delegates (Func)
                ' We scale Tangent by /10 because it approaches infinity quickly.
                DrawWaveSegment(g, Color.Red, Function(x) Math.Sin(x))
                DrawWaveSegment(g, Color.Green, Function(x) Math.Cos(x))
                DrawWaveSegment(g, Color.Blue, Function(x) Math.Tan(x) / 10)
            End Using
        Catch ex As Exception
            MessageBox.Show($"Plotting Error: {ex.Message}")
        End Try
    End Sub

    ''' <summary>
    ''' Helper to iterate across the X-axis and plot a specific math function.
    ''' </summary>
    Private Sub DrawWaveSegment(g As Graphics, color As Color, waveFunc As Func(Of Double, Double))
        Dim width As Integer = DisplayPictureBox.Width
        Dim height As Integer = DisplayPictureBox.Height
        Dim centerY As Integer = height \ 2

        Dim oldX As Integer = 0
        Dim oldY As Integer = centerY

        Using pWave As New Pen(color, 1)
            ' Loop through every pixel on the X-axis
            For x As Integer = 1 To width
                Try
                    ' Convert pixel X to a Radian value (0 to 2PI)
                    Dim radians As Double = x * 2 * Math.PI / width

                    ' Calculate Y and scale to 1/3 of the height for visibility
                    Dim yVal As Double = waveFunc(radians)
                    Dim y As Integer = centerY - CInt(yVal * (height \ 3))

                    ' CLAMPING: Prevents drawing outside the PictureBox bounds
                    y = Math.Max(0, Math.Min(y, height - 1))

                    ' Draw the infinitesimal line segment from the previous point
                    g.DrawLine(pWave, oldX, oldY, x, y)
                    oldX = x
                    oldY = y
                Catch
                    ' Handle potential math overflows (especially in Tangent)
                    oldX = x
                    oldY = centerY
                End Try
            Next
        End Using
    End Sub

    ' =========================================================================
    ' SECTION 4: SERIAL DATA PROCESSING (EXTERNAL MODE)
    ' Logic for mapping hardware potentiometer values to screen coordinates.
    ' =========================================================================

    ''' <summary>
    ''' Handles the async event from QyHandler. Marhsals data to UI thread.
    ''' </summary>
    Private Sub QyHandler_DataUpdated(a1 As String, a2 As String, di As String, dou As String)
        ' Check if we are on the background thread; if so, Invoke back to the UI thread
        If Me.InvokeRequired Then
            Me.Invoke(New Action(Of String, String, String, String)(AddressOf QyHandler_DataUpdated), a1, a2, di, dou)
            Return
        End If

        ' Only update drawing if the user has physically toggled "External (PIC)"
        If rbtnExternalMode.Checked Then
            ' Convert Hex "XX XX" string to a 16-bit Integer
            Dim valX As Integer = HexToFullValue(a1)
            Dim valY As Integer = HexToFullValue(a2)

            ' Map the hardware range (0240-FDC0) to screen pixels (800x403)
            Dim targetX As Integer = MapToDimension(valX, RawMin, RawMax, MaxWidth)
            Dim targetY As Integer = MapToDimension(valY, RawMin, RawMax, MaxHeight)

            ' Optimization: Only draw if the knob has actually moved to a new pixel
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

            ' Check for hardware button presses (Digital Input)
            HandleButtonsActiveLow(di)
        End If
    End Sub

    ' =========================================================================
    ' SECTION 5: MOUSE & UI INTERACTION
    ' Logic for manual drawing and switching between Control Modes.
    ' =========================================================================

    Private Sub DisplayPictureBox_MouseMove(sender As Object, e As MouseEventArgs) Handles DisplayPictureBox.MouseMove
        ' Logic separation: Mouse only draws if MouseMode is active
        If rbtnMouseMode.Checked Then
            ' Draw only while Left Mouse Button is depressed
            If e.Button = MouseButtons.Left AndAlso _currentX <> -1 Then
                Using g = DisplayPictureBox.CreateGraphics(), p = New Pen(DrawColor, 2)
                    g.DrawLine(p, _currentX, _currentY, e.X, e.Y)
                End Using
            End If
            ' Update tracking; ignored by External Mode via the If check
            _currentX = e.X
            _currentY = e.Y
        End If
    End Sub

    Private Sub rbtnMode_CheckedChanged(sender As Object, e As EventArgs) Handles rbtnMouseMode.CheckedChanged, rbtnExternalMode.CheckedChanged
        ' CRITICAL: Clear tracking on mode switch to prevent a "connecting line"
        ' between the last mouse click and the current knob position.
        _currentX = -1
        _currentY = -1
    End Sub

    ' =========================================================================
    ' SECTION 6: SUPPORTING ACTIONS & HELPERS
    ' Auxiliary routines for UI effects, colors, and math.
    ' =========================================================================

    Private Sub ShakeAndClear() Handles ClearButton.Click, ClearMenuItem.Click
        ' Visual feedback: Shakes the form window like a physical Etch A Sketch
        Dim start = Me.Left
        For i = 0 To 5
            Me.Left = start + 10 : Thread.Sleep(35)
            Me.Left = start - 10 : Thread.Sleep(35)
        Next
        Me.Left = start
        ' Refresh clears the PictureBox's persistent bitmap/surface
        DisplayPictureBox.Refresh()
    End Sub

    Private Sub PromptForColor() Handles SelectColorButton.Click, SelectColorMenuItem.Click
        Using cd As New ColorDialog()
            If cd.ShowDialog() = DialogResult.OK Then DrawColor = cd.Color
        End Using
    End Sub

    ''' <summary>
    ''' Maps a value from a hardware range to a screen range using Linear Interpolation.
    ''' </summary>
    Private Function MapToDimension(val As Integer, inMin As Integer, inMax As Integer, outMax As Integer) As Integer
        ' Force the value inside the calibrated floor and ceiling
        Dim clamped = Math.Max(inMin, Math.Min(val, inMax))
        ' Standard mapping formula: (clamped - min) * (target_range / input_range)
        Return CInt(Math.Round((clamped - inMin) * outMax / (inMax - inMin)))
    End Function

    ''' <summary>
    ''' Inverts active-low logic bits and triggers button actions.
    ''' </summary>
    Private Sub HandleButtonsActiveLow(hex As String)
        Try
            ' Board logic: 0xFF = Idle. 0xFE = Button1 pressed.
            ' XOR with 0xFF flips the bits so 1 = Pressed.
            Dim inverted = Convert.ToInt32(hex, 16) Xor &HFF

            ' Check only the first 2 bits (Btn 1 and Btn 2)
            If (inverted And 3) <> 0 Then
                If Not _isButtonLocked Then
                    If (inverted And 1) <> 0 Then ShakeAndClear() ' Bit 0: Clear
                    If (inverted And 2) <> 0 Then PromptForColor() ' Bit 1: Color
                    _isButtonLocked = True ' Lock until physical release
                End If
            Else
                _isButtonLocked = False ' Unlock once all buttons are FF
            End If
        Catch
        End Try
    End Sub

    ' =========================================================================
    ' SECTION 7: CONNECTION MANAGEMENT
    ' Dynamic COM port handling and status monitoring.
    ' =========================================================================

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
            ' Automatically switch to PIC control once connected
            rbtnExternalMode.Checked = True
        Catch ex As Exception
            MessageBox.Show($"Connection Error: {ex.Message}")
        End Try
    End Sub

    Private Sub mnuDisconnect_Click(sender As Object, e As EventArgs) Handles mnuDisconnect.Click
        QyHandlerInstance.Disconnect()
        lblPortValue.Text = "n/a"
        UpdateUIState()
        rbtnMouseMode.Checked = True
    End Sub

    Private Sub UpdateUIState()
        Dim isConn = QyHandlerInstance.IsConnected
        lblStatus.Text = If(isConn, "Connected", "Disconnected")
        lblStatus.ForeColor = If(isConn, Color.Green, Color.Red)
        mnuConnect.Enabled = Not isConn
        mnuDisconnect.Enabled = isConn
    End Sub

    ' Show secondary forms
    Private Sub AboutMenuItem_Click(sender As Object, e As EventArgs) Handles AboutMenuItem.Click
        Using frmAbout As New AboutForm() : frmAbout.ShowDialog() : End Using
    End Sub

    Private Sub mnuDiag_Click(sender As Object, e As EventArgs) Handles mnuDiag.Click
        Dim f As New QyUI(QyHandlerInstance) : f.Show()
    End Sub

    Private Sub ExitApp(sender As Object, e As EventArgs) Handles ExitButton.Click, ExitMenuItem.Click
        Me.Close()
    End Sub

End Class