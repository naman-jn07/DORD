Imports System
Imports System.Web
Imports System.Data
Imports System.Data.SqlClient.SqlConnection
Imports System.Data.SqlClient
Imports System.Threading
Imports System.Globalization
Imports System.IO
Imports Npgsql
Imports NpgsqlTypes
Partial Class EditReg
    Inherits System.Web.UI.Page
    Dim DT1, DT2 As DataTable
    Dim Lgn As Lang_Labels
    Dim dal As DAL_VB
    Dim cmd As NpgsqlCommand

    Dim RegDate, dt, Cat As Array
    Dim State_Code, Home, District_Code, Block_Code, dbSelectCommand, Entry_by, Reg_DateMDY, Ration_Card_No, More, AppCount, Applicant_Name, AppInMustroll, shortnm As String
    Dim Photo_File_Name, EntryErrorFound, Registration_Date, Applicant_No, age, Disabled, AppName, gender, Relation_Code, Reg_DateDMY, tblMustrollYYYY As String
    Dim tblWork_DemandYYYY, Work_DemandFY, MustrollFY, Level, reg, AppDemanded, Applicants, state_name, block_name, IAY_benef, LR_benef As String
    Dim district_name, finyear, Panchayat_Code, Panchayat_Name As String
    Dim rec_cnt As Integer
    Dim DT_FY As DataTable
    Public HomePage As String
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            If ((Not IsNothing(Request.UrlReferrer)) AndAlso Request.UrlReferrer.ToString.Contains("substring")) Or Session("State_Code_d") = "" Or Session("finyear_d") = "" Then
                Server.Transfer("logout.aspx")
            End If

            lblmsg.Visible = False
            State_Code = Session("State_Code_d")
            District_Code = Session("District_Code_d")
            Block_Code = Session("Block_Code_d")
            state_name = Session("State_Name_d")
            district_name = Session("District_Name_d")
            block_name = Session("Block_Name_d")
            finyear = Session("finyear_d")
            Entry_by = Session("Entry_by")
            shortnm = Session("short_name")
            Panchayat_Code = Session("Panchayat_Code_d")
            If Trim(Session("ExeL_d")) = "GP" Then
                Panchayat_Code = Session("Panchayat_Code_d")
                Panchayat_Name = Session("Panchayat_Name_d")
                HomePage = "IndexFrame2.aspx"
                Level = "GP"
                District_Code = Left(Panchayat_Code, 4)
                Block_Code = Left(Panchayat_Code, 7)
            ElseIf Trim(Session("ExeL_d")) = "PO" Then
                Panchayat_Code = Trim(hf_PanchayatCode.Value)
                HomePage = "ProgOfficer/PoIndexFrame2.aspx"
                Level = "BP"
                District_Code = Left(Panchayat_Code, 4)
                Block_Code = Left(Panchayat_Code, 7)
            Else
                Response.Write("You are not authorised user to use this option.")
                Response.End()
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

            dal = DAL_VB.GetInstanceforDE()

            reg = shortnm & Right(District_Code, 2) & "Registration"
            Applicants = shortnm & Right(District_Code, 2) & "Applicants"

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

                cmd = New NpgsqlCommand()
                Lgn = New Lang_Labels(Session("state_code_d"), "3")
                Dim Ht As Hashtable = New Hashtable
                Ht = Lgn.get_langs(Session("state_code_d"), "3")
                lblStatetxt.Text = Ht.Item("state") & ":"
                lblDisttxt.Text = Ht.Item("dist") & ":"
                lblBlocktxt.Text = Ht.Item("blk") & ":"
                lblPanchayattxt.Text = Ht.Item("panch") & ":"
                lblVilltxt.Text = Ht.Item("vill") & ":"
                lblRegNo.Text = Ht.Item("regid") & ":"
                ViewState("SelectPanch") = Ht.Item("selectpanch")
                ViewState("SelectVill") = Ht.Item("selectvill")
                ViewState("selectRegNo") = Ht.Item("selectregno")

                DT1 = New DataTable
                If District_Code = "1601" Or District_Code = "1609" Then
                    trLanguage.Visible = True
                    lblLanguage.Visible = True
                    Rdlst.Items.Add(New ListItem("Malayalam", "ML-NILA01"))

                    If District_Code = "1609" Then
                        Rdlst.Items.Add(New ListItem("Tamil", "KL-LATHA"))
                    Else
                        Rdlst.Items.Add(New ListItem("Kannad", "KL-TUNGA"))
                    End If

                    Rdlst.Items(0).Selected = True
                    dbSelectCommand = "Execute getStDtBlkPanName_KL @SDBP_Flag='S', @SDBP_Code = '" & State_Code & "', @Language='" & Rdlst.SelectedValue & "'"
                    'DT1 = CType(dbConnection.ExecuteQuery(dbSelectCommand), DataTable)
                    cmd.CommandText = dbSelectCommand
                    DT1 = dal.ExecuteCommand_dt(cmd)

                    If DT1.Rows.Count > 0 Then
                        state_name = DT1.Rows(0)("St_Local_Name")
                        dbSelectCommand = "Execute getStDtBlkPanName_KL @SDBP_Flag='D', @SDBP_Code = '" & District_Code & "', @Language='" & Rdlst.SelectedValue & "'"
                        'DT1 = CType(dbConnection.ExecuteQuery(dbSelectCommand), DataTable)
                        cmd.CommandText = dbSelectCommand
                        DT1 = dal.ExecuteCommand_dt(cmd)
                        district_name = DT1.Rows(0)("Dt_Name_Local")

                        dbSelectCommand = "Execute getStDtBlkPanName_KL @SDBP_Flag='B', @SDBP_Code = '" & Block_Code & "', @Language='" & Rdlst.SelectedValue & "'"
                        'DT1 = CType(dbConnection.ExecuteQuery(dbSelectCommand), DataTable)
                        cmd.CommandText = dbSelectCommand
                        DT1 = dal.ExecuteCommand_dt(cmd)
                        block_name = DT1.Rows(0)("Blk_Name_Local")
                    End If
                End If
                lblState.Text = state_name
                lblDistrict.Text = district_name
                lblBlk.Text = block_name
                If Level = "BP" Then

                    dbSelectCommand = "Display_Panchayats" ' @Block_Code = '" & Block_Code & "', @finyr='" & finyear & "'"
                    cmd.CommandText = dbSelectCommand
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("par_block_code", NpgsqlDbType.Varchar, Block_Code)
                    cmd.Parameters.AddWithValue("par_finyr", NpgsqlDbType.Varchar, finyear)

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
                    If District_Code = "1601" Or District_Code = "1609" Then  ' KL-Kasargod or Idukki 
                        dbSelectCommand = "Execute getStDtBlkPanName_KL @SDBP_Flag='P', @SDBP_Code = '" & Panchayat_Code & "', @Language='" & Rdlst.SelectedValue & "'"
                        'DT1 = CType(dbConnection.ExecuteQuery(dbSelectCommand), DataTable)
                        cmd.CommandText = dbSelectCommand
                        DT1 = dal.ExecuteCommand_dt(cmd)
                        If DT1.Rows.Count > 0 Then
                            Panchayat_Name = DT1.Rows(0)("Panch_Name_Local")
                        End If
                    End If
                    lblPnch.Text = Panchayat_Name
                    hf_PanchayatCode.Value = Panchayat_Code
                End If

                If Not Panchayat_Code Is Nothing Then
                    cmd = New NpgsqlCommand()
                    dbSelectCommand = "Display_Villages"   ' @Panchayat_Code = '" & Panchayat_Code & "', @finyr='" & finyear & "'"
                    cmd.CommandText = dbSelectCommand
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("par_panchayat_code", NpgsqlDbType.Varchar, Panchayat_Code)
                    cmd.Parameters.AddWithValue("par_finyr", NpgsqlDbType.Varchar, finyear)

                    DT1 = dal.ExecuteCommand_dt(cmd, "ref_cursor")

                    If State_Code = "16" And ddlpnch.SelectedValue <> ViewState("SelectPanch") Then
                        ' hf_PanchayatCode.Value = DT1.Rows(0)("Panchayat_Code").ToString()
                        ddlvillage.Items.Insert(0, ViewState("SelectVill"))
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

                If Session("Details") <> "" Then
                        Dim str As String
                        Dim st As Array
                        str = Session("Details")
                        st = str.Split(";")
                        If ddlpnch.Visible = True Then
                            ddlpnch.SelectedValue = st(0)
                            ddlpnch_SelectedIndexChanged(sender, e)
                            'ddlvillage_SelectedIndexChanged1(sender, e)
                        Else
                            lblPnch.Text = st(2)

                        End If
                        ddlvillage.SelectedValue = st(1)
                        ddlvillage_SelectedIndexChanged1(sender, e)
                        'ddlReg_SelectedIndexChanged(sender, e)
                    End If

                    'dbSelectCommand = "select Financial_Year from fin_year_temp (nolock) order by YEAR_code desc"
                    'cmd.CommandText = dbSelectCommand
                    'cmd.Parameters.Clear()
                    'DT1 = dal.ExecuteCommand_dt(cmd)
                    'If DT1.Rows.Count > 0 Then
                    '    ViewState("Financial_Year") = DT1
                    'End If

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
        Catch ex As Exception
            lblmsg.Visible = True
            lblmsg.Text = "Error in Loading Page"
            Return
        End Try
    End Sub

    Protected Sub ddlpnch_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlpnch.SelectedIndexChanged
        Try
            ddlReg.Items.Clear()
            ddlvillage.Items.Clear()
            tblDetails.Visible = False
            grdData.DataSource = DT1
            grdData.DataBind()
            BtnUpdate.Visible = False
            BtnCancel.Visible = False
            'If State_Code = "16" Then
            'dbSelectCommand = "Execute cboVillageKL_Language @Panchayat_Code = '" & ddlpnch.SelectedValue & "', @Language='" & Rdlst.SelectedValue & "'"
            'Else

            'dbSelectCommand = "Execute Display_Villages @Panchayat_Code = '" & ddlpnch.SelectedValue & "', @finyr='" & finyear & "'"
            'End If
            hf_PanchayatCode.Value = ddlpnch.SelectedValue
            Session("Panchayat_Code_d") = hf_PanchayatCode.Value
            'DT1 = CType(dbConnection.ExecuteQuery(dbSelectCommand), DataTable)
            Using cmd As New NpgsqlCommand
                dbSelectCommand = "Display_Villages"
                cmd.CommandText = dbSelectCommand
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("par_panchayat_code", NpgsqlDbType.Varchar, ddlpnch.SelectedValue)
                cmd.Parameters.AddWithValue("par_finyr", NpgsqlDbType.Varchar, finyear)

                DT1 = dal.ExecuteCommand_dt(cmd, "ref_cursor")
            End Using


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
                    lblmsg.Text = "No Panchayat found for the selected Block !"
                    Return
                End If
            End If
        Catch ex As Exception
            lblmsg.Visible = True
            lblmsg.Text = "Error in Binding Panchayat"
            Return
        End Try
    End Sub
    Protected Sub ddlvillage_SelectedIndexChanged1(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlvillage.SelectedIndexChanged
        If Session("Details") = "" Then
            ddlReg.ClearSelection()
            ddlReg.Items.Clear()
            tblDetails.Visible = False
            grdData.DataSource = DT1
            grdData.DataBind()
            BtnUpdate.Visible = False
            BtnCancel.Visible = False
        End If
        Try
            If (ddlvillage.SelectedItem.Text <> ViewState("SelectVill") And Session("Details") = "") Or (Session("Details") <> "") Then
                'dbSelectCommand = "" ' @tblReg = '" & reg & "', @Village_Code='" & ddlvillage.SelectedValue & "'"
                'DT1 = CType(dbConnection.ExecuteQuery(dbSelectCommand), DataTable)

                Using cmd As New NpgsqlCommand
                    dbSelectCommand = "cboRegNo_NotDelNotJCIss"
                    cmd.CommandText = dbSelectCommand
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("par_tblreg", NpgsqlDbType.Varchar, reg)
                    cmd.Parameters.AddWithValue("par_village_code", NpgsqlDbType.Char, ddlvillage.SelectedValue)

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
                    lblmsg.Text = "No Registration found (OR) Jobcard were issued to all for the selected Village !"
                    Return
                End If
            End If
        Catch ex As Exception
            lblmsg.Visible = True
            lblmsg.Text = "Error in Registration No"
        End Try
    End Sub
    Protected Sub ddlReg_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlReg.SelectedIndexChanged
        If Session("Details") = "" Then
            If ddlReg.SelectedItem.Text = ViewState("selectRegNo") Then
                grdData.DataSource = DT1
                grdData.DataBind()
                BtnUpdate.Visible = False
                BtnCancel.Visible = False
                tblDetails.Visible = False
                Return
            End If
        End If
        Try
            DT_FY = ViewState("Financial_Year")
            If DT_FY.Rows.Count <= 0 Then
                lblMessage.Text = "FINANCIAL YEAR DATA NOT FOUND."
                lblMessage.Visible = True
                Exit Sub
            End If

            cmd = New NpgsqlCommand()

            If (ddlvillage.SelectedValue <> "" And ddlReg.SelectedValue <> "" And ddlReg.SelectedValue <> ViewState("selectRegNo")) Or (ddlvillage.SelectedValue <> "" And ddlReg.SelectedValue <> "" And Session("Details") <> "") Then

                Session("Village_Code") = ddlvillage.SelectedValue
                tblDetails.Visible = True
                'dbSelectCommand = "Execute getRegNoFamilyDetlNew_aspx @State_Code='" & State_Code & "', @tblReg = '" & reg & "', @Reg_No = '" & ddlReg.SelectedValue & "'"
                'DT1 = CType(dbConnection.ExecuteQuery(dbSelectCommand), DataTable)
                cmd.CommandText = "getRegNoFamilyDetlNew_aspx"
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("par_state_code", NpgsqlDbType.Char, State_Code)
                cmd.Parameters.AddWithValue("par_tblreg", NpgsqlDbType.Varchar, reg)
                cmd.Parameters.AddWithValue("par_reg_no", NpgsqlDbType.Char, ddlReg.SelectedValue)

                DT1 = dal.ExecuteCommand_dt(cmd, "ref_cursor")
                If DT1.Rows.Count > 0 Then
                    grdData.Visible = True
                    BtnCancel.Visible = True
                    BtnUpdate.Visible = True
                    Reg_DateMDY = DT1.Rows(0)("Registration_Date").ToString()
                    RegDate = Reg_DateMDY.Split("/")
                    Reg_DateDMY = Convert.ToString(RegDate(1) + "/" + RegDate(0) + "/" + RegDate(2))  '--in US culture
                    dt = Reg_DateDMY.Split(" ")
                    Reg_DateDMY = Convert.ToString(dt(0))

                    txtDate.Text = Reg_DateDMY

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
                                rdCategory.Items(0).Selected = True
                            ElseIf item.Contains("ST") Then
                                rdCategory.Items(1).Selected = True
                            ElseIf item.Contains("OBC") Then
                                rdCategory.Items(2).Selected = True
                                'ElseIf item.Contains("IAY") Then
                                '    chkCategory.Items(0).Selected = True

                                'ElseIf item.Contains("LR") Then

                                '    chkCategory.Items(1).Selected = True
                            Else
                                rdCategory.Items(3).Selected = True
                            End If
                        Next
                    End If
                    If DT1.Rows(0)("IS_LR_Beneficiary").ToString() = "Y" Then
                        chkCategory.Items(0).Selected = True
                    End If
                    If DT1.Rows(0)("IS_IAY_Beneficiary").ToString() = "Y" Then
                        chkCategory.Items(0).Selected = True
                    End If
                    If DT1.Rows(0)("Minority").ToString() = "Y" Then
                        chkMinority.Checked = True
                    Else
                        chkMinority.Checked = False
                    End If

                    If DT1.Rows(0)("Small_Farmer").ToString() = "Y" Then
                        rdSFarmer.Checked = True
                    Else
                        rdSFarmer.Checked = False
                    End If
                    If DT1.Rows(0)("Marginal_Farmer").ToString() = "Y" Then
                        rdMFarmer.Checked = True
                    Else
                        rdMFarmer.Checked = False
                    End If
                    If State_Code = "16" Then
                        trRation.Visible = True
                        txtRation.Text = DT1.Rows(0)("Ration_Card_No").ToString()
                    Else
                        trRation.Visible = False
                    End If
                    If DT1.Rows(0)("BPL_Family").ToString() = "Y" Then
                        rdbBPL.SelectedValue = "Y"
                    Else
                        rdbBPL.SelectedValue = "N"
                    End If
                    If DT1.Rows(0)("RSBY_Beneficiary").ToString() = "Y" Then
                        rdbRSBY.SelectedValue = "Y"
                    Else
                        rdbRSBY.SelectedValue = "N"
                    End If

                    If DT1.Rows(0)("AABY_Beneficiary").ToString() = "Y" Then
                        rdbAABY.SelectedValue = "Y"
                    Else
                        rdbAABY.SelectedValue = "N"
                    End If
                    If Convert.ToString(DT1.Rows(0)("BPL_Family")) = "" Or DT1.Rows(0)("BPL_Family").ToString() = "N" Then
                        txtFamilyNo.Text = DT1.Rows(0)("BPL_Family_No").ToString()
                        txtFamilyNo.ReadOnly = True
                    Else
                        txtFamilyNo.Text = DT1.Rows(0)("BPL_Family_No").ToString()
                    End If
                    If DT1.Rows(0)("BPL_Data").ToString() = "Y" Then
                        txtCensus.Text = DT1.Rows(0)("Family_Id_2002").ToString()
                        txtCensus.ReadOnly = True
                    Else
                        txtCensus.Text = DT1.Rows(0)("Family_Id_2002").ToString()
                    End If
                    If Convert.ToString(DT1.Rows(0)("RSBY_Beneficiary")) = "" Or DT1.Rows(0)("RSBY_Beneficiary").ToString() = "N" Then
                        txtRSBY.Text = DT1.Rows(0)("RSBY_Card_No").ToString()
                        txtRSBY.ReadOnly = True
                    Else
                        txtRSBY.Text = DT1.Rows(0)("RSBY_Card_No").ToString()
                    End If
                    If Convert.ToString(DT1.Rows(0)("AABY_Beneficiary")) = "" Or DT1.Rows(0)("AABY_Beneficiary").ToString() = "N" Then
                        txtAABY.Text = DT1.Rows(0)("AABY_Insurance_No").ToString()
                        txtAABY.ReadOnly = True
                    Else
                        txtAABY.Text = DT1.Rows(0)("AABY_Insurance_No").ToString()
                    End If

                    hf_Family_Id.Value = DT1.Rows(0)("Family_Id").ToString()
                    cmd = New NpgsqlCommand()
                    dbSelectCommand = "getRegNoApplicants" ' @tblApp = '" & Applicants & "', @Reg_No = '" & ddlReg.SelectedValue & "'"
                    'DT2 = CType(dbConnection.ExecuteQuery(dbSelectCommand), DataTable)
                    cmd.CommandText = dbSelectCommand
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.Parameters.AddWithValue("par_tblapp", NpgsqlDbType.Varchar, Applicants)
                    cmd.Parameters.AddWithValue("par_reg_no", NpgsqlDbType.Varchar, ddlReg.SelectedValue)
                    DT2 = dal.ExecuteCommand_dt(cmd, "ref_cursor")
                    HF_countrowY.Value = 0
                    If DT2.Rows.Count > 0 Then
                        grdData.Visible = True
                        BtnUpdate.Visible = True
                        BtnCancel.Visible = True
                        grdData.DataSource = DT2
                        grdData.DataBind()
                        HF_countrowY.Value = grdData.Rows.Count
                        AppDemanded = ""
                        Dim i As Integer
                        Dim cnt As Integer = 0
                        For i = 0 To grdData.Rows.Count - 1
                            Applicant_No = Convert.ToString(DT2.Rows(i)("Applicant_No").ToString())
                            Relation_Code = DT2.Rows(i)("Relation_Code").ToString()
                            CType(grdData.Rows(i).FindControl("txtAge"), TextBox).Text = DT2.Rows(i)("Age").ToString()
                            If DT2.Rows(i)("Gender").ToString() = "M" Then
                                CType(grdData.Rows(i).FindControl("rdbGender"), RadioButtonList).SelectedValue = "M"
                            Else
                                CType(grdData.Rows(i).FindControl("rdbGender"), RadioButtonList).SelectedValue = "F"
                            End If
                            If DT2.Rows(i)("Disabled").ToString() = "Y" Then
                                CType(grdData.Rows(i).FindControl("rdbDisabled"), RadioButtonList).SelectedValue = "Y"
                            Else
                                CType(grdData.Rows(i).FindControl("rdbDisabled"), RadioButtonList).SelectedValue = "N"
                            End If

                            AppDemanded = ""
                            AppInMustroll = ""

                            'Dim arr As Array
                            'arr = New String() {"2122", "2021", "1920", "1819", "1718", "1617", "1516", "1415", "1314", "1213", "1112", "1011", "0910", "0809", "0708", "0607", "0506"}

                            'For Each j As String In arr
                            '    tblMustroll = j
                            '    tblMustrollYYYY = shortnm & Right(District_Code, 2) & "MUSTROLL" & tblMustroll
                            '    dbSelectCommand = "Execute TableExist @TN='" & tblMustrollYYYY & "'"
                            '    'DT1 = CType(dbConnection.ExecuteQuery(dbSelectCommand), DataTable)
                            '    cmd.CommandText = dbSelectCommand
                            '    DT1 = dal.ExecuteCommand_dt(cmd)
                            '    If DT1.Rows.Count > 0 Then
                            '        dbSelectCommand = "select Reg_No from " & tblMustrollYYYY
                            '        dbSelectCommand = dbSelectCommand & " where Reg_No = '" & ddlReg.SelectedValue & "'"
                            '        dbSelectCommand = dbSelectCommand & "   and Applicant_No = " & CInt(Applicant_No)
                            '        'DT1 = CType(dbConnection.ExecuteQuery(dbSelectCommand), DataTable)
                            '        cmd.CommandText = dbSelectCommand
                            '        DT1 = dal.ExecuteCommand_dt(cmd)
                            '        If DT1.Rows.Count > 0 Then
                            '            AppInMustroll = "Y"
                            '            AppDemanded = "Y"
                            '            cnt = cnt + 1
                            '            CType(grdData.Rows(i).FindControl("txtName"), TextBox).ReadOnly = True
                            '            grdData.Rows(i).Cells(1).BackColor = Drawing.Color.Orange
                            '            Exit For
                            '        End If
                            '    End If
                            'Next


                            For J = 0 To DT_FY.Rows.Count - 1
                                MustrollFY = Right(Left(DT_FY.Rows(J)("Financial_Year"), 4), 2) & Right(Right(DT_FY.Rows(J)("Financial_Year"), 4), 2)
                                If Not Session("Is_archived") Is Nothing And Not Session("Archived_finyear_upto") Is Nothing Then
                                    If Convert.ToInt32(MustrollFY) > Convert.ToInt32(Session("Archived_finyear_upto")) Then
                                        tblMustrollYYYY = shortnm & Right(District_Code, 2) & "MUSTROLL" & MustrollFY
                                    Else
                                        tblMustrollYYYY = Session("Archived_schema") & shortnm & Right(District_Code, 2) & "MUSTROLL" & MustrollFY
                                    End If
                                Else
                                    tblMustrollYYYY = shortnm & Right(District_Code, 2) & "MUSTROLL" & MustrollFY
                                End If

                                Try
                                    'dbSelectCommand = "select count(1) cnt from " & tblMustrollYYYY & " (nolock) where Reg_No = '" & ddlReg.SelectedValue & "' And Applicant_No = " & CInt(Applicant_No)
                                    'cmd.CommandText = dbSelectCommand

                                    cmd = New NpgsqlCommand()
                                    dbSelectCommand = "SpCommon_EntryReg_EditReg" ' @Tbl_Name = '" & tblMustrollYYYY & "', @Reg_No = '" & ddlReg.SelectedValue.Trim & "',@Applicant_no = " & CInt(Applicant_No) & ",@Sp_No = '3'"
                                    cmd.CommandText = dbSelectCommand
                                    cmd.CommandType = CommandType.StoredProcedure
                                    cmd.Parameters.AddWithValue("par_tbl_name", NpgsqlDbType.Varchar, tblMustrollYYYY)
                                    cmd.Parameters.AddWithValue("par_reg_no", NpgsqlDbType.Varchar, ddlReg.SelectedValue.Trim)
                                    cmd.Parameters.AddWithValue("par_applicant_no", NpgsqlDbType.Integer, CInt(Applicant_No))
                                    cmd.Parameters.AddWithValue("par_sp_no", NpgsqlDbType.Integer, 3)

                                    rec_cnt = dal.ExecuteScalar(cmd)

                                    If rec_cnt > 0 Then
                                        AppInMustroll = "Y"
                                        AppDemanded = "Y"
                                        cnt = cnt + 1
                                        CType(grdData.Rows(i).FindControl("txtName"), TextBox).ReadOnly = True
                                        grdData.Rows(i).Cells(1).BackColor = Drawing.Color.Orange
                                        Exit For
                                    End If
                                Catch ex As Exception
                                End Try
                                'End If
                            Next

                            If AppInMustroll = "" Then

                                'Dim ar As Array
                                'ar = New String() {"2122", "2021", "1920", "1819", "1718", "1617", "1516", "1415", "1314", "1213", "1112", "1011", "0910", "0809", "0708", "0607", "0506"}
                                'For Each j As String In arr
                                '    tblWork_Demand = j
                                '    tblWork_DemandYYYY = shortnm & Right(District_Code, 2) & "WORK_DEMAND" & tblWork_Demand
                                '    dbSelectCommand = "Execute TableExist @TN='" & tblWork_DemandYYYY & "'"
                                '    cmd.CommandText = dbSelectCommand
                                '    DT1 = dal.ExecuteCommand_dt(cmd)
                                '    If DT1.Rows.Count > 0 Then
                                '        dbSelectCommand = "Execute chkAppDemanded @tblWork_DemandYYYY = '" & tblWork_DemandYYYY & "', @Reg_No = '" & ddlReg.SelectedValue & "', @Applicant_No = '" & Convert.ToString(Applicant_No) & "'"
                                '        cmd.CommandText = dbSelectCommand
                                '        DT1 = dal.ExecuteCommand_dt(cmd)

                                '        If DT1.Rows.Count > 0 Then
                                '            AppDemanded = "Y"
                                '            cnt = cnt + 1
                                '            CType(grdData.Rows(i).FindControl("txtName"), TextBox).ReadOnly = True
                                '            grdData.Rows(i).Cells(1).BackColor = Drawing.Color.Green
                                '            Exit For
                                '        End If

                                '    End If
                                'Next


                                For J = 0 To DT_FY.Rows.Count - 1
                                    Work_DemandFY = Right(Left(DT_FY.Rows(J)("Financial_Year"), 4), 2) & Right(Right(DT_FY.Rows(J)("Financial_Year"), 4), 2)
                                    If Not Session("Is_archived") Is Nothing And Not Session("Archived_finyear_upto") Is Nothing Then
                                        If Convert.ToInt32(Work_DemandFY) > Convert.ToInt32(Session("Archived_finyear_upto")) Then
                                            tblWork_DemandYYYY = shortnm & Right(District_Code, 2) & "WORK_DEMAND" & Work_DemandFY
                                        Else
                                            tblWork_DemandYYYY = Session("Archived_schema") & shortnm & Right(District_Code, 2) & "WORK_DEMAND" & Work_DemandFY
                                        End If
                                    Else
                                        tblWork_DemandYYYY = shortnm & Right(District_Code, 2) & "WORK_DEMAND" & Work_DemandFY
                                    End If

                                    'dbSelectCommand = "Execute TableExist @TN='" & tblWork_DemandYYYY & "'"
                                    'cmd.CommandText = dbSelectCommand
                                    'DT1 = dal.ExecuteCommand_dt(cmd)
                                    'If DT1.Rows.Count > 0 Then
                                    Try
                                        dbSelectCommand = "Execute chkAppDemanded @tblWork_DemandYYYY = '" & tblWork_DemandYYYY & "', @Reg_No = '" & ddlReg.SelectedValue & "', @Applicant_No = " & CInt(Applicant_No)
                                        cmd.CommandText = dbSelectCommand
                                        'DT1 = dal.ExecuteCommand_dt(cmd)
                                        rec_cnt = dal.ExecuteScalar(cmd)
                                        If rec_cnt > 0 Then
                                            AppDemanded = "Y"
                                            cnt = cnt + 1
                                            CType(grdData.Rows(i).FindControl("txtName"), TextBox).ReadOnly = True
                                            grdData.Rows(i).Cells(1).BackColor = Drawing.Color.Green
                                            Exit For
                                        End If
                                    Catch ex As Exception
                                    End Try
                                    'End If
                                Next
                            End If

                            If State_Code = "16" Then
                                dbSelectCommand = "Execute cboRelationsKL @Language='" & Rdlst.SelectedValue & "'"
                            Else
                                dbSelectCommand = "cboRelationsBPL"
                            End If
                            cmd = New NpgsqlCommand()
                            cmd.CommandText = dbSelectCommand
                            cmd.CommandType = CommandType.StoredProcedure

                            DT1 = dal.ExecuteCommand_dt(cmd, "p_refcur")

                            CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).DataSource = DT1
                            CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).DataTextField = "Relation_Local"
                            CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).DataValueField = "Relation_Code"
                            CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).DataBind()
                            CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).Items.Insert(0, "Select Relation")
                            CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = Relation_Code

                            If i = 10 Then
                                Exit For
                            End If
                        Next
                        If cnt = 0 Then
                            lblMessage.Text = ""
                            lblMessage.Visible = False
                        Else
                            lblMessage.Text = "Applicant-Name in Green/Orange cell cannot be changed as he/she already demanded/worked !"
                            lblMessage.Visible = True
                        End If
                        'If AppDemanded = "Y" Then

                        '    'lblMessage.Text = "Applicant-Name in Green/Orange cell cannot be changed as he/she already demanded/worked !"
                        '    'lblMessage.Visible = True
                        'Else
                        '    'lblMessage.Text = ""
                        '    'lblMessage.Visible = False

                        'End If
                    Else
                        lblMessage.Text = ""
                        lblMessage.Visible = False
                        grdData.Visible = False
                        BtnUpdate.Visible = True
                        BtnCancel.Visible = True
                        Return
                    End If
                Else
                    grdData.Visible = False
                    BtnUpdate.Visible = True
                    BtnCancel.Visible = True
                    lblmsg.Visible = True
                    lblmsg.Text = "No Data Found"
                    Return
                End If
            End If
            BtnUpdate.Enabled = True
            BtnCancel.Enabled = True
        Catch ex As Exception
            lblmsg.Visible = True
            lblmsg.Text = "Error in Loading data."
            BtnUpdate.Enabled = False
            BtnCancel.Enabled = False
        End Try
    End Sub
    Protected Sub BtnCancel_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles BtnCancel.Click
        Response.Redirect(Request.Url.ToString())
    End Sub
    Protected Sub BtnUpdate_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles BtnUpdate.Click
        Try
            cmd = New NpgsqlCommand()

            Registration_Date = FormatDateMMDDYYYY_new("Registration_Date", Trim(txtDate.Text))
            If EntryErrorFound = "Y" Then
                Exit Sub
            End If

            Dim Caste, Minority, Small_Farmer, Marginal_Farmer As String
            Small_Farmer = "N"
            Marginal_Farmer = "N"
            If rdSFarmer.Checked = True Then
                Small_Farmer = "Y"
            ElseIf rdMFarmer.Checked = True Then
                Marginal_Farmer = "Y"
            End If
            If chkMinority.Checked = True Then
                Minority = "Y"
            Else
                Minority = "N"
            End If
            If rdCategory.SelectedValue = "SC" Then
                Caste = "SC"
            ElseIf rdCategory.SelectedValue = "ST" Then
                Caste = "ST"
            ElseIf rdCategory.SelectedValue = "OBC" Then
                Caste = "OBC"
            Else
                Caste = "OTH"
            End If

            '################# discontinued just below code on 10-mar-2021.

            'If chkCategory.Items(0).Selected = True Then
            '    If Caste = "SC" Or Caste = "ST" Then
            '        Caste = Caste & " ;IAY"   ' For SC/ST, Add space and then semicolon;
            '    Else
            '        Caste = Caste & ";IAY" ' For OTH/OBC, do not include blank space before semicolon ;
            '    End If
            'End If

            'If chkCategory.Items(1).Selected = True Then
            '    If chkCategory.Items(0).Selected = True Then
            '        Caste = Caste & ";LR Beneficiary"
            '    ElseIf Caste = "SC" Or Caste = "ST" Then
            '        Caste = Caste & " ;LR Beneficiary"   ' For SC/ST, Add space and then semicolon;
            '    Else
            '        Caste = Caste & ";LR Beneficiary" ' For OTH/OBC, do not include blank space before semicolon ;
            '    End If

            'Else
            '    If chkCategory.Items(0).Selected = True Then
            '        Caste = Caste & " Beneficiary"
            '    End If
            'End If

            '################# Two new column implemented which is seperated from Caste on 10-mar-2021.

            If chkCategory.Items(0).Selected = True Then
                IAY_benef = "Y"
            Else
                IAY_benef = "N"
            End If

            If chkCategory.Items(1).Selected = True Then
                LR_benef = "Y"
            Else
                LR_benef = "N"
            End If

            '################## END ###################

            lblmsg.ForeColor = Drawing.Color.Red

            If State_Code = "16" Then 'KL
                Ration_Card_No = Replace(Trim(txtRation.Text), Chr(9), "")
            End If

            '----For validating Relations--------------------------
            If grdData.Visible = True Then
                For i = 0 To grdData.Rows.Count - 1
                    Dim j As Integer = i + 1
                    If (CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue <> "Select Relation") Then
                        gender = CType(grdData.Rows(i).FindControl("rdbGender"), RadioButtonList).SelectedValue

                        If gender = "F" Then

                            If State_Code = "16" Then
                                If CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 1 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 4 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 5 Then

                                    While j <= grdData.Rows.Count - 1
                                        If CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = CType(grdData.Rows(j).FindControl("ddlRelation"), DropDownList).SelectedValue Then
                                            lblmsg.Text = "Invalid Relationship Found...! Record NOT Saved !<br/>[The relationships 'Self', 'Mother', 'Father' cannot repeat !]"
                                            lblmsg.Visible = True
                                            Return
                                        End If
                                        j = j + 1
                                    End While
                                End If
                            Else
                                If CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 1 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 3 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 4 Then
                                    While j <= grdData.Rows.Count - 1
                                        If CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = CType(grdData.Rows(j).FindControl("ddlRelation"), DropDownList).SelectedValue Then
                                            lblmsg.Text = "Relationship self,mother,father can not repeat Please Select Valid Relationship"
                                            lblmsg.Visible = True
                                            Return
                                        End If
                                        j = j + 1
                                    End While
                                End If

                            End If

                        ElseIf gender = "M" Then

                            If State_Code = "16" Then
                                If CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 1 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 4 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 5 Then
                                    While j <= grdData.Rows.Count - 1
                                        If CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = CType(grdData.Rows(j).FindControl("ddlRelation"), DropDownList).SelectedValue Then
                                            lblmsg.Text = "Relationship self,mother,father can not repeat Please Select Valid Relationship"
                                            lblmsg.Visible = True
                                            Return
                                        End If
                                        j = j + 1
                                    End While
                                End If
                            Else
                                If CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 1 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 3 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 4 Then

                                    While j <= grdData.Rows.Count - 1
                                        If CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = CType(grdData.Rows(j).FindControl("ddlRelation"), DropDownList).SelectedValue Then

                                            lblmsg.Text = "Relationship self,mother,father can not repeat Please Select Valid Relationship"
                                            lblmsg.Visible = True
                                            Return
                                        End If
                                        j = j + 1
                                    End While
                                End If

                            End If

                        End If
                    End If
                Next
                For i = 0 To grdData.Rows.Count - 1
                    Dim j As Integer = i + 1
                    If (CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue <> "Select Relation") Then
                        gender = CType(grdData.Rows(i).FindControl("rdbGender"), RadioButtonList).SelectedValue
                        If gender = "M" Then

                            If State_Code = "16" Then
                                If CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 2 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 5 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 7 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 9 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 11 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 14 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 16 Then
                                    lblmsg.Text = "Invalid Gender-Relationship combination Found...! Record NOT Saved !<br/>[The valid 'Male' relationships are 'Self', 'Husband',  'Father', 'Son', 'Father-in-Law', 'Son-in-law', 'Brother', 'Grandson', 'Grandfather' !]<br/>The valid 'Female' relationships are 'Self', 'Wife', 'Mother', 'Daughter', 'Mother-in-Law', 'Daughter-in-law', 'Sister', 'Granddaughter', 'Grandmother' !]"
                                    lblmsg.Visible = True
                                    Return
                                End If
                            Else
                                If CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 2 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 4 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 6 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 8 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 10 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 12 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 14 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 16 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 18 Then
                                    lblmsg.Text = "Invalid Gender-Relationship combination Found...! Record NOT Saved !<br/>[The valid 'Male' relationships are 'Self', 'Husband',  'Father', 'Son', 'Father-in-Law', 'Son-in-law', 'Brother', 'Grandson', 'Grandfather' !]<br/>The valid 'Female' relationships are 'Self', 'Wife', 'Mother', 'Daughter', 'Mother-in-Law', 'Daughter-in-law', 'Sister', 'Granddaughter', 'Grandmother' !]"
                                    lblmsg.Visible = True
                                    Return
                                End If

                            End If

                        ElseIf gender = "F" Then

                            If State_Code = "16" Then
                                If CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 3 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 4 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 6 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 8 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 10 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 12 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 13 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 15 Then
                                    lblmsg.Text = "Invalid Gender-Relationship combination Found...! Record NOT Saved !<br/>[The valid 'Male' relationships are 'Self', 'Husband',  'Father', 'Son', 'Father-in-Law', 'Son-in-law', 'Brother', 'Grandson', 'Grandfather' !]<br/>The valid 'Female' relationships are 'Self', 'Wife', 'Mother', 'Daughter', 'Mother-in-Law', 'Daughter-in-law', 'Sister', 'Granddaughter', 'Grandmother' !]"
                                    lblmsg.Visible = True
                                    Return
                                End If
                            Else
                                If CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 3 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 5 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 7 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 9 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 11 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 13 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 15 Or CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue = 19 Then
                                    lblmsg.Text = "Invalid Gender-Relationship combination Found...! Record NOT Saved !<br/>[The valid 'Male' relationships are 'Self', 'Husband',  'Father', 'Son', 'Father-in-Law', 'Son-in-law', 'Brother', 'Grandson', 'Grandfather' !]<br/>The valid 'Female' relationships are 'Self', 'Wife', 'Mother', 'Daughter', 'Mother-in-Law', 'Daughter-in-law', 'Sister', 'Granddaughter', 'Grandmother' !]"
                                    lblmsg.Visible = True
                                    Return
                                End If

                            End If

                        End If
                    End If
                Next
            End If

            '-------------For Checking duplicate Name and invalid caharacters name in Applicant name---------------------
            If grdData.Visible = True Then
                Dim tapp, Applicant As String
                For i = 0 To grdData.Rows.Count - 1
                    Applicant = Replace(Trim(CType(grdData.Rows(i).FindControl("txtName"), TextBox).Text), vbTab, "")
                    age = CType(grdData.Rows(i).FindControl("txtAge"), TextBox).Text.Trim
                    Dim j As Integer = i + 1
                    If CType(grdData.Rows(i).FindControl("txtName"), TextBox).Text <> "" Then
                        tapp = Applicant

                        While j <= grdData.Rows.Count - 1
                            If tapp = Replace(Trim(CType(grdData.Rows(j).FindControl("txtName"), TextBox).Text), vbTab, "") Then
                                If CType(grdData.Rows(i).FindControl("rdbGender"), RadioButtonList).SelectedValue = CType(grdData.Rows(j).FindControl("rdbGender"), RadioButtonList).SelectedValue And CType(grdData.Rows(i).FindControl("txtAge"), TextBox).Text = CType(grdData.Rows(j).FindControl("txtAge"), TextBox).Text Then
                                    lblmsg.Text = "Duplicate Applicant Name found"
                                    lblmsg.Visible = True
                                    Return
                                End If
                            End If
                            j = j + 1
                        End While
                    End If

                    If Regex.IsMatch(Applicant, "[!@#$^&*()-+=|\{};<>?,:/0123456789']") Then
                        lblmsg.Text = "Invalid characters found in Applicant Name..!!"
                        lblmsg.Visible = True
                        Return
                    End If

                    If age = "" Or CInt(age.ToString.Trim) < 18 Then
                        lblmsg.Text = "Please enter valid Applicant's AGE..!!"
                        lblmsg.Visible = True
                        Return
                    End If
                Next
            End If

            '//////////////////  Updating Records    ////////////////////////////////////////

            dal.BeginTransaction()

            dbSelectCommand = "UpdRegNew_aspx" 'Remove UpdReg1
            cmd.CommandText = dbSelectCommand
            cmd.CommandType = CommandType.StoredProcedure

            cmd.Parameters.AddWithValue("par_tblreg", NpgsqlDbType.Varchar, reg)
            cmd.Parameters.AddWithValue("par_reg_no", NpgsqlDbType.Varchar, ddlReg.SelectedValue)
            cmd.Parameters.AddWithValue("par_state_code", NpgsqlDbType.Char, State_Code)
            cmd.Parameters.AddWithValue("par_head_of_household", NpgsqlDbType.Varchar, txtHead.Text)
            cmd.Parameters.AddWithValue("par_father_or_husband_name", NpgsqlDbType.Varchar, txtFatHus.Text)
            cmd.Parameters.AddWithValue("par_house_no", NpgsqlDbType.Varchar, txtHouseNo.Text)
            cmd.Parameters.AddWithValue("par_registration_date", NpgsqlDbType.Varchar, Registration_Date)
            cmd.Parameters.AddWithValue("par_epic_no", NpgsqlDbType.Varchar, txtEpicNo.Text)
            cmd.Parameters.AddWithValue("par_bpl_family", NpgsqlDbType.Char, rdbBPL.SelectedValue)
            cmd.Parameters.AddWithValue("par_bpl_family_no", NpgsqlDbType.Varchar, txtFamilyNo.Text)
            cmd.Parameters.AddWithValue("par_entry_date", NpgsqlDbType.Timestamp, Now)
            cmd.Parameters.AddWithValue("par_entry_by", NpgsqlDbType.Varchar, Entry_by)
            cmd.Parameters.AddWithValue("par_rsby_beneficiary", NpgsqlDbType.Char, rdbRSBY.SelectedValue)
            cmd.Parameters.AddWithValue("par_rsby_card_no", NpgsqlDbType.Varchar, txtRSBY.Text)
            cmd.Parameters.AddWithValue("par_aaby_beneficiary", NpgsqlDbType.Char, rdbAABY.SelectedValue)
            cmd.Parameters.AddWithValue("par_aaby_insurance_no", NpgsqlDbType.Varchar, txtAABY.Text)
            cmd.Parameters.AddWithValue("par_minority", NpgsqlDbType.Char, Minority)
            cmd.Parameters.AddWithValue("par_small_farmer", NpgsqlDbType.Char, Small_Farmer)
            cmd.Parameters.AddWithValue("par_marginal_farmer", NpgsqlDbType.Char, Marginal_Farmer)
            cmd.Parameters.AddWithValue("par_family_id_2002", NpgsqlDbType.Varchar, txtCensus.Text)
            If Ration_Card_No Is Nothing Then
                Ration_Card_No = ""
            End If
            cmd.Parameters.AddWithValue("par_ration_card_no", NpgsqlDbType.Varchar, Ration_Card_No)
            cmd.Parameters.AddWithValue("par_font_chosen", NpgsqlDbType.Varchar, "")

            '####################### Two new column added which is seperated from Caste on 10-mar-2021.

            cmd.Parameters.AddWithValue("par_is_iay_beneficiary", NpgsqlDbType.Char, IAY_benef)
            cmd.Parameters.AddWithValue("par_is_lr_beneficiary", NpgsqlDbType.Char, LR_benef)

            '######################## END #################


            dal.ExecuteCommand_rowsaffected(cmd)

            '////////////For Updating Applicant---/////////////////////////////////////

            If grdData.Visible = True Then
                For i = 0 To grdData.Rows.Count - 1
                    Applicant_No = CType(grdData.Rows(i).FindControl("txtSno"), TextBox).Text
                    Disabled = CType(grdData.Rows(i).FindControl("rdbDisabled"), RadioButtonList).SelectedValue
                    age = CType(grdData.Rows(i).FindControl("txtAge"), TextBox).Text
                    gender = CType(grdData.Rows(i).FindControl("rdbGender"), RadioButtonList).SelectedValue
                    AppName = Replace(Trim(CType(grdData.Rows(i).FindControl("txtName"), TextBox).Text), vbTab, "")
                    Relation_Code = CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).SelectedValue
                    If Relation_Code = "Select Relation" Then
                        Relation_Code = 0
                    End If
                    cmd = New NpgsqlCommand()
                    dbSelectCommand = "UpdApp"
                    cmd.CommandText = dbSelectCommand
                    cmd.CommandType = CommandType.StoredProcedure

                    cmd.Parameters.AddWithValue("par_tblapp", NpgsqlDbType.Varchar, Applicants)
                    cmd.Parameters.AddWithValue("par_reg_no", NpgsqlDbType.Varchar, ddlReg.SelectedValue)
                    cmd.Parameters.AddWithValue("par_applicant_no", NpgsqlDbType.Integer, Int32.Parse(Applicant_No))
                    cmd.Parameters.AddWithValue("par_applicant_name", NpgsqlDbType.Varchar, AppName)
                    cmd.Parameters.AddWithValue("par_gender", NpgsqlDbType.Char, gender)
                    cmd.Parameters.AddWithValue("par_age", NpgsqlDbType.Integer, Int32.Parse(age))
                    cmd.Parameters.AddWithValue("par_relation_code", NpgsqlDbType.Integer, Int32.Parse(Relation_Code))
                    cmd.Parameters.AddWithValue("par_disabled", NpgsqlDbType.Char, Disabled)
                    cmd.Parameters.AddWithValue("par_entry_date", NpgsqlDbType.Timestamp, Now)
                    cmd.Parameters.AddWithValue("par_entry_by", NpgsqlDbType.Varchar, Entry_by)

                    dal.ExecuteCommand_rowsaffected(cmd)
                Next
            End If

            dal.CommitTransaction()
            lblmsg.Text = "Job card details UPDATED successfully for the Registration-ID : '" & ddlReg.SelectedValue & "'"
            lblmsg.Visible = True
            lblmsg.ForeColor = Drawing.Color.Green
            ScriptManager.RegisterClientScriptBlock(Me, Me.GetType(), "Alert", "alert('" & lblmsg.Text & "')", True)

            '//////////////////////////////////    Uploading of Photos has been suspended from here  //////////////////////////////

            'dbSelectCommand = "Execute getRegNoPhotoFile @tblReg = " & reg & ", @Reg_No = '" & ddlReg.SelectedValue & "'"
            'cmd.CommandText = dbSelectCommand
            'DT1 = dal.ExecuteCommand_dt(cmd)
            'If DT1.Rows.Count > 0 Then
            '    If Convert.ToString(DT1.Rows(0)("Photo_File_Name")) <> "" Then
            '        Photo_File_Name = Trim(DT1.Rows(0)("Photo_File_Name"))
            '        lnkExistPhoto.Visible = True
            '        lblAttachExist.Visible = False
            '        lblChange.Visible = True
            '        lnkExistPhoto.OnClientClick = "javascript: window.open('ViewFamilyPhoto.aspx?Reg_No=" & ddlReg.SelectedValue & "','Photo', 'width=550,height=500,left=150,top=175,screenX=0,screenY=100')"
            '    Else
            '        lnkExistPhoto.Visible = False
            '        lblChange.Visible = False

            '        lblAttachExist.Visible = True
            '        Photo_File_Name = ""
            '    End If
            '    dbSelectCommand = "select Reg_No, Applicant_No, Applicant_Name, Photo_Path, Photo_File from " & Applicants
            '    dbSelectCommand = dbSelectCommand & " where Reg_No =@reg and (event_flag is null or event_flag <> 'D')"
            '    dbSelectCommand = dbSelectCommand & " order by Applicant_No"
            '    cmd.CommandText = dbSelectCommand
            '    cmd.Parameters.Clear()
            '    cmd.Parameters.AddWithValue("reg", Trim(ddlReg.SelectedValue))
            '    DT1 = dal.ExecuteCommand_dt(cmd)
            '    AppCount = 0
            '    If DT1.Rows.Count > 0 Then
            '        grdDetails.Visible = True
            '        grdDetails.DataSource = DT1
            '        grdDetails.DataBind()
            '        Dim k As Integer
            '        For k = 0 To DT1.Rows.Count - 1
            '            AppCount = AppCount + 1
            '            Applicant_No = Convert.ToString(DT1.Rows(k)("Applicant_No"))
            '            If Trim(Convert.ToString(DT1.Rows(k)("Photo_File"))) <> "" Then
            '                CType(grdDetails.Rows(k).FindControl("lnkExistPhoto"), LinkButton).Text = "View Photo"
            '                CType(grdDetails.Rows(k).FindControl("lnkExistPhoto"), LinkButton).OnClientClick = "javascript: window.open('ViewFamilyPhoto.aspx?Reg_No=" & ddlReg.SelectedValue & "&Applicant_No=" & Applicant_No & "','Photo', 'width=550,height=500,left=150,top=175,screenX=0,screenY=100')"
            '            End If
            '        Next
            '    End If
            '    Session("AppCount") = AppCount
            '    Session("Reg_No") = Trim(ddlReg.SelectedValue)
            '    divEdit.Visible = False
            '    divMsg.Visible = True

            '    divPhoto.Visible = False 'Due to shifting of photos to new server, Uploading of Photo is temporarily suspended
            '    lblmsg.Text += "<br/>" & "Please upload workers' photo using the option 'Upload photo' in GP and PO login.Uploading of Photo is suspended here."
            '    More = "EditReg.aspx"
            'Else
            '    lblmsg.Text = "Reg. No '" & ddlReg.SelectedValue & "' Not Found ! Record NOT Updated !"
            '    lblmsg.Visible = True
            'End If
        Catch ex As SqlException
            dal.RollBackTransaction()
            lblmsg.Text = "SQL-Error found in Updating Job Card details."
            lblmsg.Visible = True
            lblmsg.ForeColor = Drawing.Color.Red
        Catch ex As Exception
            dal.RollBackTransaction()
            lblmsg.Text = "Error found in Updating Job Card details."
            lblmsg.Visible = True
            lblmsg.ForeColor = Drawing.Color.Red
        Finally
            dal.CommitTransaction()
        End Try
    End Sub

    Function FormatDateMMDDYYYY(ByVal ColName As String, ByVal inputDate As String) As String
        Dim dateDelimeter, FormatDMMDDYYYY As String
        If (String.IsNullOrEmpty(inputDate)) Or (inputDate = "") Then
            'Response.Write "<td align=center><font face=helvetica>" & ColName & "<font face=helvetica color=RED><font face=helvetica color=Black> cannot be BLANK !</b></td><tr>"
        Else
            If InStr(inputDate, "/") > 1 Then
                dateDelimeter = "/"
            ElseIf InStr(inputDate, "-") > 1 Then
                dateDelimeter = "-"
            ElseIf InStr(inputDate, ".") > 1 Then
                dateDelimeter = "."
            End If
            FormatDMMDDYYYY = Mid(inputDate, InStr(inputDate, dateDelimeter) + 1, InStrRev(inputDate, dateDelimeter) - InStr(inputDate, dateDelimeter) - 1) & "/" & Left(inputDate, InStr(inputDate, dateDelimeter) - 1) & "/" & Mid(inputDate, InStrRev(inputDate, dateDelimeter) + 1)
            'End If
        End If
        Return FormatDMMDDYYYY
    End Function

    Function FormatDateMMDDYYYY_new(ByVal ColName As String, ByVal inputDate As String) As String
        Dim dateDelimeter, FormatDMMDDYYYY As String
        If (String.IsNullOrEmpty(inputDate)) Or (inputDate = "") Then
        Else
            If InStr(inputDate, "/") > 1 Then
                dateDelimeter = "/"
            ElseIf InStr(inputDate, "-") > 1 Then
                dateDelimeter = "-"
            ElseIf InStr(inputDate, ".") > 1 Then
                dateDelimeter = "."
            End If

            If Not (IsDate(inputDate)) Then
                EntryErrorFound = "Y"
                lblmsg.Text = "Invalid Date Format"
                lblmsg.Visible = True
            End If

            FormatDMMDDYYYY = Mid(inputDate, InStr(inputDate, dateDelimeter) + 1, InStrRev(inputDate, dateDelimeter) - InStr(inputDate, dateDelimeter) - 1) & "/" & Left(inputDate, InStr(inputDate, dateDelimeter) - 1) & "/" & Mid(inputDate, InStrRev(inputDate, dateDelimeter) + 1)

            If DateDiff("d", Date.Now, FormatDMMDDYYYY) > 0 Then
                EntryErrorFound = "Y"
                lblmsg.Text = "Registration Date '" & inputDate & "' Cannot be Future date !"
                lblmsg.Visible = True

            ElseIf DateDiff("d", "02/02/2006", FormatDMMDDYYYY) < 0 Then
                EntryErrorFound = "Y"
                lblmsg.Text = "Registration Date '" & inputDate & "' cannot be earlier than '02/02/2006' <br>as NREGA Scheme was launched on '02/02/2006' !"
                lblmsg.Visible = True
            Else
                EntryErrorFound = "N"
            End If
        End If
        Return FormatDMMDDYYYY
    End Function

    Protected Sub rdbBPL_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles rdbBPL.SelectedIndexChanged
        If rdbBPL.SelectedValue = "Y" Then
            txtFamilyNo.ReadOnly = False
        ElseIf rdbBPL.SelectedValue = "N" Then
            txtFamilyNo.Text = ""
            txtFamilyNo.ReadOnly = True
        Else
            txtFamilyNo.Text = ""
            txtFamilyNo.ReadOnly = True
        End If
    End Sub

    Protected Sub rdbRSBY_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles rdbRSBY.SelectedIndexChanged
        If rdbRSBY.SelectedValue = "Y" Then
            txtRSBY.ReadOnly = False
        ElseIf rdbBPL.SelectedValue = "N" Then
            txtRSBY.Text = ""
            txtRSBY.ReadOnly = True
        Else
            txtRSBY.Text = ""
            txtRSBY.ReadOnly = True
        End If
    End Sub

    'Protected Sub lnkMore_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles lnkMore.Click
    '    ddlvillage_SelectedIndexChanged1(sender, e)
    '    divEdit.Visible = True
    '    divPhoto.Visible = False
    'End Sub
    'Protected Sub btnSave_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSave.Click
    'Try
    '    cmd = New NpgsqlCommand()
    '    Dim i As Integer
    '    Dim Photo_File_Name, RenamedFamilyPhoto, RenamedAppPhoto, Photo_Path, Photo_File, Family_Photo_Path, StatePhotoFolderPath, StatePhotoFolder, pathToFile As String
    '    RenamedFamilyPhoto = ""
    '    RenamedAppPhoto = ""
    '    lblmsg.Visible = False
    '    If shortnm = "KL" And Session("finyear_d") = "2011-2012" Then ' Use \Photos\KL1
    '        StatePhotoFolder = "Photos/KL1"
    '        StatePhotoFolderPath = "Photos/KL1/"
    '    Else
    '        StatePhotoFolder = "Photos/" & shortnm
    '        StatePhotoFolderPath = "Photos/" & shortnm & "/"
    '    End If
    '    Session("FamilyPhoto") = ""
    '    If Len(LTrim(RTrim(FileUpload.HasFile))) > 0 Then
    '        If FileUpload.HasFile Then
    '            Dim fname As String = FileUpload.FileName
    '            Dim ext As String = Path.GetExtension(fname)
    '            If LCase(Trim(Mid(fname, InStrRev(fname, ".", Len(fname)), Len(fname)))) <> ".jpg" Then
    '                lblError.Text = "Invalid File-Type for the Family-Photo ! (Only .jpg is allowed)<br/>The Family-Photo is NOT UPLOADED for the Registration-ID. : <font face=verdana size=3>" & ddlReg.SelectedValue & ""
    '                lblError.Visible = True

    '            Else
    '                If CLng(FileUpload.FileBytes.Length) <= 30720 Then
    '                    If Len(Trim(ddlvillage.SelectedValue)) = 8 Then
    '                        RenamedFamilyPhoto = Mid(Panchayat_Code, 3, 8) & ddlvillage.SelectedValue & "-" & hf_Family_Id.Value & Mid(fname, InStrRev(fname, ".", Len(fname)), Len(fname))
    '                    ElseIf Len(Trim(ddlvillage.SelectedValue)) = 18 Then ' 18 Digits
    '                        RenamedFamilyPhoto = Trim(ddlvillage.SelectedValue) & "-" & hf_Family_Id.Value & Mid(fname, InStrRev(fname, ".", Len(fname)), Len(fname))
    '                    ElseIf Len(Trim(ddlvillage.SelectedValue)) = 13 Then ' 13 Digits
    '                        RenamedFamilyPhoto = Mid(ddlvillage.SelectedValue, 3, 11) & "-" & hf_Family_Id.Value & Mid(fname, InStrRev(fname, ".", Len(fname)), Len(fname))
    '                    End If
    '                    'pathToFile = Replace(Server.MapPath("~/Photos/" & shortnm & "/"), "Netnrega\", "") & "\" & RenamedFamilyPhoto

    '                    'If shortnm = "OR" Then 'Orissa
    '                    '    pathToFile = Replace(Server.MapPath("~/Photos/" & shortnm & "/"), "Netnrega\", "") & "\" & RenamedFamilyPhoto
    '                    'Else
    '                    '    pathToFile = Replace(Replace(Server.MapPath("~/Photos/" & shortnm & "/"), "Netnrega\", ""), "H:", "I:") & "\" & RenamedFamilyPhoto
    '                    'End If

    '                    Select Case UCase(shortnm)
    '                        Case "OR"
    '                            pathToFile = Replace(Server.MapPath("~/Photos/" & shortnm & "/"), "Netnrega\", "") & "\" & RenamedFamilyPhoto
    '                        Case "KL" 'LNDC
    '                            If Session("finyear_d") = "2011-2012" Then ' Use \Photos\KL1
    '                                pathToFile = Replace(Replace(Server.MapPath("~/Photos/KL1/"), "Netnrega\", ""), "H:", "I:") & "\" & RenamedFamilyPhoto
    '                            Else
    '                                pathToFile = Replace(Replace(Server.MapPath("~/Photos/" & shortnm & "/"), "Netnrega\", ""), "H:", "I:") & "\" & RenamedFamilyPhoto
    '                            End If
    '                        Case Else 'LNDC
    '                            pathToFile = Replace(Replace(Server.MapPath("~/Photos/" & shortnm & "/"), "Netnrega\", ""), "H:", "I:") & "\" & RenamedFamilyPhoto
    '                    End Select

    '                    Family_Photo_Path = "../" & StatePhotoFolderPath & RenamedFamilyPhoto
    '                    Photo_File_Name = RenamedFamilyPhoto

    '                    FileUpload.SaveAs(pathToFile)
    '                    hf_FamilyPhoto.Value = pathToFile + ";" + Family_Photo_Path + ";" + Photo_File_Name
    '                    Session("FamilyPhoto") = hf_FamilyPhoto.Value

    '                    'dbSelectCommand = "Update " & reg
    '                    'dbSelectCommand = dbSelectCommand & " set Family_Photo_Path = '" & Family_Photo_Path & "', "
    '                    'dbSelectCommand = dbSelectCommand & " Photo_File_Name = '" & Photo_File_Name & "',"
    '                    'dbSelectCommand = dbSelectCommand & " Entry_by = N'" & Entry_by & "',"
    '                    'dbSelectCommand = dbSelectCommand & " Entry_date = '" & Now & "'"
    '                    'dbSelectCommand = dbSelectCommand & " where Reg_No = '" & ddlReg.SelectedValue & "'"
    '                    'dbConnection.ExecuteNonQuery(dbSelectCommand)

    '                    dbSelectCommand = "Update " & reg & " set Family_Photo_Path =@Family_Photo_Path,Photo_File_Name=@Photo_File_Name,Entry_by=@Entry_by,Entry_date=@Entry_date where Reg_No=@Reg_No"
    '                    cmd.CommandText = dbSelectCommand
    '                    cmd.Parameters.Clear()
    '                    cmd.Parameters.AddWithValue("@Family_Photo_Path", Family_Photo_Path)
    '                    cmd.Parameters.AddWithValue("@Photo_File_Name", Photo_File_Name)
    '                    cmd.Parameters.Add("@Entry_by", SqlDbType.NVarChar, 100).Value = Entry_by
    '                    cmd.Parameters.AddWithValue("@Entry_date", Now)
    '                    cmd.Parameters.AddWithValue("@Reg_No", ddlReg.SelectedValue)
    '                    dal.ExecuteCommand_rowsaffected(cmd)
    '                Else
    '                    lblError.Text = "Kindly reduce the size of the Family-Photo to be less than 30 KB and upload again !<br/> The Family-Photo is NOT UPLOADED for the Registration-ID. : '" & ddlReg.SelectedValue & "'"
    '                    Session("familymsg") = lblError.Text
    '                    Return
    '                End If
    '            End If
    '        End If
    '        DT1 = New DataTable()
    '        DT1.Columns.Add("ApplicantPhotoDetails")
    '        If grdDetails.Rows.Count > 0 Then
    '            For i = 0 To CInt(Session("AppCount")) - 1
    '                Dim count As Integer = 0

    '                If CType(grdDetails.Rows(i).FindControl("FileUploadAppPhoto"), FileUpload).HasFile Then

    '                    Session("count") = 0
    '                    Applicant_No = CType(grdDetails.Rows(i).FindControl("txtSno"), TextBox).Text
    '                    Dim filename As String = CType(grdDetails.Rows(i).FindControl("FileUploadAppPhoto"), FileUpload).FileName
    '                    Dim fileExt As String = Path.GetExtension(filename)
    '                    If LCase(Trim(Mid(filename, InStrRev(filename, ".", Len(filename)), Len(filename)))) <> ".jpg" Then
    '                        lblError.Text = "Invalid File-Type for the Applicant-Photo ! (Only .jpg is allowed)<br/> The Photo for Applicant No. " & Applicant_No & " is NOT UPLOADED for the Registration-ID. : '" & ddlReg.SelectedValue & "'"
    '                        lblError.Visible = True
    '                        Return
    '                    Else
    '                        If CLng(CType(grdDetails.Rows(i).FindControl("FileUploadAppPhoto"), FileUpload).FileBytes.Length) <= 30720 Then  'Max size of the Photo is 30 KB
    '                            If Len(Trim(ddlvillage.SelectedValue)) = 8 Then
    '                                RenamedAppPhoto = Mid(Panchayat_Code, 3, 8) & ddlvillage.SelectedValue & "-" & hf_Family_Id.Value & "-" & Applicant_No & Mid(filename, InStrRev(filename, ".", Len(filename)), Len(filename))
    '                            ElseIf Len(Trim(ddlvillage.SelectedValue)) = 18 Then  ' 18 Digits
    '                                RenamedAppPhoto = Trim(ddlvillage.SelectedValue) & "-" & hf_Family_Id.Value & "-" & Applicant_No & Mid(filename, InStrRev(filename, ".", Len(filename)), Len(filename))
    '                            ElseIf Len(Trim(ddlvillage.SelectedValue)) = 13 Then  ' 18 Digits
    '                                RenamedAppPhoto = Mid(ddlvillage.SelectedValue, 3, 11) & "-" & hf_Family_Id.Value & "-" & Applicant_No & Mid(filename, InStrRev(filename, ".", Len(filename)), Len(filename))
    '                            End If

    '                            Select Case UCase(shortnm)
    '                                Case "OR"
    '                                    pathToFile = Replace(Server.MapPath("~/Photos/" & shortnm & "/"), "Netnrega\", "") & "\" & RenamedAppPhoto
    '                                Case "KL"
    '                                    If Session("finyear_d") = "2011-2012" Then ' Use \Photos\KL1
    '                                        pathToFile = Replace(Replace(Server.MapPath("~/Photos/KL1/"), "Netnrega\", ""), "H:", "I:") & "\" & RenamedAppPhoto
    '                                    Else
    '                                        pathToFile = Replace(Replace(Server.MapPath("~/Photos/" & shortnm & "/"), "Netnrega\", ""), "H:", "I:") & "\" & RenamedAppPhoto
    '                                    End If
    '                                Case Else 'LNDC
    '                                    pathToFile = Replace(Replace(Server.MapPath("~/Photos/" & shortnm & "/"), "Netnrega\", ""), "H:", "I:") & "\" & RenamedAppPhoto
    '                            End Select

    '                            'If shortnm = "OR" Then 'Orissa
    '                            '    pathToFile = Replace(Server.MapPath("~/Photos/" & shortnm & "/"), "Netnrega\", "") & "\" & RenamedAppPhoto
    '                            'Else 'LNDC
    '                            '    pathToFile = Replace(Replace(Server.MapPath("~/Photos/" & shortnm & "/"), "Netnrega\", ""), "H:", "I:") & "\" & RenamedAppPhoto
    '                            'End If

    '                            Photo_Path = "../" & StatePhotoFolderPath & RenamedAppPhoto
    '                            Photo_File = RenamedAppPhoto
    '                            CType(grdDetails.Rows(i).FindControl("FileUploadAppPhoto"), FileUpload).SaveAs(pathToFile)
    '                            CType(grdDetails.Rows(i).FindControl("hf_Photo"), HiddenField).Value = pathToFile + ";" + Photo_Path + ";" + Photo_File

    '                            DT1.Rows.Add(CType(grdDetails.Rows(i).FindControl("hf_Photo"), HiddenField).Value)

    '                            'dbSelectCommand = "Update " & Applicants
    '                            'dbSelectCommand = dbSelectCommand & " set Photo_Path = '" & Photo_Path & "', "
    '                            'dbSelectCommand = dbSelectCommand & " Photo_File = '" & Photo_File & "',"
    '                            'dbSelectCommand = dbSelectCommand & " Entry_by = N'" & Entry_by & "',"
    '                            'dbSelectCommand = dbSelectCommand & " Entry_date = '" & Now & "'"
    '                            'dbSelectCommand = dbSelectCommand & " where Reg_No = '" & ddlReg.SelectedValue & "'"
    '                            'dbSelectCommand = dbSelectCommand & "   and Applicant_No = '" & Applicant_No & "'"
    '                            'dbConnection.ExecuteNonQuery(dbSelectCommand)

    '                            dbSelectCommand = "Update " & Applicants & " set Photo_Path =@Photo_Path,Photo_File=@Photo_File,Entry_by=@Entry_by,Entry_date=@Entry_date where Reg_No=@Reg_No and Applicant_No=@Applicant_No"
    '                            cmd.CommandText = dbSelectCommand
    '                            cmd.Parameters.Clear()
    '                            cmd.Parameters.AddWithValue("@Photo_Path", Photo_Path)
    '                            cmd.Parameters.AddWithValue("@Photo_File", Photo_File)
    '                            cmd.Parameters.Add("@Entry_by", SqlDbType.NVarChar, 100).Value = Entry_by
    '                            cmd.Parameters.AddWithValue("@Entry_date", Now)
    '                            cmd.Parameters.AddWithValue("@Reg_No", ddlReg.SelectedValue)
    '                            cmd.Parameters.AddWithValue("@Applicant_No", Applicant_No)
    '                            dal.ExecuteCommand_rowsaffected(cmd)
    '                        Else
    '                            lblError.Text = "Kindly reduce the size of the Applicant-Photo to be less than 30 KB and upload again !<br/> The Photo for Applicant No. " & Applicant_No & " is NOT UPLOADED for the Registration-ID. :'" & ddlReg.SelectedValue & "'"
    '                            Session("applicantmsg") = lblError.Text
    '                            Return
    '                        End If
    '                    End If
    '                Else
    '                    count = count + 1
    '                    Session("count") = count
    '                    'DT1.Rows.Add("")
    '                End If
    '            Next
    '        End If
    '        Session("Photo") = DT1
    '        If State_Code = "18" Or State_Code = "12" Or State_Code = "04" Or State_Code = "32" Or State_Code = "33" Or State_Code = "34" Then
    '            Response.Redirect("http://164.100.112.66/UploadPhotoSaveNew.aspx")
    '        ElseIf State_Code = "24" Or State_Code = "10" Or State_Code = "14" Or State_Code = "16" Or State_Code = "29" Or State_Code = "30" Or State_Code = "15" Or State_Code = "11" Or State_Code = "27" Or State_Code = "31" Then
    '            Response.Redirect("UploadPhotoSaveNew.aspx")
    '        Else
    '            Response.Redirect("http://nrega.nic.in/UploadPhotoSaveNew.aspx")
    '            'Response.Redirect("UploadPhotoSaveNew.aspx")
    '        End If
    '    End If
    'Catch ex As Exception
    '    lblError.Text = "Error In Uploading Photo"
    '    lblError.Visible = True
    '    Return
    'End Try
    'End Sub

    Protected Sub rdbAABY_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles rdbAABY.SelectedIndexChanged
        If rdbAABY.SelectedValue = "Y" Then
            txtAABY.ReadOnly = False
        ElseIf rdbAABY.SelectedValue = "N" Then
            txtAABY.Text = ""
            txtAABY.ReadOnly = True
        Else
            txtAABY.Text = ""
            txtAABY.ReadOnly = True
        End If
    End Sub
End Class
