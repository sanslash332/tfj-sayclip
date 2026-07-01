using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using sayclip;
using GTranslate.Translators;
using GTranslate.Results;
using GTranslate;
using System.Reflection;

namespace gTranslateYandexTranslatorPlugin
{
    [Export(typeof(sayclip.iSayclipPluginTranslator))]
    public class Translator : iSayclipPluginTranslator
    {
        private const string name = "gTranslate yandex translator plugin";
        private const string description = "Plugin using the translator from yandex. Functionality provided by gTranslate lib https://github.com/d4n3436/GTranslate";
        private string fromLang;
        private string toLang;
        private SayclipLanguage fromLangSayclip;
        private SayclipLanguage toLangSayclip;
        private YandexTranslator yandexTranslator;
        private ISayclipPluginContext _context;

        public Task<IEnumerable<SayclipLanguage>> getAvailableLanguages(string displayLanguaje)
        {
            List<SayclipLanguage> languajes = new List<SayclipLanguage>();
            foreach(KeyValuePair<string, Language> kv in Language.LanguageDictionary)
            {
                if(kv.Value.IsServiceSupported(TranslationServices.Yandex))
                {
                    languajes.Add(new SayclipLanguage(kv.Key, kv.Value.Name));
                }
            }
            return Task.FromResult<IEnumerable<SayclipLanguage>>(languajes);
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

        public bool initialize(ISayclipPluginContext context = null)
        {
            _context = context;
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            this.yandexTranslator = new YandexTranslator();
            this.fromLang = !string.IsNullOrEmpty(Properties.Settings.Default.fromLang) ? Properties.Settings.Default.fromLang : "en";
            this.toLang = !String.IsNullOrEmpty(Properties.Settings.Default.toLang) ? Properties.Settings.Default.toLang : "es";
            IEnumerable<SayclipLanguage> languages = getAvailableLanguages("en").GetAwaiter().GetResult();
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
            YandexTranslationResult translateResults;
            string result;
            try
            {
                _context?.Logger?.Debug($"translating {text} \n from {this.fromLang} to {this.toLang}");
                translateResults = await yandexTranslator.TranslateAsync(text, this.toLang, this.fromLang).ConfigureAwait(false);
                _context?.Logger?.Debug($"Detected results of yandex translator {translateResults}");
                result = translateResults.Translation;
                _context?.Logger?.Debug($"translation result {result}");
            }
            catch (Exception er)
            {
                _context?.Logger?.Error($"error in translation {er.Message} \n {er.StackTrace}");
                this.yandexTranslator = new YandexTranslator();
                throw;
            }
            return (result);
        }
    }
}
