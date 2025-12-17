<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class EtchASketch
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
        Me.ButtonPanel = New System.Windows.Forms.Panel()
        Me.grpMode = New System.Windows.Forms.GroupBox()
        Me.rbtnExternalMode = New System.Windows.Forms.RadioButton()
        Me.rbtnMouseMode = New System.Windows.Forms.RadioButton()
        Me.ExitButton = New System.Windows.Forms.Button()
        Me.ClearButton = New System.Windows.Forms.Button()
        Me.DrawWaveformsButton = New System.Windows.Forms.Button()
        Me.SelectColorButton = New System.Windows.Forms.Button()
        Me.MenuStrip1 = New System.Windows.Forms.MenuStrip()
        Me.FileMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ExitMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.EditMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.SelectColorMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DrawWaveformsMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ClearMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.HelpMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.AboutMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ConnectToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuConnect = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuDisconnect = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuDiag = New System.Windows.Forms.ToolStripMenuItem()
        Me.DisplayPictureBox = New System.Windows.Forms.PictureBox()
        Me.lblStatusCaption = New System.Windows.Forms.Label()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.lblPortCaption = New System.Windows.Forms.Label()
        Me.lblPortValue = New System.Windows.Forms.Label()
        Me.ButtonPanel.SuspendLayout()
        Me.grpMode.SuspendLayout()
        Me.MenuStrip1.SuspendLayout()
        CType(Me.DisplayPictureBox, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'ButtonPanel
        '
        Me.ButtonPanel.Controls.Add(Me.grpMode)
        Me.ButtonPanel.Controls.Add(Me.ExitButton)
        Me.ButtonPanel.Controls.Add(Me.ClearButton)
        Me.ButtonPanel.Controls.Add(Me.DrawWaveformsButton)
        Me.ButtonPanel.Controls.Add(Me.SelectColorButton)
        Me.ButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.ButtonPanel.Location = New System.Drawing.Point(0, 431)
        Me.ButtonPanel.Name = "ButtonPanel"
        Me.ButtonPanel.Size = New System.Drawing.Size(800, 52)
        Me.ButtonPanel.TabIndex = 2
        '
        'grpMode
        '
        Me.grpMode.Controls.Add(Me.rbtnExternalMode)
        Me.grpMode.Controls.Add(Me.rbtnMouseMode)
        Me.grpMode.Location = New System.Drawing.Point(515, 3)
        Me.grpMode.Name = "grpMode"
        Me.grpMode.Size = New System.Drawing.Size(282, 40)
        Me.grpMode.TabIndex = 4
        Me.grpMode.TabStop = False
        Me.grpMode.Text = "Control"
        '
        'rbtnExternalMode
        '
        Me.rbtnExternalMode.AutoSize = True
        Me.rbtnExternalMode.Location = New System.Drawing.Point(142, 15)
        Me.rbtnExternalMode.Name = "rbtnExternalMode"
        Me.rbtnExternalMode.Size = New System.Drawing.Size(108, 20)
        Me.rbtnExternalMode.TabIndex = 5
        Me.rbtnExternalMode.Text = "External (PIC)"
        Me.rbtnExternalMode.UseVisualStyleBackColor = True
        '
        'rbtnMouseMode
        '
        Me.rbtnMouseMode.AutoSize = True
        Me.rbtnMouseMode.Checked = True
        Me.rbtnMouseMode.Location = New System.Drawing.Point(55, 15)
        Me.rbtnMouseMode.Name = "rbtnMouseMode"
        Me.rbtnMouseMode.Size = New System.Drawing.Size(69, 20)
        Me.rbtnMouseMode.TabIndex = 4
        Me.rbtnMouseMode.TabStop = True
        Me.rbtnMouseMode.Text = "Mouse"
        Me.rbtnMouseMode.UseVisualStyleBackColor = True
        '
        'ExitButton
        '
        Me.ExitButton.Location = New System.Drawing.Point(397, 0)
        Me.ExitButton.Name = "ExitButton"
        Me.ExitButton.Size = New System.Drawing.Size(115, 50)
        Me.ExitButton.TabIndex = 3
        Me.ExitButton.Text = "&Exit"
        Me.ExitButton.UseVisualStyleBackColor = True
        '
        'ClearButton
        '
        Me.ClearButton.Location = New System.Drawing.Point(258, 0)
        Me.ClearButton.Name = "ClearButton"
        Me.ClearButton.Size = New System.Drawing.Size(133, 50)
        Me.ClearButton.TabIndex = 2
        Me.ClearButton.Text = "&Clear"
        Me.ClearButton.UseVisualStyleBackColor = True
        '
        'DrawWaveformsButton
        '
        Me.DrawWaveformsButton.Location = New System.Drawing.Point(127, 0)
        Me.DrawWaveformsButton.Name = "DrawWaveformsButton"
        Me.DrawWaveformsButton.Size = New System.Drawing.Size(125, 50)
        Me.DrawWaveformsButton.TabIndex = 1
        Me.DrawWaveformsButton.Text = "&Draw Waveforms"
        Me.DrawWaveformsButton.UseVisualStyleBackColor = True
        '
        'SelectColorButton
        '
        Me.SelectColorButton.Location = New System.Drawing.Point(0, 0)
        Me.SelectColorButton.Name = "SelectColorButton"
        Me.SelectColorButton.Size = New System.Drawing.Size(121, 50)
        Me.SelectColorButton.TabIndex = 0
        Me.SelectColorButton.Text = "&Select Color"
        Me.SelectColorButton.UseVisualStyleBackColor = True
        '
        'MenuStrip1
        '
        Me.MenuStrip1.ImageScalingSize = New System.Drawing.Size(20, 20)
        Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.FileMenuItem, Me.EditMenuItem, Me.HelpMenuItem, Me.ConnectToolStripMenuItem})
        Me.MenuStrip1.Location = New System.Drawing.Point(0, 0)
        Me.MenuStrip1.Name = "MenuStrip1"
        Me.MenuStrip1.Size = New System.Drawing.Size(800, 28)
        Me.MenuStrip1.TabIndex = 3
        Me.MenuStrip1.Text = "MenuStrip1"
        '
        'FileMenuItem
        '
        Me.FileMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ExitMenuItem})
        Me.FileMenuItem.Name = "FileMenuItem"
        Me.FileMenuItem.Size = New System.Drawing.Size(46, 24)
        Me.FileMenuItem.Text = "&File"
        '
        'ExitMenuItem
        '
        Me.ExitMenuItem.Name = "ExitMenuItem"
        Me.ExitMenuItem.Size = New System.Drawing.Size(116, 26)
        Me.ExitMenuItem.Text = "E&xit"
        '
        'EditMenuItem
        '
        Me.EditMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.SelectColorMenuItem, Me.DrawWaveformsMenuItem, Me.ClearMenuItem})
        Me.EditMenuItem.Name = "EditMenuItem"
        Me.EditMenuItem.Size = New System.Drawing.Size(49, 24)
        Me.EditMenuItem.Text = "&Edit"
        '
        'SelectColorMenuItem
        '
        Me.SelectColorMenuItem.Name = "SelectColorMenuItem"
        Me.SelectColorMenuItem.Size = New System.Drawing.Size(205, 26)
        Me.SelectColorMenuItem.Text = "Set &Color"
        '
        'DrawWaveformsMenuItem
        '
        Me.DrawWaveformsMenuItem.Name = "DrawWaveformsMenuItem"
        Me.DrawWaveformsMenuItem.Size = New System.Drawing.Size(205, 26)
        Me.DrawWaveformsMenuItem.Text = "Draw &Waveforms"
        '
        'ClearMenuItem
        '
        Me.ClearMenuItem.Name = "ClearMenuItem"
        Me.ClearMenuItem.Size = New System.Drawing.Size(205, 26)
        Me.ClearMenuItem.Text = "&Clear"
        '
        'HelpMenuItem
        '
        Me.HelpMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.AboutMenuItem})
        Me.HelpMenuItem.Name = "HelpMenuItem"
        Me.HelpMenuItem.Size = New System.Drawing.Size(55, 24)
        Me.HelpMenuItem.Text = "&Help"
        '
        'AboutMenuItem
        '
        Me.AboutMenuItem.Name = "AboutMenuItem"
        Me.AboutMenuItem.Size = New System.Drawing.Size(133, 26)
        Me.AboutMenuItem.Text = "Abo&ut"
        '
        'ConnectToolStripMenuItem
        '
        Me.ConnectToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuConnect, Me.mnuDisconnect, Me.mnuDiag})
        Me.ConnectToolStripMenuItem.Name = "ConnectToolStripMenuItem"
        Me.ConnectToolStripMenuItem.Size = New System.Drawing.Size(98, 24)
        Me.ConnectToolStripMenuItem.Text = "&Connection"
        '
        'mnuConnect
        '
        Me.mnuConnect.Name = "mnuConnect"
        Me.mnuConnect.Size = New System.Drawing.Size(165, 26)
        Me.mnuConnect.Text = "&Connect"
        '
        'mnuDisconnect
        '
        Me.mnuDisconnect.Name = "mnuDisconnect"
        Me.mnuDisconnect.Size = New System.Drawing.Size(165, 26)
        Me.mnuDisconnect.Text = "&Disconnect"
        '
        'mnuDiag
        '
        Me.mnuDiag.Name = "mnuDiag"
        Me.mnuDiag.Size = New System.Drawing.Size(165, 26)
        Me.mnuDiag.Text = "&Diag"
        '
        'DisplayPictureBox
        '
        Me.DisplayPictureBox.BackColor = System.Drawing.Color.White
        Me.DisplayPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.DisplayPictureBox.Dock = System.Windows.Forms.DockStyle.Fill
        Me.DisplayPictureBox.Location = New System.Drawing.Point(0, 28)
        Me.DisplayPictureBox.Name = "DisplayPictureBox"
        Me.DisplayPictureBox.Size = New System.Drawing.Size(800, 403)
        Me.DisplayPictureBox.TabIndex = 4
        Me.DisplayPictureBox.TabStop = False
        '
        'lblStatusCaption
        '
        Me.lblStatusCaption.AutoSize = True
        Me.lblStatusCaption.Location = New System.Drawing.Point(389, 9)
        Me.lblStatusCaption.Name = "lblStatusCaption"
        Me.lblStatusCaption.Size = New System.Drawing.Size(47, 16)
        Me.lblStatusCaption.TabIndex = 5
        Me.lblStatusCaption.Text = "Status:"
        '
        'lblStatus
        '
        Me.lblStatus.AutoSize = True
        Me.lblStatus.ForeColor = System.Drawing.Color.Orange
        Me.lblStatus.Location = New System.Drawing.Point(438, 9)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(90, 16)
        Me.lblStatus.TabIndex = 6
        Me.lblStatus.Text = "Disconnected"
        '
        'lblPortCaption
        '
        Me.lblPortCaption.AutoSize = True
        Me.lblPortCaption.Location = New System.Drawing.Point(555, 9)
        Me.lblPortCaption.Name = "lblPortCaption"
        Me.lblPortCaption.Size = New System.Drawing.Size(34, 16)
        Me.lblPortCaption.TabIndex = 7
        Me.lblPortCaption.Text = "Port:"
        '
        'lblPortValue
        '
        Me.lblPortValue.AutoSize = True
        Me.lblPortValue.Location = New System.Drawing.Point(593, 9)
        Me.lblPortValue.Name = "lblPortValue"
        Me.lblPortValue.Size = New System.Drawing.Size(26, 16)
        Me.lblPortValue.TabIndex = 8
        Me.lblPortValue.Text = "n/a"
        '
        'EtchASketch
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(800, 483)
        Me.Controls.Add(Me.lblPortValue)
        Me.Controls.Add(Me.lblPortCaption)
        Me.Controls.Add(Me.lblStatus)
        Me.Controls.Add(Me.lblStatusCaption)
        Me.Controls.Add(Me.DisplayPictureBox)
        Me.Controls.Add(Me.MenuStrip1)
        Me.Controls.Add(Me.ButtonPanel)
        Me.Name = "EtchASketch"
        Me.Text = "Better Etch-O-Sketch"
        Me.ButtonPanel.ResumeLayout(False)
        Me.grpMode.ResumeLayout(False)
        Me.grpMode.PerformLayout()
        Me.MenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.PerformLayout()
        CType(Me.DisplayPictureBox, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents ButtonPanel As Panel
    Friend WithEvents ExitButton As Button
    Friend WithEvents ClearButton As Button
    Friend WithEvents DrawWaveformsButton As Button
    Friend WithEvents SelectColorButton As Button
    Friend WithEvents MenuStrip1 As MenuStrip
    Friend WithEvents FileMenuItem As ToolStripMenuItem
    Friend WithEvents ExitMenuItem As ToolStripMenuItem
    Friend WithEvents EditMenuItem As ToolStripMenuItem
    Friend WithEvents SelectColorMenuItem As ToolStripMenuItem
    Friend WithEvents DrawWaveformsMenuItem As ToolStripMenuItem
    Friend WithEvents ClearMenuItem As ToolStripMenuItem
    Friend WithEvents HelpMenuItem As ToolStripMenuItem
    Friend WithEvents AboutMenuItem As ToolStripMenuItem
    Friend WithEvents DisplayPictureBox As PictureBox
    Friend WithEvents grpMode As GroupBox
    Friend WithEvents ConnectToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents lblStatusCaption As Label
    Friend WithEvents lblStatus As Label
    Friend WithEvents lblPortCaption As Label
    Friend WithEvents lblPortValue As Label
    Friend WithEvents mnuConnect As ToolStripMenuItem
    Friend WithEvents mnuDisconnect As ToolStripMenuItem
    Friend WithEvents mnuDiag As ToolStripMenuItem
    Friend WithEvents rbtnExternalMode As RadioButton
    Friend WithEvents rbtnMouseMode As RadioButton
End Class