Imports System.Text
Imports System.Threading
Imports System.Net.Sockets
Imports System.Net

Friend Module TheOnionRouter

    Friend Function StartClient(Optional TorConsoleVisible As Boolean = False) As Integer
        Dim TorListeningPort As Integer = GetFreeTCPPort()
        Dim TorFolder As String = IO.Path.Combine(IO.Path.GetTempPath, Guid.NewGuid.ToString)

        If Not My.Computer.FileSystem.DirectoryExists(TorFolder) Then
            My.Computer.FileSystem.CreateDirectory(TorFolder)
        End If

        Dim TorrcPath As String = IO.Path.Combine(TorFolder, "torrc")

        Dim SB As New StringBuilder
        SB.AppendLine(String.Format("SocksPort {0}", TorListeningPort))
        SB.AppendLine(String.Format("DataDirectory {0}/", TorFolder))

        Dim TorrcBuff As Byte() = System.Text.Encoding.ASCII.GetBytes(SB.ToString)
        My.Computer.FileSystem.WriteAllBytes(TorrcPath, TorrcBuff, False)

        Dim TorPath As String = IO.Path.Combine(TorFolder, "tor.exe")
        My.Computer.FileSystem.WriteAllBytes(TorPath, My.Resources.tor, False)

        Dim P As New Process
        P.StartInfo = New ProcessStartInfo(TorPath, "-f " & TorrcPath)
        P.StartInfo.UseShellExecute = False

        If TorConsoleVisible Then
            P.StartInfo.CreateNoWindow = False
        Else
            P.StartInfo.CreateNoWindow = True
        End If

        P.Start()

        Return TorListeningPort
    End Function

    Friend Function StartRDP() As String
        Dim TorListeningPort As Integer = GetFreeTCPPort()
        Dim Folder As String = IO.Path.Combine(IO.Path.GetTempPath, Guid.NewGuid.ToString)

        Dim Service As New TorService
        Service.DestinationIP = "127.0.0.1"
        Service.DestinationPort = 3389
        Service.ListeningPort = 3389

        Dim Services As New List(Of TorService)
        Services.Add(Service)

        Return StartServer(TorListeningPort, Folder, Services)
    End Function


    Friend Function StartServer(ByVal TORListeningPort As Integer, ByVal Folder As String, ByVal Services As List(Of TorService)) As String

        Dim TorFolder As String = IO.Path.Combine(Folder, "Tor")

        If Not My.Computer.FileSystem.DirectoryExists(TorFolder) Then
            My.Computer.FileSystem.CreateDirectory(TorFolder)
        End If

        Dim TorrcPath As String = IO.Path.Combine(TorFolder, "torrc")
        Dim HostNamePath As String = IO.Path.Combine(TorFolder, "hostname")

        Dim SB As New StringBuilder
        SB.AppendLine(String.Format("SocksPort {0}", TORListeningPort))
        SB.AppendLine(String.Format("DataDirectory {0}/", TorFolder))

        If Services IsNot Nothing Then
            If Services.Count > 0 Then
                SB.AppendLine(String.Format("HiddenServiceDir {0}/", TorFolder))
                For Each Service As TorService In Services
                    SB.AppendLine(String.Format("HiddenServicePort {0} {1}:{2}", Service.ListeningPort, Service.DestinationIP, Service.DestinationPort))
                Next
            End If
        End If

        Dim TorrcBuff As Byte() = System.Text.Encoding.ASCII.GetBytes(SB.ToString)

        My.Computer.FileSystem.WriteAllBytes(TorrcPath, TorrcBuff, False)


        Dim TorPath As String = IO.Path.Combine(TorFolder, "tor.exe")
        My.Computer.FileSystem.WriteAllBytes(TorPath, My.Resources.tor, False)


        Dim P As New Process
        P.StartInfo = New ProcessStartInfo(TorPath, "-f " & TorrcPath)
        P.StartInfo.UseShellExecute = True
        P.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
        P.Start()

        While Not My.Computer.FileSystem.FileExists(HostNamePath)
            Thread.Sleep(100)
        End While

        Dim HostName As String = My.Computer.FileSystem.ReadAllText(HostNamePath)
        Return HostName

    End Function

    Private Function GetFreeTCPPort() As UShort
        Dim TcpListener As New TcpListener(IPAddress.Any, 0)
        TcpListener.Start()

        Dim IPEndPoint As IPEndPoint = TcpListener.LocalEndpoint
        Dim Port As UShort = IPEndPoint.Port

        TcpListener.Stop()

        Return Port
    End Function

End Module

Friend Class TorService
    Property ListeningPort As Integer

    Property DestinationIP As String
    Property DestinationPort As Integer
End Class
