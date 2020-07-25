Imports System.Net.Sockets

Class TCPProxyClient
    Private AClient As TcpClient
    Private BClient As TcpClient

    Private AStream As NetworkStream
    Private BStream As NetworkStream

    Private BHost As String
    Private BPort As Integer

    Private ABTor As Boolean
    Private TorPort As Integer

    Sub New(Client As TcpClient, RemoteAddress As String, RemotePort As Integer, UseTorNetwork As Boolean, TorListeningPort As Integer)
        AClient = Client
        BHost = RemoteAddress
        BPort = RemotePort
        ABTor = UseTorNetwork
        TorPort = TorListeningPort
    End Sub

    Public Async Function Start() As Task
        Try
            AStream = AClient.GetStream

            If ABTor Then
                Dim SC As New SocksProxy("localhost", TorPort)
                BStream = SC.GetStream(BHost, BPort, BClient)
            Else
                BClient = New TcpClient
                Await BClient.ConnectAsync(BHost, BPort)
                BStream = BClient.GetStream
            End If

            _ProcessDataTunneling()
        Catch ex As Exception
            Me.Stop()
        End Try
    End Function


    Private Sub _ProcessDataTunneling()
        Dim TDown As Task = Download()
        Dim TUp As Task = Upload()
    End Sub

    Public Sub [Stop]()
        If AStream IsNot Nothing Then AStream.Dispose()
        If AClient IsNot Nothing Then AClient.Close()

        If BStream IsNot Nothing Then BStream.Dispose()
        If BClient IsNot Nothing Then BClient.Close()
    End Sub

    Async Function Download() As Task
        Try
            Dim Buffer(1024 * 1024) As Byte

            While AClient.Connected And BClient.Connected
                Dim bytesRead As Integer = Await AStream.ReadAsync(Buffer, 0, Buffer.Length)
                If bytesRead = 0 Then Exit While
                Await BStream.WriteAsync(Buffer, 0, bytesRead)
            End While
        Catch ex As Exception
            Me.Stop()
        End Try
    End Function

    Async Function Upload() As Task
        Try
            Dim Buffer(1024 * 1024) As Byte

            While AClient.Connected And BClient.Connected
                Dim bytesRead As Integer = Await BStream.ReadAsync(Buffer, 0, Buffer.Length)
                If bytesRead = 0 Then Exit While
                Await AStream.WriteAsync(Buffer, 0, bytesRead)
            End While
        Catch ex As Exception
            Me.Stop()
        End Try
    End Function

End Class
