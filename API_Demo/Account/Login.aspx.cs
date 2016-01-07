using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Web;
using System.Web.UI;
using System.Net;
using System.Web.Script.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Threading;
using System.ComponentModel;
using API_Demo;

public partial class Account_Login : Page
{
        HttpContext context = HttpContext.Current;

        //Move to Web.Config
        public static string CentLoginURL = ConfigurationManager.AppSettings["CentLoginURL"].ToString();
        public static string CentLogOutURL = ConfigurationManager.AppSettings["CentLogOutURL"].ToString();
        public static string CentStartAuthURL = ConfigurationManager.AppSettings["CentStartAuthURL"].ToString();
        public static string CentAdvanceAuthURL = ConfigurationManager.AppSettings["CentAdvanceAuthURL"].ToString();

        protected void Page_Load(object sender, EventArgs e)
        {
            //Track calling url for redirect
            var returnUrl = HttpUtility.UrlEncode(Request.QueryString["ReturnUrl"]);

            //Keep Password value on postback
            Password.Attributes["value"] = Password.Text;

            //Diable while not visible
            pwdValidator.Enabled = false;
            MFAAnswer_Validator.Enabled = false;
            NewPassValidator.Enabled = false;
            ConfirmNewPassValidator.Enabled = false;

            //Disable stage 2 items
            UserName.Visible = true;
            UserName_Label.Visible = true;
            Password.Visible = false;
            Password_Label.Visible = false;
            RememberMe.Visible = false;
            RememberMe_Label.Visible = false;
            Login.Visible = false;
            AuthMethod.Visible = false;
            AuthMethod_Label.Visible = false;
            AuthMethod_Second.Visible = false;
            AuthMethod_Label_Second.Visible = false;
            MFAAnswer.Visible = false;
            MFAAnswer_Label.Visible = false;
            MFAAnswer_Submit.Visible = false;
            MFAMessage.Visible = false;
            Next.Visible = true;
            ForgotPass_button.Visible = false;
            NewPass_Label.Visible = false;
            NewPass.Visible = false;
            ConfirmNewPass_Label.Visible = false;
            ConfirmNewPass.Visible = false;
            ForgotPass_Submit_button.Visible = false;
            SecretQuestion.Visible = false;
            SecretQuestion_Label.Visible = false;

            if (!IsPostBack)
            {
                try
                {
                    if (Session["OTP"].ToString() != "")
                    {
                        UpdatePanel1.Visible = false;
                    }
                    else
                    {
                        AlreadyLoggedIn.Visible = false;
                    }
                    
                }
                catch (Exception)
                {

                    AlreadyLoggedIn.Visible = false;
                }
                
                //Used for poll + text MFA
                Timer1.Enabled = false;
            }
        }


        protected void TimerTick(object sender, EventArgs e)
        {
            this.PollOnce();

            MFAAnswer.Focus();
            MFAAnswer_Label.Visible = true;
            MFAAnswer.Visible = true;
            MFAAnswer_Submit.Visible = true;
            MFAAnswer_Validator.Enabled = true;
            Next.Visible = false;

            MFAMessage.Visible = true;

            //Timer1.Enabled = false;
        }


        //Called by Next Button
        protected void Next_Login(object sender, EventArgs e)
        {
            Session["NewPodURL"] = ConfigurationManager.AppSettings["CentPodURL"].ToString();

            AuthMethod.Items.Clear();
            AuthMethod_Second.Items.Clear();

            pwdValidator.Enabled = true;

            //Populate MFA Dropdown
            string strStartAuthJSON = @"{""User"":""" + UserName.Text + @""", ""Version"":""1.0""}";
            Centrify_API_Interface centStartAuth = new Centrify_API_Interface().MakeRestCall(Session["NewPodURL"].ToString() + CentStartAuthURL, strStartAuthJSON);
            var jss = new JavaScriptSerializer();
            Dictionary<string, dynamic> centStartAuth_Dict = jss.Deserialize<Dictionary<string, dynamic>>(centStartAuth.returnedResponse);

            if (centStartAuth_Dict["success"].ToString() == "True")
            {
                //Detect if a redirect to pod was returned
                if (centStartAuth_Dict["Result"].ContainsKey("PodFqdn"))
                {
                    Session["NewPodURL"] = "https://" + centStartAuth_Dict["Result"]["PodFqdn"];
                    string test = Session["NewPodURL"].ToString() + CentStartAuthURL;
                    Centrify_API_Interface centStartAuth_redirect = new Centrify_API_Interface().MakeRestCall(Session["NewPodURL"].ToString() + CentStartAuthURL, strStartAuthJSON);

                    //Store Redirected Result in session
                    Session["StartAuth"] = centStartAuth_redirect.returnedResponse;
                    centStartAuth_Dict = jss.Deserialize<Dictionary<string, dynamic>>(centStartAuth_redirect.returnedResponse);
                }
                else
                {
                    //Store First Result in session
                    Session["StartAuth"] = centStartAuth.returnedResponse;
                }

                //Store Centrify session information in site session
                Session["TenantId"] = centStartAuth_Dict["Result"]["TenantId"];
                Session["SessionId"] = centStartAuth_Dict["Result"]["SessionId"];

                ArrayList centStartAuth_Challenges = centStartAuth_Dict["Result"]["Challenges"];

                SetDropDowns(centStartAuth_Dict, Next);

                if (AuthMethod.Visible && AuthMethod.SelectedItem.Text == "Password")
                {
                    Password.Visible = true;
                    Password_Label.Visible = true;
                }
                else if (AuthMethod_Second.Visible && AuthMethod_Second.SelectedItem.Text == "Password")
                {
                    Password.Visible = true;
                    Password_Label.Visible = true;
                }
            }
            else
            {
                FailureText.Text = centStartAuth_Dict["Message"].ToString();
                ErrorMessage.Visible = true;
            }
        }

        //Called by Login Button
        protected void AdvanceAuth(object sender, EventArgs e)
        {
            string strStartAuth_Response = null;

            //Parse Selected MFA
            if (Session["StartAuth"] != null)
            {
                strStartAuth_Response = Session["StartAuth"].ToString();

                var jssStartAuth = new JavaScriptSerializer();
                Dictionary<string, dynamic> centStartAuth_Dict = jssStartAuth.Deserialize<Dictionary<string, dynamic>>(strStartAuth_Response);
                ArrayList centStartAuth_Challenges = centStartAuth_Dict["Result"]["Challenges"];

                string strUPMechId = null;

                string strSelectedName = null;
                string strSelectedMechId = null;
                string strSelectedAnswerType = null;
                string strSecretQuestion = null;


                if (AuthMethod.Items.Count != 0)
                {
                    if(AuthMethod_Second.Items.Count == 0)
                    {
                        //One MFA Only
                        strSelectedMechId = AuthMethod.SelectedValue.ToString();
                    }
                    else
                    {
                        //2 MFA
                        strSelectedMechId = AuthMethod.SelectedValue.ToString();
                    }
                }
                else
                {
                    if (AuthMethod_Second.Items.Count == 0)
                    {
                        //Password Only
                        strUPMechId = Session["UPMechId"].ToString();
                    }
                    else
                    {
                        //Password + MFA
                        if (Session["UPMechId"].ToString() != "")
                        {
                            strUPMechId = Session["UPMechId"].ToString();
                        }

                        //Second MFA Only
                        strSelectedMechId = AuthMethod_Second.SelectedValue.ToString();
                    }
                }

                foreach (Dictionary<string, dynamic> centStartAuth_Mechs in centStartAuth_Challenges)
                {
                    foreach (ArrayList mechs in centStartAuth_Mechs.Values)
                    {
                        foreach (Dictionary<string, object> mech in mechs)
                        {
                            if (strSelectedMechId != null)
                            {
                                if (mech["MechanismId"].ToString() == strSelectedMechId)
                                {
                                    strSelectedName = mech["Name"].ToString();
                                    strSelectedAnswerType = mech["AnswerType"].ToString();

                                    if (mech["Name"].ToString() == "SQ")
                                    {
                                        strSecretQuestion = mech["Question"].ToString();
                                    }
                                }                                
                            }                          
                        }
                    }
                }

                string strAdvanceAuthJSON = null;

                //Create JSON for AdvanceAuth by checking what MFA Mechs are available and selected
                if (strUPMechId != null)
                {
                    //Password + MFA
                    if (strSelectedMechId != null)
                    {
                        if (strSelectedAnswerType == "Text")
                        {
                            strAdvanceAuthJSON = @"{""TenantId"":""" + Session["TenantId"].ToString() + @""",""SessionId"":""" + Session["SessionId"].ToString() + @""",""PersistentLogin"":" + RememberMe.Checked.ToString().ToLower() + @",""MultipleOperations"":[{""MechanismId"":""" + strUPMechId + @""",""Answer"":""" + @Password.Text + @""",""Action"":""Answer""},{""MechanismId"":""" + strSelectedMechId + @""",""Answer"":""" + SecretQuestion.Text + @""",""Action"":""Answer""}]}";
                        }
                        else
                        {
                            strAdvanceAuthJSON = @"{""TenantId"":""" + Session["TenantId"].ToString() + @""",""SessionId"":""" + Session["SessionId"].ToString() + @""",""PersistentLogin"":" + RememberMe.Checked.ToString().ToLower() + @",""MultipleOperations"":[{""MechanismId"":""" + strUPMechId + @""",""Answer"":""" + @Password.Text + @""",""Action"":""Answer""},{""MechanismId"":""" + strSelectedMechId + @""",""Action"":""StartOOB""}]}";
                        }
                    }                    
                    //Password Only - No MFA
                    else
                    {
                        strAdvanceAuthJSON = @"{""TenantId"":""" + Session["TenantId"].ToString() + @""",""SessionId"":""" + Session["SessionId"].ToString() + @""",""PersistentLogin"":" + RememberMe.Checked.ToString().ToLower() + @",""MechanismId"":""" + strUPMechId + @""",""Answer"":""" + @Password.Text + @""",""Action"":""Answer""}";
                    }
                }
                //One MFA Only
                else if (strUPMechId == null && strSelectedMechId != null)
                {                   
                    if (strSelectedName != "UP")
                    {
                        strAdvanceAuthJSON = @"{""TenantId"":""" + Session["TenantId"].ToString() + @""",""SessionId"":""" + Session["SessionId"].ToString() + @""",""PersistentLogin"":" + RememberMe.Checked.ToString().ToLower() + @",""MechanismId"":""" + strSelectedMechId + @""",""Action"":""StartOOB""}";
                    }
                    //Password Selected from MFA Dropdown
                    else
                    {
                        strAdvanceAuthJSON = @"{""TenantId"":""" + Session["TenantId"].ToString() + @""",""SessionId"":""" + Session["SessionId"].ToString() + @""",""PersistentLogin"":" + RememberMe.Checked.ToString().ToLower() + @",""MechanismId"":""" + strSelectedMechId + @""",""Answer"":""" + @Password.Text + @""",""Action"":""Answer""}";
                    }
                }
               
                if (strAdvanceAuthJSON == null)
                {
                    FailureText.Text = "Error: Please contact your system administrator. Error Reason: JSON payload was not set.";
                    ErrorMessage.Visible = true;
                }

                else if (strAdvanceAuthJSON != null)
                {
                    //Start Oob - Send MFA
                    Centrify_API_Interface centAdvanceAuth = new Centrify_API_Interface().MakeRestCall(Session["NewPodURL"].ToString() + CentAdvanceAuthURL, strAdvanceAuthJSON);
                    var jssAdvanceAuth = new JavaScriptSerializer();
                    Dictionary<string, dynamic> centAdvanceAuth_Dict = jssAdvanceAuth.Deserialize<Dictionary<string, dynamic>>(centAdvanceAuth.returnedResponse);
                    
                    if (centAdvanceAuth_Dict["success"].ToString() == "True")
                    {                        
                        if (centAdvanceAuth_Dict["Result"]["Summary"].ToString() == "LoginSuccess")
                        {
                            if (centAdvanceAuth.returnedCookie != null)
                            {
                                Session["podFQDN"] = centAdvanceAuth_Dict["Result"]["PodFqdn"].ToString();
                                Session["OTP"] = centAdvanceAuth_Dict["Result"]["Auth"].ToString();
                                Session["ASPXAUTH"] = centAdvanceAuth.returnedCookie.Value;
                                HttpContext.Current.Response.Cookies.Add(centAdvanceAuth.returnedCookie);
                                HttpContext.Current.Response.Headers.Add("Authorization", "Bearer " + Session["ASPXAUTH"].ToString());

                                String TransferPage = "<script>window.open('../" + Request.QueryString["ReturnUrl"] + "','_self');</script>";
                                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "temp", TransferPage, false);
                            }
                            else
                            {
                                FailureText.Text = "Error: Please contact your system administrator. Error Reason: Valid cookie was not returned.";
                                ErrorMessage.Visible = true;
                            }
                        }
                        //Precess MFA Types
                        else if (centAdvanceAuth_Dict["Result"]["Summary"].ToString() == "OobPending")
                        {
                            if (strSelectedAnswerType != null)
                            {
                                //Text Based MFA
                                if (strSelectedAnswerType == "StartTextOob")
                                {
                                    MFAAnswer_Label.Visible = true;
                                    MFAAnswer.Visible = true;
                                    MFAAnswer_Submit.Visible = true;
                                    MFAAnswer_Validator.Enabled = true;
                                    Next.Visible = false;

                                    if (strSecretQuestion != null)
                                    {
                                        MFAMessage.Text = "Please Enter The Answer to the Question: " + strSecretQuestion;
                                    }
                                    else
                                    {
                                        MFAMessage.Text = "Please Enter The Answer to the Selected MFA Method.";
                                    }

                                    MFAMessage.Visible = true;

                                    if (strSelectedName == "OTP")
                                    {
                                        //StartPoll();
                                        Timer1.Enabled = true;
                                    }

                                    if (strSelectedName == "SMS")
                                    {
                                        //StartPoll();
                                        Timer1.Enabled = true;
                                    }
                                }

                                //Polling MFA
                                else if (strSelectedAnswerType == "StartOob")
                                {
                                    StartPoll();
                                }
                                else
                                {
                                    FailureText.Text = "Error: Please contact your system administrator. Error Reason: Unknown Mech Answer Type.";
                                    ErrorMessage.Visible = true;
                                }
                            }
                            else
                            {
                                FailureText.Text = "Error: Please contact your system administrator. Error Reason: Mech Answer Type is null.";
                                ErrorMessage.Visible = true;
                            }
                        }
                        else if (centAdvanceAuth_Dict["Result"]["Summary"].ToString() == "NewPackage")
                        {
                            Session["StartAuth"] = centAdvanceAuth.returnedResponse;
                            AuthMethod.Items.Clear();
                            SetDropDowns(centAdvanceAuth_Dict, null);
                        }
                        else
                        {
                            FailureText.Text = "Error: Please contact your system administrator. Error Reason: Unknown Mech Result Summary.";
                            ErrorMessage.Visible = true;
                        }
                    }
                    else
                    {
                        FailureText.Text = centAdvanceAuth_Dict["Message"].ToString();
                        ErrorMessage.Visible = true;
                    }
                }
            }
            else
            {
                FailureText.Text = "Error: Please contact your system administrator. Error Reason: StartAuth was not set.";
                ErrorMessage.Visible = true;
            }
        }

        //Called by Submit button during text Answer MFA
        protected void Submit_MFA(object sender, EventArgs e)
        {
            //Set Form Visibility
            Password.Visible = false;
            Password_Label.Visible = false;
            RememberMe.Visible = false;
            RememberMe_Label.Visible = false;
            Login.Visible = false;
            AuthMethod.Visible = false;
            AuthMethod_Label.Visible = false;
            Next.Visible = false;

            string strAdvanceAuthSubmitJSON = null;

            if (AuthMethod.Items.Count != 0)
            {
                strAdvanceAuthSubmitJSON = @"{""TenantId"":""" + Session["TenantId"].ToString() + @""",""SessionId"":""" + Session["SessionId"].ToString() + @""",""PersistentLogin"":" + RememberMe.Checked.ToString().ToLower() + @",""MechanismId"":""" + AuthMethod.SelectedValue.ToString() + @""",""Answer"":""" + MFAAnswer.Text + @""",""Action"":""Answer""}";
            }
            else if (AuthMethod.Items.Count == 0 && AuthMethod_Second.Items.Count != 0)
            {
                strAdvanceAuthSubmitJSON = @"{""TenantId"":""" + Session["TenantId"].ToString() + @""",""SessionId"":""" + Session["SessionId"].ToString() + @""",""PersistentLogin"":" + RememberMe.Checked.ToString().ToLower() + @",""MechanismId"":""" + AuthMethod_Second.SelectedValue.ToString() + @""",""Answer"":""" + MFAAnswer.Text + @""",""Action"":""Answer""}";
            }

            //Submit Answer
            ProcessAdvanceAuth(strAdvanceAuthSubmitJSON);
        }

        //Called by Forgot Pass button
        protected void ForgotPass(object sender, EventArgs e)
        {
            //Clear dropdown items
            AuthMethod.Items.Clear();
            AuthMethod_Second.Items.Clear();

            //Get Mechs for Pass Reset
            string strAdvanceAuthJSON = @"{""TenantId"":""" + Session["TenantId"].ToString() + @""",""SessionId"":""" + Session["SessionId"].ToString() + @""",""PersistentLogin"":" + RememberMe.Checked.ToString().ToLower() + @",""Action"":""ForgotPassword""}";

            Centrify_API_Interface centAdvanceAuth = new Centrify_API_Interface().MakeRestCall(Session["NewPodURL"].ToString() + CentAdvanceAuthURL, strAdvanceAuthJSON);

            //Reset Start Auth
            Session["StartAuth"] = centAdvanceAuth.returnedResponse;
            string strMFAMechs = centAdvanceAuth.returnedResponse;

            var jss = new JavaScriptSerializer();
            Dictionary<string, dynamic> centStartAuth_Dict = jss.Deserialize<Dictionary<string, dynamic>>(strMFAMechs);


            if (centStartAuth_Dict["success"].ToString() == "True")
            {
                //Populate dropdown
                SetDropDowns(centStartAuth_Dict, ForgotPass_button);
            }
            else
            {
                FailureText.Text = "Error: Please contact your system administrator.";
                ErrorMessage.Visible = true;
            }
        }

        //Called by Submit Pass button
        protected void ForgotPass_Submit(object sender, EventArgs e)
        {
            //Submit New Pass
            if (NewPass.Text == ConfirmNewPass.Text)
            {
                string strResetSubmitJSON = @"{""TenantId"":""" + Session["TenantId"].ToString() + @""",""SessionId"":""" + Session["SessionId"].ToString() + @""",""PersistentLogin"":" + RememberMe.Checked.ToString().ToLower() + @",""MechanismId"":""" + Session["ResetMechId"].ToString() + @""",""Answer"":""" + NewPass.Text + @""",""Action"":""Answer""}";

                ProcessAdvanceAuth(strResetSubmitJSON);
            }
            else
            {
                FailureText.Text = "Error: Passwords did not match.";
                ErrorMessage.Visible = true;
            }
        }

        protected void LogOut_Click(object sender, EventArgs e)
        {
            //Log Out from Centrify
            Centrify_API_Interface centLogOut = new Centrify_API_Interface().MakeRestCall(Session["NewPodURL"].ToString() + CentLogOutURL, "");
            var jss = new JavaScriptSerializer();
            Dictionary<string, dynamic> centLogOut_Dict = jss.Deserialize<Dictionary<string, dynamic>>(centLogOut.returnedResponse);

            if (centLogOut_Dict["success"].ToString() == "True")
            {
                //Clear Local Cookies
                Session["OTP"] = "";
                Session.Abandon();

                Context.Response.Redirect(Context.Request.RawUrl);
            }
            else
            {
                FailureText_LogOut.Text = "Error. Please contact your system administrator. Error reason: " + centLogOut_Dict["Message"].ToString();
                ErrorMessage_LogOut.Visible = true;
            }


        }

        //Processes all AdvanceAuth calls
        public string ProcessAdvanceAuth(string strJSON)
        {
            Centrify_API_Interface centAdvanceAuth = new Centrify_API_Interface().MakeRestCall(Session["NewPodURL"].ToString() + CentAdvanceAuthURL, strJSON);
            var jssAdvanceAuthPoll = new JavaScriptSerializer();
            Dictionary<string, dynamic> centAdvanceAuth_Dict = jssAdvanceAuthPoll.Deserialize<Dictionary<string, dynamic>>(centAdvanceAuth.returnedResponse);

            if (centAdvanceAuth_Dict["success"].ToString() == "True")
            {
                if (centAdvanceAuth_Dict["Result"]["Summary"].ToString() == "OobPending")
                {
                    return "Poll";
                }
                else if (centAdvanceAuth_Dict["Result"]["Summary"].ToString() == "LoginSuccess")
                {
                    if (centAdvanceAuth.returnedCookie != null)
                    {
                        Session["podFQDN"] = centAdvanceAuth_Dict["Result"]["PodFqdn"].ToString();                        
                        Session["OTP"] = centAdvanceAuth_Dict["Result"]["Auth"].ToString();
                        Session["ASPXAUTH"] = centAdvanceAuth.returnedCookie.Value;
                        HttpContext.Current.Response.Cookies.Add(centAdvanceAuth.returnedCookie);
                        HttpContext.Current.Response.Headers.Add("Authorization", "Bearer " + Session["ASPXAUTH"].ToString());                     

                        String TransferPage = "<script>window.open('../" + Context.Request.QueryString["ReturnUrl"] + "','_self');</script>";
                        ScriptManager.RegisterStartupScript(Page, Page.GetType(), "temp", TransferPage, false);                       
                    }
                    else
                    {
                        FailureText.Text = "Error: Please contact your system administrator. Error Reason: Valid cookie was not returned.";
                        ErrorMessage.Visible = true;
                    }

                    return "LoginSuccess";

                }
                else if (centAdvanceAuth_Dict["Result"]["Summary"].ToString() == "StartNextChallenge")
                {
                    if (Session["ResetMechId"] != null)
                    {
                        NewPassValidator.Enabled = true;
                        ConfirmNewPassValidator.Enabled = true;
                        NewPass_Label.Visible = true;
                        NewPass.Visible = true;
                        ConfirmNewPass_Label.Visible = true;
                        ConfirmNewPass.Visible = true;
                        ForgotPass_Submit_button.Visible = true;
                    }
                    else
                    {
                        if (AuthMethod_Second.Items.Count != 0)
                        {
                            AuthMethod.Items.Clear();
                            Session["UPMechId"] = "";

                            AuthMethod_Label.Visible = false;
                            AuthMethod.Visible = false;
                            AuthMethod_Label_Second.Visible = true;
                            AuthMethod_Second.Visible = true;
                            Login.Visible = true;
                        }
                        else
                        {
                            FailureText.Text = "Error: Please contact your system administrator. Error Reason: Unknown Start Next Cahllenge.";
                            ErrorMessage.Visible = true;
                        }
                    }

                    return "StartNextChallenge";
                }
                else if (centAdvanceAuth_Dict["Result"]["Summary"].ToString() == "NewPackage")
                {
                    Session["StartAuth"] = centAdvanceAuth.returnedResponse;
                    AuthMethod.Items.Clear();
                    SetDropDowns(centAdvanceAuth_Dict, null);
                }
            }
            else
            {
                FailureText.Text = centAdvanceAuth_Dict["Message"].ToString();
                ErrorMessage.Visible = true;
            }

            return "NewPackage";
        }
        public void SetDropDowns(Dictionary<string, dynamic> centStartAuth_Dict, object sender)
        {
            ArrayList centStartAuth_Challenges = centStartAuth_Dict["Result"]["Challenges"];

            Dictionary<string, object> centStartAuth_Mech1 = null;
            Dictionary<string, object> centStartAuth_Mech2 = null;

            //Parse Mechlist 1 (Required List.)
            centStartAuth_Mech1 = (Dictionary<string, object>)centStartAuth_Challenges[0];

            foreach (ArrayList mechs in centStartAuth_Mech1.Values)
            {
                if (mechs.Count > 1)
                {
                    //More then one required Mech
                    SortedList<string, string> items = new SortedList<string, string>();
                    int iDupCount = 1;
                    
                    foreach (Dictionary<string, object> mech in mechs)
                    {
                        string strFieldName = "";

                        if (mech["Name"].ToString() == "UP")
                        {
                            strFieldName = "Password";
                        }
                        else if (mech["Name"].ToString() == "OTP")
                        {
                            strFieldName = "Mobile Authenticator";
                        }
                        else if (mech["Name"].ToString() == "EMAIL")
                        {
                            strFieldName = "Email..." + mech["PartialAddress"];
                        }
                        else if (mech["Name"].ToString() == "SMS")
                        {
                            strFieldName = "SMS...-" + mech["PartialDeviceAddress"];
                        }
                        else if (mech["Name"].ToString() == "PF")
                        {
                            strFieldName = "Phone Call...-" + mech["PartialPhoneNumber"];
                        }
                        else if (mech["Name"].ToString() == "SQ")
                        {
                            strFieldName = "Secret Question";
                            SecretQuestion_Label.Text = "Secret Question: " + mech["Question"];
                        }
                        else
                        {
                            FailureText.Text = "Error: Please contact your system administrator. Error Reason: Unknown mechanism detected.";
                            ErrorMessage.Visible = true;
                        }

                        string strFieldValue = mech["MechanismId"].ToString();

                        if (!items.ContainsKey(strFieldName))
                        {
                            items.Add(strFieldName, strFieldValue);
                        }
                        else
                        {
                            items.Add(strFieldName + "(" + iDupCount + ")", strFieldValue);
                            iDupCount++;
                        }
                    }

                    try
                    {
                        AuthMethod.DataTextField = "Key";
                        AuthMethod.DataValueField = "Value";
                        AuthMethod.DataSource = items;
                        AuthMethod.DataBind();

                    }
                    catch (Exception ex)
                    {

                        FailureText.Text = "Error: Please contact your system administrator. Error Reason: Could not add mechanism to list: " + ex;
                        ErrorMessage.Visible = true;
                    }

                    if (AuthMethod.SelectedItem.Text == "Password")
                    {
                        Password.Visible = true;
                        Password_Label.Visible = true;
                    }
                    else
                    {
                        Password.Visible = false;
                        Password_Label.Visible = false;
                    }
                }
                else
                {
                    SortedList<string, string> items = new SortedList<string, string>();
                    int iDupCount = 1;

                    //Only one required Mech
                    foreach (Dictionary<string, object> mech in mechs)
                    {
                        string strFieldName = "";

                        if (mech["Name"].ToString() != "UP")
                        {
                            if (mech["Name"].ToString() == "OTP")
                            {
                                strFieldName = "Mobile Authenticator";
                            }
                            else if (mech["Name"].ToString() == "EMAIL")
                            {
                                strFieldName = "Email..." + mech["PartialAddress"];
                            }
                            else if (mech["Name"].ToString() == "SMS")
                            {
                                strFieldName = "SMS...-" + mech["PartialDeviceAddress"];
                            }
                            else if (mech["Name"].ToString() == "PF")
                            {
                                strFieldName = "Phone Call...-" + mech["PartialPhoneNumber"];
                            }
                            else if (mech["Name"].ToString() == "SQ")
                            {
                                strFieldName = "Secret Question";
                                SecretQuestion_Label.Text = "Secret Question: " + mech["Question"];
                            }
                            else
                            {
                                FailureText.Text = "Error: Please contact your system administrator. Error Reason: Unknown mechanism detected.";
                                ErrorMessage.Visible = true;
                            }

                            string strFieldValue = mech["MechanismId"].ToString();

                            if (!items.ContainsKey(strFieldName))
                            {
                                items.Add(strFieldName, strFieldValue);
                            }
                            else
                            {
                                items.Add(strFieldName + "(" + iDupCount + ")", strFieldValue);
                                iDupCount++;
                            }

                            Password.Visible = false;
                            Password_Label.Visible = false;
                        }
                        else
                        {
                            Session["UPMechId"] = mech["MechanismId"].ToString();
                            Password.Visible = true;
                            Password_Label.Visible = true;
                        }
                    }

                    try
                    {
                        AuthMethod.DataTextField = "Key";
                        AuthMethod.DataValueField = "Value";
                        AuthMethod.DataSource = items;
                        AuthMethod.DataBind();

                    }
                    catch (Exception ex)
                    {

                        FailureText.Text = "Error: Please contact your system administrator. Error Reason: Could not add mechanism to list: " + ex;
                        ErrorMessage.Visible = true;
                    }
                }
            }
            
            //If there is more then one mech list
            if (centStartAuth_Challenges.Count > 1)
            {
                centStartAuth_Mech2 = (Dictionary<string, object>)centStartAuth_Challenges[1];

                foreach (ArrayList mechs in centStartAuth_Mech2.Values)
                {
                    SortedList<string, string> items = new SortedList<string, string>();
                    int iDupCount = 1;
                    foreach (Dictionary<string, object> mech in mechs)
                    {
                        string strFieldName = "";

                        if (mech["Name"].ToString() != "RESET")
                        {
                            if (mech["Name"].ToString() == "UP")
                            {
                                strFieldName = "Password";
                            }
                            else if (mech["Name"].ToString() == "OTP")
                            {
                                strFieldName = "Mobile Authenticator";
                            }
                            else if (mech["Name"].ToString() == "EMAIL")
                            {
                                strFieldName = "Email..." + mech["PartialAddress"];
                            }
                            else if (mech["Name"].ToString() == "SMS")
                            {
                                strFieldName = "SMS...-" + mech["PartialDeviceAddress"];
                            }
                            else if (mech["Name"].ToString() == "PF")
                            {
                                strFieldName = "Phone Call...-" + mech["PartialPhoneNumber"];
                            }
                            else if (mech["Name"].ToString() == "SQ")
                            {
                                strFieldName = "Secret Question";
                                SecretQuestion_Label.Text = "Secret Question: " + mech["Question"];
                            }
                            else
                            {
                                FailureText.Text = "Error: Please contact your system administrator. Error Reason: Unknown mechanism detected.";
                                ErrorMessage.Visible = true;
                            }

                            string strFieldValue = mech["MechanismId"].ToString();

                            if (!items.ContainsKey(strFieldName))
                            {
                                items.Add(strFieldName, strFieldValue);
                            }
                            else
                            {
                                items.Add(strFieldName + "(" + iDupCount + ")", strFieldValue);
                                iDupCount++;
                            }

                                           
                        }
                        else
                        {
                            Session["ResetMechId"] = mech["MechanismId"].ToString();
                        }
                    }

                    try
                    {
                        AuthMethod_Second.DataTextField = "Key";
                        AuthMethod_Second.DataValueField = "Value";
                        AuthMethod_Second.DataSource = items;
                        AuthMethod_Second.DataBind();
                    }
                    catch (Exception ex)
                    {

                        FailureText.Text = "Error: Please contact your system administrator. Error Reason: Could not add mechanism to list: " + ex;
                        ErrorMessage.Visible = true;
                    }
                }
            }

            //Turn on Stage 2 items
            UserName.Visible = true;
            UserName_Label.Visible = true;
            RememberMe.Visible = true;
            RememberMe_Label.Visible = true;

            if (sender == ForgotPass_button)
            {
                ForgotPass_button.Visible = false;
                RememberMe.Visible = false;
                RememberMe_Label.Visible = false;
                pwdValidator.Visible = false;              
            }
            else
            {
                ForgotPass_button.Visible = true;
            }

            //Turn off Stage 1 Items
            Next.Visible = false;
            UserName.Enabled = false;

            Login.Visible = true;

            if (AuthMethod.Items.Count != 0)
            {
                AuthMethod.Visible = true;
                AuthMethod_Label.Visible = true;
            }
            else if (AuthMethod.Items.Count == 0 && AuthMethod_Second.Items.Count != 0)
            {
                AuthMethod_Second.Visible = true;
                AuthMethod_Label_Second.Visible = true;
            }

            MFAAnswer.Visible = false;
            MFAAnswer_Label.Visible = false;
            MFAAnswer_Submit.Visible = false;
            MFAMessage.Visible = false;          
        }
        protected void AuthMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (AuthMethod.Items.FindByText("Secret Question") != null)
            {
                if (AuthMethod.SelectedItem.Text == "Secret Question")
                {
                    SecretQuestion.Visible = true;
                    SecretQuestion_Label.Visible = true;
                }
                else
                {
                    SecretQuestion.Visible = false;
                    SecretQuestion_Label.Visible = false;
                }
            }

            if (AuthMethod.Items.FindByText("Password") != null)
            {
                if (AuthMethod.SelectedItem.Text == "Password")
                {
                    Password.Visible = true;
                    Password_Label.Visible = true;
                }
                else
                {
                    Password.Visible = false;
                    Password_Label.Visible = false;
                }               

                Next.Visible = false;
                RememberMe.Visible = true;
                RememberMe_Label.Visible = true;
                ForgotPass_button.Visible = true;
                Login.Visible = true;
                AuthMethod.Visible = true;
                AuthMethod_Label.Visible = true;
            }
            else
            {
                Password.Visible = true;
                Password_Label.Visible = true;
                Next.Visible = false;
                RememberMe.Visible = true;
                RememberMe_Label.Visible = true;
                ForgotPass_button.Visible = true;
                Login.Visible = true;
                AuthMethod.Visible = true;
                AuthMethod_Label.Visible = true;
            }
        }
        protected void AuthMethod_Second_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (AuthMethod_Second.Items.FindByText("Secret Question") != null)
            {
                if (AuthMethod_Second.SelectedItem.Text == "Secret Question")
                {
                    SecretQuestion.Visible = true;
                    SecretQuestion_Label.Visible = true;
                }
                else
                {
                    SecretQuestion.Visible = false;
                    SecretQuestion_Label.Visible = false;
                }
            }

            if (AuthMethod_Second.Items.FindByText("Password") != null)
            {             
                if (AuthMethod_Second.SelectedItem.Text == "Password")
                {
                    Password.Visible = true;
                    Password_Label.Visible = true;
                }
                else
                {
                    Password.Visible = false;
                    Password_Label.Visible = false;
                }
               

                Next.Visible = false;
                RememberMe.Visible = true;
                RememberMe_Label.Visible = true;
                ForgotPass_button.Visible = true;
                Login.Visible = true;
                AuthMethod_Second.Visible = true;
                AuthMethod_Label_Second.Visible = true;
            }
            else
            {
                Password.Visible = true;
                Password_Label.Visible = true;
                Next.Visible = false;
                RememberMe.Visible = true;
                RememberMe_Label.Visible = true;
                ForgotPass_button.Visible = true;
                Login.Visible = true;
                AuthMethod_Second.Visible = true;
                AuthMethod_Label_Second.Visible = true;
            }
        }

        public void StartPoll()
        {
            bool bStopPolling = false;
            //Keep Polling until server receives accepted challenge
            while (!bStopPolling)
            {
                string strAdvanceAuthPollJSON = null;
                string pollResult = "Poll";

                if (AuthMethod.Items.Count != 0)
                {
                    strAdvanceAuthPollJSON = @"{""TenantId"":""" + Session["TenantId"].ToString() + @""",""SessionId"":""" + Session["SessionId"].ToString() + @""",""PersistentLogin"":" + RememberMe.Checked.ToString().ToLower() + @",""MechanismId"":""" + AuthMethod.SelectedValue.ToString() + @""",""Action"":""Poll""}";
                }
                else if (AuthMethod.Items.Count == 0 && AuthMethod_Second.Items.Count != 0)
                {
                    strAdvanceAuthPollJSON = @"{""TenantId"":""" + Session["TenantId"].ToString() + @""",""SessionId"":""" + Session["SessionId"].ToString() + @""",""PersistentLogin"":" + RememberMe.Checked.ToString().ToLower() + @",""MechanismId"":""" + AuthMethod_Second.SelectedValue.ToString() + @""",""Action"":""Poll""}";
                }

                if (strAdvanceAuthPollJSON != null)
                {
                    pollResult = ProcessAdvanceAuth(strAdvanceAuthPollJSON);
                }
                else
                {
                    pollResult = "ERROR";
                    FailureText.Text = "Error: Please contact your system administrator. Error Reason: Unknown Mech Answer Type.";
                    ErrorMessage.Visible = true;
                }

                if (pollResult != "Poll")
                {
                    bStopPolling = true;
                }
            }            
        }

        public void PollOnce()
        {
            string strAdvanceAuthPollJSON = null;
            string pollResult = "Poll";

            if (AuthMethod.Items.Count != 0)
            {
                strAdvanceAuthPollJSON = @"{""TenantId"":""" + Session["TenantId"].ToString() + @""",""SessionId"":""" + Session["SessionId"].ToString() + @""",""PersistentLogin"":" + RememberMe.Checked.ToString().ToLower() + @",""MechanismId"":""" + AuthMethod.SelectedValue.ToString() + @""",""Action"":""Poll""}";
            }
            else if (AuthMethod.Items.Count == 0 && AuthMethod_Second.Items.Count != 0)
            {
                strAdvanceAuthPollJSON = @"{""TenantId"":""" + Session["TenantId"].ToString() + @""",""SessionId"":""" + Session["SessionId"].ToString() + @""",""PersistentLogin"":" + RememberMe.Checked.ToString().ToLower() + @",""MechanismId"":""" + AuthMethod_Second.SelectedValue.ToString() + @""",""Action"":""Poll""}";
            }

            if (strAdvanceAuthPollJSON != null)
            {
                pollResult = ProcessAdvanceAuth(strAdvanceAuthPollJSON);
            }
            else
            {
                pollResult = "ERROR";
                FailureText.Text = "Error: Please contact your system administrator. Error Reason: Unknown Mech Answer Type.";
                ErrorMessage.Visible = true;
            }

            if (pollResult == "Poll")
            {
                //Timer1.Enabled = true;
            }
        }
}