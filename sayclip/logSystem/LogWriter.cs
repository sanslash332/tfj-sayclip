using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Log;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace logSystem
{
  public class LogWriter
    {
        static private StreamWriter escritor;
        static private LogStore log;
        static private LogRecordSequence logSequense;
        static private object locker;
        

        static public void init()
        {
            FileStream fs = new FileStream("sayclip.log", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            escritor = new StreamWriter(fs, Encoding.Unicode);
            if (fs.Length >= 1024 * 1024 * 10)
            {
                terminate();
                File.Delete("sayclip.log");
                init();
            }
            else
            {

                locker = new object();
                fs.Seek(fs.Length, SeekOrigin.Begin);

                escritor.WriteLine("program started at " + DateTime.Now.ToString());
                
                


            }            
        }

        public static void escribir(string obj)
        {
            Monitor.Enter(locker);
            if (escritor != null)
            {
                if (obj.Contains("Unable to cast ") == false)
                {
                    
                    escritor.WriteLine(DateTime.Now.ToString() +": " + obj);
                    escritor.Flush();

                }

            }
            Monitor.Exit(locker);

                    }

        void NetManajer_sendError(string obj)
        {
            if (escritor != null)
            {
                escritor.WriteLine(obj + "\n");

            }

        }

        public static void terminate()
        {
            escritor.Flush();
            escritor.Dispose();
            escritor.Close();
            escritor = null;

        }

        void GameCore__sendError(string obj)
        {
            if (escritor != null)
            {
                if (obj.Contains("Unable to cast ") == false)
                {

                    escritor.WriteLine(obj + " \n");
                }
                            }
                    }

        void BaseElement__sendError(string obj)
        {
            if (escritor != null)
            {
                if (obj.Contains("Unable to cast") == false)
                {

                    escritor.WriteLine(obj + " \n ");
                }


            }
                    }


    }
}
