Imports System
Imports System.Web
Imports System.Data
Imports System.Data.SqlClient.SqlConnection
Imports System.Data.SqlClient
Imports System.Threading
Imports System.Globalization
Imports Npgsql
Imports NpgsqlTypes
Partial Class UpdFRA_beneficiary_Dtl
    Inherits System.Web.UI.Page
    Dim DT1, DT2 As DataTable
    Dim dt As Array
    Dim dal As DAL_VB
    Dim cmd As NpgsqlCommand

    Public HomePage As String
    Dim State_Code, state_name, district_name, District_Code, Level, block_name, Block_Code, finyear, Home, Panchayat_Name, Panchayat_Code, Entry_by, reg, Applicants, dbSelectCommand As String
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try

            If Session("State_Code_d") = "" Or Session("Entry_type") <> "D" Or Session("finyear_d") = "" Then
                Server.Transfer("logout.aspx")
            End If
            If (Session("Exel_d") <> "PO" And Session("Exel_d") <> "GP") Then
                Response.Write("This Option is only at GP  and PO login level.")
                Response.End()
            End If

            State_Code = Session("State_Code_d")
            District_Code = Session("District_Code_d")
            Block_Code = Session("Block_Code_d")
            state_name = Session("State_Name_d")
            district_name = Session("District_Name_d")
            block_name = Session("Block_Name_d")
            finyear = Session("finyear_d")
            Entry_by = Session("Entry_by")
            lblmsg.Visible = False

            If Session("Exel_d") = "GP" Then
                Panchayat_Code = Session("Panchayat_Code_d")
                Panchayat_Name = Session("Panchayat_Name_d")
                HomePage = "IndexFrame2.aspx?Panchayat_Code=" & Panchayat_Code
                Level = "GP"
            Else
                Panchayat_Code = ddlpnch.SelectedValue
                HomePage = "ProgOfficer/PoIndexFrame2.aspx?Block_Code=" & Block_Code
                Level = "BP"
            End If
            Session("HomePage") = HomePage
            If Len(Panchayat_Code) = 10 Then
                reg = Session("short_name") & Mid(Trim(Panchayat_Code), 3, 2) & "Registration"
                Applicants = Session("short_name") & Mid(Panchayat_Code, 3, 2) & "Applicants"
            End If
            dal = DAL_VB.GetInstanceforDE()
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

                cmd = New NpgsqlCommand
                Dim Lgn As Lang_Labels
                Lgn = New Lang_Labels(Session("State_Code_d"), "3")
                Dim Ht As Hashtable = New Hashtable
                Ht = Lgn.get_langs(Session("State_Code_d"), "3")
                Dim k As Integer = 0
                lblStatetxt.Text = Ht.Item("state") & ":"
                lblDisttxt.Text = Ht.Item("dist") & ":"
                lblBlocktxt.Text = Ht.Item("blk") & ":"
                lblPanchayattxt.Text = Ht.Item("panch") & ":"
                lblVilltxt.Text = Ht.Item("vill") & ":"
                lblState.Text = state_name
                lblDistrict.Text = district_name
                lblBlk.Text = block_name
                ViewState("SelectPanch") = Ht.Item("selectpanch")
                ViewState("SelectVill") = Ht.Item("selectvill")

                If District_Code = "1601" Or District_Code = "1609" Then
                    lblLanguage.Visible = True
                    Rdlst.Items.Add(New ListItem("Malayalam", "ML-NILA01"))
                    If District_Code = "1609" Then
                        Rdlst.Items.Add(New ListItem("Tamil", "KL-LATHA"))
                    Else
                        Rdlst.Items.Add(New ListItem("Kannad", "KL-TUNGA"))
                    End If
                    Rdlst.Items(0).Selected = True

                    dbSelectCommand = "Execute getStDtBlkPanName_KL @SDBP_Flag='S', @SDBP_Code = '" & State_Code & "', @Language='" & Rdlst.SelectedValue & "'"
                    'DT1 = CType(DBConnection.ExecuteQuery(dbSelectCommand), DataTable)
                    cmd.CommandText = dbSelectCommand
                    DT1 = dal.ExecuteCommand_dt(cmd)
                    If DT1.Rows.Count > 0 Then
                        lblState.Text = Convert.ToString(DT1.Rows(0)("St_Local_Name"))
                        dbSelectCommand = "Execute getStDtBlkPanName_KL @SDBP_Flag='D', @SDBP_Code = '" & District_Code & "', @Language='" & Rdlst.SelectedValue & "'"
                        cmd.CommandText = dbSelectCommand
                        'DT1 = CType(DBConnection.ExecuteQuery(dbSelectCommand), DataTable)
                        DT1 = dal.ExecuteCommand_dt(cmd)
                        lblDistrict.Text = Convert.ToString(DT1.Rows(0)("Dt_Name_Local"))
                        dbSelectCommand = "Execute getStDtBlkPanName_KL @SDBP_Flag='B', @SDBP_Code = '" & Block_Code & "', @Language='" & Rdlst.SelectedValue & "'"
                        cmd.CommandText = dbSelectCommand
                        'DT1 = CType(DBConnection.ExecuteQuery(dbSelectCommand), DataTable)
                        DT1 = dal.ExecuteCommand_dt(cmd)
                        lblBlk.Text = Convert.ToString(DT1.Rows(0)("Blk_Name_Local"))
                        lblState.Text = state_name
                        lblDistrict.Text = district_name
                        lblBlk.Text = block_name
                    End If
                End If
                If Level = "BP" Then

                    dbSelectCommand = "Call Display_Panchayats('" & Block_Code & "', '" & finyear & "');"
                    cmd.CommandText = dbSelectCommand
                    DT1 = dal.ExecuteCommand_dt(cmd, "ref_cursor")
                    ddlpnch.DataSource = DT1
                    ddlpnch.DataTextField = "Panch_Name_Local"
                    ddlpnch.DataValueField = "Panchayat_Code"
                    ddlpnch.DataBind()
                    ddlpnch.Items.Insert(0, ViewState("SelectPanch"))
                    lblMandry.Visible = True
                    ddlpnch.Visible = True
                    lblPnch.Visible = False
                    ddlvillage.ClearSelection()
                Else
                    If District_Code = "1601" Or District_Code = "1609" Then
                        dbSelectCommand = "Execute getStDtBlkPanName_KL @SDBP_Flag='P', @SDBP_Code = '" & Panchayat_Code & "', @Language='" & Rdlst.SelectedValue & "'"
                        'DT1 = CType(DBConnection.ExecuteQuery(dbSelectCommand), DataTable)
                        cmd.CommandText = dbSelectCommand
                        DT1 = dal.ExecuteCommand_dt(cmd)
                        lblPnch.Text = DT1.Rows(0)("Panch_Name_Local").ToString()
                        'hf_PanchayatCode.Value = DT1.Rows(0)("Panchayat_Code").ToString()
                    Else
                        lblPnch.Text = Panchayat_Name
                    End If
                    'If State_Code = "16" Then
                    'dbSelectCommand = "Execute cboVillageKL @Panchayat_Code = '" & Panchayat_Code & "'"
                    'Else
                    dbSelectCommand = "Execute Display_Villages @Panchayat_Code = '" & Panchayat_Code & "', @finyr='" & finyear & "'"
                    'End If
                    'DT1 = CType(DBConnection.ExecuteQuery(dbSelectCommand), DataTable)
                    cmd.CommandText = dbSelectCommand
                    DT1 = dal.ExecuteCommand_dt(cmd)

                    If State_Code = "16" And ddlpnch.SelectedValue <> ViewState("SelectPanch") Then
                        For Each drow As DataRow In DT1.Rows
                            ddlvillage.Items.Add(New ListItem(Mid((drow("Village_Code")), 11, 3) & "-" & drow("Village_Name_Local"), drow("village_Code")))
                        Next
                        ddlvillage.DataBind()
                    ElseIf Panchayat_Code <> "" And State_Code <> "16" Then
                        ddlvillage.DataSource = DT1
                        ddlvillage.DataTextField = "Village_Name_Local"
                        ddlvillage.DataValueField = "village_Code"
                        ddlvillage.DataBind()
                        ddlvillage.Items.Insert(0, ViewState("SelectVill"))
                    End If
                End If
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

        Catch ex As Exception
            lblmsg.Visible = True
            lblmsg.Text = "Error in Loading Page"
        End Try
        'DT1 = New DataTable()
    End Sub
    Protected Sub BtnUpdate_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles BtnUpdate.Click
        Try
            Dim Flag As String = ""
            Dim Reg_No, S_No As String
            lblmsg.Visible = False
            lblmsg.Text = ""
            lblErrMsg.Text = False
            lblErrMsg.Text = ""
            Dim TickToEdit As Integer = 0
            Dim NoOfValidEntry As Integer = 0
            Dim FRA_titleD, pvt_land, FRA_bene As String
            Dim updatefld As String = String.Empty
            Dim land_acre As Single
            cmd = New NpgsqlCommand
            For i = 0 To grdData.Rows.Count - 1
                If (CType(grdData.Rows(i).FindControl("chkRowEdit"), CheckBox).Checked) Then
                    Reg_No = Trim(CType(grdData.Rows(i).FindControl("HF_HeadRegNo"), HiddenField).Value)
                    If Reg_No <> "" Then
                        TickToEdit = TickToEdit + 1
                        S_No = Trim(CType(grdData.Rows(i).FindControl("lblSno"), Label).Text)
                        If Trim(CType(grdData.Rows(i).FindControl("rbl_FRA"), RadioButtonList).SelectedValue) = "Y" Then
                            If Trim(CType(grdData.Rows(i).FindControl("txtFRA_title"), TextBox).Text) = "" Or Trim(CType(grdData.Rows(i).FindControl("txtFRA_Acre"), TextBox).Text) = "" Or Trim(CType(grdData.Rows(i).FindControl("rdb_card"), RadioButtonList).SelectedIndex) = -1 Then
                                lblmsg.Text = "FRA title deed no.,ext of land in acre and holding pvt land all fields are mandatory! for SNo. " & i + 1 & ""
                                lblmsg.Visible = True
                                Exit Sub
                            End If

                            FRA_bene = Trim(CType(grdData.Rows(i).FindControl("rbl_FRA"), RadioButtonList).SelectedValue)
                            FRA_titleD = Trim(CType(grdData.Rows(i).FindControl("txtFRA_title"), TextBox).Text)
                            land_acre = Trim(CType(grdData.Rows(i).FindControl("txtFRA_Acre"), TextBox).Text)

                            'dbSelectCommand = "select reg_no from " & reg & " where FRA_title_deed_no =@FRA_title_deed_no and reg_no <> @reg_no"
                            'cmd.CommandText = dbSelectCommand

                            cmd.CommandText = "SPCommon_UpdFRA_beneficiary_Dtl"
                            cmd.CommandType = CommandType.StoredProcedure
                            cmd.Parameters.AddWithValue("par_fra_titled", NpgsqlDbType.Varchar).Value = FRA_titleD
                            cmd.Parameters.AddWithValue("par_reg_no", Reg_No)
                            cmd.Parameters.AddWithValue("par_table_name", reg)
                            cmd.Parameters.AddWithValue("par_sp_no", 1)


                            'cmd.Parameters.Add("@FRA_titleD", SqlDbType.NVarChar).Value = FRA_titleD
                            'cmd.Parameters.AddWithValue("@reg_no", Reg_No)
                            'cmd.Parameters.AddWithValue("@Table_name", reg)
                            'cmd.Parameters.AddWithValue("@Sp_No", 1)
                            DT1 = dal.ExecuteCommand_dt(cmd, "ref_cursor")

                            If (DT1.Rows.Count > 0) Then
                                lblmsg.Text = "Error: FRA Title deed no. already exists for another reg_no:'" & DT1.Rows(0)("reg_no") & "' for S.No. " & i + 1 & ""
                                lblmsg.Visible = True
                                Exit Sub
                            End If

                            If Not IsNumeric(land_acre) Then
                                lblmsg.ForeColor = Drawing.Color.Red
                                lblmsg.Text = "Error: Extension of land in Acre should be valid NUMERIC data for S.No. " & i + 1 & ""
                                lblmsg.Visible = True
                                Exit Sub
                            End If

                            pvt_land = Trim(CType(grdData.Rows(i).FindControl("rdb_card"), RadioButtonList).SelectedValue)
                        ElseIf Trim(CType(grdData.Rows(i).FindControl("rbl_FRA"), RadioButtonList).SelectedValue) = "N" Then
                            FRA_bene = Trim(CType(grdData.Rows(i).FindControl("rbl_FRA"), RadioButtonList).SelectedValue)

                        Else
                            lblmsg.ForeColor = Drawing.Color.Red
                            lblmsg.Text = "Error: FRA Beneficiary NOT selected for S.No. " & i + 1 & ""
                            lblmsg.Visible = True
                            Exit Sub
                        End If

                        'Dim RecUpdated As Integer = 0
                        'dbSelectCommand = "Update " & reg & " set "
                        'dbSelectCommand = dbSelectCommand & " FRA_Beneficiary = '" & FRA_bene & "',"
                        'If Trim(CType(grdData.Rows(i).FindControl("rbl_FRA"), RadioButtonList).SelectedValue) = "Y" Then
                        '    dbSelectCommand = dbSelectCommand & " FRA_title_deed_no = N'" & FRA_titleD & "',"
                        '    dbSelectCommand = dbSelectCommand & " Ext_of_land_in_acre = " & land_acre & ","
                        '    dbSelectCommand = dbSelectCommand & " holding_pvt_land = '" & pvt_land & "'"
                        'ElseIf Trim(CType(grdData.Rows(i).FindControl("rbl_FRA"), RadioButtonList).SelectedValue) = "N" Then
                        '    dbSelectCommand = dbSelectCommand & " FRA_title_deed_no = NULL,"
                        '    dbSelectCommand = dbSelectCommand & " Ext_of_land_in_acre = NULL,"
                        '    dbSelectCommand = dbSelectCommand & " holding_pvt_land = NULL"
                        'End If
                        'dbSelectCommand = dbSelectCommand & " where Reg_No = '" & Reg_No & "'"
                        'RecUpdated = DBConnection.ExecuteNonQueryWithRecUpdated(dbSelectCommand)

                        Dim RecUpdated As Integer = 0

                        If Trim(CType(grdData.Rows(i).FindControl("rbl_FRA"), RadioButtonList).SelectedValue) = "Y" Then
                            Flag = "A"
                            'updatefld = "FRA_Beneficiary =''" & FRA_bene & "'', FRA_title_deed_no = N''" & FRA_titleD & "'', Ext_of_land_in_acre =''" & land_acre & "'' , holding_pvt_land = ''" & pvt_land & "''"
                        ElseIf Trim(CType(grdData.Rows(i).FindControl("rbl_FRA"), RadioButtonList).SelectedValue) = "N" Then
                            Flag = "B"
                            'updatefld = "FRA_Beneficiary =''" & FRA_bene & "'' , FRA_title_deed_no=NULL , Ext_of_land_in_acre = NULL, holding_pvt_land = NULL  "
                        End If

                        'dbSelectCommand = "EXECUTE Update_table @table='" & reg & "' , @condition1=N'" & updatefld & "' , @condition='Reg_No=''" & Reg_No & "''' "
                        'cmd.CommandText = dbSelectCommand

                        cmd.CommandText = "spcommon_updfra_beneficiary_dtltest"
                        cmd.CommandType = CommandType.StoredProcedure
                        cmd.Parameters.Clear()

                        cmd.Parameters.AddWithValue("par_table_name", reg)
                        cmd.Parameters.AddWithValue("par_fra_bene", FRA_bene)
                        cmd.Parameters.AddWithValue("par_fra_titled", FRA_titleD)
                        cmd.Parameters.AddWithValue("par_land_acre", land_acre)
                        cmd.Parameters.AddWithValue("par_pvt_land", pvt_land)
                        cmd.Parameters.AddWithValue("par_reg_no", Reg_No)
                        cmd.Parameters.AddWithValue("par_flag", Flag)
                        cmd.Parameters.AddWithValue("par_sp_no", 2)


                        'cmd.Parameters.AddWithValue("@Table_name", reg)
                        'cmd.Parameters.AddWithValue("@FRA_bene", FRA_bene)
                        'cmd.Parameters.AddWithValue("@FRA_titleD", FRA_titleD)
                        'cmd.Parameters.AddWithValue("@land_acre", land_acre)
                        'cmd.Parameters.AddWithValue("@pvt_land", pvt_land)
                        'cmd.Parameters.AddWithValue("@reg_no", Reg_No)
                        'cmd.Parameters.AddWithValue("@Flag", Flag)
                        'cmd.Parameters.AddWithValue("@Sp_No", 2)

                        RecUpdated = dal.ExecuteCommand_rowsaffected(cmd)

                        If RecUpdated = 0 Then
                            lblErrMsg.Visible = True
                            lblErrMsg.Text += "...record NOT updated for S.No.: " & S_No & "<br>"
                        Else
                            NoOfValidEntry = NoOfValidEntry + 1
                            lblmsg.Visible = True
                            lblmsg.ForeColor = Drawing.Color.Blue
                            lblmsg.Text += S_No & ","
                        End If

                        grdData.Visible = True
                        msgUpd.Visible = True
                        BtnUpdate.Visible = True
                        BtnCancel.Visible = True
                        'trMore.Visible = True
                        hf_VillageCode.Value = ddlvillage.SelectedValue
                        'More = "UpdBPL_RSBY.aspx"
                    Else
                        grdData.Visible = True
                        msgUpd.Visible = True
                        BtnCancel.Visible = True
                        BtnUpdate.Visible = True
                        lblmsg.Text = "Registration No. for the Serial No " & i + 1 & " is not available ! "
                        lblmsg.Visible = True
                    End If
                End If
            Next
            If NoOfValidEntry = 0 And TickToEdit > 0 Then
                lblmsg.Visible = True
                lblmsg.Text = " No record updated ! " & "<br>"
                lblmsg.ForeColor = Drawing.Color.Red
            ElseIf (TickToEdit = 0) Then
                lblmsg.Visible = True
                lblmsg.Text = " No record is ticked to be updated ! " & "<br>"
                lblmsg.ForeColor = Drawing.Color.Red
            Else
                lblmsg.Text = NoOfValidEntry.ToString & " record(s) updated ! (S.No: " & lblmsg.Text & ")<br>"
                lblmsg.ForeColor = Drawing.Color.Blue
            End If
        Catch ex As Exception
            lblmsg.Text = "Error while Updating Records ! "
            lblmsg.Visible = True
        End Try
    End Sub
    Protected Sub BtnCancel_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles BtnCancel.Click
        Response.Redirect(Request.Url.ToString())
    End Sub
    Protected Sub ddlpnch_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlpnch.SelectedIndexChanged
        Try
            ddlvillage.Items.Clear()
            jcr_srch_key.Text = String.Empty
            grdData.DataSource = DT1
            grdData.DataBind()
            BtnUpdate.Visible = False
            BtnCancel.Visible = False
            'If State_Code = "16" Then
            'dbSelectCommand = "Execute cboVillageKL_Language @Panchayat_Code = '" & ddlpnch.SelectedValue & "', @Language='" & Rdlst.SelectedValue & "'"
            'Else
            dbSelectCommand = "CALL Display_Villages ('" & ddlpnch.SelectedValue & "', '" & finyear & "')"
            'End If
            'hf_PanchayatCode.Value = ddlpnch.SelectedValue
            'DT1 = CType(dbConnection.ExecuteQuery(dbSelectCommand), DataTable)
            cmd = New NpgsqlCommand(dbSelectCommand)
            DT1 = dal.ExecuteCommand_dt(cmd, "ref_cursor")
            If State_Code = "16" Then
                ddlvillage.Items.Insert(0, ViewState("SelectVill"))
                For Each drow As DataRow In DT1.Rows
                    ddlvillage.Items.Add(New ListItem(Mid((drow("Village_Code")), 11, 3) & "-" & drow("Village_Name_Local"), drow("village_Code")))
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
                    lblmsg.Text = "No Village Found for the selected Panchayat ! "
                    Return
                End If
            End If
        Catch ex As Exception
            lblmsg.Visible = True
            lblmsg.Text = "Error in Binding...Village"
            Return
        End Try
    End Sub
    Protected Sub ddlvillage_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlvillage.SelectedIndexChanged
        Try
            Dim S_No As String
            Dim search_fam_id As String = ""

            search_fam_id = Replace(LTrim(RTrim(jcr_srch_key.Text)), vbTab, "")
            If ddlvillage.SelectedValue <> ViewState("SelectVill") Then

                'Dim sconditions As String = String.Empty
                'sconditions = "Village_Code=''" & Trim(ddlvillage.SelectedValue) & "'' and  (Event_Flag is NULL or Event_Flag<>''D'') "
                'Dim Is_search = ""
                'If search_fam_id <> String.Empty Then
                '    sconditions = sconditions & "  and Reg_No like ''%/" & search_fam_id & "'' "
                '    Is_search = "Y"
                'End If
                'sconditions = sconditions & " order by CONVERT(BIGINT, LEFT(SUBSTRING(Family_Id, CHARINDEX(''/'', Family_Id) + 1, LEN(Family_Id)), PATINDEX(''%[^0-9]%'', SUBSTRING(Family_Id, CHARINDEX(''/'',Family_Id) + 1, LEN(Family_Id)) + '' '') - 1)) "
                'dbSelectCommand = "Execute check_for_entry_1  @Tablename='" & reg & "' , @field_name='Reg_No, Head_of_Household, Caste,isnull(FRA_Beneficiary,'''') as FRA_Beneficiary, isnull(FRA_title_deed_no,'''') as FRA_title_deed_no,isnull(Ext_of_land_in_acre,'''') as Ext_of_land_in_acre,isnull(holding_pvt_land,'''') as holding_pvt_land' , @Condition=N'" & sconditions & "'"

                'cmd = New NpgsqlCommand(dbSelectCommand)

                Using cmd = New NpgsqlCommand()
                    cmd.CommandText = "SPCommon_UpdFRA_beneficiary_Dtl"
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("par_table_name", NpgsqlTypes.NpgsqlDbType.Varchar, reg)
                    If search_fam_id <> String.Empty Then
                        cmd.Parameters.AddWithValue("par_search_fam_id", NpgsqlTypes.NpgsqlDbType.Varchar, search_fam_id)
                    End If
                    cmd.Parameters.AddWithValue("par_vill_code", NpgsqlTypes.NpgsqlDbType.Varchar, Trim(ddlvillage.SelectedValue))
                    cmd.Parameters.AddWithValue("par_sp_no", NpgsqlTypes.NpgsqlDbType.Integer, 3)
                    DT1 = dal.ExecuteCommand_dt(cmd, "ref_cursor")
                End Using


                If DT1.Rows.Count > 0 Then
                    grdData.DataSource = DT1
                    grdData.DataBind()
                    HF_countrow.Value = grdData.Rows.Count
                    Dim count, dtrownumber As Integer
                    count = 1
                    For i = 0 To grdData.Rows.Count - 1
                        dtrownumber = (grdData.PageSize * (grdData.PageIndex)) + count - 1
                        S_No = dtrownumber + 1
                        CType(grdData.Rows(i).FindControl("lblSno"), Label).Text = (grdData.PageSize * (grdData.PageIndex)) + count
                        CType(grdData.Rows(i).FindControl("lblHead"), Label).Text = DT1.Rows(dtrownumber)("Head_Of_Household").ToString()
                        CType(grdData.Rows(i).FindControl("lblRegNo"), Label).Text = "(" + DT1.Rows(dtrownumber)("Reg_No").ToString() + ")"
                        CType(grdData.Rows(i).FindControl("HF_HeadRegNo"), HiddenField).Value = DT1.Rows(dtrownumber)("Reg_No").ToString()
                        If Trim(DT1.Rows(dtrownumber)("Caste").ToString()) = "" Then
                            CType(grdData.Rows(i).FindControl("rdbCat"), RadioButtonList).SelectedValue = "OTH"
                        Else
                            If InStr(DT1.Rows(dtrownumber)("Caste").ToString(), "SC") > 0 Then
                                CType(grdData.Rows(i).FindControl("rdbCat"), RadioButtonList).SelectedValue = "SC"
                            ElseIf InStr(DT1.Rows(dtrownumber)("Caste").ToString(), "ST") > 0 Then
                                CType(grdData.Rows(i).FindControl("rdbCat"), RadioButtonList).SelectedValue = "ST"
                            ElseIf InStr(DT1.Rows(dtrownumber)("Caste").ToString(), "OBC") > 0 Then
                                CType(grdData.Rows(i).FindControl("rdbCat"), RadioButtonList).SelectedValue = "OBC"
                            Else
                                CType(grdData.Rows(i).FindControl("rdbCat"), RadioButtonList).SelectedValue = "OTH"

                            End If
                        End If
                        CType(grdData.Rows(i).FindControl("rdbCat"), RadioButtonList).Enabled = False
                        CType(grdData.Rows(i).FindControl("txtFRA_title"), TextBox).Text = DT1.Rows(dtrownumber)("FRA_title_deed_no")
                        ViewState("txt1_val" & i) = DT1.Rows(dtrownumber)("FRA_title_deed_no")


                        Dim acreText As String = If(IsDBNull(DT1.Rows(dtrownumber)("Ext_of_land_in_acre")), String.Empty, DT1.Rows(dtrownumber)("Ext_of_land_in_acre").ToString())
                        CType(grdData.Rows(i).FindControl("txtFRA_Acre"), TextBox).Text = acreText
                        ViewState("txt2_val" & i) = acreText

                        'CType(grdData.Rows(i).FindControl("txtFRA_Acre"), TextBox).Text = DT1.Rows(dtrownumber)("Ext_of_land_in_acre")
                        'ViewState("txt2_val" & i) = DT1.Rows(dtrownumber)("Ext_of_land_in_acre")


                        Select Case UCase(DT1.Rows(dtrownumber)("holding_pvt_land"))
                            Case "Y"
                                CType(grdData.Rows(i).FindControl("rdb_card"), RadioButtonList).SelectedIndex = 0

                                ViewState("rbl" & i) = 0
                            Case "N"
                                CType(grdData.Rows(i).FindControl("rdb_card"), RadioButtonList).SelectedIndex = 1
                                ViewState("rbl" & i) = 1
                            Case Else
                                CType(grdData.Rows(i).FindControl("rdb_card"), RadioButtonList).SelectedIndex = -1
                                ViewState("rbl" & i) = -1
                        End Select

                        Select Case UCase(DT1.Rows(dtrownumber)("FRA_Beneficiary"))
                            Case "Y"
                                CType(grdData.Rows(i).FindControl("rbl_FRA"), RadioButtonList).SelectedIndex = 0
                                CType(grdData.Rows(i).FindControl("txtFRA_title"), TextBox).Enabled = True
                                CType(grdData.Rows(i).FindControl("txtFRA_Acre"), TextBox).Enabled = True
                                CType(grdData.Rows(i).FindControl("rdb_card"), RadioButtonList).Enabled = True
                            Case "N"
                                CType(grdData.Rows(i).FindControl("rbl_FRA"), RadioButtonList).SelectedIndex = 1
                                CType(grdData.Rows(i).FindControl("txtFRA_title"), TextBox).Text = ""
                                CType(grdData.Rows(i).FindControl("txtFRA_Acre"), TextBox).Text = ""
                                CType(grdData.Rows(i).FindControl("rdb_card"), RadioButtonList).SelectedIndex = -1
                            Case Else
                                CType(grdData.Rows(i).FindControl("rbl_FRA"), RadioButtonList).SelectedIndex = -1
                        End Select


                        count = count + 1
                    Next
                    grdData.Visible = True
                    msgUpd.Visible = True
                    BtnUpdate.Visible = True
                    BtnCancel.Visible = True
                Else
                    lblmsg.Text = "No records found !"
                    lblmsg.Visible = True
                    grdData.Visible = False
                    msgUpd.Visible = False

                    BtnUpdate.Visible = False
                    BtnCancel.Visible = False
                End If
            Else
                grdData.Visible = False
                msgUpd.Visible = False
                BtnUpdate.Visible = False
                BtnCancel.Visible = False
            End If
        Catch ex As Exception
            lblmsg.Visible = True
            lblmsg.Text = "Error in Binding Data....Reg"
        End Try
    End Sub
    Protected Sub LnkMore_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles LnkMore.Click
        ddlvillage_SelectedIndexChanged(sender, e)
        trMore.Visible = False
    End Sub
    Protected Sub grdData_PageIndexChanging(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewPageEventArgs) Handles grdData.PageIndexChanging
        grdData.PageIndex = e.NewPageIndex
        Call ddlvillage_SelectedIndexChanged(sender, e)
    End Sub
    Protected Sub ImgbtnSearch_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs) Handles ImgbtnSearch.Click
        Call ddlvillage_SelectedIndexChanged(sender, e)
    End Sub
    Protected Sub rbl_FRA_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim rblFRA As RadioButtonList = CType(sender, RadioButtonList)
        Dim row As GridViewRow = CType(rblFRA.NamingContainer, GridViewRow)
        'rbl_val = row.RowIndex
        Dim txt1, txt2 As TextBox
        Dim rbl1 As RadioButtonList
        txt1 = CType(row.FindControl("txtFRA_title"), TextBox)
        txt2 = CType(row.FindControl("txtFRA_Acre"), TextBox)
        rbl1 = CType(row.FindControl("rdb_card"), RadioButtonList)
        If (CType(row.FindControl("rbl_FRA"), RadioButtonList).SelectedValue) = "Y" Then
            txt1.Enabled = True
            txt2.Enabled = True
            rbl1.Enabled = True
            txt1.Text = ViewState("txt1_val" & row.RowIndex)
            txt2.Text = ViewState("txt2_val" & row.RowIndex)
            rbl1.SelectedIndex = ViewState("rbl" & row.RowIndex)
        ElseIf (CType(row.FindControl("rbl_FRA"), RadioButtonList).SelectedValue) = "N" Then
            txt1.Text = ""
            txt2.Text = ""
            rbl1.SelectedIndex = -1
            txt1.Enabled = False
            txt2.Enabled = False
            rbl1.Enabled = False
        End If
    End Sub
End Class
