using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace sayclip
{
  public  class ScreenReaderControl
    {
        private static  bool active = true;
        private static  int currentSapiVoice = 0;

        [DllImport("UniversalSpeech.dll", CharSet= CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]       
       private   static unsafe extern int speechSay(IntPtr str, int interrupt);

        [DllImport("ScreenReaderAPI.dll")]
        private static extern bool sayStringA(string text, bool interrupt);


        public static unsafe void speech(string texto, bool interrupcion)
        {
            if (active)
            {
            
                
                speechSay(Marshal.StringToBSTR(texto), Convert.ToInt32(interrupcion));

            }
        }

        public static void setSRControl(bool trigger)
        {
            active = trigger;

        }

        public static bool isSRControlActive()
        {
            return active;

        }

    }
}
