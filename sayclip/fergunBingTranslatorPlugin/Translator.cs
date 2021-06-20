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
        private SayclipLanguage fromLangSayclip;
        private SayclipLanguage toLangSayclip;
        private BingTranslator bingTranslator;

        public async Task<IEnumerable<SayclipLanguage>> getAvailableLanguages(string displayLanguaje)
        {
            List<SayclipLanguage> languajes = new List<SayclipLanguage>();
            languajes.Add(new SayclipLanguage("auto-detect", "auto", true, false));
            foreach(KeyValuePair<string, string> kv in BingTranslator.allSupportedLanguages)
            {
                languajes.Add(new SayclipLanguage(kv.Key, kv.Value));
            }

            return (languajes);
        }

        public SayclipLanguage[] getConfiguredLanguajes(string displayLanguaje)
        {
            
            return (new SayclipLanguage[]
            {
                this.fromLangSayclip,
                this.toLangSayclip
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
            this.bingTranslator = new BingTranslator();
            this.fromLang = !string.IsNullOrEmpty(Properties.Settings.Default.fromLang) ? Properties.Settings.Default.fromLang : "en";
            this.toLang = !String.IsNullOrEmpty(Properties.Settings.Default.toLang) ? Properties.Settings.Default.toLang : "es";
            Task<IEnumerable<SayclipLanguage>> languagesTask = getAvailableLanguages("en");
            languagesTask.ConfigureAwait(false);
            IEnumerable<SayclipLanguage> languages = languagesTask.Result;
            this.fromLangSayclip = languages.Where(x => x.langCode == this.fromLang).FirstOrDefault();
            this.toLangSayclip = languages.Where(x => x.langCode == this.toLang).FirstOrDefault();
            return (true);
        }

        public void setLanguages(SayclipLanguage fromLang, SayclipLanguage toLang)
        {
            this.fromLang = fromLang.langCode;
            this.toLang = toLang.langCode;
            this.fromLangSayclip = fromLang;
            this.toLangSayclip = toLang;
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
            IReadOnlyList<BingResult> translateResults;
            string result;
            try
            {
                LogWriter.getLog().Debug($"translating {text}");
                translateResults = await bingTranslator.TranslateAsync(text, this.toLang, this.fromLang).ConfigureAwait(false);
                result = translateResults[0].Translations[0].Text;
                LogWriter.getLog().Debug($"translation result {result}");
            }
            catch (Exception er)
            {
                LogWriter.getLog().Error($"error in translation {er.Message} \n {er.StackTrace}");
                this.bingTranslator = new BingTranslator();
                throw(er);
            }
            return (result);
        }
    }
}
