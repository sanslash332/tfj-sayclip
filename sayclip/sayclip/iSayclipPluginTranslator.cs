using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sayclip
{
  public interface iSayclipPluginTranslator
    {
        public string getName();
        public string getDescription(string languaje);
        public Dictionary<string, string> getAvailableLanguages(string displayLanguaje);
        public void setLanguages(string fromLang, string toLang);
        public string translate(string text);
        public bool haveConfigWindow();
        public System.Windows.Window getConfigWindow();



    }
}
