'ChristopherZ
'Fall 2025
'RCET3371
'Better Etch-O-Sketch
'https://github.com/Christopher-isu/BetterEtch-O-Sketch.git

Option Strict On
Option Explicit On
Imports Microsoft.VisualBasic.Logging

''' <summary>
''' The "Under the Hood" Diagnostic Form. 
''' Displays raw serial traffic to verify hardware health and sensor stability.
''' </summary>
Public Class QyUI
    ' Reference to the active handler created by the EtchASketch form
    Private _handler As QyHandler

    ''' <summary>
    ''' Constructor: Requires an existing handler so we aren't opening two serial ports.
    ''' </summary>
    Public Sub New(h As QyHandler)
        InitializeComponent()
        _handler = h
    End Sub

    ' =========================================================================
    ' SECTION 1: EVENT SUBSCRIPTIONS
    ' =========================================================================

    Private Sub QyUI_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Subscribe to both the data flow and the raw packet log
        AddHandler _handler.DataUpdated, AddressOf OnDataUpdated
        AddHandler _handler.PacketLogged, AddressOf OnPacketLogged
    End Sub

    ''' <summary>
    ''' Updates the "Data Boxes" in the UI with the latest hex values.
    ''' </summary>
    Private Sub OnDataUpdated(a1 As String, a2 As String, di As String, dou As String)
        ' Ensure thread safety: QyHandler events fire on a background thread
        If Me.InvokeRequired Then
            Me.Invoke(New Action(Of String, String, String, String)(AddressOf OnDataUpdated), a1, a2, di, dou)
            Return
        End If

        ' Update the read-only textboxes for the user to see
        txtAnalog1.Text = a1
        txtAnalog2.Text = a2
        txtDigitalIn.Text = di
        txtDigitalOut.Text = dou
    End Sub

    ''' <summary>
    ''' Appends raw Hex traffic to the scrolling log window.
    ''' </summary>
    Private Sub OnPacketLogged(sent As String, received As String)
        If Me.InvokeRequired Then
            Me.Invoke(New Action(Of String, String)(AddressOf OnPacketLogged), sent, received)
            Return
        End If

        ' Format: [TX] 51 | [RX] 02 44
        lstLog.Items.Add($"[TX] {sent} | [RX] {received}")

        ' Auto-scroll to the bottom of the log
        lstLog.SelectedIndex = lstLog.Items.Count - 1
    End Sub

    ' =========================================================================
    ' SECTION 2: HARDWARE TESTING
    ' =========================================================================

    ''' <summary>
    ''' Manually triggers a Digital Output command (LED test).
    ''' </summary>
    Private Sub btnSend_Click(sender As Object, e As EventArgs) Handles btnSend.Click
        Try
            ' Convert user-entered hex string (e.g., "FF") to a byte
            Dim val As Byte = Convert.ToByte(txtSendHex.Text, 16)
            _handler.SendDigitalOutput(val)
        Catch
            MessageBox.Show("Please enter a valid Hex byte (00-FF)")
        End Try
    End Sub

    Private Sub QyUI_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        ' Unsubscribe to prevent memory leaks and background crashes
        RemoveHandler _handler.DataUpdated, AddressOf OnDataUpdated
        RemoveHandler _handler.PacketLogged, AddressOf OnPacketLogged
    End Sub
End Class