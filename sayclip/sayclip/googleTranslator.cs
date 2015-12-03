using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Web;

namespace sayclip
{
    static class googleTranslator
    {
        static private string baseUrl = "https://translate.google.com/translate_a/single?client=t&dt=bd&dt=ex&dt=ld&dt=md&dt=qca&dt=rw&dt=rm&dt=ss&dt=t&dt=at&ie=UTF-8&oe=UTF-8&otf=1&ssel=0&tsel=0";
        static public string source = "";
        static public string target = "";
        
        static public string translate(string text)
        {
            //Console.WriteLine("texto a traducir: {0}", WebUtility.UrlEncode(text));
            text = prepareText(text);
            string fullurl = baseUrl + "&sl=" + source + "&tl="+target+"&hl="+target+"&tk="+ generateTocken()+"&q=" + WebUtility.UrlEncode(text);

            string results = connect(fullurl);
            
            
            

            return (results);
        }

        static private string prepareText(string text)
        {
            //Console.WriteLine("texto sin preprocesar: {0}", text);
            text = text.Replace('"', '-');

            text = text.Replace(".", " PPPUNTOPPP ");
            text = text.Replace("\n", " LALOCALINEA ");
            text = text.Replace("\r", " ELRETROTROSESO ");
            //Console.WriteLine("texto preprocesado para enviar: {0}", text);
            return (text);

        }

        static private string generateTocken()
        {
            return ("asjidojasoidjoiasdsakdlsakldksadjasiii1118282");
        }

        static private string processTranslation(string original)
        {
            string text = original;
            //Console.WriteLine("texto a decodificar: {0}", text);
            text = text.Replace("PPPUNTOPPP",". ");
            text = text.Replace( "LALOCALINEA","\n");
            text = text.Replace( "ELRETROTROSESO","\r");

            string[] partes = text.Split('"');
            //Console.WriteLine("texto decodificado: {0}", partes[1]);
            return (partes[1]);
        }

        static private string connect(string url)
        {
            
            using(webclientGalleta wc = new webclientGalleta())
            {
                wc.Encoding = Encoding.UTF8;
                string datos = "";

                try
                {
                    datos = wc.DownloadString(url);
                }
                catch (Exception e)
                {
                    return ("error: " + e.Message);
                    
                }
                 
                if(datos!="")
                {
                    return (processTranslation(datos));
                }
                else
                {
                    return ("error");
                }
            }
        }



    }
}
