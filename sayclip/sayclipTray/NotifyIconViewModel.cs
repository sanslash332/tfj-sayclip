using System;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;

using sayclip;

namespace sayclipTray
{
    /// <summary>
    /// Provides bindable properties and commands for the NotifyIcon. In this sample, the
    /// view model is assigned to the NotifyIcon in XAML. Alternatively, the startup routing
    /// in App.xaml.cs could have created this view model, and assigned it to the NotifyIcon.
    /// </summary>
    public class NotifyIconViewModel
    {
    
           
        public Dictionary<string, string> countriCodes = new Dictionary<string, string>
        {
            {"English", "en"},
            {"Spanish", "es" },
            {"Spanish (Latin American)", "es-419" },
            {"French", "fr" },
            {"Italian","it"},
            {"Portuguese (Brazil)", "pt-BR"},
            {"German","de"},
            { "Portuguese (Portugal)", "pt-PT" },
            {"Chinese (Simplified)", "zh-CN" },
            {"Swedish", "sv" },
            {"Turkish", "tr" },
            {"Ukrainian", "uk" },
            {"Chinese (Traditional)","zh-TW" },
            {"Japanece","ja"},
            {"Japanese (jw)","jw" },
            {"Korean", "ko" },
            {"Russian", "ru" },
            {"Hebrew", "iw"  },
            {"Catalan", "ca" },
            {"Basque", "eu" },
            {"Klingon","xx-klingon" },
            {"Bork bork bork", "xx-bork"},
            {"Elmer Fudd", "xx-elmer" },
            {"Hacker", "xx-hacker" },
            { "Pirate","xx-pirate" },
            
        };
        
        
        /// <summary>
        /// </summary>
        public ICommand PauseSayclipCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc= () => App.isSayclipRuning(),
                    CommandAction= ()=> App.killSayclip()

                };
            }
        }

        public ICommand ResumeSayclipCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc= ()=> !App.isSayclipRuning(),
                    CommandAction= ()=> App.startSayclip()
                };
            }
        }

        public ICommand EnableTranslationCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc= ()=> !sayclip.Properties.Settings.Default.translate,
                    CommandAction= () =>
                    {
                        App.killSayclip();
                        sayclipTray.Properties.Settings.Default.translate = true;
                        sayclipTray.Properties.Settings.Default.Save();
                        App.startSayclip();

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
                    CanExecuteFunc= ()=> sayclip.Properties.Settings.Default.translate,
                    CommandAction= ()=>
                    {
                        App.killSayclip();
                        sayclipTray.Properties.Settings.Default.translate = false;
                        sayclipTray.Properties.Settings.Default.Save();
                        App.startSayclip();

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
                    CanExecuteFunc = () => Application.Current.MainWindow == null,
                    CommandAction = () =>
                    {
                        Application.Current.MainWindow = new MainWindow();
                        Application.Current.MainWindow.Show();
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
                    CommandAction = () => Application.Current.MainWindow.Close(),
                    CanExecuteFunc = () => Application.Current.MainWindow != null
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
    }


    /// <summary>
    /// Simplistic delegate command for the demo.
    /// </summary>
    public class DelegateCommand : ICommand
    {
        public Action CommandAction { get; set; }
        public Func<bool> CanExecuteFunc { get; set; }

        public void Execute(object parameter)
        {
            CommandAction();
        }

        public bool CanExecute(object parameter)
        {
            return CanExecuteFunc == null  || CanExecuteFunc();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
