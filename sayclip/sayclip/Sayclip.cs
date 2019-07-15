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
using WK.Libraries.SharpClipboardNS;

namespace sayclip
    {
    public  class Sayclip
    {
        public static ResourceDictionary dictlang;
        private SharpClipboard sharpCP;
      
        private bool translate
        {
            get
            {
                return ConfigurationManager.getInstance.translating;
            }
        }
        private bool logAlarmSet = false;
        private string data = "";        
        public iSayclipPluginTranslator translator;
        private static object lockobj =     new object();

        private async Task setClipboardText(string data)
        {
            //Task writeTask = Task.Factory.StartNew((object milock) =>
            await excecuteInSTA(() =>
            {

                //Monitor.Enter(milock);

                try
                {
                    Clipboard.SetText(data);
                }
                catch (Exception e)
                {
                    logSystem.LogWriter.getLog().Error(string.Format("Error setting text on the clipboard: {0} \n type: {1} \n stack: {2} ", e.Message.ToString(), e.ToString(), e.StackTrace.ToString()));
                    

                }

                //Monitor.Exit(milock);

            });
            //,lockobj,CancellationToken.None,TaskCreationOptions.None,new StaTaskScheduler(1));
            //await writeTask;
            //writeTask.Dispose();



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
          
            if (translator.getName()==EmptyPlugin.emptyName)
            {
                LogWriter.getLog().Warn("no active plugin detected");
                var saytask = sayAndCopy(dictlang["internal.noactiveplugin"].ToString());

                ConfigurationManager.getInstance.translating = false;
            }

            ScreenReaderControl.speech(dictlang["internal.start"].ToString(), false);
            int interval = (int)ConfigurationManager.getInstance.clipboardPollingSpeed;
            sharpCP = new SharpClipboard();
            sharpCP.ClipboardChanged += SharpCP_ClipboardChanged;
          
          
        }

        public void shutDownCore()
        {
            LogWriter.getLog().Debug("shuting down sayclip core");
            sharpCP.ClipboardChanged -= SharpCP_ClipboardChanged;

        }

        private async void SharpCP_ClipboardChanged(object sender, SharpClipboard.ClipboardChangedEventArgs e)
        {
            if(e.ContentType== SharpClipboard.ContentTypes.Text && e.Content!=null && !checkEmptyuString(e.Content.ToString()))
            {
              

                await checkClipboard(e.Content.ToString());
            }
        }

        private bool checkEmptyuString(string text)
        {
            String clearText = text.Replace("\n", "");
            clearText = clearText.Replace("\t", "");
            clearText = clearText.Replace("\r", "");
            clearText = clearText.Replace(" ", "");
            clearText = clearText.Trim();
          
            return (clearText.Equals(""));

        }

        private async Task checkClipboard(String text)
        {
            
            if (Clipboard.ContainsText())
                {
                string rok = data;
                rok = text;
                LogWriter.getLog().Debug($"captured data is: {rok}");
              
                if (!data.Equals(rok))
                    {
                        data = rok;
                        if(Properties.Settings.Default.allowRepeat)
                        {
                        data += "\n";
                        //setClipboardText(data);

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

        public static Task excecuteInSTA(ThreadStart act)
        {
            try
            {
                LogWriter.getLog().Debug("iniciando thread en STA");
                Thread t = new Thread(act);
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
                t.Join();
                LogWriter.getLog().Debug("thread STA finalizado ");
                
            }
            catch (Exception e)
            {
                LogWriter.getLog().Error($"problemas en la ejecución de un thread en STA {e.Message} \n los detalles del error {e.StackTrace}");
                return Task.FromException(e);

                
            }
            return Task.CompletedTask;
        }

        public static async Task TimeoutAfter(Task task, TimeSpan timeout)
        {

            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {

                var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
                if (completedTask == task)
                {
                    timeoutCancellationTokenSource.Cancel();
                    await task;  // Very important in order to propagate exceptions
                }
                else
                {
                    LogWriter.getLog().Error($"operation timed out.");
                    //throw new TimeoutException("The operation has timed out.");

                }
            }
        }

        //end class
        }
    }
