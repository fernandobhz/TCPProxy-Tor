Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Runtime.CompilerServices

Public Class SocksProxy

    Private _SocksHost As String
    Private _SocksPort As Integer

    Sub New(SocksHost As String, SocksPort As Integer)
        _SocksHost = SocksHost
        _SocksPort = SocksPort
    End Sub

    Function GetStream(HostDest As String, PortDest As Integer, ByRef outTcpClient As TcpClient) As NetworkStream

        Dim client As TcpClient = New TcpClient()
        client.Connect(_SocksHost, _SocksPort)

        Dim stream As NetworkStream = client.GetStream()
        'Auth
        Dim buf = New Byte(299) {}
        buf(0) = &H5
        buf(1) = &H1
        buf(2) = &H0
        stream.Write(buf, 0, 3)

        ReadExactSize(stream, buf, 0, 2)
        If buf(0) <> &H5 Then
            Throw New IOException("Invalid Socks Version")
        End If
        If buf(1) = &HFF Then
            Throw New IOException("Socks Server does not support no-auth")
        End If
        If buf(1) <> &H0 Then
            Throw New Exception("Socks Server did choose bogus auth")
        End If

        buf(0) = &H5
        buf(1) = &H1
        buf(2) = &H0
        buf(3) = &H3
        Dim domain = Encoding.ASCII.GetBytes(HostDest)
        buf(4) = CByte(domain.Length)
        Array.Copy(domain, 0, buf, 5, domain.Length)

        'Little-endian to big-endian
        Dim port As Byte() = BitConverter.GetBytes(PortDest)
        buf(5 + domain.Length) = port(1)
        buf(6 + domain.Length) = port(0)
        stream.Write(buf, 0, domain.Length + 7)


        ' Reply
        ReadExactSize(stream, buf, 0, 4)
        If buf(0) <> &H5 Then
            Throw New IOException("Invalid Socks Version")
        End If
        If buf(1) <> &H0 Then
            Throw New IOException(String.Format("Socks Error {0:X}", buf(1)))
        End If
        Dim rdest = String.Empty
        Select Case buf(3)
            Case &H1
                ' IPv4
                ReadExactSize(stream, buf, 0, 4)
                Dim v4 = BitConverter.ToUInt32(buf, 0)
                rdest = New IPAddress(v4).ToString()
                Exit Select
            Case &H3
                ' Domain name
                ReadExactSize(stream, buf, 0, 1)
                If buf(0) = &HFF Then
                    Throw New IOException("Invalid Domain Name")
                End If
                ReadExactSize(stream, buf, 1, buf(0))
                rdest = Encoding.ASCII.GetString(buf, 1, buf(0))
                Exit Select
            Case &H4
                ' IPv6
                Dim octets = New Byte(15) {}
                ReadExactSize(stream, octets, 0, 16)
                rdest = New IPAddress(octets).ToString()
                Exit Select
            Case Else
                Throw New IOException("Invalid Address type")
        End Select
        ReadExactSize(stream, buf, 0, 2)
        Dim rport = CUShort(IPAddress.NetworkToHostOrder(CShort(BitConverter.ToUInt16(buf, 0))))

        outTcpClient = client
        Return stream
    End Function

    Private Sub ReadExactSize(stream As NetworkStream, buffer As Byte(), offset As Integer, size As Integer)
        While size <> 0
            Dim read = stream.Read(buffer, offset, size)
            If read < 0 Then
                Throw New IOException("Premature end")
            End If
            size -= read
            offset += read
        End While
    End Sub

End Class