using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using logSystem;
using NLog;

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
            checkActivePluginConfiguration();

        }

        private void checkActivePluginConfiguration()
        {

        }

        public bool setActivePlugin(string pluginName)
        {
            return false;
        }

        public List<string> getPluginsNames()
        {
            List<string> names= new List<string>();
            foreach(Lazy<iSayclipPluginTranslator> p in plugins)
            {
                names.Add(p.Value.getName());
            }

            return names;

        }

        private void loadPlugins()
        {
            LogWriter.getLog().Debug("Loading plugins");
            CompositionContainer container;
            AggregateCatalog catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new DirectoryCatalog(@".\plugins", "*.scplug.dll"));
            foreach(String dir in Directory.GetDirectories(@".\plugins"))
            {
                LogWriter.getLog().Debug($"Loading plugins from {dir}");
                catalog.Catalogs.Add(new DirectoryCatalog(dir,"*.scplug.dll"));

            }
            container = new CompositionContainer(catalog);
            try
            {
                container.ComposeParts(this);

            }
            catch (CompositionException e)
            {
                LogWriter.getLog().Error($"problem loading the plugins {e.Message}");
                throw(e);
            }
            LogWriter.getLog().Debug($"plugins loaded: {plugins.Count()}");












        }
    }
}
