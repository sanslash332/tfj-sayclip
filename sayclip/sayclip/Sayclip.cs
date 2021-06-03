using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;
using System.Runtime.InteropServices;
using System.Windows;
using System.Threading;
using System.Diagnostics;
using logSystem;
using NLog;
using WK.Libraries.SharpClipboardNS;

namespace sayclip
    {
    public  class Sayclip
    {
        public static ResourceDictionary dictlang;
        private SharpClipboard sharpCP;
        private ConfigurationManager config;
        private string data = "";
        private string lastResult = "";
        public iSayclipPluginTranslator translator;
        private Stopwatch stopwatch;
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
            ScreenReaderControl.speech(data, true);

        }
        
        public async Task Main(CancellationToken canceller)
        {
            LogWriter.getLog().Debug("Starting sayclip core");
            config = ConfigurationManager.getInstance;
            translator = PluginManager.getInstanse.getActivePlugin;
            
            if (translator.getName()==EmptyPlugin.emptyName)
            {
                LogWriter.getLog().Warn("no active plugin detected");
                var saytask = sayAndCopy(dictlang["internal.noactiveplugin"].ToString());

                config.translating = false;
            }
            LogWriter.getLog().Debug($"sayclip using {translator.getName()} plugin");
            ScreenReaderControl.speech(dictlang["internal.start"].ToString(), false);
            sharpCP = new SharpClipboard();
            sharpCP.ClipboardChanged += SharpCP_ClipboardChanged;
            stopwatch = new Stopwatch();
            stopwatch.Start();
            
        }

        public void shutDownCore()
        {
            LogWriter.getLog().Debug("shuting down sayclip core");
            sharpCP.ClipboardChanged -= SharpCP_ClipboardChanged;
            stopwatch.Stop();
        }

        private async void SharpCP_ClipboardChanged(object sender, SharpClipboard.ClipboardChangedEventArgs e)
        {
            if(checkIntervalTime() && e.ContentType== SharpClipboard.ContentTypes.Text && e.Content!=null && !checkEmptyString(e.Content.ToString()) && !checkRepeatedString(e.Content.ToString()))
            {
                data = e.Content.ToString();
                LogWriter.getLog().Debug($"captured data is: {e.Content.ToString()}");
                string translation = await translate(e.Content.ToString());
                LogWriter.getLog().Debug($"translated data is: {translation}");
                ScreenReaderControl.speech(translation, true);
                lastResult = translation;
                await copyResult(translation);
                
            }
        }

        private bool checkIntervalTime()
        {
            bool result = false;
            if(stopwatch.ElapsedMilliseconds >= (long)config.clipboardPollingSpeed)
            {
                result = true;
                stopwatch.Restart();
            }
            return (result);
        }
        private async Task<string> translate(string text)
        {
            string translation;
            if(config.translating)
            {
                try
                {
                    translation = await translator.translate(text);
                    if (checkEmptyString(translation))
                    {
                        throw (new Exception("empty answer from translation"));
                    }
                }
                catch (Exception e)
                {
                    LogWriter.getLog().Error($"error during the translation {e.Message} \n {e.StackTrace}");
                    translation = dictlang["internal.errortranslating"].ToString();
                }
            }
            else
            {
                translation = text;
            }
            
            return (translation);
        }

        private async Task copyResult(string text)
        {
            if(config.copyResultToClipboard)
            {
                try
                {
                    data = text;
                    await setClipboardText(text);
                }
                catch (Exception e)
                {
                    LogWriter.getLog().Error($"problems setting clipboard content: {e.Message}");
                    throw;
                }
            }
        }

        private bool checkRepeatedString(string text)
        {
            bool result;
            if(config.allowCopyRepeatedText)
            {
                result = text.Equals(lastResult);
            }
            else
            {
                result = text.Equals(data) || text.Equals(lastResult);
            }
            return (result);
        }

        private bool checkEmptyString(string text)
        {
            String clearText = text.Replace("\n", "");
            clearText = clearText.Replace("\t", "");
            clearText = clearText.Replace("\r", "");
            clearText = clearText.Replace(" ", "");
            clearText = clearText.Trim();
            
            return (clearText.Equals(""));

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
