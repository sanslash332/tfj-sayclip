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
            if(this.fromLang==null)
            {
                this.fromLang = Language.Auto;
            }
            if(this.toLang==null)
            {
                this.toLang = Language.Spanish;

            }
            this.setLanguages(new SayclipLanguage(this.fromLang.ISO639, this.fromLang.FullName), new SayclipLanguage(this.toLang.ISO639, this.toLang.FullName));

        }

        public List<sayclip.SayclipLanguage> getAvailableLanguages(string displayLanguaje)
        {
            List<sayclip.SayclipLanguage> langDict = new List<SayclipLanguage>();
            langDict.Add(new SayclipLanguage(Language.Auto.ISO639, Language.Auto.FullName, true, false));

            foreach(Language l in GoogleTranslator.LanguagesSupported)
            {
                langDict.Add(new SayclipLanguage(l.ISO639,l.FullName));

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

        public void setLanguages(SayclipLanguage fromLang, SayclipLanguage toLang)
        {
            if(fromLang.langCode== Language.Auto.ISO639)
            {
                this.fromLang = Language.Auto;
            }
            else
            {
                this.fromLang = GoogleTranslator.GetLanguageByISO(fromLang.langCode);
            }
          
            this.toLang = GoogleTranslator.GetLanguageByISO(toLang.langCode);
            Properties.Settings.Default.fromLang = this.fromLang.ISO639;
            Properties.Settings.Default.toLang = this.toLang.ISO639;
            Properties.Settings.Default.Save();

        }
        
        public SayclipLanguage[] getConfiguredLanguajes(string displayLanguaje)
        {
            SayclipLanguage[] langs = new SayclipLanguage[2];
            langs[0] = new SayclipLanguage(this.fromLang.ISO639,this.fromLang.FullName);
            langs[1] = new SayclipLanguage(this.toLang.ISO639, this.toLang.FullName);
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
