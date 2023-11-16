using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace sayclipTray
{
    /// <summary>
    /// Interaction logic for PluginsTab.xaml
    /// </summary>
    public partial class GeneralTab : TabItem
    {
        private bool detectChange = false;
        private App app = (App)Application.Current;
        private ConfigurationManager configuration = ConfigurationManager.getInstance;
        public ICommand pauseSayclipCommand = (ICommand)new DelegateCommand()
        {
            CanExecuteFunc = () => 
            {
                App app = (App)Application.Current;
                return (app.isSayclipRuning);
            },
            CommandAction = () =>
            {
                App app = (App)Application.Current;
                app.killSayclip();
                App.getNotifyIcon.reloadIconTitle();
            }
        };

        public ICommand resumeSayclipCommand = (ICommand)new DelegateCommand()
        {
            CanExecuteFunc = () =>
            {
                App app = (App)Application.Current;
                return (!app.isSayclipRuning);
            },
            CommandAction = () =>
            {
                App app = (App)Application.Current;
                app.startSayclip();
                App.getNotifyIcon.reloadIconTitle();
            }
        };

        public GeneralTab()
        {
            InitializeComponent();
            updateSetupKeyButton();
            resumeButton.Command = resumeSayclipCommand;
            pauseButton.Command = pauseSayclipCommand;

            
        }

        public void updateControlsConfigs()
        {
            updateSetupKeyButton();
            setupMonitorSpeedsCombobox();
            setupUiLangComboBox();

        }

        public void setupUiLangComboBox()
        {
            Dictionary<string, string> langs = ConfigValues.uiLangOptions;
            foreach (KeyValuePair<string, string> kv in langs)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = $"{kv.Value}";
                langComboBox.Items.Add(item);
            }
            string currentLang = Properties.Settings.Default.UILang;
            int position = langs.Keys.ToList().IndexOf(currentLang);

            langComboBox.Items.Refresh();
            langComboBox.SelectedIndex = position;

        }

        public void setupMonitorSpeedsCombobox()
        {
            Dictionary<int, string> speeds = ConfigValues.monitorSpeedsOptions;
            foreach(KeyValuePair<int, string> kv in speeds)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = $"{kv.Value} ({kv.Key} MS)";
                speedComboBox.Items.Add(item);
            }
            int currentSpeed = (int)configuration.clipboardPollingSpeed;
            int position = speeds.Keys.ToList().IndexOf(currentSpeed);

            speedComboBox.Items.Refresh();
            speedComboBox.SelectedIndex = position;
            
        }

        public void updateSetupKeyButton()
        {
            this.setupKeyButton.Content = $"{App.dictlang["ui.general.setupbutton"].ToString()}: { sayclipTray.Properties.Settings.Default.sayclipKey.ToString()}";
            this.setupKeyButton.UpdateLayout();
            this.detectChange = false;

        }

        private void setupKeyButton_Click(object sender, RoutedEventArgs e)
        {
            App.saveNextKey = true;
            sayclip.ScreenReaderControl.speech(App.dictlang["ui.general.setupmessaje"].ToString(), true);
            this.detectChange = true;

        }

        private void updateButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void speedComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int newindex = speedComboBox.SelectedIndex;
            int newspeed = ConfigValues.monitorSpeedsOptions.Keys.ToArray()[newindex];
            configuration.clipboardPollingSpeed = newspeed;
            if(App.getNotifyIcon != null)
            {
                App.getNotifyIcon.buildSpeedMenu();
            }

        }

        private void langComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int newindex = langComboBox.SelectedIndex;
            string newlang = ConfigValues.uiLangOptions.Keys.ToArray()[newindex];
            if (Properties.Settings.Default.UILang != newlang)
            {
                Properties.Settings.Default.UILang = newlang;
                Properties.Settings.Default.Save();
                sayclip.ScreenReaderControl.speech(App.dictlang["menu.ui.reset"].ToString(), true);

            }
            if (App.getNotifyIcon != null)
            {
                App.getNotifyIcon.buildUILangMenu();
            }

        }
    }
}
