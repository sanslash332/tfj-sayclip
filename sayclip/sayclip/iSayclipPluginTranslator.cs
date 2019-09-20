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
        List<SayclipLanguage> getAvailableLanguages(string displayLanguaje);
        void setLanguages(SayclipLanguage fromLang,SayclipLanguage toLang);
        SayclipLanguage[] getConfiguredLanguajes(string displayLanguaje);
        bool initialize();
        Task<string> translate(string text);
        bool haveConfigWindow();
        void showConfigWindow();



    }
}
