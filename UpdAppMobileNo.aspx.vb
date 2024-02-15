Imports System
Imports System.Data
Imports System.Web
Imports System.Data.SqlClient.SqlConnection
Imports System.Data.SqlClient
Imports System.IO
Imports Npgsql
Imports NpgsqlTypes
Partial Class UpdAppMobileNo
    Inherits System.Web.UI.Page
    Dim mycmd As NpgsqlCommand

    Dim DT1 As DataTable
    Dim dal As DAL_VB
    Public HomePage As String
    Dim strbuild As StringBuilder = Nothing
    Dim strShort_Name, Block_Code, Panchayat_Code, state_name, district_name, block_name, finyear, Panchayat_Name, reg, Applicants As String
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            If Session("State_Code_d") = "" Or Session("Entry_type") <> "D" Or Session("finyear_d") = "" Then
                Response.Redirect("logout.aspx", False)
                Response.End()
            End If

            mycmd = New NpgsqlCommand
            dal = DAL_VB.GetInstanceforDE()
            lblmsg.Visible = False

            If Session("Exel_d") = "GP" Then
                Panchayat_Code = Session("Panchayat_Code_d")
                Panchayat_Name = Session("Panchayat_Name_d")
                HomePage = "IndexFrame2.aspx?Panchayat_Code=" & Panchayat_Code
            ElseIf Session("Exel_d") = "PO" Or Session("Exel_d") = "BP" Then
                HomePage = "ProgOfficer/PoIndexFrame2.aspx?Block_Code=" & Block_Code
                Panchayat_Code = ddlpnch.SelectedValue
            End If
            'District_Code = Session("District_Code_d")
            'State_Code = Session("State_Code_d")
            Block_Code = Session("Block_Code_d")
            state_name = Session("State_Name_d")
            district_name = Session("District_Name_d")
            block_name = Session("Block_Name_d")
            finyear = Session("finyear_d")
            strShort_Name = Session("short_name")

            If (Not IsPostBack) Then

                'Part1 ###############CSRF(Cross Script Request Forgery Attack ) Prevention Code####
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
                'Part1 ###############CSRF(Cross Script Request Forgery Attack ) Prevention Code####End##

                Dim Lgn As Lang_Labels
                Lgn = New Lang_Labels(Session("State_Code_d"), "3")
                Dim Ht As Hashtable = New Hashtable
                Ht = Lgn.get_langs(Session("State_Code_d"), "3")
                Dim k As Integer = 0
                lblStatetxt.Text = Ht.Item("state") & ":"
                lblDisttxt.Text = Ht.Item("dist") & ":"
                lblBlocktxt.Text = Ht.Item("blk") & ":"
                lblPanchayattxt.Text = Ht.Item("panch") & ":"
                ViewState("SelectPanch") = Ht.Item("selectpanch")

                lblState.Text = state_name
                lblDistrict.Text = district_name
                lblBlk.Text = block_name

                If Session("Exel_d") = "PO" Or Session("Exel_d") = "BP" Then

                    mycmd.CommandText = "Display_Panchayats"
                    mycmd.CommandType = CommandType.StoredProcedure
                    mycmd.Parameters.Clear()
                    mycmd.Parameters.AddWithValue("par_block_code", NpgsqlDbType.Varchar, Block_Code)
                    mycmd.Parameters.AddWithValue("par_finyr", NpgsqlDbType.Varchar, finyear)

                    DT1 = dal.ExecuteCommand_dt(mycmd, "ref_cursor")

                    ddlpnch.DataSource = DT1
                    ddlpnch.DataTextField = "Panch_Name_Local"
                    ddlpnch.DataValueField = "Panchayat_Code"
                    ddlpnch.DataBind()
                    ddlpnch.Items.Insert(0, ViewState("SelectPanch"))
                    lblmandtry.Visible = True
                    ddlpnch.Visible = True
                    lblPnch.Visible = False

                ElseIf Session("Exel_d") = "GP" Then
                    Call bindvillage()
                    lblPnch.Text = Panchayat_Name
                    ddlpnch.Visible = False
                    lblPnch.Visible = True
                End If

                'Using mycmd = New NpgsqlCommand("select Relation_code,Relation from Applicant_mobile_Relations")
                '    Dim RelTbl As DataTable = dal.ExecuteCommand_dt(mycmd)
                '    If RelTbl.Rows.Count > 0 Then
                '        ViewState("RelTbl") = RelTbl
                '    End If
                'End Using

                Using mycmd = New NpgsqlCommand()
                    mycmd.CommandText = "SpCommon_UpdAppMobileNo"
                    mycmd.CommandType = CommandType.StoredProcedure
                    mycmd.Parameters.AddWithValue("par_query_no", 1)
                    Dim RelTbl As DataTable = dal.ExecuteCommand_dt(mycmd, "ref_cursor")
                    If RelTbl.Rows.Count > 0 Then
                        ViewState("RelTbl") = RelTbl
                    End If
                End Using

            Else
                'Part2 ###############CSRF(Cross Script Request Forgery Attack ) Prevention Code####
                If ASPAuth.Value Is Nothing Or ASPAuth.Value = "" Then
                    Session.Abandon()
                    Session.RemoveAll()
                    Response.Redirect("logout.aspx", False)
                Else
                    If ASPAuth.Value <> Request.Cookies("AuthToken").Value Then
                        Session.Abandon()
                        Session.RemoveAll()
                        Response.Redirect("logout.aspx", False)
                    End If
                End If
                'Part2 ###############CSRF(Cross Script Request Forgery Attack ) Prevention Code####End

                strbuild = New StringBuilder
                reg = strbuild.Append(strShort_Name).Append(Mid(Panchayat_Code, 3, 2)).Append("Registration").ToString
                strbuild.Length = 0
                Applicants = strbuild.Append(strShort_Name).Append(Mid(Panchayat_Code, 3, 2)).Append("Applicants").ToString()

            End If
            lblErrMsg.Text = ""
        Catch ex As NullReferenceException
            lblmsg.Visible = True
            lblmsg.Text = "Null-Error in DAL object creation."
            lblmsg.ForeColor = Drawing.Color.Red
        Catch ex As SqlException
            lblmsg.Visible = True
            lblmsg.Text = "DB-Error found in Page Loading"
            lblmsg.ForeColor = Drawing.Color.Red
        Catch ex As Exception
            lblmsg.Visible = True
            lblmsg.Text = "Error found in Page Loading"
            lblmsg.ForeColor = Drawing.Color.Red
        End Try
    End Sub
    Private Sub bindgrid()
        Try
            Dim S_No As String
            Dim Action As String = ""

            strbuild = New StringBuilder

            'strbuild.Append(" select A.reg_no,applicant_no,applicant_name,isnull(R.Head_of_Household,'') as Head_of_Household, isnull(mobile_no,'') as mobile_no, isnull(MobileAppRelation,'') as Mob_Rel_code from ").Append(Applicants).Append(" A inner join ").Append(reg).Append(" R on A.reg_no=R.reg_no where village_code=@vill_code and (a.event_flag is null OR a.event_flag <> 'D') and (R.event_flag is null OR R.event_flag <> 'D')")

            ''If ddlReg.SelectedValue <> "All" Then
            ''    strbuild.Append("  and A.reg_no=@reg_no")
            ''Else
            ''    If jcr_srch_key.Text <> "" Then
            ''        strbuild.Append("  and A.reg_no like '%/' + @serch_key ")
            ''    End If
            ''End If

            'If jcr_srch_key.Text <> "" Then
            '    strbuild.Append("  and A.reg_no like '%/' + @serch_key ")
            'Else
            '    If Rbl_Add_Edit.SelectedIndex = 0 Then  'Add
            '        strbuild.Append("  and isnull(A.mobile_no,'')=''")
            '    Else 'Edit
            '        strbuild.Append("  and isnull(A.mobile_no,'')<>''")
            '    End If
            'End If

            'strbuild.Append(" order by a.reg_no, a.applicant_no")

            'mycmd = New NpgsqlCommand(strbuild.ToString)
            'mycmd.Parameters.Clear()
            'mycmd.Parameters.AddWithValue("@vill_code", ddlVillage.SelectedValue)
            'mycmd.Parameters.AddWithValue("@serch_key", jcr_srch_key.Text.Trim())
            'DT1 = dal.ExecuteCommand_dt(mycmd)


            If Rbl_Add_Edit.SelectedIndex = 0 Then  'Add
                Action = "Add"
            Else 'Edit
                Action = "Edit"
            End If

            Using mycmd As New NpgsqlCommand()
                mycmd.CommandText = "SpCommon_UpdAppMobileNo"

                mycmd.CommandType = CommandType.StoredProcedure
                mycmd.Parameters.Clear()


                mycmd.Parameters.AddWithValue("par_table_name1", NpgsqlDbType.Varchar, Applicants)
                mycmd.Parameters.AddWithValue("par_table_name2", NpgsqlDbType.Varchar, reg)
                mycmd.Parameters.AddWithValue("par_village_code", NpgsqlDbType.Varchar, ddlVillage.SelectedValue)
                mycmd.Parameters.AddWithValue("par_serch_key", NpgsqlDbType.Varchar, jcr_srch_key.Text.Trim())
                mycmd.Parameters.AddWithValue("par_action", NpgsqlDbType.Varchar, Action)
                mycmd.Parameters.AddWithValue("par_query_no", NpgsqlDbType.Integer, 2)


                DT1 = dal.ExecuteCommand_dt(mycmd, "ref_cursor")

            End Using
            grdData.DataSource = DT1
            grdData.DataBind()

            If (grdData.Rows.Count > 0) Then


                BtnCancel.Visible = True
                BtnUpdate.Visible = True

                Dim count, i, dtrownumber As Integer
                count = 1
                dtrownumber = 0

                For i = 0 To grdData.Rows.Count - 1
                    dtrownumber = (grdData.PageSize * (grdData.PageIndex)) + count - 1
                    CType(grdData.Rows(i).FindControl("lblSno"), Label).Text = (grdData.PageSize * (grdData.PageIndex)) + count
                    S_No = CType(grdData.Rows(i).FindControl("lblSno"), Label).Text
                    CType(grdData.Rows(i).FindControl("txtMobile"), TextBox).Text = DT1.Rows(dtrownumber)("mobile_no")
                    If DT1.Rows(dtrownumber)("Mob_Rel_code") <> String.Empty Then
                        CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList).Items.FindByValue(DT1.Rows(dtrownumber)("Mob_Rel_code")).Selected = True
                    End If
                    count = count + 1
                Next
            Else
                lblmsg.Visible = True
                lblmsg.Text = "No Registration found !"
                BtnCancel.Visible = False
                BtnUpdate.Visible = False
            End If
            'End If
        Catch ex As SqlException
            lblmsg.Visible = True
            lblmsg.Text = "DB-Error found in loading Registration No."
        Catch ex As Exception
            lblmsg.Visible = True
            lblmsg.Text = "Error found in loading Registration No."
        End Try
    End Sub
    Protected Sub BtnUpdate_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles BtnUpdate.Click
        Try
            lblmsg.Visible = False
            lblmsg.Text = ""
            lblErrMsg.Text = False
            lblErrMsg.Text = ""
            Dim NoOfFilledAC = 0
            Dim S_No As Integer
            Dim No_of_App = grdData.Rows.Count
            Dim NoOfValidEntry = 0
            Dim i As Integer
            Dim RecUpdated As Integer = 0
            Dim Applicantno, mob_no, reg_no, errfound, applicant_name As String
            Dim sb = New StringBuilder
            Dim sr_no As New StringBuilder
            Dim DdlRel As DropDownList
            Dim errMsg As String = ""

            For i = 0 To No_of_App - 1
                If CType(grdData.Rows(i).FindControl("chkRowEdit"), CheckBox).Checked = True Then
                    errfound = "N"

                    S_No = Trim(CType(grdData.Rows(i).FindControl("lblSno"), Label).Text)
                    applicant_name = Trim(CType(grdData.Rows(i).FindControl("lblAppName"), Label).Text)
                    reg_no = CType(grdData.Rows(i).FindControl("HF_Reg_No"), HiddenField).Value
                    Applicantno = CType(grdData.Rows(i).FindControl("HF_Applicant_no"), HiddenField).Value
                    DdlRel = CType(grdData.Rows(i).FindControl("ddlRelation"), DropDownList)
                    mob_no = Replace(Trim(CType(grdData.Rows(i).FindControl("txtMobile"), TextBox).Text), vbTab, "")

                    CType(grdData.Rows(i).FindControl("rev_txtmobile"), RegularExpressionValidator).Enabled = True


                    While errfound = "N"

                        If DdlRel.SelectedIndex <= 0 Then
                            If errMsg = "" Then
                                sb.Append("Please select Relation for whom Mobile No. belongs to..!")
                            End If
                            errMsg = sb.ToString
                            ScriptManager.RegisterClientScriptBlock(Me, Me.GetType(), "Alert", "alert('" + sb.ToString + "')", True)
                            Exit While
                        End If

                        If mob_no <> "" Then
                            Dim pattern As String = "^[0-9]+$"
                            If (Len(mob_no) <> 10 Or (Left(mob_no, 1) = "0" Or Left(mob_no, 1) = "1" Or Left(mob_no, 1) = "2" Or Left(mob_no, 1) = "3" Or Left(mob_no, 1) = "4") Or System.Text.RegularExpressions.Regex.IsMatch(mob_no, pattern) = False) Then
                                errfound = "Y"
                                sb.Append("Please enter valid 10 digits mobile number")
                                ScriptManager.RegisterClientScriptBlock(Me, Me.GetType(), "Alert", "alert('" + sb.ToString + "')", True)
                                Exit While
                            End If
                        Else
                            errfound = "Y"
                            sb.Append("Mobile number is mandatory..!!")
                            ScriptManager.RegisterClientScriptBlock(Me, Me.GetType(), "Alert", "alert('" + sb.ToString + "')", True)
                            Exit While
                        End If


                        'Stop Duplicate Mobile number entering by checking in each district but outside job card.
                        'mycmd.CommandText = "[SEARCH_Mobile_NO]"
                        'mycmd.CommandText = "search_mobile_no_jc_wise"
                        'mycmd.CommandType = CommandType.StoredProcedure
                        'mycmd.Parameters.Clear()

                        'mycmd.Parameters.AddWithValue("par_mobile_no", NpgsqlDbType.Varchar, 50).Value = mob_no
                        'mycmd.Parameters.AddWithValue("par_sc", NpgsqlDbType.Varchar, 2).Value = Session("short_name")
                        'mycmd.Parameters.AddWithValue("par_default_tbl_app", NpgsqlDbType.Varchar, 16).Value = Applicants
                        'mycmd.Parameters.AddWithValue("par_reg_no", reg_no)
                        'mycmd.Parameters.AddWithValue("par_regno", NpgsqlDbType.Varchar, 34).Direction = ParameterDirection.Output

                        'dal.ExecuteCommand_rowsaffected(mycmd)




                        'mycmd.Parameters.Add("@mobile_no", SqlDbType.VarChar, 50).Value = mob_no
                        'mycmd.Parameters.Add("@SC", SqlDbType.Char, 2).Value = Session("short_name")
                        'mycmd.Parameters.Add("@default_tbl_app", SqlDbType.VarChar, 16).Value = Applicants
                        'mycmd.Parameters.AddWithValue("@Reg_No", reg_no)
                        'mycmd.Parameters.AddWithValue("@Applicant_No", Applicantno)
                        'mycmd.Parameters.Add("@RegNo", SqlDbType.NVarChar, 34).Direction = ParameterDirection.Output
                        'dal.ExecuteCommand_rowsaffected(mycmd)





                        'Dim AppTbl_Name = mycmd.Parameters("par_regno").Value
                        'If AppTbl_Name <> "" Then  'it means duplicate record found in applicant table as AppTbl_Name
                        'errfound = "Y"
                        'sb.Append("For the Applicant at SNo. : '").Append(S_No).Append("' , Mobile No. '").Append(mob_no).Append("' already exists for another Job card No.[").Append(AppTbl_Name).Append("].")
                        'ScriptManager.RegisterClientScriptBlock(Me, Me.GetType(), "Alert", "alert('Mobile No. already exists for another Job card worker.')", True)
                        'Exit While
                        'End If

                        errfound = "N"
                        If errfound = "N" Then

                            strbuild.Length = 0
                            'strbuild.Append("Update ").Append(Applicants).Append(" set Mobile_no =@mob_no, entry_by=@entry_by, entry_date=getdate(),MobileAppRelation=@MobRel where Reg_No =@reg_no and Applicant_no=@app_no")
                            Using mycmd = New NpgsqlCommand("SpUpd_Mobile_Relation")
                                mycmd.CommandType = CommandType.StoredProcedure

                                mycmd.Parameters.AddWithValue("par_app_tbl", NpgsqlDbType.Varchar, Applicants)
                                mycmd.Parameters.AddWithValue("par_mobile_no", NpgsqlDbType.Varchar, mob_no)
                                mycmd.Parameters.AddWithValue("par_reg_no", NpgsqlDbType.Varchar, reg_no)
                                mycmd.Parameters.AddWithValue("par_app_no", NpgsqlDbType.Integer, System.Int32.Parse(Applicantno))
                                mycmd.Parameters.AddWithValue("par_entry_by", NpgsqlDbType.Varchar, Session("entry_by"))
                                mycmd.Parameters.AddWithValue("par_mobrelcode", NpgsqlDbType.Varchar, DdlRel.SelectedValue.Trim)

                                'mycmd.Parameters.AddWithValue("@App_tbl", Applicants)
                                'mycmd.Parameters.AddWithValue("@mobile_no", mob_no)
                                'mycmd.Parameters.AddWithValue("@Reg_No", reg_no)
                                'mycmd.Parameters.AddWithValue("@app_no", Applicantno)
                                'mycmd.Parameters.AddWithValue("@entry_by", Session("entry_by"))
                                'mycmd.Parameters.AddWithValue("@MobRelCode", DdlRel.SelectedValue.Trim)

                                RecUpdated = dal.ExecuteCommand_rowsaffected(mycmd)
                            End Using

                            If RecUpdated = 0 Then
                                errfound = "Y"
                                sb.Append("(Error in updation..!)")
                            Else
                                CType(grdData.Rows(i).FindControl("chkRowEdit"), CheckBox).Checked = False
                                CType(grdData.Rows(i).FindControl("lbl_status"), Label).Text = "Done"
                                CType(grdData.Rows(i).FindControl("lbl_status"), Label).ForeColor = Drawing.Color.Green

                                NoOfValidEntry = NoOfValidEntry + 1
                                lblmsg.Visible = True
                                lblmsg.ForeColor = Drawing.Color.Blue
                                sr_no = sr_no.Append(S_No).Append(",")
                            End If
                            Exit While
                        End If
                    End While
                    If errfound = "Y" Then
                        sb.Append("...Record not updated for S.No.: ").Append(S_No).Append("<br>")
                    End If
                End If
            Next
            If (grdData.Rows.Count = 0) Then
                BtnCancel.Visible = False
                BtnUpdate.Visible = False
            Else
                BtnUpdate.Visible = True
                BtnCancel.Visible = True
            End If

            If NoOfValidEntry = 0 Then
                lblmsg.Visible = True
                lblmsg.Text = " No mobile number updated ! <br>"
            Else
                strbuild.Length = 0
                lblmsg.Text = strbuild.Append(NoOfValidEntry.ToString).Append(" Record updated ! (S.No.: ").Append(sr_no).Append(")<br>").ToString
                ScriptManager.RegisterClientScriptBlock(Me, Me.GetType(), "Alert", "alert('" + strbuild.ToString + "')", True)
            End If
            lblErrMsg.Visible = True
            lblErrMsg.Text = sb.ToString
        Catch ex As SqlException
            lblmsg.Visible = True
            lblmsg.Text = "DB-Error found in Updating Data."
        Catch ex As Exception
            lblmsg.Visible = True
            lblmsg.Text = "Error found in Updating Data."
        End Try
    End Sub
    Protected Sub BtnCancel_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles BtnCancel.Click
        Response.Redirect(Request.Url.ToString())
    End Sub
    Protected Sub DDL_Panchayat_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlpnch.SelectedIndexChanged
        Call bindvillage()
    End Sub
    Protected Sub ddlVillage_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlVillage.SelectedIndexChanged
        'Call bindReg()
        bindgrid()
    End Sub
    'Protected Sub ddlReg_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlReg.SelectedIndexChanged
    '    bindgrid()
    'End Sub
    Protected Sub grdData_PageIndexChanging(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewPageEventArgs) Handles grdData.PageIndexChanging
        grdData.PageIndex = e.NewPageIndex
        bindgrid()
    End Sub
    Private Sub bindvillage()
        Try
            DT1 = New DataTable()
            'ddlReg.Items.Clear()
            ddlVillage.Items.Clear()
            ddlVillage.ClearSelection()
            'ddlReg.ClearSelection()
            grdData.DataSource = DT1
            grdData.DataBind()
            BtnUpdate.Visible = False
            BtnCancel.Visible = False

            mycmd.CommandText = "Display_Villages"
            mycmd.CommandType = CommandType.StoredProcedure
            mycmd.Parameters.Clear()

            mycmd.Parameters.AddWithValue("par_panchayat_code", NpgsqlDbType.Varchar, Panchayat_Code)
            mycmd.Parameters.AddWithValue("par_finyr", NpgsqlDbType.Varchar, finyear)

            DT1 = dal.ExecuteCommand_dt(mycmd, "ref_cursor")

            If DT1.Rows.Count > 0 Then 'Or ddlpnch.SelectedItem.Text <> "--select panchayat--" 
                'If Session("State_Code_d") = "16" Then
                '    For Each drow As DataRow In DT1.Rows
                '        strbuild.Length = 0
                '        ddlVillage.Items.Add(New ListItem(strbuild.Append(Mid((drow("Village_Code")), 11, 3)).Append("-").Append(drow("Village_Name_Local")).ToString, drow("village_Code")))
                '    Next
                '    ddlVillage.DataBind()
                'Else
                ddlVillage.DataSource = DT1
                ddlVillage.DataTextField = "Village_Name_Local"
                ddlVillage.DataValueField = "village_Code"
                ddlVillage.DataBind()
                'End If
                ddlVillage.Items.Insert(0, "--Select Village--")
                'ddlReg.Items.Insert(0, "--Select Registration no---")
            Else
                lblmsg.Visible = True
                lblmsg.Text = "No Village Found for the selected Panchayat ! "
            End If
        Catch ex As SqlException
            lblmsg.Visible = True
            lblmsg.Text = "DB-Error found in binding Registration No."
        Catch ex As Exception
            lblmsg.Visible = True
            lblmsg.Text = "Error found in binding Registration No."
        End Try
    End Sub
    Protected Sub ImgbtnSearch_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs) Handles ImgbtnSearch.Click
        lblErrMsg.Text = ""
        bindgrid()
    End Sub
    'Private Sub bindReg()
    '    Try
    '        ddlReg.Items.Clear()
    '        grdData.DataSource = DT1
    '        grdData.DataBind()
    '        BtnUpdate.Visible = False
    '        BtnCancel.Visible = False
    '        ddlReg.ClearSelection()

    '        strbuild = New StringBuilder
    '        strbuild.Append("select distinct a.reg_no from ").Append(Applicants).Append(" A inner join ").Append(reg).Append(" R on r.reg_no=a.reg_no ")
    '        'strbuild.Append(" WHERE r.village_code='").Append(ddlVillage.SelectedValue).Append("'")
    '        strbuild.Append(" WHERE r.village_code=@vill_code ")
    '        strbuild.Append(" AND (a.event_flag is null OR a.event_flag <> 'D') and (R.event_flag is null OR R.event_flag <> 'D')")
    '        strbuild.Append(" order by a.reg_no")

    '        mycmd.CommandText = strbuild.ToString
    '        mycmd.CommandType = CommandType.Text
    '        mycmd.Parameters.Clear()
    '        mycmd.Parameters.AddWithValue("@vill_code", ddlVillage.SelectedValue)

    '        DT1 = dal.ExecuteCommand_dt(mycmd)
    '        ddlReg.DataSource = DT1
    '        ddlReg.DataTextField = "Reg_No"
    '        ddlReg.DataValueField = "Reg_No"
    '        ddlReg.DataBind()
    '        If (DT1.Rows.Count > 0) Then
    '            ddlReg.Items.Insert(0, "--Select Reg No.--")
    '            ddlReg.Items.Insert(1, New ListItem("All", "All"))
    '        Else
    '            lblmsg.Visible = True
    '            lblmsg.Text = "No worker found to update mobile no. for the selected Village !"
    '            Return
    '        End If
    '    Catch ex As SqlException
    '        lblmsg.Visible = True
    '        lblmsg.Text = "DB-Error found in binding Registration No."
    '    Catch ex As Exception
    '        lblmsg.Visible = True
    '        lblmsg.Text = "Error found in binding Registration No."
    '    End Try
    'End Sub

    Protected Sub grdData_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles grdData.RowDataBound
        If e.Row.RowType = DataControlRowType.DataRow Then
            Dim ddlRelation As DropDownList = CType(e.Row.FindControl("ddlRelation"), DropDownList)

            If ViewState("RelTbl").Rows.Count > 0 Then
                ddlRelation.DataSource = ViewState("RelTbl")
                ddlRelation.DataTextField = "Relation"
                ddlRelation.DataValueField = "Relation_code"
                ddlRelation.DataBind()
                ddlRelation.Items.Insert(0, New ListItem("Select Relation", "0"))
            End If
        End If
    End Sub

    Protected Sub Rbl_Add_Edit_SelectedIndexChanged(sender As Object, e As EventArgs) Handles Rbl_Add_Edit.SelectedIndexChanged
        bindgrid()
    End Sub
End Class
