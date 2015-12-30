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
    public static string CentGetUserAttributesURL = ConfigurationManager.AppSettings["CentGetUserAttributesURL"].ToString();
    public static string CentChangeUserURL = ConfigurationManager.AppSettings["CentChangeUserURL"].ToString();
    public static string CentSetPassURL = ConfigurationManager.AppSettings["CentSetPassURL"].ToString();
    public static string CentSetSecurityQuestionURL = ConfigurationManager.AppSettings["CentSetSecurityQuestionURL"].ToString();

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Request.Cookies.AllKeys.Contains(".ASPXAUTH"))
        //if (!Request.Cookies.AllKeys.Contains(".ASPXAUTH"))
        {
            NotLoggedIn.Visible = false;

            if (!IsPostBack)
            {
                string strGetUserAttributesJSON = "{ID:null}";
                Centrify_API_Interface centGetUserAttributes = new Centrify_API_Interface().MakeRestCall(Session["NewPodURL"].ToString() + CentGetUserAttributesURL, strGetUserAttributesJSON);
                var jssGetUserAttributes = new JavaScriptSerializer();
                Dictionary<string, dynamic> centGetUserAttribrutes_Dict = jssGetUserAttributes.Deserialize<Dictionary<string, dynamic>>(centGetUserAttributes.returnedResponse);

                Session["UserAttributes"] = centGetUserAttributes.returnedResponse;

                UserName.Text = centGetUserAttribrutes_Dict["Result"]["Name"];
                LoginName.Text = centGetUserAttribrutes_Dict["Result"]["DisplayName"];
                Alias.Text = centGetUserAttribrutes_Dict["Result"]["Alias"];
                Email.Text = centGetUserAttribrutes_Dict["Result"]["Mail"];
                DisplayName.Text = centGetUserAttribrutes_Dict["Result"]["DisplayName"];
                Mobile.Text = centGetUserAttribrutes_Dict["Result"]["MobileNumber"];
                OfficePhone.Text = centGetUserAttribrutes_Dict["Result"]["OfficeNumber"];
                HomePhone.Text = centGetUserAttribrutes_Dict["Result"]["HomeNumber"];

                UserName.Enabled = false;
                LoginName.Enabled = false;
                Alias.Enabled = false;

                ResetPassword_Div.Visible = false;
                SecretQuestion_Div.Visible = false;
            }
        }
        else
        {
            AccountOverview.Visible = false;
            ResetPassword_Div.Visible = false;
            SecretQuestion_Div.Visible = false;
        }      
    }
    public void UpdateSecretQuestion(object sender, EventArgs e)
    {
        AccountOverview.Visible = false;
        SecretQuestion_Div.Visible = true;
    }
    public void SubmitSecretQuestion(object sender, EventArgs e)
    {
        string centGetUserAttributes = Session["UserAttributes"].ToString();
        var jssGetUserAttributes = new JavaScriptSerializer();
        Dictionary<string, dynamic> centGetUserAttribrutes_Dict = jssGetUserAttributes.Deserialize<Dictionary<string, dynamic>>(centGetUserAttributes);

        string strUuid = centGetUserAttribrutes_Dict["Result"]["Uuid"];

        string strSetQuestionJSON = @"{""ID"":""" + strUuid + @""",""securityquestion"":""" + SecretQuestion.Text + @""",""questionanwser"":""" + SecretAnswer.Text + @"""}";

        Centrify_API_Interface centSetQuestion = new Centrify_API_Interface().MakeRestCall(Session["NewPodURL"].ToString() + CentSetSecurityQuestionURL, strSetQuestionJSON);
        var jssSetQuestion = new JavaScriptSerializer();
        Dictionary<string, dynamic> centSetQuestion_Dict = jssSetQuestion.Deserialize<Dictionary<string, dynamic>>(centSetQuestion.returnedResponse);

        if (centSetQuestion_Dict["success"].ToString() != "True")
        {
            FailureText.Text = centSetQuestion_Dict["success"].ToString();
            ErrorMessage.Visible = true;
        }
        else
        {
            SecretQuestion_Div.Visible = false;
            AccountOverview.Visible = true;
        }
    }
    public void ResetPassword(object sender, EventArgs e)
    {
        AccountOverview.Visible = false;
        ResetPassword_Div.Visible = true;

    }
    public void SubmitPassword(object sender, EventArgs e)
    {
        string centGetUserAttributes = Session["UserAttributes"].ToString();
        var jssGetUserAttributes = new JavaScriptSerializer();
        Dictionary<string, dynamic> centGetUserAttribrutes_Dict = jssGetUserAttributes.Deserialize<Dictionary<string, dynamic>>(centGetUserAttributes);

        string strUuid = centGetUserAttribrutes_Dict["Result"]["Uuid"];

        string strSetPassJSON = @"{""ID"":""" + strUuid + @""",""ConfrimPassword"":""" + NewPassword.Text + @""",""newPassword"":""" + ConfirmPassword.Text + @"""}";

        Centrify_API_Interface centSetPass = new Centrify_API_Interface().MakeRestCall(Session["NewPodURL"].ToString() + CentSetPassURL, strSetPassJSON);
        var jssSetPass = new JavaScriptSerializer();
        Dictionary<string, dynamic> centChangeUser_Dict = jssSetPass.Deserialize<Dictionary<string, dynamic>>(centSetPass.returnedResponse);

        if (centChangeUser_Dict["success"].ToString() != "True")
        {
            FailureText.Text = centChangeUser_Dict["success"].ToString();
            ErrorMessage.Visible = true;
        }   
        else
        {
            ResetPassword_Div.Visible = false;
            AccountOverview.Visible = true;
        }
    }
    public void Save(object sender, EventArgs e)
    {
        string centGetUserAttributes = Session["UserAttributes"].ToString();
        var jssGetUserAttributes = new JavaScriptSerializer();
        Dictionary<string, dynamic> centGetUserAttribrutes_Dict = jssGetUserAttributes.Deserialize<Dictionary<string, dynamic>>(centGetUserAttributes);

        string strUuid = centGetUserAttribrutes_Dict["Result"]["Uuid"];

        string strChangeUserJSON = @"{""ID"":""" + strUuid + @""",""Mail"":""" + Email.Text + @"""}";
        Centrify_API_Interface centChangeUser = new Centrify_API_Interface().MakeRestCall(Session["NewPodURL"].ToString() + CentChangeUserURL, strChangeUserJSON);
        var jssChangeUser = new JavaScriptSerializer();
        Dictionary<string, dynamic> centChangeUser_Dict = jssChangeUser.Deserialize<Dictionary<string, dynamic>>(centChangeUser.returnedResponse);

        if (centChangeUser_Dict["success"].ToString() != "True")
        {
            FailureText.Text = centChangeUser_Dict["success"].ToString();
            ErrorMessage.Visible = true;
        }        
    }
}