using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Collections.Generic;
using Hardcodet.Wpf.TaskbarNotification;
using sayclip;
using logSystem;
using NLog;

namespace sayclipTray
{
    /// <summary>
    /// Provides bindable properties and commands for the NotifyIcon. In this sample, the
    /// view model is assigned to the NotifyIcon in XAML. Alternatively, the startup routing
    /// in App.xaml.cs could have created this view model, and assigned it to the NotifyIcon.
    /// </summary>
    public partial class NotifyIconViewModel : TaskbarIcon
    {
        private ConfigurationManager scpcm = ConfigurationManager.getInstance;
        private PluginManager scppm = PluginManager.getInstanse;
        private App app = (App)Application.Current;
        public static string sourceLanheader;
        public static string targetLanHeader;
      
        public NotifyIconViewModel()
        {
            this.InitializeComponent();

            this.ContextMenu = this.systraycontextMenu;
            if(this.ContextMenu==null)
            {
                LogWriter.getLog().Warn("la wea no se creo correctamente");

                this.ContextMenu = new ContextMenu();
            }

            
        }
        
        
        public ICommand enablecopyresult
        {
            get
            {
                return new DelegateCommand()
                {
                    CanExecuteFunc= () => scpcm.translating && !scpcm.copyResultToClipboard,
                    CommandAction = () =>
                        {
                            scpcm.copyResultToClipboard = true;
                            
                        }
                        
                };
            }
        }
        
        public ICommand disablecopyresult
        {
            get
            {
                return new DelegateCommand()
                {
                    CanExecuteFunc= () => scpcm.translating&& scpcm.copyResultToClipboard,
                    CommandAction = () =>
                        {
                            scpcm.copyResultToClipboard = false;
                            
                        }
                };
            }
        }

        public ICommand PauseSayclipCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => app.isSayclipRuning,
                    CommandAction = () =>
                        {
                            app.killSayclip();
                            reloadIconTitle();
                        }
                };
            }
        }

        public ICommand ResumeSayclipCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => !app.isSayclipRuning,
                    CommandAction = () =>
                        {
                            app.startSayclip();
                            reloadIconTitle();
                        }
                };
            }
        }

        public ICommand disablingRepeatingText
        {
            get
            {
                return new DelegateCommand()
                {
                    CanExecuteFunc = () => scpcm.allowCopyRepeatedText,
                    CommandAction = () =>
                    {
                        
                        scpcm.allowCopyRepeatedText= !  scpcm.allowCopyRepeatedText;
                        reloadIconTitle();
                    }

                };
            }
        }

        public ICommand enableRepeatingText
        {
            get
            {
                return new DelegateCommand()
                {
                    CanExecuteFunc = () => !scpcm.allowCopyRepeatedText,
                    CommandAction = () =>
                        {
                        scpcm.allowCopyRepeatedText= !scpcm.allowCopyRepeatedText;
                      

                        }

                };
            }
        }

        public ICommand EnableTranslationCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc= ()=> !scpcm.translating,
                    CommandAction= () =>
                    {
                        app.killSayclip();
                        scpcm.translating= true;

                        app.startSayclip();
                        reloadIconTitle();

                    }
                };
            }
        }

        public ICommand DisableTranslationCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc= ()=> scpcm.translating,
                    CommandAction= ()=>
                    {
                        app.killSayclip();
                        scpcm.translating= false;

                        app.startSayclip();
                        reloadIconTitle();

                    }
                };
            }
        }
        public ICommand ShowWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => true,
                    CommandAction = () =>
                    {
                        //TaskbarIcon sysTray = (TaskbarIcon)Application.Current.FindResource("NotifyIcon");
                        //sysTray.ContextMenu.IsOpen = true;
                        
                        if(!Application.Current.MainWindow.IsVisible || Application.Current.MainWindow==null)
                        {
                            Application.Current.MainWindow = new MainWindow();
                            Application.Current.MainWindow.Show();
                        }
                        }
                        
                };
            }
        }

        /// <summary>
        /// Hides the main window. This command is only enabled if a window is open.
        /// </summary>
        public ICommand HideWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () => {
                        Application.Current.MainWindow.Hide();
                        Application.Current.MainWindow = null;

                    },
                    CanExecuteFunc = () => false
                };
            }
        }


        /// <summary>
        /// Shuts down the application.
        /// </summary>
        public ICommand ExitApplicationCommand
        {
            get
            {
                return new DelegateCommand {CommandAction = () => Application.Current.Shutdown()};
            }
        }

        public void buidlLanguajeMenuHeaders()
        {

        }

        public void buildPluginsMenu()
        {
            List<String> plugins = scppm.getPluginsNames();
            String activePlugin = scppm.getActivePlugin.getName();
            String pluginsmenuheader = translatormenu.Header.ToString();
            translatormenu.Header = pluginsmenuheader + $"({App.dictlang["current"]} {activePlugin})";
            List<MenuItem> pluginsItems = new List<MenuItem>();
            translatormenu.ItemsSource = pluginsItems;

            foreach(String plug in plugins)
            {
                MenuItem plugin = new MenuItem();
                plugin.Header = plug;
                plugin.Command = new DelegateCommand
                {
                    CanExecuteFunc= () => plug!=activePlugin,
                    CommandAction= () =>
                    {
                        scppm.setActivePlugin(plug);
                        app.resetSayclip();
                        translatormenu.Header = pluginsmenuheader + $"({App.dictlang["current"]} {activePlugin})";

                    }
                };

                pluginsItems.Add(plugin);
            }
            translatormenu.Items.Refresh();
        }

        public void buildUILangMenu()
        {
            /*
             
            Dictionary<string, string> uilangs = new Dictionary<string, string>
            {
                {"auto", App.dictlang["menu.ui.auto"].ToString()  },
                {"en", App.dictlang["menu.ui.en"].ToString() },
                {"es", App.dictlang["menu.ui.es"].ToString() }
                
            };
            string uilangmenuheader = uilangmenu.Header.ToString();
            uilangmenu.Header += string.Format("({1} {0})",App.dictlang["menu.ui."+  scpcm.UILang.ToString()].ToString(), App.dictlang["current"].ToString());
            foreach(KeyValuePair<string,string> k in uilangs)
            {
                MenuItem mi = new MenuItem();
                mi.Header = k.Value;
                mi.Command = new DelegateCommand()
                {
                    CanExecuteFunc= ()=>   scpcm.UILang!=k.Key,
                    CommandAction= () =>
                    {
                          scpcm.UILang = k.Key;
                          scpcm.Save();
                        sayclip.ScreenReaderControl.speech(App.dictlang["menu.ui.reset"].ToString(),true);
                        uilangmenu.Header = uilangmenuheader + string.Format("({1} {0})", App.dictlang["menu.ui." +   scpcm.UILang.ToString()].ToString(), App.dictlang["current"].ToString());
                    }
                };


                uilangmenu.Items.Add(mi);
            }
             
             */
        }

        public void buildSpeedMenu()
        {
            string speedHeader = App.dictlang["menu.monitor"].ToString();
            List<MenuItem> speedItems = new List<MenuItem>();

            monitormenu.Header= App.dictlang["menu.monitor"].ToString() + string.Format(" ({0} {1} ms)", App.dictlang["current"].ToString(),   scpcm.clipboardPollingSpeed.ToString());
            monitormenu.ItemsSource = speedItems;
            Dictionary<int, string> speeds = new Dictionary<int, string>
            {
                {2000, App.dictlang["menu.monitor.2000"].ToString() },
                {1000, App.dictlang["menu.monitor.1000"].ToString() },
                {500, App.dictlang["menu.monitor.500"].ToString() },
                {100, App.dictlang["menu.monitor.100"].ToString() },
                { 10, App.dictlang["menu.monitor.10"].ToString()  },
                {1, App.dictlang["menu.monitor.1"].ToString()  },
                
            };

            foreach(KeyValuePair<int,string> dictem in speeds)
            {
                MenuItem speed = new MenuItem();
                speed.Header = dictem.Value + "( " + dictem.Key.ToString() + " ms)";
                speed.Command = new DelegateCommand
                {
                    CanExecuteFunc= ()=> dictem.Key!= (int)  scpcm.clipboardPollingSpeed,
                    CommandAction= () =>
                    {
                        
                        scpcm.clipboardPollingSpeed = (double)dictem.Key;
                      
                        monitormenu.Header= App.dictlang["menu.monitor"].ToString() + string.Format(" ({0} {1} ms)", App.dictlang["current"].ToString(),   scpcm.clipboardPollingSpeed.ToString());
                    }

                };
                speedItems.Add(speed);
            }
            monitormenu.Items.Refresh();
          
        }

        public void buildLanguajeMenuItems()
        {
            
            
        }

        public void reloadIconTitle()
        {
            
            App app = (App)Application.Current;
            
            this.ToolTipText = "Sayclip: key " +   Properties.Settings.Default.sayclipKey;
            if (app.isSayclipRuning)
            {
                this.ToolTipText += ", runing";
                if (ConfigurationManager.getInstance.translating)
                {
                    this.ToolTipText += ", translating.";
                }
            }
            else
            {
                
                this.ToolTipText += " paused";
            }
        }

    }
    }
