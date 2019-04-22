using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public bool cloceNow = false;
        public iSayclipPluginTranslator translator;
        private static object lockobj =     new object();

        public async Task sayAndCopy(string txt)
        {
            
            ScreenReaderControl.speech(txt, true);
            Monitor.Enter(lockobj);
            data = txt;

            try
            {
                Clipboard.SetText(data);
            }
            catch(Exception e)
            {
                logSystem.LogWriter.getLog().Error(string.Format("Error setting text on the clipboard: {0} \n type: {1} \n stack: {2} ", e.Message.ToString(), e.ToString(), e.StackTrace.ToString()));
            }
            
            Monitor.Exit(lockobj);

        }
        
        
        public async Task repeatLastClipboardContent()
        {
            ScreenReaderControl.speech(data,true);

        }
        
        public async Task Main()
        {
            if(Monitor.IsEntered(lockobj))
            {
                Monitor.Exit(lockobj);
            }

            logAlarmSet = false;

            translator = PluginManager.getInstanse.getActivePlugin;
            translate = Properties.Settings.Default.translate;
            if (translator==null)
            {
                var saytask = sayAndCopy(dictlang["internal.noactiveplugin"].ToString());

                translate = false;
            }

            ScreenReaderControl.speech(dictlang["internal.start"].ToString(), false);
            int interval = (int)Properties.Settings.Default.interval;
            while (!cloceNow)
            {
                var task = checkClipboard();
                
                Thread.Sleep(interval);

            }
            
            
        }

        private async Task checkClipboard()
        {
            
                if (Clipboard.ContainsText())
                {
                string rok = data;
                
                Monitor.Enter(lockobj);
                try
                {
                    
                    rok = Clipboard.GetText();
                    
                }
                catch (Exception e)
                {
                    LogWriter.getLog().Warn("error copying the clipboard " + e.Message +" \n type: " +e.ToString() + " \n stack: " + e.StackTrace.ToString() );
                    //ScreenReaderControl.speech("error copying the clipboard " + e.Message, true);
                    //Clipboard.Flush();
                    rok = data;
                    

                }
                
                
                Monitor.Exit(lockobj);
                
                
                //Console.WriteLine("tenemos texto en el cp: {0}", rok);
                if (!data.Equals(rok))
                    {
                        data = rok;
                        if(Properties.Settings.Default.allowRepeat)
                        {
                            Monitor.Enter(lockobj);
                            data += " \n";
                        try
                        {
                            Clipboard.SetText(data);
                        }
                        catch (Exception e)
                        {
                            logSystem.LogWriter.getLog().Debug(string.Format("Error setting text on the clipboard: {0} \n type: {1} \n stack: {2} ", e.Message.ToString(), e.ToString(), e.StackTrace.ToString()));

                        }
                            
                            Monitor.Exit(lockobj);

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
                                Monitor.Enter(lockobj);
                                data = trad;
                                if(sayclip.Properties.Settings.Default.allowRepeat)
                                {
                                    data += " \n";
                                }
                                try
                                {
                                    Clipboard.SetText(data);
                                }
                                catch (Exception e)
                                {
                                    logSystem.LogWriter.getLog().Warn(string.Format("Error setting text on the clipboard: {0} \n type: {1} \n stack: {2} ", e.Message.ToString(), e.ToString(), e.StackTrace.ToString()));

                                }
                                
                                Monitor.Exit(lockobj);


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
            else
            {
                //Console.WriteLine("sin datos ");

            }
            }


        }
    }

