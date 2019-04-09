using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sayclip
{
  public interface iTranslator
    {
        string getName();
        Dictionary<string, string> getLanguages();
        void setLanguages(string fromLang, string toLang);



        
        string translate(string text);
        string[] separateSentences(string text);
        string unificateSentences(string[] sentences);

    }
}
