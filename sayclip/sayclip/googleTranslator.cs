using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using logSystem;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Web;
using RavSoft.GoogleTranslator;



namespace sayclip
{
    class googleTranslator : iTranslator
    {
         private string baseUrl = "https://translate.google.com/translate_a/single?client=t&dt=bd&dt=ex&dt=ld&dt=md&dt=qca&dt=rw&dt=rm&dt=ss&dt=t&dt=at&ie=UTF-8&oe=UTF-8&otf=1&ssel=0&tsel=0";
         public string source = "";
         public string target = "";
         private Translator translator;
         private string completeSource;
         private string completeTarget;

        
         public googleTranslator()
        {
            this.source = sayclip.Properties.Settings.Default.lan1;
            this.target = sayclip.Properties.Settings.Default.lan2;
            this.translator = new Translator();
            logSystem.LogWriter.escribir(string.Format("checking languaje conversion for google api: source {0}, target {1}", this.source, this.target));
            foreach(KeyValuePair<string,string> k in Translator.LanguagesAndCodes)
            {
                logSystem.LogWriter.escribir(string.Format("checking current entry: {0}, {1}", k.Key, k.Value));
                if(source.Equals(k.Value))
                {
                    logSystem.LogWriter.escribir("detected source");
                    
                    completeSource = k.Key;
                }
                if(target.Equals(k.Value))
                {
                    logSystem.LogWriter.escribir("detected target");
                    completeTarget = k.Key;
                }
            }
            

        }


         public string translate(string text)
         {
             return (translator.Translate(text, completeSource, completeTarget));

         }

         string[] iTranslator.separateSentences(string text)
         {
             throw new NotImplementedException();
         }

         string iTranslator.unificateSentences(string[] sentences)
         {
             throw new NotImplementedException();
         }
    }
}
