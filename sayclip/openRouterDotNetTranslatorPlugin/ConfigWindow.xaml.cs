using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using OpenRouter.NET;
using OpenRouter.NET.Models;
using sayclip;

namespace openRouterDotNetTranslatorPlugin
{
    public partial class ConfigWindow : Window
    {
        private readonly ISayclipPluginContext _context;
        private readonly ResourceDictionary _strings;

        // Snapshots of settings at open time, used to restore on cancel.
        private readonly string _originalApiKey;
        private readonly string _originalModel;
        private readonly string _originalTemperature;
        private readonly string _originalMaxTokens;
        private readonly string _originalTimeout;
        private readonly string _originalSystemPrompt;

        public ConfigWindow(ISayclipPluginContext context = null, string lang = "en")
        {
            _context = context;

            // Load the appropriate ResourceDictionary before InitializeComponent so
            // bindings are available at construction time.
            _strings = LoadStrings(lang);

            InitializeComponent();

            ApplyStrings();

            // Snapshot current settings for cancel.
            _originalApiKey = Properties.Settings.Default.apiKey ?? string.Empty;
            _originalModel = Properties.Settings.Default.model ?? string.Empty;
            _originalTemperature = Properties.Settings.Default.temperature.ToString(System.Globalization.CultureInfo.InvariantCulture);
            _originalMaxTokens = Properties.Settings.Default.maxTokens.ToString();
            _originalTimeout = Properties.Settings.Default.requestTimeoutSeconds.ToString();
            _originalSystemPrompt = Properties.Settings.Default.systemPrompt ?? string.Empty;

            // Populate controls.
            // PasswordBox has no Text property in XAML binding, so set it here.
            apiKeyTextBox.Password = _originalApiKey;
            modelTextBox.Text = _originalModel;
            temperatureTextBox.Text = _originalTemperature;
            maxTokensTextBox.Text = _originalMaxTokens;
            timeoutTextBox.Text = _originalTimeout;
            systemPromptTextBox.Text = _originalSystemPrompt;
        }

        // Loads the ResourceDictionary for the requested language.
        // Falls back to English if the requested language resource is not available.
        private ResourceDictionary LoadStrings(string lang)
        {
            string[] candidates = lang == "es"
                ? new[] { "lang/es.xaml", "lang/en.xaml" }
                : new[] { "lang/en.xaml" };

            foreach (string path in candidates)
            {
                try
                {
                    var uri = new Uri($"pack://application:,,,/openRouterDotNetTranslatorPlugin.scplug;component/{path}");
                    var dict = new ResourceDictionary { Source = uri };
                    return dict;
                }
                catch
                {
                    // Try next candidate.
                }
            }

            // Last resort: return empty dictionary; controls will show empty strings.
            _context?.Logger?.Warn("openRouterDotNetTranslatorPlugin: could not load any language resource dictionary.");
            return new ResourceDictionary();
        }

        private string S(string key, string fallback = "")
        {
            return _strings.Contains(key) ? _strings[key] as string ?? fallback : fallback;
        }

        private void ApplyStrings()
        {
            Title = S("window.title", "OpenRouter Plugin - Configuration");
            sectionConnectionLabel.Text = S("section.connection", "Connection");
            apiKeyLabel.Content = S("label.apiKey", "API Key:");
            apiKeyHintLabel.Text = S("label.apiKey.hint");
            modelLabel.Content = S("label.model", "Model:");
            modelHintLabel.Text = S("label.model.hint");
            sectionBehaviorLabel.Text = S("section.behavior", "Behavior");
            temperatureLabel.Content = S("label.temperature", "Temperature (0.0 - 1.0):");
            temperatureHintLabel.Text = S("label.temperature.hint");
            maxTokensLabel.Content = S("label.maxTokens", "Max tokens:");
            maxTokensHintLabel.Text = S("label.maxTokens.hint");
            timeoutLabel.Content = S("label.timeout", "Timeout (seconds):");
            timeoutHintLabel.Text = S("label.timeout.hint");
            sectionPromptLabel.Text = S("section.prompt", "System Prompt");
            systemPromptLabel.Content = S("label.systemPrompt", "System prompt:");
            testConnectionButton.Content = S("button.testConnection", "Test connection");
            cancelButton.Content = S("button.cancel", "Cancel");
            saveButton.Content = S("button.save", "Save");
            getApiKeyLinkText.Text = S("link.getApiKey", "Get your API key at openrouter.ai");

            // NavigateUri on the hyperlink must be set from code-behind since it is not a
            // dependency property that accepts resource dictionary values directly.
            getApiKeyLink.NavigateUri = new Uri("https://openrouter.ai/keys");
        }

        private bool ValidateAndReadFields(out string apiKey, out string model,
            out double temperature, out int maxTokens, out int timeout, out string systemPrompt)
        {
            apiKey = apiKeyTextBox.Password.Trim();
            model = modelTextBox.Text.Trim();
            systemPrompt = systemPromptTextBox.Text;
            temperature = 0;
            maxTokens = 0;
            timeout = 0;

            if (!double.TryParse(temperatureTextBox.Text,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out temperature)
                || temperature < 0.0 || temperature > 1.0)
            {
                string msg = S("msg.invalidTemperature", "Temperature must be a number between 0.0 and 1.0.");
                _context?.Accessibility?.Speak(msg, true);
                MessageBox.Show(msg, Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                temperatureTextBox.Focus();
                return false;
            }

            if (!int.TryParse(maxTokensTextBox.Text, out maxTokens) || maxTokens <= 0)
            {
                string msg = S("msg.invalidMaxTokens", "Max tokens must be a positive integer.");
                _context?.Accessibility?.Speak(msg, true);
                MessageBox.Show(msg, Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                maxTokensTextBox.Focus();
                return false;
            }

            if (!int.TryParse(timeoutTextBox.Text, out timeout) || timeout <= 0)
            {
                string msg = S("msg.invalidTimeout", "Timeout must be a positive integer (seconds).");
                _context?.Accessibility?.Speak(msg, true);
                MessageBox.Show(msg, Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                timeoutTextBox.Focus();
                return false;
            }

            return true;
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateAndReadFields(out string apiKey, out string model,
                    out double temperature, out int maxTokens, out int timeout, out string systemPrompt))
                return;

            Properties.Settings.Default.apiKey = apiKey;
            Properties.Settings.Default.model = model;
            Properties.Settings.Default.temperature = temperature;
            Properties.Settings.Default.maxTokens = maxTokens;
            Properties.Settings.Default.requestTimeoutSeconds = timeout;
            Properties.Settings.Default.systemPrompt = systemPrompt;
            Properties.Settings.Default.Save();

            _context?.Logger?.Info("openRouterDotNetTranslatorPlugin: settings saved.");
            Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Restore snapshots in case the user had edited the in-memory settings through another path.
            // No-op here because we do not mutate Properties.Settings until Save is clicked.
            Close();
        }

        private async void testConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            string apiKey = apiKeyTextBox.Password.Trim();

            if (string.IsNullOrEmpty(apiKey))
            {
                string msg = S("msg.noApiKey", "Please enter an API key before testing.");
                _context?.Accessibility?.Speak(msg, true);
                MessageBox.Show(msg, Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            testConnectionButton.IsEnabled = false;
            try
            {
                var client = new OpenRouterClient(apiKey);
                var request = new ChatCompletionRequest
                {
                    Model = string.IsNullOrEmpty(modelTextBox.Text.Trim()) ? "openai/gpt-4o-mini" : modelTextBox.Text.Trim(),
                    Messages = new System.Collections.Generic.List<Message>
                    {
                        Message.FromSystem("You are a helpful assistant."),
                        Message.FromUser("Say only the word: ok")
                    },
                    MaxTokens = 10
                };

                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(15));
                var response = await client.CreateChatCompletionAsync(request);
                string content = response.Choices?[0]?.Message?.Content?.ToString() ?? string.Empty;

                string ok = S("msg.testOk", "Connection successful. The plugin is ready to use.");
                _context?.Accessibility?.Speak(ok, true);
                MessageBox.Show(ok, Title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _context?.Logger?.Error($"openRouterDotNetTranslatorPlugin test connection failed: {ex.Message}");
                string fail = S("msg.testFail", "Connection failed. Check your API key and internet connection.");
                _context?.Accessibility?.Speak(fail, true);
                MessageBox.Show(fail, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                testConnectionButton.IsEnabled = true;
            }
        }

        private void getApiKeyLink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                _context?.Logger?.Warn($"openRouterDotNetTranslatorPlugin: could not open browser: {ex.Message}");
            }
            e.Handled = true;
        }
    }
}
