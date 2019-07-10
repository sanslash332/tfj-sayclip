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
    [Export(typeof(sayclip.iSayclipPluginTranslator))]
    public class Translator : sayclip.iSayclipPluginTranslator
    {
        private Language fromLang;
        private Language toLang;
        private const string pluginName = "google translator plugin";
        private const string description = "plugin for use the google translator free service with sayclip";
        private GoogleTranslator translator;

        public Translator()
        {
            this.translator = new GoogleTranslator();
            if(Properties.Settings.Default.fromLang != null)
            {
                this.fromLang = GoogleTranslator.GetLanguageByISO(Properties.Settings.Default.fromLang);


            }
            if(Properties.Settings.Default.toLang != null)
            {
                this.toLang = GoogleTranslator.GetLanguageByISO(Properties.Settings.Default.toLang);

            }

        }

        public Dictionary<string, string> getAvailableLanguages(string displayLanguaje)
        {
            Dictionary<string, string> langDict = new Dictionary<string, string>();
            foreach(Language l in GoogleTranslator.LanguagesSupported)
            {
                langDict.Add(l.ISO639, l.FullName);

            }
            return langDict;
            
        }

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
            Properties.Settings.Default.fromLang = this.fromLang.ISO639;
            Properties.Settings.Default.toLang = this.toLang.ISO639;
            Properties.Settings.Default.Save();

        }
        
        public string[] getConfiguredLanguajes(string displayLanguaje)
        {
            string[] langs = new string[2];
            langs[0] = this.fromLang.FullName;
            langs[1] = this.toLang.FullName;
            return langs;

        }
        public async Task<string> translate(string text)
        {
            TranslationResult resultado = await translator.TranslateLiteAsync(text, this.fromLang, this.toLang);
            return resultado.MergedTranslation;

        }

        public bool initialize()
        {
            return true;
        }

        
        public void showConfigWindow()
        {
            throw new NotImplementedException();
        }
    }
}
