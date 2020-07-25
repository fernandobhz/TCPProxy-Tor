Imports System.IO
Imports System.Net.Sockets
Imports System.Threading

Public Class Form1

    Private P As TCPProxy

    Private Async Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Await TCPProxy.Start(HostPort:=1000, RemoteAddress:="192.168.1.150", RemotePort:=5000)
    End Sub

End Class
