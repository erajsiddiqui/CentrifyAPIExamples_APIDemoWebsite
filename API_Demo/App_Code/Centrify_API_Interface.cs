using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Web.UI;
using System.Configuration;

/// <summary>
/// Summary description for Centrify_API_Interface
/// </summary>
public partial class Centrify_API_Interface : Page
{
    public HttpCookie returnedCookie { get; set; }
    public String returnedResponse { get; set; }

	public Centrify_API_Interface()
	{
        
	}

    public Centrify_API_Interface MakeRestCall(string CentEndPoint, string JSON_Payload)
    {       
        string PostData = JSON_Payload;

        var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(CentEndPoint);

        request.Method = "POST";
        request.ContentLength = 0;
        request.ContentType = "application/json";
        request.Headers.Add("X-CENTRIFY-NATIVE-CLIENT", "1");

        //Create Container for Cookie
        request.CookieContainer = new CookieContainer();

        if (Context.Request.Cookies.AllKeys.Contains(".ASPXAUTH"))
        {
            Cookie cCookie = new Cookie();
            cCookie.Name = ".ASPXAUTH";
            Uri uDomain = new Uri(CentEndPoint);
            cCookie.Domain = uDomain.Host;
            cCookie.Value = Context.Request.Cookies[".ASPXAUTH"].Value;

            request.CookieContainer.Add(cCookie);
        }

        if (!string.IsNullOrEmpty(PostData))
        {
            var encoding = new System.Text.UTF8Encoding();
            var bytes = System.Text.Encoding.GetEncoding("iso-8859-1").GetBytes(PostData);
            request.ContentLength = bytes.Length;

            using (var writeStream = request.GetRequestStream())
            {
                writeStream.Write(bytes, 0, bytes.Length);
            }
        }

        using (var response = (System.Net.HttpWebResponse)request.GetResponse())
        {
            var responseValue = string.Empty;


            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var message = String.Format("Request failed. Received HTTP {0}", response.StatusCode);
                throw new ApplicationException(message);
            }

            // grab the response
            using (var responseStream = response.GetResponseStream())
            {
                if (responseStream != null)
                    using (var reader = new System.IO.StreamReader(responseStream))
                    {
                        responseValue = reader.ReadToEnd();
                    }
            }

            returnedCookie = null;

            if (response.Cookies[".ASPXAUTH"] != null)
            {
                if (response.Cookies[".ASPXAUTH"].Value != "")
                {
                    HttpCookie cASPXAuth = new HttpCookie(".ASPXAUTH");
                    cASPXAuth.Value = response.Cookies[".ASPXAUTH"].Value;
                    //cASPXAuth.Domain = "kibble.centrify.com";
                    cASPXAuth.Expires = response.Cookies[".ASPXAUTH"].Expires;

                    returnedCookie = cASPXAuth;
                }
            }
                       
            returnedResponse = responseValue;

            return this;
        }
    }
}