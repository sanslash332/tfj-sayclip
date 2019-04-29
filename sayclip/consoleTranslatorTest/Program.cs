using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sayclip;
using logSystem;
using NLog;



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
            
            Console.WriteLine("iniciando testeador del traductor ");
            LogWriter.getLog().Info("provando el primer logueo");

            Console.WriteLine("opteniendo lista de plugins disponibles");
            PluginManager pm = PluginManager.getInstanse;
            Console.WriteLine($"Los plugins detectados son: ");
            foreach(string plug in pm.getPluginsNames())
            {
                Console.WriteLine(plug);

            }

            iSayclipPluginTranslator t = pm.getActivePlugin;
            if(t == null)
            {
                Console.WriteLine("problemas a cargar los plugins. revise el log por favor");
                Console.ReadLine();
                System.Environment.Exit(1);


            }
            Console.WriteLine($"el plugin activo es: {pm.getActivePlugin.getName()}");
            

            Console.WriteLine("idiomas disponibles: ");
            foreach(KeyValuePair<string,string> x in t.getAvailableLanguages("es"))
            {
                Console.WriteLine($" {x.Key}  {x.Value} ");


            }
            Console.WriteLine($"Los idiomas que ya tiene configurados el plugin son: {t.getConfiguredLanguajes("es")[0]} a {t.getConfiguredLanguajes("es")[1]}");

            Console.WriteLine("ingresar el idioma del cual se va a traducir, y luego hacia el cual se va a traducir, separados por , sin espacio y luego presione enter ");
            string data = Console.ReadLine();
            string[] values = data.Split(',');
            t.setLanguages(values[0], values[1]);

            // Translator t = new Translator("tfjsayclipsandl", "tfjsayclip800800comsaysaysandl");
            
            

            do
            {
                Console.WriteLine("escriba el texto que desea traducir. O exit para salir. ");
                data = Console.ReadLine();
                if(data!="exit")
                {
                    Task<string> tr = t.translate(data);

                    string r = tr.GetAwaiter().GetResult();

                    Console.WriteLine(r);

                }

            }
            while (data != "exit");

        }
    }
}
