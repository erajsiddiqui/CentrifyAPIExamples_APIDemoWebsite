using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;
using System.Configuration;
using System.Collections;

public partial class Account_Manage : System.Web.UI.Page
{
    public static string CentPodURL = ConfigurationManager.AppSettings["CentPodURL"].ToString();
    public static string CentCreateUserURL = CentPodURL + ConfigurationManager.AppSettings["CentCreateUserURL"].ToString();
    public static string CentQueryURL = CentPodURL + ConfigurationManager.AppSettings["CentQueryURL"].ToString();
    public static string CentChangeUserURL = CentPodURL + ConfigurationManager.AppSettings["CentChangeUserURL"].ToString();
    public static string CentSetUserURL = CentPodURL + ConfigurationManager.AppSettings["CentSetUserURL"].ToString();
    public static string CentSetPassURL = CentPodURL + ConfigurationManager.AppSettings["CentSetPassURL"].ToString();

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Request.Cookies.AllKeys.Contains(".ASPXAUTH"))
        //if (!Request.Cookies.AllKeys.Contains(".ASPXAUTH"))
        {
            NotLoggedIn.Visible = false;
        }
        else
        {
            Manage.Visible = false;           
        }

        ResultMessage.Visible = false;

        SetPassword.Visible = false;
        SetPassword_Label.Visible = false;
        Account_Enabled.Visible = false;
        Account_Enabled_Label.Visible = false;
        Account_Locked.Visible = false;
        Account_Locked_Label.Visible = false;
        ModifyUser_Button.Visible = false;
    }
    protected void CreateUser(object sender, EventArgs e)
    {
        string strCreateUserJSON = "{Name:'" + UserName.Text + "', Mail:'" + UserName.Text + "', Password:'" + Password.Text + "'}";
        Centrify_API_Interface centCreateUser = new Centrify_API_Interface().MakeRestCall(CentCreateUserURL, strCreateUserJSON);
        var jssAdvanceAuthPoll = new JavaScriptSerializer();
        Dictionary<string, dynamic> centCreateUser_Dict = jssAdvanceAuthPoll.Deserialize<Dictionary<string, dynamic>>(centCreateUser.returnedResponse);

        if (centCreateUser_Dict["Message"].ToString() != null)
        {
            ResultMessage.Text = centCreateUser_Dict["Message"].ToString();
        }
        else
        {
            ResultMessage.Text = "Create User Successful.";
        }

        ResultMessage.Visible = true;
    }

    protected void FindUser(object sender, EventArgs e)
    {
        string strFindUserJSON = @"{""Script"":""select * from dsusers where SystemName = '" + FindUser_UserName.Text + @"';"",""Args"":{""PageNumber"":1,""PageSize"":10000,""Limit"":10000,""SortBy"":"""",""direction"":""False"",""Caching"":-1}}";
        Centrify_API_Interface centFindUser = new Centrify_API_Interface().MakeRestCall(CentQueryURL, strFindUserJSON);
        var jssFindUser = new JavaScriptSerializer();
        Dictionary<string, dynamic> centFindUser_Dict = jssFindUser.Deserialize<Dictionary<string, dynamic>>(centFindUser.returnedResponse);

        if (centFindUser_Dict["success"].ToString() == "True")
        {
            ResultMessage.Text = "User Found";

            ArrayList centFindUser_Results = centFindUser_Dict["Result"]["Results"];
            dynamic centFindUser_Results_Column = centFindUser_Results[0];
            Dictionary<string, dynamic> centFindUser_Results_Row = centFindUser_Results_Column["Row"];

            bool bEnabled = centFindUser_Results_Row["Enabled"];
            bool bLocked = centFindUser_Results_Row["Locked"];
            Session["UserId"] = centFindUser_Results_Row["InternalName"];

            SetPassword.Visible = true;
            SetPassword_Label.Visible = true;

            if (bEnabled)
            {
                Account_Enabled.Checked = true;
            }

            Account_Enabled.Visible = true;
            Account_Enabled_Label.Visible = true;

            if (bLocked)
            {
                Account_Locked.Checked = true;
            }

            Account_Locked.Visible = true;
            Account_Locked_Label.Visible = true;
            ModifyUser_Button.Visible = true;
            ModifyUser_Button.Enabled = true; // Broken

            Find_User_Button.Visible = false;
            FindUser_UserName.Visible = false;
            FindUser_UserName_Label.Visible = false;

        }
        else
        {
            ResultMessage.Text = "Failed to find user: " + centFindUser.returnedResponse;
        }

        ResultMessage.Visible = true;
    }
    protected void ModifyUser(object sender, EventArgs e)
    {
        string strState = null;

        if (Account_Locked.Checked)
        {
            strState = "Locked";
        }
        else
        {
            strState = "None";
        }

        string strModifyUserJSON = @"{""ID"":""" + Session["UserId"].ToString() + @""", ""enableState"":" + Account_Enabled.Checked.ToString().ToLower() + @",""state"":""" + strState + @"""}";
        Centrify_API_Interface centSetUser = new Centrify_API_Interface().MakeRestCall(CentSetUserURL, strModifyUserJSON);
        var jss = new JavaScriptSerializer();
        Dictionary<string, dynamic> centSetUser_Dict = jss.Deserialize<Dictionary<string, dynamic>>(centSetUser.returnedResponse);

        if (centSetUser_Dict["success"].ToString() == "True" && centSetUser_Dict["success"].ToString() == "True")
        {
            if (SetPassword.Text != null)
            {
                string strSetPassJSON = @"{""ID"":""" + Session["UserId"].ToString() + @""",""ConfrimPassword"":""" + SetPassword.Text + @""",""newPassword"":""" + SetPassword.Text + @"""}";
                Centrify_API_Interface centSetPass = new Centrify_API_Interface().MakeRestCall(CentSetPassURL, strSetPassJSON);
                var jssSetPass = new JavaScriptSerializer();
                Dictionary<string, dynamic> centSetPass_Dict = jss.Deserialize<Dictionary<string, dynamic>>(centSetPass.returnedResponse);
                if (centSetPass_Dict["success"].ToString() == "True")
                {
                    ResultMessage.Text = "User Updated.";
                }
                else
                {
                    ResultMessage.Text = "Failed to Set Password: " + centSetPass.returnedResponse;
                }
            }
        }
        else
        {
            ResultMessage.Text = "Failed to Modify user: " + centSetUser.returnedResponse;
        }

        ResultMessage.Visible = true;

        //Reset
        Find_User_Button.Visible = true;
        FindUser_UserName.Visible = true;
        FindUser_UserName_Label.Visible = true;

        UserName.Text = null;
        Password.Text = null;
        Account_Enabled.Checked = false;
        Account_Locked.Checked = false;
    }
}