﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks.Schedulers;
using sayclip;
using logSystem;
using NLog;
using System.Windows;

namespace consoleTranslatorTest
{
    class Program
    {
        
        
        
        static void Main(string[] args)
        {
            
            Console.WriteLine("iniciando testeador del core de sayclip.");
            LogWriter.getLog().Info("provando el primer logueo");
            Sayclip.dictlang = new System.Windows.ResourceDictionary();
            Sayclip.dictlang.Add("internal.start", "¡chipaum!");
            
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
            changeLanguaje(t);
            Sayclip sayclip = new Sayclip();
            CancellationTokenSource tokenator = new CancellationTokenSource();
            CancellationToken token = tokenator.Token;
            runSayclip(sayclip, token);
            
            string data = "";


            do
            {
                Console.WriteLine("escriba una de las opciones para configurar el core. O exit para salir \n c: para cambiar los idiomas configurados \n t para activar o desactivar la traducción \n r: para admitir texto repetido \n p: para copiar el resultado de la traducción al portapapeles.");
                data = Console.ReadLine();
                switch (data)
                {
                    case "exit":
                        Console.WriteLine("cerrando");
                        break;
                    case "c":
                        Console.WriteLine("cambiando idiomas configurados");
                        changeLanguaje(t);
                        break;
                    case "t":
                        ConfigurationManager.getInstance.translating = !ConfigurationManager.getInstance.translating;
                        Console.WriteLine($"la traducción ha sido cambiada a {ConfigurationManager.getInstance.translating}");
                        break;
                    case "r":
                        ConfigurationManager.getInstance.allowCopyRepeatedText = !ConfigurationManager.getInstance.allowCopyRepeatedText;
                        Console.WriteLine($"la copia de texto repetido ha sido cambiada a {ConfigurationManager.getInstance.allowCopyRepeatedText}");
                        break;
                    case "p":
                        ConfigurationManager.getInstance.copyResultToClipboard= !ConfigurationManager.getInstance.copyResultToClipboard;
                        Console.WriteLine($"la copia de resultados al portapapeles ha sido cambiada a {ConfigurationManager.getInstance.copyResultToClipboard}");
                        break;

                    default:
                        Console.WriteLine("noup, eso no ta");
                        break;
                }
                
            }
            while (data != "exit");

            tokenator.Cancel();

        }

        private static async Task runSayclip(Sayclip s,CancellationToken token)
        {
            try
            {
                s.Main(token);
                /*
                StaTaskScheduler schel = new StaTaskScheduler(1);
                Task stak = Task.Factory.StartNew(() => s.Main(token), token, TaskCreationOptions.None, schel);

                await stak;
                */
                
            }
            catch (Exception e)
            {
                LogWriter.getLog().Error($"error detected in sayclip run {e.ToString() }");
                Console.WriteLine("waaa too explotoooo ");

                
            }
            


        }

        private static void changeLanguaje(iSayclipPluginTranslator t)
        {
            Console.WriteLine("idiomas disponibles: ");
            foreach (SayclipLanguage x in t.getAvailableLanguages("es"))
            {
                Console.WriteLine($"{x.langCode}  {x.displayName}");

            }
            Console.WriteLine($"Los idiomas que ya tiene configurados el plugin son: {t.getConfiguredLanguajes("es")[0].displayName} a {t.getConfiguredLanguajes("es")[1].displayName}");

            Console.WriteLine("ingresar el idioma del cual se va a traducir, y luego hacia el cual se va a traducir, separados por , sin espacio y luego presione enter ");
            string data = Console.ReadLine();
            string[] values = data.Split(',');
            SayclipLanguage fromLang = t.getAvailableLanguages("es").Find((SayclipLanguage x) => {
                return (x.langCode == values[0]);
            });
            SayclipLanguage toLang = t.getAvailableLanguages("es").Find((SayclipLanguage x) => {
                return (x.langCode == values[1]);
            });

            t.setLanguages(fromLang, toLang);
            Console.WriteLine($"Los idiomas que fueron configurados el plugin son: {t.getConfiguredLanguajes("es")[0].displayName} a {t.getConfiguredLanguajes("es")[1].displayName}");

        }
    }
}
