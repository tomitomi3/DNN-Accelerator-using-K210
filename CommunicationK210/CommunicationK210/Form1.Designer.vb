<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMain
    Inherits System.Windows.Forms.Form

    'フォームがコンポーネントの一覧をクリーンアップするために dispose をオーバーライドします。
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Windows フォーム デザイナーで必要です。
    Private components As System.ComponentModel.IContainer

    'メモ: 以下のプロシージャは Windows フォーム デザイナーで必要です。
    'Windows フォーム デザイナーを使用して変更できます。  
    'コード エディターを使って変更しないでください。
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.btnOpenClose = New System.Windows.Forms.Button()
        Me.cbxPort = New System.Windows.Forms.ComboBox()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.pbxMainRaw = New OpenCvSharp.UserInterface.PictureBoxIpl()
        Me.cmbCamID = New System.Windows.Forms.ComboBox()
        Me.btnCamOpen = New System.Windows.Forms.Button()
        CType(Me.pbxMainRaw, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'btnOpenClose
        '
        Me.btnOpenClose.Location = New System.Drawing.Point(197, 10)
        Me.btnOpenClose.Name = "btnOpenClose"
        Me.btnOpenClose.Size = New System.Drawing.Size(75, 23)
        Me.btnOpenClose.TabIndex = 0
        Me.btnOpenClose.Text = "ComOpen"
        Me.btnOpenClose.UseVisualStyleBackColor = True
        '
        'cbxPort
        '
        Me.cbxPort.FormattingEnabled = True
        Me.cbxPort.Location = New System.Drawing.Point(12, 12)
        Me.cbxPort.Name = "cbxPort"
        Me.cbxPort.Size = New System.Drawing.Size(179, 20)
        Me.cbxPort.TabIndex = 8
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(338, 88)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(75, 23)
        Me.Button1.TabIndex = 9
        Me.Button1.Text = "Send"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'pbxMainRaw
        '
        Me.pbxMainRaw.Location = New System.Drawing.Point(12, 88)
        Me.pbxMainRaw.Name = "pbxMainRaw"
        Me.pbxMainRaw.Size = New System.Drawing.Size(320, 240)
        Me.pbxMainRaw.TabIndex = 10
        Me.pbxMainRaw.TabStop = False
        '
        'cmbCamID
        '
        Me.cmbCamID.FormattingEnabled = True
        Me.cmbCamID.Location = New System.Drawing.Point(12, 38)
        Me.cmbCamID.Name = "cmbCamID"
        Me.cmbCamID.Size = New System.Drawing.Size(179, 20)
        Me.cmbCamID.TabIndex = 11
        '
        'btnCamOpen
        '
        Me.btnCamOpen.Location = New System.Drawing.Point(197, 36)
        Me.btnCamOpen.Name = "btnCamOpen"
        Me.btnCamOpen.Size = New System.Drawing.Size(75, 23)
        Me.btnCamOpen.TabIndex = 12
        Me.btnCamOpen.Text = "CamOpen"
        Me.btnCamOpen.UseVisualStyleBackColor = True
        '
        'frmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(530, 360)
        Me.Controls.Add(Me.btnCamOpen)
        Me.Controls.Add(Me.cmbCamID)
        Me.Controls.Add(Me.pbxMainRaw)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.cbxPort)
        Me.Controls.Add(Me.btnOpenClose)
        Me.Name = "frmMain"
        Me.Text = "Communication K210"
        CType(Me.pbxMainRaw, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents btnOpenClose As Button
    Friend WithEvents cbxPort As ComboBox
    Friend WithEvents Button1 As Button
    Friend WithEvents pbxMainRaw As OpenCvSharp.UserInterface.PictureBoxIpl
    Friend WithEvents cmbCamID As ComboBox
    Friend WithEvents btnCamOpen As Button
End Class
