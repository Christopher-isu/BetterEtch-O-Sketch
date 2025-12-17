'ChristopherZ
'Fall 2025
'RCET3371
'Better Etch-O-Sketch
'https://github.com/Christopher-isu/BetterEtch-O-Sketch.git

Option Strict On
Option Explicit On

Imports System.IO.Ports ' Access to physical serial hardware
Imports System.Timers   ' High-resolution timer for polling loops

''' <summary>
''' The Communication Layer. 
''' Handles hardware-level polling, packet reconstruction, and thread-safe data distribution.
''' </summary>
Public Class QyHandler
    ' --- DATA STORAGE ---
    ' Raw hex strings stored exactly as they arrive from the PIC for diagnostic transparency
    Private _analog1Raw As String = "00 00"
    Private _analog2Raw As String = "00 00"
    Private _digitalInRaw As String = "00"
    Private _digitalOutRaw As String = "00"

    ' --- OBJECTS & THREADING ---
    ' _lock ensures that the UI doesn't read data while the background thread is writing it
    Private ReadOnly _lock As New Object()
    Private WithEvents _serialPort As SerialPort
    Private _pollTimer As Timer

    ' Polling at 100ms (10Hz) balances responsiveness with CPU efficiency
    Private Const PollIntervalMs As Double = 100.0

    ' --- EVENTS ---
    ' Triggered whenever a full poll cycle (A1, A2, DI) completes successfully
    Public Event DataUpdated(analog1 As String, analog2 As String, digitalIn As String, digitalOut As String)
    ' Used by the Diag form to show raw packet traffic
    Public Event PacketLogged(sentHex As String, receivedHex As String)

    ' =========================================================================
    ' SECTION 1: CONNECTIVITY LOGIC
    ' =========================================================================

    Public ReadOnly Property IsConnected As Boolean
        Get
            Return _serialPort IsNot Nothing AndAlso _serialPort.IsOpen
        End Get
    End Property

    ''' <summary>
    ''' Configures the UART parameters (19200, 8N1) and starts the polling heartbeat.
    ''' </summary>
    Public Sub Connect(portName As String)
        If IsConnected Then Return

        ' 19200 Baud is the specific requirement for the QyBoard PIC firmware
        _serialPort = New SerialPort(portName, 19200, Parity.None, 8, StopBits.One)
        _serialPort.ReadTimeout = 200 ' Prevent the app from hanging if the board is unplugged

        Try
            _serialPort.Open()
            _pollTimer = New Timer(PollIntervalMs) With {.AutoReset = True}
            AddHandler _pollTimer.Elapsed, AddressOf PollTimer_Elapsed
            _pollTimer.Start()
        Catch ex As Exception
            Disconnect()
            Throw ' Pass the error up to the UI to show a MessageBox
        End Try
    End Sub

    ' =========================================================================
    ' SECTION 2: THE POLLING LOOP (SEQUENTIAL STATE MACHINE)
    ' This section prevents "Data Collision" by waiting for a response before 
    ' sending the next request.
    ' =========================================================================

    Private Sub PollTimer_Elapsed(sender As Object, e As ElapsedEventArgs)
        If Not IsConnected Then Return

        Try
            ' Clear the hardware buffer to ensure we aren't reading "stale" data from the past
            _serialPort.DiscardInBuffer()

            ' 1. REQUEST ANALOG 1 (X-Axis)
            _serialPort.Write({&H51}, 0, 1) ' Send Command 0x51
            Dim b1(1) As Byte
            ReadExactly(b1, 2) ' Board returns 2 bytes (High/Low)
            Dim a1 = b1(0).ToString("X2") & " " & b1(1).ToString("X2")

            ' 2. REQUEST ANALOG 2 (Y-Axis)
            _serialPort.Write({&H52}, 0, 1) ' Send Command 0x52
            Dim b2(1) As Byte
            ReadExactly(b2, 2)
            Dim a2 = b2(0).ToString("X2") & " " & b2(1).ToString("X2")

            ' 3. REQUEST DIGITAL IN (Buttons)
            _serialPort.Write({&H30}, 0, 1) ' Send Command 0x30
            Dim b3(0) As Byte
            ReadExactly(b3, 1) ' Board returns 1 byte (Bitmask)
            Dim di = b3(0).ToString("X2")

            ' Update internal state within a Thread-Safe lock
            SyncLock _lock
                _analog1Raw = a1
                _analog2Raw = a2
                _digitalInRaw = di
            End SyncLock

            ' Broadcast the new data to all open forms (Main and Diag)
            RaiseEvent DataUpdated(_analog1Raw, _analog2Raw, _digitalInRaw, _digitalOutRaw)
            RaiseEvent PacketLogged("51|52|30", a1 & "|" & a2 & "|" & di)
        Catch
            ' Silently fail during poll to prevent crashing if a single packet is lost
        End Try
    End Sub

    ''' <summary>
    ''' Critical Helper: Forces the thread to wait until the expected number of bytes 
    ''' has actually arrived, or the ReadTimeout is reached.
    ''' </summary>
    Private Sub ReadExactly(buffer() As Byte, count As Integer)
        Dim offset As Integer = 0
        While offset < count
            Dim read As Integer = _serialPort.Read(buffer, offset, count - offset)
            If read = 0 Then Throw New Exception("Timeout")
            offset += read
        End While
    End Sub

    ' =========================================================================
    ' SECTION 3: COMMAND EXECUTION
    ' =========================================================================

    ''' <summary>
    ''' Sends a command to the PIC to toggle LEDs or other Digital Outputs.
    ''' </summary>
    Public Sub SendDigitalOutput(value As Byte)
        If Not IsConnected Then Return
        ' Command 0x20 + 1 Byte Payload
        _serialPort.Write({&H20, value}, 0, 2)
        _digitalOutRaw = value.ToString("X2")
    End Sub

    Public Sub Disconnect()
        If _pollTimer IsNot Nothing Then _pollTimer.Stop()
        If _serialPort IsNot Nothing AndAlso _serialPort.IsOpen Then _serialPort.Close()
    End Sub
End Class