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
        public PluginsTab getPluginsTab;
        
        public MainWindow()
        {

            InitializeComponent();
            getPluginsTab = pluginsTab;
            
          
            this.Closing += MainWindow_Closing;
            //this.setupKeyButton.Content += sayclipTray.Properties.Settings.Default.sayclipKey.ToString();
            
            this.KeyDown += MainWindow_KeyDown;
          
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
            this.Hide();
          
        }

        private void discardButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        
    }
}
