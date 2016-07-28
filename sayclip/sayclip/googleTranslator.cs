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


        public string repairLines(string txt)
        {
            //logSystem.LogWriter.escribir("The text to repair is: " + txt);
        
            string final = "";
            for(int i=0;i<txt.Length;i++)
            {
                if(i>=txt.Length)
                {
                    break;
                }
                if(i==txt.Length-1 || i==txt.Length-2 || i==txt.Length-3)
                {
                    final += txt[i];
                    continue;
                }
                string analyce = txt[i].ToString() + txt[i + 1].ToString()+txt[i+2].ToString()+txt[i+3].ToString();
                //logSystem.LogWriter.escribir("the analyzed part is: " + analyce);
                if(analyce.Equals("\\r\\n"))
                {
                    i += 4;
                    continue;
                }
                else
                {
                    final += txt[i];
                }
                

            }
            return (final);
        }

        private string translateLarge(string text)
        {
            string[] parts = separateSentences(text);
            List<string> translatedParts = new List<string>();
            foreach(string part in parts)
            {
                translatedParts.Add(this.translator.Translate(part, this.completeSource, this.completeTarget));

            }

            return unificateSentences(translatedParts.ToArray());

        }

         public string translate(string text)
         {
            if(text.Length<=2000)
            {
                return(repairLines(translator.Translate(text, completeSource, completeTarget)));
            }
            else if(text.Length<=6000)
            {
                ScreenReaderControl.speech("You're translating more than 2000 characters! This can take a long while!",true);
                return repairLines(translateLarge(text));
            }
            else
            {
                return ("The text is too long for the google translator api. Please copy less text, or use microsoft api instead.");
            }
             

         }

         public string[] separateSentences(string text)
         {
            List<string> prefinal = new List<string>();
            string[] parts = text.Split('.');
            foreach(string s in parts)
            {
                if (s.Length <= 2000)
                {
                    prefinal.Add(s);
                }
                else
                {
                    string part = "";
                    foreach(char c in s.ToCharArray())
                    {
                        if(part.Length==2000)
                        {
                            prefinal.Add(part);
                            part = "";
                        }
                        part += c.ToString();
                    }
                    if(!part.Equals(""))
                    {
                        prefinal.Add(part);
                    }

                }
            }
            return prefinal.ToArray();
         }

         public string unificateSentences(string[] sentences)
         {
            string final = "";
            foreach(string s in sentences)
            {
                final += s + ". ";
            }
            return final;
         }
    }
}
