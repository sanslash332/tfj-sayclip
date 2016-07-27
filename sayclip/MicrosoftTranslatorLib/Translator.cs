using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;


namespace MicrosoftTranslatorLib
{
    public class Translator
    {
        /// <summary> Class that works as a wrapper for the microsoft translator soap.
        ///     
        /// </summary>
        private string appId;
        private string appSecret;
        private AdmAuthentication auth;
        private TranslatorService.LanguageServiceClient client;
        private string headerValue;
        private HttpRequestMessageProperty httprequestProperty;
        private TranslatorService.TranslateOptions options;

        /// <summary>
        /// Constructor of the class.
        /// </summary>
        /// <param name="appId">Required, the AppId that you have registered on microsoft asure  </param>
        /// <param name="appSecret"> required: The password for your ID </param>
        public Translator(string appId, string appSecret)
        {
            this.appId = appId;
            this.appSecret = appSecret;
            this.auth = new AdmAuthentication(this.appId, this.appSecret);
            this.client = new TranslatorService.LanguageServiceClient();
            this.options = new TranslatorService.TranslateOptions();
            this.options.Category = "general";
            this.options.ContentType = "text/plain";





            AdmAuthentication.headerRenewed += this.headerUpdated;
            this.headerValue = auth.getHeader();
            this.setHttpMessageProperty();
        }
        
        /// <summary>
        /// A simply method that return the tocken returned from ms servers
        /// </summary>
        /// <returns> The token, or a string with error.</returns>
        public string getTocken()
        {
            AdmAccessToken tock = auth.GetAccessToken();
            if(tock.access_token==null)
            {
                return ("error");
            }
            else
            {
                return (tock.access_token);
            }
            

            
        }

        /// <summary>
        /// Translate a array of text scentences, from one languaje to other.
        /// 
        /// </summary>
        /// <param name="texts"> The list of texts to will be translated </param>
        /// <param name="from"> The source language. Can be empty and set to autodettect. </param>
        /// <param name="to"> The target language.</param>
        /// <returns> The array of translated texts. </returns>
        public string[] translateArray(string[] texts, string from, string to)
        {
            string[] messages;
            using (OperationContextScope scope = new OperationContextScope(this.client.InnerChannel))
            {
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = this.httprequestProperty;
                TranslatorService.TranslateArrayResponse[] responces;
                try
                {

                    responces = this.client.TranslateArray("", texts, from, to, this.options);
                    messages = new string[responces.Length];
                    for(int i = 0; i < responces.Length; i++)
                    {
                        messages[i] = responces[i].TranslatedText;

                    }
                }
                catch (Exception ex)
                {
                    outLog.onSendLog(string.Format("Error when translating: {0}", ex.Message), 2);
                    messages = new string[1];
                    messages[0] = "error";

                }
            }
            return (messages);

        }

        /// <summary>
        ///  Translate one scentense from one languaje to other
        /// </summary>
        /// <param name="text"> The original text to be translated </param>
        /// <param name="from"> Optional: The languaje of the source. You can give a empty string, for active autodettect </param>
        /// <param name="to"> Required: Thelanguage to translate your text. </param>
        /// <returns> The translated text </returns>
        public string translate(string text, string from, string to)
        {
            using (OperationContextScope scope = new OperationContextScope(this.client.InnerChannel))
            {
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = this.httprequestProperty;
                string result = "";
                try
                {
                
                    result = this.client.Translate("", text, from, to, "text/plain", "general", "");
                    return (result);
                }
                 catch(Exception ex)
                {
                    outLog.onSendLog(string.Format("Error when translating: {0}",ex.Message), 2);
                }
                return (result);

            }
        }
        private void headerUpdated()
        {
            Monitor.Enter(AdmAuthentication.lockSecurityObj);
            headerValue = auth.getHeader();
            this.setHttpMessageProperty();
            Monitor.Exit(AdmAuthentication.lockSecurityObj);

        }
        private void setHttpMessageProperty()
        {
           HttpRequestMessageProperty httprequestPropertytemp = new HttpRequestMessageProperty();
            httprequestPropertytemp.Method = "POST";
            httprequestPropertytemp.Headers.Add("Authorization", this.headerValue);
            httprequestProperty = httprequestPropertytemp;

        }


    }
}
