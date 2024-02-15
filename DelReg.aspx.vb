Imports System
Imports System.Web
Imports System.Data
Imports System.Data.SqlClient
Imports Npgsql
Imports NpgsqlTypes
Partial Class DelReg
    Inherits System.Web.UI.Page
    Dim DT1, DT3, DT4 As DataTable
    Dim dal As DAL_VB
    Dim mycmd, cmd, cmd1 As NpgsqlCommand

    Dim Lgn As Lang_Labels
    Public HomePage As String
    Dim RegDate, Cat, dt, MaxDate, deldt, dt2 As Array
    Dim State_Code, DiffDays, tblWork_Demand, tblWork_DemandYYYY, del_date, demand_id, Event_Date_MDY, tblWork_Allotted As String
    Dim tblWork_AllottedYYYY, member_exists, RegDemanded, RegInMustroll, tblMustrollYYYY, MaxDt_To, MaxDt_To_MDY, Entry_by, tblMustroll, Home As String
    Dim Reg_Date, finyear, reg, Applicants, Panchayat_Name, Level, District_Code As String
    Dim Block_Code, Panchayat_Code, Event_Date_DMY, Fin_Year, shortnm, dbSelectCommand As String
    Dim Sb As StringBuilder
    Dim myutil As New Util
    Dim DT_FY As DataTable
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            If Session("State_Code_d") = "" Or Session("finyear_d") = "" Or Trim(Session("ExeL_d")) = "" Then
                Server.Transfer("logout.aspx")
            ElseIf Session("State_Code_d") = "32" And Trim(Session("ExeL_d")) <> "DPC" Then 'this option is disabled for WB.
                Response.Write("'Delete Registration/JC' option has been moved to DPC login. Pls, request your DPC to delete the Job-card.")
                Response.End()
            Else
                dal = DAL_VB.GetInstanceforDE()
            End If

            lblmsg.Visible = False
            State_Code = Session("State_Code_d")
            District_Code = Session("District_Code_d")
            Block_Code = Session("Block_Code_d")
            finyear = Session("finyear_d")
            Entry_by = Session("Entry_by")

            If Trim(Session("ExeL_d")) = "GP" Then
                Panchayat_Code = Session("Panchayat_Code_d")
                Panchayat_Name = Session("Panchayat_Name_d")
                HomePage = "IndexFrame2.aspx?Panchayat_Code=" & Panchayat_Code
                Level = "GP"
            ElseIf Session("ExeL_d") = "PO" Then
                Panchayat_Code = Trim(hf_PanchayatCode.Value)
                HomePage = "ProgOfficer/PoIndexFrame2.aspx?Block_Code=" & Block_Code
                Level = "PO"
            ElseIf Session("ExeL_d") = "DPC" Then
                Level = "DPC"
                HomePage = "DPC/dpcindexFrame2.aspx"
            End If

            If ddlpnch.SelectedValue = "" Or ddlpnch.SelectedIndex <= 0 Then
                District_Code = Session("District_Code_d")
                Block_Code = Session("Block_Code_d")
                Panchayat_Code = Session("Panchayat_Code_d")
            Else
                District_Code = Left(ddlpnch.SelectedValue, 4)
                Block_Code = Left(ddlpnch.SelectedValue, 7)
                Panchayat_Code = ddlpnch.SelectedValue
            End If

            Sb = New StringBuilder()
            Lgn = New Lang_Labels(Session("state_code_d"), "3")
            reg = Sb.Append(Lgn.Short_Name).Append(Right(District_Code, 2)).Append("Registration").ToString
            Sb.Length = 0
            Applicants = Sb.Append(Lgn.Short_Name).Append(Right(District_Code, 2)).Append("Applicants").ToString

            If (Not IsPostBack) Then

                '###############CSRF(Cross Script Request Forgery Attack ) Prevention Code####
                If Request.Cookies("AuthToken") IsNot Nothing Then
                    If Session("AuthToken") <> Request.Cookies("AuthToken").Value Then
                        Session.Abandon()
                        Session.RemoveAll()
                        Response.Write("Session Expired due to Illegal Operation Performed..!!")
                        Response.End()
                    Else
                        ASPAuth.Value = Session("AuthToken")
                    End If
                Else
                    Session.Abandon()
                    Session.RemoveAll()
                    Response.Write("Session Expired due to Illegal Operation Performed..!!")
                    Response.End()
                End If
                '###############CSRF(Cross Script Request Forgery Attack ) Prevention Code####

                Dim Ht As Hashtable = New Hashtable
                Ht = Lgn.get_langs(Session("state_code_d"), "3")
                Dim k As Integer = 0
                lblStatetxt.Text = Ht.Item("state") & ":"
                lblDisttxt.Text = Ht.Item("dist") & ":"
                lblBlocktxt.Text = Ht.Item("blk") & ":"
                lblPanchayattxt.Text = Ht.Item("panch") & ":"
                lblVilltxt.Text = Ht.Item("vill") & ":"
                lblRegNo.Text = Ht.Item("regid") & ":"
                ViewState("SelectPanch") = Ht.Item("selectpanch")
                ViewState("SelectVill") = Ht.Item("selectvill")
                ViewState("selectRegNo") = Ht.Item("selectregno")

                lblState.Text = Session("State_Name_d")
                lblDistrict.Text = Session("District_Name_d")
                lblBlk.Text = Session("Block_Name_d")

                If Level = "PO" Then
                    ddlBlock.Visible = False
                    panch_bind(Session("block_code_d"))
                    lblMandry.Visible = True
                    ddlpnch.Visible = True
                    lblPnch.Visible = False
                    ddlvillage.ClearSelection()
                ElseIf Level = "GP" Then
                    ddlBlock.Visible = False
                    lblPnch.Text = Panchayat_Name

                    'Sb.Length = 0
                    'dbSelectCommand = Sb.Append("Execute Display_Villages @Panchayat_Code = '").Append(Panchayat_Code).Append("', @finyr='").Append(finyear).Append("'").ToString
                    'DT1 = dal.ExecuteCommand_dt(New NpgsqlCommand(dbSelectCommand))

                    Using cmd As New NpgsqlCommand
                        cmd.CommandText = "Display_Villages"
                        cmd.CommandType = CommandType.StoredProcedure
                        cmd.Parameters.AddWithValue("@Panchayat_Code", Panchayat_Code)
                        cmd.Parameters.AddWithValue("@finyr", finyear)
                        DT1 = dal.ExecuteCommand_dt(cmd)
                    End Using

                    If State_Code = "16" And ddlpnch.SelectedValue <> ViewState("SelectPanch") Then
                        ddlvillage.Items.Insert(0, ViewState("SelectVill"))
                        For Each drow As DataRow In DT1.Rows
                            Sb.Length = 0
                            ddlvillage.Items.Add(New ListItem(Sb.Append(Mid((drow("Village_Code")), 11, 3)).Append("-").Append(drow("Village_Name_Local")).ToString, drow("village_Code")))
                        Next
                        ddlvillage.DataBind()
                    ElseIf Panchayat_Code <> "" And State_Code <> "16" Then
                        ddlvillage.DataSource = DT1
                        ddlvillage.DataTextField = "Village_Name_Local"
                        ddlvillage.DataValueField = "village_Code"
                        ddlvillage.DataBind()
                        ddlvillage.Items.Insert(0, ViewState("SelectVill"))
                    End If

                ElseIf Level = "DPC" Then
                    If Session("state_code_d") = "32" Then  'option given at DPC login for WB only.
                        ddlpnch.Visible = True
                        block_bind(Session("District_code_d"))
                        ddlBlock.Visible = True
                        lblBlk.Visible = False
                        ddlpnch.ClearSelection()
                        ddlvillage.ClearSelection()
                    Else
                        Response.Write("Unauthorised access!!")
                        Response.End()
                    End If
                End If

                'Using cmd As New NpgsqlCommand
                '    dbSelectCommand = "select Financial_Year from fin_year_temp (nolock) order by YEAR_code desc"
                '    cmd.CommandText = dbSelectCommand
                '    cmd.Parameters.Clear()
                '    DT1 = dal.ExecuteCommand_dt(cmd)
                '    If DT1.Rows.Count > 0 Then
                '        ViewState("Financial_Year") = DT1
                '    End If
                'End Using

                Using cmd As New NpgsqlCommand
                    cmd.CommandText = "GetFY_Since_begining"
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("par_state_code", State_Code)
                    DT1 = dal.ExecuteCommand_dt(cmd, "p_refcur")
                    If DT1.Rows.Count > 0 Then
                        ViewState("Financial_Year") = DT1
                    End If
                End Using

            End If

            '###############CSRF(Cross Script Request Forgery Attack ) Prevention Code####
            If ASPAuth.Value Is Nothing Or ASPAuth.Value = "" Then
                Session.Abandon()
                Session.RemoveAll()
                'Response.Write("Session Expired due to Illegal Operation Performed..!!")
                'Response.End()
                Response.Redirect("logout.aspx", False)
            Else
                If ASPAuth.Value <> Request.Cookies("AuthToken").Value Then
                    Session.Abandon()
                    Session.RemoveAll()
                    'Response.Write("Session Expired due to Illegal Operation Performed..!!")
                    'Response.End()
                    Response.Redirect("logout.aspx", False)
                End If
            End If
            '###############CSRF(Cross Script Request Forgery Attack ) Prevention Code####
        Catch ex1 As NullReferenceException
            lblmsg.Visible = True
            lblmsg.Text = "Null-Error found in Loading Page"
        Catch ex2 As SqlException
            lblmsg.Visible = True
            lblmsg.Text = "DB-Error found in Loading Page"
        Catch ex As Exception
            lblmsg.Visible = True
            lblmsg.Text = "Error found in Loading Page"
        End Try
        DT1 = New DataTable()

    End Sub
    Protected Sub ddlpnch_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlpnch.SelectedIndexChanged
        Try
            Sb = New StringBuilder
            ddlReg.Items.Clear()
            ddlvillage.Items.Clear()
            tblDetails.Visible = False
            lblNote.Visible = False
            grdData.DataSource = DT1
            grdData.DataBind()
            BtnSubmit.Visible = False
            BtnCancel.Visible = False

            'dbSelectCommand = Sb.Append("Execute Display_Villages @Panchayat_Code = '").Append(ddlpnch.SelectedValue).Append("', @finyr='").Append(finyear).Append("'").ToString
            'hf_PanchayatCode.Value = ddlpnch.SelectedValue
            'DT1 = dal.ExecuteCommand_dt(New NpgsqlCommand(dbSelectCommand))

            Using cmd As New NpgsqlCommand
                cmd.CommandText = "Display_Villages"
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("par_panchayat_code", NpgsqlDbType.Varchar, ddlpnch.SelectedValue)
                cmd.Parameters.AddWithValue("par_finyr", NpgsqlDbType.Varchar, finyear)

                DT1 = dal.ExecuteCommand_dt(cmd, "ref_cursor")

            End Using
            hf_PanchayatCode.Value = ddlpnch.SelectedValue

            If State_Code = "16" Then
                ddlvillage.Items.Insert(0, ViewState("SelectVill"))
                For Each drow As DataRow In DT1.Rows
                    Sb.Length = 0
                    ddlvillage.Items.Add(New ListItem(Sb.Append(Mid((drow("Village_Code")), 11, 3)).Append("-").Append(drow("Village_Name_Local")).ToString, drow("village_Code")))
                Next
                ddlvillage.DataBind()
            Else
                ddlvillage.DataSource = DT1
                ddlvillage.DataTextField = "Village_Name_Local"
                ddlvillage.DataValueField = "village_Code"
                ddlvillage.DataBind()
                If DT1.Rows.Count > 0 Then
                    ddlvillage.Items.Insert(0, ViewState("SelectVill"))
                ElseIf DT1.Rows.Count <= 0 And ddlpnch.SelectedValue <> ViewState("SelectPanch") Then
                    lblmsg.Visible = True
                    lblmsg.Text = "No Panchayat found for the selected Block !"
                    Return
                End If
            End If
        Catch ex1 As SqlException
            lblmsg.Visible = True
            lblmsg.Text = "DB-Error found in Binding Panchayat"
        Catch ex As Exception
            lblmsg.Visible = True
            lblmsg.Text = "Error found in Binding Panchayat"
        End Try
    End Sub
    Protected Sub ddlvillage_SelectedIndexChanged1(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlvillage.SelectedIndexChanged
        Try
            ddlReg.ClearSelection()
            ddlReg.Items.Clear()
            tblDetails.Visible = False
            lblNote.Visible = False
            grdData.DataSource = DT1
            grdData.DataBind()
            BtnSubmit.Visible = False
            BtnCancel.Visible = False

            'Sb = New StringBuilder
            If (ddlvillage.SelectedItem.Text <> ViewState("SelectVill")) Then
                'dbSelectCommand = Sb.Append("Execute cboRegNo_NotDel @tblReg = '").Append(reg).Append("', @Village_Code='").Append(ddlvillage.SelectedValue).Append("'").ToString
                'DT1 = dal.ExecuteCommand_dt(New NpgsqlCommand(dbSelectCommand))

                Using cmd As New NpgsqlCommand
                    cmd.CommandText = "cboRegNo_NotDel"
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("par_tblreg", reg)
                    cmd.Parameters.AddWithValue("par_village_code", ddlvillage.SelectedValue)
                    DT1 = dal.ExecuteCommand_dt(cmd, "ref_cursor")
                End Using

                If (DT1.Rows.Count > 0) Then
                    ddlReg.DataSource = DT1
                    ddlReg.DataTextField = "Reg_No"
                    ddlReg.DataValueField = "Reg_No"
                    ddlReg.DataBind()
                    ddlReg.Items.Insert(0, New ListItem(ViewState("selectRegNo"), ViewState("selectRegNo")))
                Else
                    lblmsg.Visible = True
                    lblmsg.Text = "No Registration found for the selected Village !"
                    Return
                End If
            End If
        Catch ex1 As SqlException
            lblmsg.Visible = True
            lblmsg.Text = "DB-Error found in binding Registration No"
            myutil.Exception_log(ex1)
        Catch ex As Exception
            lblmsg.Visible = True
            lblmsg.Text = "Error found in binding Registration No"
        End Try
    End Sub
    Protected Sub ddlReg_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlReg.SelectedIndexChanged
        Try

            txtHead.ReadOnly = True
            txtFatHus.ReadOnly = True
            txtEffect.Text = ""
            If ddlReg.SelectedItem.Text = ViewState("selectRegNo") Then
                grdData.DataSource = DT1
                grdData.DataBind()
                BtnSubmit.Visible = False
                BtnCancel.Visible = False
                tblDetails.Visible = False
                lblNote.Visible = False
                Return
            End If

            Dim no_of_app As Integer = 0

            Sb = New StringBuilder()
            cmd1 = New NpgsqlCommand()
            If (Level = "PO") Then
                Panchayat_Code = ddlpnch.SelectedValue
            End If
            DT_FY = ViewState("Financial_Year")
            If DT_FY.Rows.Count <= 0 Then
                lblmsg.Visible = True
                lblmsg.Text = "Financial years are missing..!!"
                Exit Sub
            End If

            chkCategory.ClearSelection()
            If (ddlvillage.SelectedValue <> "" And ddlReg.SelectedValue <> "" And ddlReg.SelectedValue <> ViewState("selectRegNo")) Then

                tblDetails.Visible = True
                'dbSelectCommand = Sb.Append("Execute getRegNoFamily @tblReg = '").Append(reg).Append("', @Reg_No = '").Append(ddlReg.SelectedValue).Append("'").ToString
                'DT1 = dal.ExecuteCommand_dt(New NpgsqlCommand(dbSelectCommand))

                Using cmd As New NpgsqlCommand
                    cmd.CommandText = "getRegNoFamily"
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("par_tblreg", reg)
                    cmd.Parameters.AddWithValue("par_reg_no", ddlReg.SelectedValue)
                    DT1 = dal.ExecuteCommand_dt(cmd, "ref_cursor")
                End Using

                If DT1.Rows.Count > 0 Then
                    lblNote.Visible = True
                    grdData.Visible = True
                    BtnCancel.Visible = True
                    BtnSubmit.Visible = True
                    Reg_Date = DT1.Rows(0)("Registration_Date").ToString()
                    RegDate = Reg_Date.Split("/")  '/ in Date comes only when US culture is set.
                    Sb.Length = 0
                    'Reg_Date = Convert.ToString(RegDate(1) + "/" + RegDate(0) + "/" + RegDate(2))
                    Reg_Date = Sb.Append(RegDate(1)).Append("/").Append(RegDate(0)).Append("/").Append(RegDate(2)).ToString
                    dt = Reg_Date.Split(" ")
                    txtDate.Text = Convert.ToString(dt(0))
                    hf_job_card_iss.Value = UCase(Trim(DT1.Rows(0)("job_card_iss").ToString()))
                    txtHead.Text = DT1.Rows(0)("Head_of_Household").ToString()
                    txtHouseNo.Text = DT1.Rows(0)("House_No").ToString()
                    txtFatHus.Text = DT1.Rows(0)("Father_or_Husband_Name").ToString()
                    txtEpicNo.Text = DT1.Rows(0)("Epic_No").ToString()

                    If DT1.Rows(0)("Caste").ToString() = "" Then
                        rdCategory.SelectedValue = "OTH"
                    Else
                        Cat = DT1.Rows(0)("Caste").ToString.Split(";")
                        For Each item As String In Cat
                            If item.Contains("SC") Then
                                rdCategory.SelectedValue = "SC"
                            ElseIf item.Contains("ST") Then
                                rdCategory.SelectedValue = "ST"
                            ElseIf item.Contains("IAY") Then
                                chkCategory.SelectedValue = "IAY"
                            ElseIf item.Contains("LR") Then
                                chkCategory.SelectedValue = "LR"
                            Else
                                rdCategory.SelectedValue = "OTH"
                            End If
                        Next

                    End If
                    RegDemanded = ""
                    RegInMustroll = ""

                    'Dim arr As Array
                    'arr = New String() {"2021", "1920", "1819", "1718", "1617", "1516", "1415", "1314", "1213", "1112", "1011", "0910", "0809", "0708", "0607", "0506"}
                    'For Each j As String In arr
                    '    tblMustroll = j
                    '    Sb.Length = 0
                    '    tblMustrollYYYY = Sb.Append(Lgn.Short_Name).Append(Right(District_Code, 2)).Append("MUSTROLL").Append(tblMustroll).ToString
                    shortnm = Lgn.Short_Name
                    For J = 0 To DT_FY.Rows.Count - 1
                        Fin_Year = Right(Left(DT_FY.Rows(J)("Financial_Year"), 4), 2) & Right(Right(DT_FY.Rows(J)("Financial_Year"), 4), 2)
                        If Session("Is_archived") = "Y" And (Not Session("Archived_finyear_upto") Is Nothing And Session("Archived_finyear_upto") <> "") Then
                            If Convert.ToInt32(Fin_Year) > Convert.ToInt32(Session("Archived_finyear_upto")) Then
                                tblMustrollYYYY = shortnm & Right(District_Code, 2) & "MUSTROLL" & Fin_Year
                            Else
                                tblMustrollYYYY = Session("Archived_schema") & shortnm & Right(District_Code, 2) & "MUSTROLL" & Fin_Year
                            End If
                        Else
                            tblMustrollYYYY = shortnm & Right(District_Code, 2) & "MUSTROLL" & Fin_Year
                        End If

                        'Sb.Length = 0
                        'dbSelectCommand = Sb.Append("Execute TableExist @TN='").Append(tblMustrollYYYY).Append("'").ToString
                        'DT1 = dal.ExecuteCommand_dt(New NpgsqlCommand(dbSelectCommand))

                        cmd1.CommandText = "TableExist"
                        cmd1.CommandType = CommandType.StoredProcedure
                        cmd1.Parameters.Clear()
                        cmd1.Parameters.AddWithValue("par_tn", tblMustrollYYYY)
                        DT1 = dal.ExecuteCommand_dt(cmd1, "p_refcur")

                        If DT1.Rows.Count > 0 Then
                            'Sb.Length = 0
                            'dbSelectCommand = Sb.Append("Execute chkRegInMustroll @tblMustrollYYYY= '").Append(tblMustrollYYYY).Append("', @Reg_No = '").Append(ddlReg.SelectedValue).Append("'").ToString
                            'DT1 = dal.ExecuteCommand_dt(New NpgsqlCommand(dbSelectCommand))

                            cmd1.CommandText = "chkRegInMustroll"
                            cmd1.CommandType = CommandType.StoredProcedure
                            cmd1.Parameters.Clear()
                            cmd1.Parameters.AddWithValue("@tblMustrollYYYY", tblMustrollYYYY)
                            cmd1.Parameters.AddWithValue("@Reg_No", ddlReg.SelectedValue)
                            DT1 = dal.ExecuteCommand_dt(cmd1)

                            If DT1.Rows.Count > 0 Then
                                RegInMustroll = "Y"
                                RegDemanded = "Y"
                                cmd = New NpgsqlCommand("check_for_entry_1")
                                cmd.CommandType = CommandType.StoredProcedure
                                cmd.CommandTimeout = 0
                                cmd.Parameters.Clear()
                                cmd.Parameters.Add("@Tablename", SqlDbType.VarChar, 100).Value = tblMustrollYYYY
                                cmd.Parameters.Add("@field_name", SqlDbType.VarChar, 500).Value = " Max(Isnull(Dt_to,'')) as MaxDt_To"
                                Sb.Length = 0
                                cmd.Parameters.Add("@Condition", SqlDbType.VarChar, 1000).Value = Sb.Append(" Reg_No='").Append(ddlReg.SelectedValue).Append("'").ToString
                                DT1 = dal.ExecuteCommand_dt(cmd)
                                If DT1.Rows.Count > 0 Then
                                    If DT1.Rows(0)("MaxDt_To").ToString() <> "" Or Year(DT1.Rows(0)("MaxDt_To").ToString()) <> "1900" Then
                                        MaxDt_To = DT1.Rows(0)("MaxDt_To").ToString()
                                        MaxDate = MaxDt_To.Split("/")
                                        Sb.Length = 0
                                        'MaxDt_To = Convert.ToString(MaxDate(1) + "/" + MaxDate(0) + "/" + MaxDate(2))
                                        MaxDt_To = Sb.Append(MaxDate(1)).Append("/").Append(MaxDate(0)).Append("/").Append(MaxDate(2)).ToString
                                        dt = MaxDt_To.Split(" ")
                                        MaxDt_To = Convert.ToString(dt(0))
                                        Sb.Length = 0
                                        'MaxDt_To_MDY = Convert.ToString(MaxDate(0) + "/" + MaxDate(1) + "/" + MaxDate(2))
                                        MaxDt_To_MDY = Sb.Append(MaxDate(0)).Append("/").Append(MaxDate(1)).Append("/").Append(MaxDate(2)).ToString
                                        dt = MaxDt_To_MDY.Split(" ")
                                        MaxDt_To_MDY = Convert.ToString(dt(0))
                                        hf_MaxDt_To_MDY.Value = MaxDt_To_MDY
                                        hf_MaxDt_To.Value = MaxDt_To
                                    Else
                                        MaxDt_To_MDY = ""
                                        MaxDt_To = ""
                                    End If

                                End If
                                Exit For
                            End If
                        End If
                    Next
                    If RegInMustroll <> "Y" Then
                        'Dim ar As Array
                        'ar = New String() {"2021", "1920", "1819", "1718", "1617", "1516", "1415", "1314", "1213", "1112", "1011", "0910", "0809", "0708", "0607", "0506"}
                        'For Each j As String In arr
                        '    tblWork_Demand = j
                        '    Sb.Length = 0
                        '    tblWork_DemandYYYY = Sb.Append(Lgn.Short_Name).Append(Right(District_Code, 2)).Append("WORK_DEMAND").Append(tblWork_Demand).ToString

                        'Change for financial year dynamic
                        shortnm = Lgn.Short_Name
                        For J = 0 To DT_FY.Rows.Count - 1
                            Fin_Year = Right(Left(DT_FY.Rows(J)("Financial_Year"), 4), 2) & Right(Right(DT_FY.Rows(J)("Financial_Year"), 4), 2)
                            Sb.Length = 0
                            If Session("Is_archived") = "Y" And (Not Session("Archived_finyear_upto") Is Nothing And Session("Archived_finyear_upto") <> "") Then
                                If Convert.ToInt32(Fin_Year) > Convert.ToInt32(Session("Archived_finyear_upto")) Then
                                    tblWork_DemandYYYY = Sb.Append(shortnm).Append(Right(District_Code, 2)).Append("WORK_DEMAND").Append(Fin_Year).ToString
                                Else
                                    tblWork_DemandYYYY = Sb.Append(Session("Archived_schema")).Append(shortnm).Append(Right(District_Code, 2)).Append("WORK_DEMAND").Append(Fin_Year).ToString
                                End If
                            Else
                                tblWork_DemandYYYY = Sb.Append(shortnm).Append(Right(District_Code, 2)).Append("WORK_DEMAND").Append(Fin_Year).ToString
                            End If
                            'Sb.Length = 0
                            'dbSelectCommand = Sb.Append("Execute TableExist @TN='").Append(tblWork_DemandYYYY).Append("'").ToString
                            'DT1 = dal.ExecuteCommand_dt(New NpgsqlCommand(dbSelectCommand))

                            cmd1.CommandText = "TableExist"
                            cmd1.CommandType = CommandType.StoredProcedure
                            cmd1.Parameters.Clear()
                            cmd1.Parameters.AddWithValue("par_tn", tblWork_DemandYYYY)
                            DT1 = dal.ExecuteCommand_dt(cmd1, "p_refcur")

                            If DT1.Rows.Count > 0 Then
                                'Sb.Length = 0
                                'dbSelectCommand = Sb.Append("Execute chkRegDemanded @tblWork_DemandYYYY = '").Append(tblWork_DemandYYYY).Append("', @Reg_No = '").Append(ddlReg.SelectedValue).Append("'").ToString
                                'DT1 = dal.ExecuteCommand_dt(New NpgsqlCommand(dbSelectCommand))

                                cmd1.CommandText = "chkRegDemanded"
                                cmd1.CommandType = CommandType.StoredProcedure
                                cmd1.Parameters.Clear()
                                cmd1.Parameters.AddWithValue("@tblWork_DemandYYYY", tblWork_DemandYYYY)
                                cmd1.Parameters.AddWithValue("@Reg_No", ddlReg.SelectedValue)
                                DT1 = dal.ExecuteCommand_dt(cmd1)

                                If DT1.Rows.Count > 0 Then
                                    RegDemanded = "Y"
                                    Exit For
                                End If
                            End If
                        Next
                    End If
                    hf_RegDemanded.Value = RegDemanded
                    hf_RegInMustroll.Value = RegInMustroll
                    del_date = Now
                    deldt = del_date.Split("/")
                    Sb.Length = 0
                    del_date = Sb.Append(deldt(1)).Append("/").Append(deldt(0)).Append("/").Append(deldt(2)).ToString

                    dt2 = del_date.Split(" ")
                    del_date = Convert.ToString(dt2(0))
                    hf_del_Date.Value = del_date

                    'Sb.Length = 0
                    'dbSelectCommand = Sb.Append("Execute getRegNoApplicants @tblApp = '").Append(Applicants).Append("', @Reg_No = '").Append(ddlReg.SelectedValue & "'").ToString
                    'DT1 = dal.ExecuteCommand_dt(New NpgsqlCommand(dbSelectCommand))

                    cmd1.CommandText = "getRegNoApplicants"
                    cmd1.CommandType = CommandType.StoredProcedure
                    cmd1.Parameters.Clear()
                    cmd1.Parameters.AddWithValue("par_tblapp", Applicants)
                    cmd1.Parameters.AddWithValue("par_reg_no", ddlReg.SelectedValue)
                    DT1 = dal.ExecuteCommand_dt(cmd1, "ref_cursor")


                    Dim note1 As String = ""
                    If DT1.Rows.Count > 0 Then

                        no_of_app = DT1.Rows.Count

                        lblNote.Visible = True
                        grdData.Visible = True
                        grdData.DataSource = DT1
                        grdData.DataBind()

                        RegDemanded = ""
                        For i = 0 To DT1.Rows.Count - 1
                            member_exists = "Y"
                            If DT1.Rows(i)("Gender").ToString() = "M" Then
                                CType(grdData.Rows(i).FindControl("rdGender"), RadioButtonList).SelectedValue = "M"
                            ElseIf DT1.Rows(i)("Gender").ToString() = "F" Then
                                CType(grdData.Rows(i).FindControl("rdGender"), RadioButtonList).SelectedValue = "F"
                            ElseIf DT1.Rows(i)("Gender").ToString() = "T" Then
                                CType(grdData.Rows(i).FindControl("rdGender"), RadioButtonList).SelectedValue = "T"
                            End If

                            If DT1.Rows(i)("Disabled").ToString() = "Y" Then
                                CType(grdData.Rows(i).FindControl("rdbDisabled"), RadioButtonList).SelectedValue = "Y"
                            ElseIf DT1.Rows(i)("Disabled").ToString() = "N" Then
                                CType(grdData.Rows(i).FindControl("rdbDisabled"), RadioButtonList).SelectedValue = "N"
                            End If
                            CType(grdData.Rows(i).FindControl("lblAge"), Label).Text = DT1.Rows(i)("Age").ToString()
                        Next
                        ddlDelReason.Items.Clear()
                        ddlDelReason.Items.Add(New ListItem("Select Reason for Deletion", "00"))
                        'ddlDelReason.Items.Add(New ListItem("Family had been shifted", "Family had been shifted"))
                        'ddlDelReason.Items.Add(New ListItem("Non-existent in Panchayat", "Non-existent in Panchayat"))
                        ddlDelReason.Items.Add(New ListItem("Migrated/Non-existent in Panchayat", "Non-existent in Panchayat"))
                        If member_exists <> "Y" Then
                            ddlDelReason.Items.Add(New ListItem("No member in the family", "No member in the family"))
                        End If
                        'ddlDelReason.Items.Add(New ListItem("Wants to surrender the Job-Card", "Wants to surrender the Job-Card"))
                        ddlDelReason.Items.Add(New ListItem("Duplicate Job Card", "Duplicate Job Card"))
                        ddlDelReason.Items.Add(New ListItem("Fake Job Card", "Fake Job Card"))
                        ddlDelReason.Items.Add(New ListItem("Incorrect Job Card", "Incorrect Job Card"))
                        ddlDelReason.Items.Add(New ListItem("Not willing to work", "Not willing to work"))
                        ddlDelReason.Items.Add(New ListItem("Village becomes urban", "Village becomes urban"))
                        'option removed for others on 17/08/2017
                        'ddlDelReason.Items.Add(New ListItem("Others", "Others"))

                        'Added on 20/03/2023 to delete Job card in case single worker in the family expired (implemented first for the WB First)
                        If (no_of_app = 1 And Session("State_Code_d") = "32") Then
                            ddlDelReason.Items.Add(New ListItem("Single person expired in the family", "Person Expired"))
                        End If

                        ddlDelReason.SelectedValue = "00"
                        If (ddlReg.SelectedValue <> "" And ddlReg.SelectedValue <> "00") Then
                            Sb.Length = 0
                            lblNote.Text = Sb.Append("Warning: Clicking of 'DELETE' button will delete the Registration No.: '").Append(ddlReg.SelectedValue).Append("' and its family details !<br>").Append(note1).ToString
                            lblNote.Visible = True
                        End If
                    Else
                        grdData.Visible = False
                        ddlDelReason.Items.Clear()
                        ddlDelReason.Items.Add(New ListItem("Select Reason for Deletion", "00"))
                        'ddlDelReason.Items.Add(New ListItem("Family had been shifted", "Family had been shifted"))
                        'ddlDelReason.Items.Add(New ListItem("Non-existent in Panchayat", "Non-existent in Panchayat"))
                        ddlDelReason.Items.Add(New ListItem("Migrated/Non-existent in Panchayat", "Non-existent in Panchayat"))
                        If member_exists <> "Y" Then
                            ddlDelReason.Items.Add(New ListItem("No member in the family", "No member in the family"))
                        End If
                        'ddlDelReason.Items.Add(New ListItem("Wants to surrender the Job-Card", "Wants to surrender the Job-Card"))
                        ddlDelReason.Items.Add(New ListItem("Duplicate Job Card", "Duplicate Job Card"))
                        ddlDelReason.Items.Add(New ListItem("Incorrect Job Card", "Incorrect Job Card"))
                        ddlDelReason.Items.Add(New ListItem("Not willing to work", "Not willing to work"))
                        ddlDelReason.Items.Add(New ListItem("Village becomes urban", "Village becomes urban"))
                        'option removed for others on 17/08/2017
                        'ddlDelReason.Items.Add(New ListItem("Others", "Others"))

                        ddlDelReason.SelectedValue = "00"
                        If (ddlReg.SelectedValue <> "" And ddlReg.SelectedValue <> "00") Then
                            Sb.Length = 0
                            lblNote.Text = Sb.Append("Warning: Clicking of 'DELETE' button will be marked as deleted for the Registration No.: '").Append(ddlReg.SelectedValue).Append("' and its family details !<br>").Append(note1).ToString
                            lblNote.Visible = True
                        End If
                    End If
                Else
                    lblNote.Visible = False
                    grdData.Visible = False
                    BtnSubmit.Visible = True
                    BtnCancel.Visible = True
                    lblmsg.Visible = True
                    lblmsg.Text = "No Data Found"
                    Return
                End If
            End If
            lblNote.ForeColor = Drawing.Color.Blue

        Catch ex2 As NullReferenceException
            lblmsg.Visible = True
            lblmsg.Text = "Null-Error found in Loading the Data"
            myutil.Exception_log(ex2)
        Catch ex1 As SqlException
            lblmsg.Visible = True
            lblmsg.Text = "DB-Error found in Loading the Data"
            myutil.Exception_log(ex1)
        Catch ex As Exception
            lblmsg.Visible = True
            lblmsg.Text = "Error found in Loading the Data"
            myutil.Exception_log(ex)
        End Try
    End Sub

    Protected Sub BtnSubmit_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles BtnSubmit.Click
        Try
            Sb = New StringBuilder()
            cmd1 = New NpgsqlCommand

            DT_FY = ViewState("Financial_Year")
            If DT_FY.Rows.Count <= 0 Then
                lblmsg.Text = "FINANCIAL YEAR DATA NOT FOUND."
                lblmsg.Visible = True
                Exit Sub
            End If

            Try
                If txtEffect.Text <> "" Then
                    Event_Date_DMY = txtEffect.Text
                    If IsDate(FormatDateMMDDYYYY("DATE", txtEffect.Text)) Then
                        Event_Date_MDY = FormatDateMMDDYYYY("Deletion w.e.f.(DD/MM/YYYY)", txtEffect.Text)
                        If (DateDiff("d", DateTime.Today.ToShortDateString(), Event_Date_MDY)) > 0 Then
                            lblmsg.Visible = True
                            Sb.Length = 0
                            lblmsg.Text = Sb.Append("Date of Deletion ").Append(Event_Date_DMY).Append(" Cannot be Future date !").ToString
                            Return
                        End If
                    Else
                        lblmsg.Visible = True
                        lblmsg.Text = "Please Enter Valid Datetime Format in Deletion Date"
                        Return
                    End If
                Else
                    txtEffect.Text = ""
                End If
            Catch ex As Exception
                lblmsg.Visible = True
                lblmsg.Text = "Please Enter Valid Datetime Format in Deletion Date"
                Return
            End Try
            RegInMustroll = hf_RegInMustroll.Value
            RegDemanded = hf_RegDemanded.Value
            MaxDt_To_MDY = hf_MaxDt_To_MDY.Value
            MaxDt_To = hf_MaxDt_To.Value
            If DateDiff("d", FormatDateMMDDYYYY("date", txtDate.Text), Event_Date_MDY) >= 0 Then
            Else
                lblmsg.Text = "Date of Deletion should be more than Registration Date"
                lblmsg.Visible = True
                Return
            End If
            If RegInMustroll = "Y" Then
                DiffDays = DateDiff("D", MaxDt_To_MDY, Event_Date_MDY)
                If DiffDays <= 0 Then
                    Sb.Length = 0
                    lblmsg.Text = Sb.Append("CANNOT MARK as DELETED for the selected Reg. No. '").Append(ddlReg.SelectedValue).Append("' <br>as applicant(s) of this household worked till '").Append(MaxDt_To).Append("' !<br>(Date of Deletion w.e.f. should be greater than the last-worked-date)<br>You can delete this Registration after the last-worked-date ").Append(MaxDt_To).Append(" !").ToString
                    lblmsg.Visible = True
                    Return
                End If
            End If

            If ddlDelReason.SelectedValue = "Wrong Entry" Then
                'w.e.f  8/mar/16

                'If RegInMustroll = "" And RegDemanded = "" Then
                '    'dbSelectCommand = "insert into delRegHistory (Reg_No, State_Code, District_Code, Block_Code, Panchayat_Code, Village_Code, Family_Id, Head_Of_Household, Father_Or_Husband_Name, House_No, Caste, Family_Photo_Path, Photo_File_Name, Registration_Date, Entry_Date, Work_days_alloted0506, Work_days_alloted0607, Work_days_alloted0708, Work_days_alloted0809, Work_days_alloted0910, Job_Card_Iss, Dt_Job_Card_Iss, Job_Slip_Iss, "
                '    'dbSelectCommand = dbSelectCommand & " Remark, Epic_No, Dt_Appln, Bpl_Data, Entry_By, Event_Flag, Event_Reason, Event_Date, BPL_Family, BPL_Family_No, delDateTime)"
                '    'dbSelectCommand = dbSelectCommand & " select Reg_No, State_Code, District_Code, Block_Code, Panchayat_Code, Village_Code, Family_Id, Head_Of_Household, Father_Or_Husband_Name, House_No, Caste, Family_Photo_Path, Photo_File_Name, Registration_Date, Entry_Date, Work_days_alloted0506, Work_days_alloted0607, Work_days_alloted0708, Work_days_alloted0809, Work_days_alloted0910, Job_Card_Iss, Dt_Job_Card_Iss, Job_Slip_Iss, "
                '    'dbSelectCommand = dbSelectCommand & " Remark, Epic_No, Dt_Appln, Bpl_Data, N'" & Entry_by & "', Event_Flag, Event_Reason, Event_Date, BPL_Family, BPL_Family_No, GetDate() from " & reg & " where Reg_No ='" & ddlReg.SelectedValue & "'"
                '    'dal.ExecuteCommand_rowsaffected(dbSelectCommand)


                '    'dbSelectCommand = "insert into delAppHistory (Reg_No, Applicant_No, Applicant_Name, Gender, Age, Skilled, AC_No, Photo_Path, Photo_File, Disabled, Relation_Code, AppEpicNo, BnkName, Branch_Code, BrnchName, PostOff_Code, PostOff_Name, PostOff_Address, AC_Flag, Entry_Date, Entry_By, Event_Flag, Event_Reason, Event_Date, Mode_Pay, Dt_ModePay_Change, delDateTime)"
                '    'dbSelectCommand = dbSelectCommand & " select Reg_No, Applicant_No, Applicant_Name, Gender, Age, Skilled, AC_No, Photo_Path, Photo_File, Disabled, Relation_Code, AppEpicNo, BnkName, Branch_Code, BrnchName, PostOff_Code, PostOff_Name, PostOff_Address, AC_Flag, Entry_Date, N'" & Entry_by & "', Event_Flag, Event_Reason, Event_Date, Mode_Pay, Dt_ModePay_Change, GetDate() from " & Applicants & " where Reg_No ='" & ddlReg.SelectedValue & "'"
                '    'dal.ExecuteCommand_rowsaffected(dbSelectCommand)

                '    'dbSelectCommand = "Delete from " & Applicants & " where Reg_No ='" & ddlReg.SelectedValue & "'"
                '    'dal.ExecuteCommand_rowsaffected(dbSelectCommand)

                '    'dbSelectCommand = "Delete from " & reg & " where Reg_No ='" & ddlReg.SelectedValue & "'"
                '    'dal.ExecuteCommand_rowsaffected(dbSelectCommand)

                'Else

                '    If RegInMustroll = "Y" Then
                '        lblmsg.Text = "CANNOT DELETE the selected Reg. No. '" & ddlReg.SelectedValue & "' permanently <br> as applicant(s) of this household already in Mustroll !"
                '        lblmsg.Visible = True
                '        Return
                '    ElseIf RegDemanded = "Y" Then
                '        lblmsg.Text = "CANNOT DELETE the selected Reg. No. '" & ddlReg.SelectedValue & "' permanently <br> as applicant(s) of this household demanded for job ! <br> Still if you want to delete, delete the allocation/demand entries first !"
                '        lblmsg.Visible = True
                '        Return
                '    Else
                '        lblmsg.Text = "CANNOT DELETE the selected Reg. No. '" & ddlReg.SelectedValue & "' permanently ! as transaction records Exist ! "
                '        lblmsg.Visible = True
                '        Return
                '        Response.Write("CANNOT DELETE the selected Reg. No. '" & ddlReg.SelectedValue & "' permanently ! as transaction records Exist ! ")
                '    End If
                'End If
            Else
                If RegInMustroll = "Y" Then
                    'Dim ar As Array
                    Dim upflde As String = String.Empty
                    Dim scondi As String = String.Empty
                    'ar = New String() {"2021", "1920", "1819", "1718", "1617", "1516", "1415", "1314", "1213", "1112", "1011", "0910", "0809", "0708", "0607", "0506"}
                    'Dim arr As Array
                    'arr = New String() {"2021", "1920", "1819", "1718", "1617", "1516", "1415", "1314", "1213", "1112", "1011", "0910", "0809", "0708", "0607", "0506"}
                    'For Each j As String In arr

                    '    tblWork_Allotted = j
                    '    tblWork_Demand = j
                    '    Sb.Length = 0
                    '    tblWork_AllottedYYYY = Sb.Append(Lgn.Short_Name).Append(Right(District_Code, 2)).Append("WORK_ALLOTTED").Append(tblWork_Allotted).ToString
                    '    Sb.Length = 0
                    '    tblWork_DemandYYYY = Sb.Append(Lgn.Short_Name).Append(Right(District_Code, 2)).Append("WORK_DEMAND").Append(tblWork_Demand).ToString

                    'Change for financial year dynamic
                    shortnm = Lgn.Short_Name
                    For J = 0 To DT_FY.Rows.Count - 1
                        Fin_Year = Right(Left(DT_FY.Rows(J)("Financial_Year"), 4), 2) & Right(Right(DT_FY.Rows(J)("Financial_Year"), 4), 2)
                        If Session("Is_archived") = "Y" And (Not Session("Archived_finyear_upto") Is Nothing And Session("Archived_finyear_upto") <> "") Then
                            If Convert.ToInt32(Fin_Year) > Convert.ToInt32(Session("Archived_finyear_upto")) Then
                                Sb.Length = 0
                                tblWork_AllottedYYYY = Sb.Append(shortnm).Append(Right(District_Code, 2)).Append("WORK_ALLOTTED").Append(Fin_Year).ToString
                                Sb.Length = 0
                                tblWork_DemandYYYY = Sb.Append(shortnm).Append(Right(District_Code, 2)).Append("WORK_DEMAND").Append(Fin_Year).ToString

                            Else
                                Sb.Length = 0
                                tblWork_AllottedYYYY = Sb.Append(Session("Archived_schema")).Append(shortnm).Append(Right(District_Code, 2)).Append("WORK_ALLOTTED").Append(Fin_Year).ToString
                                Sb.Length = 0
                                tblWork_DemandYYYY = Sb.Append(Session("Archived_schema")).Append(shortnm).Append(Right(District_Code, 2)).Append("WORK_DEMAND").Append(Fin_Year).ToString
                            End If
                        Else
                            Sb.Length = 0
                            tblWork_AllottedYYYY = Sb.Append(shortnm).Append(Right(District_Code, 2)).Append("WORK_ALLOTTED").Append(Fin_Year).ToString
                            Sb.Length = 0
                            tblWork_DemandYYYY = Sb.Append(shortnm).Append(Right(District_Code, 2)).Append("WORK_DEMAND").Append(Fin_Year).ToString
                        End If
                        'Sb.Length = 0
                        'dbSelectCommand = Sb.Append("Execute TableExist @TN='").Append(tblWork_AllottedYYYY).Append("'").ToString
                        'DT1 = dal.ExecuteCommand_dt(New NpgsqlCommand(dbSelectCommand))

                        cmd1.CommandText = "TableExist"
                        cmd1.CommandType = CommandType.StoredProcedure
                        cmd1.Parameters.Clear()
                        cmd1.Parameters.AddWithValue("par_tn", tblWork_AllottedYYYY)
                        DT1 = dal.ExecuteCommand_dt(cmd1, "p_refcur")

                        If DT1.Rows.Count > 0 Then

                            cmd = New NpgsqlCommand("check_for_entry_1")
                            cmd.CommandType = CommandType.StoredProcedure
                            cmd.CommandTimeout = 0
                            cmd.Parameters.Clear()
                            cmd.Parameters.Add("@Tablename", SqlDbType.VarChar, 100).Value = tblWork_DemandYYYY
                            cmd.Parameters.Add("@field_name", SqlDbType.VarChar, 500).Value = " distinct demand_id"
                            Sb.Length = 0
                            cmd.Parameters.Add("@Condition", SqlDbType.VarChar, 1000).Value = Sb.Append(" reg_no = '").Append(ddlReg.SelectedValue).Append("' and ('").Append(MaxDt_To_MDY).Append("' between work_demand_from and work_demand_to)").ToString
                            DT1 = dal.ExecuteCommand_dt(cmd)

                            If DT1.Rows.Count <= 0 Then

                                'Sb.Length = 0
                                'Sb.Append("Delete from ").Append(tblWork_AllottedYYYY).Append(" where reg_no =@reg_no and demand_id in ")
                                'Sb.Append(" (select demand_id from ").Append(tblWork_DemandYYYY)
                                'Sb.Append(" where reg_no =@reg_no and work_demand_to >=@work_demand_to)")

                                'mycmd = New NpgsqlCommand()
                                'mycmd.CommandText = Sb.ToString
                                'mycmd.Parameters.Add("@reg_no", SqlDbType.VarChar).Value = ddlReg.SelectedValue
                                'mycmd.Parameters.Add("@work_demand_to", SqlDbType.DateTime).Value = Event_Date_MDY
                                'dal.ExecuteCommand_rowsaffected(mycmd)
                                'mycmd.Parameters.Clear()


                                mycmd = New NpgsqlCommand()
                                mycmd.CommandText = "SpCommon_DelReg"
                                mycmd.CommandType = CommandType.StoredProcedure
                                mycmd.Parameters.Add("@table_name1", SqlDbType.VarChar, 30).Value = tblWork_AllottedYYYY
                                mycmd.Parameters.Add("@table_name2", SqlDbType.VarChar, 30).Value = tblWork_DemandYYYY
                                mycmd.Parameters.Add("@reg_no", SqlDbType.VarChar).Value = ddlReg.SelectedValue
                                mycmd.Parameters.Add("@work_demand_to", SqlDbType.DateTime).Value = Event_Date_MDY
                                mycmd.Parameters.Add("@Sp_No", SqlDbType.Int).Value = 1
                                dal.ExecuteCommand_rowsaffected(mycmd)
                                mycmd.Parameters.Clear()


                                'Sb.Length = 0
                                'dbSelectCommand = Sb.Append("EXECUTE  DeleteForAll   @Tablename='").Append(tblWork_DemandYYYY).Append("' , @Condition='Reg_No =''").Append(ddlReg.SelectedValue).Append("'' and work_demand_to >= ''").Append(Event_Date_MDY).Append("'''").ToString
                                'dal.ExecuteCommand_rowsaffected(dbSelectCommand)

                                cmd1.CommandText = "DeleteForAll"
                                cmd1.CommandType = CommandType.StoredProcedure
                                cmd1.Parameters.Clear()
                                cmd1.Parameters.AddWithValue("@Tablename", tblWork_DemandYYYY)
                                Sb.Length = 0
                                cmd1.Parameters.AddWithValue("@Condition", Sb.Append("Reg_No ='").Append(ddlReg.SelectedValue).Append("' and work_demand_to >= '").Append(Event_Date_MDY).Append("'").ToString)
                                dal.ExecuteCommand_rowsaffected(cmd1)

                            Else
                                For i = 0 To DT1.Rows.Count - 1
                                    demand_id = DT1.Rows(i)("demand_id").ToString()

                                    cmd = New NpgsqlCommand("check_for_entry_1")
                                    cmd.CommandType = CommandType.StoredProcedure
                                    cmd.CommandTimeout = 0
                                    cmd.Parameters.Clear()
                                    cmd.Parameters.Add("@Tablename", SqlDbType.VarChar, 100).Value = tblWork_DemandYYYY
                                    cmd.Parameters.Add("@field_name", SqlDbType.VarChar, 500).Value = " demand_id"
                                    Sb.Length = 0
                                    cmd.Parameters.Add("@Condition", SqlDbType.VarChar, 1000).Value = Sb.Append(" reg_no = '").Append(ddlReg.SelectedValue).Append("' and demand_id='").Append(demand_id).Append("'  and demand_left_from > '").Append(MaxDt_To_MDY).Append("'").ToString
                                    DT3 = dal.ExecuteCommand_dt(cmd)

                                    If DT3.Rows.Count > 0 Then
                                        ' Delete from Work-Allocations

                                        Sb.Length = 0
                                        Sb.Append("Delete from ").Append(tblWork_AllottedYYYY).Append(" where reg_no =@reg_no and demand_id in ")
                                        Sb.Append(" (select demand_id from ").Append(tblWork_DemandYYYY)
                                        Sb.Append(" where reg_no =@reg_no and demand_id=@demand_id and demand_left_from > @demand_left_from)")

                                        mycmd = New NpgsqlCommand()
                                        mycmd.CommandText = Sb.ToString
                                        mycmd.Parameters.Add("@reg_no", SqlDbType.VarChar).Value = ddlReg.SelectedValue
                                        mycmd.Parameters.Add("@demand_id", SqlDbType.Int).Value = demand_id
                                        mycmd.Parameters.Add("@demand_left_from", SqlDbType.DateTime).Value = MaxDt_To_MDY
                                        dal.ExecuteCommand_rowsaffected(mycmd)
                                        mycmd.Parameters.Clear()


                                        mycmd = New NpgsqlCommand() ''
                                        mycmd.CommandText = "SpCommon_DelReg"
                                        mycmd.CommandType = CommandType.StoredProcedure
                                        mycmd.Parameters.Add("@table_name1", SqlDbType.VarChar, 30).Value = tblWork_AllottedYYYY
                                        mycmd.Parameters.Add("@table_name2", SqlDbType.VarChar, 30).Value = tblWork_DemandYYYY
                                        mycmd.Parameters.Add("@reg_no", SqlDbType.VarChar).Value = ddlReg.SelectedValue
                                        mycmd.Parameters.Add("@demand_left_from", SqlDbType.DateTime).Value = MaxDt_To_MDY
                                        mycmd.Parameters.Add("@Sp_No", SqlDbType.Int).Value = 2
                                        dal.ExecuteCommand_rowsaffected(mycmd)
                                        mycmd.Parameters.Clear()

                                        'Update Work-Allocations

                                        Sb.Length = 0
                                        upflde = Sb.Append(" work_allot_to = '").Append(MaxDt_To_MDY).Append("', work_demand_to = '").Append(MaxDt_To_MDY).Append("', Entry_Date='").Append(Now).Append("', Entry_By=N'").Append(Entry_by).Append("' ").ToString
                                        Sb.Length = 0
                                        scondi = Sb.Append(" reg_no = '").Append(ddlReg.SelectedValue).Append("' and  demand_id='").Append(demand_id).Append("'  and work_allot_to>'").Append(MaxDt_To_MDY).Append("' ").ToString
                                        mycmd = New NpgsqlCommand("Update_table")
                                        mycmd.CommandType = CommandType.StoredProcedure
                                        mycmd.CommandTimeout = 0
                                        mycmd.Parameters.Clear()
                                        mycmd.Parameters.Add("@table", SqlDbType.VarChar, 50).Value = tblWork_AllottedYYYY
                                        mycmd.Parameters.Add("@condition1", SqlDbType.NVarChar, 700).Value = upflde
                                        mycmd.Parameters.Add("@condition", SqlDbType.NVarChar, 700).Value = scondi
                                        dal.ExecuteCommand_rowsaffected(mycmd)
                                        mycmd.Parameters.Clear()

                                        'Delete Work-Demands

                                        mycmd = New NpgsqlCommand("DeleteForAll")
                                        mycmd.CommandType = CommandType.StoredProcedure
                                        mycmd.CommandTimeout = 0
                                        mycmd.Parameters.Clear()
                                        mycmd.Parameters.Add("@Tablename", SqlDbType.VarChar, 50).Value = tblWork_DemandYYYY
                                        Sb.Length = 0
                                        mycmd.Parameters.Add("@Condition", SqlDbType.NVarChar, 1000).Value = Sb.Append(" reg_no = '").Append(ddlReg.SelectedValue).Append("' and demand_id ='").Append(demand_id).Append("' and demand_left_from > '").Append(MaxDt_To_MDY).Append("'").ToString
                                        dal.ExecuteCommand_rowsaffected(mycmd)
                                        mycmd.Parameters.Clear()

                                        'Update Work-Demands

                                        Sb.Length = 0
                                        upflde = Sb.Append(" bal_days =0, work_demand_to = '").Append(MaxDt_To_MDY).Append("', demand_left_from=null , demand_left_to=null , Entry_Date='").Append(Now).Append("', Entry_By=N'").Append(Entry_by).Append("' ").ToString
                                        Sb.Length = 0
                                        scondi = Sb.Append(" reg_no = '").Append(ddlReg.SelectedValue).Append("' and  demand_id='").Append(demand_id).Append("'  and  work_demand_to>'").Append(MaxDt_To_MDY).Append("' ").ToString

                                        mycmd = New NpgsqlCommand("Update_table")
                                        mycmd.CommandType = CommandType.StoredProcedure
                                        mycmd.CommandTimeout = 0
                                        mycmd.Parameters.Clear()
                                        mycmd.Parameters.Add("@table", SqlDbType.VarChar, 50).Value = tblWork_DemandYYYY
                                        mycmd.Parameters.Add("@condition1", SqlDbType.NVarChar, 700).Value = upflde
                                        mycmd.Parameters.Add("@condition", SqlDbType.NVarChar, 700).Value = scondi
                                        dal.ExecuteCommand_rowsaffected(mycmd)
                                        mycmd.Parameters.Clear()

                                    End If
                                Next
                            End If
                        End If
                    Next
                ElseIf RegDemanded = "Y" Then
                    'Dim ar As Array
                    'ar = New String() {"2021", "1920", "1819", "1718", "1617", "1516", "1415", "1314", "1213", "1112", "1011", "0910", "0809", "0708", "0607", "0506"}
                    'Dim arr As Array
                    'arr = New String() {"2021", "1920", "1819", "1718", "1617", "1516", "1415", "1314", "1213", "1112", "1011", "0910", "0809", "0708", "0607", "0506"}
                    'For Each j As String In arr
                    '    tblWork_Allotted = j
                    '    tblWork_Demand = j

                    '    Sb.Length = 0
                    '    tblWork_AllottedYYYY = Sb.Append(Lgn.Short_Name).Append(Right(District_Code, 2)).Append("WORK_ALLOTTED").Append(tblWork_Allotted).ToString
                    '    Sb.Length = 0
                    '    tblWork_DemandYYYY = Sb.Append(Lgn.Short_Name).Append(Right(District_Code, 2)).Append("WORK_DEMAND").Append(tblWork_Demand).ToString
                    'Change for financial year dynamic
                    shortnm = Lgn.Short_Name
                    For J = 0 To DT_FY.Rows.Count - 1
                        Fin_Year = Right(Left(DT_FY.Rows(J)("Financial_Year"), 4), 2) & Right(Right(DT_FY.Rows(J)("Financial_Year"), 4), 2)
                        If Session("Is_archived") = "Y" And (Not Session("Archived_finyear_upto") Is Nothing And Session("Archived_finyear_upto") <> "") Then
                            If Convert.ToInt32(Fin_Year) > Convert.ToInt32(Session("Archived_finyear_upto")) Then
                                Sb.Length = 0
                                tblWork_AllottedYYYY = Sb.Append(shortnm).Append(Right(District_Code, 2)).Append("WORK_ALLOTTED").Append(Fin_Year).ToString
                                Sb.Length = 0
                                tblWork_DemandYYYY = Sb.Append(shortnm).Append(Right(District_Code, 2)).Append("WORK_DEMAND").Append(Fin_Year).ToString
                            Else
                                Sb.Length = 0
                                tblWork_AllottedYYYY = Sb.Append(Session("Archived_schema")).Append(shortnm).Append(Right(District_Code, 2)).Append("WORK_ALLOTTED").Append(Fin_Year).ToString
                                Sb.Length = 0
                                tblWork_DemandYYYY = Sb.Append(Session("Archived_schema")).Append(shortnm).Append(Right(District_Code, 2)).Append("WORK_DEMAND").Append(Fin_Year).ToString

                            End If
                        Else
                            Sb.Length = 0
                            tblWork_AllottedYYYY = Sb.Append(shortnm).Append(Right(District_Code, 2)).Append("WORK_ALLOTTED").Append(Fin_Year).ToString
                            Sb.Length = 0
                            tblWork_DemandYYYY = Sb.Append(shortnm).Append(Right(District_Code, 2)).Append("WORK_DEMAND").Append(Fin_Year).ToString
                        End If
                        'Sb.Length = 0
                        'dbSelectCommand = Sb.Append("Execute TableExist @TN='").Append(tblWork_AllottedYYYY).Append("'").ToString
                        'DT1 = dal.ExecuteCommand_dt(New NpgsqlCommand(dbSelectCommand))

                        cmd1.CommandText = "TableExist"
                        cmd1.CommandType = CommandType.StoredProcedure
                        cmd1.Parameters.Clear()
                        cmd1.Parameters.AddWithValue("par_tn", tblWork_AllottedYYYY)
                        DT1 = dal.ExecuteCommand_dt(cmd1, "p_refcur")



                        If DT1.Rows.Count > 0 Then
                            Sb.Length = 0
                            Sb.Append("Delete  from ").Append(tblWork_AllottedYYYY).Append(" where reg_no =@reg_no and demand_id in ")
                            Sb.Append(" (select demand_id from ").Append(tblWork_DemandYYYY)
                            Sb.Append(" where reg_no =@reg_no and  work_demand_to>=@work_demand_to)")

                            mycmd = New NpgsqlCommand()
                            mycmd.CommandText = Sb.ToString
                            mycmd.Parameters.Add("@reg_no", SqlDbType.VarChar).Value = ddlReg.SelectedValue
                            mycmd.Parameters.Add("@work_demand_to", SqlDbType.DateTime).Value = Event_Date_MDY
                            dal.ExecuteCommand_rowsaffected(mycmd)
                            mycmd.Parameters.Clear()

                            mycmd = New NpgsqlCommand("DeleteForAll")
                            mycmd.CommandType = CommandType.StoredProcedure
                            mycmd.CommandTimeout = 0
                            mycmd.Parameters.Clear()
                            mycmd.Parameters.Add("@Tablename", SqlDbType.VarChar, 50).Value = tblWork_DemandYYYY
                            Sb.Length = 0
                            mycmd.Parameters.Add("@Condition", SqlDbType.NVarChar, 1000).Value = Sb.Append(" Reg_No = '").Append(ddlReg.SelectedValue).Append("' and  work_demand_to >= '").Append(Event_Date_MDY).Append("' ").ToString
                            dal.ExecuteCommand_rowsaffected(mycmd)
                            mycmd.Parameters.Clear()

                        End If
                    Next
                End If

                'Sb.Length = 0
                'dbSelectCommand = Sb.Append("Execute DelRegSave @tblReg =").Append(reg).Append(", @tblApplicants='").Append(Applicants).Append("',@Event_Reason=N'").Append(ddlDelReason.SelectedValue).Append("', @Reg_No = '").Append(ddlReg.SelectedValue).Append("',@Event_Date='").Append(Event_Date_MDY).Append("',@Entry_By=N'").Append(Entry_by).Append("'").ToString
                'DT1 = dal.ExecuteCommand_dt(New NpgsqlCommand(dbSelectCommand))
                Try
                    dal.BeginTransaction()
                    Using cmd1 As New NpgsqlCommand
                        cmd1.CommandText = "SpHistory_Reg_Applicant"
                        cmd1.CommandType = CommandType.StoredProcedure
                        cmd1.Parameters.Clear()
                        cmd1.Parameters.Add("par_reg_no", NpgsqlDbType.Varchar, 34).Value = ddlReg.SelectedValue
                        cmd1.Parameters.Add("par_entry_by", NpgsqlDbType.Varchar, 50).Value = String.Format("{0}", Session("entry_by")).ToString
                        cmd1.Parameters.Add("par_event_log_activity", NpgsqlDbType.Varchar, 20).Value = "del_reg"
                        cmd1.Parameters.Add("par_reg_tbl", NpgsqlDbType.Varchar, 20).Value = reg
                        dal.ExecuteCommand_rowsaffected(cmd1)

                        cmd1.CommandText = "DelRegSave"
                        cmd1.CommandType = CommandType.StoredProcedure
                        cmd1.Parameters.Clear()
                        cmd1.Parameters.AddWithValue("par_tblreg", NpgsqlDbType.Varchar, reg)
                        cmd1.Parameters.AddWithValue("par_tblapplicants", NpgsqlDbType.Varchar, Applicants)
                        cmd1.Parameters.Add("par_event_reason", NpgsqlDbType.Varchar, 100).Value = ddlDelReason.SelectedValue
                        cmd1.Parameters.AddWithValue("par_reg_no", NpgsqlDbType.Varchar, ddlReg.SelectedValue)
                        cmd1.Parameters.AddWithValue("par_event_date", NpgsqlDbType.Varchar, Event_Date_MDY)
                        cmd1.Parameters.Add("par_entry_by", NpgsqlDbType.Varchar, 50).Value = Entry_by
                        'DT1 = dal.ExecuteCommand_dt(cmd1)
                        dal.ExecuteCommand_rowsaffected(cmd1)
                    End Using
                    dal.CommitTransaction()
                Catch sqlex As SqlException
                    dal.RollBackTransaction()
                    Throw sqlex
                Catch ex As Exception
                    dal.RollBackTransaction()
                    Throw ex
                Finally
                    dal.CommitTransaction()
                End Try
            End If
            If ddlDelReason.SelectedValue <> "Wrong Entry" Then
                Sb.Length = 0
                lblmsg.Text = Sb.Append("The Registration No.: ").Append(ddlReg.SelectedValue).Append(" has been marked as DELETED with effect from '").Append(Event_Date_DMY).Append("' !").ToString
                lblmsg.Visible = True
                ddlvillage_SelectedIndexChanged1(sender, e)
                Return
            End If
        Catch ex2 As NullReferenceException
            lblmsg.Visible = True
            lblmsg.Text = "Null-Error found while Updating Data"
            myutil.Exception_log(ex2)
        Catch ex1 As SqlException
            lblmsg.Visible = True
            lblmsg.Text = "DB-Error found while Updating Data"
            myutil.Exception_log(ex1)
        Catch ex As Exception
            lblmsg.Visible = True
            lblmsg.Text = "Error found while Updating Data"
            myutil.Exception_log(ex)
        End Try
    End Sub
    Protected Sub BtnCancel_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles BtnCancel.Click
        Response.Redirect(Request.Url.ToString())
    End Sub
    Function FormatDateMMDDYYYY(ByVal ColName As String, ByVal inputDate As String) As String
        Dim dateDelimeter, FormatDMMDDYYYY As String
        dateDelimeter = Nothing
        If (String.IsNullOrEmpty(inputDate)) Or (inputDate = "") Then
            FormatDMMDDYYYY = Nothing
        Else
            If InStr(inputDate, "/") > 1 Then
                dateDelimeter = "/"
            ElseIf InStr(inputDate, "-") > 1 Then
                dateDelimeter = "-"
            ElseIf InStr(inputDate, ".") > 1 Then
                dateDelimeter = "."
            End If
            Sb.Length = 0
            FormatDMMDDYYYY = Sb.Append(Mid(inputDate, InStr(inputDate, dateDelimeter) + 1, InStrRev(inputDate, dateDelimeter) - InStr(inputDate, dateDelimeter) - 1)).Append("/").Append(Left(inputDate, InStr(inputDate, dateDelimeter) - 1)).Append("/").Append(Mid(inputDate, InStrRev(inputDate, dateDelimeter) + 1)).ToString
        End If
        Return FormatDMMDDYYYY
    End Function
    Protected Sub panch_bind(ByVal Block_Code As String)
        Try
            'sb.Length = 0
            'dbSelectCommand = sb.Append("Execute Display_Panchayats @Block_Code = '").Append(Block_Code).Append("', @finyr='").Append(finyear).Append("'").ToString
            'DT1 = dal.ExecuteCommand_dt(New NpgsqlCommand(dbSelectCommand))

            Using cmd As New NpgsqlCommand
                cmd.CommandText = "Display_Panchayats"
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("par_block_code", NpgsqlDbType.Varchar, Block_Code)
                cmd.Parameters.AddWithValue("par_finyr", NpgsqlDbType.Varchar, finyear)

                DT1 = dal.ExecuteCommand_dt(cmd, "ref_cursor")

            End Using

            ddlpnch.DataSource = DT1
            ddlpnch.DataTextField = "Panch_Name_Local"
            ddlpnch.DataValueField = "Panchayat_Code"
            ddlpnch.DataBind()
            ddlpnch.Items.Insert(0, ViewState("SelectPanch"))
            If DT1.Rows.Count = 0 Then
                lblmsg.Visible = True
                lblmsg.Text = "No panchayat found for the selected block !"
                Return
            End If
        Catch ex1 As SqlException
            lblmsg.Visible = True
            lblmsg.Text = "DB-Error found in panchayat Binding..!"
        Catch ex As Exception
            lblmsg.Visible = True
            lblmsg.Text = "Error found in panchayat Binding..!"
        End Try
    End Sub
    Protected Sub block_bind(ByVal dist_code As String)
        Try
            'sb.Length = 0
            'dbSelectCommand = sb.Append("Execute Display_Blocks @District_Code = '").Append(dist_code).Append("', @finyr='").Append(finyear).Append("'").ToString
            'DT1 = dal.ExecuteCommand_dt(New NpgsqlCommand(dbSelectCommand))

            Using cmd As New NpgsqlCommand
                cmd.CommandText = "Display_Blocks"
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("@District_Code", dist_code)
                cmd.Parameters.AddWithValue("@finyr", finyear)
                DT1 = dal.ExecuteCommand_dt(cmd)
            End Using

            ddlBlock.DataSource = DT1
            ddlBlock.DataTextField = "Block_Name"
            ddlBlock.DataValueField = "Block_Code"
            ddlBlock.DataBind()
            ddlBlock.Items.Insert(0, "Select Block")
            ddlpnch.ClearSelection()
            If DT1.Rows.Count = 0 Then
                lblmsg.Visible = True
                lblmsg.Text = "No Block found for the selected District !"
                Return
            End If
        Catch ex1 As SqlException
            lblmsg.Visible = True
            lblmsg.Text = "DB-Error found in Block Binding..!"
        Catch ex As Exception
            lblmsg.Visible = True
            lblmsg.Text = "Error found in Block Binding..!"
        End Try
    End Sub
    Protected Sub ddlBlock_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlBlock.SelectedIndexChanged
        If ddlBlock.SelectedIndex >= 1 Then
            ddlpnch.Items.Clear()
            ddlReg.Items.Clear()
            ddlvillage.Items.Clear()
            tblDetails.Visible = False
            lblNote.Visible = False
            grdData.DataSource = DT1
            grdData.DataBind()
            BtnSubmit.Visible = False
            BtnCancel.Visible = False
            panch_bind(ddlBlock.SelectedValue)
        End If
    End Sub
End Class
