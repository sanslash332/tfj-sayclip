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
using System.Windows.Input;
using logSystem;
using Gma.System.MouseKeyHook;
using System.Windows.Media;


namespace sayclipTray
{
    /// <summary>
    /// Simple application. Check the XAML for comments.
    /// </summary>
    public partial class App : Application
    {
        private MainWindow win;
        public static bool saveNextKey;

        private IKeyboardMouseEvents kmEvents;
        private System.Windows.Forms.NotifyIcon nicon;

        private static TaskbarIcon notifyIcon;
        private static Thread sayclipThread;
        private static Dictionary<string, string> countriCodes;
        public static ResourceDictionary dictlang;




        public static void rebuildCountryCodes(ResourceDictionary dictLang)
        {
            countriCodes = new Dictionary<string, string>
            {
                {dictLang["menu.ui.auto"].ToString(), ""},
            {dictLang["country.en"].ToString(), "en"},
            {dictLang["country.es"].ToString(), "es" },
            {dictLang["country.esla"].ToString(), "es-419" },
            {dictLang["country.fr"].ToString(), "fr" },
            {dictLang["country.it"].ToString(),"it"},
            {dictLang["country.ptbr"].ToString(), "pt-BR"},
            { dictLang["country.ptpt"].ToString(), "pt-PT" },
            {dictLang["country.de"].ToString(),"de"},
            {dictLang["country.zhcn"].ToString(), "zh-CN" },
            {dictLang["country.zhtw"].ToString(),"zh-TW" },
            {dictLang["country.ja"].ToString(),"ja"},
            {dictLang["country.ko"].ToString(), "ko" },
            {dictLang["country.sv"].ToString(), "sv" },
            {dictLang["country.tr"].ToString(), "tr" },
            {dictLang["country.uk"].ToString(), "uk" },
            {dictLang["country.ru"].ToString(), "ru" },
            {dictLang["country.iw"].ToString(), "iw"  },
            {dictLang["country.ca"].ToString(), "ca" },
            {dictLang["country.eu"].ToString(), "eu" },



            };
        }
        public static void reloadIconTitle()
        {
            TaskbarIcon tb = notifyIcon;
            tb.ToolTipText = "Sayclip: key " + sayclipTray.Properties.Settings.Default.sayclipKey;
            if(isSayclipRuning())
            {
                tb.ToolTipText += ", runing";
                if(sayclipTray.Properties.Settings.Default.translate)
                {
                    tb.ToolTipText += ", translating.";
                }
            }
            else
            {
                tb.ToolTipText += " paused";
            }
        }
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
            //sayclip.Properties.Settings.Default = sayclipTray.Properties.Settings.Default;
            
            sayclip.Properties.Settings.Default.lan1 = sayclipTray.Properties.Settings.Default.lan1;
            sayclip.Properties.Settings.Default.lan2 = sayclipTray.Properties.Settings.Default.lan2;
            sayclip.Properties.Settings.Default.translate = sayclipTray.Properties.Settings.Default.translate;
            sayclip.Properties.Settings.Default.copyresult = sayclipTray.Properties.Settings.Default.copyresult;
            sayclip.Properties.Settings.Default.active = sayclipTray.Properties.Settings.Default.active;
            sayclip.Properties.Settings.Default.allowRepeat = sayclipTray.Properties.Settings.Default.allowRepeat;
            sayclip.Properties.Settings.Default.translator = sayclipTray.Properties.Settings.Default.translator;
            sayclip.Properties.Settings.Default.msAppID = sayclipTray.Properties.Settings.Default.msAppID;
            sayclip.Properties.Settings.Default.msAppSecret = sayclipTray.Properties.Settings.Default.msAppSecret;
            sayclip.Properties.Settings.Default.interval = sayclipTray.Properties.Settings.Default.interval;
            //sayclip.Properties.Settings.Default.Save();
            Sayclip.dictlang = dictlang;


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

        protected virtual void onKeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            //LogWriter.escribir("tecla detectada: " + e.KeyCode.ToString() + "\n con el código: " +e.KeyValue.ToString());
            //ScreenReaderControl.speech("tecla detectada: " + e.KeyCode.ToString(), true);
            if(saveNextKey)
            {
                sayclipTray.Properties.Settings.Default.sayclipKey = e.KeyCode;
                sayclipTray.Properties.Settings.Default.Save();
                Application.Current.MainWindow.Close();
                Application.Current.MainWindow = new MainWindow();
                Application.Current.MainWindow.Show();


                

                saveNextKey = false;
                App.reloadIconTitle();
                return;

            }
            if(e.KeyCode == sayclipTray.Properties.Settings.Default.sayclipKey && e.Control== true && e.Shift==true)
            {
                ScreenReaderControl.speech(dictlang["menu.open"].ToString(),true);
                notifyIcon.Focus();
                win = new MainWindow();
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
                    Application.Current.MainWindow = new MainWindow();

                    Application.Current.MainWindow.Show();

                }
                else
                {
                    sayclip.ScreenReaderControl.speech(dictlang["ui.opened"].ToString(), true);

                }
            }

            else if(e.Control== true && e.KeyCode== sayclipTray.Properties.Settings.Default.sayclipKey)
            {
                sayclip.Sayclip.repeatLastClipboardContent();
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ResourceDictionary dictLang;
            switch(sayclipTray.Properties.Settings.Default.UILang)
            {
                case "en":
                    dictLang = new ResourceDictionary() { Source = new Uri("lang\\en.xaml", UriKind.Relative) };
                    break;
                case "es":
                    dictLang = new ResourceDictionary() { Source = new Uri("lang\\es.xaml", UriKind.Relative) };
                    break;
                case "auto":
                    string cult = Thread.CurrentThread.CurrentCulture.ToString();
                    if(cult.StartsWith("es"))
                    {
                        dictLang = new ResourceDictionary() { Source = new Uri("lang\\es.xaml", UriKind.Relative) };
                    }
                    else
                    {
                        dictLang = new ResourceDictionary() { Source = new Uri("lang\\en.xaml", UriKind.Relative) };
                    }
                    
                    break;
                default:
                    dictLang = new ResourceDictionary() { Source = new Uri("lang\\en.xaml", UriKind.Relative) };
                    break;
            }

            this.Resources.MergedDictionaries.Add(dictLang);
            dictlang = dictLang;
            rebuildCountryCodes(dictLang);
            

            win = new sayclipTray.MainWindow();
            win.Hide();
            Application.Current.MainWindow = win;
            //create the notifyicon (it's a resource declared in NotifyIconResources.xaml
            LogWriter.init();
            kmEvents = Hook.GlobalEvents();
            kmEvents.KeyUp+= new System.Windows.Forms.KeyEventHandler(onKeyUp);
            kmEvents.KeyPress += KmEvents_KeyPress;
            /*
            nicon = new System.Windows.Forms.NotifyIcon();
            nicon.Text = "other sayclip icon ";
                        nicon.BalloonTipText = "say say sayclip";
             nicon.Icon = new System.Drawing.Icon("Red.ico");
            nicon.Visible = true;
            System.Windows.Forms.ContextMenuStrip niconmenu = new System.Windows.Forms.ContextMenuStrip();
            niconmenu.Items.Add("una opcion ");
            niconmenu.Items.Add("otra ");
            nicon.ContextMenuStrip = niconmenu;
            */

            notifyIcon= (TaskbarIcon) FindResource("NotifyIcon");
            ContextMenu systraymenu = notifyIcon.ContextMenu;
            systraymenu.Closed += Systraymenu_Closed;
            
            //notifyIcon.KeyDown += NotifyIcon_KeyDown;
            //notifyIcon.TrayMouseDoubleClick += NotifyIcon_TrayMouseDoubleClick;
            notifyIcon.MenuActivation = PopupActivationMode.LeftOrRightClick;

            
            MenuItem transmenu = null;
            MenuItem uilangmenu = null;
            MenuItem SourseLanMenu=null;
            MenuItem TargetLanMenu=null;
            MenuItem speedmenu = null;
            MenuItem exchangeOption = null;


            foreach(MenuItem item in systraymenu.Items)
            {
                //logSystem.LogWriter.escribir("Revisando el item: " + item.Header.ToString());
                if(item.Name.ToString().Equals("SourceLanMenu"))
                {
                    SourseLanMenu = item;
                }
                else if(item.Name.ToString().Equals("uilangmenu"))
                {
                    uilangmenu = item;
                }
                else if (item.Name.ToString().Equals("translatormenu"))
                {
                    //logSystem.LogWriter.escribir("encontrado transmenu");
                    transmenu = item;
                    }
                else if(item.Name.ToString().Equals("TargetLanMenu"))
                {
                    TargetLanMenu = item;

                }
                else if(item.Name.ToString().Equals("monitormenu"))
                {
                    speedmenu = item;
                }
                else if(item.Name.ToString().Equals("exchangemenu"))
                {
                    exchangeOption = item;
                }
            }

            string SourseLanMenuHeader = SourseLanMenu.Header.ToString();
            string targetLanMenuHeader = TargetLanMenu.Header.ToString();
            string transmenuHeader = transmenu.Header.ToString();
            SourseLanMenu.Header = SourseLanMenuHeader + " (" + countriCodes.FirstOrDefault(x => x.Value == sayclipTray.Properties.Settings.Default.lan1).Key + ")";
            TargetLanMenu.Header = targetLanMenuHeader + " (" + countriCodes.FirstOrDefault(x => x.Value == sayclipTray.Properties.Settings.Default.lan2).Key + ")";
            transmenu.Header = transmenuHeader + string.Format("({0} {1} )", dictLang["current"].ToString(), sayclipTray.Properties.Settings.Default.translator.ToString());


            foreach(MenuItem r in transmenu.Items)
            {


                r.Command = new DelegateCommand()
                {
                    CanExecuteFunc = () =>
                    {
                        if(sayclipTray.Properties.Settings.Default.translator== translatorType.google && r.Header.ToString().Equals("Google API"))
                        {
                            return (false);

                        }
                        else if (sayclipTray.Properties.Settings.Default.translator == translatorType.microsoft && r.Header.ToString().Equals("Microsoft API"))
                        {
                            return (false);
                        }
                        else
                        {
                            return (true);
                        }
                    },
                    CommandAction = () =>
                        {

                            if (r.Header.ToString().Equals("Google API"))
                            {
                                if(sayclipTray.Properties.Settings.Default.lan1=="")
                                {
                                    sayclip.ScreenReaderControl.speech(dictlang["menu.plugin.google.badlang"].ToString(),true);
                                    return;
                                }
                                sayclipTray.Properties.Settings.Default.translator = translatorType.google;
                                sayclipTray.Properties.Settings.Default.Save();

                                transmenu.Header = transmenuHeader + string.Format("({0} {1} )", dictLang["current"].ToString(), sayclipTray.Properties.Settings.Default.translator.ToString());
                                sayclip.ScreenReaderControl.speech(dictlang["selected"].ToString() + " google api", false);
                                resetSayclip();
                            }
                            else if (r.Header.ToString().Equals("Microsoft API"))
                            {
                                if (sayclipTray.Properties.Settings.Default.msAppSecret.Equals("") || sayclipTray.Properties.Settings.Default.msAppSecret == null || sayclipTray.Properties.Settings.Default.msAppID.Equals("") || sayclipTray.Properties.Settings.Default.msAppID == null)
                                {
                                    Sayclip.sayAndCopy(dictlang["menu.plugin.microsoft.empty"].ToString());
                                    return;

                                }

                                if(Sayclip.checkMSCredentials())
                                {
                                    sayclipTray.Properties.Settings.Default.translator = translatorType.microsoft;
                                    sayclipTray.Properties.Settings.Default.Save();

                                    transmenu.Header = transmenuHeader + string.Format("({0} {1} )", dictLang["current"].ToString(), sayclipTray.Properties.Settings.Default.translator.ToString());
                                    sayclip.ScreenReaderControl.speech(dictlang["selected"].ToString() + " microsoft api", false);
                                    resetSayclip();
                                    
                                }
                                else
                                {
                                    
                                    
                                    Sayclip.sayAndCopy(dictlang["menu.plugin.microsoft.error"].ToString());
                                    
                                    

                                }
                            }
                            }

                };
            }

            exchangeOption.Command = new DelegateCommand
            {
                CanExecuteFunc = () => sayclipTray.Properties.Settings.Default.lan1.ToString()!="",
                CommandAction = () =>
                {
                    killSayclip();
                    string bclan = sayclipTray.Properties.Settings.Default.lan1;
                    sayclipTray.Properties.Settings.Default.lan1 = sayclipTray.Properties.Settings.Default.lan2;
                    sayclipTray.Properties.Settings.Default.lan2 = bclan;
                    sayclipTray.Properties.Settings.Default.Save();
                    SourseLanMenu.Header = SourseLanMenuHeader + " (" + countriCodes.FirstOrDefault(x => x.Value == sayclipTray.Properties.Settings.Default.lan1).Key + ")";
                    TargetLanMenu.Header = targetLanMenuHeader + " (" + countriCodes.FirstOrDefault(x => x.Value == sayclipTray.Properties.Settings.Default.lan2).Key + ")";

                    startSayclip();

                }

            };

            foreach(KeyValuePair<string,string> dictem in countriCodes)
            {
                MenuItem item = new MenuItem();
                item.Header = dictem.Key;
                item.Command = new languageCommand
                {
                    lanCode = dictem.Value,
                    CanExecuteFunc = () =>
                    {
                        if(sayclipTray.Properties.Settings.Default.translator== translatorType.google && dictem.Value=="")
                        {
                            return (false);
                        }
                        else
                        {
                            return (sayclipTray.Properties.Settings.Default.lan1 != dictem.Value && dictem.Value != sayclipTray.Properties.Settings.Default.lan2);
                        }
                            
                        
                        

                    },
                        
                    CommandAction = () =>
                    {
                        killSayclip();
                        sayclipTray.Properties.Settings.Default.lan1=dictem.Value;
                sayclipTray.Properties.Settings.Default.Save();
                startSayclip();
                SourseLanMenu.Header = SourseLanMenuHeader + " (" + dictem.Key + ")";
                    }

                };

                SourseLanMenu.Items.Add(item);
                MenuItem item2 = new MenuItem();
                item2.Header = dictem.Key;
                item2.Command = new languageCommand
                {
                    lanCode=dictem.Value,
                    CanExecuteFunc= ()=> sayclipTray.Properties.Settings.Default.lan2!=dictem.Value && sayclipTray.Properties.Settings.Default.lan1!=dictem.Value,
                    CommandAction= () =>
                    {
                        killSayclip();
                        sayclipTray.Properties.Settings.Default.lan2 = dictem.Value;
                        sayclipTray.Properties.Settings.Default.Save();
                        startSayclip();
                        TargetLanMenu.Header = targetLanMenuHeader + " (" + dictem.Key + ")";
                    }
                };
                if(dictem.Value!="")
                {
                    TargetLanMenu.Items.Add(item2);
                }
                
                

            }

            string speedHeader = speedmenu.Header.ToString();
            speedmenu.Header = speedHeader + string.Format(" ({0} {1} ms)", dictLang["current"].ToString(), sayclipTray.Properties.Settings.Default.interval.ToString());
            Dictionary<int, string> speeds = new Dictionary<int, string>
            {
                {2000, dictLang["menu.monitor.2000"].ToString() },
                {1000, dictLang["menu.monitor.1000"].ToString() },
                {500, dictLang["menu.monitor.500"].ToString() },
                {100, dictLang["menu.monitor.100"].ToString() },
                { 10, dictLang["menu.monitor.10"].ToString()  },
                {1, dictLang["menu.monitor.1"].ToString()  },
                
            };

            foreach(KeyValuePair<int,string> dictem in speeds)
            {
                MenuItem speed = new MenuItem();
                speed.Header = dictem.Value + "( " + dictem.Key.ToString() + " ms)";
                speed.Command = new DelegateCommand
                {
                    CanExecuteFunc= ()=> dictem.Key!= (int)sayclipTray.Properties.Settings.Default.interval,
                    CommandAction= () =>
                    {
                        killSayclip();
                        sayclipTray.Properties.Settings.Default.interval = (double)dictem.Key;
                        sayclipTray.Properties.Settings.Default.Save();
                        speedmenu.Header = speedHeader + string.Format(" ({0} {1} ms)", dictLang["current"].ToString(), sayclipTray.Properties.Settings.Default.interval.ToString());

                        startSayclip();
                        
                    }

                };
                speedmenu.Items.Add(speed);
            }

            Dictionary<string, string> uilangs = new Dictionary<string, string>
            {
                {"auto", dictLang["menu.ui.auto"].ToString()  },
                {"en", dictLang["menu.ui.en"].ToString() },
                {"es", dictLang["menu.ui.es"].ToString() }
                
            };
            string uilangmenuheader = uilangmenu.Header.ToString();
            uilangmenu.Header += string.Format("({1} {0})",dictLang["menu.ui."+sayclipTray.Properties.Settings.Default.UILang.ToString()].ToString(), dictLang["current"].ToString());
            foreach(KeyValuePair<string,string> k in uilangs)
            {
                MenuItem mi = new MenuItem();
                mi.Header = k.Value;
                mi.Command = new DelegateCommand()
                {
                    CanExecuteFunc= ()=> sayclipTray.Properties.Settings.Default.UILang!=k.Key,
                    CommandAction= () =>
                    {
                        sayclipTray.Properties.Settings.Default.UILang = k.Key;
                        sayclipTray.Properties.Settings.Default.Save();
                        sayclip.ScreenReaderControl.speech(dictlang["menu.ui.reset"].ToString(),true);
                        uilangmenu.Header = uilangmenuheader + string.Format("({1} {0})", dictLang["menu.ui." + sayclipTray.Properties.Settings.Default.UILang.ToString()].ToString(), dictLang["current"].ToString());
                    }
                };


                uilangmenu.Items.Add(mi);
            }
            startSayclip();
            App.reloadIconTitle();
        }

        private void Systraymenu_Closed(object sender, RoutedEventArgs e)
        {
            if(win!= null)
            {
                win.Hide();

            }
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
            notifyIcon.Dispose(); //the icon would clean up automatically, but this is cleaner
            LogWriter.terminate();
            killSayclip();

            kmEvents.KeyUp-= onKeyUp;

            kmEvents.Dispose();            
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
