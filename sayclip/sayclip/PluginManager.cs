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
using System.Reflection;
using System.IO.Compression;

namespace sayclip
{
    public sealed class PluginManager
    {
        private const string PluginsFolderName = "plugins";
        private const string PluginBackupsFolderName = "plugins-backup";
        private const string PluginUnpackTempFolderName = "unpack-tmp";

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
        public List<iSayclipPluginTranslator> getPlugins
        {
            get {
                List<iSayclipPluginTranslator> pluginsList = new List<iSayclipPluginTranslator>();
                if(plugins != null)
                {
                    foreach(Lazy<iSayclipPluginTranslator> plug in plugins)
                    {
                        pluginsList.Add(plug.Value);
                    }
                }
                return (pluginsList);
            }
        }
        public static PluginManager getInstanse
        {
            get
            {
                return managerInstanse.Value;
            }
        }

        private PluginManager()
        {
            detectPluginPacks();
            loadPlugins();
            checkActivePluginConfiguration();

        }

        public void reloadPlugins()
        {
            plugins = null;
            activePlugin = null;

            detectPluginPacks();
            loadPlugins();
            checkActivePluginConfiguration();

        }

        private void checkActivePluginConfiguration()
        {
            LogWriter.getLog().Debug($"the saved configuration value is: {Properties.Settings.Default.translator}");
            if(plugins == null)
            {
                plugins = new List<Lazy<iSayclipPluginTranslator>>();
                LogWriter.getLog().Debug($"The plugins list is null, so reinitialiced as empty. Please check load logs.");
            }

            foreach(Lazy<iSayclipPluginTranslator> plug in plugins)
            {
                if(plug.Value.getName() == Properties.Settings.Default.translator)
                {
                    setActivePlugin(plug.Value.getName());
                    LogWriter.getLog().Debug($"{activePlugin.getName()} seted as active plugin");
                    return;
                }
                
            }

            if(plugins.Count() >0)
            {
                foreach (Lazy<iSayclipPluginTranslator> plug in plugins)
                {
                    if(setActivePlugin(plug.Value.getName()))
                    {
                        break;
                    }
                }

                LogWriter.getLog().Debug($"{activePlugin.getName()} seted as active plugin");
                
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
            ISayclipPluginContext context = new SayclipPluginContext();
            foreach(Lazy<iSayclipPluginTranslator> plug in plugins)
            {
                if(plug.Value.getName() == pluginName)
                {
                    LogWriter.getLog().Debug($"initializing {pluginName}");
                    bool initialized = plug.Value.initialize(context);
                    if(!initialized)
                    {
                        LogWriter.getLog().Warn($"{pluginName} canot be initialized. Fallback to empyPlugin");
                        this.activePlugin = new EmptyPlugin();
                    }
                    else
                    {
                        this.activePlugin = plug.Value;
                        Properties.Settings.Default.translator = pluginName;
                        Properties.Settings.Default.Save();
                        LogWriter.getLog().Debug($"{activePlugin.getName()} seted as active plugin");

                    }

                    return initialized;

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
            string pluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PluginsFolderName);
            if (!Directory.Exists(pluginsPath))
            {
                LogWriter.getLog().Warn($"Plugins directory does not exist: {pluginsPath}");
                plugins = new List<Lazy<iSayclipPluginTranslator>>();
                return;
            }

            catalog.Catalogs.Add(new DirectoryCatalog(pluginsPath, "*.scplug.dll"));
            foreach(String dir in Directory.GetDirectories(pluginsPath))
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
                if(Sayclip.dictlang != null)
                {
                    ScreenReaderControl.speech(Sayclip.dictlang["internal.pluginLoadError"].ToString(), true);
                }
                else
                {
                    ScreenReaderControl.speech("internal.pluginLoadError", true);
                }
                
            }
            catch(Exception e)
            {
                LogWriter.getLog().Error($"problem loading the plugins {e.Message}");
                if (Sayclip.dictlang != null)
                {
                    ScreenReaderControl.speech(Sayclip.dictlang["internal.pluginLoadError"].ToString(), true);
                }
                else
                {
                    ScreenReaderControl.speech("internal.pluginLoadError", true);
                }

            }
            string pluginsLoadedMessage = !(plugins is null) ? $"plugins loaded: {plugins.Count()}" : $"Canot load plugins.";
            LogWriter.getLog().Info(pluginsLoadedMessage);
            
        }

        private void detectPluginPacks()
        {
            if (!Properties.Settings.Default.unpackNewPlugins)
            {
                LogWriter.getLog().Debug("Plugin package unpack disabled by configuration.");
                return;
            }

            string appRoot = AppDomain.CurrentDomain.BaseDirectory;
            string pluginsPath = Path.Combine(appRoot, PluginsFolderName);

            var packageCandidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string file in Directory.GetFiles(appRoot))
            {
                if (isPluginPackageCandidate(file))
                {
                    packageCandidates.Add(file);
                }
            }

            if (Directory.Exists(pluginsPath))
            {
                foreach (string file in Directory.GetFiles(pluginsPath))
                {
                    if (isPluginPackageCandidate(file))
                    {
                        packageCandidates.Add(file);
                    }
                }
            }

            if (packageCandidates.Count == 0)
            {
                LogWriter.getLog().Debug("No plugin packages detected.");
                return;
            }

            foreach (string packagePath in packageCandidates.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    installPluginPackage(packagePath, appRoot);
                }
                catch (Exception ex)
                {
                    LogWriter.getLog().Error($"Error processing plugin package {packagePath}", ex);
                }
            }
        }

        private bool isPluginPackageCandidate(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            if (fileName.EndsWith(".installed", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string extension = Path.GetExtension(fileName);
            if (!extension.Equals(".zip", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".package", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string baseName = Path.GetFileNameWithoutExtension(fileName);
            return baseName.IndexOf("plugin", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void installPluginPackage(string packagePath, string appRoot)
        {
            LogWriter.getLog().Info($"Detected plugin package: {packagePath}");

            string unpackRoot = Path.Combine(appRoot, PluginUnpackTempFolderName, Guid.NewGuid().ToString("N"));
            string pluginsPath = Path.Combine(appRoot, PluginsFolderName);
            Directory.CreateDirectory(unpackRoot);

            try
            {
                extractPackageSafely(packagePath, unpackRoot);

                string extractedPluginsFolder = Directory.GetDirectories(unpackRoot)
                    .FirstOrDefault(dir => Path.GetFileName(dir).Equals(PluginsFolderName, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(extractedPluginsFolder))
                {
                    backupCompletePluginsFolderIfExists(appRoot, pluginsPath, packagePath);
                    Directory.CreateDirectory(pluginsPath);
                    copyDirectory(extractedPluginsFolder, pluginsPath, true);
                    LogWriter.getLog().Info($"Installed full plugins package from {packagePath}");
                }
                else
                {
                    Directory.CreateDirectory(pluginsPath);
                    backupCollisionsForIndividualPackage(unpackRoot, pluginsPath, appRoot, packagePath);
                    installIndividualPackageContents(unpackRoot, pluginsPath);
                    LogWriter.getLog().Info($"Installed plugin entries from {packagePath}");
                }

                postProcessPackageFile(packagePath);
            }
            finally
            {
                try
                {
                    if (Directory.Exists(unpackRoot))
                    {
                        Directory.Delete(unpackRoot, true);
                    }
                }
                catch (Exception cleanupEx)
                {
                    LogWriter.getLog().Warn($"Cannot cleanup temporary unpack folder {unpackRoot}. {cleanupEx.Message}");
                }
            }
        }

        private void extractPackageSafely(string packagePath, string destinationFolder)
        {
            string destinationFullPath = Path.GetFullPath(destinationFolder);

            using (ZipArchive archive = ZipFile.OpenRead(packagePath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (string.IsNullOrEmpty(entry.FullName))
                    {
                        continue;
                    }

                    string targetPath = Path.GetFullPath(Path.Combine(destinationFullPath, entry.FullName));
                    if (!targetPath.StartsWith(destinationFullPath, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidDataException($"Package contains invalid path: {entry.FullName}");
                    }

                    if (entry.FullName.EndsWith("/", StringComparison.Ordinal) || entry.FullName.EndsWith("\\", StringComparison.Ordinal))
                    {
                        Directory.CreateDirectory(targetPath);
                        continue;
                    }

                    string targetDirectory = Path.GetDirectoryName(targetPath);
                    if (!string.IsNullOrEmpty(targetDirectory))
                    {
                        Directory.CreateDirectory(targetDirectory);
                    }

                    entry.ExtractToFile(targetPath, true);
                }
            }
        }

        private void backupCompletePluginsFolderIfExists(string appRoot, string pluginsPath, string packagePath)
        {
            if (!Directory.Exists(pluginsPath))
            {
                return;
            }

            string backupRoot = createBackupRoot(appRoot, packagePath);
            string backupPluginsPath = Path.Combine(backupRoot, PluginsFolderName);
            copyDirectory(pluginsPath, backupPluginsPath, true);
            LogWriter.getLog().Info($"Created full plugins backup at {backupPluginsPath}");
        }

        private void backupCollisionsForIndividualPackage(string unpackRoot, string pluginsPath, string appRoot, string packagePath)
        {
            string backupRoot = string.Empty;

            foreach (string entry in Directory.GetFileSystemEntries(unpackRoot))
            {
                string name = Path.GetFileName(entry);
                if (name.Equals(PluginsFolderName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string destination = Path.Combine(pluginsPath, name);
                bool destinationExists = File.Exists(destination) || Directory.Exists(destination);
                if (!destinationExists)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(backupRoot))
                {
                    backupRoot = createBackupRoot(appRoot, packagePath);
                }

                string backupDestination = Path.Combine(backupRoot, PluginsFolderName, name);
                if (Directory.Exists(destination))
                {
                    copyDirectory(destination, backupDestination, true);
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(backupDestination));
                    File.Copy(destination, backupDestination, true);
                }
            }

            if (!string.IsNullOrEmpty(backupRoot))
            {
                LogWriter.getLog().Info($"Created collision backup at {backupRoot}");
            }
        }

        private void installIndividualPackageContents(string unpackRoot, string pluginsPath)
        {
            foreach (string entry in Directory.GetFileSystemEntries(unpackRoot))
            {
                string name = Path.GetFileName(entry);
                if (name.Equals(PluginsFolderName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string destination = Path.Combine(pluginsPath, name);
                if (Directory.Exists(entry))
                {
                    copyDirectory(entry, destination, true);
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(destination));
                    File.Copy(entry, destination, true);
                }
            }
        }

        private void postProcessPackageFile(string packagePath)
        {
            if (Properties.Settings.Default.deletePluginsPackagesAfterInstall)
            {
                File.Delete(packagePath);
                LogWriter.getLog().Info($"Deleted package after install: {packagePath}");
                return;
            }

            string installedPath = packagePath + ".installed";
            if (File.Exists(installedPath))
            {
                installedPath = installedPath + "." + DateTime.Now.ToString("yyyyMMddHHmmssfff");
            }

            File.Move(packagePath, installedPath);
            LogWriter.getLog().Info($"Renamed installed package: {installedPath}");
        }

        private string createBackupRoot(string appRoot, string packagePath)
        {
            string packageName = Path.GetFileNameWithoutExtension(packagePath);
            foreach (char invalid in Path.GetInvalidFileNameChars())
            {
                packageName = packageName.Replace(invalid, '_');
            }

            string backupRoot = Path.Combine(
                appRoot,
                PluginBackupsFolderName,
                DateTime.Now.ToString("yyyyMMdd-HHmmssfff") + "-" + packageName);
            Directory.CreateDirectory(backupRoot);
            return backupRoot;
        }

        private void copyDirectory(string sourceDir, string destinationDir, bool overwrite)
        {
            Directory.CreateDirectory(destinationDir);

            foreach (string sourceFile in Directory.GetFiles(sourceDir))
            {
                string destinationFile = Path.Combine(destinationDir, Path.GetFileName(sourceFile));
                File.Copy(sourceFile, destinationFile, overwrite);
            }

            foreach (string sourceSubdirectory in Directory.GetDirectories(sourceDir))
            {
                string destinationSubdirectory = Path.Combine(destinationDir, Path.GetFileName(sourceSubdirectory));
                copyDirectory(sourceSubdirectory, destinationSubdirectory, overwrite);
            }
        }

    }
}
