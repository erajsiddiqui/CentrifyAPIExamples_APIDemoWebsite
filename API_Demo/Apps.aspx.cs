using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;
using System.Configuration;

public partial class Contact : Page
{
    public static string CentGetAppsURL = ConfigurationManager.AppSettings["CentGetAppsURL"].ToString();
    public static string CentPodURL = ConfigurationManager.AppSettings["CentPodURL"].ToString();
    public static string CentRunAppURL = ConfigurationManager.AppSettings["CentRunAppURL"].ToString();

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (Session["OTP"].ToString() != "")
            {
                string loginJSON = @"{""force"":""True""}";
                Centrify_API_Interface cent = new Centrify_API_Interface().MakeRestCall(CentGetAppsURL, loginJSON);

                string strApps = cent.returnedResponse;

                var jss = new JavaScriptSerializer();
                Dictionary<string, dynamic> sData = jss.Deserialize<Dictionary<string, dynamic>>(strApps);

                var dApps = sData["Result"]["Apps"];

                int iCount = 0;

                foreach (var app in dApps)
                {
                    string strDisplayName = app["DisplayName"];
                    string strAppKey = app["AppKey"];
                    string strIcon = app["Icon"];

                    AddUrls(strAppKey, strDisplayName, strIcon, iCount);

                    iCount++;

                }

                NotLoggedIn.Visible = false;
            }
            else
            {
                Apps.Visible = false;
            }
        }
        catch (Exception)
        {
            Apps.Visible = false;            
        }
    }

    protected void AddUrls(string strAppKey, string strName, string strIcon, int count)
    {
        HyperLink link = new HyperLink();
        link.ID = "CentrifyApp" + count;
        link.NavigateUrl = CentRunAppURL + strAppKey + "&Auth=" + Session["OTP"].ToString();
        link.Text = strName;
        link.ImageUrl = CentPodURL + strIcon;
        link.ImageHeight = 75;
        link.ImageWidth = 75;


        if (count % 7 == 0)
        {
            Apps.Controls.Add(new LiteralControl("<br />"));
        }
        else
        {
            Apps.Controls.Add(new LiteralControl("&nbsp; &nbsp; &nbsp; &nbsp; &nbsp;"));

        }

        Apps.Controls.Add(link);
    }
}