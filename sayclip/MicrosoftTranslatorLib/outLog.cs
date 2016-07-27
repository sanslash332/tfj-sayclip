using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MicrosoftTranslatorLib
{
     public static class outLog
    {
        public static event Action<string, int> sendLog;
        public static void onSendLog(string t, int v )
        {
            sendLog(t, v);

        }

    }
}
