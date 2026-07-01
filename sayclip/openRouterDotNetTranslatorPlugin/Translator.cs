using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using OpenRouter.NET;
using OpenRouter.NET.Models;
using sayclip;

namespace openRouterDotNetTranslatorPlugin
{
    [Export(typeof(sayclip.iSayclipPluginTranslator))]
    public class Translator : iSayclipPluginTranslator
    {
        private const string pluginName = "OpenRouter translator plugin";
        private const string pluginDescription = "Plugin that uses OpenRouter.NET to translate text via any LLM model available on OpenRouter (https://openrouter.ai)";

        private string fromLang;
        private string toLang;
        private SayclipLanguage fromLangSayclip;
        private SayclipLanguage toLangSayclip;
        private ISayclipPluginContext _context;
        private OpenRouterClient _client;

        // Courtesy messages when apiKey is not configured, keyed by BCP-47/ISO language code.
        // Fallback to "en" if toLang is not in this dictionary.
        private static readonly Dictionary<string, string> NoApiKeyMessages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "en", "[OpenRouter plugin] No API key configured. Please open the plugin settings and enter your OpenRouter API key." },
            { "es", "[OpenRouter plugin] No hay API key configurada. Por favor abre la configuracion del plugin e introduce tu API key de OpenRouter." },
            { "fr", "[OpenRouter plugin] Aucune cle API configuree. Veuillez ouvrir les parametres du plugin et entrer votre cle API OpenRouter." },
            { "de", "[OpenRouter plugin] Kein API-Schlussel konfiguriert. Bitte oeffnen Sie die Plugin-Einstellungen und geben Sie Ihren OpenRouter-API-Schlussel ein." },
            { "pt", "[OpenRouter plugin] Nenhuma chave de API configurada. Abra as configuracoes do plugin e insira sua chave de API do OpenRouter." },
            { "it", "[OpenRouter plugin] Nessuna chiave API configurata. Aprire le impostazioni del plugin e inserire la chiave API di OpenRouter." },
            { "ja", "[OpenRouter plugin] APIキーが設定されていません。プラグインの設定を開き、OpenRouterのAPIキーを入力してください。" },
            { "zh", "[OpenRouter plugin] 未配置API密钥。请打开插件设置并输入您的OpenRouter API密钥。" },
            { "ru", "[OpenRouter plugin] Klyuch API ne nastroyen. Pozhaluysta, otkroyte nastroyki plagina i vvedite svoy klyuch API OpenRouter." },
        };

        // Static list of widely supported languages for getAvailableLanguages.
        // OpenRouter models generally support all major world languages.
        private static readonly List<SayclipLanguage> SupportedLanguages = new List<SayclipLanguage>
        {
            new SayclipLanguage("auto", "Auto-detect", true, false),
            new SayclipLanguage("af", "Afrikaans"),
            new SayclipLanguage("sq", "Albanian"),
            new SayclipLanguage("ar", "Arabic"),
            new SayclipLanguage("hy", "Armenian"),
            new SayclipLanguage("az", "Azerbaijani"),
            new SayclipLanguage("eu", "Basque"),
            new SayclipLanguage("be", "Belarusian"),
            new SayclipLanguage("bn", "Bengali"),
            new SayclipLanguage("bs", "Bosnian"),
            new SayclipLanguage("bg", "Bulgarian"),
            new SayclipLanguage("ca", "Catalan"),
            new SayclipLanguage("zh", "Chinese (Simplified)"),
            new SayclipLanguage("zh-TW", "Chinese (Traditional)"),
            new SayclipLanguage("hr", "Croatian"),
            new SayclipLanguage("cs", "Czech"),
            new SayclipLanguage("da", "Danish"),
            new SayclipLanguage("nl", "Dutch"),
            new SayclipLanguage("en", "English"),
            new SayclipLanguage("eo", "Esperanto"),
            new SayclipLanguage("et", "Estonian"),
            new SayclipLanguage("fi", "Finnish"),
            new SayclipLanguage("fr", "French"),
            new SayclipLanguage("gl", "Galician"),
            new SayclipLanguage("ka", "Georgian"),
            new SayclipLanguage("de", "German"),
            new SayclipLanguage("el", "Greek"),
            new SayclipLanguage("gu", "Gujarati"),
            new SayclipLanguage("ht", "Haitian Creole"),
            new SayclipLanguage("he", "Hebrew"),
            new SayclipLanguage("hi", "Hindi"),
            new SayclipLanguage("hu", "Hungarian"),
            new SayclipLanguage("is", "Icelandic"),
            new SayclipLanguage("id", "Indonesian"),
            new SayclipLanguage("ga", "Irish"),
            new SayclipLanguage("it", "Italian"),
            new SayclipLanguage("ja", "Japanese"),
            new SayclipLanguage("kn", "Kannada"),
            new SayclipLanguage("kk", "Kazakh"),
            new SayclipLanguage("ko", "Korean"),
            new SayclipLanguage("lv", "Latvian"),
            new SayclipLanguage("lt", "Lithuanian"),
            new SayclipLanguage("mk", "Macedonian"),
            new SayclipLanguage("ms", "Malay"),
            new SayclipLanguage("ml", "Malayalam"),
            new SayclipLanguage("mt", "Maltese"),
            new SayclipLanguage("mr", "Marathi"),
            new SayclipLanguage("mn", "Mongolian"),
            new SayclipLanguage("ne", "Nepali"),
            new SayclipLanguage("nb", "Norwegian"),
            new SayclipLanguage("fa", "Persian"),
            new SayclipLanguage("pl", "Polish"),
            new SayclipLanguage("pt", "Portuguese"),
            new SayclipLanguage("pa", "Punjabi"),
            new SayclipLanguage("ro", "Romanian"),
            new SayclipLanguage("ru", "Russian"),
            new SayclipLanguage("sr", "Serbian"),
            new SayclipLanguage("sk", "Slovak"),
            new SayclipLanguage("sl", "Slovenian"),
            new SayclipLanguage("es", "Spanish"),
            new SayclipLanguage("sw", "Swahili"),
            new SayclipLanguage("sv", "Swedish"),
            new SayclipLanguage("tl", "Tagalog"),
            new SayclipLanguage("ta", "Tamil"),
            new SayclipLanguage("te", "Telugu"),
            new SayclipLanguage("th", "Thai"),
            new SayclipLanguage("tr", "Turkish"),
            new SayclipLanguage("uk", "Ukrainian"),
            new SayclipLanguage("ur", "Urdu"),
            new SayclipLanguage("uz", "Uzbek"),
            new SayclipLanguage("vi", "Vietnamese"),
            new SayclipLanguage("cy", "Welsh"),
        };

        public string getName()
        {
            return pluginName;
        }

        public string getDescription(string languaje)
        {
            return pluginDescription;
        }

        public Task<IEnumerable<SayclipLanguage>> getAvailableLanguages(string displayLanguaje)
        {
            return Task.FromResult<IEnumerable<SayclipLanguage>>(SupportedLanguages);
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

        public SayclipLanguage[] getConfiguredLanguajes(string displayLanguaje)
        {
            return new SayclipLanguage[] { this.fromLangSayclip, this.toLangSayclip };
        }

        public bool initialize(ISayclipPluginContext context = null)
        {
            _context = context;

            fromLang = !string.IsNullOrEmpty(Properties.Settings.Default.fromLang)
                ? Properties.Settings.Default.fromLang : "auto";
            toLang = !string.IsNullOrEmpty(Properties.Settings.Default.toLang)
                ? Properties.Settings.Default.toLang : "es";

            fromLangSayclip = SupportedLanguages.FirstOrDefault(x => x.langCode == fromLang)
                ?? SupportedLanguages.FirstOrDefault(x => x.langCode == "auto");
            toLangSayclip = SupportedLanguages.FirstOrDefault(x => x.langCode == toLang)
                ?? SupportedLanguages.FirstOrDefault(x => x.langCode == "es");

            if (string.IsNullOrEmpty(Properties.Settings.Default.apiKey))
            {
                _context?.Logger?.Warn($"{pluginName}: no API key configured. Plugin loaded but translation will be unavailable until key is set.");
                // Return true so the plugin still loads and appears in the list.
                // The user can configure the key via showConfigWindow().
                return true;
            }

            try
            {
                _client = new OpenRouterClient(Properties.Settings.Default.apiKey);
                _context?.Logger?.Info($"{pluginName}: initialized with model '{Properties.Settings.Default.model}'");
                return true;
            }
            catch (Exception ex)
            {
                _context?.Logger?.Error($"{pluginName}: error creating OpenRouter client: {ex.Message}");
                return false;
            }
        }

        public bool haveConfigWindow()
        {
            return true;
        }

        public void showConfigWindow(string displayLanguaje)
        {
            // displayLanguaje is the culture string from the UI (e.g. "es", "en", "es-ES").
            // Normalize to base language code for resource lookup; fallback to "en".
            string lang = "en";
            if (!string.IsNullOrEmpty(displayLanguaje))
            {
                string baseLang = displayLanguaje.Split('-')[0].ToLowerInvariant();
                if (baseLang == "es")
                    lang = "es";
                // Future languages: add else-if branches here as the contract expands.
            }

            var window = new ConfigWindow(_context, lang);
            window.ShowDialog();

            // Reinitialize client if API key was changed in config window.
            if (!string.IsNullOrEmpty(Properties.Settings.Default.apiKey))
            {
                try
                {
                    _client = new OpenRouterClient(Properties.Settings.Default.apiKey);
                    _context?.Logger?.Info($"{pluginName}: client reinitialized after config window.");
                }
                catch (Exception ex)
                {
                    _context?.Logger?.Error($"{pluginName}: error reinitializing client after config: {ex.Message}");
                }
            }
        }

        public async Task<string> translate(string text)
        {
            if (string.IsNullOrEmpty(Properties.Settings.Default.apiKey))
            {
                string langKey = !string.IsNullOrEmpty(toLang) ? toLang : "en";
                // Try full code first, then base language (e.g. "pt-BR" -> "pt"), then fallback to "en".
                if (!NoApiKeyMessages.TryGetValue(langKey, out string noKeyMsg))
                {
                    string baseLang = langKey.Split('-')[0];
                    if (!NoApiKeyMessages.TryGetValue(baseLang, out noKeyMsg))
                        noKeyMsg = NoApiKeyMessages["en"];
                }
                _context?.Logger?.Warn($"{pluginName}: translate called without API key.");
                return noKeyMsg;
            }

            if (_client == null)
            {
                try
                {
                    _client = new OpenRouterClient(Properties.Settings.Default.apiKey);
                }
                catch (Exception ex)
                {
                    _context?.Logger?.Error($"{pluginName}: failed to create client on demand: {ex.Message}");
                    return text;
                }
            }

            string fromLangName = fromLangSayclip?.displayName ?? fromLang;
            string toLangName = toLangSayclip?.displayName ?? toLang;

            string userPrompt = fromLang == "auto"
                ? $"Translate the following text to {toLangName}:\n\n{text}"
                : $"Translate the following text from {fromLangName} to {toLangName}:\n\n{text}";

            try
            {
                var request = new ChatCompletionRequest
                {
                    Model = Properties.Settings.Default.model,
                    Messages = new System.Collections.Generic.List<Message>
                    {
                        Message.FromSystem(Properties.Settings.Default.systemPrompt),
                        Message.FromUser(userPrompt)
                    },
                    Temperature = (float)Properties.Settings.Default.temperature,
                    MaxTokens = Properties.Settings.Default.maxTokens
                };

                using var cts = new System.Threading.CancellationTokenSource(
                    TimeSpan.FromSeconds(Properties.Settings.Default.requestTimeoutSeconds));

                var response = await _client.CreateChatCompletionAsync(request);
                string result = response.Choices?[0]?.Message?.Content?.ToString() ?? string.Empty;
                _context?.Logger?.Debug($"{pluginName}: translation complete ({result.Length} chars)");
                return result;
            }
            catch (OperationCanceledException)
            {
                _context?.Logger?.Warn($"{pluginName}: request timed out after {Properties.Settings.Default.requestTimeoutSeconds}s");
                return text;
            }
            catch (Exception ex)
            {
                _context?.Logger?.Error($"{pluginName}: translation error: {ex.Message}");
                return text;
            }
        }
    }
}
