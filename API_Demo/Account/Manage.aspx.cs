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
    public static string CentCreateUserURL = ConfigurationManager.AppSettings["CentCreateUserURL"].ToString();
    public static string CentQueryURL = ConfigurationManager.AppSettings["CentQueryURL"].ToString();
    public static string CentSetUserURL = ConfigurationManager.AppSettings["CentSetUserURL"].ToString();
    public static string CentSetPassURL = ConfigurationManager.AppSettings["CentSetPassURL"].ToString();
    public static string CentStoreRoleURL = ConfigurationManager.AppSettings["CentStoreRoleURL"].ToString();
    public static string CentGetRoleMemebersURL = ConfigurationManager.AppSettings["CentGetRoleMemebersURL"].ToString();
    public static string CentUpdateRoleURL = ConfigurationManager.AppSettings["CentUpdateRoleURL"].ToString();

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Request.Cookies.AllKeys.Contains(".ASPXAUTH"))
        //if (!Request.Cookies.AllKeys.Contains(".ASPXAUTH"))
        {
            NotLoggedIn.Visible = false;

            if (!IsPostBack)
            {
                string strGetRolesJSON = @"{""Script"":""select * from Role"",""Args"":{""PageNumber"":1,""PageSize"":10000,""Limit"":10000,""SortBy"":"""",""direction"":""False"",""Caching"":-1}}";
                Centrify_API_Interface centGetRoles = new Centrify_API_Interface().MakeRestCall(Session["NewPodURL"].ToString() + CentQueryURL, strGetRolesJSON);
                var jssGetRoles = new JavaScriptSerializer();
                Dictionary<string, dynamic> centGetRoles_Dict = jssGetRoles.Deserialize<Dictionary<string, dynamic>>(centGetRoles.returnedResponse);

                SortedList<string, string> RolesList = new SortedList<string, string>();

                ArrayList centGetRoles_Roles = centGetRoles_Dict["Result"]["Results"];
                foreach (Dictionary<string, object> dRoles in centGetRoles_Roles)
                {
                    dynamic dRole = dRoles["Row"];
                    if (dRole["Name"] != null)
                    {
                        RolesList.Add(dRole["Name"], dRole["ID"]);
                    }
                    else
                    {
                        RolesList.Add(dRole["ID"], dRole["ID"]);
                    }
                }

                Roles_Dropdown.DataTextField = "Key";
                Roles_Dropdown.DataValueField = "Value";
                Roles_Dropdown.DataSource = RolesList;
                Roles_Dropdown.DataBind();
            }
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
        Centrify_API_Interface centCreateUser = new Centrify_API_Interface().MakeRestCall(Session["NewPodURL"].ToString() + CentCreateUserURL, strCreateUserJSON);
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
        Centrify_API_Interface centFindUser = new Centrify_API_Interface().MakeRestCall(Session["NewPodURL"].ToString() + CentQueryURL, strFindUserJSON);
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
        Centrify_API_Interface centSetUser = new Centrify_API_Interface().MakeRestCall(Session["NewPodURL"].ToString() + CentSetUserURL, strModifyUserJSON);
        var jss = new JavaScriptSerializer();
        Dictionary<string, dynamic> centSetUser_Dict = jss.Deserialize<Dictionary<string, dynamic>>(centSetUser.returnedResponse);

        if (centSetUser_Dict["success"].ToString() == "True" && centSetUser_Dict["success"].ToString() == "True")
        {
            if (SetPassword.Text != null)
            {
                string strSetPassJSON = @"{""ID"":""" + Session["UserId"].ToString() + @""",""ConfrimPassword"":""" + SetPassword.Text + @""",""newPassword"":""" + SetPassword.Text + @"""}";
                Centrify_API_Interface centSetPass = new Centrify_API_Interface().MakeRestCall(Session["NewPodURL"].ToString() + CentSetPassURL, strSetPassJSON);
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
    protected void CreateRole(object sender, EventArgs e)
    {
        string strStoreRoleJSON = @"{""Name"":""" + CreateRole_Name.Text + @"""}";
        Centrify_API_Interface centStoreRole = new Centrify_API_Interface().MakeRestCall(Session["NewPodURL"].ToString() + CentStoreRoleURL, strStoreRoleJSON);
        var jssStoreRole = new JavaScriptSerializer();
        Dictionary<string, dynamic> centSetPass_Dict = jssStoreRole.Deserialize<Dictionary<string, dynamic>>(centStoreRole.returnedResponse);

        if (centSetPass_Dict["success"].ToString() == "True")
        {
            ResultMessage.Text = "Role Created.";
        }
        else
        {
            ResultMessage.Text = "Failed to Set Password: " + centStoreRole.returnedResponse;
        }
    }
    protected void AddUserToRole(object sender, EventArgs e)
    {       
        string strGetRoleMemebersJSON = "";
        Centrify_API_Interface centGetRoleMemebers = new Centrify_API_Interface().MakeRestCall(Session["NewPodURL"].ToString() + CentGetRoleMemebersURL + "?name=" + Roles_Dropdown.SelectedValue.ToString(), strGetRoleMemebersJSON);
        var jssGetRoleMemebers = new JavaScriptSerializer();
        Dictionary<string, dynamic> centGetRoleMemebers_Dict = jssGetRoleMemebers.Deserialize<Dictionary<string, dynamic>>(centGetRoleMemebers.returnedResponse);

        if (centGetRoleMemebers_Dict["success"].ToString() == "True")
        {
            ResultMessage.Text = "Role Memebers Found.";
        }
        else
        {
            ResultMessage.Text = "Failed to Get Role Memebers: " + centGetRoleMemebers.returnedResponse;
        }

        ArrayList centGetRoleMemebers_Results = centGetRoleMemebers_Dict["Result"]["Results"];

        string strUpdateRoleJSONUsers_Prefix = @"{""Users"":[""";
        string strUpdateRoleJSONGroups_Prefix = @"""Groups"":[],";
        string strUpdateRoleJSONRoles_Prefix = @"""Roles"":[],";

        foreach (Dictionary<string, dynamic> dMemebers in centGetRoleMemebers_Results)
        {
            dynamic dMemeber = dMemebers["Row"];
            strUpdateRoleJSONUsers_Prefix = strUpdateRoleJSONUsers_Prefix + dMemeber["Guid"] + @""", """;           
        }

        string strFindUserJSON = @"{""Script"":""select * from dsusers where SystemName = '" + UserToAdd.Text + @"';"",""Args"":{""PageNumber"":1,""PageSize"":10000,""Limit"":10000,""SortBy"":"""",""direction"":""False"",""Caching"":-1}}";

        Centrify_API_Interface centFindUser = new Centrify_API_Interface().MakeRestCall(Session["NewPodURL"].ToString() + CentQueryURL, strFindUserJSON);
        var jssFindUser = new JavaScriptSerializer();
        Dictionary<string, dynamic> centFindUser_Dict = jssFindUser.Deserialize<Dictionary<string, dynamic>>(centFindUser.returnedResponse);

        if (centFindUser_Dict["success"].ToString() == "True")
        {
            ResultMessage.Text = "User GUID Found.";
        }
        else
        {
            ResultMessage.Text = "Failed to Find User: " + centFindUser.returnedResponse;
        }

        ArrayList centFindUser_Results = centFindUser_Dict["Result"]["Results"];
        dynamic centFindUser_Results_Column = centFindUser_Results[0];
        Dictionary<string, dynamic> centFindUser_Results_Row = centFindUser_Results_Column["Row"];

        string strUserUuid = centFindUser_Results_Row["InternalName"];
        
        strUpdateRoleJSONUsers_Prefix = strUpdateRoleJSONUsers_Prefix + strUserUuid + @"""],";

        string strUpdateRoleJSON = strUpdateRoleJSONUsers_Prefix + strUpdateRoleJSONGroups_Prefix + strUpdateRoleJSONRoles_Prefix + @"""Name"":""" + Roles_Dropdown.SelectedValue.ToString() + @"""}";

        Centrify_API_Interface centUpdateRole = new Centrify_API_Interface().MakeRestCall(Session["NewPodURL"].ToString() + CentUpdateRoleURL, strUpdateRoleJSON);
        var jssUpdateRole = new JavaScriptSerializer();
        Dictionary<string, dynamic> centUpdateRole_Dict = jssGetRoleMemebers.Deserialize<Dictionary<string, dynamic>>(centUpdateRole.returnedResponse);

        if (centFindUser_Dict["success"].ToString() == "True")
        {
            ResultMessage.Text = "User Successfully Added to Role.";
        }
        else
        {
            ResultMessage.Text = "Failed to Add User to Role: " + centUpdateRole.returnedResponse;
        }
    }
}