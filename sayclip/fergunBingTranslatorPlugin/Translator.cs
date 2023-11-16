using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using sayclip;
using logSystem;
using NLog;
using GTranslate.Translators;
using GTranslate.Results;
using GTranslate;
using System.Reflection;

namespace gTranslateBingTranslatorPlugin
{
    [Export(typeof(sayclip.iSayclipPluginTranslator))]
    public class Translator : iSayclipPluginTranslator
    {
        private const string name = "gTranslate bing translator plugin";
        private const string description = "Plugin using the translator from bing searcher. Functionality provided by gTranslate lib https://github.com/d4n3436/GTranslate";
        private string fromLang;
        private string toLang;
        private SayclipLanguage fromLangSayclip;
        private SayclipLanguage toLangSayclip;
        private BingTranslator bingTranslator;

        public async Task<IEnumerable<SayclipLanguage>> getAvailableLanguages(string displayLanguaje)
        {
            List<SayclipLanguage> languajes = new List<SayclipLanguage>();
            languajes.Add(new SayclipLanguage("auto-detect", "auto", true, false));
            foreach(KeyValuePair<string, Language> kv in Language.LanguageDictionary)
            {
                if(kv.Value.IsServiceSupported( TranslationServices.Bing))
                {
                    languajes.Add(new SayclipLanguage(kv.Key, kv.Value.Name));
                }
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
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
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

        private System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name);
            if (name.Name == "System.Runtime.CompilerServices.Unsafe")
            {
                return typeof(System.Runtime.CompilerServices.Unsafe).Assembly;
            }
            return null;


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
            
            BingTranslationResult translateResults;
            string result;
            try
            {
                LogWriter.getLog().Debug($"translating {text} \n from {this.fromLang} to {this.toLang}");
                translateResults = await bingTranslator.TranslateAsync(text, this.toLang, this.fromLang).ConfigureAwait(false);
                result = translateResults.Translation;
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
