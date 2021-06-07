using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using logSystem;
using NLog;

namespace Fergun.APIs.BingTranslator
{
    public static class BingTranslatorApi
    {
        public const string BingHost = "https://www.bing.com";

        private static readonly HttpClient _httpClient = new HttpClient();

        public static IReadOnlyDictionary<string, string> allSupportedLanguages { get; } = new Dictionary<string, string>
        {
            { "af", "Afrikaans" },
            { "sq", "Albanian" },
            { "am", "Amharic" },
            { "ar", "Arabic" },
            { "hy", "Armenian" },
            { "az", "Azerbaijani" },
            { "eu", "Basque" },
            { "be", "Belarusian" },
            { "bn", "Bengali" },
            { "bs", "Bosnian" },
            { "bg", "Bulgarian" },
            { "ca", "Catalan" },
            { "ceb", "Cebuano" },
            { "ny", "Chichewa" },
            { "zh-CN", "Chinese Simplified" },
            { "zh-TW", "Chinese Traditional" },
            { "co", "Corsican" },
            { "hr", "Croatian" },
            { "cs", "Czech" },
            { "da", "Danish" },
            { "nl", "Dutch" },
            { "en", "English" },
            { "eo", "Esperanto" },
            { "et", "Estonian" },
            { "tl", "Filipino" },
            { "fi", "Finnish" },
            { "fr", "French" },
            { "fy", "Frisian" },
            { "gl", "Galician" },
            { "ka", "Georgian" },
            { "de", "German" },
            { "el", "Greek" },
            { "gu", "Gujarati" },
            { "ht", "Haitian Creole" },
            { "ha", "Hausa" },
            { "haw", "Hawaiian" },
            { "iw", "Hebrew" },
            { "hi", "Hindi" },
            { "hmn", "Hmong" },
            { "hu", "Hungarian" },
            { "is", "Icelandic" },
            { "ig", "Igbo" },
            { "id", "Indonesian" },
            { "ga", "Irish" },
            { "it", "Italian" },
            { "ja", "Japanese" },
            { "jw", "Javanese" },
            { "kn", "Kannada" },
            { "kk", "Kazakh" },
            { "km", "Khmer" },
            { "ko", "Korean" },
            { "ku", "Kurdish (Kurmanji)" },
            { "ky", "Kyrgyz" },
            { "lo", "Lao" },
            { "la", "Latin" },
            { "lv", "Latvian" },
            { "lt", "Lithuanian" },
            { "lb", "Luxembourgish" },
            { "mk", "Macedonian" },
            { "mg", "Malagasy" },
            { "ms", "Malay" },
            { "ml", "Malayalam" },
            { "mt", "Maltese" },
            { "mi", "Maori" },
            { "mr", "Marathi" },
            { "mn", "Mongolian" },
            { "my", "Myanmar (Burmese)" },
            { "ne", "Nepali" },
            { "no", "Norwegian" },
            { "ps", "Pashto" },
            { "fa", "Persian" },
            { "pl", "Polish" },
            { "pt", "Portuguese" },
            { "ma", "Punjabi" },
            { "ro", "Romanian" },
            { "ru", "Russian" },
            { "sm", "Samoan" },
            { "gd", "Scots Gaelic" },
            { "sr", "Serbian" },
            { "st", "Sesotho" },
            { "sn", "Shona" },
            { "sd", "Sindhi" },
            { "si", "Sinhala" },
            { "sk", "Slovak" },
            { "sl", "Slovenian" },
            { "so", "Somali" },
            { "es", "Spanish" },
            { "su", "Sundanese" },
            { "sw", "Swahili" },
            { "sv", "Swedish" },
            { "tg", "Tajik" },
            { "ta", "Tamil" },
            { "te", "Telugu" },
            { "th", "Thai" },
            { "tr", "Turkish" },
            { "uk", "Ukrainian" },
            { "ur", "Urdu" },
            { "uz", "Uzbek" },
            { "vi", "Vietnamese" },
            { "cy", "Welsh" },
            { "xh", "Xhosa" },
            { "yi", "Yiddish" },
            { "yo", "Yoruba" },
            { "zu", "Zulu" }
        };

        static BingTranslatorApi()
        {
            _httpClient.DefaultRequestHeaders.Referrer = new Uri($"{BingHost}/ttranslatev3");
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.138 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Connection.ParseAdd("close");
            _httpClient.DefaultRequestHeaders.ConnectionClose = true;
        }

        public static async Task<List<BingResult>> TranslateAsync(string text, string toLanguage, string fromLanguage = "auto-detect")
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentNullException(nameof(text));
            }
            if (SupportedLanguages.All(x => x != toLanguage))
            {
                throw new ArgumentException("Invalid target language.", nameof(toLanguage));
            }
            if (fromLanguage != "auto-detect" && SupportedLanguages.All(x => x != fromLanguage))
            {
                throw new ArgumentException("Invalid source language.", nameof(fromLanguage));
            }

            // Convert Google Translate language codes to Bing Translator equivalent.
            toLanguage = toLanguage switch
            {
                "no" => "nb",
                "pt" => "pt-pt",
                "zh-CN" => "zh-Hans",
                "zh-TW" => "zh-Hant",
                _ => toLanguage
            };

            var data = new Dictionary<string, string>
            {
                { "fromLang", fromLanguage },
                { "text", text },
                { "to", toLanguage }
            };

            string json;
            using (var content = new FormUrlEncodedContent(data))
            {
                var response = await _httpClient.PostAsync(new Uri($"{BingHost}/ttranslatev3"), content);
                response.EnsureSuccessStatusCode();
                json = await response.Content.ReadAsStringAsync();
            }
            LogWriter.getLog().Debug($"the result of the bing api call is: \n {json}");
            return JsonConvert.DeserializeObject<List<BingResult>>(json);
        }

        public static IReadOnlyList<string> SupportedLanguages { get; } = new[]
        {
            "af",
            "ar",
            "bn",
            "bs",
            "bg",
            "ca",
            "zh-CN",
            "zh-TW",
            "hr",
            "cs",
            "da",
            "nl",
            "no",
            "en",
            "et",
            "fi",
            "fr",
            "de",
            "el",
            "gu",
            "ht",
            "hi",
            "hu",
            "is",
            "id",
            "ga",
            "it",
            "ja",
            "kn",
            "kk",
            "ko",
            "lv",
            "lt",
            "mg",
            "ml",
            "mt",
            "mi",
            "mr",
            "fa",
            "pl",
            "pt",
            "ro",
            "ru",
            "sm",
            "sk",
            "sl",
            "es",
            "sw",
            "sv",
            "ta",
            "te",
            "th",
            "tr",
            "uk",
            "ur",
            "vi",
            "cy"
        };
    }
}