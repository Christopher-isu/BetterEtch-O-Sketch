Option Strict On
Option Explicit On

Imports System.IO.Ports
Imports System.Timers

Public Class QyHandler

    ' Raw data fields (hex strings exactly as required for UI display)
    Private _analog1Raw As String = "00 00"
    Private _analog2Raw As String = "00 00"
    Private _digitalInRaw As String = "00"
    Private _digitalOutRaw As String = "00"   ' last sent value

    ' Serial port
    Private WithEvents _serialPort As SerialPort
    Private _pollTimer As Timer

    ' Polling interval in milliseconds (200 ms = 5 updates/sec, low UART load at 9600 baud)
    Private Const PollIntervalMs As Double = 200.0

    Public Event DataUpdated(analog1 As String, analog2 As String, digitalIn As String, digitalOut As String)
    Public Event PacketLogged(sentHex As String, receivedHex As String)

    Public ReadOnly Property IsConnected As Boolean
        Get
            Return _serialPort IsNot Nothing AndAlso _serialPort.IsOpen
        End Get
    End Property

    Public Sub Connect(portName As String)
        If IsConnected Then Return

        _serialPort = New SerialPort(portName) With {
            .BaudRate = 9600,
            .Parity = Parity.None,
            .DataBits = 8,
            .StopBits = StopBits.One,
            .ReadTimeout = 500,
            .WriteTimeout = 500
        }

        Try
            _serialPort.Open()

            ' Start polling timer
            _pollTimer = New Timer(PollIntervalMs) With {
                .AutoReset = True
            }
            AddHandler _pollTimer.Elapsed, AddressOf PollTimer_Elapsed
            _pollTimer.Start()

            ' Immediate first poll
            PerformPoll()

        Catch ex As Exception
            Disconnect()
            Throw
        End Try
    End Sub

    Public Sub Disconnect()
        If _pollTimer IsNot Nothing Then
            _pollTimer.Stop()
            _pollTimer.Dispose()
            _pollTimer = Nothing
        End If

        If _serialPort IsNot Nothing Then
            If _serialPort.IsOpen Then _serialPort.Close()
            _serialPort.Dispose()
            _serialPort = Nothing
        End If
    End Sub

    Private Sub PollTimer_Elapsed(sender As Object, e As ElapsedEventArgs)
        PerformPoll()
    End Sub

    Private Sub PerformPoll()
        If Not IsConnected Then Return

        Try
            ' Send the three read commands in one batch
            Dim commands() As Byte = {&H51, &H52, &H30}
            _serialPort.Write(commands, 0, commands.Length)

            ' Convert sent to hex string for logging
            Dim sentHex As String = String.Join(" ", Array.ConvertAll(commands, Function(b) b.ToString("X2")))

            ' Expected response: 8 bytes
            Dim buffer(7) As Byte
            _serialPort.Read(buffer, 0, 8)

            ' Convert received to hex string for logging
            Dim receivedHex As String = String.Join(" ", Array.ConvertAll(buffer, Function(b) b.ToString("X2")))

            ' Log the packet exchange
            RaiseEvent PacketLogged(sentHex, receivedHex)

            ' Parse responses - fixed positions based on observed batch response format
            ' 0: Analog1 MSB, 1: Analog1 LSB, 2: Analog2 MSB, 3: Analog2 LSB,
            ' 4: Digital Inputs byte, 5-7: unused/zero
            Dim a1Msb As Byte = buffer(0)
            Dim a1Lsb As Byte = buffer(1)
            Dim a2Msb As Byte = buffer(2)
            Dim a2Lsb As Byte = buffer(3)
            Dim diByte As Byte = buffer(4)

            _analog1Raw = a1Msb.ToString("X2") & " " & a1Lsb.ToString("X2")
            _analog2Raw = a2Msb.ToString("X2") & " " & a2Lsb.ToString("X2")
            _digitalInRaw = diByte.ToString("X2")

            RaiseEvent DataUpdated(_analog1Raw, _analog2Raw, _digitalInRaw, _digitalOutRaw)

        Catch ex As TimeoutException
            ' Log timeout
            RaiseEvent PacketLogged(String.Join(" ", Array.ConvertAll(New Byte() {&H51, &H52, &H30}, Function(b) b.ToString("X2"))), "Timeout")
        Catch ex As Exception
            ' Other serial errors - stop polling until reconnect
            Disconnect()
        End Try
    End Sub

    Public Sub SendDigitalOutput(value As Byte)
        If Not IsConnected Then Return

        Try
            Dim cmd() As Byte = {&H20, value}
            _serialPort.Write(cmd, 0, cmd.Length)
            _digitalOutRaw = value.ToString("X2")

            RaiseEvent DataUpdated(_analog1Raw, _analog2Raw, _digitalInRaw, _digitalOutRaw)

            ' Log the output command
            RaiseEvent PacketLogged(&H20.ToString("X2") & " " & value.ToString("X2"), "")
        Catch
            ' Ignore write errors
        End Try
    End Sub

    Public Function SendReadStatus() As String
        If Not IsConnected Then Return "Not Connected"

        Try
            Dim cmd() As Byte = {&H10}
            _serialPort.Write(cmd, 0, cmd.Length)

            Dim buffer(0) As Byte
            _serialPort.Read(buffer, 0, 1)

            Dim sentHex As String = &H10.ToString("X2")
            Dim receivedHex As String = buffer(0).ToString("X2")

            RaiseEvent PacketLogged(sentHex, receivedHex)

            Return receivedHex
        Catch ex As TimeoutException
            RaiseEvent PacketLogged(&H10.ToString("X2"), "Timeout")
            Return "Timeout"
        Catch ex As Exception
            Return "Error: " & ex.Message
        End Try
    End Function

    Protected Overrides Sub Finalize()
        Disconnect()
        MyBase.Finalize()
    End Sub

End Class