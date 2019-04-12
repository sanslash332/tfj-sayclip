using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel.Composition;
using logSystem;

namespace sayclip
{
    sealed class PluginManager
    {
        private static readonly Lazy<PluginManager> managerInstanse = new Lazy<PluginManager>(() => new PluginManager() );
        private iSayclipPluginTranslator activePlugin;
        public iSayclipPluginTranslator getActivePlugin
        {
            get
            {
                return activePlugin;
            }
        }
        [ImportMany]
        private IEnumerable<Lazy<iSayclipPluginTranslator>> plugins;
        public static PluginManager getInstanse
        {
            get
            {
                return managerInstanse.Value;
            }
        }

        private PluginManager()
        {
            loadPlugins();
            if(!setActivePlugin(sayclip.Properties.Settings.Default.translator))
            {

            }



        }
        public bool setActivePlugin(string pluginName)
        {

        }

        public List<string> getPluginsNames()
        {
            List<string> names;

            return names;

        }

        public void loadPlugins()
        {

        }
    }
}
