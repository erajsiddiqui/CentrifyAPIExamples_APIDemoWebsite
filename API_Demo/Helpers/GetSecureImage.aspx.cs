using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Drawing;
using System.Net;

public partial class GetSecureImage : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string strIcon = Request.QueryString["Icon"].ToString();

        //Get Icon from Centrify
        WebClient wc = new WebClient();
        wc.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.CacheIfAvailable);
        wc.Headers.Add("Authorization", "Bearer " + Session["ASPXAUTH"].ToString());
        byte[] bytes = wc.DownloadData(Session["NewPodURL"].ToString() + strIcon);
        Response.ContentType = "image/jpg";
        Response.BinaryWrite(bytes);
    }
}