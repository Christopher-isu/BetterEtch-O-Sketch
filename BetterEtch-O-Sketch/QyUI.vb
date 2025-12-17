'Christopher Z
'Fall 2025
'Better Etch-O-Sketch Project
'https://github.com/Christopher-isu/BetterEtch-O-Sketch.git

Option Strict On ' Enforces strict data typing to ensure reliable hex-to-byte conversions
Option Explicit On ' Requires explicit variable declarations for better maintainability

Imports System.IO.Ports ' Required for COM port awareness

' =========================================================================================
' CLASS: QyUI
' PURPOSE: Provides a secondary diagnostic window for real-time hardware monitoring. 
'          It displays raw sensor packets (Analog/Digital) and allows manual command 
'          testing without interfering with the main EtchASketch drawing logic.
' =========================================================================================
Public Class QyUI

    ' -------------------------------------------------------------------------------------
    ' SECTION 1: FIELDS & REFERENCES
    ' -------------------------------------------------------------------------------------

    ' Holds a reference to the QyHandler instance owned by the EtchASketch form.
    ' This ensures both forms look at the same physical serial connection.
    Private ReadOnly _handlerRef As QyHandler

    ' Alternating bit patterns (10101010 vs 01010101) used to test the board's LED outputs.
    Private testOutputValue As Byte = CByte(&HAA)

    ' -------------------------------------------------------------------------------------
    ' SECTION 2: CONSTRUCTOR & INITIALIZATION
    ' Logic for linking to the existing serial engine and preparing the UI.
    ' -------------------------------------------------------------------------------------

    ''' <summary>
    ''' Constructor that dependency-injects the active serial handler.
    ''' </summary>
    ''' <param name="activeHandler">The running instance of QyHandler from the Main Form.</param>
    Public Sub New(activeHandler As QyHandler)
        InitializeComponent() ' Necessary for WinForms designer support

        ' Link this diagnostic window to the shared communication engine
        _handlerRef = activeHandler

        ' Verify if we should allow testing commands based on connection state
        If btnTestOutput IsNot Nothing Then
            btnTestOutput.Enabled = _handlerRef.IsConnected
        End If

        ' Clear all text fields to their "neutral" hex states
        ClearDataFields()
    End Sub

    ''' <summary>
    ''' Subscription point for hardware events.
    ''' </summary>
    Private Sub QyUI_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Only listen to the board if it is currently connected
        If _handlerRef.IsConnected Then
            ' Bind the UI update methods to the handler's event broadcasts
            AddHandler _handlerRef.DataUpdated, AddressOf Handler_DataUpdated
            AddHandler _handlerRef.PacketLogged, AddressOf Handler_PacketLogged
        End If
    End Sub

    ' -------------------------------------------------------------------------------------
    ' SECTION 3: DIAGNOSTIC COMMANDS
    ' Methods that trigger manual requests or outputs to the PIC.
    ' -------------------------------------------------------------------------------------

    ''' <summary>
    ''' Sends a test byte to the board's digital outputs and toggles the pattern for the next click.
    ''' </summary>
    Private Sub btnTestOutput_Click(sender As Object, e As EventArgs) Handles btnTestOutput.Click
        If Not _handlerRef.IsConnected Then
            MessageBox.Show("Not connected.")
            Return
        End If

        ' Send the current pattern (AA or 55) to the board
        _handlerRef.SendDigitalOutput(testOutputValue)

        ' Toggle the value: 10101010 becomes 01010101 and vice versa
        testOutputValue = If(testOutputValue = CByte(&HAA), CByte(&H55), CByte(&HAA))
    End Sub

    ''' <summary>
    ''' Directly queries the PIC for its internal status register (Command 0x10).
    ''' </summary>
    Private Sub btnStatus_Click(sender As Object, e As EventArgs) Handles btnStatus.Click
        If Not _handlerRef.IsConnected Then
            MessageBox.Show("Not connected.")
            Return
        End If

        ' Performs a synchronous read/write through the handler
        Dim response As String = _handlerRef.SendReadStatus()
        MessageBox.Show("Status Response: " & response)
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs)
        Me.Close()
    End Sub

    ' -------------------------------------------------------------------------------------
    ' SECTION 4: THREAD-SAFE UI UPDATING
    ' These methods handle data arriving from the background Serial thread.
    ' -------------------------------------------------------------------------------------

    ''' <summary>
    ''' Updates the text fields showing the interpreted Analog and Digital values.
    ''' </summary>
    Private Sub Handler_DataUpdated(analog1 As String, analog2 As String, digitalIn As String, digitalOut As String)
        ' InvokeRequired check: Prevents cross-thread exceptions since Serial runs on a background thread
        If Me.InvokeRequired Then
            ' Use BeginInvoke for non-blocking UI updates
            Me.BeginInvoke(New Action(Of String, String, String, String)(AddressOf Handler_DataUpdated),
                             analog1, analog2, digitalIn, digitalOut)
            Return
        End If

        ' Reflect the raw hex values in the diagnostic text boxes
        txtAnalog1.Text = analog1
        txtAnalog2.Text = analog2
        txtDigitalIn.Text = digitalIn
        txtDigitalOut.Text = digitalOut
    End Sub

    ''' <summary>
    ''' Logs the exact Byte sequence sent and received into the scrolling listbox.
    ''' </summary>
    Private Sub Handler_PacketLogged(sentHex As String, receivedHex As String)
        If Me.InvokeRequired Then
            Me.BeginInvoke(New Action(Of String, String)(AddressOf Handler_PacketLogged),
                             sentHex, receivedHex)
            Return
        End If

        ' Format the entry for the log
        Dim logEntry As String = "Sent: " & sentHex & " | Recv: " & receivedHex

        ' Insert at index 0 so the most recent traffic is always at the top
        lstPackets.Items.Insert(0, logEntry)

        ' PERFORMANCE: Trim the list to 5 items to keep the diagnostic window lightweight
        While lstPackets.Items.Count > 5
            lstPackets.Items.RemoveAt(lstPackets.Items.Count - 1)
        End While
    End Sub

    ' -------------------------------------------------------------------------------------
    ' SECTION 5: CLEANUP & DISPOSAL
    ' -------------------------------------------------------------------------------------

    ''' <summary>
    ''' Sets default text for the fields when the form resets or initializes.
    ''' </summary>
    Private Sub ClearDataFields()
        txtAnalog1.Text = "00 00"
        txtAnalog2.Text = "00 00"
        txtDigitalIn.Text = "00"
        txtDigitalOut.Text = "00"
    End Sub

    ''' <summary>
    ''' Cleans up event subscriptions without closing the global Serial connection.
    ''' </summary>
    Private Sub QyUI_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        ' IMPORTANT: We do NOT call _handlerRef.Disconnect() here.
        ' Doing so would kill the drawing capabilities of the main EtchASketch form.
        If _handlerRef IsNot Nothing Then
            Try
                ' Detach the event handlers to prevent memory leaks while the form is closed
                RemoveHandler _handlerRef.DataUpdated, AddressOf Handler_DataUpdated
                RemoveHandler _handlerRef.PacketLogged, AddressOf Handler_PacketLogged
            Catch
                ' Ignore errors if handlers were not attached
            End Try
        End If
    End Sub
End Class