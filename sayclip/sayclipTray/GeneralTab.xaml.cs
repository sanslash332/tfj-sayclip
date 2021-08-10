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
    }
}
