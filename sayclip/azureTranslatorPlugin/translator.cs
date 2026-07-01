using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sayclip;
using System.ComponentModel.Composition;
using TranslatorService;
using TranslatorService.Models.Translation;

namespace azureTranslatorPlugin
{
    [Export(typeof(sayclip.iSayclipPluginTranslator))]
    public class Translator : iSayclipPluginTranslator
    {
        private const string pluginName = "azure translator plugin";
        private const string description = "Plugin for use the microsoft translator from azure cognitive services";
        private string fromLang;
        private string toLang;
        private SayclipLanguage fromLangSayclip;
        private SayclipLanguage toLangSayclip;
        private TranslatorClient client;
        private List<ServiceLanguage> availableLanguages;
        private ISayclipPluginContext _context;

        public async Task<IEnumerable<SayclipLanguage>> getAvailableLanguages(string displayLanguaje)
        {
            List<SayclipLanguage> sayclipLanguajes = new List<SayclipLanguage>();
            try
            {
                availableLanguages = (List<ServiceLanguage>)await this.client.GetLanguagesAsync(displayLanguaje).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _context?.Logger?.Error($"canot get available azure languages: {e.Message} \n {e.StackTrace}");
                sayclipLanguajes.Add(new SayclipLanguage("empty", "empty"));
                return (sayclipLanguajes);
            }
            
            sayclipLanguajes.Add(new SayclipLanguage("auto", "auto", true, false));
            foreach(ServiceLanguage lang in availableLanguages)
            {
                sayclipLanguajes.Add(new SayclipLanguage(lang.Code, lang.Name));
            }
            return (sayclipLanguajes);
        }

        public SayclipLanguage[] getConfiguredLanguajes(string displayLanguaje)
        {
            
            return (new SayclipLanguage[2] { this.fromLangSayclip, this.toLangSayclip});
        }

        public string getDescription(string languaje)
        {
            return (description);
        }

        public string getName()
        {
            return (pluginName);
        }

        public bool haveConfigWindow()
        {
            return (true);
        }

        public bool initialize(ISayclipPluginContext context = null)
        {
            _context = context;
            if(Properties.Settings.Default.TranslatorApiKey == null || Properties.Settings.Default.TranslatorApiKey == "")
            {
                _context?.Logger?.Warn($"Translator apiKey not configured. the plugin cant be started");
                return (false);
            }
            this.client = new TranslatorClient(Properties.Settings.Default.TranslatorApiKey.ToString());
            Task initTask = this.client.InitializeAsync();
            initTask.ConfigureAwait(false);
            initTask.Wait();
            _context?.Logger?.Debug($"preloading languajes");
            Task<IEnumerable<SayclipLanguage>> langTask = getAvailableLanguages("en");
            langTask.ConfigureAwait(false);
            IEnumerable<SayclipLanguage> result = langTask.Result;
            int langCount = result.Count();
            _context?.Logger?.Debug($"languajes loaded {langCount}");
            if(langCount <= 1)
            {
                return (false);
            }
            this.fromLang = Properties.Settings.Default.fromLang != null ? Properties.Settings.Default.fromLang : "en";
            this.fromLangSayclip = result.Where(x => x.langCode == this.fromLang).FirstOrDefault();
            this.toLang = Properties.Settings.Default.toLang != null ? Properties.Settings.Default.toLang : "es";
            this.toLangSayclip = result.Where(x => x.langCode == this.toLang).FirstOrDefault();
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
            ConfigWindow window = new ConfigWindow(_context);
            window.ShowDialog();
        }

        public async Task<string> translate(string text)
        {
            TranslationResponse response;
            try
            {
                if (this.fromLang == "auto")
                {
                    response = await this.client.TranslateAsync(text, this.toLang);
                }
                else
                {
                    response = await this.client.TranslateAsync(text, this.fromLang, this.toLang);
                }

            }
            catch (Exception e)
            {
                _context?.Logger?.Debug($"error in translation. {e.Message}");
                throw(e);
            }
            return (response.Translation.Text);

        }
    }
}
