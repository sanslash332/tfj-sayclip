using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sayclip
{
    class EmptyPlugin : iSayclipPluginTranslator
    {
        public const String emptyName = "empty plugin";
        public async Task<IEnumerable<SayclipLanguage>> getAvailableLanguages(string displayLanguaje)
        {
            List<SayclipLanguage> langs = new List<SayclipLanguage>();

            langs.Add(new SayclipLanguage("code","empty"));
            return (langs);

        }

        public SayclipLanguage[] getConfiguredLanguajes(string displayLanguaje)
        {
            SayclipLanguage[] langs = new SayclipLanguage[]
            {
                new SayclipLanguage("code","empty"),
                new SayclipLanguage("code","empty")
            };
            return (langs);
        }

        public string getDescription(string languaje)
        {
            return ("");
        }

        public string getName()
        {
            return (emptyName);
        }

        public bool haveConfigWindow()
        {
            return (false);
        }

        public bool initialize()
        {
            return (true);
        }

        public void setLanguages(SayclipLanguage fromLang, SayclipLanguage toLang)
        {
            
        }

        public void showConfigWindow(string lang)
        {
            
        }

        public async Task<string> translate(string text)
        {
            
            return (text);

        }
    }
}
