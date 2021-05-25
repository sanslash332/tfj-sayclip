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
    public sealed class PluginManager
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

        public void reloadPlugins()
        {
            plugins = null;
            activePlugin = null;

            loadPlugins();
            checkActivePluginConfiguration();

        }
        private void checkActivePluginConfiguration()
        {
            LogWriter.getLog().Debug($"the saved configuration value is: {Properties.Settings.Default.translator}");

            foreach(Lazy<iSayclipPluginTranslator> plug in plugins)
            {
                if(plug.Value.getName() == Properties.Settings.Default.translator)
                {
                    activePlugin = plug.Value;
                    LogWriter.getLog().Debug($"{activePlugin.getName()} seted as active plugin");
                    return;
                }
                
            }

            if(plugins.Count() >0)
            {
                activePlugin = plugins.First().Value;
                LogWriter.getLog().Debug($"{activePlugin.getName()} seted as active plugin");
                Properties.Settings.Default.translator = activePlugin.getName();
                Properties.Settings.Default.Save();

            }
            else
            {
                activePlugin = new EmptyPlugin();
                ConfigurationManager.getInstance.translating = false;
                LogWriter.getLog().Debug("not loaded plugins to set as active");
            }
            

        }

        public bool setActivePlugin(string pluginName)
        {
            foreach(Lazy<iSayclipPluginTranslator> plug in plugins)
            {
                if(plug.Value.getName() == pluginName)
                {
                    this.activePlugin = plug.Value;
                    Properties.Settings.Default.translator = pluginName;
                    Properties.Settings.Default.Save();
                    LogWriter.getLog().Debug($"{activePlugin.getName()} seted as active plugin");

                    return true;

                }
            }
            return (false);
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
            LogWriter.getLog().Info("Loading plugins");
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
                ScreenReaderControl.speech(Sayclip.dictlang["internal.pluginLoadError"].ToString(), true);
            }
            catch(Exception e)
            {
                LogWriter.getLog().Error($"problem loading the plugins {e.Message}");
                ScreenReaderControl.speech(Sayclip.dictlang["internal.pluginLoadError"].ToString(), true);
            }
            LogWriter.getLog().Info($"plugins loaded: {plugins.Count()}");

        }
    }
}
