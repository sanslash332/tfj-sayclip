using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicrosoftTranslatorLib;



namespace sayclip
{
    class MicrosoftTranslator : iTranslator
    {
        
        public string sourceLang;
        public string targetLang;
        private string msappid;
        private string msappsecret;
        private Translator translator;


        public MicrosoftTranslator()
        {
            MicrosoftTranslatorLib.outLog.sendLog += outLog_sendLog;
            this.sourceLang = sayclip.Properties.Settings.Default.lan1;
            this.targetLang = sayclip.Properties.Settings.Default.lan2;
            this.msappid = sayclip.Properties.Settings.Default.msAppID;
            this.msappsecret = sayclip.Properties.Settings.Default.msAppSecret;
            this.translator = new Translator(this.msappid, this.msappsecret);
            

            
            


        }

        void outLog_sendLog(string arg1, int arg2)
        {
            logSystem.LogWriter.escribir("Problem detected in ms translator: " + arg1);
        }

        
        public bool checkTocken()
        {
            string tocken = this.translator.getTocken();
            logSystem.LogWriter.escribir("the current tocken: " + tocken);
            if(tocken.Equals("error"))
            {
                return (false);
            }
            else
            {
                return (true);
            }

        }
        
        public  string translate(string text)
        {
            
            if(text.Length<=2000)
            {
                return (this.translator.translate(text, this.sourceLang, this.targetLang));
            }
            else if(text.Length<=6000)
            {
                ScreenReaderControl.speech("You're translating more than 2000 characters! This can take a long while!",true);
                return(unificateSentences(this.translator.translateArray(this.separateSentences(text),this.sourceLang,this.targetLang)));
            }
            else
            {
                return("This tool isn't for translate a book, please copy less characters");
            }

        }

        public string unificateSentences(string[] sentences)
        {
            string final = "";
            foreach(string s in sentences)
            {
                final += s + ". ";
            }
            return (final);

            
        }

        public string[] separateSentences(string text)
        {
            List<string> prefinal = new List<string>();
            string[] parts = text.Split('.');
            foreach (string s in parts)
            {
                if (s.Length <= 2000)
                {
                    prefinal.Add(s);
                }
                else
                {
                    string part = "";
                    foreach (char c in s.ToCharArray())
                    {
                        if (part.Length == 2000)
                        {
                            prefinal.Add(part);
                            part = "";
                        }
                        part += c.ToString();
                    }
                    if (!part.Equals(""))
                    {
                        prefinal.Add(part);
                    }

                }
            }
            return prefinal.ToArray();
        }



    }
}
