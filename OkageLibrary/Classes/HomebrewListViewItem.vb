Public Class HomebrewListViewItem

    Private _FileName As String
    Private _Console As String
    Private _Firmware As String
    Private _FilePath As String

    Public Property FileName As String
        Get
            Return _FileName
        End Get
        Set
            _FileName = Value
        End Set
    End Property

    Public Property Console As String
        Get
            Return _Console
        End Get
        Set
            _Console = Value
        End Set
    End Property

    Public Property Firmware As String
        Get
            Return _Firmware
        End Get
        Set
            _Firmware = Value
        End Set
    End Property

    Public Property FilePath As String
        Get
            Return _FilePath
        End Get
        Set
            _FilePath = Value
        End Set
    End Property

End Class
