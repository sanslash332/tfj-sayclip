using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using sayclip;
using logSystem;
using NLog;

namespace azureTranslatorPlugin
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        private string oldApiKey;
        public ConfigWindow()
        {
            InitializeComponent();
            oldApiKey = Properties.Settings.Default.TranslatorApiKey;
            translatorApiKeyTextbox.Text = Properties.Settings.Default.TranslatorApiKey != null ? Properties.Settings.Default.TranslatorApiKey : "";

        }

        private void acceptButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.TranslatorApiKey = translatorApiKeyTextbox.Text;
            Translator translator = new Translator();
            bool initialized;
            try
            {
                initialized = translator.initialize();
            }
            catch (Exception er)
            {
                LogWriter.getLog().Warn($"error initializing the plugin. {er.Message} \n {er.StackTrace}");
                initialized = false;
            }
            if(initialized)
            {
                Properties.Settings.Default.Save();
                ScreenReaderControl.speech($"Config OK. You can active azure translator plugin without problem.", true);
                this.Close();
            }
            else
            {
                ScreenReaderControl.speech($"problem validating your apiKey. Check it if is OK, and your internet connection. If the problem persist, check the logs.", true);
                
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;

        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.TranslatorApiKey = oldApiKey;
            this.Close();
        }
    }
}
