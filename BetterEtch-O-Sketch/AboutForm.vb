Public Class AboutForm
    Private Sub AboutForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
    ' =========================================================================
    ' SECTION: UI INTERACTION
    ' Handles the user closing the modal About information window.
    ' =========================================================================

    ''' <summary>
    ''' Closes the AboutForm and returns control to the parent EtchASketch form.
    ''' </summary>
    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        ' Close the current form instance and release resources
        Me.Close()
    End Sub
End Class