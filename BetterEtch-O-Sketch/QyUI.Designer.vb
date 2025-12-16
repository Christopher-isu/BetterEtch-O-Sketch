<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class QyUI
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.lblAnalog1 = New System.Windows.Forms.Label()
        Me.txtAnalog1 = New System.Windows.Forms.TextBox()
        Me.lblAnalog2 = New System.Windows.Forms.Label()
        Me.txtAnalog2 = New System.Windows.Forms.TextBox()
        Me.lblDigitalIn = New System.Windows.Forms.Label()
        Me.txtDigitalIn = New System.Windows.Forms.TextBox()
        Me.lblDigitalOut = New System.Windows.Forms.Label()
        Me.txtDigitalOut = New System.Windows.Forms.TextBox()
        Me.grpControls = New System.Windows.Forms.GroupBox()
        Me.lblPort = New System.Windows.Forms.Label()
        Me.grpData = New System.Windows.Forms.GroupBox()
        Me.btnTestOutput = New System.Windows.Forms.Button()
        Me.btnStatus = New System.Windows.Forms.Button()
        Me.grpPackets = New System.Windows.Forms.GroupBox()
        Me.lstPackets = New System.Windows.Forms.ListBox()
        Me.grpControls.SuspendLayout()
        Me.grpData.SuspendLayout()
        Me.grpPackets.SuspendLayout()
        Me.SuspendLayout()
        '
        'lblAnalog1
        '
        Me.lblAnalog1.AutoSize = True
        Me.lblAnalog1.Location = New System.Drawing.Point(30, 31)
        Me.lblAnalog1.Name = "lblAnalog1"
        Me.lblAnalog1.Size = New System.Drawing.Size(98, 16)
        Me.lblAnalog1.TabIndex = 0
        Me.lblAnalog1.Text = "Analog1 (0x51):"
        '
        'txtAnalog1
        '
        Me.txtAnalog1.Location = New System.Drawing.Point(146, 28)
        Me.txtAnalog1.Name = "txtAnalog1"
        Me.txtAnalog1.ReadOnly = True
        Me.txtAnalog1.Size = New System.Drawing.Size(44, 22)
        Me.txtAnalog1.TabIndex = 1
        Me.txtAnalog1.Text = "00 00"
        '
        'lblAnalog2
        '
        Me.lblAnalog2.AutoSize = True
        Me.lblAnalog2.Location = New System.Drawing.Point(30, 61)
        Me.lblAnalog2.Name = "lblAnalog2"
        Me.lblAnalog2.Size = New System.Drawing.Size(98, 16)
        Me.lblAnalog2.TabIndex = 2
        Me.lblAnalog2.Text = "Analog2 (0x52):"
        '
        'txtAnalog2
        '
        Me.txtAnalog2.Location = New System.Drawing.Point(146, 58)
        Me.txtAnalog2.Name = "txtAnalog2"
        Me.txtAnalog2.ReadOnly = True
        Me.txtAnalog2.Size = New System.Drawing.Size(44, 22)
        Me.txtAnalog2.TabIndex = 3
        Me.txtAnalog2.Text = "00 00"
        '
        'lblDigitalIn
        '
        Me.lblDigitalIn.AutoSize = True
        Me.lblDigitalIn.Location = New System.Drawing.Point(29, 91)
        Me.lblDigitalIn.Name = "lblDigitalIn"
        Me.lblDigitalIn.Size = New System.Drawing.Size(99, 16)
        Me.lblDigitalIn.TabIndex = 2
        Me.lblDigitalIn.Text = "Digital In (0x30):"
        '
        'txtDigitalIn
        '
        Me.txtDigitalIn.Location = New System.Drawing.Point(146, 88)
        Me.txtDigitalIn.Name = "txtDigitalIn"
        Me.txtDigitalIn.ReadOnly = True
        Me.txtDigitalIn.Size = New System.Drawing.Size(44, 22)
        Me.txtDigitalIn.TabIndex = 3
        Me.txtDigitalIn.Text = "00"
        '
        'lblDigitalOut
        '
        Me.lblDigitalOut.AutoSize = True
        Me.lblDigitalOut.Location = New System.Drawing.Point(19, 121)
        Me.lblDigitalOut.Name = "lblDigitalOut"
        Me.lblDigitalOut.Size = New System.Drawing.Size(109, 16)
        Me.lblDigitalOut.TabIndex = 2
        Me.lblDigitalOut.Text = "Digital Out (0x20):"
        '
        'txtDigitalOut
        '
        Me.txtDigitalOut.Location = New System.Drawing.Point(146, 118)
        Me.txtDigitalOut.Name = "txtDigitalOut"
        Me.txtDigitalOut.ReadOnly = True
        Me.txtDigitalOut.Size = New System.Drawing.Size(44, 22)
        Me.txtDigitalOut.TabIndex = 3
        Me.txtDigitalOut.Text = "00"
        '
        'grpControls
        '
        Me.grpControls.Controls.Add(Me.lblPort)
        Me.grpControls.Location = New System.Drawing.Point(9, 15)
        Me.grpControls.Name = "grpControls"
        Me.grpControls.Size = New System.Drawing.Size(181, 248)
        Me.grpControls.TabIndex = 4
        Me.grpControls.TabStop = False
        Me.grpControls.Text = "Controls"
        '
        'lblPort
        '
        Me.lblPort.AutoSize = True
        Me.lblPort.Location = New System.Drawing.Point(34, 88)
        Me.lblPort.Name = "lblPort"
        Me.lblPort.Size = New System.Drawing.Size(112, 16)
        Me.lblPort.TabIndex = 0
        Me.lblPort.Text = "Controls disabled"
        '
        'grpData
        '
        Me.grpData.Controls.Add(Me.btnTestOutput)
        Me.grpData.Controls.Add(Me.btnStatus)
        Me.grpData.Controls.Add(Me.lblAnalog2)
        Me.grpData.Controls.Add(Me.lblAnalog1)
        Me.grpData.Controls.Add(Me.txtDigitalOut)
        Me.grpData.Controls.Add(Me.txtAnalog1)
        Me.grpData.Controls.Add(Me.txtDigitalIn)
        Me.grpData.Controls.Add(Me.lblDigitalIn)
        Me.grpData.Controls.Add(Me.txtAnalog2)
        Me.grpData.Controls.Add(Me.lblDigitalOut)
        Me.grpData.Location = New System.Drawing.Point(219, 15)
        Me.grpData.Name = "grpData"
        Me.grpData.Size = New System.Drawing.Size(210, 248)
        Me.grpData.TabIndex = 5
        Me.grpData.TabStop = False
        Me.grpData.Text = "Data"
        '
        'btnTestOutput
        '
        Me.btnTestOutput.Location = New System.Drawing.Point(51, 164)
        Me.btnTestOutput.Name = "btnTestOutput"
        Me.btnTestOutput.Size = New System.Drawing.Size(126, 23)
        Me.btnTestOutput.TabIndex = 3
        Me.btnTestOutput.Text = "Output (0x20)"
        Me.btnTestOutput.UseVisualStyleBackColor = True
        '
        'btnStatus
        '
        Me.btnStatus.Location = New System.Drawing.Point(51, 206)
        Me.btnStatus.Name = "btnStatus"
        Me.btnStatus.Size = New System.Drawing.Size(126, 23)
        Me.btnStatus.TabIndex = 3
        Me.btnStatus.Text = "Status (0x10)"
        Me.btnStatus.UseVisualStyleBackColor = True
        '
        'grpPackets
        '
        Me.grpPackets.Controls.Add(Me.lstPackets)
        Me.grpPackets.Location = New System.Drawing.Point(9, 274)
        Me.grpPackets.Name = "grpPackets"
        Me.grpPackets.Size = New System.Drawing.Size(419, 103)
        Me.grpPackets.TabIndex = 6
        Me.grpPackets.TabStop = False
        Me.grpPackets.Text = "GroupBox1"
        '
        'lstPackets
        '
        Me.lstPackets.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lstPackets.FormattingEnabled = True
        Me.lstPackets.ItemHeight = 16
        Me.lstPackets.Location = New System.Drawing.Point(3, 18)
        Me.lstPackets.Name = "lstPackets"
        Me.lstPackets.Size = New System.Drawing.Size(413, 82)
        Me.lstPackets.TabIndex = 0
        '
        'QyUI
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(434, 380)
        Me.Controls.Add(Me.grpPackets)
        Me.Controls.Add(Me.grpData)
        Me.Controls.Add(Me.grpControls)
        Me.Name = "QyUI"
        Me.Text = "QiUI"
        Me.grpControls.ResumeLayout(False)
        Me.grpControls.PerformLayout()
        Me.grpData.ResumeLayout(False)
        Me.grpData.PerformLayout()
        Me.grpPackets.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents lblAnalog1 As Label
    Friend WithEvents txtAnalog1 As TextBox
    Friend WithEvents lblAnalog2 As Label
    Friend WithEvents txtAnalog2 As TextBox
    Friend WithEvents lblDigitalIn As Label
    Friend WithEvents txtDigitalIn As TextBox
    Friend WithEvents lblDigitalOut As Label
    Friend WithEvents txtDigitalOut As TextBox
    Friend WithEvents grpControls As GroupBox
    Friend WithEvents grpData As GroupBox
    Friend WithEvents lblPort As Label
    Friend WithEvents grpPackets As GroupBox
    Friend WithEvents btnStatus As Button
    Friend WithEvents lstPackets As ListBox
    Friend WithEvents btnTestOutput As Button
End Class
