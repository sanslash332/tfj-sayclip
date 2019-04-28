using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sayclip
{
  public interface iSayclipPluginTranslator
    {
        string getName();
        string getDescription(string languaje);
        Dictionary<string, string> getAvailableLanguages(string displayLanguaje);
        void setLanguages(string fromLang, string toLang);
        string[] getConfiguredLanguajes(string displayLanguaje);
        bool initialize();
        Task<string> translate(string text);
        bool haveConfigWindow();
        void showConfigWindow();



    }
}
