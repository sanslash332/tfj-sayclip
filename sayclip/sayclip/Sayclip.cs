using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows;
using System.Threading;
using logSystem;

    	
namespace sayclip
    
{
    [Serializable]
    public enum translatorType
    {
        google,
        microsoft
    }
    
  public  class Sayclip
    {
        
        static private bool translate;
        private static bool logAlarmSet = false;
        
        static private string data = "";
        public static bool cloceNow = false;
        public static iTranslator g;

        public static void sayAndCopy(string txt)
        {
            ScreenReaderControl.speech(txt, true);
            data = txt;
            Clipboard.SetText(data);

        }
        public static bool checkMSCredentials()
        {
            MicrosoftTranslator m = new MicrosoftTranslator();
            m.sourceLang = "en";
            m.targetLang = "es";
            if(!m.checkTocken())
            {
                logSystem.LogWriter.escribir("error in tocken. bad credentials, or error in conection");
                return (false);
            }
            else
            {
                string resl = m.translate("hello world");
                if(resl.ToLower().Equals("hola mundo"))
                {
                    logSystem.LogWriter.escribir("tocken is correct, valid api key");
                    return (true);
                }
                else
                {
                    logSystem.LogWriter.escribir("error in translation check. some extrange occurs, the result: " + resl);
                    return (false);
                }

            }


        }
        public static void repeatLastClipboardContent()
        {
            ScreenReaderControl.speech(data,true);

        }

        [STAThread]
        public static void Main()
        {

            if(sayclip.Properties.Settings.Default.translator== translatorType.google)
            {
                g = new googleTranslator();
            }
            else if(sayclip.Properties.Settings.Default.translator== translatorType.microsoft)
            {
                g = new MicrosoftTranslator();
            }
            

            translate = Properties.Settings.Default.translate;


            //timer = new System.Timers.Timer(Properties.Settings.Default.interval);

            //timer.Elapsed += threadFired;

            //timer.AutoReset = true;
            //timer.Enabled = true;
            ScreenReaderControl.speech("sayclip initialiced", false);
            int interval = (int)Properties.Settings.Default.interval;
            while (!cloceNow)
            {
                threadFired();
                System.Threading.Thread.CurrentThread.Join(5);
                Thread.Sleep(interval);

            }
            //Clipboard.SetText(pruebaDetraduccion);
            
                   }



        private static void threadFired()
        {
            //ThreadStart tst = Timer_Elapsed;
            //Thread ted = new Thread(tst);
            //ted.SetApartmentState(ApartmentState.STA);
            //ted.Start();
            Timer_Elapsed();


        }
        public static void stoptimer()
        {
            //timer.Stop();
            //timer.Dispose();

        }

        [STAThread]
        public static void Timer_Elapsed()
        {
            //Console.WriteLine("evento disparado");
            

            
                if (Clipboard.ContainsText())
                {
                string rok = data;
                //Console.WriteLine("el cp tiene texto ");
                try
                {
                    rok = Clipboard.GetText();
                }
                catch (Exception e)
                {
                    LogWriter.escribir("error copying the clipboard " + e.Message +" \n type: " +e.ToString() + " \n stack: " + e.StackTrace.ToString() );
                    //ScreenReaderControl.speech("error copying the clipboard " + e.Message, true);
                    //Clipboard.Flush();
                    return;

                }
                     
                    //Console.WriteLine("tenemos texto en el cp: {0}", rok);
                    if (!data.Equals(rok))
                    {
                        data = rok;
                        if(Properties.Settings.Default.allowRepeat)
                        {
                            data += " \n";
                            Clipboard.SetText(data);

                        }
                        
                        if (translate)
                        {
                            string trad = g.translate(data);
                        if(trad.Equals("") && !logAlarmSet)
                        {
                            ScreenReaderControl.speech("error translating, see the log ", true);
                            logAlarmSet = true;
                        }
                        else
                        {
                            ScreenReaderControl.speech(trad, true);
                            logAlarmSet = false;
                            if(sayclip.Properties.Settings.Default.copyresult)
                            {
                                data = trad;
                                if(sayclip.Properties.Settings.Default.allowRepeat)
                                {
                                    data += " \n";
                                }
                                Clipboard.SetText(data);


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

