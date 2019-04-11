using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sayclip;
using System.ComponentModel.Composition;
using GoogleTranslateFreeApi;

namespace googleTranslatorPlugin
{
    [Export(typeof(iSayclipPluginTranslator))]
    public class Translator : iSayclipPluginTranslator
    {
        private Language fromLang;
        private Language toLang;
        private const string pluginName = "google translator plugin";
        private const string description = "plugin for use the google translator free service with sayclip";
        private GoogleTranslator translator;

        public Translator()
        {
            this.translator = new GoogleTranslator();

        }

        public Dictionary<string, string> getAvailableLanguages(string displayLanguaje)
        {
            Dictionary<string, string> langDict = new Dictionary<string, string>();
            foreach(Language l in GoogleTranslator.LanguagesSupported)
            {
                langDict.Add(l.FullName, l.ISO639);

            }
            return langDict;
            
        }

        /*
         * 
        public System.Windows.Window getConfigWindow()
        {
            throw new NotImplementedException();
        }
        */

        public string getDescription(string languaje)
        {
            return description;
        }

        public string getName()
        {
            return pluginName;

        }

        public bool haveConfigWindow()
        {
            return false;
        }

        public void setLanguages(string fromLang, string toLang)
        {
            this.fromLang = GoogleTranslator.GetLanguageByISO(fromLang);
            this.toLang = GoogleTranslator.GetLanguageByISO(toLang);

        }

        public async Task<string> translate(string text)
        {
            TranslationResult resultado = await translator.TranslateLiteAsync(text, this.fromLang, this.toLang);
            return resultado.MergedTranslation;

        }
    }
}
