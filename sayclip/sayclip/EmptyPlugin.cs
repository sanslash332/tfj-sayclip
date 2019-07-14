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
        public Dictionary<string, string> getAvailableLanguages(string displayLanguaje)
        {
            Dictionary<String, String> langs = new Dictionary<string, string>();
            langs.Add("","empty");
            return (langs);


        }

        public string[] getConfiguredLanguajes(string displayLanguaje)
        {
            String[] langs = new String[]
            {
                "empty",
                "empty"
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

        public void setLanguages(string fromLang, string toLang)
        {
            
        }

        public void showConfigWindow()
        {
            
        }

        public async Task<string> translate(string text)
        {
            
            return (text);

        }
    }
}
