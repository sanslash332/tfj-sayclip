using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows;
using System.Threading;
	
namespace sayclip
    
{
  public  class Sayclip
    {
        
        static private bool translate;
        
        static private string data = "";
        public static bool cloceNow = false;
        
        [STAThread]
        public static void Main()
        {
 
            googleTranslator.source = Properties.Settings.Default.lan1;
            googleTranslator.target = Properties.Settings.Default.lan2;

            translate = Properties.Settings.Default.translate;


            //timer = new System.Timers.Timer(Properties.Settings.Default.interval);

            //timer.Elapsed += threadFired;

            //timer.AutoReset = true;
            //timer.Enabled = true;
            ScreenReaderControl.speech("sayclip initialiced", true);
            int interval = (int)Properties.Settings.Default.interval;
            while (!cloceNow)
            {
                threadFired();
                Thread.Sleep(interval);

            }
            //Clipboard.SetText(pruebaDetraduccion);
            
                   }



        private static void threadFired()
        {
            ThreadStart tst = Timer_Elapsed;
            Thread ted = new Thread(tst);
            ted.SetApartmentState(ApartmentState.STA);
            ted.Start();


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
                //Console.WriteLine("el cp tiene texto ");
                    string rok = Clipboard.GetText();
                    //Console.WriteLine("tenemos texto en el cp: {0}", rok);
                    if (!data.Equals(rok))
                    {
                        data = rok;
                        if (translate)
                        {
                            string trad = googleTranslator.translate(data);
                            ScreenReaderControl.speech(trad, true);
                            Console.WriteLine(trad);
                        }
                        else
                        {

                            ScreenReaderControl.speech(data, true);
                            Console.WriteLine(data);
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

