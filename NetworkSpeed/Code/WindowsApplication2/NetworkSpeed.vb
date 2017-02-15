Imports Echevil
Imports NetworkSpeed.Libor.ClassUkazkaInstance
Imports System.Threading.Tasks
Imports System.Threading

Public Class NetworkSpeed

    Public Property VerzeProgramu As String = "Verze: 1.0"
    Public Property DatumProgramu As String = "© 28.10.2015 by Jiří Ličman"
    '´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´´
    Public Property AdresarLOGSouboruINI As String
    Public Property NetworkAdapter As NetworkAdapter
    Private trd As Thread
    Delegate Sub SetTextCallback([text] As String)


    Private Sub NetworkSpeed_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Dim INISoubor As New CINISoubor

        ZminimalizujOknoMimoTaskBar()

        Me.ToolStripStatusLabel1.Text = CStr("Počet minut cyklu: " & INISoubor.PocetMinutCyklu & "min.| Limitní nejn.hodnota: " & INISoubor.LimitniNejnizsiHodnota & " kbps")

        trd = New Thread(AddressOf LoopForSpeedMeasurement)
        trd.IsBackground = True
        trd.Start()

    End Sub


    Private Sub LoopForSpeedMeasurement()

        Dim INISoubor As New CINISoubor
        Dim NetworkMonitor As New NetworkMonitor
        Dim Adapters() As NetworkAdapter = NetworkMonitor.Adapters
        Dim KolikratProbehlaSmycka As Integer
        Dim PocetSekundCyklu As Integer = 60 * INISoubor.PocetMinutCyklu
        Dim SpeedDownload As Integer
        Dim CisloAdapteru As Integer
        Dim HighestSpeedInLoop As Double = 0
        Dim LimitniNejnizsiHodnota As Integer = INISoubor.LimitniNejnizsiHodnota
        Dim NazevSouboru As String = CStr(Microsoft.VisualBasic.DateAndTime.Year(Now)) & CStr(Microsoft.VisualBasic.DateAndTime.Month(Now)) & CStr(Microsoft.VisualBasic.DateAndTime.Day(Now)) & "_" _
                                     & Environment.MachineName.ToString & ".txt"

        'Find out of active Network Adapter
        CisloAdapteru = ActiveNetworkAdapter()

        Me.Set_TxtBox_ControledAdapter(Adapters(CisloAdapteru).Name)
        NetworkMonitor.StartMonitoring(Adapters(CisloAdapteru))

        Do
            Thread.Sleep(1000)
            PocetSekundCyklu = PocetSekundCyklu - 1
            Me.Set_TxtBox_Cas(CStr(Microsoft.VisualBasic.DateAndTime.Hour(Now)) & ":" & CStr(Microsoft.VisualBasic.DateAndTime.Minute(Now)))
            Me.Set_TxtBox_AktualniRychlost(Math.Round(Adapters(CisloAdapteru).DownloadSpeedKbps, 2).ToString)
            Me.Set_TxtBox_ProcentoStahovani(CStr(Math.Round(KolikratProbehlaSmycka / ((INISoubor.PocetMinutCyklu * 60) * 0.01), 0)) & " %")
            Me.Set_TxtBox_NejvyssiRychlost(CStr(Math.Round(HighestSpeedInLoop, 2)))
            Me.Set_TxtBox_Smycka(PocetSekundCyklu.ToString)



            If Adapters(CisloAdapteru).DownloadSpeedKbps >= LimitniNejnizsiHodnota Then
                SpeedDownload = SpeedDownload + Adapters(CisloAdapteru).DownloadSpeedKbps
                KolikratProbehlaSmycka = KolikratProbehlaSmycka + 1
                If Adapters(CisloAdapteru).DownloadSpeedKbps > HighestSpeedInLoop Then HighestSpeedInLoop = Adapters(CisloAdapteru).DownloadSpeedKbps
            End If

            If PocetSekundCyklu = 0 Then
                NetworkMonitor.StopMonitoring(Adapters(CisloAdapteru))
                CreateFileofMeasurement(INISoubor.AdresarLOGSouboruINI & NazevSouboru, CStr(Microsoft.VisualBasic.DateAndTime.Hour(Now)) & ":" & CStr(Microsoft.VisualBasic.DateAndTime.Minute(Now)), _
                                        CStr(Math.Round((SpeedDownload / KolikratProbehlaSmycka), 2)), CStr(Math.Round(HighestSpeedInLoop, 2)), _
                                        CStr(Math.Round(KolikratProbehlaSmycka / ((INISoubor.PocetMinutCyklu * 60) * 0.01), 0)) & " %")

                KolikratProbehlaSmycka = 0
                HighestSpeedInLoop = 0
                SpeedDownload = 0
                PocetSekundCyklu = INISoubor.PocetMinutCyklu * 60
                Me.Set_TxtBox_Smycka("Zapsáno do logu")
                NetworkMonitor.StartMonitoring(Adapters(CisloAdapteru))
            End If
        Loop

    End Sub

    'Find out of active Network Adapter
    Private Function ActiveNetworkAdapter() As Integer

        Dim NetworkMonitor As New NetworkMonitor
        Dim Adapters() As NetworkAdapter = NetworkMonitor.Adapters
        Dim myLines(Adapters.Count - 1) As String
        Dim TestSpeed As Integer
        Dim CisloAdapteru As Integer

        Do Until TestSpeed > 0
            For x = 0 To Adapters.Count - 1
                myLines(x) = Adapters(x).Name
                ' Write all founded Network Adapters to Texbox
                Me.Set_TxtBox_FoundAllOfNetworkAdapters(myLines(x).ToString)
                NetworkMonitor.StartMonitoring(Adapters(x))
                For xx = 0 To 10
                    Thread.Sleep(100)
                    TestSpeed = TestSpeed + Adapters(x).DownloadSpeedKbps
                Next
                If TestSpeed > 0 Then
                    NetworkMonitor.StopMonitoring(Adapters(x))
                    CisloAdapteru = x
                    Exit Do
                End If
            Next
        Loop

        Return CisloAdapteru

    End Function

    Private Sub OtevreniSouboru(Soubor As String)

        Dim INISoubor As New CINISoubor

        If Soubor = "LOG" Then
            Soubor = INISoubor.AdresarLOGSouboruINI & CStr(Microsoft.VisualBasic.DateAndTime.Year(Now)) & CStr(Microsoft.VisualBasic.DateAndTime.Month(Now)) & _
                CStr(Microsoft.VisualBasic.DateAndTime.Day(Now)) & "_" & Environment.MachineName.ToString & ".txt"
        End If

        Shell("notepad.exe " & Soubor, AppWinStyle.NormalFocus, False)

    End Sub

    Private Sub ZminimalizujOknoMimoTaskBar()

        ContextMenuStrip1.Enabled = False
        Me.ControlBox = False
        Me.MaximizeBox = False
        Me.MinimizeBox = False

        Me.WindowState = FormWindowState.Minimized
        'Don't show in the task bar
        Me.ShowInTaskbar = False
        'Enable the Context Menu Strip
        ContextMenuStrip1.Enabled = True

    End Sub

    Private Sub ShowToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ShowToolStripMenuItem.Click
        'When Show menu clicks, it will show the form:
        Me.WindowState = FormWindowState.Normal
        'Show in the task bar:
        Me.ShowInTaskbar = True
        'Disable the Context Menu:
        ContextMenuStrip1.Enabled = False
    End Sub


#Region "Set_TxtBox"


    Private Sub Set_TxtBox_FoundAllOfNetworkAdapters(ByVal [text] As String)

        ' InvokeRequired required compares the thread ID of the
        ' calling thread to the thread ID of the creating thread.
        ' If these threads are different, it returns true.
        If Me.TextBox_FoundAllOfNetworkAdapters.InvokeRequired Then
            Dim d As New SetTextCallback(AddressOf Set_TxtBox_FoundAllOfNetworkAdapters)
            Me.Invoke(d, New Object() {[text]})
        Else
            Me.TextBox_FoundAllOfNetworkAdapters.Text = Me.TextBox_FoundAllOfNetworkAdapters.Text & Environment.NewLine & [text]
        End If
    End Sub

    Private Sub Set_TxtBox_ControledAdapter(ByVal [text] As String)

        If Me.TextBox_ControledAdapter.InvokeRequired Then
            Dim d As New SetTextCallback(AddressOf Set_TxtBox_ControledAdapter)
            Me.Invoke(d, New Object() {[text]})
        Else
            Me.TextBox_ControledAdapter.Text = [text]
        End If
    End Sub

    Private Sub Set_TxtBox_Cas(ByVal [text] As String)

        If Me.TextBox_Cas.InvokeRequired Then
            Dim d As New SetTextCallback(AddressOf Set_TxtBox_Cas)
            Me.Invoke(d, New Object() {[text]})
        Else
            Me.TextBox_Cas.Text = [text]
        End If
    End Sub

    Private Sub Set_TxtBox_AktualniRychlost(ByVal [text] As String)

        If Me.TextBox_AktualniRychlost.InvokeRequired Then
            Dim d As New SetTextCallback(AddressOf Set_TxtBox_AktualniRychlost)
            Me.Invoke(d, New Object() {[text]})
        Else
            Me.TextBox_AktualniRychlost.Text = [text]
        End If
    End Sub


    Private Sub Set_TxtBox_NejvyssiRychlost(ByVal [text] As String)

        If Me.TextBox_NejvyssiRychlost.InvokeRequired Then
            Dim d As New SetTextCallback(AddressOf Set_TxtBox_NejvyssiRychlost)
            Me.Invoke(d, New Object() {[text]})
        Else
            Me.TextBox_NejvyssiRychlost.Text = [text]
        End If
    End Sub

    Private Sub Set_TxtBox_ProcentoStahovani(ByVal [text] As String)

        If Me.TextBox_ProcentoStahovani.InvokeRequired Then
            Dim d As New SetTextCallback(AddressOf Set_TxtBox_ProcentoStahovani)
            Me.Invoke(d, New Object() {[text]})
        Else
            Me.TextBox_ProcentoStahovani.Text = [text]
        End If
    End Sub

    Private Sub Set_TxtBox_Smycka(ByVal [text] As String)

        If Me.TextBox_Smycka.InvokeRequired Then
            Dim d As New SetTextCallback(AddressOf Set_TxtBox_Smycka)
            Me.Invoke(d, New Object() {[text]})
        Else
            Me.TextBox_Smycka.Text = [text]
        End If
    End Sub

#End Region


    Private Sub CreateFileofMeasurement(LogSoubor As String, AktCas As String, PrumernaRychlost As String, NejvyssiRychlost As String, NejnizsiRychlost As String)

        My.Computer.FileSystem.WriteAllText(LogSoubor, AktCas & ";" & PrumernaRychlost & ";" & NejvyssiRychlost & ";" & NejnizsiRychlost & vbNewLine, True, System.Text.Encoding.ASCII)

    End Sub

    Private Sub Button_Cancel_Click(sender As Object, e As EventArgs) Handles Button_Cancel.Click

        End

    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click

        End

    End Sub

    Private Sub INIFileEditaceToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles INIFileEditaceToolStripMenuItem1.Click

        OtevreniSouboru("NetworkSpeed.ini")

    End Sub

    Private Sub AboutToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AboutToolStripMenuItem.Click

        Application.DoEvents()
        MsgBox("NetworkSpeed" & vbCrLf & Me.DatumProgramu & vbCrLf & Me.VerzeProgramu, MsgBoxStyle.Information, "About")

    End Sub

    Private Sub Button_ReadLog_Click(sender As Object, e As EventArgs) Handles Button_ReadLog.Click

        OtevreniSouboru("LOG")

    End Sub

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub

    Private Sub NačteníAktloguToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles NačteníAktloguToolStripMenuItem.Click

        OtevreniSouboru("LOG")

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button_Schovat.Click

        ZminimalizujOknoMimoTaskBar()

    End Sub
End Class
