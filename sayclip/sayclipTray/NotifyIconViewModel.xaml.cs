using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Linq;
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
                        
                        if(!Application.Current.MainWindow.IsVisible)
                        {
                            //Application.Current.MainWindow = new MainWindow();
                            LogWriter.getLog().Debug($"show window command triggered");
                            Application.Current.MainWindow.Show();
                            
                        }
                        app.closeWindowAfterCloseSystrayMenu = false;
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

        public ICommand exchangeLanguajesCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc= () => {
                        SayclipLanguage fromLangKey = scppm.getActivePlugin.getConfiguredLanguajes("en")[0];
                        SayclipLanguage toLangKey = scppm.getActivePlugin.getConfiguredLanguajes("en")[1];
                        return ((fromLangKey.isForSource && fromLangKey.isForTarget) && ( toLangKey.isForSource && toLangKey.isForTarget));
                    },
                    CommandAction= () =>
                    {
                        
                        SayclipLanguage fromLangKey = scppm.getActivePlugin.getConfiguredLanguajes("en")[0];
                        SayclipLanguage toLangKey = scppm.getActivePlugin.getConfiguredLanguajes("en")[1];
                        scppm.getActivePlugin.setLanguages(toLangKey, fromLangKey);
                        this.buidlLanguajeMenuHeaders();
                        this.buildLanguajeMenuItems();


                    }
                };
            }
        }

        public void buidlLanguajeMenuHeaders()
        {
            SourceLanMenu.Header = $"{App.dictlang["menu.source"]} ({App.dictlang["current"]} {scppm.getActivePlugin.getConfiguredLanguajes("en")[0].displayName})";
            TargetLanMenu.Header= $"{App.dictlang["menu.target"]} ({App.dictlang["current"]} {scppm.getActivePlugin.getConfiguredLanguajes("en")[1].displayName})";

        }

        public void buildPluginsMenu()
        {
            List<String> plugins = scppm.getPluginsNames();
            String activePlugin = scppm.getActivePlugin.getName();
            
            translatormenu.Header = $"{App.dictlang["menu.plugin"]} ({App.dictlang["current"]} {activePlugin})";
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
                        if(scppm.setActivePlugin(plug))
                        {
                            app.resetSayclip();
                            
                            buidlLanguajeMenuHeaders();
                            buildLanguajeMenuItems();
                            buildPluginsMenu();

                        }
else
                        {
                            ScreenReaderControl.speech(String.Format(App.dictlang["menu.plugin.changeError"].ToString(), plug), true);
                            scppm.setActivePlugin(activePlugin);
                            

                        }
                    }
                };

                pluginsItems.Add(plugin);
            }
            translatormenu.Items.Refresh();
        }

        public void buildUILangMenu()
        {
            List<MenuItem> languajeItems = new List<MenuItem>();
            uilangmenu.ItemsSource = languajeItems;

            Dictionary<string, string> uilangs = ConfigValues.uiLangOptions;
            
            string uilangmenuheader = App.dictlang["menu.ui"].ToString();
            uilangmenu.Header = string.Format("{2} ({1} {0})",App.dictlang["menu.ui."+  Properties.Settings.Default.UILang.ToString()].ToString(), App.dictlang["current"].ToString(), uilangmenuheader);
            foreach(KeyValuePair<string,string> k in uilangs)
            {
                MenuItem mi = new MenuItem();
                mi.Header = k.Value;
                mi.Command = new DelegateCommand()
                {
                    CanExecuteFunc= ()=>  Properties.Settings.Default.UILang !=k.Key,
                    CommandAction= () =>
                    {
                        Properties.Settings.Default.UILang= k.Key;
                        Properties.Settings.Default.Save();
                        sayclip.ScreenReaderControl.speech(App.dictlang["menu.ui.reset"].ToString(),true);
                        uilangmenu.Header = uilangmenuheader + string.Format("({1} {0})", App.dictlang["menu.ui." +   Properties.Settings.Default.UILang.ToString()].ToString(), App.dictlang["current"].ToString());

                    }
                };


                languajeItems.Add(mi);
            }

            uilangmenu.Items.Refresh();
        }

        public void buildSpeedMenu()
        {
            string speedHeader = App.dictlang["menu.monitor"].ToString();
            List<MenuItem> speedItems = new List<MenuItem>();

            monitormenu.Header= App.dictlang["menu.monitor"].ToString() + string.Format(" ({0} {1} ms)", App.dictlang["current"].ToString(),   scpcm.clipboardPollingSpeed.ToString());
            monitormenu.ItemsSource = speedItems;
            Dictionary<int, string> speeds = ConfigValues.monitorSpeedsOptions;

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

        public async void buildLanguajeMenuItems()
        {
            List<MenuItem> sourceMenuItems = new List<MenuItem>();
            List<MenuItem> targetMenuItems = new List<MenuItem>();
            SourceLanMenu.ItemsSource = sourceMenuItems;
            TargetLanMenu.ItemsSource = targetMenuItems;
            List<SayclipLanguage> langs = (List<SayclipLanguage>)await scppm.getActivePlugin.getAvailableLanguages(App.uiLang);
            SayclipLanguage fromLang = scppm.getActivePlugin.getConfiguredLanguajes(App.uiLang)[0];
            SayclipLanguage toLang = scppm.getActivePlugin.getConfiguredLanguajes(App.uiLang)[1];

            foreach (SayclipLanguage lang in langs)
            {
                MenuItem source = new MenuItem();
                MenuItem target = new MenuItem();
                source.Header = lang.displayName;
                target.Header = lang.displayName;
                source.Command = new DelegateCommand
                {
                    CanExecuteFunc= ()=> lang.langCode !=scppm.getActivePlugin.getConfiguredLanguajes("en")[0].langCode && lang.isForSource,
                    CommandAction= () =>
                    {
                        scppm.getActivePlugin.setLanguages(lang,toLang);
                        this.buidlLanguajeMenuHeaders();
                        this.buildLanguajeMenuItems();

                    }
                };

                target.Command = new DelegateCommand
                {
                    CanExecuteFunc = () => lang.langCode != scppm.getActivePlugin.getConfiguredLanguajes("en")[1].langCode && lang.isForTarget,
                    CommandAction= ()=>
                    {
                        scppm.getActivePlugin.setLanguages(fromLang, lang);
                        this.buidlLanguajeMenuHeaders();
                        this.buildLanguajeMenuItems();


                    }
                };

                sourceMenuItems.Add(source);
                targetMenuItems.Add(target);

            }

            SourceLanMenu.Items.Refresh();
            TargetLanMenu.Items.Refresh();
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
