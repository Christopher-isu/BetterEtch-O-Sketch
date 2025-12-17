'Christopher Z
'Fall 2025
'Better Etch-O-Sketch Project
'https://github.com/Christopher-isu/BetterEtch-O-Sketch.git

Option Strict On ' Enforces strong typing to prevent data loss and narrowing conversion errors
Option Explicit On ' Requires all variables to be declared, preventing typos in variable names

Imports System.IO.Ports ' Provides classes for serial communication
Imports System.Timers   ' Provides the Timer class for background polling tasks

' =========================================================================================
' CLASS: QyHandler
' PURPOSE: Acts as the low-level hardware abstraction layer. It manages the SerialPort 
'          connection to the QyBoard PIC, handles asynchronous polling of analog/digital 
'          inputs, and provides thread-safe data via Events.
' =========================================================================================
Public Class QyHandler

    ' -------------------------------------------------------------------------------------
    ' SECTION 1: DATA STORAGE & STATE VARIABLES
    ' -------------------------------------------------------------------------------------

    ' Raw Hex strings representing the last known state of the hardware sensors
    Private _analog1Raw As String = "00 00" ' High/Low byte pair for Potentiometer 1 (X)
    Private _analog2Raw As String = "00 00" ' High/Low byte pair for Potentiometer 2 (Y)
    Private _digitalInRaw As String = "00"  ' Bitmask representing button states
    Private _digitalOutRaw As String = "00" ' Last state sent to the board's LEDs/Outputs

    ' Objects for thread synchronization and background processing
    Private ReadOnly _lock As New Object()        ' Prevents race conditions during data updates
    Private WithEvents _serialPort As SerialPort  ' The physical communication interface
    Private _pollTimer As Timer                   ' Triggers periodic data requests

    ' Polling timing: 150ms is chosen to prevent buffer overflows on the PIC firmware
    ' while maintaining a responsive 6.6Hz refresh rate for the UI.
    Private Const PollIntervalMs As Double = 150.0

    ' -------------------------------------------------------------------------------------
    ' SECTION 2: COMMUNICATION EVENTS
    ' These allow other forms (EtchASketch, QyUI) to receive data without knowing 
    ' the serial protocols.
    ' -------------------------------------------------------------------------------------

    ' Fires when a complete set of sensor data has been retrieved
    Public Event DataUpdated(analog1 As String, analog2 As String, digitalIn As String, digitalOut As String)

    ' Fires for diagnostic logging, showing the raw Request-Response traffic
    Public Event PacketLogged(sentHex As String, receivedHex As String)

    ' -------------------------------------------------------------------------------------
    ' SECTION 3: CONNECTION MANAGEMENT
    ' -------------------------------------------------------------------------------------

    ''' <summary>
    ''' Checks if the serial port exists and is currently in an active state.
    ''' </summary>
    Public ReadOnly Property IsConnected As Boolean
        Get
            Return _serialPort IsNot Nothing AndAlso _serialPort.IsOpen
        End Get
    End Property

    ''' <summary>
    ''' Configures UART settings (19200 Baud, 8N1) and initializes the polling cycle.
    ''' </summary>
    Public Sub Connect(portName As String)
        If IsConnected Then Return ' Avoid re-opening an already active port

        ' QyBoard hardware requires specific 19200 baud rate for reliable timing
        _serialPort = New SerialPort(portName, 19200, Parity.None, 8, StopBits.One)
        _serialPort.ReadTimeout = 200 ' 200ms timeout prevents the UI from freezing on hardware failure

        Try
            _serialPort.Open()
            ' Initialize the background timer to handle the Sequential Poll loop
            _pollTimer = New Timer(PollIntervalMs) With {.AutoReset = True}
            AddHandler _pollTimer.Elapsed, AddressOf PollTimer_Elapsed
            _pollTimer.Start()
        Catch ex As Exception
            Disconnect() ' Ensure cleanup if the port is busy or unavailable
            Throw ' Re-throw the exception for the UI to catch and display
        End Try
    End Sub

    ' -------------------------------------------------------------------------------------
    ' SECTION 4: POLLING ENGINE (THE HEARTBEAT)
    ' Handles the timed requests for data from the PIC.
    ' -------------------------------------------------------------------------------------

    ''' <summary>
    ''' Triggered by the timer; redirects to the main sequential polling logic.
    ''' </summary>
    Private Sub PollTimer_Elapsed(sender As Object, e As ElapsedEventArgs)
        PerformSequentialPoll()
    End Sub

    ''' <summary>
    ''' Sends individual Hex commands to the PIC and waits for the specific byte responses.
    ''' </summary>
    Private Sub PerformSequentialPoll()
        If Not IsConnected Then Return

        Try
            ' --- 1. Request Analog 1 (X-Axis) ---
            ' DiscardInBuffer ensures we aren't reading old data stuck in the serial buffer
            _serialPort.DiscardInBuffer()
            _serialPort.Write({&H51}, 0, 1) ' Command 0x51: Request A1
            Dim buf1(1) As Byte ' Buffer sized for 2 bytes (High/Low)
            ReadExactly(buf1, 2)
            Dim a1 = buf1(0).ToString("X2") & " " & buf1(1).ToString("X2")

            ' --- 2. Request Analog 2 (Y-Axis) ---
            _serialPort.Write({&H52}, 0, 1) ' Command 0x52: Request A2
            Dim buf2(1) As Byte
            ReadExactly(buf2, 2)
            Dim a2 = buf2(0).ToString("X2") & " " & buf2(1).ToString("X2")

            ' --- 3. Request Digital In (Buttons) ---
            _serialPort.Write({&H30}, 0, 1) ' Command 0x30: Request Inputs
            Dim buf3(0) As Byte ' Buffer sized for 1 byte (Bitmask)
            ReadExactly(buf3, 1)
            Dim di = buf3(0).ToString("X2")

            ' --- 4. Thread-Safe Update ---
            ' SyncLock prevents the UI thread from reading these strings while we are writing them
            SyncLock _lock
                _analog1Raw = a1
                _analog2Raw = a2
                _digitalInRaw = di
            End SyncLock

            ' Broadcast the findings to the application
            RaiseEvent PacketLogged("51-52-30", a1 & " | " & a2 & " | " & di)
            RaiseEvent DataUpdated(_analog1Raw, _analog2Raw, _digitalInRaw, _digitalOutRaw)

        Catch ex As Exception
            ' If a read/write fails due to noise or timeout, we catch it here so 
            ' the polling loop can try again in the next cycle without crashing.
        End Try
    End Sub

    ' -------------------------------------------------------------------------------------
    ' SECTION 5: DATA TRANSMISSION & COMMANDS
    ' Methods used to send instructions to the board.
    ' -------------------------------------------------------------------------------------

    ''' <summary>
    ''' Block-waits until the exact number of bytes requested is returned by the hardware.
    ''' Prevents "Partial Packet" errors common in serial comms.
    ''' </summary>
    Private Sub ReadExactly(buffer() As Byte, count As Integer)
        Dim offset As Integer = 0
        While offset < count
            ' Incremental read: keep reading until the requested 'count' is satisfied
            Dim read As Integer = _serialPort.Read(buffer, offset, count - offset)
            offset += read
        End While
    End Sub

    ''' <summary>
    ''' Diagnostic function for QyUI to check the PIC status register (0x10).
    ''' </summary>
    Public Function SendReadStatus() As String
        If Not IsConnected Then Return "Error"
        Try
            _serialPort.Write({&H10}, 0, 1) ' Command 0x10: Status
            Return _serialPort.ReadByte().ToString("X2")
        Catch
            Return "ERR"
        End Try
    End Function

    ''' <summary>
    ''' Sends a 2-byte packet to the board to update Digital Outputs (LEDs).
    ''' </summary>
    Public Sub SendDigitalOutput(value As Byte)
        If Not IsConnected Then Return
        ' Command 0x20 is the "Write Outputs" instruction
        _serialPort.Write({&H20, value}, 0, 2)
        _digitalOutRaw = value.ToString("X2")

        ' Update UI immediately so the change is reflected in diagnostics
        RaiseEvent DataUpdated(_analog1Raw, _analog2Raw, _digitalInRaw, _digitalOutRaw)
    End Sub

    ''' <summary>
    ''' Safely terminates the timer and releases the serial port resources.
    ''' </summary>
    Public Sub Disconnect()
        If _pollTimer IsNot Nothing Then _pollTimer.Stop()
        If _serialPort IsNot Nothing AndAlso _serialPort.IsOpen Then
            _serialPort.Close()
        End If
    End Sub
End Class