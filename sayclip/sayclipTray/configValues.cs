using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace sayclipTray
{
    public static class ConfigValues
    {
        private static App app = (App)Application.Current;
        public static Dictionary<int, string> monitorSpeedsOptions = new Dictionary<int, string>
            {
                {2000, App.dictlang["menu.monitor.2000"].ToString() },
                {1000, App.dictlang["menu.monitor.1000"].ToString() },
                {500, App.dictlang["menu.monitor.500"].ToString() },
                {100, App.dictlang["menu.monitor.100"].ToString() },
                { 50, App.dictlang["menu.monitor.50"].ToString()  }
            };
        public static Dictionary<string, string> uiLangOptions = new Dictionary<string, string>
        {
                {"auto", App.dictlang["menu.ui.auto"].ToString()  },
                {"en", App.dictlang["menu.ui.en"].ToString() },
                { "es", App.dictlang["menu.ui.es"].ToString() }
                
            };



    }
}
