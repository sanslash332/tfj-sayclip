using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MicrosoftTranslatorLib;
using RavSoft.GoogleTranslator;
namespace consoleTranslatorTest
{
    public enum translatorSelector
    {
        googleTranslator,
        MicrosoftTranslator
    }
    class Program
    {
        
        
        
        public static void printLog(string logstr, int level)
        {
            Console.WriteLine("Log info: {0}", logstr);
        }

        static void Main(string[] args)
        {
            outLog.sendLog += printLog;
            Console.WriteLine("iniciando testeador del traductor ");
            Console.WriteLine("idiomas disponibles: ");
            foreach(string x in RavSoft.GoogleTranslator.Translator.Languages)
            {
                Console.WriteLine(x);

            }
            Console.WriteLine("ingresar el idioma del cual se va a traducir, y luego hacia el cual se va a traducir, separados por , sin espacio y luego presione enter ");
            string data = Console.ReadLine();
            string[] values = data.Split(',');

            // Translator t = new Translator("tfjsayclipsandl", "tfjsayclip800800comsaysaysandl");
            RavSoft.GoogleTranslator.Translator t = new RavSoft.GoogleTranslator.Translator();
            

            do
            {
                Console.WriteLine("escriba el texto que desea traducir. O exit para salir. ");
                data = Console.ReadLine();
                if(data!="exit")
                {
                    string r = t.Translate(data, values[0], values[1]);
                    Console.WriteLine(r);


                }

            }
            while (data != "exit");

        }
    }
}
