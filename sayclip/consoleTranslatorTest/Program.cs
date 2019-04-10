using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoogleTranslateFreeApi;

namespace consoleTranslatorTest
{
    class Program
    {
        
        
        
        public static void printLog(string logstr, int level)
        {
            Console.WriteLine("Log info: {0}", logstr);
        }

        static void Main(string[] args)
        {
            
            GoogleTranslator t = new GoogleTranslator();
            
            Console.WriteLine("iniciando testeador del traductor ");
            Console.WriteLine("idiomas disponibles: ");
            foreach(Language x in GoogleTranslator.LanguagesSupported)
            {
                Console.WriteLine(x);

            }
            Console.WriteLine("ingresar el idioma del cual se va a traducir, y luego hacia el cual se va a traducir, separados por , sin espacio y luego presione enter ");
            string data = Console.ReadLine();
            string[] values = data.Split(',');

            // Translator t = new Translator("tfjsayclipsandl", "tfjsayclip800800comsaysaysandl");
            
            

            do
            {
                Console.WriteLine("escriba el texto que desea traducir. O exit para salir. ");
                data = Console.ReadLine();
                if(data!="exit")
                {
                    Task<TranslationResult> tr = t.TranslateLiteAsync(data, GoogleTranslator.GetLanguageByISO(values[0]), GoogleTranslator.GetLanguageByISO(values[1]));

                    TranslationResult r = tr.GetAwaiter().GetResult();

                    Console.WriteLine(r.MergedTranslation);

                }

            }
            while (data != "exit");

        }
    }
}
