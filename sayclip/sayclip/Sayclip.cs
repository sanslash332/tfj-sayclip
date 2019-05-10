using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;
using System.Runtime.InteropServices;
using System.Windows;
using System.Threading;
using logSystem;
using NLog;

namespace sayclip
    {
    public  class Sayclip
    {
        public static ResourceDictionary dictlang;
        
        private bool translate;
        private bool logAlarmSet = false;
        private string data = "";        
        public iSayclipPluginTranslator translator;
        private static object lockobj =     new object();

        private async Task setClipboardText(string data)
        {
            await Task.Factory.StartNew(() =>
            {

                Monitor.Enter(lockobj);
                
                try
                {
                    Clipboard.SetText(data);
                }
                catch (Exception e)
                {
                    logSystem.LogWriter.getLog().Error(string.Format("Error setting text on the clipboard: {0} \n type: {1} \n stack: {2} ", e.Message.ToString(), e.ToString(), e.StackTrace.ToString()));
                }

                Monitor.Exit(lockobj);

            },CancellationToken.None,TaskCreationOptions.None,new StaTaskScheduler(1));


        }

        public async Task sayAndCopy(string txt)
        {
            
            ScreenReaderControl.speech(txt, true);
            await setClipboardText(txt);

        }
        
        
        public void repeatLastClipboardContent()
        {
            ScreenReaderControl.speech(data,true);

        }
        
        public async Task Main(CancellationToken canceller)
        {
            LogWriter.getLog().Debug("Starting sayclip core");

            if(Monitor.IsEntered(lockobj))
            {
                Monitor.Exit(lockobj);
            }

            logAlarmSet = false;

            translator = PluginManager.getInstanse.getActivePlugin;
            translate = Properties.Settings.Default.translate;
            if (translator==null)
            {
                LogWriter.getLog().Warn("no active plugin detected");
                var saytask = sayAndCopy(dictlang["internal.noactiveplugin"].ToString());

                translate = false;
            }

            ScreenReaderControl.speech(dictlang["internal.start"].ToString(), false);
            int interval = (int)Properties.Settings.Default.interval;
            while (!canceller.IsCancellationRequested)
            {
                //var task = Task.Factory.StartNew(checkClipboard,canceller,TaskCreationOptions.None, new System.Threading.Tasks.Schedulers.StaTaskScheduler(2));
                var task = checkClipboard();
                
                await Task.Delay(interval);
                
            }
            LogWriter.getLog().Debug("shuting down sayclip core");

            
        }

        private async Task checkClipboard()
        {
            
            if (Clipboard.ContainsText())
                {
                string rok = data;
                await Task.Factory.StartNew(() =>
                {
                    Monitor.Enter(lockobj);
                    try
                    {

                        rok = Clipboard.GetText();

                    }
                    catch (Exception e)
                    {
                        LogWriter.getLog().Warn("error copying the clipboard " + e.Message + " \n type: " + e.ToString() + " \n stack: " + e.StackTrace.ToString());
                        //ScreenReaderControl.speech("error copying the clipboard " + e.Message, true);
                        //Clipboard.Flush();
                        rok = data;


                    }


                    Monitor.Exit(lockobj);


                },CancellationToken.None,TaskCreationOptions.None,new StaTaskScheduler(1));
                
                //Console.WriteLine("tenemos texto en el cp: {0}", rok);
                if (!data.Equals(rok))
                    {
                        data = rok;
                        if(Properties.Settings.Default.allowRepeat)
                        {
                        data += "\n";
                        setClipboardText(data);

                        }
                        
                        if (translate)
                        {
                            string trad = await translator.translate(data);
                        if(trad.Equals("") && !logAlarmSet)
                        {
                            var task = sayAndCopy(dictlang["internal.errortranslating"].ToString());
                            logAlarmSet = true;
                        }
                        else
                        {
                            ScreenReaderControl.speech(trad, true);
                            logAlarmSet = false;
                            if(sayclip.Properties.Settings.Default.copyresult)
                            {
                                data = trad;
                                if (sayclip.Properties.Settings.Default.allowRepeat)
                                {
                                    data += "\n";
                                }

                                setClipboardText(data);

                            }
                        }
                            
                            //Console.WriteLine(trad);
                        }
                        else
                        {

                            ScreenReaderControl.speech(data, true);
                        //Console.WriteLine(data);
                        logAlarmSet = false;
                        }

                    }


                }
            
            }


        }
    }

