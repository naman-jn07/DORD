Imports Microsoft.VisualBasic
Imports System.Data.SqlClient
Imports System.Data
Imports System.Threading
Imports System.Collections.Generic
Imports System
Imports System.Web
Imports System.Diagnostics
Imports System.IO
Imports Npgsql
Imports NpgsqlTypes
Imports ADODB


Public Class DAL_VB

    'Private sqlCon As String
    Private stateCode As String
    Private ds As DataSet = Nothing
    Private dt As DataTable = Nothing
    Private dr As NpgsqlDataReader = Nothing
    Private da As NpgsqlDataAdapter = Nothing
    Private cmd As NpgsqlCommand = Nothing
    Private conn As NpgsqlConnection = Nothing
    Shared countRE As New Dictionary(Of String, Integer)
    Shared countDE As New Dictionary(Of String, Integer)
    Private Const DE_RE_Ratio As Integer = 80
    Private instanceType As String
    Private oTransaction As NpgsqlTransaction = Nothing
    ' Private Const countMax As Integer = 8000
    Private Const min_sleep As Integer = 300
    Private Const max_sleep As Integer = 500
    Private Const min_sleep_DL As Integer = 4
    Private Const max_sleep_DL As Integer = 10
    Private Const cmdTimeOut As Integer = 300
    Private Const retriesCount As Integer = 4
    Private Const max_tries As Integer = 5
    Private Const DeadlockErrorCode As Integer = 1205
    Private Const LogRequired As Boolean = False
    Private Const msgServerBusy As String = "All ports are being used currently by available users. Kindly try after sometime."

    Shared Sub New()
        'Dim s As String
        'countDE.Add("10", 0)
        For i As Integer = 1 To 37
            countDE.Add(Right(CStr(100 + i), 2), 0)
            countRE.Add(Right(CStr(100 + i), 2), 0)
        Next

        countDE.Add("SECC", 0)
        countRE.Add("SECC", 0)

        countDE.Add("NREGA", 0)
        countRE.Add("NREGA", 0)

    End Sub

    Private Sub New(ByVal iType As String, ByVal sCode As String)
        instanceType = iType
        stateCode = sCode
        conn = Connect(sCode)
    End Sub
    ''' <summary>
    ''' Starts a database transaction.
    ''' </summary>
    Public Function BeginTransaction() As NpgsqlTransaction
        If oTransaction Is Nothing Then
            OpenConnection()
            conn.ReloadTypes()

            oTransaction = conn.BeginTransaction()
        End If
        Return oTransaction
    End Function

    ''' <summary>
    ''' Commits the database transaction
    ''' </summary>
    Public Sub CommitTransaction()
        Try
            If Not oTransaction Is Nothing Then
                oTransaction.Commit()
            End If
        Finally
            oTransaction = Nothing
            CloseConnection()
        End Try
    End Sub

    ''' <summary>
    ''' Rolls back a transaction from a pending state.
    ''' </summary>
    Public Sub RollBackTransaction()
        Try
            If Not oTransaction Is Nothing Then
                oTransaction.Rollback()
            End If
        Finally
            oTransaction = Nothing
            CloseConnection()
        End Try
    End Sub


    Private Sub OpenConnection()
        Dim rnd As Random = New Random()
        Dim cnt As Integer = 0
        If Not oTransaction Is Nothing Then
            Exit Sub
        End If
        If conn.State = ConnectionState.Closed Then
            If instanceType = "DE" Then
                'If (countDE.Item(stateCode) + countRE.Item(stateCode)) >= countMax Then
                '    Throw New Exception(msgServerBusy)
                'End If
                '=============================For Wait Begin======================================================
                'If (countDE.Item(stateCode) + countRE.Item(stateCode)) >= countMax Then
                'For count As Integer = 1 To 5
                '    If (countDE.Item(stateCode) + countRE.Item(stateCode)) < countMax Then
                '        Exit For
                '    Else
                '        Threading.Thread.Sleep(rnd.Next(min_sleep, max_sleep))
                '    End If
                'Next
                '    If (countDE.Item(stateCode) + countRE.Item(stateCode)) >= countMax Then
                '        Throw New Exception("Server is too Busy, Please try after sometime.")
                '    End If
                'End If
                '=============================For Wait End======================================================
                SyncLock countDE
                    countDE.Item(stateCode) += 1
                End SyncLock
                If LogRequired Then
                    'DalLog(stateCode, "OpenConnection " + (New System.Diagnostics.StackTrace).GetFrame(0).GetMethod.Name, "DE : " + Convert.ToString(countDE(stateCode)))
                    DalLog(stateCode, "OpenConnection ", "DE : " + Convert.ToString(countDE(stateCode)))
                End If
            Else
                '=============================For Wait Begin======================================================
                'If (countDE.Item(stateCode) + countRE.Item(stateCode)) >= countMax * DE_RE_Ratio / 100 Then
                '    For count As Integer = 1 To 5
                '        If (countDE.Item(stateCode) + countRE.Item(stateCode)) < countMax * DE_RE_Ratio / 100 Then
                '            Exit For
                '        Else
                ''            If count = 5 Then
                ''                Throw New Exception("Server is too Busy, Please try after sometime.")
                ''            End If
                '            Threading.Thread.Sleep(rnd.Next(min_sleep, max_sleep))
                '        End If
                '    Next
                '    If (countDE.Item(stateCode) + countRE.Item(stateCode)) >= countMax * DE_RE_Ratio / 100 Then
                '        Throw New Exception("Server is too Busy, Please try after sometime.")
                '    End If
                'End If
                '=============================For Wait End======================================================
                'If (countDE.Item(stateCode) + countRE.Item(stateCode)) >= countMax * DE_RE_Ratio / 100 Then
                '    Throw New Exception(msgServerBusy)
                'End If
                SyncLock countRE
                    countRE.Item(stateCode) += 1
                End SyncLock
                If LogRequired Then
                    DalLog(stateCode, "OpenConnection", "RE : " + Convert.ToString(countRE(stateCode)))
                End If
            End If
            conn.Open()

        End If
    End Sub

    Private Sub CloseConnection()
        Dim reduceCount As Boolean = False
        Try
            If conn.State = ConnectionState.Open And oTransaction Is Nothing Then
                reduceCount = True
                conn.Close()
            End If
        Finally
            If reduceCount Then
                If instanceType = "DE" Then
                    SyncLock countDE
                        countDE.Item(stateCode) -= 1
                    End SyncLock
                    If LogRequired Then
                        'DalLog(stateCode, "CloseConnection " + (New System.Diagnostics.StackTrace).GetFrame(0).GetMethod.Name, "DE : " + Convert.ToString(countDE(stateCode)))
                        DalLog(stateCode, "CloseConnection ", "DE : " + Convert.ToString(countDE(stateCode)))
                    End If
                Else
                    SyncLock countRE
                        countRE.Item(stateCode) -= 1
                    End SyncLock
                    If LogRequired Then
                        DalLog(stateCode, "CloseConnection", "RE : " + Convert.ToString(countRE(stateCode)))
                    End If
                End If
            End If
        End Try
    End Sub

    ''' <summary>
    ''' Closes the SqlDataReader object.
    ''' </summary>
    Public Sub CloseSqlDataReader(ByVal dr As NpgsqlDataReader)
        If dr IsNot Nothing And dr.IsClosed = False Then
            dr.Close()
        End If
        If oTransaction Is Nothing Then
            CloseConnection()
        End If
    End Sub

    ''' <summary>
    ''' Takes state_code as param and Returns instance of DAL_VB for DE
    ''' </summary>
    Public Shared Function GetInstanceforDE(ByVal FTO_scode As String) As DAL_VB
        Dim sCode As String
        sCode = FTO_scode
        '=====================Write Log Begins============================
        'Dim frame As StackFrame = New StackFrame(1, True)
        'Dim _app_log As Application_Log = New Application_Log()
        '_app_log.LogMessage("New Request, By Page:" + Path.GetFileName(frame.GetFileName()) + "; Method:" + frame.GetMethod.Name +
        '                    "; Count DE:" + countDE.Item(sCode).ToString() + "; Count RE:" + countRE.Item(sCode).ToString(), sCode)
        '=====================Write Log Ends============================
        'If (countDE.Item(sCode) + countRE.Item(sCode)) >= countMax Then
        '    Throw New Exception(msgServerBusy)
        'End If
        Return New DAL_VB("DE", sCode)
    End Function

    ''' <summary>
    ''' Takes state_code as param and Returns instance of DAL_VB for RE
    ''' </summary>
    Public Shared Function GetInstanceforRE(ByVal FTO_scode As String) As DAL_VB

        'Dim filepath As String = frame.GetFileName()
        'Dim filename As String = Path.GetFileName(frame.GetFileName())
        'Dim methodname As String = frame.GetMethod.Name

        Dim sCode As String
        sCode = FTO_scode
        '=====================Write Log Begins============================
        'Dim frame As StackFrame = New StackFrame(1, True)
        'Dim _app_log As Application_Log = New Application_Log()
        '_app_log.LogMessage("New Request, By Page:" + Path.GetFileName(frame.GetFileName()) + "; Method:" + frame.GetMethod.Name +
        '                    "; Count DE:" + countDE.Item(sCode).ToString() + "; Count RE:" + countRE.Item(sCode).ToString(), sCode)
        '=====================Write Log Ends============================
        'If (countDE.Item(sCode) + countRE.Item(sCode)) >= countMax * DE_RE_Ratio / 100 Then
        '    Throw New Exception(msgServerBusy)
        'End If
        Return New DAL_VB("RE", sCode)
    End Function

    ''' <summary>
    ''' Takes state_code as param and Returns instance of DAL_VB for DE
    ''' </summary>
    Public Shared Function GetInstanceforDE() As DAL_VB
        Dim sCode As String

        If Trim(System.Web.HttpContext.Current.Session("Entry_type")) = "D" Then
            sCode = System.Web.HttpContext.Current.Session("State_Code_d")
        Else
            sCode = System.Web.HttpContext.Current.Session("State_Code")
        End If
        '=====================Write Log Begins============================
        'Dim frame As StackFrame = New StackFrame(1, True)
        'Dim _app_log As Application_Log = New Application_Log()
        '_app_log.LogMessage("New Request, By Page:" + Path.GetFileName(frame.GetFileName()) + "; Method:" + frame.GetMethod.Name +
        '                    "; Count DE:" + countDE.Item(sCode).ToString() + "; Count RE:" + countRE.Item(sCode).ToString(), sCode)
        '=====================Write Log Ends==============================
        'If (countDE.Item(sCode) + countRE.Item(sCode)) >= countMax Then
        '    Throw New Exception(msgServerBusy)
        'End If
        'count = count + 1
        Return New DAL_VB("DE", sCode)
    End Function

    ''' <summary>
    ''' Takes state_code as param and Returns instance of DAL_VB for RE
    ''' </summary>
    Public Shared Function GetInstanceforRE() As DAL_VB
        Dim frame As StackFrame = New StackFrame(1, True)
        Dim sCode As String
        If Trim(System.Web.HttpContext.Current.Session("Entry_type")) = "D" Then
            sCode = System.Web.HttpContext.Current.Session("State_Code_d")
        Else
            sCode = System.Web.HttpContext.Current.Session("State_Code")
        End If
        '=====================Write Log Begins============================
        'Dim _app_log As Application_Log = New Application_Log()
        '_app_log.LogMessage("New Request, By Page:" + Path.GetFileName(frame.GetFileName()) + "; Method:" + frame.GetMethod.Name +
        '                    "; Count DE:" + countDE.Item(sCode).ToString() + "; Count RE:" + countRE.Item(sCode).ToString(), sCode)
        '=====================Write Log Ends==============================
        'If (countDE.Item(sCode) + countRE.Item(sCode)) >= countMax * DE_RE_Ratio / 100 Then
        '    Throw New Exception(msgServerBusy)
        'End If

        'count = count + 1
        Return New DAL_VB("RE", sCode)
    End Function


    Public Sub CloseInstance()
        CloseConnection()
        'If instanceType = "DE" Then
        '    countDE.Item(stateCode) -= 1
        'Else
        '    countRE.Item(stateCode) -= 1
        'End If
        'Me.Finalize()
    End Sub
    ''' <summary>
    ''' Takes Sqlcommand Executes a Transact-SQL statement against the connection and returns the number of rows affected.
    ''' </summary>
    Public Function ExecuteCommand_rowsaffected(ByVal cmd As NpgsqlCommand) As Integer
        Dim rowsaffected As Integer
        If Not oTransaction Is Nothing Then
            cmd.Transaction = oTransaction
        End If
        cmd.Connection = conn
        Dim retries As Integer = retriesCount
        Try
            OpenConnection()
            While retries > 0
                Try
                    cmd.CommandTimeout = cmdTimeOut
                    If oTransaction IsNot Nothing Then
                        cmd.Transaction = oTransaction
                    End If
                    rowsaffected = cmd.ExecuteNonQuery()
                    retries = 0
                Catch exception As SqlException
                    If exception.Number = DeadlockErrorCode Then
                        'If oTransaction IsNot Nothing Then
                        '    RollBackTransaction()
                        'End If
                        retries -= 1
                        If retries = 0 Then
                            Throw
                        End If
                        Thread.Sleep(New Random().[Next](min_sleep_DL, max_sleep_DL))
                    Else
                        Throw
                    End If
                End Try
            End While
        Catch ex As Exception
            retries = 0
            Throw
        Finally
            CloseConnection()
        End Try
        Return rowsaffected
    End Function


    ''' <summary>
    ''' Takes parameter name and Dictionary of Parameters
    ''' Executes a Transact-SQL statement against the connection and returns the number of rows affected.
    ''' </summary>
    Public Function ExecuteProc_rowsaffected(ByVal SQLQuery As String, ByVal param As Dictionary(Of String, Object)) As Integer
        Dim rowsaffected As Integer
        cmd = New NpgsqlCommand(SQLQuery, conn)
        If Not oTransaction Is Nothing Then
            cmd.Transaction = oTransaction
        End If
        cmd.CommandType = CommandType.StoredProcedure
        Dim pair As KeyValuePair(Of String, Object)
        If param.Values.Count > 0 Then
            For Each pair In param
                cmd.Parameters.AddWithValue(pair.Key, pair.Value)
            Next
        End If
        Dim retries As Integer = retriesCount
        Try
            OpenConnection()
            While retries > 0
                Try
                    cmd.CommandTimeout = cmdTimeOut
                    If oTransaction IsNot Nothing Then
                        cmd.Transaction = oTransaction
                    End If
                    rowsaffected = cmd.ExecuteNonQuery()
                    retries = 0
                Catch exception As SqlException
                    If exception.Number = DeadlockErrorCode Then
                        retries -= 1
                        If retries = 0 Then
                            Throw
                        End If
                        Thread.Sleep(New Random().[Next](min_sleep_DL, max_sleep_DL))
                    Else
                        Throw
                    End If
                End Try
            End While
        Catch ex As Exception
            retries = 0
            Throw
        Finally
            CloseConnection()
        End Try
        Return rowsaffected
    End Function

    ''' <summary>
    ''' Takes StringCommand Executes a Transact-SQL statement against the connection and returns the number of rows affected.
    ''' </summary>
    Public Function ExecuteCommand_rowsaffected(ByVal StringSQL As String) As Integer
        Dim rowsaffected As Integer
        cmd = New NpgsqlCommand(StringSQL, conn)
        If Not oTransaction Is Nothing Then
            cmd.Transaction = oTransaction
        End If
        Dim retries As Integer = retriesCount
        Try
            OpenConnection()
            While retries > 0
                Try
                    cmd.CommandTimeout = cmdTimeOut
                    If oTransaction IsNot Nothing Then
                        cmd.Transaction = oTransaction
                    End If
                    rowsaffected = cmd.ExecuteNonQuery()
                    retries = 0
                Catch exception As SqlException
                    If exception.Number = DeadlockErrorCode Then
                        retries -= 1
                        If retries = 0 Then
                            Throw
                        End If
                        Thread.Sleep(New Random().[Next](min_sleep_DL, max_sleep_DL))
                    Else
                        Throw
                    End If
                End Try
            End While
        Catch ex As Exception
            retries = 0
            Throw
        Finally
            CloseConnection()
        End Try
        Return rowsaffected
    End Function

    ''' <summary>
    ''' Takes String Command as parameter and Returns a DataSet Object
    ''' </summary>
    Public Function ExecuteCommand_ds(ByVal StringSQL As String) As DataSet
        cmd = New NpgsqlCommand(StringSQL, conn)
        If Not oTransaction Is Nothing Then
            cmd.Transaction = oTransaction
        End If
        Dim retries As Integer = retriesCount
        While retries > 0
            Try
                ds = New DataSet()
                da = New NpgsqlDataAdapter(cmd)
                da.SelectCommand.CommandTimeout = cmdTimeOut
                da.Fill(ds)
                retries = 0
            Catch exception As SqlException
                If exception.Number = DeadlockErrorCode Then
                    retries -= 1
                    If retries = 0 Then
                        Throw
                    End If
                    Thread.Sleep(New Random().[Next](min_sleep_DL, max_sleep_DL))
                Else
                    Throw
                End If
            Catch ex As Exception
                retries = 0
                Throw
            End Try
        End While
        Return ds
    End Function

    ''' <summary>
    ''' Takes SqlCommand as parameter and Returns a SqlDataReader Object
    ''' </summary>
    Public Function ExecuteCommand_dr(ByVal cmd As NpgsqlCommand) As NpgsqlDataReader

        cmd.Connection = conn
        BeginTransaction()

        If Not oTransaction Is Nothing Then
            cmd.Transaction = oTransaction
        End If
        Dim retries As Integer = retriesCount
        Try
            OpenConnection()
            While retries > 0
                Try
                    cmd.CommandTimeout = cmdTimeOut
                    dr = cmd.ExecuteReader()

                    retries = 0
                Catch exception As SqlException
                    If exception.Number = DeadlockErrorCode Then
                        retries -= 1
                        If retries = 0 Then
                            Throw
                        End If
                        Thread.Sleep(New Random().[Next](min_sleep_DL, max_sleep_DL))
                    Else
                        Throw
                    End If
                End Try
            End While
        Catch ex As Exception
            retries = 0
            CloseSqlDataReader(dr)
            Throw
        End Try
        Return dr
    End Function


    Public Function ExecuteCommand_dr(ByVal cmd As NpgsqlCommand, ByRef outParam As NpgsqlParameter) As NpgsqlDataReader

        cmd.Connection = conn
        BeginTransaction()

        If Not oTransaction Is Nothing Then
            cmd.Transaction = oTransaction
        End If
        Dim retries As Integer = retriesCount
        Try
            OpenConnection()
            While retries > 0
                Try
                    cmd.CommandTimeout = cmdTimeOut
                    cmd.ExecuteNonQuery()
                    Dim refCursorCommand = New NpgsqlCommand("FETCH ALL IN \"" + outParam.Value + " \ "", conn)
                    dr = refCursorCommand.ExecuteReader()

                    retries = 0
                Catch exception As SqlException
                    If exception.Number = DeadlockErrorCode Then
                        retries -= 1
                        If retries = 0 Then
                            Throw
                        End If
                        Thread.Sleep(New Random().[Next](min_sleep_DL, max_sleep_DL))
                    Else
                        Throw
                    End If
                End Try
            End While
        Catch ex As Exception
            retries = 0
            CloseSqlDataReader(dr)
            Throw
        End Try
        Return dr
    End Function







    Public Function GetDBConnection() As NpgsqlConnection
        Return conn
    End Function




    ''' <summary>
    ''' Takes SqlCommand as parameter and Returns DataSet Object
    ''' </summary>
    Public Function ExecuteCommand_ds(ByVal cmd As NpgsqlCommand) As DataSet
        cmd.Connection = conn
        If Not oTransaction Is Nothing Then
            cmd.Transaction = oTransaction
        End If
        Dim retries As Integer = retriesCount
        While retries > 0
            Try
                ds = New DataSet()
                da = New NpgsqlDataAdapter(cmd)
                da.SelectCommand.CommandTimeout = cmdTimeOut
                da.Fill(ds)
                retries = 0
            Catch exception As SqlException
                If exception.Number = DeadlockErrorCode Then
                    retries -= 1
                    If retries = 0 Then
                        Throw
                    End If
                    Thread.Sleep(New Random().[Next](min_sleep_DL, max_sleep_DL))
                Else
                    Throw
                End If
            Catch ex As Exception
                retries = 0
                Throw
            End Try
        End While
        Return ds
    End Function

    ''' <summary>
    ''' Takes SqlCommand and Table name
    ''' Returns DataSet with Table name
    ''' </summary>
    Public Function ExecuteCommand_ds_WithTable(ByVal cmd As NpgsqlCommand, ByVal tbl As String) As DataSet
        cmd.Connection = conn
        If Not oTransaction Is Nothing Then
            cmd.Transaction = oTransaction
        End If
        Dim retries As Integer = retriesCount
        While retries > 0
            Try
                ds = New DataSet()
                da = New NpgsqlDataAdapter(cmd)
                da.SelectCommand.CommandTimeout = cmdTimeOut
                da.Fill(ds, tbl)
                retries = 0
            Catch exception As SqlException
                If exception.Number = DeadlockErrorCode Then
                    retries -= 1
                    If retries = 0 Then
                        Throw
                    End If
                    Thread.Sleep(New Random().[Next](min_sleep_DL, max_sleep_DL))
                Else
                    Throw
                End If
            Catch ex As Exception
                retries = 0
                Throw
            End Try
        End While
        Return ds
    End Function

    ''' <summary>
    ''' Takes SqlCommand as parameter and Returns a DataTable Object
    ''' </summary>
    Public Function ExecuteCommand_dt(ByVal cmd As NpgsqlCommand) As DataTable
        cmd.Connection = conn
        If Not oTransaction Is Nothing Then
            cmd.Transaction = oTransaction
        End If

        Dim retries As Integer = retriesCount
        Try
            OpenConnection()
            While retries > 0
                Try
                    cmd.CommandTimeout = cmdTimeOut
                    dr = cmd.ExecuteReader()
                    dt = New DataTable()
                    dt.Load(dr)
                    dr.Close()
                    retries = 0
                Catch exception As SqlException
                    If exception.Number = DeadlockErrorCode Then
                        retries -= 1
                        If retries = 0 Then
                            Throw
                        End If
                        Thread.Sleep(New Random().[Next](min_sleep_DL, max_sleep_DL))
                    Else
                        Throw
                    End If
                End Try
            End While
        Catch ex As Exception
            retries = 0
            Throw
        Finally
            CloseConnection()
        End Try
        Return dt
    End Function


    Public Function ExecuteCommand_dt(ByVal cmd As NpgsqlCommand, ByVal outParamName As String) As DataTable
        cmd.Connection = conn
        BeginTransaction()
        conn.ReloadTypes()

        If Not oTransaction Is Nothing Then
            cmd.Transaction = oTransaction
        End If

        Dim outParam As NpgsqlParameter = New NpgsqlParameter(outParamName, NpgsqlDbType.Refcursor)
        outParam.Direction = ParameterDirection.Output
        cmd.Parameters.Add(outParam)

        Dim retries As Integer = retriesCount
        Try
            While retries > 0
                Try
                    cmd.CommandTimeout = cmdTimeOut
                    cmd.ExecuteNonQuery()
                    Dim commandText As String = "FETCH ALL IN """ + outParam.Value + """"
                    Dim refCursorCommand As NpgsqlCommand = New NpgsqlCommand(commandText, conn)
                    dr = refCursorCommand.ExecuteReader()
                    dt = New DataTable()
                    dt.Load(dr)
                    dr.Close()
                    CommitTransaction()

                    retries = 0
                Catch exception As SqlException
                    If exception.Number = DeadlockErrorCode Then
                        retries -= 1
                        If retries = 0 Then
                            Throw
                        End If
                        Thread.Sleep(New Random().[Next](min_sleep_DL, max_sleep_DL))
                    Else
                        RollBackTransaction()
                        Throw
                    End If
                End Try
            End While
        Catch ex As Exception
            retries = 0
            RollBackTransaction()
            Throw
        Finally
            CloseConnection()
        End Try
        Return dt
    End Function

    ''' <summary>
    ''' Takes Procedure name as String and Dictionary as parameters
    ''' Returns SqlDataReader
    ''' </summary>
    Public Function ExecuteProc_dr(ByVal SQLQuery As String, ByVal param As Dictionary(Of String, Object)) As NpgsqlDataReader
        cmd = New NpgsqlCommand(SQLQuery, conn)
        If Not oTransaction Is Nothing Then
            cmd.Transaction = oTransaction
        End If
        cmd.CommandType = CommandType.StoredProcedure
        Dim pair As KeyValuePair(Of String, Object)
        If param.Values.Count > 0 Then
            For Each pair In param
                cmd.Parameters.AddWithValue(pair.Key, pair.Value)
            Next
        End If

        Dim retries As Integer = retriesCount
        Try
            OpenConnection()
            While retries > 0
                Try
                    cmd.CommandTimeout = cmdTimeOut
                    dr = cmd.ExecuteReader(CommandBehavior.SingleResult)
                    retries = 0
                Catch exception As SqlException
                    If exception.Number = DeadlockErrorCode Then
                        retries -= 1
                        If retries = 0 Then
                            Throw
                        End If
                        Thread.Sleep(New Random().[Next](min_sleep_DL, max_sleep_DL))
                    Else
                        Throw
                    End If
                End Try
            End While
        Catch ex As Exception
            retries = 0
            CloseConnection()
            Throw
        End Try
        Return dr
    End Function

    ''' <summary>
    ''' Takes Procedure name as String and Dictionary as parameters
    ''' Returns DataSet
    ''' </summary>
    Public Function ExecuteProc_ds(ByVal SQLQuery As String, ByVal param As Dictionary(Of String, Object)) As DataSet
        cmd = New NpgsqlCommand(SQLQuery, conn)
        If Not oTransaction Is Nothing Then
            cmd.Transaction = oTransaction
        End If
        cmd.CommandType = CommandType.StoredProcedure
        Dim pair As KeyValuePair(Of String, Object)
        If param.Values.Count > 0 Then
            For Each pair In param
                cmd.Parameters.AddWithValue(pair.Key, pair.Value)
            Next
        End If
        Dim retries As Integer = retriesCount
        While retries > 0
            Try
                ds = New DataSet()
                da = New NpgsqlDataAdapter(cmd)
                da.SelectCommand.CommandTimeout = cmdTimeOut
                da.Fill(ds)
                retries = 0
            Catch exception As SqlException
                If exception.Number = DeadlockErrorCode Then
                    retries -= 1
                    If retries = 0 Then
                        Throw
                    End If
                    Thread.Sleep(New Random().[Next](min_sleep_DL, max_sleep_DL))
                Else
                    Throw
                End If
            Catch ex As Exception
                retries = 0
                Throw
            End Try
        End While
        Return ds
    End Function

    ''' <summary>
    ''' Takes Procedure name as String and Dictionary as parameters
    ''' Returns DataTable
    ''' </summary>
    Public Function ExecuteProc_dt(ByVal SQLQuery As String, ByVal param As Dictionary(Of String, Object)) As DataTable
        cmd = New NpgsqlCommand(SQLQuery, conn)
        If Not oTransaction Is Nothing Then
            cmd.Transaction = oTransaction
        End If
        cmd.CommandType = CommandType.StoredProcedure
        Dim pair As KeyValuePair(Of String, Object)
        If param.Values.Count > 0 Then
            For Each pair In param
                cmd.Parameters.AddWithValue(pair.Key, pair.Value)
            Next
        End If

        Dim retries As Integer = retriesCount

        Try
            OpenConnection()
            While retries > 0
                Try
                    cmd.CommandTimeout = cmdTimeOut
                    dr = cmd.ExecuteReader()
                    dt = New DataTable()
                    dt.Load(dr)
                    dr.Close()
                    retries = 0
                Catch exception As SqlException
                    If exception.Number = DeadlockErrorCode Then
                        retries -= 1
                        If retries = 0 Then
                            Throw
                        End If
                        Thread.Sleep(New Random().[Next](min_sleep_DL, max_sleep_DL))
                    Else
                        Throw
                    End If
                End Try
            End While
        Catch ex As Exception
            retries = 0
            Throw
        Finally
            CloseConnection()
        End Try
        Return dt
    End Function

    ''' <summary>
    ''' Executes the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored.
    ''' </summary>
    Function ExecuteScalar(ByVal cmd As NpgsqlCommand) As Object
        Dim result As Object = Nothing
        cmd.Connection = conn
        If Not oTransaction Is Nothing Then
            cmd.Transaction = oTransaction
        End If

        Dim retries As Integer = retriesCount
        Try
            OpenConnection()
            While retries > 0
                Try
                    cmd.CommandTimeout = cmdTimeOut
                    If oTransaction IsNot Nothing Then
                        cmd.Transaction = oTransaction
                    End If
                    result = cmd.ExecuteScalar()
                    retries = 0
                Catch exception As SqlException
                    If exception.Number = DeadlockErrorCode Then
                        retries -= 1
                        If retries = 0 Then
                            Throw
                        End If
                        Thread.Sleep(New Random().[Next](min_sleep_DL, max_sleep_DL))
                    Else
                        Throw
                    End If
                End Try
            End While
        Catch ex As Exception
            retries = 0
            Throw
        Finally
            CloseConnection()
        End Try
        Return result
    End Function
    ''' <summary>
    ''' Takes NpgsqlCommand as parameter and returns a SqlDataAdapter
    ''' </summary>
    Public Function GetAdapter(ByVal cmd As NpgsqlCommand) As NpgsqlDataAdapter
        cmd.Connection = conn
        If Not oTransaction Is Nothing Then
            cmd.Transaction = oTransaction
        End If
        ' OpenConnection()
        ' ds = New DataSet()
        da = New NpgsqlDataAdapter(cmd)
        Return da
    End Function

    'Public Sub DisposeAdapter()
    '    da.Dispose()
    'End Sub

    Private Function Connect(ByVal stateCode As String) As NpgsqlConnection
        Dim conn As ConnectNREGA = New ConnectNREGA()
        Dim con As NpgsqlConnection
        con = conn.Connect(stateCode)
        'con = New SqlConnection(conn.Connection(stateCode))
        'con = New SqlConnection(sqlCon)
        Return con
    End Function

    Private Function connectCitizen(ByVal stateCode As String) As NpgsqlConnection
        Dim conn As ConnectNREGA = New ConnectNREGA()
        Dim con As NpgsqlConnection

        con = conn.connectCitizen(stateCode)
        Return con
    End Function



    Private Sub DalLog(ByVal StateCode As String, ByVal EventType As String, ByVal CountConnection As String)
        Dim AbsoluteUrl As String = HttpContext.Current.Request.Url.AbsoluteUri.ToString()
        Dim AbsolutePath As String = HttpContext.Current.Request.Url.AbsolutePath.ToString()

        'Dim Insert_LogDatetime As New DateTime()


        'Insert_LogDatetime = DateTime.Parse(DateTime.Now.ToString(), System.Globalization.CultureInfo.CreateSpecificCulture("en-AU").DateTimeFormat)

        'Insert_LogDatetime = DateTime.Now



        Dim connection As NpgsqlConnection
        Dim conobj As New ConnectNREGA()
        connection = conobj.Connect(StateCode)
        ' Connect with NREGA DB Server
        Try
            Dim command As New NpgsqlCommand()
            connection.Open()
            command.Connection = connection
            command.CommandText = "Insert Into DalLog (StateCode,TypeOfEvent,CountConnection, AbsoluteUrl, AbsolutePath, Log_Datetime ) Values (@StateCode, @TypeOfEvent, @CountConnection, @AbsoluteUrl, @AbsolutePath, @Log_Datetime)"
            'command.CommandText = "Insert Into DalLog (StateCode,TypeOfEvent,CountConnection, AbsoluteUrl, AbsolutePath) Values (@StateCode, @TypeOfEvent, @CountConnection, @AbsoluteUrl, @AbsolutePath)"
            command.Parameters.Add("@StateCode", SqlDbType.VarChar, 2).Value = StateCode
            command.Parameters.Add("@TypeOfEvent", SqlDbType.VarChar, 200).Value = EventType
            command.Parameters.Add("@CountConnection", SqlDbType.VarChar, 50).Value = CountConnection
            command.Parameters.Add("@AbsoluteUrl", SqlDbType.VarChar, 100).Value = AbsoluteUrl
            command.Parameters.Add("@AbsolutePath", SqlDbType.VarChar, 50).Value = AbsolutePath
            command.Parameters.Add("@Log_Datetime", SqlDbType.DateTime).Value = DateTime.Now
            command.ExecuteScalar()
            'connection.Close()
        Catch
            'HttpContext.Current.Response.Write("ex")

            'Throw

        Finally
            'If connection.State = ConnectionState.Open Then
            connection.Close()
            'End If
        End Try
    End Sub
End Class
