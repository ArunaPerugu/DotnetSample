﻿' <snippet1>
Imports System.ServiceModel

' This code generated by svcutil.exe.
<ServiceContract()>
Interface IMath
    <OperationContract()>
    Function Add(A As Double, B As Double) As Double
End Interface

Public Class Math
    Implements IMath

    Function Add(A As Double, B As Double) As Double Implements IMath.Add
        Return A + B
    End Function
End Class

Public Class Test
    Public Shared Sub Main()
    End Sub

    Public Sub Run()
        ' This code is written by an application developer.
        ' Create a channel factory.
        Dim myBinding As New BasicHttpBinding
        Dim myEndpoint As New EndpointAddress("http://localhost/MathService/Ep1")

        Dim myChannelFactory As ChannelFactory(Of IMath) =
        New ChannelFactory(Of IMath)(myBinding, myEndpoint)

        'Create a channel.
        Dim proxy1 As IMath = myChannelFactory.CreateChannel()
        Dim s As Integer = proxy1.Add(3, 39)
        Console.WriteLine(s.ToString())

        ' Create another channel
        Dim proxy2 As IMath = myChannelFactory.CreateChannel()
        s = proxy2.Add(15, 27)
        Console.WriteLine(s.ToString())
    End Sub
End Class
' </snippet1>
