using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using sayclip;
using logSystem;
using NLog;
using Fergun.APIs.GTranslate;

namespace fergunGoogleTranslatorPlugin
{
    [Export(typeof(sayclip.iSayclipPluginTranslator))]
    public class Translator : iSayclipPluginTranslator
    {
        private const string name = "fergun google translator plugin";
        private const string description = "Plugin using the translator from google translate. Functionality taken from fergun discod bot: https://github.com/d4n3436/Fergun/";
        private string fromLang;
        private string toLang;
        private GTranslator googleTranslator;
        public async Task<IEnumerable<SayclipLanguage>> getAvailableLanguages(string displayLanguaje)
        {
            List<SayclipLanguage> languajes = new List<SayclipLanguage>();
            languajes.Add(new SayclipLanguage("auto", "auto"));
            foreach(KeyValuePair<string, string> kv in GTranslator.SupportedLanguages)
            {
                languajes.Add(new SayclipLanguage(kv.Key, kv.Value));
            }

            return (languajes);
        }

        public SayclipLanguage[] getConfiguredLanguajes(string displayLanguaje)
        {
            string fromLangName;
            if (this.fromLang == "auto")
            {
                fromLangName = "auto";
            }
            else
            {
                fromLangName = GTranslator.SupportedLanguages.Where(x => x.Key == this.fromLang).First().Value;
            }
            
            string toLangName = GTranslator.SupportedLanguages.Where(x => x.Key == this.toLang).First().Value;
            return (new SayclipLanguage[]
            {
                new SayclipLanguage(this.fromLang, fromLang),
                new SayclipLanguage(this.toLang, toLangName)
            });
        }

        public string getDescription(string languaje)
        {
            return (description);
        }

        public string getName()
        {
            return (name);
        }

        public bool haveConfigWindow()
        {
            return (false);
        }

        public bool initialize()
        {
            this.fromLang = !string.IsNullOrEmpty(Properties.Settings.Default.fromLang) ? Properties.Settings.Default.fromLang : "en";
            this.toLang = !String.IsNullOrEmpty(Properties.Settings.Default.toLang) ? Properties.Settings.Default.toLang : "es";
            googleTranslator = new GTranslator();
            return (true);
        }

        public void setLanguages(SayclipLanguage fromLang, SayclipLanguage toLang)
        {
            this.fromLang = fromLang.langCode;
            this.toLang = toLang.langCode;
            Properties.Settings.Default.fromLang = this.fromLang;
            Properties.Settings.Default.toLang = this.toLang;
            Properties.Settings.Default.Save();
        }

        public void showConfigWindow(string displayLanguaje)
        {
            throw new NotImplementedException();
        }

        public async Task<string> translate(string text)
        {
            TranslationResult translateResult;
            string result;
            try
            {
                LogWriter.getLog().Debug($"translating {text}");
                translateResult = await googleTranslator.TranslateAsync(text, this.toLang, this.fromLang).ConfigureAwait(false);
                result = translateResult.Translation;
                LogWriter.getLog().Debug($"translation result {result}");
            }
            catch (Exception er)
            {
                LogWriter.getLog().Error($"error in translation {er.Message} \n {er.StackTrace}");
                throw(er);
            }
            return (result);
        }
    }
}
