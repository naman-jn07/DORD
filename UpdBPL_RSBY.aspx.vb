Imports System
Imports System.Web
Imports System.Data
Imports System.Data.SqlClient.SqlConnection
Imports System.Data.SqlClient
Imports System.Threading
Imports System.Globalization
Imports Npgsql
Imports NpgsqlTypes
Partial Class UpdBPL_RSBY
    Inherits System.Web.UI.Page
    Dim dal As DAL_VB
    Dim DT1 As DataTable
    Dim cmd As NpgsqlCommand
    Dim myreader As NpgsqlDataReader
    Dim dt As Array
    Public HomePage As String
    Dim IsUpdateCat As String
    Dim CasteTillDate As String

    Dim dbSelectCommand, State_Code, District_Code, Caste, PrevCaste, Reg_No, IAY, LR, Small_Farmer, NomadicTribe, denotTribe, BPL_Family_No, More, Minority As String
    Dim Family_Id_2002, BPL_Family, RSBY_Beneficiary, AABY_Beneficiary, RSBY_Card_No, Entry_by, AABY_Insurance_No, reg, Marginal_Farmer, SelectPanch, SelectVill As String
    Dim selectRegNo, strSQL, Block_Code, Applicants, state_name, district_name, Level, block_name, finyear, Home, Panchayat_Name, Panchayat_Code, strShort_Name, MKSP As String
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try

            If Session("State_Code_d") = "" Or Session("Entry_type") <> "D" Then
                Server.Transfer("logout.aspx")
            End If

            Try
                dal = DAL_VB.GetInstanceforDE(Session("State_Code_d"))
            Catch ex As Exception
                lblmsg.Visible = True
                lblmsg.Text = "Error in DAL object creation."
                lblmsg.ForeColor = Drawing.Color.Red
                Return
            End Try
            lblmsg.Visible = False
            State_Code = Session("State_Code_d")
            District_Code = Session("District_Code_d")
            Block_Code = Session("Block_Code_d")
            'Panchayat_Code = session("Panchayat_Code_d")
            state_name = Session("State_Name_d")
            district_name = Session("District_Name_d")
            block_name = Session("Block_Name_d")
            finyear = Session("finyear_d")
            Entry_by = Session("Entry_by")



            If Session("Exel_d") = "GP" Then
                Panchayat_Code = Session("Panchayat_Code_d")
                Panchayat_Name = Session("Panchayat_Name_d")
                HomePage = "IndexFrame2.aspx?Panchayat_Code=" & Panchayat_Code
                Level = "GP"
            Else
                Panchayat_Code = Trim(hf_PanchayatCode.Value)
                HomePage = "ProgOfficer/PoIndexFrame2.aspx?Block_Code=" & Block_Code
                Level = "BP"
            End If
            Session("HomePage") = HomePage
            strShort_Name = Session("short_name")

            Dim Lgn As Lang_Labels
            Lgn = New Lang_Labels(Session("State_Code_d"), "3")
            'reg = Lgn.Short_Name & Right(District_Code, 2) & "Registration"
            'Applicants = Lgn.Short_Name & Right(District_Code, 2) & "Applicants"
            Dim Ht As Hashtable = New Hashtable
            Ht = Lgn.get_langs(Session("State_Code_d"), "3")
            Dim k As Integer = 0
            lblStatetxt.Text = Ht.Item("state") & ":"
            lblDisttxt.Text = Ht.Item("dist") & ":"
            lblBlocktxt.Text = Ht.Item("blk") & ":"
            lblPanchayattxt.Text = Ht.Item("panch") & ":"
            lblVilltxt.Text = Ht.Item("vill") & ":"
            'lblRegNo.Text = Ht.Item("regid") & ":"
            SelectPanch = Ht.Item("selectpanch")
            SelectVill = Ht.Item("selectvill")
            selectRegNo = Ht.Item("selectregno")
            lblState.Text = state_name
            lblDistrict.Text = district_name
            lblBlk.Text = block_name
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
                    'DT1 = CType(dbConnection.ExecuteQuery(dbSelectCommand), DataTable)
                    DT1 = dal.ExecuteCommand_dt(New NpgsqlCommand(dbSelectCommand))
                    If DT1.Rows.Count > 0 Then
                        lblState.Text = Convert.ToString(DT1.Rows(0)("St_Local_Name"))
                        dbSelectCommand = "Execute getStDtBlkPanName_KL @SDBP_Flag='D', @SDBP_Code = '" & District_Code & "', @Language='" & Rdlst.SelectedValue & "'"
                        DT1 = dal.ExecuteCommand_dt(New NpgsqlCommand(dbSelectCommand))
                        lblDistrict.Text = Convert.ToString(DT1.Rows(0)("Dt_Name_Local"))
                        dbSelectCommand = "Execute getStDtBlkPanName_KL @SDBP_Flag='B', @SDBP_Code = '" & Block_Code & "', @Language='" & Rdlst.SelectedValue & "'"
                        DT1 = dal.ExecuteCommand_dt(New NpgsqlCommand(dbSelectCommand))
                        lblBlk.Text = Convert.ToString(DT1.Rows(0)("Blk_Name_Local"))

                        lblState.Text = state_name
                        lblDistrict.Text = district_name
                        lblBlk.Text = block_name
                    End If
                End If
                ViewState("IsUpdateCat") = GetStateChangeCasteTillDate(Session("State_Code_d"))

                If Trim(ViewState("IsUpdateCat")) = "Y" Then
                    spanId.InnerText = "Note: Edit category option has been exempted till date " + CasteTillDate + " afterwords this option will be disabled"
                    spanId.Style.Add("color", "black")
                    spanId.Style.Add("background-color", "yellow")
                    spanId.Visible = True
                Else

                    spanId.Visible = False




                End If


                If Level = "BP" Then

                    Dim mycmd = New NpgsqlCommand
                    mycmd.CommandText = "Display_Panchayats"
                    mycmd.CommandType = CommandType.StoredProcedure
                    mycmd.Parameters.AddWithValue("par_finyr", NpgsqlTypes.NpgsqlDbType.Varchar, finyear)
                    mycmd.Parameters.AddWithValue("par_block_code", NpgsqlTypes.NpgsqlDbType.Varchar, Block_Code)
                    DT1 = dal.ExecuteCommand_dt(mycmd, "ref_cursor")
                    ddlpnch.DataSource = DT1
                    ddlpnch.DataTextField = "Panch_Name_Local"
                    ddlpnch.DataValueField = "Panchayat_Code"
                    ddlpnch.DataBind()
                    ddlpnch.Items.Insert(0, SelectPanch)
                    lblMandry.Visible = True
                    ddlpnch.Visible = True
                    lblPnch.Visible = False
                    ddlvillage.ClearSelection()
                Else

                    If District_Code = "1601" Or District_Code = "1609" Then

                        dbSelectCommand = "Execute getStDtBlkPanName_KL @SDBP_Flag='P', @SDBP_Code = '" & Panchayat_Code & "', @Language='" & Rdlst.SelectedValue & "'"
                        DT1 = dal.ExecuteCommand_dt(New NpgsqlCommand(dbSelectCommand))
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
                    DT1 = dal.ExecuteCommand_dt(New NpgsqlCommand(dbSelectCommand))
                    If State_Code = "16" And ddlpnch.SelectedValue <> SelectPanch Then

                        For Each drow As DataRow In DT1.Rows
                            ddlvillage.Items.Add(New ListItem(Mid((drow("Village_Code")), 11, 3) & "-" & drow("Village_Name_Local"), drow("village_Code")))
                        Next
                        ddlvillage.DataBind()
                    ElseIf Panchayat_Code <> "" And State_Code <> "16" Then
                        ddlvillage.DataSource = DT1
                        ddlvillage.DataTextField = "Village_Name_Local"
                        ddlvillage.DataValueField = "village_Code"
                        ddlvillage.DataBind()
                        ddlvillage.Items.Insert(0, SelectVill)
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

            If Session("Exel_d") = "PO" Then
                Panchayat_Code = ddlpnch.SelectedValue
            End If
            If Len(Panchayat_Code) = 10 Then
                reg = strShort_Name & Mid(Trim(Panchayat_Code), 3, 2) & "Registration"
                Applicants = strShort_Name & Mid(Panchayat_Code, 3, 2) & "Applicants"
            End If
        Catch ex As Exception
            lblmsg.Visible = True
            lblmsg.Text = "Error in Loading Page"
            Return

        End Try
        DT1 = New DataTable()
    End Sub


    Protected Function GetStateChangeCasteTillDate(ByVal StateCode As String) As String
        Dim ChangeCasteTillDate As String
        ChangeCasteTillDate = "N"
        Try
            dal.BeginTransaction()
            'strSQL = "select isnull(ChangeCasteTillDate, DateAdd(month,-2,GETDATE())) as ChangeCasteTillDate from states where state_code='" + StateCode + "'"
            'cmd = New NpgsqlCommand(strSQL)
            'myreader = dal.ExecuteCommand_dr(cmd)

            Using mycmd = New NpgsqlCommand()
                mycmd.CommandText = "SPCommon_UpdBPL_RSBY"
                mycmd.Connection = dal.GetDBConnection()
                mycmd.CommandType = CommandType.StoredProcedure
                mycmd.Parameters.AddWithValue("par_state_code", StateCode)
                mycmd.Parameters.AddWithValue("par_query_no", 1)
                Dim outParam = New NpgsqlParameter("ref_cursor", NpgsqlDbType.Refcursor)
                outParam.Direction = ParameterDirection.Output
                mycmd.Parameters.Add(outParam)
                mycmd.ExecuteNonQuery()
                Using refCursorCommand = New NpgsqlCommand("FETCH ALL IN """ + outParam.Value + """", dal.GetDBConnection())
                    myreader = refCursorCommand.ExecuteReader()
                    While myreader.Read
                        If ((CDate(Date.Now.ToShortDateString()) <= CDate(CDate(myreader("ChangeCasteTillDate")).ToShortDateString()))) Then
                            ChangeCasteTillDate = "Y"
                            CasteTillDate = CDate(myreader("ChangeCasteTillDate")).ToShortDateString()
                        End If
                    End While

                End Using
            End Using
            dal.CommitTransaction()

        Catch ex1 As SqlException
            lblmsg.Visible = True
            lblmsg.Text = "DB-Error found when fetching Exemption Date upto which Edit-option is permitted"
            dal.RollBackTransaction()
        Catch ex As Exception
            lblmsg.Visible = True
            lblmsg.Text = "DB-Error found when fetching Exemption Date upto which Edit-option is permitted."
            dal.RollBackTransaction()
        Finally
            dal.CloseSqlDataReader(myreader)
        End Try
        Return ChangeCasteTillDate
    End Function



    Protected Sub BtnUpdate_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles BtnUpdate.Click
        Try

            'caste value for LR and IAY beneficiery has been seperated into new two columns in REgistration table
            'Changed on : 9-March-2021
            'reason : ST/SC/OBC JCs budget has been seperated.

            Dim NoOfValidEntry As Integer = 0
            Dim FRA, iay_lr As String
            For i = 0 To grdData.Rows.Count - 1
                iay_lr = ""
                If CType(grdData.Rows(i).FindControl("chkRowEdit"), CheckBox).Checked = True Then
                    Reg_No = Trim(CType(grdData.Rows(i).FindControl("HF_HeadRegNo"), HiddenField).Value)
                    If Reg_No <> "" Then

                        If Trim(CType(grdData.Rows(i).FindControl("chkBxIAY"), CheckBox).Checked) Then
                            IAY = "Y"
                        Else
                            IAY = "N"
                        End If

                        If Trim(CType(grdData.Rows(i).FindControl("chkBxLR"), CheckBox).Checked) Then
                            LR = "Y"
                        Else
                            LR = "N"
                        End If


                        'If IAY = "IAY" And LR = "LR" Then
                        '    iay_lr = ";IAY;LR Beneficiary"
                        'Else
                        '    If IAY = "IAY" Then
                        '        iay_lr = ";IAY Beneficiary"
                        '    ElseIf LR = "LR" Then
                        '        iay_lr = ";LR Beneficiary"
                        '    End If
                        'End If

                        Caste = UCase(Trim(CType(grdData.Rows(i).FindControl("rdbCat"), RadioButtonList).SelectedValue))
                        PrevCaste = UCase(Trim(CType(grdData.Rows(i).FindControl("hidCaste"), HiddenField).Value))

                        If Trim(CType(grdData.Rows(i).FindControl("chkNomTribe"), CheckBox).Checked) = True Then
                            NomadicTribe = "Y"
                            denotTribe = "N"
                        Else
                            NomadicTribe = "N"
                        End If
                        If Trim(CType(grdData.Rows(i).FindControl("chkDenotTribe"), CheckBox).Checked) = True Then
                            denotTribe = "Y"
                            NomadicTribe = "N"
                        Else
                            denotTribe = "N"
                        End If

                        If Trim(CType(grdData.Rows(i).FindControl("chkMinority"), CheckBox).Checked) = True Then
                            Minority = "Y"
                        Else
                            Minority = "N"
                        End If

                        If Trim(CType(grdData.Rows(i).FindControl("chkBPL"), CheckBox).Checked) = True Then
                            BPL_Family = "Y"
                            BPL_Family_No = Trim(CType(grdData.Rows(i).FindControl("txtBPLId"), TextBox).Text)
                        Else
                            BPL_Family = "N"
                            BPL_Family_No = ""
                        End If
                        If Trim(CType(grdData.Rows(i).FindControl("chkRSBY"), CheckBox).Checked) = True Then
                            RSBY_Beneficiary = "Y"
                            RSBY_Card_No = Trim(CType(grdData.Rows(i).FindControl("txtRSBYNo"), TextBox).Text)
                        Else
                            RSBY_Beneficiary = "N"
                            RSBY_Card_No = ""
                        End If
                        If Trim(CType(grdData.Rows(i).FindControl("chkAABY"), CheckBox).Checked) = True Then
                            AABY_Beneficiary = "Y"
                            AABY_Insurance_No = Trim(CType(grdData.Rows(i).FindControl("txtAABYNo"), TextBox).Text)
                        Else
                            AABY_Beneficiary = "N"
                            AABY_Insurance_No = ""
                        End If
                        If Trim(CType(grdData.Rows(i).FindControl("chkSmallFarmer"), CheckBox).Checked) = True Then
                            Small_Farmer = "Y"
                            Marginal_Farmer = "N"
                        Else
                            Small_Farmer = "N"
                        End If
                        If Trim(CType(grdData.Rows(i).FindControl("chkMargFarmer"), CheckBox).Checked) = True Then
                            Marginal_Farmer = "Y"
                            Small_Farmer = "N"
                        Else
                            Marginal_Farmer = "N"
                        End If

                        Family_Id_2002 = Trim(CType(grdData.Rows(i).FindControl("txtFamilyId"), TextBox).Text)
                        If Trim(CType(grdData.Rows(i).FindControl("chkFRA"), CheckBox).Checked) = True Then
                            FRA = "Y"
                        Else
                            FRA = "N"
                        End If

                        If Trim(CType(grdData.Rows(i).FindControl("chkMKSP"), CheckBox).Checked) = True Then
                            MKSP = "Y"
                        Else
                            MKSP = "N"
                        End If


                        Dim RecUpdated As Integer = 0

                        'dbSelectCommand = "Update " & reg & " set "
                        ''caste should not edit after SC/ST budget have been seperated.code Changed on 09-mar-2021
                        ''dbSelectCommand = dbSelectCommand & " Caste = '" & Caste & "',"

                        ''new coloumn implemented on 09-Mar-2021 instead of caste. 
                        'dbSelectCommand = dbSelectCommand & " IS_LR_Beneficiary = '" & LR & "',"
                        'dbSelectCommand = dbSelectCommand & " IS_IAY_Beneficiary = '" & IAY & "',"

                        'dbSelectCommand = dbSelectCommand & " Minority = '" & Minority & "',"
                        'dbSelectCommand = dbSelectCommand & " BPL_Family = '" & BPL_Family & "',"
                        'dbSelectCommand = dbSelectCommand & " BPL_Family_No = '" & BPL_Family_No & "',"
                        'dbSelectCommand = dbSelectCommand & " RSBY_Beneficiary = '" & RSBY_Beneficiary & "',"
                        'dbSelectCommand = dbSelectCommand & " RSBY_Card_No = '" & RSBY_Card_No & "',"
                        'dbSelectCommand = dbSelectCommand & " AABY_Beneficiary = '" & AABY_Beneficiary & "',"
                        'dbSelectCommand = dbSelectCommand & " AABY_Insurance_No = '" & AABY_Insurance_No & "',"
                        'dbSelectCommand = dbSelectCommand & " Small_Farmer = '" & Small_Farmer & "',"
                        'dbSelectCommand = dbSelectCommand & " Marginal_Farmer = '" & Marginal_Farmer & "',"
                        'dbSelectCommand = dbSelectCommand & " nomadic_tribe = '" & NomadicTribe & "',"
                        'dbSelectCommand = dbSelectCommand & " denotified_tribe = '" & denotTribe & "',"
                        'dbSelectCommand = dbSelectCommand & " Family_Id_2002 = '" & Family_Id_2002 & "',"
                        'dbSelectCommand = dbSelectCommand & " Entry_by = N'" & Entry_by & "',"
                        'dbSelectCommand = dbSelectCommand & " FRA_Beneficiary = '" & FRA & "'," 'new column add
                        'dbSelectCommand = dbSelectCommand & " MKSP_farmer = '" & MKSP & "'," 'new column addded on 14/09/2016
                        'dbSelectCommand = dbSelectCommand & " Entry_date = getdate() where Reg_No = '" & Reg_No & "'"
                        'RecUpdated = dal.ExecuteCommand_rowsaffected(New NpgsqlCommand(dbSelectCommand))

                        'TODO : Akshay to update.

                        Using cmd = New NpgsqlCommand()
                            cmd.CommandText = "UpdBPL_RSBY_FRA_Category"
                            cmd.CommandType = CommandType.StoredProcedure
                            cmd.Parameters.Clear()


                            cmd.Parameters.AddWithValue("p_tblreg", NpgsqlTypes.NpgsqlDbType.Varchar, reg)
                            cmd.Parameters.AddWithValue("p_state_code", NpgsqlTypes.NpgsqlDbType.Varchar, Session("state_code_d"))
                            cmd.Parameters.AddWithValue("p_reg_no", NpgsqlTypes.NpgsqlDbType.Varchar, Reg_No)
                            cmd.Parameters.AddWithValue("p_is_lr_beneficiary", NpgsqlTypes.NpgsqlDbType.Varchar, LR)
                            cmd.Parameters.AddWithValue("p_is_iay_beneficiary", NpgsqlTypes.NpgsqlDbType.Varchar, IAY)
                            cmd.Parameters.AddWithValue("p_minority", NpgsqlTypes.NpgsqlDbType.Varchar, Minority)
                            cmd.Parameters.AddWithValue("p_bpl_family", NpgsqlTypes.NpgsqlDbType.Varchar, BPL_Family)
                            cmd.Parameters.AddWithValue("p_bpl_family_no", NpgsqlTypes.NpgsqlDbType.Varchar, BPL_Family_No)
                            cmd.Parameters.AddWithValue("p_rsby_beneficiary", NpgsqlTypes.NpgsqlDbType.Varchar, RSBY_Beneficiary)
                            cmd.Parameters.AddWithValue("p_rsby_card_no", NpgsqlTypes.NpgsqlDbType.Varchar, RSBY_Card_No)
                            cmd.Parameters.AddWithValue("p_aaby_beneficiary", NpgsqlTypes.NpgsqlDbType.Varchar, AABY_Beneficiary)
                            cmd.Parameters.AddWithValue("p_aaby_insurance_no", NpgsqlTypes.NpgsqlDbType.Varchar, AABY_Insurance_No)
                            cmd.Parameters.AddWithValue("p_small_farmer", NpgsqlTypes.NpgsqlDbType.Varchar, Small_Farmer)
                            cmd.Parameters.AddWithValue("p_marginal_farmer", NpgsqlTypes.NpgsqlDbType.Varchar, Marginal_Farmer)
                            cmd.Parameters.AddWithValue("p_nomadic_tribe", NpgsqlTypes.NpgsqlDbType.Varchar, NomadicTribe)
                            cmd.Parameters.AddWithValue("p_denotified_tribe", NpgsqlTypes.NpgsqlDbType.Varchar, denotTribe)
                            cmd.Parameters.AddWithValue("p_family_id_2002", NpgsqlTypes.NpgsqlDbType.Varchar, Family_Id_2002)
                            cmd.Parameters.AddWithValue("p_entry_by", NpgsqlTypes.NpgsqlDbType.Varchar, 50).Value = Entry_by
                            cmd.Parameters.AddWithValue("p_fra_beneficiary", NpgsqlTypes.NpgsqlDbType.Varchar, FRA)
                            cmd.Parameters.AddWithValue("p_mksp_farmer", NpgsqlTypes.NpgsqlDbType.Varchar, MKSP)
                            cmd.Parameters.AddWithValue("p_caste", NpgsqlTypes.NpgsqlDbType.Varchar, Caste)
                            cmd.Parameters.AddWithValue("p_prevcaste", NpgsqlTypes.NpgsqlDbType.Varchar, PrevCaste)
                            cmd.Parameters.AddWithValue("p_isupdatecat", NpgsqlTypes.NpgsqlDbType.Varchar, ViewState("IsUpdateCat"))





                            'cmd.Parameters.AddWithValue("@TblReg", reg)
                            'cmd.Parameters.AddWithValue("@state_code", Session("state_code_d"))
                            'cmd.Parameters.AddWithValue("@Reg_no", Reg_No)
                            'cmd.Parameters.AddWithValue("@IS_LR_Beneficiary", LR)
                            'cmd.Parameters.AddWithValue("@IS_IAY_Beneficiary", IAY)
                            'cmd.Parameters.AddWithValue("@Minority", Minority)
                            'cmd.Parameters.AddWithValue("@BPL_Family", BPL_Family)
                            'cmd.Parameters.AddWithValue("@BPL_Family_No", BPL_Family_No)
                            'cmd.Parameters.AddWithValue("@RSBY_Beneficiary", RSBY_Beneficiary)
                            'cmd.Parameters.AddWithValue("@RSBY_Card_No", RSBY_Card_No)
                            'cmd.Parameters.AddWithValue("@AABY_Beneficiary", AABY_Beneficiary)
                            'cmd.Parameters.AddWithValue("@AABY_Insurance_No", AABY_Insurance_No)
                            'cmd.Parameters.AddWithValue("@Small_Farmer", Small_Farmer)
                            'cmd.Parameters.AddWithValue("@Marginal_Farmer", Marginal_Farmer)
                            'cmd.Parameters.AddWithValue("@nomadic_tribe", NomadicTribe)
                            'cmd.Parameters.AddWithValue("@denotified_tribe", denotTribe)
                            'cmd.Parameters.AddWithValue("@Family_Id_2002", Family_Id_2002)
                            'cmd.Parameters.Add("@Entry_by", SqlDbType.NVarChar, 50).Value = Entry_by
                            'cmd.Parameters.AddWithValue("@FRA_Beneficiary", FRA)
                            'cmd.Parameters.AddWithValue("@MKSP_farmer", MKSP)
                            'cmd.Parameters.AddWithValue("@Caste", Caste)
                            'cmd.Parameters.AddWithValue("@PrevCaste", PrevCaste)
                            'cmd.Parameters.AddWithValue("@IsUpdateCat", ViewState("IsUpdateCat"))

                            RecUpdated = dal.ExecuteCommand_rowsaffected(cmd)
                        End Using

                        If RecUpdated <> 0 Then
                            NoOfValidEntry = NoOfValidEntry + 1
                        End If

                        grdData.Visible = False
                        msgUpd.Visible = False
                        BtnUpdate.Visible = False
                        BtnCancel.Visible = False
                        trMore.Visible = True
                        hf_VillageCode.Value = ddlvillage.SelectedValue
                        More = "UpdBPL_RSBY.aspx"
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

            If NoOfValidEntry = 0 Then
                lblmsg.Visible = True
                lblmsg.Text = " No records updated ! "
                lblmsg.ForeColor = Drawing.Color.Red
            Else
                lblmsg.Visible = True
                lblmsg.Text = NoOfValidEntry.ToString & " record(s) UPDATED...! "
                lblmsg.ForeColor = Drawing.Color.Green
            End If

        Catch ex As SqlException
            lblmsg.Text = "SQL-Error found while Updating Records ! "
            lblmsg.Visible = True
        Catch ex1 As Exception
            lblmsg.Text = "Error found while Updating Records ! "
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

            Dim mycmd = New NpgsqlCommand
            mycmd.CommandType = CommandType.StoredProcedure
            mycmd.CommandText = "Display_Villages"
            mycmd.Parameters.AddWithValue("par_finyr", NpgsqlDbType.Varchar, finyear)
            mycmd.Parameters.AddWithValue("par_panchayat_code", NpgsqlDbType.Varchar, ddlpnch.SelectedValue)

            hf_PanchayatCode.Value = ddlpnch.SelectedValue
            DT1 = dal.ExecuteCommand_dt(mycmd, "ref_cursor")
            If State_Code = "16" Then
                ddlvillage.Items.Insert(0, SelectVill)
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
                    ddlvillage.Items.Insert(0, SelectVill)
                ElseIf DT1.Rows.Count <= 0 And ddlpnch.SelectedValue <> SelectPanch Then
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
            Dim search_fam_id As String = ""
            search_fam_id = Replace(LTrim(RTrim(jcr_srch_key.Text)), vbTab, "")
            If search_fam_id Is Nothing Then
                search_fam_id = ""
            End If
            If ddlvillage.SelectedValue <> SelectVill Then

                'dbSelectCommand = "select Reg_No, Head_of_Household, isnull(Caste,'') as Caste, Minority, Family_id, Isnull(BPL_Family,'') as BPL_Family, Isnull(BPL_Family_No,'') as BPL_Family_No, Isnull(RSBY_Beneficiary,'') as RSBY_Beneficiary, Isnull(RSBY_Card_No,'') as RSBY_Card_No, isnull(Small_Farmer,'') as Small_Farmer, isnull(Marginal_Farmer,'') as Marginal_Farmer,"
                'dbSelectCommand = dbSelectCommand & " isnull(nomadic_tribe,'') as nomadic_tribe,isnull(denotified_tribe,'') as denotified_tribe,isnull(Family_Id_2002,'') as Family_Id_2002 , Isnull(BPL_Data,'') as BPL_Data, isnull(AABY_Beneficiary,'') as AABY_Beneficiary, isnull(AABY_Insurance_No,'') as AABY_Insurance_No,isnull(FRA_Beneficiary,'') as FRA_Beneficiary,isnull(MKSP_farmer,'') as MKSP_farmer, isnull(IS_IAY_Beneficiary,'') as IS_IAY_Beneficiary,isnull(IS_LR_Beneficiary,'') as IS_LR_Beneficiary from " & reg
                'dbSelectCommand = dbSelectCommand & " where Village_Code ='" & ddlvillage.SelectedValue & "'"
                'dbSelectCommand = dbSelectCommand & " and (Event_Flag is NULL or Event_Flag<>'D') "
                'If search_fam_id <> String.Empty Then
                '    dbSelectCommand = dbSelectCommand & " and Reg_No like '%/" & search_fam_id & "'"
                'End If
                'dbSelectCommand = dbSelectCommand & " order by CONVERT(BIGINT, LEFT(Family_Id, PATINDEX('%[^0-9]%', Family_Id + ' ') - 1))"   'here patindex returns index of first occurence of specified paturn e.g (%[^0-9]%) in expression e.g(Family_Id + ' ')

                'DT1 = dal.ExecuteCommand_dt(New NpgsqlCommand(dbSelectCommand))


                Using mycmd = New NpgsqlCommand()
                    mycmd.CommandText = "SPCommon_UpdBPL_RSBY"
                    mycmd.CommandType = CommandType.StoredProcedure
                    mycmd.Parameters.AddWithValue("par_table_name", NpgsqlDbType.Varchar, reg)
                    mycmd.Parameters.AddWithValue("par_village_code", NpgsqlDbType.Varchar, ddlvillage.SelectedValue)
                    mycmd.Parameters.AddWithValue("par_search_fam_id", NpgsqlDbType.Varchar, search_fam_id)
                    mycmd.Parameters.AddWithValue("par_query_no", NpgsqlDbType.Integer, 2)
                    DT1 = dal.ExecuteCommand_dt(mycmd, "ref_cursor")
                End Using

                If DT1.Rows.Count > 0 Then
                    grdData.DataSource = DT1
                    grdData.DataBind()
                    HF_countrow.Value = grdData.Rows.Count
                    Dim count, dtrownumber As Integer
                    count = 1
                    For i = 0 To grdData.Rows.Count - 1

                        dtrownumber = (grdData.PageSize * (grdData.PageIndex)) + count - 1
                        CType(grdData.Rows(i).FindControl("lblSno"), Label).Text = (grdData.PageSize * (grdData.PageIndex)) + count
                        CType(grdData.Rows(i).FindControl("lblHead"), Label).Text = DT1.Rows(dtrownumber)("Head_Of_Household").ToString()
                        CType(grdData.Rows(i).FindControl("lblRegNo"), Label).Text = "(" + DT1.Rows(dtrownumber)("Reg_No").ToString() + ")"
                        CType(grdData.Rows(i).FindControl("HF_HeadRegNo"), HiddenField).Value = DT1.Rows(dtrownumber)("Reg_No").ToString()

                        'Disabled radiobuttonList for Caste means edit is not allowed in caste value. 

                        'Mail Ref: One time provision in MIS for rectification of wrong classification of works under Mahatma Gandhi NREGA.
                        'Option used for Kerala for a specific period of time as per Approved later by PD. 
                        'If Session("state_code_d") = "16" AndAlso (Date.Now < CDate("2021-06-27")) Then
                        '    CType(grdData.Rows(i).FindControl("rdbCat"), RadioButtonList).Enabled = True
                        'Else
                        '    CType(grdData.Rows(i).FindControl("rdbCat"), RadioButtonList).Enabled = False
                        'End If

                        If Trim(ViewState("IsUpdateCat")) = "Y" Then
                            CType(grdData.Rows(i).FindControl("rdbCat"), RadioButtonList).Enabled = True
                        Else
                            CType(grdData.Rows(i).FindControl("rdbCat"), RadioButtonList).Enabled = False
                        End If

                        If Trim(DT1.Rows(dtrownumber)("Caste").ToString()) = "" Then
                            CType(grdData.Rows(i).FindControl("rdbCat"), RadioButtonList).SelectedValue = "OTH"
                            'CType(grdData.Rows(i).FindControl("rdbCat"), RadioButtonList).ClearSelection()
                            CType(grdData.Rows(i).FindControl("hidCaste"), HiddenField).Value = "OTH"
                        Else
                            If InStr(DT1.Rows(dtrownumber)("Caste").ToString(), "SC") > 0 Then
                                CType(grdData.Rows(i).FindControl("rdbCat"), RadioButtonList).SelectedValue = "SC"
                                CType(grdData.Rows(i).FindControl("hidCaste"), HiddenField).Value = "SC"
                            ElseIf InStr(DT1.Rows(dtrownumber)("Caste").ToString(), "ST") > 0 Then
                                CType(grdData.Rows(i).FindControl("rdbCat"), RadioButtonList).SelectedValue = "ST"
                                CType(grdData.Rows(i).FindControl("hidCaste"), HiddenField).Value = "ST"
                            ElseIf InStr(DT1.Rows(dtrownumber)("Caste").ToString(), "OBC") > 0 Then
                                CType(grdData.Rows(i).FindControl("rdbCat"), RadioButtonList).SelectedValue = "OBC"
                                CType(grdData.Rows(i).FindControl("hidCaste"), HiddenField).Value = "OBC"
                            Else
                                CType(grdData.Rows(i).FindControl("rdbCat"), RadioButtonList).SelectedValue = "OTH"
                                CType(grdData.Rows(i).FindControl("hidCaste"), HiddenField).Value = "OTH"
                            End If
                        End If

                        'If Trim(DT1.Rows(dtrownumber)("Caste").ToString()) = "" Then
                        '    CType(grdData.Rows(i).FindControl("hf_IAY"), HiddenField).Value = ""
                        '    CType(grdData.Rows(i).FindControl("hf_LR"), HiddenField).Value = ""
                        'Else
                        '    If InStr(DT1.Rows(dtrownumber)("Caste").ToString(), "IAY") > 0 Then
                        '        CType(grdData.Rows(i).FindControl("chkBxIAY"), CheckBox).Checked = True
                        '        CType(grdData.Rows(i).FindControl("chkBxIAY"), CheckBox).Enabled = False
                        '    End If
                        '    If InStr(DT1.Rows(dtrownumber)("Caste").ToString(), "LR") > 0 Then
                        '        CType(grdData.Rows(i).FindControl("chkBxLR"), CheckBox).Checked = True
                        '    End If
                        'End If

                        'caste value for LR and IAY beneficiery has been seperated into new two columns in REgistration table
                        'Changed on : 17-March-2021
                        'reason : ST/SC/OBC JCs budget has been seperated.

                        If Trim(DT1.Rows(dtrownumber)("IS_IAY_Beneficiary").ToString()) = "Y" Then
                            CType(grdData.Rows(i).FindControl("chkBxIAY"), CheckBox).Checked = True
                            CType(grdData.Rows(i).FindControl("chkBxIAY"), CheckBox).Enabled = False
                        End If
                        If Trim(DT1.Rows(dtrownumber)("IS_LR_Beneficiary").ToString()) = "Y" Then
                            CType(grdData.Rows(i).FindControl("chkBxLR"), CheckBox).Checked = True
                        End If

                        If Trim(DT1.Rows(dtrownumber)("nomadic_tribe").ToString()) = "Y" Then
                            CType(grdData.Rows(i).FindControl("chkNomTribe"), CheckBox).Checked = True
                        End If
                        If Trim(DT1.Rows(dtrownumber)("denotified_tribe").ToString()) = "Y" Then
                            CType(grdData.Rows(i).FindControl("chkDenotTribe"), CheckBox).Checked = True
                        End If

                        If Trim(DT1.Rows(dtrownumber)("Minority").ToString()) = "Y" Then

                            CType(grdData.Rows(i).FindControl("chkMinority"), CheckBox).Checked = True

                        End If

                        CType(grdData.Rows(i).FindControl("lblFamilyId"), Label).Text = DT1.Rows(dtrownumber)("Family_id").ToString()
                        CType(grdData.Rows(i).FindControl("txtBPLId"), TextBox).Text = DT1.Rows(dtrownumber)("BPL_Family_No").ToString()
                        CType(grdData.Rows(i).FindControl("txtRSBYNo"), TextBox).Text = DT1.Rows(dtrownumber)("RSBY_Card_No").ToString()
                        CType(grdData.Rows(i).FindControl("txtAABYNo"), TextBox).Text = DT1.Rows(dtrownumber)("AABY_Insurance_No").ToString()
                        If Trim(DT1.Rows(dtrownumber)("BPL_Family").ToString()) = "Y" Then
                            CType(grdData.Rows(i).FindControl("chkBPL"), CheckBox).Checked = True
                            If CType(grdData.Rows(i).FindControl("txtBPLId"), TextBox).Text = "" Then
                                CType(grdData.Rows(i).FindControl("txtBPLId"), TextBox).ReadOnly = False
                            Else
                                CType(grdData.Rows(i).FindControl("txtBPLId"), TextBox).ReadOnly = True
                            End If
                        Else
                            CType(grdData.Rows(i).FindControl("chkBPL"), CheckBox).Checked = False
                            CType(grdData.Rows(i).FindControl("txtBPLId"), TextBox).ReadOnly = True
                        End If

                        If Trim(DT1.Rows(dtrownumber)("RSBY_Beneficiary").ToString()) = "Y" Then
                            CType(grdData.Rows(i).FindControl("chkRSBY"), CheckBox).Checked = True
                            If CType(grdData.Rows(i).FindControl("txtRSBYNo"), TextBox).Text = "" Then
                                CType(grdData.Rows(i).FindControl("txtRSBYNo"), TextBox).ReadOnly = False
                            Else
                                CType(grdData.Rows(i).FindControl("txtRSBYNo"), TextBox).ReadOnly = True
                            End If
                        Else
                            CType(grdData.Rows(i).FindControl("chkRSBY"), CheckBox).Checked = False
                            CType(grdData.Rows(i).FindControl("txtRSBYNo"), TextBox).ReadOnly = True
                        End If

                        If Trim(DT1.Rows(dtrownumber)("AABY_Beneficiary").ToString()) = "Y" Then
                            CType(grdData.Rows(i).FindControl("chkAABY"), CheckBox).Checked = True
                            If CType(grdData.Rows(i).FindControl("txtAABYNo"), TextBox).Text = "" Then
                                CType(grdData.Rows(i).FindControl("txtAABYNo"), TextBox).ReadOnly = False
                            Else
                                CType(grdData.Rows(i).FindControl("txtAABYNo"), TextBox).ReadOnly = True
                            End If
                        Else
                            CType(grdData.Rows(i).FindControl("chkAABY"), CheckBox).Checked = False
                            CType(grdData.Rows(i).FindControl("txtAABYNo"), TextBox).ReadOnly = True
                        End If
                        If Trim(DT1.Rows(dtrownumber)("Small_Farmer").ToString()) = "Y" Then
                            CType(grdData.Rows(i).FindControl("chkSmallFarmer"), CheckBox).Checked = True
                        End If
                        If Trim(DT1.Rows(dtrownumber)("Marginal_Farmer").ToString()) = "Y" Then
                            CType(grdData.Rows(i).FindControl("chkMargFarmer"), CheckBox).Checked = True
                        End If

                        CType(grdData.Rows(i).FindControl("txtFamilyId"), TextBox).Text = DT1.Rows(dtrownumber)("Family_Id_2002").ToString()
                        If Trim(DT1.Rows(dtrownumber)("FRA_Beneficiary").ToString()) = "Y" Then
                            CType(grdData.Rows(i).FindControl("chkFRA"), CheckBox).Checked = True
                        End If

                        'implementeed on 14/9/2016
                        If Trim(DT1.Rows(dtrownumber)("MKSP_farmer").ToString()) = "Y" Then
                            CType(grdData.Rows(i).FindControl("chkMKSP"), CheckBox).Checked = True
                        End If

                        'If DT1.Rows(dtrownumber)("FRA_Beneficiary").ToString() = "Y" Then
                        '    CType(grdData.Rows(i).FindControl("RblFRA"), RadioButtonList).Text = "Yes"
                        '    CType(grdData.Rows(i).FindControl("RblFRA"), RadioButtonList).SelectedValue = "Y"
                        'ElseIf DT1.Rows(dtrownumber)("FRA_Beneficiary") = "N" Then
                        '    CType(grdData.Rows(i).FindControl("RblFRA"), RadioButtonList).Text = "No"
                        '    CType(grdData.Rows(i).FindControl("RblFRA"), RadioButtonList).SelectedValue = "N"
                        'End If

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

    Protected Sub chk_CheckedChanged1(ByVal sender As Object, ByVal e As System.EventArgs) Handles chk.CheckedChanged
        Try

            DT1 = New DataTable()
            Dim rownumber = Convert.ToInt32((CType(sender, CheckBox).ClientID.ToString().Split("_")(3)).Replace("ctl", "")) - 2

            If CType(grdData.Rows(rownumber).FindControl("chkBPL"), CheckBox).Checked = True Then
                If CType(grdData.Rows(rownumber).FindControl("txtBPLId"), TextBox).Text = "" Then
                    CType(grdData.Rows(rownumber).FindControl("txtBPLId"), TextBox).ReadOnly = False
                Else
                    CType(grdData.Rows(rownumber).FindControl("txtBPLId"), TextBox).ReadOnly = True
                End If
            Else
                CType(grdData.Rows(rownumber).FindControl("txtBPLId"), TextBox).Text = ""
                CType(grdData.Rows(rownumber).FindControl("txtBPLId"), TextBox).ReadOnly = True

            End If

        Catch ex As Exception
            lblmsg.Text = "Error In binding Data...BPL"
            lblmsg.Visible = True
        End Try

    End Sub
    Protected Sub chk_CheckedChanged2(ByVal sender As Object, ByVal e As System.EventArgs) Handles chk.CheckedChanged
        Try

            DT1 = New DataTable()
            Dim rownumber = Convert.ToInt32((CType(sender, CheckBox).ClientID.ToString().Split("_")(3)).Replace("ctl", "")) - 2

            If CType(grdData.Rows(rownumber).FindControl("chkRSBY"), CheckBox).Checked = True Then
                If CType(grdData.Rows(rownumber).FindControl("txtRSBYNo"), TextBox).Text = "" Then
                    CType(grdData.Rows(rownumber).FindControl("txtRSBYNo"), TextBox).ReadOnly = False
                Else
                    CType(grdData.Rows(rownumber).FindControl("txtRSBYNo"), TextBox).ReadOnly = True
                End If
            Else
                CType(grdData.Rows(rownumber).FindControl("txtRSBYNo"), TextBox).Text = ""
                CType(grdData.Rows(rownumber).FindControl("txtRSBYNo"), TextBox).ReadOnly = True
            End If

        Catch ex As Exception
            lblmsg.Text = "Error In binding Data...RSBY"
            lblmsg.Visible = True
        End Try

    End Sub
    Protected Sub chk_CheckedChanged3(ByVal sender As Object, ByVal e As System.EventArgs) Handles chk.CheckedChanged
        Try

            DT1 = New DataTable()
            Dim rownumber = Convert.ToInt32((CType(sender, CheckBox).ClientID.ToString().Split("_")(3)).Replace("ctl", "")) - 2

            If CType(grdData.Rows(rownumber).FindControl("chkAABY"), CheckBox).Checked = True Then
                If CType(grdData.Rows(rownumber).FindControl("txtAABYNo"), TextBox).Text = "" Then
                    CType(grdData.Rows(rownumber).FindControl("txtAABYNo"), TextBox).ReadOnly = False
                Else
                    CType(grdData.Rows(rownumber).FindControl("txtAABYNo"), TextBox).ReadOnly = True
                End If
            Else
                CType(grdData.Rows(rownumber).FindControl("txtAABYNo"), TextBox).Text = ""
                CType(grdData.Rows(rownumber).FindControl("txtAABYNo"), TextBox).ReadOnly = True
            End If
            'Next
        Catch ex As Exception
            lblmsg.Text = "Error In binding Data...AABY"
            lblmsg.Visible = True
        End Try

    End Sub

    Protected Sub LnkMore_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles LnkMore.Click
        ddlvillage_SelectedIndexChanged(sender, e)
        trMore.Visible = False
    End Sub

    Protected Sub grdData_RowDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewRowEventArgs) Handles grdData.RowDataBound
        Try

            For i = 0 To grdData.Rows.Count - 1
                If CType(grdData.Rows(i).FindControl("chkBPL"), CheckBox).Checked = True Then
                    If CType(grdData.Rows(i).FindControl("txtBPLId"), TextBox).Text = "" Then
                        CType(grdData.Rows(i).FindControl("txtBPLId"), TextBox).ReadOnly = False
                    Else
                        CType(grdData.Rows(i).FindControl("txtBPLId"), TextBox).ReadOnly = True
                    End If
                Else
                    CType(grdData.Rows(i).FindControl("txtBPLId"), TextBox).Text = ""
                    CType(grdData.Rows(i).FindControl("txtBPLId"), TextBox).ReadOnly = True

                End If
                If CType(grdData.Rows(i).FindControl("chkRSBY"), CheckBox).Checked = True Then
                    If CType(grdData.Rows(i).FindControl("txtRSBYNo"), TextBox).Text = "" Then
                        CType(grdData.Rows(i).FindControl("txtRSBYNo"), TextBox).ReadOnly = False
                    Else
                        CType(grdData.Rows(i).FindControl("txtRSBYNo"), TextBox).ReadOnly = True
                    End If
                Else
                    CType(grdData.Rows(i).FindControl("txtRSBYNo"), TextBox).Text = ""
                    CType(grdData.Rows(i).FindControl("txtRSBYNo"), TextBox).ReadOnly = True
                End If
                If CType(grdData.Rows(i).FindControl("chkAABY"), CheckBox).Checked = True Then
                    If CType(grdData.Rows(i).FindControl("txtAABYNo"), TextBox).Text = "" Then
                        CType(grdData.Rows(i).FindControl("txtAABYNo"), TextBox).ReadOnly = False
                    Else
                        CType(grdData.Rows(i).FindControl("txtAABYNo"), TextBox).ReadOnly = True
                    End If
                Else
                    CType(grdData.Rows(i).FindControl("txtAABYNo"), TextBox).Text = ""
                    CType(grdData.Rows(i).FindControl("txtAABYNo"), TextBox).ReadOnly = True
                End If
            Next
        Catch ex As Exception
            lblmsg.Text = "Error In binding Data...RowDataBound"
            lblmsg.Visible = True
        End Try
    End Sub
    Protected Sub grdData_PageIndexChanging(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewPageEventArgs) Handles grdData.PageIndexChanging
        grdData.PageIndex = e.NewPageIndex
        Call ddlvillage_SelectedIndexChanged(sender, e)
    End Sub
    Protected Sub ImgbtnSearch_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs) Handles ImgbtnSearch.Click
        Call ddlvillage_SelectedIndexChanged(sender, e)
    End Sub

End Class
