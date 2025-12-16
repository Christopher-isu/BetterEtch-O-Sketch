Option Strict On
Option Explicit On

Imports System.IO.Ports
' NOTE: We must ensure QyHandler is accessible here.

Public Class QyUI
    ' Private ReadOnly handler As New QyHandler() <-- REMOVED!

    Private ReadOnly _handlerRef As QyHandler ' Reference to the main application's handler instance
    Private testOutputValue As Byte = CByte(&HAA)

    ' NEW: Constructor to accept the active QyHandler instance
    Public Sub New(activeHandler As QyHandler)
        InitializeComponent() ' Required for WinForms forms

        ' Set the internal reference
        _handlerRef = activeHandler

        ' Ensure buttons irrelevant to diagnosis are disabled/removed if they exist
        ' (Assuming btnConnect, btnRefresh, cboPorts were removed in designer)
        If btnTestOutput IsNot Nothing Then
            ' Only enable test output if connected when the form loads
            btnTestOutput.Enabled = _handlerRef.IsConnected
        End If

        ' Initial state clear
        ClearDataFields()
    End Sub

    Private Sub QyUI_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Old RefreshPortList() logic is no longer needed here.

        If _handlerRef.IsConnected Then
            ' Only add handlers if a connection is active (or will be immediately activated by the main form)
            AddHandler _handlerRef.DataUpdated, AddressOf Handler_DataUpdated
            AddHandler _handlerRef.PacketLogged, AddressOf Handler_PacketLogged
        End If
    End Sub

    ' The RefreshPortList and btnRefresh_Click are removed/not needed.
    ' The btnConnect_Click is removed/not needed.

    Private Sub btnTestOutput_Click(sender As Object, e As EventArgs) Handles btnTestOutput.Click
        If Not _handlerRef.IsConnected Then
            MessageBox.Show("Not connected.")
            Return
        End If

        _handlerRef.SendDigitalOutput(testOutputValue)
        ' Toggle the value
        testOutputValue = If(testOutputValue = CByte(&HAA), CByte(&H55), CByte(&HAA))
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs)
        Me.Close()
    End Sub

    Private Sub btnStatus_Click(sender As Object, e As EventArgs) Handles btnStatus.Click
        If Not _handlerRef.IsConnected Then
            MessageBox.Show("Not connected.")
            Return
        End If

        Dim response As String = _handlerRef.SendReadStatus()
        MessageBox.Show("Status Response: " & response)
    End Sub

    Private Sub Handler_DataUpdated(analog1 As String, analog2 As String, digitalIn As String, digitalOut As String)
        If Me.InvokeRequired Then
            ' Use BeginInvoke for non-critical, UI-updating events
            Me.BeginInvoke(New Action(Of String, String, String, String)(AddressOf Handler_DataUpdated),
                             analog1, analog2, digitalIn, digitalOut)
            Return
        End If

        txtAnalog1.Text = analog1
        txtAnalog2.Text = analog2
        txtDigitalIn.Text = digitalIn
        txtDigitalOut.Text = digitalOut
    End Sub

    Private Sub Handler_PacketLogged(sentHex As String, receivedHex As String)
        If Me.InvokeRequired Then
            Me.BeginInvoke(New Action(Of String, String)(AddressOf Handler_PacketLogged),
                             sentHex, receivedHex)
            Return
        End If

        Dim logEntry As String = "Sent: " & sentHex & " | Recv: " & receivedHex
        lstPackets.Items.Insert(0, logEntry)

        While lstPackets.Items.Count > 5
            lstPackets.Items.RemoveAt(lstPackets.Items.Count - 1)
        End While
    End Sub

    Private Sub ClearDataFields()
        txtAnalog1.Text = "00 00"
        txtAnalog2.Text = "00 00"
        txtDigitalIn.Text = "00"
        txtDigitalOut.Text = "00"
    End Sub

    Private Sub QyUI_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        ' CRITICAL CHANGE: Do NOT disconnect the handler!
        ' The handler instance belongs to the main form (EtchASketch).
        If _handlerRef IsNot Nothing Then
            Try
                RemoveHandler _handlerRef.DataUpdated, AddressOf Handler_DataUpdated
                RemoveHandler _handlerRef.PacketLogged, AddressOf Handler_PacketLogged
            Catch
                ' Ignore if handlers already removed
            End Try
        End If
        ' The form closes, but the connection remains active in the main app.
    End Sub
End Class