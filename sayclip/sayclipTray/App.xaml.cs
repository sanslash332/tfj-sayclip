using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using sayclip;
using System.Threading;
using System.Windows.Controls;

namespace sayclipTray
{
    /// <summary>
    /// Simple application. Check the XAML for comments.
    /// </summary>
    public partial class App : Application
    {
        private TaskbarIcon notifyIcon;
        private static Thread sayclipThread;

        public static bool isSayclipRuning()
        {
            return (sayclipThread.IsAlive);
        }

        public static void resetSayclip()
        {
            killSayclip();
            startSayclip();

        }
       public static  void startSayclip()
        {
            sayclip.Properties.Settings.Default.lan1 = sayclipTray.Properties.Settings.Default.lan1;
            sayclip.Properties.Settings.Default.lan2 = sayclipTray.Properties.Settings.Default.lan2;
            sayclip.Properties.Settings.Default.translate = sayclipTray.Properties.Settings.Default.translate;
            sayclip.Properties.Settings.Default.copyresult = sayclipTray.Properties.Settings.Default.copyresult;
            sayclip.Properties.Settings.Default.active = sayclipTray.Properties.Settings.Default.active;


            sayclipThread = new Thread(Sayclip.Main);
            sayclipThread.SetApartmentState(ApartmentState.STA);
            sayclipThread.Start();

        }

        public static void killSayclip()
        {
            try
            {
                sayclipThread.Abort();
            }
            catch (System.Exception)
            {

                
            }


            
         
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //create the notifyicon (it's a resource declared in NotifyIconResources.xaml
            notifyIcon = (TaskbarIcon) FindResource("NotifyIcon");
            MenuItem randomit = new MenuItem();
            randomit.Header = "random menu";
            notifyIcon.ContextMenu.Items.Add(randomit);
            startSayclip();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon.Dispose(); //the icon would clean up automatically, but this is cleaner
            killSayclip();
            
            base.OnExit(e);
        }
    }
}
