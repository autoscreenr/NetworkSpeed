Imports Microsoft.VisualBasic
Imports System.Runtime.InteropServices
Imports System.Text

Public Class CINISoubor

    Private Declare Auto Function GetPrivateProfileString Lib "kernel32" (ByVal lpAppName As String, ByVal lpKeyName As String, ByVal lpDefault As String, _
                                                                          ByVal lpReturnedString As StringBuilder, ByVal nSize As Integer, ByVal lpFileName As String) As Integer

    Private Declare Auto Function WritePrivateProfileString Lib "kernel32" (ByVal lpAppName As String, ByVal lpKeyName As String, ByVal lpString As String, _
                                                                            ByVal lpFileName As String) As Boolean

    Sub New()

        Me.INISoubor = FileSystem.CurDir() & "\NetworkSpeed.ini"
        Me.NactiHodnoty(Me.INISoubor)

    End Sub

    Public Property AdresarLOGSouboruINI As String
    Public Property PocetMinutCyklu As Integer
    Public Property LimitniNejnizsiHodnota As Integer
    Private Property INISoubor As String

    Private Sub NactiHodnoty(INISoubor As String)

        Dim PromenaProNacteniINI As Integer
        Dim SB As StringBuilder = New StringBuilder(500)

        PromenaProNacteniINI = GetPrivateProfileString("NetworkMonitor", "Adresar", "", SB, SB.Capacity, INISoubor)
        Me.AdresarLOGSouboruINI = SB.ToString

        PromenaProNacteniINI = GetPrivateProfileString("NetworkMonitor", "PocetMinutCyklu", "", SB, SB.Capacity, INISoubor)
        Me.PocetMinutCyklu = SB.ToString

        PromenaProNacteniINI = GetPrivateProfileString("NetworkMonitor", "LimitniNejnizsiHodnota", "", SB, SB.Capacity, INISoubor)
        Me.LimitniNejnizsiHodnota = SB.ToString


    End Sub


End Class

