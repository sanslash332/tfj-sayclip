﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using sayclip;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using logSystem;
using NLog;
using Gma.System.MouseKeyHook;
using System.Windows.Media;
using AutoUpdaterDotNET;

namespace sayclipTray
{
    /// <summary>
    /// Simple application. Check the XAML for comments.
    /// </summary>
    public partial class App : Application
    {
        private MainWindow win;
        public static bool saveNextKey;
        public bool closeWindowAfterCloseSystrayMenu;
        private IKeyboardMouseEvents kmEvents;
        //private System.Windows.Forms.NotifyIcon nicon;
        private static NotifyIconViewModel notifyIcon;
        public static NotifyIconViewModel getNotifyIcon
        {
            get
            {
                return (notifyIcon);
            }
        }
        private Task sayclipTask;
        private CancellationTokenSource tokenSource;
        private Sayclip scp;
        private static Mutex singleInstanceMutex = new Mutex(false, "sayclipSingleInstance");
        private bool singleInstanceForcedExit = false;
        public static ResourceDictionary dictlang;
        public static String uiLang;

        public bool isSayclipRuning
        {
            get
            {
                if(scp==null)
                {
                    return (false);

                }
                return (true);
            }
        }

        public void resetSayclip()
        {
            killSayclip();
            startSayclip();
            if(win.IsVisible)
            {
                win.getPluginsTab.buildPluginsListbox();
            }
        }

        public void startSayclip()
        {
            Sayclip.dictlang = dictlang;
            this.tokenSource = new CancellationTokenSource();
            CancellationToken token = this.tokenSource.Token;
            scp = new Sayclip();
            sayclipTask = scp.Main(token);

        }

        public void killSayclip()
        {
            try
            {
                tokenSource.Cancel(true);
                scp.shutDownCore();
                scp = null;
                //sayclipTask.Wait(new TimeSpan(0,0,5));

            }
            catch (System.Exception e)
            {
                LogWriter.getLog().Error($"problems shuting down the sayclip core. \n {e.Message} \n {e.StackTrace} ");

            }
        }

        protected virtual void onKeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
          
            //LogWriter.escribir("tecla detectada: " + e.KeyCode.ToString() + "\n con el código: " +e.KeyValue.ToString());
            //ScreenReaderControl.speech("tecla detectada: " + e.KeyCode.ToString(), true);
            if(saveNextKey)
            {
                sayclipTray.Properties.Settings.Default.sayclipKey = e.KeyCode;
                sayclipTray.Properties.Settings.Default.Save();
                Application.Current.MainWindow.Close();
                win = new MainWindow();
                Application.Current.MainWindow = win;
                Application.Current.MainWindow.Show();
                saveNextKey = false;

                notifyIcon.reloadIconTitle();

                return;

            }
            if(e.KeyCode == sayclipTray.Properties.Settings.Default.sayclipKey && e.Control== true && e.Shift==true)
            {
                ScreenReaderControl.speech(dictlang["menu.open"].ToString(),true);
                notifyIcon.Focus();
                //win = new MainWindow();
                win.Show();

                ContextMenu systraymenu = notifyIcon.ContextMenu;
                systraymenu.IsOpen = true;
                systraymenu.Visibility = Visibility.Visible;
                systraymenu.IsOpen = true;
                systraymenu.Focus();
                //win.Hide();
              
            }
            else if (e.Alt == true && e.Control == true && e.KeyCode == sayclipTray.Properties.Settings.Default.sayclipKey)
            {
                if (!Application.Current.MainWindow.IsVisible)
                {
                    sayclip.ScreenReaderControl.speech(dictlang["ui.open"].ToString(), true);
                    //win = new MainWindow();
                    Application.Current.MainWindow = win; ;

                    Application.Current.MainWindow.Show();

                }
                else
                {
                    sayclip.ScreenReaderControl.speech(dictlang["ui.opened"].ToString(), true);

                }
            }
            else if (e.Shift && e.Alt && e.KeyCode == sayclipTray.Properties.Settings.Default.sayclipKey)
            {
                if(isSayclipRuning)
                {
                    killSayclip();
                    ScreenReaderControl.speech(dictlang["internal.stop"].ToString(), true);

                }
                else
                {
                    startSayclip();
                }
                notifyIcon.reloadIconTitle();
            }
            else if(e.Control== true && e.KeyCode== sayclipTray.Properties.Settings.Default.sayclipKey)
            {
                if (scp != null)
                {
                    scp.repeatLastClipboardContent();
                }
            }
        }

        public void loadLanguajeUI()
        {

            ResourceDictionary dictLang;
            switch (sayclipTray.Properties.Settings.Default.UILang)
            {
                case "en":
                    dictLang = new ResourceDictionary() { Source = new Uri("lang\\en.xaml", UriKind.Relative) };
                    uiLang = "en";
                    break;
                case "es":
                    dictLang = new ResourceDictionary() { Source = new Uri("lang\\es.xaml", UriKind.Relative) };
                    uiLang = "es";
                    break;
                case "auto":
                    string cult = Thread.CurrentThread.CurrentCulture.ToString();
                    if (cult.StartsWith("es"))
                    {
                        dictLang = new ResourceDictionary() { Source = new Uri("lang\\es.xaml", UriKind.Relative) };
                        uiLang = "es";
                    }
                    else
                    {
                        dictLang = new ResourceDictionary() { Source = new Uri("lang\\en.xaml", UriKind.Relative) };
                        uiLang = "en";
                    }

                    break;
                default:
                    dictLang = new ResourceDictionary() { Source = new Uri("lang\\en.xaml", UriKind.Relative) };
                    uiLang = "en";
                    break;
            }

            this.Resources.MergedDictionaries.Add(dictLang);
            dictlang = dictLang;
            Sayclip.dictlang = dictlang;



        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            checkSettingsUpgrade();
            loadLanguajeUI();
            if (!checkSingleInstance())
            {
                return;
            }
            ScreenReaderControl.speech(dictlang["update.check"].ToString(), true);
            AutoUpdater.Start("https://github.com/sanslash332/tfj-sayclip/releases/latest/download/version.xml");

            PluginManager  pm = PluginManager.getInstanse;
            win = new sayclipTray.MainWindow();
            Application.Current.MainWindow = win;
            //create the notifyicon (it's a resource declared in NotifyIconResources.xaml
            LogWriter.getLog().Info("starting sayclip");
            closeWindowAfterCloseSystrayMenu = true;
            kmEvents = Hook.GlobalEvents();
            kmEvents.KeyUp+= new System.Windows.Forms.KeyEventHandler(onKeyUp);
            kmEvents.KeyPress += KmEvents_KeyPress;
            
            //notifyIcon= (TaskbarIcon) FindResource("NotifyIcon");

            NotifyIconViewModel tb = new NotifyIconViewModel();
            notifyIcon = tb;
//this.Resources.Add("taskbar", tb);

            if(notifyIcon==null||tb==null)
            {
                LogWriter.getLog().Error($"Problema, uno de los componentes no se ha inicializado correctamente ");

                this.Shutdown();
                System.Environment.Exit(1);

            }

            //tb.TBIcon = notifyIcon;
            ContextMenu systraymenu = tb.ContextMenu;
                systraymenu.Closed += Systraymenu_Closed;
          
            //notifyIcon.KeyDown += NotifyIcon_KeyDown;
            //notifyIcon.TrayMouseDoubleClick += NotifyIcon_TrayMouseDoubleClick;
            notifyIcon.MenuActivation = PopupActivationMode.LeftOrRightClick;
            tb.buidlLanguajeMenuHeaders();
            tb.buildLanguajeMenuItems();
            tb.buildPluginsMenu();
            tb.buildSpeedMenu();
            tb.buildUILangMenu();
            tokenSource = new CancellationTokenSource();

            startSayclip();
            tb.reloadIconTitle();
            win.Show();
            win.Activate();
            win.Focus();
            if(sayclipTray.Properties.Settings.Default.startsMinimised)
            {
                win.Hide();
            }
        }

        private bool checkSingleInstance()
        {
            bool continueExcecution = true;
            if(sayclipTray.Properties.Settings.Default.useSingleInstance)
            {
                try
                {
                    if (!singleInstanceMutex.WaitOne(0, false))
                    {
                        MessageBox.Show(dictlang["ui.singleInstance"].ToString(), "Sayclip alert", MessageBoxButton.OK);
                        LogWriter.getLog().Info($"detected a previous runing sayclip. closing this instanse");
                        continueExcecution = false;
                        singleInstanceForcedExit = true;
                        Application.Current.Shutdown();
                    }
                }
                catch (Exception e)
                {
                    LogWriter.getLog().Warn($"catched an orfaned instanse {e.Message} \n maybe a previous sayclip was unexpectelly cloced");

                }
                
            }
            return (continueExcecution);
        }

        private void checkSettingsUpgrade()
        {
            if(sayclipTray.Properties.Settings.Default.needSettingsUpgrade)
            {
                sayclipTray.Properties.Settings.Default.Upgrade();
                sayclipTray.Properties.Settings.Default.needSettingsUpgrade = false;
                sayclipTray.Properties.Settings.Default.Save();
            }
        }

        private void Systraymenu_Closed(object sender, RoutedEventArgs e)
        {
            if(win!= null && closeWindowAfterCloseSystrayMenu)
            {
                LogWriter.getLog().Debug($"closing window on systray close event");
                win.Hide();

            }
            closeWindowAfterCloseSystrayMenu = true;
        }

        private void KmEvents_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {/*
            if(e.KeyChar== 'a')
            {
                //ScreenReaderControl.speech("a detectada", true);

            }
            */
        }

        private void NotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            notifyIcon.ContextMenu.IsOpen = true;

        }

        private void NotifyIcon_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key== Key.Enter)
            {
                notifyIcon.ContextMenu.IsOpen = true;

            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if(notifyIcon != null)
            {
                notifyIcon.Dispose(); //the icon would clean up automatically, but this is cleaner
            }
            
            LogWriter.getLog().Info("closing sayclip");
            if(!singleInstanceForcedExit)
            {
                singleInstanceMutex.ReleaseMutex();
                singleInstanceMutex.Dispose();
                killSayclip();

                kmEvents.KeyUp -= onKeyUp;
                kmEvents.Dispose();

            }
            base.OnExit(e);
        }

        /// <summary>
        /// Finds a Child of a given item in the visual tree. 
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, 
        /// a null parent is being returned.</returns>
        public static T FindChild<T>(DependencyObject parent, string childName)
           where T : DependencyObject
        {
            // Confirm parent and childName are valid.  
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child 
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree 
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child.  
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search 
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name 
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found. 
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }


    }
}
