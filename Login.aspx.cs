using System;
using System.Web;
using System.Data;
using System.Text;
using System.Diagnostics;
using System.Web.Security;
using System.Data.SqlClient;
using Npgsql;
using System.Web.SessionState;
using log4net;
using java.io;
using ADODB;
using System.Activities.Expressions;
using NpgsqlTypes;
using System.Web.UI;
using System.Web.UI.WebControls;
using Org.BouncyCastle.Security;


public partial class Login : System.Web.UI.Page
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    private DAL_VB DAL;
    private NpgsqlCommand my_cmd = null;
    private NpgsqlDataReader my_red = null;

    StringBuilder o_StringBuild = new StringBuilder("");
    private Login_Checks LoginChk = new Login_Checks();
    public string Yr = null, state_code, my_query, Login_lvl, valid_dba_login, Val_LoginHistory, Return_LoginHistory, Result_ValFinYr, salt, Fin, Ret_BlockFTO_Record, Res_BlockedDist, Res_BlockFTO, Result_DistRec, Ret_StateFTO, UnAuth_IP, Auth_IP_State, Static_State, Redirect_Url, Ret_PanchRecord, Ret_Designation, User_IPAdd, Res_LogAud, PW, Res_Captcha, Ret_Blank_Check = "", PW_Flag_Test, Salt_Random;
    long minute = 0;

    protected void Page_Load(object sender, EventArgs e)//A
    {
        log.Info("Inside Page_Load function...");
        try
        {
            if (Request.QueryString["level"] != null)
            {
                User_IPAdd = Request.UserHostAddress.ToString();
                UnAuth_IP = "10.24.218.78";
                Auth_IP_State = "10.131.23.93";
                Static_State = "11";
                Login_lvl = Request.QueryString["level"].ToString();
                if (Session["Entry_type"] != null && Session["Entry_type"].ToString() == "D")
                {
                    Server.Transfer("logout.aspx");
                }
                if (Request.QueryString["state_code"] == null || Request.QueryString["state_code"] == "")
                {
                    Session.Abandon();
                    Response.Redirect("http://nrega.nic.in");
                }
                else
                {
                    state_code = Request.QueryString["state_code"].ToString();
                }
                DAL = DAL_VB.GetInstanceforDE(state_code);
                FormsAuthentication.SignOut();
                txt_UserID.Text= "3499480840";
                txt_UserID.Attributes.Add("autocomplete", "off");

                txt_Captcha.Attributes.Add("autocomplete", "off");
                txt_Password.Attributes.Add("autocomplete", "off");
                // dynSalt.Value = Session.SessionID;
                if (Login_lvl == "Homestciti")
                {
                    TR_Role.Visible = true;
                }
                else
                {
                    TR_Role.Visible = false;
                }
               // dynSalt = Get_PassSalt();
                if (!Page.IsPostBack)
                {
                    // dynSalt.Value = LoginChk.Get_PassSalt();
                    TR_Role.Visible = false;
                    State(); 
                    DDL_FinYear();
                    switch (Login_lvl)
                    {
                        case "HomeGP":
                            Div_District.Visible = true;
                            Div_Block.Visible = true;
                            Div_Panchayat.Visible = true;
                            ddl_District.Items.Insert(0, "Select District");
                            ddl_Block.Items.Insert(0, "Select Block");
                            ddl_Panch.Items.Insert(0, "Select Panchayat");
                            lbl_Heading.Text = "GRAM PANCHAYAT DATA ENTRY LOGIN";
                            break;
                        case "HomeBP":
                            Div_District.Visible = true;
                            Div_Block.Visible = true;
                            Div_Panchayat.Visible = false;
                            ddl_District.Items.Insert(0, "Select District");
                            ddl_Block.Items.Insert(0, "Select Block");
                            lbl_Heading.Text = "BLOCK PANCHAYAT DATA ENTRY LOGIN";
                            break;
                        case "HomeZP":
                            Div_District.Visible = true;
                            Div_Block.Visible = false;
                            Div_Panchayat.Visible = false;
                            ddl_District.Items.Insert(0, "Select District");
                            lbl_Heading.Text = "DISTRICT PANCHAYAT DATA ENTRY LOGIN";
                            break;
                        case "HomePO":
                            Div_District.Visible = true;
                            Div_Block.Visible = true;
                            Div_Panchayat.Visible = false;
                            ddl_District.Items.Insert(0, "Select District");
                            ddl_Block.Items.Insert(0, "Select Block");
                            lbl_Heading.Text = "PROGRAMME OFFICER LOGIN";
                            System.Console.Write("Opening HomePO");
                            break;
                        case "HomePODBA":
                            Div_District.Visible = true;
                            Div_Block.Visible = true;
                            Div_Panchayat.Visible = false;
                            ddl_District.Items.Insert(0, "Select District");
                            ddl_Block.Items.Insert(0, "Select Block");
                            lbl_Heading.Text = "BLOCK ADMINISTRATOR LOGIN";
                            break;
                        case "HomeDPC":
                            Div_District.Visible = true;
                            Div_Block.Visible = false;
                            Div_Panchayat.Visible = false;
                            lbl_Heading.Text = "DISTRICT PROGRAMME COORDINATOR LOGIN";
                            ddl_FinYr.SelectedItem.Text = Request.QueryString["fin_year"].ToString();
                            ddl_FinYr.Enabled = false;
                            Rqd_ddlFin.Enabled = false;
                            Fin = ddl_FinYr.SelectedItem.ToString();
                            Yr = Fin.Substring(2, 2) + Fin.Substring(7, 2);
                            Session["Yr"] = Yr;
                            Session["finyear_d"] = ddl_FinYr.SelectedItem.ToString();
                            Session["fin_year_d"] = ddl_FinYr.SelectedItem.ToString();                           
                            DDL_District();
                            ddl_District.SelectedValue = Request.QueryString["District_Code"].ToString();
                            ddl_District.Enabled = true;
                            Rqd_ddlDistrict.Enabled = true;
                            break;
                        case "HomeDPCDBA":
                            Div_District.Visible = true;
                            Div_Block.Visible = false;
                            Div_Panchayat.Visible = false;
                            ddl_District.Items.Insert(0, "Select District");
                            lbl_Heading.Text = "DPC ADMINISTRATOR LOGIN";
                            break;
                        case "Homestciti":
                            TR_Role.Visible = true;
                            DDL_Role();
                            Div_District.Visible = false;
                            Div_Block.Visible = false;
                            Div_Panchayat.Visible = false;
                            ddl_District.Items.Insert(0, "Select District");
                            ddl_Block.Items.Insert(0, "Select Block");
                            lbl_Heading.Text = "STATE LOGIN";
                            if (Request.QueryString["state_name"] != null)
                            {
                                lbl_State.Text = Request.QueryString["state_name"].ToString();
                            }
                            if (Request.QueryString["fin_year"] != null)
                            {
                                ddl_FinYr.SelectedItem.Text = Request.QueryString["fin_year"].ToString();
                                ddl_FinYr.Enabled = false;
                                Rqd_ddlFin.Enabled = false;
                                Session["fin_year"] = Request.QueryString["fin_year"].ToString();
                            }
                            Session["type"] = "S";
                            break;
                    }
                    //Captcha                  
                    Refresh_Captcha();
                    if (string.IsNullOrEmpty(Res_Captcha) && (!Res_Captcha.StartsWith("ERROR")))
                    {
                        Context.Session["CaptchaImageText"] = Res_Captcha;
                    }
                }
                if (ddl_FinYr.SelectedIndex > 0)
                {
                    Fin = ddl_FinYr.SelectedItem.ToString();
                    Yr = Fin.Substring(2, 2) + Fin.Substring(7, 2);
                    Session["finyear_d"] = ddl_FinYr.SelectedItem.ToString();
                    Session["fin_year_d"] = ddl_FinYr.SelectedItem.ToString();
                }
                if (ddl_FinYr.SelectedItem.Text != "Select Financial Year")
                {
                    if (Login_lvl == "HomeDPC" || Login_lvl == "Homestciti")
                    {
                        Session["finyear_d"] = ddl_FinYr.SelectedItem.Text;
                        Session["fin_year_d"] = ddl_FinYr.SelectedItem.Text;
                        Fin = Session["finyear_d"].ToString();
                        Yr = Fin.Substring(2, 2) + Fin.Substring(7, 2);
                    }
                }
            }
            else
            {
                Server.Transfer("logout.aspx");
            }
        }
        catch (NullReferenceException)
        {
            Message("Null - Error(A-001) occured on page. Please try later.");
        }
        catch (SqlException)
        {
            Message("DB- Error(A-002) occured on page. Please try later.");
        }
        catch (Exception)
        {
            Message("Error(A-003) occured on page. Please try later.");
        }

    }
    
    protected void DDL_FinYear()//B
    {

        try
        {

            NpgsqlConnection conn = DAL.GetDBConnection();
            DAL.BeginTransaction();
            using (conn)
            {
                
                using (my_cmd = new NpgsqlCommand("poc_nrega_jh_nrega_jh.cboFinyear", conn))
                {
                    my_cmd.CommandType = CommandType.StoredProcedure;
                    my_cmd.CommandTimeout = 0;
                    my_cmd.Parameters.Clear();
                    var outParam = new NpgsqlParameter("p_refcur", NpgsqlDbType.Refcursor)
                    {
                        Direction = ParameterDirection.Output
                    };
                    my_cmd.Parameters.Add(outParam);

                    //1. ExecuteNonQuery
                    my_cmd.ExecuteNonQuery();


                    //my_red = FETCH ALL Query
                    using (var refCursorCommand = new NpgsqlCommand("FETCH ALL IN \"" + outParam.Value + "\"", conn))
                    {
                        using (my_red = refCursorCommand.ExecuteReader())
                        {
                            ddl_FinYr.DataSource = my_red;
                            ddl_FinYr.DataBind();
                            ddl_FinYr.Items.Insert(0, "Please select Fin year");
                            ddl_FinYr.Items[0].Value = "0";
                        }
                    }
                }
                DAL.CommitTransaction();
            }
            
        }
        catch (NullReferenceException)
        {
            Message("Null - Error(B-001) occured on Financial Year. Please try later.");
            DAL.RollBackTransaction();
        }
        catch (SqlException)
        {
            Message("DB - Error(B-002) occured on Financial Year. Please try later.");
            DAL.RollBackTransaction();
        }
        catch (Exception)
        {
            Message("Error(B-003) occured on Financial Year. Please try later.");
            DAL.RollBackTransaction();
        }
        finally
        {
            if ((my_red != null && my_red.IsClosed == false))
            {
                DAL.CloseSqlDataReader(my_red);
                my_cmd.Dispose();
            }
        }
    }

    public void State() 
    {
        try
        {
            DAL.BeginTransaction();
            NpgsqlConnection conn = DAL.GetDBConnection();
            
                using (var command = new NpgsqlCommand("cbostatewithcode", conn))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    //assuming theses parmater for cbostatewithcode
                    command.Parameters.AddWithValue("par_state_code", NpgsqlDbType.Varchar, "34"); // Example parameter

                    // Define the output parameter for the refcursor
                    var outParam = new NpgsqlParameter("p_refcur", NpgsqlDbType.Refcursor)

                    {
                        Direction = ParameterDirection.Output
                    };

                    command.Parameters.Add(outParam);

                    // Execute the command
                    command.ExecuteNonQuery();


                    // Fetch the result set from the refcursor
                    using (var refCursorCommand = new NpgsqlCommand("FETCH ALL IN \"" + outParam.Value + "\"", conn))
                    {
                        using (my_red = refCursorCommand.ExecuteReader())
                        {
                            if (my_red.Read())
                            {
                                lbl_State.Text = Convert.ToString(my_red["state_name"]);
                                ViewState["shortname"] = Convert.ToString(my_red["short_name"]);
                                ViewState["lang_code"] = Convert.ToString(my_red["lang_code"]);
                                ViewState["lang_name"] = Convert.ToString(my_red["lang_name"]);
                                ViewState["exempt_dem"] = Convert.ToString(my_red["exempt_dem"]);
                                ViewState["exempt_msr_100days"] = Convert.ToString(my_red["exempt_msr_100days"]);
                                ViewState["flag_debited"] = Convert.ToString(my_red["flag_debited"]);
                                ViewState["fund_trans_dr_ac"] = Convert.ToString(my_red["fund_trans_dr_ac"]);
                                ViewState["exempt_avl"] = Convert.ToString(my_red["exempt_avl"]);
                                ViewState["fund_trans_dr_ac_additional"] = Convert.ToString(my_red["fund_trans_dr_ac_additional"]);
                                ViewState["anticipated_entry"] = Convert.ToString(my_red["anticipated_entry"]);
                                ViewState["finyear_anti"] = Convert.ToString(my_red["finyear_anti"]);
                                ViewState["anticipated_dt_upto"] = Convert.ToString(my_red["anticipated_dt_upto"]);
                                ViewState["exempt6040"] = Convert.ToString(my_red["exempt6040"]);
                                ViewState["exempt6040_dt"] = Convert.ToString(my_red["exempt6040_dt"]);
                                ViewState["mark_if"] = Convert.ToString(my_red["mark_if"]);
                                ViewState["exempt_LB_dmd"] = Convert.ToString(my_red["exempt_LB_dmd"]);
                                ViewState["exempt_LB_dmd_dt"] = Convert.ToString(my_red["exempt_LB_dmd_dt"]);
                                ViewState["state_name"] = Convert.ToString(my_red["state_name"]);

                                //Add session for archived 16/feb/2021
                                ViewState["Is_archived"] = Convert.ToString(my_red["Is_archived"]);
                                ViewState["Archived_finyear_upto"] = Convert.ToString(my_red["Archived_finyear_upto"]);
                                ViewState["Archived_schema"] = Convert.ToString(my_red["Archived_schema"]);
                            }
                        }
                    }


                }

            
            DAL.CommitTransaction();
        }
        
        catch (NullReferenceException)
        {
            Message("Null - Error(C-001) occured on State Details. Please try later.");
            DAL.RollBackTransaction();
        }
        catch (SqlException)
        {
            Message("DB - Error(C-002) occured on State Details. Please try later.");
            DAL.RollBackTransaction();
        }
        catch (Exception)
        {
            Message("Error(C-001) occured on State Details. Please try later.");
            DAL.RollBackTransaction();
        }
        finally
        {
            if ((my_red != null && my_red.IsClosed == false))
            {
                DAL.CloseSqlDataReader(my_red);
                //my_cmd.Dispose();
            }
        }
    }

    protected void ddl_FinYr_SelectedIndexChanged(object sender, EventArgs e)//D
    {
        try
        {
            if (ddl_FinYr.SelectedIndex != 0)
            {
                Session["Fin"] = ddl_FinYr.SelectedValue;
                DDL_District();
                lblmsg.Visible = false;
            }
            else
            {
                ddl_District.Items.Clear();
                ddl_District.Items.Insert(0, "Select District");
                ddl_Block.Items.Clear();
                ddl_Block.Items.Insert(0, "Select Block");
                ddl_Panch.Items.Clear();
                ddl_Panch.Items.Insert(0, "Select Panchayat");
                lblmsg.Visible = false;
                txt_UserID.Text = "";
                txt_Password.Text = "";
            }
        }
        catch (SqlException)
        {
            Message("DB - Error(D-001) occured on Financial Year Selection. Please try later.");
        }
        catch (Exception)
        {
            Message("Error(D-002) occured on Financial Year Selection. Please try later.");
        }
    }

    public void DDL_District()
    {
        try
        {
            DAL.BeginTransaction();
            using (NpgsqlConnection conn = DAL.GetDBConnection())
            {
                //conn.Open();

                using (var command = new NpgsqlCommand("poc_nrega_jh_nrega_jh.display_districts_rajesh", conn))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Assuming 'display_districtsfunc' takes these parameters
                    command.Parameters.AddWithValue("par_state_code", NpgsqlDbType.Varchar, "34"); // Example parameter
                    command.Parameters.AddWithValue("par_finyr", NpgsqlDbType.Varchar, "2022-2023"); // Example parameter
                    command.Parameters.AddWithValue("par_yr", NpgsqlDbType.Varchar, "1"); // Example parameter

                    // Define the output parameter for the refcursor
                    var outParam = new NpgsqlParameter("ref_cursor", NpgsqlDbType.Refcursor)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(outParam);

                    // Execute the command
                    command.ExecuteNonQuery();

                    // Fetch the result set from the refcursor
                    using (var refCursorCommand = new NpgsqlCommand("FETCH ALL IN \"" + outParam.Value + "\"", conn))
                    {
                        using (NpgsqlDataReader dr = refCursorCommand.ExecuteReader())
                        {
                            ddl_District.DataSource = dr;
                            ddl_District.DataTextField = "district_name"; // Adjust as per actual column name
                            ddl_District.DataValueField = "district_code"; // Adjust as per actual column name
                            ddl_District.DataBind();
                        }
                    }
                }

                // Insert the default item at the first position
                ddl_District.Items.Insert(0, new ListItem("Select District", ""));
            }
            //DAL.CommitTransaction();
        }
        catch (Exception ex)
        {
            // Handle exceptions
            Message("Error occurred while retrieving districts: " + ex.Message);
        }
        finally
        {
            if ((my_red != null && my_red.IsClosed == false))
            {
                DAL.CloseSqlDataReader(my_red);
            }
        }
    }


    public void DDL_Block()//F
    {
        try
        {
            DAL.BeginTransaction();
            using (NpgsqlConnection conn = DAL.GetDBConnection())
            {
                //conn.Open();

                using (var command = new NpgsqlCommand("poc_nrega_jh_nrega_jh.display_blocks", conn))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    //assuming theses parmater for display_blocks
                    command.Parameters.AddWithValue("par_district_code", NpgsqlDbType.Varchar, "3401"); // Example parameter
                    command.Parameters.AddWithValue("par_finyr", NpgsqlDbType.Varchar, "2022-2023"); // Example parameter

                    // Define the output parameter for the refcursor
                    var outParam = new NpgsqlParameter("ref_cursor", NpgsqlDbType.Refcursor)

                    {
                        Direction = ParameterDirection.Output
                    };

                    command.Parameters.Add(outParam);

                    // Execute the command
                    command.ExecuteNonQuery();

                    // Fetch the result set from the refcursor
                    using (var refCursorCommand = new NpgsqlCommand("FETCH ALL IN \"" + outParam.Value + "\"", conn))
                    {
                        using (my_red = refCursorCommand.ExecuteReader())
                        {
                            ddl_Block.DataSource = my_red;
                            ddl_Block.DataTextField = "Block_Name";
                            ddl_Block.DataValueField = "Block_Code";
                            ddl_Block.DataBind();
                            ddl_Block.Items.Insert(0, "Select Block");
                        }
                    }
                }
            }
            DAL.CommitTransaction();
        }
        catch (NullReferenceException)
        {
            Message("Null - Error(F-001) occured on Block Bind. Please try later.");
        }
        catch (SqlException)
        {
            Message("DB - Error(F-002) occured on Block Bind. Please try later.");
        }
        catch (Exception)
        {
            Message("Error(F-003) occured on Block Bind. Please try later.");
        }
        finally
        {
            if ((my_red != null && my_red.IsClosed == false))
            {
                DAL.CloseSqlDataReader(my_red);
            }
        }
    }

    public void DDL_Panchayat()//G
    {
        try
        {
            my_cmd = new NpgsqlCommand();
            my_cmd.CommandText = "display_panchayats";
            my_cmd.CommandType = CommandType.StoredProcedure;
            my_cmd.CommandTimeout = 0;
            my_cmd.Parameters.Clear();
            my_cmd.Parameters.Add("@Block_code", NpgsqlTypes.NpgsqlDbType.Varchar, 7).Value = ddl_Block.SelectedValue.ToString();
            my_cmd.Parameters.Add("@finyr", NpgsqlTypes.NpgsqlDbType.Varchar, 9).Value = ddl_FinYr.SelectedItem.Text;
            my_red = DAL.ExecuteCommand_dr(my_cmd);
            ddl_Panch.DataSource = my_red;
            ddl_Panch.DataTextField = "Panchayat_name";
            ddl_Panch.DataValueField = "Panchayat_code";
            ddl_Panch.DataBind();
            ddl_Panch.Items.Insert(0, "Select Panchayat");
        }
        catch (NullReferenceException)
        {
            Message("Null - Error(G-001) occured on Panchayat Bind. Please try later.");
        }
        catch (SqlException)
        {
            Message("DB - Error(G-002) occured on Panchayat Bind. Please try later.");
        }
        catch (Exception)
        {
            Message("Error(G-003) occured on Panchayat Bind. Please try later.");
        }
        finally
        {
            if ((my_red != null && my_red.IsClosed == false))
            {
                DAL.CloseSqlDataReader(my_red);
            }
        }
    }

    public void DDL_Role()//H
    {
        try
        {
            my_cmd = new NpgsqlCommand();
            my_cmd.CommandText = "SELECT role_code, role_desc FROM rolemaster where (role_level = 'ST') and Role_Code not in ('STACC','STSEC')";
            my_cmd.CommandType = CommandType.Text;
            my_red = DAL.ExecuteCommand_dr(my_cmd);
            ddl_Role.DataSource = my_red;
            ddl_Role.DataTextField = "role_desc";
            ddl_Role.DataValueField = "role_code";
            ddl_Role.DataBind();
            ddl_Role.Items.Insert(0, "Select Role");
        }
        catch (NullReferenceException)
        {
            Message("Null - Error(H-001) occured on Role Bind. Please try later.");
        }
        catch (SqlException)
        {
            Message("DB - Error(H-002) occured on Role Bind. Please try later.");
        }
        catch (Exception)
        {
            Message("Error(H-003) occured on Role Bind. Please try later.");
        }
        finally
        {
            if ((my_red != null && my_red.IsClosed == false))
            {
                DAL.CloseSqlDataReader(my_red);
            }
        }
    }

    protected void ddl_District_SelectedIndexChanged(object sender, EventArgs e)//I
    {
        try
        {
            if (ddl_District.SelectedIndex != 0)
            {
                if (Login_lvl == "HomeGP" || Login_lvl == "HomeBP" || Login_lvl == "HomePO" || Login_lvl == "HomePODBA" || Login_lvl == "HomeBA")
                {
                    Session["District"] = ddl_District.SelectedValue;
                    DDL_Block();
                }
                if (Login_lvl == "HomeZP" || Login_lvl == "HomeDPC")
                {
                    Session["District"] = ddl_District.SelectedValue;
                }
                lblmsg.Visible = false;
            }
            else
            {
                ddl_Block.Items.Clear();
                ddl_Block.Items.Insert(0, "Select Block");
                ddl_Panch.Items.Clear();
                ddl_Panch.Items.Insert(0, "Select Panchayat");
                txt_UserID.Text = "";
                txt_Password.Text = "";
                lblmsg.Visible = false;
            }
        }
        catch (SqlException)
        {
            Message("DB - Error(I-001) occured on District selection. Please try later.");
        }
        catch (Exception)
        {
            Message("Error(I-002) occured on District selection. Please try later.");
        }
    }

    public void Message(string Msg_text)//J
    {
        try
        {
            txt_UserID.Text = "";
            txt_Password.Text = "";
            Refresh_Captcha();
            lblmsg.Text = Msg_text;
            lblmsg.Visible = true;
        }
        catch (Exception)
        {
            Message("Error(J-001) occured on Messages display. Please try later.");
        }
    }

    protected void ddl_Block_SelectedIndexChanged(object sender, EventArgs e)//K
    {
        try
        {
            if (ddl_Block.SelectedIndex != 0)
            {
                if (Login_lvl == "HomeGP")
                {
                    Session["Block"] = ddl_Block.SelectedValue;
                    DDL_Panchayat();
                }
                if (Login_lvl == "HomeBP" || Login_lvl == "HomeZP" || Login_lvl == "HomePO" || Login_lvl == "Homestciti")
                {
                    Session["Block"] = ddl_Block.SelectedValue;
                }
                lblmsg.Visible = false;
            }
            else
            {
                ddl_Panch.Items.Clear();
                ddl_Panch.Items.Insert(0, "Select Panchayat");
                txt_UserID.Text = "";
                txt_Password.Text = "";
                lblmsg.Visible = false;
            }
        }
        catch (SqlException)
        {
            Message("DB - Error(K-001) occured on Block selection. Please try later.");
        }
        catch (Exception)
        {
            Message("Error(K-002) occured on Block selection. Please try later.");
        }
    }

    protected void ddl_Panch_SelectedIndexChanged(object sender, EventArgs e)//L
    {
        try
        {
            if (ddl_Panch.SelectedIndex != 0)
            {
                if (Login_lvl == "HomeGP")
                {
                    Session["panch"] = ddl_Panch.SelectedValue;
                }
                lblmsg.Visible = false;
            }
            else
            {
                txt_UserID.Text = "";
                txt_Password.Text = "";
                lblmsg.Visible = false;
            }
        }
        catch (NullReferenceException)
        {
            Message("Null - Error(L-001) occured on Panchayat selection. Please try later.");
        }
        catch (Exception)
        {
            Message("Error(L-002) occured on Panchayat selection. Please try later.");
        }
    }

    public string State_Record()//M
    {
        try
        {
            o_StringBuild = new StringBuilder();
            o_StringBuild.Length = 0;
            if (Session["Result_Designation"].ToString() == "STDBA")
            {
                my_query = o_StringBuild.Append("select state_Code,state_name, short_name, lang_code, isnull(lang_name,'English') lang_name,isnull(trans_ac_exempt,'N') trans_ac_exempt, isnull(fund_trans_dr_ac,'N') fund_trans_dr_ac, isnull(fund_trans_dr_ac_additional,'N') fund_trans_dr_ac_additional, Nefms_date from STATES where state_Code='").Append(state_code).Append("'").ToString();

            }
            else
            {
                my_query = o_StringBuild.Append("select state_Code,state_name, short_name, lang_code, isnull(lang_name,'English') lang_name,isnull(trans_ac_exempt,'N') trans_ac_exempt, isnull(fund_trans_dr_ac,'N') fund_trans_dr_ac, isnull(fund_trans_dr_ac_additional,'N') fund_trans_dr_ac_additional, Nefms_date, isnull(flag_debited,'N') flag_debited from STATES where state_Code='").Append(state_code).Append("'").ToString();
            }
            my_cmd = new NpgsqlCommand();
            my_cmd.CommandText = my_query;
            my_cmd.CommandType = CommandType.Text;
            my_red = DAL.ExecuteCommand_dr(my_cmd);
            if (my_red.Read())
            {
                Session["trans_ac_exempt"] = Convert.ToString(my_red["trans_ac_exempt"]).Trim();
                Session["fund_trans_dr_ac"] = Convert.ToString(my_red["fund_trans_dr_ac"]).Trim();
                Session["lang_name"] = Convert.ToString(my_red["lang_name"]).Trim();
                Session["lang_code"] = Convert.ToString(my_red["lang_code"]).Trim();
                Session["short_name"] = Convert.ToString(my_red["short_name"]).Trim();
                Session["state_name"] = Convert.ToString(my_red["state_name"]).Trim();
                Session["state_name_d"] = Convert.ToString(my_red["state_name"]);
                Session["fund_trans_dr_ac_additional"] = Convert.ToString(my_red["fund_trans_dr_ac_additional"]).Trim();
                Session["Nefms_date"] = Convert.ToString(my_red["Nefms_date"]).Trim();
                if (Session["Result_Designation"].ToString() != "STDBA")
                {
                    Session["flag_debited"] = Convert.ToString(my_red["flag_debited"]).Trim();
                }
                return "1";
            }
            else
            {
                o_StringBuild.Length = 0;
                return o_StringBuild.Append("Account of Financial year ").Append(Request.QueryString["fin_year"].ToString()).Append(" has been closed!").ToString();
            }
        }
        catch (NullReferenceException)
        {
            return "Null - Error(M-001) occured on State Record. Please try later.";
        }
        catch (SqlException)
        {
            return "DB - Error(M-002) occured on State Record. Please try later.";
        }
        catch (Exception)
        {
            return "Error(M-003) occured on State Record. Please try later.";
        }
        finally
        {
            if ((my_red != null && my_red.IsClosed == false))
            {
                DAL.CloseSqlDataReader(my_red);
                my_cmd.Dispose();
            }
        }
    }

    public string District_Record()//N
    {
        try
        {
            my_query = "Select COALESCE(drought_effective,'N') drought_effective,COALESCE(pm_dbt_flag,'N') pm_dbt_flag,COALESCE(Exempt_pre_dem_uid,'N') Exempt_pre_dem_uid,COALESCE(Exempt_cur_dem_uid,'N') Exempt_cur_dem_uid, COALESCE(Exempt_bill_uid,'N') Exempt_bill_uid, COALESCE(closeaccdpc,'0') as closeaccDPC from poc_nrega_jh_nrega_jh.districts where district_code=@District_code";
            my_cmd = new NpgsqlCommand();
            my_cmd.Parameters.Clear();
            my_cmd.CommandText = my_query;
            my_cmd.Parameters.Add("@District_code", NpgsqlTypes.NpgsqlDbType.Char, 4).Value = ddl_District.SelectedValue.Trim();
            my_cmd.CommandType = CommandType.Text;
            my_red = DAL.ExecuteCommand_dr(my_cmd);
            if (my_red.Read())
            {
                Session["drought_effective"] = Convert.ToString(my_red["drought_effective"]);
                Session["pm_dbt_flag"] = Convert.ToString(my_red["pm_dbt_flag"]);
                Session["Exempt_pre_dem_uid"] = Convert.ToString(my_red["Exempt_pre_dem_uid"]);
                Session["Exempt_cur_dem_uid"] = Convert.ToString(my_red["Exempt_cur_dem_uid"]);
                Session["Exempt_bill_uid"] = Convert.ToString(my_red["Exempt_bill_uid"]);
                if (Login_lvl == "HomeDPC")
                {
                    Session["close_d"] = Convert.ToString(my_red["closeaccDPC"]);
                }
                Result_DistRec = "1";
            }
            return Result_DistRec;
        }
        catch (NullReferenceException)
        {
            return Result_DistRec = "Null - Error(N-001) occured on District Record. Please try later.";
        }
        catch (SqlException)
        {
            return Result_DistRec = "DB - Error(N-002) occured on District Record. Please try later.";
        }
        catch (Exception)
        {
            return Result_DistRec = "Error(N-003) occured on District Record. Please try later.";
        }
        finally
        {
            if ((my_red != null && my_red.IsClosed == false))
            {
                DAL.CloseSqlDataReader(my_red);
                my_cmd.Dispose();
            }
        }
    }

    public void Read_StaffDetails()//O
    {
            try
                {
                    my_query = "select Staff_id, designation_code, additional_charge, COALESCE(PW_Flag) as PW_Flag from poc_nrega_jh_nrega_jh.functionary_detail_level where Userid = @UserID";
                    my_cmd = new NpgsqlCommand();
                    my_cmd.Parameters.Clear();
                    my_cmd.Parameters.Add("@UserID", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Int64.Parse(txt_UserID.Text.Trim());
                    my_cmd.CommandText = my_query;
                    my_cmd.CommandType = CommandType.Text;
                    my_red = DAL.ExecuteCommand_dr(my_cmd);
                    if (my_red.Read())
                    {
                        Session["staff_id"] = Convert.ToString(my_red["Staff_id"]).Trim();
                        Session["designation_code"] = Convert.ToString(my_red["designation_code"]).Trim();
                        Session["staff_type"] = "STAFF";
                        Session["Additional_Charge"] = Convert.ToString(my_red["additional_charge"]).Trim();
                        Session["PW_Flag"] = my_red["PW_Flag"].ToString().Trim().ToUpper();
                        PW_Flag_Test =   my_red["PW_Flag"].ToString().Trim().ToUpper();
                    }
                    else
                    {
                        return;
                    }
                }
        catch (NullReferenceException)
        {
            Message("Null - Error(O-001) occured on Staff details. Please try later.");
        }
        catch (SqlException)
        {
            Message("DB - Error(O-002) occured on Staff details. Please try later.");
        }
        catch (Exception)
        {
            Message("Error(O-003) occured on Staff details. Please try later.");
        }
        finally
        {
            if ((my_red != null && my_red.IsClosed == false))
            {
                DAL.CloseSqlDataReader(my_red);
            }
        }
    }

    public void Block_Record()//P
    {
        try
        {
            my_query = "select isnull(BPL_Data_Loaded,'N') BPL_Data_Loaded,isnull(online,'Y') as online, isnull(closeaccPO,0) as closeaccPO, PO_CExpiredDt, isnull(efms_pilot,'N') efms_pilot, isnull(PO_EnrollmentFlag,'N') Acc_flag, isnull(PO_cemail,po_cemail) acc_po_cemail from Blocks where Block_Code = @Block";
            my_cmd = new NpgsqlCommand();
            my_cmd.Parameters.Clear();
            my_cmd.Parameters.Add("@Block", NpgsqlTypes.NpgsqlDbType.Char, 7).Value = ddl_Block.SelectedValue.Trim();
            my_cmd.CommandText = my_query;
            my_cmd.CommandType = CommandType.Text;
            my_red = DAL.ExecuteCommand_dr(my_cmd);
            if (my_red.Read())
            {
                Session["BPL_Data_Loaded"] = Convert.ToString(my_red["BPL_Data_Loaded"]);
                Session["online"] = Convert.ToString(my_red["online"]);
                Session["closeaccPO"] = Convert.ToString(my_red["closeaccPO"]);
            }
        }
        catch (NullReferenceException)
        {
            Message("Null - Error(P-001) occured on Block record. Please try later.");
        }
        catch (SqlException)
        {
            Message("DB - Error(P-002) occured on Block record. Please try later.");
        }
        catch (Exception)
        {
            Message("Error(P-003) occured on Block record. Please try later.");
        }
        finally
        {
            if ((my_red != null && my_red.IsClosed == false))
            {
                DAL.CloseSqlDataReader(my_red);
                my_cmd.Dispose();
            }
        }
    }

    public void Block_CloseAC()//Q
    {
        try
        {
            o_StringBuild.Length = 0;
            string Tbl_Blk = "blocks_rep" + Yr;
            my_query = o_StringBuild.Append("select isnull(closeaccPO,0) as closeaccPO from ").Append(Tbl_Blk).Append(" where Block_Code = @Block").ToString();
            my_cmd = new NpgsqlCommand();
            my_cmd.Parameters.Clear();
            my_cmd.Parameters.Add("@Block", NpgsqlTypes.NpgsqlDbType.Char, 7).Value = ddl_Block.SelectedValue.Trim();
            my_cmd.CommandText = my_query;
            my_cmd.CommandType = CommandType.Text;
            my_red = DAL.ExecuteCommand_dr(my_cmd);
            if (my_red.Read())
            {
                Session["close_d"] = Convert.ToString(my_red["closeaccPO"]);
            }
        }
        catch (NullReferenceException)
        {
            Message("Null - Error(Q-001) occured on Block close A/C. Please try later.");
        }
        catch (SqlException)
        {
            Message("DB - Error(Q-002) occured on Block close A/C. Please try later.");
        }
        catch (Exception)
        {
            Message("Error(Q-003) occured on Block close A/C. Please try later.");
        }
        finally
        {
            if ((my_red != null && my_red.IsClosed == false))
            {
                DAL.CloseSqlDataReader(my_red);
                my_cmd.Dispose();
            }
        }
    }

    public string Panchayat_Record()//R
    {
        try
        {
            if (Login_lvl == "HomeGP")
            {
                my_query = "Select isnull(exempt_ac,'N') exempt_ac,isnull(isonline,'Y') as online,isnull(efms_start,'N') efms_start, isnull(closeacc,0) as closeacc from panchayats where panchayat_code = @Panchayat";
            }
            if (Login_lvl == "HomeACGP")
            {
                my_query = "Select ACC_CExpiredDt, isnull(efms_pilot,'N') efms_pilot, isnull(ACC_EnrollmentFlag,'N')  Acc_flag,isnull(acc_cemail,po_cemail) acc_po_cemail,isnull(exempt_ac,'N') exempt_ac,isnull(isonline,'Y') as online,isnull(efms_start,'N') efms_start, isnull(closeacc,0) as closeacc from panchayats where panchayat_code = @Panchayat";
            }
            if (Login_lvl == "HomeWLGP")
            {
                my_query = "select isnull(exempt_ac,'N') exempt_ac,isnull(isonline,'Y') as online,isnull(efms_start,'N') efms_start, isnull(closeacc,0) as closeacc, PO_CExpiredDt, isnull(efms_pilot,'N') efms_pilot, isnull(PO_EnrollmentFlag,'N') Acc_flag, isnull(PO_cemail,po_cemail) acc_po_cemail from Panchayats where Panchayat_Code = @Panchayat";
            }
            my_cmd = new NpgsqlCommand();
            my_cmd.Parameters.Clear();
            my_cmd.Parameters.Add("@Panchayat", NpgsqlTypes.NpgsqlDbType.Char, 10).Value = ddl_Panch.SelectedValue.Trim();
            my_cmd.Parameters.Add("@UserID", NpgsqlTypes.NpgsqlDbType.Bigint).Value = txt_UserID.Text.Trim();
            my_cmd.CommandText = my_query;
            my_cmd.CommandType = CommandType.Text;
            my_red = DAL.ExecuteCommand_dr(my_cmd);
            if (my_red.Read())
            {
                Session["online"] = Convert.ToString(my_red["online"]);
                ViewState["exempt_ac"] = Convert.ToString(my_red["exempt_ac"]);
                ViewState["efms_start"] = Convert.ToString(my_red["efms_start"]);
                Session["close_d"] = Convert.ToString(my_red["closeacc"]);
                if (Login_lvl == "HomeACGP" || Login_lvl == "HomeWLGP")
                {
                    Session["Acc_flag"] = Convert.ToString(my_red["Acc_flag"]);
                    Session["acc_po_cemail"] = Convert.ToString(my_red["acc_po_cemail"]);
                    ViewState["efms_pilot"] = Convert.ToString(my_red["efms_pilot"]);
                }
                if (Login_lvl == "HomeACGP" || Login_lvl == "HomeWLGP")
                {
                    if (Convert.ToString(my_red["Acc_flag"]) == "I")
                    {
                        return Ret_PanchRecord = "DSC is already enrolled and is pending at DPC for approval.";
                    }
                    else if (Convert.ToString(my_red["Acc_flag"]) == "R")
                    {
                        return Ret_PanchRecord = "DSC is revoked by State. Please contact to State DBA.";
                    }
                }
                o_StringBuild.Length = 0;
                if (Login_lvl == "HomeACGP")
                {
                    ViewState["DSC_exp_date"] = Convert.ToString(my_red["ACC_CExpiredDt"]);
                    if (my_red["Acc_flag"].ToString() == "Y" && (DBNull.Value.Equals(my_red["ACC_CExpiredDt"].ToString())) == false && ((Convert.ToDateTime(my_red["ACC_CExpiredDt"]) - DateTime.Now).TotalSeconds) <= 0)
                    {
                        return Ret_PanchRecord = o_StringBuild.Append("DSC Expired on ").Append(Convert.ToString(my_red["ACC_CExpiredDt"])).Append("(MM/DD/YYYY HH:MM:SS). Please renew the DSC.").ToString();
                    }
                }
                if (Login_lvl == "HomeWLGP")
                {
                    ViewState["DSC_exp_date"] = Convert.ToString(my_red["PO_CExpiredDt"]);
                    if (Convert.ToString(my_red["Acc_flag"]) == "Y" && (DBNull.Value.Equals(my_red["PO_CExpiredDt"].ToString())) == false && (Convert.ToDateTime(my_red["PO_CExpiredDt"].ToString()) - DateTime.Now).TotalSeconds <= 0)
                    {
                        return Ret_PanchRecord = o_StringBuild.Append("DSC Expired on ").Append(Convert.ToString(my_red["PO_CExpiredDt"])).Append("(MM/DD/YYYY HH:MM:SS). Please renew the DSC.").ToString();
                    }
                }

                return Ret_PanchRecord = "1";
            }
            return Ret_PanchRecord;
        }
        catch (NullReferenceException)
        {
            return Ret_PanchRecord = "Null - Error(R-001) occured on Panchayat Record. Please try later.";
        }
        catch (SqlException)
        {
            return Ret_PanchRecord = "DB - Error(R-002) occured on Panchayat Record. Please try later.";
        }
        catch (Exception)
        {
            return Ret_PanchRecord = "Error(R-003) occured on Panchayat Record. Please try later.";
        }
        finally
        {
            if ((my_red != null && my_red.IsClosed == false))
            {
                DAL.CloseSqlDataReader(my_red);
                my_cmd.Dispose();
            }
        }
    }

    public void Panchayat_CloseAC()//S
    {
        try
        {
            string Tbl_Blk = "panchayats_rep" + Yr;
            o_StringBuild.Length=0;
            my_query =o_StringBuild.Append("select isnull(panch_name_local,panchayat_name) as panch_name_local,isnull(closeacc,0) as closeacc from ").Append(Tbl_Blk).Append(" where Panchayat_Code = @Panchayat").ToString();
            my_cmd = new NpgsqlCommand();
            my_cmd.Parameters.Clear();
            my_cmd.Parameters.Add("@Panchayat", NpgsqlTypes.NpgsqlDbType.Char, 10).Value = ddl_Panch.SelectedValue.Trim();
            my_cmd.CommandText = my_query;
            my_cmd.CommandType = CommandType.Text;
            my_red = DAL.ExecuteCommand_dr(my_cmd);
            if (my_red.Read())
            {
                Session["close_d"] = Convert.ToString(my_red["closeacc"]);
            }
        }
        catch (NullReferenceException)
        {
            Message("Null - Error(S-001) occured on Panchayat Close AC. Please try later.");
        }
        catch (SqlException)
        {
            Message("DB - Error(S-002) occured on Panchayat Close AC. Please try later.");
        }
        catch (Exception)
        {
            Message("Error(S-003) occured on Panchayat Close AC. Please try later.");
        }
        finally
        {
            if ((my_red != null && my_red.IsClosed == false))
            {
                DAL.CloseSqlDataReader(my_red);
                my_cmd.Dispose();
            }
        }
    }

    public string LoginHistory(string Var_exe_code, string Var_Login_Level_Code)//T
    {
        try
        {
            loginHistory lhobj = new loginHistory();
            lhobj.userId = txt_UserID.Text.Trim();
            lhobj.role = Var_exe_code;
            lhobj.locationCode = Var_Login_Level_Code;
            lhobj.dalObject = DAL;
            bool loginStatus = lhobj.loginEntry();
            if (loginStatus == false)
            {
                DAL.CloseSqlDataReader(my_red);
                my_cmd.Dispose();
                return Val_LoginHistory = "Error: On LoginHistory";
            }
            return Val_LoginHistory = "1";
        }
        catch (NullReferenceException)
        {
            return Val_LoginHistory = "Null - Error(T-001) occured on Login History. Please try later.";
        }
        catch (SqlException)
        {
            return Val_LoginHistory = "DB - Error(T-002) occured on Login History. Please try later.";
        }
        catch (Exception)
        {
            return Val_LoginHistory = "Error(T-003) occured on Login History. Please try later.";
        }
        finally
        {
            if ((my_red != null && my_red.IsClosed == false))
            {
                DAL.CloseSqlDataReader(my_red);
            }
        }
    }


    public void Fields_Clear()//U
    {
        try
        {
            txt_UserID.Text = "";
            txt_Password.Text = "";
            txt_Captcha.Text = "";
            if (Login_lvl == "Homestciti")
            {
                DDL_Role();
            }
        }
        catch (Exception)
        {
            Message("Error(U-001) occured on Fields Clear. Please try later.");
        }
    }

    public string Fields_Check()//V
    {
        try
        {
            if (Login_lvl == "HomeGP" || Login_lvl == "HomeACGP" || Login_lvl == "HomeWLGP" || Login_lvl == "HomeBP" || Login_lvl == "HomeACBP" || Login_lvl == "HomeWLBP" || Login_lvl == "HomeZP" || Login_lvl == "HomePO" || Login_lvl == "HomePODBA" || Login_lvl == "HomeBA" || Login_lvl == "HomeDPC" || Login_lvl == "HomeDPCDBA" || Login_lvl == "HomeAC" || Login_lvl == "HomeWL" || Login_lvl == "HomeACDPC" || Login_lvl == "HomeWLDPC" || Login_lvl == "Homestciti" || Login_lvl == "HomeACST" || Login_lvl == "HomeWLST")
            {
                if (ddl_FinYr.SelectedItem.Text == "Select Financial Year")
                {
                    return Ret_Blank_Check = "Pls Select Fin Year...";
                }
            }
            if (Login_lvl == "HomeGP" || Login_lvl == "HomeACGP" || Login_lvl == "HomeWLGP" || Login_lvl == "HomeBP" || Login_lvl == "HomeACBP" || Login_lvl == "HomeWLBP" || Login_lvl == "HomeZP" || Login_lvl == "HomePO" || Login_lvl == "HomePODBA" || Login_lvl == "HomeBA" || Login_lvl == "HomeDPC" || Login_lvl == "HomeDPCDBA" || Login_lvl == "HomeAC" || Login_lvl == "HomeWL" || Login_lvl == "HomeACDPC" || Login_lvl == "HomeWLDPC")
            {
                if (ddl_District.SelectedItem.Text == "Select District")
                {
                    return Ret_Blank_Check = "Pls Select District...";
                }
            }
            if (Login_lvl == "HomeGP" || Login_lvl == "HomeACGP" || Login_lvl == "HomeWLGP" || Login_lvl == "HomeBP" || Login_lvl == "HomeACBP" || Login_lvl == "HomeWLBP" || Login_lvl == "HomePO" || Login_lvl == "HomePODBA" || Login_lvl == "HomeBA" || Login_lvl == "HomeAC" || Login_lvl == "HomeWL")
            {
                if (ddl_Block.SelectedItem.Text == "Select Block")
                {
                    return Ret_Blank_Check = "Pls Select Block...";
                }
            }
            if (Login_lvl == "HomeGP" || Login_lvl == "HomeACGP" || Login_lvl == "HomeWLGP")
            {
                if (ddl_Panch.SelectedItem.Text == "Select Panchayat")
                {
                    return Ret_Blank_Check = "Pls Select Panchayat...";
                }
            }
            if (string.IsNullOrEmpty(txt_UserID.Text.Trim()) == true)
            {
                return Ret_Blank_Check = "Pls Enter User ID...";
            }
            if (txt_UserID.Text != "" && LoginChk.Invalid_Word(txt_UserID.Text.ToLower()) == true)
            {
                return Ret_Blank_Check = "Sorry, the words like Select, Union, Drop, Insert, Delete, Truncate, Xp etc., are not allowed in User ID. Please try again";
            }
            if (string.IsNullOrEmpty(txt_Password.Text.Trim()) == true)
            {
                return Ret_Blank_Check = "Pls Enter Password...";
            }
            if (txt_Password.Text != "" && LoginChk.Invalid_Word(txt_Password.Text.ToLower()) == true)
            {
                return Ret_Blank_Check = "Sorry, the words like Select, Union, Drop, Insert, Delete, Truncate, Xp etc., are not allowed in Password. Please try again";
            }
            if (string.IsNullOrEmpty(txt_Captcha.Text.Trim()) == true)
            {
                return Ret_Blank_Check = "Pls Enter Password...";
            }
            if (txt_Captcha.Text != "" && LoginChk.Invalid_Word(txt_Captcha.Text.ToLower()) == true)
            {
                return Ret_Blank_Check = "Sorry, the words like Select, Union, Drop, Insert, Delete, Truncate, Xp etc., are not allowed in Captcha. Please try again";
            }
            if (Login_lvl == "Homestciti")
            {
                if (ddl_Role.SelectedIndex == 0)
                {
                    return Ret_Blank_Check = "Please select role.";
                }
            }
            return Ret_Blank_Check = "1";
        }
        catch (NullReferenceException)
        {
            return "Null - Error(V-001) occured on Fields Check. Please try later.";
        }
        catch (Exception)
        {
            return "Error(V-002) occured on Fields Check. Please try later.";
        }
    }

    protected void btn_Login_Click(object sender, EventArgs e)   //W 
    {
        try
        {  
         
            //Check 2st for User ID
            string exe_lvl = "", exe_code = "", Login_Level_Code = "", role = "", exe_agency_code = "", Table_Column = "", Login_Level = "";
            switch (Login_lvl)
            {
                case "HomeGP":
                    role = "GPDEO";
                    exe_lvl = "GP";
                    exe_code = "GPDEO";
                    exe_agency_code = "3";
                    Login_Level = "pan_account";
                    Table_Column = "Panchayat_code";
                    Login_Level_Code = ddl_Panch.SelectedValue;
                    string Result_DisRec = District_Record();
                    //if (Result_DisRec != "1")
                    //{
                    //    Message(Result_DisRec);
                    //    return;
                    //}
                    break;
                case "HomeBP":
                    role = "BPDEO";
                    exe_lvl = "BP";
                    exe_agency_code = "2";
                    Login_Level = "bp_account";
                    Table_Column = "Block_code";
                    exe_code = ddl_Block.SelectedValue;
                    Login_Level_Code = ddl_Block.SelectedValue;
                    break;
                case "HomeZP":
                    role = "ZPDEO";
                    exe_lvl = "ZP";
                    exe_agency_code = "1";
                    Login_Level = "zp_account";
                    Table_Column = "District_code";
                    exe_code = ddl_District.SelectedValue;
                    Login_Level_Code = ddl_District.SelectedValue;
                    break;
                case "HomePO":
                    role = "PODEO";
                    exe_lvl = "PO";
                    exe_agency_code = "3";
                    Table_Column = "Block_code";
                    Login_Level = "block_account";
                    exe_code = ddl_Block.SelectedValue;
                    Login_Level_Code = ddl_Block.SelectedValue;
                    District_Record();
                    break;
                case "HomePODBA":
                    role = "PODBA";
                    exe_lvl = "POA";
                    exe_agency_code = "PODBA";
                    Login_Level = "bp_account";
                    Table_Column = "Block_code";
                    exe_code = ddl_Block.SelectedValue;
                    Login_Level_Code = ddl_Block.SelectedValue;
                    break;
                case "HomeDPC":
                    role = "DPCDEO";
                    exe_lvl = "DPC";
                    exe_agency_code = "1";
                    Login_Level = "zp_account";
                    Table_Column = "District_code";
                    exe_code = ddl_District.SelectedValue;
                    Login_Level_Code = ddl_District.SelectedValue;
                    break;
                case "HomeDPCDBA":
                    role = "DPCDBA";
                    exe_lvl = "DPCA";
                    exe_agency_code = "DPCDBA";
                    Login_Level = "zp_account";
                    Table_Column = "District_code";
                    exe_code = ddl_District.SelectedValue;
                    Login_Level_Code = ddl_District.SelectedValue;
                    break;
                case "Homestciti":
                    if (ddl_Role.SelectedIndex == 0)
                    {
                        Message("Please select role.");
                        return;
                    }
                    if (state_code == null)
                    {
                        Message("Not a Valid User.");
                        return;
                    }
                    Session["state_code"] = "";
                    Session["state_code_d"] = "";
                    Session["short_name"] = "";
                    Session["state_name"] = "";
                    Session["fin_year"] = "";
                    Session["LName"] = "";
                    Session["Entry_type"] = "";
                    valid_dba_login = "N";
                    string Ret_Designation = ddl_Role.SelectedValue.Trim();
                    if (Ret_Designation != null && Ret_Designation != "1")
                    {
                        Session["Result_Designation"] = Ret_Designation.Trim();
                        if (Session["Result_Designation"].ToString() == "STDBA")
                        {
                            string Client_ip = HttpContext.Current.Request.ServerVariables["remote_addr"].ToString();
                            if (Request.QueryString["state_Code"] == "17" && Client_ip != Auth_IP_State)
                            {
                                Message("Invalid Request");
                                return;
                            }
                            role = ddl_Role.SelectedValue.Trim();
                            exe_lvl = "ST";
                            exe_agency_code = "3";
                            Table_Column = "state_code";
                            Login_Level = "state_account";
                            exe_code = state_code;
                            Login_Level_Code = state_code;
                        }
                        else
                        {
                            role = ddl_Role.SelectedValue.Trim();
                            exe_lvl = "ST";
                            exe_agency_code = "3";
                            Table_Column = "state_code";
                            Login_Level = "state_account";
                            exe_code = "ST";
                            Login_Level_Code = state_code;
                        }
                        string Ret_StateRecord = State_Record();
                        if (Ret_StateRecord != "1")
                        {
                            Message(Ret_StateRecord);
                            return;
                        }
                    }
                    else
                    {
                        Message("Invalid User/Wrong Password.");
                        return;
                    }
                    break;
            }
          
            //for check PW_Flag           

            txt_Password.Text = "1";
            if (txt_Password.Text.Trim() == "1")//
                {
                    Session["State_Name_d"] = lbl_State.Text;
                    Session["State_code"] = state_code;
                    Session["userid"] = txt_UserID.Text.Trim();
                  //  Session["Exist_PW"] = PW.Trim();
                   // Response.Redirect("Login_ResetPW.aspx", false);
                }
                else
                {
                    Res_LogAud = LoginChk.Login_Audit_Entry(state_code, txt_UserID.Text.Trim(), "", role, "Wrong Password", "Login.aspx", "Unsuccessful", Session.SessionID, User_IPAdd);
                    if ((!string.IsNullOrEmpty(Res_LogAud)) && Res_LogAud != "1")
                    {
                        Message(Res_LogAud);
                        return;
                    }
                 
                    Message("Invalid User / Wrong Password.");
                    return;
                }
                       
                    FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, txt_UserID.Text.Trim(), DateTime.Now, DateTime.Now.AddMinutes(10), false, role, FormsAuthentication.FormsCookiePath);
                    string hash = FormsAuthentication.Encrypt(ticket);
                    HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, hash);
                    if (ticket.IsPersistent) cookie.Expires = ticket.Expiration;
                    Response.Cookies.Add(cookie);

                   
                    // DAL.CloseSqlDataReader(my_red);
                    string returnUrl = "";
                    ViewState["exempt_LB_dmd_dt"] = ViewState["exempt_LB_dmd_dt"].ToString().Trim();
                    Session["lang_name"] = ViewState["lang_name"].ToString().Trim();
                    Session["lang_code"] = ViewState["lang_code"].ToString().Trim();
                    Session["short_name"] = ViewState["shortname"].ToString().Trim();
                    Session["State_Name_d"] = lbl_State.Text;
                    Session["State_code_d"] = Request.QueryString["state_code"].ToString().Trim();// "06";                   
                    Session["exempt_msr_100days"] = ViewState["exempt_msr_100days"].ToString().Trim();
                    Session["exempt_avl"] = ViewState["exempt_avl"].ToString().Trim();
                    o_StringBuild.Length = 0;
                    Session["Entry_by"] = o_StringBuild.Append(txt_UserID.Text.Trim()).Append("_").Append(Request.ServerVariables["remote_addr"]).ToString();
                    Session["finyear_d"] = ddl_FinYr.SelectedItem.ToString();
                    Session["fin_year_d"] = ddl_FinYr.SelectedItem.ToString();
                    Session["Entry_type"] = "D";
                    Session["exempt6040"] = ViewState["exempt6040"].ToString();
                    Session["mark_if"] = ViewState["mark_if"].ToString();
                    Session["exempt6040_dt"] = ViewState["exempt6040_dt"].ToString();
                    Session["userid"] = txt_UserID.Text.Trim();
                    Session["state_code"] = Request.QueryString["state_code"].ToString().Trim();//New session req for Social Audit

                    //Add session for archived 16/feb/2021
                    Session["Is_archived"] = ViewState["Is_archived"].ToString().Trim();
                    Session["Archived_finyear_upto"] = ViewState["Archived_finyear_upto"].ToString().Trim();
                    Session["Archived_schema"] = ViewState["Archived_schema"].ToString().Trim();

                    switch (Login_lvl)
                    {
                        case "HomeGP":
                            Session["District_Code_d"] = ddl_Panch.SelectedValue.Substring(0, 4).Trim();//ddl_District.SelectedValue;  //Updated on 31May18
                            Session["District_Name_d"] = ddl_District.SelectedItem.ToString().Trim();
                            Session["exempt_dem"] = ViewState["exempt_dem"].ToString().Trim();
                            Session["ori_Block_Code_d"] = ddl_Block.SelectedValue.ToString().Trim();
                            Session["flag_debited"] = ViewState["flag_debited"].ToString().Trim();
                            Session["fund_trans_dr_ac"] = ViewState["fund_trans_dr_ac"].ToString().Trim();
                            Session["Block_Name_d"] = ddl_Block.SelectedItem.ToString().Trim();
                            Session["Panchayat_Name_d"] = ddl_Panch.SelectedItem.ToString().Trim();
                            Session["ori_District_Code_d"] = ddl_District.SelectedValue.ToString().Trim();
                            Session["Block_Code_d"] = ddl_Panch.SelectedValue.Substring(0, 7);// ddl_Block.SelectedValue; //Updated on 31May18
                            Session["Exel_d"] = "GP";
                            Session["ExeCode_d"] = "3";
                            Session["ori_Panchayat_Code_d"] = ddl_Panch.SelectedValue.Trim();
                            Session["Panchayat_Code_d"] = ddl_Panch.SelectedValue;
                            Session["exempt_ac"] = ViewState["exempt_ac"];
                            Session["exempt_LB_dmd"] = ViewState["exempt_LB_dmd"];
                            if (Request.QueryString["salogin"] == "Y")
                            {
                                Session["salogin"] = "Y";
                            }
                            returnUrl = ("~/indexframe2.aspx");
                            break;
                        case "HomeBP":
                            Session["District_Code_d"] = ddl_Block.SelectedValue.Substring(0, 4).Trim();//ddl_District.SelectedValue;
                            Session["District_Name_d"] = ddl_District.SelectedItem.ToString().Trim();
                            Session["Block_Code_d"] = ddl_Block.SelectedValue;
                            Session["Block_Name_d"] = ddl_Block.SelectedItem.ToString();
                            Session["ori_Block_Code_d"] = ddl_Block.SelectedValue.ToString().Trim();
                            Session["ori_District_Code_d"] = ddl_District.SelectedValue.ToString().Trim();
                            Session["ExeL_d"] = "BP";
                            Session["ExeCode_d"] = "2";
                            Session["efms_start"] = "N";
                            returnUrl = "~/BP/BpIndexFrame2.aspx";
                            break;
                        case "HomeZP":
                            Session["District_Code_d"] = ddl_District.SelectedValue;
                            Session["District_Name_d"] = ddl_District.SelectedItem.ToString().Trim();
                            Session["ori_District_Code_d"] = ddl_District.SelectedValue.ToString().Trim();
                            Session["Exel_d"] = "ZP";
                            Session["ExeCode_d"] = "1";
                            Session["efms_start"] = "N";
                            returnUrl = "~/ZP/ZPindexframe2.aspx";
                            break;
                        case "HomePO":
                            Session["District_Code_d"] = ddl_Block.SelectedValue.Substring(0, 4).Trim();//ddl_District.SelectedValue;
                            Session["District_Name_d"] = ddl_District.SelectedItem.ToString().Trim();
                            Session["ori_District_Code_d"] = ddl_District.SelectedValue.ToString().Trim();
                            Session["exempt_dem"] = ViewState["exempt_dem"];
                            Session["flag_debited"] = ViewState["flag_debited"];
                            Session["fund_trans_dr_ac"] = ViewState["fund_trans_dr_ac"].ToString();
                            Session["ori_Block_Code_d"] = ddl_Block.SelectedValue;
                            Session["Block_Name_d"] = ddl_Block.SelectedItem.ToString();
                            Session["state_code_d"] = Request.QueryString["state_code"];
                            Session["Block_Code_d"] = ddl_Block.SelectedValue;
                            Session["Exel_d"] = "PO";
                            Session["ExeCode_d"] = "3";
                            Session["fund_trans_dr_ac_additional"] = ViewState["fund_trans_dr_ac_additional"].ToString();
                            Session["anticipated_entry"] = ViewState["anticipated_entry"];
                            Session["finyear_anti_dup"] = ViewState["finyear_anti"];
                            Session["anticipated_dt_upto"] = ViewState["anticipated_dt_upto"];
                            Session["exempt_LB_dmd"] = ViewState["exempt_LB_dmd"];
                            returnUrl = ("~/Progofficer/PoindexFrame2.aspx");
                            break;
                        case "HomePODBA":
                            Session["ori_District_Code_d"] = ddl_District.SelectedValue.ToString().Trim();
                            Session["District_Code_d"] = ddl_District.SelectedValue;
                            Session["District_Name_d"] = ddl_District.SelectedItem.ToString().Trim();
                            Session["exempt_dem"] = ViewState["exempt_dem"].ToString().Trim();
                            Session["flag_debited"] = ViewState["flag_debited"].ToString().Trim();
                            Session["fund_trans_dr_ac"] = ViewState["fund_trans_dr_ac"].ToString().Trim();
                            Session["ori_Block_Code_d"] = ddl_Block.SelectedValue.ToString().Trim();
                            Session["Block_Name_d"] = ddl_Block.SelectedItem.ToString().Trim();
                            Session["Block_Code_d"] = ddl_Block.SelectedValue;
                            Session["Exel_d"] = "PODBA";
                            Session["ExeCode_d"] = "3";
                            returnUrl = "~/PODBA/podbaindexpage.aspx";
                            break;
                        case "HomeBA":
                            Session["State_Name_C"] = ViewState["state_name"].ToString().Trim();
                            Session["State_Code_C"] = Request.QueryString["state_code"].ToString().Trim();
                            Session["district_code_C"] = ddl_District.SelectedValue;
                            Session["block_code_C"] = ddl_Block.SelectedValue;
                            Session["District_Name_C"] = ddl_District.SelectedItem.ToString();
                            Session["Block_Name_C"] = ddl_Block.SelectedItem.ToString();
                            Session["Fin_year_C"] = ddl_FinYr.SelectedItem.ToString();
                            Session["Finyear_C"] = ddl_FinYr.SelectedItem.ToString();
                            Session["exempt_dem"] = ViewState["exempt_dem"].ToString().Trim();
                            Session["flag_debited"] = ViewState["flag_debited"].ToString().Trim();
                            Session["fund_trans_dr_ac"] = ViewState["fund_trans_dr_ac"].ToString().Trim();
                            Session["ori_District_Code_C"] = ddl_District.SelectedValue;
                            Session["ori_Block_Code_C"] = ddl_Block.SelectedValue;
                            Session["Exel_C"] = "PO";
                            Session["ExeCode_C"] = "3";
                            Session["Entry_by"] = Request.ServerVariables["remote_addr"];
                            Session["Entry_type"] = "D";
                            returnUrl = "indexframe_c.aspx";
                            break;
                        case "HomeDPC":
                            Session["ori_District_Code_d"] = Request.QueryString["District_Code"].ToString().Trim();
                            Session["District_Code_d"] = Request.QueryString["District_Code"].ToString().Trim();
                            Session["District_Name_d"] = Request.QueryString["district_name"].ToString().Trim();
                            Session["flag_debited"] = ViewState["flag_debited"].ToString().Trim();
                            Session["fund_trans_dr_ac"] = ViewState["fund_trans_dr_ac"].ToString().Trim();
                            Session["Exel_d"] = "DPC";
                            Session["ExeCode_d"] = "1";
                            Session["fin"] = Request.QueryString["fin_year"].ToString().Trim();
                            Session["fin_year"] = Request.QueryString["fin_year"].ToString().Trim();
                            Session["fin_year_d"] = Request.QueryString["fin_year"].ToString().Trim();
                            Session["finyear_d"] = Request.QueryString["fin_year"].ToString().Trim();
                            Session["scheme_name"] = "MGNREGA";
                            Session["scheme_code"] = "26";
                            Session["fund_trans_dr_ac_additional"] = ViewState["fund_trans_dr_ac_additional"].ToString().Trim();
                            returnUrl = "~/DPC/dpcindexFrame2.aspx";
                            break;
                        case "HomeDPCDBA":
                            Session["District_Code_d"] = ddl_District.SelectedValue;
                            Session["ori_District_Code_d"] = ddl_District.SelectedValue;
                            Session["District_Name_d"] = ddl_District.SelectedItem.ToString().Trim();
                            Session["Exel_d"] = "DPCDBA";
                            Session["ExeCode_d"] = "1";
                            returnUrl = "~/DPCDBA/dpcdbaindexpage.aspx";
                            break;
                        case "Homestciti":
                            if (Session["Result_Designation"].ToString() == "STDBA")
                            {
                                valid_dba_login = "Y";
                                Session["state_code"] = Request.QueryString["state_code"].ToString().Trim();
                                Session["finyear"] = Request.QueryString["fin_year"].ToString().Trim();
                                Session["fin_year"] = Request.QueryString["fin_year"].ToString().Trim();
                                Session["finyear_d"] = Request.QueryString["fin_year"].ToString().Trim();
                                Session["LName"] = txt_UserID.Text.Trim();
                                Session["Exel_d"] = "STATE";
                                Session["dba"] = "STDBA";
                                if (valid_dba_login == "Y")
                                {
                                    salt = System.DateTime.Now.Date.Year.ToString();
                                    Session["salt"] = salt;
                                }
                                returnUrl = "~/States/stindexframe2.aspx?flage=1";
                            }
                            else
                            {
                                Session["state_code"] = Request.QueryString["state_code"].ToString().Trim();
                                Session["fin_year"] = Request.QueryString["fin_year"].ToString().Trim();
                                Session["fin_year_d"] = Request.QueryString["fin_year"].ToString().Trim();
                                Session["finyear"] = Request.QueryString["fin_year"].ToString().Trim();
                                Session["finyear_d"] = Request.QueryString["fin_year"].ToString().Trim();
                                Session["data_entry"] = "D";
                                Session["Entry_type"] = "D";
                                Session["Exel_d"] = "STATE";
                                Session["dba"] = "ST";
                                returnUrl = "~/States/stindexframe2.aspx";
                            }
                            break;
                    }
                    Res_LogAud = LoginChk.Login_Audit_Entry(state_code, txt_UserID.Text.Trim(), "", role, "Login", "Login.aspx", "Successful", Session.SessionID, User_IPAdd);
                    if ((!string.IsNullOrEmpty(Res_LogAud)) && Res_LogAud != "1")
                    {
                        Message(Res_LogAud);
                        return;
                    }
                    Session["AuthToken"] = Guid.NewGuid().ToString();
                    Response.Cookies.Add(new HttpCookie("AuthToken", Session["AuthToken"].ToString()));
                    Response.Redirect(returnUrl, false);
                             
                
                  }
        catch (NullReferenceException)
        {
            Fields_Clear();
            Message("Null - Error(W-001) occured on Login. Please try later.");
        }
        catch (SqlException)
        {
            Fields_Clear();
            Message("DB - Error(W-002) occured on Login. Please try later.");
        }
        catch (Exception)
        {
            Fields_Clear();
            Message("Some error found! Pls. report to your Adminsitrator!");
        }
        finally
        {
            if ((my_red != null && my_red.IsClosed == false))
            {
                DAL.CloseSqlDataReader(my_red);
            }
        }
    }

    protected void lnk_forgotPW_Click(object sender, EventArgs e)
    {
        Response.Redirect("Login_ForgotPW.aspx?state_code=" + state_code);
    }

    protected void btreset_Click(object sender, EventArgs e)//X
    {
        try
        {
            ddl_FinYr.SelectedIndex = 0;
            ddl_District.Items.Clear();
            ddl_District.Items.Insert(0, "Select District");
            ddl_Block.Items.Clear();
            ddl_Block.Items.Insert(0, "Select Block");
            ddl_Panch.Items.Clear();
            ddl_Panch.Items.Insert(0, "Select Panchayat");
            DDL_Role();
            txt_UserID.Text = "";
            txt_Password.Text = "";
            txt_Captcha.Text = "";
            lblmsg.Visible = false;
            Refresh_Captcha();
        }
        catch (Exception)
        {
            Message("Error(X-001) occured on Reset button. Please try later.");
        }
        finally
        {
            if ((my_red != null && my_red.IsClosed == false))
            {
                DAL.CloseSqlDataReader(my_red);
            }
        }
    }

    protected void lnk_forgotUID_Click(object sender, EventArgs e)
    {
        Response.Redirect("Login_ForgotUserID.aspx?state_code=" + state_code);
    }

    protected void ddl_Role_SelectedIndexChanged(object sender, EventArgs e)//Y
    {
        try
        {
            if (ddl_Role.SelectedIndex == 0)
            {
                lblmsg.Text = "Please select role.";
                lblmsg.Visible = true;
                Fields_Clear();
            }
            else
            {
                lblmsg.Visible = false;
            }
        }
        catch (Exception)
        {
            Message("Error(Y-001) occured on Role Selection. Please try later.");
        }
    }

    protected void txt_UserID_TextChanged(object sender, EventArgs e)//Z
    {
        try
        {            
            Read_StaffDetails();
            if (!String.IsNullOrEmpty(PW_Flag_Test))
            {
                SetFocus(txt_Password);
            }
            Salt_Random = LoginChk.Get_PassSalt();
            ViewState["dyn_salt"] = Salt_Random;            
        }
        catch (Exception)
        {
            Message("Error(Z-001) occured On UserID Text Changed. Please try later.");
        }
    }

    protected void refresh_Click(object sender, System.Web.UI.ImageClickEventArgs e)
    {
        Refresh_Captcha();
    }

    public void Refresh_Captcha()//AA
    {
        try
        {
            txt_Captcha.Text = "";
            Res_Captcha = LoginChk.GetRandomString();
            Context.Session["CaptchaImageText"] = Res_Captcha;
        }
        catch (Exception)
        {
            Message("Error(AA-001) occured on Refresh Captcha. Please try later.");
        }
    }
}



