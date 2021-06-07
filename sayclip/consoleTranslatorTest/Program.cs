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
            int index = 0;
            foreach(string plug in pm.getPluginsNames())
            {
                Console.WriteLine($"{index}: {plug}");
                index++;

            }

            Console.WriteLine($"seleccione el plugin que quiere usar (inserte su número correspondiente)");
            bool selectionResult;
            index = -1;
            do
            {
                Console.WriteLine($"seleccione el plugin que quiere usar (inserte su número correspondiente)");
                selectionResult = Int32.TryParse(Console.ReadLine(), out index);
                Console.WriteLine($"utilizando el plugin {index} {selectionResult}");
            }
            while (!selectionResult && index <= 0 && index >= pm.getPluginsNames().Count);

            if(!pm.setActivePlugin(pm.getPluginsNames()[index]))
            {
                Console.WriteLine($"problemas al activar el plugin {pm.getPluginsNames()[index]}. Utilizando el valor anterior");
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
            List<SayclipLanguage> languajes = (List<SayclipLanguage>)t.getAvailableLanguages("es").GetAwaiter().GetResult();
            foreach (SayclipLanguage x in languajes)
            {
                Console.WriteLine($" {x.langCode}  {x.displayName} ");


            }
            
            Console.WriteLine($"Los idiomas que ya tiene configurados el plugin son: {t.getConfiguredLanguajes("es")[0].displayName} a {t.getConfiguredLanguajes("es")[1].displayName}");

            Console.WriteLine("ingresar el idioma del cual se va a traducir, y luego hacia el cual se va a traducir, separados por , sin espacio y luego presione enter ");
            string data = Console.ReadLine();
            string[] values = data.Split(',');
            SayclipLanguage fromLang = languajes.Find((SayclipLanguage x) => {
                return (x.langCode == values[0]);
            });
            SayclipLanguage toLang = languajes.Find((SayclipLanguage x) => {
                return (x.langCode == values[1]);
            });

            try
            {
                t.setLanguages(fromLang, toLang);
            }
            catch (Exception e)
            {
                Console.WriteLine($"ups, algo pasó en la selección de idiomas. {e.Message}");
                Console.ReadLine();
                Environment.Exit(1);
                
            }
            


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
