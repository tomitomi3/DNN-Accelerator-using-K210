Imports System.IO.Ports
Imports System.Runtime.InteropServices
Imports System.Threading
Imports OpenCvSharp

Public Class frmMain
    Declare Function AllocConsole Lib "kernel32" () As Int32
    Private _serialPort As SerialPort = Nothing
    Private _rcvThread As Thread = Nothing
    Private _capThread As Thread = Nothing

    ''' <summary>video cap object</summary>
    Private _cap As VideoCapture = Nothing

    Private _sendData As New List(Of Byte)

    ''' <summary>スレッド同期(CS)</summary>
    Private _capLock = New Object()

    Private _rawClipMat As Mat = Nothing

    Private Const ReadBufferSize As Integer = 32
    Private _lock = New Object
    Private _recvData As New List(Of Byte)

    Private Sub frmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        AllocConsole()
        _serialPort = New SerialPort()
        _serialPort.BaudRate = 1843200 '3686400 '9600
        _serialPort.StopBits = StopBits.One
        _serialPort.RtsEnable = False
        _serialPort.DataBits = 8
        _serialPort.Parity = False

        Dim ports = System.IO.Ports.SerialPort.GetPortNames()
        For Each portName In ports
            Console.WriteLine("{0}", portName)
        Next
        If ports.Length <> 0 Then
            For Each p In ports
                Me.cbxPort.Items.Add(String.Format("{0}", p))
            Next
            Me.cbxPort.SelectedIndex = 0
        End If

        'video
        Dim camIds As New List(Of Integer)
        For i As Integer = 0 To 10 - 1
            Dim temp As VideoCapture = Nothing
            Try
                temp = New OpenCvSharp.VideoCapture(i)
                If temp.IsOpened() = False Then
                    Continue For
                Else
                    Console.WriteLine("CAMERA ID:{0}", i)
                    Console.WriteLine(" {0} {1}", temp.Get(VideoCaptureProperties.FrameHeight), temp.Get(VideoCaptureProperties.FrameWidth))
                    camIds.Add(i)
                End If
            Finally
                temp.Release()
            End Try
        Next

        'cmb box camera ID
        cmbCamID.DropDownStyle = ComboBoxStyle.DropDownList
        cmbCamID.Items.Clear()
        For Each camId In camIds
            cmbCamID.Items.Add(camId.ToString())
        Next
        If cmbCamID.Items.Count = 0 Then
            'do nothing
        Else
            cmbCamID.SelectedIndex = 0
        End If
    End Sub

    Private Sub frmMain_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Me.CloseProcess()
    End Sub

    ''' <summary>
    ''' 終了処理
    ''' </summary>
    Private Sub CloseProcess()
        If _capThread IsNot Nothing Then
            If _capThread.IsAlive = True Then
                _capThread.Abort()
            End If
        End If

        If _serialPort IsNot Nothing Then
            If _serialPort.IsOpen Then
                _serialPort.Close()
            End If
        End If
    End Sub

    ''' <summary>
    ''' COMオープン
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub btnOpenClose_Click(sender As Object, e As EventArgs) Handles btnOpenClose.Click
        Try
            If Me._serialPort.IsOpen Then
                Me.btnOpenClose.Text = "Open"
                Return
            Else
                Me.btnOpenClose.Text = "Close"
            End If

            Me._serialPort.PortName = Me.cbxPort.SelectedItem.ToString
            Me._serialPort.Open()

            'thread start
            Me._rcvThread = New Thread(AddressOf RcvThread)
            Me._rcvThread.IsBackground = True
            Thread.Sleep(500)
            Me._rcvThread.Start()
            Thread.Sleep(500)

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' カメラ開く
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub btnCamOpen_Click(sender As Object, e As EventArgs) Handles btnCamOpen.Click
        'open
        If _cap Is Nothing Then
            'open cap
            _capThread = New Threading.Thread(AddressOf CamCapThread)
            _capThread.Priority = System.Threading.ThreadPriority.Highest
            _capThread.Name = "Cap Thread"
            _capThread.Start()

            btnCamOpen.Text = "CamClose"
            btnCamOpen.BackColor = Color.Aqua
        Else
            'close cap
            _capThread.Abort()
            While (_capThread.ThreadState <> Threading.ThreadState.Aborted)
                Console.WriteLine("exit...")
                Threading.Thread.Sleep(50)
                _capThread.Join()
            End While

            btnCamOpen.Text = "CamOpen"
            btnCamOpen.BackColor = Color.AliceBlue
        End If
    End Sub

    ''' <summary>
    ''' 受信用スレッド
    ''' </summary>
    Private Sub RcvThread()
        Dim recentReadCount As Integer = 10
        _recvData.Clear()

        While (True)
            'バッファオーバーチェック
            If Me._serialPort.BytesToRead > (Me._serialPort.ReadBufferSize / 2) Then
                Exit While
            End If

            'データ読み込み
            SyncLock (_lock)
                Dim isRead As Boolean = True
                Dim temp() As Byte = Nothing
                Dim readSize As Integer = 0

                '受信サイズ計算
                Dim nowReadBufferSize As Integer = Me._serialPort.BytesToRead
                If nowReadBufferSize = 0 Then
                    isRead = False
                ElseIf nowReadBufferSize > ReadBufferSize Then
                    '指定したサイズ分をバッファから読み込む
                    ReDim temp(ReadBufferSize - 1)
                    readSize = ReadBufferSize
                ElseIf recentReadCount > 10 Then
                    'バッファサイズに変更がない＝バッファをすべて空にする
                    recentReadCount = 0
                    ReDim temp(nowReadBufferSize - 1)
                    readSize = nowReadBufferSize
                Else
                    recentReadCount += 1
                    isRead = False
                End If

                '受信
                If isRead = True Then
                    'Console.WriteLine(Me.oSerialPort.BytesToRead)

                    'read
                    For i As Integer = 0 To readSize - 1
                        i += _serialPort.Read(temp, 0, readSize - i)
                    Next

                    'Console.WriteLine(Me.oSerialPort.BytesToRead)
                    For Each value In temp
                        _recvData.Add(value)
                    Next
                    Console.WriteLine("ReadSize:{0}", _recvData.Count)
                    Console.Write("ReadData:")
                    For Each tempValue In _recvData
                        Console.Write("{0} ", tempValue)
                    Next
                    Console.WriteLine("")
                    _recvData.Clear()
                End If
            End SyncLock

            Thread.Sleep(10)
        End While
    End Sub

    ''' <summary>
    ''' camera
    ''' </summary>
    Private Sub CamCapThread()
        Dim sw As New Stopwatch()
        While (True)
            sw.Restart()
            Try
                'create VideoCapture instance
                If _cap Is Nothing Then
                    Dim camId = 0
                    Me.Invoke(
                    Sub()
                        camId = CInt(Me.cmbCamID.SelectedItem.ToString())
                    End Sub
                    )
                    Me._cap = New OpenCvSharp.VideoCapture(camId)
                    _cap.Set(VideoCaptureProperties.FrameWidth, 640)
                    _cap.Set(VideoCaptureProperties.FrameHeight, 480)
                End If

                'capture
                SyncLock _capLock
                    Dim mat = New Mat()
                    If Me._cap.Read(mat) = False Then
                        Continue While
                    End If

                    Dim grayMat = New Mat()
                    Cv2.CvtColor(mat, grayMat, ColorConversionCodes.BGRA2GRAY)

                    Dim resizeMat As New Mat()
                    Cv2.Resize(grayMat, resizeMat, New OpenCvSharp.Size(320, 240), interpolation:=InterpolationFlags.Cubic)
                    'Cv2.Resize(mat, resizeMat, New OpenCvSharp.Size(240, 180), interpolation:=InterpolationFlags.Cubic)

                    Dim doneMat = New Mat()
                    Cv2.Flip(resizeMat, doneMat, FlipMode.Y)

                    Me._rawClipMat = doneMat.Clone() 'copy

                    'update
                    Me.Invoke(
                        Sub()
                            'update edit
                            Me.pbxMainRaw.ImageIpl = doneMat
                        End Sub
                        )
                End SyncLock

            Catch ex As Threading.ThreadAbortException
                Console.WriteLine("throw ThreadAbortException")
                Me._cap.Release()
                Me._cap = Nothing
                Exit While
            Catch ex As Exception
                Console.WriteLine("Catch Exception!")
                Console.WriteLine("{0}", ex.Message)
            Finally
                Dim sumUsingMemory = GC.GetTotalMemory(True)
                Dim thd As Long = 1024 * 1024 * 256
                If sumUsingMemory > thd Then
                    GC.Collect()
                End If
            End Try
            sw.Stop()
        End While

        'done
        Console.WriteLine("WorkerDone")
    End Sub

    ''' <summary>
    ''' Send K210
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If Me._serialPort.IsOpen = True AndAlso _capThread.IsAlive = True Then
            SyncLock _capLock
                Dim size = _rawClipMat.Width * _rawClipMat.Height * _rawClipMat.ElemSize()
                Dim dataArray(size - 1) As Byte
                Marshal.Copy(_rawClipMat.Data, dataArray, 0, dataArray.Length)
                _serialPort.Write(dataArray, 0, dataArray.Count)
            End SyncLock
        Else
            Console.WriteLine("Can not open serialport or camera.")
        End If
    End Sub
End Class
