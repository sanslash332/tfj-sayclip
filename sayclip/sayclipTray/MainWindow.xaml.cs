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
using System.Diagnostics;

namespace sayclipTray
{
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool detectChange = false;
        private string msappidbackup;
        private string msappsecretbackup;

        public MainWindow()
        {

            InitializeComponent();
            this.Closing += MainWindow_Closing;
            this.msappidbackup = sayclipTray.Properties.Settings.Default.msAppID;
            this.msappsecretbackup = sayclipTray.Properties.Settings.Default.msAppSecret;
            //this.setupKeyButton.Content += sayclipTray.Properties.Settings.Default.sayclipKey.ToString();
            updateButton();
            this.KeyDown += MainWindow_KeyDown;
            
        }

        public void updateButton()
        {
            this.setupKeyButton.Content = "setup sayclip key: " + sayclipTray.Properties.Settings.Default.sayclipKey.ToString();
            this.setupKeyButton.UpdateLayout();
            this.detectChange = false;
            
        }
        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key== Key.Escape)
            {
                this.Hide();
                return;
            }
            
            
                
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel=true;
            this.Hide();

        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void applyButton_Click(object sender, RoutedEventArgs e)
        {
            ScreenReaderControl.speech("checking credentials", true);
            sayclipTray.Properties.Settings.Default.msAppID = this.msappidbox.Text;
            sayclipTray.Properties.Settings.Default.msAppSecret = this.msappsecretbox.Text;
            sayclip.Properties.Settings.Default.msAppSecret = sayclipTray.Properties.Settings.Default.msAppSecret;
            sayclip.Properties.Settings.Default.msAppID = sayclipTray.Properties.Settings.Default.msAppID;
            if(Sayclip.checkMSCredentials())
            {
                ScreenReaderControl.speech("Credentials are OK, api key is valid", true);
                sayclipTray.Properties.Settings.Default.Save();
                App.resetSayclip();
                this.Hide();
            }
            else
            {
                Sayclip.sayAndCopy("error validating your microsoft api credentials. please see the log and verify your internet conection, and your microsoft translator api credentials");
                this.discardButton_Click(sender,e);
                this.Show();

            }
            
            

            
        }

        private void discardButton_Click(object sender, RoutedEventArgs e)
        {
            sayclipTray.Properties.Settings.Default.msAppSecret = this.msappsecretbackup;
            sayclipTray.Properties.Settings.Default.msAppID = this.msappidbackup;
            sayclipTray.Properties.Settings.Default.Save();
            App.resetSayclip();
            this.Hide();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;

        }

        private void setupKeyButton_Click(object sender, RoutedEventArgs e)
        {
            App.saveNextKey = true;
            sayclip.ScreenReaderControl.speech("Please press the key that you want for sayclip key", true);
            this.detectChange = true;
            

        }
    }
}
