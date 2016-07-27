using System;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using Hardcodet.Wpf.TaskbarNotification;
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
    
           
        /// <summary>
        /// </summary>
        public static string sourceLanheader;
        public static string targetLanHeader;
        
        public ICommand enablecopyresult
        {
            get
            {
                return new DelegateCommand()
                {
                    CanExecuteFunc= () => sayclipTray.Properties.Settings.Default.translate && !sayclipTray.Properties.Settings.Default.copyresult,
                    CommandAction = () =>
                        {
                            sayclipTray.Properties.Settings.Default.copyresult = true;
                            sayclipTray.Properties.Settings.Default.Save();
                            App.resetSayclip();
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
                    CanExecuteFunc= () => sayclipTray.Properties.Settings.Default.translate && sayclipTray.Properties.Settings.Default.copyresult,
                    CommandAction = () =>
                        {
                            sayclipTray.Properties.Settings.Default.copyresult = false;
                            sayclipTray.Properties.Settings.Default.Save();
                            App.resetSayclip();

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
                    CanExecuteFunc = () => App.isSayclipRuning(),
                    CommandAction = () =>
                        {
                            App.killSayclip();
                            App.reloadIconTitle();
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
                    CanExecuteFunc = () => !App.isSayclipRuning(),
                    CommandAction = () =>
                        {
                            App.startSayclip();
                            App.reloadIconTitle();
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
                    CanExecuteFunc = () => sayclip.Properties.Settings.Default.allowRepeat,
                    CommandAction = () =>
                    {
                        App.reloadIconTitle();
                        sayclipTray.Properties.Settings.Default.allowRepeat = !sayclipTray.Properties.Settings.Default.allowRepeat;
                        sayclipTray.Properties.Settings.Default.Save();
                        App.resetSayclip();

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
                    CanExecuteFunc = () => !sayclip.Properties.Settings.Default.allowRepeat,
                    CommandAction = () =>
                        {
                            sayclipTray.Properties.Settings.Default.allowRepeat = !sayclipTray.Properties.Settings.Default.allowRepeat;
                            sayclipTray.Properties.Settings.Default.Save();
                            App.resetSayclip();

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
                    CanExecuteFunc= ()=> !sayclip.Properties.Settings.Default.translate,
                    CommandAction= () =>
                    {
                        App.killSayclip();
                        sayclipTray.Properties.Settings.Default.translate = true;
                        sayclipTray.Properties.Settings.Default.Save();
                        App.resetSayclip();
                        App.reloadIconTitle();

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
                        App.resetSayclip();
                        App.reloadIconTitle();

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

    public class languageCommand : DelegateCommand
    {
        public string lanCode { get; set;  }
    }
}
