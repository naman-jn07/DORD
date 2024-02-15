Imports Microsoft.VisualBasic
Imports System
Imports System.Web
Imports System.Data
Imports System.Data.SqlClient
Imports Npgsql


Public Class ConnectNREGA

    Private pgsqlCon As String
    Private stateCode As String
    Private state_Code As String
    Public Con As NpgsqlConnection
    Private yr As String
    Private SFTP As String

    Public Property SFTPPath() As String
        Get
            Return SFTP
        End Get

        Set(ByVal value As String)
            SFTP = value
        End Set

    End Property
    Public Function Connect(ByVal stateCode As String) As NpgsqlConnection
        Dim ValRequest As New ValidateRequest
        ValRequest.app_BeginRequest(HttpContext.Current.Request)
        Dim match As New Match
        match.panchayat()
        Dim con As NpgsqlConnection = New NpgsqlConnection()
        Select Case (stateCode)
            Case "34"
                pgsqlCon = "Host=mord-postgrs-db.chdnrak0wfkn.ap-south-1.rds.amazonaws.com;Username=postgres;Database=newdord;Password=postgres123;SearchPath=poc_nrega_jh_nrega_jh; "

            Case Else
                pgsqlCon = "Host=mord-postgrs-db.chdnrak0wfkn.ap-south-1.rds.amazonaws.com;Username=postgres;Database=newdord;Password=postgres123;SearchPath=poc_nrega_jh_nrega_jh; "
        End Select

        con.ConnectionString = pgsqlCon

        Console.Write("Connecting to Postgres RDS...")
        Return con
    End Function

    Public Function Connection(ByVal stateCode As String) As String
        Dim sqlCon = ""
        Return sqlCon
    End Function

    Public Function ReadData(ByVal str As String, ByVal statecode As String) As Object
        Dim ValRequest As New ValidateRequest
        ValRequest.app_BeginRequest(HttpContext.Current.Request)
        Dim msg As String = ""
        If statecode <> "" Then
            Dim con As New NpgsqlConnection
            con = Connect(statecode)
            con.Open()
            Dim cmd As NpgsqlCommand = New NpgsqlCommand(str, con)
            Dim MyReader As NpgsqlDataReader
            MyReader = cmd.ExecuteReader()
            If MyReader.HasRows = True Then
                Return MyReader
            Else
                Return Nothing
            End If
        Else
            Return Nothing
        End If
    End Function

    Public Function TableData(ByVal str As String, ByVal statecode As String) As Object
        Dim ValRequest As New ValidateRequest
        ValRequest.app_BeginRequest(HttpContext.Current.Request)
        Dim msg As String = ""
        If statecode <> "" Then
            Dim con As New NpgsqlConnection
            con = Connect(statecode)
            Dim cmd As NpgsqlCommand = New NpgsqlCommand(str, con)
            Dim ad As Npgsql.NpgsqlDataAdapter = New Npgsql.NpgsqlDataAdapter(cmd)
            Dim dt As DataTable = New DataTable
            ad.Fill(dt)
            If dt.Rows.Count <> 0 Then
                Return dt
            Else
                Return Nothing
            End If
        Else
            Return Nothing
        End If
    End Function

    Public Function connectefms(ByVal state_code As String) As NpgsqlConnection
        Dim sqlCon = ""
        Dim objcon As NpgsqlConnection
        objcon = New NpgsqlConnection(sqlCon)
        Return objcon
    End Function
    Public Function connectCitizen(ByVal state_code As String) As NpgsqlConnection
        Dim sqlCon = ""
        Dim objcon As NpgsqlConnection
        objcon = New NpgsqlConnection(sqlCon)
        Return objcon
    End Function

    Public Function Connectfund() As Object
        Dim ValRequest As New ValidateRequest
        ValRequest.app_BeginRequest(HttpContext.Current.Request)
        pgsqlCon = ""
        Dim objcon As NpgsqlConnection
        objcon = New NpgsqlConnection(pgsqlCon)
        Return objcon
    End Function
    Public Function GetUrl(ByVal state_code As String, ByVal param As String) As String

        Dim path As String = ""
        Return path
    End Function

End Class

