using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using sayclip;
using logSystem;
using NLog;
using Fergun.APIs.BingTranslator;

namespace fergunBingTranslatorPlugin
{
    [Export(typeof(sayclip.iSayclipPluginTranslator))]
    public class Translator : iSayclipPluginTranslator
    {
        private const string name = "fergun bing translator plugin";
        private const string description = "Plugin using the translator from bing searcher. Functionality taken from fergun discod bot: https://github.com/d4n3436/Fergun/";
        private string fromLang;
        private string toLang;

        public async Task<IEnumerable<SayclipLanguage>> getAvailableLanguages(string displayLanguaje)
        {
            List<SayclipLanguage> languajes = new List<SayclipLanguage>();
            languajes.Add(new SayclipLanguage("auto-detect", "auto"));
            foreach(KeyValuePair<string, string> kv in BingTranslatorApi.allSupportedLanguages)
            {
                languajes.Add(new SayclipLanguage(kv.Key, kv.Value));
            }

            return (languajes);
        }

        public SayclipLanguage[] getConfiguredLanguajes(string displayLanguaje)
        {
            string fromLangName;
            if (this.fromLang == "auto-detect")
            {
                fromLangName = "auto";
            }
            else
            {
                fromLangName = BingTranslatorApi.allSupportedLanguages.Where(x => x.Key == this.fromLang).First().Value;
            }
            
            string toLangName = BingTranslatorApi.allSupportedLanguages.Where(x => x.Key == this.toLang).First().Value;
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
            List<BingResult> translateResults;
            string result;
            try
            {
                LogWriter.getLog().Debug($"translating {text}");
                translateResults = await BingTranslatorApi.TranslateAsync(text, this.toLang, this.fromLang).ConfigureAwait(false);
                result = translateResults[0].Translations[0].Text;
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
