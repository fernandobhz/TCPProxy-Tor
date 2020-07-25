Module Main

    Sub Main()

        Dim Args() = Environment.GetCommandLineArgs()

        Dim localListeningPort As Integer
        Dim remoteHost As String
        Dim remotePort As Integer
        Dim useTor As Boolean
        Dim torConsoleVisible As Boolean
        
        Select Case Args.Count
            Case 5
                localListeningPort = Args(1)
                remoteHost = Args(2)
                remotePort = Args(3)
                useTor = Args(4)
            Case 1
                localListeningPort = InputBox("Local listening port")
                remoteHost = InputBox("Remote host")
                remotePort = InputBox("Remote port")
                useTor = InputBox("Use tor", "", 1)
                torConsoleVisible = InputBox("Tor console visible", "", 1)
            Case Else
                Console.WriteLine("Usage TCPProxy localport remoteHost remotePort")
                Exit Sub
        End Select


        Dim TorListeningPort As Integer

        If useTor Then
            TorListeningPort = TheOnionRouter.StartClient(torConsoleVisible)
        End If


        TCPProxy.Start(localListeningPort, remoteHost, remotePort, useTor, TorListeningPort).Wait()
    End Sub

End Module
