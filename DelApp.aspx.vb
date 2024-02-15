Imports System
Imports System.Web
Imports System.Data
Imports System.Data.SqlClient
Imports Npgsql
Partial Class DelApp
    Inherits System.Web.UI.Page
    Dim DT1, DT2, DT3, DT4, DT5, DT6, DT7 As DataTable
    Dim dal As DAL_VB
    Dim Lgn As Lang_Labels
    Dim arr() As String
    Dim mycmd, cmd, cmd1 As NpgsqlCommand

    Dim RegDate, Cat, dt, MaxDate, deldt, EventDate As Array
    Public HomePage As String
    Dim State_Code, AppReadOnly, MaxDt_To, tblWork_Demand, DeletionDateDMY, DeleteDateMMDDYYYY, deldt_To, DeletionDate, DiffDays As String
    Dim demand_id, tblWork_DemandYYYY, MaxDt_To_MDY, Entry_Date, tblWork_AllottedYYYY, CannotDelete, DelFlagClicked, tblMustrollYYYY, Event_Reason As String
    Dim DelFlagStatus, AppBgColor, tblMustroll, AppInMustroll, AppDemanded, Event_Date, SelectPanch, Event_Flag, Entry_by, Gender, ApplicantName, ApplicantNo As String
    Dim Age, Reg_Date, finyear, reg, Applicants, Panchayat_Name, Level, District_Code, Block_Code, Panchayat_Code, short_name, Fin_Year, shortnm As String
    Dim sb As StringBuilder
    Dim myutil As New Util
    Dim DT_FY As DataTable
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            If Session("State_Code_d") = "" Or Session("finyear_d") = "" Or Trim(Session("ExeL_d")) = "" Then
                Server.Transfer("logout.aspx")
            ElseIf Session("State_Code_d") = "32" And Trim(Session("ExeL_d")) <> "DPC" Then 'this option is disabled for WB.
                Response.Write("'Delete Worker/Applicant' option has been moved to DPC login. Pls, request your DPC/DNO to delete the worker.")
                Response.End()
            Else
                dal = DAL_VB.GetInstanceforDE()
            End If

            DT1 = New DataTable()
            lblmsg.Visible = False
            lbl.Text = ""
            State_Code = Session("State_Code_d")
            District_Code = Session("District_Code_d")
            Block_Code = Session("Block_Code_d")
            Panchayat_Code = Session("Panchayat_Code_d")
            finyear = Session("finyear_d")
            Entry_by = Session("Entry_By")
            short_name = Session("Short_Name")
            Entry_Date = Now
            If Session("ExeL_d") = "GP" Then
                Panchayat_Code = Session("Panchayat_Code_d")
                Panchayat_Name = Session("Panchayat_Name_d")
                HomePage = "IndexFrame2.aspx?Panchayat_Code=" & Panchayat_Code
                Level = "GP"
            ElseIf Session("ExeL_d") = "PO" Then
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
            sb = New StringBuilder
            reg = sb.Append(Session("Short_Name")).Append(Right(District_Code, 2)).Append("Registration").ToString
            sb.Length = 0
            Applicants = sb.Append(Session("Short_Name")).Append(Right(District_Code, 2)).Append("Applicants").ToString

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

                Lgn = New Lang_Labels(Session("state_code_d"), "3")
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

                    'sb.Length = 0
                    'dbSelectCommand = sb.Append("Execute Display_Villages @Panchayat_Code = '").Append(Panchayat_Code).Append("', @finyr='").Append(finyear).Append("'").ToString
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
                            sb.Length = 0
                            ddlvillage.Items.Add(New ListItem(sb.Append(Mid((drow("Village_Code")), 11, 3)).Append("-").Append(drow("Village_Name_Local")).ToString, drow("village_Code")))
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
                    If Session("state_code_d") = "32" Then
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
                '    Sb.Length = 0
                '    Sb.Append("select Financial_Year from fin_year_temp (nolock) order by YEAR_code desc")
                '    cmd.CommandText = Sb.ToString
                '    cmd.Parameters.Clear()
                '    DT1 = dal.ExecuteCommand_dt(cmd)
                '    If DT1.Rows.Count > 0 Then
                '        ViewState("Financial_Year") = DT1
                '    End If
                '    Sb.Length = 0
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
        Catch nullex1 As NullReferenceException
            lblmsg.Visible = True
            lblmsg.Text = "Null-Error found in loading Page."
        Catch ex1 As SqlException
            lblmsg.Visible = True
            lblmsg.Text = "DB-Error found in loading Page."
        Catch ex As Exception
            lblmsg.Visible = True
            lblmsg.Text = "Error found in loading Page."
        End Try
    End Sub
    Protected Sub ddlpnch_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlpnch.SelectedIndexChanged
        Try
            ddlReg.Items.Clear()
            ddlvillage.Items.Clear()
            Warning.Visible = False
            change.Visible = False
            note.Visible = False
            tblDetails.Visible = False
            lblNote.Visible = False
            grdData.DataSource = DT1
            grdData.DataBind()
            BtnDelete.Visible = False
            BtnCancel.Visible = False

            'sb = New StringBuilder
            'dbSelectCommand = sb.Append("Execute Display_Villages @Panchayat_Code = '").Append(ddlpnch.SelectedValue).Append("', @finyr='").Append(finyear).Append("'").ToString
            'DT1 = dal.ExecuteCommand_dt(New NpgsqlCommand(dbSelectCommand))

            Using cmd As New NpgsqlCommand
                cmd.CommandText = "Display_Villages"
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("par_panchayat_code", ddlpnch.SelectedValue)
                cmd.Parameters.AddWithValue("par_finyr", finyear)
                DT1 = dal.ExecuteCommand_dt(cmd, "ref_cursor")
            End Using

            If State_Code = "16" Then
                ddlvillage.Items.Insert(0, ViewState("SelectVill"))
                For Each drow As DataRow In DT1.Rows
                    sb.Length = 0
                    ddlvillage.Items.Add(New ListItem(sb.Append(Mid((drow("Village_Code")), 11, 3)).Append("-").Append(drow("Village_Name_Local")).ToString, drow("village_Code")))
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
                    lblmsg.Text = "No Panchayat found for the selected Block ! "
                    Return
                End If
            End If
        Catch ex1 As SqlException
            lblmsg.Visible = True
            lblmsg.Text = "DB-Error found in binding panchayat"
        Catch ex As Exception
            lblmsg.Visible = True
            lblmsg.Text = "Error found in binding panchayat"
        End Try
    End Sub
    Protected Sub ddlvillage_SelectedIndexChanged1(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlvillage.SelectedIndexChanged
        ddlReg.ClearSelection()
        ddlReg.Items.Clear()
        Warning.Visible = False
        change.Visible = False
        note.Visible = False
        tblDetails.Visible = False
        lblNote.Visible = False
        grdData.DataSource = DT1
        grdData.DataBind()
        BtnDelete.Visible = False
        BtnCancel.Visible = False
        Try
            If (ddlvillage.SelectedItem.Text <> ViewState("SelectVill")) Then
                'sb = New StringBuilder
                'dbSelectCommand = sb.Append("Execute cboRegNo_NotDel @tblReg = '").Append(reg).Append("', @Village_Code='").Append(ddlvillage.SelectedValue).Append("'").ToString
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
                    lblmsg.Text = "No Registration found for the selected Village ! ! "
                    Return
                End If
            End If
        Catch ex1 As SqlException
            lblmsg.Visible = True
            lblmsg.Text = "DB-Error found in Registration No."
        Catch ex As Exception
            lblmsg.Visible = True
            lblmsg.Text = "Error found in Registration No."
        End Try
    End Sub
    Protected Sub ddlReg_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlReg.SelectedIndexChanged
        Try
            Dim note1 As String = ""
            chkChange.Checked = False
            Dim Sconditions As String = String.Empty
            txtHead.ReadOnly = True
            txtFatHus.ReadOnly = True

            If ddlReg.SelectedItem.Text = ViewState("selectRegNo") Then
                grdData.DataSource = DT1
                grdData.DataBind()
                BtnDelete.Visible = False
                BtnCancel.Visible = False
                tblDetails.Visible = False
                lblNote.Visible = False
                Warning.Visible = False
                change.Visible = False
                note.Visible = False
                Return
            End If

            cmd1 = New NpgsqlCommand
            If (Level = "BP") Then
                Panchayat_Code = ddlpnch.SelectedValue
            End If
            DT_FY = ViewState("Financial_Year")
            If DT_FY.Rows.Count <= 0 Then
                lblMessage.Text = "FINANCIAL YEAR DATA NOT FOUND."
                lblMessage.Visible = True
                Exit Sub
            End If

            chkCategory.ClearSelection()
            sb = New StringBuilder
            If (ddlvillage.SelectedValue <> "" And ddlReg.SelectedValue <> "" And ddlReg.SelectedValue <> ViewState("selectRegNo")) Then
                tblDetails.Visible = True

                'dbSelectCommand = sb.Append("Execute getRegNoFamily @tblReg = '").Append(reg).Append("', @Reg_No = '").Append(ddlReg.SelectedValue).Append("'").ToString
                'DT1 = dal.ExecuteCommand_dt(New NpgsqlCommand(dbSelectCommand))

                Using cmd As New NpgsqlCommand
                    cmd.CommandText = "getRegNoFamily"
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("par_tblreg", reg)
                    cmd.Parameters.AddWithValue("par_reg_no", ddlReg.SelectedValue)
                    DT1 = dal.ExecuteCommand_dt(cmd, "ref_cursor")
                End Using

                If DT1.Rows.Count <= 0 Then
                    lblmsg.Visible = True
                    lblmsg.Text = "No Data Found"
                    Return
                End If
                Reg_Date = DT1.Rows(0)("Registration_Date").ToString()
                RegDate = Reg_Date.Split("/")
                sb.Length = 0
                'Reg_Date = Convert.ToString(RegDate(1) + "/" + RegDate(0) + "/" + RegDate(2))
                Reg_Date = sb.Append(RegDate(1)).Append("/").Append(RegDate(0)).Append("/").Append(RegDate(2)).ToString
                dt = Reg_Date.Split(" ")
                txtDate.Text = Convert.ToString(dt(0))
                txtHead.Text = DT1.Rows(0)("Head_of_Household").ToString()
                txtHouseNo.Text = DT1.Rows(0)("House_No").ToString()
                txtFatHus.Text = DT1.Rows(0)("Father_or_Husband_Name").ToString()
                txtEpicNo.Text = DT1.Rows(0)("Epic_No").ToString()
                JobCard.Value = DT1.Rows(0)("job_card_iss").ToString()
                JobCard.Value = JobCard.Value.ToUpper.ToString()
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
                sb.Length = 0
                Sconditions = sb.Append("Reg_No='").Append(ddlReg.SelectedValue).Append("'  Order by Applicant_No ").ToString
                'sb.Length = 0
                'dbSelectCommand = sb.Append("exec check_for_entry_1  @Tablename='").Append(Applicants).Append("' ,  @field_name='Applicant_No, Applicant_name, Gender, Age, Disabled, Isnull(Event_Flag,'''') as Event_Flag, Event_Date, Event_Reason', @Condition='").Append(Sconditions).Append("'  ").ToString
                'DT2 = dal.ExecuteCommand_dt(New NpgsqlCommand(dbSelectCommand))

                Using cmd As New NpgsqlCommand
                    cmd.CommandText = "check_for_entry_1"
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("par_tablename", Applicants)
                    cmd.Parameters.AddWithValue("par_field_name", "Applicant_No, Applicant_name, Gender, Age, Disabled, coalesce(Event_Flag,'') as Event_Flag, Event_Date, Event_Reason")
                    cmd.Parameters.AddWithValue("par_condition", Sconditions)
                    DT2 = dal.ExecuteCommand_dt(cmd, "ref_cursor")
                End Using

                If (DT2.Rows.Count > 0) Then
                    lblNote.Visible = True
                    grdData.Visible = True
                    grdData.DataSource = DT2
                    grdData.DataBind()
                    change.Visible = True
                    note.Visible = True
                    Warning.Visible = True
                    For i = 0 To grdData.Rows.Count - 1
                        hf_Countrow.Value = grdData.Rows.Count
                        ApplicantNo = DT2.Rows(i)("Applicant_No").ToString()
                        ApplicantName = DT2.Rows(i)("Applicant_name").ToString()
                        Age = DT2.Rows(i)("Age").ToString()
                        Gender = DT2.Rows(i)("Gender").ToString()
                        Event_Flag = DT2.Rows(i)("Event_Flag").ToString()
                        Event_Reason = DT2.Rows(i)("Event_Reason").ToString()
                        Event_Date = DT2.Rows(i)("Event_Date").ToString()
                        If Event_Date.Contains("1900") Then
                            Event_Date = ""
                        End If
                        If String.IsNullOrEmpty(Event_Date) Then

                        Else
                            EventDate = Event_Date.Split("/")
                            Event_Date = Convert.ToString(EventDate(1) + "/" + EventDate(0) + "/" + EventDate(2))
                            dt = Event_Date.Split(" ")
                            Event_Date = Convert.ToString(dt(0))
                        End If
                        AppBgColor = ""
                        AppInMustroll = ""
                        AppDemanded = ""
                        If AppInMustroll = "" Then
                            'Dim arr As Array
                            'arr = New String() {"2021", "1920", "1819", "1718", "1617", "1516", "1415", "1314", "1213", "1112", "1011", "0910", "0809", "0708", "0607", "0506"}
                            'For Each j As String In arr

                            '    tblMustroll = j
                            '    AppReadOnly = ""
                            '    sb.Length = 0
                            '    tblMustrollYYYY = sb.Append(short_name).Append(Right(District_Code, 2)).Append("MUSTROLL").Append(tblMustroll).ToString

                            ' Change for Fin Year Dynamic

                            For J = 0 To DT_FY.Rows.Count - 1
                                Fin_Year = Right(Left(DT_FY.Rows(J)("Financial_Year"), 4), 2) & Right(Right(DT_FY.Rows(J)("Financial_Year"), 4), 2)
                                AppReadOnly = ""
                                sb.Length = 0
                                If Session("Is_archived") = "Y" And (Not Session("Archived_finyear_upto") Is Nothing And Session("Archived_finyear_upto") <> "") Then
                                    If Convert.ToInt32(Fin_Year) > Convert.ToInt32(Session("Archived_finyear_upto")) Then
                                        tblMustrollYYYY = sb.Append(short_name).Append(Right(District_Code, 2)).Append("MUSTROLL").Append(Fin_Year).ToString
                                    Else
                                        tblMustrollYYYY = sb.Append(Session("Archived_schema")).Append(short_name).Append(Right(District_Code, 2)).Append("MUSTROLL").Append(Fin_Year).ToString
                                    End If
                                Else
                                    tblMustrollYYYY = sb.Append(short_name).Append(Right(District_Code, 2)).Append("MUSTROLL").Append(Fin_Year).ToString
                                End If
                                'sb.Length = 0
                                'dbSelectCommand = sb.Append("Execute TableExist @TN='").Append(tblMustrollYYYY).Append("'").ToString
                                'DT3 = dal.ExecuteCommand_dt(New NpgsqlCommand(dbSelectCommand))

                                'Using cmd As New NpgsqlCommand
                                cmd1.CommandText = "TableExist"
                                cmd1.CommandType = CommandType.StoredProcedure
                                cmd1.Parameters.Clear()
                                cmd1.Parameters.AddWithValue("par_tn", tblMustrollYYYY)
                                DT3 = dal.ExecuteCommand_dt(cmd1, "p_refcur")
                                'End Using

                                If (DT3.Rows.Count > 0) Then
                                    'sb.Length = 0
                                    'dbSelectCommand = sb.Append("exec check_for_entry_1  @Tablename='").Append(tblMustrollYYYY).Append("' ,  @field_name='Reg_No', @Condition='Reg_No=''").Append(ddlReg.SelectedValue).Append("'' and Applicant_No=''").Append(CInt(ApplicantNo)).Append("''' ").ToString
                                    'DT4 = dal.ExecuteCommand_dt(New NpgsqlCommand(dbSelectCommand))

                                    cmd1.CommandText = "check_for_entry_1"
                                    cmd1.CommandType = CommandType.StoredProcedure
                                    cmd1.Parameters.Clear()
                                    cmd1.Parameters.AddWithValue("par_tablename", tblMustrollYYYY)
                                    cmd1.Parameters.AddWithValue("par_field_name", "Reg_No")
                                    sb.Length = 0
                                    cmd1.Parameters.AddWithValue("par_condition", sb.Append("Reg_No='").Append(ddlReg.SelectedValue).Append("' and Applicant_No='").Append(CInt(ApplicantNo)).Append("'").ToString)
                                    DT4 = dal.ExecuteCommand_dt(cmd1)

                                    If (DT4.Rows.Count > 0) Then
                                        AppInMustroll = "Y"
                                        AppDemanded = "Y"

                                        'sb.Length = 0
                                        'dbSelectCommand = sb.Append("exec check_for_entry_1  @Tablename='").Append(tblMustrollYYYY).Append("',  @field_name='Max(Isnull(Dt_to,'''')) as MaxDt_To', @Condition='Reg_No=''").Append(ddlReg.SelectedValue).Append("'' and Applicant_No=''").Append(CInt(ApplicantNo)).Append("''' ").ToString
                                        'DT5 = dal.ExecuteCommand_dt(New NpgsqlCommand(dbSelectCommand))

                                        cmd1.CommandText = "check_for_entry_1"
                                        cmd1.CommandType = CommandType.StoredProcedure
                                        cmd1.Parameters.Clear()
                                        cmd1.Parameters.AddWithValue("@Tablename", tblMustrollYYYY)
                                        cmd1.Parameters.AddWithValue("@field_name", "Max(Isnull(Dt_to,'')) as MaxDt_To")
                                        sb.Length = 0
                                        cmd1.Parameters.AddWithValue("@Condition", sb.Append("Reg_No='").Append(ddlReg.SelectedValue).Append("' and Applicant_No='").Append(CInt(ApplicantNo)).Append("'").ToString)
                                        DT5 = dal.ExecuteCommand_dt(cmd1)

                                        If (DT5.Rows.Count > 0) Then
                                            AppReadOnly = "Readonly"
                                            grdData.Rows(i).Cells(1).BackColor = Drawing.Color.Orange
                                            If DT5.Rows(0)("MaxDt_To").ToString() <> "" Or Year(DT5.Rows(0)("MaxDt_To").ToString()) <> "1900" Then

                                                MaxDt_To = DT5.Rows(0)("MaxDt_To").ToString()
                                                MaxDate = MaxDt_To.Split("/")
                                                MaxDt_To = Convert.ToString(MaxDate(1) + "/" + MaxDate(0) + "/" + MaxDate(2))
                                                dt = MaxDt_To.Split(" ")
                                                MaxDt_To = Convert.ToString(dt(0))
                                                MaxDt_To_MDY = Convert.ToString(MaxDate(0) + "/" + MaxDate(1) + "/" + MaxDate(2))
                                                dt = MaxDt_To_MDY.Split(" ")
                                                MaxDt_To_MDY = Convert.ToString(dt(0))
                                                Exit For 'line used/implemented on 13/04/2023
                                            End If
                                        End If
                                    End If
                                End If
                            Next
                        End If
                        If AppInMustroll <> "Y" Then
                            'Dim arr As Array
                            'arr = New String() {"2021", "1920", "1819", "1718", "1617", "1516", "1415", "1314", "1213", "1112", "1011", "0910", "0809", "0708", "0607", "0506"}
                            'For Each j As String In arr
                            '    tblWork_Demand = j
                            '    AppReadOnly = ""
                            '    DelFlagStatus = ""
                            '    sb.Length = 0
                            '    tblWork_DemandYYYY = sb.Append(short_name).Append(Right(District_Code, 2)).Append("WORK_DEMAND").Append(tblWork_Demand).ToString

                            ' Change for Fin Year Dynamic

                            For J = 0 To DT_FY.Rows.Count - 1
                                Fin_Year = Right(Left(DT_FY.Rows(J)("Financial_Year"), 4), 2) & Right(Right(DT_FY.Rows(J)("Financial_Year"), 4), 2)
                                AppReadOnly = ""
                                DelFlagStatus = ""
                                sb.Length = 0
                                If Session("Is_archived") = "Y" And (Not Session("Archived_finyear_upto") Is Nothing And Session("Archived_finyear_upto") <> "") Then
                                    If Convert.ToInt32(Fin_Year) > Convert.ToInt32(Session("Archived_finyear_upto")) Then
                                        tblWork_DemandYYYY = sb.Append(short_name).Append(Right(District_Code, 2)).Append("WORK_DEMAND").Append(tblWork_Demand).ToString
                                    Else
                                        tblWork_DemandYYYY = sb.Append(Session("Archived_schema")).Append(short_name).Append(Right(District_Code, 2)).Append("WORK_DEMAND").Append(tblWork_Demand).ToString
                                    End If
                                Else
                                    tblWork_DemandYYYY = sb.Append(short_name).Append(Right(District_Code, 2)).Append("WORK_DEMAND").Append(tblWork_Demand).ToString
                                End If
                                'sb.Length = 0
                                'dbSelectCommand = sb.Append("Execute TableExist @TN='").Append(tblWork_DemandYYYY).Append("'").ToString
                                'DT6 = dal.ExecuteCommand_dt(New NpgsqlCommand(dbSelectCommand))

                                cmd1.CommandText = "TableExist"
                                cmd1.CommandType = CommandType.StoredProcedure
                                cmd1.Parameters.Clear()
                                cmd1.Parameters.AddWithValue("par_tn", tblWork_DemandYYYY)
                                DT6 = dal.ExecuteCommand_dt(cmd1, "p_refcur")

                                If (DT6.Rows.Count > 0) Then

                                    'sb.Length = 0
                                    'dbSelectCommand = sb.Append("exec check_for_entry_1  @Tablename='").Append(tblWork_DemandYYYY).Append("' ,  @field_name='Reg_No, Applicant_No', @Condition='Reg_No=''").Append(ddlReg.SelectedValue).Append("'' and Applicant_No=''").Append(CInt(ApplicantNo)).Append("''' ").ToString
                                    'DT7 = dal.ExecuteCommand_dt(New NpgsqlCommand(dbSelectCommand))

                                    cmd1.CommandText = "check_for_entry_1"
                                    cmd1.CommandType = CommandType.StoredProcedure
                                    cmd1.Parameters.Clear()
                                    cmd1.Parameters.AddWithValue("par_tablename", tblWork_DemandYYYY)
                                    cmd1.Parameters.AddWithValue("par_field_name", "Reg_No, Applicant_No")
                                    sb.Length = 0
                                    cmd1.Parameters.AddWithValue("par_condition", sb.Append("Reg_No='").Append(ddlReg.SelectedValue).Append("' and Applicant_No='").Append(CInt(ApplicantNo)).Append("'").ToString)
                                    DT7 = dal.ExecuteCommand_dt(cmd1)

                                    If (DT7.Rows.Count > 0) Then
                                        AppDemanded = "Y"
                                        AppReadOnly = "Readonly"
                                        grdData.Rows(i).Cells(1).BackColor = Drawing.Color.Green
                                        Exit For
                                    End If
                                End If
                            Next
                        End If
                        If Event_Flag = "D" Then
                            CType(grdData.Rows(i).FindControl("chkDelete"), CheckBox).Enabled = False
                            CType(grdData.Rows(i).FindControl("chkDelete"), CheckBox).Checked = True
                            CType(grdData.Rows(i).FindControl("ddlReason"), DropDownList).Enabled = False
                            CType(grdData.Rows(i).FindControl("ddlReason"), DropDownList).Items.Add(New ListItem(Event_Reason, Event_Reason))
                            CType(grdData.Rows(i).FindControl("ddlReason"), DropDownList).SelectedValue = Event_Reason
                            CType(grdData.Rows(i).FindControl("txtDate"), TextBox).Enabled = False
                            CType(grdData.Rows(i).FindControl("txtName"), TextBox).Enabled = False
                            CType(grdData.Rows(i).FindControl("txtDate"), TextBox).Text = Event_Date

                        Else
                            CType(grdData.Rows(i).FindControl("chkDelete"), CheckBox).Enabled = True
                            CType(grdData.Rows(i).FindControl("ddlReason"), DropDownList).Items.Add(New ListItem("Select Reason for Deletion", "00"))
                            CType(grdData.Rows(i).FindControl("ddlReason"), DropDownList).Items.Add(New ListItem("Person Expired", "Person Expired"))
                            CType(grdData.Rows(i).FindControl("ddlReason"), DropDownList).Items.Add(New ListItem("Unwilling to work", "unwilling to work"))
                            CType(grdData.Rows(i).FindControl("ddlReason"), DropDownList).Items.Add(New ListItem("Person shifted to a new family", "Person shifted to a new family"))
                            '****** w.e.f 17-09-2016 ******
                            CType(grdData.Rows(i).FindControl("ddlReason"), DropDownList).Items.Add(New ListItem("Duplicate Applicant", "Duplicate Applicant"))
                            CType(grdData.Rows(i).FindControl("ddlReason"), DropDownList).Items.Add(New ListItem("Fake Applicant", "Fake Applicant"))
                            '******************************
                            'other option removed on 17/08/2017
                            'CType(grdData.Rows(i).FindControl("ddlReason"), DropDownList).Items.Add(New ListItem("Others", "Others"))

                            CType(grdData.Rows(i).FindControl("ddlReason"), DropDownList).SelectedValue = "Select Reason for Deletion"

                            'w.e.f 8-mar-2016
                            note1 = "Note: Wrong Entry (Delete Permanently) option is removed. "
                            CType(grdData.Rows(i).FindControl("txtDate"), TextBox).ReadOnly = False
                        End If

                        If Session("State_Code_d") = "32" Then
                            If i = 20 Then
                                Exit For
                            End If
                        Else
                            If i = 15 Then
                                Exit For
                            End If
                        End If

                        CType(grdData.Rows(i).FindControl("hf_AppDemand"), HiddenField).Value = AppDemanded
                        CType(grdData.Rows(i).FindControl("hf_AppInMustroll"), HiddenField).Value = AppInMustroll
                        CType(grdData.Rows(i).FindControl("hf_Event_Flag"), HiddenField).Value = Event_Flag
                        CType(grdData.Rows(i).FindControl("hf_MaxDt_To_MDY"), HiddenField).Value = MaxDt_To_MDY
                        CType(grdData.Rows(i).FindControl("hf_MaxDt_To"), HiddenField).Value = MaxDt_To
                    Next
                    If (ddlReg.SelectedValue <> "" And ddlReg.SelectedValue <> ViewState("selectRegNo")) Then
                        If JobCard.Value = "YES" Then
                            lblWarning.Visible = True
                            lblWarning.Text = "Warning: Job-Card already Issued for the selected Reg-No!"
                        End If
                        lblNote.Text = "Warning: Clicking of 'DELETE' button will mark as deleted for the applicant(s) ticked under 'Delete' column ! <br>" + note1
                    End If
                Else
                    grdData.Visible = False
                    change.Visible = True
                    Warning.Visible = True
                    lblWarning.Visible = True
                    lblWarning.Text = "Warning: Job-Card already Issued for the selected Reg-No!"
                    lblNote.Visible = False
                    note.Visible = False
                End If
                If (grdData.Rows.Count <= 0) Then

                Else
                    BtnDelete.Visible = True
                    BtnCancel.Visible = True
                End If
            Else
                lblmsg.Visible = True
                lblmsg.Text = "Data Not Found"
                Return
            End If
        Catch ex1 As SqlException
            lblmsg.Visible = True
            lblmsg.Text = "DB-Error found in loading the Data"
            myutil.Exception_log(ex1)
        Catch ex2 As NullReferenceException
            lblmsg.Visible = True
            lblmsg.Text = "Null-Error found in loading the Data"
            myutil.Exception_log(ex2)
        Catch ex As Exception
            lblmsg.Visible = True
            lblmsg.Text = "Error in Loading the Data"
            myutil.Exception_log(ex)
        End Try
    End Sub
    Protected Sub BtnCancel_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles BtnCancel.Click
        Response.Redirect(Request.Url.ToString())
    End Sub
    Protected Sub BtnDelete_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles BtnDelete.Click
        Try
            Dim updatefld As String = String.Empty
            sb = New StringBuilder
            cmd1 = New NpgsqlCommand
            If chkChange.Checked = True Then
                If Trim(txtHead.Text) = "" Or Trim(txtFatHus.Text) = "" Then
                    lblmsg.Visible = True
                    lblmsg.Text = "RECORD NOT SAVED as Head of Household/Father or Husband-Name is Blank !"
                    Return
                End If


                'sb.Append("Update ").Append(reg).Append(" set Head_Of_Household=@Head_Of_Household , Father_or_Husband_Name=@Father_or_Husband_Name , Entry_by=@Entry_by, Entry_Date =@Entry_Date where Reg_No =@Reg_No")
                'mycmd = New NpgsqlCommand()
                'mycmd.CommandText = sb.ToString
                'mycmd.Parameters.Add("@Head_Of_Household", NpgsqlTypes.NpgsqlDbType.Varchar, 75).Value = txtHead.Text
                'mycmd.Parameters.Add("@Father_or_Husband_Name", NpgsqlTypes.NpgsqlDbType.Varchar, 75).Value = txtFatHus.Text
                'mycmd.Parameters.Add("@Entry_by", NpgsqlTypes.NpgsqlDbType.Varchar, 50).Value = Entry_by
                'mycmd.Parameters.Add("@Entry_Date", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = Entry_Date
                'mycmd.Parameters.Add("@Reg_No", NpgsqlTypes.NpgsqlDbType.Varchar).Value = ddlReg.SelectedValue
                'dal.ExecuteCommand_rowsaffected(mycmd)
                'mycmd.Parameters.Clear()
                'mycmd.Dispose()


                Using mycmd As New NpgsqlCommand()
                    mycmd.CommandText = "SpCommon_DelApp"
                    mycmd.CommandType = CommandType.StoredProcedure
                    mycmd.Parameters.Add("p_Tbl_Name", NpgsqlTypes.NpgsqlDbType.Varchar, 75).Value = txtHead.Text
                    mycmd.Parameters.Add("p_Head_Of_Household", NpgsqlTypes.NpgsqlDbType.Varchar, 75).Value = txtHead.Text
                    mycmd.Parameters.Add("p_Father_or_Husband_Name", NpgsqlTypes.NpgsqlDbType.Varchar, 75).Value = txtFatHus.Text
                    mycmd.Parameters.Add("p_Entry_by", NpgsqlTypes.NpgsqlDbType.Varchar, 50).Value = Entry_by
                    mycmd.Parameters.Add("p_Entry_Date", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = Entry_Date
                    mycmd.Parameters.Add("p_reg_no", NpgsqlTypes.NpgsqlDbType.Varchar).Value = ddlReg.SelectedValue
                    mycmd.Parameters.Add("p_Sp_No", NpgsqlTypes.NpgsqlDbType.Integer).Value = 1
                    dal.ExecuteCommand_rowsaffected(mycmd)
                End Using

                lbl.Visible = True
                sb.Length = 0
                lbl.Text = sb.Append("For the Reg. No. '").Append(ddlReg.SelectedValue).Append("',  Head of Household as '").Append(txtHead.Text).Append("' and <br>Father/Husband Name as '").Append(txtFatHus.Text).Append("'<br>have been updated ! ").ToString
            End If

            '************ check whether all applicant or last single applicant are selected do delete which should not get deleted.********
            Dim cnt As Integer = 0
            For i = 0 To grdData.Rows.Count - 1
                Event_Flag = CType(grdData.Rows(i).FindControl("hf_Event_Flag"), HiddenField).Value
                If CType(grdData.Rows(i).FindControl("chkDelete"), CheckBox).Checked = True Then
                    cnt = cnt + 1
                End If
            Next
            If cnt = grdData.Rows.Count Then  'means whether all applicant or last single applicant are selected do delete
                lblmsg.Text = "You cannot delete all the applicant(s) from a Family using 'Delete Applicant' option ! <br> Use 'Delete Registration' option to delete all applicants (means to delete a jobcard)!"
                lblmsg.Visible = True
                Return
            End If
            '***********end****************

            For i = 0 To grdData.Rows.Count - 1
                AppDemanded = CType(grdData.Rows(i).FindControl("hf_AppDemand"), HiddenField).Value
                AppInMustroll = CType(grdData.Rows(i).FindControl("hf_AppInMustroll"), HiddenField).Value
                Event_Flag = CType(grdData.Rows(i).FindControl("hf_Event_Flag"), HiddenField).Value
                MaxDt_To_MDY = CType(grdData.Rows(i).FindControl("hf_MaxDt_To_MDY"), HiddenField).Value
                MaxDt_To = CType(grdData.Rows(i).FindControl("hf_MaxDt_To"), HiddenField).Value
                ApplicantNo = CType(grdData.Rows(i).FindControl("lblApp"), Label).Text
                ApplicantName = CType(grdData.Rows(i).FindControl("txtName"), TextBox).Text
                DeletionDate = CType(grdData.Rows(i).FindControl("txtDate"), TextBox).Text

                If CType(grdData.Rows(i).FindControl("chkDelete"), CheckBox).Enabled = True Then
                    If CType(grdData.Rows(i).FindControl("chkDelete"), CheckBox).Checked = True And Event_Flag <> "D" Then

                        Try
                            If DeletionDate <> "" Then
                                DeletionDateDMY = DeletionDate
                                If IsDate(myutil.FormatDateMMDDYYYY("DATE", DeletionDate)) Then
                                    DeletionDate = myutil.FormatDateMMDDYYYY("Deletion w.e.f.(DD/MM/YYYY)", DeletionDate)
                                    If (DateDiff("d", DateTime.Today.ToShortDateString(), DeletionDate)) > 0 Then
                                        lblmsg.Visible = True
                                        sb.Length = 0
                                        lblmsg.Text = sb.Append("Date of Deletion ").Append(DeletionDateDMY).Append(" Cannot be Future date !").ToString
                                        Return
                                    End If
                                    deldt = DeletionDate.Split("/")
                                    'deldt_To = Convert.ToString(deldt(1) + "/" + deldt(0) + "/" + deldt(2))
                                    sb.Length = 0
                                    deldt_To = sb.Append(deldt(1) + "/" + deldt(0) + "/" + deldt(2)).ToString
                                    dt = deldt_To.Split(" ")
                                    DeletionDate = Convert.ToString(dt(0))
                                Else
                                    lblmsg.Visible = True
                                    lblmsg.Text = "Please Enter Valid Datetime Format in Deletion Date"
                                    Return
                                End If
                            Else
                                DeletionDate = ""
                            End If
                        Catch ex As Exception
                            lblmsg.Visible = True
                            lblmsg.Text = "Please Enter Valid Datetime Format in Deletion Date"
                            Return
                        End Try

                        CannotDelete = ""
                        DelFlagClicked = "Y"
                        If CType(grdData.Rows(i).FindControl("ddlReason"), DropDownList).SelectedValue = "00" Then
                            lblmsg.Visible = True
                            lblmsg.Text = "Pls. select 'Reason for Deletion', if 'Delete' checkbox is ticked !"
                            Return
                        End If
                        If DeletionDate = "" Then
                            lblmsg.Visible = True
                            lblmsg.Text = "Pls. enter Deletion with-effect-from..!"
                            Return
                        End If

                        '------Check Any Demand made by Applicant on or after DelDate
                        DeleteDateMMDDYYYY = myutil.FormatDateMMDDYYYY("Deletion Date", DeletionDate)

                        If CType(grdData.Rows(i).FindControl("ddlReason"), DropDownList).SelectedValue = "Wrong Entry" Then
                        Else
                            'If AppInMustroll = "Y" Then
                            '    If DeletionDate <> "" Then
                            '        DiffDays = DateDiff("D", MaxDt_To_MDY, DeleteDateMMDDYYYY)
                            '        'test
                            '        'If DiffDays <= 0 Then
                            '        '    lblmsg.Visible = True
                            '        '    sb.Length = 0
                            '        '    lblmsg.Text = sb.Append("CANNOT MARK as DELETED for the selected Applicant '").Append(ApplicantName).Append("' <br>as applicant has worked till '").Append(MaxDt_To + "' !<br>(Date of Deletion w.e.f. should be greater than the last-working-date in Mustroll)<br>You can delete this applicant after the last-working-date ").Append(MaxDt_To).Append(" in Mustroll!").ToString
                            '        '    CannotDelete = "Y"
                            '        '    Return
                            '        'Else
                            '        sb.Length = 0
                            '        If Month(DeleteDateMMDDYYYY) > 3 Then
                            '            finyear = sb.Append(Trim(Mid(Year(DeleteDateMMDDYYYY), 3, 2))).Append(Mid(Year(DeleteDateMMDDYYYY) + 1, 3, 2)).ToString
                            '        Else
                            '            finyear = sb.Append(Trim(Mid(Year(DeleteDateMMDDYYYY) - 1, 3, 2))).Append(Mid(Year(DeleteDateMMDDYYYY), 3, 2)).ToString
                            '        End If

                            '        Dim tblWork_Demand As Array
                            '        tblWork_Demand = New String() {"0506", "0607", "0708", "0809", "0910", "1011", "1112", "1213", "1314", "1415", "1516", "1617", "1718", "1819", "1920", "2021", "2122", "2223", "2324"}
                            '        Dim tblWork_Allotted As Array
                            '        tblWork_Allotted = New String() {"0506", "0607", "0708", "0809", "0910", "1011", "1112", "1213", "1314", "1415", "1516", "1617", "1718", "1819", "1920", "2021", "2122", "2223", "2324"}
                            '        Dim j As Integer
                            '        Select Case finyear
                            '            Case "2324"
                            '                j = 18
                            '            Case "2223"
                            '                j = 17
                            '            Case "2122"
                            '                j = 16
                            '            Case "2021"
                            '                j = 15
                            '            Case "1920"
                            '                j = 14
                            '            Case "1819"
                            '                j = 13
                            '            Case "1718"
                            '                j = 12
                            '            Case "1617"
                            '                j = 11
                            '            Case "1516"
                            '                j = 10
                            '            Case "1415"
                            '                j = 9
                            '            Case "1314"
                            '                j = 8
                            '            Case "1213"
                            '                j = 7
                            '            Case "1112"
                            '                j = 6
                            '            Case "1011"
                            '                j = 5
                            '            Case "0910"
                            '                j = 4
                            '            Case "0809"
                            '                j = 3
                            '            Case "0708"
                            '                j = 2
                            '            Case "0607"
                            '                j = 1
                            '            Case "0506"
                            '                j = 0
                            '        End Select
                            '        Do While j <= 9

                            '            'sb.Length = 0
                            '            'tblWork_AllottedYYYY = sb.Append(short_name).Append(Right(District_Code, 2)).Append("WORK_ALLOTTED").Append(tblWork_Allotted(j)).ToString
                            '            'sb.Length = 0
                            '            'tblWork_DemandYYYY = sb.Append(short_name).Append(Right(District_Code, 2)).Append("WORK_DEMAND").Append(tblWork_Demand(j)).ToString

                            '            'Change After Data tables are archived in the same server on 24th/March/2021
                            '            Fin_Year = tblWork_Allotted(j)

                            '            If Session("Is_archived") = "Y" And (Not Session("Archived_finyear_upto") Is Nothing And Session("Archived_finyear_upto") <> "") Then
                            '                If Convert.ToInt32(Fin_Year) > Convert.ToInt32(Session("Archived_finyear_upto")) Then
                            '                    sb.Length = 0
                            '                    tblWork_AllottedYYYY = sb.Append(short_name).Append(Right(District_Code, 2)).Append("WORK_ALLOTTED").Append(Fin_Year).ToString
                            '                    sb.Length = 0
                            '                    tblWork_DemandYYYY = sb.Append(short_name).Append(Right(District_Code, 2)).Append("WORK_DEMAND").Append(Fin_Year).ToString

                            '                Else
                            '                    sb.Length = 0
                            '                    tblWork_AllottedYYYY = sb.Append(Session("Archived_schema")).Append(short_name).Append(Right(District_Code, 2)).Append("WORK_ALLOTTED").Append(Fin_Year).ToString
                            '                    sb.Length = 0
                            '                    tblWork_DemandYYYY = sb.Append(Session("Archived_schema")).Append(short_name).Append(Right(District_Code, 2)).Append("WORK_DEMAND").Append(Fin_Year).ToString

                            '                End If
                            '            Else
                            '                sb.Length = 0
                            '                tblWork_AllottedYYYY = sb.Append(short_name).Append(Right(District_Code, 2)).Append("WORK_ALLOTTED").Append(Fin_Year).ToString
                            '                sb.Length = 0
                            '                tblWork_DemandYYYY = sb.Append(short_name).Append(Right(District_Code, 2)).Append("WORK_DEMAND").Append(Fin_Year).ToString
                            '            End If

                            '            cmd1.CommandText = "TableExist"
                            '            cmd1.CommandType = CommandType.StoredProcedure
                            '            cmd1.Parameters.Clear()
                            '            cmd1.Parameters.AddWithValue("@TN", tblWork_AllottedYYYY)
                            '            DT1 = dal.ExecuteCommand_dt(cmd1)

                            '            If (DT1.Rows.Count > 0) Then

                            '                'mycmd = New NpgsqlCommand()
                            '                'sb.Length = 0
                            '                'sb.Append("select distinct demand_id from ").Append(tblWork_DemandYYYY)
                            '                'sb.Append(" where reg_no =@reg_no  and Applicant_No =@Applicant_No")
                            '                'sb.Append(" and ('").Append(MaxDt_To_MDY).Append("' between work_demand_from and work_demand_to)")

                            '                'mycmd.CommandText = sb.ToString
                            '                'mycmd.Parameters.Add("@reg_no", NpgsqlTypes.NpgsqlDbType.Varchar).Value = ddlReg.SelectedValue
                            '                'mycmd.Parameters.Add("@Applicant_No", NpgsqlTypes.NpgsqlDbType.Integer).Value = ApplicantNo
                            '                'DT2 = dal.ExecuteCommand_dt(mycmd)
                            '                'mycmd.Parameters.Clear()
                            '                'mycmd.Dispose()

                            '                Using mycmd As New NpgsqlCommand()
                            '                    mycmd.CommandText = "SpCommon_DelApp"
                            '                    mycmd.CommandType = CommandType.StoredProcedure
                            '                    mycmd.Parameters.Add("@Tbl_Name", NpgsqlTypes.NpgsqlDbType.Varchar, 30).Value = tblWork_DemandYYYY
                            '                    mycmd.Parameters.Add("@reg_no", NpgsqlTypes.NpgsqlDbType.Varchar, 34).Value = ddlReg.SelectedValue
                            '                    mycmd.Parameters.Add("@Applicant_No", NpgsqlTypes.NpgsqlDbType.Integer).Value = ApplicantNo
                            '                    mycmd.Parameters.Add("@Sp_No", NpgsqlTypes.NpgsqlDbType.Integer).Value = 2
                            '                    DT2 = dal.ExecuteCommand_dt(mycmd)
                            '                End Using


                            '                If (DT2.Rows.Count <= 0) Then

                            '                    'cmd = New NpgsqlCommand()
                            '                    'sb.Append("Delete from ").Append(tblWork_AllottedYYYY).Append(" where reg_no =@reg_no and Applicant_No =@Applicant_No")
                            '                    'sb.Append(" and demand_id in (select demand_id from ").Append(tblWork_DemandYYYY)
                            '                    'sb.Append(" where reg_no =@reg_no  and Applicant_No =@Applicant_No  and work_demand_to >= @work_demand_to)")

                            '                    'cmd.CommandText = sb.ToString

                            '                    'cmd.Parameters.Add("@reg_no", NpgsqlTypes.NpgsqlDbType.Varchar).Value = ddlReg.SelectedValue
                            '                    'cmd.Parameters.Add("@Applicant_No", NpgsqlTypes.NpgsqlDbType.Integer).Value = ApplicantNo
                            '                    'cmd.Parameters.Add("@work_demand_to", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DeleteDateMMDDYYYY
                            '                    'dal.ExecuteCommand_rowsaffected(cmd)
                            '                    'cmd.Parameters.Clear()
                            '                    'cmd.Dispose()

                            '                    Using cmd As New NpgsqlCommand()
                            '                        cmd.CommandText = "SpCommon_DelApp"
                            '                        cmd.CommandType = CommandType.StoredProcedure
                            '                        cmd.Parameters.Add("@Tbl_Name", NpgsqlTypes.NpgsqlDbType.Varchar, 30).Value = tblWork_AllottedYYYY
                            '                        cmd.Parameters.Add("@Tbl_Name1", NpgsqlTypes.NpgsqlDbType.Varchar, 30).Value = tblWork_DemandYYYY
                            '                        cmd.Parameters.Add("@reg_no", NpgsqlTypes.NpgsqlDbType.Varchar).Value = ddlReg.SelectedValue
                            '                        cmd.Parameters.Add("@Applicant_No", NpgsqlTypes.NpgsqlDbType.Integer).Value = ApplicantNo
                            '                        cmd.Parameters.Add("@work_demand_to", NpgsqlTypes.NpgsqlDbType.Varchar, 30).Value = DeleteDateMMDDYYYY
                            '                        cmd.Parameters.Add("@Sp_No", NpgsqlTypes.NpgsqlDbType.Integer).Value = 3
                            '                        dal.ExecuteCommand_rowsaffected(cmd)
                            '                    End Using



                            '                    'sb.Length = 0
                            '                    'dbSelectCommand = sb.Append("EXECUTE DeleteForAll @Tablename='").Append(tblWork_DemandYYYY).Append("' , @Condition='Reg_No =''").Append(ddlReg.SelectedValue).Append("'' and Applicant_No =''").Append(ApplicantNo).Append("'' and work_demand_to >= ''").Append(DeleteDateMMDDYYYY).Append("'''").ToString
                            '                    'dal.ExecuteCommand_rowsaffected(dbSelectCommand)

                            '                    cmd1.CommandText = "DeleteForAll"
                            '                    cmd1.CommandType = CommandType.StoredProcedure
                            '                    cmd1.Parameters.Clear()
                            '                    cmd1.Parameters.AddWithValue("@Tablename", tblWork_DemandYYYY)
                            '                    sb.Length = 0
                            '                    cmd1.Parameters.AddWithValue("@Condition", sb.Append("Reg_No ='").Append(ddlReg.SelectedValue).Append("' and Applicant_No ='").Append(ApplicantNo).Append("' and work_demand_to >= '").Append(DeleteDateMMDDYYYY).Append("'").ToString)
                            '                    dal.ExecuteCommand_rowsaffected(cmd1)

                            '                Else
                            '                    For Each drow As DataRow In DT2.Rows
                            '                        demand_id = DT2.Rows(0)("demand_id").ToString()

                            '                        'sb.Length = 0
                            '                        'sb.Append("EXECUTE check_for_entry_1  @Tablename='").Append(tblWork_DemandYYYY).Append("' ,@field_name='demand_id' , ")
                            '                        'sb.Append("@Condition='reg_no=''").Append(ddlReg.SelectedValue).Append("'' and Applicant_No =''").Append(ApplicantNo).Append("'' and demand_id=''").Append(demand_id).Append("'' and demand_left_from > ''").Append(MaxDt_To_MDY).Append("'''")
                            '                        'DT3 = dal.ExecuteCommand_dt(New NpgsqlCommand(sb.ToString))

                            '                        cmd1.CommandText = "check_for_entry_1"
                            '                        cmd1.CommandType = CommandType.StoredProcedure
                            '                        cmd1.Parameters.Clear()
                            '                        cmd1.Parameters.AddWithValue("@Tablename", tblWork_DemandYYYY)
                            '                        cmd1.Parameters.AddWithValue("@field_name", "demand_id")
                            '                        sb.Length = 0
                            '                        cmd1.Parameters.AddWithValue("@Condition", sb.Append("reg_no='").Append(ddlReg.SelectedValue).Append("' and Applicant_No ='").Append(ApplicantNo).Append("' and demand_id='").Append(demand_id).Append("' and demand_left_from > '").Append(MaxDt_To_MDY).Append("'").ToString)
                            '                        DT3 = dal.ExecuteCommand_dt(cmd1)

                            '                        If (DT3.Rows.Count > 0) Then

                            '                            'sb.Length = 0
                            '                            'sb.Append("Delete from ").Append(tblWork_AllottedYYYY).Append(" where reg_no =@reg_no  and Applicant_No =@Applicant_No and demand_id in ")
                            '                            'sb.Append(" (select demand_id from ").Append(tblWork_DemandYYYY)
                            '                            'sb.Append(" where reg_no =@reg_no  and Applicant_No =@Applicant_No and demand_id =@demand_id   and  demand_left_from >@demand_left_from)")

                            '                            'mycmd = New NpgsqlCommand()
                            '                            'mycmd.CommandText = sb.ToString
                            '                            'mycmd.Parameters.Add("@reg_no", NpgsqlTypes.NpgsqlDbType.Varchar).Value = ddlReg.SelectedValue
                            '                            'mycmd.Parameters.Add("@Applicant_No", NpgsqlTypes.NpgsqlDbType.Integer).Value = ApplicantNo
                            '                            'mycmd.Parameters.Add("@demand_id", NpgsqlTypes.NpgsqlDbType.Integer).Value = demand_id
                            '                            'mycmd.Parameters.Add("@demand_left_from", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = MaxDt_To_MDY
                            '                            'DT4 = dal.ExecuteCommand_dt(mycmd)
                            '                            'mycmd.Parameters.Clear()
                            '                            'mycmd.Dispose()


                            '                            Using mycmd As New NpgsqlCommand()
                            '                                mycmd.CommandText = "SpCommon_DelApp"
                            '                                mycmd.CommandType = CommandType.StoredProcedure
                            '                                mycmd.Parameters.Add("@Tbl_Name", NpgsqlTypes.NpgsqlDbType.Varchar, 30).Value = tblWork_AllottedYYYY
                            '                                mycmd.Parameters.Add("@Tbl_Name1", NpgsqlTypes.NpgsqlDbType.Varchar, 30).Value = tblWork_DemandYYYY
                            '                                mycmd.Parameters.Add("@reg_no", NpgsqlTypes.NpgsqlDbType.Varchar).Value = ddlReg.SelectedValue
                            '                                mycmd.Parameters.Add("@Applicant_No", NpgsqlTypes.NpgsqlDbType.Integer).Value = ApplicantNo
                            '                                mycmd.Parameters.Add("@demand_id", NpgsqlTypes.NpgsqlDbType.Integer).Value = demand_id
                            '                                mycmd.Parameters.Add("@demand_left_from", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = MaxDt_To_MDY
                            '                                mycmd.Parameters.Add("@Sp_No", NpgsqlTypes.NpgsqlDbType.Integer).Value = 4
                            '                                DT4 = dal.ExecuteCommand_dt(mycmd)
                            '                            End Using


                            '                            DT4 = New DataTable
                            '                            sb.Length = 0
                            '                            updatefld = sb.Append("work_allot_to =''").Append(MaxDt_To_MDY).Append("'', work_demand_to =''").Append(MaxDt_To_MDY).Append("'' , Entry_Date=''").Append(Entry_Date).Append("'', Entry_By=N''").Append(Entry_by).Append("'' ").ToString

                            '                            'sb.Length = 0
                            '                            'dbSelectCommand = sb.Append("EXECUTE Update_table  @table='").Append(tblWork_AllottedYYYY).Append("' ,@condition1=N'").Append(updatefld).Append("' , @condition='reg_no=''").Append(ddlReg.SelectedValue).Append("'' and  Applicant_No=''").Append(ApplicantNo).Append("'' and demand_id=''").Append(demand_id).Append("''  and work_allot_to > ''").Append(MaxDt_To_MDY).Append("''' ").ToString
                            '                            'DT4 = dal.ExecuteCommand_dt(New NpgsqlCommand(dbSelectCommand))

                            '                            cmd1.CommandText = "Update_table"
                            '                            cmd1.CommandType = CommandType.StoredProcedure
                            '                            cmd1.Parameters.Clear()
                            '                            cmd1.Parameters.AddWithValue("@table", tblWork_AllottedYYYY)
                            '                            cmd1.Parameters.Add("@condition1", NpgsqlTypes.NpgsqlDbType.Varchar, 700).Value = updatefld
                            '                            sb.Length = 0
                            '                            cmd1.Parameters.AddWithValue("@Condition", sb.Append("reg_no='").Append(ddlReg.SelectedValue).Append("' and  Applicant_No='").Append(ApplicantNo).Append("' and demand_id='").Append(demand_id).Append("'  and work_allot_to > '").Append(MaxDt_To_MDY).Append("'").ToString)
                            '                            dal.ExecuteCommand_rowsaffected(cmd1)

                            '                            'DT4 = New DataTable

                            '                            'sb.Length = 0
                            '                            'dbSelectCommand = sb.Append("EXECUTE  DeleteForAll  @Tablename='").Append(tblWork_DemandYYYY).Append("' , @Condition='reg_no = ''").Append(ddlReg.SelectedValue).Append("'' and Applicant_No =''").Append(ApplicantNo).Append("'' and demand_id =''").Append(demand_id).Append("'' and demand_left_from >''").Append(MaxDt_To_MDY).Append("'''").ToString
                            '                            'DT4 = dal.ExecuteCommand_dt(New NpgsqlCommand(dbSelectCommand))

                            '                            cmd1.CommandText = "DeleteForAll"
                            '                            cmd1.CommandType = CommandType.StoredProcedure
                            '                            cmd1.Parameters.Clear()
                            '                            cmd1.Parameters.AddWithValue("@Tablename", tblWork_DemandYYYY)
                            '                            sb.Length = 0
                            '                            cmd1.Parameters.AddWithValue("@Condition", sb.Append("reg_no = '").Append(ddlReg.SelectedValue).Append("' and Applicant_No ='").Append(ApplicantNo).Append("' and demand_id ='").Append(demand_id).Append("' and demand_left_from >'").Append(MaxDt_To_MDY).Append("'").ToString)
                            '                            dal.ExecuteCommand_rowsaffected(cmd1)

                            '                            'DT4 = New DataTable


                            '                            'sb.Length = 0
                            '                            'sb.Append("Update ").Append(tblWork_DemandYYYY).Append(" set bal_days=0, work_demand_to =@work_demand_to , demand_left_from=null, demand_left_to=null, ")
                            '                            'sb.Append(" Entry_Date=@Entry_Date, Entry_By=@Entry_By where reg_no =@reg_no and Applicant_No = @Applicant_No and demand_id =@demand_id and work_demand_to > @work_demand_to ")

                            '                            'cmd1.Parameters.Add("@work_demand_to", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = MaxDt_To_MDY
                            '                            'cmd1.Parameters.Add("@Entry_Date", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = Entry_Date
                            '                            'cmd1.Parameters.Add("@Entry_By", NpgsqlTypes.NpgsqlDbType.Varchar, 50).Value = Entry_by
                            '                            'cmd1.Parameters.Add("@reg_no", NpgsqlTypes.NpgsqlDbType.Varchar).Value = ddlReg.SelectedValue
                            '                            'cmd1.Parameters.Add("@Applicant_No", NpgsqlTypes.NpgsqlDbType.Integer).Value = ApplicantNo
                            '                            'cmd1.Parameters.Add("@demand_id", NpgsqlTypes.NpgsqlDbType.Integer).Value = demand_id
                            '                            'dal.ExecuteCommand_rowsaffected(cmd1)
                            '                            'cmd1.Parameters.Clear()

                            '                            cmd1.CommandText = "SpCommon_DelApp"
                            '                            cmd1.CommandType = CommandType.StoredProcedure
                            '                            cmd1.Parameters.Add("@Tbl_Name", NpgsqlTypes.NpgsqlDbType.Varchar, 30).Value = tblWork_DemandYYYY
                            '                            cmd1.Parameters.Add("@work_demand_to", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = MaxDt_To_MDY
                            '                            cmd1.Parameters.Add("@Entry_Date", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = Entry_Date
                            '                            cmd1.Parameters.Add("@Entry_By", NpgsqlTypes.NpgsqlDbType.Varchar, 50).Value = Entry_by
                            '                            cmd1.Parameters.Add("@reg_no", NpgsqlTypes.NpgsqlDbType.Varchar).Value = ddlReg.SelectedValue
                            '                            cmd1.Parameters.Add("@Applicant_No", NpgsqlTypes.NpgsqlDbType.Integer).Value = ApplicantNo
                            '                            cmd1.Parameters.Add("@demand_id", NpgsqlTypes.NpgsqlDbType.Integer).Value = demand_id
                            '                            cmd1.Parameters.Add("@Sp_No", NpgsqlTypes.NpgsqlDbType.Integer).Value = 5
                            '                            dal.ExecuteCommand_rowsaffected(cmd1)
                            '                            cmd1.Parameters.Clear()

                            '                            DT4 = New DataTable
                            '                        End If

                            '                    Next

                            '                End If
                            '            End If
                            '            j = j + 1
                            '        Loop

                            '        'End If
                            '    Else
                            '        lblmsg.Text = "Please Enter Deletion Date"
                            '        lblmsg.Visible = True
                            '        Return
                            '    End If
                            'ElseIf AppDemanded = "Y" Then
                            '    sb.Length = 0
                            '    If Month(DeleteDateMMDDYYYY) > 3 Then
                            '        finyear = sb.Append(Trim(Mid(Year(DeleteDateMMDDYYYY), 3, 2))).Append(Mid(Year(DeleteDateMMDDYYYY) + 1, 3, 2)).ToString
                            '    Else
                            '        finyear = sb.Append(Trim(Mid(Year(DeleteDateMMDDYYYY) - 1, 3, 2))).Append(Mid(Year(DeleteDateMMDDYYYY), 3, 2)).ToString
                            '    End If

                            '    Dim tblWork_Demand As Array
                            '    tblWork_Demand = New String() {"0506", "0607", "0708", "0809", "0910", "1011", "1112", "1213", "1314", "1415", "1516", "1617", "1718", "1819", "1920", "2021", "2122", "2223", "2324"}
                            '    Dim tblWork_Allotted As Array
                            '    tblWork_Allotted = New String() {"0506", "0607", "0708", "0809", "0910", "1011", "1112", "1213", "1314", "1415", "1516", "1617", "1718", "1819", "1920", "2021", "2122", "2223", "2324"}
                            '    Dim j As Integer
                            '    Select Case finyear
                            '        Case "2324"
                            '            j = 18
                            '        Case "2223"
                            '            j = 17
                            '        Case "2122"
                            '            j = 16
                            '        Case "2021"
                            '            j = 15
                            '        Case "1920"
                            '            j = 14
                            '        Case "1819"
                            '            j = 13
                            '        Case "1718"
                            '            j = 12
                            '        Case "1617"
                            '            j = 11
                            '        Case "1516"
                            '            j = 10
                            '        Case "1415"
                            '            j = 9
                            '        Case "1314"
                            '            j = 8
                            '        Case "1213"
                            '            j = 7
                            '        Case "1112"
                            '            j = 6
                            '        Case "1011"
                            '            j = 5
                            '        Case "0910"
                            '            j = 4
                            '        Case "0809"
                            '            j = 3
                            '        Case "0708"
                            '            j = 2
                            '        Case "0607"
                            '            j = 1
                            '        Case "0506"
                            '            j = 0
                            '    End Select
                            '    Do While j <= 9
                            '        'sb.Length = 0
                            '        'tblWork_AllottedYYYY = sb.Append(short_name).Append(Right(District_Code, 2)).Append("WORK_ALLOTTED").Append(tblWork_Allotted(j)).ToString
                            '        'sb.Length = 0
                            '        'tblWork_DemandYYYY = sb.Append(short_name).Append(Right(District_Code, 2)).Append("WORK_DEMAND").Append(tblWork_Demand(j)).ToString

                            '        'Change After Data tables are archived in the same server on 24th/March/2021
                            '        Fin_Year = tblWork_Allotted(j)

                            '        If Session("Is_archived") = "Y" And (Not Session("Archived_finyear_upto") Is Nothing And Session("Archived_finyear_upto") <> "") Then
                            '            If Convert.ToInt32(Fin_Year) > Convert.ToInt32(Session("Archived_finyear_upto")) Then
                            '                sb.Length = 0
                            '                tblWork_AllottedYYYY = sb.Append(short_name).Append(Right(District_Code, 2)).Append("WORK_ALLOTTED").Append(Fin_Year).ToString
                            '                sb.Length = 0
                            '                tblWork_DemandYYYY = sb.Append(short_name).Append(Right(District_Code, 2)).Append("WORK_DEMAND").Append(Fin_Year).ToString

                            '            Else
                            '                sb.Length = 0
                            '                tblWork_AllottedYYYY = sb.Append(Session("Archived_schema")).Append(short_name).Append(Right(District_Code, 2)).Append("WORK_ALLOTTED").Append(Fin_Year).ToString
                            '                sb.Length = 0
                            '                tblWork_DemandYYYY = sb.Append(Session("Archived_schema")).Append(short_name).Append(Right(District_Code, 2)).Append("WORK_DEMAND").Append(Fin_Year).ToString

                            '            End If
                            '        Else
                            '            sb.Length = 0
                            '            tblWork_AllottedYYYY = sb.Append(short_name).Append(Right(District_Code, 2)).Append("WORK_ALLOTTED").Append(Fin_Year).ToString
                            '            sb.Length = 0
                            '            tblWork_DemandYYYY = sb.Append(short_name).Append(Right(District_Code, 2)).Append("WORK_DEMAND").Append(Fin_Year).ToString
                            '        End If


                            '        cmd1.CommandText = "TableExist"
                            '        cmd1.CommandType = CommandType.StoredProcedure
                            '        cmd1.Parameters.Clear()
                            '        cmd1.Parameters.AddWithValue("@TN", tblWork_AllottedYYYY)
                            '        DT1 = dal.ExecuteCommand_dt(cmd1)

                            '        If (DT1.Rows.Count > 0) Then

                            '            'mycmd = New NpgsqlCommand()
                            '            'sb.Length = 0
                            '            'sb.Append("Delete from ").Append(tblWork_AllottedYYYY).Append(" where reg_no =@reg_no  and Applicant_No =@Applicant_No and demand_id in ")
                            '            'sb.Append(" (select demand_id from ").Append(tblWork_DemandYYYY)
                            '            'sb.Append(" where reg_no =@reg_no  and Applicant_No =@Applicant_No   and work_demand_to >=@work_demand_to)")
                            '            'mycmd.CommandText = sb.ToString

                            '            'mycmd.Parameters.Add("@reg_no", NpgsqlTypes.NpgsqlDbType.Varchar).Value = ddlReg.SelectedValue
                            '            'mycmd.Parameters.Add("@Applicant_No", NpgsqlTypes.NpgsqlDbType.Integer).Value = ApplicantNo
                            '            'mycmd.Parameters.Add("@work_demand_to", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DeleteDateMMDDYYYY
                            '            'dal.ExecuteCommand_rowsaffected(mycmd)
                            '            'mycmd.Parameters.Clear()
                            '            'mycmd.Dispose()


                            '            Using mycmd As New NpgsqlCommand()
                            '                mycmd.CommandText = "SpCommon_DelApp"
                            '                mycmd.CommandType = CommandType.StoredProcedure
                            '                mycmd.Parameters.Add("@Tbl_Name", NpgsqlTypes.NpgsqlDbType.Varchar, 30).Value = tblWork_AllottedYYYY
                            '                mycmd.Parameters.Add("@Tbl_Name1", NpgsqlTypes.NpgsqlDbType.Varchar, 30).Value = tblWork_DemandYYYY
                            '                mycmd.Parameters.Add("@reg_no", NpgsqlTypes.NpgsqlDbType.Varchar).Value = ddlReg.SelectedValue
                            '                mycmd.Parameters.Add("@Applicant_No", NpgsqlTypes.NpgsqlDbType.Integer).Value = ApplicantNo
                            '                mycmd.Parameters.Add("@work_demand_to", NpgsqlTypes.NpgsqlDbType.Varchar, 30).Value = DeleteDateMMDDYYYY
                            '                mycmd.Parameters.Add("@Sp_No", NpgsqlTypes.NpgsqlDbType.Integer).Value = 3  '3 (because same query is used twice)
                            '                dal.ExecuteCommand_rowsaffected(mycmd)
                            '            End Using


                            '            'sb.Length = 0
                            '            'dbSelectCommand = sb.Append("EXECUTE DeleteForAll   @Tablename='").Append(tblWork_DemandYYYY).Append("' ,@Condition='Reg_No = ''").Append(ddlReg.SelectedValue).Append("''  and Applicant_No =''").Append(ApplicantNo).Append("'' and work_demand_to >= ''").Append(DeleteDateMMDDYYYY).Append("'''").ToString
                            '            'dal.ExecuteCommand_rowsaffected(dbSelectCommand)

                            '            cmd1.CommandText = "DeleteForAll"
                            '            cmd1.CommandType = CommandType.StoredProcedure
                            '            cmd1.Parameters.Clear()
                            '            cmd1.Parameters.AddWithValue("@Tablename", tblWork_DemandYYYY)
                            '            sb.Length = 0
                            '            cmd1.Parameters.AddWithValue("@Condition", sb.Append("Reg_No = '").Append(ddlReg.SelectedValue).Append("'  and Applicant_No ='").Append(ApplicantNo).Append("' and work_demand_to >= '").Append(DeleteDateMMDDYYYY).Append("'").ToString)
                            '            dal.ExecuteCommand_rowsaffected(cmd1)

                            '        End If
                            '        j = j + 1
                            '    Loop
                            'End If


                            If CannotDelete <> "Y" Then
                                Try
                                    dal.BeginTransaction()
                                    Using mycmd As New NpgsqlCommand()
                                        mycmd.CommandText = "SpHistory_Reg_Applicant"
                                        mycmd.CommandType = CommandType.StoredProcedure
                                        mycmd.Parameters.Clear()
                                        mycmd.Parameters.Add("par_reg_no", NpgsqlTypes.NpgsqlDbType.Varchar, 34).Value = ddlReg.SelectedValue
                                        mycmd.Parameters.Add("par_applicant_no", NpgsqlTypes.NpgsqlDbType.Integer).Value = System.Int32.Parse(ApplicantNo)
                                        mycmd.Parameters.Add("par_applicant_name", NpgsqlTypes.NpgsqlDbType.Varchar, 50).Value = ApplicantName
                                        mycmd.Parameters.Add("par_entry_by", NpgsqlTypes.NpgsqlDbType.Varchar, 50).Value = String.Format("{0}", Session("entry_by")).ToString
                                        mycmd.Parameters.Add("par_event_log_activity", NpgsqlTypes.NpgsqlDbType.Varchar, 20).Value = "del_app"
                                        mycmd.Parameters.Add("par_app_tbl", NpgsqlTypes.NpgsqlDbType.Varchar, 20).Value = Applicants
                                        dal.ExecuteCommand_rowsaffected(mycmd)

                                        'sb.Length = 0
                                        'sb.Append("Update ").Append(Applicants).Append(" set Entry_by=@Entry_by , Entry_Date =@Entry_Date , Event_Flag=@Event_Flag, Event_Reason=@Event_Reason, Event_Date=@Event_Date where Reg_No =@Reg_No and Applicant_No =@Applicant_No").ToString()
                                        'mycmd.CommandType = CommandType.Text
                                        'mycmd.CommandText = sb.ToString
                                        'mycmd.Parameters.Clear()
                                        'mycmd.Parameters.Add("@Entry_by", NpgsqlTypes.NpgsqlDbType.Varchar, 50).Value = Entry_by
                                        'mycmd.Parameters.Add("@Entry_Date", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = Now
                                        'mycmd.Parameters.Add("@Event_Flag", SqlDbType.Char, 1).Value = "D"
                                        'mycmd.Parameters.Add("@Event_Reason", NpgsqlTypes.NpgsqlDbType.Varchar, 50).Value = CType(grdData.Rows(i).FindControl("ddlReason"), DropDownList).SelectedValue
                                        'mycmd.Parameters.Add("@Event_Date", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DeleteDateMMDDYYYY
                                        'mycmd.Parameters.Add("@Reg_No", NpgsqlTypes.NpgsqlDbType.Varchar).Value = ddlReg.SelectedValue
                                        'mycmd.Parameters.Add("@Applicant_No", NpgsqlTypes.NpgsqlDbType.Integer).Value = ApplicantNo
                                        'dal.ExecuteCommand_rowsaffected(mycmd)


                                        mycmd.CommandText = "spcommon_delapp"
                                        mycmd.CommandType = CommandType.StoredProcedure
                                        mycmd.Parameters.Clear()
                                        mycmd.Parameters.Add("p_tbl_name", NpgsqlTypes.NpgsqlDbType.Varchar, 75).Value = Applicants

                                        mycmd.Parameters.Add("p_entry_by", NpgsqlTypes.NpgsqlDbType.Varchar, 50).Value = Entry_by
                                        mycmd.Parameters.Add("p_entry_date", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = Now
                                        mycmd.Parameters.Add("p_event_reason", NpgsqlTypes.NpgsqlDbType.Varchar, 50).Value = CType(grdData.Rows(i).FindControl("ddlReason"), DropDownList).SelectedValue
                                        mycmd.Parameters.Add("p_event_date", NpgsqlTypes.NpgsqlDbType.Varchar).Value = DeleteDateMMDDYYYY
                                        mycmd.Parameters.Add("p_reg_no", NpgsqlTypes.NpgsqlDbType.Varchar).Value = ddlReg.SelectedValue
                                        mycmd.Parameters.Add("p_applicant_no", NpgsqlTypes.NpgsqlDbType.Integer).Value = System.Int32.Parse(ApplicantNo)

                                        mycmd.Parameters.Add("p_sp_no", NpgsqlTypes.NpgsqlDbType.Integer).Value = 6
                                        dal.ExecuteCommand_rowsaffected(mycmd)


                                    End Using
                                    lblmsg.Visible = True
                                    lblmsg.ForeColor = Drawing.Color.Green
                                    sb.Length = 0
                                    If i = 0 Then
                                        lblmsg.Text = sb.Append("The Applicant '").Append(ApplicantName).Append("' of the Reg-No. '").Append(ddlReg.SelectedValue).Append("' has been marked as DELETED with effect from ").Append(DeletionDate).Append("").ToString
                                    Else
                                        If lblmsg.Text.Contains("has been marked as DELETED with effect from") Or lblmsg.Text.Contains("has been DELETED PERMANENTLY !") Then
                                            lblmsg.Text = sb.Append(lblmsg.Text + "<br/>" + "The Applicant '").Append(ApplicantName).Append("' of the Reg-No. '").Append(ddlReg.SelectedValue).Append("' has been marked as DELETED with effect from ").Append(DeletionDate).Append("").ToString
                                        Else
                                            lblmsg.Text = sb.Append("The Applicant '").Append(ApplicantName).Append("' of the Reg-No. '").Append(ddlReg.SelectedValue).Append("' has been marked as DELETED with effect from ").Append(DeletionDate).Append("").ToString
                                        End If
                                    End If
                                    CType(grdData.Rows(i).FindControl("chkDelete"), CheckBox).Enabled = False
                                    CType(grdData.Rows(i).FindControl("ddlReason"), DropDownList).Enabled = False
                                    CType(grdData.Rows(i).FindControl("txtDate"), TextBox).ReadOnly = True
                                    CType(grdData.Rows(i).FindControl("txtDate"), TextBox).Enabled = False
                                    CType(grdData.Rows(i).FindControl("txtName"), TextBox).Enabled = False

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
                        End If

                    End If
                End If
            Next
            If DelFlagClicked = "" Then
                lblmsg.Visible = True
                lblmsg.Text = "No Applicant has been deleted as Delete checkbox is NOT Ticked !"
                Return
            End If
        Catch ex1 As SqlException
            lblmsg.Visible = True
            lblmsg.Text = "DB-Error found while Deleting Applicant's Record."
            myutil.Exception_log(ex1)
        Catch ex2 As NullReferenceException
            lblmsg.Visible = True
            lblmsg.Text = "Null-Error found while Deleting Applicant's Record."
            myutil.Exception_log(ex2)
        Catch ex As Exception
            lblmsg.Visible = True
            lblmsg.Text = "Error found while Deleting Applicant's Record."
            myutil.Exception_log(ex)
        End Try
    End Sub

    Protected Sub chkChange_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkChange.CheckedChanged
        If (chkChange.Checked = True) Then
            txtHead.ReadOnly = False
            txtFatHus.ReadOnly = False
            txtHead.Focus()
        Else
            txtHead.ReadOnly = True
            txtFatHus.ReadOnly = True
        End If
    End Sub
    Protected Sub grdData_PageIndexChanging(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewPageEventArgs) Handles grdData.PageIndexChanging
        grdData.PageIndex = e.NewPageIndex
        ddlReg_SelectedIndexChanged(sender, e)
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
            lblmsg.Text = "DB-Error found in block binding."
        Catch ex As Exception
            lblmsg.Visible = True
            lblmsg.Text = "Error found in block binding."
        End Try
    End Sub
    Protected Sub panch_bind(ByVal Block_Code As String)
        Try
            'sb.Length = 0
            'dbSelectCommand = sb.Append("Execute Display_Panchayats @Block_Code = '").Append(Block_Code).Append("', @finyr='").Append(finyear).Append("'").ToString
            'DT1 = dal.ExecuteCommand_dt(New NpgsqlCommand(dbSelectCommand))

            Using cmd As New NpgsqlCommand
                cmd.CommandText = "Display_Panchayats"
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("par_block_code", Block_Code)
                cmd.Parameters.AddWithValue("par_finyr", finyear)
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
            lblmsg.Text = "DB-Error found in panchayat binding."
        Catch ex As Exception
            lblmsg.Visible = True
            lblmsg.Text = "Error found in panchayat binding."
        End Try
    End Sub
    Protected Sub ddlBlock_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ddlBlock.SelectedIndexChanged
        Try
            ddlReg.Items.Clear()
            ddlpnch.Items.Clear()
            ddlvillage.Items.Clear()
            Warning.Visible = False
            change.Visible = False
            note.Visible = False
            tblDetails.Visible = False
            lblNote.Visible = False
            grdData.DataSource = DT1
            grdData.DataBind()
            BtnDelete.Visible = False
            BtnCancel.Visible = False
            If ddlBlock.SelectedIndex > 0 Then
                panch_bind(ddlBlock.SelectedValue)
            End If
        Catch ex As Exception
            lblmsg.Visible = True
            lblmsg.Text = "Error found in block changing."
        End Try
    End Sub
    Protected Sub grdData_RowCreated(sender As Object, e As GridViewRowEventArgs) Handles grdData.RowCreated
        If Session("State_Code_d") = "32" Then
            grdData.PageSize = 20
        Else
            grdData.PageSize = 15
        End If
    End Sub
End Class
