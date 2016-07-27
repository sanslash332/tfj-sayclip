using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Web;
using System.Media;

using System.Threading;


namespace MicrosoftTranslatorLib
{
  internal class AdmAuthentication
    {

        public static readonly string DatamarketAccessUri = "https://datamarket.accesscontrol.windows.net/v2/OAuth2-13";
        public static object lockSecurityObj = new object();
        private string clientId;
        private string clientSecret;
        private string request;
        private AdmAccessToken token;
        private Timer accessTokenRenewer;
        private string headerValue;
        public static event Action headerRenewed;
        //Access token expires every 10 minutes. Renew it every 9 minutes only. 
        private const int RefreshTokenDuration = 9;
        public AdmAuthentication(string clientId, string clientSecret)
        {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            //If clientid or client secret has special characters, encode before sending request 
            this.request = string.Format("grant_type=client_credentials&client_id={0}&client_secret={1}&scope=http://api.microsofttranslator.com", HttpUtility.UrlEncode(clientId), HttpUtility.UrlEncode(clientSecret));
            this.token = HttpPost(DatamarketAccessUri, this.request);
            this.generateHeader();
            //renew the token every specified minutes 
            accessTokenRenewer = new Timer(new TimerCallback(OnTokenExpiredCallback), this, TimeSpan.FromMinutes(RefreshTokenDuration), TimeSpan.FromMilliseconds(-1));
        }


        public AdmAccessToken GetAccessToken()
        {
            return this.token;
        }

        public string getHeader()
        {
            return (this.headerValue);

        }

        private void generateHeader()
        {
            Monitor.Enter(lockSecurityObj);
            this.headerValue = "Bearer " + this.token.access_token;
            Monitor.Exit(lockSecurityObj);

        }
        private void RenewAccessToken()
        {
            AdmAccessToken newAccessToken = HttpPost(DatamarketAccessUri, this.request);
            //swap the new token with old one 
            //Note: the swap is thread unsafe 
            Monitor.Enter(lockSecurityObj);
            this.token = newAccessToken;
            Monitor.Exit(lockSecurityObj);
            this.generateHeader();
            headerRenewed();

            outLog.onSendLog(string.Format("Renewed token for user: {0} is: {1}", this.clientId, this.token.access_token),0);
        }
        private void OnTokenExpiredCallback(object stateInfo)
        {
            try
            {
                RenewAccessToken();
            }
            catch (Exception ex)
            {
                outLog.onSendLog(string.Format("Failed renewing access token. Details: {0}", ex.Message), 2);
            }
            finally
            {
                try
                {
                    accessTokenRenewer.Change(TimeSpan.FromMinutes(RefreshTokenDuration), TimeSpan.FromMilliseconds(-1));
                }
                catch (Exception ex)
                {
                    outLog.onSendLog(string.Format("Failed to reschedule the timer to renew access token. Details: {0}", ex.Message),1);
                }
            }
        }
        private AdmAccessToken HttpPost(string DatamarketAccessUri, string requestDetails)
        {
            //Prepare OAuth request  

            try
            {



                WebRequest webRequest = WebRequest.Create(DatamarketAccessUri);
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.Method = "POST";
                byte[] bytes = Encoding.ASCII.GetBytes(requestDetails);
                webRequest.ContentLength = bytes.Length;
                using (Stream outputStream = webRequest.GetRequestStream())
                {
                    outputStream.Write(bytes, 0, bytes.Length);
                }
                using (WebResponse webResponse = webRequest.GetResponse())
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AdmAccessToken));
                    //Get deserialized object from JSON stream 
                    AdmAccessToken token = (AdmAccessToken)serializer.ReadObject(webResponse.GetResponseStream());
                    outLog.onSendLog(string.Format("tocken optained: {0}", token.access_token),2);
                    return token;
                }
            }
           catch(Exception ex)
            {
                outLog.onSendLog(string.Format("Error retrieving the acces tocken: {0}",ex.Message), 2);
            }
            return (new AdmAccessToken());

        }
    }


}
